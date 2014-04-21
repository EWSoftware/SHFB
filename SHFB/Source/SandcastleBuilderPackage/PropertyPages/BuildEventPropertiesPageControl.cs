//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : BuildEventPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 04/20/2014
// Note    : Copyright 2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Build Events category properties.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/19/2014  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the Build Events category project properties
    /// </summary>
    [Guid("88926D2F-6A3F-440C-929D-144888C8EFFD")]
    public partial class BuildEventPropertiesPageControl : BasePropertyPage
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public BuildEventPropertiesPageControl()
        {
            InitializeComponent();

#if STANDALONEGUI
            // The standalone GUI does not execute pre-build and post-build events so show a warning.
            lblStandaloneGUI.Visible = true;
#endif

            // Set the maximum size to prevent an unnecessary vertical scrollbar
            this.MaximumSize = new System.Drawing.Size(2048, this.Height);

            this.Title = "Build Events";
            this.HelpKeyword = "682c2e1c-54d2-4128-80ff-f6dc63d2f58d";

            cboRunPostBuildEvent.DisplayMember = "Value";
            cboRunPostBuildEvent.ValueMember = "Key";

            cboRunPostBuildEvent.DataSource = (new Dictionary<string, string> {
                { "OnBuildSuccess", "On successful build" },
                { "Always", "Always" }
            }).ToList();
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override bool IsValid
        {
            get
            {
                if(cboRunPostBuildEvent.SelectedIndex == -1)
                    cboRunPostBuildEvent.SelectedIndex = 0;

                return true;
            }
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Edit the pre-build/post-build event in the extended editor form
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnEditBuildEvent_Click(object sender, EventArgs e)
        {
            TextBox tb;
            string title;

            if(sender == btnEditPreBuildEvent)
            {
                tb = txtPreBuildEvent;
                title = "Edit Pre-Build Event Command Line";
            }
            else
            {
                tb = txtPostBuildEvent;
                title = "Edit Post-Build Event Command Line";
            }

            using(var dlg = new BuildEventEditorForm())
            {
                dlg.Text = title;
                dlg.BuildEventText = tb.Text;

#if!STANDALONEGUI
                dlg.DetermineMacroValues(base.ProjectMgr.BuildProject);
#else
                dlg.DetermineMacroValues(base.CurrentProject.MSBuildProject);
#endif

                if(dlg.ShowDialog() == DialogResult.OK)
                {
                    tb.Text = dlg.BuildEventText;
                    tb.Select(0, 0);
                    tb.ScrollToCaret();
                }
            }
        }

        /// <summary>
        /// Clear the highlight when entered so that we don't accidentally lose the text
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void txtBuildEvent_Enter(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;

            if(tb.SelectionLength != 0)
            {
                tb.Select(0, 0);
                tb.ScrollToCaret();
            }
        }
        #endregion
    }
}
