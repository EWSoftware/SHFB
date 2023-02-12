//===============================================================================================================
// System  : Sandcastle Build Components
// File    : MSHCComponent.cs
// Note    : Copyright 2010-2023 Microsoft Corporation
//
// This file contains a modified version of the original MSHCComponent that allows the inclusion of a sortOrder
// attribute on the table of contents file elements.  This allows the sort order of the elements to be defined
// to set the proper placement of the TOC entries when parented to an entry outside of the help file and to
// parent the API content within a conceptual content folder.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Change History
// 02/16/2012 - EFW - Merged my changes into the code
// 09/28/2012 - EFW - Changed "SelfBranded" to "Microsoft.Help.SelfBranded" for Help Viewer 2.0 support.
//                    Removed the ContentType metadata as it's output by the XSL transformations.
//                    Removed header bottom fix up code as it is handled in the XSL transformations and script.
// 12/23/2013 - EFW - Updated the build component to be discoverable via MEF
// 12/24/2015 - EFW - Updated to be thread safe
// 03/01/2022 - EFW - Removed all data island related code as the new presentation styles render the help viewer
//                    elements directly now.
//===============================================================================================================

// Ignore Spelling: Fehr

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Sandcastle.Tools.BuildComponents
{
    /// <summary>
    /// This class is a modified version of the original <c>MSHCComponent</c> that is used to add MS Help Viewer
    /// meta data to the topics.  This version allows the inclusion of a <c>sortOrder</c> attribute on the table
    /// of contents file elements.  This allows the sort order of the elements to be defined to set the proper
    /// placement of the TOC entries when parented to an entry outside of the help file and to parent the API
    /// content within a conceptual content folder.
    /// </summary>
    /// <remarks>The <c>sortOrder</c> attributes are optional.  If not found, standard ordering is applied
    /// starting from zero.  If a <c>sortOrder</c> attribute is found, numbering starts from that value for the
    /// associated topic and increments by one for all subsequent topics until another <c>sortOrder</c> attribute
    /// is encountered or the end of the group is reached.</remarks>
    /// <example>
    /// <code language="xml" title="Example Component Configuration">
    /// &lt;component id="Microsoft Help Viewer Metadata Component"&gt;
    ///   &lt;data self-branded="true" topic-version="100" toc-file="toc.xml"
    ///   toc-parent="" toc-parent-version="100" /&gt;
    /// &lt;/component&gt;
    /// </code>
    ///
    /// <code language="xml" title="Example toc.xml File">
    /// &lt;?xml version="1.0" encoding="utf-8"?&gt;
    /// &lt;topics&gt;
    ///   &lt;!-- Sort our content below that of the parent node's existing sub-topics --&gt;
    ///   &lt;topic id="d4648875-d41a-783b-d5f4-638df39ee413" file="d4648875-d41a-783b-d5f4-638df39ee413" sortOrder="100"&gt;
    ///     &lt;topic id="57f7aedc-17d3-4547-bdf9-5b468a08a1bc" file="57f7aedc-17d3-4547-bdf9-5b468a08a1bc" /&gt;
    ///     &lt;topic id="0e6bbd29-775a-8deb-c4f5-5b1e63349ef1" file="0e6bbd29-775a-8deb-c4f5-5b1e63349ef1" /&gt;
    ///     &lt;topic id="fcdfafc4-7625-f407-d8e9-ec006944e1d7" file="fcdfafc4-7625-f407-d8e9-ec006944e1d7" /&gt;
    ///     &lt;!-- API content (7 namespaces, merged later) goes here and this topic follows it --&gt;
    ///     &lt;topic id="ce37cf86-fd95-49fc-b048-ba7d25d68d87" file="ce37cf86-fd95-49fc-b048-ba7d25d68d87" sortOrder="10"&gt;
    ///   &lt;/topic&gt;
    ///   .
    ///   .
    ///   .
    /// &lt;/topics&gt;
    /// </code>
    /// </example>
    public class MSHCComponent : BuildComponentCore
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("Microsoft Help Viewer Metadata Component")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new MSHCComponent(this.BuildAssembler);
            }
        }
        #endregion

        #region TOC information class
        //=====================================================================

        // TOC information of a document
        private class TocInfo
        {
            public TocInfo(string parent, string parentVersion, int order)
            {
                this.Parent = parent;
                this.ParentVersion = parentVersion;
                this.Order = order;
            }

            public string Parent { get ; }
            public string ParentVersion { get; }
            public int Order { get; }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private string _topicVersion = "100", _tocParent = "-1", _tocParentVersion = "100", iconPath,
            styleSheetPath, scriptPath;
        private readonly Dictionary<string, TocInfo> _toc = new Dictionary<string, TocInfo>();

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected MSHCComponent(IBuildAssembler buildAssembler) : base(buildAssembler)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            iconPath = this.BuildAssembler.TopicTransformation.IconPath;
            styleSheetPath = this.BuildAssembler.TopicTransformation.StyleSheetPath;
            scriptPath = this.BuildAssembler.TopicTransformation.ScriptPath;

            string tocFile = "toc.xml";
            XPathNavigator data = configuration.SelectSingleNode("data");

            if(data != null)
            {
                string value = data.GetAttribute("topic-version", String.Empty);

                if(!String.IsNullOrEmpty(value))
                    _topicVersion = value;

                value = data.GetAttribute("toc-parent", String.Empty);

                if(!String.IsNullOrEmpty(value))
                    _tocParent = value;

                value = data.GetAttribute("toc-parent-version", String.Empty);

                if(!String.IsNullOrEmpty(value))
                    _tocParentVersion = value;

                value = data.GetAttribute("toc-file", String.Empty);

                if(!String.IsNullOrEmpty(value))
                    tocFile = value;
            }

            using(var reader = XmlReader.Create(Path.GetFullPath(Environment.ExpandEnvironmentVariables(tocFile)),
              new XmlReaderSettings { CloseInput = true }))
            {
                XPathDocument document = new XPathDocument(reader);
                XPathNavigator navigator = document.CreateNavigator();
                LoadToc(navigator.SelectSingleNode("/topics"), _tocParent, _tocParentVersion);
            }
        }

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            if(document == null)
                throw new ArgumentNullException(nameof(document));

            XmlElement html = document.DocumentElement;
            
            // Remove the relative path from src and href attribute values for icons, style sheets, and scripts
            foreach(XmlNode srcHref in html.SelectNodes("//*[@src|@href]"))
            {
                var attr = srcHref.Attributes["src"] ?? srcHref.Attributes["href"];

                if(attr.Value.StartsWith(iconPath, StringComparison.OrdinalIgnoreCase) ||
                  attr.Value.StartsWith(styleSheetPath, StringComparison.OrdinalIgnoreCase) ||
                  attr.Value.StartsWith(scriptPath, StringComparison.OrdinalIgnoreCase))
                {
                    while(attr.Value.StartsWith("../", StringComparison.Ordinal))
                        attr.Value = attr.Value.Substring(3);
                }
            }

            XmlNode head = html.SelectSingleNode("head");

            if(head == null)
            {
                head = document.CreateElement("head");

                if(!html.HasChildNodes)
                    html.AppendChild(head);
                else
                    html.InsertBefore(head, html.FirstChild);
            }

            AddMHSMeta(head, "Microsoft.Help.TopicVersion", _topicVersion);

            string id = null;

            if(head.SelectSingleNode("meta[@name='Microsoft.Help.Id']") is XmlElement idElement)
                id = idElement.GetAttribute("content");

            if(id != null && _toc.TryGetValue(id, out TocInfo tocInfo))
            {
                AddMHSMeta(head, "Microsoft.Help.TocParent", tocInfo.Parent);

                if(tocInfo.Parent != "-1")
                    AddMHSMeta(head, "Microsoft.Help.TOCParentTopicVersion", tocInfo.ParentVersion);

                AddMHSMeta(head, "Microsoft.Help.TocOrder", tocInfo.Order.ToString(CultureInfo.InvariantCulture));
            }

            // Work around an odd bug in the help viewer.  Depending on the ordering of the metadata and lack of
            // whitespace, it can miss the title element and the TOC is blank.  By adding a CR/LF before and
            // after the title element, we can work around the issue.
            XmlNode title = head.SelectSingleNode("title");

            head.InsertBefore(document.CreateTextNode("\r\n"), title);
            head.InsertAfter(document.CreateTextNode("\r\n"), title);
        }
        #endregion

        #region Private helper methods
        //=====================================================================

        // Loads TOC structure from an XPathNavigator
        private void LoadToc(XPathNavigator navigator, string parent, string parentVersion)
        {
            // EFW - Reworked to support sortOrder attribute
            int sortOrder = 0;
            XPathNodeIterator interator = navigator.SelectChildren("topic", String.Empty);

            while(interator.MoveNext())
            {
                XPathNavigator current = interator.Current;
                string id = current.GetAttribute("id", String.Empty);

                // If a sort order is defined, start from that value
                string order = current.GetAttribute("sortOrder", String.Empty);

                if(!String.IsNullOrEmpty(order) && Int32.TryParse(order, out int tempOrder))
                    sortOrder = tempOrder;

                if(!String.IsNullOrEmpty(id))
                {
                    TocInfo info = new TocInfo(parent, parentVersion, sortOrder++);

                    // EFW - Work around a bug in Sandcastle that can result in duplicate IDs
                    // by using the indexer to add the topic rather than Add() which throws
                    // an exception when the duplicate is encountered.
                    _toc[id] = info;

                    LoadToc(current, id, _topicVersion);
                }
            }
        }

        // Adds Microsoft Help System meta data to the output document
        private static void AddMHSMeta(XmlNode headElement, string name, string content)
        {
            if(!String.IsNullOrEmpty(content) && headElement.OwnerDocument.SelectSingleNode(
              $"//meta[@name='{name}']") == null)
            {
                var elem = headElement.OwnerDocument.CreateElement("meta");
                elem.SetAttribute("name", name);
                elem.SetAttribute("content", content);
                headElement.AppendChild(elem);
            }
        }
        #endregion
    }
}
