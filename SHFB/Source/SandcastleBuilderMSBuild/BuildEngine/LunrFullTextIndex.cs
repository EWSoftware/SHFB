//===============================================================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : LunrFullTextIndex.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/27/2025
// Note    : Copyright 2007-2025, Eric Woodruff, All rights reserved
//
// This file contains a class used to create a full-text index used to search for topics using Lunr.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/26/2025  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: ko ru zh

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using Sandcastle.Core;

using SandcastleBuilder.MSBuild.Lunr;
using SandcastleBuilder.MSBuild.Lunr.Languages;

namespace SandcastleBuilder.MSBuild.BuildEngine;

/// <summary>
/// This class is used to generate the full-text index used to search for topics using Lunr
/// </summary>
public class LunrFullTextIndex : IFullTextIndex
{
    #region File index info
    //=====================================================================

    /// <summary>
    /// This is used to serialize the file index information used to provide the filenames, titles, and
    /// a brief preview of the content for the search results.
    /// </summary>
    private sealed class FileIndex
    {
        /// <summary>
        /// The index ID
        /// </summary>
        [JsonIgnore]
        public int Id { get; }

        /// <summary>
        /// The topic filename (filename only, no path or extension)
        /// </summary>
        [JsonPropertyName("f")]
        public string Filename { get; }

        /// <summary>
        /// The topic title
        /// </summary>
        [JsonPropertyName("t")]
        public string Title { get; }

        /// <summary>
        /// Content (200 characters max)
        /// </summary>
        [JsonPropertyName("c")]
        public string Content { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The file ID</param>
        /// <param name="filename">The filename</param>
        /// <param name="title">The title</param>
        /// <param name="content">The content</param>
        public FileIndex(int id, string filename, string title, string content)
        {
            this.Id = id;
            this.Filename = filename;
            this.Title = WebUtility.HtmlEncode(title);

            if(content.StartsWith(title, StringComparison.OrdinalIgnoreCase))
                content = content.Substring(title.Length).Trim();

            if(!String.IsNullOrWhiteSpace(content) && content.Length > 200)
            {
                int idx = 200;

                // Truncate at the nearest word boundary
                if(Char.IsWhiteSpace(content[idx]))
                {
                    while(idx > 0 && Char.IsWhiteSpace(content[idx]))
                        idx--;

                    idx++;
                }
                else
                {
                    while(idx < content.Length && !Char.IsWhiteSpace(content[idx]))
                        idx++;
                }

                content = content.Substring(0, idx);
            }

            this.Content = WebUtility.HtmlEncode(content);
        }
    }
    #endregion

    #region Private class members
    //=====================================================================

    private static readonly JsonSerializerOptions options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    private static readonly Dictionary<string, Action<Builder>> otherLanguages = new()
    {
        ["de"] = (b) => German.Register(b),
        ["es"] = (b) => Spanish.Register(b),
        ["fr"] = (b) => French.Register(b),
        ["it"] = (b) => Italian.Register(b),
        ["ja"] = (b) => Japanese.Register(b),
        ["ko"] = (b) => Korean.Register(b),
        ["pt"] = (b) => Portuguese.Register(b),
        ["ru"] = (b) => Russian.Register(b),
        ["zh"] = (b) => Chinese.Register(b),
    };

