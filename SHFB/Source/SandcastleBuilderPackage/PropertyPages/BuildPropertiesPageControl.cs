//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : BuildPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 07/01/2015
// Note    : Copyright 2011-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Build category properties.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/27/2011  EFW  Created the code
// 03/31/2012  EFW  Added BuildAssembler Verbosity property
// 10/28/2012  EFW  Updated for use in the standalone GUI
// 02/15/2014  EFW  Added support for the Open XML output format
// 03/30/2015  EFW  Added support for the Markdown output format
// 05/03/2015  EFW  Removed support for the MS Help 2 file format
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Microsoft.Build.Evaluation;

using Sandcastle.Core;
using Sandcastle.Core.Reflection;

using SandcastleBuilder.Utils;

#if !STANDALONEGUI
using SandcastleBuilder.Package.Nodes;
using SandcastleBuilder.Package.Properties;
#endif

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the Build category project properties
    /// </summary>
    [Guid("DD354863-2956-4B3B-B8EE-FFB3AAF30F82")]
    public partial class BuildPropertiesPageControl : BasePropertyPage
    {
        #region Help file format item
        //=====================================================================

        /// <summary>
        /// This is used to create a checked list box entry
        /// </summary>
        private class HelpFileFormatItem
        {
            /// <summary>The help file format</summary>
            public HelpFileFormats Format { get; set; }

            /// <summary>The description</summary>
            public string Description { get; set; }

            /// <summary>
            /// This returns the description
            /// </summary>
            /// <returns>The description</returns>
            public override string ToString()
            {
                return this.Description;
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private ReflectionDataSetDictionary reflectionDataSets;
        private string messageBoxTitle, lastProjectName;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public BuildPropertiesPageControl()
        {
            InitializeComponent();

#if !STANDALONEGUI
            messageBoxTitle = Resources.PackageTitle;
#else
            messageBoxTitle = Constants.AppName;
#endif
            // Set the maximum size to prevent an unnecessary vertical scrollbar
            this.MaximumSize = new System.Drawing.Size(2048, this.Height);

            this.Title = "Build";
            this.HelpKeyword = "da405a33-3eeb-4451-9aa8-a55be5026434";

            cblHelpFileFormat.Items.AddRange(new []
            {
                new HelpFileFormatItem { Format = HelpFileFormats.HtmlHelp1, Description = "HTML Help 1 (chm)" },
                new HelpFileFormatItem { Format = HelpFileFormats.MSHelpViewer, Description = "MS Help Viewer (mshc)" },
                new HelpFileFormatItem { Format = HelpFileFormats.OpenXml, Description = "Open XML (docx)" },
                new HelpFileFormatItem { Format = HelpFileFormats.Markdown, Description = "Markdown (md)" },
                new HelpFileFormatItem { Format = HelpFileFormats.Website, Description = "Website (HTML/ASP.NET)" }
            });

            cboBuildAssemblerVerbosity.DisplayMember = "Value";
            cboBuildAssemblerVerbosity.ValueMember = "Key";

            cboBuildAssemblerVerbosity.DataSource = (new Dictionary<string, string> {
                { BuildAssemblerVerbosity.AllMessages.ToString(), "All Messages" },
                { BuildAssemblerVerbosity.OnlyWarningsAndErrors.ToString(), "Only warnings and errors" },
                { BuildAssemblerVerbosity.OnlyErrors.ToString(), "Only Errors" } }).ToList();

            cboBuildAssemblerVerbosity.SelectedIndex = 1;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Try to load information about all available framework reflection data sets
        /// </summary>
        /// <param name="currentProject">The current Sandcastle project</param>
        private void LoadReflectionDataSetInfo(SandcastleProject currentProject)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                lastProjectName = currentProject == null ? null : currentProject.Filename;

                if(currentProject != null)
                    reflectionDataSets = new ReflectionDataSetDictionary(new[] {
                        currentProject.ComponentPath, Path.GetDirectoryName(currentProject.Filename) });
                else
                    reflectionDataSets = new ReflectionDataSetDictionary(null);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                MessageBox.Show("Unexpected error loading plug-ins: " + ex.Message, messageBoxTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            if(reflectionDataSets.Keys.Count == 0)
            {
                epWarning.SetError(cboFrameworkVersion, "No valid reflection data sets found.  Do you need " +
                    "to install the NuGet packages for them?");

                MessageBox.Show("No valid reflection data sets found", messageBoxTitle, MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                reflectionDataSets.Add(ReflectionDataSetDictionary.DefaultFrameworkTitle,
                    new ReflectionDataSet { Title = ReflectionDataSetDictionary.DefaultFrameworkTitle });
            }
            else
                epWarning.SetError(cboFrameworkVersion, String.Empty);
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override bool IsValid
        {
            get
            {
                // At least one format must be selected
                if(cblHelpFileFormat.CheckedItems.Count == 0)
                    cblHelpFileFormat.SetItemChecked(0, true);

                return true;
            }
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
#if!STANDALONEGUI
            // Set the project as the base path provider so that the folder is correct
            if(base.ProjectMgr != null)
            {
                SandcastleProject project = ((SandcastleBuilderProjectNode)base.ProjectMgr).SandcastleProject;
                txtBuildLogFile.File = new FilePath(project);
            }
#else
            // Set the project as the base path provider so that the folder is correct
            if(base.CurrentProject != null)
                txtBuildLogFile.File = new FilePath(base.CurrentProject);
#endif
        }

        /// <inheritdoc />
        protected override bool BindControlValue(System.Windows.Forms.Control control)
        {
            ProjectProperty projProp;
            HelpFileFormats format;
            int idx;

            if(control.Name == "cboFrameworkVersion")
            {
#if !STANDALONEGUI
                SandcastleProject currentProject = null;

                if(base.ProjectMgr != null)
                    currentProject = ((SandcastleBuilderProjectNode)base.ProjectMgr).SandcastleProject;

                if(reflectionDataSets == null || currentProject == null || currentProject.Filename != lastProjectName)
                    this.LoadReflectionDataSetInfo(currentProject);
#else
                if(reflectionDataSets == null || base.CurrentProject == null || base.CurrentProject.Filename != lastProjectName)
                    this.LoadReflectionDataSetInfo(base.CurrentProject);
#endif

                cboFrameworkVersion.Items.Clear();
                cboFrameworkVersion.Items.AddRange(reflectionDataSets.Keys.OrderBy(k => k).ToArray());
                cboFrameworkVersion.SelectedItem = ReflectionDataSetDictionary.DefaultFrameworkTitle;

                return false;
            }

            // Get the selected help file formats
            if(control.Name == "cblHelpFileFormat")
            {
                List<string> allFormats = cblHelpFileFormat.Items.OfType<HelpFileFormatItem>().Select(
                    f => f.Format.ToString()).ToList();

#if !STANDALONEGUI
                projProp = this.ProjectMgr.BuildProject.GetProperty("HelpFileFormat");
#else
                projProp = this.CurrentProject.MSBuildProject.GetProperty("HelpFileFormat");
#endif

                if(projProp == null || !Enum.TryParse<HelpFileFormats>(projProp.UnevaluatedValue, out format))
                    format = HelpFileFormats.HtmlHelp1;

                for(idx = 0; idx < cblHelpFileFormat.Items.Count; idx++)
                    cblHelpFileFormat.SetItemChecked(idx, false);

                foreach(string s in format.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    idx = allFormats.IndexOf(s.Trim());

                    if(idx != -1)
                        cblHelpFileFormat.SetItemChecked(idx, true);
                }

                return true;
            }

            return false;
        }

        /// <inheritdoc />
        protected override bool StoreControlValue(System.Windows.Forms.Control control)
        {
            string formats;

#if !STANDALONEGUI
            if(this.ProjectMgr == null)
                return false;
#else
            if(this.CurrentProject == null)
                return false;
#endif
            // Set the selected help file formats value
            if(control.Name == "cblHelpFileFormat")
            {
                formats = String.Join(", ", cblHelpFileFormat.CheckedItems.Cast<HelpFileFormatItem>().Select(
                    f => f.Format.ToString()).ToArray());

#if !STANDALONEGUI
                this.ProjectMgr.SetProjectProperty("HelpFileFormat", formats);
#else
                this.CurrentProject.MSBuildProject.SetProperty("HelpFileFormat", formats);
#endif
                return true;
            }

            return false;
        }
        #endregion
    }
}
