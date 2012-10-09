//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : SandcastleBuilderOptions.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/23/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that defines the general package options.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.0  03/27/2011  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Shell;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This class defines the general package options
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual), Guid("DE9C39B5-826B-46BE-98B7-DA7909000C95"),
      CLSCompliant(false), ComVisible(true), ToolboxItem(false)]
    public class SandcastleBuilderOptionsPage : DialogPage
    {
        #region Private data members
        //=====================================================================

        private GeneralOptionsControl control;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the path to the HTML Help 2 viewer tool
        /// </summary>
        public string HxsViewerPath { get; set; }

        /// <summary>
        /// This is used to get or set the path to the MS Help Viewer tool
        /// </summary>
        public string MSHelpViewerPath { get; set; }

        /// <summary>
        /// This is used to get or set the port to use when launcing the
        /// ASP.NET development web server.
        /// </summary>
        public int AspNetDevelopmentServerPort { get; set; }

        /// <summary>
        /// This is used to get or set whether or not verbose logging is enabled
        /// when building a help file.
        /// </summary>
        public bool VerboseLogging { get; set; }

        /// <summary>
        /// This is used to get or set whether or not to use the external browser
        /// when viewing website output.
        /// </summary>
        public bool UseExternalWebBrowser { get; set; }

        /// <summary>
        /// This is used to get or set whether or not to open the help file
        /// after a successful build.
        /// </summary>
        public bool OpenHelpAfterBuild { get; set; }

        /// <summary>
        /// This is used to get or set whether or not to open the build log
        /// viewer tool window after a failed build.
        /// </summary>
        /// <remarks>The log viewer is not opened if the build is cancelled</remarks>
        public bool OpenLogViewerOnFailedBuild { get; set; }

        /// <summary>
        /// This is overridden to return an instance of our custom user
        /// interface control to edit the properties.
        /// </summary>
        protected override System.Windows.Forms.IWin32Window Window
        {
            get
            {
                if(control == null)
                    control = new GeneralOptionsControl();

                return control;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public SandcastleBuilderOptionsPage()
        {
            this.AspNetDevelopmentServerPort = 12345;
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// Reset the settings to their default values
        /// </summary>
        public override void ResetSettings()
        {
            this.HxsViewerPath = this.MSHelpViewerPath = null;
            this.AspNetDevelopmentServerPort = 12345;
            this.VerboseLogging = this.UseExternalWebBrowser = this.OpenHelpAfterBuild = false;

            if(control != null)
                control.SetValues(this);
        }

        /// <summary>
        /// This is overridden to put the current values in the control
        /// </summary>
        /// <param name="e"></param>
        protected override void OnActivate(CancelEventArgs e)
        {
            if(control != null)
                control.SetValues(this);

            base.OnActivate(e);
        }

        /// <summary>
        /// This is overridden to validate the option values
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnDeactivate(CancelEventArgs e)
        {
            base.OnDeactivate(e);

            if(!e.Cancel && control != null)
            {
                e.Cancel = !control.IsValid;

                // We must apply changes here if valid or they don't stick
                if(!e.Cancel)
                    control.ApplyChanges(this);
            }
        }

        /// <summary>
        /// This is overridden to dispose of the control
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if(control != null)
            {
                control.Dispose();
                control = null;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
