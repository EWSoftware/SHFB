//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : ProjectCommands.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/04/2025
// Note    : Copyright 2011-2025, Eric Woodruff, All rights reserved
//
// This file contains a class for the help file builder's routed UI commands for projects
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/18/2011  EFW  Created the code
//===============================================================================================================

using System.Windows.Input;

namespace SandcastleBuilder.WPF.Commands
{
    /// <summary>
    /// This class contains the help file builder's routed UI commands for projects
    /// </summary>
    public static class ProjectCommands
    {
        #region Add from template command
        //=====================================================================

        /// <summary>
        /// Add a file to the project from a template file
        /// </summary>
        /// <remarks>This command has no default key binding</remarks>
        public static RoutedUICommand AddFromTemplate { get; } = new RoutedUICommand("Add File from a Template",
            "AddFromTemplate", typeof(ProjectCommands));

        #endregion

        #region Add an existing file
        //=====================================================================

        /// <summary>
        /// Add an existing file to the project
        /// </summary>
        /// <remarks>This command has no default key binding</remarks>
        public static RoutedUICommand AddExistingFile { get; } = new RoutedUICommand("Add an Existing File",
            "AddExistingFile", typeof(ProjectCommands));

        #endregion

        #region Add all files from a folder
        //=====================================================================

        /// <summary>
        /// Add all files from a folder to the project
        /// </summary>
        /// <remarks>This command has no default key binding</remarks>
        public static RoutedUICommand AddAllFromFolder { get; } =  new RoutedUICommand("Add All Files from a Folder",
            "AddAllFromFolder", typeof(ProjectCommands));

        #endregion
    }
}
