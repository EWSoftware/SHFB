//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BuildComponentManager.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/03/2011
// Note    : Copyright 2007-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that manages the set of third party build
// components including language syntax filters.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.2  11/01/2007  EFW  Created the code
// 1.8.0.0  10/06/2008  EFW  Changed the default location of custom components
// 1.8.0.3  07/04/2009  EFW  Merged build component and plug-in folder
// 1.8.0.3  11/10/2009  EFW  Added support for custom syntax filter components
// 1.8.0.4  03/07/2010  EFW  Added support for SHFBCOMPONENTROOT
//=============================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.XPath;

using SandcastleBuilder.Utils.BuildEngine;

// All classes go in the SandcastleBuilder.Utils.BuildComponent namespace
namespace SandcastleBuilder.Utils.BuildComponent
{
    /// <summary>
    /// This class is used to manage the set of third party build components
    /// including language syntax filters.
    /// </summary>
    public static class BuildComponentManager
    {
        #region Private data members
        //=====================================================================

        private static Dictionary<string, BuildComponentInfo> buildComponents;
        private static Dictionary<string, SyntaxFilterInfo> syntaxFilters;
        private static string sandcastlePath, shfbFolder, buildComponentFolder;

        private static Regex reMatchPath = new Regex(@"[A-Z]:\\.[^;]+\\Sandcastle(?=\\Prod)",
            RegexOptions.IgnoreCase);
        private static Regex reMatchShfbFolder = new Regex("{@SHFBFolder}", RegexOptions.IgnoreCase);
        private static Regex reMatchCompFolder = new Regex("{@ComponentsFolder}", RegexOptions.IgnoreCase);
        private static Regex reMatchSandcastleFolder = new Regex("{@SandcastlePath}", RegexOptions.IgnoreCase);
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This returns the default syntax filter setting
        /// </summary>
        /// <value>This returns "Standard" to add the standard C#, VB.NET and
        /// C++ syntax filter to each API topic.</value>
        public static string DefaultSyntaxFilter
        {
            get { return "Standard"; }
        }

        /// <summary>
        /// This read-only property returns the build components folder
        /// </summary>
        public static string BuildComponentsFolder
        {
            get { return buildComponentFolder; }
        }

        /// <summary>
        /// This is used to set or get the Sandcastle installation folder
        /// </summary>
        public static string SandcastlePath
        {
            get
            {
                // Figure out where Sandcastle is if not specified
                if(String.IsNullOrEmpty(sandcastlePath))
                {
                    // Try to find it based on the DXROOT environment variable
                    sandcastlePath = Environment.GetEnvironmentVariable("DXROOT");

                    if(String.IsNullOrEmpty(sandcastlePath) || !sandcastlePath.Contains(@"\Sandcastle"))
                        sandcastlePath = String.Empty;

                    if(sandcastlePath.Length == 0)
                    {
                        // Search for it in the PATH environment variable
                        Match m = reMatchPath.Match(Environment.GetEnvironmentVariable("PATH"));

                        // If not found in the path, search all fixed drives
                        if(m.Success)
                            sandcastlePath = m.Value;
                        else
                        {
                            sandcastlePath = BuildProcess.FindOnFixedDrives(@"\Sandcastle");

                            // If not found there, try the VS 2005 SDK folders
                            if(sandcastlePath.Length == 0)
                            {
                                sandcastlePath = BuildProcess.FindSdkExecutable("MRefBuilder.exe");

                                if(sandcastlePath.Length != 0)
                                    sandcastlePath = sandcastlePath.Substring(0, sandcastlePath.LastIndexOf('\\'));
                            }
                        }
                    }
                }
                else
                    if(!File.Exists(sandcastlePath + @"ProductionTools\MRefBuilder.exe"))
                        sandcastlePath = String.Empty;

                return sandcastlePath;
            }
            set
            {
                if(String.IsNullOrEmpty(value))
                    sandcastlePath = null;
                else
                    sandcastlePath = value;
            }
        }

        /// <summary>
        /// This returns a dictionary containing the loaded build component
        /// information.
        /// </summary>
        /// <value>The dictionary keys are the component IDs.</value>
        public static Dictionary<string, BuildComponentInfo> BuildComponents
        {
            get
            {
                if(buildComponents == null || buildComponents.Count == 0)
                    LoadBuildComponents();

                return buildComponents;
            }
        }

        /// <summary>
        /// This returns a dictionary containing the loaded language syntax
        /// filter build component information.
        /// </summary>
        /// <value>The dictionary keys are the syntax filter IDs.</value>
        public static Dictionary<string, SyntaxFilterInfo> SyntaxFilters
        {
            get
            {
                if(syntaxFilters == null || syntaxFilters.Count == 0)
                    LoadSyntaxFilters();

                return syntaxFilters;
            }
        }
        #endregion

