//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : Topic.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/05/2025
// Note    : Copyright 2008-2025, Eric Woodruff, All rights reserved
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
using System.Runtime.CompilerServices;
using System.Xml;

namespace Sandcastle.Core.ConceptualContent;

/// <summary>
/// This represents a conceptual content topic
/// </summary>
public class Topic : INotifyPropertyChanged
{
    #region Private data members
    //=====================================================================

    private string contentId;
    private bool noFile;

    private readonly TopicCollection subtopics;
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
        get;
        set
        {
            field = value;

            if(field != null)
            {
                noFile = false;
                contentId = field.Id;
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
    public string Id => this.TopicFile?.Id ?? contentId;

    /// <summary>
    /// This is used to get the unique alternate ID of the topic
    /// </summary>
    public string AlternateId => this.TopicFile?.AlternateId;

    /// <summary>
    /// This is used to get the filename of the related project file (if any)
    /// </summary>
    public string TopicFilename => this.TopicFile?.ContentFile.PersistablePath ?? (noFile ? "(None)" : "(Not found)");

    /// <summary>
    /// This read-only property is used to get the document type
    /// </summary>
    public DocumentType DocumentType => this.TopicFile?.DocumentType ?? DocumentType.None;

    /// <summary>
    /// This is used to get the required title that should be used for the topic
    /// </summary>
    /// <value>If not set, the topic filename without a path or extension is used</value>
    public string Title
    {
        get;
        set
        {
            if(field != value)
            {
                if(value != null && value.Trim().Length == 0)
                    value = null;

                field = value;
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
        get;
        set
        {
            if(field != value)
            {
                if(value != null && value.Trim().Length == 0)
                    value = null;

                field = value;
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
        get;
        set
        {
            if(field != value)
            {
                if(value != null && value.Trim().Length == 0)
                    value = null;

                field = value;
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
        get;
        set
        {
            if(field != value)
            {
                field = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(ToolTip));  // Affects tool tip too

                // The default topic must be visible.  The MSHV root must not be visible.  A hidden topic
                // cannot be the API insertion point.
                if(!field)
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
            if(this.TocTitle != null)
                return this.TocTitle;

            if(this.Title != null)
                return this.Title;

            if(this.LinkText != null)
                return this.LinkText;

            if(this.TopicFile == null)
                return "(No topic file specified)";

            if(this.TopicFile.DocumentType == DocumentType.Invalid)
                return "(Invalid document format: " + this.TopicFile.ErrorMessage + ")";

            if(this.TopicFile.DocumentType == DocumentType.NotFound)
                return "(File not found)";

            if(this.TopicFile.DocumentType == DocumentType.None)
                return "(Unknown document type)";

            return Path.GetFileNameWithoutExtension(this.TopicFile.Name);
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
        get;
        set
        {
            if(field != value)
            {
                field = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(ToolTip));  // Affects tool tip too

                // The default topic must be visible and cannot be the MSHV root container
                if(field)
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
        get;
        set
        {
            if(field != value)
            {
                field = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(ToolTip));  // Affects tool tip too

                // The API parent node must be visible and cannot be the MSHV root container
                if(field != ApiParentMode.None)
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
        get;
        set
        {
            if(field != value)
            {
                field = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(ToolTip));  // Affects tool tip too

                // The MSHV root container must not be visible and cannot be the default topic or API
                // insertion point.
                if(field)
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
        get;
        set
        {
            if(field != value)
            {
                field = value;
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
        get => field && this.Subtopics.Count != 0;
        set
        {
            if(field != value)
            {
                field = value;
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

            if(this.IsMSHVRootContentContainer)
                description += "\nMS Help Viewer root container";
            else
            {
                if(!this.Visible)
                    description += "\nHidden, will not appear in the TOC";
                else
                {
                    if(this.IsDefaultTopic)
                        description += "\nDefault topic";

                    if(this.ApiParentMode != ApiParentMode.None)
                    {
                        if(this.IsDefaultTopic)
                            description += " / ";
                        else
                            description += "\n";

                        switch(this.ApiParentMode)
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
            }

            return description;
        }
    }

    /// <summary>
    /// This read-only property returns true if the file is a MAML topic or an empty container node and false
    /// if it is a Markdown topic.
    /// </summary>
    public bool IsMamlTopic => this.TopicFile?.MarkdownFile is null;

    #endregion

    #region Constructor
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    public Topic()
    {
        contentId = Guid.NewGuid().ToString();
        subtopics = new(null);
        keywords = [];

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
        this.Parent?.ChildListChanged(this, e);
    }

    /// <summary>
    /// Convert the topic to its Markdown link form
    /// </summary>
    /// <param name="innerText">Optional inner text</param>
    /// <returns>The topic in its Markdown link form</returns>
    public string ToMarkdownLink(string innerText)
    {
        return $"[{innerText}](@{contentId})";
    }

    /// <summary>
    /// Convert the topic to its <c>&lt;link&gt;</c> element form
    /// </summary>
    /// <param name="innerText">Optional inner text</param>
    /// <returns>The topic in its <c>&lt;link&gt;</c> element form</returns>
    public string ToLink(string innerText)
    {
        if(String.IsNullOrEmpty(innerText))
            return $"<link xlink:href=\"{contentId}\" />";

        return $"<link xlink:href=\"{contentId}\">{innerText}</link>";
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
        {
            while(!xr.EOF)
            {
                xr.Read();

                if(xr.NodeType == XmlNodeType.EndElement && xr.Name == "Topic")
                    break;

                if(xr.NodeType == XmlNodeType.Element)
                {
                    if(xr.Name == "HelpKeywords")
                        keywords.ReadXml(xr);
                    else
                    {
                        if(xr.Name == "Topic")
                        {
                            newTopic = new Topic();
                            newTopic.ReadXml(xr);
                            this.Subtopics.Add(newTopic);
                        }
                    }
                }
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

        if(this.TopicFile != null)
            xw.WriteAttributeString("id", this.TopicFile.Id);
        else
        {
            if(contentId != null)
                xw.WriteAttributeString("id", contentId);
        }

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

        // These are stored in the Markdown files so don't need to appear in the content layout file
        if(this.TopicFile?.MarkdownFile is null)
        {
            if(this.Title != null)
                xw.WriteAttributeString("title", this.Title);

            if(this.TocTitle != null)
                xw.WriteAttributeString("tocTitle", this.TocTitle);

            if(this.LinkText != null)
                xw.WriteAttributeString("linkText", this.LinkText);

            if(keywords.Count != 0)
                keywords.WriteXml(xw);
        }

        if(subtopics.Count != 0)
        {
            foreach(Topic t in subtopics)
                t.WriteXml(xw);
        }

        xw.WriteEndElement();
    }
    #endregion
}
