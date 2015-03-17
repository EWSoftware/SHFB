//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ComponentUtilities.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/12/2015
// Note    : Copyright 2007-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class containing properties and methods used to locate and work with build components,
// plug-ins, syntax generators, and presentation styles.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
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
//          01/02/2014  EFW  Moved the component manager class to Sandcastle.Core
//          08/05/2014  EFW  Added support for getting a list of syntax generator resource item files
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Sandcastle.Core.BuildAssembler.SyntaxGenerator;

namespace Sandcastle.Core
{
    /// <summary>
    /// This class contains properties and methods used to locate and work with build components, plug-ins,
    /// syntax generators, and presentation styles.
    /// </summary>
    public static class ComponentUtilities
    {
        #region Private data members
        //=====================================================================

        private static string toolsFolder, componentsFolder;

        private static Regex reSyntaxSplitter = new Regex(",\\s*");

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the path to the Sandcastle Help File and Tools assemblies
        /// </summary>
        public static string ToolsFolder
        {
            get
            {
                SetPaths();
                return toolsFolder;
            }
        }

        /// <summary>
        /// This read-only property returns the common application data build components folder
        /// </summary>
        ///<remarks>Third party components should be located in the <strong>EWSoftware\Sandcastle Help File
        /// Builder\Components and Plug-Ins</strong> folder or a subfolder beneath it in the common application
        /// data folder.</remarks>
        public static string ComponentsFolder
        {
            get
            {
                SetPaths();
                return componentsFolder;
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
        /// This is used to get the physical location of the specified assembly
        /// </summary>
        /// <param name="assembly">The assembly for which to get the physical location (the currently executing
        /// (calling) assembly if null).</param>
        /// <returns>This returns the actual location of the assembly, where it was found versus where it is
        /// executing from, which may be different if shadow copied.  This is required in order to find
        /// supporting files which will not be present in the shadow copied location.</returns>
        public static string AssemblyFolder(Assembly assembly)
        {
            string location = null;

            try
            {
                if(assembly == null)
                    assembly = Assembly.GetCallingAssembly();

                Uri codeBase = new Uri(assembly.CodeBase);

                if(codeBase.IsFile)
                    location = codeBase.LocalPath;
                else
                    location = assembly.Location;

                location = Path.GetDirectoryName(location);
            }
            catch
            {
                // Ignore errors.  If there are any, just return the Location value or the current folder.
                if(assembly != null)
                    location = Path.GetDirectoryName(assembly.Location);
                else
                    location = Directory.GetCurrentDirectory();
            }

            return location;
        }

        /// <summary>
        /// Set the paths used to find component configuration files and assemblies
        /// </summary>
        private static void SetPaths()
        {
            if(toolsFolder == null)
            {
                // Special case for Visual Studio.  Try for SHFBROOT first since the VSPackage needs it as we
                // will be running from the assembly in the extension's folder which does not contain all of the
                // supporting files.
                string assemblyLocation = AssemblyFolder(null);

                // We must check for the local app data folder as well as the All Users locations
                if(assemblyLocation.IndexOf(Environment.GetFolderPath(
                  Environment.SpecialFolder.LocalApplicationData), StringComparison.OrdinalIgnoreCase) != -1 ||
                  assemblyLocation.IndexOf(@"\IDE\Extensions\", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    toolsFolder = Environment.GetEnvironmentVariable("SHFBROOT");
                }

                // If not found, use the executing assembly's folder.  Builds and other stuff that relies on the
                // true location will fail under Visual Studio.  The project system should have set SHFBROOT
                // temporarily when it was initialized if it was found in the project.
                if(String.IsNullOrWhiteSpace(toolsFolder))
                    toolsFolder = assemblyLocation;

                if(toolsFolder[toolsFolder.Length - 1] != '\\')
                    toolsFolder += @"\";

                componentsFolder = Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.CommonApplicationData), Constants.ComponentsAndPlugInsFolder);

                if(componentsFolder[componentsFolder.Length - 1] != '\\')
                    componentsFolder += @"\";
            }
        }

        /// <summary>
        /// This is used to return a composition container filled with the available build components (SHFB
        /// plug-ins, presentation styles, and BuildAssembler components and syntax generators).
        /// </summary>
        /// <param name="folders">An enumerable list of additional folders to search recursively for components.</param>
        /// <remarks>The following folders are searched in the following order.  If the given folder has not been
        /// specified or does not exist, it is ignored.
        /// 
        /// <list type="number">
        ///     <item>The enumerable list of additional folders - This is typically the current project's
        /// <c>ComponentPath</c> folder and the current project's folder.  This allows for project-specific
        /// build components.  If the project's <c>ComponentPath</c> property is set, that folder is searched
        /// first and then the project's folder is searched.</item>
        ///     <item>Common application data folder - The help file builder's common application data folder
        /// where third-party build components are typically installed.</item>
        ///     <item><c>SHFBROOT</c> - The root Sandcastle Help File Builder installation folder and its
        /// subfolders.  This allows for XCOPY deployments that keep everything together.</item>
        /// </list>
        /// 
        /// All folders and their subfolders are search recursively for assemblies (*.dll).  There may be
        /// duplicate component IDs across the assemblies found.  Only the first component for a unique
        /// ID will be used.  As such, assemblies in a folder with a higher search precedence can override
        /// copies in folders lower in the search order.</remarks>
        public static CompositionContainer CreateComponentContainer(IEnumerable<string> folders)
        {
            var catalog = new AggregateCatalog();
            HashSet<string> searchedFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach(string folder in folders)
                AddAssemblyCatalogs(catalog, folder, searchedFolders, true);

            AddAssemblyCatalogs(catalog, ComponentsFolder, searchedFolders, true);

            // As noted in the comments above, the root SHFB folder is always searched first due to how MEF
            // uses directory catalogs.  This will add components from subfolders beneath it too.
            AddAssemblyCatalogs(catalog, ToolsFolder, searchedFolders, true);

            return new CompositionContainer(catalog);
        }

        /// <summary>
        /// This adds assembly catalogs to the given aggregate catalog for the given folder and all of its
        /// subfolders recursively.
        /// </summary>
        /// <param name="catalog">The aggregate catalog to which the assembly catalogs are added.</param>
        /// <param name="folder">The root folder to search.  It and all subfolders recursively will be searched
        /// for assemblies to add to the aggregate catalog.</param>
        /// <param name="searchedFolders">A hash set of folders that have already been searched and added.</param>
        /// <param name="includeSubfolders">True to search subfolders recursively, false to only search the given
        /// folder.</param>
        /// <remarks>It is done this way to prevent a single assembly that would normally be discovered via a
        /// directory catalog from preventing all assemblies from loading if it cannot be examined when the parts
        /// are composed (i.e. trying to load a Windows Store assembly on Windows 7).</remarks>
        private static void AddAssemblyCatalogs(AggregateCatalog catalog, string folder,
          HashSet<string> searchedFolders, bool includeSubfolders)
        {
            if(!String.IsNullOrWhiteSpace(folder) && Directory.Exists(folder) && !searchedFolders.Contains(folder))
            {
                foreach(var file in Directory.EnumerateFiles(folder, "*.dll"))
                {
                    try
                    {
                        var asmCat = new AssemblyCatalog(file);

                        // Force MEF to load the assembly and figure out if there are any exports.  Valid
                        // assemblies won't throw any exceptions and will contain parts and will be added to
                        // the catalog.  Use Count() rather than Any() to ensure it touches all parts in case
                        // that makes a difference.
                        if(asmCat.Parts.Count() > 0)
                            catalog.Catalogs.Add(asmCat);
                        else
                            asmCat.Dispose();
                    }
                    catch(FileNotFoundException ex)
                    {
                        // Ignore the errors we may expect to see but log them for debugging purposes
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                    catch(FileLoadException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                    catch(BadImageFormatException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                    catch(IOException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                    catch(System.Security.SecurityException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                    catch(UnauthorizedAccessException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                    catch(TypeLoadException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                    catch(ReflectionTypeLoadException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);

                        foreach(var lex in ex.LoaderExceptions)
                            System.Diagnostics.Debug.WriteLine(lex);
                    }
                }

                // Enumerate subfolders separately so that we can skip future requests for the same folder
                if(includeSubfolders)
                    try
                    {
                        foreach(string subfolder in Directory.EnumerateDirectories(folder, "*", SearchOption.AllDirectories))
                            AddAssemblyCatalogs(catalog, subfolder, searchedFolders, false);
                    }
                    catch(IOException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                    catch(System.Security.SecurityException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                    catch(UnauthorizedAccessException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
            }
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
                        if(definedCount == 4 && (definedFilters.All(df => df.Id == "C#" ||
                          df.Id == "Visual Basic" || df.Id == "Managed C++" || df.Id == "F#")))
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

            foreach(string id in reSyntaxSplitter.Split(filterIds).Where(f => f.Length != 0))
            {
                // IDs are matched in lowercase
                syntaxId = id.ToLowerInvariant();

                // Translate from some common alternate names if necessary
                foreach(var sf in allFilters)
                    if(reSyntaxSplitter.Split((sf.AlternateIds ?? String.Empty).ToLowerInvariant()).Where(
                      f => f.Length != 0).Contains(syntaxId))
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
                            sf.Id.Equals("C#", StringComparison.OrdinalIgnoreCase) ||
                            sf.Id.Equals("Visual Basic", StringComparison.OrdinalIgnoreCase) ||
                            sf.Id.Equals("Managed C++", StringComparison.OrdinalIgnoreCase) ||
                            sf.Id.Equals("F#", StringComparison.OrdinalIgnoreCase)));
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
                    sb.AppendFormat("<generator id=\"{0}\">{1}</generator>\r\n", generator.Id,
                        generator.DefaultConfiguration);
                }
                else
                    sb.AppendFormat("<generator id=\"{0}\" />\r\n", generator.Id);

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

        /// <summary>
        /// This returns an enumerable list of resource item files for all defined syntax filters
        /// </summary>
        /// <param name="componentContainer">The component container from which to get the available syntax
        /// filter generators.</param>
        /// <param name="language">The language used to find localized versions if they exist</param>
        /// <returns>An enumerable list of syntax filter generator resource item files.  If localized versions in
        /// the specified language do not exit, the default resource item files (typically English US) will be
        /// returned.</returns>
        public static IEnumerable<string> SyntaxGeneratorResourceItemFiles(
          CompositionContainer componentContainer, CultureInfo language)
        {
            string resourceItemPath, path;

            foreach(var filter in componentContainer.GetExports<ISyntaxGeneratorFactory, ISyntaxGeneratorMetadata>())
            {
                resourceItemPath = filter.Value.ResourceItemFileLocation;

                if(resourceItemPath != null)
                {
                    if(language == null)
                    {
                        path = Path.Combine(resourceItemPath, filter.Metadata.LanguageElementName + ".xml");

                        if(!File.Exists(path))
                            path = null;
                    }
                    else
                    {
                        path = Path.Combine(resourceItemPath, language.Name, filter.Metadata.LanguageElementName + ".xml");

                        if(!File.Exists(path))
                        {
                            path = Path.Combine(resourceItemPath, language.TwoLetterISOLanguageName,
                                filter.Metadata.LanguageElementName + ".xml");

                            if(!File.Exists(path))
                            {
                                path = Path.Combine(resourceItemPath, filter.Metadata.LanguageElementName + ".xml");

                                if(!File.Exists(path))
                                    path = null;
                            }
                        }
                    }

                    if(path != null)
                        yield return path;
                }
            }
        }
        #endregion
    }
}
