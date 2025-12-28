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

namespace SandcastleBuilder.MSBuild.Lunr.Languages;

/// <summary>
/// Base class for Snowball stemmer implementations.
/// Provides common functionality for stemming algorithms.
/// </summary>
public class SnowballProgram
{
    private string _current = "";

    /// <summary>
    /// Gets or sets the bra cursor position.
    /// </summary>
    public int Bra { get; set; }

    /// <summary>
    /// Gets or sets the ket cursor position.
    /// </summary>
    public int Ket { get; set; }

    /// <summary>
    /// Gets or sets the limit position.
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// Gets or sets the cursor position.
    /// </summary>
    public int Cursor { get; set; }

    /// <summary>
    /// Gets or sets the limit backward position.
    /// </summary>
    public int LimitBackward { get; set; }

    /// <summary>
    /// Sets the current word to be stemmed.
    /// </summary>
    /// <param name="word">The word to stem.</param>
    public void SetCurrent(string word)
    {
        _current = word ?? throw new ArgumentNullException(nameof(word));
        Cursor = 0;
        Limit = word.Length;
        LimitBackward = 0;
        Bra = Cursor;
        Ket = Limit;
    }

    /// <summary>
    /// Gets the current word being stemmed.
    /// </summary>
    /// <returns>The current word.</returns>
    public string Current => _current;

