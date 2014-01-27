//===============================================================================================================
// System  : Sandcastle Build Components
// File    : SyntaxComponentConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/24/2014
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
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
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
        #region Private data members
        //=====================================================================

        private List<ISyntaxGeneratorMetadata> syntaxGenerators;    // All known syntax generators
        private Dictionary<string, string> configurations;          // The current configurations

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

            configurations = new Dictionary<string, string>();
            syntaxGenerators = new List<ISyntaxGeneratorMetadata>();

            // Get a list of all configurable syntax generators
            try
            {
                var generators = container.GetExports<ISyntaxGeneratorFactory, ISyntaxGeneratorMetadata>().Where(
                    g => g.Metadata.IsConfigurable).Select(g => g.Metadata).ToList();

                // There may be duplicate generator IDs across the assemblies found.  See
                // BuildComponentManger.GetComponentContainer() for the folder search precedence.  Only the
                // first component for a unique ID will be used.
                foreach(var generator in generators)
                    if(!generatorIds.Contains(generator.Id))
                    {
                        syntaxGenerators.Add(generator);
                        generatorIds.Add(generator.Id);
                    }


                syntaxGenerators = syntaxGenerators.OrderBy(s => s.Id).ToList();
            }
            catch(Exception ex)
            {
                syntaxGenerators = new List<ISyntaxGeneratorMetadata>();

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

            // Configurations are stored separately since the actual syntax filters are added at build time
            node = config.Element("configurations");

            if(node != null)
                foreach(var generator in node.Descendants("generator"))
                    if(syntaxGenerators.Any(s => s.Id == generator.Attribute("id").Value))
                    {
                        var reader = generator.CreateReader();
                        reader.MoveToContent();

                        configurations[generator.Attribute("id").Value] = reader.ReadInnerXml();
                    }

            foreach(var generator in syntaxGenerators)
                tvGenerators.Nodes.Add(generator.Id);

            if(tvGenerators.Nodes.Count != 0)
            {
                btnReset.Enabled = txtConfiguration.Enabled = true;
                tvGenerators.SelectedNode = tvGenerators.Nodes[0];
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

            node = config.Element("configurations");

            if(node == null)
            {
                node = new XElement("configurations");
                config.Add(node);
            }

            node.RemoveNodes();

            // Configurations are stored separately since the actual syntax filters are added at build time
            foreach(var kv in configurations)
                node.Add(XElement.Parse(String.Format("<generator id=\"{0}\">{1}</generator>", kv.Key, kv.Value)));

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
                if(txtConfiguration.Text.Trim().Length == 0)
                {
                    epErrors.SetError(lblConfiguration, "A configuration is required");
                    e.Cancel = true;
                    return;
                }

                string originalConfiguration = syntaxGenerators.First(s => s.Id == node.Text).DefaultConfiguration;

                // Only update it if it changed
                if(originalConfiguration != txtConfiguration.Text)
                {
                    try
                    {
                        var element = XElement.Parse("<configuration>" + txtConfiguration.Text + "</configuration>");
                    }
                    catch(Exception ex)
                    {
                        epErrors.SetError(lblConfiguration, ex.Message);
                        e.Cancel = true;
                    }

                    if(!e.Cancel)
                        configurations[node.Text] = txtConfiguration.Text;
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
            string configuration;

            if(node != null)
            {
                if(!configurations.TryGetValue(node.Text, out configuration))
                    configuration = syntaxGenerators.First(s => s.Id == node.Text).DefaultConfiguration;

                txtConfiguration.Text = configuration;
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
                configurations.Remove(node.Text);
                tvGenerators_AfterSelect(sender, new TreeViewEventArgs(node));
            }
        }
        #endregion
    }
}
