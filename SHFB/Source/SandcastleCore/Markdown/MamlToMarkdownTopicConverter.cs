//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : MamlToMarkdownTopicConverter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/05/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains a class used to convert MAML topic files and their content layout file entries to
// Markdown topics.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/09/2021  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

using Sandcastle.Core.ConceptualContent;
using Sandcastle.Core.PresentationStyle.Conversion;
using Sandcastle.Core.PresentationStyle.Transformation;
using Sandcastle.Core.PresentationStyle.Transformation.Elements;

namespace Sandcastle.Core.Markdown;

/// <summary>
/// This class is used to convert MAML topic files and their content layout file entries to Markdown topics
/// </summary>
public class MamlToMarkdownTopicConverter
{
    #region Private data members
    //=====================================================================

    private readonly MarkdownConversionPresentationStyle presentationStyle = new();
    private readonly Dictionary<string, ResourceItem> resourceItems = [];

    private readonly Regex reCodeInline = new("`.*?(&.{2,20};).*?`", RegexOptions.None);
    private readonly Regex reFencedCode = new("```[^`]*?(&.{2,20};)[^`]*?```", RegexOptions.Singleline);

    #endregion

    #region Properties
    //=====================================================================

    /// <summary>
    /// This is used to get or set whether or not to use the filenames for the topic unique IDs rather than the
    /// existing topic IDs which are usually GUIDs.
    /// </summary>
    /// <remarks>If true, the default, the filenames will be used and the existing topic IDs will be stored as an
    /// alternate topic ID in the converted files.  If false, the existing topics IDs will be retained and the
    /// filename will be stored as the alternate topic ID.  The filenames will have the extension removed and
    /// any spaces will be converted to dashes.</remarks>
    public bool UseFilenamesForUniqueIds { get; set; } = true;

    #endregion

    #region Constructor
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="languageName">The language name used to load shared resource item files.  If not specified,
    /// en-US is assumed.</param>
    /// <param name="resourceItemFiles">The list of project resource item files to load, if any</param>
    public MamlToMarkdownTopicConverter(string languageName, IEnumerable<string> resourceItemFiles)
    {
        // Load the stock and project resource item files used to translate include items to literal text
        foreach(var sharedFile in presentationStyle.ResourceItemFiles(languageName ?? "en-US"))
            LoadItemFile(sharedFile);

        if(resourceItemFiles != null)
        {
            foreach(var projectFile in resourceItemFiles)
                LoadItemFile(projectFile);
        }
    }
    #endregion

    #region Methods
    //=====================================================================

    /// <summary>
    /// This is used to load a resource item file's content
    /// </summary>
    /// <param name="filename">The file to load</param>
    private void LoadItemFile(string filename)
    {
        ResourceItem r;
        XmlReaderSettings settings = new() { CloseInput = true };

        try
        {
            using var xr = XmlReader.Create(filename, settings);
            xr.MoveToContent();

            while(!xr.EOF)
            {
                if(xr.NodeType == XmlNodeType.Element && xr.Name == "item")
                {
                    r = new ResourceItem(filename, xr.GetAttribute("id"), xr.ReadInnerXml(), false);

                    resourceItems[r.Id] = r;
                }

                xr.Read();
            }
        }
        catch(Exception ex)
        {
            // Ignore exceptions.  We'll just have missing resource items.  This is unlikely to happen for
            // the core resource item files.
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    /// <summary>
    /// Convert the topic to Markdown format
    /// </summary>
    /// <param name="topic">The topic to process</param>
    /// <returns>True if successful, false if not</returns>
    public bool ConvertTopic(Topic topic)
    {
        // Ignore empty container nodes and non-MAML files
        if(topic.TopicFile == null || !Path.GetExtension(topic.TopicFile.FullPath).Equals(".aml",
          StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var topicContent = XDocument.Load(topic.TopicFile.FullPath).Root!;

        string topicPath = Path.GetDirectoryName(topic.TopicFile.FullPath);
        string id = topic.Id,
            alternateId = MarkdownFile.GenerateIdFromTitle(Path.GetFileNameWithoutExtension(topic.TopicFile.FullPath));

        if(this.UseFilenamesForUniqueIds)
            (id, alternateId) = (alternateId, id);

        var metadata = new XElement("metadata",
            new XElement("id", id),
            new XElement("alternateId", alternateId),
            new XElement("title", topic.Title ?? Path.GetFileNameWithoutExtension(topic.TopicFile.Name)));

        if(topic.LinkText != null)
            metadata.Add(new XElement("linkText", topic.LinkText));

        if(topic.TocTitle != null)
            metadata.Add(new XElement("tocTitle", topic.TocTitle));

        foreach(var kw in topic.Keywords)
            metadata.Add(new XElement("keyword", new XAttribute("index", "K"), kw.Term));

        var document = new XDocument(new XElement("document", new XAttribute("type", "MAML"), metadata));

        document.Root!.Add(topicContent);

        // We leverage the presentation style transformation to do most of the work for us.  It's not a
        // perfect conversion but gets us close enough.  The topic will need to be reviewed anyway.
        var markdownTopic = presentationStyle.TopicTransformation.Render(id, document);

        // Replace include items with their content from the shared resource files
        foreach(var includeItem in markdownTopic.Descendants("include").ToArray())
        {
            var itemId = includeItem.Attribute("item")?.Value;

            if(itemId != null && resourceItems.TryGetValue(itemId, out var resourceItem))
                includeItem.ReplaceWith(new XText(resourceItem.Value));
        }

        // Save the transformed topic without the containing document element as text
        var sb = new StringBuilder(10240);

        using(var xw = XmlWriter.Create(sb, new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            Indent = false,
            ConformanceLevel = ConformanceLevel.Fragment
        }))
        {
            foreach(var node in markdownTopic.Element("document")!.Nodes())
                node.WriteTo(xw);

            xw.Flush();
        }

        // Replace encoded characters with their literal equivalents
        sb.Replace("&amp;#60;", "<").Replace("&amp;#62;", ">");

        // Decode entities inline and fenced code blocks
        string convertedContent = sb.ToString();

        convertedContent = reCodeInline.Replace(convertedContent, m => WebUtility.HtmlDecode(m.Value));
        convertedContent = reFencedCode.Replace(convertedContent, m => WebUtility.HtmlDecode(m.Value));

        File.WriteAllText(Path.Combine(topicPath,
            Path.GetFileNameWithoutExtension(topic.TopicFile.FullPath) + ".md"), convertedContent);

        return true;
    }
    #endregion
}
