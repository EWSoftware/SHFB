﻿//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : SqlResolveReferenceLinksComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/24/2021
//
// This is a version of the ResolveReferenceLinksComponent that stores the member ID URLs and the framework
// targets in persistent SQL database tables.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/14/2013  EFW  Created the code
// 12/26/2013  EFW  Updated the build component to be discoverable via MEF
// 04/12/2021  EFW  Merged SHFB build components into the main build components assembly
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

using Sandcastle.Tools.BuildComponents;
using Sandcastle.Tools.BuildComponents.Targets;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This is a version of the <c>ResolveReferenceLinksComponent</c> that stores the member ID URLs and the
    /// framework targets in persistent SQL databases.
    /// </summary>
    public class SqlResolveReferenceLinksComponent : ResolveReferenceLinksComponent
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component used to resolve reference links
        /// </summary>
        [BuildComponentExport("Resolve Reference Links (SQL Cache)", IsVisible = true,
          Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
          Description = "This component is used to resolve reference links in topics.  It uses a Microsoft " +
            "SQL Server database to cache member ID URLs and .NET Framework reference link targets.  This " +
            "speeds up initialization and conserves memory at the expense of some build time in larger " +
            "projects.  For extremely large projects, it is also possible to cache project reference link " +
            "data to conserve memory.\r\n\r\nThis component requires access to a Microsoft SQL Server " +
            "instance.  Express and LocalDB versions are supported.  Some initial configuration and set up " +
            "steps are required.")]
        public sealed class SqlResolveReferenceLinksComponentFactory : BuildComponentFactory
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public SqlResolveReferenceLinksComponentFactory()
            {
                this.ReferenceBuildPlacement = new ComponentPlacement(PlacementAction.Replace,
                    "Resolve Reference Links Component");
                this.ConceptualBuildPlacement = new ComponentPlacement(PlacementAction.Replace,
                    "Resolve Reference Links Component");
            }

            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new SqlResolveReferenceLinksComponent(this.BuildAssembler);
            }

            /// <inheritdoc />
            public override string DefaultConfiguration => @"{@HRefFormat}
<locale value=""{@Locale}"" />
<linkTarget value=""{@SdkLinkTarget}"" />

<helpOutput format=""HtmlHelp1"">
	<sqlCache connectionString="""" urlLocalCacheSize=""2500"" frameworkLocalCacheSize=""2500""
		projectLocalCacheSize=""2500"" cacheProject=""false"" />
	<memberIdUrlCache path=""{@LocalDataFolder}Cache\MemberIdUrl.cache"" />
	<targets base=""{@FrameworkReflectionDataFolder}"" recurse=""true"" files=""*.xml""
		type=""{@HtmlSdkLinkType}"" groupId=""FrameworkTargets"">
		{@ReferenceLinkNamespaceFiles}
	</targets>
	<targets files=""reflection.xml"" type=""Local"" groupId=""Project_{@UniqueID}"" />
</helpOutput>

<helpOutput format=""MSHelpViewer"">
	<sqlCache connectionString="""" urlLocalCacheSize=""2500"" frameworkLocalCacheSize=""2500""
		projectLocalCacheSize=""2500"" cacheProject=""false"" />
	<memberIdUrlCache path=""{@LocalDataFolder}Cache\MemberIdUrl.cache"" />
	<targets base=""{@FrameworkReflectionDataFolder}"" recurse=""true"" files=""*.xml""
		type=""{@MSHelpViewerSdkLinkType}"" groupId=""FrameworkTargets"">
		{@ReferenceLinkNamespaceFiles}
	</targets>
	<targets files=""reflection.xml"" type=""Id"" groupId=""Project_{@UniqueID}"" />
</helpOutput>

<helpOutput format=""Website"">
	<sqlCache connectionString="""" urlLocalCacheSize=""2500"" frameworkLocalCacheSize=""2500""
		projectLocalCacheSize=""2500"" cacheProject=""false"" />
	<memberIdUrlCache path=""{@LocalDataFolder}Cache\MemberIdUrl.cache"" />
	<targets base=""{@FrameworkReflectionDataFolder}"" recurse=""true"" files=""*.xml""
		type=""{@WebsiteSdkLinkType}"" groupId=""FrameworkTargets"">
		{@ReferenceLinkNamespaceFiles}
	</targets>
	<targets files=""reflection.xml"" type=""Local"" groupId=""Project_{@UniqueID}"" />
</helpOutput>

<helpOutput format=""OpenXml"">
	<sqlCache connectionString="""" urlLocalCacheSize=""2500"" frameworkLocalCacheSize=""2500""
		projectLocalCacheSize=""2500"" cacheProject=""false"" />
	<memberIdUrlCache path=""{@LocalDataFolder}Cache\MemberIdUrl.cache"" />
	<targets base=""{@FrameworkReflectionDataFolder}"" recurse=""true"" files=""*.xml""
		type=""{@WebsiteSdkLinkType}"" groupId=""FrameworkTargets"">
		{@ReferenceLinkNamespaceFiles}
	</targets>
	<targets files=""reflection.xml"" type=""Local"" groupId=""Project_{@UniqueID}"" />
</helpOutput>

