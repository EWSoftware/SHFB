//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : BuildEventEditorDlg.xaml.cs
// Author  : Eric Woodruff
// Updated : 07/03/2025
// Note    : Copyright 2014-2025, Eric Woodruff, All rights reserved
//
// This file contains a form used to edit a build event and insert macro placeholders
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/20/2014  EFW  Created the code
// 10/03/2017  EFW  Converted the form to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Microsoft.Build.Evaluation;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This form is used to edit a build event and insert macro placeholders
    /// </summary>
    public partial class BuildEventEditorDlg : Window
    {
        #region Private data members
        //=====================================================================

        private readonly SortedDictionary<string, string> macros;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the build event text to edit
        /// </summary>
        public string BuildEventText
        {
            get => txtBuildEvent.Text;
            set
            {
                txtBuildEvent.Text = value;
                txtBuildEvent.Select(0, 0);
                txtBuildEvent.ScrollToHome();
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public BuildEventEditorDlg()
        {
            InitializeComponent();

            macros = [];
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to determine the current macro values which can be inserted
        /// </summary>
        /// <param name="project">The project used to resolve the current values</param>
        public void DetermineMacroValues(Project project)
        {
            string propertyValue = null, outputPath;

            try
            {
                foreach(string property in new[] { "SolutionDir", "SolutionName", "SolutionFileName", "SolutionPath" })
                {
                    project?.GlobalProperties.TryGetValue(property, out propertyValue);

                    macros.Add(property, propertyValue ?? "(Currently undefined)");
                }
            }
            catch(Exception ex)
            {
                macros["Globals"] = "Unable to determine global property values: " + ex.Message;
                btnInsertMacro.IsEnabled = false;
            }

            try
            {
                foreach(string property in new[] { "Configuration", "HtmlHelpName", "OutDir", "OutputPath",
                  "Platform", "ProjectDir", "ProjectFileName", "ProjectName", "ProjectPath", "WorkingPath" })
                {
                    if(project != null)
                        propertyValue = project.GetPropertyValue(property);

                    macros.Add(property, propertyValue ?? "(Currently undefined)");
                }

                // Expand the output and working paths if relative
                outputPath = macros["OutputPath"];

                if(outputPath.Length == 0 || outputPath[0] != '(')
                {
                    if(outputPath.Length == 0)
                        outputPath = "Help\\";

                    if(!Path.IsPathRooted(outputPath) && project != null)
                    {
                        outputPath = Path.GetFullPath(Path.Combine(project.DirectoryPath, outputPath));
                        macros["OutputPath"] = outputPath;
                    }

                    if(macros["OutDir"] == ".\\")
                        macros["OutDir"] = outputPath;

                    propertyValue = macros["WorkingPath"];

                    if((propertyValue.Length == 0 || propertyValue[0] != '(') && !Path.IsPathRooted(propertyValue))
                    {
                        if(propertyValue.Length == 0)
                            propertyValue = "Working\\";

                        macros["WorkingPath"] = Path.GetFullPath(Path.Combine(outputPath, propertyValue));
                    }
                }
            }
            catch(Exception ex)
            {
                macros["Project"] = "Unable to determine project property values: " + ex.Message;
                btnInsertMacro.IsEnabled = false;
            }

            dgMacros.ItemsSource = macros.ToList();
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Close the form and accept the edited text
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        /// <summary>
        /// Insert the selected macro name
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnInsertMacro_Click(object sender, RoutedEventArgs e)
        {
            if(dgMacros.SelectedIndex == -1)
            {
                if(dgMacros.Items.Count != 0)
                    dgMacros.SelectedIndex = 0;
            }
            else
            {
                var selectedItem = (KeyValuePair<string, string>)dgMacros.SelectedValue;

                txtBuildEvent.SelectedText = "$(" + selectedItem.Key + ")";
            }
            
            txtBuildEvent.Focus();
        }

        /// <summary>
        /// Treat cell double clicks like insert requests
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void dgMacros_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(dgMacros.SelectedIndex != -1)
                btnInsertMacro_Click(sender, e);
        }
        #endregion
    }
}
