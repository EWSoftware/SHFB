//===============================================================================================================
// System  : Visual Studio Spell Checker Package
// File    : ThemeColors.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/17/2021
// Note    : Copyright 2015-2021, Eric Woodruff, All rights reserved
//
// This file contains a class used to provide Visual Studio theme colors in a version independent manner
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

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

#if !STANDALONEGUI
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
#endif

namespace SandcastleBuilder.WPF.Theme
{
    /// <summary>
    /// This class is used to provide Visual Studio Theme colors in a version independent manner
    /// </summary>
#if !STANDALONEGUI
    public class ThemeColors : INotifyPropertyChanged
#else
    public class ThemeColors
#endif
    {
        #region Private data members
        //=====================================================================

        private static ThemeColors instance;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the theme color instance
        /// </summary>
        public static ThemeColors Instance => instance ?? (instance = new ThemeColors());

#pragma warning disable CA1822

        /// <summary>
        /// Button background color
        /// </summary>
        public Color ButtonBackgroundColor => GetColor(ThemeColorId.ButtonBackgroundColor);

        /// <summary>
        /// Button border color
        /// </summary>
        public Color ButtonBorderColor => GetColor(ThemeColorId.ButtonBorderColor);

        /// <summary>
        /// Button disabled background color
        /// </summary>
        public Color ButtonDisabledBackgroundColor => GetColor(ThemeColorId.ButtonDisabledBackgroundColor);

        /// <summary>
        /// Button disabled border color
        /// </summary>
        public Color ButtonDisabledBorderColor => GetColor(ThemeColorId.ButtonDisabledBorderColor);

        /// <summary>
        /// Button disabled foreground color
        /// </summary>
        public Color ButtonDisabledForegroundColor => GetColor(ThemeColorId.ButtonDisabledForegroundColor);

        /// <summary>
        /// Button foreground color
        /// </summary>
        public Color ButtonForegroundColor => GetColor(ThemeColorId.ButtonForegroundColor);

        /// <summary>
        /// Button hover background color
        /// </summary>
        public Color ButtonHoverBackgroundColor => GetColor(ThemeColorId.ButtonHoverBackgroundColor);

        /// <summary>
        /// Button hover border color
        /// </summary>
        public Color ButtonHoverBorderColor => GetColor(ThemeColorId.ButtonHoverBorderColor);

        /// <summary>
        /// Button hover foreground color
        /// </summary>
        public Color ButtonHoverForegroundColor => GetColor(ThemeColorId.ButtonHoverForegroundColor);

        /// <summary>
        /// Button pressed background color
        /// </summary>
        public Color ButtonPressedBackgroundColor => GetColor(ThemeColorId.ButtonPressedBackgroundColor);

        /// <summary>
        /// Button pressed border color
        /// </summary>
        public Color ButtonPressedBorderColor => GetColor(ThemeColorId.ButtonPressedBorderColor);

        /// <summary>
        /// Button pressed foreground color
        /// </summary>
        public Color ButtonPressedForegroundColor => GetColor(ThemeColorId.ButtonPressedForegroundColor);

        /// <summary>
        /// Combo box button mouse over color
        /// </summary>
        public Color ComboBoxButtonMouseOverBackgroundColor => GetColor(ThemeColorId.ComboBoxButtonMouseOverBackgroundColor);

        /// <summary>
        /// Combo box disabled glyph color
        /// </summary>
        public Color ComboBoxDisabledGlyphColor => GetColor(ThemeColorId.ComboBoxDisabledGlyphColor);

        /// <summary>
        /// Combo box glyph color
        /// </summary>
        public Color ComboBoxGlyphColor => GetColor(ThemeColorId.ComboBoxGlyphColor);

        /// <summary>
        /// Combo box pop-up background color
        /// </summary>
        public Color ComboBoxPopupBackground => GetColor(ThemeColorId.ComboBoxPopupBackground);

        /// <summary>
        /// Disabled text color
        /// </summary>
        public Color DisabledTextColor => GetColor(ThemeColorId.DisabledTextColor);

        /// <summary>
        /// Item border color
        /// </summary>
        public Color ItemBorderColor => GetColor(ThemeColorId.ItemBorderColor);

        /// <summary>
        /// Item color
        /// </summary>
        public Color ItemColor => GetColor(ThemeColorId.ItemColor);

        /// <summary>
        /// Item hover color
        /// </summary>
        public Color ItemHoverColor => GetColor(ThemeColorId.ItemHoverColor);

        /// <summary>
        /// Item hover border color
        /// </summary>
        public Color ItemHoverBorderColor => GetColor(ThemeColorId.ItemHoverBorderColor);

        /// <summary>
        /// Item hover text color
        /// </summary>
        public Color ItemHoverTextColor => GetColor(ThemeColorId.ItemHoverTextColor);

        /// <summary>
        /// Item selected border color
        /// </summary>
        public Color ItemSelectedBorderColor => GetColor(ThemeColorId.ItemSelectedBorderColor);

        /// <summary>
        /// Item selected border not focused color
        /// </summary>
        public Color ItemSelectedBorderNotFocusedColor => GetColor(ThemeColorId.ItemSelectedBorderNotFocusedColor);

        /// <summary>
        /// Item selected color
        /// </summary>
        public Color ItemSelectedColor => GetColor(ThemeColorId.ItemSelectedColor);

        /// <summary>
        /// Item selected not focused color
        /// </summary>
        public Color ItemSelectedNotFocusedColor => GetColor(ThemeColorId.ItemSelectedNotFocusedColor);

        /// <summary>
        /// Item selected text color
        /// </summary>
        public Color ItemSelectedTextColor => GetColor(ThemeColorId.ItemSelectedTextColor);

        /// <summary>
        /// Item selected text not focused color
        /// </summary>
        public Color ItemSelectedTextNotFocusedColor => GetColor(ThemeColorId.ItemSelectedTextNotFocusedColor);

        /// <summary>
        /// Item text color
        /// </summary>
        public Color ItemTextColor => GetColor(ThemeColorId.ItemTextColor);

        /// <summary>
        /// Light border color
        /// </summary>
        public Color LightBorderColor => GetColor(ThemeColorId.LightBorderColor);

        /// <summary>
        /// Link text color
        /// </summary>
        public Color LinkTextColor => GetColor(ThemeColorId.LinkTextColor);

        /// <summary>
        /// Link text hover color
        /// </summary>
        public Color LinkTextHoverColor => GetColor(ThemeColorId.LinkTextHoverColor);

        /// <summary>
        /// Menu background color
        /// </summary>
        public Color MenuBackgroundColor => GetColor(ThemeColorId.MenuBackgroundColor);

        /// <summary>
        /// Menu border color
        /// </summary>
        public Color MenuBorderColor => GetColor(ThemeColorId.MenuBorderColor);

        /// <summary>
        /// Menu hover background color
        /// </summary>
        public Color MenuHoverBackgroundColor => GetColor(ThemeColorId.MenuHoverBackgroundColor);

        /// <summary>
        /// Menu hover text color
        /// </summary>
        public Color MenuHoverTextColor => GetColor(ThemeColorId.MenuHoverTextColor);

        /// <summary>
        /// Menu separator color
        /// </summary>
        public Color MenuSeparatorColor => GetColor(ThemeColorId.MenuSeparatorColor);

        /// <summary>
        /// Menu text color
        /// </summary>
        public Color MenuTextColor => GetColor(ThemeColorId.MenuTextColor);

        /// <summary>
        /// Notification color
        /// </summary>
        public Color NotificationColor => GetColor(ThemeColorId.NotificationColor);

        /// <summary>
        /// Notification text color
        /// </summary>
        public Color NotificationTextColor => GetColor(ThemeColorId.NotificationTextColor);

        /// <summary>
        /// Text box border color
        /// </summary>
        public Color TextBoxBorderColor => GetColor(ThemeColorId.TextBoxBorderColor);

        /// <summary>
        /// Text box color
        /// </summary>
        public Color TextBoxColor => GetColor(ThemeColorId.TextBoxColor);

        /// <summary>
        /// Text box text color
        /// </summary>
        public Color TextBoxTextColor => GetColor(ThemeColorId.TextBoxTextColor);

        /// <summary>
        /// Tool window background color
        /// </summary>
        public Color ToolWindowBackgroundColor => GetColor(ThemeColorId.ToolWindowBackgroundColor);

        /// <summary>
        /// Tool window border color
        /// </summary>
        public Color ToolWindowBorderColor => GetColor(ThemeColorId.ToolWindowBorderColor);

        /// <summary>
        /// Tool window text color
        /// </summary>
        public Color ToolWindowTextColor => GetColor(ThemeColorId.ToolWindowTextColor);

        /// <summary>
        /// Tree view glyph color
        /// </summary>
        public Color TreeViewGlyphColor => GetColor(ThemeColorId.TreeViewGlyphColor);

        /// <summary>
        /// Tree view mouse over glyph color
        /// </summary>
        public Color TreeViewHoverGlyphColor => GetColor(ThemeColorId.TreeViewHoverGlyphColor);

#pragma warning restore CA1822

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Private constructor
        /// </summary>
        private ThemeColors()
        {
#if !STANDALONEGUI
            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
#endif
        }
        #endregion

#if !STANDALONEGUI
        #region INotifyPropertyChanged implementation
        //=====================================================================

        /// <summary>
        /// The property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This raises the <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="propertyName">The property name that changed</param>
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// This event handler is called when the Visual Studio theme changes and raise the property change
        /// notification so that the colors are updated in any controls that use them.
        /// </summary>
        /// <param name="e">The event arguments</param>
        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            this.OnPropertyChanged(null);
        }
        #endregion
#endif

