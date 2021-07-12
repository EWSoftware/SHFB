//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : VisibilityPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 04/20/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This user control is used to edit the Visibility category properties
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
// 12/03/2013  EFW  Added support for no-PIA types
// 06/19/2015  EFW  Added support for public compiler generated types/members
// 09/22/2017  EFW  Added support for filtering by EditorBrowsableAttribute and BrowsableAttribute state
// 10/23/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.Runtime.InteropServices;

using Microsoft.Build.Evaluation;

#if !STANDALONEGUI
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

using SandcastleBuilder.Package.Nodes;
#endif
using Sandcastle.Core;
using SandcastleBuilder.Utils;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the Visibility category project properties
    /// </summary>
    [Guid("318912BF-0E26-485C-9183-CC35B8D47591")]
    public partial class VisibilityPropertiesPageControl : BasePropertyPage
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public VisibilityPropertiesPageControl()
        {
            InitializeComponent();

            this.Title = "Visibility";
            this.HelpKeyword = "3c489bd6-598c-4684-aafb-fbe9400864d3";
            this.MinimumSize = DetermineMinimumSize(ucVisibilityPropertiesPageContent);
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override void Initialize()
        {
            base.Initialize();

            ucVisibilityPropertiesPageContent.ApplyChanges += (s, e) =>
            {
#if !STANDALONEGUI
                Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

                e.ChangesApplied = (this.ProjectMgr != null && (!this.IsDirty ||
                    ((IPropertyPage)this).Apply() == VSConstants.S_OK));
#else
                e.ChangesApplied = (this.CurrentProject != null && (!this.IsDirty || this.Apply()));
#endif
            };

            ucVisibilityPropertiesPageContent.PropertyChanged += this.OnPropertyChanged;
        }

        /// <inheritdoc />
        protected override bool BindControlValue(string propertyName)
        {
            ProjectProperty projProp;

#if !STANDALONEGUI
            if(this.ProjectMgr == null)
                return false;
#else
            if(this.CurrentProject == null)
                return false;
#endif

            if(propertyName == "ApiFilter")
            {
                // Pass it the Sandcastle project instance as we use the designer dialog to edit the collection
                // and it obtains it from the collection to do the required partial build.
#if !STANDALONEGUI
                var filter = new ApiFilterCollection {
                    Project = ((SandcastleBuilderProjectNode)this.ProjectMgr).SandcastleProject };

                projProp = this.ProjectMgr.BuildProject.GetProperty("ApiFilter");
#else
                var filter = new ApiFilterCollection { Project = this.CurrentProject };

                projProp = this.CurrentProject.MSBuildProject.GetProperty("ApiFilter");
#endif
                if(projProp != null && !String.IsNullOrEmpty(projProp.UnevaluatedValue))
                    filter.FromXml(projProp.UnevaluatedValue);

                ucVisibilityPropertiesPageContent.ApiFilter = filter;
                ucVisibilityPropertiesPageContent.ApiFilterHasChanges = false;
                ucVisibilityPropertiesPageContent.UpdateApiFilterInfo();
                return true;
            }

#if !STANDALONEGUI
            projProp = this.ProjectMgr.BuildProject.GetProperty("VisibleItems");
#else
            projProp = this.CurrentProject.MSBuildProject.GetProperty("VisibleItems");
#endif
            // If not found or not valid, we'll ignore it and use the defaults
            if(projProp == null || !Enum.TryParse(projProp.UnevaluatedValue, out VisibleItems items))
            {
                items = VisibleItems.InheritedFrameworkMembers | VisibleItems.InheritedMembers |
                    VisibleItems.Protected | VisibleItems.ProtectedInternalAsProtected | VisibleItems.NonBrowsable;
            }

            ucVisibilityPropertiesPageContent.VisibleItems = items;
            return true;
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
            if(propertyName == "ApiFilter")
            {
                if(ucVisibilityPropertiesPageContent.ApiFilterHasChanges)
                {
#if !STANDALONEGUI
                    this.ProjectMgr.SetProjectProperty("ApiFilter",
                        ucVisibilityPropertiesPageContent.ApiFilter.ToXml());
#else
                    this.CurrentProject.MSBuildProject.SetProperty("ApiFilter",
                        ucVisibilityPropertiesPageContent.ApiFilter.ToXml());
#endif
                    ucVisibilityPropertiesPageContent.ApiFilterHasChanges = false;
                }

                return true;
            }

            VisibleItems items = ucVisibilityPropertiesPageContent.VisibleItems;

#if !STANDALONEGUI
            this.ProjectMgr.SetProjectProperty("VisibleItems", items.ToString());
#else
            this.CurrentProject.MSBuildProject.SetProperty("VisibleItems", items.ToString());
#endif
            return true;
        }
        #endregion
    }
}
