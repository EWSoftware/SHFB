//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : InclusionBlockParser.cs
// Author  : .NET Foundation
// Updated : 04/05/2026
// Note    : Copyright 2026, SHFB project, All rights reserved
//
// Block parser for inclusion blocks.
//
// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you
// under the MIT license.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/05/2025  JMC  Created the code based on https://github.com/dotnet/docfx/commits/2a436495ed0716eebbc7eaad84d652f18600c2f6/
//===============================================================================================================

using Markdig.Helpers;
using Markdig.Parsers;

namespace Sandcastle.Core.Markdown.Extensions.Inclusion;

/// <summary>
/// Markdig block parser for inclusion blocks '[!INCLUDE [&lt;title&gt;](&lt;filepath&gt;)]'.
/// </summary>
public class InclusionBlockParser : BlockParser
{
    private const string StartString = "[!include";

    /// <summary>
    /// Initializes a new instance of the <see cref="InclusionBlockParser"/> class.
    /// </summary>
    public InclusionBlockParser()
    {
        // important: if you don't set this, the parser won't be called!
        OpeningCharacters = ['['];
    }

    /// <summary>
    /// Tries to match a block opening.
    /// </summary>
    /// <param name="processor">The parser processor.</param>
    /// <returns>The result of the match.</returns>
    public override BlockState TryOpen(BlockProcessor processor)
    {
        // stop processing if we're in a code block
        if(processor.IsCodeIndent)
        {
            return BlockState.None;
        }

        // [!include[<title>](<filepath>)]
        int column = processor.Column;
        StringSlice line = processor.Line;

        if(!ExtensionsHelper.MatchStart(ref line, StartString, false))
        {
            return BlockState.None;
        }
        else
        {
            if(line.CurrentChar == '+')
            {
                line.NextChar();
            }
        }

        string title = null, path = null;

        if(!ExtensionsHelper.MatchLink(ref line, ref title, ref path) || !ExtensionsHelper.MatchInclusionEnd(ref line))
        {
            return BlockState.None;
        }

        while(line.CurrentChar.IsSpaceOrTab()) line.NextChar();
        if(line.CurrentChar != '\0')
        {
            return BlockState.None;
        }

        InclusionBlock incBlock = new(this)
        {
            Title = title,
            IncludedFilePath = path,
            Line = processor.LineIndex,
            Column = column,
        };
        processor.NewBlocks.Push(incBlock);

        return BlockState.BreakDiscard;
    }
}
