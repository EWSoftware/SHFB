using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetStandardTestCases
{
    /// <summary>
    /// Extension method test
    /// </summary>
    public static class ExtensionTests
    {
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
