//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : MarkdownFile.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/02/2026
// Note    : Copyright 2025-2026, Eric Woodruff, All rights reserved
//
// This file contains a class used to parse metadata from a Markdown file and resolve any includes in the
// content recursively.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/26/2025  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Sandcastle.Core.Markdown;

/// <summary>
/// This class is used to parse metadata from a Markdown file and resolve any includes in the content
/// recursively.
/// </summary>
public class MarkdownFile
{
    #region Private data members
    //=====================================================================

    private static readonly Regex reInclude = new(@"\\?\[!INCLUDE\s+(\[.*?\])?\((?<FilePath>.*?)\)\]",
        RegexOptions.IgnoreCase);

    #endregion

    #region Properties
    //=====================================================================

    /// <summary>
    /// This read-only property is used to get the filename
    /// </summary>
    public string Filename { get; }

    /// <summary>
    /// This is used to get or set the unique ID
    /// </summary>
    public string UniqueId { get; set; }

    /// <summary>
    /// This is used to get or set the alternate ID
    /// </summary>
    public string AlternateId { get; set; }

    /// <summary>
    /// This is used to get or set the title
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// This is used to get or set the table of contents title
    /// </summary>
    public string TocTitle { get; set; }

    /// <summary>
    /// This is used to get or set the link text
    /// </summary>
    public string LinkText { get; set; }

    /// <summary>
    /// This is used to get or set the summary text
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// This is used to get a list of the topic keywords
    /// </summary>
    public List<string> Keywords { get; set; } = [];

    /// <summary>
    /// This read-only property is used to get the file content with any includes resolved.
    /// </summary>
    public string Content
    {
        get
        {
            if(String.IsNullOrWhiteSpace(field))
                this.LoadContent(true);

            return field;
        }
        private set;
    }

    /// <summary>
    /// This is read-only property returns true if the markdown file has any missing includes
    /// </summary>
    public bool HasMissingIncludes { get; private set; }

    /// <summary>
    /// This is read-only property returns true if the markdown file has any circular reference includes
    /// </summary>
    public bool HasCircularReferenceIncludes { get; private set; }

    #endregion

    #region Constructors
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="filename">The full path to the Markdown file from which to extract metadata</param>
    /// <overloads>There are two overloads for the constructor</overloads>
    public MarkdownFile(string filename)
    {
        this.Filename = filename;
        this.ParseMetadata();

        // We don't keep the content since this is typically only called to edit the content layout file which
        // doesn't need to keep the content around after it has been parsed.
        this.Content = null;
    }

    /// <summary>
    /// This constructor is used for the preview tool window
    /// </summary>
    /// <param name="filename">The full path to the Markdown file from which to extract metadata</param>
    /// <param name="content">The content to use instead of the file content</param>
    /// <remarks>This allows previewing content from an open editor window while still allowing for resolution
    /// of include directives.</remarks>
    public MarkdownFile(string filename, string content)
    {
        this.Filename = filename;
        this.Content = content;
        this.LoadContent(false);
    }
    #endregion

    #region Helper methods
    //=====================================================================

    /// <summary>
    /// This is used to generate a content ID from a title
    /// </summary>
    /// <param name="title">The title to convert to an ID</param>
    /// <returns>An ID generated from the title.  All non-alphanumeric characters are replaced with dashes.
    /// Runs of multiple dashes are replaced with a single dash.  Leading and trailing dashes are removed.</returns>
    public static string GenerateIdFromTitle(string title)
    {
        if(String.IsNullOrWhiteSpace(title))
            return null;

        StringBuilder sb = new(256);

        foreach(char c in title)
        {
            if(Char.IsLetterOrDigit(c))
                sb.Append(c);
            else
            {
                if(sb.Length != 0 && sb[sb.Length - 1] != '-')
                    sb.Append('-');
            }
        }

        if(sb[sb.Length - 1] == '-')
            sb.Length--;

        return sb.ToString();
    }

