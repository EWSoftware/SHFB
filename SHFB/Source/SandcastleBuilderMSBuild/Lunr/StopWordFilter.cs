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

using System.Collections.Generic;

namespace SandcastleBuilder.MSBuild.Lunr;

/// <summary>
/// Pipeline function for filtering stop words from tokens.
/// </summary>
public static class StopWordFilter
{
    /// <summary>
    /// Default English language stop words.
    /// </summary>
    public static readonly HashSet<string> EnglishStopWords =
    [
        "a", "able", "about", "across", "after", "all", "almost", "also", "am", "among",
        "an", "and", "any", "are", "as", "at", "be", "because", "been", "but", "by",
        "can", "cannot", "could", "dear", "did", "do", "does", "either", "else", "ever",
        "every", "for", "from", "get", "got", "had", "has", "have", "he", "her", "hers",
        "him", "his", "how", "however", "i", "if", "in", "into", "is", "it", "its",
        "just", "least", "let", "like", "likely", "may", "me", "might", "most", "must",
        "my", "neither", "no", "nor", "not", "of", "off", "often", "on", "only", "or",
        "other", "our", "own", "rather", "said", "say", "says", "she", "should", "since",
        "so", "some", "than", "that", "the", "their", "them", "then", "there", "these",
        "they", "this", "tis", "to", "too", "twas", "us", "wants", "was", "we", "were",
        "what", "when", "where", "which", "while", "who", "whom", "why", "will", "with",
        "would", "yet", "you", "your"
    ];

    static StopWordFilter()
    {
        Pipeline.RegisterFunction(StopWordFilterFunction, "stopWordFilter");
    }

    /// <summary>
    /// Generates a stop word filter function from the provided list of stop words.
    /// </summary>
    /// <param name="stopWords">The list of stop words to filter.</param>
    /// <returns>A pipeline function that filters stop words.</returns>
    public static PipelineFunction GenerateStopWordFilter(IEnumerable<string> stopWords)
    {
        var words = new HashSet<string>(stopWords);

        return (token, index, tokens) =>
        {
            if(token == null)
            {
                return null;
            }

            string tokenStr = token.ToString();
            if(words.Contains(tokenStr))
            {
                return null;
            }

            return token;
        };
    }

    /// <summary>
    /// Default English language stop word filter pipeline function.
    /// Any words contained in the stop word list will not be passed through the filter.
    /// </summary>
    /// <param name="token">The token to check for being a stop word.</param>
    /// <param name="index">The index of this token in the complete list of tokens.</param>
    /// <param name="tokens">All tokens for this document/field.</param>
    /// <returns>The token if it's not a stop word, otherwise null.</returns>
    public static object StopWordFilterFunction(Token token, int index, Token[] tokens)
    {
        if(token == null)
        {
            return null;
        }

        string tokenStr = token.ToString();
        if(EnglishStopWords.Contains(tokenStr))
        {
            return null;
        }

        return token;
    }
}
