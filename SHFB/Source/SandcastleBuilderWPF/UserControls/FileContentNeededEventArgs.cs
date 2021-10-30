//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : FileContentNeededEventArgs.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/17/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This file contains the class used for the EntityReferencesControl FileContentNeeded event
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/28/2011  EFW  Created the code
//===============================================================================================================

using System.Collections.Generic;
using System.Windows;

using SandcastleBuilder.Utils.ConceptualContent;

namespace SandcastleBuilder.WPF.UserControls
{
    /// <summary>
    /// This is used to contain the event arguments for the
    /// <see cref="EntityReferencesControl.FileContentNeeded"/> event.
    /// </summary>
    public class FileContentNeededEventArgs : RoutedEventArgs
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to return the topic collections for content layout files
        /// </summary>
        /// <value>The key is the filename, the value is the topic collection</value>
        public Dictionary<string, TopicCollection> ContentLayoutFiles { get; }

        /// <summary>
        /// This is used to return the topic collections for site map files
        /// </summary>
        /// <value>The key is the filename, the value is the topic collection</value>
        public Dictionary<string, TocEntryCollection> SiteMapFiles { get; }

        /// <summary>
        /// This is used to return the token collections for token files
        /// </summary>
        /// <value>The key is the filename, the value is the token collection</value>
        public Dictionary<string, TokenCollection> TokenFiles { get; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="routedEvent">The routed event</param>
        /// <param name="source">The routed event source</param>
        public FileContentNeededEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
        {
            this.ContentLayoutFiles = new Dictionary<string, TopicCollection>();
            this.SiteMapFiles = new Dictionary<string, TocEntryCollection>();
            this.TokenFiles = new Dictionary<string, TokenCollection>();
        }
        #endregion
    }
}
