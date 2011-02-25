//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : FolderDialogAttribute.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/02/2006
// Note    : Copyright 2006, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains an attribute class that is used to associate folder
// dialog parameters with a class property for use in editing it in a property
// grid.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  08/02/2006  EFW  Created the code
//=============================================================================

using System;
using System.Windows.Forms;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This attribute is used to associate folder dialog parameters with a
    /// class property for use in editing it in a property grid.
    /// </summary>
    /// <seealso cref="FolderPathStringEditor" />
    /// <seealso cref="FolderPathObjectEditor" />
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FolderDialogAttribute : System.Attribute
    {
        #region Private data members
        //=====================================================================
        // Private data members

        private string dlgDesc;
        private bool showNewFolderBtn;
        private Environment.SpecialFolder root, defFolder;
        #endregion

        #region Properties
        //=====================================================================
        // Properties

        /// <summary>
        /// This is used to get the folder dialog description
        /// </summary>
        public string Description
        {
            get { return dlgDesc; }
        }

        /// <summary>
        /// This is used to get whether or not to show the New Folder button
        /// </summary>
        public bool ShowNewFolderButton
        {
            get { return showNewFolderBtn; }
        }

        /// <summary>
        /// This is used to get the root folder used to limit browsing
        /// </summary>
        public Environment.SpecialFolder RootFolder
        {
            get { return root; }
        }

        /// <summary>
        /// This is used to get the default folder from which to start browsing
        /// </summary>
        public Environment.SpecialFolder DefaultFolder
        {
            get { return defFolder; }
        }
        #endregion

        //=====================================================================
        // Methods, etc.

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="description">The description for the folder dialog</param>
        /// <param name="showNewFolderButton">True to show the New Folder
        /// button or false to hide it.</param>
        /// <param name="rootFolder">The root folder used to limit browsing.</param>
        /// <param name="defaultFolder">The default folder from which to
        /// start browsing.</param>
        /// <overloads>There are three overloads for the constructor.</overloads>
        public FolderDialogAttribute(string description,
          bool showNewFolderButton, Environment.SpecialFolder rootFolder,
          Environment.SpecialFolder defaultFolder)
        {
            dlgDesc = description;
            showNewFolderBtn = showNewFolderButton;
            root = rootFolder;
            defFolder = defaultFolder;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="description">The description for the folder dialog</param>
        /// <remarks>The New Folder button is hidden and browsing starts
        /// at the desktop.</remarks>
        public FolderDialogAttribute(string description)
        {
            dlgDesc = description;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="description">The description for the folder dialog</param>
        /// <param name="defaultFolder">The default folder from which to
        /// start browsing.</param>
        /// <remarks>The New Folder button is hidden and browsing starts
        /// at the desktop.</remarks>
        public FolderDialogAttribute(string description,
          Environment.SpecialFolder defaultFolder)
        {
            dlgDesc = description;
            defFolder = defaultFolder;
        }
    }
}
