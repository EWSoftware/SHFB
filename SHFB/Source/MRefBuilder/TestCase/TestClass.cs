//===============================================================================================================
// System  : Sandcastle MRefBuilder
// File    : TestClass.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/04/2013
// Compiler: Microsoft Visual C#
//
// This class is used to test small snippets of code with MRefBuilder to diagnose problems
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.   This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.0.0.0  02/01/2012  EFW  Created the code
//===============================================================================================================

// For debugging:
// /out:reflection.org C:\CP\TFS01\SHFB\SHFB\Source\MRefBuilder\TestCase\bin\Debug\TestCase.dll

using System;

namespace MyClassLibrary
{
    /// <summary>
    /// MyBaseClass
    /// </summary>
    /// <typeparam name="T">t</typeparam>
    /// <typeparam name="U">u</typeparam>
    public class MyBaseClass<T, U>
    {
        /// <summary>
        /// Test constant
        /// </summary>
        public const int TestConstant = 1234;

        /// <summary>
        /// Test field
        /// </summary>
        public int TestField = 1234;

        /// <summary>
        /// Blah
        /// </summary>
        /// <param name="t">t</param>
        /// <param name="u">u</param>
        public void Blah(T t, U u)
        {

        }

        /// <summary>
        /// Blah
        /// </summary>
        /// <param name="t">t</param>
        /// <param name="u">u</param>
        /// <param name="i">i</param>
        public void Blah(T t, U u, int i)
        {

        }
    }

    /// <summary>
    /// MyChildClass
    /// </summary>
    /// <typeparam name="T">t</typeparam>
    public class MyChildClass<T> : MyBaseClass<T, T>
    {

    }
}
