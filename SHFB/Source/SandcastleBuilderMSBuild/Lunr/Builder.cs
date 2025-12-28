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

#pragma warning disable CA1002

using System;
using System.Collections.Generic;
using System.Linq;

namespace SandcastleBuilder.MSBuild.Lunr;

/// <summary>
/// Delegate for extracting a field from a document.
/// </summary>
/// <param name="doc">The document being added to the index.</param>
/// <returns>The object that will be indexed for this field.</returns>
public delegate object FieldExtractor(Dictionary<string, object> doc);

/// <summary>
/// Field attributes for configuring field indexing.
/// </summary>
public class FieldAttributes
{
    /// <summary>
    /// Gets or sets the boost applied to all terms within this field.
    /// </summary>
    public double Boost { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the function to extract a field from a document.
    /// </summary>
    public FieldExtractor Extractor { get; set; }
}

/// <summary>
/// Document attributes for configuring document indexing.
/// </summary>
public class DocumentAttributes
{
    /// <summary>
    /// Gets or sets the boost applied to all terms within this document.
    /// </summary>
    public double Boost { get; set; } = 1.0;
}

/// <summary>
/// Builder performs indexing on a set of documents and returns instances of Index ready for querying.
/// </summary>
public class Builder
{
    private string _ref = "id";
    private readonly Dictionary<string, FieldAttributes> _fields;
    private readonly Dictionary<string, DocumentAttributes> _documents;
    private double _b = 0.75;
    private double _k1 = 1.2;

    /// <summary>
    /// Initializes a new instance of the <see cref="Builder"/> class.
    /// </summary>
    public Builder()
    {
        _fields = [];
        _documents = [];
        InvertedIndex = [];
        FieldTermFrequencies = [];
        FieldLengths = [];
        Tokenizer = Lunr.Tokenizer.DefaultTokenizer;
        Pipeline = new Pipeline();
        SearchPipeline = new Pipeline();
        DocumentCount = 0;
        TermIndex = 0;
        MetadataWhitelist = [];
        AverageFieldLength = [];
    }

    /// <summary>
    /// Gets the inverted index that maps terms to document fields.
    /// </summary>
    public Dictionary<string, Dictionary<string, object>> InvertedIndex { get; }

    /// <summary>
    /// Gets the document term frequencies.
    /// </summary>
    public Dictionary<string, Dictionary<string, int>> FieldTermFrequencies { get; }

    /// <summary>
    /// Gets the field lengths.
    /// </summary>
    public Dictionary<string, int> FieldLengths { get; }

    /// <summary>
    /// Gets or sets the tokenizer function.
    /// </summary>
    public Func<object, Dictionary<string, object>, Token[]> Tokenizer { get; set; }

    /// <summary>
    /// Gets the pipeline for text processing.
    /// </summary>
    public Pipeline Pipeline { get; }

    /// <summary>
    /// Gets the search pipeline.
    /// </summary>
    public Pipeline SearchPipeline { get; }

    /// <summary>
    /// Gets the total number of documents indexed.
    /// </summary>
    public int DocumentCount { get; private set; }

    /// <summary>
    /// Gets the current term index.
    /// </summary>
    public int TermIndex { get; private set; }

    /// <summary>
    /// Gets the metadata whitelist.
    /// </summary>
    public List<string> MetadataWhitelist { get; }

    /// <summary>
    /// Gets the average field lengths.
    /// </summary>
    public Dictionary<string, double> AverageFieldLength { get; private set; }

    /// <summary>
    /// Gets the field vectors.
    /// </summary>
    public Dictionary<string, Vector> FieldVectors { get; private set; } = [];

    /// <summary>
    /// Sets the document field used as the document reference.
    /// </summary>
    /// <param name="refField">The name of the reference field in the document.</param>
    public void Ref(string refField)
    {
        _ref = refField;
    }

    /// <summary>
    /// Adds a field to the list of document fields that will be indexed.
    /// </summary>
    /// <param name="fieldName">The name of a field to index in all documents.</param>
    /// <param name="attributes">Optional attributes associated with this field.</param>
    public void Field(string fieldName, FieldAttributes attributes = null)
    {
        if(fieldName == null)
            throw new ArgumentNullException(nameof(fieldName));

        if(fieldName.Contains('/'))
        {
            throw new ArgumentException($"Field '{fieldName}' contains illegal character '/'");
        }

        _fields[fieldName] = attributes ?? new FieldAttributes();
    }

