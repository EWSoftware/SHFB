//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : AjaxDocConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/05/2021
// Note    : Copyright 2007-2021, Eric Woodruff, All rights reserved
//
// This file contains a form that is used to configure the settings for the AjaxDoc Builder plug-in
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/15/2007  EFW  Created the code
// 05/05/2021  EFW  Added MEF configuration editor export and converted the form to WPF for better high DPI
//                  scaling support on 4K displays.
//===============================================================================================================

// Ignore Spelling: url

using System;
using System.Windows;
using System.Xml.Linq;

using Sandcastle.Core;
using Sandcastle.Platform.Windows;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;

namespace SandcastleBuilder.PlugIns.UI
{
    /// <summary>
    /// This form is used to configure the settings for the AjaxDoc plug-in
    /// </summary>
    public partial class AjaxDocConfigDlg : Window
    {
        #region Plug-in configuration editor factory for MEF
        //=====================================================================

        /// <summary>
        /// This allows editing of the plug-in configuration
        /// </summary>
        [PlugInConfigurationEditorExport("AjaxDoc Builder")]
        public sealed class Factory : IPlugInConfigurationEditor
        {
            /// <inheritdoc />
            public bool EditConfiguration(SandcastleProject project, XElement configuration)
            {
                var dlg = new AjaxDocConfigDlg(configuration);

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
        public AjaxDocConfigDlg(XElement configuration)
        {
            InitializeComponent();

            this.chkUseProxyServer_CheckedChanged(this, null);

            chkUseDefaultCredentials.IsChecked = chkUseProxyDefCreds.IsChecked = true;

            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // Load the current settings
            if(!configuration.IsEmpty)
            {
                var node = configuration.Element("ajaxDoc");

                if(node != null)
                {
                    txtAjaxDocUrl.Text = node.Attribute("url").Value;
                    txtProjectName.Text = node.Attribute("project").Value;
                    chkRegenerateFiles.IsChecked = (bool)node.Attribute("regenerate");
                }

                var userCreds = UserCredentials.FromXml(configuration);

                chkUseDefaultCredentials.IsChecked = userCreds.UseDefaultCredentials;
                txtUserName.Text = userCreds.UserName;
                txtPassword.Text = userCreds.Password;

                var proxyCreds = ProxyCredentials.FromXml(configuration);

                chkUseProxyServer.IsChecked = proxyCreds.UseProxyServer;
                txtProxyServer.Text = proxyCreds.ProxyServer?.OriginalString;

                chkUseProxyDefCreds.IsChecked = proxyCreds.Credentials.UseDefaultCredentials;
                txtProxyUserName.Text = proxyCreds.Credentials.UserName;
                txtProxyPassword.Text = proxyCreds.Credentials.Password;
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
            UiUtility.ShowHelpTopic("99773472-ff05-4eba-8194-b36a0536280e");
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
        /// Enable or disable the proxy server settings based on the Use Proxy Server check state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkUseProxyServer_CheckedChanged(object sender, RoutedEventArgs e)
        {
            txtProxyServer.IsEnabled = chkUseProxyDefCreds.IsEnabled = txtProxyUserName.IsEnabled =
                txtProxyPassword.IsEnabled = chkUseProxyServer.IsChecked.Value;

            if(chkUseProxyServer.IsChecked.Value)
                this.chkUseProxyDefCreds_CheckedChanged(sender, e);
        }

        /// <summary>
        /// Enable or disable the proxy user credentials based on the Proxy Default Credentials check state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkUseProxyDefCreds_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if(chkUseProxyDefCreds.IsEnabled)
                txtProxyUserName.IsEnabled = txtProxyPassword.IsEnabled = !chkUseProxyDefCreds.IsChecked.Value;
        }

        /// <summary>
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            bool isValid = true;

            foreach(var c in new[] { txtAjaxDocUrl, txtProjectName, txtUserName, txtPassword, txtProxyServer,
              txtProxyUserName, txtProxyPassword })
            {
                c.Text = c.Text.Trim();
                c.SetValidationState(true, null);
            }

            if(txtAjaxDocUrl.Text.Length == 0)
            {
                txtAjaxDocUrl.SetValidationState(false, "An AjaxDoc URL is required");
                isValid = false;
            }
            else
            {
                if(!Uri.TryCreate(txtAjaxDocUrl.Text, UriKind.RelativeOrAbsolute, out _))
                {
                    txtAjaxDocUrl.SetValidationState(false, "The AjaxDoc URL does not appear to be valid");
                    isValid = false;
                }
            }

            if(txtProjectName.Text.Length == 0)
            {
                txtProjectName.SetValidationState(false, "A project filename is required");
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

            if(Uri.TryCreate(txtProxyServer.Text, UriKind.RelativeOrAbsolute, out Uri proxyServer))
                proxyServer = null;

            if(chkUseProxyServer.IsChecked.Value)
            {
                if(txtProxyServer.Text.Length == 0)
                {
                    txtProxyServer.SetValidationState(false, "A proxy server is required if one is used");
                    isValid = false;
                }
                else
                {
                    if(proxyServer == null)
                    {
                        txtProxyServer.SetValidationState(false, "The proxy server name does not appear to be valid");
                        isValid = false;
                    }
                }

                if(!chkUseProxyDefCreds.IsChecked.Value)
                {
                    if(txtProxyUserName.Text.Length == 0)
                    {
                        txtProxyUserName.SetValidationState(false, "A user name is required if not using " +
                            "default credentials");
                        isValid = false;
                    }

                    if(txtProxyPassword.Text.Length == 0)
                    {
                        txtProxyPassword.SetValidationState(false, "A password is required if not using default " +
                            "credentials");
                        isValid = false;
                    }
                }
            }

            if(!isValid)
                return;

            var userCreds = new UserCredentials(chkUseDefaultCredentials.IsChecked.Value, txtUserName.Text,
                txtPassword.Text);
            var proxyCreds = new ProxyCredentials(chkUseProxyServer.IsChecked.Value, proxyServer,
                new UserCredentials(chkUseProxyDefCreds.IsChecked.Value, txtProxyUserName.Text, txtProxyPassword.Text));

            configuration.RemoveNodes();

            configuration.Add(
                new XElement("ajaxDoc",
                    new XAttribute("url", txtAjaxDocUrl.Text),
                    new XAttribute("project", txtProjectName.Text),
                    new XAttribute("regenerate", chkRegenerateFiles.IsChecked.Value)),
                userCreds.ToXml(),
                proxyCreds.ToXml());

            this.DialogResult = true;
            this.Close();
        }
        #endregion
    }
}
