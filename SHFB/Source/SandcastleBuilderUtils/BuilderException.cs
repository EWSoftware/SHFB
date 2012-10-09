//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BuilderException.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/26/2008
// Note    : Copyright 2006-2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the exception class for the help file builder
// applications.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  08/04/2006  EFW  Created the code
//=============================================================================

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This exception class is thrown by the application if it encounters an
    /// unrecoverable error.
    /// </summary>
    [Serializable]
    public class BuilderException : Exception
    {
        #region Private data members
        //=====================================================================

        private string errorCode;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// Get the error code associated with the exception
        /// </summary>
        public string ErrorCode
        {
            get { return errorCode; }
        }
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
        public BuilderException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

        /// <summary>
        /// This constructor takes an error code and a message string
        /// </summary>
        /// <param name="code">The error code.  The suggested format is
        /// one to four letters to identify the component or process followed
        /// by four digits, zero padded to the left, with no spaces.</param>
        /// <param name="message">The exception message</param>
        public BuilderException(string code, string message) : base(message)
        {
            errorCode = code;
        }

        /// <summary>
        /// This constructor takes an error code, a message string, and an
        /// inner exception.
        /// </summary>
        /// <param name="code">The error code.  The suggested format is
        /// one to four letters to identify the component or process followed
        /// by four digits, zero padded to the left, with no spaces.</param>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public BuilderException(string code, string message,
          Exception innerException) : base(message, innerException)
        {
            errorCode = code;
        }

        /// <summary>
        /// Deserialization constructor for use with
        /// <see cref="System.Runtime.Serialization.ISerializable"/>.
        /// </summary>
        /// <param name="info">The serialization info object</param>
        /// <param name="context">The streaming context object</param>
        protected BuilderException(SerializationInfo info,
          StreamingContext context) : base(info, context)
        {
            if(info != null)
                errorCode = info.GetString("ErrorCode");
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// This implements the <see cref="System.Runtime.Serialization.ISerializable"/>
        /// interface and adds the error code to the serialization information.
        /// </summary>
        /// <param name="info">The serialization info object</param>
        /// <param name="context">The streaming context</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info,
          StreamingContext context)
        {
            if(info != null)
            {
                base.GetObjectData(info, context);
                info.AddValue("ErrorCode", errorCode);
            }
        }
        #endregion
    }
}
