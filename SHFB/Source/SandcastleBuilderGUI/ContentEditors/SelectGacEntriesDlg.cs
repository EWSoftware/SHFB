//=============================================================================
// System  : Sandcastle Help File Builder
// File    : SelectGacEntriesDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/28/2008
// Note    : Copyright 2006-2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to select GAC entries that should be
// project references.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.2.0.0  09/02/2006  EFW  Created the code
// 1.8.0.0  07/28/2008  EFW  Moved the form into the Sandcastle Builder GUI
//=============================================================================

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Forms;

using SandcastleBuilder.Utils.Gac;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This form is used to let the user select the GAC entries to add as
    /// project references.
    /// </summary>
    public partial class SelectGacEntriesDlg : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SelectGacEntriesDlg()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This is used to return the list of selected references
        /// </summary>
        public Collection<string> SelectedEntries
        {
            get
            {
                ListBox.SelectedObjectCollection selectedItems =
                    lbGACEntries.SelectedItems;

                Collection<string> items = new Collection<string>();

                for(int idx = 0; idx < selectedItems.Count; idx++)
                    items.Add((string)selectedItems[idx]);

                return items;
            }
        }

        /// <summary>
        /// Load the GAC list when activated
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void SelectGacEntriesDlg_Activated(object sender, EventArgs e)
        {
            try
            {
                Application.DoEvents();
                Cursor.Current = Cursors.WaitCursor;
                lbGACEntries.DataSource = GacEnumerator.GacList;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
    }
}
