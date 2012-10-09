//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : ProjectSummaryEditorDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/04/2009
// Note    : Copyright 2006-2009, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to edit the project summary comments.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.2.0.0  09/04/2006  EFW  Created the code
// 1.8.0.0  07/28/2008  EFW  Reworked to support MSBuild project format
//=============================================================================

using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This form is used to edit the project summary comments.
    /// </summary>
    public partial class ProjectSummaryEditorDlg : Form
    {
        //=====================================================================

        /// <summary>
        /// Get or set the project summary comments
        /// </summary>
        public string Summary
        {
            get { return txtSummary.Text; }
            set { txtSummary.Text = value; }
        }

        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public ProjectSummaryEditorDlg()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Clear the selection to prevent accidental deletion of the text
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void txtSummary_Enter(object sender, EventArgs e)
        {
            txtSummary.Select(0, 0);
            txtSummary.ScrollToCaret();
        }
    }
}
