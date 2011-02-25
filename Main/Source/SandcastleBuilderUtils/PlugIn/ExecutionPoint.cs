//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ExecutionPoint.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/14/2008
// Note    : Copyright 2007-2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that defines when a plug-in gets executed during
// the build process.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.2.0  09/09/2007  EFW  Created the code
// 1.8.0.1  11/14/2008  EFW  Added execution priority support
//=============================================================================

using System;

using SandcastleBuilder.Utils.BuildEngine;

// All classes go in the SandcastleBuilder.Utils.PlugIn namespace
namespace SandcastleBuilder.Utils.PlugIn
{
    /// <summary>
    /// This class defines when a plug-in gets executed during the build
    /// process.
    /// </summary>
    public class ExecutionPoint
    {
        #region Private data members
        //=====================================================================

        internal const int DefaultPriority = 1000;

        private BuildStep buildStep;
        private ExecutionBehaviors behavior;
        private int priority;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the build step in which the
        /// plug-in will run.
        /// </summary>
        public BuildStep BuildStep
        {
            get { return buildStep; }
        }

        /// <summary>
        /// This read-only property returns the execution behavior of the
        /// plug-in.
        /// </summary>
        public ExecutionBehaviors Behavior
        {
            get { return behavior; }
        }

        /// <summary>
        /// This read-only property returns the execution priority of the
        /// plug-in.
        /// </summary>
        /// <value>Plug-ins with a higher priority value will execute before
        /// those with a lower priority value.  If not specified, the default
        /// is 1,000.</value>
        public int Priority
        {
            get { return priority; }
        }
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="plugInBuildStep">The step in which the plug-in
        /// should run.</param>
        /// <param name="plugInBehavior">The behavior of the plug-in when it
        /// is ran.</param>
        /// <exception cref="ArgumentException">This is thrown if an attempt
        /// is made to set the Before or After behavior with the InsteadOf
        /// behavior.  It is also thrown for invalid combinations of build
        /// step and behavior, i.e. Initializing with Before or InsteadOf.
        /// See the help file for a full list.</exception>
        /// <overloads>There are two overloads for the constructor.</overloads>
        public ExecutionPoint(BuildStep plugInBuildStep,
          ExecutionBehaviors plugInBehavior)
        {
            bool isValid = true;

            buildStep = plugInBuildStep;
            behavior = plugInBehavior;
            priority = DefaultPriority;

            // Don't allow Before or After if InsteadOf is specified
            if((behavior & ExecutionBehaviors.InsteadOf) != 0 &&
              (behavior & ExecutionBehaviors.BeforeAndAfter) != 0)
                throw new ArgumentException("Before and/or After cannot be " +
                    "specified with InsteadOf", "plugInBehavior");

            if(buildStep == BuildStep.None)
                throw new ArgumentException("None is not a valid build " +
                    "step for a plug-in", "plugInBuildStep");

            // This was getting messy so it's broken up to be more readable

            // Before and InsteadOf can't be used with Initializing,
            // Canceled, or Failed.
            if((behavior & (ExecutionBehaviors.Before |
              ExecutionBehaviors.InsteadOf)) != 0 && (buildStep ==
              BuildStep.Initializing || buildStep > BuildStep.Completed))
                isValid = false;

            // InsteadOf cannot be used with FindingTools
            // or Completed.
            if(behavior == ExecutionBehaviors.InsteadOf && (
              buildStep == BuildStep.FindingTools ||
              buildStep == BuildStep.Completed))
                isValid = false;

            if(!isValid)
                throw new ArgumentException("The specified combination of " +
                    "build step and execution behavior is not valid.  See " +
                    "the help file for details.", "plugInBehavior");
        }

        /// <summary>
        /// This constructor is used to set a specific execution priority.
        /// </summary>
        /// <param name="plugInBuildStep">The step in which the plug-in
        /// should run.</param>
        /// <param name="plugInBehavior">The behavior of the plug-in when it
        /// is ran.</param>
        /// <param name="plugInPriority">The execution priority for the
        /// plug-in.</param>
        public ExecutionPoint(BuildStep plugInBuildStep,
          ExecutionBehaviors plugInBehavior, int plugInPriority) :
          this(plugInBuildStep, plugInBehavior)
        {
            priority = plugInPriority;
        }
        #endregion
    }
}
