//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : ESentResolveReferenceLinksComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/24/2014
// Compiler: Microsoft Visual C#
//
// This is a version of the ResolveReferenceLinksComponent that stores the MSDN content IDs and the framework
// targets in persistent ESENT databases.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.7.0  12/31/2012  EFW  Created the code
// -------  12/26/2013  EFW  Updated the build component to be discoverable via MEF
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

using Microsoft.Ddue.Tools;
using Microsoft.Ddue.Tools.Targets;

using Microsoft.Isam.Esent.Collections.Generic;

using SandcastleBuilder.Components.Targets;
using SandcastleBuilder.Components.UI;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This is a version of the <c>ResolveReferenceLinksComponent</c> that stores the MSDN content IDs and the
    /// framework targets in persistent ESENT databases.
    /// </summary>
    public class ESentResolveReferenceLinksComponent : ResolveReferenceLinksComponent
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component used to resolve reference links
        /// </summary>
        [BuildComponentExport("Resolve Reference Links (ESENT Cache)", IsVisible = true, IsConfigurable = true,
          Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
          Description = "This component is used to resolve reference links in topics.  It uses ESENT " +
            "databases to cache MSDN content IDs and .NET Framework reference link targets.  This speeds up " +
            "initialization and conserves memory at the expense of some build time in larger projects.  For " +
            "extremely large projects, it is also possible to cache project reference link data to conserve " +
            "memory.\r\n\r\nThe ESENT database engine is part of every version of Microsoft Windows and no " +
            "set up is required.")]
        public sealed class ESentResolveReferenceLinksComponentFactory : BuildComponentFactory
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public ESentResolveReferenceLinksComponentFactory()
            {
                base.ReferenceBuildPlacement = new ComponentPlacement(PlacementAction.Replace,
                    "Resolve Reference Links Component");
                base.ConceptualBuildPlacement = new ComponentPlacement(PlacementAction.Replace,
                    "Resolve Reference Links Component");
            }

            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new ESentResolveReferenceLinksComponent(base.BuildAssembler);
            }

            /// <inheritdoc />
            public override string DefaultConfiguration
            {
                get
                {
                    return @"<locale value=""{@Locale}"" />
<linkTarget value=""{@SdkLinkTarget}"" />

<helpOutput format=""HtmlHelp1"">
	<msdnContentIdCache path=""{@LocalDataFolder}Cache\MsdnContentId.cache""
		cachePath=""{@LocalDataFolder}Cache\ESentMsdnContentIdCache"" localCacheSize=""2500"" />
	<targets base=""{@SHFBFolder}Data\Reflection"" recurse=""true"" files=""*.xml"" type=""{@HtmlSdkLinkType}""
		id=""FrameworkTargets"" cachePath=""{@LocalDataFolder}Cache\ESentFrameworkTargetCache"" localCacheSize=""2500"">
		{@ReferenceLinkNamespaceFiles}
	</targets>
	<targets files=""reflection.xml"" type=""Local"" id=""ProjectTargets"" />
</helpOutput>

<helpOutput format=""MSHelp2"">
	<msdnContentIdCache path=""{@LocalDataFolder}Cache\MsdnContentId.cache""
		cachePath=""{@LocalDataFolder}Cache\ESentMsdnContentIdCache"" localCacheSize=""2500"" />
	<targets base=""{@SHFBFolder}Data\Reflection"" recurse=""true"" files=""*.xml"" type=""{@MSHelp2SdkLinkType}""
		id=""FrameworkTargets"" cachePath=""{@LocalDataFolder}Cache\ESentFrameworkTargetCache"" localCacheSize=""2500"">
		{@ReferenceLinkNamespaceFiles}
	</targets>
	<targets files=""reflection.xml"" type=""Index"" id=""ProjectTargets"" />
</helpOutput>

<helpOutput format=""MSHelpViewer"">
	<msdnContentIdCache path=""{@LocalDataFolder}Cache\MsdnContentId.cache""
		cachePath=""{@LocalDataFolder}Cache\ESentMsdnContentIdCache"" localCacheSize=""2500"" />
	<targets base=""{@SHFBFolder}Data\Reflection"" recurse=""true"" files=""*.xml"" type=""{@MSHelpViewerSdkLinkType}""
		id=""FrameworkTargets"" cachePath=""{@LocalDataFolder}Cache\ESentFrameworkTargetCache"" localCacheSize=""2500"">
		{@ReferenceLinkNamespaceFiles}
	</targets>
	<targets files=""reflection.xml"" type=""Id"" id=""ProjectTargets"" />
</helpOutput>

<helpOutput format=""Website"">
	<msdnContentIdCache path=""{@LocalDataFolder}Cache\MsdnContentId.cache""
		cachePath=""{@LocalDataFolder}Cache\ESentMsdnContentIdCache"" localCacheSize=""2500"" />
	<targets base=""{@SHFBFolder}Data\Reflection"" recurse=""true"" files=""*.xml"" type=""{@WebsiteSdkLinkType}""
		id=""FrameworkTargets"" cachePath=""{@LocalDataFolder}Cache\ESentFrameworkTargetCache"" localCacheSize=""2500"">
		{@ReferenceLinkNamespaceFiles}
	</targets>
	<targets files=""reflection.xml"" type=""Local"" id=""ProjectTargets"" />
</helpOutput>";
                }
            }

            /// <inheritdoc />
            public override string ConfigureComponent(string currentConfiguration, CompositionContainer container)
            {
                using(var dlg = new ESentResolveReferenceLinksConfigDlg(currentConfiguration))
                {
                    if(dlg.ShowDialog() == DialogResult.OK)
                        currentConfiguration = dlg.Configuration;
                }

                return currentConfiguration;
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private bool ownsResolverCache;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected ESentResolveReferenceLinksComponent(BuildAssemblerCore buildAssembler) : base(buildAssembler)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            base.WriteMessage(MessageLevel.Info, String.Format(CultureInfo.InvariantCulture,
                "[{0}, version {1}]\r\n    ESENT Resolve Reference Links Component.  {2}\r\n" +
                "    http://SHFB.CodePlex.com", fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright));

            base.Initialize(configuration);
        }

        /// <summary>
        /// This is overridden to allow use of an ESENT backed MSDN content ID cache
        /// </summary>
        /// <param name="configuration">The component configuration</param>
        /// <returns>An MSDN resolver instance</returns>
        protected override MsdnResolver CreateMsdnResolver(XPathNavigator configuration)
        {
            MsdnResolver resolver;
            IDictionary<string, string> cache = null;
            int localCacheSize;

            if(BuildComponentCore.Data.ContainsKey(SharedMsdnContentIdCacheId))
                cache = BuildComponentCore.Data[SharedMsdnContentIdCacheId] as IDictionary<string, string>;

            // If the shared cache already exists, return an instance that uses it.  It is assumed that all
            // subsequent instances will use the same cache.
            if(cache != null)
                return new MsdnResolver(cache, true);

            XPathNavigator node = configuration.SelectSingleNode("msdnContentIdCache");

            // If an <msdnContentIdCache> element is not specified, use the default resolver
            if(node == null)
                resolver = base.CreateMsdnResolver(configuration);
            else
            {
                string msdnIdCachePath = node.GetAttribute("cachePath", String.Empty);

                // If a cache path is not defined, use the default resolver
                if(String.IsNullOrWhiteSpace(msdnIdCachePath))
                    resolver = base.CreateMsdnResolver(configuration);
                else
                {
                    msdnIdCachePath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(msdnIdCachePath));

                    string cacheSize = node.GetAttribute("localCacheSize", String.Empty);

                    if(String.IsNullOrWhiteSpace(cacheSize) || !Int32.TryParse(cacheSize, out localCacheSize))
                        localCacheSize = 2500;

                    // Load or create the cache database and the resolver.  The resolver will dispose of the
                    // dictionary when it is disposed of since it implements IDisposable.  We won't compress the
                    // columns as they're typically not that big and there aren't usually that many entries.
                    // This gives a slight performance increase.
                    resolver = new MsdnResolver(new PersistentDictionary<string, string>(msdnIdCachePath, false)
                        { LocalCacheSize = localCacheSize }, false);

                    // We own the cache and will report statistics when done
                    ownsResolverCache = true;

                    int cacheCount = resolver.MsdnContentIdCache.Count;

                    if(cacheCount == 0)
                    {
                        // Log a diagnostic message since looking up all IDs can significantly slow the build
                        base.WriteMessage(MessageLevel.Diagnostic, "The ESENT MSDN content ID cache in '" +
                            msdnIdCachePath + "' does not exist yet.  All IDs will be looked up in this build " +
                            "which will slow it down.");
                    }
                    else
                        base.WriteMessage(MessageLevel.Info, "{0} cached MSDN content ID entries exist", cacheCount);

                    BuildComponentCore.Data[SharedMsdnContentIdCacheId] = resolver.MsdnContentIdCache;
                }
            }

            return resolver;
        }

        /// <summary>
        /// This is overridden to create a target dictionary that utilizes an ESENT database for persistence
        /// </summary>
        /// <param name="configuration">The configuration element for the target dictionary</param>
        /// <returns>A simple dictionary if no <c>cachePath</c> attribute is found or an ESENT backed target
        /// dictionary if the attribute is found.</returns>
        public override TargetDictionary CreateTargetDictionary(XPathNavigator configuration)
        {
            TargetDictionary td = null;

            string cachePath = configuration.GetAttribute("cachePath", String.Empty);

            // If no database path is specified, use the simple target dictionary (i.e. project references)
            if(String.IsNullOrWhiteSpace(cachePath))
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
            if(ownsResolverCache && base.MsdnResolver != null)
            {
                var cache = base.MsdnResolver.MsdnContentIdCache as PersistentDictionary<string, string>;

                if(cache != null)
                {
                    if(base.MsdnResolver.CacheItemsAdded)
                        base.WriteMessage(MessageLevel.Diagnostic, "New MSDN content ID cache size: {0} entries",
                            cache.Count);

                    base.WriteMessage(MessageLevel.Diagnostic, "MSDN content ID ESENT local cache flushed {0} " +
                        "time(s).  Current ESENT local cache usage: {1} of {2}.", cache.LocalCacheFlushCount,
                        cache.CurrentLocalCacheCount, cache.LocalCacheSize);
                }
            }

            base.UpdateMsdnContentIdCache();
        }
        #endregion
    }
}
