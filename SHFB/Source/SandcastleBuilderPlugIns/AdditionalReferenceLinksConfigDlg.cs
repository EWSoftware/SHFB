//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : AdditionalReferenceLinksConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/17/2011
// Note    : Copyright 2008-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to edit the additional reference links
// plug-in configuration.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.5  02/25/2008  EFW  Created the code
// 1.8.0.0  08/13/2008  EFW  Updated to support the new project format
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
    /// This form is used to edit the <see cref="AdditionalReferenceLinksPlugIn"/>
    /// configuration.
    /// </summary>
    internal partial class AdditionalReferenceLinksConfigDlg : Form
    {
        #region Private data members
        //=====================================================================

        private ReferenceLinkSettingsCollection items;
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
        public AdditionalReferenceLinksConfigDlg(SandcastleProject currentProject,
          string currentConfig)
        {
            XPathNavigator navigator, root;

            InitializeComponent();
            project = currentProject;

            lnkCodePlexSHFB.Links[0].LinkData = "http://SHFB.CodePlex.com";
            lbReferences.DisplayMember = lbReferences.ValueMember =
                "ListDescription";

            items = new ReferenceLinkSettingsCollection();

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
                foreach(ReferenceLinkSettings rl in items)
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
        /// Add a new help file builder project to the reference link settings.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAddFile_Click(object sender, EventArgs e)
        {
            ReferenceLinkSettings newItem;
            int idx = 0;

            using(OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Select the help file builder project(s)";
                dlg.Filter = "Sandcastle Help File Builder Project Files " +
                    "(*.shfbproj)|*.shfbproj|All Files (*.*)|*.*";
                dlg.InitialDirectory = Directory.GetCurrentDirectory();
                dlg.DefaultExt = "shfbproj";
                dlg.Multiselect = true;

                // If selected, add the file(s)
                if(dlg.ShowDialog() == DialogResult.OK)
                {
                    foreach(string file in dlg.FileNames)
                    {
                        newItem = new ReferenceLinkSettings();
                        newItem.HelpFileProject = new FilePath(file, project);

                        // It will end up on the last one added
                        idx = lbReferences.Items.Add(newItem);
                    }

                    pgProps.Enabled = btnDelete.Enabled = true;
                    lbReferences.SelectedIndex = idx;
                }
            }
        }

        /// <summary>
        /// Delete a reference links item
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
                ReferenceLinkSettings rl =
                    (ReferenceLinkSettings)lbReferences.SelectedItem;
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
            List<string> projects = new List<string>();
            ReferenceLinkSettings rl;
            XmlNode root;
            bool isValid = true;

            epErrors.Clear();
            epErrors.SetIconAlignment(lbReferences,
                ErrorIconAlignment.BottomRight);

            items.Clear();

            for(int idx = 0; idx < lbReferences.Items.Count; idx++)
            {
                rl = (ReferenceLinkSettings)lbReferences.Items[idx];

                // There can't be duplicate IDs or projects
                if(projects.Contains(rl.HelpFileProject))
                {
                    epErrors.SetError(lbReferences, "Project filenames must " +
                        "be unique");
                    isValid = false;
                    break;
                }

                items.Add(rl);
                projects.Add(rl.HelpFileProject);
            }

            if(!isValid)
                return;

            // Store the changes
            root = config.SelectSingleNode("configuration");

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
