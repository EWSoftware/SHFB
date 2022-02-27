//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : ESentResolveReferenceLinksComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/24/2021
//
// This is a version of the ResolveReferenceLinksComponent that stores the member ID URLs and the framework
// targets in persistent ESENT databases.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/31/2012  EFW  Created the code
// 12/26/2013  EFW  Updated the build component to be discoverable via MEF
// 04/12/2021  EFW  Merged SHFB build components into the main build components assembly
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml.XPath;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

using Sandcastle.Tools.BuildComponents;
using Sandcastle.Tools.BuildComponents.Targets;

using Microsoft.Isam.Esent.Collections.Generic;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This is a version of the <c>ResolveReferenceLinksComponent</c> that stores the member ID URLs and the
    /// framework targets in persistent ESENT databases.
    /// </summary>
    public class ESentResolveReferenceLinksComponent : ResolveReferenceLinksComponent
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component used to resolve reference links
        /// </summary>
        [BuildComponentExport("Resolve Reference Links (ESENT Cache)", IsVisible = true,
          Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
          Description = "This component is used to resolve reference links in topics.  It uses ESENT " +
            "databases to cache member ID URLs and .NET Framework reference link targets.  This speeds up " +
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
                this.ReferenceBuildPlacement = new ComponentPlacement(PlacementAction.Replace,
                    "Resolve Reference Links Component");
                this.ConceptualBuildPlacement = new ComponentPlacement(PlacementAction.Replace,
                    "Resolve Reference Links Component");
            }

            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new ESentResolveReferenceLinksComponent(this.BuildAssembler);
            }

            /// <inheritdoc />
            public override string DefaultConfiguration => @"{@HRefFormat}
<locale value=""{@Locale}"" />
<linkTarget value=""{@SdkLinkTarget}"" />

<helpOutput format=""HtmlHelp1"">
	<memberIdUrlCache path=""{@LocalDataFolder}Cache\MemberIdUrl.cache""
		cachePath=""{@LocalDataFolder}Cache\ESentMemberIdUrlCache"" localCacheSize=""2500"" />
	<targets base=""{@FrameworkReflectionDataFolder}"" recurse=""true"" files=""*.xml""
		type=""{@HtmlSdkLinkType}"" id=""FrameworkTargets""
		cachePath=""{@LocalDataFolder}Cache\ESentFrameworkTargetCache"" localCacheSize=""2500"">
		{@ReferenceLinkNamespaceFiles}
	</targets>
	<targets files=""reflection.xml"" type=""Local"" id=""ProjectTargets"" />
</helpOutput>

<helpOutput format=""MSHelpViewer"">
	<memberIdUrlCache path=""{@LocalDataFolder}Cache\MemberIdUrl.cache""
		cachePath=""{@LocalDataFolder}Cache\ESentMemberIdUrlCache"" localCacheSize=""2500"" />
	<targets base=""{@FrameworkReflectionDataFolder}"" recurse=""true"" files=""*.xml""
		type=""{@MSHelpViewerSdkLinkType}"" id=""FrameworkTargets""
		cachePath=""{@LocalDataFolder}Cache\ESentFrameworkTargetCache"" localCacheSize=""2500"">
		{@ReferenceLinkNamespaceFiles}
	</targets>
	<targets files=""reflection.xml"" type=""Id"" id=""ProjectTargets"" />
</helpOutput>

<helpOutput format=""Website"">
	<memberIdUrlCache path=""{@LocalDataFolder}Cache\MemberIdUrl.cache""
		cachePath=""{@LocalDataFolder}Cache\ESentMemberIdUrlCache"" localCacheSize=""2500"" />
	<targets base=""{@FrameworkReflectionDataFolder}"" recurse=""true"" files=""*.xml""
		type=""{@WebsiteSdkLinkType}"" id=""FrameworkTargets""
		cachePath=""{@LocalDataFolder}Cache\ESentFrameworkTargetCache"" localCacheSize=""2500"">
		{@ReferenceLinkNamespaceFiles}
	</targets>
	<targets files=""reflection.xml"" type=""Local"" id=""ProjectTargets"" />
</helpOutput>

<helpOutput format=""OpenXml"">
	<memberIdUrlCache path=""{@LocalDataFolder}Cache\MemberIdUrl.cache""
		cachePath=""{@LocalDataFolder}Cache\ESentMemberIdUrlCache"" localCacheSize=""2500"" />
	<targets base=""{@FrameworkReflectionDataFolder}"" recurse=""true"" files=""*.xml""
		type=""{@WebsiteSdkLinkType}"" id=""FrameworkTargets""
		cachePath=""{@LocalDataFolder}Cache\ESentFrameworkTargetCache"" localCacheSize=""2500"">
		{@ReferenceLinkNamespaceFiles}
	</targets>
	<targets files=""reflection.xml"" type=""Local"" id=""ProjectTargets"" />
</helpOutput>

