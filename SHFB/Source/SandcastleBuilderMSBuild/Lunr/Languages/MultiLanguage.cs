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

using System;
using System.Collections.Generic;

namespace SandcastleBuilder.MSBuild.Lunr.Languages;

/// <summary>
/// Provides multi-language support for indexing content in multiple languages.
/// </summary>
public static class MultiLanguage
{
    private static readonly Dictionary<string, LanguageSupport> SupportedLanguages = new()
    {
        { "en", new LanguageSupport
            {
                WordCharacters = "\\w",
                Trimmer = Trimmer.TrimmerFunction,
                StopWordFilter = StopWordFilter.StopWordFilterFunction,
                Stemmer = Stemmer.StemmerFunction
            }
        },
        { "de", new LanguageSupport
            {
                WordCharacters = German.WordCharacters,
                Trimmer = German.Trimmer,
                StopWordFilter = German.StopWordFilter,
                Stemmer = German.Stemmer
            }
        },
        { "es", new LanguageSupport
            {
                WordCharacters = Spanish.WordCharacters,
                Trimmer = Spanish.Trimmer,
                StopWordFilter = Spanish.StopWordFilter,
                Stemmer = Spanish.Stemmer
            }
        },
        { "fr", new LanguageSupport
            {
                WordCharacters = French.WordCharacters,
                Trimmer = French.Trimmer,
                StopWordFilter = French.StopWordFilter,
                Stemmer = French.Stemmer
            }
        },
        { "it", new LanguageSupport
            {
                WordCharacters = Italian.WordCharacters,
                Trimmer = Italian.Trimmer,
                StopWordFilter = Italian.StopWordFilter,
                Stemmer = Italian.Stemmer
            }
        },
        { "pt", new LanguageSupport
            {
                WordCharacters = Portuguese.WordCharacters,
                Trimmer = Portuguese.Trimmer,
                StopWordFilter = Portuguese.StopWordFilter,
                Stemmer = Portuguese.Stemmer
            }
        },
        { "ru", new LanguageSupport
            {
                WordCharacters = Russian.WordCharacters,
                Trimmer = Russian.Trimmer,
                StopWordFilter = Russian.StopWordFilter,
                Stemmer = Russian.Stemmer
            }
        },
        { "zh", new LanguageSupport
            {
                WordCharacters = Chinese.WordCharacters,
                Trimmer = Chinese.Trimmer,
                StopWordFilter = Chinese.StopWordFilter,
                Stemmer = Chinese.Stemmer
            }
        },
        { "ko", new LanguageSupport
            {
                WordCharacters = Korean.WordCharacters,
                Trimmer = Korean.Trimmer,
                StopWordFilter = Korean.StopWordFilter,
                Stemmer = Korean.Stemmer
            }
        },
        { "ja", new LanguageSupport
            {
                WordCharacters = Japanese.WordCharacters,
                Trimmer = Japanese.Trimmer,
                StopWordFilter = Japanese.StopWordFilter,
                Stemmer = Japanese.Stemmer
            }
        }
    };

    /// <summary>
    /// Registers a custom language for multi-language support.
    /// </summary>
    /// <param name="languageCode">The language code (e.g., "fr", "es").</param>
    /// <param name="wordCharacters">The word characters pattern for this language.</param>
    /// <param name="trimmer">The trimmer function.</param>
    /// <param name="stopWordFilter">The stop word filter function.</param>
    /// <param name="stemmer">The stemmer function.</param>
    public static void RegisterLanguage(
        string languageCode,
        string wordCharacters,
        PipelineFunction trimmer,
        PipelineFunction stopWordFilter,
        PipelineFunction stemmer)
    {
        SupportedLanguages[languageCode] = new LanguageSupport
        {
            WordCharacters = wordCharacters,
            Trimmer = trimmer,
            StopWordFilter = stopWordFilter,
            Stemmer = stemmer
        };
    }

    /// <summary>
    /// Creates a multi-language configuration for the specified languages.
    /// </summary>
    /// <param name="languageCodes">The language codes to support (e.g., "en", "de").</param>
    /// <returns>An action that configures a builder for multi-language support.</returns>
    public static Action<Builder> Create(params string[] languageCodes)
    {
        if(languageCodes == null || languageCodes.Length == 0)
        {
            throw new ArgumentException("At least one language must be specified", nameof(languageCodes));
        }

        // Validate all languages are supported
        foreach(var lang in languageCodes)
        {
            if(!SupportedLanguages.ContainsKey(lang))
            {
                throw new ArgumentException($"Language '{lang}' is not supported. Please register it first or use a supported language.", nameof(languageCodes));
            }
        }

        string nameSuffix = String.Join("-", languageCodes);
        string wordCharacters = "";
        var pipeline = new List<PipelineFunction>();
        var searchPipeline = new List<PipelineFunction>();

        // Build word characters and collect pipeline functions
        foreach(var lang in languageCodes)
        {
            var support = SupportedLanguages[lang];
            wordCharacters += support.WordCharacters;

            if(support.StopWordFilter != null)
            {
                pipeline.Insert(0, support.StopWordFilter);
            }

            if(support.Stemmer != null)
            {
                pipeline.Add(support.Stemmer);
                searchPipeline.Add(support.Stemmer);
            }
        }

        // Create multi-language trimmer
        var multiTrimmer = TrimmerSupport.GenerateTrimmer(wordCharacters);
        Pipeline.RegisterFunction(multiTrimmer, $"lunr-multi-trimmer-{nameSuffix}");
        pipeline.Insert(0, multiTrimmer);

        return (builder) =>
        {
            builder.Pipeline.Reset();
            foreach(var func in pipeline)
            {
                builder.Pipeline.Add(func);
            }

            builder.SearchPipeline.Reset();
            foreach(var func in searchPipeline)
            {
                builder.SearchPipeline.Add(func);
            }
        };
    }

    /// <summary>
    /// Configures a builder for multi-language support.
    /// </summary>
    /// <param name="builder">The builder to configure.</param>
    /// <param name="languageCodes">The language codes to support.</param>
    public static void Register(Builder builder, params string[] languageCodes)
    {
        var configureAction = Create(languageCodes);
        configureAction(builder);
    }

    private sealed class LanguageSupport
    {
        public string WordCharacters { get; set; } = "";
        public PipelineFunction Trimmer { get; set; } = null!;
        public PipelineFunction StopWordFilter { get; set; }
        public PipelineFunction Stemmer { get; set; }
    }
}
