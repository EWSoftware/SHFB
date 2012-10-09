//=============================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : HierarchicalTocConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/17/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a form that is used to configure the settings for the
// Hierarchical TOC plug-in.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.6  03/17/2008  EFW  Created the code
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
    /// <see cref="HierarchicalTocPlugIn"/>.
    /// </summary>
    internal partial class HierarchicalTocConfigDlg : Form
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
        public HierarchicalTocConfigDlg(string currentConfig)
        {
            XPathNavigator navigator, root;
            string option;
            int minParts;

            InitializeComponent();

            lnkCodePlexSHFB.Links[0].LinkData = "http://SHFB.CodePlex.com";

            // Load the current settings
            config = new XmlDocument();
            config.LoadXml(currentConfig);
            navigator = config.CreateNavigator();

            root = navigator.SelectSingleNode("configuration/toc");
            if(root == null)
                return;

            option = root.GetAttribute("minParts", String.Empty);
            if(!String.IsNullOrEmpty(option))
            {
                minParts = Convert.ToInt32(option, CultureInfo.InvariantCulture);

                if(minParts < 1)
                    minParts = 1;

                udcMinParts.Value = minParts;
            }

            option = root.GetAttribute("insertBelow", String.Empty);
            if(!String.IsNullOrEmpty(option))
                chkInsertBelow.Checked = Convert.ToBoolean(option,
                    CultureInfo.InvariantCulture);
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
        /// Launch the URL in the web browser.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void project_LinkClicked(object sender,
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
            XmlNode root, node;

            // Store the changes
            root = config.SelectSingleNode("configuration");

            node = root.SelectSingleNode("toc");
            if(node == null)
            {
                node = config.CreateNode(XmlNodeType.Element,
                    "toc", null);
                root.AppendChild(node);

                attr = config.CreateAttribute("minParts");
                node.Attributes.Append(attr);
                attr = config.CreateAttribute("insertBelow");
                node.Attributes.Append(attr);
            }

            node.Attributes["minParts"].Value = udcMinParts.Value.ToString(
                CultureInfo.InvariantCulture);
            node.Attributes["insertBelow"].Value =
                chkInsertBelow.Checked.ToString();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
