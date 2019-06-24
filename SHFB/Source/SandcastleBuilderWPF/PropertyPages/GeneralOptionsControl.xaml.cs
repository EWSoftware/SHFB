//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : GeneralOptionsControl.xaml.cs
// Author  : Eric Woodruff
// Updated : 06/19/2019
// Note    : Copyright 2011-2019, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to modify the general help file builder package preferences that are unrelated to
// individual projects.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/27/2011  EFW  Created the code
// 03/04/2013  EFW  Added link to display the About box
// 05/03/2015  EFW  Removed support for the MS Help 2 file format
// 09/28/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

// Ignore Spelling: exe

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Win32;

using Sandcastle.Core;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This user control is used to modify the general help file builder package preferences that are unrelated
    /// to individual projects.
    /// </summary>
    [ToolboxItem(false)]
    public partial class GeneralOptionsControl : UserControl
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to see if the values are valid
        /// </summary>
        public bool IsValid
        {
            get
            {
                bool isValid = true;
                string filePath = txtMSHelpViewerPath.Text.Trim();

                if(filePath.Length != 0)
                {
                    try
                    {
                        txtMSHelpViewerPath.Text = filePath = Path.GetFullPath(filePath);

                        if(!File.Exists(filePath))
                            isValid = false;
                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                        isValid = false;
                    }
                }

                if(!isValid)
                    txtMSHelpViewerPath.SetValidationState(false, "The viewer application does not exist");
                else
                    txtMSHelpViewerPath.SetValidationState(true, null);

                return isValid;
            }
        }

        /// <summary>
        /// This is used to get or set the path to the MS Help Viewer tool
        /// </summary>
        public string MSHelpViewerPath
        {
            get => txtMSHelpViewerPath.Text;
            set => txtMSHelpViewerPath.Text = value;
        }

        /// <summary>
        /// This is used to get or set the port to use when launching the ASP.NET development web server
        /// </summary>
        public int AspNetDevelopmentServerPort
        {
            get => udcASPNetDevServerPort.Value.Value;
            set => udcASPNetDevServerPort.Value = value;
        }

        /// <summary>
        /// This is used to get or set whether or not verbose logging is enabled when building a help file
        /// </summary>
        public bool VerboseLogging
        {
            get => chkVerboseLogging.IsChecked.Value;
            set => chkVerboseLogging.IsChecked = value;
        }

        /// <summary>
        /// This is used to get or set whether or not to use the external browser when viewing website output
        /// </summary>
        public bool UseExternalWebBrowser
        {
            get => chkUseExternalBrowser.IsChecked.Value;
            set => chkUseExternalBrowser.IsChecked = value;
        }

        /// <summary>
        /// This is used to get or set whether or not to open the help file after a successful build
        /// </summary>
        public bool OpenHelpAfterBuild
        {
            get => chkOpenHelpAfterBuild.IsChecked.Value;
            set => chkOpenHelpAfterBuild.IsChecked = value;
        }

        /// <summary>
        /// This is used to get or set whether or not to open the build log viewer tool window after a failed
        /// build.
        /// </summary>
        public bool OpenLogViewerOnFailedBuild
        {
            get => chkOpenLogViewerOnFailure.IsChecked.Value;
            set => chkOpenLogViewerOnFailure.IsChecked = value;
        }

        /// <summary>
        /// This is used to get or set whether or not the extended XML comments completion source options are
        /// enabled.
        /// </summary>
        public bool EnableExtendedXmlCommentsCompletion
        {
            get => chkEnableExtendedXmlComments.IsChecked.Value;
            set => chkEnableExtendedXmlComments.IsChecked = value;
        }

        /// <summary>
        /// This is used to get or set whether or not the MAML and XML comments element Go To Definition and tool
        /// tip option is enabled.
        /// </summary>
        public bool EnableGoToDefinition
        {
            get => chkEnableGoToDefinition.IsChecked.Value;
            set => chkEnableGoToDefinition.IsChecked = value;
        }

        /// <summary>
        /// Related to the above, if enabled, Ctrl+clicking on a target will invoke the Go To Definition option
        /// </summary>
        public bool EnableCtrlClickGoToDefinition
        {
            get => chkEnableCtrlClickGoToDefinition.IsChecked.Value;
            set => chkEnableCtrlClickGoToDefinition.IsChecked = value;
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public GeneralOptionsControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Select a help viewer application
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnSelectMSHCViewer_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select the MS Help Viewer (.mshc) application",
                Filter = "Executable files (*.exe)|*.exe|All Files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                DefaultExt = "exe"
            };

            if(dlg.ShowDialog() ?? false)
                txtMSHelpViewerPath.Text = dlg.FileName;
        }

        /// <summary>
        /// Enable or disable the Ctrl+click option based on the overall Go To Definition
        /// setting.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkEnableGoToDefinition_Click(object sender, RoutedEventArgs e)
        {
            chkEnableCtrlClickGoToDefinition.IsEnabled = chkEnableGoToDefinition.IsChecked.Value;
        }

        /// <summary>
        /// View the Project website
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lnkSHFBInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(lnkSHFBInfo.NavigateUri.AbsoluteUri);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to navigate to website.  Reason: " + ex.Message,
                    Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        #endregion
    }
}
