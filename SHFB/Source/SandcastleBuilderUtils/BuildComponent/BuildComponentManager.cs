//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BuildComponentManager.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/27/2013
// Note    : Copyright 2007-2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that manages the set of third party build components including language syntax
// filters.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.6.0.2  11/01/2007  EFW  Created the code
// 1.8.0.0  10/06/2008  EFW  Changed the default location of custom components
// 1.8.0.3  07/04/2009  EFW  Merged build component and plug-in folder
// 1.8.0.3  11/10/2009  EFW  Added support for custom syntax filter components
// 1.8.0.4  03/07/2010  EFW  Added support for SHFBCOMPONENTROOT
// -------  12/17/2013  EFW  Removed the SandcastlePath property and all references to it.  Updated to use MEF
//                           to load plug-ins.
//          12/20/2013  EFW  Updated to use MEF to load the syntax filters and removed support for
//                           SHFBCOMPONENTROOT.
//          12/26/2013  EFW  Updated to use MEF to load BuildAssembler build components
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Sandcastle.Core.BuildAssembler.SyntaxGenerator;

namespace SandcastleBuilder.Utils.BuildComponent
{
    /// <summary>
    /// This class is used to manage the set of third party build components including language syntax filters.
    /// </summary>
    public static class BuildComponentManager
    {
        #region Private data members
        //=====================================================================

        private static string shfbFolder, buildComponentsFolder;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the path to the Sandcastle Help File builder assemblies
        /// </summary>
        public static string HelpFileBuilderFolder
        {
            get
            {
                SetPaths();
                return shfbFolder;
            }
        }

        /// <summary>
        /// This read-only property returns the common application data build components folder
        /// </summary>
        public static string BuildComponentsFolder
        {
            get
            {
                SetPaths();
                return buildComponentsFolder;
            }
        }

        /// <summary>
        /// This read-only property returns the default syntax filter setting
        /// </summary>
        /// <value>This returns "Standard" to add the standard C#, VB.NET and C++ syntax filter to each API
        /// topic.</value>
        public static string DefaultSyntaxFilter
        {
            get { return "Standard"; }
        }
        #endregion

        #region General component methods
        //=====================================================================

