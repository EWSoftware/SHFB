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
// /out:reflection.org C:\CP\TFS01\SHFB\Sandcastle\Source\MRefBuilder\TestCase\bin\Debug\TestCase.dll

using System;

namespace MyClassLibrary
{
    /// <summary>
    /// Base class
    /// </summary>
    public class Base
    {
        /// <summary>
        /// Base class method
        /// </summary>
        public void BaseMethod()
        {
        }

        /// <summary>
        /// Protected virtual method
        /// </summary>
        protected virtual void ProtectedVirtualMethod()
        {
        }

        /// <summary>
        /// Non-virtual protected method of base class
        /// </summary>
        protected void BaseProtectedMethod()
        {
        }
    }

    /// <summary>
    /// Derived sealed class
    /// </summary>
    public sealed class Derived : Base
    {
        /// <summary>
        /// Public method of derived class
        /// </summary>
        public void DerivedMethod()
        {
        }

/*        /// <summary>
        /// New protected member in a sealed class.  Just a warning about this so it's possible.
        /// </summary>
        protected void Test()
        {
        }*/

        /// <summary>
        /// Overridden protected method in sealed class
        /// </summary>
        protected override void ProtectedVirtualMethod()
        {
            base.ProtectedVirtualMethod();
        }
    }
}
