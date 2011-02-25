//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : EscapeValueAttribute.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/28/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains an attribute used to mark properties that need their
// value escaped when stored in an MSBuild project file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  06/21/2008  EFW  Created the code
//=============================================================================

using System;
using System.Text.RegularExpressions;
using System.Web;

using Microsoft.Build.BuildEngine;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This is used to mark a property that needs its value escaped when
    /// stored in an MSBuild project file.
    /// </summary>
    /// <remarks>MSBuild requires that the following characters be escaped in
    /// property values unless they are intended to be interpreted by the
    /// build engine:  % * ? @ $ ( ) ; '.  In addition, this attribute will
    /// cause the values to be HTML encoded so that any HTML characters,
    /// especially tag delimiters are not interpreted.  MSBuild tends to add
    /// XML namespaces to things it thinks are XML elements.</remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EscapeValueAttribute : Attribute
    {
        private static Regex reUnescape = new Regex("%[0-9a-f]{2}",
            RegexOptions.IgnoreCase);

        private static MatchEvaluator onEscapeMatch = new MatchEvaluator(
            OnEscapeMatch);

        /// <summary>
        /// This is used to HTML encode and escape an MSBuild property value.
        /// </summary>
        /// <param name="unescapedValue">The unescaped value</param>
        /// <returns>The HTML encoded escaped value</returns>
        public static string Escape(string unescapedValue)
        {
            string escaped = HttpUtility.HtmlEncode(unescapedValue);
            return Utilities.Escape(escaped);
        }

        /// <summary>
        /// This is used to return an unescaped and HTML decoded MSBuild value
        /// </summary>
        /// <param name="escapedValue">The escaped value</param>
        /// <returns>The unescaped and HTML decoded value</returns>
        /// <remarks>MSBuild provides an escape method but no unescape method.
        /// Go figure.</remarks>
        public static string Unescape(string escapedValue)
        {
            string unescaped = reUnescape.Replace(escapedValue, onEscapeMatch);
            return HttpUtility.HtmlDecode(unescaped);
        }

        /// <summary>
        /// This is used to convert the match to its unescaped character
        /// </summary>
        /// <param name="m">The match</param>
        /// <returns>The unescaped character as a string</returns>
        private static string OnEscapeMatch(Match m)
        {
            int high = HexToInt(m.Value[1]), low = HexToInt(m.Value[2]);

            if(high >=0 && low >= 0)
                return new String((char)((high << 4) | low), 1);

            return m.Value;
        }

        /// <summary>
        /// Convert a hex character to its integer value
        /// </summary>
        /// <param name="h">The hex character</param>
        /// <returns>The integer value</returns>
        private static int HexToInt(char h)
        {
            if((h >= '0') && (h <= '9'))
                return (h - '0');

            if((h >= 'a') && (h <= 'f'))
                return ((h - 'a') + 10);

            if((h >= 'A') && (h <= 'F'))
                return ((h - 'A') + 10);

            return -1;
        }
    }
}
