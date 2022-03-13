//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : RenderedSectionEventArgs.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/11/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains an event arguments class used to report when a topic section has been rendered
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/11/2022  EFW  Created the code
//===============================================================================================================

using System;

namespace Sandcastle.Core.PresentationStyle.Transformation
{
    /// <summary>
    /// This event arguments class is used to report when a topic section has been rendered
    /// </summary>
    /// <remarks>Note that rendered section events are raised regardless of whether or not any content was
    /// actually rendered.</remarks>
    public class RenderedSectionEventArgs : EventArgs
    {
        /// <summary>
        /// The topic key
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// The section name that was rendered
        /// </summary>
        public RenderedSection SectionName { get; }

        /// <summary>
        /// If <see cref="SectionName"/> is <c>Custom</c>, this contains the custom name of the section
        /// </summary>
        public string CustomSectionName { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">The topic key</param>
        /// <param name="sectionName">The section name that was rendered</param>
        /// <param name="customSectionName">The custom section name or null if it is a known section</param>
        public RenderedSectionEventArgs(string key, RenderedSection sectionName, string customSectionName)
        {
            this.Key = key;
            this.SectionName = sectionName;
            this.CustomSectionName = customSectionName;
        }
    }
}
