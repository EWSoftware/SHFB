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
        /// Simple test of extension method documentation
        /// </summary>
        /// <param name="s">The string to modify</param>
        /// <param name="x">The string to append</param>
        public static string ExtendTest(this string s, string x)
        {
            return s + x;
        }

        /// <summary>Test this:
        /// <para><see cref="Predicate{T}"/></para>
        /// <para><see cref="List{T}"/></para>
        /// <para><see cref="List{T}.Clear()"/></para>
        /// <para><see cref="List{T}.RemoveAll(Predicate{T})"/></para>
        /// </summary>
        public static void Test()
        {
        }
    }
}
