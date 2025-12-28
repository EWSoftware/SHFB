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
/// A vector is used to construct the vector space of documents and queries. These
/// vectors support operations to determine the similarity between two documents or
/// a document and a query.
/// </summary>
public class Vector
{
    private double _magnitude;
    private readonly List<double> _elements;

    /// <summary>
    /// Initializes a new instance of the <see cref="Vector"/> class.
    /// </summary>
    /// <param name="elements">The flat list of element index and element value pairs.</param>
    public Vector(IEnumerable<double> elements = null)
    {
        _magnitude = 0;
        _elements = elements?.ToList() ?? [];
    }

    /// <summary>
    /// Gets the elements of this vector.
    /// </summary>
    public IReadOnlyList<double> Elements => _elements.AsReadOnly();

    /// <summary>
    /// Calculates the position within the vector to insert a given index.
    /// </summary>
    /// <param name="index">The index at which the element should be inserted.</param>
    /// <returns>The position for the index.</returns>
    public int PositionForIndex(int index)
    {
        if(_elements.Count == 0)
        {
            return 0;
        }

        int start = 0;
        int end = _elements.Count / 2;
        int sliceLength = end - start;
        int pivotPoint = sliceLength / 2;
        int pivotIndex = (int)_elements[pivotPoint * 2];

        while(sliceLength > 1)
        {
            if(pivotIndex < index)
            {
                start = pivotPoint;
            }

            if(pivotIndex > index)
            {
                end = pivotPoint;
            }

            if(pivotIndex == index)
            {
                break;
            }

            sliceLength = end - start;
            pivotPoint = start + sliceLength / 2;
            pivotIndex = (int)_elements[pivotPoint * 2];
        }

        if(pivotIndex == index)
        {
            return pivotPoint * 2;
        }

        if(pivotIndex > index)
        {
            return pivotPoint * 2;
        }

        return (pivotPoint + 1) * 2;
    }

    /// <summary>
    /// Inserts an element at an index within the vector.
    /// Does not allow duplicates, will throw an error if there is already an entry for this index.
    /// </summary>
    /// <param name="insertIdx">The index at which the element should be inserted.</param>
    /// <param name="val">The value to be inserted into the vector.</param>
    public void Insert(int insertIdx, double val)
    {
        Upsert(insertIdx, val, (a, b) => throw new InvalidOperationException("duplicate index"));
    }

    /// <summary>
    /// Inserts or updates an existing index within the vector.
    /// </summary>
    /// <param name="insertIdx">The index at which the element should be inserted.</param>
    /// <param name="val">The value to be inserted into the vector.</param>
    /// <param name="fn">A function that is called for updates, the existing value and the requested value are passed as arguments.</param>
    public void Upsert(int insertIdx, double val, Func<double, double, double> fn)
    {
        if(fn == null)
            throw new ArgumentNullException(nameof(fn));

        _magnitude = 0;
        int position = PositionForIndex(insertIdx);

        if(position < _elements.Count && _elements[position] == insertIdx)
        {
            _elements[position + 1] = fn(_elements[position + 1], val);
        }
        else
        {
            _elements.Insert(position, insertIdx);
            _elements.Insert(position + 1, val);
        }
    }

    /// <summary>
    /// Calculates the magnitude of this vector.
    /// </summary>
    /// <returns>The magnitude.</returns>
    public double Magnitude()
    {
        if(_magnitude != 0)
            return _magnitude;

        double sumOfSquares = 0;
        int elementsLength = _elements.Count;

        for(int i = 1; i < elementsLength; i += 2)
        {
            double val = _elements[i];
            sumOfSquares += val * val;
        }

        return _magnitude = Math.Sqrt(sumOfSquares);
    }

    /// <summary>
    /// Calculates the dot product of this vector and another vector.
    /// </summary>
    /// <param name="otherVector">The vector to compute the dot product with.</param>
    /// <returns>The dot product.</returns>
    public double Dot(Vector otherVector)
    {
        if(otherVector == null)
            throw new ArgumentNullException(nameof(otherVector));

        double dotProduct = 0;
        var a = _elements;
        var b = otherVector._elements;
        int aLen = a.Count;
        int bLen = b.Count;
        int i = 0;
        int j = 0;

        while(i < aLen && j < bLen)
        {
            double aVal = a[i];
            double bVal = b[j];

            if(aVal < bVal)
            {
                i += 2;
            }
            else if(aVal > bVal)
            {
                j += 2;
            }
            else // aVal == bVal
            {
                dotProduct += a[i + 1] * b[j + 1];
                i += 2;
                j += 2;
            }
        }

        return dotProduct;
    }

    /// <summary>
    /// Calculates the similarity between this vector and another vector.
    /// </summary>
    /// <param name="otherVector">The other vector to calculate the similarity with.</param>
    /// <returns>The similarity score.</returns>
    public double Similarity(Vector otherVector)
    {
        double magnitude = Magnitude();
        return magnitude != 0 ? Dot(otherVector) / magnitude : 0;
    }

    /// <summary>
    /// Converts the vector to an array of the values within the vector.
    /// </summary>
    /// <returns>An array of values.</returns>
    public double[] ToArray()
    {
        var output = new double[_elements.Count / 2];

        for(int i = 1, j = 0; i < _elements.Count; i += 2, j++)
        {
            output[j] = _elements[i];
        }

        return output;
    }

    /// <summary>
    /// Gets a JSON serializable representation of the vector.
    /// </summary>
    /// <returns>The elements array.</returns>
    public List<double> ToJson()
    {
        return _elements;
    }
}
