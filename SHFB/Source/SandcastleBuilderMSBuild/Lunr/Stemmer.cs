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
using System.Text.RegularExpressions;

namespace SandcastleBuilder.MSBuild.Lunr;

/// <summary>
/// Pipeline function for stemming English words using the Porter Stemmer algorithm.
/// This is a C# implementation of the Porter Stemmer taken from http://tartarus.org/~martin
/// </summary>
public static class Stemmer
{
    private static readonly Dictionary<string, string> Step2List = new()
    {
        { "ational", "ate" },
        { "tional", "tion" },
        { "enci", "ence" },
        { "anci", "ance" },
        { "izer", "ize" },
        { "bli", "ble" },
        { "alli", "al" },
        { "entli", "ent" },
        { "eli", "e" },
        { "ousli", "ous" },
        { "ization", "ize" },
        { "ation", "ate" },
        { "ator", "ate" },
        { "alism", "al" },
        { "iveness", "ive" },
        { "fulness", "ful" },
        { "ousness", "ous" },
        { "aliti", "al" },
        { "iviti", "ive" },
        { "biliti", "ble" },
        { "logi", "log" }
    };

    private static readonly Dictionary<string, string> Step3List = new()
    {
        { "icate", "ic" },
        { "ative", "" },
        { "alize", "al" },
        { "iciti", "ic" },
        { "ical", "ic" },
        { "ful", "" },
        { "ness", "" }
    };

    private const string C = "[^aeiou]";           // consonant
    private const string V = "[aeiouy]";           // vowel
    private const string CC = C + "[^aeiouy]*";    // consonant sequence
    private const string VV = V + "[aeiou]*";      // vowel sequence

    private static readonly Regex Mgr0 = new("^(" + CC + ")?" + VV + CC);
    private static readonly Regex Meq1 = new("^(" + CC + ")?" + VV + CC + "(" + VV + ")?$");
    private static readonly Regex Mgr1 = new("^(" + CC + ")?" + VV + CC + VV + CC);
    private static readonly Regex SV = new("^(" + CC + ")?" + V);

    private static readonly Regex Re1a = new(@"^(.+?)(ss|i)es$");
    private static readonly Regex Re2_1a = new(@"^(.+?)([^s])s$");
    private static readonly Regex Re1b = new(@"^(.+?)eed$");
    private static readonly Regex Re2_1b = new(@"^(.+?)(ed|ing)$");
    private static readonly Regex Re1b_2 = new(@"^.$");
    private static readonly Regex Re2_1b_2 = new(@"(at|bl|iz)$");
    private static readonly Regex Re3_1b_2 = new(@"([^aeiouylsz])\1$");
    private static readonly Regex Re4_1b_2 = new("^" + CC + V + "[^aeiouwxy]$");

    private static readonly Regex Re1c = new(@"^(.+?[^aeiou])y$");
    private static readonly Regex Re2 = new(@"^(.+?)(ational|tional|enci|anci|izer|bli|alli|entli|eli|ousli|ization|ation|ator|alism|iveness|fulness|ousness|aliti|iviti|biliti|logi)$");
    private static readonly Regex Re3 = new(@"^(.+?)(icate|ative|alize|iciti|ical|ful|ness)$");
    private static readonly Regex Re4 = new(@"^(.+?)(al|ance|ence|er|ic|able|ible|ant|ement|ment|ent|ou|ism|ate|iti|ous|ive|ize)$");
    private static readonly Regex Re2_4 = new(@"^(.+?)(s|t)(ion)$");
    private static readonly Regex Re5 = new(@"^(.+?)e$");
    private static readonly Regex Re5_1 = new(@"ll$");
    private static readonly Regex Re3_5 = new("^" + CC + V + "[^aeiouwxy]$");

    static Stemmer()
    {
        Pipeline.RegisterFunction(StemmerFunction, "stemmer");
    }

