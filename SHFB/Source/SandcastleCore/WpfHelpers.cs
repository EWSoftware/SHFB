//===============================================================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : WpfHelpers.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/20/2017
// Note    : Copyright 2017, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains helper methods and extension methods for WPF forms and controls
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/05/2017  EFW  Created the code
//===============================================================================================================

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace Sandcastle.Core
{
    /// <summary>
    /// This class contains helper methods and extension methods for WPF forms and controls
    /// </summary>
    public static class WpfHelpers
    {
        #region WPF modal dialog helper
        //=====================================================================

        /// <summary>
        /// This is used to get or set the main window handle used as the owner for WPF modal dialogs
        /// </summary>
        public static IntPtr MainWindowHandle { get; set; }

        /// <summary>
        /// Show a modal WPF window parented to the main Visual Studio window
        /// </summary>
        /// <param name="dialog">The WPF window to show modally</param>
        /// <returns>The <c>ShowDialog()</c> result from the window</returns>
        /// <remarks>By using the main window as the owner, the form appears over the main window regardless of
        /// which monitor it may be on.</remarks>
        public static bool? ShowModalDialog(this Window dialog)
        {
            if(MainWindowHandle != IntPtr.Zero)
                new WindowInteropHelper(dialog) { Owner = MainWindowHandle };

            return dialog.ShowDialog();
        }

        /// <summary>
        /// This is used to set the validation state for a control
        /// </summary>
        /// <param name="control">The control on which to set the validation state</param>
        /// <param name="isValid">True if valid, false if not</param>
        /// <param name="tooltip">The tooltip to show or null for none</param>
        /// <remarks>If not valid, the border is set to red to highlight the invalid control</remarks>
        public static void SetValidationState(this Control control, bool isValid, string tooltip)
        {
            if(isValid)
            {
                control.ClearValue(Control.BorderBrushProperty);
                control.ClearValue(Control.BorderThicknessProperty);
            }
            else
            {
                control.BorderBrush = Brushes.Red;
                control.BorderThickness = new Thickness(1.25);
            }

            control.ToolTip = tooltip;
        }
        #endregion
    }
}
