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

#pragma warning disable CA1819

using System;

namespace SandcastleBuilder.MSBuild.Lunr.Languages;

/// <summary>
/// Represents a substring match in the stemming process.
/// </summary>
public class Among
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Among"/> class.
    /// </summary>
    /// <param name="s">The string to match.</param>
    /// <param name="substringIndex">The substring index.</param>
    /// <param name="result">The result value.</param>
    /// <param name="method">Optional method to execute.</param>
    public Among(string s, int substringIndex, int result, Func<bool> method = null)
    {
        if(s == null)
            throw new ArgumentNullException(nameof(s));

        SSize = s.Length;
        S = ToCharArray(s);
        SubstringIndex = substringIndex;
        Result = result;
        Method = method;
    }

    /// <summary>
    /// Gets the size of the string.
    /// </summary>
    public int SSize { get; }

    /// <summary>
    /// Gets the character array representation of the string.
    /// </summary>
    public int[] S { get; }

    /// <summary>
    /// Gets the substring index.
    /// </summary>
    public int SubstringIndex { get; }

    /// <summary>
    /// Gets the result value.
    /// </summary>
    public int Result { get; }

    /// <summary>
    /// Gets the optional method to execute.
    /// </summary>
    public Func<bool> Method { get; }

    private static int[] ToCharArray(string s)
    {
        int[] charArr = new int[s.Length];
        for(int i = 0; i < s.Length; i++)
        {
            charArr[i] = s[i];
        }
        return charArr;
    }
}
