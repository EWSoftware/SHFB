//===============================================================================================================
// System  : Visual Studio Spell Checker Package
// File    : ThemeColorId.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/20/2019
// Note    : Copyright 2015-2019, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains an enumerated type used to map theme color IDs to Visual Studio theme key elements
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB
// This notice, the author's name, and all copyright notices must remain intact in all applications,
// documentation, and source files.
//
//    Date     Who  Comments
//===============================================================================================================
// 08/17/2015  EFW  Created the code
//===============================================================================================================

namespace SandcastleBuilder.WPF.Theme
{
    /// <summary>
    /// This enumerated type is used to map theme color IDs to Visual Studio theme key elements
    /// </summary>
    internal enum ThemeColorId
    {
        /// <summary>Button background color</summary>
        ButtonBackgroundColor,
        /// <summary>Button border color</summary>
        ButtonBorderColor,
        /// <summary>Button disabled background color</summary>
        ButtonDisabledBackgroundColor,
        /// <summary>Button disabled border color</summary>
        ButtonDisabledBorderColor,
        /// <summary>Button disabled foreground color</summary>
        ButtonDisabledForegroundColor,
        /// <summary>Button foreground color</summary>
        ButtonForegroundColor,
        /// <summary>Button hover background color</summary>
        ButtonHoverBackgroundColor,
        /// <summary>Button hover border color</summary>
        ButtonHoverBorderColor,
        /// <summary>Button hover foreground color</summary>
        ButtonHoverForegroundColor,
        /// <summary>Button pressed background color</summary>
        ButtonPressedBackgroundColor,
        /// <summary>Button pressed border color</summary>
        ButtonPressedBorderColor,
        /// <summary>Button pressed foreground color</summary>
        ButtonPressedForegroundColor,

        /// <summary>Combo box mouse over background color</summary>
        ComboBoxButtonMouseOverBackgroundColor,
        /// <summary>Combo box disabled glyph color</summary>
        ComboBoxDisabledGlyphColor,
        /// <summary>Combo box glyph color</summary>
        ComboBoxGlyphColor,
        /// <summary>Combo box pop-up background color</summary>
        ComboBoxPopupBackground,

        /// <summary>Disabled text color</summary>
        DisabledTextColor,

        /// <summary>Item border color</summary>
        ItemBorderColor,
        /// <summary>Item color</summary>
        ItemColor,
        /// <summary>Item hover color</summary>
        ItemHoverColor,
        /// <summary>Item hover border color</summary>
        ItemHoverBorderColor,
        /// <summary>Item hover text color</summary>
        ItemHoverTextColor,
        /// <summary>Item selected border color</summary>
        ItemSelectedBorderColor,
        /// <summary>Item selected border not focused color</summary>
        ItemSelectedBorderNotFocusedColor,
        /// <summary>Item selected color</summary>
        ItemSelectedColor,
        /// <summary>Item selected not focused color</summary>
        ItemSelectedNotFocusedColor,
        /// <summary>Item selected text color</summary>
        ItemSelectedTextColor,
        /// <summary>Item selected text not focused color</summary>
        ItemSelectedTextNotFocusedColor,
        /// <summary>Item text color</summary>
        ItemTextColor,

        /// <summary>Link text color</summary>
        LinkTextColor,
        /// <summary>Link text hover color</summary>
        LinkTextHoverColor,

        /// <summary>Menu background color</summary>
        MenuBackgroundColor,
        /// <summary>Menu border color</summary>
        MenuBorderColor,
        /// <summary>Menu hover background color</summary>
        MenuHoverBackgroundColor,
        /// <summary>Menu hover text color</summary>
        MenuHoverTextColor,
        /// <summary>Menu separator color</summary>
        MenuSeparatorColor,
        /// <summary>Menu text color</summary>
        MenuTextColor,

        /// <summary>Light border color</summary>
        LightBorderColor,

        /// <summary>Notification color</summary>
        NotificationColor,
        /// <summary>Notification text color</summary>
        NotificationTextColor,

        /// <summary>Text box border color</summary>
        TextBoxBorderColor,
        /// <summary>Text box color</summary>
        TextBoxColor,
        /// <summary>Text box text color</summary>
        TextBoxTextColor,

        /// <summary>Tool window background color</summary>
        ToolWindowBackgroundColor,
        /// <summary>Tool window border color</summary>
        ToolWindowBorderColor,
        /// <summary>Tool window text color</summary>
        ToolWindowTextColor,

        /// <summary>Tree view glyph color</summary>
        TreeViewGlyphColor,
        /// <summary>Tree view hover glyph color</summary>
        TreeViewHoverGlyphColor
    }
}
