//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : SummaryPropertiesPageContent.xaml.cs
// Author  : Eric Woodruff
// Updated : 06/19/2025
// Note    : Copyright 2017-2025, Eric Woodruff, All rights reserved
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
// 10/10/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
// ==============================================================================================================

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Sandcastle.Platform.Windows;

using Sandcastle.Core.Project;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This user control is used to edit the Summaries category properties
    /// </summary>
    public partial class SummaryPropertiesPageContent : UserControl
    {
        #region Private data members
        //=====================================================================

        private NamespaceSummaryItemCollection namespaceSummaries;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the namespace
        /// </summary>
        public NamespaceSummaryItemCollection NamespaceSummaries
        {
            get => namespaceSummaries;
            set
            {
                namespaceSummaries = value;
                this.HasChanges = false;
            }
        }

        /// <summary>
        /// This is used to get or set whether the control has changes to the namespace summaries
        /// </summary>
        public bool HasChanges { get; set; }

        #endregion

        #region Events
        //=====================================================================

        /// <summary>
        /// This event is used to indicate that the parent should apply any pending changes to the project as an
        /// action is about to occur that depends on the updated property values.
        /// </summary>
        public event EventHandler<ApplyChangesEventArgs> ApplyChanges;

        /// <summary>
        /// This event is used to signal that the content has been modified
        /// </summary>
        public event EventHandler SummariesModified;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public SummaryPropertiesPageContent()
        {
            InitializeComponent();
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

            if(namespaceSummaries == null || namespaceSummaries.Count == 0)
                lblNamespaceSummaryState.Content = "No summaries are defined in the project";
            else
            {
                excluded = namespaceSummaries.Count(n => !n.IsDocumented);
                withSummary = namespaceSummaries.Count(n => !String.IsNullOrEmpty(n.Summary));
                lblNamespaceSummaryState.Content = $"{withSummary} with summary, {excluded} excluded in the project";
            }
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Edit the namespace summaries
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnEditNamespaceSummaries_Click(object sender, RoutedEventArgs e)
        {
            var args = new ApplyChangesEventArgs();

            this.ApplyChanges?.Invoke(this, args);

            if(args.ChangesApplied)
            {
                string oldSummaries, newSummaries;

                var dlg = new NamespaceSummaryItemEditorDlg(namespaceSummaries);

                oldSummaries = namespaceSummaries.ToXml();
                dlg.ShowModalDialog();
                newSummaries = namespaceSummaries.ToXml();

                // If it changes, mark the page as dirty and update the summary info
                if(oldSummaries != newSummaries)
                {
                    this.SummariesModified?.Invoke(this, EventArgs.Empty);
                    this.HasChanges = true;
                    this.UpdateNamespaceSummaryInfo();
                }
            }
        }
        #endregion
    }
}
