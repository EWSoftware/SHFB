//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ExtensionsHelper.cs
// Author  : .NET Foundation
// Updated : 04/05/2026
// Note    : Copyright 2026, SHFB project, All rights reserved
//
// Extensions for parsing markdown strings.
//
// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you
// under the MIT license.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/05/2025  JMC  Created the code based on https://github.com/dotnet/docfx/commits/2a436495ed0716eebbc7eaad84d652f18600c2f6/
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;

using Markdig.Helpers;

namespace Sandcastle.Core.Markdown.Extensions;

/// <summary>
/// Helper classes for markdown extensions.
/// </summary>
public static class ExtensionsHelper
{
    /// <summary>
    /// Matches the start of the current string slice with a given string.
    /// </summary>
    /// <param name="slice">The markdown string slice.</param>
    /// <param name="startString">The start string to match.</param>
    /// <param name="isCaseSensitive">if set to <see langword="true"/> then perform a case insensitive search.</param>
    /// <returns><see langword="true"/> if there is a match, <see langword="false"/> otherwise.</returns>
    public static bool MatchStart(ref StringSlice slice, string startString, bool isCaseSensitive = true)
    {
        var c = slice.CurrentChar;
        var index = 0;

        while(c != '\0' && index < startString.Length && CharEqual(c, startString[index], isCaseSensitive))
        {
            c = slice.NextChar();
            index++;
        }

        return index == startString.Length;
    }

    /// <summary>
    /// Matches the inclusion end character.
    /// </summary>
    /// <param name="slice">The markdown string slice.</param>
    /// <returns><see langword="true"/> if there is a match, <see langword="false"/> otherwise.</returns>
    public static bool MatchInclusionEnd(ref StringSlice slice)
    {
        if(slice.CurrentChar != ']')
        {
            return false;
        }

        slice.NextChar();

        return true;
    }

    /// <summary>
    /// Matches a link template in the form of `[title](path)` and extracts the title and path. The title is optional,
    /// but if present must be in square brackets. The path is required and must be in parentheses. The path can
    /// optionally be enclosed in angle brackets. If the path is not enclosed in angle brackets, then it can contain a
    /// title after the path, which will be ignored. The method returns <see langword="true"/> if a link template is
    /// matched, <see langword="false"/> otherwise. If a match is found, the title and path are extracted and returned
    /// via the <paramref name="title"/> and <paramref name="path"/> parameters.
    /// </summary>
    /// <param name="slice">The markdown string slice.</param>
    /// <param name="title">The extracted title.</param>
    /// <param name="path">The extracted path.</param>
    /// <returns><see langword="true"/> if there is a match, <see langword="false"/> otherwise.</returns>
    public static bool MatchLink(ref StringSlice slice, ref string title, ref string path)
    {
        if(MatchTitle(ref slice, ref title) && MatchPath(ref slice, ref path))
        {
            return true;
        }

        return false;
    }

    private static bool MatchTitle(ref StringSlice slice, ref string title)
    {
        SkipSpace(ref slice);

        if(slice.CurrentChar != '[')
        {
            return false;
        }

        var c = slice.NextChar();
        var str = StringBuilderCache.Local();
        var hasEscape = false;

        while(c != '\0' && (c != ']' || hasEscape))
        {
            if(c == '\\' && !hasEscape)
            {
                hasEscape = true;
            }
            else
            {
                str.Append(c);
                hasEscape = false;
            }
            c = slice.NextChar();
        }

        if(c == ']')
        {
            title = str.ToString().Trim();
            slice.NextChar();

            return true;
        }

        return false;
    }

    private static bool MatchPath(ref StringSlice slice, ref string path)
    {
        if(slice.CurrentChar != '(')
        {
            return false;
        }

        slice.NextChar();
        SkipWhitespace(ref slice);

        string includedFilePath;
        if(slice.CurrentChar == '<')
        {
            includedFilePath = TryGetStringBeforeChars([')', '>'], ref slice, breakOnWhitespace: true);
        }
        else
        {
            includedFilePath = TryGetStringBeforeChars([')'], ref slice, breakOnWhitespace: true);
        }

        if(includedFilePath is null)
        {
            return false;
        }

        if(includedFilePath.Length >= 1 && includedFilePath.First() == '<' && slice.CurrentChar == '>')
        {
            includedFilePath = includedFilePath.Substring(1).Trim();
        }

        if(slice.CurrentChar == ')')
        {
            path = includedFilePath;
            slice.NextChar();
            return true;
        }
        else
        {
            var title = TryGetStringBeforeChars([')'], ref slice, breakOnWhitespace: false);
            if(title is null)
            {
                return false;
            }
            else
            {
                path = includedFilePath;
                slice.NextChar();
                return true;
            }
        }
    }

    private static void SkipSpace(ref StringSlice slice)
    {
        while(slice.CurrentChar == ' ')
        {
            slice.NextChar();
        }
    }

    private static void SkipWhitespace(ref StringSlice slice)
    {
        var c = slice.CurrentChar;
        while(c != '\0' && c.IsWhitespace())
        {
            c = slice.NextChar();
        }
    }
    private static bool CharEqual(char ch1, char ch2, bool isCaseSensitive)
    {
        return isCaseSensitive ? ch1 == ch2 : Char.ToLower(ch1) == Char.ToLower(ch2);
    }

    private static string TryGetStringBeforeChars(IReadOnlyList<char> chars, ref StringSlice slice, bool breakOnWhitespace = false)
    {
        StringSlice savedSlice = slice;
        var c = slice.CurrentChar;
        var hasEscape = false;
        var builder = StringBuilderCache.Local();

        while(c != '\0' && (!breakOnWhitespace || !c.IsWhitespace()) && (hasEscape || !chars.Contains(c)))
        {
            if(c == '\\' && !hasEscape)
            {
                hasEscape = true;
            }
            else
            {
                builder.Append(c);
                hasEscape = false;
            }
            c = slice.NextChar();
        }

        if(c == '\0' && !chars.Contains('\0'))
        {
            slice = savedSlice;
            builder.Length = 0;
            return null;
        }
        else
        {
            var result = builder.ToString().Trim();
            builder.Length = 0;
            return result;
        }
    }
}
