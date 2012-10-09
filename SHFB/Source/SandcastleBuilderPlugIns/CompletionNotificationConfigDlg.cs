//=============================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : CompletionNotificationConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/09/2008
// Note    : Copyright 2007-2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a form that is used to configure the settings for the
// Completion Notification plug-in.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.2.0  09/10/2007  EFW  Created the code
// 1.6.0.6  03/09/2008  EFW  Added support for log file XSL transform
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
    /// <see cref="CompletionNotificationPlugIn"/>.
    /// </summary>
    internal partial class CompletionNotificationConfigDlg : Form
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
        public CompletionNotificationConfigDlg(string currentConfig)
        {
            XPathNavigator navigator, root, node;
            UserCredentials credentials;
            string attrValue;
            int port;

            InitializeComponent();

            lnkCodePlexSHFB.Links[0].LinkData = "http://SHFB.CodePlex.com";

            // Load the current settings
            config = new XmlDocument();
            config.LoadXml(currentConfig);
            navigator = config.CreateNavigator();

            root = navigator.SelectSingleNode("configuration");

            if(root.IsEmptyElement)
                return;

            node = root.SelectSingleNode("smtpServer");
            if(node != null)
            {
                txtSmtpServer.Text = node.GetAttribute("host", String.Empty);

                attrValue = node.GetAttribute("port", String.Empty);
                if(attrValue.Length != 0)
                    if(Int32.TryParse(attrValue, out port))
                        udcSmtpPort.Value = port;
            }

            credentials = UserCredentials.FromXPathNavigator(root);
            chkUseDefaultCredentials.Checked = credentials.UseDefaultCredentials;
            txtUserName.Text = credentials.UserName;
            txtPassword.Text = credentials.Password;

            node = root.SelectSingleNode("fromEMail");
            if(node != null)
                txtFromEMail.Text = node.GetAttribute("address", String.Empty);

            node = root.SelectSingleNode("successEMail");
            if(node != null)
            {
                txtSuccessEMailAddress.Text = node.GetAttribute("address",
                    String.Empty);

                attrValue = node.GetAttribute("attachLog", string.Empty);
                if(attrValue.Length != 0)
                    chkAttachLogFileOnSuccess.Checked =
                        Convert.ToBoolean(attrValue,
                            CultureInfo.InvariantCulture);
            }

            node = root.SelectSingleNode("failureEMail");
            if(node != null)
            {
                txtFailureEMailAddress.Text = node.GetAttribute("address",
                    String.Empty);

                attrValue = node.GetAttribute("attachLog", String.Empty);
                if(attrValue.Length != 0)
                    chkAttachLogFileOnFailure.Checked =
                        Convert.ToBoolean(attrValue,
                            CultureInfo.InvariantCulture);
            }

            node = root.SelectSingleNode("xslTransform");
            if(node != null)
                txtXSLTransform.Text = node.GetAttribute("filename",
                    String.Empty);
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
        private void lnkCodePlexSHFB_LinkClicked(object sender,
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
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            XmlAttribute attr;
            XmlNode root, node;
            UserCredentials credentials;
            bool isValid = true;

            txtSmtpServer.Text = txtSmtpServer.Text.Trim();
            txtUserName.Text = txtUserName.Text.Trim();
            txtPassword.Text = txtPassword.Text.Trim();
            txtSuccessEMailAddress.Text = txtSuccessEMailAddress.Text.Trim();
            txtFailureEMailAddress.Text = txtFailureEMailAddress.Text.Trim();
            txtXSLTransform.Text = txtXSLTransform.Text.Trim();
            epErrors.Clear();

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

            if(txtFailureEMailAddress.Text.Length == 0)
            {
                epErrors.SetError(txtFailureEMailAddress, "A failure " +
                    "e-mail address is required");
                isValid = false;
            }

            if(!isValid)
                return;


            // Store the changes
            root = config.SelectSingleNode("configuration");

            node = root.SelectSingleNode("smtpServer");
            if(node == null)
            {
                node = config.CreateNode(XmlNodeType.Element,
                    "smtpServer", null);
                root.AppendChild(node);

                attr = config.CreateAttribute("host");
                node.Attributes.Append(attr);
                attr = config.CreateAttribute("port");
                node.Attributes.Append(attr);
            }

            node.Attributes["host"].Value = txtSmtpServer.Text;
            node.Attributes["port"].Value = udcSmtpPort.Value.ToString(
                CultureInfo.InvariantCulture);

            credentials = new UserCredentials(chkUseDefaultCredentials.Checked,
                txtUserName.Text, txtPassword.Text);
            credentials.ToXml(config, root);

            node = root.SelectSingleNode("fromEMail");
            if(node == null)
            {
                node = config.CreateNode(XmlNodeType.Element,
                    "fromEMail", null);
                root.AppendChild(node);

                attr = config.CreateAttribute("address");
                node.Attributes.Append(attr);
            }

            node.Attributes["address"].Value = txtFromEMail.Text;

            node = root.SelectSingleNode("successEMail");
            if(node == null)
            {
                node = config.CreateNode(XmlNodeType.Element,
                    "successEMail", null);
                root.AppendChild(node);

                attr = config.CreateAttribute("address");
                node.Attributes.Append(attr);
                attr = config.CreateAttribute("attachLog");
                node.Attributes.Append(attr);
            }

            node.Attributes["address"].Value = txtSuccessEMailAddress.Text;
            node.Attributes["attachLog"].Value =
                chkAttachLogFileOnSuccess.Checked.ToString().ToLower(
                    CultureInfo.InvariantCulture);

            node = root.SelectSingleNode("failureEMail");
            if(node == null)
            {
                node = config.CreateNode(XmlNodeType.Element,
                    "failureEMail", null);
                root.AppendChild(node);

                attr = config.CreateAttribute("address");
                node.Attributes.Append(attr);
                attr = config.CreateAttribute("attachLog");
                node.Attributes.Append(attr);
            }

            node.Attributes["address"].Value = txtFailureEMailAddress.Text;
            node.Attributes["attachLog"].Value =
                chkAttachLogFileOnFailure.Checked.ToString().ToLower(
                    CultureInfo.InvariantCulture);

            node = root.SelectSingleNode("xslTransform");
            if(node == null)
            {
                node = config.CreateNode(XmlNodeType.Element,
                    "xslTransform", null);
                root.AppendChild(node);

                attr = config.CreateAttribute("filename");
                node.Attributes.Append(attr);
            }

            node.Attributes["filename"].Value = txtXSLTransform.Text;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
