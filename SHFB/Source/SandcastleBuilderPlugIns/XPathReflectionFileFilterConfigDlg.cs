//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : XPathReflectionFileFilterConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)  Based on code by Eyal Post
// Updated : 01/17/2011
// Note    : Copyright 2008-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to edit the XPath reflection file filter
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
// 1.8.0.1  10/31/2008  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This form is used to edit the <see cref="XPathReflectionFileFilterPlugIn"/>
    /// configuration.
    /// </summary>
    internal partial class XPathReflectionFileFilterConfigDlg : Form
    {
        #region Private data members
        //=====================================================================

        private XmlDocument config;     // The configuration
        private bool isDeleting;
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
        /// <param name="currentConfig">The current XML configuration
        /// XML fragment</param>
        public XPathReflectionFileFilterConfigDlg(string currentConfig)
        {
            XPathNavigator navigator, root;

            InitializeComponent();

            lnkCodePlexSHFB.Links[0].LinkData = "http://SHFB.CodePlex.com";

            // Load the current settings
            config = new XmlDocument();
            config.LoadXml(currentConfig);
            navigator = config.CreateNavigator();

            root = navigator.SelectSingleNode("configuration");

            if(!root.IsEmptyElement)
                foreach(XPathNavigator nav in root.Select("expressions/expression"))
                    tvExpressions.Nodes.Add(nav.InnerXml);

            if(tvExpressions.Nodes.Count != 0)
                btnDelete.Enabled = txtExpression.Enabled = true;
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
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            XmlNode root, node, expr;

            TreeViewCancelEventArgs te = new TreeViewCancelEventArgs(
                tvExpressions.SelectedNode, false, TreeViewAction.Unknown);
            tvExpressions_BeforeSelect(tvExpressions, te);

            if(te.Cancel)
                return;

            // Store the changes
            root = config.SelectSingleNode("configuration");

            node = root.SelectSingleNode("expressions");
            if(node == null)
            {
                node = config.CreateNode(XmlNodeType.Element,
                    "expressions", null);
                root.AppendChild(node);
            }

            node.RemoveAll();

            foreach(TreeNode n in tvExpressions.Nodes)
            {
                expr = config.CreateNode(XmlNodeType.Element,
                    "expression", null);
                expr.InnerXml = n.Text;
                node.AppendChild(expr);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Add a new XPath expression
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            TreeViewCancelEventArgs te = new TreeViewCancelEventArgs(
                tvExpressions.SelectedNode, false, TreeViewAction.Unknown);
            tvExpressions_BeforeSelect(tvExpressions, te);

            if(te.Cancel)
                return;

            TreeNode node = new TreeNode(
                "/reflection/apis/api[contains(@id,\"#ctor\")]");

            tvExpressions.Nodes.Add(node);
            tvExpressions.SelectedNode = node;
            node.EnsureVisible();
            txtExpression.Focus();

            btnDelete.Enabled = txtExpression.Enabled = true;
        }

        /// <summary>
        /// Delete the selected XPath expression
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            TreeNode node = tvExpressions.SelectedNode;

            if(node != null && MessageBox.Show("Do you want to delete the " +
              "expression '" + node.Text + "?", Constants.AppName,
              MessageBoxButtons.YesNo, MessageBoxIcon.Question,
              MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                isDeleting = true;
                epErrors.Clear();
                tvExpressions.Nodes.Remove(node);

                if(tvExpressions.Nodes.Count == 0)
                {
                    txtExpression.Text = null;
                    btnDelete.Enabled = txtExpression.Enabled = false;
                }

                isDeleting = false;
            }
        }

        /// <summary>
        /// Validate the expression and store it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvExpressions_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode node = tvExpressions.SelectedNode;

            epErrors.Clear();

            if(node == null || isDeleting)
                return;

            if(txtExpression.Text.Trim().Length == 0)
            {
                epErrors.SetError(lblExpression, "An expression is required");
                e.Cancel = true;
                return;
            }

            try
            {
                // Make an attempt at validating the expression.  Just its
                // syntax, not necessarily that it will work in the reflection
                // file.
                XmlDocument doc = new XmlDocument();
                doc.SelectNodes(txtExpression.Text);
            }
            catch(Exception ex)
            {
                epErrors.SetError(lblExpression, ex.Message);
                e.Cancel = true;
            }

            if(!e.Cancel)
                node.Text = txtExpression.Text.Trim();
        }

        /// <summary>
        /// Load the selected expression for editing
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvExpressions_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode node = tvExpressions.SelectedNode;

            if(node == null)
                return;

            txtExpression.Text = node.Text;
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
