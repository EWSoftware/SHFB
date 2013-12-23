//===============================================================================================================
// System  : Sandcastle Tools - XML Comments Example
// File    : SetDocumentation.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/08/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This class is used to demonstrate the inheritdoc XML comments element.  It serves no useful purpose.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.0.0.0  12/06/2012  EFW  Created the code
//===============================================================================================================

using System;
using System.Runtime.Serialization;

namespace XMLCommentsExamples.DocumentationInheritance
{
    #region Constructor documentation inheritance
    /// <summary>
    /// This exception class is thrown by the application if it encounters an
    /// unrecoverable error.
    /// </summary>
    /// <conceptualLink target="86453FFB-B978-4A2A-9EB5-70E118CA8073" />
    [Serializable]
    public class CustomException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <overloads>There are four overloads for the constructor</overloads>
        public CustomException()
        {
        }

        /// <inheritdoc />
        public CustomException(string message) : base(message)
        {
            // Inherit documentation from the base Exception class matching
            // this constructor's signature.
        }

        /// <inheritdoc />
        public CustomException(string message, Exception innerException) :
          base(message, innerException)
        {
            // Inherit documentation from the base Exception class matching
            // this constructor's signature.
        }

        /// <inheritdoc />
        protected CustomException(SerializationInfo info,
          StreamingContext context) : base(info, context)
        {
            // Inherit documentation from the base Exception class matching
            // this constructor's signature.
        }
    }
    #endregion
}
