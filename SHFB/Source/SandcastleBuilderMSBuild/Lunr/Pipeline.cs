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

using System;
using System.Collections.Generic;
using System.Linq;

namespace SandcastleBuilder.MSBuild.Lunr;

/// <summary>
/// Delegate for pipeline functions that process tokens.
/// </summary>
/// <param name="token">The token to process.</param>
/// <param name="index">The index of this token in the complete list of tokens.</param>
/// <param name="tokens">All tokens for this document/field.</param>
/// <returns>The processed token(s), or null to exclude the token.</returns>
public delegate object PipelineFunction(Token token, int index, Token[] tokens);

/// <summary>
/// Pipelines maintain an ordered list of functions to be applied to all tokens
/// in documents entering the search index.
/// </summary>
public class Pipeline
{
    private static readonly Dictionary<string, PipelineFunction> RegisteredFunctions = [];
    private readonly List<PipelineFunction> _stack;

    /// <summary>
    /// Initializes a new instance of the <see cref="Pipeline"/> class.
    /// </summary>
    public Pipeline()
    {
        _stack = [];
    }

    /// <summary>
    /// Registers a function with the pipeline for serialization purposes.
    /// </summary>
    /// <param name="fn">The function to register.</param>
    /// <param name="label">The label to register this function with.</param>
    public static void RegisterFunction(PipelineFunction fn, string label)
    {
        if(RegisteredFunctions.ContainsKey(label))
        {
            System.Diagnostics.Debug.WriteLine($"Warning: Overwriting existing registered function: {label}");
        }

        RegisteredFunctions[label] = fn;
    }

    /// <summary>
    /// Loads a previously serialized pipeline.
    /// </summary>
    /// <param name="serialized">The serialized pipeline function names.</param>
    /// <returns>A new Pipeline instance.</returns>
    public static Pipeline Load(string[] serialized)
    {
        if(serialized == null)
            throw new ArgumentNullException(nameof(serialized));

        var pipeline = new Pipeline();

        foreach(var fnName in serialized)
        {
            if(RegisteredFunctions.TryGetValue(fnName, out var fn))
            {
                pipeline.Add(fn);
            }
            else
            {
                throw new InvalidOperationException($"Cannot load unregistered function: {fnName}");
            }
        }

        return pipeline;
    }

    /// <summary>
    /// Adds new functions to the end of the pipeline.
    /// </summary>
    /// <param name="functions">Functions to add to the pipeline.</param>
    public void Add(params PipelineFunction[] functions)
    {
        if(functions == null)
            throw new ArgumentNullException(nameof(functions));

        foreach(var fn in functions)
        {
            _stack.Add(fn);
        }
    }

    /// <summary>
    /// Adds a single function after a function that already exists in the pipeline.
    /// </summary>
    /// <param name="existingFn">A function that already exists in the pipeline.</param>
    /// <param name="newFn">The new function to add to the pipeline.</param>
    public void After(PipelineFunction existingFn, PipelineFunction newFn)
    {
        int pos = _stack.IndexOf(existingFn);
        if(pos == -1)
        {
            throw new InvalidOperationException("Cannot find existingFn");
        }

        _stack.Insert(pos + 1, newFn);
    }

    /// <summary>
    /// Adds a single function before a function that already exists in the pipeline.
    /// </summary>
    /// <param name="existingFn">A function that already exists in the pipeline.</param>
    /// <param name="newFn">The new function to add to the pipeline.</param>
    public void Before(PipelineFunction existingFn, PipelineFunction newFn)
    {
        int pos = _stack.IndexOf(existingFn);
        if(pos == -1)
        {
            throw new InvalidOperationException("Cannot find existingFn");
        }

        _stack.Insert(pos, newFn);
    }

    /// <summary>
    /// Removes a function from the pipeline.
    /// </summary>
    /// <param name="fn">The function to remove from the pipeline.</param>
    public void Remove(PipelineFunction fn)
    {
        _stack.Remove(fn);
    }

    /// <summary>
    /// Runs the current list of functions that make up the pipeline against the passed tokens.
    /// </summary>
    /// <param name="tokens">The tokens to run through the pipeline.</param>
    /// <returns>Processed tokens.</returns>
    public Token[] Run(Token[] tokens)
    {
        var currentTokens = tokens.ToList();

        foreach(var fn in _stack)
        {
            var memo = new List<Token>();

            for(int j = 0; j < currentTokens.Count; j++)
            {
                var result = fn(currentTokens[j], j, tokens);

                if(result == null || (result is string str && String.IsNullOrEmpty(str)))
                {
                    continue;
                }

                if(result is Token[] tokenArray)
                {
                    memo.AddRange(tokenArray);
                }
                else if(result is Token token)
                {
                    memo.Add(token);
                }
            }

            currentTokens = memo;
        }

        return [.. currentTokens];
    }

    /// <summary>
    /// Convenience method for passing a string through a pipeline and getting strings out.
    /// </summary>
    /// <param name="str">The string to pass through the pipeline.</param>
    /// <param name="metadata">Optional metadata to associate with the token.</param>
    /// <returns>Processed strings.</returns>
    public string[] RunString(string str, Dictionary<string, object> metadata = null)
    {
        var token = new Token(str, metadata);
        return [.. Run([token]).Select(t => t.ToString())];
    }

    /// <summary>
    /// Resets the pipeline by removing any existing processors.
    /// </summary>
    public void Reset()
    {
        _stack.Clear();
    }

    /// <summary>
    /// Returns a representation of the pipeline ready for serialization.
    /// </summary>
    /// <returns>An array of function labels.</returns>
    public string[] ToJson()
    {
        var labels = new List<string>();

        foreach(var fn in _stack)
        {
            var label = RegisteredFunctions.FirstOrDefault(x => x.Value == fn).Key;
            if(label != null)
            {
                labels.Add(label);
            }
        }

        return [.. labels];
    }
}
