//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : ICommonCommandSupport.cs
// Author  : Istvan Novak
// Updated : 12/26/2011
// Source  : http://learnvsxnow.codeplex.com/
// Note    : Copyright 2008-2011, Istvan Novak, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that defines an interface that allows editor pane
// implementations or their hosted controls to declare and execute common
// commands that they may support.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.3  12/26/2011  EFW  Added the code to the project
//=============================================================================

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