    /// <summary>
    /// Checks if the cursor is in the specified grouping.
    /// </summary>
    public bool InGrouping(int[] s, int min, int max)
    {
        if(s == null)
            throw new ArgumentNullException(nameof(s));

        if(Cursor < Limit)
        {
            int ch = _current[Cursor];
            if(ch <= max && ch >= min)
            {
                ch -= min;
                if((s[ch >> 3] & (0x1 << (ch & 0x7))) != 0)
                {
                    Cursor++;
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the cursor is in the specified grouping (backward).
    /// </summary>
    public bool InGroupingB(int[] s, int min, int max)
    {
        if(s == null)
            throw new ArgumentNullException(nameof(s));

        if(Cursor > LimitBackward)
        {
            int ch = _current[Cursor - 1];
            if(ch <= max && ch >= min)
            {
                ch -= min;
                if((s[ch >> 3] & (0x1 << (ch & 0x7))) != 0)
                {
                    Cursor--;
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the cursor is outside the specified grouping.
    /// </summary>
    public bool OutGrouping(int[] s, int min, int max)
    {
        if(s == null)
            throw new ArgumentNullException(nameof(s));

        if(Cursor < Limit)
        {
            int ch = _current[Cursor];
            if(ch > max || ch < min)
            {
                Cursor++;
                return true;
            }
            ch -= min;
            if((s[ch >> 3] & (0x1 << (ch & 0x7))) == 0)
            {
                Cursor++;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the cursor is outside the specified grouping (backward).
    /// </summary>
    public bool OutGroupingB(int[] s, int min, int max)
    {
        if(s == null)
            throw new ArgumentNullException(nameof(s));

        if(Cursor > LimitBackward)
        {
            int ch = _current[Cursor - 1];
            if(ch > max || ch < min)
            {
                Cursor--;
                return true;
            }
            ch -= min;
            if((s[ch >> 3] & (0x1 << (ch & 0x7))) == 0)
            {
                Cursor--;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the string equals the specified substring.
    /// </summary>
    public bool EqS(int sSize, string s)
    {
        if(s == null)
            throw new ArgumentNullException(nameof(s));

        if(Limit - Cursor < sSize)
            return false;

        for(int i = 0; i < sSize; i++)
        {
            if(_current[Cursor + i] != s[i])
                return false;
        }

        Cursor += sSize;
        return true;
    }

    /// <summary>
    /// Checks if the string equals the specified substring (backward).
    /// </summary>
    public bool EqSB(int sSize, string s)
    {
        if(s == null)
            throw new ArgumentNullException(nameof(s));

        if(Cursor - LimitBackward < sSize)
            return false;

        for(int i = 0; i < sSize; i++)
        {
            if(_current[Cursor - sSize + i] != s[i])
                return false;
        }

        Cursor -= sSize;
        return true;
    }

    /// <summary>
    /// Finds among a set of strings.
    /// </summary>
    public int FindAmong(Among[] v, int vSize)
    {
        int i = 0;
        int j = vSize;
        int c = Cursor;
        int l = Limit;
        int commonI = 0;
        int commonJ = 0;
        bool firstKeyInspected = false;

        if(v == null)
            throw new ArgumentNullException(nameof(v));

        while(true)
        {
            int k = i + ((j - i) >> 1);
            int diff = 0;
            int common = commonI < commonJ ? commonI : commonJ;
            Among w = v[k];

            for(int i2 = common; i2 < w.SSize; i2++)
            {
                if(c + common == l)
                {
                    diff = -1;
                    break;
                }
                diff = _current[c + common] - w.S[i2];
                if(diff != 0)
                    break;
                common++;
            }

            if(diff < 0)
            {
                j = k;
                commonJ = common;
            }
            else
            {
                i = k;
                commonI = common;
            }

            if(j - i <= 1)
            {
                if(i > 0 || j == i || firstKeyInspected)
                    break;
                firstKeyInspected = true;
            }
        }

        while(true)
        {
            Among w = v[i];
            if(commonI >= w.SSize)
            {
                Cursor = c + w.SSize;
                if(w.Method == null)
                    return w.Result;

                bool res = w.Method();
                Cursor = c + w.SSize;
                if(res)
                    return w.Result;
            }
            i = w.SubstringIndex;
            if(i < 0)
                return 0;
        }
    }

    /// <summary>
    /// Finds among a set of strings (backward).
    /// </summary>
    public int FindAmongB(Among[] v, int vSize)
    {
        if(v == null)
            throw new ArgumentNullException(nameof(v));

        int i = 0;
        int j = vSize;
        int c = Cursor;
        int lb = LimitBackward;
        int commonI = 0;
        int commonJ = 0;
        bool firstKeyInspected = false;

        while(true)
        {
            int k = i + ((j - i) >> 1);
            int diff = 0;
            int common = commonI < commonJ ? commonI : commonJ;
            Among w = v[k];

            for(int i2 = w.SSize - 1 - common; i2 >= 0; i2--)
            {
                if(c - common == lb)
                {
                    diff = -1;
                    break;
                }
                diff = _current[c - 1 - common] - w.S[i2];
                if(diff != 0)
                    break;
                common++;
            }

            if(diff < 0)
            {
                j = k;
                commonJ = common;
            }
            else
            {
                i = k;
                commonI = common;
            }

            if(j - i <= 1)
            {
                if(i > 0 || j == i || firstKeyInspected)
                    break;
                firstKeyInspected = true;
            }
        }

        while(true)
        {
            Among w = v[i];
            if(commonI >= w.SSize)
            {
                Cursor = c - w.SSize;
                if(w.Method == null)
                    return w.Result;

                bool res = w.Method();
                Cursor = c - w.SSize;
                if(res)
                    return w.Result;
            }
            i = w.SubstringIndex;
            if(i < 0)
                return 0;
        }
    }

    /// <summary>
    /// Replaces a substring.
    /// </summary>
    public int ReplaceS(int cBra, int cKet, string s)
    {
        if(s == null)
            throw new ArgumentNullException(nameof(s));

        int adjustment = s.Length - (cKet - cBra);
        string left = _current.Substring(0, cBra);
        string right = _current.Substring(cKet);
        _current = left + s + right;
        Limit += adjustment;

        if(Cursor >= cKet)
            Cursor += adjustment;
        else if(Cursor > cBra)
            Cursor = cBra;

        return adjustment;
    }

    /// <summary>
    /// Checks the slice bounds.
    /// </summary>
    public void SliceCheck()
    {
        if(Bra < 0 || Bra > Ket || Ket > Limit || Limit > _current.Length)
        {
            throw new InvalidOperationException("faulty slice operation");
        }
    }

    /// <summary>
    /// Replaces the slice with the specified string.
    /// </summary>
    public void SliceFrom(string s)
    {
        SliceCheck();
        ReplaceS(Bra, Ket, s);
    }

    /// <summary>
    /// Deletes the slice.
    /// </summary>
    public void SliceDel()
    {
        SliceFrom("");
    }

    /// <summary>
    /// Inserts a string at the specified position.
    /// </summary>
    public void Insert(int cBra, int cKet, string s)
    {
        int adjustment = ReplaceS(cBra, cKet, s);
        if(cBra <= Bra)
            Bra += adjustment;
        if(cBra <= Ket)
            Ket += adjustment;
    }

    /// <summary>
    /// Gets the current slice.
    /// </summary>
    public string SliceTo()
    {
        SliceCheck();
        return _current.Substring(Bra, Ket - Bra);
    }

    /// <summary>
    /// Checks if the slice equals the specified string (backward).
    /// </summary>
    public bool EqVB(string s)
    {
        if(s == null)
            throw new ArgumentNullException(nameof(s));

        return EqSB(s.Length, s);
    }
}
