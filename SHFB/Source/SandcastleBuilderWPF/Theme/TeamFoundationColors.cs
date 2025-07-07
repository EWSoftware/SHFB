//===============================================================================================================
// System  : Visual Studio Spell Checker Package
// File    : TeamFoundationColors.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/06/2025
// Note    : Copyright 2015-2025, Eric Woodruff, All rights reserved
//
// This file contains a class that returns theme colors for button elements that are found in the Team
// Foundation theme color category.
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

using Microsoft.VisualStudio.Shell;

namespace SandcastleBuilder.WPF.Theme
{
    /// <summary>
    /// This class is used to obtain theme colors from the Team Foundation category
    /// </summary>
    /// <remarks>This category of colors isn't publicly exposed like the other environment colors so we have to
    /// go get them ourselves.  We are specifically interested in the button element colors which do not appear
    /// in any of the other color types.</remarks>
    internal static class TeamFoundationColors
    {
        #region Private data members
        //=====================================================================

        // This is the category GUID used to obtain the Team Foundation theme colors
        private static readonly Guid tfsCategory = new("4aff231b-f28a-44f0-a66b-1beeb17cb920");

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// Button color key
        /// </summary>
        public static ThemeResourceKey ButtonColorKey { get; } =
            new ThemeResourceKey(tfsCategory, "Button", ThemeResourceKeyType.BackgroundColor);

        /// <summary>
        /// Button text color key
        /// </summary>
        public static ThemeResourceKey ButtonTextColorKey { get; } =
            new ThemeResourceKey(tfsCategory, "Button", ThemeResourceKeyType.ForegroundColor);

        /// <summary>
        /// Button border color key
        /// </summary>
        public static ThemeResourceKey ButtonBorderColorKey { get; } =
            new ThemeResourceKey(tfsCategory, "ButtonBorder", ThemeResourceKeyType.BackgroundColor);

        /// <summary>
        /// Button disabled color key
        /// </summary>
        public static ThemeResourceKey ButtonDisabledColorKey { get; } =
            new ThemeResourceKey(tfsCategory, "ButtonDisabled", ThemeResourceKeyType.BackgroundColor);

        /// <summary>
        /// Button disabled text color key
        /// </summary>
        public static ThemeResourceKey ButtonDisabledTextColorKey { get; } =
            new ThemeResourceKey(tfsCategory, "ButtonDisabled", ThemeResourceKeyType.ForegroundColor);

        /// <summary>
        /// Button disabled border color key
        /// </summary>
        public static ThemeResourceKey ButtonDisabledBorderColorKey { get; } =
            new ThemeResourceKey(tfsCategory, "ButtonDisabledBorder", ThemeResourceKeyType.BackgroundColor);

        /// <summary>
        /// Button mouse over color key
        /// </summary>
        public static ThemeResourceKey ButtonMouseOverColorKey { get; } =
            new ThemeResourceKey(tfsCategory, "ButtonMouseOver", ThemeResourceKeyType.BackgroundColor);

        /// <summary>
        /// Button mouse over text color key
        /// </summary>
        public static ThemeResourceKey ButtonMouseOverTextColorKey { get; } =
            new ThemeResourceKey(tfsCategory, "ButtonMouseOver", ThemeResourceKeyType.ForegroundColor);

        /// <summary>
        /// Button mouse over border color key
        /// </summary>
        public static ThemeResourceKey ButtonMouseOverBorderColorKey { get; } =
            new ThemeResourceKey(tfsCategory, "ButtonMouseOverBorder", ThemeResourceKeyType.BackgroundColor);

        /// <summary>
        /// Button pressed color key
        /// </summary>
        public static ThemeResourceKey ButtonPressedColorKey { get; } =
            new ThemeResourceKey(tfsCategory, "ButtonPressed", ThemeResourceKeyType.BackgroundColor);

        /// <summary>
        /// Button pressed text color key
        /// </summary>
        public static ThemeResourceKey ButtonPressedTextColorKey { get; } =
            new ThemeResourceKey(tfsCategory, "ButtonPressed", ThemeResourceKeyType.ForegroundColor);

        /// <summary>
        /// Button pressed border color key
        /// </summary>
        public static ThemeResourceKey ButtonPressedBorderColorKey { get; } =
            new ThemeResourceKey(tfsCategory, "ButtonPressedBorder", ThemeResourceKeyType.BackgroundColor);

        #endregion
    }
}
