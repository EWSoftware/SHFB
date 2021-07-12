//===============================================================================================================
// System  : Sandcastle Help File Builder - Generate Inherited Documentation
// File    : InheritedDocsException.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/09/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains the exception class for the inherited documentation generation process.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/28/2008  EFW  Created the code
//===============================================================================================================

using System;
using System.Runtime.Serialization;

namespace SandcastleBuilder.Utils.InheritedDocumentation
{
    /// <summary>
    /// This exception class is thrown by the application if it encounters an unrecoverable error
    /// </summary>
    [Serializable]
    public class InheritedDocsException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <overloads>There are three overloads for the constructor</overloads>
        public InheritedDocsException()
        {
        }

        /// <inheritdoc />
        public InheritedDocsException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public InheritedDocsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc />
        protected InheritedDocsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
