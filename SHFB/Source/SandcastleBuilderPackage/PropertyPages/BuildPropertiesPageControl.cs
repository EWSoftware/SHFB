//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : BuildPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 11/16/2017
// Note    : Copyright 2011-2017, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Build category properties
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
// 11/08/2017  EFW  Moved the Presentation Style and Syntax Filters from the Help File page to this one
// 11/15/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.IO;
using System.Runtime.InteropServices;

using Microsoft.Build.Evaluation;

using Sandcastle.Core;

using SandcastleBuilder.Utils;
using SandcastleBuilder.WPF.PropertyPages;

#if !STANDALONEGUI
using SandcastleBuilder.Package.Nodes;
#endif

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the Build category project properties
    /// </summary>
    [Guid("DD354863-2956-4B3B-B8EE-FFB3AAF30F82")]
    public partial class BuildPropertiesPageControl : BasePropertyPage
    {
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
            this.MinimumSize = DetermineMinimumSize(ucBuildPropertiesPageContent);
            this.Disposed += (s, e) => ucBuildPropertiesPageContent.Dispose();
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override void Initialize()
        {
            ucBuildPropertiesPageContent.PropertyChanged += (s, e) => this.IsDirty = true;
            ucBuildPropertiesPageContent.BuildPropertiesNeeded += ucBuildPropertiesPageContent_BuildPropertiesNeeded;

            // Set the project as the base path provider so that the folder is correct
#if!STANDALONEGUI
            if(base.ProjectMgr != null)
            {
                SandcastleProject project = ((SandcastleBuilderProjectNode)base.ProjectMgr).SandcastleProject;
#else
            // Set the project as the base path provider so that the folder is correct
            if(base.CurrentProject != null)
            {
                SandcastleProject project = base.CurrentProject;
#endif
                ucBuildPropertiesPageContent.SetCurrentProject(project);
            }
        }

        /// <inheritdoc />
        protected override bool BindControlValue(string propertyName)
        {
            SandcastleProject currentProject = null;

#if !STANDALONEGUI
            if(this.ProjectMgr == null)
            {
                ucBuildPropertiesPageContent.LoadBuildFormatInfo(null, null);
                return false;
            }

            currentProject = ((SandcastleBuilderProjectNode)this.ProjectMgr).SandcastleProject;
#else
            if(this.CurrentProject == null)
            {
                ucBuildPropertiesPageContent.LoadBuildFormatInfo(null, null);
                return false;
            }

            currentProject = this.CurrentProject;
#endif
            if(propertyName == "FrameworkVersion")
            {
                ucBuildPropertiesPageContent.LoadReflectionDataSetInfo(currentProject);
                return false;
            }

            // Get the selected help file formats
            if(propertyName == "HelpFileFormat")
            {
                HelpFileFormats formats;

                ProjectProperty projProp = currentProject.MSBuildProject.GetProperty("HelpFileFormat");

                if(projProp == null || !Enum.TryParse<HelpFileFormats>(projProp.UnevaluatedValue, out formats))
                    formats = HelpFileFormats.HtmlHelp1;

                ucBuildPropertiesPageContent.SelectedHelpFileFormats = formats;
                return true;
            }

            // Load the presentation styles and syntax filters
            if(propertyName == "SyntaxFilters")
            {
                ucBuildPropertiesPageContent.LoadBuildFormatInfo(currentProject.Filename,
                    new[] { currentProject.ComponentPath, Path.GetDirectoryName(currentProject.Filename) });

                return true;
            }

            // This is loaded along with the syntax filters after the components are determined
            if(propertyName == "PresentationStyle")
                return true;

            return false;
        }

        /// <inheritdoc />
        protected override bool StoreControlValue(string propertyName)
        {
#if !STANDALONEGUI
            if(this.ProjectMgr == null)
                return false;
#else
            if(this.CurrentProject == null)
                return false;
#endif
            if(propertyName == "HelpFileFormat")
            {
#if !STANDALONEGUI
                this.ProjectMgr.SetProjectProperty("HelpFileFormat",
                    ucBuildPropertiesPageContent.SelectedHelpFileFormats.ToString());
#else
                this.CurrentProject.MSBuildProject.SetProperty("HelpFileFormat",
                    ucBuildPropertiesPageContent.SelectedHelpFileFormats.ToString());
#endif
                return true;
            }

            if(propertyName == "SyntaxFilters")
            {
#if !STANDALONEGUI
                this.ProjectMgr.SetProjectProperty("SyntaxFilters",
                    ucBuildPropertiesPageContent.SelectedSyntaxFilters);
#else
                this.CurrentProject.MSBuildProject.SetProperty("SyntaxFilters",
                    ucBuildPropertiesPageContent.SelectedSyntaxFilters);
#endif
                return true;
            }


            if(propertyName == "PresentationStyle")
            {
#if !STANDALONEGUI
                this.ProjectMgr.SetProjectProperty("PresentationStyle",
                    ucBuildPropertiesPageContent.SelectedPresentationStyle);
#else
                this.CurrentProject.MSBuildProject.SetProperty("SyntaxFilters",
                    ucBuildPropertiesPageContent.SelectedPresentationStyle);
#endif
            }

            return false;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// This is used to get the current build property values from the project when needed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ucBuildPropertiesPageContent_BuildPropertiesNeeded(object sender,
          BuildPropertiesNeededEventArgs e)
        {
            ProjectProperty presentationStyleProp, syntaxFiltersProp;

#if !STANDALONEGUI
            if(this.IsDisposed || this.ProjectMgr == null)
                return;

            presentationStyleProp = this.ProjectMgr.BuildProject.GetProperty("PresentationStyle");
            syntaxFiltersProp = this.ProjectMgr.BuildProject.GetProperty("SyntaxFilters");
#else
            if(this.IsDisposed || this.CurrentProject == null)
                return;

            presentationStyleProp = this.CurrentProject.MSBuildProject.GetProperty("PresentationStyle");
            syntaxFiltersProp = this.CurrentProject.MSBuildProject.GetProperty("SyntaxFilters");
#endif

            e.ProjectLoaded = true;
            e.PresentationStyle = presentationStyleProp?.UnevaluatedValue;
            e.SyntaxFilters = syntaxFiltersProp?.UnevaluatedValue;
        }
        #endregion
    }
}
