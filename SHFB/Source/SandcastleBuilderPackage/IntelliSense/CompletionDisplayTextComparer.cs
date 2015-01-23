//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : CompletionDisplayTextComparer.cs
// Author  : Sam Harwell  (sam@tunnelvisionlabs.com)
// Updated : 03/21/2014
// Note    : Copyright 2014, Sam Harwell, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a completion display text comparer
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/21/2014  EFW  Added the code to the help file builder package
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Microsoft.VisualStudio.Language.Intellisense;

namespace SandcastleBuilder.Package.IntelliSense
{
    /// <summary>
    /// Represents a comparer for <see cref="Completion"/> instances, based on the
    /// <see cref="Completion.DisplayText"/> property.
    /// </summary>
    /// <remarks>The <see cref="Completion"/> instances are initially ordered using
    /// <see cref="StringComparer.OrdinalIgnoreCase"/>.  If two items have the same name with case ignored, they
    /// are recompared using <see cref="StringComparer.Ordinal"/>.  If they are still the same,
    /// <see cref="RuntimeHelpers.GetHashCode"/> is used to ensure that the <see cref="Compare"/> method only
    /// returns 0 for two identical instances.</remarks>
    internal sealed class CompletionDisplayTextComparer : IComparer<Completion>
    {
        /// <summary>
        /// Gets a default instance of <see cref="CompletionDisplayTextComparer"/>.
        /// </summary>
        public static readonly IComparer<Completion> Default = new CompletionDisplayTextComparer();

        /// <summary>
        /// Initializes a new instance of <see cref="CompletionDisplayTextComparer"/>.
        /// </summary>
        private CompletionDisplayTextComparer()
        {
        }

        /// <inheritdoc />
        public int Compare(Completion x, Completion y)
        {
            int result = StringComparer.OrdinalIgnoreCase.Compare(x.DisplayText, y.DisplayText);

            if(result != 0)
                return result;

            result = StringComparer.Ordinal.Compare(x.DisplayText, y.DisplayText);

            if(result != 0)
                return result;

            return RuntimeHelpers.GetHashCode(x) - RuntimeHelpers.GetHashCode(y);
        }
    }
}