    private static readonly Regex rePageTitle = new(@"<title>(?<Title>.*)</title>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline);

    private static readonly Regex reStripScriptStyleHead = new(
        @"<script[^>]*(?<!/)>.*?</script\s*>|<style[^>]*(?<!/)>.*?</style\s*>|<head[^>]*(?<!/)>.*?</head\s*>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline);

    private static readonly Regex reStripTags = new("<[^>]+>");
    private static readonly Regex reCondenseWS = new(@"\s+");

    private static readonly Regex reTopicContent = new(@"<div id=""TopicContent"".*?(div|footer) " +
        @"id=""(InThisArticleColumn|PageFooter).*?>", RegexOptions.Singleline);

    private readonly CultureInfo lang;
    private readonly ConcurrentBag<Dictionary<string, object>> documents;

    #endregion

    #region Constructor
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="language">The culture information</param>
    public LunrFullTextIndex(CultureInfo language)
    {
        lang = language;
        documents = [];
    }
    #endregion

    #region Methods
    //=====================================================================

    /// <summary>
    /// Create a full-text index from web pages found in the specified file path
    /// </summary>
    /// <param name="filePath">The path containing the files to index</param>
    /// <remarks>Words in the exclusion list and those that are less than two characters long will not appear
    /// in the index.</remarks>
    public void CreateFullTextIndex(string filePath)
    {
        int rootPathLength, id = 0;

        if(filePath == null)
            throw new ArgumentNullException(nameof(filePath));

        if(filePath[filePath.Length - 1] == Path.DirectorySeparatorChar)
            rootPathLength = filePath.Length;
        else
            rootPathLength = filePath.Length + 1;

        foreach(string name in Directory.EnumerateFiles(filePath, "*.htm?", SearchOption.AllDirectories))
        {
            Encoding enc = Encoding.Default;
            string title, filename = Path.GetFileNameWithoutExtension(name),
                content = ComponentUtilities.ReadWithEncoding(name, ref enc);

            // Extract the page title
            Match m = rePageTitle.Match(content);

            if(!m.Success)
                title = filename;
            else
                title = m.Groups["Title"].Value.Trim();

            // Limit the indexed text to the page content if possible.  This avoids indexing things in the
            // page header and footer that probably don't need to be in the index such as copyright text
            // that appears on every page.
            var contentMatch = reTopicContent.Match(content);

            if(contentMatch.Success)
                content = contentMatch.Value;
#if DEBUG
            else
            {
                // If it stops here, the regex needs updating or we should probably let the presentation
                // style tell us how to find its content div.  For now, the layout is common.
                Debugger.Break();
            }
#endif
            // Put some space between tags
            content = content.Replace("><", "> <");

            // Remove script, style sheet, and head blocks as they won't contain any usable keywords.  Pre
            // tags contain code which may or may not be useful but we'll leave them alone for now.
            content = reStripScriptStyleHead.Replace(content, " ");

            content = reStripTags.Replace(content, " ");
            content = reCondenseWS.Replace(content, " ");
            content = WebUtility.HtmlDecode(content);

            // Single character IDs are used to keep the serialized index as small as possible
            documents.Add(new Dictionary<string, object>
            {
                ["i"] = id++,
                ["f"] = filename,
                ["t"] = title,
                ["c"] = content.Trim()
            });
        };
    }

    /// <summary>
    /// Save the index information to the specified location.
    /// </summary>
    /// <param name="indexPath">The path to which the index files are saved.</param>
    public void SaveIndex(string indexPath)
    {
        var builder = new Builder();

        builder.Pipeline.Add(Trimmer.TrimmerFunction);
        builder.Pipeline.Add(StopWordFilter.StopWordFilterFunction);
        builder.Pipeline.Add(Stemmer.StemmerFunction);
        builder.SearchPipeline.Add(Stemmer.StemmerFunction);

        if(otherLanguages.TryGetValue(lang.TwoLetterISOLanguageName, out var registerLanguage))
            registerLanguage(builder);

        // An integer value is used for the ID to keep the index smaller.  We only need to index the content
        // as the title appears within it.
        builder.Ref("i");
        builder.Field("c");

        foreach(var d in documents)
            builder.Add(d);

        var index = builder.Build();

        File.WriteAllText(Path.Combine(indexPath, "searchIndex.json"), index.Serialize(), Encoding.UTF8);

        var fileIndex = documents.Select(d => new FileIndex((int)d["i"]!, (string)d["f"]!, (string)d["t"]!,
            (string)d["c"]!)).OrderBy(f => f.Id).ToList();

        File.WriteAllText(Path.Combine(indexPath, "fileIndex.json"), JsonSerializer.Serialize(fileIndex, options),
            Encoding.UTF8);
    }
    #endregion
}
