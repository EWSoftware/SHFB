//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : XPathReflectionFileFilterConfigDlgOld.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)  Based on code by Eyal Post
// Updated : 06/20/2025
// Note    : Copyright 2008-2025, Eric Woodruff, All rights reserved
//
// This file contains the form used to edit the XPath reflection file filter plug-in configuration
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/31/2008  EFW  Created the code
// 05/07/2021  EFW  Added MEF configuration editor export and converted the form to WPF for better high DPI
//                  scaling support on 4K displays.
//===============================================================================================================

// Ignore Spelling: Eyal

using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Linq;
using System.Xml.XPath;

using Sandcastle.Core;
using Sandcastle.Core.PlugIn;
using Sandcastle.Core.Project;

using Sandcastle.Platform.Windows;

namespace SandcastleBuilder.PlugIns.UI
{
    /// <summary>
    /// This form is used to edit the XPath reflection file filter plug-in settings
    /// </summary>
    public partial class XPathReflectionFileFilterConfigDlg : Window
    {
        #region Plug-in configuration editor factory for MEF
        //=====================================================================

        /// <summary>
        /// This allows editing of the plug-in configuration
        /// </summary>
        [PlugInConfigurationEditorExport("XPath Reflection File Filter")]
        public sealed class Factory : IPlugInConfigurationEditor
        {
            /// <inheritdoc />
            public bool EditConfiguration(ISandcastleProject project, XElement configuration)
            {
                var dlg = new XPathReflectionFileFilterConfigDlg(configuration);

                return dlg.ShowModalDialog() ?? false;
            }
        }
        #endregion

        #region Expression item for the list box
        //=====================================================================

        /// <summary>
        /// This is used to edit the expression
        /// </summary>
        private class ExpressionItem : INotifyPropertyChanged
        {
            /// <summary>
            /// The XPath query expression
            /// </summary>
            public string Expression
            {
                get;
                set
                {
                    if(field != value)
                    {
                        field = value;

                        try
                        {
                            // Make an attempt at validating the expression.  Just its syntax, not necessarily that
                            // it will work in the reflection file.
                            XDocument doc = new();
                            doc.XPathSelectElements(field);

                            this.ErrorMessage = null;
                        }
                        catch(Exception ex)
                        {
                            this.ErrorMessage = "Invalid expression: " + ex.Message;
                        }

                        this.OnPropertyChanged();
                    }
                }
            }

            /// <summary>
            /// The error if the expression is not valid
            /// </summary>
            public string ErrorMessage
            {
                get;
                set
                {
                    field = value;
                    this.OnPropertyChanged();
                }
            }

            /// <summary>
            /// The property changed event
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            /// <summary>
            /// This raises the <see cref="PropertyChanged"/> event
            /// </summary>
            /// <param name="propertyName">The property name that changed</param>
            protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        public XPathReflectionFileFilterConfigDlg(XElement configuration)
        {
            InitializeComponent();

            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // Load the current settings
            foreach(var n in configuration.Descendants("expression"))
                lbXPathQueries.Items.Add(new ExpressionItem { Expression = n.Value });

            btnDelete.IsEnabled = txtExpression.IsEnabled = lbXPathQueries.Items.Count != 0;

            if(lbXPathQueries.Items.Count != 0)
                lbXPathQueries.SelectedIndex = 0;
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
            UiUtility.ShowHelpTopic("cd68ef02-3493-4af6-96de-1957b0aaf858");
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
        /// Add a new XPath expression
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            int idx = lbXPathQueries.Items.Add(new ExpressionItem {
                Expression = "/reflection/apis/api[contains(@id,\"#ctor\")]" });
            lbXPathQueries.SelectedIndex = idx;

            btnDelete.IsEnabled = txtExpression.IsEnabled = true;
            txtExpression.Focus();
        }

        /// <summary>
        /// Delete the selected XPath expression
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            int idx = lbXPathQueries.SelectedIndex;

            if(idx != -1 && MessageBox.Show("Do you want to delete the expression '" +
              ((ExpressionItem)lbXPathQueries.SelectedItem).Expression + "'?", Constants.AppName,
              MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                lbXPathQueries.Items.RemoveAt(idx);

                if(lbXPathQueries.Items.Count == 0)
                {
                    txtExpression.Text = null;
                    btnDelete.IsEnabled = txtExpression.IsEnabled = false;
                }
                else
                {
                    if(idx < lbXPathQueries.Items.Count)
                        lbXPathQueries.SelectedIndex = idx;
                    else
                        lbXPathQueries.SelectedIndex = lbXPathQueries.Items.Count - 1;
                }
            }
        }

        /// <summary>
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            var expressions = lbXPathQueries.Items.Cast<ExpressionItem>().ToList();

            if(expressions.Any(ex => ex.ErrorMessage != null))
            {
                MessageBox.Show("One or more XPath query expressions are invalid.  Please fix the expressions " +
                    "before saving them.", Constants.AppName, MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }

            // Store the changes
            configuration.RemoveNodes();

            configuration.Add(new XElement("expressions",
                expressions.Select(ex => new XElement("expression", ex.Expression))));

            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Enable or disable the controls based on the selection
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbXPathQueries_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            btnDelete.IsEnabled = txtExpression.IsEnabled = lbXPathQueries.SelectedIndex != -1;
        }
        #endregion
    }
}