    /// <summary>
    /// Stems a word using the Porter Stemmer algorithm.
    /// </summary>
    /// <param name="w">The word to stem.</param>
    /// <returns>The stemmed word.</returns>
    public static string PorterStemmer(string w)
    {
        if(w == null)
            throw new ArgumentNullException(nameof(w));

        if(w.Length < 3)
        {
            return w;
        }

        string firstch = w.Substring(0, 1);
        if(firstch == "y")
        {
            w = firstch.ToUpperInvariant() + w.Substring(1);
        }

        // Step 1a
        Match match;
        if(Re1a.IsMatch(w))
        {
            w = Re1a.Replace(w, "$1$2");
        }
        else if(Re2_1a.IsMatch(w))
        {
            w = Re2_1a.Replace(w, "$1$2");
        }

        // Step 1b
        if(Re1b.IsMatch(w))
        {
            match = Re1b.Match(w);
            if(Mgr0.IsMatch(match.Groups[1].Value))
            {
                w = Re1b_2.Replace(w, "");
            }
        }
        else if(Re2_1b.IsMatch(w))
        {
            match = Re2_1b.Match(w);
            string stem = match.Groups[1].Value;
            if(SV.IsMatch(stem))
            {
                w = stem;
                if(Re2_1b_2.IsMatch(w))
                {
                    w += "e";
                }
                else if(Re3_1b_2.IsMatch(w))
                {
                    w = w.Substring(0, w.Length - 1);
                }
                else if(Re4_1b_2.IsMatch(w))
                {
                    w += "e";
                }
            }
        }

        // Step 1c
        if(Re1c.IsMatch(w))
        {
            match = Re1c.Match(w);
            string stem = match.Groups[1].Value;
            w = stem + "i";
        }

        // Step 2
        if(Re2.IsMatch(w))
        {
            match = Re2.Match(w);
            string stem = match.Groups[1].Value;
            string suffix = match.Groups[2].Value;
            if(Mgr0.IsMatch(stem))
            {
                w = stem + Step2List[suffix];
            }
        }

        // Step 3
        if(Re3.IsMatch(w))
        {
            match = Re3.Match(w);
            string stem = match.Groups[1].Value;
            string suffix = match.Groups[2].Value;
            if(Mgr0.IsMatch(stem))
            {
                w = stem + Step3List[suffix];
            }
        }

        // Step 4
        if(Re4.IsMatch(w))
        {
            match = Re4.Match(w);
            string stem = match.Groups[1].Value;
            if(Mgr1.IsMatch(stem))
            {
                w = stem;
            }
        }
        else if(Re2_4.IsMatch(w))
        {
            match = Re2_4.Match(w);
            string stem = match.Groups[1].Value + match.Groups[2].Value;
            if(Mgr1.IsMatch(stem))
            {
                w = stem;
            }
        }

        // Step 5
        if(Re5.IsMatch(w))
        {
            match = Re5.Match(w);
            string stem = match.Groups[1].Value;
            if(Mgr1.IsMatch(stem) || (Meq1.IsMatch(stem) && !Re3_5.IsMatch(stem)))
            {
                w = stem;
            }
        }

        if(Re5_1.IsMatch(w) && Mgr1.IsMatch(w))
        {
            w = Re1b_2.Replace(w, "");
        }

        // Turn initial Y back to y
        if(firstch == "y")
        {
            w = firstch.ToLowerInvariant() + w.Substring(1);
        }

        return w;
    }

    /// <summary>
    /// Pipeline function that stems tokens using the Porter Stemmer algorithm.
    /// </summary>
    /// <param name="token">The token to stem.</param>
    /// <param name="index">The index of this token in the complete list of tokens.</param>
    /// <param name="tokens">All tokens for this document/field.</param>
    /// <returns>The stemmed token.</returns>
    public static object StemmerFunction(Token token, int index, Token[] tokens)
    {
        if(token == null)
        {
            return null;
        }

        return token.Update((str, metadata) => PorterStemmer(str));
    }
}
