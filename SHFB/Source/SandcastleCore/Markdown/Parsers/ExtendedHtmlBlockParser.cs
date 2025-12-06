//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ExtendedHtmlBlockParser.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/01/2025
// Note    : Based on Markdig.Parsers.HtmlBlockParser.  Copyright (c) Alexandre Mutel. All rights
//           reserved.  Licensed under the BSD-Clause 2 license.  https://github.com/xoofx/markdig
//
// This file contains a class used to parse HTML blocks
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/20/2025  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Sandcastle.Core.Markdown.Parsers;

/// <summary>
/// This class is used to parse HTML blocks with extended capabilities over the built-in Markdig parser
/// </summary>
/// <remarks>This parser allows definition of the block and special elements.  It is based largely on the
/// Markdig HTML block parser which could not be extended due to its internal design.</remarks>
public class ExtendedHtmlBlockParser : BlockParser
{
    #region Private data members and constants
    //=====================================================================

    private const string EndOfComment = "-->";
    private const string EndOfCDATA = "]]>";
    private const string EndOfProcessingInstruction = "?>";

    private readonly HashSet<string> htmlTags, doNotParseTags;
    private readonly List<string> doNoParseEndTags = [];

    #endregion

    #region Constructor
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="blockTags">An enumerable list of tags that should be treated as blocks</param>
    /// <param name="doNotParseTags">An enumerable list of tags that should not be parsed for Markdown
    /// content (code, scripts, pre, style, textarea, etc.).</param>
    public ExtendedHtmlBlockParser(IEnumerable<string> blockTags, IEnumerable<string> doNotParseTags)
    {
        if(blockTags == null)
            throw new ArgumentNullException(nameof(blockTags));

        this.OpeningCharacters = ['<'];

        this.htmlTags = new HashSet<string>(blockTags, StringComparer.OrdinalIgnoreCase);
        this.doNotParseTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        this.doNoParseEndTags.AddRange(doNotParseTags.Select(t => $"</{t}>"));
    }
    #endregion

    #region Methods
    //=====================================================================

    /// <inheritdoc />
    public override BlockState TryOpen(BlockProcessor processor)
    {
        var result = MatchStart(processor);

        // An end-tag can occur on the same line, so we try to parse it here
        if(result == BlockState.Continue)
            result = MatchEnd(processor, (HtmlBlock)processor.NewBlocks.Peek());

        return result;
    }

    /// <inheritdoc />
    public override BlockState TryContinue(BlockProcessor processor, Block block)
    {
        var htmlBlock = (HtmlBlock)block;

        return MatchEnd(processor, htmlBlock);
    }

    private BlockState MatchStart(BlockProcessor state)
    {
        if(state.IsCodeIndent)
            return BlockState.None;

        var line = state.Line;
        var startPosition = line.Start;

        line.SkipChar();

        var result = TryParseTagType16(state, line, state.ColumnBeforeIndent, startPosition);

        // HTML blocks of type 7 cannot interrupt a paragraph
        if(result == BlockState.None && state.CurrentBlock is not ParagraphBlock)
            result = TryParseTagType7(state, line, state.ColumnBeforeIndent, startPosition);

        return result;
    }

    private BlockState TryParseTagType7(BlockProcessor state, StringSlice line, int startColumn, int startPosition)
    {
        var builder = new StringBuilder();
        var c = line.CurrentChar;
        var result = BlockState.None;

        if((c == '/' && TryParseHtmlCloseTag(ref line, ref builder)) || TryParseHtmlTagOpenTag(ref line, ref builder))
        {
            // Must be followed by whitespace only
            bool hasOnlySpaces = true;
            c = line.CurrentChar;

            while(true)
            {
                if(c == '\0')
                    break;

                if(!c.IsWhitespace())
                {
                    hasOnlySpaces = false;
                    break;
                }

                c = line.NextChar();
            }

            if(hasOnlySpaces)
                result = CreateHtmlBlock(state, HtmlBlockType.NonInterruptingBlock, startColumn, startPosition);
        }

        return result;
    }