        #region Private helper methods
        //=====================================================================

        /// <summary>
        /// Set the paths used to find component configuration files and
        /// assemblies.
        /// </summary>
        private static void SetPaths()
        {
            if(shfbFolder == null)
            {
                Assembly asm = Assembly.GetExecutingAssembly();

                // Third party build components should be located in a
                // "EWSoftware\Sandcastle Help File Builder\Components and Plug-Ins"
                // folder in the common application data folder.
                shfbFolder = asm.Location;
                shfbFolder = shfbFolder.Substring(0, shfbFolder.LastIndexOf('\\') + 1);
                buildComponentFolder = FolderPath.TerminatePath(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    Constants.ComponentsAndPlugInsFolder));
            }
        }

        /// <summary>
        /// Load the build components found in the Components and Plug-Ins
        /// folder and its subfolders.
        /// </summary>
        private static void LoadBuildComponents()
        {
            List<string> allFiles = new List<string>();
            XPathDocument configFile;
            XPathNavigator navConfig;
            BuildComponentInfo info;
            string componentPath;

            SetPaths();

            buildComponents = new Dictionary<string, BuildComponentInfo>();

            // Give precedence to components in the optional SHFBCOMPONENTROOT
            // environment variable folder.
            componentPath = Environment.ExpandEnvironmentVariables("%SHFBCOMPONENTROOT%");

            if(!String.IsNullOrEmpty(componentPath) && Directory.Exists(componentPath))
                allFiles.AddRange(Directory.GetFiles(componentPath, "*.components",
                    SearchOption.AllDirectories));

            // Add the standard component config file and any third-party
            // component config files in the installation folder.  This
            // allows for XCOPY deployments of SHFB to build servers.
            allFiles.AddRange(Directory.GetFiles(shfbFolder, "*.components",
                SearchOption.AllDirectories));

            // Finally, check the common app data build components folder
            if(Directory.Exists(buildComponentFolder))
                allFiles.AddRange(Directory.GetFiles(buildComponentFolder,
                    "*.components", SearchOption.AllDirectories));

            foreach(string file in allFiles)
            {
                configFile = new XPathDocument(file);
                navConfig = configFile.CreateNavigator();

                foreach(XPathNavigator component in navConfig.Select("components/component"))
                {
                    info = new BuildComponentInfo(component);

                    // Ignore components with duplicate IDs
                    if(!buildComponents.ContainsKey(info.Id))
                        buildComponents.Add(info.Id, info);
                }
            }
        }

        /// <summary>
        /// Load the syntax filter information found in the Components and
        /// Plug-Ins folder and its subfolders.
        /// </summary>
        private static void LoadSyntaxFilters()
        {
            List<string> allFiles = new List<string>();
            XPathDocument configFile;
            XPathNavigator navConfig;
            SyntaxFilterInfo info;
            string id;

            SetPaths();

            syntaxFilters = new Dictionary<string, SyntaxFilterInfo>();

            if(Directory.Exists(buildComponentFolder))
                allFiles.AddRange(Directory.GetFiles(buildComponentFolder,
                    "*.filters", SearchOption.AllDirectories));

            // Add the standard syntax filter config file and any third-party
            // component config files in the installation folder too.  This
            // allows for XCOPY deployments of SHFB to build servers.
            allFiles.AddRange(Directory.GetFiles(shfbFolder, "*.filters",
                SearchOption.AllDirectories));

            foreach(string file in allFiles)
            {
                configFile = new XPathDocument(file);
                navConfig = configFile.CreateNavigator();

                foreach(XPathNavigator filter in navConfig.Select(
                  "syntaxFilters/filter"))
                {
                    info = new SyntaxFilterInfo(filter);

                    // The dictionary stores the keys in lowercase so as to
                    // match keys without regard to the case entered in the
                    // project property.
                    id = info.Id.ToLowerInvariant();

                    // Ignore components with duplicate IDs
                    if(!syntaxFilters.ContainsKey(id))
                        syntaxFilters.Add(id, info);
                }
            }
        }
        #endregion

        #region Public methods
        //=====================================================================

        /// <summary>
        /// This is used to resolve replacement tags and environment variables
        /// in a build component's assembly path and return the actual path
        /// to it.
        /// </summary>
        /// <param name="path">The path to resolve</param>
        /// <returns>The actual absolute path to the assembly</returns>
        public static string ResolveComponentPath(string path)
        {
            if(String.IsNullOrEmpty(shfbFolder))
                LoadBuildComponents();

            path = reMatchShfbFolder.Replace(path, shfbFolder);
            path = reMatchCompFolder.Replace(path, buildComponentFolder);
            path = reMatchSandcastleFolder.Replace(path, SandcastlePath);

            return Environment.ExpandEnvironmentVariables(path);
        }

