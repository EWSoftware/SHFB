//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ConversionProgressEventArgs.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/07/2021
// Note    : Copyright 2006-2021, Eric Woodruff, All rights reserved
//
// This file contains the event arguments class for the conversion progress event
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/17/2008  EFW  Created the code
//===============================================================================================================

using System;

namespace HtmlToMamlConversion
{
    /// <summary>
    /// This is a custom event arguments class for the <see cref="HtmlToMaml.ConversionProgress"/> event
    /// </summary>
    public class ConversionProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Get the message associated with the progress report
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="progressMessage">The progress message</param>
        public ConversionProgressEventArgs(string progressMessage)
        {
            this.Message = progressMessage;
        }
    }
}
