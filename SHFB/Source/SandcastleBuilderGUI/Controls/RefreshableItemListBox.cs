//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : RefreshableItemListBox.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/18/2021
// Note    : Copyright 2006-2021, Eric Woodruff, All rights reserved
//
// This file contains a simple derived list box in which you can refresh a specified item to show updates to the
// text displayed in the list box for the item.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/09/2006  EFW  Created the code
//===============================================================================================================

namespace SandcastleBuilder.Gui.Controls
{
    /// <summary>
    /// This is a a simple derived list box in which you can refresh a specified item to show updates to the
    /// text displayed in the list box for the item.
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
