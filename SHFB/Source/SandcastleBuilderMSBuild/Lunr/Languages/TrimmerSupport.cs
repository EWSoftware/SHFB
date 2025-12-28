/*!
 * Snowball JavaScript Library v0.3
 * http://code.google.com/p/urim/
 * http://snowball.tartarus.org/
 *
 * Copyright 2010, Oleg Mazko
 * http://www.mozilla.org/MPL/
 *
 * Original JavaScript source and project website: https://github.com/MihaiValentin/lunr-languages
 *
 * EFW - 12/24/2025 - Ported to C# using Copilot.  This is a minimal port that only includes the languages
 * currently supported by the help file builder.
 */

using System.Text.RegularExpressions;

namespace SandcastleBuilder.MSBuild.Lunr.Languages;

/// <summary>
/// Provides support for generating custom trimmers for different character sets.
/// </summary>
public static class TrimmerSupport
{
    /// <summary>
    /// Generates a trimmer function for the specified word characters.
    /// </summary>
    /// <param name="wordCharacters">The word characters pattern.</param>
    /// <returns>A pipeline function that trims non-word characters.</returns>
    public static PipelineFunction GenerateTrimmer(string wordCharacters)
    {
        var startRegex = new Regex($"^[^{wordCharacters}]+");
        var endRegex = new Regex($"[^{wordCharacters}]+$");

        return (token, index, tokens) =>
        {
            if(token == null)
            {
                return null;
            }

            return token.Update((str, metadata) =>
            {
                str = startRegex.Replace(str, "");
                str = endRegex.Replace(str, "");
                return str;
            });
        };
    }
}
