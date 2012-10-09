//=============================================================================
// System  : Sandcastle Help File Builder
// File    : BeforeBuildAction.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/24/2008
// Note    : Copyright 2006-2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the Before Build Action enumerated type.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.1  10/24/2008  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.Text;

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
