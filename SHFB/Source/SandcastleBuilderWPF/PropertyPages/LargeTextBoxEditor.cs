//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : LargeTextBoxEditor.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/17/2021
// Note    : Copyright 2017-2021, Eric Woodruff, All rights reserved
//
// This file contains the class used to edit user-defined project properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/26/2017  EFW  Created the code
//===============================================================================================================

using System;
using System.Windows;
using System.Windows.Controls;

using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This is used in the property grid control to provide a larger text box for editing strings
    /// </summary>
    public class LargeTextBoxEditor : TextBoxEditor
    {
        private string currentValue;

        /// <summary>
        /// Set the control properties
        /// </summary>
        /// <param name="propertyItem">The property item</param>
        protected override void SetControlProperties(PropertyItem propertyItem)
        {
            if(propertyItem == null)
                throw new ArgumentNullException(nameof(propertyItem));

            base.SetControlProperties(propertyItem);

            this.Editor.Height = 75;
            this.Editor.MinWidth = this.Editor.MaxWidth = 350;
            this.Editor.TextWrapping = TextWrapping.Wrap;
            this.Editor.HorizontalAlignment = HorizontalAlignment.Left;
            this.Editor.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            currentValue = propertyItem.Value?.ToString();

            // Force changes to be committed when losing keyboard focus and the value changed
            this.Editor.LostKeyboardFocus += (s, e) =>
            {
                if(currentValue != this.Editor.Text)
                {
                    this.Editor.CommitChanges();

                    currentValue = this.Editor.Text;
                }
            };
        }
    }
}
