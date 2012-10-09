//=============================================================================
// System  : Sandcastle MRefBuilder
// File    : TestClass.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/01/2012
// Compiler: Microsoft Visual C#
//
// This class is used to test small snippets of code with MRefBuilder to
// diagnose problems.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://Sandcastle.CodePlex.com.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  02/01/2012  EFW  Created the code
//=============================================================================

// For debugging:
// /out:reflection.org C:\CP\TFS05\Sandcastle\Main\Source\MRefBuilder\TestCase\bin\Debug\TestCase.dll

using System;

namespace MyClassLibrary
{
    /// <summary>
    /// MyClass Summary
    /// </summary>
    /// <typeparam name="TD"></typeparam>
    /// <typeparam name="TT"></typeparam>
    public abstract partial class MyClass<TD, TT>
    {
        /// <summary>
        /// IBase Summary
        /// </summary>
        public interface IBase
        {
            NestedClass<T> RequestElementsTyped<T>();
        }

        /// <summary>
        /// NestedClass Summary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class NestedClass<T> : IEquatable<NestedClass<T>>
        {
            public bool Equals(NestedClass<T> other)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// IAnother Summary
        /// </summary>
        public interface IAnother : IBase { }
    }
}
