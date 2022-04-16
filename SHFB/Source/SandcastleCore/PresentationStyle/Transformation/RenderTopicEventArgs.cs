//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : RenderTopicEventArgs.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/02/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains an event arguments class used to report when a topic is starting to be or has finished
// being rendered by the presentation style topic transformation.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/31/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation
{
    /// <summary>
    /// This event arguments class is used to report when a topic is starting to be or has finished being
    /// rendered by the presentation style topic transformation.
    /// </summary>
    public class RenderTopicEventArgs : EventArgs
    {
        /// <summary>
        /// The topic key
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// The topic content
        /// </summary>
        public XDocument TopicContent { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">The topic key</param>
        /// <param name="topicContent">The topic content</param>
        public RenderTopicEventArgs(string key, XDocument topicContent)
        {
            this.Key = key;
            this.TopicContent = topicContent;
        }
    }
}
