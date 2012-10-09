using System;
using System.Collections.Generic;

namespace TestDoc.ExtensionMethods
{
    /// <summary>
    /// Test extension method documentation
    /// </summary>
    public static class ExtensionTest
    {
        /// <summary>
        /// Simple test of extension method documentation.  This shows up
        /// correctly.
        /// </summary>
        /// <param name="s">The string to modify</param>
        /// <param name="x">The string to append</param>
        public static string ExtendTest(this string s, string x)
        {
            return s + x;
        }

        /// <summary>
        /// As of the June 2010 release, this method doesn't show up as a
        /// member of the class.  In addition, it causes an empty Extension
        /// Methods section to appear in every class member list page.  The
        /// apparent cause is that it extends type Object.
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>The string value or null if null or an empty string</returns>
        public static string ToStringOrNull(this object value)
        {
            if(value == null || value == DBNull.Value)
                return null;

            string s = value.ToString();

            return (s.Length == 0) ? null : s;
        }
    }
}
