//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ConvertConceptualContent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/28/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used to convert conceptual content settings in
// version 1.7.0.0 and prior SHFB project files to the new MSBuild format.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  07/24/2008  EFW  Created the code
//=============================================================================

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using SandcastleBuilder.Utils.BuildEngine;
using SandcastleBuilder.Utils.ConceptualContent;

namespace SandcastleBuilder.Utils.Conversion
{
    /// <summary>
    /// This class is used to convert conceptual content settings in version
    /// 1.7.0.0 and prior SHFB project files to the new MSBuild format.
    /// </summary>
	public class ConvertConceptualContent
	{
        #region Private data members
        //=====================================================================

        private ConvertFromShfbFile converter;

        private static Regex reFindHead = new Regex(@"</head>",
            RegexOptions.IgnoreCase);
        private static Regex reFindTopic = new Regex(@"<topic ",
            RegexOptions.IgnoreCase);
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="projectConverter">The project converter object</param>
        public ConvertConceptualContent(ConvertFromShfbFile projectConverter)
        {
            converter = projectConverter;
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// This is used to convert the conceptual content from the old project
        /// into the format required by the new project.
        /// </summary>
        public void ConvertContent()
        {
            XmlTextReader xr = converter.Reader;

            // Add code snippets file if defined
            string dest, file = xr.GetAttribute("snippetFile");

            if(file != null && file.Trim().Length > 0)
            {
                dest = Path.Combine(converter.ProjectFolder, Path.GetFileName(
                    file));
                dest = Path.ChangeExtension(dest, ".snippets");
                converter.Project.AddFileToProject(converter.FullPath(file),
                    dest);
            }

            if(!xr.IsEmptyElement)
                while(!xr.EOF && xr.NodeType != XmlNodeType.EndElement)
                {
                    if(xr.NodeType == XmlNodeType.Element &&
                      xr.Name == "tokens" && !xr.IsEmptyElement)
                        this.CreateTokenFile();

                    if(xr.NodeType == XmlNodeType.Element &&
                      xr.Name == "images" && !xr.IsEmptyElement)
                        this.AddImageFiles();

                    if(xr.NodeType == XmlNodeType.Element &&
                      xr.Name == "topics" && !xr.IsEmptyElement)
                        this.CreateContentFile();

                    xr.Read();
                }

        }

        /// <summary>
        /// This converts the token entries to a token file and adds it to
        /// the project.
        /// </summary>
        private void CreateTokenFile()
        {
            XmlReader xr = converter.Reader;
            StreamWriter sw = null;
            string tokenFile = Path.Combine(converter.ProjectFolder,
                Path.GetFileNameWithoutExtension(converter.Project.Filename) +
                ".tokens");

            // Create an empty token file
            try
            {
                sw = File.CreateText(tokenFile);
                sw.WriteLine("<content/>");
            }
            finally
            {
                if(sw != null)
                    sw.Close();
            }

            FileItem fileItem = converter.Project.AddFileToProject(tokenFile,
                tokenFile);
            TokenCollection tokens = new TokenCollection(fileItem);

            while(!xr.EOF && xr.NodeType != XmlNodeType.EndElement)
            {
                if(xr.NodeType == XmlNodeType.Element && xr.Name == "token")
                    tokens.Add(new Token(xr.GetAttribute("name"),
                        xr.GetAttribute("value")));

                xr.Read();
            }

            tokens.Save();
        }

        /// <summary>
        /// This adds all image files to the project
        /// </summary>
        private void AddImageFiles()
        {
            XmlReader xr = converter.Reader;
            SandcastleProject project = converter.Project;
            FileItem fileItem;
            string sourceFile, id, altText, destFile,
                oldFolder = converter.OldFolder,
                projectFolder = converter.ProjectFolder;
            bool alwaysCopy;

            while(!xr.EOF && xr.NodeType != XmlNodeType.EndElement)
            {
                if(xr.NodeType == XmlNodeType.Element && xr.Name == "image")
                {
                    id = xr.GetAttribute("id");

                    if(id == null || id.Trim().Length == 0)
                        id = Guid.NewGuid().ToString();

                    sourceFile = xr.GetAttribute("file");

                    if(sourceFile == null || sourceFile.Trim().Length == 0)
                        sourceFile = "Unknown.jpg";

                    sourceFile = converter.FullPath(sourceFile);

                    // Maintain any additional path info beyond the old base
                    // project folder.
                    if(sourceFile.StartsWith(oldFolder,
                      StringComparison.OrdinalIgnoreCase))
                        destFile = projectFolder + sourceFile.Substring(
                            oldFolder.Length);
                    else
                        destFile = Path.Combine(projectFolder,
                            Path.GetFileName(sourceFile));

                    altText = xr.GetAttribute("altText");
                    alwaysCopy = Convert.ToBoolean(xr.GetAttribute(
                        "alwaysCopy"), CultureInfo.InvariantCulture);

                    fileItem = project.AddFileToProject(sourceFile, destFile);
                    fileItem.BuildAction = BuildAction.Image;
                    fileItem.ProjectElement.SetMetadata("ImageId", id);

                    if(!String.IsNullOrEmpty(altText))
                        fileItem.ProjectElement.SetMetadata("AlternateText",
                            altText);

                    if(alwaysCopy)
                        fileItem.ProjectElement.SetMetadata("CopyToMedia",
                            "True");
                }

                xr.Read();
            }
        }

        /// <summary>
        /// This converts the topic entries to a content file and adds it to
        /// the project.
        /// </summary>
        private void CreateContentFile()
        {
            SandcastleProject project = converter.Project;
            XmlReader xr = converter.Reader;
            XmlWriterSettings settings = new XmlWriterSettings();
            XmlWriter xw = null;
            string contentFilename, attr;

            try
            {
                settings.Indent = true;
                settings.CloseOutput = true;
                contentFilename = Path.Combine(converter.ProjectFolder,
                    Path.GetFileNameWithoutExtension(
                    converter.Project.Filename) + ".content");

                xw = XmlWriter.Create(contentFilename, settings);
                xw.WriteStartDocument();
                xw.WriteStartElement("Topics");

                // Add the default topic and split TOC attributes if present
                attr = xr.GetAttribute("defaultTopic");

                if(!String.IsNullOrEmpty(attr))
                    xw.WriteAttributeString("defaultTopic", attr);

                attr = xr.GetAttribute("splitTOCTopic");

                if(!String.IsNullOrEmpty(attr))
                    xw.WriteAttributeString("splitTOCTopic", attr);

                while(!xr.EOF && xr.NodeType != XmlNodeType.EndElement)
                {
                    if(xr.NodeType == XmlNodeType.Element && xr.Name == "topic")
                        this.ConvertTopic(xr, xw, project);

                    xr.Read();
                }
                
                xw.WriteEndElement();   // </Topics>
                xw.WriteEndDocument();
            }
            finally
            {
                if(xw != null)
                    xw.Close();
            }

            converter.Project.AddFileToProject(contentFilename, contentFilename);
        }

        /// <summary>
        /// Convert a conceptual content topic and all of its children
        /// </summary>
        /// <param name="xr">The XML reader containing the topics</param>
        /// <param name="xw">The XML writer to which they are written</param>
        /// <param name="project">The project to which the files are added</param>
        private void ConvertTopic(XmlReader xr, XmlWriter xw,
          SandcastleProject project)
        {
            FileItem newFile = null;
            string id, file, title, tocTitle, linkText, destFile, ext, name,
                value, oldFolder = converter.OldFolder, projectFolder =
                converter.ProjectFolder;
            bool visible;
            int revision;

            id = xr.GetAttribute("id");

            // If not set, an ID will be created when needed
            if(id == null || id.Trim().Length == 0)
                id = new Guid(id).ToString();

            file = xr.GetAttribute("file");
            title = xr.GetAttribute("title");
            tocTitle = xr.GetAttribute("tocTitle");
            linkText = xr.GetAttribute("linkText");

            if(!Int32.TryParse(xr.GetAttribute("revision"), out revision) ||
              revision < 1)
                revision = 1;

            if(!Boolean.TryParse(xr.GetAttribute("visible"), out visible))
                visible = true;

            // Add the topic to the content file and the project
            xw.WriteStartElement("Topic");
            xw.WriteAttributeString("id", id);

            if(file != null && file.Trim().Length > 0)
            {
                file = converter.FullPath(file);

                // Maintain any additional path info beyond the old base
                // project folder.
                if(file.StartsWith(oldFolder, StringComparison.OrdinalIgnoreCase))
                    destFile = projectFolder + file.Substring(oldFolder.Length);
                else
                    destFile = Path.Combine(projectFolder, Path.GetFileName(file));

                ext = Path.GetExtension(destFile).ToLower(
                    CultureInfo.InvariantCulture);

                // Change the extension on .xml files to .aml and add the
                // root topic element.
                if(ext == ".xml")
                {
                    destFile = Path.ChangeExtension(destFile, ".aml");
                    ConvertTopic(file, destFile, id, revision);
                    file = destFile;
                }

                // Add meta elements for the ID and revision number
                if(ext == ".htm" || ext == ".html" || ext == ".topic")
                {
                    ConvertTopic(file, destFile, id, revision);
                    file = destFile;
                }

                newFile = project.AddFileToProject(file, destFile);
                newFile.BuildAction = BuildAction.None;
            }
            else
                xw.WriteAttributeString("noFile", "True");

            if(title != null)
                xw.WriteAttributeString("title", title);

            if(tocTitle != null)
                xw.WriteAttributeString("tocTitle", tocTitle);

            if(linkText != null)
                xw.WriteAttributeString("linkText", linkText);

            xw.WriteAttributeString("visible",
                visible.ToString(CultureInfo.InvariantCulture));

            // Add child elements, if any
            if(!xr.IsEmptyElement)
                while(!xr.EOF)
                {
                    xr.Read();

                    if(xr.NodeType == XmlNodeType.EndElement &&
                      xr.Name == "topic")
                        break;

                    if(xr.NodeType == XmlNodeType.Element)
                        if(xr.Name == "helpAttributes")
                        {
                            xw.WriteStartElement("HelpAttributes");

                            while(!xr.EOF && xr.NodeType != XmlNodeType.EndElement)
                            {
                                if(xr.NodeType == XmlNodeType.Element &&
                                  xr.Name == "helpAttribute")
                                {
                                    name = xr.GetAttribute("name");
                                    value = xr.GetAttribute("value");

                                    if(!String.IsNullOrEmpty(name))
                                    {
                                        xw.WriteStartElement("HelpAttribute");
                                        xw.WriteAttributeString("name", name);
                                        xw.WriteAttributeString("value", value);
                                        xw.WriteEndElement();
                                    }
                                }

                                xr.Read();
                            }

                            xw.WriteEndElement();
                        }
                        else
                            if(xr.Name == "helpKeywords")
                            {
                                xw.WriteStartElement("HelpKeywords");

                                while(!xr.EOF && xr.NodeType != XmlNodeType.EndElement)
                                {
                                    if(xr.NodeType == XmlNodeType.Element &&
                                      xr.Name == "helpKeyword")
                                    {
                                        name = xr.GetAttribute("index");
                                        value = xr.GetAttribute("term");

                                        if(!String.IsNullOrEmpty(name))
                                        {
                                            xw.WriteStartElement("HelpKeyword");
                                            xw.WriteAttributeString("index", name);
                                            xw.WriteAttributeString("term", value);
                                            xw.WriteEndElement();
                                        }
                                    }

                                    xr.Read();
                                }

                                xw.WriteEndElement();
                            }
                            else
                                if(xr.Name == "topic")
                                    this.ConvertTopic(xr, xw, project);
                }

            xw.WriteEndElement();
        }

        /// <summary>
        /// Convert a conceptual topic to the new format
        /// </summary>
        /// <param name="source">The source file</param>
        /// <param name="dest">The destination file</param>
        /// <param name="id">The topic ID</param>
        /// <param name="revision">The revision number</param>
        private static void ConvertTopic(string source, string dest, string id,
          int revision)
        {
            XmlDocument doc;
            XmlNode root;
            XmlAttribute attr;
            Encoding enc = Encoding.Default;
            Match m;
            string content;

            if(!Directory.Exists(Path.GetDirectoryName(dest)))
                Directory.CreateDirectory(Path.GetDirectoryName(dest));

            if(dest.EndsWith(".aml", StringComparison.OrdinalIgnoreCase))
            {
                doc = new XmlDocument();
                doc.Load(source);

                // Already there?
                root = doc.SelectSingleNode("topic");

                if(root != null)
                {
                    if(root.Attributes["id"] == null ||
                      root.Attributes["revisionNumber"] == null)
                        throw new FormatException(source + " contains a " +
                            "root topic element without an id and/or " +
                            "revision attribute");

                    doc.Save(dest);
                    return;
                }

                // Add the root node
                root = doc.CreateElement("topic");
                attr = doc.CreateAttribute("id");
                attr.Value = id;
                root.Attributes.Append(attr);
                attr = doc.CreateAttribute("revisionNumber");
                attr.Value = revision.ToString(CultureInfo.InvariantCulture);
                root.Attributes.Append(attr);

                root.AppendChild(doc.ChildNodes[1]);
                doc.AppendChild(root);
                doc.Save(dest);
                return;
            }

            content = BuildProcess.ReadWithEncoding(source, ref enc);

            if(!source.EndsWith(".topic", StringComparison.OrdinalIgnoreCase))
            {
                m = reFindHead.Match(content);

                if(!m.Success)
                    throw new FormatException(source + " does not contain a " +
                        "<head> element");

                content = content.Insert(m.Index, String.Format(
                    CultureInfo.InvariantCulture, "<meta name=\"id\" " +
                    "content=\"{0}\">\r\n<meta name=\"revisionNumber\" " +
                    "content=\"{1}\">\r\n", id, revision));
            }
            else
            {
                m = reFindTopic.Match(content);

                if(!m.Success)
                    throw new FormatException(source + " does not contain a " +
                        "<topic> element");

                content = content.Insert(m.Index + m.Length, String.Format(
                    CultureInfo.InvariantCulture, " id=\"{0}\" " +
                    "revisionNumber=\"{1}\" ", id, revision));
            }

            // Write the file back out with the appropriate encoding
            using(StreamWriter sw = new StreamWriter(dest, false, enc))
            {
                sw.Write(content);
            }
        }
        #endregion
	}
}