<helpOutput format=""Markdown"">
	<sqlCache connectionString="""" urlLocalCacheSize=""2500"" frameworkLocalCacheSize=""2500""
		projectLocalCacheSize=""2500"" cacheProject=""false"" />
	<memberIdUrlCache path=""{@LocalDataFolder}Cache\MemberIdUrl.cache"" />
	<targets base=""{@FrameworkReflectionDataFolder}"" recurse=""true"" files=""*.xml""
		type=""{@WebsiteSdkLinkType}"" groupId=""FrameworkTargets"">
		{@ReferenceLinkNamespaceFiles}
	</targets>
	<targets files=""reflection.xml"" type=""Local"" groupId=""Project_{@UniqueID}"" />
</helpOutput>";
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected SqlResolveReferenceLinksComponent(BuildAssemblerCore buildAssembler) : base(buildAssembler)
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
                "[{0}, version {1}]\r\n    SQL Resolve Reference Links Component.  {2}\r\n" +
                "    https://GitHub.com/EWSoftware/SHFB", fvi.ProductName, fvi.ProductVersion,
                fvi.LegalCopyright));

            base.Initialize(configuration);
        }

        /// <summary>
        /// This is overridden to allow use of an SQL backed member ID URL cache
        /// </summary>
        /// <param name="configuration">The component configuration</param>
        /// <returns>A member ID URL resolver instance</returns>
        protected override IMemberIdUrlResolver CreateMemberIdResolver(XPathNavigator configuration)
        {
            IMemberIdUrlResolver resolver;
            IDictionary<string, string> cache = null;

            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if(Data.ContainsKey(SharedMemberUrlCacheId))
                cache = Data[SharedMemberUrlCacheId] as IDictionary<string, string>;

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
                node = configuration.SelectSingleNode("sqlCache");
                string connectionString = node.GetAttribute("connectionString", String.Empty);

                // If a connection string is not defined, use the default resolver
                if(String.IsNullOrWhiteSpace(connectionString))
                    resolver = base.CreateMemberIdResolver(configuration);
                else
                {
                    string cacheSize = node.GetAttribute("urlLocalCacheSize", String.Empty);

                    if(String.IsNullOrWhiteSpace(cacheSize) || !Int32.TryParse(cacheSize, out int localCacheSize))
                        localCacheSize = 2500;

                    // Load or create the cache database and the resolver.  The resolver will dispose of the
                    // dictionary when it is disposed of since it implements IDisposable.
                    resolver = base.CreateMemberIdResolver(new SqlDictionary<string>(connectionString, "MemberIdUrls",
                        "TargetKey", "MemberUrl") { LocalCacheSize = localCacheSize }, false);

                    int cacheCount = resolver.CachedUrls.Count;

                    if(cacheCount == 0)
                    {
                        // Log a diagnostic message since looking up all IDs can significantly slow the build
                        this.WriteMessage(MessageLevel.Diagnostic, "The SQL member ID URL cache in '" +
                            connectionString + "' does not exist yet.  All IDs will be looked up in this " +
                            "build which will slow it down.");
                    }
                    else
                        this.WriteMessage(MessageLevel.Info, "{0} cached member ID URL entries exist", cacheCount);

                    Data[SharedMemberUrlCacheId] = resolver.CachedUrls;
                }
            }

            return resolver;
        }

        /// <summary>
        /// This is overridden to create a target dictionary that utilizes an SQL database for persistence
        /// </summary>
        /// <param name="configuration">The configuration element for the target dictionary</param>
        /// <returns>A simple dictionary if no <c>connectionString</c> attribute is found or a SQL backed target
        /// dictionary if the attribute is found.</returns>
        public override TargetDictionary CreateTargetDictionary(XPathNavigator configuration)
        {
            TargetDictionary td = null;
            string connectionString, groupId, attrValue;
            int frameworkCacheSize, projectCacheSize;
            bool cacheProject, isProjectData;

            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var parent = configuration.Clone();
            parent.MoveToParent();

            var cache = parent.SelectSingleNode("sqlCache");

            connectionString = cache.GetAttribute("connectionString", String.Empty);

            attrValue = cache.GetAttribute("frameworkLocalCacheSize", String.Empty);
            frameworkCacheSize = Convert.ToInt32(attrValue, CultureInfo.InvariantCulture);

            attrValue = cache.GetAttribute("projectLocalCacheSize", String.Empty);
            projectCacheSize = Convert.ToInt32(attrValue, CultureInfo.InvariantCulture);

            attrValue = cache.GetAttribute("cacheProject", String.Empty);
            cacheProject = Convert.ToBoolean(attrValue, CultureInfo.InvariantCulture);

            groupId = configuration.GetAttribute("groupId", String.Empty);
            isProjectData = groupId.StartsWith("Project_", StringComparison.OrdinalIgnoreCase);

            // If no connection is specified or if it is project data and we aren't caching it, use the simple
            // target dictionary.
            if(String.IsNullOrWhiteSpace(connectionString) || (isProjectData && !cacheProject))
                td = base.CreateTargetDictionary(configuration);
            else
            {
                try
                {
                    td = new SqlTargetDictionary(this, configuration, connectionString, groupId,
                        isProjectData ? projectCacheSize : frameworkCacheSize, isProjectData);
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
            if(this.UrlResolver != null)
            {
                // Only report if we own the cache (it won't have been disposed off yet)
                if(this.UrlResolver.CachedUrls is SqlDictionary<string> cache && !cache.IsDisposed)
                {
                    if(this.UrlResolver.CacheItemsAdded != 0)
                        this.WriteMessage(MessageLevel.Diagnostic, "{0} entries added to the member ID URL " +
                            "cache.  New cache size: {1} entries", this.UrlResolver.CacheItemsAdded, cache.Count);

                    this.WriteMessage(MessageLevel.Diagnostic, "member ID URL SQL local cache flushed {0} " +
                        "time(s).  Current SQL local cache usage: {1} of {2}.", cache.LocalCacheFlushCount,
                        cache.CurrentLocalCacheCount, cache.LocalCacheSize);
                }
            }

            base.UpdateUrlCache();
        }
        #endregion
    }
}
