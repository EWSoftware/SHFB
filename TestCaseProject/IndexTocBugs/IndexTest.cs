using System;
using System.ComponentModel;

namespace TestDoc.IndexTocBugs
{
    /// <summary>
    /// Test for duplicate/incorrect entries in CHM index file
    /// </summary>
    public class IndexTest
    {
        /// <summary>
        /// A property.  See <see cref="MethodA(String)">Method A (&lt;see&gt;
        /// inner text test)</see>. See <see cref="MethodB">IndexText.Method B</see>.
        /// </summary>
        /// <seealso cref="MethodB">Method B (&lt;see&gt; inner text
        /// test)</seealso>
        /// <seealso cref="PropertyB">Method B (and another)</seealso>
        public string PropertyA { get { return null; } }

        /// <inheritdoc />
        [Description("This is the description for PropertyB")]
        public float PropertyB { get { return 0F; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <example>
        /// Test language ID mapping.
        /// 
        /// <code lang="C#">
        /// // C#
        /// public void Test(string a)
        /// {
        ///     int x;
        /// 
        ///     Console.WriteLine("Test");
        /// }
        /// </code>
        /// <code lang="J#">
        /// // J#
        /// public void Test(string a)
        /// {
        ///     int x;
        /// 
        ///     Console.WriteLine("Test");
        /// }
        /// </code>
        /// <code lang="c#">
        /// // c#
        /// public void Test(string a)
        /// {
        ///     int x;
        /// 
        ///     Console.WriteLine("Test");
        /// }
        /// </code>
        /// <code lang="j#">
        /// // j#
        /// public void Test(string a)
        /// {
        ///     int x;
        /// 
        ///     Console.WriteLine("Test");
        /// }
        /// </code>
        /// </example>
        public IndexTest() { }

        /// <summary>
        /// A method
        /// </summary>
        /// <param name="o"></param>
        public IndexTest(object o) { }

        /// <summary>
        /// An overloaded method
        /// </summary>
        /// <param name="s"></param>
        public void MethodA(string s) { }

        /// <summary>
        /// Another overload
        /// </summary>
        /// <param name="i"></param>
        public void MethodA(int i) { }

        /// <summary>
        /// Another method
        /// </summary>
        /// <remarks>See <see cref="TestArrayMethod(string[])"/> for details</remarks>
        public void MethodB() { }

        /// <summary>
        /// Test method with array parameter
        /// </summary>
        /// <param name="args">A string array</param>
        /// <remarks>The indexer is <see cref="this[int]"/>.</remarks>
        public void TestArrayMethod(string[] args)
        {
        }

        /// <summary>
        /// Item indexer
        /// </summary>
        /// <param name="idx">The index to return</param>
        /// <returns>Test property.  Always returns 0</returns>
        public int this[int idx]
        {
            get { return 0; }
        }
    }

}

