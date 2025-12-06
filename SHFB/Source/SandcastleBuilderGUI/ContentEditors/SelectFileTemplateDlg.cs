//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : SelectFileTemplateDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/26/2025
// Note    : Copyright 2011-2025, Eric Woodruff, All rights reserved
//
// This file contains the form used to select a template file to use as a new topic
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/24/2011  EFW  Created the code
//===============================================================================================================

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

using Sandcastle.Core;
using Sandcastle.Core.Markdown;

namespace SandcastleBuilder.Gui.ContentEditors;

/// <summary>
/// This is used to select a template file to add as a new topic
/// </summary>
public partial class SelectFileTemplateDlg : Form
{
    #region Private data members
    //=====================================================================

    private readonly string filePath;

    #endregion

    #region Properties
    //=====================================================================

    /// <summary>
    /// This returns the new filename if the topic file is created
    /// </summary>
    public string NewFilename => Path.Combine(filePath, txtNewFilename.Text.Trim() + (string)txtNewFilename.Tag);

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
            lblExtension.Text = ".md/.aml";
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
        string name, templatePath = Path.Combine(Path.GetDirectoryName(
          Assembly.GetExecutingAssembly().Location), "ConceptualTemplates");

        root = tvTemplates.Nodes.Add("Standard Templates");

        // Add the topic templates to the New Topic context menu
        foreach(string file in Directory.EnumerateFiles(templatePath, "*.md").Concat(
          Directory.EnumerateFiles(templatePath, "*.aml")))
        {
            name = Path.GetFileNameWithoutExtension(file);
            child = root.Nodes.Add(name);
            child.Tag = file;

            if(name.Equals("Conceptual Topic", StringComparison.OrdinalIgnoreCase))
                defaultSelection = child;
        }

        if(root.Nodes.Count == 0)
            root.Nodes.Add("No standard templates found");

        // Look for custom templates in the local application data folder
        root = tvTemplates.Nodes.Add("Custom Templates");

        name = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            ProjectExplorerWindow.ConceptualTemplates);

        if(Directory.Exists(name))
        {
            foreach(string file in Directory.EnumerateFiles(name, "*.md").Concat(
              Directory.EnumerateFiles(name, "*.aml")))
            {
                name = Path.GetFileNameWithoutExtension(file);
                child = root.Nodes.Add(name);
                child.Tag = file;
            }
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
            ProjectExplorerWindow.ItemTemplates);

        if(Directory.Exists(name))
        {
            foreach(string file in Directory.EnumerateFiles(name, "*.htm?"))
            {
                name = Path.GetFileNameWithoutExtension(file);
                child = root.Nodes.Add(name);
                child.Tag = file;
            }
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
            txtNewFilename.Tag = Path.GetExtension(templateName);
            txtNewFilename.Enabled = btnAddTopic.Enabled = true;
        }
        else
        {
            txtNewFilename.Text = null;
            txtNewFilename.Tag = null;
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
        {
            return;
        }

        try
        {
            File.Copy(templateFilename, newFilename, true);

            if(Path.GetExtension(newFilename).Equals(".aml", StringComparison.OrdinalIgnoreCase))
            {
                // Set a unique ID in new MAML topics
                XmlDocument doc = new();

                using(var reader = XmlReader.Create(newFilename, new XmlReaderSettings { CloseInput = true }))
                {
                    doc.Load(reader);
                }

                XmlNode node = doc.SelectSingleNode("topic") ??
                    throw new InvalidOperationException("Unable to locate root topic node to set new ID");

                if(node.Attributes["id"] == null)
                {
                    throw new InvalidOperationException("Unable to locate 'id' attribute on root topic " +
                        "node to set new ID");
                }

                node.Attributes["id"].Value = Guid.NewGuid().ToString();
                doc.Save(newFilename);
            }
            else
            {
                if(Path.GetExtension(newFilename).Equals(".md", StringComparison.OrdinalIgnoreCase))
                {
                    string content = File.ReadAllText(newFilename),
                        name = Path.GetFileNameWithoutExtension(newFilename);

                    content = content.Replace("{uid}", MarkdownFile.GenerateIdFromTitle(name)).Replace("{title}", name);

                    File.WriteAllText(newFilename, content);
                }
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