        /// <summary>
        /// Set the paths used to find component configuration files and assemblies
        /// </summary>
        private static void SetPaths()
        {
            if(shfbFolder == null)
            {
                // Try for SHFBROOT first (the VSPackage needs it)
                shfbFolder = Environment.GetEnvironmentVariable("SHFBROOT");

                // If not, use the executing assembly's folder
                if(String.IsNullOrWhiteSpace(shfbFolder))
                    shfbFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                if(!shfbFolder.EndsWith(@"\", StringComparison.Ordinal))
                    shfbFolder += @"\";

                // Third party build components should be located in a
                // "EWSoftware\Sandcastle Help File Builder\Components and Plug-Ins"
                // folder in the common application data folder.
                buildComponentsFolder = FolderPath.TerminatePath(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    Constants.ComponentsAndPlugInsFolder));
            }
        }

        /// <summary>
        /// This is used to return a composition container filled with the available build components (SHFB
        /// plug-ins, MRefBuilder add-ins, and BuildAssembler components and syntax generators).
        /// </summary>
        /// <param name="currentProject">The current project or null to ignore the current project.</param>
        /// <remarks>The following folders are searched in the following order.  If the given folder has not been
        /// specified or does not exist, it is ignored.
        /// 
        /// <list type="number">
        ///     <item><c>SHFBROOT</c> - The root Sandcastle Help File Builder installation folder.  Assemblies in
        /// the root help file builder folder always have the highest precedence.  Unfortunately, this is a side
        /// effect of how MEF uses directory catalogs.  It always searches the executing assembly's folder for
        /// components in addition to the given folder.  Subfolders are searched last (see below).</item>
        ///     <item>Current project <c>ComponentPath</c> or project folder - This allows for project-specific
        /// build components.  If the project's <c>ComponentPath</c> property is set, that folder is searched.
        /// If not, the project's folder is searched instead.</item>
        ///     <item>Common application data folder - The help file builder's common application data folder
        /// where third-party build components are typically installed.</item>
        ///     <item>Subfolders under <c>SHFBROOT</c> - Subfolders under the Sandcastle Help File Builder
        /// installation folder.  This allows for XCOPY deployments that keep everything together.</item>
        /// </list>
        /// 
        /// All folders and their subfolders are search recursively for assemblies (*.dll).  There may be
        /// duplicate component IDs across the assemblies found.  Only the first component for a unique
        /// ID will be used.  As such, assemblies in a folder with a higher search precedence can override
        /// copies in folders lower in the search order.</remarks>
        public static CompositionContainer GetComponentContainer(SandcastleProject currentProject)
        {
            FolderPath projectFolder = new FolderPath(null),
                componentsFolder = new FolderPath(BuildComponentManager.BuildComponentsFolder, null),
                helpFileBuilderFolder = new FolderPath(BuildComponentManager.HelpFileBuilderFolder, null);

            if(currentProject != null)
                if(currentProject.ComponentPath.Path.Length != 0)
                    projectFolder = currentProject.ComponentPath;
                else
                    projectFolder = new FolderPath(Path.GetDirectoryName(currentProject.Filename), null);

            // Create an aggregate catalog that combines directory catalogs for all of the possible component
            // locations.
            var catalog = new AggregateCatalog();

            if(projectFolder.Path.Length != 0 && Directory.Exists(projectFolder))
                AddDirectoryCatalogs(catalog, projectFolder);

            if(componentsFolder.Path.Length != 0 && Directory.Exists(componentsFolder))
                AddDirectoryCatalogs(catalog, componentsFolder);

            // As noted in the comments above, the root SHFB folder is always searched first due to how MEF
            // uses directory catalogs.  This will add components from subfolders beneath it too.
            AddDirectoryCatalogs(catalog, helpFileBuilderFolder);

            return new CompositionContainer(catalog);
        }

        /// <summary>
        /// This adds a directory catalog to the given aggregate catalog for the given folder and all of its
        /// subfolders recursively.
        /// </summary>
        /// <param name="catalog">The aggregate catalog to which the directory catalogs are added</param>
        /// <param name="folder">The root folder to search.  It and all subfolders recursively will be added
        /// to the aggregate catalog if they contain assemblies.</param>
        private static void AddDirectoryCatalogs(AggregateCatalog catalog, string folder)
        {
            if(Directory.EnumerateFiles(folder, "*.dll").Any())
                catalog.Catalogs.Add(new DirectoryCatalog(folder));

            foreach(string subfolder in Directory.EnumerateDirectories(folder, "*", SearchOption.AllDirectories))
                if(Directory.EnumerateFiles(subfolder, "*.dll").Any())
                    catalog.Catalogs.Add(new DirectoryCatalog(subfolder));
        }
        #endregion

        #region Syntax filter methods
        //=====================================================================

        /// <summary>
        /// This is used to convert the given set of comma-separated syntax filter IDs to a set of recognized
        /// filter IDs.
        /// </summary>
        /// <param name="allFilters">The list of all available syntax filter generators</param>
        /// <param name="filterIds">A comma-separated list of syntax filter IDs to convert</param>
        /// <returns>The validated and recognized set of syntax filter IDs.  If possible, the value is condensed
        /// to one of a set of combination values such as None, All, AllButUsage, or Standard.</returns>
        public static string ToRecognizedSyntaxFilterIds(IEnumerable<ISyntaxGeneratorMetadata> allFilters,
          string filterIds)
        {
            var definedFilters = SyntaxFiltersFrom(allFilters, filterIds);

            // Convert to None, All, AllButUsage, or Standard?  If not, then convert to the list of defined
            // filters that we know about.
            int definedCount = definedFilters.Count();

            if(definedCount == 0)
                filterIds = "None";
            else
                if(definedCount == allFilters.Count())
                    filterIds = "All";
                else
                    if(definedCount == allFilters.Count(af => af.Id.IndexOf("usage",
                      StringComparison.OrdinalIgnoreCase) == -1))
                        filterIds = "AllButUsage";
                    else
                        if(definedCount == 3 && (definedFilters.All(df => df.Id == "CSharp" ||
                          df.Id == "VisualBasic" || df.Id == "CPlusPlus")))
                            filterIds = "Standard";
                        else
                            filterIds = String.Join(", ", definedFilters.Select(f => f.Id).ToArray());

            return filterIds;
        }

        /// <summary>
        /// This is used to return a collection of syntax filters based on the comma-separated list of IDs passed
        /// to the method.
        /// </summary>
        /// <param name="allFilters">The list of all available syntax filter generators</param>
        /// <param name="filterIds">A comma-separated list of syntax filter ID values.</param>
        /// <returns>An enumerable list of <see cref="ISyntaxGeneratorMetadata" /> representing the syntax
        /// filters found.</returns>
        /// <remarks>The following special IDs are also recognized: None = No filters, All = all filters,
        /// AllButUsage = All but syntax filters with "Usage" in their ID (i.e. VisualBasicUsage), Standard = C#,
        /// VB.NET, and C++ only.</remarks>
        public static IEnumerable<ISyntaxGeneratorMetadata> SyntaxFiltersFrom(
          IEnumerable<ISyntaxGeneratorMetadata> allFilters, string filterIds)
        {
            var filters = new List<ISyntaxGeneratorMetadata>();
            string syntaxId;

            if(filterIds == null)
                filterIds = String.Empty;

            foreach(string id in filterIds.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                // IDs are matched in lowercase
                syntaxId = id.ToLowerInvariant();

                // Translate from some common alternate names if necessary
                foreach(var sf in allFilters)
                    if(sf.AlternateIds.ToLowerInvariant().Split(new[] { ',', ' ' },
                      StringSplitOptions.RemoveEmptyEntries).Contains(syntaxId))
                    {
                        syntaxId = sf.Id.ToLowerInvariant();
                        break;
                    }

                if(syntaxId.Length == 0)
                    syntaxId = "none";

                // Handle special cases for backward compatibility.  These were defined when SyntaxFilters was
                // an enumerated type.
                switch(syntaxId)
                {
                    case "none":    // No syntax filters
                        break;

                    case "all":     // All filters
                        filters.AddRange(allFilters);
                        break;

                    case "allbutusage":     // All but usage filters
                        filters.AddRange(allFilters.Where(sf => sf.Id.IndexOf("usage",
                            StringComparison.Ordinal) == -1));
                        break;

                    case "standard":    // Standard syntax filters
                        filters.AddRange(allFilters.Where(sf =>
                            sf.Id.Equals("CSharp", StringComparison.OrdinalIgnoreCase) ||
                            sf.Id.Equals("VisualBasic", StringComparison.OrdinalIgnoreCase) ||
                            sf.Id.Equals("CPlusPlus", StringComparison.OrdinalIgnoreCase)));
                        break;

                    default:
                        // Unknown filter IDs and ones already there are ignored
                        var found = allFilters.FirstOrDefault(f => f.Id.Equals(syntaxId,
                            StringComparison.OrdinalIgnoreCase));

                        if(found != null && !filters.Contains(found))
                            filters.Add(found);
                        break;
                }
            }

            return filters.OrderBy(sf => sf.SortOrder).ThenBy(sf => sf.Id);
        }

        /// <summary>
        /// This returns the syntax generator XML elements to insert into a BuildAssembler configuration file for
        /// the comma-separated list of syntax filter IDs.
        /// </summary>
        /// <param name="allFilters">The list of all available syntax filter generators</param>
        /// <param name="filterIds">A comma-separated list of syntax filter ID values.</param>
        /// <returns>A string containing the generator XML elements for the specified syntax filter IDs.</returns>
        public static string SyntaxFilterGeneratorsFrom(IEnumerable<ISyntaxGeneratorMetadata> allFilters,
          string filterIds)
        {
            StringBuilder sb = new StringBuilder(1024);

            foreach(var generator in SyntaxFiltersFrom(allFilters, filterIds))
                if(!String.IsNullOrWhiteSpace(generator.DefaultConfiguration))
                {
                    sb.AppendFormat("<generator id=\"{0}\" name=\"{1}\">{2}</generator>\r\n", generator.Id,
                        generator.LanguageElementName, generator.DefaultConfiguration);
                }
                else
                    sb.AppendFormat("<generator id=\"{0}\" name=\"{1}\" />\r\n", generator.Id,
                        generator.LanguageElementName);

            return sb.ToString();
        }

        /// <summary>
        /// This returns the syntax language XML elements to insert into a BuildAssembler configuration file for
        /// the comma-separated list of syntax filter IDs.
        /// </summary>
        /// <param name="allFilters">The list of all available syntax filter generators</param>
        /// <param name="filterIds">A comma-separated list of syntax filter ID values.</param>
        /// <returns>A string containing the language XML elements for the specified syntax filter IDs.</returns>
        public static string SyntaxFilterLanguagesFrom(IEnumerable<ISyntaxGeneratorMetadata> allFilters,
          string filterIds)
        {
            StringBuilder sb = new StringBuilder(1024);

            foreach(var generator in SyntaxFiltersFrom(allFilters, filterIds))
                sb.AppendFormat("<language name=\"{0}\" style=\"{1}\" />\r\n",
                    generator.LanguageElementName, generator.KeywordStyleParameter);

            return sb.ToString();
        }
        #endregion
    }
}
