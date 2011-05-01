using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

#pragma warning disable 1591

using SandcastleBuilder.Package.ToolWindows;

namespace SandcastleBuilder.Package
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
	[ProvideToolWindow(typeof(BuildLogToolWindow), Orientation=ToolWindowOrientation.Right, Style=VsDockStyle.MDI, MultiInstances = false, Transient = false, PositionX = 100 , PositionY = 100 , Width = 300 , Height = 300 )]
    public abstract class PackageBase : Microsoft.VisualStudio.Project.ProjectPackage
    {
		/// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public PackageBase()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

			// Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
				CommandID commandId;
				OleMenuCommand menuItem;

				// Create the command for button AddDocSource
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.AddDocSource);
                menuItem = new OleMenuCommand(AddDocSourceExecuteHandler, AddDocSourceChangeHandler, AddDocSourceQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);
				// Create the command for button ViewHelpFile
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.ViewHelpFile);
                menuItem = new OleMenuCommand(ViewHelpFileExecuteHandler, ViewHelpFileChangeHandler, ViewHelpFileQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);
				// Create the command for button ViewHtmlHelp
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.ViewHtmlHelp);
                menuItem = new OleMenuCommand(ViewHtmlHelpExecuteHandler, ViewHtmlHelpChangeHandler, ViewHtmlHelpQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);
				// Create the command for button ViewHxSHelp
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.ViewHxSHelp);
                menuItem = new OleMenuCommand(ViewHxSHelpExecuteHandler, ViewHxSHelpChangeHandler, ViewHxSHelpQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);
				// Create the command for button ViewMshcHelp
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.ViewMshcHelp);
                menuItem = new OleMenuCommand(ViewMshcHelpExecuteHandler, ViewMshcHelpChangeHandler, ViewMshcHelpQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);
				// Create the command for button LaunchHelpLibMgr
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.LaunchHelpLibMgr);
                menuItem = new OleMenuCommand(LaunchHelpLibMgrExecuteHandler, LaunchHelpLibMgrChangeHandler, LaunchHelpLibMgrQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);
				// Create the command for button ViewAspNetWebsite
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.ViewAspNetWebsite);
                menuItem = new OleMenuCommand(ViewAspNetWebsiteExecuteHandler, ViewAspNetWebsiteChangeHandler, ViewAspNetWebsiteQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);
				// Create the command for button ViewHtmlWebsite
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.ViewHtmlWebsite);
                menuItem = new OleMenuCommand(ViewHtmlWebsiteExecuteHandler, ViewHtmlWebsiteChangeHandler, ViewHtmlWebsiteQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);
				// Create the command for button ViewFaq
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.ViewFaq);
                menuItem = new OleMenuCommand(ViewFaqExecuteHandler, ViewFaqChangeHandler, ViewFaqQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);
				// Create the command for button ViewShfbHelp
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.ViewShfbHelp);
                menuItem = new OleMenuCommand(ViewShfbHelpExecuteHandler, ViewShfbHelpChangeHandler, ViewShfbHelpQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);
				// Create the command for button OpenInStandaloneGUI
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.OpenInStandaloneGUI);
                menuItem = new OleMenuCommand(OpenInStandaloneGUIExecuteHandler, OpenInStandaloneGUIChangeHandler, OpenInStandaloneGUIQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);
				// Create the command for button AboutSHFB
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.AboutSHFB);
                menuItem = new OleMenuCommand(AboutSHFBExecuteHandler, AboutSHFBChangeHandler, AboutSHFBQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);
				// Create the command for button ViewBuildLog
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.ViewBuildLog);
                menuItem = new OleMenuCommand(ViewBuildLogExecuteHandler, ViewBuildLogChangeHandler, ViewBuildLogQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);
				// Create the command for button FilterLog
                commandId = new CommandID(GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.FilterLog);
                menuItem = new OleMenuCommand(FilterLogExecuteHandler, FilterLogChangeHandler, FilterLogQueryStatusHandler, commandId);
                mcs.AddCommand(menuItem);

			}
		}
		
		#endregion

		#region Handlers for Button: AddDocSource

		protected virtual void AddDocSourceExecuteHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void AddDocSourceChangeHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void AddDocSourceQueryStatusHandler(object sender, EventArgs e)
		{
		}

		#endregion

		#region Handlers for Button: ViewHelpFile

		protected virtual void ViewHelpFileExecuteHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ViewHelpFileChangeHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ViewHelpFileQueryStatusHandler(object sender, EventArgs e)
		{
		}

		#endregion

		#region Handlers for Button: ViewHtmlHelp

		protected virtual void ViewHtmlHelpExecuteHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ViewHtmlHelpChangeHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ViewHtmlHelpQueryStatusHandler(object sender, EventArgs e)
		{
		}

		#endregion

		#region Handlers for Button: ViewHxSHelp

		protected virtual void ViewHxSHelpExecuteHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ViewHxSHelpChangeHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ViewHxSHelpQueryStatusHandler(object sender, EventArgs e)
		{
		}

		#endregion

		#region Handlers for Button: ViewMshcHelp

		protected virtual void ViewMshcHelpExecuteHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ViewMshcHelpChangeHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ViewMshcHelpQueryStatusHandler(object sender, EventArgs e)
		{
		}

		#endregion

		#region Handlers for Button: LaunchHelpLibMgr

		protected virtual void LaunchHelpLibMgrExecuteHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void LaunchHelpLibMgrChangeHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void LaunchHelpLibMgrQueryStatusHandler(object sender, EventArgs e)
		{
		}

		#endregion

		#region Handlers for Button: ViewAspNetWebsite

		protected virtual void ViewAspNetWebsiteExecuteHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ViewAspNetWebsiteChangeHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ViewAspNetWebsiteQueryStatusHandler(object sender, EventArgs e)
		{
		}

		#endregion

		#region Handlers for Button: ViewHtmlWebsite

		protected virtual void ViewHtmlWebsiteExecuteHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ViewHtmlWebsiteChangeHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ViewHtmlWebsiteQueryStatusHandler(object sender, EventArgs e)
		{
		}

		#endregion

		#region Handlers for Button: ViewFaq

		protected virtual void ViewFaqExecuteHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ViewFaqChangeHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ViewFaqQueryStatusHandler(object sender, EventArgs e)
		{
		}

		#endregion

		#region Handlers for Button: ViewShfbHelp

		protected virtual void ViewShfbHelpExecuteHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ViewShfbHelpChangeHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ViewShfbHelpQueryStatusHandler(object sender, EventArgs e)
		{
		}

		#endregion

		#region Handlers for Button: OpenInStandaloneGUI

		protected virtual void OpenInStandaloneGUIExecuteHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void OpenInStandaloneGUIChangeHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void OpenInStandaloneGUIQueryStatusHandler(object sender, EventArgs e)
		{
		}

		#endregion

		#region Handlers for Button: AboutSHFB

		protected virtual void AboutSHFBExecuteHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void AboutSHFBChangeHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void AboutSHFBQueryStatusHandler(object sender, EventArgs e)
		{
		}

		#endregion

		#region Handlers for Button: ViewBuildLog

		protected virtual void ViewBuildLogExecuteHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ViewBuildLogChangeHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void ViewBuildLogQueryStatusHandler(object sender, EventArgs e)
		{
		}

		#endregion

		#region Handlers for Button: FilterLog

		protected virtual void FilterLogExecuteHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void FilterLogChangeHandler(object sender, EventArgs e)
		{
		}
		
		protected virtual void FilterLogQueryStatusHandler(object sender, EventArgs e)
		{
		}

		#endregion

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindowBuildLog(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(BuildLogToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(String.Format("Can not create Toolwindow: BuildLog"));
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        protected void ShowMessage(string message)
        {
            // Show a Message Box to prove we were here
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            int result;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       "SandcastleBuilderPackage",
                       string.Format(CultureInfo.CurrentCulture, message, this.ToString()),
                       string.Empty,
                       0,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       OLEMSGICON.OLEMSGICON_INFO,
                       0,        // false
                       out result));
        }
    }
}
