//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ManifestTopic.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/06/2021
// Note    : Copyright 2015-2021, Eric Woodruff, All rights reserved
//
// This file contains a class used to hold the topic ID and type for a BuildAssembler manifest file entry
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/20/2015  EFW  Created the code
//===============================================================================================================

namespace Sandcastle.Core.BuildAssembler
{
    /// <summary>
    /// This is used to hold the topic ID and type for a BuildAssembler manifest file entry
    /// </summary>
    public class ManifestTopic
    {
        /// <summary>
        /// This read-only property is used to get the topic ID
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// This read-only property is used to get the topic type
        /// </summary>
        public string TopicType { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The topic ID</param>
        /// <param name="topicType">The topic type</param>
        public ManifestTopic(string id, string topicType)
        {
            this.Id = id;
            this.TopicType = topicType;
        }
    }
}
