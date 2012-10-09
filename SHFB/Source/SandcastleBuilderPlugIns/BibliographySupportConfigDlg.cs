//=============================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : BibliographySupportConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/07/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a form that is used to configure the settings for the
// bibliography support plug-in.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.5  02/18/2008  EFW  Created the code
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
    /// <see cref="BibliographySupportPlugIn"/>.
    /// </summary>
    internal partial class BibliographySupportConfigDlg : Form
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
        public BibliographySupportConfigDlg(string currentConfig)
        {
            XPathNavigator navigator, root, node;

            InitializeComponent();

            lnkCodePlexSHFB.Links[0].LinkData = "http://SHFB.CodePlex.com";

            // Load the current settings
            config = new XmlDocument();
            config.LoadXml(currentConfig);
            navigator = config.CreateNavigator();

            root = navigator.SelectSingleNode("configuration");

            if(root.IsEmptyElement)
                return;

            node = root.SelectSingleNode("bibliography");
            if(node != null)
                txtBibliographyFile.Text = node.GetAttribute("path", String.Empty);
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
        /// Select the path to the bibliography file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnSelectLocation_Click(object sender, EventArgs e)
        {
            using(OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Select the bibliography file";
                dlg.Filter = "Bibliography files (*.xml)|*.xml|All Files (*.*)|*.*";
                dlg.InitialDirectory = Directory.GetCurrentDirectory();
                dlg.DefaultExt = "xml";

                // If one is selected, use that file
                if(dlg.ShowDialog() == DialogResult.OK)
                    txtBibliographyFile.Text = dlg.FileName;
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
            bool isValid = true;

            txtBibliographyFile.Text = txtBibliographyFile.Text.Trim();
            epErrors.Clear();

            if(txtBibliographyFile.Text.Length == 0)
            {
                epErrors.SetIconPadding(txtBibliographyFile, 35);
                epErrors.SetError(txtBibliographyFile,
                    "The path to the bibliography XML file is required");
                isValid = false;
            }

            if(!isValid)
                return;

            // Store the changes
            root = config.SelectSingleNode("configuration");

            node = root.SelectSingleNode("bibliography");
            if(node == null)
            {
                node = config.CreateNode(XmlNodeType.Element,
                    "bibliography", null);
                root.AppendChild(node);

                attr = config.CreateAttribute("path");
                node.Attributes.Append(attr);
            }

            node.Attributes["path"].Value = txtBibliographyFile.Text;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
