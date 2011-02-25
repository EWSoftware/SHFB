//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ExecutionContext.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/25/2007
// Note    : Copyright 2007, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that defines the execution context in which the
// plug-in is being called during the build process.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.2.0  09/10/2007  EFW  Created the code
//=============================================================================

using System;

using SandcastleBuilder.Utils.BuildEngine;

// All classes go in the SandcastleBuilder.Utils.PlugIn namespace
namespace SandcastleBuilder.Utils.PlugIn
{
    /// <summary>
    /// This class defines the execution context in which the plug-in is being
    /// called during the build process.
    /// </summary>
    public class ExecutionContext
    {
        private BuildStep buildStep;
        private ExecutionBehaviors behavior;
        private bool executed;

        /// <summary>
        /// This read-only property returns the current build step
        /// </summary>
        public BuildStep BuildStep
        {
            get { return buildStep; }
        }

        /// <summary>
        /// This read-only property returns the execution behavior for the
        /// plug-in in the current context.
        /// </summary>
        /// <value><b>Before</b> indicates that it is being called before the
        /// normal help file builder processing.  <b>After</b> indicates that
        /// it is being called after the normal help file builder processing.
        /// <b>InsteadOf</b> indicates that it is being called instead of the
        /// normal help file builder processing.</value>
        public ExecutionBehaviors Behavior
        {
            get { return behavior; }
        }

        /// <summary>
        /// This property is used to set or get whether or not the plug-in
        /// actually executed.
        /// </summary>
        /// <value>It is true by default.  Set it to false if the plug-in
        /// did not execute.</value>
        public bool Executed
        {
            get { return executed; }
            set { executed = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="plugInBuildStep">The current build step.</param>
        /// <param name="plugInBehavior">The behavior of the plug-in for the
        /// current context.</param>
        /// <exception cref="ArgumentException">This is thrown if an attempt
        /// is made to specify more than one behavior type.</exception>
        internal ExecutionContext(BuildStep plugInBuildStep,
          ExecutionBehaviors plugInBehavior)
        {
            buildStep = plugInBuildStep;
            behavior = plugInBehavior;
            executed = true;

            // Don't allow more than one behavior type
            if(behavior != ExecutionBehaviors.Before && behavior !=
             ExecutionBehaviors.After && behavior != ExecutionBehaviors.InsteadOf)
                throw new ArgumentException("Combinations of behavior are " +
                    "not allowed for the execution context", "plugInBehavior");
        }
    }
}
