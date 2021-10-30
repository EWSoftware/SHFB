//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : BeforeBuildAction.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/20/2021
// Note    : Copyright 2006-2021, Eric Woodruff, All rights reserved
//
// This file contains the Before Build Action enumerated type.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/24/2008  EFW  Created the code
//===============================================================================================================

namespace SandcastleBuilder.Gui
{
    /// <summary>
    /// This is used to determine what to do with unsaved files before building
    /// a project.
    /// </summary>
    public enum BeforeBuildAction
    {
        /// <summary>Save all changes to documents and the project.</summary>
        SaveAllChanges,
        /// <summary>Only save changes to open documents.</summary>
        SaveOpenDocuments,
        /// <summary>Prompt to save changes to all open documents and the
        /// project.</summary>
        PromptToSaveAll,
        /// <summary>Do not save any changes.</summary>
        DoNotSave
    }
}
