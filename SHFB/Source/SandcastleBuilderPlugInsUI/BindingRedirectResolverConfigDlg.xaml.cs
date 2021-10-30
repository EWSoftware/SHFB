//===============================================================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : BindingRedirectResolverConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/14/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains the form used to edit the assembly binding redirection resolver plug-in configuration
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/14/2008  EFW  Created the code
// 03/31/2012  EFW  Added Use GAC option
// 11/25/2012  EFW  Added support for Ignore if Unresolved assembly names
// 05/11/2021  EFW  Added MEF configuration editor export and converted the form to WPF for better high DPI
//                  scaling support on 4K displays.
//===============================================================================================================

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

using Sandcastle.Core;
using Sandcastle.Platform.Windows;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;

namespace SandcastleBuilder.PlugIns.UI
{
    /// <summary>
    /// This form is used to edit the assembly binding redirection resolver plug-in configuration
    /// </summary>
    public partial class BindingRedirectResolverConfigDlg : Window
    {
        #region Plug-in configuration editor factory for MEF
        //=====================================================================

        /// <summary>
        /// This allows editing of the plug-in configuration
        /// </summary>
        [PlugInConfigurationEditorExport("Assembly Binding Redirection")]
        public sealed class Factory : IPlugInConfigurationEditor
        {
            /// <inheritdoc />
            public bool EditConfiguration(SandcastleProject project, XElement configuration)
            {
                var dlg = new BindingRedirectResolverConfigDlg(project, configuration);

                return dlg.ShowModalDialog() ?? false;
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private readonly XElement configuration;
        private readonly SandcastleProject project;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">The current project</param>
        /// <param name="configuration">The current configuration element</param>
        public BindingRedirectResolverConfigDlg(SandcastleProject project, XElement configuration)
        {
            InitializeComponent();

            this.project = project ?? throw new ArgumentNullException(nameof(project));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            if(!configuration.IsEmpty)
            {
                chkUseGac.IsChecked = (bool)configuration.Attribute("useGAC");

                // Load the current settings
                foreach(var binding in configuration.Element("assemblyBinding").Descendants("dependentAssembly"))
                    lbBindingRedirections.Items.Add(BindingRedirectSettings.FromXml(project, binding));

                foreach(var ignoredName in configuration.Element("ignoreIfUnresolved").Descendants("assemblyIdentity"))
                    lbIgnoreIfUnresolved.Items.Add(ignoredName.Attribute("name").Value);
            }

            btnDeleteBindingRedirect.IsEnabled = grpBindingRedirects.IsEnabled = lbBindingRedirections.Items.Count != 0;

            if(lbBindingRedirections.Items.Count != 0)
                lbBindingRedirections.SelectedIndex = 0;

            if(lbIgnoreIfUnresolved.Items.Count == 0)
            {
                lbIgnoreIfUnresolved.Items.Add("BusinessObjects.Licensing.KeycodeDecoder");
                lbIgnoreIfUnresolved.Items.Add("Microsoft.VisualStudio.TestTools.UITest.Playback");
            }

            lbIgnoreIfUnresolved.SelectedIndex = 0;
        }
        #endregion

        #region General event handlers
        //=====================================================================

        /// <summary>
        /// View help for this plug-in
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            UiUtility.ShowHelpTopic("f5051d32-b973-4fe1-9ffe-e30531007691");
        }

        /// <summary>
        /// Go to the project site
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lnkProjectSite_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
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
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            var redirects = lbBindingRedirections.Items.Cast<BindingRedirectSettings>().ToList();
            var ignored = lbIgnoreIfUnresolved.Items.Cast<string>().ToList();