    /// <summary>
    /// Parse the metadata from the YAML Front Matter block in the file if it exists
    /// </summary>
    private void ParseMetadata()
    {
        if(File.Exists(this.Filename))
        {
            this.LoadContent(true);

            using var sr = new StringReader(this.Content);
            var lines = new List<string>();

            while(sr.Peek() != -1)
                lines.Add(sr.ReadLine());

            int lineIndex = 1, endIndex = lines.Count;

            if(lines[0] == "---")
            {
                while(lineIndex < lines.Count)
                {
                    if(lines[lineIndex] == "---")
                    {
                        endIndex = lineIndex;
                        lineIndex = 1;
                        break;
                    }

                    lineIndex++;
                }

                while(lineIndex < endIndex)
                {
                    string line = lines[lineIndex].Trim();

                    if(line == "---")
                        break;

                    int colonIndex = line.IndexOf(':');

                    if(colonIndex != -1)
                    {
                        string key = line.Substring(0, colonIndex).Trim().ToLowerInvariant(),
                            value = line.Substring(colonIndex + 1).Trim().Trim('"');

                        switch(key)
                        {
                            case "uid":
                                this.UniqueId = value;
                                break;

                            case "alt-uid":
                                this.AlternateId = value;
                                break;

                            case "title":
                                this.Title = value;
                                break;

                            case "toctitle":
                                this.TocTitle = value;
                                break;

                            case "linktext":
                                this.LinkText = value;
                                break;

                            case "summary":
                                this.Summary = value;
                                break;

                            case "keywords":
                                foreach(var keyword in ParseCommaDelimitedList(value))
                                    this.Keywords.Add(keyword);
                                break;

                            default:
                                break;
                        }
                    }

                    lineIndex++;
                }
            }

            if(String.IsNullOrWhiteSpace(this.Title))
            {
                lineIndex = (endIndex == lines.Count) ? 0 : endIndex + 1;

                while(lineIndex < lines.Count && lines[lineIndex].Length > 1 && lines[lineIndex][0] != '#')
                    lineIndex++;

                if(lineIndex < lines.Count)
                {
                    endIndex = 1;

                    while(endIndex < lines[lineIndex].Length)
                    {
                        if(lines[lineIndex][endIndex] != '#')
                        {
                            this.Title = lines[lineIndex].Substring(endIndex).Trim();
                            break;
                        }

                        endIndex++;
                    }
                }
            }
        }

        // If no title was found, use the other fields or filename
        if(String.IsNullOrWhiteSpace(this.Title))
        {
            if(!String.IsNullOrWhiteSpace(this.TocTitle))
                this.Title = this.TocTitle;
            else
            {
                if(!String.IsNullOrWhiteSpace(this.LinkText))
                    this.Title = this.LinkText;
                else
                    this.Title = Path.GetFileNameWithoutExtension(this.Filename);
            }
        }

        // If no unique ID was found, use the other fields or filename to generate one
        if(String.IsNullOrWhiteSpace(this.UniqueId))
        {
            if(!String.IsNullOrWhiteSpace(this.Title))
                this.UniqueId = GenerateIdFromTitle(this.Title);
            else
            {
                if(!String.IsNullOrWhiteSpace(this.TocTitle))
                    this.UniqueId = GenerateIdFromTitle(this.TocTitle);
                else
                {
                    if(!String.IsNullOrWhiteSpace(this.LinkText))
                        this.UniqueId = GenerateIdFromTitle(this.LinkText);
                    else
                        this.UniqueId = GenerateIdFromTitle(Path.GetFileNameWithoutExtension(this.Filename));
                }
            }
        }
    }

    /// <summary>
    /// Parse a comma-delimited list, handling quoted values with commas
    /// </summary>
    /// <param name="value">The comma-delimited string</param>
    /// <returns>A list of parsed values</returns>
    private static IEnumerable<string> ParseCommaDelimitedList(string value)
    {
        if(!String.IsNullOrWhiteSpace(value))
        {
            StringBuilder sb = new(value.Length);
            int i = 0;
            bool inQuotes = false;

            while(i < value.Length)
            {
                char c = value[i];

                if(c == '"')
                    inQuotes = !inQuotes;
                else
                {
                    if(c == ',' && !inQuotes)
                    {
                        var item = sb.ToString().Trim();

                        if(item.Length > 0)
                            yield return item;

                        sb.Clear();
                    }
                    else
                        sb.Append(c);
                }

                i++;
            }

            var lastItem = sb.ToString().Trim();
            
            if(lastItem.Length > 0)
                yield return lastItem;

        }
    }

    /// <summary>
    /// Load the file content and resolve any includes recursively
    /// </summary>
    /// <param name="fromFile">True to load the content from the file or false to use the assigned content</param>
    private void LoadContent(bool fromFile)
    {
        if(fromFile && !String.IsNullOrWhiteSpace(this.Filename))
            this.Content = File.ReadAllText(this.Filename);

        HashSet<string> includeChain = new(StringComparer.OrdinalIgnoreCase) { this.Filename };

        this.Content = this.ResolveIncludes(this.Content, Path.GetDirectoryName(this.Filename)!, includeChain);
    }

    /// <summary>
    /// Recursively resolve includes in the given content, skipping missing files and any files already in the
    /// include chain to prevent circular references.
    /// </summary>
    /// <param name="content">The content in which to resolve includes</param>
    /// <param name="basePath">The directory of the file that owns this content.  Any relative paths are
    /// assumed to be relative to this base path.</param>
    /// <param name="includeChain">The set of files that have already been seen.  This is used to detect circular
    /// references.</param>
    /// <returns>The content with all includes resolved</returns>
    private string ResolveIncludes(string content, string basePath, HashSet<string> includeChain)
    {
        if(String.IsNullOrWhiteSpace(content))
            return String.Empty;

        // A match evaluator is used so that each include directive can be processed and replaced without
        // affecting any others that may be present in the content (i.e. includes and escaped includes that
        // reference the same file).
        return reInclude.Replace(content, match =>
        {
            // Ignore escaped include directives and let them pass through as a literal
            if(match.Value.StartsWith("\\", StringComparison.Ordinal))
                return match.Value.Substring(1);

            string includeFile = match.Groups["FilePath"].Value.CorrectFilePathSeparators();

            if(!Path.IsPathRooted(includeFile))
                includeFile = Path.GetFullPath(Path.Combine(basePath, includeFile));

            if(!File.Exists(includeFile))
            {
                this.HasMissingIncludes = true;
                return $"**!! Missing include file: {includeFile}**";
            }

            if(includeChain.Contains(includeFile))
            {
                this.HasCircularReferenceIncludes = true;
                return $"**!! Circular reference detected for file: {includeFile}\r\n" +
                    $"File chain:{String.Join("\r\n  -> ", includeChain)}**";
            }

            string includeContent = File.ReadAllText(includeFile);

            includeChain.Add(includeFile);
            includeContent = ResolveIncludes(includeContent, Path.GetDirectoryName(includeFile)!, includeChain);
            includeChain.Remove(includeFile);

            return includeContent;
        });
    }
    #endregion
}
