//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : SandcastleBuilderOptions.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/10/2017
// Note    : Copyright 2011-2017, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that defines the general package options
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/27/2011  EFW  Created the code
// 05/03/2015  EFW  Removed support for the MS Help 2 file format
//===============================================================================================================

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;

using Microsoft.VisualStudio.Shell;

using SandcastleBuilder.WPF.PropertyPages;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This class defines the general package options
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual), Guid("DE9C39B5-826B-46BE-98B7-DA7909000C95"),
      CLSCompliant(false), ComVisible(true), ToolboxItem(false)]
    public class SandcastleBuilderOptionsPage : UIElementDialogPage
    {
        #region Private data members
        //=====================================================================

        private GeneralOptionsControl control;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the path to the MS Help Viewer tool
        /// </summary>
        public string MSHelpViewerPath { get; set; }

        /// <summary>
        /// This is used to get or set the port to use when launching the ASP.NET development web server
        /// </summary>
        public int AspNetDevelopmentServerPort { get; set; }

        /// <summary>
        /// This is used to get or set whether or not verbose logging is enabled when building a help file
        /// </summary>
        public bool VerboseLogging { get; set; }

        /// <summary>
        /// This is used to get or set whether or not to use the external browser when viewing website output
        /// </summary>
        public bool UseExternalWebBrowser { get; set; }

        /// <summary>
        /// This is used to get or set whether or not to open the help file after a successful build
        /// </summary>
        public bool OpenHelpAfterBuild { get; set; }

        /// <summary>
        /// This is used to get or set whether or not to open the build log viewer tool window after a failed
        /// build.
        /// </summary>
        /// <remarks>The log viewer is not opened if the build is canceled</remarks>
        public bool OpenLogViewerOnFailedBuild { get; set; }

        /// <summary>
        /// This is overridden to return an instance of our custom user interface control to edit the properties
        /// </summary>
        protected override UIElement Child
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

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Set the current control values
        /// </summary>
        public void SetValues()
        {
            if(control != null)
            {
                control.MSHelpViewerPath = this.MSHelpViewerPath;
                control.AspNetDevelopmentServerPort = this.AspNetDevelopmentServerPort;
                control.VerboseLogging  = this.VerboseLogging;
                control.OpenLogViewerOnFailedBuild = this.OpenLogViewerOnFailedBuild;
                control.OpenHelpAfterBuild = this.OpenHelpAfterBuild;
                control.UseExternalWebBrowser = this.UseExternalWebBrowser;

                // MEF provider options are stored separately to avoid loading the entire package just to access
                // these options.
                var mefOptions = new MefProviderOptions(this.Site);

                control.EnableExtendedXmlCommentsCompletion = mefOptions.EnableExtendedXmlCommentsCompletion;
                control.EnableGoToDefinition = mefOptions.EnableGoToDefinition;
                control.EnableCtrlClickGoToDefinition = mefOptions.EnableCtrlClickGoToDefinition;
                control.EnableGoToDefinitionInCRef = mefOptions.EnableGoToDefinitionInCRef;
            }
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// Reset the settings to their default values
        /// </summary>
        public override void ResetSettings()
        {
            this.MSHelpViewerPath = null;
            this.AspNetDevelopmentServerPort = 12345;
            this.VerboseLogging = this.UseExternalWebBrowser = this.OpenHelpAfterBuild = false;
            this.SetValues();
        }

        /// <summary>
        /// This is overridden to put the current values in the control
        /// </summary>
        /// <param name="e"></param>
        protected override void OnActivate(CancelEventArgs e)
        {
            this.SetValues();
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
                {
                    this.MSHelpViewerPath = control.MSHelpViewerPath;
                    this.AspNetDevelopmentServerPort = control.AspNetDevelopmentServerPort;
                    this.VerboseLogging = control.VerboseLogging;
                    this.OpenLogViewerOnFailedBuild = control.OpenLogViewerOnFailedBuild;
                    this.OpenHelpAfterBuild = control.OpenHelpAfterBuild;
                    this.UseExternalWebBrowser = control.UseExternalWebBrowser;

                    // MEF provider options are stored separately to avoid loading the entire package just to
                    // access these options.
                    var mefOptions = new MefProviderOptions(this.Site);

                    mefOptions.EnableExtendedXmlCommentsCompletion = control.EnableExtendedXmlCommentsCompletion;
                    mefOptions.EnableGoToDefinition = control.EnableGoToDefinition;
                    mefOptions.EnableCtrlClickGoToDefinition = control.EnableCtrlClickGoToDefinition;
                    mefOptions.EnableGoToDefinitionInCRef = control.EnableGoToDefinitionInCRef;

                    mefOptions.SaveConfiguration();
                };
            }
        }
        #endregion
    }
}
