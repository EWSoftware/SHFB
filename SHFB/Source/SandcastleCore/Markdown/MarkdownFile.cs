//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : MarkdownFile.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/26/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains a class used to parse metadata from a Markdown file
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

namespace Sandcastle.Core.Markdown;

/// <summary>
/// This class is used to parse metadata from a Markdown file
/// </summary>
public class MarkdownFile
{
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

    #endregion

    #region Constructor
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="filename">The full path to the Markdown file from which to extract metadata</param>
    public MarkdownFile(string filename)
    {
        this.Filename = filename;
        this.ParseMetadata();
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
            var lines = File.ReadAllLines(this.Filename);
            int lineIndex = 1, endIndex = lines.Length;

            if(lines[0] == "---")
            {
                while(lineIndex < lines.Length)
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
                lineIndex = (endIndex == lines.Length) ? 0 : endIndex + 1;

                while(lineIndex < lines.Length && lines[lineIndex].Length > 1 && lines[lineIndex][0] != '#')
                    lineIndex++;

                if(lineIndex < lines.Length)
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
    #endregion
}
