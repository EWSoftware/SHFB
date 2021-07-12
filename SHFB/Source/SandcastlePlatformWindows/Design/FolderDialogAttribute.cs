//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : FolderDialogAttribute.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/16/2021
// Note    : Copyright 2006-2021, Eric Woodruff, All rights reserved
//
// This file contains an attribute class that is used to associate folder dialog parameters with a class
// property for use in editing it in a property grid.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/02/2006  EFW  Created the code
// 05/16/2021  EFW  Moved the code to the Windows platform assembly from SandcastleBuilder.Utils
//===============================================================================================================

using System;

namespace Sandcastle.Platform.Windows.Design
{
    /// <summary>
    /// This attribute is used to associate folder dialog parameters with a class property for use in editing it
    /// in a property grid.
    /// </summary>
    /// <seealso cref="FolderPathStringEditor" />
    /// <seealso cref="FolderPathObjectEditor" />
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FolderDialogAttribute : Attribute
    {
        #region Properties
        //=====================================================================
        // Properties

        /// <summary>
        /// This is used to get the folder dialog description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// This is used to get whether or not to show the New Folder button
        /// </summary>
        public bool ShowNewFolderButton { get; }

        /// <summary>
        /// This is used to get the root folder used to limit browsing
        /// </summary>
        public Environment.SpecialFolder RootFolder { get; }

        /// <summary>
        /// This is used to get the default folder from which to start browsing
        /// </summary>
        public Environment.SpecialFolder DefaultFolder { get; }

        #endregion

        //=====================================================================
        // Methods, etc.

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="description">The description for the folder dialog</param>
        /// <param name="showNewFolderButton">True to show the New Folder button or false to hide it.</param>
        /// <param name="rootFolder">The root folder used to limit browsing.</param>
        /// <param name="defaultFolder">The default folder from which to start browsing.</param>
        /// <overloads>There are three overloads for the constructor.</overloads>
        public FolderDialogAttribute(string description, bool showNewFolderButton,
          Environment.SpecialFolder rootFolder, Environment.SpecialFolder defaultFolder)
        {
            this.Description = description;
            this.ShowNewFolderButton = showNewFolderButton;
            this.RootFolder = rootFolder;
            this.DefaultFolder = defaultFolder;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="description">The description for the folder dialog</param>
        /// <remarks>The New Folder button is hidden and browsing starts at the desktop</remarks>
        public FolderDialogAttribute(string description)
        {
            this.Description = description;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="description">The description for the folder dialog</param>
        /// <param name="defaultFolder">The default folder from which to start browsing.</param>
        /// <remarks>The New Folder button is hidden and browsing starts at the desktop.</remarks>
        public FolderDialogAttribute(string description, Environment.SpecialFolder defaultFolder)
        {
            this.Description = description;
            this.DefaultFolder = defaultFolder;
        }
    }
}
