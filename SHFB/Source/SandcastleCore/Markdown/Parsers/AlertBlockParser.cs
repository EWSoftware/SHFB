//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : AlertBlockParser.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/28/2025
// Note    : Based on Markdig.Parsers.QuoteBlockParser.  Copyright (c) Alexandre Mutel. All rights
//           reserved.  Licensed under the BSD-Clause 2 license.  https://github.com/xoofx/markdig
//
// This file contains a block parser used to detect alert blocks in Markdown
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/20/2025  EFW  Created the code
//===============================================================================================================

using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Sandcastle.Core.Markdown.Parsers;

/// <summary>
/// This class is a block parser that detects quote lines that contain only an alert marker (e.g. "[!NOTE]")
/// and creates an <see cref="AlertBlock"/> instead of a plain <c>QuoteBlock</c>.
/// </summary>
/// <remarks>This version of the parser supports an optional title following the alert type</remarks>
public class AlertBlockParser : BlockParser
{
    #region Constructor
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    public AlertBlockParser()
    {
        OpeningCharacters = ['>'];
    }
    #endregion

    #region Methods
    //=====================================================================

    /// <inheritdoc />
    public override BlockState TryOpen(BlockProcessor processor)
    {
        if(processor.IsCodeIndent)
            return BlockState.None;

        // Look ahead but don't change the position until we know we have an alert block
        int offset = 1;

        while(offset < 6 && (processor.PeekChar(offset) == ' ' || processor.PeekChar(offset) == '\t'))
            offset++;

        if(offset > 5 || processor.PeekChar(offset) != '[' || processor.PeekChar(offset + 1) != '!')
            return BlockState.None;

        var sourcePosition = processor.Start;
        var quoteChar = processor.CurrentChar;
        var column = processor.Column;
        bool hasSpaceAfterQuoteChar = false;

        var c = processor.NextChar();

        if(c == ' ')
        {
            processor.NextColumn();
            hasSpaceAfterQuoteChar = true;
            processor.SkipFirstUnwindSpace = true;
        }
        else
        {
            if(c == '\t')
                processor.NextColumn();
        }

        // Grab the remaining text of the line (from current position)
        var type = processor.Line.ToString();

        if(type.Length == 0 || type[type.Length - 1] != ']')
            return BlockState.None;

        while(processor.CurrentChar != '\x0')
            processor.NextChar();

        // Build the alert block
        var alert = new AlertBlock(this, new StringSlice(type.Substring(2, type.Length - 3)))
        {
            QuoteChar = quoteChar,
            Span = new SourceSpan(sourcePosition, processor.Line.End),
            Line = processor.LineIndex,
            Column = column
        };

        if(processor.TrackTrivia)
        {
            alert.LinesBefore = processor.LinesBefore;
            processor.LinesBefore = null;

            // Record the quote line metadata similar to QuoteBlockParser
            alert.QuoteLines.Add(new QuoteBlockLine
            {
                QuoteChar = true,
                HasSpaceAfterQuoteChar = hasSpaceAfterQuoteChar,
                NewLine = processor.Line.NewLine,
                TriviaBefore = processor.UseTrivia(sourcePosition - 1),
                TriviaAfter = processor.Line.IsEmptyOrWhitespace() ? processor.UseTrivia(processor.Line.End) : StringSlice.Empty
            });

            // If not an empty line, reset TriviaStart to current position
            if(!processor.Line.IsEmptyOrWhitespace())
                processor.TriviaStart = processor.Start;
        }

        // push the new AlertBlock to the processor's new blocks
        processor.NewBlocks.Push(alert);

        return BlockState.Continue;
    }

    /// <inheritdoc />
    public override BlockState TryContinue(BlockProcessor processor, Block block)
    {
        if(processor.IsCodeIndent)
            return BlockState.None;

        var alert = (AlertBlock)block;
        var sourcePosition = processor.Start;
        var c = processor.CurrentChar;

        if(c != alert.QuoteChar)
        {
            if(processor.IsBlankLine)
                return BlockState.BreakDiscard;

            if(processor.TrackTrivia)
            {
                alert.QuoteLines.Add(new QuoteBlockLine
                {
                    QuoteChar = false,
                    NewLine = processor.Line.NewLine,
                });
            }

            return BlockState.None;
        }

        bool hasSpaceAfterQuoteChar = false;
        c = processor.NextChar(); // Skip quote marker char

        if(c == ' ')
        {
            processor.NextColumn();
            hasSpaceAfterQuoteChar = true;
            processor.SkipFirstUnwindSpace = true;
        }
        else if(c == '\t')
        {
            processor.NextColumn();
        }

        if(processor.TrackTrivia)
        {
            var triviaSpaceBefore = processor.UseTrivia(sourcePosition - 1);
            StringSlice triviaAfter = StringSlice.Empty;
            bool wasEmptyLine = false;

            if(processor.Line.IsEmptyOrWhitespace())
            {
                processor.TriviaStart = processor.Start;
                triviaAfter = processor.UseTrivia(processor.Line.End);
                wasEmptyLine = true;
            }

            alert.QuoteLines.Add(new QuoteBlockLine
            {
                QuoteChar = true,
                HasSpaceAfterQuoteChar = hasSpaceAfterQuoteChar,
                TriviaBefore = triviaSpaceBefore,
                TriviaAfter = triviaAfter,
                NewLine = processor.Line.NewLine,
            });

            if(!wasEmptyLine)
                processor.TriviaStart = processor.Start;
        }

        block.UpdateSpanEnd(processor.Line.End);

        return BlockState.Continue;
    }
    #endregion
}
