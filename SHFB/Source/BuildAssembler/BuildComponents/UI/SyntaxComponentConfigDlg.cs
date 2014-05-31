//===============================================================================================================
// System  : Sandcastle Build Components
// File    : SyntaxComponentConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/30/2014
// Note    : Copyright 2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a form that is used to configure the settings for the syntax build component and the
// syntax generators.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/24/2014  EFW  Created the code
// 05/25/2014  EFW  Added support for setting the syntax generator order
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

using Sandcastle.Core.BuildAssembler.SyntaxGenerator;

namespace Microsoft.Ddue.Tools.UI
{
    /// <summary>
    /// This is used to configure the syntax component and the syntax generators
    /// </summary>
    internal partial class SyntaxComponentConfigDlg : Form
    {
        #region Syntax generator configuration settings
        //=====================================================================

        /// <summary>
        /// This is used to hold the syntax generator settings for the configuration dialog
        /// </summary>
        private class SyntaxGeneratorSettings
        {
            /// <summary>
            /// This is used to get or set the syntax generator ID
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// This is used to get or set the sort order
            /// </summary>
            public int SortOrder { get; set; }

            /// <summary>
            /// This is used to get or set whether or not the syntax generator is configurable
            /// </summary>
            public bool IsConfigurable { get; set; }
            
            /// <summary>
            /// This is used to get or set the default configuration
            /// </summary>
            public string DefaultConfiguration { get; set; }

            /// <summary>
            /// This is used to get or set the current configuration
            /// </summary>
            public string CurrentConfiguration { get; set; }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private List<SyntaxGeneratorSettings> syntaxGenerators;    // All known syntax generators

