//===============================================================================================================
// System  : Sandcastle Tools - Windows platform specific code
// File    : FileDialogAttribute.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/16/2021
// Note    : Copyright 2006-2021, Eric Woodruff, All rights reserved
//
// This file contains an attribute class that is used to associate file dialog parameters with a class property
// for use in editing it in a property grid.
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
    /// This attribute is used to associate file dialog parameters with a class property for use in editing it in
    /// a property grid.
    /// </summary>
    /// <seealso cref="FilePathStringEditor" />
    /// <seealso cref="FilePathObjectEditor" />
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FileDialogAttribute : Attribute
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get the file dialog title
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// This is used to get the file dialog filter
        /// </summary>
        public string Filter { get; }

        /// <summary>
        /// This is used to get the file dialog type
        /// </summary>
        public FileDialogType DialogType { get; }

        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title">The title for the file dialog</param>
        /// <param name="filter">The filter to use for the file dialog</param>
        /// <param name="dialogType">The type of file dialog to display</param>
        /// <overloads>There are two overloads for the constructor.</overloads>
        public FileDialogAttribute(string title, string filter, FileDialogType dialogType)
        {
            this.Title = title;
            this.Filter = filter;
            this.DialogType = dialogType;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title">The title for the file dialog</param>
        /// <remarks>The filter defaults to "All Files (*.*)|*.*" and the dialog type defaults to
        /// <strong>FileOpen</strong>.</remarks>
        public FileDialogAttribute(string title)
        {
            this.Title = title;
            this.Filter = "All Files (*.*)|*.*";
        }
        #endregion
    }
}
