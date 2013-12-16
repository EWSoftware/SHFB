//=============================================================================
// System  : HTML to MAML Converter
// File    : Topic.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/07/2012
// Note    : Copyright 2008-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that represents an individual topic.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  09/12/2008  EFW  Created the code
// 1.0.0.2  08/07/2012  EFW  Incorporated various changes from Dany R
//=============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

using SandcastleBuilder.Utils;

namespace HtmlToMamlConversion
{
    /// <summary>
    /// This represents a topic file
    /// </summary>
    public class Topic
    {
        #region Private data members
        //=====================================================================

        private FilePath sourceFile;
        private TopicCollection subtopics;
        private MSHelpAttrCollection helpAttributes;
        private MSHelpKeywordCollection helpKeywords;
        private Guid id;
        private string title, body, topicAbstract, revisionNumber;
        private bool tocExclude, defaultTopic, splitToc;
        private int sortOrder;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get the source HTML filename
        /// </summary>
        /// <value>If set to null, the item will represent a container node with no associated topic.</value>
        public FilePath SourceFile
        {
            get { return sourceFile; }
            set { sourceFile = value; }
        }

        /// <summary>
        /// This is used to get the collection of sub-topics, if any
        /// </summary>
        public TopicCollection Subtopics
        {
            get { return subtopics; }
        }

        /// <summary>
        /// This returns the topic's unique ID
        /// </summary>
        public Guid Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// Get or set the topic's revision number
        /// </summary>
        public string RevisionNumber
        {
            get { return revisionNumber; }
            set { revisionNumber = value; }
        }

        /// <summary>
        /// Get or set the topic's title
        /// </summary>
        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        /// <summary>
        /// Get or set the topic's abstract
        /// </summary>
        public string TopicAbstract
        {
            get { return topicAbstract; }
            set { topicAbstract = value; }
        }

        /// <summary>
        /// Get or set the topic's body
        /// </summary>
        public string Body
        {
            get { return body; }
            set { body = value; }
        }

        /// <summary>
        /// This returns the TOC exclude flag if found in the topic
        /// </summary>
        public bool TocExclude
        {
            get { return tocExclude; }
        }

        /// <summary>
        /// This returns the default topic flag if found in the topic
        /// </summary>
        public bool IsDefaultTopic
        {
            get { return defaultTopic; }
        }

        /// <summary>
        /// This returns the split TOC flag if found in the topic
        /// </summary>
        public bool SplitToc
        {
            get { return splitToc; }
        }

        /// <summary>
        /// This returns the sort order value if found in the topic
        /// </summary>
        public int SortOrder
        {
            get { return sortOrder; }
        }

        /// <summary>
        /// Get the help attributes collection
        /// </summary>
        /// <value>If null or empty, there are no attributes</value>
        public MSHelpAttrCollection HelpAttributes
        {
            get { return helpAttributes; }
        }

        /// <summary>
        /// Get the help keywords collection
        /// </summary>
        /// <value>If null or empty, there are no keywords </value>
        public MSHelpKeywordCollection HelpKeywords
        {
            get { return helpKeywords; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">The source file or null to create an empty container node with no
        /// associated topic</param>
        public Topic(string source)
        {
            id = Guid.NewGuid();
            revisionNumber = "1";
            sortOrder = Int32.MaxValue;

            if(source != null)
                sourceFile = new FilePath(source, HtmlToMaml.PathProvider);

            subtopics = new TopicCollection();
        }
        #endregion

        #region ToString
        //=====================================================================

        /// <summary>
        /// Convert to string for debugging purposes
        /// </summary>
        /// <returns>The string representation of the topic</returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2} sub-topics",
                sourceFile.PersistablePath, id, subtopics.Count);
        }
        #endregion

        #region Conversion helper methods
        //=====================================================================

        /// <summary>
        /// Parse the topic and its sub-topic files to extract the information for conversion
        /// </summary>
        /// <param name="fileParser">The file parser</param>
        /// <param name="imageDictionary">The image dictionary</param>
        public void ParseFile(FileParser fileParser, Dictionary<FilePath, ImageReference> imageDictionary)
        {
            if(sourceFile != null && !String.IsNullOrEmpty(sourceFile.Path))
            {
                fileParser.ParseFile(sourceFile);

                if(fileParser.TopicId != Guid.Empty)
                    id = fileParser.TopicId;

                if(!String.IsNullOrEmpty(fileParser.RevisionNumber))
                    revisionNumber = fileParser.RevisionNumber;

                title = fileParser.Title;
                topicAbstract = fileParser.TopicAbstract;
                body = fileParser.Body;
                tocExclude = fileParser.TocExclude;
                defaultTopic = fileParser.IsDefaultTopic;
                splitToc = fileParser.SplitToc;
                sortOrder = fileParser.SortOrder;
                helpAttributes = fileParser.HelpAttributes;
                helpKeywords = fileParser.HelpKeywords;
            }

            subtopics.ParseFiles(fileParser, imageDictionary);
        }
        #endregion

        #region Write to XML
        //=====================================================================

        /// <summary>
        /// This is used to save the topic information to the project file
        /// </summary>
        /// <param name="xw">The XML text writer to which the information is written</param>
        internal void WriteXml(XmlWriter xw)
        {
            xw.WriteStartElement("Topic");

            xw.WriteAttributeString("id", id.ToString());
            xw.WriteAttributeString("visible", "True");

            if(String.IsNullOrEmpty(body))
                xw.WriteAttributeString("noFile", "True");

            if(title != null)
                xw.WriteAttributeString("title", title);

            if(helpAttributes != null && helpAttributes.Count != 0)
                helpAttributes.WriteXml(xw, true);

            if(helpKeywords != null && helpKeywords.Count != 0)
                helpKeywords.WriteXml(xw);

            if(subtopics.Count != 0)
                foreach(Topic t in subtopics)
                    t.WriteXml(xw);

            xw.WriteEndElement();
        }
        #endregion
    }
}
