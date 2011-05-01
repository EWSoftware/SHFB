//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : BuildLogToolWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/22/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to view the build log content.  This will
// eventually be replaced by something with more features such as item lookup.
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
//=============================================================================

using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace SandcastleBuilder.Package.ToolWindows
{
    /// <summary>
    /// This is used to view the build log content
    /// </summary>
    /// <remarks>This will eventually be replaced by something with more
    /// features such as item lookup.</remarks>
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
            base.Content = new BuildLogControl();
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
            ((BuildLogControl)base.Content).LogFilename = filename;
        }

        /// <summary>
        /// This is used to clear the log file
        /// </summary>
        public void ClearLog()
        {
            ((BuildLogControl)base.Content).LogFilename = null;
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// This is overridden to connect the toolbar event handlers
        /// </summary>
        /// <remarks>The VSPackage Builder creates handlers in the package but it's not convenient to find the
        /// toolbar and delegate the actions to it so we'll connect our own handlers here.</remarks>
        protected override void Initialize()
        {
            base.Initialize();

            OleMenuCommandService mcs = this.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            if(mcs != null)
            {
                CommandID commandId;
                OleMenuCommand menuItem;

                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.FilterLog);
                menuItem = new OleMenuCommand(FilterLogExecuteHandler, null, FilterLogQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);
            }
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// This is used to execute the toolbar commands
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected virtual void FilterLogExecuteHandler(object sender, EventArgs e)
        {
            OleMenuCommand command = (OleMenuCommand)sender;
            BuildLogControl content = (BuildLogControl)base.Content;

            if(content != null)
                command.Checked = content.IsFiltered = !content.IsFiltered;
        }

        /// <summary>
        /// This is used to set the state of the toolbar commands
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected virtual void FilterLogQueryStatusHandler(object sender, EventArgs e)
        {
            OleMenuCommand command = (OleMenuCommand)sender;
            BuildLogControl content = (BuildLogControl)base.Content;

            command.Enabled = (content != null && !String.IsNullOrEmpty(content.LogFilename));
        }
        #endregion
    }
}