        #region Helper Methods
        //=====================================================================

        /// <summary>
        /// This is used to get a Visual Studio theme color for the given theme color ID
        /// </summary>
        /// <param name="id">The theme color ID for which to get the Visual Studio theme color</param>
        /// <returns>The theme color to use</returns>
        /// <remarks>Theme colors do not appear to be available at design time.  As such, this will return
        /// related default system colors in their place.</remarks>
        private static Color GetColor(ThemeColorId id)
        {
#if !STANDALONEGUI
            Color? vsColor = GetVisualStudioColor(id);

            if(vsColor != null)
                return vsColor.Value;
#endif
            // Get colors for design time or if something failed or wasn't defined
            return GetDefaultColor(id);
        }

#if !STANDALONEGUI
        /// <summary>
        /// This is used to return a Visual Studio theme color for the given theme color ID
        /// </summary>
        /// <param name="id">The theme color ID for which to get the Visual Studio theme color</param>
        /// <returns>The theme color to use or null if it could not be obtained was not recognized</returns>
        private static Color? GetVisualStudioColor(ThemeColorId id)
        {
            switch(id)
            {
                case ThemeColorId.ButtonBackgroundColor:
                    return GetThemeColor(TeamFoundationColors.ButtonColorKey);

                case ThemeColorId.ButtonBorderColor:
                    return GetThemeColor(TeamFoundationColors.ButtonBorderColorKey);

                case ThemeColorId.ButtonDisabledBackgroundColor:
                    return GetThemeColor(TeamFoundationColors.ButtonDisabledColorKey);

                case ThemeColorId.ButtonDisabledBorderColor:
                    return GetThemeColor(TeamFoundationColors.ButtonDisabledBorderColorKey);

                case ThemeColorId.ButtonDisabledForegroundColor:
                    return GetThemeColor(TeamFoundationColors.ButtonDisabledTextColorKey);

                case ThemeColorId.ButtonForegroundColor:
                    return GetThemeColor(TeamFoundationColors.ButtonTextColorKey);

                case ThemeColorId.ButtonHoverBackgroundColor:
                    return GetThemeColor(TeamFoundationColors.ButtonMouseOverColorKey);

                case ThemeColorId.ButtonHoverBorderColor:
                    return GetThemeColor(TeamFoundationColors.ButtonMouseOverBorderColorKey);

                case ThemeColorId.ButtonHoverForegroundColor:
                    return GetThemeColor(TeamFoundationColors.ButtonMouseOverTextColorKey);

                case ThemeColorId.ButtonPressedBackgroundColor:
                    return GetThemeColor(TeamFoundationColors.ButtonPressedColorKey);

                case ThemeColorId.ButtonPressedBorderColor:
                    return GetThemeColor(TeamFoundationColors.ButtonPressedBorderColorKey);

                case ThemeColorId.ButtonPressedForegroundColor:
                    return GetThemeColor(TeamFoundationColors.ButtonPressedTextColorKey);

                case ThemeColorId.ComboBoxButtonMouseOverBackgroundColor:
                    return GetThemeColor(EnvironmentColors.ComboBoxButtonMouseOverBackgroundColorKey);

                case ThemeColorId.ComboBoxDisabledGlyphColor:
                    return GetThemeColor(EnvironmentColors.ComboBoxDisabledGlyphColorKey);

                case ThemeColorId.ComboBoxGlyphColor:
                    return GetThemeColor(EnvironmentColors.ComboBoxGlyphColorKey);

                case ThemeColorId.ComboBoxPopupBackground:
                    return GetThemeColor(EnvironmentColors.ComboBoxPopupBackgroundBeginColorKey);

                case ThemeColorId.DisabledTextColor:
                case ThemeColorId.LightBorderColor:
                    return GetThemeColor(EnvironmentColors.SystemGrayTextColorKey);

                case ThemeColorId.ItemBorderColor:
                case ThemeColorId.ItemColor:
                    return GetThemeColor(TreeViewColors.BackgroundColorKey);

                case ThemeColorId.ItemHoverColor:
                case ThemeColorId.ItemHoverBorderColor:
                    return GetThemeColor(EnvironmentColors.CommandBarMouseOverBackgroundMiddle1ColorKey);

                case ThemeColorId.ItemHoverTextColor:
                case ThemeColorId.MenuHoverTextColor:
                    return GetThemeColor(EnvironmentColors.CommandBarTextHoverColorKey);

                case ThemeColorId.ItemSelectedBorderColor:
                case ThemeColorId.ItemSelectedColor:
                    return GetThemeColor(TreeViewColors.SelectedItemActiveColorKey);

                case ThemeColorId.ItemSelectedBorderNotFocusedColor:
                case ThemeColorId.ItemSelectedNotFocusedColor:
                    return GetThemeColor(TreeViewColors.SelectedItemInactiveColorKey);

                case ThemeColorId.ItemSelectedTextColor:
                    return GetThemeColor(TreeViewColors.SelectedItemActiveTextColorKey);

                case ThemeColorId.ItemSelectedTextNotFocusedColor:
                    return GetThemeColor(TreeViewColors.SelectedItemInactiveTextColorKey);

                case ThemeColorId.ItemTextColor:
                    return GetThemeColor(TreeViewColors.BackgroundTextColorKey);

                case ThemeColorId.LinkTextColor:
                    return GetThemeColor(EnvironmentColors.ControlLinkTextColorKey);

                case ThemeColorId.LinkTextHoverColor:
                    return GetThemeColor(EnvironmentColors.ControlLinkTextHoverColorKey);

                case ThemeColorId.MenuBackgroundColor:
                    return GetThemeColor(EnvironmentColors.CommandBarMenuBackgroundGradientBeginColorKey);

                case ThemeColorId.MenuBorderColor:
                    return GetThemeColor(EnvironmentColors.CommandBarMenuBorderColorKey);

                case ThemeColorId.MenuHoverBackgroundColor:
                    return GetThemeColor(EnvironmentColors.CommandBarMouseOverBackgroundBeginColorKey);

                case ThemeColorId.MenuSeparatorColor:
                    return GetThemeColor(EnvironmentColors.CommandBarMenuSeparatorColorKey);

                case ThemeColorId.MenuTextColor:
                    return GetThemeColor(EnvironmentColors.CommandBarTextActiveColorKey);

                case ThemeColorId.NotificationColor:
                    return GetThemeColor(EnvironmentColors.SystemInfoBackgroundColorKey);

                case ThemeColorId.NotificationTextColor:
                    return GetThemeColor(EnvironmentColors.SystemInfoTextColorKey);

                case ThemeColorId.TextBoxBorderColor:
                    return GetThemeColor(EnvironmentColors.ComboBoxBorderColorKey);

                case ThemeColorId.TextBoxColor:
                    return GetThemeColor(EnvironmentColors.ComboBoxBackgroundColorKey);

                case ThemeColorId.TextBoxTextColor:
                    return GetThemeColor(EnvironmentColors.ComboBoxItemTextColorKey);

                case ThemeColorId.ToolWindowBackgroundColor:
                    return GetThemeColor(EnvironmentColors.ToolWindowBackgroundColorKey);

                case ThemeColorId.ToolWindowBorderColor:
                    return GetThemeColor(EnvironmentColors.ToolWindowBorderColorKey);

                case ThemeColorId.ToolWindowTextColor:
                    return GetThemeColor(EnvironmentColors.ToolWindowTextColorKey);

                case ThemeColorId.TreeViewGlyphColor:
                    return GetThemeColor(TreeViewColors.GlyphColorKey);

                case ThemeColorId.TreeViewHoverGlyphColor:
                    return GetThemeColor(TreeViewColors.GlyphMouseOverColorKey);

                default:
                    return null;
            }
        }
#endif
        /// <summary>
        /// This is used to return a default system color for the given theme color ID
        /// </summary>
        /// <param name="id">The theme color ID for which to get the default system color</param>
        /// <returns>The default system color to use</returns>
        private static Color GetDefaultColor(ThemeColorId id)
        {
            switch(id)
            {
                case ThemeColorId.ButtonBackgroundColor:
                    return SystemColors.ControlLightColor;

                case ThemeColorId.ButtonBorderColor:
                case ThemeColorId.ButtonDisabledBorderColor:
                case ThemeColorId.ComboBoxButtonMouseOverBackgroundColor:
                case ThemeColorId.LightBorderColor:
                case ThemeColorId.MenuBorderColor:
                case ThemeColorId.MenuSeparatorColor:
                case ThemeColorId.ToolWindowBorderColor:
                case ThemeColorId.TreeViewHoverGlyphColor:
                    return SystemColors.ControlDarkColor;

                case ThemeColorId.ButtonDisabledBackgroundColor:
                case ThemeColorId.ButtonHoverBackgroundColor:
                case ThemeColorId.ButtonPressedBackgroundColor:
                case ThemeColorId.MenuBackgroundColor:
                case ThemeColorId.ToolWindowBackgroundColor:
                    return SystemColors.ControlColor;

                case ThemeColorId.ButtonDisabledForegroundColor:
                case ThemeColorId.ButtonForegroundColor:
                case ThemeColorId.ButtonHoverForegroundColor:
                case ThemeColorId.ButtonPressedForegroundColor:
                case ThemeColorId.ComboBoxGlyphColor:
                case ThemeColorId.MenuTextColor:
                case ThemeColorId.TreeViewGlyphColor:
                    return SystemColors.ControlTextColor;

                case ThemeColorId.ButtonHoverBorderColor:
                case ThemeColorId.ButtonPressedBorderColor:
                case ThemeColorId.LinkTextColor:
                case ThemeColorId.LinkTextHoverColor:
                    return SystemColors.HotTrackColor;

                case ThemeColorId.ComboBoxDisabledGlyphColor:
                case ThemeColorId.DisabledTextColor:
                    return SystemColors.GrayTextColor;

                case ThemeColorId.ComboBoxPopupBackground:
                case ThemeColorId.ItemBorderColor:
                case ThemeColorId.ItemColor:
                case ThemeColorId.NotificationColor:
                case ThemeColorId.TextBoxColor:
                    return SystemColors.WindowColor;

                case ThemeColorId.ItemHoverColor:
                case ThemeColorId.ItemHoverBorderColor:
                case ThemeColorId.ItemSelectedNotFocusedColor:
                    return SystemColors.ControlLightColor;

                case ThemeColorId.ItemHoverTextColor:
                case ThemeColorId.ItemSelectedTextNotFocusedColor:
                case ThemeColorId.ItemTextColor:
                case ThemeColorId.NotificationTextColor:
                case ThemeColorId.TextBoxTextColor:
                case ThemeColorId.ToolWindowTextColor:
                    return SystemColors.WindowTextColor;

                case ThemeColorId.ItemSelectedBorderColor:
                case ThemeColorId.ItemSelectedBorderNotFocusedColor:
                case ThemeColorId.ItemSelectedColor:
                case ThemeColorId.MenuHoverBackgroundColor:
                    return SystemColors.HighlightColor;

                case ThemeColorId.ItemSelectedTextColor:
                case ThemeColorId.MenuHoverTextColor:
                    return SystemColors.HighlightTextColor;

                case ThemeColorId.TextBoxBorderColor:
                    return SystemColors.WindowFrameColor;

                default:
                    return SystemColors.ControlTextColor;
            }
        }

#if !STANDALONEGUI
        /// <summary>
        /// This is used to get the theme color for the given them resource key
        /// </summary>
        /// <param name="themeResourceKey">The theme resource key for which to get the color</param>
        /// <returns>The color for the theme resource key or null if it could not be obtained</returns>
        private static Color? GetThemeColor(ThemeResourceKey themeResourceKey)
        {
            try
            {
                System.Drawing.Color vsThemeColor = VSColorTheme.GetThemedColor(themeResourceKey);
                return Color.FromArgb(vsThemeColor.A, vsThemeColor.R, vsThemeColor.G, vsThemeColor.B);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to get Visual Studio theme color {0}.  Exception:\r\n{1}",
                    themeResourceKey.Name, ex);
            }

            return null;
        }
#endif
#endregion
    }
}
