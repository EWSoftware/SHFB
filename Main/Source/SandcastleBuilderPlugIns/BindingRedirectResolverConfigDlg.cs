//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : BindingRedirectResolverConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/14/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to edit the assembly binding redirection
// resolver plug-in configuration.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.1  11/14/2008  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This form is used to edit the <see cref="BindingRedirectResolverPlugIn"/>
    /// configuration.
    /// </summary>
    internal partial class BindingRedirectResolverConfigDlg : Form
    {
        #region Private data members
        //=====================================================================

        private BindingRedirectSettingsCollection items;
        private XmlDocument config;     // The configuration
        private SandcastleProject project;
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
        /// <param name="currentProject">The current project</param>
        /// <param name="currentConfig">The current XML configuration
        /// XML fragment</param>
        public BindingRedirectResolverConfigDlg(SandcastleProject currentProject,
          string currentConfig)
        {
            XPathNavigator navigator, root;

            InitializeComponent();
            project = currentProject;

            lnkCodePlexSHFB.Links[0].LinkData = "http://SHFB.CodePlex.com";

            items = new BindingRedirectSettingsCollection();

            // Load the current settings
            config = new XmlDocument();
            config.LoadXml(currentConfig);
            navigator = config.CreateNavigator();

            root = navigator.SelectSingleNode("configuration");

            if(!root.IsEmptyElement)
                items.FromXml(project, root);

            if(items.Count == 0)
                pgProps.Enabled = btnDelete.Enabled = false;
            else
            {
                // Binding the collection to the list box caused some
                // odd problems with the property grid so we'll add the
                // items to the list box directly.
                foreach(BindingRedirectSettings brs in items)
                    lbRedirects.Items.Add(brs);

                lbRedirects.SelectedIndex = 0;
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
        /// Add a new setting item to the collection
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAddFile_Click(object sender, EventArgs e)
        {
            int idx = lbRedirects.Items.Add(
                new BindingRedirectSettings(project));

            pgProps.Enabled = btnDelete.Enabled = true;
            lbRedirects.SelectedIndex = idx;
        }

        /// <summary>
        /// Delete a binding settings item
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            int idx = lbRedirects.SelectedIndex;

            if(idx == -1)
                lbRedirects.SelectedIndex = 0;
            else
            {
                lbRedirects.Items.RemoveAt(idx);

                if(lbRedirects.Items.Count == 0)
                    pgProps.Enabled = btnDelete.Enabled = false;
                else
                    if(idx < lbRedirects.Items.Count)
                        lbRedirects.SelectedIndex = idx;
                    else
                        lbRedirects.SelectedIndex =
                            lbRedirects.Items.Count - 1;
            }
        }

        /// <summary>
        /// Update the property grid with the selected item
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbReferences_SelectedIndexChanged(object sender, EventArgs e)
        {
            pgProps.SelectedObject = lbRedirects.SelectedItem;
            pgProps.Refresh();
        }

        /// <summary>
        /// Refresh the list box item when a property changes
        /// </summary>
        /// <param name="s">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void pgProps_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            lbRedirects.Refresh(lbRedirects.SelectedIndex);
        }

        /// <summary>
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            XmlNode root;

            // Store the changes
            root = config.SelectSingleNode("configuration");

            items.Clear();

            foreach(BindingRedirectSettings brs in lbRedirects.Items)
                items.Add(brs);

            items.ToXml(config, root);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Launch the URL in the web browser
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lnkCodePlexSHFB_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
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
        #endregion
    }
}