    private BlockState TryParseTagType16(BlockProcessor state, StringSlice line, int startColumn, int startPosition)
    {
        char c;
        c = line.CurrentChar;

        if(c == '!')
        {
            c = line.NextChar();

            if(c == '-' && line.PeekChar() == '-')
                return CreateHtmlBlock(state, HtmlBlockType.Comment, startColumn, startPosition); // group 2

            if(c.IsAlphaUpper())
                return CreateHtmlBlock(state, HtmlBlockType.DocumentType, startColumn, startPosition); // group 4

            if(c == '[' && line.Match("CDATA[", 1))
                return CreateHtmlBlock(state, HtmlBlockType.CData, startColumn, startPosition); // group 5

            return BlockState.None;
        }

        if(c == '?')
            return CreateHtmlBlock(state, HtmlBlockType.ProcessingInstruction, startColumn, startPosition); // group 3

        var hasLeadingClose = c == '/';

        if(hasLeadingClose)
            c = line.NextChar();

        // MAML tags can be longer than HTML tags so the maximum length is increased here
        Span<char> tag = stackalloc char[40];
        var count = 0;

        for(; count < tag.Length; count++)
        {
            if(!c.IsAlphaNumeric())
                break;

            // store lower invariant
            tag[count] = Char.ToLowerInvariant(c);
            c = line.NextChar();
        }

        if(!(c == '>' || (!hasLeadingClose && c == '/' && line.PeekChar() == '>') || c.IsWhiteSpaceOrZero()))
            return BlockState.None;

        if(count == 0)
            return BlockState.None;

        // build tag string
        var tagStr = new string(tag.Slice(0, count).ToArray());

        if(!htmlTags.Contains(tagStr))
            return BlockState.None;

        // Check for special group
        if(doNotParseTags.Contains(tagStr))
        {
            if(c == '/' || hasLeadingClose)
                return BlockState.None;

            // Check for a self-closing tag
            if(line.Length > 2)
            {
                int end = line.Length - 1;

                while(end > 2 && line.PeekChar(end).IsWhitespace())
                    end--;

                if(line.PeekChar(end - 1) == '/' && line.PeekChar(end) == '>')
                    return BlockState.None;
            }

            return CreateHtmlBlock(state, HtmlBlockType.ScriptPreOrStyle, startColumn, startPosition);
        }

        return CreateHtmlBlock(state, HtmlBlockType.InterruptingBlock, startColumn, startPosition);
    }

    private BlockState MatchEnd(BlockProcessor state, HtmlBlock htmlBlock)
    {
        state.GoToColumn(state.ColumnBeforeIndent);

        // Early exit if it is not starting by an HTML tag
        var line = state.Line;
        var result = BlockState.Continue;
        int index;

        switch(htmlBlock.Type)
        {
            case HtmlBlockType.Comment:
                index = line.IndexOf(EndOfComment);

                if(index >= 0)
                {
                    htmlBlock.UpdateSpanEnd(index + EndOfComment.Length);
                    result = BlockState.Break;
                }
                break;

            case HtmlBlockType.CData:
                index = line.IndexOf(EndOfCDATA);

                if(index >= 0)
                {
                    htmlBlock.UpdateSpanEnd(index + EndOfCDATA.Length);
                    result = BlockState.Break;
                }
                break;

            case HtmlBlockType.ProcessingInstruction:
                index = line.IndexOf(EndOfProcessingInstruction);

                if(index >= 0)
                {
                    htmlBlock.UpdateSpanEnd(index + EndOfProcessingInstruction.Length);
                    result = BlockState.Break;
                }
                break;

            case HtmlBlockType.DocumentType:
                index = line.IndexOf('>');

                if(index >= 0)
                {
                    htmlBlock.UpdateSpanEnd(index + 1);
                    result = BlockState.Break;
                }
                break;

            case HtmlBlockType.ScriptPreOrStyle:
                foreach(string endTag in doNoParseEndTags)
                {
                    index = line.IndexOf(endTag, 0, true);

                    if(index >= 0)
                    {
                        htmlBlock.UpdateSpanEnd(index + endTag.Length);
                        result = BlockState.Break;
                        break;
                    }
                }
                break;

            case HtmlBlockType.InterruptingBlock:
                if(state.IsBlankLine)
                    result = BlockState.BreakDiscard;
                break;

            case HtmlBlockType.NonInterruptingBlock:
                if(state.IsBlankLine)
                    result = BlockState.BreakDiscard;
                break;
        }

        // Update only if we don't have a break discard
        if(result != BlockState.BreakDiscard)
        {
            htmlBlock.Span.End = line.End;
            htmlBlock.NewLine = state.Line.NewLine;
        }

        return result;
    }

    private BlockState CreateHtmlBlock(BlockProcessor state, HtmlBlockType type, int startColumn, int startPosition)
    {
        var htmlBlock = new HtmlBlock(this)
        {
            Column = startColumn,
            Type = type,
            // By default, setup to the end of line
            Span = new SourceSpan(startPosition, startPosition + state.Line.End),
        };

        if(state.TrackTrivia)
        {
            htmlBlock.LinesBefore = state.LinesBefore;
            state.LinesBefore = null;
            htmlBlock.NewLine = state.Line.NewLine;
        }

        state.NewBlocks.Push(htmlBlock);

        return BlockState.Continue;
    }

