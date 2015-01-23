//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ICopyComponentFactory.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/28/2013
// Note    : Copyright 2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains an interface that defines the factory method for BuildAssembler copy components
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/27/2013  EFW  Created the code
//===============================================================================================================

namespace Sandcastle.Core.BuildAssembler.BuildComponent
{
    /// <summary>
    /// This interface defines the factory method for copy components
    /// </summary>
    public interface ICopyComponentFactory
    {
        /// <summary>
        /// This is implemented to provide a syntax generator factory
        /// </summary>
        /// <param name="parent">The parent build component</param>
        /// <returns>A new instance of a syntax generator</returns>
        CopyComponentCore Create(BuildComponentCore parent);
    }
}
