//===============================================================================================================
// System  : Sandcastle MRefBuilder
// File    : TestClass.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/20/2014
// Compiler: Microsoft Visual C#
//
// This class is used to test small snippets of code with MRefBuilder to diagnose problems
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.0.0.0  02/01/2012  EFW  Created the code
//===============================================================================================================

// For debugging:
// Command line arguments: /out:reflection.org C:\GH\SHFB\SHFB\Source\MRefBuilder\TestCase\bin\Debug\TestCase.dll
// Working directory: C:\GH\SHFB\SHFB\Deploy\

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
