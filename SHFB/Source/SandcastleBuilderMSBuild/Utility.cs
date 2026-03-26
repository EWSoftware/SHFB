//===============================================================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : Utility.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/25/2026
// Note    : Copyright 2011-2026, Eric Woodruff, All rights reserved
//
// This file contains a utility class with extension and utility methods
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/15/2011  EFW  Created the code
//===============================================================================================================

using System.Collections.Generic;
using System.Linq;

using Sandcastle.Core.BuildEngine;
using Sandcastle.Core.PlugIn;

namespace SandcastleBuilder.MSBuild;

/// <summary>
/// This class contains utility and extension methods
/// </summary>
public static class Utility
{
    #region Plug-in execution point extension methods
    //=====================================================================

    /// <summary>
    /// This is used to determine if the enumerable list of execution points contains an entry for the
    /// specified build step and behavior.
    /// </summary>
    /// <param name="executionPoints">An enumerable list of execution points to check</param>
    /// <param name="step">The build step</param>
    /// <param name="behavior">The behavior</param>
    /// <returns>True if the enumerable list of execution points contains an entry for the specified build
    /// step and behavior or false if it does not.</returns>
    public static bool RunsAt(this IEnumerable<ExecutionPoint> executionPoints, BuildStep step,
      ExecutionBehaviors behavior)
    {
        return executionPoints.Any(p => p.BuildStep == step && (p.Behavior & behavior) != 0);
    }

    /// <summary>
    /// This is used to obtain the execution priority for a plug-in within the given build step and behavior
    /// </summary>
    /// <param name="executionPoints">An enumerable list of execution points to search</param>
    /// <param name="step">The build step</param>
    /// <param name="behavior">The behavior</param>
    /// <returns>The execution priority is used to determine the order in which the plug-ins will be
    /// executed.  Those with a higher priority value will be executed before those with a lower value.
    /// Those with an identical priority may be executed in any order within their group.</returns>
    public static int PriorityFor(this IEnumerable<ExecutionPoint> executionPoints, BuildStep step,
      ExecutionBehaviors behavior)
    {
        var point = executionPoints.FirstOrDefault(p => p.BuildStep == step && (p.Behavior & behavior) != 0);

        if(point != null)
            return point.Priority;

        return ExecutionPoint.DefaultPriority;
    }
    #endregion
}
