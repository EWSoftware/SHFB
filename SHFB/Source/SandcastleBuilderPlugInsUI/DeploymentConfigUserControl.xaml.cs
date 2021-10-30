//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : DeploymentConfigUserControl.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/10/2021
// Note    : Copyright 2009-2021, Eric Woodruff, All rights reserved
//
// This file contains a user control used to edit deployment configuration settings
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/05/2009  EFW  Moved deployment config controls to user control
// 05/10/2021  EFW  Converted the form to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.Windows;
using System.Windows.Controls;

using Sandcastle.Platform.Windows;

using SandcastleBuilder.Utils.BuildComponent;

namespace SandcastleBuilder.PlugIns.UI
{
    /// <summary>
    /// This user control is used to edit deployment configuration settings
    /// </summary>
    public partial class DeploymentConfigUserControl : UserControl
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public DeploymentConfigUserControl()
        {
            InitializeComponent();

            this.chkUseProxyServer_CheckedChanged(this, null);

            chkUseDefaultCredentials.IsChecked = chkUseProxyDefCreds.IsChecked = true;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Load the control values from the specified deployment location
        /// settings object.
        /// </summary>
        /// <param name="location">The location settings</param>
        public void LoadFromSettings(DeploymentLocation location)
        {
            if(location == null)
                throw new ArgumentNullException(nameof(location));

            txtTargetLocation.Text = location.Location?.OriginalString;

            chkUseDefaultCredentials.IsChecked = location.UserCredentials.UseDefaultCredentials;
            txtUserName.Text = location.UserCredentials.UserName;
            txtPassword.Text = location.UserCredentials.Password;

            chkUseProxyServer.IsChecked = location.ProxyCredentials.UseProxyServer;
            txtProxyServer.Text = location.ProxyCredentials.ProxyServer?.OriginalString;
            chkUseProxyDefCreds.IsChecked = location.ProxyCredentials.Credentials.UseDefaultCredentials;
            txtProxyUserName.Text = location.ProxyCredentials.Credentials.UserName;
            txtProxyPassword.Text = location.ProxyCredentials.Credentials.Password;
        }

        /// <summary>
        /// Validate the control values and, if valid, create and return a new
        /// deployment location settings object.
        /// </summary>
        /// <returns>The deployment location settings if they are valid or
        /// null if they are not valid.</returns>
        public DeploymentLocation CreateDeploymentLocation()
        {
            DeploymentLocation location = null;
            bool isValid = true;

            foreach(var c in new[] { txtTargetLocation, txtUserName, txtPassword, txtProxyServer, txtProxyUserName,
              txtProxyPassword })
            {
                c.Text = c.Text.Trim();
                c.SetValidationState(true, null);
            }

            if(txtTargetLocation.Text.Length == 0 || !Uri.TryCreate(txtTargetLocation.Text,
              UriKind.RelativeOrAbsolute, out Uri targetUri))
            {
                targetUri = null;
            }

            if(txtTargetLocation.Text.Length != 0 && targetUri == null)
            {
                txtTargetLocation.SetValidationState(false, "The target location does not appear to be valid");
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

            if(!Uri.TryCreate(txtProxyServer.Text, UriKind.RelativeOrAbsolute, out Uri proxyServer))
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

            if(isValid)
            {
                var userCreds = new UserCredentials(chkUseDefaultCredentials.IsChecked.Value, txtUserName.Text,
                    txtPassword.Text);
                var proxyCreds = new ProxyCredentials(chkUseProxyServer.IsChecked.Value, proxyServer,
                    new UserCredentials(chkUseProxyDefCreds.IsChecked.Value, txtProxyUserName.Text,
                        txtProxyPassword.Text));
                location = new DeploymentLocation(targetUri, userCreds, proxyCreds);
            }

            return location;
        }
        #endregion

        #region Event handlers
        //=====================================================================

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
        #endregion
    }
}
