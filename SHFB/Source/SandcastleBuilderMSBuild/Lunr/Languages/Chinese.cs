/*!
 * Lunr languages, `Chinese` language
 * https://github.com/MihaiValentin/lunr-languages
 *
 * Copyright 2019, Felix Lian (repairearth)
 * http://www.mozilla.org/MPL/
 *
 * based on
 * Snowball zhvaScript Library v0.3
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
/// Chinese language support for Lunr.
/// Provides tokenization and stop word filtering for Chinese text.
/// Uses a bigram approach for tokenization since Chinese doesn't have word boundaries.
/// </summary>
public static class Chinese
{
    /// <summary>
    /// Word characters specific to Chinese language (CJK Unified Ideographs + Latin).
    /// </summary>
    public const string WordCharacters = "\\w\u4e00-\u9fa5";

    private static PipelineFunction _trimmer;
    private static PipelineFunction _stemmer;
    private static PipelineFunction _stopWordFilter;

    /// <summary>
    /// Gets the Chinese trimmer pipeline function.
    /// </summary>
    public static PipelineFunction Trimmer
    {
        get
        {
            if(_trimmer == null)
            {
                _trimmer = TrimmerSupport.GenerateTrimmer(WordCharacters);
                Pipeline.RegisterFunction(_trimmer, "trimmer-zh");
            }
            return _trimmer;
        }
    }

    /// <summary>
    /// Gets the Chinese stemmer pipeline function.
    /// Note: Chinese doesn't use traditional stemming, so this returns words unchanged.
    /// </summary>
    public static PipelineFunction Stemmer
    {
        get
        {
            if(_stemmer == null)
            {
                _stemmer = CreateStemmer();
                Pipeline.RegisterFunction(_stemmer, "stemmer-zh");
            }
            return _stemmer;
        }
    }

    /// <summary>
    /// Gets the Chinese stop word filter pipeline function.
    /// </summary>
    public static PipelineFunction StopWordFilter
    {
        get
        {
            if(_stopWordFilter == null)
            {
                _stopWordFilter = Lunr.StopWordFilter.GenerateStopWordFilter(StopWords);
                Pipeline.RegisterFunction(_stopWordFilter, "stopWordFilter-zh");
            }
            return _stopWordFilter;
        }
    }

    /// <summary>
    /// Chinese stop words.
    /// </summary>
    public static readonly HashSet<string> StopWords =
    [
        "的", "一", "不", "在", "人", "有", "是", "为", "為", "以", "于", "於", "上", "他", "而", "后", "後", "之", "来", "來", "及", "了",
        "因", "下", "可", "到", "由", "这", "這", "与", "與", "也", "此", "但", "并", "並", "个", "個", "其", "已", "无", "無", "小", "我",
        "们", "們", "起", "最", "再", "今", "去", "好", "只", "又", "或", "很", "亦", "某", "把", "那", "你", "乃", "它", "吧", "被", "比",
        "别", "趁", "当", "當", "从", "從", "得", "打", "凡", "儿", "兒", "尔", "爾", "该", "該", "各", "给", "給", "跟", "和", "何", "还",
        "還", "即", "几", "幾", "既", "看", "据", "據", "距", "靠", "啦", "另", "么", "麽", "每", "嘛", "拿", "哪", "您", "凭", "憑", "且",
        "却", "卻", "让", "讓", "仍", "啥", "如", "若", "使", "谁", "誰", "虽", "雖", "随", "隨", "同", "所", "她", "哇", "嗡", "往", "些",
        "向", "沿", "哟", "喲", "用", "咱", "则", "則", "怎", "曾", "至", "致", "着", "著", "诸", "諸", "自"
    ];

    private static PipelineFunction CreateStemmer()
    {
        // Chinese doesn't use traditional stemming
        return (token, index, tokens) => token;
    }

    /// <summary>
    /// Custom tokenizer for Chinese text that uses bigram segmentation.
    /// This is a simplified approach that works well for search without external dependencies.
    /// </summary>
    public static Func<object, Dictionary<string, object>, Token[]> Tokenizer => (obj, metadata) =>
    {
        if(obj == null)
            return [];

        var str = obj.ToString();
        if(String.IsNullOrWhiteSpace(str))
            return [];

        var tokens = new List<Token>();
        str = str.Trim().ToLower(new CultureInfo("zh-CN"));

        int position = 0;
        var chars = new List<char>();

        // Collect all characters
        for(int i = 0; i < str.Length; i++)
        {
            char c = str[i];

            // Check if it's a CJK character (U+4E00-U+9FFF)
            if(IsCJKCharacter(c))
            {
                chars.Add(c);
            }
            else if(Char.IsWhiteSpace(c))
            {
                // Process accumulated CJK characters as bigrams
                if(chars.Count > 0)
                {
                    tokens.AddRange(CreateBigrams(chars, position - chars.Count, tokens.Count));
                    chars.Clear();
                }
            }
            else if(Char.IsLetterOrDigit(c))
            {
                // For Latin characters, accumulate and add as word
                var wordStart = i;
                var word = new StringBuilder();
                while(i < str.Length && (Char.IsLetterOrDigit(str[i]) || str[i] == '_'))
                {
                    word.Append(str[i]);
                    i++;
                }
                i--; // Adjust for the loop increment

                if(word.Length > 0)
                {
                    var tokenMeta = new Dictionary<string, object>
                    {
                        ["position"] = new[] { wordStart, word.Length },
                        ["index"] = tokens.Count
                    };
                    tokens.Add(new Token(word.ToString(), tokenMeta));
                }
            }

            position++;
        }

        // Process any remaining CJK characters
        if(chars.Count > 0)
        {
            tokens.AddRange(CreateBigrams(chars, position - chars.Count, tokens.Count));
        }

        return [.. tokens];
    };

    private static bool IsCJKCharacter(char c)
    {
        // CJK Unified Ideographs: U+4E00-U+9FFF
        // This covers most common Chinese characters
        int code = c;
        return (code >= 0x4E00 && code <= 0x9FFF) ||
               (code >= 0x3400 && code <= 0x4DBF) ||  // CJK Extension A
               (code >= 0x20000 && code <= 0x2A6DF) || // CJK Extension B
               (code >= 0x2A700 && code <= 0x2B73F) || // CJK Extension C
               (code >= 0x2B740 && code <= 0x2B81F) || // CJK Extension D
               (code >= 0x2B820 && code <= 0x2CEAF) || // CJK Extension E
               (code >= 0xF900 && code <= 0xFAFF) ||   // CJK Compatibility Ideographs
               (code >= 0x2F800 && code <= 0x2FA1F);   // CJK Compatibility Supplement
    }

    private static List<Token> CreateBigrams(List<char> chars, int startPosition, int startIndex)
    {
        var tokens = new List<Token>();

        if(chars.Count == 0)
            return tokens;

        // Single character
        if(chars.Count == 1)
        {
            var tokenMeta = new Dictionary<string, object>
            {
                ["position"] = new[] { startPosition, 1 },
                ["index"] = startIndex
            };
            tokens.Add(new Token(chars[0].ToString(), tokenMeta));
            return tokens;
        }

        // Create bigrams (pairs of consecutive characters)
        // This helps with search while keeping index size reasonable
        for(int i = 0; i < chars.Count; i++)
        {
            // Add individual character
            var charMeta = new Dictionary<string, object>
            {
                ["position"] = new[] { startPosition + i, 1 },
                ["index"] = startIndex + tokens.Count
            };
            tokens.Add(new Token(chars[i].ToString(), charMeta));

            // Add bigram if not at the end
            if(i < chars.Count - 1)
            {
                var bigram = new string([chars[i], chars[i + 1]]);
                var bigramMeta = new Dictionary<string, object>
                {
                    ["position"] = new[] { startPosition + i, 2 },
                    ["index"] = startIndex + tokens.Count
                };
                tokens.Add(new Token(bigram, bigramMeta));
            }
        }

        return tokens;
    }

    /// <summary>
    /// Configures a Builder to use Chinese language processing.
    /// </summary>
    /// <param name="builder">The builder to configure.</param>
    public static void Register(Builder builder)
    {
        if(builder == null)
            throw new ArgumentNullException(nameof(builder));

        // Set the custom tokenizer for Chinese
        builder.Tokenizer = Tokenizer;

        builder.Pipeline.Reset();
        builder.Pipeline.Add(Trimmer);
        builder.Pipeline.Add(StopWordFilter);
        builder.Pipeline.Add(Stemmer);

        builder.SearchPipeline.Reset();
        builder.SearchPipeline.Add(Stemmer);
    }
}
