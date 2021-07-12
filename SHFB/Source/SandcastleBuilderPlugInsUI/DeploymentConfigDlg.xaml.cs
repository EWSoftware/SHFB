//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : DeploymentConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/10/2021
// Note    : Copyright 2007-2021, Eric Woodruff, All rights reserved
//
// This file contains a form that is used to configure the settings for the Output Deployment plug-in
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/24/2007  EFW  Created the code
// 07/05/2009  EFW  Added support for MS Help Viewer deployment
// 02/06/2012  EFW  Added support for renaming MSHA file
// 03/09/2014  EFW  Added support for Open XML deployment
// 04/03/2014  EFW  Added support for markdown content deployment
// 05/03/2015  EFW  Removed support for the MS Help 2 file format
// 05/10/2021  EFW  Added MEF configuration editor export and converted the form to WPF for better high DPI
//                  scaling support on 4K displays.
//===============================================================================================================

using System;
using System.Windows;
using System.Xml.Linq;
using System.Xml.XPath;

using Sandcastle.Core;
using Sandcastle.Platform.Windows;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;

namespace SandcastleBuilder.PlugIns.UI
{
    /// <summary>
    /// This form is used to configure the settings for the Output Deployment plug-in
    /// </summary>
    public partial class DeploymentConfigDlg : Window
    {
        #region Plug-in configuration editor factory for MEF
        //=====================================================================

        /// <summary>
        /// This allows editing of the plug-in configuration
        /// </summary>
        [PlugInConfigurationEditorExport("Output Deployment")]
        public sealed class Factory : IPlugInConfigurationEditor
        {
            /// <inheritdoc />
            public bool EditConfiguration(SandcastleProject project, XElement configuration)
            {
                var dlg = new DeploymentConfigDlg(configuration);

                return dlg.ShowModalDialog() ?? false;
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private readonly XElement configuration;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">The configuration to edit</param>
        public DeploymentConfigDlg(XElement configuration)
        {
            InitializeComponent();

            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // Load the current settings
            if(!configuration.IsEmpty)
            {
                chkDeleteAfterDeploy.IsChecked = (bool)configuration.Attribute("deleteAfterDeploy");
                chkVerboseLogging.IsChecked = (bool?)configuration.Attribute("verboseLogging") ?? false;

                // Get HTML Help 1 deployment information
                ucHelp1.LoadFromSettings(DeploymentLocation.FromXml(configuration, "help1x"));

                // Get MS Help Viewer deployment information
                var msHelpViewer = configuration.XPathSelectElement("deploymentLocation[@id='helpViewer']");

                if(msHelpViewer != null)
                    chkRenameMSHA.IsChecked = (bool)msHelpViewer.Attribute("renameMSHA");

                ucMSHelpViewer.LoadFromSettings(DeploymentLocation.FromXml(configuration, "helpViewer"));

                // Get website deployment information
                ucWebsite.LoadFromSettings(DeploymentLocation.FromXml(configuration, "website"));

                // Get Open XML deployment information
                ucOpenXml.LoadFromSettings(DeploymentLocation.FromXml(configuration, "openXml"));

                // Get markdown content deployment information
                ucMarkdown.LoadFromSettings(DeploymentLocation.FromXml(configuration, "markdown"));
            }
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// View help for this plug-in
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            UiUtility.ShowHelpTopic("796d6bbe-5130-4c87-8790-5cceb66219c7");
        }

        /// <summary>
        /// Go to the project site
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lnkProjectSite_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(e.Uri.ToString());
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show("Unable to launch link target.  Reason: " + ex.Message, Constants.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DeploymentLocation htmlHelp1, msHelpViewer, website, openXml, markdown;
            bool isValid = false;

            htmlHelp1 = ucHelp1.CreateDeploymentLocation();
            msHelpViewer = ucMSHelpViewer.CreateDeploymentLocation();
            website = ucWebsite.CreateDeploymentLocation();
            openXml = ucOpenXml.CreateDeploymentLocation();
            markdown = ucMarkdown.CreateDeploymentLocation();

            if(htmlHelp1 == null)
                tabConfig.SelectedIndex = 0;
            else if(msHelpViewer == null)
                tabConfig.SelectedIndex = 1;
            else if(website == null)
                tabConfig.SelectedIndex = 2;
            else if(openXml == null)
                tabConfig.SelectedIndex = 3;
            else if(markdown == null)
                tabConfig.SelectedIndex = 4;
            else
                isValid = true;

            if(isValid && htmlHelp1.Location == null && msHelpViewer.Location == null &&
              website.Location == null && openXml.Location == null && markdown.Location == null)
            {
                tabConfig.SelectedIndex = 0;

                MessageBox.Show("At least one help file format must have a target location specified",
                    Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                isValid = false;
            }

            if(isValid)
            {
                // Store the changes
                configuration.RemoveNodes();
                configuration.RemoveAttributes();

                var mshv = msHelpViewer.ToXml("helpViewer");
                mshv.Add(new XAttribute("renameMSHA", chkRenameMSHA.IsChecked.Value));

                configuration.Add(new XAttribute("deleteAfterDeploy", chkDeleteAfterDeploy.IsChecked.Value),
                    new XAttribute("verboseLogging", chkVerboseLogging.IsChecked.Value),
                    htmlHelp1.ToXml("help1x"),
                    mshv,
                    website.ToXml("website"),
                    openXml.ToXml("openXml"),
                    markdown.ToXml("markdown"));

                this.DialogResult = true;
                this.Close();
            }
        }
        #endregion
    }
}