        private XElement config;     // The configuration
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
        /// <param name="configuration">The current configuration in an XML fragment</param>
        /// <param name="container">The composition container user to locate the syntax generators</param>
        public SyntaxComponentConfigDlg(string configuration, CompositionContainer container)
        {
            HashSet<string> generatorIds = new HashSet<string>();
            XElement node;
            XAttribute attr;
            bool value;

            InitializeComponent();

            syntaxGenerators = new List<SyntaxGeneratorSettings>();

            // Get a list of all configurable syntax generators
            try
            {
                var generators = container.GetExports<ISyntaxGeneratorFactory, ISyntaxGeneratorMetadata>().Select(
                    g => g.Metadata).ToList();

                // There may be duplicate generator IDs across the assemblies found.  See
                // BuildComponentManger.GetComponentContainer() for the folder search precedence.  Only the
                // first component for a unique ID will be used.
                foreach(var generator in generators)
                    if(!generatorIds.Contains(generator.Id))
                    {
                        syntaxGenerators.Add(new SyntaxGeneratorSettings
                        {
                            Id = generator.Id,
                            SortOrder = generator.SortOrder,
                            IsConfigurable = generator.IsConfigurable,
                            DefaultConfiguration = generator.DefaultConfiguration,
                            CurrentConfiguration = generator.DefaultConfiguration
                        });

                        generatorIds.Add(generator.Id);
                    }
            }
            catch(Exception ex)
            {
                syntaxGenerators = new List<SyntaxGeneratorSettings>();
                btnOK.Enabled = false;

                MessageBox.Show("Unable to obtain a list of syntax generators: " + ex.Message, "Syntax Component",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            // Load the current settings
            config = XElement.Parse(configuration);
            node = config.Element("syntax");

            if(node != null)
            {
                attr = node.Attribute("renderReferenceLinks");

                if(!Boolean.TryParse(attr.Value, out value))
                    value = false;

                chkRenderReferenceLinks.Checked = value;
            }

            node = config.Element("containerElement");

            if(node != null)
            {
                attr = node.Attribute("addNoExampleTabs");

                if(!Boolean.TryParse(attr.Value, out value))
                    value = false;

                chkAddNoExampleTabs.Checked = value;

                if(value)
                {
                    attr = node.Attribute("includeOnSingleSnippets");

                    if(!Boolean.TryParse(attr.Value, out value))
                        value = false;

                    chkIncludeOnSingleSnippets.Checked = value;
                }
            }


            // Configurations are stored separately since the actual syntax filters are added at build time
            node = config.Element("configurations");

            if(node != null)
            {
                int idx = 0;

                foreach(var generator in node.Descendants("generator"))
                {
                    var sg = syntaxGenerators.FirstOrDefault(g => g.Id == generator.Attribute("id").Value);

                    if(sg != null)
                    {
                        var reader = generator.CreateReader();
                        reader.MoveToContent();

                        sg.SortOrder = idx;

                        if(sg.IsConfigurable)
                            sg.CurrentConfiguration = reader.ReadInnerXml();
                    }

                    idx++;
                }
            }

            foreach(var generator in syntaxGenerators.OrderBy(g => g.SortOrder))
                tvGenerators.Nodes.Add(generator.Id);

            if(tvGenerators.Nodes.Count != 0)
                tvGenerators.SelectedNode = tvGenerators.Nodes[0];
            else
                btnReset.Enabled = txtConfiguration.Enabled = btnMoveUp.Enabled = btnMoveDown.Enabled = false;
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
            XElement node;
            XAttribute attr;

            TreeViewCancelEventArgs te = new TreeViewCancelEventArgs(tvGenerators.SelectedNode, false,
                TreeViewAction.Unknown);
            tvGenerators_BeforeSelect(tvGenerators, te);

            if(te.Cancel)
                return;

            node = config.Element("syntax");

            if(node == null)
                return;

            attr = node.Attribute("renderReferenceLinks");

            if(attr == null)
            {
                attr = new XAttribute("renderReferenceLinks", "false");
                node.Add(attr);
            }

            attr.Value = chkRenderReferenceLinks.Checked.ToString().ToLowerInvariant();

            node = config.Element("containerElement");

            if(node != null)
            {
                attr = node.Attribute("addNoExampleTabs");

                if(attr == null)
                {
                    attr = new XAttribute("addNoExampleTabs", "false");
                    node.Add(attr);
                }

                attr.Value = chkAddNoExampleTabs.Checked.ToString().ToLowerInvariant();

                attr = node.Attribute("includeOnSingleSnippets");

                if(attr == null)
                {
                    attr = new XAttribute("includeOnSingleSnippets", "false");
                    node.Add(attr);
                }

                attr.Value = chkIncludeOnSingleSnippets.Checked.ToString().ToLowerInvariant();
            }

            node = config.Element("configurations");

            if(node == null)
            {
                node = new XElement("configurations");
                config.Add(node);
            }

            node.RemoveNodes();

            // Configurations are stored separately since the actual syntax filters are added at build time.
            // This also allows us to store the selected order.
            foreach(TreeNode tn in tvGenerators.Nodes)
            {
                var sg = syntaxGenerators.First(g => g.Id == tn.Text);

                if(!sg.IsConfigurable)
                {
                    node.Add(XElement.Parse(String.Format(CultureInfo.InvariantCulture,
                        "<generator id=\"{0}\" />", sg.Id)));
                }
                else
                {
                    node.Add(XElement.Parse(String.Format(CultureInfo.InvariantCulture,
                        "<generator id=\"{0}\">{1}</generator>", sg.Id, sg.CurrentConfiguration)));
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Validate the configuration and store it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvGenerators_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode node = tvGenerators.SelectedNode;

            epErrors.Clear();

            if(node != null)
            {
                var sg = syntaxGenerators.First(g => g.Id == node.Text);

                if(sg.IsConfigurable)
                {
                    if(txtConfiguration.Text.Trim().Length == 0)
                    {
                        epErrors.SetError(lblConfiguration, "A configuration is required");
                        e.Cancel = true;
                    }
                    else
                    {
                        // Only update it if it changed
                        if(sg.CurrentConfiguration != txtConfiguration.Text)
                        {
                            try
                            {
                                var element = XElement.Parse("<configuration>" + txtConfiguration.Text +
                                    "</configuration>");
                            }
                            catch(Exception ex)
                            {
                                epErrors.SetError(lblConfiguration, ex.Message);
                                e.Cancel = true;
                            }

                            if(!e.Cancel)
                                sg.CurrentConfiguration = txtConfiguration.Text;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Load the selected configuration for editing
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvGenerators_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode node = tvGenerators.SelectedNode;

            if(node != null)
            {
                var sg = syntaxGenerators.First(g => g.Id == node.Text);

                if(sg.IsConfigurable)
                {
                    txtConfiguration.Text = sg.CurrentConfiguration;
                    btnReset.Enabled = txtConfiguration.Enabled = true;
                }
                else
                {
                    txtConfiguration.Text = "(Not configurable)";
                    btnReset.Enabled = txtConfiguration.Enabled = false;
                }

                btnMoveUp.Enabled = (node.PrevNode != null);
                btnMoveDown.Enabled = (node.NextNode != null);
            }
        }

        /// <summary>
        /// Reset the configuration
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnReset_Click(object sender, EventArgs e)
        {
            TreeNode node = tvGenerators.SelectedNode;

            if(node != null)
            {
                var sg = syntaxGenerators.First(g => g.Id == node.Text);

                sg.CurrentConfiguration = sg.DefaultConfiguration;

                tvGenerators_AfterSelect(sender, new TreeViewEventArgs(node));
            }
        }

        /// <summary>
        /// Move the selected syntax generator up in the sort order
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            TreeNode node = tvGenerators.SelectedNode;
            int idx;

            if(node != null)
            {
                idx = tvGenerators.Nodes.IndexOf(node.PrevNode);
                tvGenerators.Nodes.Remove(node);
                tvGenerators.Nodes.Insert(idx, node);
                tvGenerators.SelectedNode = node;
            }
        }

        /// <summary>
        /// Move the selected syntax generator down in the sort order
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            TreeNode node = tvGenerators.SelectedNode;
            int idx;

            if(node != null)
            {
                idx = tvGenerators.Nodes.IndexOf(node.NextNode);
                tvGenerators.Nodes.Remove(node);
                tvGenerators.Nodes.Insert(idx, node);
                tvGenerators.SelectedNode = node;
            }
        }

        /// <summary>
        /// Update the "include on standalone snippets" checkbox state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkAddNoExampleTabs_CheckedChanged(object sender, EventArgs e)
        {
            if(chkAddNoExampleTabs.Checked)
                chkIncludeOnSingleSnippets.Enabled = true;
            else
                chkIncludeOnSingleSnippets.Enabled = chkIncludeOnSingleSnippets.Checked = false;
        }
        #endregion
    }
}
