//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : MissingTagPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 10/28/2012
// Note    : Copyright 2011-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Missing Tags category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.3.0  03/27/2011  EFW  Created the code
// 1.9.6.0  10/28/2012  EFW  Updated for use in the standalone GUI
//===============================================================================================================

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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

            // Set the maximum size to prevent an unnecessary vertical scrollbar
            this.MaximumSize = new System.Drawing.Size(2048, this.Height);

            this.Title = "Missing Tags";
            this.HelpKeyword = "5a2ab898-7161-454d-b5d3-959df0de0e36";

            // We are responsible for connecting the change event to all of the unbound controls
            chkAutoDocumentDisposeMethods.CheckedChanged += base.OnPropertyChanged;
            chkShowMissingIncludeTargets.CheckedChanged += base.OnPropertyChanged;
            chkShowMissingNamespaces.CheckedChanged += base.OnPropertyChanged;
            chkShowMissingParams.CheckedChanged += base.OnPropertyChanged;
            chkShowMissingRemarks.CheckedChanged += base.OnPropertyChanged;
            chkShowMissingReturns.CheckedChanged += base.OnPropertyChanged;
            chkShowMissingSummaries.CheckedChanged += base.OnPropertyChanged;
            chkShowMissingTypeParams.CheckedChanged += base.OnPropertyChanged;
            chkShowMissingValues.CheckedChanged += base.OnPropertyChanged;
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>For this page, we only need to bind one control as all the values are stored in a single
        /// property.</remarks>
        protected override bool BindControlValue(Control control)
        {
            MissingTags tags;

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
            if(projProp != null && Enum.TryParse<MissingTags>(projProp.UnevaluatedValue, out tags))
            {
                chkAutoDocumentConstructors.Checked = ((tags & MissingTags.AutoDocumentCtors) != 0);
                chkAutoDocumentDisposeMethods.Checked = ((tags & MissingTags.AutoDocumentDispose) != 0);
                chkShowMissingIncludeTargets.Checked = ((tags & MissingTags.IncludeTargets) != 0);
                chkShowMissingNamespaces.Checked = ((tags & MissingTags.Namespace) != 0);
                chkShowMissingParams.Checked = ((tags & MissingTags.Parameter) != 0);
                chkShowMissingRemarks.Checked = ((tags & MissingTags.Remarks) != 0);
                chkShowMissingReturns.Checked = ((tags & MissingTags.Returns) != 0);
                chkShowMissingSummaries.Checked = ((tags & MissingTags.Summary) != 0);
                chkShowMissingTypeParams.Checked = ((tags & MissingTags.TypeParameter) != 0);
                chkShowMissingValues.Checked = ((tags & MissingTags.Value) != 0);
            }

            return true;
        }

        /// <inheritdoc />
        /// <remarks>For this page, we only need to bind one control as all the values are stored in a single
        /// property.</remarks>
        protected override bool StoreControlValue(Control control)
        {
            MissingTags tags = MissingTags.None;

#if !STANDALONEGUI
            if(this.ProjectMgr == null)
                return false;
#else
            if(this.CurrentProject == null)
                return false;
#endif
            if(chkAutoDocumentConstructors.Checked)
                tags |= MissingTags.AutoDocumentCtors;

            if(chkAutoDocumentDisposeMethods.Checked)
                tags |= MissingTags.AutoDocumentDispose;

            if(chkShowMissingIncludeTargets.Checked)
                tags |= MissingTags.IncludeTargets;

            if(chkShowMissingNamespaces.Checked)
                tags |= MissingTags.Namespace;
            
            if(chkShowMissingParams.Checked)
                tags |= MissingTags.Parameter;
            
            if(chkShowMissingRemarks.Checked)
                tags |= MissingTags.Remarks;
            
            if(chkShowMissingReturns.Checked)
                tags |= MissingTags.Returns;
            
            if(chkShowMissingSummaries.Checked)
                tags |= MissingTags.Summary;
            
            if(chkShowMissingTypeParams.Checked)
                tags |= MissingTags.TypeParameter;

            if(chkShowMissingValues.Checked)
                tags |= MissingTags.Value;

#if !STANDALONEGUI
            this.ProjectMgr.SetProjectProperty("MissingTags", tags.ToString());
#else
            this.CurrentProject.MSBuildProject.SetProperty("MissingTags", tags.ToString());
#endif
            return true;
        }
        #endregion
    }
}
