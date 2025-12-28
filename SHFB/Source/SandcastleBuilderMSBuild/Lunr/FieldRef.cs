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

namespace SandcastleBuilder.MSBuild.Lunr;

/// <summary>
/// A field reference identifies a field within a document by combining the document reference and field name.
/// </summary>
public class FieldRef
{
    /// <summary>
    /// The separator used to join field name and document reference.
    /// </summary>
    public const string Joiner = "/";

    private string _stringValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldRef"/> class.
    /// </summary>
    /// <param name="docRef">The document reference.</param>
    /// <param name="fieldName">The field name.</param>
    /// <param name="stringValue">Optional pre-computed string value.</param>
    public FieldRef(string docRef, string fieldName, string stringValue = null)
    {
        DocRef = docRef;
        FieldName = fieldName;
        _stringValue = stringValue;
    }

    /// <summary>
    /// Gets the document reference.
    /// </summary>
    public string DocRef { get; }

    /// <summary>
    /// Gets the field name.
    /// </summary>
    public string FieldName { get; }

    /// <summary>
    /// Creates a FieldRef from a string representation.
    /// </summary>
    /// <param name="s">The string representation.</param>
    /// <returns>A new FieldRef instance.</returns>
    public static FieldRef FromString(string s)
    {
        if(s == null)
            throw new ArgumentNullException(nameof(s));

        int n = s.IndexOf(Joiner, StringComparison.Ordinal);

        if(n == -1)
        {
            throw new ArgumentException("malformed field ref string");
        }

        string fieldRef = s.Substring(0, n);
        string docRef = s.Substring(n + 1);

        return new FieldRef(docRef, fieldRef, s);
    }

    /// <summary>
    /// Returns the string representation of the field reference.
    /// </summary>
    /// <returns>The string representation.</returns>
    public override string ToString()
    {
        _stringValue ??= FieldName + Joiner + DocRef;

        return _stringValue;
    }

    /// <summary>
    /// Gets the hash code for this field reference.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    /// <summary>
    /// Determines whether the specified object is equal to this field reference.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns>True if equal, otherwise false.</returns>
    public override bool Equals(object obj)
    {
        if(obj is FieldRef other)
        {
            return ToString() == other.ToString();
        }
        return false;
    }
}
