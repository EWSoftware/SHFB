//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : WildcardReferencesConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/17/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to edit the wildcard references plug-in
// configuration.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.2.0  01/17/2011  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This form is used to edit the <see cref="WildcardReferencesPlugIn"/>
    /// configuration.
    /// </summary>
    internal partial class WildcardReferencesConfigDlg : Form
    {
        #region Private data members
        //=====================================================================

        private XElement config;     // The configuration
        private SandcastleProject project;
        private WildcardReferenceSettingsCollection items;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to return the configuration information
        /// </summary>
        public string Configuration
        {
            get { return config.ToString(); }
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
        public WildcardReferencesConfigDlg(SandcastleProject currentProject, string currentConfig)
        {
            InitializeComponent();
            project = currentProject;

            lnkCodePlexSHFB.Links[0].LinkData = "http://SHFB.CodePlex.com";
            lbReferences.DisplayMember = lbReferences.ValueMember = "ListDescription";

            items = new WildcardReferenceSettingsCollection();

            // Load the current settings
            config = XElement.Parse(currentConfig);

            if(!config.IsEmpty)
                items.FromXml(project, config);

            if(items.Count == 0)
                pgProps.Enabled = btnDelete.Enabled = false;
            else
            {
                // Binding the collection to the list box caused some odd problems with the property grid so
                // we'll add the items to the list box directly.
                foreach(WildcardReferenceSettings rl in items)
                    lbReferences.Items.Add(rl);

                lbReferences.SelectedIndex = 0;
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
        /// Add a new folder to the wildcard reference settings.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAddReferencePath_Click(object sender, EventArgs e)
        {
            WildcardReferenceSettings newItem;

            using(FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select the folder for the new project";
                dlg.SelectedPath = Directory.GetCurrentDirectory();

                // If selected, add the file(s)
                if(dlg.ShowDialog() == DialogResult.OK)
                {
                    newItem = new WildcardReferenceSettings();
                    newItem.ReferencePath = new FolderPath(dlg.SelectedPath, project);
                    lbReferences.SelectedIndex = lbReferences.Items.Add(newItem);

                    pgProps.Enabled = btnDelete.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Delete a wildcard reference folder item
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            int idx = lbReferences.SelectedIndex;

            if(idx == -1)
                lbReferences.SelectedIndex = 0;
            else
            {
                lbReferences.Items.RemoveAt(idx);

                if(lbReferences.Items.Count == 0)
                    pgProps.Enabled = btnDelete.Enabled = false;
                else
                    if(idx < lbReferences.Items.Count)
                        lbReferences.SelectedIndex = idx;
                    else
                        lbReferences.SelectedIndex =
                            lbReferences.Items.Count - 1;
            }
        }

        /// <summary>
        /// Update the property grid with the selected item
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbReferences_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(lbReferences.SelectedItem != null)
            {
                WildcardReferenceSettings rl = (WildcardReferenceSettings)lbReferences.SelectedItem;
                pgProps.SelectedObject = rl;
            }
            else
                pgProps.SelectedObject = null;

            pgProps.Refresh();
        }

        /// <summary>
        /// Refresh the list box item when a property changes
        /// </summary>
        /// <param name="s">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void pgProps_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            lbReferences.Refresh(lbReferences.SelectedIndex);
        }

        /// <summary>
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            List<string> paths = new List<string>();
            WildcardReferenceSettings rl;

            epErrors.Clear();
            epErrors.SetIconAlignment(lbReferences, ErrorIconAlignment.BottomRight);

            items.Clear();

            for(int idx = 0; idx < lbReferences.Items.Count; idx++)
            {
                rl = (WildcardReferenceSettings)lbReferences.Items[idx];

                // There can't be duplicate IDs or projects
                if(paths.Contains(rl.ReferencePath))
                {
                    epErrors.SetError(lbReferences, "Reference paths must be unique");
                    return;
                }

                items.Add(rl);
                paths.Add(rl.ReferencePath);
            }

            // Store the changes
            items.ToXml(config);

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
