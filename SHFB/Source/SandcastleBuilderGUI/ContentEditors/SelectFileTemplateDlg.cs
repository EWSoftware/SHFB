//=============================================================================
// System  : Sandcastle Help File Builder
// File    : SelectFileTemplateDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/29/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to select a template file to use as a new
// topic.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.3  12/24/2011  EFW  Created the code
//=============================================================================

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Windows.Forms;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This is used to select a template file to add as a new topic
    /// </summary>
    public partial class SelectFileTemplateDlg : Form
    {
        #region Private data members
        //=====================================================================

        private string filePath;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This returns the new filename if the topic file is created
        /// </summary>
        public string NewFilename
        {
            get { return Path.Combine(filePath, txtNewFilename.Text.Trim() + lblExtension.Text); }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isConceptualContent">True to select a conceptual content template, false to
        /// select an HTML template.</param>
        /// <param name="filePath">The new file's path</param>
        public SelectFileTemplateDlg(bool isConceptualContent, string filePath)
        {
            InitializeComponent();

            this.filePath = filePath;

            if(isConceptualContent)
            {
                lblExtension.Text = ".aml";
                this.AddConceptualContentTemplates();
            }
            else
            {
                lblExtension.Text = ".html";
                this.AddAdditionalContentTemplates();
            }

            tvTemplates.ExpandAll();
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Add conceptual content templates to the tree view
        /// </summary>
        private void AddConceptualContentTemplates()
        {
            TreeNode root, child, defaultSelection = null;
            string name;

            root = tvTemplates.Nodes.Add("Standard Templates");

            // Add the topic templates to the New Topic context menu
            foreach(string file in Directory.EnumerateFiles(Path.Combine(Path.GetDirectoryName(
              Assembly.GetExecutingAssembly().Location), "ConceptualTemplates"), "*.aml"))
            {
                name = Path.GetFileNameWithoutExtension(file);
                child = root.Nodes.Add(name);
                child.Tag = file;

                // For Conceptual.aml, make it the default selection
                if(name == "Conceptual")
                    defaultSelection = child;
            }

            if(root.Nodes.Count == 0)
                root.Nodes.Add("No standard templates found");

            // Look for custom templates in the local application data folder
            root = tvTemplates.Nodes.Add("Custom Templates");

            name = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Constants.ConceptualTemplates);

            if(Directory.Exists(name))
                foreach(string file in Directory.EnumerateFiles(name, "*.aml"))
                {
                    name = Path.GetFileNameWithoutExtension(file);
                    child = root.Nodes.Add(name);
                    child.Tag = file;
                }

            if(root.Nodes.Count == 0)
                root.Nodes.Add("No custom templates found");

            if(defaultSelection != null)
                tvTemplates.SelectedNode = defaultSelection;
            else
                tvTemplates.SelectedNode = tvTemplates.Nodes[0];
        }

        /// <summary>
        /// Add additional content templates to the tree view
        /// </summary>
        private void AddAdditionalContentTemplates()
        {
            TreeNode root, child, defaultSelection = null;
            string name;

            root = tvTemplates.Nodes.Add("Standard Templates");

            // Add the topic templates to the New Topic context menu
            foreach(string file in Directory.EnumerateFiles(Path.Combine(Path.GetDirectoryName(
              Assembly.GetExecutingAssembly().Location), "ItemTemplates"), "*.htm?"))
            {
                name = Path.GetFileNameWithoutExtension(file);
                child = root.Nodes.Add(name);
                child.Tag = file;

                // For Html Page.html, make it the default selection
                if(name == "Html Page")
                    defaultSelection = child;
            }

            if(root.Nodes.Count == 0)
                root.Nodes.Add("No standard templates found");

            // Look for custom templates in the local application data folder
            root = tvTemplates.Nodes.Add("Custom Templates");

            name = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Constants.ItemTemplates);

            if(Directory.Exists(name))
                foreach(string file in Directory.EnumerateFiles(name, "*.htm?"))
                {
                    name = Path.GetFileNameWithoutExtension(file);
                    child = root.Nodes.Add(name);
                    child.Tag = file;
                }

            if(root.Nodes.Count == 0)
                root.Nodes.Add("No custom templates found");

            if(defaultSelection != null)
                tvTemplates.SelectedNode = defaultSelection;
            else
                tvTemplates.SelectedNode = tvTemplates.Nodes[0];
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Set the default filename when a template is selected
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvTemplates_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string templateName = null;

            if(tvTemplates.SelectedNode != null)
                templateName = tvTemplates.SelectedNode.Tag as string;

            if(templateName != null)
            {
                txtNewFilename.Text = Path.GetFileNameWithoutExtension(templateName);
                txtNewFilename.Enabled = btnAddTopic.Enabled = true;
            }
            else
            {
                txtNewFilename.Text = null;
                txtNewFilename.Enabled = btnAddTopic.Enabled = false;
            }
        }

        /// <summary>
        /// Create the new file from the selected template
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAddTopic_Click(object sender, EventArgs e)
        {
            string newFilename, templateFilename = (string)tvTemplates.SelectedNode.Tag;

            if(txtNewFilename.Text.Trim().Length == 0)
                tvTemplates_AfterSelect(sender, new TreeViewEventArgs(tvTemplates.SelectedNode));

            newFilename = this.NewFilename;

            if(File.Exists(newFilename) && MessageBox.Show(String.Format(CultureInfo.CurrentCulture,
              "The file '{0}' already exists.  Do you want to overwrite it?", newFilename),
              Constants.AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
              MessageBoxDefaultButton.Button2) == DialogResult.No)
                return;

            try
            {
                File.Copy(templateFilename, newFilename, true);

                if(lblExtension.Text == ".aml")
                {
                    // Set a unique ID in new MAML topics
                    XmlDocument doc = new XmlDocument();
                    doc.Load(newFilename);

                    XmlNode node = doc.SelectSingleNode("topic");

                    if(node == null)
                        throw new InvalidOperationException("Unable to locate root topic node to set new ID");

                    if(node.Attributes["id"] == null)
                        throw new InvalidOperationException("Unable to locate 'id' attribute on root topic " +
                            "node to set new ID");

                    node.Attributes["id"].Value = Guid.NewGuid().ToString();
                    doc.Save(newFilename);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to create new topic file.  Reason:" + ex.Message, Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}
