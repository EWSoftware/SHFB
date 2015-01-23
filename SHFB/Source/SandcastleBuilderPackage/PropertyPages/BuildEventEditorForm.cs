//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : BuildEventEditorForm.cs
// Author  : Eric Woodruff
// Updated : 03/21/2014
// Note    : Copyright 2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a form used to edit a build event and insert macro placeholders
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/20/2014  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Microsoft.Build.Evaluation;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This form is used to edit a build event and insert macro placeholders
    /// </summary>
    public partial class BuildEventEditorForm : Form
    {
        #region Private data members
        //=====================================================================

        private SortedDictionary<string, string> macros;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the build event text to edit
        /// </summary>
        public string BuildEventText
        {
            get
            {
                return txtBuildEvent.Text;
            }
            set
            {
                txtBuildEvent.Text = value;
                txtBuildEvent.Select(0, 0);
                txtBuildEvent.ScrollToCaret();
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public BuildEventEditorForm()
        {
            InitializeComponent();

            macros = new SortedDictionary<string, string>();
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
                    if(project != null)
                        project.GlobalProperties.TryGetValue(property, out propertyValue);

                    macros.Add(property, propertyValue ?? "(Currently undefined)");
                }
            }
            catch(Exception ex)
            {
                macros["Globals"] = "Unable to determine global property values: " + ex.Message;
                btnInsertMacro.Enabled = false;
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

                    if(!Path.IsPathRooted(outputPath))
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
                btnInsertMacro.Enabled = false;
            }

            dgvMacros.DataSource = macros.ToList();
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Clear the highlight when entered so that we don't accidentally lose the text
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void txtBuildEvent_Enter(object sender, EventArgs e)
        {
            if(txtBuildEvent.SelectionLength != 0)
            {
                txtBuildEvent.Select(0, 0);
                txtBuildEvent.ScrollToCaret();
            }
        }

        /// <summary>
        /// Insert the selected macro name
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnInsertMacro_Click(object sender, EventArgs e)
        {
            if(dgvMacros.CurrentCellAddress.Y == -1)
            {
                if(dgvMacros.RowCount != 0)
                    dgvMacros.CurrentCell = dgvMacros[0, 0];
            }
            else
                txtBuildEvent.SelectedText = "$(" + (string)dgvMacros[0, dgvMacros.CurrentCellAddress.Y].Value + ")";

            txtBuildEvent.Focus();
        }

        /// <summary>
        /// Treat cell double clicks like insert requests
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void dgvMacros_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex > -1 && e.ColumnIndex > -1)
                btnInsertMacro_Click(sender, e);
        }
        #endregion
    }
}
