//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : ProjectCommands.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/17/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
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

        private static RoutedUICommand addFromTemplate;

        /// <summary>
        /// Add a file to the project from a template file
        /// </summary>
        /// <remarks>This command has no default key binding</remarks>
        public static RoutedUICommand AddFromTemplate
        {
            get
            {
                if(addFromTemplate == null)
                    addFromTemplate = new RoutedUICommand("Add File from a Template", "AddFromTemplate",
                        typeof(ProjectCommands));

                return addFromTemplate;
            }
        }
        #endregion

        #region Add an existing file
        //=====================================================================

        private static RoutedUICommand addExistingFile;

        /// <summary>
        /// Add an existing file to the project
        /// </summary>
        /// <remarks>This command has no default key binding</remarks>
        public static RoutedUICommand AddExistingFile
        {
            get
            {
                if(addExistingFile == null)
                    addExistingFile = new RoutedUICommand("Add an Existing File", "AddExistingFile",
                        typeof(ProjectCommands));

                return addExistingFile;
            }
        }
        #endregion

        #region Add all files from a folder
        //=====================================================================

        private static RoutedUICommand addAllFromFolder;

        /// <summary>
        /// Add all files from a folder to the project
        /// </summary>
        /// <remarks>This command has no default key binding</remarks>
        public static RoutedUICommand AddAllFromFolder
        {
            get
            {
                if(addAllFromFolder == null)
                    addAllFromFolder = new RoutedUICommand("Add All Files from a Folder", "AddAllFromFolder",
                        typeof(ProjectCommands));

                return addAllFromFolder;
            }
        }
        #endregion
    }
}
