//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : VsProjectCapabilitiesPresenceChecker.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/03/2021
// Note    : Copyright 2021, Eric Woodruff, All rights reserved
//
// This file contains the class used to enable NuGet package support in help file builder projects
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/21/2021  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.Shell.Interop;

namespace SandcastleBuilder.Package.Nodes
{
    /// <summary>
    /// This class is used to advertise the capabilities supported in order to get NuGet package support in
    /// help file builder projects.
    /// </summary>
    /// <remarks>The NuGet package manager in Visual Studio currently only provides support for packages.config
    /// files not PackageReference elements in third-party project systems like the help file builder.  This gets
    /// the command to light up on the context menus but we handle the command in our project and reference
    /// container nodes to show our own package manager that will add PackageReference elements to the project.
    /// It's very limited and there is an ugly workaround to get it to refresh the project after a reference is
    /// added but it does work.  We could have stayed with the packages.config method but PackageReference
    /// elements are cleaner, we know they work, and it has been shown as the way to include the tools and data
    /// packages for server-side builds for quite a while now.</remarks>
    internal class VsProjectCapabilitiesPresenceChecker : IVsBooleanSymbolPresenceChecker
    {
        // These are the project capabilities required in order to support NuGet packages.  SHFB supports these
        // so we can support NuGet packages.
        private static readonly HashSet<string> ActualProjectCapabilities = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "AssemblyReferences",
            "DeclaredSourceItems",
            "UserSourceItems",
            "PackageReferences"
        };

        /// <inheritdoc />
        /// <remarks>The capabilities do not change so this always returns false</remarks>
        public bool HasChangedSince(ref object versionObject)
        {
            return false;
        }

        /// <inheritdoc />
        public bool IsSymbolPresent(string symbol)
        {
            return ActualProjectCapabilities.Contains(symbol);
        }
    }
}
