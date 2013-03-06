//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : ESentResolveReferenceLinksComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/28/2013
// Compiler: Microsoft Visual C#
//
// This is a version of the ResolveReferenceLinksComponent2 that stores the MSDN content IDs and the framework
// targets in persistent ESent databases.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.7.0  12/31/2012  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml.XPath;

using Microsoft.Ddue.Tools;
using Microsoft.Ddue.Tools.Targets;

using Microsoft.Isam.Esent.Collections.Generic;

using SandcastleBuilder.Components.Targets;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This is a version of the <c>ResolveReferenceLinksComponent2</c> that stores the MSDN content IDs and the
    /// framework targets in persistent ESent databases.
    /// </summary>
    public class ESentResolveReferenceLinksComponent : ResolveReferenceLinksComponent2
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembler">A reference to the build assembler.</param>
        /// <param name="configuration">The configuration information</param>
        /// <remarks>This component is obsolete and will be removed in a future release.</remarks>
        public ESentResolveReferenceLinksComponent(BuildAssembler assembler, XPathNavigator configuration) :
          base(assembler, configuration)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            base.WriteMessage(MessageLevel.Info, String.Format(CultureInfo.InvariantCulture,
                "\r\n    [{0}, version {1}]\r\n    ESent Resolve Reference Links Component.  {2}\r\n" +
                "    http://SHFB.CodePlex.com", fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright));
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// This is overridden to allow use of an ESent backed MSDN content ID cache
        /// </summary>
        /// <param name="configuration">The component configuration</param>
        /// <returns>An MSDN resolver instance</returns>
        protected override MsdnResolver CreateMsdnResolver(XPathNavigator configuration)
        {
            MsdnResolver resolver;
            IDictionary<string, string> cache = null;
            int localCacheSize;

            if(BuildComponent.Data.ContainsKey(SharedMsdnContentIdCacheId))
                cache = BuildComponent.Data[SharedMsdnContentIdCacheId] as IDictionary<string, string>;

            // If the shared cache already exists, return an instance that uses it.  It is assumed that all
            // subsequent instances will use the same cache.
            if(cache != null)
                return new MsdnResolver(cache);

            XPathNavigator node = configuration.SelectSingleNode("msdnContentIdCache");

            // If a <cache> element is not specified, use the default resolver
            if(node == null)
                resolver = base.CreateMsdnResolver(configuration);
            else
            {
                string msdnIdDbPath = node.GetAttribute("dbPath", String.Empty);

                // If a database path is not defined, use the default resolver
                if(String.IsNullOrWhiteSpace(msdnIdDbPath))
                    resolver = base.CreateMsdnResolver(configuration);
                else
                {
                    msdnIdDbPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(msdnIdDbPath));

                    string cacheSize = node.GetAttribute("localCacheSize", String.Empty);

                    if(String.IsNullOrWhiteSpace(cacheSize) || !Int32.TryParse(cacheSize, out localCacheSize))
                        localCacheSize = 1000;

                    // Load or create the cache database and the resolver.  The resolver will dispose of the
                    // dictionary when it is disposed of since it implements IDisposable.  We won't compress the
                    // columns as they're typically not that big and there aren't usually that many entries.
                    // This gives a slight performance increase.
                    resolver = new MsdnResolver(new PersistentDictionary<string, string>(msdnIdDbPath, false)
                    { LocalCacheSize = localCacheSize });

                    int cacheCount = resolver.MsdnContentIdCache.Count;

                    if(cacheCount == 0)
                    {
                        // Log a diagnostic message since looking up all IDs can significantly slow the build
                        base.WriteMessage(MessageLevel.Diagnostic, "The ESent MSDN content ID cache in '" +
                            msdnIdDbPath + "' does not exist yet.  All IDs will be looked up in this build " +
                            "which will slow it down.");
                    }
                    else
                        base.WriteMessage(MessageLevel.Info, "{0} cached MSDN content ID entries exist", cacheCount);

                    BuildComponent.Data[SharedMsdnContentIdCacheId] = resolver.MsdnContentIdCache;
                }
            }

            return resolver;
        }

        /// <summary>
        /// This is overridden to create a target dictionary that utilizes an ESent database for persistence
        /// </summary>
        /// <param name="configuration">The configuration element for the target dictionary</param>
        /// <returns>A simple dictionary if no <c>dbPath</c> attribute is found or an ESent backed target
        /// dictionary if the attribute is found.</returns>
        public override TargetDictionary CreateTargetDictionary(XPathNavigator configuration)
        {
            TargetDictionary td = null;

            string dbPath = configuration.GetAttribute("dbPath", String.Empty);

            // If no database path is specified, use the simple target dictionary (i.e. project references)
            if(String.IsNullOrWhiteSpace(dbPath))
                td = base.CreateTargetDictionary(configuration);
            else
            {
                try
                {
                    td = new ESentTargetDictionary(this, configuration);
                }
                catch(Exception ex)
                {
                    base.WriteMessage(MessageLevel.Error, BuildComponentUtilities.GetExceptionMessage(ex));
                }
            }

            return td;
        }

        /// <summary>
        /// This is overridden to report the persistent cache information
        /// </summary>
        public override void UpdateMsdnContentIdCache()
        {
            if(base.MsdnResolver != null)
            {
                var cache = base.MsdnResolver.MsdnContentIdCache as PersistentDictionary<string, string>;

                if(cache != null)
                {
                    if(base.MsdnResolver.CacheItemsAdded)
                        base.WriteMessage(MessageLevel.Diagnostic, "New MSDN content ID cache size: {0} entries",
                            cache.Count);

                    base.WriteMessage(MessageLevel.Diagnostic, "MSDN content ID ESent local cache flushed {0} " +
                        "time(s).  Current ESent local cache usage: {1} of {2}.", cache.LocalCacheFlushCount,
                        cache.CurrentLocalCacheCount, cache.LocalCacheSize);
                }
            }

            base.UpdateMsdnContentIdCache();
        }
        #endregion
    }
}
