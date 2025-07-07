//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : TransformArgumentsPageControl.cs
// Author  : Eric Woodruff
// Updated : 06/22/2025
// Note    : Copyright 2012-2025, Eric Woodruff, All rights reserved
//
// This user control is used to edit the Transform Arguments category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/14/2012  EFW  Created the code
// 01/07/2014  EFW  Updated to use MEF for loading the presentation styles
// 10/25/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
// ==============================================================================================================

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;

using Microsoft.Build.Evaluation;

using Sandcastle.Core;
using Sandcastle.Core.Project;

using SandcastleBuilder.MSBuild.HelpProject;



#if !STANDALONEGUI
using SandcastleBuilder.Package.Nodes;
#endif
using SandcastleBuilder.WPF.PropertyPages;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the Transform Arguments category project properties
    /// </summary>
    [Guid("5325A09E-BFFF-4056-A331-0FBDF9FF8AF4")]
    public partial class TransformArgumentsPageControl : BasePropertyPage
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public TransformArgumentsPageControl()
        {
            InitializeComponent();

            this.Title = "Transform Args";
            this.HelpKeyword = "c584509f-0b18-49a8-ab06-114b0058a739";
            this.MinimumSize = DetermineMinimumSize(ucTransformArgumentsPageContent);
            this.Disposed += (s, e) => ucTransformArgumentsPageContent.Dispose();
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override bool IsValid => ucTransformArgumentsPageContent.IsValid;

        /// <inheritdoc />
        protected override void Initialize()
        {
            base.Initialize();

            ucTransformArgumentsPageContent.PropertyChanged += this.OnPropertyChanged;
            ucTransformArgumentsPageContent.PresentationStyleSettingsNeeded += ucTransformArgumentsPageContent_PresentationStyleSettingsNeeded;
            ucTransformArgumentsPageContent.RefreshValues += (s, e) => this.BindControlValue(null);
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
            if(currentProject == null)
                ucTransformArgumentsPageContent.LoadArgumentSettings(null, null);
            else
            {
                ucTransformArgumentsPageContent.LoadArgumentSettings(currentProject.Filename,
                    currentProject.ComponentSearchPaths);
            }

            return true;
        }

        /// <inheritdoc />
        protected override bool StoreControlValue(string propertyName)
        {
            XElement root = new("TransformComponentArguments",
                ucTransformArgumentsPageContent.TransformationArguments.Select(t => t.ToXml()));

            var reader = root.CreateReader();
            reader.MoveToContent();

#if !STANDALONEGUI
            if(this.ProjectMgr == null)
                return false;

            this.ProjectMgr.SetProjectProperty("TransformComponentArguments", reader.ReadInnerXml());
#else
            if(this.CurrentProject == null)
                return false;

            ((SandcastleProject)this.CurrentProject).MSBuildProject.SetProperty("TransformComponentArguments", reader.ReadInnerXml());
#endif
            return true;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// This is used to get the current presentation style settings from the project when needed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ucTransformArgumentsPageContent_PresentationStyleSettingsNeeded(object sender,
          PresentationStyleSettingsNeededEventArgs e)
        {
            ProjectProperty argsProp, styleProp;

#if !STANDALONEGUI
            if(this.IsDisposed || this.ProjectMgr == null)
                return;

            argsProp = this.ProjectMgr.BuildProject.GetProperty("TransformComponentArguments");
            styleProp = this.ProjectMgr.BuildProject.GetProperty("PresentationStyle");
#else
            if(this.IsDisposed || this.CurrentProject == null)
                return;

            var project = ((SandcastleProject)this.CurrentProject).MSBuildProject;

            argsProp = project.GetProperty("TransformComponentArguments");
            styleProp = project.GetProperty("PresentationStyle");
#endif
            e.ProjectLoaded = true;
            e.PresentationStyle = (styleProp != null) ? styleProp.UnevaluatedValue : Constants.DefaultPresentationStyle;
            e.TransformComponentArguments = argsProp?.UnevaluatedValue;
        }
        #endregion
    }
}
