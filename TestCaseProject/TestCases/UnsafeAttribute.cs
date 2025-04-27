using System;

namespace TestDoc
{
    /// <summary>
    /// A test attribute for a notice
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class UnsafeAttribute : Attribute
    {
        /// <summary>
        /// The message to display
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public UnsafeAttribute() 
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">The message to display</param>
        public UnsafeAttribute(string message)
        {
            this.Message = message;
        }
    }
}
