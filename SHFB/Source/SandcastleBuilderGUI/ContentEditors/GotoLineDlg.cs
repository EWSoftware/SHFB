//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : GotoLineDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/19/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains the dialog used to enter a line number for the Goto Line editor action
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/26/2008  EFW  Created the code
//===============================================================================================================

using System.Windows.Forms;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This is used to enter the line number for the Goto Line editor action.
    /// </summary>
    public partial class GotoLineDlg : Form
    {
        /// <summary>
        /// This read-only property returns the entered line number
        /// </summary>
        public int LineNumber => (int)udcLineNo.Value;

        /// <summary>
        /// Constructor
        /// </summary>
        public GotoLineDlg()
        {
            InitializeComponent();
            udcLineNo.Text = null;
        }
    }
}
