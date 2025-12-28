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
/// A token wraps a string representation of a token as it is passed through the text processing pipeline.
/// </summary>
public class Token
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Token"/> class.
    /// </summary>
    /// <param name="str">The string token being wrapped.</param>
    /// <param name="metadata">Metadata associated with this token.</param>
    public Token(string str = "", Dictionary<string, object> metadata = null)
    {
        Str = str ?? "";
        Metadata = metadata ?? [];
    }

    /// <summary>
    /// Gets or sets the string representation of the token.
    /// </summary>
    public string Str { get; set; }

    /// <summary>
    /// Gets the metadata associated with this token.
    /// </summary>
    public Dictionary<string, object> Metadata { get; }

    /// <summary>
    /// Returns the token string that is being wrapped by this object.
    /// </summary>
    /// <returns>The string representation.</returns>
    public override string ToString()
    {
        return Str;
    }

    /// <summary>
    /// Applies the given function to the wrapped string token.
    /// </summary>
    /// <param name="fn">A function to apply to the token string.</param>
    /// <returns>This token instance.</returns>
    public Token Update(Func<string, Dictionary<string, object>, string> fn)
    {
        if(fn == null)
            throw new ArgumentNullException(nameof(fn));

        Str = fn(Str, Metadata);
        return this;
    }

    /// <summary>
    /// Creates a clone of this token. Optionally a function can be applied to the cloned token.
    /// </summary>
    /// <param name="fn">An optional function to apply to the cloned token.</param>
    /// <returns>A cloned token.</returns>
    public Token Clone(Func<string, Dictionary<string, object>, string> fn = null)
    {
        fn ??= (s, _) => s;
        return new Token(fn(Str, Metadata), new Dictionary<string, object>(Metadata));
    }
}
