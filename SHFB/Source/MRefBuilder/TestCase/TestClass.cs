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
/*
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
    }*/

    /// <summary>
    /// Test
    /// </summary>
    internal interface IValidatable
    {
        /// <summary>
        /// Test
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>True or false</returns>
        bool IsValid(object value);
    }

    /// <summary>
    /// Test
    /// </summary>
    public interface IUpdateable
    {
        /// <summary>
        /// Test
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>True or false</returns>
        bool IsUpdateable(object value);
    }

    /// <summary>
    /// Test
    /// </summary>
    public class NotificationDelivery : IValidatable, IUpdateable
    {
        #region IValidatable Members

        /// <summary>
        /// Test
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsValid(object value)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region IUpdateable Members

        /// <summary>
        /// Test
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsUpdateable(object value)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
