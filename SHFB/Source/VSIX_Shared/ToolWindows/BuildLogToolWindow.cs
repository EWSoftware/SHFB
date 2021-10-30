//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : BuildLogToolWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/26/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This file contains the class used to view the build log content
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB
// This notice, the author's name, and all copyright notices must remain intact in all applications,
// documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/18/2011  EFW  Created the code
// 01/07/2012  EFW  Replaced the user control with a common user control shared by this and the standalone GUI
//===============================================================================================================

using System;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Shell;

using SandcastleBuilder.WPF.UserControls;

namespace SandcastleBuilder.Package.ToolWindows
{
    /// <summary>
    /// This is used to view the build log content
    /// </summary>
    /// <remarks>In Visual Studio, tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.  This class derives from the <c>ToolWindowPane</c> class
    /// provided from the MPF in order to use its implementation of the <c>IVsUIElementPane</c> interface.</remarks>
    [Guid("1ac33b36-3ffe-46b8-a340-090d35ab9ecf")]
    public class BuildLogToolWindow : ToolWindowPane
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public BuildLogToolWindow() : base(null)
        {
            this.Caption = "Build Log Content";
            this.Content = new BuildLogViewerControl();
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
