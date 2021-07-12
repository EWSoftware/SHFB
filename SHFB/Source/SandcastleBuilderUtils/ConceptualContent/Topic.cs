//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : Topic.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/14/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains a class representing a conceptual content topic.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/24/2008  EFW  Created the code
// 08/07/2008  EFW  Modified for use with the new project format
// 06/06/2010  EFW  Added support for MS Help Viewer output
// 07/01/2010  EFW  Added support for API parent mode setting
// 12/15/2011  EFW  Updated for use with the new content layout editor
// 06/05/2015  EFW  Removed support for Help 2 attributes
//===============================================================================================================

// Ignore Spelling: utf xlink

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

using Sandcastle.Core;
using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.Utils.ConceptualContent
{
    /// <summary>
    /// This represents a conceptual content topic
    /// </summary>
    public class Topic : INotifyPropertyChanged
    {
        #region Private data members
        //=====================================================================

        private TopicFile topicFile;
        private readonly TopicCollection subtopics;
        private string contentId, title, tocTitle, linkText;
        private bool noFile, isSelected, isExpanded, isVisible, isDefaultTopic, isMSHVRoot;
        private ApiParentMode apiParentMode;
        private readonly MSHelpKeywordCollection keywords;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to track the topic's parent collection
        /// </summary>
        /// <remarks>This is used by the designer to move items around within the collection</remarks>
        public TopicCollection Parent { get; internal set; }

        /// <summary>
        /// This is used to get or set the topic file information related to the topic
        /// </summary>
        /// <value>If there is no topic file, this topic serves as a container node for its sub-topics and no
        /// content will be displayed for it when selected in the help file's table of contents.</value>
        public TopicFile TopicFile
        {
            get => topicFile;
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

                // This may affect the display title property
                this.OnPropertyChanged(nameof(DisplayTitle));
            }
        }

        /// <summary>
        /// This read-only property returns true if there is no associated topic file by choice rather than it
        /// not being found.
        /// </summary>
        public bool NoTopicFile => noFile;

        /// <summary>
        /// This is used to get the content ID from the content layout file
        /// </summary>
        /// <remarks>This should match an ID from a project file.  If not, it will serve as an container node
        /// with no associated topic.</remarks>
        public string ContentId => contentId;

        /// <summary>
        /// This is used to get the unique ID of the topic
        /// </summary>
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

                return topicFile.ContentFile.PersistablePath;
            }
        }

        /// <summary>
        /// This read-only property is used to get the document type
        /// </summary>
        public DocumentType DocumentType => topicFile.DocumentType;

        /// <summary>
        /// This is used to get the required title that should be used for the topic
        /// </summary>
        /// <value>If not set, the topic filename without a path or extension is used</value>
        public string Title
        {
            get => title;
            set
            {
                if(value != title)
                {
                    if(value != null && value.Trim().Length == 0)
                        value = null;

                    title = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(DisplayTitle));
                }
            }
        }

        /// <summary>
        /// This is used to get or set the topic's optional table of contents title
        /// </summary>
        /// <value>This can be used to provide a different topic title in the table of contents.  If not set, it
        /// will be set to the <see cref="Title" /> value.</value>
        public string TocTitle
        {
            get => tocTitle;
            set
            {
                if(value != tocTitle)
                {
                    if(value != null && value.Trim().Length == 0)
                        value = null;

                    tocTitle = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(DisplayTitle));
                }
            }
        }

        /// <summary>
        /// This is used to get or set the topic's optional link text
        /// </summary>
        /// <value>This can be used to provide different text that is used in links that refer to the topic.  If
        /// not set, it will be set to the <see cref="Title" /> value.</value>
        public string LinkText
        {
            get => linkText;
            set
            {
                if(value != linkText)
                {
                    if(value != null && value.Trim().Length == 0)
                        value = null;

                    linkText = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set whether or not the topic is visible in the table of contents
        /// </summary>
        /// <value>If set to false, the item will still be added to the help file but to be accessible, a link to
        /// it must appear in one of the other topics.</value>
        public bool Visible
        {
            get => isVisible;
            set
            {
                if(value != isVisible)
                {
                    isVisible = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(ToolTip));  // Affects tool tip too

                    // The default topic must be visible.  The MSHV root must not be visible.  A hidden topic
                    // cannot be the API insertion point.
                    if(!isVisible)
                    {
                        this.IsDefaultTopic = false;
                        this.ApiParentMode = ApiParentMode.None;
                    }
                    else
                        this.IsMSHVRootContentContainer = false;
                }
            }
        }

        /// <summary>
        /// This is used to get the index keywords that will be added to the MAML topic
        /// </summary>
        public MSHelpKeywordCollection Keywords => keywords;

        /// <summary>
        /// This read-only property is used to get a title for display (i.e. in the designer)
        /// </summary>
        /// <value>If there is a <see cref="TocTitle" /> specified, it is used.  If not, the <see cref="Title" />
        /// value is used.  If it does not contain a value, the filename without the path and extension is used.
        /// If the file has not been specified, does not exist, the document type is not recognized, or it is
        /// invalid (i.e. badly formed), it returns an appropriate message describing the problem.</value>
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
                    return "(Invalid document format: " + topicFile.ErrorMessage + ")";

                if(topicFile.DocumentType == DocumentType.NotFound)
                    return "(File not found)";

                if(topicFile.DocumentType == DocumentType.None)
                    return "(Unknown document type)";

                return Path.GetFileNameWithoutExtension(topicFile.Name);
            }
        }

        /// <summary>
        /// This is used to get the sub-topics beneath this topic
        /// </summary>
        public TopicCollection Subtopics => subtopics;

        /// <summary>
        /// This is used to when merging TOC files to determine the default topic
        /// </summary>
        public bool IsDefaultTopic
        {
            get => isDefaultTopic;
            set
            {
                if(value != isDefaultTopic)
                {
                    isDefaultTopic = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(ToolTip));  // Affects tool tip too

                    // The default topic must be visible and cannot be the MSHV root container
                    if(isDefaultTopic)
                    {
                        this.Visible = true;
                        this.IsMSHVRootContentContainer = false;
                    }
                }
            }
        }

        /// <summary>
        /// This is used to specify how API content is parented to this topic or the topic's parent
        /// </summary>
        public ApiParentMode ApiParentMode
        {
            get => apiParentMode;
            set
            {
                if(value != apiParentMode)
                {
                    apiParentMode = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(ToolTip));  // Affects tool tip too

                    // The API parent node must be visible and cannot be the MSHV root container
                    if(value != ApiParentMode.None)
                    {
                        this.Visible = true;
                        this.IsMSHVRootContentContainer = false;
                    }
                }
            }
        }

        /// <summary>
        /// This is used to get or set whether or not the topic will server as the root content container in MS
        /// Help Viewer output
        /// </summary>
        public bool IsMSHVRootContentContainer
        {
            get => isMSHVRoot;
            set
            {
                if(value != isMSHVRoot)
                {
                    isMSHVRoot = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(ToolTip));  // Affects tool tip too

                    // The MSHV root container must not be visible and cannot be the default topic or API
                    // insertion point.
                    if(isMSHVRoot)
                    {
                        this.Visible = this.IsDefaultTopic = false;
                        this.ApiParentMode = ApiParentMode.None;
                    }
                    else
                        this.Visible = true;
                }
            }
        }

        /// <summary>
        /// This is used to get or set whether or not the entity is selected
        /// </summary>
        /// <remarks>Used by the editor for binding in the tree view.  The value is serialized when saved so that
        /// its state is remembered when reloaded.</remarks>
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if(value != isSelected)
                {
                    isSelected = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set whether or not the entity is expanded
        /// </summary>
        /// <remarks>Used by the editor for binding in the tree view.  The value is serialized when saved so that
        /// its state is remembered when reloaded.</remarks>
        public bool IsExpanded
        {
            get => isExpanded && this.Subtopics.Count != 0;
            set
            {
                if(value != isExpanded)
                {
                    isExpanded = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This returns a description of the topic that can be used as a tool tip
        /// </summary>
        public string ToolTip
        {
            get
            {
                string description = this.DisplayTitle;

                if(isMSHVRoot)
                    description += "\nMS Help Viewer root container";
                else
                    if(!isVisible)
                        description += "\nHidden, will not appear in the TOC";
                    else
                    {
                        if(isDefaultTopic)
                            description += "\nDefault topic";

                        if(apiParentMode != ApiParentMode.None)
                        {
                            if(isDefaultTopic)
                                description += " / ";
                            else
                                description += "\n";

                            switch(apiParentMode)
                            {
                                case ApiParentMode.InsertAfter:
                                    description += "Insert API content after topic";
                                    break;

                                case ApiParentMode.InsertBefore:
                                    description += "Insert API content before topic";
                                    break;

                                default:
                                    description += "Insert API content as child of topic";
                                    break;
                            }
                        }
                    }

                return description;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public Topic()
        {
            contentId = Guid.NewGuid().ToString();
            subtopics = new TopicCollection(null);
            keywords = new MSHelpKeywordCollection();
            this.Visible = true;

            subtopics.ListChanged += childList_ListChanged;
            keywords.ListChanged += childList_ListChanged;
        }
        #endregion

        #region INotifyPropertyChanged Members
        //=====================================================================

        /// <summary>
        /// The property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This raises the <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="propertyName">The property name that changed</param>
        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Mark the project as dirty if the contained lists change
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        /// <remarks>This may not be the best way to handle this</remarks>
        private void childList_ListChanged(object sender, ListChangedEventArgs e)
        {
            if(this.Parent != null)
                this.Parent.ChildListChanged(this, e);
        }
        #endregion

        #region Read/write as XML
        //=====================================================================

        /// <summary>
        /// This is used to load the topic information from the project file
        /// </summary>
        /// <param name="xr">The XML text reader from which the information is loaded</param>
        internal void ReadXml(XmlReader xr)
        {
            Topic newTopic;
            string guid, parentMode;

            guid = xr.GetAttribute("id");

            if(guid != null && guid.Trim().Length != 0)
                contentId = guid;
            else
                contentId = Guid.NewGuid().ToString();

            if(!Boolean.TryParse(xr.GetAttribute("noFile"), out noFile))
                noFile = false;

            if(!Boolean.TryParse(xr.GetAttribute("visible"), out bool visible))
                visible = true;

            if(Boolean.TryParse(xr.GetAttribute("isDefault"), out bool attrValue))
                this.IsDefaultTopic = attrValue;

            if(Boolean.TryParse(xr.GetAttribute("isMSHVRoot"), out attrValue))
                this.IsMSHVRootContentContainer = attrValue;

            if(Boolean.TryParse(xr.GetAttribute("isExpanded"), out attrValue))
                this.IsExpanded = attrValue;

            if(Boolean.TryParse(xr.GetAttribute("isSelected"), out attrValue))
                this.IsSelected = attrValue;

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
        /// <param name="xw">The XML text writer to which the information is written</param>
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

            if(this.IsExpanded)
                xw.WriteAttributeString("isExpanded", "true");

            if(this.IsSelected)
                xw.WriteAttributeString("isSelected", "true");

            if(this.ApiParentMode != ApiParentMode.None)
                xw.WriteAttributeString("apiParentMode", this.ApiParentMode.ToString());

            if(title != null)
                xw.WriteAttributeString("title", title);

            if(tocTitle != null)
                xw.WriteAttributeString("tocTitle", tocTitle);

            if(linkText != null)
                xw.WriteAttributeString("linkText", linkText);

            if(keywords.Count != 0)
                keywords.WriteXml(xw);

            if(subtopics.Count != 0)
                foreach(Topic t in subtopics)
                    t.WriteXml(xw);

            xw.WriteEndElement();
        }

        /// <summary>
        /// This is used to create the companion file used by the build component that resolves conceptual links
        /// </summary>
        /// <param name="folder">The folder in which to place the file</param>
        /// <param name="builder">The build process</param>
        /// <remarks>The file will be named using the ID and a ".xml" extension</remarks>
        internal void WriteCompanionFile(string folder, BuildProcess builder)
        {
            string linkElement = String.Empty;

            // MS Help Viewer doesn't support empty place holders so we automatically generate a dummy place
            // holder file for them.
            if(!noFile || (builder.CurrentProject.HelpFileFormat & HelpFileFormats.MSHelpViewer) != 0)
            {
                // Link text is optional
                if(!String.IsNullOrEmpty(linkText))
                    linkElement = String.Format(CultureInfo.InvariantCulture, "    <linkText>{0}</linkText>\r\n",
                        WebUtility.HtmlEncode(linkText));

                // It's small enough that we'll just write it out as a string rather than using an XML writer
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
                        "</metadata>\r\n", WebUtility.HtmlEncode(this.Id),
                        WebUtility.HtmlEncode(this.DisplayTitle), linkElement);
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
            // MS Help Viewer doesn't support empty place holders so we automatically generate a dummy place
            // holder file for them.
            if(!noFile || (builder.CurrentProject.HelpFileFormat & HelpFileFormats.MSHelpViewer) != 0)
            {
                writer.WriteStartElement("topic");
                writer.WriteAttributeString("id", this.Id);
                writer.WriteAttributeString("revisionNumber",
                    this.RevisionNumber.ToString(CultureInfo.InvariantCulture));

                // Write out the help file version project property value
                writer.WriteStartElement("item");
                writer.WriteAttributeString("id", "PBM_FileVersion");
                writer.WriteValue(builder.SubstitutionTags.TransformText(builder.CurrentProject.HelpFileVersion));
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

                // Add topic-specific index keywords
                foreach(MSHelpKeyword kw in keywords)
                {
                    writer.WriteStartElement("keyword");
                    writer.WriteAttributeString("index", kw.Index);

                    // Replace tags with their project property value
                    writer.WriteValue(builder.SubstitutionTags.TransformText(kw.Term));

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();   // </topic>
            }

            // Write metadata for sub-topics too
            foreach(Topic t in subtopics)
                t.WriteMetadata(writer, builder);
        }

        /// <summary>
        /// Write out the <strong>BuildAssembler</strong> manifest entry
        /// </summary>
        /// <param name="writer">The XML writer to which the entry is written</param>
        /// <param name="builder">The build process</param>
        /// <remarks>This will recursively write out entries for sub-topics as well</remarks>
        internal void WriteManifest(XmlWriter writer, BuildProcess builder)
        {
            // MS Help Viewer doesn't support empty place holders so we automatically generate a dummy place
            // holder file for them.
            if(!noFile || (builder.CurrentProject.HelpFileFormat & HelpFileFormats.MSHelpViewer) != 0)
            {
                writer.WriteStartElement("topic");
                writer.WriteAttributeString("id", this.Id);
                writer.WriteAttributeString("type", "MAML");
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
