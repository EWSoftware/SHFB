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
using System.Text;

namespace SandcastleBuilder.MSBuild.Lunr;

/// <summary>
/// A token set is used to store the unique list of all tokens within an index.
/// Token sets are also used to represent an incoming query to the index.
/// Token sets are implemented as a minimal finite state automata.
/// </summary>
public class TokenSet
{
    private static int _nextId = 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenSet"/> class.
    /// </summary>
    public TokenSet()
    {
        Final = false;
        Edges = [];
        Id = _nextId;
        _nextId += 1;
    }

    /// <summary>
    /// Gets or sets a value indicating whether this token set is final.
    /// </summary>
    public bool Final { get; set; }

    /// <summary>
    /// Gets the edges from this token set.
    /// </summary>
    public Dictionary<char, TokenSet> Edges { get; }

    /// <summary>
    /// Gets the unique identifier for this token set.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Creates a TokenSet instance from the given sorted array of words.
    /// </summary>
    /// <param name="arr">A sorted array of strings to create the set from.</param>
    /// <returns>A new TokenSet.</returns>
    public static TokenSet FromArray(string[] arr)
    {
        if(arr == null)
            throw new ArgumentNullException(nameof(arr));

        var builder = new TokenSetBuilder();

        for(int i = 0; i < arr.Length; i++)
        {
            builder.Insert(arr[i]);
        }

        builder.Finish();
        return builder.Root;
    }

    /// <summary>
    /// Creates a TokenSet from a string.
    /// The string may contain one or more wildcard characters (*).
    /// </summary>
    /// <param name="str">The string to create a TokenSet from.</param>
    /// <returns>A new TokenSet.</returns>
    public static TokenSet FromString(string str)
    {
        if(str == null)
            throw new ArgumentNullException(nameof(str));

        var node = new TokenSet();
        var root = node;

        for(int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            bool final = (i == str.Length - 1);

            if(c == '*')
            {
                node.Edges[c] = node;
                node.Final = final;
            }
            else
            {
                var next = new TokenSet { Final = final };
                node.Edges[c] = next;
                node = next;
            }
        }

        return root;
    }

    /// <summary>
    /// Converts this TokenSet into an array of strings contained within the TokenSet.
    /// </summary>
    /// <returns>An array of strings.</returns>
    public string[] ToArray()
    {
        var words = new List<string>();
        var stack = new Stack<(string prefix, TokenSet node)>();
        stack.Push(("", this));

        while(stack.Count > 0)
        {
            var frame = stack.Pop();
            var edges = frame.node.Edges.Keys.ToArray();

            if(frame.node.Final)
            {
                words.Add(frame.prefix);
            }

            foreach(var edge in edges)
            {
                stack.Push((frame.prefix + edge, frame.node.Edges[edge]));
            }
        }

        return [.. words];
    }

    /// <summary>
    /// Generates a string representation of a TokenSet.
    /// </summary>
    /// <returns>A string representation.</returns>
    public override string ToString()
    {
        var str = new StringBuilder(Final ? "1" : "0");
        var labels = Edges.Keys.OrderBy(k => k).ToArray();

        foreach(var label in labels)
        {
            var node = Edges[label];
            str.Append(label);
            str.Append(node.Id);
        }

        return str.ToString();
    }

    /// <summary>
    /// Returns a new TokenSet that is the intersection of this TokenSet and the passed TokenSet.
    /// </summary>
    /// <param name="b">Another TokenSet to intersect with.</param>
    /// <returns>The intersection TokenSet.</returns>
    public TokenSet Intersect(TokenSet b)
    {
        var output = new TokenSet();
        var stack = new Stack<(TokenSet qNode, TokenSet output, TokenSet node)>();
        stack.Push((b, output, this));

        while(stack.Count > 0)
        {
            var frame = stack.Pop();
            var qEdges = frame.qNode.Edges.Keys.ToArray();
            var nEdges = frame.node.Edges.Keys.ToArray();

            foreach(var qEdge in qEdges)
            {
                foreach(var nEdge in nEdges)
                {
                    if(nEdge == qEdge || qEdge == '*')
                    {
                        var node = frame.node.Edges[nEdge];
                        var qNode = frame.qNode.Edges[qEdge];
                        bool final = node.Final && qNode.Final;

                        TokenSet next;
                        if(frame.output.Edges.TryGetValue(nEdge, out TokenSet value))
                        {
                            next = value;
                            next.Final = next.Final || final;
                        }
                        else
                        {
                            next = new TokenSet { Final = final };
                            frame.output.Edges[nEdge] = next;
                        }

                        stack.Push((qNode, next, node));
                    }
                }
            }
        }

        return output;
    }
}
