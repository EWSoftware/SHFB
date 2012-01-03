//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : FileContentNeededEventArgs.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/29/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used for the EntityReferencesControl
// FileContentNeeded event.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.3  12/28/2011  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public Dictionary<string, TopicCollection> ContentLayoutFiles { get; private set; }

        /// <summary>
        /// This is used to return the topic collections for site map files
        /// </summary>
        /// <value>The key is the filename, the value is the topic collection</value>
        public Dictionary<string, TocEntryCollection> SiteMapFiles { get; private set; }

        /// <summary>
        /// This is used to return the token collections for token files
        /// </summary>
        /// <value>The key is the filename, the value is the token collection</value>
        public Dictionary<string, TokenCollection> TokenFiles { get; private set; }

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
