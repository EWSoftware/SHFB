//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : MSHelpKeywordEditorDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/15/2011
// Note    : Copyright 2008-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to edit the help index keywords.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.7  03/27/2008  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This form is used to edit the help index keyword collection
    /// </summary>
    internal partial class MSHelpKeywordEditorDlg : Form
    {
        #region Private data members
        //=====================================================================

        private MSHelpKeywordCollection keywords;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="currentKeywords">The item collection to edit</param>
        public MSHelpKeywordEditorDlg(MSHelpKeywordCollection currentKeywords)
        {
            InitializeComponent();

            keywords = currentKeywords;
            dgvKeywords.AutoGenerateColumns = false;
            dgvKeywords.DataSource = keywords;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Close the form
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnClose_Click(object sender, EventArgs e)
        {
            keywords.Sort();
            this.Close();
        }

        /// <summary>
        /// Delete the selected keyword
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            int idx;

            if(dgvKeywords.SelectedRows.Count != 0)
            {
                idx = dgvKeywords.SelectedRows[0].Index;

                if(idx < keywords.Count)
                {
                    dgvKeywords.EndEdit();
                    keywords.RemoveAt(idx);
                }
            }
        }

        /// <summary>
        /// View help for this form
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnHelp_Click(object sender, EventArgs e)
        {
            Utility.ShowHelpTopic("7d28bf8f-923f-44c1-83e1-337a416947a1#HelpAttributes");
        }

        /// <summary>
        /// Mark the collection as dirty if changed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void dgvKeywords_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if(keywords != null)
                keywords.MarkAsDirty();
        }
        #endregion
    }
}
