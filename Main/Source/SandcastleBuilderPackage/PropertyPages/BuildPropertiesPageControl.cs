//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : BuildPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 03/31/2012
// Note    : Copyright 2011-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Build category properties.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.0  03/27/2011  EFW  Created the code
// 1.9.4.0  03/31/2012  EFW  Added BuildAssembler Verbosity property
//=============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using Microsoft.Build.Evaluation;

using SandcastleBuilder.Package.Nodes;
using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.Design;

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
        /// This is used to create a checked listbox entry
        /// </summary>
        private class HelpFileFormatItem
        {
            /// <summary>The help file format</summary>
            public HelpFileFormat Format { get; set; }

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

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public BuildPropertiesPageControl()
        {
            InitializeComponent();

            this.Title = "Build";
            this.HelpKeyword = "da405a33-3eeb-4451-9aa8-a55be5026434";

            cblHelpFileFormat.Items.AddRange(new []
            {
                new HelpFileFormatItem { Format = HelpFileFormat.HtmlHelp1, Description = "HTML Help 1 (CHM)" },
                new HelpFileFormatItem { Format = HelpFileFormat.MSHelp2, Description = "MS Help 2 (HxS)" },
                new HelpFileFormatItem { Format = HelpFileFormat.MSHelpViewer, Description = "MS Help Viewer (MSHC)" },
                new HelpFileFormatItem { Format = HelpFileFormat.Website, Description = "Website (HTML/ASP.NET)" }
            });

            cboFrameworkVersion.Items.AddRange(FrameworkVersionTypeConverter.StandardValues.ToArray());
            cboFrameworkVersion.SelectedItem = FrameworkVersionTypeConverter.LatestFrameworkMatching(".NET 4");

            cboBuildAssemblerVerbosity.DisplayMember = "Value";
            cboBuildAssemblerVerbosity.ValueMember = "Key";

            cboBuildAssemblerVerbosity.DataSource = (new Dictionary<string, string> {
                { BuildAssemblerVerbosity.AllMessages.ToString(), "All Messages" },
                { BuildAssemblerVerbosity.OnlyWarningsAndErrors.ToString(), "Only warnings and errors" },
                { BuildAssemblerVerbosity.OnlyErrors.ToString(), "Only Errors" } }).ToList();
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
            // Set the project as the base path provider so that the folder is correct
            if(base.ProjectMgr != null)
            {
                SandcastleProject project = ((SandcastleBuilderProjectNode)base.ProjectMgr).SandcastleProject;
                txtBuildLogFile.File = new FilePath(project);
            }
        }

        /// <inheritdoc />
        protected override bool BindControlValue(System.Windows.Forms.Control control)
        {
            ProjectProperty projProp;
            HelpFileFormat format;
            int idx;

            // Get the selected help file formats
            if(control.Name == "cblHelpFileFormat")
            {
                for(idx = 0; idx < cblHelpFileFormat.Items.Count; idx++)
                    cblHelpFileFormat.SetItemChecked(idx, false);

                List<string> allFormats = cblHelpFileFormat.Items.OfType<HelpFileFormatItem>().Select(
                    f => f.Format.ToString()).ToList();

                projProp = this.ProjectMgr.BuildProject.GetProperty("HelpFileFormat");

                if(projProp == null || !Enum.TryParse<HelpFileFormat>(projProp.UnevaluatedValue, out format))
                    format = HelpFileFormat.HtmlHelp1;

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

            if(this.ProjectMgr == null)
                return false;

            // Set the selected help file formats value
            if(control.Name == "cblHelpFileFormat")
            {
                formats = String.Join(", ", cblHelpFileFormat.CheckedItems.Cast<HelpFileFormatItem>().Select(
                    f => f.Format.ToString()).ToArray());

                this.ProjectMgr.SetProjectProperty("HelpFileFormat", formats);

                return true;
            }

            return false;
        }
        #endregion
    }
}
