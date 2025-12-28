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

namespace SandcastleBuilder.MSBuild.Lunr;

/// <summary>
/// Builder for constructing minimized TokenSets.
/// </summary>
public class TokenSetBuilder
{
    private string _previousWord;
    private readonly List<UncheckedNode> _uncheckedNodes;
    private readonly Dictionary<string, TokenSet> _minimizedNodes;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenSetBuilder"/> class.
    /// </summary>
    public TokenSetBuilder()
    {
        _previousWord = "";
        Root = new TokenSet();
        _uncheckedNodes = [];
        _minimizedNodes = [];
    }

    /// <summary>
    /// Gets the root TokenSet.
    /// </summary>
    public TokenSet Root { get; }

    /// <summary>
    /// Inserts a word into the TokenSet.
    /// Words must be inserted in sorted order.
    /// </summary>
    /// <param name="word">The word to insert.</param>
    public void Insert(string word)
    {
        if(word == null)
            throw new ArgumentNullException(nameof(word));

        if(String.CompareOrdinal(word, _previousWord) < 0)
        {
            throw new InvalidOperationException("Out of order word insertion");
        }

        int commonPrefix = 0;
        for(int i = 0; i < word.Length && i < _previousWord.Length; i++)
        {
            if(word[i] != _previousWord[i])
                break;
            commonPrefix++;
        }

        Minimize(commonPrefix);

        TokenSet node;
        if(_uncheckedNodes.Count == 0)
        {
            node = Root;
        }
        else
        {
            node = _uncheckedNodes[_uncheckedNodes.Count - 1].Child;
        }

        for(int i = commonPrefix; i < word.Length; i++)
        {
            var nextNode = new TokenSet();
            char c = word[i];

            node.Edges[c] = nextNode;

            _uncheckedNodes.Add(new UncheckedNode
            {
                Parent = node,
                Char = c,
                Child = nextNode
            });

            node = nextNode;
        }

        node.Final = true;
        _previousWord = word;
    }

    /// <summary>
    /// Finalizes the TokenSet construction.
    /// </summary>
    public void Finish()
    {
        Minimize(0);
    }

    private void Minimize(int downTo)
    {
        for(int i = _uncheckedNodes.Count - 1; i >= downTo; i--)
        {
            var node = _uncheckedNodes[i];
            string childKey = node.Child.ToString();

            if(_minimizedNodes.TryGetValue(childKey, out TokenSet value))
            {
                node.Parent.Edges[node.Char] = value;
            }
            else
            {
                _minimizedNodes[childKey] = node.Child;
            }

            _uncheckedNodes.RemoveAt(i);
        }
    }

    private sealed class UncheckedNode
    {
        public TokenSet Parent { get; set; } = null!;
        public char Char { get; set; }
        public TokenSet Child { get; set; } = null!;
    }
}
