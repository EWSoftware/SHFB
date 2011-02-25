//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BuildProgressEventArgs.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/10/2006
// Note    : Copyright 2006, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the event arguments class for the build progress event
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  08/04/2006  EFW  Created the code
//=============================================================================

using System;

namespace SandcastleBuilder.Utils.BuildEngine
{
    /// <summary>
    /// This is a custom event arguments class for the
    /// <see cref="BuildProcess.BuildProgress"/> event.
    /// </summary>
    public class BuildProgressEventArgs : EventArgs
    {
        //=====================================================================
        // Private class members

        private BuildStep step;
        private string message;
        private bool hasCompleted;

        //=====================================================================
        // Properties

        /// <summary>
        /// Get the current build step of the build process
        /// </summary>
        public BuildStep BuildStep
        {
            get { return step; }
            internal set { step = value; }
        }

        /// <summary>
        /// Get the message associated with the progress report
        /// </summary>
        public string Message
        {
            get { return message; }
            internal set { message = value; }
        }

        /// <summary>
        /// Get a flag indicating whether or not the build has completed
        /// </summary>
        public bool HasCompleted
        {
            get { return hasCompleted; }
            internal set { hasCompleted = value; }
        }

        //=====================================================================
        // Methods, etc.

        /// <summary>
        /// Constructor
        /// </summary>
        public BuildProgressEventArgs()
        {
            message = String.Empty;
        }
    }
}
