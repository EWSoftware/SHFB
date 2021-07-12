﻿//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ReflectionDataSetDictionary.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/23/2021
// Note    : Copyright 2012-2021, Eric Woodruff, All rights reserved
//
// This file contains a class representing a dictionary of reflection data settings for the various .NET
// Framework platforms and versions.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/09/2012  EFW  Created the code
// 11/17/2012  EFW  Moved the code from the framework type converter into this class
// 01/02/2014  EFW  Moved the frameworks code to Sandcastle.Core
// 06/24/2015  EFW  Changed the framework settings classes to reflection data to be more general in nature
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sandcastle.Core.Reflection
{
    /// <summary>
    /// This dictionary contains reflection data settings for the various .NET Framework platforms and versions
    /// </summary>
    public sealed class ReflectionDataSetDictionary : Dictionary<string, ReflectionDataSet>
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get the title of the default framework version to use
        /// </summary>
        /// <remarks>The default is the .NET Framework 4.7.2</remarks>
        public static string DefaultFrameworkTitle => ".NET Framework 4.7.2";

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="componentLocations">An optional enumerable list of additional component locations to
        /// search for reflection data set files.</param>
        /// <remarks>Keys are case-insensitive</remarks>
        /// <remarks>The following folders are searched in the following order.  If the given folder has not been
        /// specified or does not exist, it is ignored.
        /// 
        /// <list type="number">
        ///     <item><c>SHFBROOT</c> - The root Sandcastle Help File Builder installation folder and its
        /// subfolders.  This allows for XCOPY deployments that keep everything together.</item>
        ///     <item>Common application data folder - The help file builder's common application data folder
        /// where third-party build components are typically installed.</item>
        ///     <item>The enumerable list of additional folders - This is typically the current project's
        /// <c>ComponentPath</c> folder and the current project's folder.  This allows for project-specific
        /// reflection data files.  If the project's <c>ComponentPath</c> property is set, that folder is
        /// searched first and then the project's folder is searched.</item>
        /// </list>
        /// 
        /// All folders and their subfolders are search recursively for reflection data files (*.reflection).
        /// There may be duplicate titles across the files found.  If duplicates are found, the last one seen is
        /// used.  As such, reflection data files in a folder with a later search precedence can override
        /// copies in folders earlier in the search order.</remarks>
        public ReflectionDataSetDictionary(IEnumerable<string> componentLocations) : base(StringComparer.OrdinalIgnoreCase)
        {
            List<string> files = new List<string>();

            if(Directory.Exists(ComponentUtilities.CoreReflectionDataFolder))
                foreach(string file in Directory.EnumerateFiles(ComponentUtilities.CoreReflectionDataFolder, "*.reflection",
                  SearchOption.AllDirectories))
                    files.Add(file);

            if(Directory.Exists(ComponentUtilities.ThirdPartyComponentsFolder))
                foreach(string file in Directory.EnumerateFiles(ComponentUtilities.ThirdPartyComponentsFolder, "*.reflection",
                  SearchOption.AllDirectories))
                    files.Add(file);

            if(componentLocations != null)
                foreach(string folder in componentLocations)
                    if(!String.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
                    {
                        foreach(string file in Directory.EnumerateFiles(folder, "*.reflection",
                          SearchOption.AllDirectories))
                            files.Add(file);
                    }

            foreach(string file in files)
            {
                var rds = new ReflectionDataSet(file);

                // A file with an identical title will override an existing entry
                this[rds.Title] = rds;
            }
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to find the first core framework matching the specified title or the next highest
        /// version if found but not installed if redirection is requested.
        /// </summary>
        /// <param name="title">The title to find</param>
        /// <param name="withRedirect">True to redirect to the next highest version that is installed if the
        /// requested title is not installed or false to return the exact match even if not present.</param>
        /// <returns>The framework settings if found or null if not found</returns>
        public ReflectionDataSet CoreFrameworkByTitle(string title, bool withRedirect)
        {
            if(!this.TryGetValue(title, out ReflectionDataSet dataSet))
                dataSet = null;
            else
            {
                if(withRedirect && dataSet.Platform != PlatformType.DotNet &&
                  dataSet.Platform != PlatformType.DotNetStandard && (!dataSet.IsPresent || !dataSet.IsCoreFramework))
                {
                    dataSet = this.Values.Where(v => v.Platform == dataSet.Platform && v.IsCoreFramework).OrderBy(
                        v => v.Version).FirstOrDefault(v => v.Version > dataSet.Version && v.IsPresent);
                }
            }

            return dataSet;
        }

        /// <summary>
        /// This is used to find the first core framework matching the specified platform and version or the next
        /// highest version if not found and redirection is requested.
        /// </summary>
        /// <param name="platform">The platform to find.</param>
        /// <param name="version">The version to find.  This can be a partial version number if redirection is
        /// requested.</param>
        /// <param name="withRedirect">True to redirect to the next highest version that is installed or false to
        /// return the exact match even if not present.</param>
        /// <returns>The framework settings if found or null if not found</returns>
        public ReflectionDataSet CoreFrameworkMatching(string platform, Version version, bool withRedirect)
        {
            var dataSet = this.Values.FirstOrDefault(f => f.Platform == platform && f.IsCoreFramework &&
                f.Version == version);

            if((dataSet == null || !dataSet.IsPresent) && withRedirect)
            {
                dataSet = this.Values.Where(v => v.Platform == platform && v.IsCoreFramework).OrderBy(
                    v => v.Version).LastOrDefault(v => v.Version.Major == version.Major &&
                         v.Version.Minor == version.Minor && v.IsPresent);

                if(dataSet == null)
                    dataSet = this.Values.Where(v => v.Platform == platform && v.IsCoreFramework).OrderBy(
                        v => v.Version).FirstOrDefault(v => v.Version > version && v.IsPresent);
            }

            return dataSet;
        }

        /// <summary>
        /// This is used to find the most recent core framework matching the specified platform
        /// </summary>
        /// <param name="platform">The platform to find</param>
        /// <returns>The framework settings if found or null if not found</returns>
        public ReflectionDataSet CoreFrameworkMostRecent(string platform)
        {
            var dataSet = this.Values.Where(f => f.Platform == platform && f.IsCoreFramework &&
                f.IsPresent).OrderByDescending(f => f.Version).FirstOrDefault();

            return dataSet;
        }

        /// <summary>
        /// This is used to find the best match for the given set of framework identifiers
        /// </summary>
        /// <param name="frameworks">An enumerable list of platform ID/version pairs</param>
        /// <returns>The best matching reflection data set or null if one could not be found</returns>
        public ReflectionDataSet BestMatchFor(IEnumerable<(string PlatformType, string Version)> frameworks)
        {
            if(frameworks == null)
                throw new ArgumentNullException(nameof(frameworks));

            List<ReflectionDataSet> bestMatches = new List<ReflectionDataSet>();
            ReflectionDataSet match;

            foreach(var framework in frameworks)
            {
                switch(framework.PlatformType)
                {
                    case PlatformType.DotNet:
                    case PlatformType.DotNetCore:
                    case PlatformType.DotNetCoreApp:
                    case PlatformType.DotNetStandard:
                        match = this.CoreFrameworkMostRecent(PlatformType.DotNet);
                        break;

                    case PlatformType.DotNetPortable:
                        match = this.CoreFrameworkMatching(framework.PlatformType, new Version(framework.Version), false);

                        if(match == null)
                            match = this.CoreFrameworkMostRecent(PlatformType.DotNetFramework);
                        break;

                    default:
                        match = this.CoreFrameworkMatching(framework.PlatformType, new Version(framework.Version), true);
                        break;
                }

                if(match != null)
                    bestMatches.Add(match);
            }

            if(bestMatches.Count == 1)
                return bestMatches[0];

            if(bestMatches.Count == 0)
                return null;

            // If we get multiple matches, at least one of them should be DotNetFramework, DotNet, or
            // DotNetStandard so choose the highest version favoring DotNETFramework, DotNet, and then
            // DotNetStandard as they will have the most types in that order and we want all of their XML
            // comments files.
            match = bestMatches.Where(m => m.Platform == PlatformType.DotNetFramework).OrderByDescending(
                m => m.Version).FirstOrDefault();

            if(match != null)
                return match;

            match = bestMatches.Where(m => m.Platform == PlatformType.DotNet).OrderByDescending(
                m => m.Version).FirstOrDefault();

            if(match != null)
                return match;

            return bestMatches.Where(m => m.Platform == PlatformType.DotNetStandard).OrderByDescending(
                m => m.Version).FirstOrDefault();
        }
        #endregion
    }
}
