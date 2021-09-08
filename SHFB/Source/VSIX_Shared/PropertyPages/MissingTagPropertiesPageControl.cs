//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : MissingTagPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 04/20/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This user control is used to edit the Missing Tags category properties
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
// 10/04/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.Runtime.InteropServices;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the Missing Tags category project properties
    /// </summary>
    [Guid("D3532308-9069-4DCA-8FC0-E0897D3F5C07")]
    public partial class MissingTagPropertiesPageControl : BasePropertyPage
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public MissingTagPropertiesPageControl()
        {
            InitializeComponent();

            this.Title = "Missing Tags";
            this.HelpKeyword = "5a2ab898-7161-454d-b5d3-959df0de0e36";
            this.MinimumSize = DetermineMinimumSize(ucMissingTagPropertiesContent);
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override void Initialize()
        {
            base.Initialize();

            ucMissingTagPropertiesContent.PropertyChanged += this.OnPropertyChanged;
        }

        /// <inheritdoc />
        protected override bool BindControlValue(string propertyName)
        {
#if !STANDALONEGUI
            if(this.ProjectMgr == null)
                return false;

            var projProp = this.ProjectMgr.BuildProject.GetProperty("MissingTags");
#else
            if(this.CurrentProject == null)
                return false;

            var projProp = this.CurrentProject.MSBuildProject.GetProperty("MissingTags");
#endif
            // If not found or not valid, we'll ignore it and use the defaults
            if(projProp == null || !Enum.TryParse(projProp.UnevaluatedValue, out MissingTags tags))
            {
                tags = MissingTags.Summary | MissingTags.Parameter | MissingTags.TypeParameter |
                    MissingTags.Returns | MissingTags.AutoDocumentCtors | MissingTags.Namespace |
                    MissingTags.AutoDocumentDispose;
            }

            ucMissingTagPropertiesContent.MissingTags = tags;

            return true;
        }

        /// <inheritdoc />
        protected override bool StoreControlValue(string propertyName)
        {
#if !STANDALONEGUI
            if(this.ProjectMgr == null)
                return false;

            this.ProjectMgr.SetProjectProperty("MissingTags",
                ucMissingTagPropertiesContent.MissingTags.ToString());
#else
            if(this.CurrentProject == null)
                return false;

            this.CurrentProject.MSBuildProject.SetProperty("MissingTags",
                ucMissingTagPropertiesContent.MissingTags.ToString());
#endif
            return true;
        }
        #endregion
    }
}
