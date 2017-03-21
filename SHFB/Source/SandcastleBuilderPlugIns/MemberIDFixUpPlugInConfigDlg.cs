//===============================================================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : MemberIdFixUpPlugInConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/12/2017
// Note    : Copyright 2014-2017, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to edit the member ID fix-up plug-in configuration
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/14/2014  EFW  Created the code
//===============================================================================================================

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;

using Sandcastle.Core;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This form is used to edit the <see cref="MemberIdFixUpPlugIn"/> configuration
    /// </summary>
    internal partial class MemberIdFixUpPlugInConfigDlg : Form
    {
        #region Private data members
        //=====================================================================

        private XElement config;     // The configuration
        private bool isDeleting;
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
        /// <param name="currentConfig">The current XML configuration XML fragment</param>
        public MemberIdFixUpPlugInConfigDlg(string currentConfig)
        {
            InitializeComponent();

            lnkProjectSite.Links[0].LinkData = "https://GitHub.com/EWSoftware/SHFB";

            // Load the current settings
            config = XElement.Parse(currentConfig);

            if(!config.IsEmpty)
                foreach(var expr in config.Descendants("expression"))
                {
                    var matchExpr = new MemberIdMatchExpression
                    {
                        MatchExpression = expr.Attribute("matchExpression").Value,
                        ReplacementValue = expr.Attribute("replacementValue").Value,
                        MatchAsRegEx = (bool)expr.Attribute("matchAsRegEx")
                    };

                    tvExpressions.Nodes.Add(new TreeNode
                    {
                        Text = matchExpr.MatchExpression,
                        Tag = matchExpr
                    });
                }

            if(tvExpressions.Nodes.Count != 0)
                btnDelete.Enabled = txtMatchExpression.Enabled = txtReplacementValue.Enabled =
                    chkMatchAsRegEx.Enabled = true;
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
            MemberIdMatchExpression matchExpr;

            TreeViewCancelEventArgs te = new TreeViewCancelEventArgs(tvExpressions.SelectedNode, false,
                TreeViewAction.Unknown);
            tvExpressions_BeforeSelect(tvExpressions, te);

            if(te.Cancel)
                return;

            // Store the changes
            var node = config.Element("expressions");

            if(node == null)
            {
                node = new XElement("expressions");
                config.Add(node);
            }
            else
                node.RemoveAll();

            foreach(TreeNode n in tvExpressions.Nodes)
            {
                matchExpr = (MemberIdMatchExpression)n.Tag;

                node.Add(new XElement("expression",
                    new XAttribute("matchExpression", matchExpr.MatchExpression),
                    new XAttribute("replacementValue", matchExpr.ReplacementValue),
                    new XAttribute("matchAsRegEx", matchExpr.MatchAsRegEx)));
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Add a new match expression
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            TreeViewCancelEventArgs te = new TreeViewCancelEventArgs(tvExpressions.SelectedNode, false,
                TreeViewAction.Unknown);
            tvExpressions_BeforeSelect(tvExpressions, te);

            if(te.Cancel)
                return;

            TreeNode node = new TreeNode
            {
                Text = "T:MyNamespace.MyClass1",
                Tag = new MemberIdMatchExpression
                {
                    MatchExpression = "T:MyNamespace.MyClass1",
                    ReplacementValue = "T:MyNamespace.MyClass2"
                }
            };

            tvExpressions.Nodes.Add(node);
            tvExpressions.SelectedNode = node;
            node.EnsureVisible();
            txtMatchExpression.Focus();

            btnDelete.Enabled = txtMatchExpression.Enabled = txtReplacementValue.Enabled =
                chkMatchAsRegEx.Enabled = true;
        }

        /// <summary>
        /// Add the common C++ fix-up expressions
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnCPPFixes_Click(object sender, EventArgs e)
        {
            TreeNode node = null;

            string[,] cppExpressions = new[,] {
                // Overload topic links fix-up
                { "!:O:", "O:" },
                // Strip out "`" followed by digits
                { "`[0-9]+(\\{)", "$1" },
                // Strip out superfluous "^"
                { "(member name=\".*?System\\.Collections\\.Generic.*?)(\\^)", "$1" },
                // Fix-up valid cref attributes that the compiler couldn't figure out
                { "cref=\"!:([EFGMNPT]|Overload):", "cref=\"$1:" },
                // Convert interior_ptr<T> to an explicit dereference
                { @"cli\.interior_ptr{([^}]+?)}", "$1@!System.Runtime.CompilerServices.IsExplicitlyDereferenced" } };

            for(int i = 0; i <= cppExpressions.GetUpperBound(0); i++)
                if(!tvExpressions.Nodes.OfType<TreeNode>().Any(t => t.Text == cppExpressions[i, 0]))
                {
                    node = new TreeNode
                    {
                        Text = cppExpressions[i, 0],
                        Tag = new MemberIdMatchExpression
                        {
                            MatchExpression = cppExpressions[i, 0],
                            ReplacementValue = cppExpressions[i, 1],
                            MatchAsRegEx = true
                        }
                    };

                    tvExpressions.Nodes.Add(node);
                }

            if(node != null)
            {
                tvExpressions.SelectedNode = node;
                node.EnsureVisible();
                txtMatchExpression.Focus();

                btnDelete.Enabled = txtMatchExpression.Enabled = txtReplacementValue.Enabled =
                    chkMatchAsRegEx.Enabled = true;
            }
        }

        /// <summary>
        /// Delete the selected match expression
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            TreeNode node = tvExpressions.SelectedNode;

            if(node != null && MessageBox.Show("Do you want to delete the match expression '" + node.Text + "?",
              Constants.AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
              MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                isDeleting = true;
                epErrors.Clear();
                tvExpressions.Nodes.Remove(node);

                if(tvExpressions.Nodes.Count == 0)
                {
                    txtMatchExpression.Text = null;
                    btnDelete.Enabled = txtMatchExpression.Enabled = txtReplacementValue.Enabled =
                        chkMatchAsRegEx.Enabled = false;
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

            if(txtMatchExpression.Text.Trim().Length == 0)
            {
                epErrors.SetError(lblMatchExpression, "A match expression is required");
                e.Cancel = true;
            }

            if(txtReplacementValue.Text.Trim().Length == 0)
            {
                epErrors.SetError(lblReplacementValue, "A replacement value is required");
                e.Cancel = true;
            }

            if(chkMatchAsRegEx.Checked)
                try
                {
                    // Make an attempt at validating the expression.  Just its syntax, not necessarily that it
                    // will work in the reflection file.
                    Regex reTest = new Regex(txtMatchExpression.Text.Trim());

                    reTest.Replace("T:System.Object", txtReplacementValue.Text.Trim());
                }
                catch(Exception ex)
                {
                    epErrors.SetError(lblMatchExpression, ex.Message);
                    e.Cancel = true;
                }

            if(!e.Cancel)
            {
                var matchExpr = (MemberIdMatchExpression)node.Tag;

                node.Text = matchExpr.MatchExpression = txtMatchExpression.Text.Trim();
                matchExpr.ReplacementValue = txtReplacementValue.Text.Trim();
                matchExpr.MatchAsRegEx = chkMatchAsRegEx.Checked;
            }
        }

        /// <summary>
        /// Load the selected expression for editing
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvExpressions_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode node = tvExpressions.SelectedNode;

            if(node != null)
            {
                var matchExpr = (MemberIdMatchExpression)node.Tag;

                txtMatchExpression.Text = matchExpr.MatchExpression;
                txtReplacementValue.Text = matchExpr.ReplacementValue;
                chkMatchAsRegEx.Checked = matchExpr.MatchAsRegEx;
            }
        }

        /// <summary>
        /// Launch the URL in the web browser
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
                MessageBox.Show("Unable to launch link target.  Reason: " + ex.Message, Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Clear the selection when the text box gains the focus to prevent loss of all text
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void Expression_Enter(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;

            tb.Select(0, 0);
            tb.ScrollToCaret();
        }
        #endregion
    }
}
