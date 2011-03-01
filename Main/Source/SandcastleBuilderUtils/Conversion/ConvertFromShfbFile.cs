//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ConvertFromShfbFile.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/09/2010
// Note    : Copyright 2008-2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used to convert version 1.7.0.0 and prior SHFB
// project files to the new MSBuild format project files.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  07/23/2008  EFW  Created the code
// 1.9.0.0  06/20/2010  EFW  Removed ProjectLinkType property
// 1.9.1.0  07/09/2010  EFW  Updated for use with .NET 4.0 and MSBuild 4.0.
//=============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace SandcastleBuilder.Utils.Conversion
{
    /// <summary>
    /// This class is used to convert version 1.7.0.0 and prior SHFB project
    /// files to the new MSBuild format project files.
    /// </summary>
    public sealed class ConvertFromShfbFile : ConvertToMSBuildFormat
    {
        #region Private data members
        //=====================================================================

        private XmlTextReader xr;
        private ConvertConceptualContent contentConverter;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// Get the XML text reader used for the conversion
        /// </summary>
        protected internal override XmlTextReader Reader
        {
            get { return xr; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="oldProjectFile">The old project filename</param>
        /// <param name="folder">The folder in which to place the new project
        /// and its related files.  This cannot be the same folder as the
        /// old project file.</param>
        public ConvertFromShfbFile(string oldProjectFile, string folder) :
          base(oldProjectFile, folder)
        {
            contentConverter = new ConvertConceptualContent(this);
        }
        #endregion

        #region Conversion methods
        //=====================================================================

        /// <summary>
        /// This is used to perform the actual conversion
        /// </summary>
        /// <returns>The new project filename on success.  An exception is
        /// thrown if the conversion fails.</returns>
        public override string ConvertProject()
        {
            FilePath filePath;
            Version schemaVersion, lastShfbVersion = new Version(1, 7, 0, 0);
            Dictionary<string, string> renameProps = new Dictionary<string, string>();

            object value;
            string version, deps, propName = null, path, dest, filter, helpFileFormat;
            string[] depList;

            // Create a list of property names that need renaming due to
            // changes from version to version.
            renameProps.Add("showAttributes", "DocumentAttributes");        // v1.0.0.0 name
            renameProps.Add("hhcPath", "HtmlHelp1xCompilerPath");           // v1.3.3.1 and prior
            renameProps.Add("hxcompPath", "HtmlHelp2xCompilerPath");        // v1.3.3.1 and prior
            renameProps.Add("rootNSContainer", "RootNamespaceContainer");   // v1.3.3.1 and prior

            // The HelpFileFormat enum values changed in v1.8.0.3
            Dictionary<string, string> translateFormat = new Dictionary<string, string> {
                { "HTMLHELP1X", "HtmlHelp1" },
                { "HTMLHELP2X", "MSHelp2" },
                { "HELP1XANDHELP2X", "HtmlHelp1, MSHelp2" },
                { "HELP1XANDWEBSITE", "HtmlHelp1, Website" },
                { "HELP2XANDWEBSITE", "MSHelp2, Website" },
                { "HELP1XAND2XANDWEBSITE", "HtmlHelp1, MSHelp2, Website" } };

            try
            {
                xr = new XmlTextReader(new StreamReader(base.OldProjectFile));
                xr.MoveToContent();

                if(xr.EOF)
                {
                    base.Project.SaveProject(base.Project.Filename);
                    return base.Project.Filename;
                }

                version = xr.GetAttribute("schemaVersion");

                if(String.IsNullOrEmpty(version))
                    throw new BuilderException("CVT0003", "Invalid or missing schema version");

                schemaVersion = new Version(version);

                if(schemaVersion > lastShfbVersion)
                    throw new BuilderException("CVT0004", "Unrecognized schema version");

                while(!xr.EOF)
                {
                    if(xr.NodeType == XmlNodeType.Element)
                    {
                        propName = xr.Name;

                        switch(propName)
                        {
                            case "project":     // Ignore the main project node
                                break;

                            case "PurgeDuplicateTopics":
                            case "ShowFeedbackControl":
                            case "ProjectLinkType":
                            case "projectLinks":    // ProjectLinkType in v1.3.3.1 and prior
                                // PurgeDuplicateTopics was removed in v1.6.0.7
                                // ShowFeedbackControl was removed in v1.8.0.3
                                // ProjectLinkType was removed in v1.9.0.0
                                break;

                            case "sdkLinks":        // SdkLinkType in v1.3.3.1 and prior
                            case "SdkLinkType":
                                switch(xr.ReadString().ToLowerInvariant())
                                {
                                    case "none":
                                    case "index":
                                    case "local":
                                        base.Project.HtmlSdkLinkType = base.Project.WebsiteSdkLinkType =
                                            HtmlSdkLinkType.None;
                                        base.Project.MSHelp2SdkLinkType = MSHelp2SdkLinkType.Index;
                                        base.Project.MSHelpViewerSdkLinkType = MSHelpViewerSdkLinkType.Id;
                                        break;

                                    default:    // MSDN links
                                        break;
                                }
                                break;

                            case "additionalContent":
                                this.ConvertAdditionalContent();
                                break;

                            case "conceptualContent":
                                contentConverter.ConvertContent();
                                break;

                            case "assemblies":
                                this.ConvertAssemblies();
                                break;

                            case "componentConfigurations":
                                this.ConvertComponentConfigs();
                                break;

                            case "plugInConfigurations":
                                this.ConvertPlugInConfigs();
                                break;

                            case "namespaceSummaries":
                                this.ConvertNamespaceSummaries();
                                break;

                            case "apiFilter":
                                this.ConvertApiFilter();
                                break;

                            case "helpAttributes":
                                this.ConvertHelpAttributes();
                                break;

                            case "dependencies":
                                // The first version used a comma-separated
                                // string of dependencies.
                                if(schemaVersion.Major == 1 && schemaVersion.Minor == 0)
                                {
                                    deps = xr.ReadString();

                                    if(deps.Length != 0)
                                    {
                                        depList = deps.Split(new char[] { ',' });

                                        foreach(string s in depList)
                                            base.Project.References.AddReference(
                                                Path.GetFileNameWithoutExtension(s),
                                                base.FullPath(s));
                                    }
                                }
                                else
                                    this.ConvertDependencies();
                                break;

                            case "SyntaxFilters":
                                // 1.6.0.4 used ScriptSharp but 1.6.0.5 renamed
                                // it to JavaScript which is more generic as it
                                // can apply to other projects too.
                                filter = xr.ReadString();

                                if(schemaVersion.Major == 1 &&
                                  schemaVersion.Minor == 6 &&
                                  schemaVersion.Build == 0 &&
                                  schemaVersion.Revision < 5)
                                    filter = filter.Replace("ScriptSharp",
                                        "JavaScript");

                                base.SetProperty(xr.Name, filter);
                                break;

                            case "ContentSiteMap":
                                // In 1.6.0.7, this became a sub-property
                                // of the additional content collection.
                                path = xr.GetAttribute("path");

                                if(path != null && path.Trim().Length > 0)
                                {
                                    dest = Path.Combine(base.ProjectFolder,
                                        Path.GetFileName(path));
                                    dest = Path.ChangeExtension(dest,
                                        ".sitemap");

                                    base.Project.AddFileToProject(
                                        base.FullPath(path), dest);
                                    this.UpdateSiteMap(dest);
                                }
                                break;

                            case "TopicFileTransform":
                                // In 1.6.0.7, this became a sub-property
                                // of the additional content collection.
                                path = xr.GetAttribute("path");

                                if(path != null && path.Trim().Length > 0)
                                {
                                    dest = Path.Combine(base.ProjectFolder,
                                        Path.GetFileName(path));
                                    dest = Path.ChangeExtension(dest, ".xsl");
                                    base.Project.AddFileToProject(
                                        base.FullPath(path), dest);
                                }
                                break;

                            case "HelpFileFormat":
                                // The enum value names changed in v1.8.0.3
                                helpFileFormat = xr.ReadString().ToUpper(
                                    CultureInfo.InvariantCulture);

                                foreach(string key in translateFormat.Keys)
                                    helpFileFormat = helpFileFormat.Replace(key,
                                        translateFormat[key]);

                                base.SetProperty(propName, helpFileFormat);
                                break;

                            default:
                                if(renameProps.ContainsKey(propName))
                                    propName = renameProps[propName];

                                value = base.SetProperty(propName, xr.ReadString());

                                filePath = value as FilePath;

                                // For file and folder paths, set the value
                                // from the attribute if present.
                                path = xr.GetAttribute("path");

                                if(filePath != null && !String.IsNullOrEmpty(path))
                                {
                                    // Adjust relative paths for the new location
                                    if(Path.IsPathRooted(path))
                                    {
                                        filePath.Path = path;
                                        filePath.IsFixedPath = true;
                                    }
                                    else
                                        filePath.Path = base.FullPath(path);
                                }
                                break;
                        }
                    }

                    xr.Read();
                }

                // The default for SealedProtected changed to true in v1.3.1.1
                Version changeVer = new Version(1, 3, 1, 1);

                if(schemaVersion < changeVer)
                    base.Project.DocumentSealedProtected = true;

                // Missing namespaces were always indicated prior to v1.4.0.2
                changeVer = new Version(1, 4, 0, 2);

                if(schemaVersion < changeVer)
                    base.Project.ShowMissingNamespaces = true;

                // The missing type parameters option was added in 1.6.0.7
                changeVer = new Version(1, 6, 0, 7);

                if(schemaVersion < changeVer)
                    base.Project.ShowMissingTypeParams = true;

                base.CreateFolderItems();
                base.Project.SaveProject(base.Project.Filename);
            }
            catch(Exception ex)
            {
                throw new BuilderException("CVT0005", String.Format(
                    CultureInfo.CurrentCulture, "Error reading project " +
                    "from '{0}' (last property = {1}):\r\n{2}",
                    base.OldProjectFile, propName, ex.Message), ex);
            }
            finally
            {
                if(xr != null)
                    xr.Close();
            }

            return base.Project.Filename;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Convert the additional content
        /// </summary>
        private void ConvertAdditionalContent()
        {
            Dictionary<string, string> includeFileSpec = new Dictionary<string, string>(),
                excludeFileSpec = new Dictionary<string, string>();
            HashSet<string> excludeFiles = new HashSet<string>();
            bool exclude;
            string source, dest, destFile;

            // Add content site map if defined
            source = xr.GetAttribute("contentSiteMap");

            if(source != null && source.Trim().Length > 0)
            {
                dest = Path.Combine(base.ProjectFolder, Path.GetFileName(source));
                dest = Path.ChangeExtension(dest, ".sitemap");
                base.Project.AddFileToProject(base.FullPath(source), dest);
                this.UpdateSiteMap(dest);
            }

            // Add topic file transformation if defined
            source = xr.GetAttribute("topicFileTransform");

            if(source != null && source.Trim().Length > 0)
            {
                dest = Path.Combine(base.ProjectFolder, Path.GetFileName(source));
                dest = Path.ChangeExtension(dest, ".xsl");
                base.Project.AddFileToProject(base.FullPath(source), dest);
            }

            if(!xr.IsEmptyElement)
                while(!xr.EOF && xr.NodeType != XmlNodeType.EndElement)
                {
                    if(xr.NodeType == XmlNodeType.Element &&
                      xr.Name == "contentItem")
                    {
                        source = xr.GetAttribute("sourcePath");
                        dest = xr.GetAttribute("destPath");
                        exclude = Convert.ToBoolean(xr.GetAttribute(
                            "excludeItems"), CultureInfo.InvariantCulture);

                        if(!exclude)
                            includeFileSpec[base.FullPath(source)] = dest;
                        else
                            excludeFileSpec[base.FullPath(source)] = dest;
                    }

                    xr.Read();
                }

            // Add included files less excluded files
            foreach(string fileSpec in excludeFileSpec.Keys)
                foreach(string file in ExpandWildcard(fileSpec, true))
                    excludeFiles.Add(file);

            // Copy files to the destination folder off of the root of the
            // new project.  Note that this may change the folder layout from
            // the original project.  This is by design as when the project is
            // built, the project folder layout is used as the folder layout
            // in the compiled help file.
            foreach(string fileSpec in includeFileSpec.Keys)
            {
                source = Path.GetDirectoryName(fileSpec);
                dest = Path.GetFullPath(Path.Combine(base.ProjectFolder,
                    includeFileSpec[fileSpec]));

                foreach(string file in ExpandWildcard(fileSpec, true))
                    if(!excludeFiles.Contains(file))
                    {
                        // Remove the base path but keep any subfolders
                        destFile = file.Substring(source.Length + 1);
                        destFile = Path.Combine(dest, destFile);
                        base.Project.AddFileToProject(file, destFile);
                    }
            }
        }

        /// <summary>
        /// Convert the assemblies
        /// </summary>
        private void ConvertAssemblies()
        {
            string asmPath, xmlPath;

            while(!xr.EOF && xr.NodeType != XmlNodeType.EndElement)
            {
                if(xr.NodeType == XmlNodeType.Element && xr.Name == "assembly")
                {
                    asmPath = xr.GetAttribute("assemblyPath");
                    xmlPath = xr.GetAttribute("xmlCommentsPath");

                    if(!String.IsNullOrEmpty(asmPath))
                        base.Project.DocumentationSources.Add(base.FullPath(
                            asmPath), null, null, false);

                    if(!String.IsNullOrEmpty(xmlPath))
                        base.Project.DocumentationSources.Add(base.FullPath(
                            xmlPath), null, null, false);
                }

                xr.Read();
            }
        }

        /// <summary>
        /// Convert the component configurations
        /// </summary>
        private void ConvertComponentConfigs()
        {
            string id, config, state;
            bool enabled;

            while(!xr.EOF && xr.NodeType != XmlNodeType.EndElement)
            {
                if(xr.NodeType == XmlNodeType.Element &&
                  xr.Name == "component")
                {
                    id = xr.GetAttribute("id");

                    // Enabled wasn't in versions prior to 1.6.0.2
                    state = xr.GetAttribute("enabled");

                    if(String.IsNullOrEmpty(state))
                        enabled = true;
                    else
                        enabled = Convert.ToBoolean(state,
                            CultureInfo.InvariantCulture);

                    config = xr.GetAttribute("configuration");
                    base.Project.ComponentConfigurations.Add(id, enabled, config);
                    base.Project.ComponentConfigurations.IsDirty = true;
                }

                xr.Read();
            }
        }

        /// <summary>
        /// Convert the plug-in configurations
        /// </summary>
        private void ConvertPlugInConfigs()
        {
            string id, config;
            bool enabled;

            while(!xr.EOF && xr.NodeType != XmlNodeType.EndElement)
            {
                if(xr.NodeType == XmlNodeType.Element &&
                  xr.Name == "plugIn")
                {
                    id = xr.GetAttribute("id");
                    enabled = Convert.ToBoolean(xr.GetAttribute(
                        "enabled"), CultureInfo.InvariantCulture);
                    config = xr.GetAttribute("configuration");

                    base.Project.PlugInConfigurations.Add(id, enabled, config);
                    base.Project.PlugInConfigurations.IsDirty = true;
                }

                xr.Read();
            }
        }

        /// <summary>
        /// Convert the namespace summaries
        /// </summary>
        private void ConvertNamespaceSummaries()
        {
            string name, summary;
            bool isDocumented;

            while(!xr.EOF && xr.NodeType != XmlNodeType.EndElement)
            {
                if(xr.NodeType == XmlNodeType.Element &&
                  xr.Name == "namespaceSummaryItem")
                {
                    name = xr.GetAttribute("name");
                    isDocumented = Convert.ToBoolean(xr.GetAttribute(
                        "isDocumented"), CultureInfo.InvariantCulture);
                    summary = xr.ReadString();

                    base.Project.NamespaceSummaries.Add(name, isDocumented,
                        summary);
                }

                xr.Read();
            }
        }

        /// <summary>
        /// Convert the API filter
        /// </summary>
        private void ConvertApiFilter()
        {
            string filter = xr.ReadInnerXml();

            filter = filter.Replace("<filter", "<Filter");
            filter = filter.Replace("</filter", "</Filter");

            base.Project.MSBuildProject.SetProperty("ApiFilter", filter);
        }

        /// <summary>
        /// Convert the help attributes
        /// </summary>
        private void ConvertHelpAttributes()
        {
            string name, value;

            while(!xr.EOF && xr.NodeType != XmlNodeType.EndElement)
            {
                if(xr.NodeType == XmlNodeType.Element &&
                  xr.Name == "helpAttribute")
                {
                    name = xr.GetAttribute("name");
                    value = xr.GetAttribute("value");

                    if(!String.IsNullOrEmpty(name))
                        base.Project.HelpAttributes.Add(name, value);
                }

                xr.Read();
            }
        }

        /// <summary>
        /// Convert the dependencies
        /// </summary>
        private void ConvertDependencies()
        {
            string dep, ext;

            while(!xr.EOF && xr.NodeType != XmlNodeType.EndElement)
            {
                if(xr.NodeType == XmlNodeType.Element &&
                  xr.Name == "dependencyItem")
                {
                    dep = xr.GetAttribute("depPath");

                    if(dep.StartsWith("GAC:", StringComparison.Ordinal))
                        base.Project.References.AddReference(dep.Substring(4), null);
                    else
                        if(dep.IndexOfAny(new char[] { '*', '?' }) == -1)
                            base.Project.References.AddReference(
                                Path.GetFileNameWithoutExtension(dep),
                                base.FullPath(dep));
                        else
                            foreach(string file in ExpandWildcard(
                              base.FullPath(dep), false))
                            {
                                ext = Path.GetExtension(file).ToLower(
                                    CultureInfo.InvariantCulture);

                                if(ext == ".dll" || ext == ".exe")
                                    base.Project.References.AddReference(
                                        Path.GetFileNameWithoutExtension(file),
                                        file);
                            }
                }

                xr.Read();
            }
        }

        /// <summary>
        /// This is used to copy the files referenced in a site map file into
        /// the project and update the URLs in it to point to the new copies.
        /// </summary>
        /// <param name="siteMap">The site map file to update.</param>
        private void UpdateSiteMap(string siteMap)
        {
            XmlDocument doc = new XmlDocument();
            FileItem newItem;
            string source, dest, basePath = this.OldFolder;

            doc.Load(siteMap);
            XmlNamespaceManager nsm = new XmlNamespaceManager(doc.NameTable);
            nsm.AddNamespace("sm", "http://schemas.microsoft.com/AspNet/SiteMap-File-1.0");

            // NOTE: If files appear in here and in the AdditionalContent
            // property, it may add duplicate files in a different location
            // due to the restructuring of the AdditionalContent folder layout.
            foreach(XmlNode node in doc.SelectNodes("//sm:siteMapNode/@url", nsm))
                if(!String.IsNullOrEmpty(node.Value))
                {
                    source = base.FullPath(node.Value);

                    // If not in the base path, it will be added to the root
                    // folder for the project.
                    if(!source.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                        dest = Path.Combine(base.ProjectFolder,
                            Path.GetFileName(source));
                    else
                        dest = Path.Combine(base.ProjectFolder,
                            source.Substring(basePath.Length));

                    newItem = base.Project.AddFileToProject(source, dest);
                    node.Value = newItem.Include.PersistablePath;
                }

            doc.Save(siteMap);
        }
        #endregion
    }
}
