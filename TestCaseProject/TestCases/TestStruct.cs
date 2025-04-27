using System;
using System.Diagnostics.CodeAnalysis;

using TestDoc.ExtensionMethods;

namespace TestDoc
{
    /// <summary>
    /// Docs go here.
    /// </summary>
    [Obsolete("This type is obsolete")]
    public ref struct TestRefStruct
    {
    }

    /// <summary>
    /// Docs go here.
    /// </summary>
    [Experimental("TEST0001")]
    public readonly ref struct TestReadOnlyRefStruct
    {
    }

    /// <summary>
    /// Test structure for extension method tests
    /// </summary>
    public struct TestStruct
    {
        /// <summary>
        /// X value
        /// </summary>
        /// <preliminary/>
        public int X { get; set; }

        /// <summary>
        /// Y value
        /// </summary>
        public int Y { get; set; }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="y">Y value</param>
        [Experimental("TEST0002")]
        public TestStruct(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Test method
        /// </summary>
        [Obsolete("This method is obsolete")]
        public void TestMethod()
        {
            this.ExtendStruct();
            this.ExtendStructByRef();

            TestStruct? other = null;

            other.ExtendNullableStruct();
            other.ExtendNullableStructByRef();
        }
    }
}