            if(redirects.Any(ex => ex.ErrorMessage != null))
            {
                MessageBox.Show("One or more binding redirections are invalid.  Please fix them before saving them.",
                    Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // Store the changes
            configuration.RemoveNodes();
            configuration.RemoveAttributes();

            configuration.Add(new XAttribute("useGAC", chkUseGac.IsChecked.Value),
                new XElement("assemblyBinding",
                    redirects.Select(br => br.ToXml(true))),
                new XElement("ignoreIfUnresolved",
                    ignored.Select(i => new XElement("assemblyIdentity", new XAttribute("name", i)))));

            this.DialogResult = true;
            this.Close();
        }
        #endregion

        #region Binding redirection tab event handlers
        //=====================================================================

        /// <summary>
        /// Add a new binding redirection
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAddBindingRedirect_Click(object sender, RoutedEventArgs e)
        {
            int idx = lbBindingRedirections.Items.Add(new BindingRedirectSettings(project)
            {
                AssemblyName = "assemblyName",
                OldVersion = "1.0.0.0",
                NewVersion = "2.0.0.0"
            });

            lbBindingRedirections.SelectedIndex = idx;
            txtAssemblyName.Focus();

            btnDeleteBindingRedirect.IsEnabled = grpBindingRedirects.IsEnabled = true;
        }

        /// <summary>
        /// Delete the selected binding redirection
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDeleteBindingRedirect_Click(object sender, RoutedEventArgs e)
        {
            int idx = lbBindingRedirections.SelectedIndex;

            if(idx != -1 && MessageBox.Show("Do you want to delete the binding redirection '" +
              ((BindingRedirectSettings)lbBindingRedirections.SelectedItem).BindingRedirectDescription + "'?",
              Constants.AppName, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) ==
              MessageBoxResult.Yes)
            {
                lbBindingRedirections.Items.RemoveAt(idx);

                if(lbBindingRedirections.Items.Count == 0)
                    btnDeleteBindingRedirect.IsEnabled = grpBindingRedirects.IsEnabled = false;
                else
                {
                    if(idx < lbBindingRedirections.Items.Count)
                        lbBindingRedirections.SelectedIndex = idx;
                    else
                        lbBindingRedirections.SelectedIndex = lbBindingRedirections.Items.Count - 1;
                }
            }
        }

        /// <summary>
        /// Enable or disable controls based on whether or not there are binding redirections
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbBindingRedirections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnDeleteBindingRedirect.IsEnabled = grpBindingRedirects.IsEnabled = lbBindingRedirections.SelectedIndex != -1;
        }
        #endregion

        #region Ignored assembly names tab event handlers
        //=====================================================================

        /// <summary>
        /// Add an ignored assembly name
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAddIgnoredName_Click(object sender, RoutedEventArgs e)
        {
            txtAssemblyName.Text = txtAssemblyName.Text.Trim();

            if(txtAssemblyName.Text.Length != 0 && !lbIgnoreIfUnresolved.Items.Contains(txtAssemblyName.Text))
            {
                lbIgnoreIfUnresolved.Items.Add(txtAssemblyName.Text);
                btnDeleteIgnoredName.IsEnabled = true;
                txtAssemblyName.Text = null;
                txtAssemblyName.Focus();
            }
        }

        /// <summary>
        /// Delete the selected ignored assembly names
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDeleteIgnoredName_Click(object sender, RoutedEventArgs e)
        {
            int idx = lbIgnoreIfUnresolved.SelectedIndex;

            if(idx != -1 && MessageBox.Show("Do you want to delete the assembly name '" +
              (string)lbIgnoreIfUnresolved.SelectedItem + "'?", Constants.AppName, MessageBoxButton.YesNo,
              MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                lbIgnoreIfUnresolved.Items.RemoveAt(idx);

                if(lbIgnoreIfUnresolved.Items.Count == 0)
                    btnDeleteIgnoredName.IsEnabled = false;
                else
                {
                    if(idx < lbIgnoreIfUnresolved.Items.Count)
                        lbIgnoreIfUnresolved.SelectedIndex = idx;
                    else
                        lbIgnoreIfUnresolved.SelectedIndex = lbIgnoreIfUnresolved.Items.Count - 1;
                }
            }
        }

        /// <summary>
        /// Enable or disable controls based on whether or not there are ignored assembly names
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbIgnoreIfUnresolved_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnDeleteIgnoredName.IsEnabled = lbIgnoreIfUnresolved.SelectedIndex != -1;
        }
        #endregion
    }
}
