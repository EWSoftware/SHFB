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

using System.Text.RegularExpressions;

namespace SandcastleBuilder.MSBuild.Lunr;

/// <summary>
/// Pipeline function for trimming non-word characters from the beginning and end of tokens.
/// </summary>
public static class Trimmer
{
    private static readonly Regex LeadingNonWord = new(@"^\W+");
    private static readonly Regex TrailingNonWord = new(@"\W+$");

    static Trimmer()
    {
        Pipeline.RegisterFunction(TrimmerFunction, "trimmer");
    }

    /// <summary>
    /// Trims non-word characters from the beginning and end of a token.
    /// This implementation may not work correctly for non-latin characters and should
    /// either be removed or adapted for use with languages with non-latin characters.
    /// </summary>
    /// <param name="token">The token to trim.</param>
    /// <param name="index">The index of this token in the complete list of tokens.</param>
    /// <param name="tokens">All tokens for this document/field.</param>
    /// <returns>The trimmed token.</returns>
    public static object TrimmerFunction(Token token, int index, Token[] tokens)
    {
        if(token == null)
        {
            return null;
        }

        return token.Update((str, metadata) =>
        {
            str = LeadingNonWord.Replace(str, "");
            str = TrailingNonWord.Replace(str, "");
            return str;
        });
    }
}
