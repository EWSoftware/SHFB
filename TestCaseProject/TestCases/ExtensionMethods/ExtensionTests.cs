using System;

namespace TestDoc.ExtensionMethods
{
    /// <summary>
    /// Test extension method documentation
    /// </summary>
    public static class ExtensionTests
    {
        /// <summary>
        /// Simple test of extension method documentation.  This shows up
        /// correctly.
        /// </summary>
        /// <param name="s">The string to modify</param>
        /// <param name="x">The string to append</param>
        /// <returns>s + x</returns>
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

        /// <summary>
        /// An extension for <see cref="TestClass"/>.
        /// </summary>
        /// <param name="testClass">A <see cref="TestClass"/> instance.</param>
        public static void ExtendClass(this TestClass testClass)
        {
        }

        /// <summary>
        /// An extension for <see cref="TestStruct"/>.
        /// </summary>
        /// <param name="testStruct">A <see cref="TestStruct"/> instance.</param>
        public static void ExtendStruct(this TestStruct testStruct)
        {
        }

        /// <summary>
        /// An extension for nullable <see cref="TestStruct"/>.
        /// </summary>
        /// <param name="testStruct">A <see cref="TestStruct"/> instance.</param>
        public static void ExtendNullableStruct(this TestStruct? testStruct)
        {
        }

        /// <summary>
        /// An extension for <see cref="TestStruct"/> by reference.
        /// </summary>
        /// <param name="testStruct">A <see cref="TestStruct"/> instance.</param>
        public static void ExtendStructByRef(this in TestStruct testStruct)
        {
        }

        /// <summary>
        /// An extension for <see cref="TestStruct"/> by reference.
        /// </summary>
        /// <param name="testStruct">A <see cref="TestStruct"/> instance.</param>
        public static void ExtendStructByActualRef(this ref TestStruct testStruct)
        {
        }

        /// <summary>
        /// An extension for a nullable <see cref="TestStruct"/> by reference.
        /// </summary>
        /// <param name="testStruct">A <see cref="TestStruct"/> instance.</param>
        public static void ExtendNullableStructByRef(this in TestStruct? testStruct)
        {
        }

        /// <summary>
        /// An extension for a nullable <see cref="TestStruct"/> by reference.
        /// </summary>
        /// <param name="testStruct">A <see cref="TestStruct"/> instance.</param>
        public static void ExtendNullableStructByActualRef(this ref TestStruct? testStruct)
        {
        }
    }
}
