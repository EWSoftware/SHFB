//=============================================================================
// System  : Sandcastle Help File Builder
// File    : GotoLineDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/12/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the dialog used to enter a line number for the Goto Line
// editor action.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  07/26/2008  EFW  Created the code
//=============================================================================

using System;
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
        public int LineNumber
        {
            get { return (int)udcLineNo.Value; }
        }

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
