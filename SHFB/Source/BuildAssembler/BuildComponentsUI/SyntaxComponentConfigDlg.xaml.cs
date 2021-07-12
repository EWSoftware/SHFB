//===============================================================================================================
// System  : Sandcastle Build Components
// File    : SyntaxComponentConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/31/2021
// Note    : Copyright 2014-2021, Eric Woodruff, All rights reserved
//
// This file contains a form that is used to configure the settings for the syntax build component and the
// syntax generators.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/24/2014  EFW  Created the code
// 05/25/2014  EFW  Added support for setting the syntax generator order
// 12/21/2017  EFW  Converted the form to WPF for better high DPI scaling support on 4K displays
// 04/11/2021  EFW  Moved the form to a separate platform-specific assembly
// 04/24/2021  EFW  Added MEF configuration editor export 
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Xml.Linq;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.SyntaxGenerator;

using Sandcastle.Platform.Windows;

namespace Sandcastle.Tools.BuildComponents.UI
{
    /// <summary>
    /// This form is used to configure the syntax generators for the Syntax Component
    /// </summary>
    public partial class SyntaxComponentConfigDlg : Window
    {
        #region Build component configuration editor factory for MEF
        //=====================================================================

        /// <summary>
        /// This allows editing of the component configuration
        /// </summary>
        [ConfigurationEditorExport("Syntax Component")]
        public sealed class Factory : IConfigurationEditor
        {
            /// <inheritdoc />
            public bool EditConfiguration(XElement configuration, CompositionContainer container)
            {
                var dlg = new SyntaxComponentConfigDlg(configuration, container);

                return dlg.ShowModalDialog() ?? false;
            }
        }
        #endregion

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

