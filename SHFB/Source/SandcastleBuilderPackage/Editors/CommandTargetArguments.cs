﻿//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : CommandTargetArguments.cs
// Author  : Istvan Novak
// Updated : 05/26/2021
// Source  : http://learnvsxnow.codeplex.com/
// Note    : Copyright 2008-2021, Istvan Novak, All rights reserved
//
// This file contains classes that define event arguments used by the SimpleEditorPane class
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

using System;
using Microsoft.VisualStudio.OLE.Interop;

namespace SandcastleBuilder.Package.Editors
{
    #region ExecArgs class
    //=====================================================================

    // ==================================================================================
    /// <summary>
    /// This class represents the arguments of an IOleCommandTarget.Exec method.
    /// </summary>
    // ==================================================================================
    public class ExecArgs
    {
        #region Private fields

        private readonly Guid _GroupId;
        private readonly uint _CommandId;
        private uint _CommandExecOpt;
        private IntPtr _pvaIn;
        private IntPtr _pvaOut;

        #endregion

        #region Lifecycle methods

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Creates a new instance of this class with the specified command identifiers.
        /// </summary>
        /// <param name="groupId">ID of the command group.</param>
        /// <param name="commandId">ID of the command within the group.</param>
        // --------------------------------------------------------------------------------
        public ExecArgs(Guid groupId, uint commandId)
        {
            _GroupId = groupId;
            _CommandId = commandId;
        }

        #endregion

        #region Public properties

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Gets the ID of the command group.
        /// </summary>
        // --------------------------------------------------------------------------------
        public Guid GroupId => _GroupId;

        // --------------------------------------------------------------------------------
        /// <summary>
        /// Gets the ID of the command within the group.
        /// </summary>
        // --------------------------------------------------------------------------------
        public uint CommandId => _CommandId;

        /// <summary>
        /// Options for the command
        /// </summary>
        public uint CommandExecOpt
        {
            get => _CommandExecOpt;
            set => _CommandExecOpt = value;
        }

        /// <summary>
        /// Pointer to input arguments
        /// </summary>
        public IntPtr PvaIn
        {
            get => _pvaIn;
            set => _pvaIn = value;
        }

        /// <summary>
        /// Pointer to output arguments
        /// </summary>
        public IntPtr PvaOut
        {
            get => _pvaOut;
            set => _pvaOut = value;
        }
        #endregion
    }
    #endregion

    #region QueryStatusArgs class
    //=====================================================================

    // ==================================================================================
    /// <summary>
    /// This class represents the arguments of an IOleCommandTarget.QueryStatus method.
    /// </summary>
    // ==================================================================================
    public sealed class QueryStatusArgs
    {
        #region Private fields

        private readonly Guid _GroupId;
        private uint _CommandCount;
        private OLECMD[] _Commands;
        private IntPtr _pCmdText;

        #endregion

        #region Lifecycle methods

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="groupId">The group ID</param>
        public QueryStatusArgs(Guid groupId)
        {
            _GroupId = groupId;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// The group ID
        /// </summary>
        public Guid GroupId => _GroupId;

        /// <summary>
        /// The command count
        /// </summary>
        public uint CommandCount
        {
            get => _CommandCount;
            set => _CommandCount = value;
        }

        /// <summary>
        /// The commands
        /// </summary>
        public OLECMD[] Commands
        {
            get => _Commands;
            set => _Commands = value;
        }

        /// <summary>
        /// The command text
        /// </summary>
        public IntPtr PCmdText
        {
            get => _pCmdText;
            set => _pCmdText = value;
        }

        /// <summary>
        /// The first command ID
        /// </summary>
        public uint FirstCommandId => _Commands[0].cmdID;

        /// <summary>
        /// The first command status
        /// </summary>
        public uint FirstCommandStatus
        {
            get => _Commands[0].cmdf;
            set => _Commands[0].cmdf = value;
        }
        #endregion
    }
    #endregion
}
