//=============================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : DeploymentConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/27/2009
// Note    : Copyright 2007-2009, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a form that is used to configure the settings for the
// Output Deployement plug-in.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.2.0  09/24/2007  EFW  Created the code
// 1.8.0.3  07/05/2009  EFW  Added support for MS Help Viewer deployment
//=============================================================================

using System;
using System.Globalization;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This form is used to configure the settings for the
    /// <see cref="DeploymentPlugIn"/>.
    /// </summary>
    internal partial class DeploymentConfigDlg : Form
    {
        #region Private data members
        //=====================================================================

        private XmlDocument config;     // The configuration
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to return the configuration information
        /// </summary>
        public string Configuration
        {
            get { return config.OuterXml; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="currentConfig">The current XML configuration
        /// XML fragment</param>
        public DeploymentConfigDlg(string currentConfig)
        {
            XPathNavigator navigator, root;
            string value;

            InitializeComponent();

            lnkCodePlexSHFB.Links[0].LinkData = "http://SHFB.CodePlex.com";

            // Load the current settings
            config = new XmlDocument();
            config.LoadXml(currentConfig);
            navigator = config.CreateNavigator();

            root = navigator.SelectSingleNode("configuration");

            if(root.IsEmptyElement)
                return;

            value = root.GetAttribute("deleteAfterDeploy", String.Empty);

            if(!String.IsNullOrEmpty(value))
                chkDeleteAfterDeploy.Checked = Convert.ToBoolean(value,
                    CultureInfo.InvariantCulture);

            // Get HTML Help 1 deployment information
            ucHtmlHelp1.LoadFromSettings(DeploymentLocation.FromXPathNavigator(
                root, "help1x"));

            // Get MS Help 2 deployment information
            ucMSHelp2.LoadFromSettings(DeploymentLocation.FromXPathNavigator(
                root, "help2x"));

            // Get MS Help Viewer deployment information
            ucMSHelpViewer.LoadFromSettings(DeploymentLocation.FromXPathNavigator(
                root, "helpViewer"));

            // Get website deployment information
            ucWebsite.LoadFromSettings(DeploymentLocation.FromXPathNavigator(
                root, "website"));
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Close without saving
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Go to the CodePlex home page of the Sandcastle Help File Builder
        /// project.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void codePlex_LinkClicked(object sender,
          LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start((string)e.Link.LinkData);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show("Unable to launch link target.  " +
                    "Reason: " + ex.Message, Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            XmlAttribute attr;
            XmlNode root;
            DeploymentLocation htmlHelp1, msHelp2, msHelpViewer, website;
            bool isValid = true;

            epErrors.Clear();

            htmlHelp1 = ucHtmlHelp1.CreateDeploymentLocation();
            msHelp2 = ucMSHelp2.CreateDeploymentLocation();
            msHelpViewer = ucMSHelpViewer.CreateDeploymentLocation();
            website = ucWebsite.CreateDeploymentLocation();

            if(htmlHelp1 == null)
            {
                tabConfig.SelectedIndex = 0;
                isValid = false;
            }
            else
                if(msHelp2 == null)
                {
                    tabConfig.SelectedIndex = 1;
                    isValid = false;
                }
                else
                    if(msHelpViewer == null)
                    {
                        tabConfig.SelectedIndex = 2;
                        isValid = false;
                    }
                    else
                        if(website == null)
                        {
                            tabConfig.SelectedIndex = 3;
                            isValid = false;
                        }

            if(isValid && htmlHelp1.Location == null &&
              msHelp2.Location == null && msHelpViewer.Location == null &&
              website.Location == null)
            {
                tabConfig.SelectedIndex = 0;
                epErrors.SetError(chkDeleteAfterDeploy, "At least one help " +
                    "file format must have a target location specified");
                isValid = false;
            }

            if(!isValid)
                return;

            // Store the changes
            root = config.SelectSingleNode("configuration");
            attr = root.Attributes["deleteAfterDeploy"];

            if(attr == null)
            {
                attr = config.CreateAttribute("deleteAfterDeploy");
                root.Attributes.Append(attr);
            }

            attr.Value = chkDeleteAfterDeploy.Checked.ToString().ToLower(
                CultureInfo.InvariantCulture);

            htmlHelp1.ToXml(config, root, "help1x");
            msHelp2.ToXml(config, root, "help2x");
            msHelpViewer.ToXml(config, root, "helpViewer");
            website.ToXml(config, root, "website");

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        #endregion
    }
}
