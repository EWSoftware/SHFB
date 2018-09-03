//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : SummaryPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 09/02/2018
// Note    : Copyright 2011-2018, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Summaries category properties
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
// 10/10/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
// ==============================================================================================================

using System;
using System.Runtime.InteropServices;

using Microsoft.Build.Evaluation;

#if !STANDALONEGUI
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

using SandcastleBuilder.Package.Nodes;
#endif
using SandcastleBuilder.Utils;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the Summaries category project properties
    /// </summary>
    [Guid("C2055DCA-54C2-4047-B0BD-87464BA6BA95")]
    public partial class SummaryPropertiesPageControl : BasePropertyPage
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public SummaryPropertiesPageControl()
        {
            InitializeComponent();

            this.Title = "Summaries";
            this.HelpKeyword = "eb7e1bc7-21c5-4453-bbaf-dec8c62c15bd";
            this.MinimumSize = DetermineMinimumSize(ucSummaryPropertiesPageContent);
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override bool IsEscapedProperty(string propertyName)
        {
            return (propertyName == "ProjectSummary");
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            base.Initialize();

            ucSummaryPropertiesPageContent.ApplyChanges += (s, e) =>
            {
#if !STANDALONEGUI
                Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

                e.ChangesApplied = (this.ProjectMgr != null && (!this.IsDirty ||
                    ((IPropertyPage)this).Apply() == VSConstants.S_OK));
#else
                e.ChangesApplied = (this.CurrentProject != null && (!this.IsDirty || this.Apply()));
#endif
            };

#pragma warning disable VSTHRD010
            ucSummaryPropertiesPageContent.SummariesModified += (s, e) => this.IsDirty = true;
#pragma warning restore VSTHRD010
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
            if(propertyName == "NamespaceSummaries")
            {
                // Pass it the Sandcastle project instance as we use the designer dialog to edit the collection
                // and it obtains it from the collection to do the required partial build.
#if !STANDALONEGUI
                var namespaceSummaries = new NamespaceSummaryItemCollection {
                    Project = ((SandcastleBuilderProjectNode)this.ProjectMgr).SandcastleProject };

                projProp = this.ProjectMgr.BuildProject.GetProperty("NamespaceSummaries");
#else
                var namespaceSummaries = new NamespaceSummaryItemCollection() { Project = this.CurrentProject };

                projProp = this.CurrentProject.MSBuildProject.GetProperty("NamespaceSummaries");
#endif
                if(projProp != null && !String.IsNullOrEmpty(projProp.UnevaluatedValue))
                    namespaceSummaries.FromXml(projProp.UnevaluatedValue);

                ucSummaryPropertiesPageContent.NamespaceSummaries = namespaceSummaries;
                ucSummaryPropertiesPageContent.HasChanges = false;
                ucSummaryPropertiesPageContent.UpdateNamespaceSummaryInfo();
                return true;
            }

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
            if(propertyName == "NamespaceSummaries")
            {
                if(ucSummaryPropertiesPageContent.HasChanges)
                {
#if !STANDALONEGUI
                    this.ProjectMgr.SetProjectProperty("NamespaceSummaries",
                        ucSummaryPropertiesPageContent.NamespaceSummaries.ToXml());
#else
                    this.CurrentProject.MSBuildProject.SetProperty("NamespaceSummaries",
                        ucSummaryPropertiesPageContent.NamespaceSummaries.ToXml());
#endif
                    ucSummaryPropertiesPageContent.HasChanges = false;
                }

                return true;
            }

            return false;
        }
        #endregion
    }
}
