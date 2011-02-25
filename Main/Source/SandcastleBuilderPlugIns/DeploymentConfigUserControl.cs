//=============================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : DeploymentConfigUserControl.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/06/2009
// Note    : Copyright 2009, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a user control used to edit deployment configuration
// settings.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.3  07/05/2009  EFW  Moved deployment config controls to user control
//=============================================================================

using System;
using System.Windows.Forms;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This user control is used to edit deployment configuration settings
    /// </summary>
    internal partial class DeploymentConfigUserControl : UserControl
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public DeploymentConfigUserControl()
        {
            InitializeComponent();
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
            txtTargetLocation.Text = (location.Location != null) ?
                location.Location.OriginalString : null;
            chkUseDefaultCredentials.Checked =
                location.UserCredentials.UseDefaultCredentials;
            txtUserName.Text = location.UserCredentials.UserName;
            txtPassword.Text = location.UserCredentials.Password;

            chkUseProxyServer.Checked =
                location.ProxyCredentials.UseProxyServer;
            txtProxyServer.Text =
                (location.ProxyCredentials.ProxyServer == null) ? null :
                location.ProxyCredentials.ProxyServer.OriginalString;
            chkUseProxyDefCreds.Checked =
                location.ProxyCredentials.Credentials.UseDefaultCredentials;
            txtProxyUserName.Text =
                location.ProxyCredentials.Credentials.UserName;
            txtProxyPassword.Text =
                location.ProxyCredentials.Credentials.Password;
        }

        /// <summary>
        /// Validate the control values and, if valid, create and return a new
        /// deployment location settings object.
        /// </summary>
        /// <returns>The deployment location settings if they are valud or
        /// null if they are not valid.</returns>
        public DeploymentLocation CreateDeploymentLocation()
        {
            DeploymentLocation location = null;
            UserCredentials userCreds;
            ProxyCredentials proxyCreds;
            Uri targetUri = null, proxyUri = null;
            bool isValid = true;

            epErrors.Clear();

            txtTargetLocation.Text = txtTargetLocation.Text.Trim();
            txtUserName.Text = txtUserName.Text.Trim();
            txtPassword.Text = txtPassword.Text.Trim();
            txtProxyServer.Text = txtProxyServer.Text.Trim();
            txtProxyUserName.Text = txtProxyUserName.Text.Trim();
            txtProxyPassword.Text = txtProxyPassword.Text.Trim();

            if(txtTargetLocation.Text.Length != 0 && !Uri.TryCreate(
              txtTargetLocation.Text, UriKind.RelativeOrAbsolute, out targetUri))
            {
                epErrors.SetError(txtTargetLocation, "The target location " +
                    "does not appear to be valid");
                isValid = false;
            }

            if(!chkUseDefaultCredentials.Checked)
            {
                if(txtUserName.Text.Length == 0)
                {
                    epErrors.SetError(txtUserName, "A user name is required " +
                        "if not using default credentials");
                    isValid = false;
                }

                if(txtPassword.Text.Length == 0)
                {
                    epErrors.SetError(txtPassword, "A password is required " +
                        "if not using default credentials");
                    isValid = false;
                }
            }

            if(chkUseProxyServer.Checked)
            {
                if(txtProxyServer.Text.Length == 0)
                {
                    epErrors.SetError(txtProxyServer, "A proxy server is " +
                        "required if one is used");
                    isValid = false;
                }
                else
                {
                    Uri.TryCreate(txtProxyServer.Text, UriKind.RelativeOrAbsolute,
                        out proxyUri);

                    if(proxyUri == null)
                    {
                        epErrors.SetError(txtProxyServer, "The proxy server " +
                            "name does not appear to be valid");
                        isValid = false;
                    }
                }

                if(!chkUseProxyDefCreds.Checked)
                {
                    if(txtProxyUserName.Text.Length == 0)
                    {
                        epErrors.SetError(txtProxyUserName, "A user name is " +
                            "required if not using default credentials");
                        isValid = false;
                    }

                    if(txtProxyPassword.Text.Length == 0)
                    {
                        epErrors.SetError(txtProxyPassword, "A password is " +
                            "required if not using default credentials");
                        isValid = false;
                    }
                }
            }

            if(isValid)
            {
                userCreds = new UserCredentials(chkUseDefaultCredentials.Checked,
                    txtUserName.Text, txtPassword.Text);
                proxyCreds = new ProxyCredentials(chkUseProxyServer.Checked,
                    proxyUri, new UserCredentials(chkUseProxyDefCreds.Checked,
                    txtProxyUserName.Text, txtProxyPassword.Text));
                location = new DeploymentLocation(targetUri, userCreds,
                    proxyCreds);
            }

            return location;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Enable or disable the user name and password controls based on
        /// the Default Credentials check state.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkUseDefaultCredentials_CheckedChanged(object sender, EventArgs e)
        {
            txtUserName.Enabled = txtPassword.Enabled =
                !chkUseDefaultCredentials.Checked;
        }

        /// <summary>
        /// Enable or disable the proxy server settings based on the Use
        /// Proxy Server check state.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkUseProxyServer_CheckedChanged(object sender, EventArgs e)
        {
            txtProxyServer.Enabled = chkUseProxyDefCreds.Enabled =
                txtProxyUserName.Enabled = txtProxyPassword.Enabled =
                chkUseProxyServer.Checked;

            if(chkUseProxyServer.Checked)
                chkUseProxyDefCreds_CheckedChanged(chkUseProxyDefCreds, e);
        }

        /// <summary>
        /// Enable or disable the proxy user credentials based on the Proxy
        /// Default Credentials check state.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkUseProxyDefCreds_CheckedChanged(object sender, EventArgs e)
        {
            if(chkUseProxyDefCreds.Enabled)
                txtProxyUserName.Enabled = txtProxyPassword.Enabled =
                    !chkUseProxyDefCreds.Checked;
        }
        #endregion
    }
}