        /// <summary>
        /// This is used to return a collection of syntax filters based on
        /// the comma-separated list of IDs passed to the method.
        /// </summary>
        /// <param name="filterIds">A comma-separated list of syntax filter
        /// ID values</param>
        /// <returns>A collection containing <see cref="SyntaxFilterInfo" />
        /// entries for each syntax filter ID found.</returns>
        /// <remarks>The following special IDs are also recognized: None = No
        /// filters, All = all filters, AllButUsage = All but syntax filters
        /// with "Usage" in their ID (i.e. VisualBasicUsage), Standard = C#,
        /// VB.NET, and C++ only.</remarks>
        public static Collection<SyntaxFilterInfo> SyntaxFiltersFrom(string filterIds)
        {
            List<SyntaxFilterInfo> filters = new List<SyntaxFilterInfo>();
            SyntaxFilterInfo info;
            string syntaxId;

            if(syntaxFilters == null || syntaxFilters.Count == 0)
                LoadSyntaxFilters();

            foreach(string id in filterIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                // IDs are matched in lowercase
                syntaxId = id.Trim().ToLowerInvariant();

                // Translate from some common alternate names if necessary
                foreach(var sf in syntaxFilters.Values)
                    if(sf.AlternateNames.Contains(syntaxId))
                    {
                        syntaxId = sf.Id.ToLowerInvariant();
                        break;
                    }

                if(syntaxId.Length == 0)
                    syntaxId = "none";

                // Handle special cases for backward compatibility.  These
                // were defined when SyntaxFilters was an enumerated type.
                switch(syntaxId)
                {
                    case "none":    // No syntax filters
                        break;

                    case "all":     // All filters
                        foreach(SyntaxFilterInfo i in syntaxFilters.Values)
                            filters.Add(i);
                        break;

                    case "allbutusage":     // All but usage filters
                        foreach(string sfId in syntaxFilters.Keys)
                            if(sfId.IndexOf("usage", StringComparison.Ordinal) == -1)
                                filters.Add(syntaxFilters[sfId]);
                        break;

                    case "standard":    // Standard syntax filters
                        if(syntaxFilters.TryGetValue("csharp", out info))
                            filters.Add(info);

                        if(syntaxFilters.TryGetValue("visualbasic", out info))
                            filters.Add(info);

                        if(syntaxFilters.TryGetValue("cplusplus", out info))
                            filters.Add(info);
                        break;

                    default:
                        // Unknown filter IDs and ones already there are ignored
                        if(syntaxFilters.TryGetValue(syntaxId, out info))
                            if(!filters.Any(f => f.Id == info.Id))
                                filters.Add(info);
                        break;
                }
            }

            filters.Sort((x, y) =>
                {
                    if(x.SortOrder == y.SortOrder)
                        return String.Compare(x.Id, y.Id, StringComparison.OrdinalIgnoreCase);

                    return (x.SortOrder < y.SortOrder) ? -1 : 1;
                });

            return new Collection<SyntaxFilterInfo>(filters);
        }

        /// <summary>
        /// This returns the syntax generator XML elements to insert into a
        /// BuildAssembler configuration file for the comma-separated list of
        /// syntax filter IDs.
        /// </summary>
        /// <param name="filterIds">A comma-separated list of syntax filter
        /// ID values</param>
        /// <returns>A string containing the generator XML elements for the
        /// specified syntax filter IDs.</returns>
        public static string SyntaxFilterGeneratorsFrom(string filterIds)
        {
            StringBuilder sb = new StringBuilder(1024);

            foreach(SyntaxFilterInfo info in SyntaxFiltersFrom(filterIds))
                sb.AppendFormat("{0}\r\n", info.GeneratorXml);

            return sb.ToString();
        }

        /// <summary>
        /// This returns the syntax language XML elements to insert into a
        /// BuildAssembler configuration file for the comma-separated list of
        /// syntax filter IDs.
        /// </summary>
        /// <param name="filterIds">A comma-separated list of syntax filter
        /// ID values</param>
        /// <returns>A string containing the language XML elements for the
        /// specified syntax filter IDs.</returns>
        public static string SyntaxFilterLanguagesFrom(string filterIds)
        {
            StringBuilder sb = new StringBuilder(1024);

            foreach(SyntaxFilterInfo info in SyntaxFiltersFrom(filterIds))
                sb.AppendFormat("{0}\r\n", info.LanguageXml);

            return sb.ToString();
        }
        #endregion
    }
}