        private readonly XElement configuration;
        private readonly BindingList<SyntaxGeneratorSettings> syntaxGenerators;
        private SyntaxGeneratorSettings currentGenerator;
        private bool changingGenerator;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">The current configuration element</param>
        /// <param name="container">The composition container user to locate the syntax generators</param>
        public SyntaxComponentConfigDlg(XElement configuration, CompositionContainer container)
        {
            HashSet<string> generatorIds = new HashSet<string>();
            XElement node;
            XAttribute attr;
            bool value;

            InitializeComponent();

            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            syntaxGenerators = new BindingList<SyntaxGeneratorSettings>();

            // Get a list of all configurable syntax generators
            try
            {
                if(container == null)
                    throw new ArgumentNullException(nameof(container));

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
                syntaxGenerators = new BindingList<SyntaxGeneratorSettings>();
                btnOK.IsEnabled = false;

                MessageBox.Show("Unable to obtain a list of syntax generators: " + ex.Message, Constants.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            // Load the current settings
            node = configuration.Element("syntax");

            if(node != null)
            {
                attr = node.Attribute("renderReferenceLinks");

                if(!Boolean.TryParse(attr.Value, out value))
                    value = false;

                chkRenderReferenceLinks.IsChecked = value;
            }

            node = configuration.Element("containerElement");

            if(node != null)
            {
                attr = node.Attribute("addNoExampleTabs");

                if(!Boolean.TryParse(attr.Value, out value))
                    value = false;

                chkAddNoExampleTabs.IsChecked = value;

                if(value)
                {
                    attr = node.Attribute("includeOnSingleSnippets");

                    if(!Boolean.TryParse(attr.Value, out value))
                        value = false;

                    chkIncludeOnSingleSnippets.IsChecked = value;
                }
            }

            // Configurations are stored separately since the actual syntax filters are added at build time
            node = configuration.Element("configurations");

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

            syntaxGenerators = new BindingList<SyntaxGeneratorSettings>(syntaxGenerators.OrderBy(g => g.SortOrder).ToList());
            lbGenerators.ItemsSource = syntaxGenerators;

            if(lbGenerators.Items.Count != 0)
                lbGenerators.SelectedIndex = 0;
            else
                btnReset.IsEnabled = txtConfiguration.IsEnabled = btnMoveUp.IsEnabled = btnMoveDown.IsEnabled = false;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to determine whether or not the current configuration is valid and to store the changes
        /// </summary>
        /// <returns></returns>
        private bool IsValid()
        {
            bool isValid = true;

            txtConfiguration.SetValidationState(true, null);

            if(currentGenerator != null)
            {
                if(currentGenerator.IsConfigurable)
                {
                    if(txtConfiguration.Text.Trim().Length == 0)
                    {
                        txtConfiguration.SetValidationState(false, "A configuration is required");
                        isValid = false;
                    }
                    else
                    {
                        // Only update it if it changed
                        if(currentGenerator.CurrentConfiguration != txtConfiguration.Text)
                        {
                            try
                            {
                                // Parse it to ensure it's valid XML
                                _ = XElement.Parse("<configuration>" + txtConfiguration.Text + "</configuration>");

                                currentGenerator.CurrentConfiguration = txtConfiguration.Text;
                            }
                            catch(Exception ex)
                            {
                                txtConfiguration.SetValidationState(false, ex.Message);
                                isValid = false;
                            }
                        }
                    }
                }
            }

            return isValid;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// View help for this component
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            UiUtility.ShowHelpTopic("3127217a-9b11-424e-aeb4-b88ca4569bad");
        }

        /// <summary>
        /// Validate and update the selected configuration when the selection changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbGenerators_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(currentGenerator != null)
            {
                if(changingGenerator)
                    return;

                // Revert to the previous item if it's not valid
                if(!this.IsValid())
                {
                    changingGenerator = e.Handled = true;
                    lbGenerators.SelectedValue = currentGenerator;
                    changingGenerator = false;
                    return;
                }
            }

            var sg = (SyntaxGeneratorSettings)lbGenerators.SelectedValue;

            if(sg != null)
            {
                if(sg.IsConfigurable)
                {
                    txtConfiguration.Text = sg.CurrentConfiguration;
                    btnReset.IsEnabled = txtConfiguration.IsEnabled = true;
                }
                else
                {
                    txtConfiguration.Text = "(Not configurable)";
                    btnReset.IsEnabled = txtConfiguration.IsEnabled = false;
                }

                btnMoveUp.IsEnabled = (lbGenerators.SelectedIndex != 0);
                btnMoveDown.IsEnabled = (lbGenerators.SelectedIndex < lbGenerators.Items.Count - 1);
            }

            currentGenerator = sg;
        }

        /// <summary>
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            XElement node;
            XAttribute attr;

            if(!this.IsValid())
                return;

            node = configuration.Element("syntax");

            if(node == null)
                return;

            attr = node.Attribute("renderReferenceLinks");

            if(attr == null)
            {
                attr = new XAttribute("renderReferenceLinks", "false");
                node.Add(attr);
            }

            attr.Value = chkRenderReferenceLinks.IsChecked.ToString().ToLowerInvariant();

            node = configuration.Element("containerElement");

            if(node != null)
            {
                attr = node.Attribute("addNoExampleTabs");

                if(attr == null)
                {
                    attr = new XAttribute("addNoExampleTabs", "false");
                    node.Add(attr);
                }

                attr.Value = chkAddNoExampleTabs.IsChecked.ToString().ToLowerInvariant();

                attr = node.Attribute("includeOnSingleSnippets");

                if(attr == null)
                {
                    attr = new XAttribute("includeOnSingleSnippets", "false");
                    node.Add(attr);
                }

                attr.Value = chkIncludeOnSingleSnippets.IsChecked.ToString().ToLowerInvariant();
            }

            node = configuration.Element("configurations");

            if(node == null)
            {
                node = new XElement("configurations");
                configuration.Add(node);
            }

            node.RemoveNodes();

            // Configurations are stored separately since the actual syntax filters are added at build time.
            // This also allows us to store the selected order.
            foreach(var sg in syntaxGenerators)
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

            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Go to the Sandcastle Help File Builder project site
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lnkProjectSite_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(e.Uri.ToString());
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show("Unable to launch link target.  Reason: " + ex.Message, Constants.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// Reset the configuration
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if(currentGenerator != null)
                txtConfiguration.Text = currentGenerator.CurrentConfiguration = currentGenerator.DefaultConfiguration;
        }

        /// <summary>
        /// Move the selected syntax generator up in the sort order
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnMoveUp_Click(object sender, RoutedEventArgs e)
        {
            var sg = currentGenerator;
            int idx = lbGenerators.SelectedIndex;

            if(sg != null && idx > 0)
            {
                syntaxGenerators.RemoveAt(idx);
                syntaxGenerators.Insert(idx - 1, sg);
                lbGenerators.SelectedIndex = idx - 1;
            }
        }

        /// <summary>
        /// Move the selected syntax generator down in the sort order
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            var sg = currentGenerator;
            int idx = lbGenerators.SelectedIndex;

            if(sg != null && idx < lbGenerators.Items.Count - 1)
            {
                syntaxGenerators.RemoveAt(idx);
                syntaxGenerators.Insert(idx + 1, sg);
                lbGenerators.SelectedIndex = idx + 1;
            }
        }

        /// <summary>
        /// Update the "include on standalone snippets" checkbox state
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkAddNoExampleTabs_Click(object sender, RoutedEventArgs e)
        {
            if(chkAddNoExampleTabs.IsChecked ?? false)
                chkIncludeOnSingleSnippets.IsEnabled = true;
            else
                chkIncludeOnSingleSnippets.IsChecked = chkIncludeOnSingleSnippets.IsEnabled = false;
        }
        #endregion
    }
}
