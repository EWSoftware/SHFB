//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : UnhandledElementEventArgs.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/11/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains an event arguments class used to report unhandled elements found while transforming a
// presentation style topic.
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
    /// This event arguments class is used to report unhandled elements found while transforming a presentation
    /// style topic.
    /// </summary>
    public class UnhandledElementEventArgs : EventArgs
    {
        /// <summary>
        /// The topic key
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// The unhandled element name
        /// </summary>
        public string ElementName { get; }

        /// <summary>
        /// The parent element name of the unhandled element
        /// </summary>
        public string ParentElementName { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">The topic key</param>
        /// <param name="elementName">The unhandled element name</param>
        /// <param name="parentElementName">The parent element name of the unhandled element</param>
        public UnhandledElementEventArgs(string key, string elementName, string parentElementName)
        {
            this.Key = key;
            this.ElementName = elementName;
            this.ParentElementName = parentElementName;
        }
    }
}
