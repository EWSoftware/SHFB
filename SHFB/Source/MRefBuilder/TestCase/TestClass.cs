//===============================================================================================================
// System  : Sandcastle MRefBuilder
// File    : TestClass.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/20/2014
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

using System.Runtime.CompilerServices;

namespace MyClassLibrary
{
    /// <summary>
    /// Test parameter attributes
    /// </summary>
    public class InteropAttributeTest
    {
        /// <summary>
        /// Test method parameter attributes
        /// </summary>
        /// <param name="callerMemberName">Caller member name</param>
        /// <param name="callerLineNumber">Caller line number</param>
        /// <param name="callerFilePath">Caller file path</param>
        public void TestMethodParameterAttributes([CallerMemberName]string callerMemberName = "",
            [CallerLineNumber]int callerLineNumber = 0, [CallerFilePath]string callerFilePath = "")
        {
        }
    }
}
