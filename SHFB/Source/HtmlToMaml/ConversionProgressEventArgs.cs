//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ConversionProgressEventArgs.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/17/2006
// Note    : Copyright 2006, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the event arguments class for the conversion progress
// event.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  09/17/2008  EFW  Created the code
//=============================================================================

using System;

namespace HtmlToMamlConversion
{
    /// <summary>
    /// This is a custom event arguments class for the
    /// <see cref="HtmlToMaml.ConversionProgress"/> event.
    /// </summary>
    public class ConversionProgressEventArgs : EventArgs
    {
        //=====================================================================
        // Private class members

        private string message;

        //=====================================================================
        // Properties

        /// <summary>
        /// Get the message associated with the progress report
        /// </summary>
        public string Message
        {
            get { return message; }
        }

        //=====================================================================
        // Methods, etc.

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="progressMessage">The progress message</param>
        public ConversionProgressEventArgs(string progressMessage)
        {
            message = progressMessage;
        }
    }
}
