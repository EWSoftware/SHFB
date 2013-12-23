//===============================================================================================================
// System  : Sandcastle Tools - XML Comments Example
// File    : ThreadSafetyClass.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/08/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This class is used to demonstrate the various XML comments elements.  It serves no useful purpose.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.0.0.0  12/08/2012  EFW  Created the code
//===============================================================================================================

namespace XMLCommentsExamples
{
    #region threadsafety Example

    /// <summary>
    /// This class demonstrates the <c>threadsafety</c> XML comments element.
    /// </summary>
    /// <threadsafety static="true" instance="false" />
    /// <conceptualLink target="fb4625cb-52d0-428e-9c7c-7a0d88e1b692" />
    public class ThreadSafetyClass
    {
        /// <summary>
        /// Per the <c>threadsafety</c> XML comments element on the class, the developer has
        /// indicated that static methods like this one are safe for multi-threaded use.
        /// </summary>
        /// <conceptualLink target="fb4625cb-52d0-428e-9c7c-7a0d88e1b692" />
        public static void StaticMethod()
        {
            // Thread-safe code goes here
        }

        /// <summary>
        /// Per the <c>threadsafety</c> XML comments element on the class, the developer has
        /// indicated that instance method like this one are not safe for multi-threaded use.
        /// </summary>
        /// <conceptualLink target="fb4625cb-52d0-428e-9c7c-7a0d88e1b692" />
        public void InstanceMethod()
        {
            // Non-thread-safe code goes here
        }
    }
    #endregion
}
