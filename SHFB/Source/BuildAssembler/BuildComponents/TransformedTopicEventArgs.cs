//===============================================================================================================
// System  : Sandcastle Build Components
// File    : TransformedTopicEventArgs.cs
//
// This file contains an event arguments class used by the TransformComponent to indicate that it has finished
// transforming the given topic.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Change History
// 10/14/2012 - EFW - Created the code
//===============================================================================================================

using System;
using System.Xml;

namespace Microsoft.Ddue.Tools.BuildComponent
{
    /// <summary>
    /// This is used by the <see cref="TransformComponent"/> to indicate that it has finished transforming the
    /// given topic.
    /// </summary>
    public class TransformedTopicEventArgs : EventArgs
    {
        /// <summary>
        /// This read-only property returns the topic key
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// This read-only property returns the transformed topic document
        /// </summary>
        /// <remarks>Event handlers can further modify the topic's XML as needed</remarks>
        public XmlDocument Document { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">The transformed topic key</param>
        /// <param name="document">The transformed topic document</param>
        public TransformedTopicEventArgs(string key, XmlDocument document)
        {
            this.Key = key;
            this.Document = document;
        }
    }
}
