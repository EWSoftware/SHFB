using System;
using System.Runtime.Serialization;

namespace TestDoc.DocumentationInheritance
{
    #region Namespace documentation
    //=====================================================================

    /// <summary>
    /// These are comments from the DocumentationInheritance namespace's
    /// NamespaceDoc class.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    class NamespaceDoc
    {
    }
    #endregion

    #region Constructor documentation inheritance
    /// <summary>
    /// This exception class is thrown by the application if it encounters an
    /// unrecoverable error.
    /// </summary>
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
