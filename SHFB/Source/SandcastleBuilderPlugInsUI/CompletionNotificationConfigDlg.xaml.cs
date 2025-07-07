//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : CompletionNotificationConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/20/2025
// Note    : Copyright 2007-2025, Eric Woodruff, All rights reserved
//
// This file contains a form that is used to configure the settings for the Completion Notification plug-in
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/10/2007  EFW  Created the code
// 03/09/2008  EFW  Added support for log file XSL transform
// 05/07/2021  EFW  Added MEF configuration editor export and converted the form to WPF for better high DPI
//                  scaling support on 4K displays.
//===============================================================================================================

using System;
using System.Windows;
using System.Xml.Linq;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler.BuildComponent;
using Sandcastle.Core.PlugIn;
using Sandcastle.Core.Project;

using Sandcastle.Platform.Windows;

namespace SandcastleBuilder.PlugIns.UI
{
    /// <summary>
    /// This form is used to configure the settings for the Completion Notification plug-in
    /// </summary>
    public partial class CompletionNotificationConfigDlg : Window
    {
        #region Plug-in configuration editor factory for MEF
        //=====================================================================

        /// <summary>
        /// This allows editing of the plug-in configuration
        /// </summary>
        [PlugInConfigurationEditorExport("Completion Notification")]
        public sealed class Factory : IPlugInConfigurationEditor
        {
            /// <inheritdoc />
            public bool EditConfiguration(ISandcastleProject project, XElement configuration)
            {
                var dlg = new CompletionNotificationConfigDlg(configuration);

                return dlg.ShowModalDialog() ?? false;
            }
        }
        #endregion

        #region private data members
        //=====================================================================

        private readonly XElement configuration;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">The current configuration element</param>
        public CompletionNotificationConfigDlg(XElement configuration)
        {
            InitializeComponent();

            chkUseDefaultCredentials.IsChecked = true;

            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // Load the current settings
            if(!configuration.IsEmpty)
            {
                var node = configuration.Element("smtpServer");

                if(node != null)
                {
                    txtSmtpServer.Text = node.Attribute("host").Value;
                    udcPort.Value = (int)node.Attribute("port");
                }

                var userCreds = UserCredentials.FromXml(configuration);

                chkUseDefaultCredentials.IsChecked = userCreds.UseDefaultCredentials;
                txtUserName.Text = userCreds.UserName;
                txtPassword.Text = userCreds.Password;

                node = configuration.Element("fromEMail");

                if(node != null)
                    txtFromEMail.Text = node.Attribute("address").Value;

                node = configuration.Element("successEMail");

                if(node != null)
                {
                    txtSuccessEMail.Text = node.Attribute("address").Value;
                    chkAttachLogFileOnSuccess.IsChecked = (bool)node.Attribute("attachLog");
                }

                node = configuration.Element("failureEMail");

                if(node != null)
                {
                    txtFailureEMail.Text = node.Attribute("address").Value;
                    chkAttachLogFileOnFailure.IsChecked = (bool)node.Attribute("attachLog");
                }

                node = configuration.Element("xslTransform");

                if(node != null)
                {
                    txtXSLTransform.Text = node.Attribute("filename").Value;

                    // Legacy support.  Convert {@SHFBFolder} to {@SHFBRoot}
                    txtXSLTransform.Text = txtXSLTransform.Text.Replace("{@SHFBFolder}", "{@SHFBRoot}");
                }
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
            UiUtility.ShowHelpTopic("d0ac3f31-bc25-4c5a-b2e6-d64a74fdb67c");
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

        /// <summary>
        /// Enable or disable the user name and password controls based on the Default Credentials check state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkUseDefaultCredentials_CheckedChanged(object sender, RoutedEventArgs e)
        {
            txtUserName.IsEnabled = txtPassword.IsEnabled = !chkUseDefaultCredentials.IsChecked.Value;
        }

        /// <summary>
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            bool isValid = true;

            foreach(var c in new[] { txtSmtpServer, txtUserName, txtPassword, txtFromEMail, txtSuccessEMail,
              txtFailureEMail, txtXSLTransform })
            {
                c.SetValidationState(true, null);
            }

            if(txtSmtpServer.Text.Length == 0)
            {
                txtSmtpServer.SetValidationState(false, "An SMTP server name is required");
                isValid = false;
            }

            if(!chkUseDefaultCredentials.IsChecked.Value)
            {
                if(txtUserName.Text.Length == 0)
                {
                    txtUserName.SetValidationState(false, "A user name is required if not using default credentials");
                    isValid = false;
                }

                if(txtPassword.Text.Length == 0)
                {
                    txtPassword.SetValidationState(false, "A password is required if not using default credentials");
                    isValid = false;
                }
            }

            if(txtFromEMail.Text.Length == 0)
            {
                txtFromEMail.SetValidationState(false, "A from e-mail address is required");
                isValid = false;
            }

            if(txtFailureEMail.Text.Length == 0)
            {
                txtFailureEMail.SetValidationState(false, "A failure e-mail address is required");
                isValid = false;
            }

            if(!isValid)
                return;

            var userCreds = new UserCredentials(chkUseDefaultCredentials.IsChecked.Value, txtUserName.Text,
                txtPassword.Text);

            configuration.RemoveNodes();

            configuration.Add(
                new XElement("smtpServer",
                    new XAttribute("host", txtSmtpServer.Text),
                    new XAttribute("port", udcPort.Value)),
                userCreds.ToXml(),
                new XElement("fromEMail", new XAttribute("address", txtFromEMail.Text)),
                new XElement("successEMail",
                    new XAttribute("address", txtSuccessEMail.Text),
                    new XAttribute("attachLog", chkAttachLogFileOnSuccess.IsChecked)),
                new XElement("failureEMail",
                    new XAttribute("address", txtFailureEMail.Text),
                    new XAttribute("attachLog", chkAttachLogFileOnFailure.IsChecked)),
                new XElement("xslTransform", new XAttribute("filename", txtXSLTransform.Text)));

            this.DialogResult = true;
            this.Close();
        }
        #endregion
    }
}
