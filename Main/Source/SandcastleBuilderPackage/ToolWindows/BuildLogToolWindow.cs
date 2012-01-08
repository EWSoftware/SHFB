//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : BuildLogToolWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/07/2012
// Note    : Copyright 2011-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to view the build log content
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.0  03/18/2011  EFW  Created the code
// 1.9.3.4  01/07/2012  EFW  Replaced the user control with a common user
//                           control shared by this and the standalone GUI.
//=============================================================================

using System;
using System.Runtime.InteropServices;

using SandcastleBuilder.WPF.UserControls;

namespace SandcastleBuilder.Package.ToolWindows
{
    /// <summary>
    /// This is used to view the build log content
    /// </summary>
    [Guid("1ac33b36-3ffe-46b8-a340-090d35ab9ecf")]
    public class BuildLogToolWindow : BuildLogToolWindowBase
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public BuildLogToolWindow()
        {
            base.Content = new BuildLogViewerControl();
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to load a log file
        /// </summary>
        /// <param name="filename">The log file to load</param>
        public void LoadLogFile(string filename)
        {
            ((BuildLogViewerControl)base.Content).LogFilename = filename;
        }

        /// <summary>
        /// This is used to clear the log file
        /// </summary>
        public void ClearLog()
        {
            ((BuildLogViewerControl)base.Content).LogFilename = null;
        }
        #endregion
    }
}
