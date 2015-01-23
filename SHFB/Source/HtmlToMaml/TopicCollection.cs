//=============================================================================
// System  : HTML to MAML Converter
// File    : TopicCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/17/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used to contain a collection of topics.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: https://GitHub.com/EWSoftware/SHFB.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  09/12/2008  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

using SandcastleBuilder.Utils;

namespace HtmlToMamlConversion
{
    /// <summary>
    /// This is a collection of topic items
    /// </summary>
    public class TopicCollection : Collection<Topic>
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get the default topic
        /// </summary>
        /// <value>It returns the default topic or null if one is not set</value>
        public Topic DefaultTopic
        {
            get
            {
                Topic defaultTopic = null;

                foreach(Topic t in this)
                {
                    if(t.IsDefaultTopic)
                        defaultTopic = t;
                    else
                        defaultTopic = t.Subtopics.DefaultTopic;

                    if(defaultTopic != null)
                        break;
                }

                return defaultTopic;
            }
        }

        /// <summary>
        /// This is used to get the topic at which the table of content is
        /// split by the API content.
        /// </summary>
        /// <value>This will only be valid if it refers to a root level
        /// topic.  It will return null if a split location has not been
        /// set at the root level.</value>
        public Topic SplitTocAtTopic
        {
            get
            {
                foreach(Topic t in this)
                    if(t.SplitToc)
                        return t;

                return null;
            }
        }
        #endregion

        #region Conversion helpers
        //=====================================================================

        /// <summary>
        /// Add all topics from the specified folder recursively to the
        /// topic collection.
        /// </summary>
        /// <param name="folder">The folder from which to get the files</param>
        /// <param name="topicDictionary">A dictionary used to contain the list
        /// of files index by name.</param>
        /// <remarks>Only files with a .htm? or .topic extension are added</remarks>
        public void AddTopicsFromFolder(string folder,
          Dictionary<FilePath, Topic> topicDictionary)
        {
            Topic topic, removeTopic;
            FilePath topicPath;
            string[] files = Directory.GetFiles(folder, "*.htm?");
            string name;

            // Add files
            foreach(string file in files)
            {
                topic = new Topic(file);
                this.Add(topic);
                topicDictionary.Add(topic.SourceFile, topic);
            }

            files = Directory.GetFiles(folder, "*.topic");

            foreach(string file in files)
            {
                topic = new Topic(file);
                this.Add(topic);
                topicPath = new FilePath(Path.ChangeExtension(
                  topic.SourceFile.Path, ".html"), topic.SourceFile.BasePathProvider);
                topicDictionary.Add(topicPath, topic);
            }

            // Add folders recursively
            files = Directory.GetDirectories(folder);

            foreach(string folderName in files)
            {
                topic = new Topic(null);
                topic.Title = name = Path.GetFileName(folderName);
                topic.Subtopics.AddTopicsFromFolder(folderName, topicDictionary);

                // Ignore empty folders
                if(topic.Subtopics.Count == 0)
                    continue;

                this.Add(topic);

                // Look for a file with the same name as the folder
                removeTopic = null;

                foreach(Topic t in topic.Subtopics)
                    if(t.SourceFile != null && Path.GetFileNameWithoutExtension(
                      t.SourceFile) == name)
                    {
                        // If found, remove it as it represents the container node
                        topic.Title = null;
                        topic.SourceFile = t.SourceFile;
                        topic.Id = t.Id;
                        removeTopic = t;
                        topicDictionary[topic.SourceFile] = topic;
                        break;
                    }

                if(removeTopic != null)
                    topic.Subtopics.Remove(removeTopic);
            }
        }

        /// <summary>
        /// Parse all files in the collection to extract the information for
        /// conversion.
        /// </summary>
        /// <param name="fileParser">The file parser</param>
        /// <param name="imageDictionary">The image dictionary</param>
        public void ParseFiles(FileParser fileParser,
          Dictionary<FilePath, ImageReference> imageDictionary)
        {
            foreach(Topic t in this)
                t.ParseFile(fileParser, imageDictionary);
        }
        #endregion

        #region Save to content layout file
        //=====================================================================

        /// <summary>
        /// Save the topic collection to the named content layout file
        /// </summary>
        /// <param name="filename">The filename to which the content layout
        /// is saved.</param>
        public void Save(string filename)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            XmlWriter writer = null;
            Topic defaultTopic = this.DefaultTopic, splitToc = this.SplitTocAtTopic;

            try
            {
                this.Sort();

                settings.Indent = true;
                settings.CloseOutput = true;
                writer = XmlWriter.Create(filename, settings);

                writer.WriteStartDocument();
                writer.WriteStartElement("Topics");

                if(defaultTopic != null)
                    writer.WriteAttributeString("defaultTopic",
                        defaultTopic.Id.ToString());

                if(splitToc != null)
                    writer.WriteAttributeString("splitTOCTopic",
                        splitToc.Id.ToString());

                foreach(Topic t in this)
                    t.WriteXml(writer);

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            finally
            {
                if(writer != null)
                    writer.Close();
            }
        }
        #endregion

        #region Sort collection
        //=====================================================================
        
        /// <summary>
        /// This is used to sort the collection
        /// </summary>
        /// <remarks>Values are sorted by display title.  Comparisons are
        /// case-sensitive.</remarks>
        public void Sort()
        {
            ((List<Topic>)base.Items).Sort(
                delegate(Topic x, Topic y)
                {
                    if(x.SortOrder < y.SortOrder)
                        return -1;

                    if(x.SortOrder > y.SortOrder)
                        return 1;

                    return String.Compare(x.Title, y.Title,
                        StringComparison.CurrentCultureIgnoreCase);
                });

            foreach(Topic t in this)
                t.Subtopics.Sort();
        }
        #endregion
    }
}
