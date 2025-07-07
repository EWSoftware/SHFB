//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : MemberIdFixUpPlugInConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/20/2025
// Note    : Copyright 2014-2025, Eric Woodruff, All rights reserved
//
// This file contains the form used to edit the member ID fix-up plug-in configuration
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/14/2014  EFW  Created the code
// 05/10/2021  EFW  Added MEF configuration editor export and converted the form to WPF for better high DPI
//                  scaling support on 4K displays.
//===============================================================================================================

// Ignore Spelling: cli

using System;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

using Sandcastle.Core;
using Sandcastle.Core.PlugIn;
using Sandcastle.Core.Project;

using Sandcastle.Platform.Windows;

namespace SandcastleBuilder.PlugIns.UI
{
    /// <summary>
    /// This form is used to edit the member ID fix-up plug-in configuration
    /// </summary>
    public partial class MemberIDFixUpPlugInConfigDlg : Window
    {
        #region Plug-in configuration editor factory for MEF
        //=====================================================================

        /// <summary>
        /// This allows editing of the plug-in configuration
        /// </summary>
        [PlugInConfigurationEditorExport("Member ID Fix-Ups")]
        public sealed class Factory : IPlugInConfigurationEditor
        {
            /// <inheritdoc />
            public bool EditConfiguration(ISandcastleProject project, XElement configuration)
            {
                var dlg = new MemberIDFixUpPlugInConfigDlg(configuration);

                return dlg.ShowModalDialog() ?? false;
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private readonly XElement configuration;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">The configuration to edit</param>
        public MemberIDFixUpPlugInConfigDlg(XElement configuration)
        {
            InitializeComponent();

            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // Load the current settings
            foreach(var fixUp in configuration.Descendants("expression"))
                lbExpressions.Items.Add(new MemberIdMatchExpression
                {
                    MatchExpression = fixUp.Attribute("matchExpression").Value,
                    ReplacementValue = fixUp.Attribute("replacementValue").Value,
                    MatchAsRegEx = (bool)fixUp.Attribute("matchAsRegEx")
                });

            btnDelete.IsEnabled = txtMatchExpression.IsEnabled = txtReplacementValue.IsEnabled =
                chkUseRegEx.IsEnabled = lbExpressions.SelectedIndex != -1;

            if(lbExpressions.Items.Count != 0)
                lbExpressions.SelectedIndex = 0;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// View help for this plug-in
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            UiUtility.ShowHelpTopic("17d977a6-e3dc-4ef4-9bbf-718ef4823854");
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
        /// Enable or disable the controls based on the selection
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbExpressions_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            btnDelete.IsEnabled = txtMatchExpression.IsEnabled = txtReplacementValue.IsEnabled =
                chkUseRegEx.IsEnabled = lbExpressions.SelectedIndex != -1;
        }

        /// <summary>
        /// Add a new fix-up expression
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            int idx = lbExpressions.Items.Add(new MemberIdMatchExpression
            {
                MatchExpression = "T:MyNamespace.MyClass1",
                ReplacementValue = "T:MyNamespace.MyClass2"
            });

            lbExpressions.SelectedIndex = idx;

            btnDelete.IsEnabled = txtMatchExpression.IsEnabled = txtReplacementValue.IsEnabled =
                chkUseRegEx.IsEnabled = true;
            txtMatchExpression.Focus();
        }

        /// <summary>
        /// Delete the selected fix-up expression
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            int idx = lbExpressions.SelectedIndex;

            if(idx != -1 && MessageBox.Show("Do you want to delete the expression '" +
              ((MemberIdMatchExpression)lbExpressions.SelectedItem).MatchExpression + "'?", Constants.AppName,
              MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                lbExpressions.Items.RemoveAt(idx);

                if(lbExpressions.Items.Count == 0)
                {
                    txtMatchExpression.Text = txtReplacementValue.Text = null;
                    chkUseRegEx.IsChecked = false;
                    btnDelete.IsEnabled = txtMatchExpression.IsEnabled = txtReplacementValue.IsEnabled =
                        chkUseRegEx.IsEnabled = false;
                }
                else
                {
                    if(idx < lbExpressions.Items.Count)
                        lbExpressions.SelectedIndex = idx;
                    else
                        lbExpressions.SelectedIndex = lbExpressions.Items.Count - 1;
                }
            }
        }

        /// <summary>
        /// Add the common C++ fix-up expressions
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnCppFixUps_Click(object sender, RoutedEventArgs e)
        {
            var cppExpressions = new (string Match, string Replacement, bool MatchAsRegEx)[] {
                // Overload topic links fix-up
                ("!:O:", "O:", false),
                // Strip out "`" followed by digits
                ("`[0-9]+(\\{)", "$1", true),
                // Strip out superfluous "^"
                ("(member name=\".*?System\\.Collections\\.Generic.*?)(\\^)", "$1", true),
                // Convert exclamation point to pipe
                ("(member name=\".*?System\\.Collections\\.Generic.*?)\\!", "$1|", true),
                // Fix-up valid cref attributes that the compiler couldn't figure out
                ("cref=\"!:([EFGMNPT]|Overload):", "cref=\"$1:", true),
                // Convert interior_ptr<T> to an explicit dereference
                (@"cli\.interior_ptr{([^}]+?)}", "$1@!System.Runtime.CompilerServices.IsExplicitlyDereferenced", true) };

            var expressions = lbExpressions.Items.Cast<MemberIdMatchExpression>().ToList();
            int idx = -1;

            foreach(var fixUp in cppExpressions)
            {
                if(!expressions.Any(t => t.MatchExpression == fixUp.Match))
                {
                    idx = lbExpressions.Items.Add(new MemberIdMatchExpression
                    {
                        MatchExpression = fixUp.Match,
                        ReplacementValue = fixUp.Replacement,
                        MatchAsRegEx = fixUp.MatchAsRegEx
                    });
                }
            }

            if(idx != -1)
            {
                lbExpressions.SelectedIndex = idx;

                btnDelete.IsEnabled = txtMatchExpression.IsEnabled = txtReplacementValue.IsEnabled =
                    chkUseRegEx.IsEnabled = true;
                txtMatchExpression.Focus();
            }
        }

        /// <summary>
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            var expressions = lbExpressions.Items.Cast<MemberIdMatchExpression>().ToList();

            if(expressions.Any(ex => ex.ErrorMessage != null))
            {
                MessageBox.Show("One or more member ID fix-up expressions are invalid.  Please fix the " +
                    "expressions before saving them.", Constants.AppName, MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }

            // Store the changes
            configuration.RemoveNodes();

            configuration.Add(new XElement("expressions",
                expressions.Select(ex => new XElement("expression",
                    new XAttribute("matchExpression", ex.MatchExpression),
                    new XAttribute("replacementValue", ex.ReplacementValue),
                    new XAttribute("matchAsRegEx", ex.MatchAsRegEx)))));

            this.DialogResult = true;
            this.Close();
        }
        #endregion
    }
}
