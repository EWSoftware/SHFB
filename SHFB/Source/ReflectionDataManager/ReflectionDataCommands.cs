﻿//===============================================================================================================
// System  : Sandcastle Reflection Data Manager
// File    : ReflectionDataCommands.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/05/2025
// Note    : Copyright 2015-2025, Eric Woodruff, All rights reserved
//
// This file contains the reflection data manager's routed UI commands
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/29/2015  EFW  Created the code
//===============================================================================================================

using System.Windows.Input;

namespace ReflectionDataManager
{
    /// <summary>
    /// This class contains the reflection data manager's routed UI commands
    /// </summary>
    public static class ReflectionDataCommands
    {
        #region Build command
        //=====================================================================

        /// <summary>
        /// Build the reflection data
        /// </summary>
        /// <remarks>The default key binding is Ctrl+Shift+B</remarks>
        public static RoutedUICommand Build =>
            field ??= new RoutedUICommand("Build Reflection Data", "Build", typeof(ReflectionDataCommands),
                    new InputGestureCollection(new[] { new KeyGesture(Key.B,
                        ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+B") }));

        #endregion
    }
}
