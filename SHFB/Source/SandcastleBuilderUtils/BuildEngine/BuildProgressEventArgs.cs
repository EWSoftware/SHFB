//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BuildProgressEventArgs.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/15/2021
// Note    : Copyright 2006-2021, Eric Woodruff, All rights reserved
//
// This file contains the event arguments class for the build progress event
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/04/2006  EFW  Created the code
//===============================================================================================================

using System;

namespace SandcastleBuilder.Utils.BuildEngine
{
    /// <summary>
    /// This is a custom event arguments class use to report build progress
    /// </summary>
    public class BuildProgressEventArgs : EventArgs
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get the current build step of the build process
        /// </summary>
        public BuildStep BuildStep { get; }

        /// <summary>
        /// This read-only property is used to get whether or not the build step has changed
        /// </summary>
        public bool StepChanged { get; }

        /// <summary>
        /// This read-only property is used to get whether or not the build has completed
        /// </summary>
        /// <remarks>The build may have succeeded, failed, or been canceled.  See <see cref="BuildStep"/> for the
        /// final disposition.</remarks>
        public bool HasCompleted => (this.BuildStep >= BuildStep.Completed);

        /// <summary>
        /// This read-only property is used to get the message associated with the progress report
        /// </summary>
        public string Message { get; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildStep">The current build step</param>
        /// <param name="stepChanged">True if the build step has changed, false if not</param>
        /// <param name="message">The message to report</param>
        public BuildProgressEventArgs(BuildStep buildStep, bool stepChanged, string message)
        {
            this.BuildStep = buildStep;
            this.StepChanged = stepChanged;
            this.Message = (message ?? String.Empty);
        }
        #endregion
    }
}
