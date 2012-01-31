//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : TopicContentNeededEventArgs.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/20/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used for the TopicPreviewerControl
// TopicContentNeeded event.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.4  01/20/2012  EFW  Created the code
//=============================================================================

using System.Collections.Generic;
using System.Windows;

using SandcastleBuilder.Utils.ConceptualContent;

namespace SandcastleBuilder.WPF.UserControls
{
    /// <summary>
    /// This is used to contain the event arguments for the
    /// <see cref="TopicPreviewerControl.TopicContentNeeded"/> event.
    /// </summary>
    public class TopicContentNeededEventArgs : RoutedEventArgs
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the topic filename that the previewer wants
        /// </summary>
        public string TopicFilename { get; private set; }

        /// <summary>
        /// This is used to get or set the topic file content if it is found
        /// </summary>
        /// <value>If null upon return from the event, the topic was not found in an open editor and the
        /// actual file content will be used.</value>
        public string TopicContent { get; set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="routedEvent">The routed event</param>
        /// <param name="source">The routed event source</param>
        /// <param name="topicFilename">The topic filename for which to search</param>
        public TopicContentNeededEventArgs(RoutedEvent routedEvent, object source, string topicFilename) :
          base(routedEvent, source)
        {
            this.TopicFilename = topicFilename;
        }
        #endregion
    }
}
