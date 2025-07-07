//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ExecutionContext.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2007-2025, Eric Woodruff, All rights reserved
//
// This file contains a class that defines the execution context in which the plug-in is being called during the
// build process.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/10/2007  EFW  Created the code
//===============================================================================================================

using System;

using Sandcastle.Core.BuildEngine;

namespace Sandcastle.Core.PlugIn
{
    /// <summary>
    /// This class defines the execution context in which the plug-in is being called during the build process
    /// </summary>
    public class ExecutionContext
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the current build step
        /// </summary>
        public BuildStep BuildStep { get; }

        /// <summary>
        /// This read-only property returns the execution behavior for the plug-in within the current context
        /// </summary>
        /// <value><c>Before</c> indicates that it is being called before the normal help file builder
        /// processing.  <c>After</c> indicates that it is being called after the normal help file builder
        /// processing.  <c>InsteadOf</c> indicates that it is being called instead of the normal help file
        /// builder processing.</value>
        public ExecutionBehaviors Behavior { get; }

        /// <summary>
        /// This property is used to set or get whether or not the plug-in actually executed
        /// </summary>
        /// <value>It is true by default.  Set it to false if the plug-in did not execute.</value>
        public bool Executed { get; set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildStep">The current build step.</param>
        /// <param name="behavior">The behavior of the plug-in for the current context.</param>
        /// <exception cref="ArgumentException">This is thrown if an attempt is made to specify more than one
        /// behavior type.</exception>
        public ExecutionContext(BuildStep buildStep, ExecutionBehaviors behavior)
        {
            this.BuildStep = buildStep;
            this.Behavior = behavior;
            this.Executed = true;

            // Don't allow more than one behavior type
            if(behavior != ExecutionBehaviors.Before && behavior != ExecutionBehaviors.After &&
              behavior != ExecutionBehaviors.InsteadOf)
            {
                throw new ArgumentException("Combinations of behavior are not allowed for the execution context",
                    nameof(behavior));
            }
        }
        #endregion
    }
}
