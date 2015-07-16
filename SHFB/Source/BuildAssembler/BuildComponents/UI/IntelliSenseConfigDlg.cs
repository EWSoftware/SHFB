//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : IntelliSenseConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/21/2012
// Note    : Copyright 2006-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a form that is used to configure the settings for the IntelliSense build component
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.6.0.2  11/07/2007  EFW  Created the code
// 2.7.3.0  12/21/2012  EFW  Moved the configuration dialog into the Sandcastle build components assembly
//===============================================================================================================

using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;

using Microsoft.Ddue.Tools.BuildComponent;

namespace Microsoft.Ddue.Tools.UI
{
    /// <summary>
    /// This form is used to configure the settings for the
    /// <see cref="IntelliSenseComponent"/>.
    /// </summary>
    internal partial class IntelliSenseConfigDlg : Form
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
        /// <param name="currentConfig">The current XML configuration XML fragment</param>
        public IntelliSenseConfigDlg(string currentConfig)
        {
            XmlNode component, node;
            XmlAttribute attr;
            bool itemChecked;

            InitializeComponent();

            lnkProjectSite.Links[0].LinkData = "https://GitHub.com/EWSoftware/SHFB";

            // Load the current settings
            config = new XmlDocument();
            config.LoadXml(currentConfig);

            component = config.SelectSingleNode("component");
            node = component.SelectSingleNode("output");

            if(node != null)
            {
                attr = node.Attributes["includeNamespaces"];

                if(attr != null && Boolean.TryParse(attr.Value, out itemChecked))
                    chkIncludeNamespaces.Checked = itemChecked;

                attr = node.Attributes["namespacesFile"];

                if(attr != null)
                    txtNamespacesFile.Text = attr.Value;

                attr = node.Attributes["folder"];

                if(attr != null)
                    txtFolder.Text = attr.Value;
            }
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
        /// Go to the Sandcastle Help File Builder project site
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lnkProjectSite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start((string)e.Link.LinkData);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show("Unable to launch link target.  Reason: " + ex.Message,
                    "IntelliSense Component", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
            XmlNode component, node;

            txtFolder.Text = txtFolder.Text.Trim();
            txtNamespacesFile.Text = txtNamespacesFile.Text.Trim();

            // Store the changes
            component = config.SelectSingleNode("component");
            node = component.SelectSingleNode("output");

            if(node == null)
            {
                node = config.CreateNode(XmlNodeType.Element, "output", null);
                component.AppendChild(node);
            }

            attr = node.Attributes["includeNamespaces"];

            if(attr == null)
            {
                attr = config.CreateAttribute("includeNamespaces");
                node.Attributes.Append(attr);
            }

            attr.Value = chkIncludeNamespaces.Checked.ToString().ToLowerInvariant();
            attr = node.Attributes["namespacesFile"];

            if(attr == null)
            {
                attr = config.CreateAttribute("namespacesFile");
                node.Attributes.Append(attr);
            }

            attr.Value = txtNamespacesFile.Text;

            attr = node.Attributes["folder"];

            if(attr == null)
            {
                attr = config.CreateAttribute("folder");
                node.Attributes.Append(attr);
            }

            attr.Value = txtFolder.Text;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Select the base source folder
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using(FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select the IntelliSense output folder";
                dlg.SelectedPath = Directory.GetCurrentDirectory();

                // If selected, set the new folder
                if(dlg.ShowDialog() == DialogResult.OK)
                    txtFolder.Text = dlg.SelectedPath + @"\";
            }
        }
        #endregion
    }
}
