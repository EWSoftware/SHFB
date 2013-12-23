//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ExecutionPoint.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/17/2013
// Note    : Copyright 2007-2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that defines when a plug-in gets executed during the build process.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.5.2.0  09/09/2007  EFW  Created the code
// 1.8.0.1  11/14/2008  EFW  Added execution priority support
//===============================================================================================================

using System;

using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.Utils.BuildComponent
{
    /// <summary>
    /// This class defines when a plug-in gets executed during the build process
    /// </summary>
    public class ExecutionPoint
    {
        #region Private data members
        //=====================================================================

        internal const int DefaultPriority = 1000;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the build step in which the plug-in will run.
        /// </summary>
        public BuildStep BuildStep { get; private set; }

        /// <summary>
        /// This read-only property returns the execution behavior of the plug-in.
        /// </summary>
        public ExecutionBehaviors Behavior { get; private set; }

        /// <summary>
        /// This read-only property returns the execution priority of the plug-in.
        /// </summary>
        /// <value>Plug-ins with a higher priority value will execute before those with a lower priority value.
        /// If not specified, the default is 1,000.</value>
        public int Priority { get; private set; }
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildStep">The step in which the plug-in should run.</param>
        /// <param name="behavior">The behavior of the plug-in when it is ran.</param>
        /// <exception cref="ArgumentException">This is thrown if an attempt is made to set the Before or After
        /// behavior with the InsteadOf behavior.  It is also thrown for invalid combinations of build step and
        /// behavior, i.e. Initializing with Before or InsteadOf.  See the help file for a full list.</exception>
        /// <overloads>There are two overloads for the constructor.</overloads>
        public ExecutionPoint(BuildStep buildStep, ExecutionBehaviors behavior)
        {
            bool isValid = true;

            this.BuildStep = buildStep;
            this.Behavior = behavior;
            this.Priority = DefaultPriority;

            // Don't allow Before or After if InsteadOf is specified
            if((behavior & ExecutionBehaviors.InsteadOf) != 0 &&
              (behavior & ExecutionBehaviors.BeforeAndAfter) != 0)
                throw new ArgumentException("Before and/or After cannot be specified with InsteadOf", "behavior");

            if(buildStep == BuildStep.None)
                throw new ArgumentException("None is not a valid build step for a plug-in", "buildStep");

            // This was getting messy so it's broken up to be more readable

            // Before and InsteadOf can't be used with Initializing, Canceled, or Failed.
            if((behavior & (ExecutionBehaviors.Before | ExecutionBehaviors.InsteadOf)) != 0 &&
              (buildStep == BuildStep.Initializing || buildStep > BuildStep.Completed))
                isValid = false;

            // InsteadOf cannot be used with or Completed, Canceled, or Failed.
            if(behavior == ExecutionBehaviors.InsteadOf && buildStep >= BuildStep.Completed)
                isValid = false;

            if(!isValid)
                throw new ArgumentException("The specified combination of build step and execution behavior " +
                    "is not valid.  See the help file for details.", "behavior");
        }

        /// <summary>
        /// This constructor is used to set a specific execution priority.
        /// </summary>
        /// <param name="buildStep">The step in which the plug-in should run.</param>
        /// <param name="behavior">The behavior of the plug-in when it is ran.</param>
        /// <param name="priority">The execution priority for the plug-in.</param>
        public ExecutionPoint(BuildStep buildStep, ExecutionBehaviors behavior, int priority) :
          this(buildStep, behavior)
        {
            this.Priority = priority;
        }
        #endregion
    }
}
