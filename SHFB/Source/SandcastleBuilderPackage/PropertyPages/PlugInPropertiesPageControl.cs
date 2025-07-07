//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : PlugInPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 06/22/2025
// Note    : Copyright 2011-2025, Eric Woodruff, All rights reserved
//
// This user control is used to edit the Plug-Ins category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/27/2011  EFW  Created the code
// 10/28/2012  EFW  Updated for use in the standalone GUI
// 12/18/2013  EFW  Updated to use MEF for the plug-ins
// 11/07/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.Runtime.InteropServices;

using Microsoft.Build.Evaluation;

using Sandcastle.Core.PlugIn;
using Sandcastle.Core.Project;

using SandcastleBuilder.MSBuild.HelpProject;



#if !STANDALONEGUI
using SandcastleBuilder.Package.Nodes;
#endif

using SandcastleBuilder.WPF.PropertyPages;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the Plug-In category project properties
    /// </summary>
    [Guid("8FB53BCE-82A8-4207-9DB6-7D30696C780C")]
    public partial class PlugInPropertiesPageControl : BasePropertyPage
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public PlugInPropertiesPageControl()
        {
            InitializeComponent();

            this.Title = "Plug-Ins";
            this.HelpKeyword = "be2b5b09-cf5f-4fc3-be8c-f6d8a27c3691";
            this.MinimumSize = DetermineMinimumSize(ucPlugInPropertiesPageContent);
            this.Disposed += (s, e) => ucPlugInPropertiesPageContent.Dispose();
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override void Initialize()
        {
            base.Initialize();

#pragma warning disable VSTHRD010
            ucPlugInPropertiesPageContent.PlugInsModified += (s, e) => this.IsDirty = true;
#pragma warning restore VSTHRD010
            ucPlugInPropertiesPageContent.ComponentSettingsNeeded += ucPlugInPropertiesPageContent_ComponentSettingsNeeded;
        }

        /// <inheritdoc />
        protected override bool BindControlValue(string propertyName)
        {
            ISandcastleProject currentProject = null;

#if !STANDALONEGUI
            if(this.ProjectMgr != null)
                currentProject = ((SandcastleBuilderProjectNode)this.ProjectMgr).SandcastleProject;
#else
            currentProject = this.CurrentProject;
#endif
            ucPlugInPropertiesPageContent.Project = currentProject;

            if(currentProject == null)
                ucPlugInPropertiesPageContent.LoadPlugInSettings(null, null);
            else
            {
                ucPlugInPropertiesPageContent.LoadPlugInSettings(currentProject.Filename,
                    currentProject.ComponentSearchPaths);
            }

            return true;
        }

        /// <inheritdoc />
        protected override bool StoreControlValue(string propertyName)
        {
#if !STANDALONEGUI
            if(this.ProjectMgr == null)
                return false;

            this.ProjectMgr.SetProjectProperty("PlugInConfigurations",
                ucPlugInPropertiesPageContent.SelectedPlugIns.ToXml());
#else
            if(this.CurrentProject == null)
                return false;

            ((SandcastleProject)this.CurrentProject).MSBuildProject.SetProperty("PlugInConfigurations",
                ucPlugInPropertiesPageContent.SelectedPlugIns.ToXml());
#endif
            return true;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// This is used to get the current plug-in settings from the project when needed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ucPlugInPropertiesPageContent_ComponentSettingsNeeded(object sender,
          ComponentSettingsNeededEventArgs e)
        {
            ProjectProperty plugInsProp;

#if !STANDALONEGUI
            if(this.IsDisposed || this.ProjectMgr == null)
                return;

            plugInsProp = this.ProjectMgr.BuildProject.GetProperty("PlugInConfigurations");
#else
            if(this.IsDisposed || this.CurrentProject == null)
                return;

            plugInsProp = ((SandcastleProject)this.CurrentProject).MSBuildProject.GetProperty("PlugInConfigurations");
#endif
            var currentConfigs = new PlugInConfigurationDictionary();

            if(plugInsProp != null && !String.IsNullOrEmpty(plugInsProp.UnevaluatedValue))
                currentConfigs.FromXml(plugInsProp.UnevaluatedValue);

            e.ProjectLoaded = true;
            e.PlugIns = currentConfigs;
        }
        #endregion
    }
}
