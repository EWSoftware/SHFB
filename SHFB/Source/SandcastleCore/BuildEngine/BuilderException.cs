//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : BuilderException.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/20/2025
// Note    : Copyright 2006-2025, Eric Woodruff, All rights reserved
//
// This file contains the exception class for the help file builder applications
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/04/2006  EFW  Created the code
//===============================================================================================================

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Sandcastle.Core.BuildEngine
{
    /// <summary>
    /// This exception class is thrown by the application if it encounters an unrecoverable error
    /// </summary>
    [Serializable]
    public class BuilderException : Exception
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// Get the error code associated with the exception
        /// </summary>
        public string ErrorCode { get; }

        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <overloads>There are six overloads for the constructor</overloads>
        public BuilderException()
        {
        }

        /// <inheritdoc />
        public BuilderException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public BuilderException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// This constructor takes an error code and a message string
        /// </summary>
        /// <param name="code">The error code.  The suggested format is one to four letters to identify the
        /// component or process followed by four digits, zero padded to the left, with no spaces.</param>
        /// <param name="message">The exception message</param>
        public BuilderException(string code, string message) : base(message)
        {
            this.ErrorCode = code;
        }

        /// <summary>
        /// This constructor takes an error code, a message string, and an inner exception
        /// </summary>
        /// <param name="code">The error code.  The suggested format is one to four letters to identify the
        /// component or process followed by four digits, zero padded to the left, with no spaces.</param>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public BuilderException(string code, string message, Exception innerException) : base(message, innerException)
        {
            this.ErrorCode = code;
        }

        /// <summary>
        /// Deserialization constructor for use with <see cref="ISerializable"/>
        /// </summary>
        /// <param name="info">The serialization info object</param>
        /// <param name="context">The streaming context object</param>
        protected BuilderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if(info != null)
                this.ErrorCode = info.GetString("ErrorCode");
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// This implements the <see cref="ISerializable"/> interface and adds the error code to the
        /// serialization information.
        /// </summary>
        /// <param name="info">The serialization info object</param>
        /// <param name="context">The streaming context</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if(info != null)
            {
                base.GetObjectData(info, context);
                info.AddValue("ErrorCode", this.ErrorCode);
            }
        }
        #endregion
    }
}
