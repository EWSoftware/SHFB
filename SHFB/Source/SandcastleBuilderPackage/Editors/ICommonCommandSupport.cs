﻿//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : ICommonCommandSupport.cs
// Author  : Istvan Novak
// Updated : 05/26/2021
// Source  : http://learnvsxnow.codeplex.com/
// Note    : Copyright 2008-2021, Istvan Novak, All rights reserved
//
// This file contains a class that defines an interface that allows editor pane implementations or their hosted
// controls to declare and execute common commands that they may support.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/26/2011  EFW  Added the code to the project
//===============================================================================================================

namespace SandcastleBuilder.Package.Editors
{
    // ==================================================================================
    /// <summary>
    /// This interface allows implementor editor panes to declare and execute common
    /// commands supported by them.
    /// </summary>
    // ==================================================================================
    public interface ICommonCommandSupport
    {
        // --------------------------------------------------------------------------------
        /// <summary>
        /// Get the flag indicating if "SelectAll" command is supported or not.
        /// </summary>
        // --------------------------------------------------------------------------------
        bool SupportsSelectAll { get; }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Get the flag indicating if "Copy" command is supported or not.
        /// </summary>
        // --------------------------------------------------------------------------------
        bool SupportsCopy { get; }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Get the flag indicating if "Cut" command is supported or not.
        /// </summary>
        // --------------------------------------------------------------------------------
        bool SupportsCut { get; }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Get the flag indicating if "Paste" command is supported or not.
        /// </summary>
        // --------------------------------------------------------------------------------
        bool SupportsPaste { get; }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Get the flag indicating if "Redo" command is supported or not.
        /// </summary>
        // --------------------------------------------------------------------------------
        bool SupportsRedo { get; }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Get the flag indicating if "Undo" command is supported or not.
        /// </summary>
        // --------------------------------------------------------------------------------
        bool SupportsUndo { get; }

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Executes the "SelectAll" command.
        /// </summary>
        // --------------------------------------------------------------------------------
        void DoSelectAll();

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Executes the "Copy" command.
        /// </summary>
        // --------------------------------------------------------------------------------
        void DoCopy();

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Executes the "Cut" command.
        /// </summary>
        // --------------------------------------------------------------------------------
        void DoCut();

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Executes the "Paste" command.
        /// </summary>
        // --------------------------------------------------------------------------------
        void DoPaste();

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Executes the "Redo" command.
        /// </summary>
        // --------------------------------------------------------------------------------
        void DoRedo();

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Executes the "Undo" command.
        /// </summary>
        // --------------------------------------------------------------------------------
        void DoUndo();
    }
}
