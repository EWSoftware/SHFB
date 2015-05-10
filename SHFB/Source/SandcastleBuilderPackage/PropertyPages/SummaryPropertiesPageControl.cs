//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : SummaryPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 10/28/2012
// Note    : Copyright 2011-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Summaries category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.3.0  03/27/2011  EFW  Created the code
// 1.9.6.0  10/28/2012  EFW  Updated for use in the standalone GUI
// ==============================================================================================================

using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Microsoft.Build.Evaluation;

#if !STANDALONEGUI
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

using SandcastleBuilder.Package.Nodes;
#endif
using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the Summaries category project properties
    /// </summary>
    [Guid("C2055DCA-54C2-4047-B0BD-87464BA6BA95")]
    public partial class SummaryPropertiesPageControl : BasePropertyPage
    {
        #region Private data members
        //=====================================================================

        private NamespaceSummaryItemCollection namespaceSummaries;
        private bool summariesChanged;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public SummaryPropertiesPageControl()
        {
            InitializeComponent();

            // Set the maximum size to prevent an unnecessary vertical scrollbar
            this.MaximumSize = new System.Drawing.Size(2048, this.Height);

            this.Title = "Summaries";
            this.HelpKeyword = "eb7e1bc7-21c5-4453-bbaf-dec8c62c15bd";
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to update the namespace summary information
        /// </summary>
        public void UpdateNamespaceSummaryInfo()
        {
            int excluded, withSummary;

            if(namespaceSummaries.Count == 0)
                lblNamespaceSummaryState.Text = "No summaries are defined in the project";
            else
            {
                excluded = namespaceSummaries.Count(n => !n.IsDocumented);
                withSummary = namespaceSummaries.Count(n => !String.IsNullOrEmpty(n.Summary));

                lblNamespaceSummaryState.Text = String.Format(CultureInfo.CurrentCulture,
                    "{0} with summary, {1} excluded in the project", withSummary, excluded);
            }
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
        protected override bool BindControlValue(Control control)
        {
            ProjectProperty projProp;

#if !STANDALONEGUI
            if(this.ProjectMgr == null)
                return false;
#else
            if(this.CurrentProject == null)
                return false;
#endif

            if(control.Name == "lblNamespaceSummaryState")
            {
                // Pass it the Sandcastle project instance as we use the designer dialog to edit the collection
                // and it obtains it from the collection to do the required partial build.
#if !STANDALONEGUI
                namespaceSummaries = new NamespaceSummaryItemCollection
                {
                    Project = ((SandcastleBuilderProjectNode)base.ProjectMgr).SandcastleProject
                };

                projProp = this.ProjectMgr.BuildProject.GetProperty("NamespaceSummaries");
#else
                namespaceSummaries = new NamespaceSummaryItemCollection() { Project = base.CurrentProject };

                projProp = this.CurrentProject.MSBuildProject.GetProperty("NamespaceSummaries");
#endif
                summariesChanged = false;

                if(projProp != null && !String.IsNullOrEmpty(projProp.UnevaluatedValue))
                    namespaceSummaries.FromXml(projProp.UnevaluatedValue);

                this.UpdateNamespaceSummaryInfo();
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        protected override bool StoreControlValue(Control control)
        {
#if !STANDALONEGUI
            if(this.ProjectMgr == null)
                return false;
#else
            if(this.CurrentProject == null)
                return false;
#endif
            if(control.Name == "lblNamespaceSummaryState")
            {
                if(summariesChanged)
                {
#if !STANDALONEGUI
                    this.ProjectMgr.SetProjectProperty("NamespaceSummaries", namespaceSummaries.ToXml());
#else
                    this.CurrentProject.MSBuildProject.SetProperty("NamespaceSummaries", namespaceSummaries.ToXml());
#endif
                    summariesChanged = false;
                }

                return true;
            }

            return false;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Clear the highlight when entered so that we don't accidentally lose the text
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void txtProjectSummary_Enter(object sender, EventArgs e)
        {
            txtProjectSummary.Select(0, 0);
            txtProjectSummary.ScrollToCaret();
        }

        /// <summary>
        /// Edit the project's namespace summaries
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnEditNamespaces_Click(object sender, EventArgs e)
        {
            string oldSummaries, newSummaries;

#if !STANDALONEGUI
            if(this.ProjectMgr == null)
                return;

            // Apply any pending changes first
            if(this.IsDirty && ((IPropertyPage)this).Apply() != VSConstants.S_OK)
                return;
#else
            if(this.CurrentProject == null)
                return;

            // Apply any pending changes first
            if(this.IsDirty && !this.Apply())
                return;
#endif

            using(NamespaceSummaryItemEditorDlg dlg = new NamespaceSummaryItemEditorDlg(namespaceSummaries))
            {
                oldSummaries = namespaceSummaries.ToXml();
                dlg.ShowDialog();
                newSummaries = namespaceSummaries.ToXml();

                // If it changes, mark the page as dirty and update the summary info
                if(oldSummaries != newSummaries)
                {
                    this.IsDirty = summariesChanged = true;
                    this.UpdateNamespaceSummaryInfo();
                }
            }
        }
        #endregion
    }
}
