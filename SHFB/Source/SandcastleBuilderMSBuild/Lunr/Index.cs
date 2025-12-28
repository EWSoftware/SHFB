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

#pragma warning disable CA1819

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SandcastleBuilder.MSBuild.Lunr;

/// <summary>
/// An index contains the built index of all documents and provides serialization capabilities.
/// </summary>
public class Index
{
    private static readonly JsonSerializerOptions options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    /// <summary>
    /// Gets the Lunr version.
    /// </summary>
    public const string Version = "2.3.9";

    /// <summary>
    /// Initializes a new instance of the <see cref="Index"/> class.
    /// </summary>
    /// <param name="invertedIndex">An index of term/field to document reference.</param>
    /// <param name="fieldVectors">Field vectors.</param>
    /// <param name="tokenSet">A set of all corpus tokens.</param>
    /// <param name="fields">The names of indexed document fields.</param>
    /// <param name="pipeline">The pipeline to use for search terms.</param>
    public Index(
        Dictionary<string, Dictionary<string, object>> invertedIndex,
        Dictionary<string, Vector> fieldVectors,
        TokenSet tokenSet,
        string[] fields,
        Pipeline pipeline)
    {
        InvertedIndex = invertedIndex;
        FieldVectors = fieldVectors;
        TokenSet = tokenSet;
        Fields = fields;
        Pipeline = pipeline;
    }

    /// <summary>
    /// Gets the inverted index.
    /// </summary>
    public Dictionary<string, Dictionary<string, object>> InvertedIndex { get; }

    /// <summary>
    /// Gets the field vectors.
    /// </summary>
    public Dictionary<string, Vector> FieldVectors { get; }

    /// <summary>
    /// Gets the token set.
    /// </summary>
    public TokenSet TokenSet { get; }

    /// <summary>
    /// Gets the indexed fields.
    /// </summary>
    public string[] Fields { get; }

    /// <summary>
    /// Gets the pipeline.
    /// </summary>
    public Pipeline Pipeline { get; }

    /// <summary>
    /// Serializes the index to a JSON string.
    /// </summary>
    /// <returns>A JSON string representation of the index.</returns>
    public string Serialize()
    {
        var invertedIndex = InvertedIndex.Keys.OrderBy(k => k, StringComparer.Ordinal).Select(
            term => new object[] { term, InvertedIndex[term] }).ToArray();

        var fieldVectors = FieldVectors.Keys.Select(
            refKey => new object[] { refKey, FieldVectors[refKey].ToJson() }).ToArray();

        var data = new IndexData
        {
            Version = Version,
            Fields = Fields,
            FieldVectors = fieldVectors,
            InvertedIndex = invertedIndex,
            Pipeline = Pipeline.ToJson()
        };

        return JsonSerializer.Serialize(data, options);
    }

    /// <summary>
    /// Loads a previously serialized index.
    /// </summary>
    /// <param name="serializedIndex">A previously serialized index.</param>
    /// <returns>A new Index instance.</returns>
    public static Index Load(IndexData serializedIndex)
    {
        if(serializedIndex == null)
            throw new ArgumentNullException(nameof(serializedIndex));

        var fieldVectors = new Dictionary<string, Vector>();
        var invertedIndex = new Dictionary<string, Dictionary<string, object>>();
        var tokenSetBuilder = new TokenSetBuilder();
        var pipeline = Pipeline.Load(serializedIndex.Pipeline);

        if(serializedIndex.Version != Version)
            throw new InvalidOperationException($"Version mismatch when loading serialized index. Current version '{Version}' does not match serialized index '{serializedIndex.Version}'");

        foreach(var tuple in serializedIndex.FieldVectors)
        {
            string refKey = ((JsonElement)tuple[0]).GetString()!;
            var elements = ((JsonElement)tuple[1]).EnumerateArray().Select(e => e.GetDouble());
            fieldVectors[refKey] = new Vector(elements);
        }

        foreach(var tuple in serializedIndex.InvertedIndex)
        {
            string term = ((JsonElement)tuple[0]).GetString()!;
            var posting = JsonSerializer.Deserialize<Dictionary<string, object>>((JsonElement)tuple[1]);

            tokenSetBuilder.Insert(term);
            invertedIndex[term] = posting!;
        }

        tokenSetBuilder.Finish();

        return new Index(
            invertedIndex,
            fieldVectors,
            tokenSetBuilder.Root,
            serializedIndex.Fields,
            pipeline
        );
    }

    /// <summary>
    /// Loads a previously serialized index from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <returns>A new Index instance.</returns>
    public static Index Deserialize(string json)
    {
        var data = JsonSerializer.Deserialize<IndexData>(json);
        return data == null ? throw new InvalidOperationException("Failed to deserialize index data") : Load(data);
    }
}

/// <summary>
/// Serializable index data structure.
/// </summary>
public class IndexData
{
    /// <summary>
    /// Gets or sets the Lunr version.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = Index.Version;

    /// <summary>
    /// Gets or sets the indexed fields.
    /// </summary>
    [JsonPropertyName("fields")]
    public string[] Fields { get; set; } = [];

    /// <summary>
    /// Gets or sets the field vectors.
    /// </summary>
    [JsonPropertyName("fieldVectors")]
    public object[][] FieldVectors { get; set; } = [];

    /// <summary>
    /// Gets or sets the inverted index.
    /// </summary>
    [JsonPropertyName("invertedIndex")]
    public object[][] InvertedIndex { get; set; } = [];

    /// <summary>
    /// Gets or sets the pipeline function names.
    /// </summary>
    [JsonPropertyName("pipeline")]
    public string[] Pipeline { get; set; } = [];
}
