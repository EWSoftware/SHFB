//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : CommentsCacheEventArgs.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/20/2025
// Note    : Copyright 2008-2025, Eric Woodruff, All rights reserved
//
// This file contains an event arguments class used by the comments cache.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB. This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/12/2008  EFW  Created the code
//===============================================================================================================

using System;

namespace Sandcastle.Core.InheritedDocumentation
{
    /// <summary>
    /// This is used by the comments cache to report a warning message
    /// </summary>
    public class CommentsCacheEventArgs : EventArgs
    {
        /// <summary>
        /// The message to report
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">The message text</param>
        public CommentsCacheEventArgs(string text)
        {
            this.Message = text;
        }
    }
}
