/*!
 * lunr - http://lunrjs.com - A bit like Solr, but much smaller and not as bright - 2.3.9
 * Copyright (C) 2020 Oliver Nightingale
 * @license MIT
 *
 * Original JavaScript source and project website: https://github.com/olivernn/lunr.js
 *
 * EFW - 12/24/2025 - Ported to C# using Copilot.  This is a minimal port that only includes the code necessary
 * to build the index so that it can use the pre-built version client-side rather than building it there which
 * would likely be much slower.  Querying and other features have not been ported.  Equivalent language support
 * was also ported.
 */

#pragma warning disable CA1308

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SandcastleBuilder.MSBuild.Lunr;

/// <summary>
/// Tokenizer for splitting strings into tokens for indexing.
/// </summary>
public static class Tokenizer
{
    /// <summary>
    /// The separator pattern used to split a string into tokens.
    /// 
    /// EFW - Unlike the JavaScript version, this one splits text on all non-word characters which yields better
    /// results as you won't end up with index entries containing multiple words separated by non-word characters
    /// like commas, colons, and semi-colons when there is no whitespace after them.
    /// </summary>
    public static Regex Separator { get; set; } = new Regex(@"\W");

    /// <summary>
    /// The default tokenizer function for splitting strings into tokens.
    /// </summary>
    /// <param name="obj">The object to convert into tokens.</param>
    /// <param name="metadata">Optional metadata to associate with every token.</param>
    /// <returns>An array of tokens.</returns>
    public static Token[] DefaultTokenizer(object obj, Dictionary<string, object> metadata = null)
    {
        if(obj == null)
        {
            return [];
        }

        if(obj is Array arr)
        {
            // EFW - Inlined Utils.AsString() since it's only used here
            return [.. arr.Cast<object>().Select(
                t => new Token((t?.ToString() ?? String.Empty).ToLowerInvariant(), Clone(metadata)))];
        }

        string str = obj.ToString()?.ToLowerInvariant() ?? "";
        int len = str.Length;
        var tokens = new List<Token>();

        int sliceStart = 0;
        for(int sliceEnd = 0; sliceEnd <= len; sliceEnd++)
        {
            char c = sliceEnd < len ? str[sliceEnd] : ' '; // Use space as end marker
            int sliceLength = sliceEnd - sliceStart;

            if(Separator.IsMatch(c.ToString()) || sliceEnd == len)
            {
                if(sliceLength > 0)
                {
                    var tokenMetadata = Clone(metadata) ?? [];
                    tokenMetadata["position"] = new[] { sliceStart, sliceLength };
                    tokenMetadata["index"] = tokens.Count;

                    tokens.Add(new Token(
                        str.Substring(sliceStart, sliceLength),
                        tokenMetadata
                    ));
                }

                sliceStart = sliceEnd + 1;
            }
        }

        return [.. tokens];
    }

    // EFW - Moved from Utils since it's only used here
    /// <summary>
    /// Clones a dictionary with primitive values.
    /// Only shallow cloning is supported.
    /// </summary>
    /// <param name="obj">The dictionary to clone.</param>
    /// <returns>A clone of the passed dictionary.</returns>
    public static Dictionary<string, object> Clone(Dictionary<string, object> obj)
    {
        if(obj == null)
        {
            return null;
        }

        var clone = new Dictionary<string, object>();

        foreach(var kvp in obj)
        {
            if(kvp.Value is Array arr)
            {
                clone[kvp.Key] = ((ICloneable)arr).Clone();
            }
            else if(kvp.Value is string || kvp.Value is int || kvp.Value is long ||
                     kvp.Value is double || kvp.Value is float || kvp.Value is bool ||
                     kvp.Value is decimal || kvp.Value == null)
            {
                clone[kvp.Key] = kvp.Value;
            }
            else if(kvp.Value is ICloneable cloneable)
            {
                clone[kvp.Key] = cloneable.Clone();
            }
            else
            {
                throw new InvalidOperationException("Clone is not deep and does not support nested objects");
            }
        }

        return clone;
    }
}
