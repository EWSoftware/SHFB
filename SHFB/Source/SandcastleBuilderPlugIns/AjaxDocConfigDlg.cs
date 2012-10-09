//=============================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : AjaxDocConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/29/2008
// Note    : Copyright 2007-2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a form that is used to configure the settings for the
// AjaxDoc Builder plug-in.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.2.0  09/15/2007  EFW  Created the code
//=============================================================================

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This form is used to configure the settings for the
    /// <see cref="AjaxDocPlugIn"/>.
    /// </summary>
    internal partial class AjaxDocConfigDlg : Form
    {
        private XmlDocument config;     // The configuration

        /// <summary>
        /// This is used to return the configuration information
        /// </summary>
        public string Configuration
        {
            get { return config.OuterXml; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="currentConfig">The current XML configuration
        /// XML fragment</param>
        public AjaxDocConfigDlg(string currentConfig)
        {
            XPathNavigator navigator, root, node;
            UserCredentials userCreds;
            ProxyCredentials proxyCreds;

            InitializeComponent();

            lnkCodePlexSHFB.Links[0].LinkData = "http://SHFB.CodePlex.com";
            lnkCodePlexAjaxDoc.Links[0].LinkData = "http://AjaxDoc.CodePlex.com";

            // Load the current settings
            config = new XmlDocument();
            config.LoadXml(currentConfig);
            navigator = config.CreateNavigator();

            root = navigator.SelectSingleNode("configuration");

            if(root.IsEmptyElement)
                return;

            node = root.SelectSingleNode("ajaxDoc");
            if(node != null)
            {
                txtAjaxDocUrl.Text = node.GetAttribute("url", String.Empty);
                txtProjectName.Text = node.GetAttribute("project", String.Empty);
                chkRegenerateFiles.Checked = Convert.ToBoolean(
                    node.GetAttribute("regenerate", String.Empty),
                    CultureInfo.InvariantCulture);
            }

            userCreds = UserCredentials.FromXPathNavigator(root);
            chkUseDefaultCredentials.Checked = userCreds.UseDefaultCredentials;
            txtUserName.Text = userCreds.UserName;
            txtPassword.Text = userCreds.Password;

            proxyCreds = ProxyCredentials.FromXPathNavigator(root);
            chkUseProxyServer.Checked = proxyCreds.UseProxyServer;
            txtProxyServer.Text = (proxyCreds.ProxyServer == null) ? null :
                proxyCreds.ProxyServer.OriginalString;
            chkUseProxyDefCreds.Checked = proxyCreds.Credentials.UseDefaultCredentials;
            txtProxyUserName.Text = proxyCreds.Credentials.UserName;
            txtProxyPassword.Text = proxyCreds.Credentials.Password;
        }

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
        /// Enable or disable the user name and password controls based on
        /// the Default Credentials check state.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkUseDefaultCredentials_CheckedChanged(object sender,
            EventArgs e)
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
                chkUseProxyDefCreds_CheckedChanged(sender, e);
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

        /// <summary>
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            XmlAttribute attr;
            XmlNode root, node;
            UserCredentials userCreds;
            ProxyCredentials proxyCreds;
            bool isValid = true;
            Uri ajaxDoc = null, proxyServer = null;

            txtAjaxDocUrl.Text = txtAjaxDocUrl.Text.Trim();
            txtProjectName.Text = txtProjectName.Text.Trim();
            txtUserName.Text = txtUserName.Text.Trim();
            txtPassword.Text = txtPassword.Text.Trim();
            txtProxyServer.Text = txtProxyServer.Text.Trim();
            txtProxyUserName.Text = txtProxyUserName.Text.Trim();
            txtProxyPassword.Text = txtProxyPassword.Text.Trim();
            epErrors.Clear();

            if(txtAjaxDocUrl.Text.Length == 0)
            {
                epErrors.SetError(txtAjaxDocUrl, "An AjaxDoc URL is required");
                isValid = false;
            }
            else
                if(!Uri.TryCreate(txtAjaxDocUrl.Text,
                  UriKind.RelativeOrAbsolute, out ajaxDoc))
                {
                    epErrors.SetError(txtAjaxDocUrl, "The AjaxDoc URL does " +
                        "not appear to be valid");
                    isValid = false;
                }

            if(txtProjectName.Text.Length == 0)
            {
                epErrors.SetError(txtProjectName, "A project filename " +
                    "is required");
                isValid = false;
            }

            if(!chkUseDefaultCredentials.Checked)
            {
                if(txtUserName.Text.Length == 0)
                {
                    epErrors.SetError(txtUserName, "A user name is " +
                        "required if not using default credentials");
                    isValid = false;
                }

                if(txtPassword.Text.Length == 0)
                {
                    epErrors.SetError(txtPassword, "A password is " +
                        "required if not using default credentials");
                    isValid = false;
                }
            }

            Uri.TryCreate(txtProxyServer.Text, UriKind.RelativeOrAbsolute,
                out proxyServer);

            if(chkUseProxyServer.Checked)
            {
                if(txtProxyServer.Text.Length == 0)
                {
                    epErrors.SetError(txtProxyServer, "A proxy server is " +
                        "required if one is used");
                    isValid = false;
                }
                else
                    if(proxyServer == null)
                    {
                        epErrors.SetError(txtProxyServer, "The proxy server " +
                            "name does not appear to be valid");
                        isValid = false;
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

            if(!isValid)
                return;

            if(txtAjaxDocUrl.Text[txtAjaxDocUrl.Text.Length - 1] != '/')
                txtAjaxDocUrl.Text += "/";

            // Store the changes
            root = config.SelectSingleNode("configuration");

            node = root.SelectSingleNode("ajaxDoc");
            if(node == null)
            {
                node = config.CreateNode(XmlNodeType.Element,
                    "ajaxDoc", null);
                root.AppendChild(node);

                attr = config.CreateAttribute("url");
                node.Attributes.Append(attr);
                attr = config.CreateAttribute("project");
                node.Attributes.Append(attr);
                attr = config.CreateAttribute("regenerate");
                node.Attributes.Append(attr);
            }

            node.Attributes["url"].Value = txtAjaxDocUrl.Text;
            node.Attributes["project"].Value = txtProjectName.Text;
            node.Attributes["regenerate"].Value =
                chkRegenerateFiles.Checked.ToString().ToLower(
                    CultureInfo.InvariantCulture);

            userCreds = new UserCredentials(chkUseDefaultCredentials.Checked,
                txtUserName.Text, txtPassword.Text);
            userCreds.ToXml(config, root);

            proxyCreds = new ProxyCredentials(chkUseProxyServer.Checked,
                proxyServer, new UserCredentials(chkUseProxyDefCreds.Checked,
                txtProxyUserName.Text, txtProxyPassword.Text));
            proxyCreds.ToXml(config, root);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
