//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : RefreshableItemListBox.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/25/2008
// Note    : Copyright 2006-2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a simple derived list box in which you can refresh a
// specified item to show updates to the text displayed in the list box for
// the item.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  08/09/2006  EFW  Created the code
//=============================================================================

using System;
using System.Windows.Forms;

namespace SandcastleBuilder.Utils.Controls
{
    /// <summary>
    /// This is a a simple derived list box in which you can refresh a
    /// specified item to show updates to the text displayed in the list box
    /// for the item.
    /// </summary>
    public class RefreshableItemListBox : System.Windows.Forms.ListBox
    {
        /// <summary>
        /// Refresh the specified item in the list box
        /// </summary>
        /// <param name="index">The index of the item to refresh</param>
        public void Refresh(int index)
        {
 	        base.RefreshItem(index);
        }
    }
}
