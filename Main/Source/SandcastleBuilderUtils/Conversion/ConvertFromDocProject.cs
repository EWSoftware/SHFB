//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ConvertFromDocProject.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/08/2012
// Note    : Copyright 2008-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used to convert project files created by
// DocProject to the MSBuild format project files used by SHFB.
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

using SandcastleBuilder.Utils.ConceptualContent;
using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Utils.Conversion
{
    /// <summary>
    /// This class is used to convert DocProject project files to the MSBuild
    /// format project files used by the help file builder.
    /// </summary>
    public class ConvertFromDocProject : ConvertToMSBuildFormat
    {
        #region Private data members
        //=====================================================================

        private XmlDocument docProject;
        private XmlNamespaceManager nsm;
        private SandcastleProject project;
        private Dictionary<string, Topic> topicSettings;
        private string topicLayoutFilename;
        #endregion

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
        public ConvertFromDocProject(string oldProjectFile, string folder) : base(oldProjectFile, folder)
        {
            topicSettings = new Dictionary<string, Topic>();
        }

        /// <inheritdoc />
        public ConvertFromDocProject(string oldProjectFile, SandcastleProject newProject) :
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
            XPathNavigator navProject;
            Topic t;
            string lastProperty = null;

            try
            {
                project = base.Project;
                project.HelpTitle = project.HtmlHelpName =
                    Path.GetFileNameWithoutExtension(base.OldProjectFile);

                // We'll process it as XML rather than an MSBuild project as
                // it may contain references to target files that don't exist
                // which would prevent it from loading.
                docProject = new XmlDocument();
                docProject.Load(base.OldProjectFile);
                nsm = new XmlNamespaceManager(docProject.NameTable);
                nsm.AddNamespace("prj", docProject.DocumentElement.NamespaceURI);
                navProject = docProject.CreateNavigator();

                lastProperty = "ProjectExtensions/VisualStudio";
                this.ImportProjectExtensionProperties();

                // Parse each build item to look for stuff we need to add
                // to the project.
                foreach(XPathNavigator buildItem in navProject.Select(
                  "//prj:Project/prj:ItemGroup/*", nsm))
                {
                    lastProperty = String.Format(CultureInfo.InvariantCulture,
                        "{0} ({1})", buildItem.Name,
                        buildItem.GetAttribute("Include", String.Empty));

                    switch(buildItem.Name)
                    {
                        case "Content":
                        case "None":
                            this.ImportFile(buildItem);
                            break;

                        case "ProjectReference":
                            project.DocumentationSources.Add(base.FullPath(
                                buildItem.GetAttribute("Include", String.Empty)),
                                null, null, false);
                            break;

                        default:    // Ignore
                            break;
                    }
                }

                // Add project-level help attributes if any
                if(topicSettings.TryGetValue("*", out t))
                    foreach(MSHelpAttr ha in t.HelpAttributes)
                        project.HelpAttributes.Add(ha);

                if(topicLayoutFilename != null)
                    this.CreateContentLayoutFile();
                else
                    if(topicSettings.Count != 0)
                        this.CreateDefaultContentLayoutFile();

                base.CreateFolderItems();
                project.SaveProject(project.Filename);
            }
            catch(Exception ex)
            {
                throw new BuilderException("CVT0005", String.Format(
                    CultureInfo.CurrentCulture, "Error reading project " +
                    "from '{0}' (last property = {1}):\r\n{2}",
                    base.OldProjectFile, lastProperty, ex.Message), ex);
            }

            return project.Filename;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Parse project properties from the project extensions section
        /// </summary>
        private void ImportProjectExtensionProperties()
        {
            XmlNode props = docProject.SelectSingleNode("//prj:Project/" +
                "prj:ProjectExtensions/prj:VisualStudio/prj:UserProperties",
                nsm);
            string value;

            if(props == null)
                return;

            if(docProject.SelectSingleNode("//prj:Project/prj:ProjectExtensions" +
              "/prj:VisualStudio/prj:FlavorProperties", nsm) != null)
                project.HelpFileFormat |= HelpFileFormat.Website;

            foreach(XmlAttribute attr in props.Attributes)
                switch(attr.Name)
                {
                    case "DocumentationScope":
                        if(String.Compare(attr.Value, "PublicOnly",
                          StringComparison.OrdinalIgnoreCase) != 0)
                            project.DocumentInternals =
                                project.DocumentPrivates = true;
                        break;

                    case "ExternalSources":
                        foreach(string source in attr.Value.Split('|'))
                        {
                            value = source.Trim();

                            if(value.Length != 0)
                                if(value[value.Length - 1] == '\\')
                                    project.DocumentationSources.Add(
                                        base.FullPath(Path.Combine(value, "*.*")),
                                        null, null, false);
                                else
                                    project.DocumentationSources.Add(
                                        base.FullPath(value), null, null, false);
                        }
                        break;

                    case "Sandcastle_PresentationName":
                        if(attr.Value.IndexOf("prototype", StringComparison.OrdinalIgnoreCase) != -1)
                            value = "prototype";
                        else
                            if(attr.Value.IndexOf("hana", StringComparison.OrdinalIgnoreCase) != -1)
                                value = "hana";
                            else
                                value = "vs2005";

                        project.PresentationStyle =
                            PresentationStyleTypeConverter.FirstMatching(value);
                        break;

                    case "Sandcastle_DocumentationSetName":
                        project.HelpTitle = attr.Value;
                        break;

                    case "Sandcastle_UseFriendlyHtmlFileNames":
                        if(String.Compare(attr.Value, "True",
                          StringComparison.OrdinalIgnoreCase) == 0)
                            project.NamingMethod = NamingMethod.MemberName;
                        break;

                    case "Sandcastle_ProduceHelp1x":
                        if(String.Compare(attr.Value, "False",
                          StringComparison.OrdinalIgnoreCase) == 0)
                            if(project.HelpFileFormat == HelpFileFormat.HtmlHelp1)
                                project.HelpFileFormat = HelpFileFormat.MSHelp2;
                            else
                                project.HelpFileFormat &= ~HelpFileFormat.HtmlHelp1;
                        break;

                    case "Sandcastle_ProduceHelp2x":
                        if(String.Compare(attr.Value, "True",
                          StringComparison.OrdinalIgnoreCase) == 0)
                            project.HelpFileFormat |= HelpFileFormat.MSHelp2;
                        break;

                    case "Sandcastle_GenerateRootApiTopic":
                        if(String.Compare(attr.Value, "True",
                          StringComparison.OrdinalIgnoreCase) == 0)
                            project.RootNamespaceContainer = true;
                        break;

                    case "Sandcastle_MissingDependencies":
                        foreach(string file in attr.Value.Split('|'))
                        {
                            value = file.Trim();

                            if(value.Length == 0)
                                continue;

                            if(value[value.Length - 1] != '\\')
                                project.References.AddReference(
                                    Path.GetFileNameWithoutExtension(value),
                                    base.FullPath(value));
                            else
                                foreach(string reference in
                                  ConvertToMSBuildFormat.ExpandWildcard(
                                  Path.Combine(base.FullPath(value), "*.*"), true))
                                    if(reference.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
                                      reference.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                                        project.References.AddReference(
                                            Path.GetFileNameWithoutExtension(reference),
                                            reference);
                        }
                        break;

                    default:    // Ignored
                        break;
                }
        }

        /// <summary>
        /// Import a file into the project
        /// </summary>
        /// <param name="buildItem">The file information</param>
        private void ImportFile(XPathNavigator buildItem)
        {
            string newPath, name, file = buildItem.GetAttribute("Include",
                String.Empty);
            FileItem fileItem;
            bool isTokens = false, isSnippets = false;

            if(String.IsNullOrEmpty(file))
                return;

            // Ignore these paths as they are most likely presenation style files
            if(file.StartsWith(@"Help\Icons\", StringComparison.OrdinalIgnoreCase) ||
              file.StartsWith(@"Help\Presentation\", StringComparison.OrdinalIgnoreCase) ||
              file.StartsWith(@"Help\Scripts\", StringComparison.OrdinalIgnoreCase) ||
              file.StartsWith(@"Help\Styles\", StringComparison.OrdinalIgnoreCase) ||
              file.StartsWith(@"Help\Schemas\", StringComparison.OrdinalIgnoreCase))
                return;

            // For websites, ignore these folders and files
            if((project.HelpFileFormat & HelpFileFormat.Website) != 0)
                if(file.StartsWith(@"App_Themes\", StringComparison.OrdinalIgnoreCase) ||
                  file.StartsWith(@"App_GlobalResources\", StringComparison.OrdinalIgnoreCase) ||
                  file.StartsWith(@"App_LocalResources\", StringComparison.OrdinalIgnoreCase) ||
                  file.StartsWith(@"Controls\", StringComparison.OrdinalIgnoreCase) ||
                  file.StartsWith(@"Scripts\", StringComparison.OrdinalIgnoreCase) ||
                  file.StartsWith(@"DocSite.", StringComparison.OrdinalIgnoreCase) ||
                  file.StartsWith(@"FileNotFound.htm", StringComparison.OrdinalIgnoreCase) ||
                  file.EndsWith(".config", StringComparison.OrdinalIgnoreCase) ||
                  file.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
                    return;

            // Conceptual content media file
            if(file.StartsWith(@"Help\Art\", StringComparison.OrdinalIgnoreCase))
            {
                newPath = Path.Combine(base.ProjectFolder, file.Substring(5));
                file = Path.Combine(base.OldFolder, file);
                fileItem = project.AddFileToProject(file, newPath);
                fileItem.BuildAction = BuildAction.None;
                return;
            }

            // Additional XML comments file, add as documentation source
            if(file.StartsWith(@"Help\Comments\", StringComparison.OrdinalIgnoreCase))
            {
                newPath = Path.Combine(base.ProjectFolder, file.Substring(5));
                file = Path.Combine(base.OldFolder, file);

                if(!Directory.Exists(Path.GetDirectoryName(newPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(newPath));

                fileItem = project.AddFileToProject(file, newPath);
                fileItem.BuildAction = BuildAction.None;
                project.DocumentationSources.Add(newPath, null, null, false);
                return;
            }

            // Content settings files
            if(file.StartsWith(@"Help\Settings\", StringComparison.OrdinalIgnoreCase))
            {
                name = Path.GetFileName(file).ToLower(CultureInfo.InvariantCulture);

                // Filter and process by name
                switch(name)
                {
                    case "conceptual_art.xml":
                        this.ImportMediaFile(Path.Combine(base.OldFolder, file));
                        return;

                    case "conceptual_snippets.xml":
                        isSnippets = true;
                        break;

                    case "metadata.xml":
                        this.ImportCompanionFileInfo(Path.Combine(
                            base.OldFolder, file));
                        return;

                    case "tokens.xml":
                        isTokens = true;
                        break;

                    case "topics.xml":  // This gets processed later
                        topicLayoutFilename = Path.Combine(base.OldFolder, file);
                        return;

                    case "help1x.config": // Ignore
                    case "help2x.xslt":
                        return;

                    default:    // Add as content
                        break;
                }

                newPath = Path.Combine(base.ProjectFolder, file.Substring(5));

                if(isTokens)
                    newPath = Path.ChangeExtension(newPath, ".tokens");
                else
                    if(isSnippets)
                        newPath = Path.ChangeExtension(newPath, ".snippets");

                file = Path.Combine(base.OldFolder, file);
                fileItem = project.AddFileToProject(file, newPath);

                if(isTokens)
                    fileItem.BuildAction = BuildAction.Tokens;
                else
                    if(isSnippets)
                        fileItem.BuildAction = BuildAction.CodeSnippets;

                return;
            }

            // Import companion file info?
            if(file.EndsWith(".cmp", StringComparison.OrdinalIgnoreCase))
                this.ImportCompanionFileInfo(Path.Combine(base.OldFolder, file));
            else
            {
                // General content file
                if(file.StartsWith(@"Help\", StringComparison.OrdinalIgnoreCase))
                    newPath = Path.Combine(base.ProjectFolder, file.Substring(5));
                else
                    newPath = Path.Combine(base.ProjectFolder, file);

                file = Path.Combine(base.OldFolder, file);
                fileItem = project.AddFileToProject(file, newPath);
            }
        }

        /// <summary>
        /// Import image file information from a media content file.
        /// </summary>
        /// <param name="filename">The media content filename</param>
        private void ImportMediaFile(string filename)
        {
            List<string> filesSeen = new List<string>();
            ImageReferenceCollection images;
            XPathDocument media;
            XPathNavigator navMedia, file, altText;
            FileItem fileItem;
            string guid, id, path, newName;
            int uniqueId;

            images = new ImageReferenceCollection(base.Project);
            media = new XPathDocument(filename);
            navMedia = media.CreateNavigator();

            foreach(XPathNavigator item in navMedia.Select("//item"))
            {
                guid = null;
                file = altText = null;
                id = item.GetAttribute("id", String.Empty);
                file = item.SelectSingleNode("image/@file");
                altText = item.SelectSingleNode("image/altText");

                if(!String.IsNullOrEmpty(id))
                    guid = id.Trim();

                if(!String.IsNullOrEmpty(guid) &&
                  images.FindId(guid) == null && file != null &&
                  !String.IsNullOrEmpty(file.Value))
                {
                    path = newName = file.Value;

                    // If relative, get the full path.  If no path, assume
                    // Help\Art as the default.
                    if(String.IsNullOrEmpty(Path.GetDirectoryName(path)))
                    {
                        path = Path.Combine(@"Help\Art", path);
                        newName = path.Substring(5);
                    }

                    if(Path.IsPathRooted(path))
                        newName = Path.GetFileName(path);
                    else
                        path = Path.GetFullPath(Path.Combine(base.OldFolder,
                            path));

                    // It's possible that two entries share the same file
                    // so we'll need to create a new copy as in SHFB, the
                    // settings are managed via the project explorer and each
                    // file is unique.
                    uniqueId = 1;

                    while(filesSeen.Contains(newName))
                    {
                        newName = Path.Combine(Path.GetDirectoryName(newName),
                            Path.GetFileNameWithoutExtension(newName) +
                            uniqueId.ToString(CultureInfo.InvariantCulture) +
                            Path.GetExtension(newName));
                        uniqueId++;
                    }

                    filesSeen.Add(newName);

                    fileItem = project.AddFileToProject(path,
                        Path.Combine(base.ProjectFolder, newName));
                    fileItem.BuildAction = BuildAction.Image;
                    fileItem.ImageId = guid;

                    if(altText != null)
                        fileItem.AlternateText = altText.Value;
                }
            }
        }

        /// <summary>
        /// Import attribute information from a companion file
        /// </summary>
        /// <param name="filename">The companion filename</param>
        private void ImportCompanionFileInfo(string filename)
        {
            XPathDocument info = new XPathDocument(filename);
            XPathNavigator navTopics = info.CreateNavigator();
            Topic t;

            foreach(XPathNavigator topic in navTopics.Select("metadata/topic"))
            {
                if(!topicSettings.TryGetValue(topic.GetAttribute("id",
                  String.Empty), out t))
                {
                    t = new Topic();
                    topicSettings.Add(topic.GetAttribute("id", String.Empty), t);
                }

                foreach(XPathNavigator attr in topic.Select("*"))
                    switch(attr.Name)
                    {
                        case "title":
                            t.Title = attr.Value;
                            break;

                        case "tableOfContentsTitle":
                            t.TocTitle = attr.Value;
                            break;

                        case "attribute":
                            t.HelpAttributes.Add(attr.GetAttribute("name",
                                String.Empty), attr.Value);
                            break;

                        default:
                            break;
                    }
            }
        }

        /// <summary>
        /// Create a content layout file for the conceptual topics
        /// </summary>
        private void CreateContentLayoutFile()
        {
            string filename = Path.Combine(base.ProjectFolder,
                "ContentLayout.content");
            XmlTextReader reader = null;
            XmlWriterSettings settings = new XmlWriterSettings();
            XmlWriter writer = null;
            int topicsAdded = 0;

            try
            {
                settings.Indent = true;
                settings.CloseOutput = true;
                writer = XmlWriter.Create(filename, settings);

                writer.WriteStartDocument();
                writer.WriteStartElement("Topics");

                reader = new XmlTextReader(new StreamReader(topicLayoutFilename));
                reader.MoveToContent();

                while(!reader.EOF && reader.NodeType != XmlNodeType.EndElement)
                {
                    if(reader.NodeType == XmlNodeType.Element &&
                      reader.Name == "topics" && !reader.IsEmptyElement)
                    {
                        while(!reader.EOF && reader.NodeType != XmlNodeType.EndElement)
                        {
                            if(reader.NodeType == XmlNodeType.Element && reader.Name == "topic")
                            {
                                this.ConvertTopic(reader, writer);
                                topicsAdded++;
                            }

                            reader.Read();
                        }
                    }

                    reader.Read();
                }

                writer.WriteEndElement();   // </Topics>
                writer.WriteEndDocument();
            }
            catch(Exception ex)
            {
                throw new BuilderException("CVT0006", String.Format(
                    CultureInfo.CurrentCulture, "Error converting " +
                    "content layout file ({0}):\r\n{1}", topicLayoutFilename,
                    ex.Message), ex);
            }
            finally
            {
                if(reader != null)
                    reader.Close();

                if(writer != null)
                    writer.Close();
            }

            if(topicsAdded != 0)
                project.AddFileToProject(filename, filename);
            else
                this.CreateDefaultContentLayoutFile();
        }

        /// <summary>
        /// Convert a conceptual content topic and all of its children
        /// </summary>
        /// <param name="xr">The XML reader containing the topics</param>
        /// <param name="xw">The XML writer to which they are written</param>
        private void ConvertTopic(XmlReader xr, XmlWriter xw)
        {
            Topic t, commonSettings;
            string id;

            id = xr.GetAttribute("id");

            // If not set, an ID will be created when needed
            if(id == null || id.Trim().Length == 0)
                id = new Guid(id).ToString();

            xw.WriteStartElement("Topic");
            xw.WriteAttributeString("id", id);
            xw.WriteAttributeString("visible", "true");

            if(topicSettings.TryGetValue(id, out t))
            {
                if(topicSettings.TryGetValue("*", out commonSettings))
                    foreach(MSHelpKeyword kw in commonSettings.Keywords)
                        t.Keywords.Add(kw);

                if(!String.IsNullOrEmpty(t.Title))
                    xw.WriteAttributeString("title", t.Title);

                if(!String.IsNullOrEmpty(t.TocTitle) && t.TocTitle != t.Title)
                    xw.WriteAttributeString("tocTitle", t.TocTitle);

                if(!String.IsNullOrEmpty(t.LinkText))
                    xw.WriteAttributeString("linkText", t.LinkText);

                if(t.HelpAttributes.Count != 0)
                    t.HelpAttributes.WriteXml(xw, true);

                if(t.Keywords.Count != 0)
                    t.Keywords.WriteXml(xw);
            }

            // Add child elements, if any
            if(!xr.IsEmptyElement)
                while(!xr.EOF)
                {
                    xr.Read();

                    if(xr.NodeType == XmlNodeType.EndElement &&
                      xr.Name == "topic")
                        break;

                    if(xr.Name == "topic")
                        this.ConvertTopic(xr, xw);
                }

            xw.WriteEndElement();
        }

        /// <summary>
        /// Topic settings were found but no content layout file.  In such
        /// cases, this is called to create a default content layout file
        /// based on the settings alone.
        /// </summary>
        private void CreateDefaultContentLayoutFile()
        {
            string filename = Path.Combine(base.ProjectFolder,
                "ContentLayout.content");
            Topic commonSettings, t;

            if(!topicSettings.TryGetValue("*", out commonSettings))
                commonSettings = new Topic();
            else
                topicSettings.Remove("*");

            if(topicSettings.Count == 0)
                return;

            XmlWriterSettings settings = new XmlWriterSettings();
            XmlWriter writer = null;

            try
            {
                settings.Indent = true;
                settings.CloseOutput = true;
                writer = XmlWriter.Create(filename, settings);

                writer.WriteStartDocument();
                writer.WriteStartElement("Topics");

                foreach(string key in topicSettings.Keys)
                {
                    t = topicSettings[key];

                    foreach(MSHelpKeyword kw in commonSettings.Keywords)
                        t.Keywords.Add(kw);

                    writer.WriteStartElement("Topic");
                    writer.WriteAttributeString("id", key);
                    writer.WriteAttributeString("visible", "true");

                    if(!String.IsNullOrEmpty(t.Title))
                        writer.WriteAttributeString("title", t.Title);

                    if(!String.IsNullOrEmpty(t.TocTitle) && t.TocTitle != t.Title)
                        writer.WriteAttributeString("tocTitle", t.TocTitle);

                    if(!String.IsNullOrEmpty(t.LinkText))
                        writer.WriteAttributeString("linkText", t.LinkText);

                    if(t.HelpAttributes.Count != 0)
                        t.HelpAttributes.WriteXml(writer, true);

                    if(t.Keywords.Count != 0)
                        t.Keywords.WriteXml(writer);

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            finally
            {
                if(writer != null)
                    writer.Close();
            }

            project.AddFileToProject(filename, filename);
        }
        #endregion
    }
}
