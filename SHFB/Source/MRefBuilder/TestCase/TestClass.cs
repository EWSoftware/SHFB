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
/*    /// <summary>
    /// Summary
    /// </summary>
    /// <typeparam name="T1">Some Type</typeparam>
    /// <typeparam name="T2">Some Type</typeparam>
    public interface ITest2<T1, T2>
    {
        /// <summary>
        /// Summary
        /// </summary>
        /// <param name="other">parameter</param>
        /// <returns>returns</returns>
        T1 Map(T2 paramT2);

        /// <summary>
        /// Summary
        /// </summary>
        /// <param name="other">parameter</param>
        /// <returns>returns</returns>
        T2 Map(T1 paramT1);
    }

    /// <summary>
    /// Summary
    /// </summary>
    /// <typeparam name="T">Some Type</typeparam>
    public interface ITest<T> : ITest2<Exception, T>
    {
        // A duplicate entry for the base method ITest2`2.Map(`1) is generated

        /// <summary>
        /// Summary
        /// </summary>
        /// <param name="other">parameter</param>
        /// <returns>returns</returns>
        T Map(string paramString);
    }
*/
    /// <summary>
    /// Test
    /// </summary>
    public interface Foo
    {
        /// <summary>
        /// Overload 1
        /// </summary>
        void Foo();

        /// <summary>
        /// Overload 2
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        void Foo<T>();
    }

    /// <summary>
    /// Test
    /// </summary>
    public class Implementation : Foo
    {
        #region Foo Members

        // Methods not match correctly

        /// <inheritdoc />
        public void Foo()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Foo<T>()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
