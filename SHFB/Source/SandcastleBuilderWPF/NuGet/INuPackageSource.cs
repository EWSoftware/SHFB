//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : INuGetPackageSource.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/18/2021
// Note    : Copyright 2021, Eric Woodruff, All rights reserved
//
// This file contains a class that is used to implement a NuGet package source 
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/06/2021  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;

namespace SandcastleBuilder.WPF.NuGet
{
    /// <summary>
    /// This is used to implement a NuGet package source
    /// </summary>
    public interface INuGetPackageSource
    {
        /// <summary>
        /// This read-only property returns the package source name
        /// </summary>
        string PackageSourceName { get; }

        /// <summary>
        /// This read-only property returns the package source location
        /// </summary>
        string PackageSourceLocation { get; }

        /// <summary>
        /// This read-only property returns whether or not the package source has been indexed and is ready
        /// for searching
        /// </summary>
        bool IsIndexed { get; }

        /// <summary>
        /// Search the package source for packages using the given search terms
        /// </summary>
        /// <param name="searchTerms">An enumerable list of the search terms to use</param>
        /// <param name="newSearch">True if this is a new search, false if continuing a prior search to get
        /// more results</param>
        /// <param name="includePreRelease">True to include pre-release versions, false to exclude them</param>
        /// <returns>An enumerable list of the packages found using the search terms or an empty list if there
        /// are no more results</returns>
        IEnumerable<NuGetPackage> SearchForPackages(IEnumerable<string> searchTerms, bool newSearch,
            bool includePreRelease);

        /// <summary>
        /// This is used to index the package source so that it can be searched
        /// </summary>
        void IndexPackageSource();
    }
}
