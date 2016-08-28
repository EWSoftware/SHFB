//===============================================================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : FileDialogType.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/28/2016
// Note    : Copyright 2006-2016, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
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
// 08/28/2016  EFW  Split the enum out into its own source code file so that MRefBuilder can find it
//===============================================================================================================

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// The type of file dialog to display
    /// </summary>
    public enum FileDialogType
    {
        /// <summary>Display a File Open dialog box</summary>
        FileOpen,
        /// <summary>Display a File Save dialog box</summary>
        FileSave
    }

}
