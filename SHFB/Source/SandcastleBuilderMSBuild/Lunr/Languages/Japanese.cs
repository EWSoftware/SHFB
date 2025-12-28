/*!
 * Lunr languages, `Japanese` language
 * https://github.com/MihaiValentin/lunr-languages
 *
 * Copyright 2014, Chad Liu
 * http://www.mozilla.org/MPL/
 *
 * based on
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SandcastleBuilder.MSBuild.Lunr.Languages;

/// <summary>
/// Japanese language support for Lunr.
/// Provides tokenization and stop word filtering for Japanese text.
/// Uses character-based tokenization for mixed Hiragana, Katakana, and Kanji scripts.
/// This is a simplified approach that doesn't require TinySegmenter.
/// </summary>
public static class Japanese
{
    /// <summary>
    /// Word characters specific to Japanese language (Hiragana, Katakana, Kanji, Latin).
    /// </summary>
    public const string WordCharacters = "一二三四五六七八九十百千万億兆一-龠々〆ヵヶぁ-んァ-ヴーｱ-ﾝﾞa-zA-Zａ-ｚＡ-Ｚ0-9０-９";

    private static PipelineFunction _trimmer;
    private static PipelineFunction _stemmer;
    private static PipelineFunction _stopWordFilter;

    /// <summary>
    /// Gets the Japanese trimmer pipeline function.
    /// </summary>
    public static PipelineFunction Trimmer
    {
        get
        {
            if(_trimmer == null)
            {
                _trimmer = TrimmerSupport.GenerateTrimmer(WordCharacters);
                Pipeline.RegisterFunction(_trimmer, "trimmer-ja");
            }
            return _trimmer;
        }
    }

    /// <summary>
    /// Gets the Japanese stemmer pipeline function.
    /// Note: Japanese doesn't use traditional stemming.
    /// </summary>
    public static PipelineFunction Stemmer
    {
        get
        {
            if(_stemmer == null)
            {
                _stemmer = CreateStemmer();
                Pipeline.RegisterFunction(_stemmer, "stemmer-ja");
            }
            return _stemmer;
        }
    }

    /// <summary>
    /// Gets the Japanese stop word filter pipeline function.
    /// </summary>
    public static PipelineFunction StopWordFilter
    {
        get
        {
            if(_stopWordFilter == null)
            {
                _stopWordFilter = Lunr.StopWordFilter.GenerateStopWordFilter(StopWords);
                Pipeline.RegisterFunction(_stopWordFilter, "stopWordFilter-ja");
            }
            return _stopWordFilter;
        }
    }

    /// <summary>
    /// Japanese stop words.
    /// </summary>
    public static readonly HashSet<string> StopWords =
    [
        "これ", "それ", "あれ", "この", "その", "あの", "ここ", "そこ", "あそこ", "こちら", "どこ", "だれ", "なに", "なん", "何", "私",
        "貴方", "貴方方", "我々", "私達", "あの人", "あのかた", "彼女", "彼", "です", "あります", "おります", "います", "は", "が",
        "の", "に", "を", "で", "え", "から", "まで", "より", "も", "どの", "と", "し", "それで", "しかし"
    ];

    private static PipelineFunction CreateStemmer()
    {
        // Japanese doesn't use traditional stemming
        return (token, index, tokens) => token;
    }

    /// <summary>
    /// Custom tokenizer for Japanese text.
    /// Handles Hiragana, Katakana, Kanji, and mixed Japanese/Latin text.
    /// Uses character-type based segmentation.
    /// </summary>
    public static Func<object, Dictionary<string, object>, Token[]> Tokenizer => (obj, metadata) =>
    {
        if(obj == null)
            return [];

        var str = obj.ToString();
        if(String.IsNullOrWhiteSpace(str))
            return [];

        var tokens = new List<Token>();
        str = str.Trim().ToLower(new CultureInfo("ja-JP"));

        int position = 0;
        var currentWord = new StringBuilder();
        int wordStart = 0;
        CharType currentType = CharType.None;

        for(int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            CharType charType = GetCharType(c);

            if(charType == CharType.Whitespace || charType == CharType.Punctuation)
            {
                // End current word if any
                if(currentWord.Length > 0)
                {
                    ProcessWord(tokens, currentWord.ToString(), wordStart, currentType);
                    currentWord.Clear();
                    currentType = CharType.None;
                }
            }
            else if(charType != currentType && currentType != CharType.None)
            {
                // Character type changed, process current word
                if(currentWord.Length > 0)
                {
                    ProcessWord(tokens, currentWord.ToString(), wordStart, currentType);
                    currentWord.Clear();
                }
                currentType = charType;
                currentWord.Append(c);
                wordStart = position;
            }
            else
            {
                // Same type or starting new word
                if(currentType == CharType.None)
                {
                    currentType = charType;
                    wordStart = position;
                }
                currentWord.Append(c);
            }

            position++;
        }

        // Add final word if any
        if(currentWord.Length > 0)
        {
            ProcessWord(tokens, currentWord.ToString(), wordStart, currentType);
        }

        return [.. tokens];
    };

    private enum CharType
    {
        None,
        Hiragana,
        Katakana,
        Kanji,
        Latin,
        Number,
        Whitespace,
        Punctuation
    }

    private static CharType GetCharType(char c)
    {
        int code = c;

        // Whitespace
        if(Char.IsWhiteSpace(c))
            return CharType.Whitespace;

        // Punctuation
        if(Char.IsPunctuation(c) || (code >= 0x3000 && code <= 0x303F))
            return CharType.Punctuation;

        // Hiragana: U+3040-U+309F
        if(code >= 0x3040 && code <= 0x309F)
            return CharType.Hiragana;

        // Katakana: U+30A0-U+30FF, Half-width Katakana: U+FF65-U+FF9F
        if((code >= 0x30A0 && code <= 0x30FF) || (code >= 0xFF65 && code <= 0xFF9F))
            return CharType.Katakana;

        // Kanji (CJK Unified Ideographs): U+4E00-U+9FFF
        if(code >= 0x4E00 && code <= 0x9FFF)
            return CharType.Kanji;

        // Numbers
        if(Char.IsDigit(c) || (code >= 0xFF10 && code <= 0xFF19)) // Full-width numbers
            return CharType.Number;

        // Latin letters (including full-width)
        if(Char.IsLetter(c) || (code >= 0xFF21 && code <= 0xFF5A))
            return CharType.Latin;

        return CharType.None;
    }

    private static void ProcessWord(List<Token> tokens, string word, int startPos, CharType type)
    {
        // For Kanji, create bigrams similar to Chinese
        if(type == CharType.Kanji && word.Length > 1)
        {
            // Add individual characters
            for(int i = 0; i < word.Length; i++)
            {
                var charMeta = new Dictionary<string, object>
                {
                    ["position"] = new[] { startPos + i, 1 },
                    ["index"] = tokens.Count
                };
                tokens.Add(new Token(word[i].ToString(), charMeta));
            }

            // Add bigrams
            for(int i = 0; i < word.Length - 1; i++)
            {
                var bigram = word.Substring(i, 2);
                var bigramMeta = new Dictionary<string, object>
                {
                    ["position"] = new[] { startPos + i, 2 },
                    ["index"] = tokens.Count
                };
                tokens.Add(new Token(bigram, bigramMeta));
            }
        }
        else
        {
            // For Hiragana, Katakana, Latin, and Numbers, add as whole word
            var tokenMeta = new Dictionary<string, object>
            {
                ["position"] = new[] { startPos, word.Length },
                ["index"] = tokens.Count
            };
            tokens.Add(new Token(word, tokenMeta));
        }
    }

    /// <summary>
    /// Configures a Builder to use Japanese language processing.
    /// </summary>
    /// <param name="builder">The builder to configure.</param>
    public static void Register(Builder builder)
    {
        if(builder == null)
            throw new ArgumentNullException(nameof(builder));

        // Set the custom tokenizer for Japanese
        builder.Tokenizer = Tokenizer;

        builder.Pipeline.Reset();
        builder.Pipeline.Add(Trimmer);
        builder.Pipeline.Add(StopWordFilter);
        builder.Pipeline.Add(Stemmer);

        builder.SearchPipeline.Reset();
        builder.SearchPipeline.Add(Stemmer);
    }
}