    private static bool TryParseHtmlTagOpenTag(ref StringSlice text, ref StringBuilder builder)
    {
        var c = text.CurrentChar;

        // Parse the tag name
        if(!c.IsAlpha())
            return false;

        builder.Append(c);

        while(true)
        {
            c = text.NextChar();

            if(c.IsAlphaNumeric() || c == '-')
                builder.Append(c);
            else
                break;
        }

        bool hasAttribute = false;

        while(true)
        {
            var hasWhitespaces = false;

            // Skip any whitespaces
            while(c.IsWhitespace())
            {
                builder.Append(c);
                c = text.NextChar();
                hasWhitespaces = true;
            }

            switch(c)
            {
                case '\0':
                    return false;

                case '>':
                    text.SkipChar();
                    builder.Append(c);
                    return true;

                case '/':
                    builder.Append('/');
                    c = text.NextChar();

                    if(c != '>')
                        return false;

                    text.SkipChar();
                    builder.Append('>');
                    return true;

                case '=':
                    if(!hasAttribute)
                        return false;

                    builder.Append('=');

                    // Skip any spaces after
                    c = text.NextChar();

                    while(c.IsWhitespace())
                    {
                        builder.Append(c);
                        c = text.NextChar();
                    }

                    // Parse a quoted string
                    if(c == '\'' || c == '\"')
                    {
                        builder.Append(c);
                        char openingStringChar = c;

                        while(true)
                        {
                            c = text.NextChar();

                            if(c == '\0')
                                return false;

                            if(c != openingStringChar)
                                builder.Append(c);
                            else
                                break;
                        }

                        builder.Append(c);
                        c = text.NextChar();
                    }
                    else
                    {
                        // Parse until we match a space or a special HTML character
                        int matchCount = 0;

                        while(true)
                        {
                            if(c == '\0')
                                return false;

                            if(IsSpaceOrSpecialHtmlChar(c))
                                break;

                            matchCount++;
                            builder.Append(c);
                            c = text.NextChar();
                        }

                        [MethodImpl(MethodImplOptions.AggressiveInlining)]
                        static bool IsSpaceOrSpecialHtmlChar(char c)
                        {
                            if(c > '>')
                                return c == '`';

                            const long BitMask =
                                  (1L << ' ')
                                | (1L << '\n')
                                | (1L << '"')
                                | (1L << '\'')
                                | (1L << '=')
                                | (1L << '<')
                                | (1L << '>');

                            return (BitMask & (1L << c)) != 0;
                        }

                        // We need at least one char after '='
                        if(matchCount == 0)
                            return false;
                    }

                    hasAttribute = false;
                    continue;

                default:
                    if(!hasWhitespaces)
                        return false;

                    // Parse the attribute name
                    if(!(c.IsAlpha() || c == '_' || c == ':'))
                        return false;

                    builder.Append(c);

                    while(true)
                    {
                        c = text.NextChar();

                        if(c.IsAlphaNumeric() || IsCharToAppend(c))
                            builder.Append(c);
                        else
                            break;

                        [MethodImpl(MethodImplOptions.AggressiveInlining)]
                        static bool IsCharToAppend(char c)
                        {
                            if((uint)(c - '-') > '_' - '-')
                                return false;

                            const long BitMask =
                                  (1L << '_')
                                | (1L << ':')
                                | (1L << '.')
                                | (1L << '-');

                            return (BitMask & (1L << c)) != 0;
                        }
                    }

                    hasAttribute = true;
                    break;
            }
        }
    }

    private static bool TryParseHtmlCloseTag(ref StringSlice text, ref StringBuilder builder)
    {
        // </[A-Za-z][A-Za-z0-9]+\s*>
        builder.Append('/');

        var c = text.NextChar();

        if(!c.IsAlpha())
            return false;

        builder.Append(c);

        bool skipSpaces = false;

        while(true)
        {
            c = text.NextChar();

            if(c == '>')
            {
                text.SkipChar();
                builder.Append('>');
                return true;
            }

            if(skipSpaces)
            {
                if(c != ' ')
                {
                    break;
                }
            }
            else if(c == ' ')
            {
                skipSpaces = true;
            }
            else if(!(c.IsAlphaNumeric() || c == '-'))
            {
                break;
            }

            builder.Append(c);
        }

        return false;
    }
    #endregion
}
