//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : Topic.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/02/2010
// Note    : Copyright 2008-2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing a conceptual content topic.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.7  04/24/2008  EFW  Created the code
// 1.8.0.0  08/07/2008  EFW  Modified for use with the new project format
// 1.9.0.0  06/06/2010  EFW  Added support for MS Help Viewer output
// 1.9.0.0  07/01/2010  EFW  Added support for API parent mode setting
//=============================================================================

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;

using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.Utils.ConceptualContent
{
    /// <summary>
    /// This represents a conceptual content topic.
    /// </summary>
    /// <remarks>This class is serializable so that it can be copied to the
    /// clipboard.</remarks>
    [DefaultProperty("Title")]
    public class Topic
    {
        #region Private data members
        //=====================================================================

        private TopicFile topicFile;
        private TopicCollection subtopics;
        private string contentId, title, tocTitle, linkText;
        private bool noFile;
        private MSHelpAttrCollection helpAttributes;
        private MSHelpKeywordCollection keywords;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to track the topic's parent collection
        /// </summary>
        /// <remarks>This is used by the designer to move items around within
        /// the collection.</remarks>
        [Browsable(false)]
        public TopicCollection Parent { get; internal set; }

        /// <summary>
        /// This is used to get or set the topic file information related to
        /// the topic.
        /// </summary>
        /// <value>If there is no topic file, this topic serves as a container
        /// node for its sub-topics and no content will be displayed for it
        /// when selected in the help file's table of contents.</value>
        [Browsable(false)]
        public TopicFile TopicFile
        {
            get { return topicFile; }
            set
            {
                topicFile = value;

                if(topicFile != null)
                {
                    noFile = false;
                    contentId = topicFile.Id;
                }
                else
                {
                    noFile = true;
                    contentId = Guid.NewGuid().ToString();
                }
            }
        }

        /// <summary>
        /// This read-only property returns true if there is no associated
        /// topic file by choice rather than it not being found.
        /// </summary>
        [Browsable(false)]
        public bool NoTopicFile
        {
            get { return noFile; }
        }

        /// <summary>
        /// This is used to get the content ID from the content layout file.
        /// </summary>
        /// <remarks>This should match an ID from a project file.  If not,
        /// it will serve as an container node with no associated topic.</remarks>
        [Browsable(false)]
        public string ContentId
        {
            get { return contentId; }
        }

        /// <summary>
        /// This is used to get the unique ID of the topic
        /// </summary>
        [Category("File"), Description("The unique ID of the topic")]
        public string Id
        {
            get
            {
                if(topicFile == null)
                    return contentId;

                return topicFile.Id;
            }
        }

        /// <summary>
        /// This is used to get the topic's revision number
        /// </summary>
        [Category("File"), Description("The topic's revision number")]
        public int RevisionNumber
        {
            get
            {
                if(topicFile == null)
                    return 1;

                return topicFile.RevisionNumber;
            }
        }

        /// <summary>
        /// This is used to get the filename of the related project file (if any)
        /// </summary>
        [Category("File"), Description("The associated project file (if any)")]
        public string TopicFilename
        {
            get
            {
                if(topicFile == null)
                {
                    if(noFile)
                        return "(None)";

                    return "(Not found)";
                }

                return topicFile.FileItem.Include.PersistablePath;
            }
        }

        /// <summary>
        /// This read-only property is used to get the document type
        /// </summary>
        [Browsable(false)]
        public DocumentType DocumentType
        {
            get { return topicFile.DocumentType; }
        }

        /// <summary>
        /// This is used to get the required title that should be used for the
        /// topic.
        /// </summary>
        /// <value>If not set, the topic filename without a path or extension
        /// is used.</value>
        [Category("Topic"), Description("The required topic title.  If " +
          "not set, it will use the filename without a path or extension."),
          DefaultValue(null)]
        public string Title
        {
            get { return title; }
            set
            {
                if(value != null && value.Trim().Length == 0)
                    value = null;

                title = value;
            }
        }

        /// <summary>
        /// This is used to get or set the topic's optional table of contents
        /// title.
        /// </summary>
        /// <value>This can be used to provide a different topic title in the
        /// table of contents.  If not set, it will be set to the
        /// <see cref="Title" /> value.</value>
        [Category("Topic"), Description("The optional table of contents " +
          "title.  If not set, it will use the Title property value."),
          DefaultValue(null)]
        public string TocTitle
        {
            get { return tocTitle; }
            set
            {
                if(value != null && value.Trim().Length == 0)
                    value = null;

                tocTitle = value;
            }
        }

        /// <summary>
        /// This is used to get or set the topic's optional link text.
        /// </summary>
        /// <value>This can be used to provide different text that is used in
        /// links that refer to the topic.  If not set, it will be set to the
        /// <see cref="Title" /> value.</value>
        [Category("Topic"), Description("The optional text to use for links " +
          "that refer to this topic.  If not set, it will use the Title " +
          "property value."), DefaultValue(null)]
        public string LinkText
        {
            get { return linkText; }
            set
            {
                if(value != null && value.Trim().Length == 0)
                    value = null;

                linkText = value;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not the topic is visible
        /// in the table of contents.
        /// </summary>
        /// <value>If set to false, the item will still be added to the
        /// help file but to be accessible, a link to it must appear in
        /// one of the other topics.</value>
        [Category("Topic"), Description("Indicate whether or not the " +
          "topic is visible in the table of contents"), DefaultValue(true)]
        public bool Visible { get; set; }

        /// <summary>
        /// This is used to get the additional attributes that will be added
        /// to MAML topic.
        /// </summary>
        [Category("Topic Metadata"), Description("Additional help attributes " +
          "to add to the generated help topic (MAML topics only)")]
        public MSHelpAttrCollection HelpAttributes
        {
            get { return helpAttributes; }
        }

        /// <summary>
        /// This is used to get the index keywords that will be added to the
        /// MAML topic.
        /// </summary>
        [Category("Topic Metadata"), Description("Help index keyword that " +
          "should be add to the generated help topic (MAML topics only)")]
        public MSHelpKeywordCollection Keywords
        {
            get { return keywords; }
        }

        /// <summary>
        /// This read-only property is used to get a title for display
        /// (i.e. in the designer).
        /// </summary>
        /// <value>If there is a <see cref="TocTitle" /> specified, it is used.
        /// If not, the <see cref="Title" /> value is used.  If it does not
        /// contain a value, the filename without the path and extension is
        /// used.  If the file has not been specified, does not exist, the
        /// document type is not recognized, or it is invalid (i.e. badly
        /// formed), it returns an appropriate message describing the
        /// problem.</value>
        [Browsable(false)]
        public string DisplayTitle
        {
            get
            {
                if(tocTitle != null)
                    return tocTitle;

                if(title != null)
                    return title;

                if(topicFile == null)
                    return "(No topic file specified)";

                if(topicFile.DocumentType == DocumentType.Invalid)
                    return "(Invalid document format: " +
                        topicFile.ErrorMessage + ")";

                if(topicFile.DocumentType == DocumentType.NotFound)
                    return "(File not found)";

                if(topicFile.DocumentType == DocumentType.None)
                    return "(Unknown document type)";

                return Path.GetFileNameWithoutExtension(topicFile.Name);
            }
        }

        /// <summary>
        /// This is used to get the sub-topics beneath this topic.
        /// </summary>
        [Browsable(false)]
        public TopicCollection Subtopics
        {
            get { return subtopics; }
        }

        /// <summary>
        /// This is used to when merging TOC files to determine the default
        /// topic.
        /// </summary>
        [Browsable(false)]
        public bool IsDefaultTopic { get; set; }

        /// <summary>
        /// This is used to specify how API content is parented to this topic
        /// or the topic's parent
        /// </summary>
        [Browsable(false)]
        public ApiParentMode ApiParentMode { get; set; }

        /// <summary>
        /// This is used to get or set whether or not the topic will server as
        /// the root content container in MS Help Viewer output
        /// </summary>
        [Browsable(false)]
        public bool IsMSHVRootContentContainer { get; set; }
        #endregion

        #region Designer methods
        //=====================================================================
        // Designer methods

        /// <summary>
        /// This is used to see if the <see cref="HelpAttributes"/> property
        /// should be serialized.
        /// </summary>
        /// <returns>True to serialize it, false if it matches the default
        /// and should not be serialized.</returns>
        /// <remarks>We do not allow resetting this property as it is a
        /// collection and we don't want to lose all items accidentally.</remarks>
        private bool ShouldSerializeHelpAttributes()
        {
            return (this.HelpAttributes.Count != 0);
        }

        /// <summary>
        /// This is used to see if the <see cref="Keywords"/> property should
        /// be serialized.
        /// </summary>
        /// <returns>True to serialize it, false if it matches the default
        /// and should not be serialized.</returns>
        /// <remarks>We do not allow resetting this property as it is a
        /// collection and we don't want to lose all items accidentally.</remarks>
        private bool ShouldSerializeKeywords()
        {
            return (this.Keywords.Count != 0);
        }
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public Topic()
        {
            contentId = Guid.NewGuid().ToString();
            subtopics = new TopicCollection(null);
            helpAttributes = new MSHelpAttrCollection(null);
            keywords = new MSHelpKeywordCollection();
            this.Visible = true;

            subtopics.ListChanged += new ListChangedEventHandler(
                childList_ListChanged);
            helpAttributes.ListChanged += new ListChangedEventHandler(
                childList_ListChanged);
            keywords.ListChanged += new ListChangedEventHandler(
                childList_ListChanged);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Mark the project as dirty if the contained lists change
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        /// <remarks>This may not be the best way to handle this.</remarks>
        private void childList_ListChanged(object sender, ListChangedEventArgs e)
        {
            if(this.Parent != null)
                this.Parent.ChildListChanged(this);
        }
        #endregion

        #region Read/write as XML
        //=====================================================================

        /// <summary>
        /// This is used to load the topic information from the project
        /// file.
        /// </summary>
        /// <param name="xr">The XML text reader from which the information
        /// is loaded.</param>
        internal void ReadXml(XmlReader xr)
        {
            Topic newTopic;
            string guid, parentMode;
            bool visible, attrValue;

            guid = xr.GetAttribute("id");

            if(guid != null && guid.Trim().Length != 0)
                contentId = guid;
            else
                contentId = Guid.NewGuid().ToString();

            if(!Boolean.TryParse(xr.GetAttribute("noFile"), out noFile))
                noFile = false;

            if(!Boolean.TryParse(xr.GetAttribute("visible"), out visible))
                visible = true;

            if(Boolean.TryParse(xr.GetAttribute("isDefault"), out attrValue))
                this.IsDefaultTopic = attrValue;

            if(Boolean.TryParse(xr.GetAttribute("isMSHVRoot"), out attrValue))
                this.IsMSHVRootContentContainer = attrValue;

            parentMode = xr.GetAttribute("apiParentMode");

            if(!String.IsNullOrEmpty(parentMode))
                this.ApiParentMode = (ApiParentMode)Enum.Parse(typeof(ApiParentMode), parentMode, true);

            this.Visible = visible;
            this.Title = xr.GetAttribute("title");
            this.TocTitle = xr.GetAttribute("tocTitle");
            this.LinkText = xr.GetAttribute("linkText");

            if(!xr.IsEmptyElement)
                while(!xr.EOF)
                {
                    xr.Read();

                    if(xr.NodeType == XmlNodeType.EndElement && xr.Name == "Topic")
                        break;

                    if(xr.NodeType == XmlNodeType.Element)
                        if(xr.Name == "HelpAttributes")
                            helpAttributes.ReadXml(xr);
                        else
                            if(xr.Name == "HelpKeywords")
                                keywords.ReadXml(xr);
                            else
                                if(xr.Name == "Topic")
                                {
                                    newTopic = new Topic();
                                    newTopic.ReadXml(xr);
                                    this.Subtopics.Add(newTopic);
                                }
                }
        }

        /// <summary>
        /// This is used to save the topic information to the project file
        /// </summary>
        /// <param name="xw">The XML text writer to which the information
        /// is written.</param>
        internal void WriteXml(XmlWriter xw)
        {
            xw.WriteStartElement("Topic");

            if(topicFile != null)
                xw.WriteAttributeString("id", topicFile.Id);
            else
                if(contentId != null)
                    xw.WriteAttributeString("id", contentId);

            xw.WriteAttributeString("visible", this.Visible.ToString(CultureInfo.InvariantCulture));

            if(noFile)
                xw.WriteAttributeString("noFile", "true");

            if(this.IsDefaultTopic)
                xw.WriteAttributeString("isDefault", "true");

            if(this.IsMSHVRootContentContainer)
                xw.WriteAttributeString("isMSHVRoot", "true");

            if(this.ApiParentMode != ApiParentMode.None)
                xw.WriteAttributeString("apiParentMode", this.ApiParentMode.ToString());

            if(title != null)
                xw.WriteAttributeString("title", title);

            if(tocTitle != null)
                xw.WriteAttributeString("tocTitle", tocTitle);

            if(linkText != null)
                xw.WriteAttributeString("linkText", linkText);

            if(helpAttributes.Count != 0)
                helpAttributes.WriteXml(xw, true);

            if(keywords.Count != 0)
                keywords.WriteXml(xw);

            if(subtopics.Count != 0)
                foreach(Topic t in subtopics)
                    t.WriteXml(xw);

            xw.WriteEndElement();
        }

        /// <summary>
        /// This is used to create the companion file used by the build
        /// component that resolves conceptual links.
        /// </summary>
        /// <param name="folder">The folder in which to place the file</param>
        /// <param name="builder">The build process</param>
        /// <remarks>The file will be named using the ID and a ".xml"
        /// extension.</remarks>
        internal void WriteCompanionFile(string folder, BuildProcess builder)
        {
            string linkElement = String.Empty;

            // MS Help Viewer doesn't support empty place holders so we automatically
            // generate a dummy place holder file for them.
            if(!noFile || (builder.CurrentProject.HelpFileFormat & HelpFileFormat.MSHelpViewer) != 0)
            {
                // Link text is optional
                if(!String.IsNullOrEmpty(linkText))
                    linkElement = String.Format(CultureInfo.InvariantCulture,
                        "    <linkText>{0}</linkText>\r\n", HttpUtility.HtmlEncode(linkText));

                // It's small enough that we'll just write it out as a string
                // rather than using an XML writer.
                using(StreamWriter sw = new StreamWriter(Path.Combine(folder, this.Id + ".cmp.xml"),
                  false, Encoding.UTF8))
                {
                    sw.WriteLine(
                        "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                        "<metadata>\r\n" +
                        "  <topic id=\"{0}\">\r\n" +
                        "    <title>{1}</title>\r\n" +
                        "{2}" +
                        "  </topic>\r\n" +
                        "</metadata>\r\n", this.Id, HttpUtility.HtmlEncode(this.DisplayTitle), linkElement);
                }
            }

            foreach(Topic t in subtopics)
                t.WriteCompanionFile(folder, builder);
        }

        /// <summary>
        /// Write out the topic metadata
        /// </summary>
        /// <param name="writer">The writer to which the metadata is written</param>
        /// <param name="builder">The build process</param>
        /// <remarks>This will recursively write out metadata for sub-topics
        /// as well.</remarks>
        internal void WriteMetadata(XmlWriter writer, BuildProcess builder)
        {
            // MS Help Viewer doesn't support empty place holders so we automatically
            // generate a dummy place holder file for them.
            if(!noFile || (builder.CurrentProject.HelpFileFormat & HelpFileFormat.MSHelpViewer) != 0)
            {
                writer.WriteStartElement("topic");
                writer.WriteAttributeString("id", this.Id);
                writer.WriteAttributeString("revisionNumber",
                    this.RevisionNumber.ToString(CultureInfo.InvariantCulture));

                // Write out the help file version project property value
                writer.WriteStartElement("item");
                writer.WriteAttributeString("id", "PBM_FileVersion");
                writer.WriteValue(builder.TransformText(builder.CurrentProject.HelpFileVersion));
                writer.WriteEndElement();

                // If no title is specified, use the display title
                writer.WriteStartElement("title");

                if(!String.IsNullOrEmpty(title))
                    writer.WriteString(title);
                else
                    writer.WriteString(this.DisplayTitle);

                writer.WriteEndElement();

                // TOC title is optional
                if(!String.IsNullOrEmpty(tocTitle))
                    writer.WriteElementString("tableOfContentsTitle", tocTitle);

                // The running header text ID is set to "runningHeaderText"
                // so that it pulls in the shared content item by that name.
                // This will equate to the project's HTML encoded HelpTitle
                // property value.
                writer.WriteStartElement("runningHeaderText");
                writer.WriteAttributeString("uscid", "runningHeaderText");
                writer.WriteEndElement();

                // Each topic includes the project-level help attributes
                foreach(MSHelpAttr attr in builder.CurrentProject.HelpAttributes)
                {
                    writer.WriteStartElement("attribute");
                    writer.WriteAttributeString("name", attr.AttributeName);

                    // Replace tags with their project property value
                    writer.WriteValue(builder.TransformText(attr.AttributeValue));

                    writer.WriteEndElement();
                }

                // Add topic-specific attributes
                foreach(MSHelpAttr attr in helpAttributes)
                {
                    writer.WriteStartElement("attribute");
                    writer.WriteAttributeString("name", attr.AttributeName);

                    // Replace tags with their project property value
                    writer.WriteValue(builder.TransformText(attr.AttributeValue));

                    writer.WriteEndElement();
                }

                // Add topic-specific index keywords
                foreach(MSHelpKeyword kw in keywords)
                {
                    writer.WriteStartElement("keyword");
                    writer.WriteAttributeString("index", kw.Index);

                    // Replace tags with their project property value
                    writer.WriteValue(builder.TransformText(kw.Term));

                    writer.WriteEndElement();
                }

                // If this is the default topic and the NamedUrlIndex keywords
                // for DefaultPage and/or HomePage are not present, add them.
                if(this.IsDefaultTopic)
                {
                    if(!keywords.Contains(new MSHelpKeyword("NamedUrlIndex", "DefaultPage")))
                    {
                        writer.WriteStartElement("keyword");
                        writer.WriteAttributeString("index", "NamedUrlIndex");
                        writer.WriteValue("DefaultPage");
                        writer.WriteEndElement();
                    }

                    if(!keywords.Contains(new MSHelpKeyword("NamedUrlIndex", "HomePage")))
                    {
                        writer.WriteStartElement("keyword");
                        writer.WriteAttributeString("index", "NamedUrlIndex");
                        writer.WriteValue("HomePage");
                        writer.WriteEndElement();
                    }
                }

                writer.WriteEndElement();   // </topic>
            }

            // Write metadata for sub-topics too
            foreach(Topic t in subtopics)
                t.WriteMetadata(writer, builder);
        }

        /// <summary>
        /// Write out the <b>BuildAssembler</b> manifest entry
        /// </summary>
        /// <param name="writer">The XML writer to which the entry is written</param>
        /// <param name="builder">The build process</param>
        /// <remarks>This will recursively write out entries for sub-topics
        /// as well.</remarks>
        internal void WriteManifest(XmlWriter writer, BuildProcess builder)
        {
            // MS Help Viewer doesn't support empty place holders so we automatically
            // generate a dummy place holder file for them.  Don't add an entry for
            // raw HTML files.
            if((!noFile && topicFile.DocumentType != DocumentType.Html) ||
              (noFile && (builder.CurrentProject.HelpFileFormat & HelpFileFormat.MSHelpViewer) != 0))
            {
                writer.WriteStartElement("topic");
                writer.WriteAttributeString("id", this.Id);
                writer.WriteEndElement();
            }

            foreach(Topic t in subtopics)
                t.WriteManifest(writer, builder);
        }
        #endregion

        #region Convert to link element format
        //=====================================================================

        /// <summary>
        /// Convert the topic to its <c>&lt;link&gt;</c> element form
        /// </summary>
        /// <param name="innerText">Optional inner text</param>
        /// <returns>The topic in its <c>&lt;link&gt;</c> element form</returns>
        public string ToLink(string innerText)
        {
            if(String.IsNullOrEmpty(innerText))
                return String.Concat("<link xlink:href=\"", contentId, "\" />");

            return String.Format(CultureInfo.CurrentCulture,
                "<link xlink:href=\"{0}\">{1}</link>", contentId, innerText);
        }

        /// <summary>
        /// Convert the topic to its <c>&lt;a&gt;</c> element form
        /// </summary>
        /// <param name="innerText">Optional inner text</param>
        /// <returns>The topic in its <c>&lt;a&gt;</c> element form</returns>
        public string ToAnchor(string innerText)
        {
            if(String.IsNullOrEmpty(innerText))
                return String.Format(CultureInfo.CurrentCulture, "<a href=\"html/{0}.htm\">{1}</a>", contentId,
                    this.DisplayTitle);

            return String.Format(CultureInfo.CurrentCulture, "<a href=\"html/{0}.htm\">{1}</a>", contentId,
                innerText);
        }
        #endregion
    }
}