<helpOutput format=""Markdown"">
	<memberIdUrlCache path=""{@LocalDataFolder}Cache\MemberIdUrl.cache""
		cachePath=""{@LocalDataFolder}Cache\ESentMemberIdUrlCache"" localCacheSize=""2500"" />
	<targets base=""{@FrameworkReflectionDataFolder}"" recurse=""true"" files=""*.xml""
		type=""{@WebsiteSdkLinkType}"" id=""FrameworkTargets""
		cachePath=""{@LocalDataFolder}Cache\ESentFrameworkTargetCache"" localCacheSize=""2500"">
		{@ReferenceLinkNamespaceFiles}
	</targets>
	<targets files=""reflection.xml"" type=""Local"" id=""ProjectTargets"" />
</helpOutput>";
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
        protected ESentResolveReferenceLinksComponent(IBuildAssembler buildAssembler) : base(buildAssembler)
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

            this.WriteMessage(MessageLevel.Info, String.Format(CultureInfo.InvariantCulture,
                "[{0}, version {1}]\r\n    ESENT Resolve Reference Links Component.  {2}\r\n" +
                "    https://GitHub.com/EWSoftware/SHFB", fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright));

            base.Initialize(configuration);
        }

        /// <summary>
        /// This is overridden to allow use of an ESENT backed member ID URL cache
        /// </summary>
        /// <param name="configuration">The component configuration</param>
        /// <returns>A member ID URL resolver instance</returns>
        protected override IMemberIdUrlResolver CreateMemberIdResolver(XPathNavigator configuration)
        {
            IMemberIdUrlResolver resolver;
            IDictionary<string, string> cache = null;

            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if(this.BuildAssembler.Data.ContainsKey(SharedMemberUrlCacheId))
                cache = this.BuildAssembler.Data[SharedMemberUrlCacheId] as IDictionary<string, string>;

            // If the shared cache already exists, return an instance that uses it.  It is assumed that all
            // subsequent instances will use the same cache.
            if(cache != null)
                return base.CreateMemberIdResolver(cache, true);

            XPathNavigator node = configuration.SelectSingleNode("memberIdUrlCache");

            // If a memberIdUrlCache element is not specified, use the default resolver
            if(node == null)
                resolver = base.CreateMemberIdResolver(configuration);
            else
            {
                string cachePath = node.GetAttribute("cachePath", String.Empty);

                // If a cache path is not defined, use the default resolver
                if(String.IsNullOrWhiteSpace(cachePath))
                    resolver = base.CreateMemberIdResolver(configuration);
                else
                {
                    cachePath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(cachePath));

                    string cacheSize = node.GetAttribute("localCacheSize", String.Empty);

                    if(String.IsNullOrWhiteSpace(cacheSize) || !Int32.TryParse(cacheSize, out int localCacheSize))
                        localCacheSize = 2500;

                    // Load or create the cache database and the resolver.  The resolver will dispose of the
                    // dictionary when it is disposed of since it implements IDisposable.  We won't compress the
                    // columns as they're typically not that big and there aren't usually that many entries.
                    // This gives a slight performance increase.
                    resolver = base.CreateMemberIdResolver(new PersistentDictionary<string, string>(cachePath, false)
                        { LocalCacheSize = localCacheSize }, false);

                    // We own the cache and will report statistics when done
                    ownsResolverCache = true;

                    int cacheCount = resolver.CachedUrls.Count;

                    if(cacheCount == 0)
                    {
                        // Log a diagnostic message since looking up all IDs can significantly slow the build
                        this.WriteMessage(MessageLevel.Diagnostic, "The ESENT member ID URL cache in '" +
                            cachePath + "' does not exist yet.  All IDs will be looked up in this build " +
                            "which will slow it down.");
                    }
                    else
                        this.WriteMessage(MessageLevel.Info, "{0} cached member ID URL entries exist", cacheCount);

                    this.BuildAssembler.Data[SharedMemberUrlCacheId] = resolver.CachedUrls;
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

            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

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
                    this.WriteMessage(MessageLevel.Error, BuildComponentUtilities.GetExceptionMessage(ex));
                }
            }

            return td;
        }

        /// <summary>
        /// This is overridden to report the persistent cache information
        /// </summary>
        public override void UpdateUrlCache()
        {
            if(ownsResolverCache && this.UrlResolver != null)
            {
                if(this.UrlResolver.CachedUrls is PersistentDictionary<string, string> cache)
                {
                    if(this.UrlResolver.CacheItemsAdded != 0)
                        this.WriteMessage(MessageLevel.Diagnostic, "{0} entries added to the member ID URL " +
                            "cache.  New cache size: {1} entries", this.UrlResolver.CacheItemsAdded, cache.Count);

                    this.WriteMessage(MessageLevel.Diagnostic, "Member ID URL ESENT local cache flushed {0} " +
                        "time(s).  Current ESENT local cache usage: {1} of {2}.", cache.LocalCacheFlushCount,
                        cache.CurrentLocalCacheCount, cache.LocalCacheSize);
                }
            }

            base.UpdateUrlCache();
        }
        #endregion
    }
}
