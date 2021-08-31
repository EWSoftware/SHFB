//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : ComponentPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 08/20/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This user control is used to edit the Components category properties
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
// 12/26/2013  EFW  Updated to use MEF for the build components
// 11/01/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.IO;
using System.Runtime.InteropServices;

using Microsoft.Build.Evaluation;

#if !STANDALONEGUI
using SandcastleBuilder.Package.Nodes;
#endif
using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;

using SandcastleBuilder.WPF.PropertyPages;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the Components category project properties
    /// </summary>
    [Guid("F3BA863D-9E18-477E-A62F-DFD679C8FEF7")]
    public partial class ComponentPropertiesPageControl : BasePropertyPage
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public ComponentPropertiesPageControl()
        {
            InitializeComponent();

            this.Title = "Components";
            this.HelpKeyword = "d1ec47f6-b611-41cf-a78c-f68e01d6ae9e";
            this.MinimumSize = DetermineMinimumSize(ucComponentPropertiesPageContent);
            this.Disposed += (s, e) => ucComponentPropertiesPageContent.Dispose();
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override void Initialize()
        {
            base.Initialize();

#pragma warning disable VSTHRD010
            ucComponentPropertiesPageContent.ComponentsModified += (s, e) => this.IsDirty = true;
#pragma warning restore VSTHRD010
            ucComponentPropertiesPageContent.ComponentSettingsNeeded += ucComponentPropertiesPageContent_ComponentSettingsNeeded;
        }

        /// <inheritdoc />
        protected override bool BindControlValue(string propertyName)
        {
            SandcastleProject currentProject = null;

#if !STANDALONEGUI
            if(this.ProjectMgr != null)
                currentProject = ((SandcastleBuilderProjectNode)this.ProjectMgr).SandcastleProject;
#else
            currentProject = this.CurrentProject;
#endif
            if(currentProject == null)
                ucComponentPropertiesPageContent.LoadComponentSettings(null, null);
            else
            {
                ucComponentPropertiesPageContent.LoadComponentSettings(currentProject.Filename,
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

            this.ProjectMgr.SetProjectProperty("ComponentConfigurations",
                ucComponentPropertiesPageContent.SelectedComponents.ToXml());
#else
            if(this.CurrentProject == null)
                return false;

            this.CurrentProject.MSBuildProject.SetProperty("ComponentConfigurations",
                ucComponentPropertiesPageContent.SelectedComponents.ToXml());
#endif
            return true;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// This is used to get the current component settings from the project when needed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ucComponentPropertiesPageContent_ComponentSettingsNeeded(object sender,
          ComponentSettingsNeededEventArgs e)
        {
            ProjectProperty componentsProp;

#if !STANDALONEGUI
            if(this.IsDisposed || this.ProjectMgr == null)
                return;

            componentsProp = this.ProjectMgr.BuildProject.GetProperty("ComponentConfigurations");
#else
            if(this.IsDisposed || this.CurrentProject == null)
                return;

            componentsProp = this.CurrentProject.MSBuildProject.GetProperty("ComponentConfigurations");
#endif
            var currentConfigs = new ComponentConfigurationDictionary();

            if(componentsProp != null && !String.IsNullOrEmpty(componentsProp.UnevaluatedValue))
                currentConfigs.FromXml(componentsProp.UnevaluatedValue);

            e.ProjectLoaded = true;
            e.Components = currentConfigs;
        }
        #endregion
    }
}