    /// <summary>
    /// Sets the b parameter for field length normalization.
    /// </summary>
    /// <param name="number">The value to set (clamped to 0-1).</param>
    public void B(double number)
    {
        if(number < 0)
        {
            _b = 0;
        }
        else if(number > 1)
        {
            _b = 1;
        }
        else
        {
            _b = number;
        }
    }

    /// <summary>
    /// Sets the k1 parameter for term frequency saturation.
    /// </summary>
    /// <param name="number">The value to set.</param>
    public void K1(double number)
    {
        _k1 = number;
    }

    /// <summary>
    /// Adds a document to the index.
    /// </summary>
    /// <param name="doc">The document to add to the index.</param>
    /// <param name="attributes">Optional attributes associated with this document.</param>
    public void Add(Dictionary<string, object> doc, DocumentAttributes attributes = null)
    {
        if(doc == null)
            throw new ArgumentNullException(nameof(doc));

        if(!doc.TryGetValue(_ref, out var docRefObj) || docRefObj == null)
        {
            throw new ArgumentException($"Document missing required field: {_ref}");
        }

        string docRef = docRefObj.ToString() ?? throw new ArgumentException("Document reference cannot be null");
        var fields = _fields.Keys.ToArray();

        _documents[docRef] = attributes ?? new DocumentAttributes();
        DocumentCount += 1;

        foreach(var fieldName in fields)
        {
            var extractor = _fields[fieldName].Extractor;
            var field = extractor != null ? extractor(doc) : (doc.TryGetValue(fieldName, out var f) ? f : null);

            var tokens = Tokenizer(field, new Dictionary<string, object> { ["fields"] = new[] { fieldName } });
            var terms = Pipeline.Run(tokens);
            var fieldRef = new FieldRef(docRef, fieldName);
            var fieldRefStr = fieldRef.ToString();
            var fieldTerms = new Dictionary<string, int>();

            FieldTermFrequencies[fieldRefStr] = fieldTerms;
            FieldLengths[fieldRefStr] = 0;
            FieldLengths[fieldRefStr] += terms.Length;

            foreach(var term in terms)
            {
                string termStr = term.ToString();

                if(!fieldTerms.ContainsKey(termStr))
                {
                    fieldTerms[termStr] = 0;
                }

                fieldTerms[termStr] += 1;

                if(!InvertedIndex.TryGetValue(termStr, out Dictionary<string, object> value))
                {
                    var posting = new Dictionary<string, object>
                    {
                        ["_index"] = TermIndex
                    };
                    TermIndex += 1;

                    foreach(var fieldKey in fields)
                    {
                        posting[fieldKey] = new Dictionary<string, Dictionary<string, object>>();
                    }

                    value = posting;
                    InvertedIndex[termStr] = value;
                }

                var fieldPosting = (Dictionary<string, Dictionary<string, object>>)value[fieldName];

                if(!fieldPosting.ContainsKey(docRef))
                {
                    fieldPosting[docRef] = [];
                }

                foreach(var metadataKey in MetadataWhitelist)
                {
                    if(term.Metadata.TryGetValue(metadataKey, out var metadata))
                    {
                        if(!fieldPosting[docRef].TryGetValue(metadataKey, out object value1))
                        {
                            value1 = new List<object>();
                            fieldPosting[docRef][metadataKey] = value1;
                        }

                        ((List<object>)value1).Add(metadata);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Calculates the average document length for this index.
    /// </summary>
    private void CalculateAverageFieldLengths()
    {
        var fieldRefs = FieldLengths.Keys.ToArray();
        var accumulator = new Dictionary<string, double>();
        var documentsWithField = new Dictionary<string, int>();

        foreach(var fieldRefStr in fieldRefs)
        {
            var fieldRef = FieldRef.FromString(fieldRefStr);
            string field = fieldRef.FieldName;

            if(!documentsWithField.ContainsKey(field))
            {
                documentsWithField[field] = 0;
            }
            documentsWithField[field] += 1;

            if(!accumulator.ContainsKey(field))
            {
                accumulator[field] = 0;
            }
            accumulator[field] += FieldLengths[fieldRefStr];
        }

        var fields = _fields.Keys.ToArray();
        foreach(var fieldName in fields)
        {
            accumulator[fieldName] = accumulator[fieldName] / documentsWithField[fieldName];
        }

        AverageFieldLength = accumulator;
    }

    /// <summary>
    /// Builds a vector space model of every document.
    /// </summary>
    private void CreateFieldVectors()
    {
        var fieldVectors = new Dictionary<string, Vector>();
        var fieldRefs = FieldTermFrequencies.Keys.ToArray();
        var termIdfCache = new Dictionary<string, double>();

        foreach(var fieldRefStr in fieldRefs)
        {
            var fieldRef = FieldRef.FromString(fieldRefStr);
            string fieldName = fieldRef.FieldName;
            int fieldLength = FieldLengths[fieldRefStr];
            var fieldVector = new Vector();
            var termFrequencies = FieldTermFrequencies[fieldRefStr];
            var terms = termFrequencies.Keys.ToArray();

            double fieldBoost = _fields[fieldName].Boost;
            double docBoost = _documents[fieldRef.DocRef].Boost;

            foreach(var term in terms)
            {
                int tf = termFrequencies[term];
                int termIndex = (int)InvertedIndex[term]["_index"];
                double idf;

                if(!termIdfCache.TryGetValue(term, out double value))
                {
                    idf = CalculateIdf(InvertedIndex[term], DocumentCount);
                    termIdfCache[term] = idf;
                }
                else
                {
                    idf = value;
                }

                double score = idf * ((_k1 + 1) * tf) / (_k1 * (1 - _b + _b * (fieldLength / AverageFieldLength[fieldName])) + tf);
                score *= fieldBoost;
                score *= docBoost;
                double scoreWithPrecision = Math.Round(score * 1000) / 1000.0;

                fieldVector.Insert(termIndex, scoreWithPrecision);
            }

            fieldVectors[fieldRefStr] = fieldVector;
        }

        FieldVectors = fieldVectors;
    }

    /// <summary>
    /// Creates a token set of all tokens in the index.
    /// </summary>
    /// <returns>The token set.</returns>
    private TokenSet CreateTokenSet()
    {
        return TokenSet.FromArray([.. InvertedIndex.Keys.OrderBy(k => k, StringComparer.Ordinal)]);
    }

    /// <summary>
    /// Builds the index.
    /// </summary>
    /// <returns>A new Index instance.</returns>
    public Index Build()
    {
        CalculateAverageFieldLengths();
        CreateFieldVectors();
        var tokenSet = CreateTokenSet();

        return new Index(
            InvertedIndex,
            FieldVectors,
            tokenSet,
            [.. _fields.Keys],
            SearchPipeline
        );
    }

    /// <summary>
    /// Applies a plugin to the index builder.
    /// </summary>
    /// <param name="plugin">The plugin to apply.</param>
    /// <param name="args">Additional arguments for the plugin.</param>
    public void Use(Action<Builder, object[]> plugin, params object[] args)
    {
        plugin?.Invoke(this, args);
    }

    // EFW - Moved here from Idf.cs since it was the only method and it's only used here
    /// <summary>
    /// Calculates the inverse document frequency for a posting.
    /// </summary>
    /// <param name="posting">The posting for a given term.</param>
    /// <param name="documentCount">The total number of documents.</param>
    /// <returns>The IDF value.</returns>
    public static double CalculateIdf(Dictionary<string, object> posting, int documentCount)
    {
        if(posting == null)
            throw new ArgumentNullException(nameof(posting));

        int documentsWithTerm = 0;

        foreach(var fieldName in posting.Keys)
        {
            if(fieldName == "_index")
                continue; // Ignore the term index, it's not a field

            if(posting[fieldName] is Dictionary<string, Dictionary<string, object>> fieldPosting)
            {
                documentsWithTerm += fieldPosting.Keys.Count;
            }
        }

        double x = (documentCount - documentsWithTerm + 0.5) / (documentsWithTerm + 0.5);
        return Math.Log(1 + Math.Abs(x));
    }
}
