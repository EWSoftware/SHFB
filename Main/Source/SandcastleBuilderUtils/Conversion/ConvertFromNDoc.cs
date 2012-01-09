//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ConvertFromNDoc.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/08/2012
// Note    : Copyright 2008-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used to convert NDoc 1.x project files to the
// MSBuild format project files used by SHFB.
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
// 1.9.3.4  01/08/2012  EFW  Added constructor to support use from VSPackage
//=============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Utils.Conversion
{
    /// <summary>
    /// This class is used to convert NDoc 1.x project files to the MSBuild
    /// format project files used by the help file builder.
    /// </summary>
    public class ConvertFromNDoc : ConvertToMSBuildFormat
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// An XML reader isn't used by this converter
        /// </summary>
        protected internal override XmlTextReader Reader
        {
            get { return null; }
        }
        #endregion

        #region Constructors
        //=====================================================================

        /// <inheritdoc />
        public ConvertFromNDoc(string oldProjectFile, string folder) : base(oldProjectFile, folder)
        {
        }

        /// <inheritdoc />
        public ConvertFromNDoc(string oldProjectFile, SandcastleProject newProject) :
          base(oldProjectFile, newProject)
        {
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
            XPathDocument sourceFile;
            XPathNavigator navNDoc;
            SearchOption searchOpts;
            SandcastleProject project = base.Project;
            FileItem fileItem;
            List<string> syntaxFilters = new List<string> { "CSharp", "VisualBasic", "CPlusPlus" };
            string assemblyName, commentsName, folderName, value, destFile,
                propName = null;
            string[] list;

            try
            {
                sourceFile = new XPathDocument(base.OldProjectFile);
                navNDoc = sourceFile.CreateNavigator();

                // Add the assemblies
                foreach(XPathNavigator assembly in navNDoc.Select(
                  "project/assemblies/assembly"))
                {
                    assemblyName = this.FullPath(
                        assembly.GetAttribute("location", String.Empty));
                    commentsName = this.FullPath(
                        assembly.GetAttribute("documentation", String.Empty));

                    if(!String.IsNullOrEmpty(assemblyName))
                        project.DocumentationSources.Add(assemblyName, null,
                            null, false);

                    if(!String.IsNullOrEmpty(commentsName))
                        project.DocumentationSources.Add(commentsName, null,
                            null, false);
                }

                // Add reference paths
                foreach(XPathNavigator reference in navNDoc.Select(
                   "project/referencePaths/referencePath"))
                {
                    folderName = this.FullPath(reference.GetAttribute(
                        "path", String.Empty));

                    if(folderName.EndsWith("\\**", StringComparison.Ordinal))
                    {
                        searchOpts = SearchOption.AllDirectories;
                        folderName = folderName.Substring(0,
                            folderName.Length - 3);
                    }
                    else
                        searchOpts = SearchOption.TopDirectoryOnly;

                    foreach(string refFile in Directory.EnumerateFiles(folderName, "*.dll", searchOpts))
                        Project.References.AddReference(Path.GetFileNameWithoutExtension(refFile), refFile);
                }

                // Add the namespace summaries
                foreach(XPathNavigator ns in navNDoc.Select(
                  "project/namespaces/namespace"))
                    if(!String.IsNullOrEmpty(ns.InnerXml))
                        project.NamespaceSummaries.Add(ns.GetAttribute("name",
                            String.Empty), true, ns.InnerXml);

                // Add one for the global namespace if it isn't there
                if(project.NamespaceSummaries[String.Empty] == null)
                    project.NamespaceSummaries.Add(String.Empty, false,
                        String.Empty);

                project.NamespaceSummaries.Sort();

                // Add options from the MSDN documenters.  This will be a
                // merger of all the documenters present in the file since
                // we can't tell which one is the active one.  The file can
                // be edited by hand to delete unwanted documenters or
                // properties before converting it.
                foreach(XPathNavigator node in navNDoc.Select(
                  "project/documenters/documenter[@name=\"MSDN\" or " +
                  "@name=\"MSDN-CHM\" or @name=\"VS.NET 2003\" or " +
                  "@name=\"MSDN 2003\"]"))
                    foreach(XPathNavigator child in node.Select("*"))
                    {
                        propName = child.GetAttribute("name", String.Empty);

                        switch(propName)
                        {
                            case "AdditionalContentResourceDirectory":
                                this.ConvertAdditionalContent(child.GetAttribute(
                                    "value", String.Empty));
                                break;

                            case "BinaryTOC":
                                project.BinaryTOC = Convert.ToBoolean(
                                    child.GetAttribute("value", String.Empty),
                                        CultureInfo.InvariantCulture);
                                break;

                            case "CleanIntermediates":
                                project.CleanIntermediates = Convert.ToBoolean(
                                    child.GetAttribute("value", String.Empty),
                                    CultureInfo.InvariantCulture);
                                break;

                            case "CopyrightHref":
                                project.CopyrightHref = child.GetAttribute(
                                    "value", String.Empty);
                                break;

                            case "CopyrightText":
                                project.CopyrightText = child.GetAttribute(
                                    "value", String.Empty);
                                break;

                            case "DocumentAttributes":
                                project.DocumentAttributes = Convert.ToBoolean(
                                    child.GetAttribute("value", String.Empty),
                                    CultureInfo.InvariantCulture);
                                break;

                            case "DocumentExplicitInterfaceImplementations":
                                project.DocumentExplicitInterfaceImplementations =
                                    Convert.ToBoolean(child.GetAttribute(
                                        "value", String.Empty),
                                        CultureInfo.InvariantCulture);
                                break;

                            case "DocumentInheritedMembers":
                                project.DocumentInheritedMembers =
                                    Convert.ToBoolean(child.GetAttribute(
                                        "value", String.Empty),
                                        CultureInfo.InvariantCulture);
                                break;

                            case "DocumentInheritedFrameworkMembers":
                                project.DocumentInheritedFrameworkMembers =
                                    Convert.ToBoolean(child.GetAttribute(
                                        "value", String.Empty),
                                        CultureInfo.InvariantCulture);
                                break;

                            case "DocumentInternals":
                                project.DocumentInternals = Convert.ToBoolean(
                                    child.GetAttribute("value", String.Empty),
                                    CultureInfo.InvariantCulture);
                                break;

                            case "DocumentPrivates":
                                project.DocumentPrivates = Convert.ToBoolean(
                                    child.GetAttribute("value", String.Empty),
                                    CultureInfo.InvariantCulture);
                                break;

                            case "DocumentProtected":
                                project.DocumentProtected = Convert.ToBoolean(
                                    child.GetAttribute("value", String.Empty),
                                    CultureInfo.InvariantCulture);
                                break;

                            case "DocumentProtectedInternalAsProtected":
                                project.DocumentProtectedInternalAsProtected =
                                    Convert.ToBoolean(child.GetAttribute(
                                        "value", String.Empty),
                                        CultureInfo.InvariantCulture);
                                break;

                            case "DocumentSealedProtected":
                                project.DocumentSealedProtected =
                                    Convert.ToBoolean(child.GetAttribute(
                                        "value", String.Empty),
                                        CultureInfo.InvariantCulture);
                                break;

                            case "FeedbackEmailAddress":
                                project.FeedbackEMailAddress =
                                    child.GetAttribute("value", String.Empty);
                                break;

                            case "HtmlHelpName":
                                project.HtmlHelpName = child.GetAttribute(
                                    "value", String.Empty);
                                break;

                            case "IncludeFavorites":
                                project.IncludeFavorites = Convert.ToBoolean(
                                    child.GetAttribute("value", String.Empty),
                                        CultureInfo.InvariantCulture);
                                break;

                            case "OutputDirectory":
                                folderName = child.GetAttribute("value",
                                    String.Empty);

                                if(folderName != @".\doc\")
                                    project.OutputPath = new FolderPath(
                                        folderName, project);
                                break;

                            case "Preliminary":
                                project.Preliminary = Convert.ToBoolean(
                                    child.GetAttribute("value", String.Empty),
                                        CultureInfo.InvariantCulture);
                                break;

                            case "Title":
                                value = child.GetAttribute("value", String.Empty);

                                if(value != "An NDoc Documented Class Library" &&
                                  value != "An NDoc documented library")
                                    project.HelpTitle = value;
                                break;

                            case "RootPageContainsNamespaces":
                                project.RootNamespaceContainer =
                                    Convert.ToBoolean(child.GetAttribute(
                                        "value", String.Empty),
                                        CultureInfo.InvariantCulture);
                                break;

                            case "RootPageFileName":
                                value = this.FullPath(child.GetAttribute(
                                    "value", String.Empty));
                                destFile = Path.Combine(base.ProjectFolder,
                                    Path.GetFileName(value));
                                project.AddFileToProject(value, destFile);
                                break;

                            case "RootPageTOCName":
                                project.RootNamespaceTitle = child.GetAttribute(
                                    "value", String.Empty);
                                break;

                            case "FilesToInclude":
                                foreach(string filename in child.GetAttribute(
                                  "value", String.Empty).Split(new char[] { '|' }))
                                {
                                    value = this.FullPath(filename.Trim());
                                    destFile = Path.Combine(base.ProjectFolder,
                                        Path.GetFileName(value));
                                    project.AddFileToProject(value, destFile);
                                }
                                break;

                            case "AutoDocumentConstructors":
                                project.AutoDocumentConstructors =
                                    Convert.ToBoolean(child.GetAttribute(
                                        "value", String.Empty),
                                        CultureInfo.InvariantCulture);
                                break;

                            case "ShowMissingSummaries":
                                project.ShowMissingSummaries =
                                    Convert.ToBoolean(child.GetAttribute(
                                        "value", String.Empty),
                                        CultureInfo.InvariantCulture);
                                break;

                            case "ShowMissingRemarks":
                                project.ShowMissingRemarks =
                                    Convert.ToBoolean(child.GetAttribute(
                                        "value", String.Empty),
                                        CultureInfo.InvariantCulture);
                                break;

                            case "ShowMissingParams":
                                project.ShowMissingParams =
                                    Convert.ToBoolean(child.GetAttribute(
                                        "value", String.Empty),
                                        CultureInfo.InvariantCulture);
                                break;

                            case "ShowMissingReturns":
                                project.ShowMissingReturns =
                                    Convert.ToBoolean(child.GetAttribute(
                                        "value", String.Empty),
                                        CultureInfo.InvariantCulture);
                                break;

                            case "ShowMissingValues":
                                project.ShowMissingValues =
                                    Convert.ToBoolean(child.GetAttribute(
                                        "value", String.Empty),
                                        CultureInfo.InvariantCulture);
                                break;

                            case "ShowVisualBasic":
                                if(child.GetAttribute("value", String.Empty) == "False")
                                    syntaxFilters.Remove("VisualBasic");
                                break;

                            case "AboutPageIconPage":
                            case "AboutPageInfo":
                            case "EmptyIndexTermPage":
                            case "IntroductionPage":
                            case "NavFailPage":
                                value = this.FullPath(child.GetAttribute(
                                    "value", String.Empty));
                                destFile = Path.Combine(base.ProjectFolder,
                                    Path.GetFileName(value));
                                fileItem = project.AddFileToProject(value,
                                    destFile);
                                fileItem.ExcludeFromToc = true;
                                break;

                            case "CollectionTOCStyle":
                                project.CollectionTocStyle =
                                    (CollectionTocStyle)Enum.Parse(
                                    typeof(CollectionTocStyle),
                                    child.GetAttribute("value", String.Empty),
                                    true);
                                break;

                            case "DocSetList":
                                list = child.GetAttribute("value",
                                    String.Empty).Split(',');

                                foreach(string docSet in list)
                                    project.HelpAttributes.Add("DocSet",
                                        docSet.Trim());
                                break;

                            case "IncludeDefaultStopWordList":
                                project.IncludeStopWordList = Convert.ToBoolean(
                                    child.GetAttribute("value", String.Empty),
                                    CultureInfo.InvariantCulture);
                                break;

                            case "Version":
                                project.HelpFileVersion = child.GetAttribute(
                                    "value", String.Empty);
                                break;

                            case "OutputTarget":
                                value = child.GetAttribute("value", String.Empty);

                                if(value == "HtmlHelp")
                                    project.HelpFileFormat = HelpFileFormat.HtmlHelp1;
                                else
                                    if(value == "Web")
                                        project.HelpFileFormat = HelpFileFormat.Website;
                                    else
                                        project.HelpFileFormat = HelpFileFormat.HtmlHelp1 |
                                            HelpFileFormat.Website;
                                break;
                            
                            case "SdkDocLanguage":
                                project.Language = new CultureInfo(
                                    child.GetAttribute("value", String.Empty));
                                break;

                            case "SdkDocVersion":
                                value = child.GetAttribute("value",
                                    String.Empty).Substring(5).Replace('_', '.');
                                project.FrameworkVersion = FrameworkVersionTypeConverter.LatestFrameworkMatching(value);
                                break;

                            case "SdkLinksOnWeb":
                                if(child.GetAttribute("value", String.Empty) == "True")
                                {
                                    project.HtmlSdkLinkType = project.WebsiteSdkLinkType = HtmlSdkLinkType.Msdn;
                                    project.MSHelp2SdkLinkType = MSHelp2SdkLinkType.Msdn;
                                    project.MSHelpViewerSdkLinkType = MSHelpViewerSdkLinkType.Msdn;
                                }
                                else
                                {
                                    project.HtmlSdkLinkType = project.WebsiteSdkLinkType = HtmlSdkLinkType.None;
                                    project.MSHelp2SdkLinkType = MSHelp2SdkLinkType.Index;
                                    project.MSHelpViewerSdkLinkType = MSHelpViewerSdkLinkType.Id;
                                }
                                break;

                            default:
                                break;
                        }
                    }

                // Set the syntax filters
                project.SyntaxFilters = String.Join(", ", syntaxFilters.ToArray());

                base.CreateFolderItems();
                project.SaveProject(project.Filename);
            }
            catch(Exception ex)
            {
                throw new BuilderException("CVT0005", String.Format(
                    CultureInfo.CurrentCulture, "Error reading project " +
                    "from '{0}' (last property = {1}):\r\n{2}",
                    base.OldProjectFile, propName, ex.Message), ex);
            }

            return project.Filename;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Add additional content to the project
        /// </summary>
        /// <param name="folder">The folder containing the content</param>
        private void ConvertAdditionalContent(string folder)
        {
            string fileSpec, source, dest, destFile;

            fileSpec = base.FullPath(Path.Combine(folder, "*.*"));

            if(folder.Length > 1 && folder[0] == '.' && folder[1] == '\\')
                dest = folder.Substring(2);
            else
                if(folder.Length > 2 && folder[0] == '.' && folder[1] == '.' &&
                  folder[2] == '\\')
                    dest = folder.Substring(3);
                else
                    dest = String.Empty;

            source = Path.GetDirectoryName(fileSpec);
            dest = Path.GetFullPath(Path.Combine(base.ProjectFolder, dest));

            foreach(string file in ExpandWildcard(fileSpec, true))
            {
                // Remove the base path but keep any subfolders
                destFile = file.Substring(source.Length + 1);
                destFile = Path.Combine(dest, destFile);
                base.Project.AddFileToProject(file, destFile);
            }
        }
        #endregion
    }
}
