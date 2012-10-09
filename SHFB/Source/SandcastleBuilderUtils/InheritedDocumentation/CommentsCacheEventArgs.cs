//=============================================================================
// System  : Sandcastle Help File Builder - Generate Inherited Documentation
// File    : CommentsCacheEventArgs.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/12/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains an event arguments class used by the comments cache.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.7  05/12/2008  EFW  Created the code
//=============================================================================

using System;
using System.Runtime.Serialization;

namespace SandcastleBuilder.Utils.InheritedDocumentation
{
    /// <summary>
    /// This is used by the comments cache to report a warning message
    /// </summary>
    public class CommentsCacheEventArgs : EventArgs
    {
        private string message;

        /// <summary>
        /// The message to report
        /// </summary>
        public string Message
        {
            get { return message; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">The message text</param>
        public CommentsCacheEventArgs(string text)
        {
            message = text;
        }
    }
}
