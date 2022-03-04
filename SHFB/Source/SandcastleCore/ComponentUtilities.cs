//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ComponentUtilities.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/27/2022
// Note    : Copyright 2007-2022, Eric Woodruff, All rights reserved
//
// This file contains a class containing properties and methods used to locate and work with build components,
// plug-ins, syntax generators, and presentation styles.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/01/2007  EFW  Created the code
// 10/06/2008  EFW  Changed the default location of custom components
// 07/04/2009  EFW  Merged build component and plug-in folder
// 11/10/2009  EFW  Added support for custom syntax filter components
// 03/07/2010  EFW  Added support for SHFBCOMPONENTROOT
// 12/17/2013  EFW  Removed the SandcastlePath property and all references to it.  Updated to use MEF to load
//                  plug-ins.
// 12/20/2013  EFW  Updated to use MEF to load the syntax filters and removed support for SHFBCOMPONENTROOT
// 12/26/2013  EFW  Updated to use MEF to load BuildAssembler build components
// 01/02/2014  EFW  Moved the component manager class to Sandcastle.Core
// 08/05/2014  EFW  Added support for getting a list of syntax generator resource item files
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
using System.Threading;
using System.Xml;
using System.Xml.Linq;

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

        private static string rootFolder, toolsFolder, coreComponentsFolder,
            thirdPartyComponentsFolder, coreReflectionDataFolder;

        private static readonly Regex reSyntaxSplitter = new Regex(",\\s*");
        private static List<CultureInfo> supportedLanguages;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the path to the Sandcastle Help File Builder root folder
        /// </summary>
        public static string RootFolder
        {
            get
            {
                SetPaths();
                return rootFolder;
            }
        }

        /// <summary>
        /// This read-only property returns the path to the Sandcastle Help File Builder tools folder
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
        /// This read-only property returns the core build components folder
        /// </summary>
        ///<remarks>These are the common components distributed with the Help File Builder</remarks>
        public static string CoreComponentsFolder
        {
            get
            {
                SetPaths();
                return coreComponentsFolder;
            }
        }

        /// <summary>
        /// This read-only property returns the common application data build components folder
        /// </summary>
        ///<remarks>Third party components should be located in the <strong>EWSoftware\Sandcastle Help File
        /// Builder\Components and Plug-Ins</strong> folder or a subfolder beneath it in the common application
        /// data folder.</remarks>
        public static string ThirdPartyComponentsFolder
        {
            get
            {
                SetPaths();
                return thirdPartyComponentsFolder;
            }
        }

        /// <summary>
        /// This read-only property returns the core reflection data file folder
        /// </summary>
        ///<remarks>These are the common reflection data files distributed with the Help File Builder</remarks>
        public static string CoreReflectionDataFolder
        {
            get
            {
                SetPaths();
                return coreReflectionDataFolder;
            }
        }

        /// <summary>
        /// This read-only property returns the default syntax filter setting
        /// </summary>
        /// <value>This returns "Standard" to add the standard C#, VB.NET and C++ syntax filter to each API
        /// topic.</value>
        public static string DefaultSyntaxFilter => "Standard";

        /// <summary>
        /// This read-only property returns a list of languages supported by the help file builder presentation
        /// styles.
        /// </summary>
        /// <value>The available language resources are determined by seeing what stop word list translations are
        /// available.</value>
        public static IEnumerable<CultureInfo> SupportedLanguages
        {
            get
            {
                if(supportedLanguages == null)
                {
                    string stopWordListFolder = Path.Combine(CoreComponentsFolder, "Shared", "StopWordList");

                    try
                    {
                        supportedLanguages = Directory.EnumerateFiles(stopWordListFolder, "*.txt").Select(
                            f => new CultureInfo(Path.GetFileNameWithoutExtension(f))).OrderBy(c => c.DisplayName).ToList();
                    }
                    catch
                    {
                        // Ignore any errors, just return a default list with the en-US language
                        supportedLanguages = new List<CultureInfo> { new CultureInfo("en-US") };
                    }
                }

                return supportedLanguages;
            }
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
            string location;

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
            if(rootFolder == null)
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
                    rootFolder = Path.Combine(Environment.GetEnvironmentVariable("SHFBROOT"), "net472");
                }

                // If not found, use the executing assembly's folder.  Builds and other stuff that relies on the
                // true location will fail under Visual Studio.  The project system should have set SHFBROOT
                // temporarily when it was initialized if it was found in the project.
                if(String.IsNullOrWhiteSpace(rootFolder))
                    rootFolder = assemblyLocation;

                if(rootFolder[rootFolder.Length - 1] == Path.DirectorySeparatorChar)
                    rootFolder = rootFolder.Substring(rootFolder.Length - 1);

                if(File.Exists(Path.Combine(rootFolder, "..", "SandcastleHelpFileBuilder.targets")))
                    rootFolder = rootFolder.Substring(0, rootFolder.LastIndexOf(Path.DirectorySeparatorChar));

                toolsFolder = Path.Combine(rootFolder, "Tools");
                coreComponentsFolder = Path.Combine(rootFolder, "Components");
                thirdPartyComponentsFolder = Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.CommonApplicationData), Constants.ComponentsAndPlugInsFolder);
                coreReflectionDataFolder = Path.Combine(rootFolder, "Data");
            }
        }

        /// <summary>
        /// This is used to return a composition container filled with the available build components (SHFB
        /// plug-ins, presentation styles, BuildAssembler components, and syntax generators).
        /// </summary>
        /// <param name="folders">An enumerable list of additional folders to search recursively for components.</param>
        /// <param name="resolver">A component assembly resolver to use or null to use a temporary one</param>
        /// <param name="cancellationToken">An optional cancellation token or null if not supported by the caller.</param>
        /// <returns>The a composition container that contains all of the available components</returns>
        /// <remarks>The following folders are searched in the following order.  If the given folder has not been
        /// specified or does not exist, it is ignored.
        /// 
        /// <list type="number">
        ///     <item>The enumerable list of additional folders - This is typically the current project's
        /// NuGet packages (package tool paths from the <c>SHFBComponentPath</c> item in their properties file),
        /// the project's <c>ComponentPath</c> folder, and the current project's folder.  This allows for
        /// project-specific build components.  Paths are searched in the order given above if specified.</item>
        ///     <item>Common application data folder - The help file builder's common application data folder
        /// where third-party build components are typically installed.</item>
        ///     <item><c>SHFBROOT</c> core components folder - The core Sandcastle Help File Builder components
        /// folder and its subfolders.  This allows for XCOPY deployments that keep everything together.</item>
        /// </list>
        /// 
        /// All folders and their subfolders are search recursively for assemblies (*.dll).  There may be
        /// duplicate component IDs across the assemblies found.  Only the first component for a unique
        /// ID will be used.  As such, assemblies in a folder with a higher search precedence can override
        /// copies in folders lower in the search order.</remarks>
        public static CompositionContainer CreateComponentContainer(IEnumerable<string> folders,
          ComponentAssemblyResolver resolver, CancellationToken cancellationToken)
        {
            if(folders == null)
                throw new ArgumentNullException(nameof(folders));

            var catalog = new AggregateCatalog();
            HashSet<string> searchedFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            bool disposeResolver = false;

            try
            {
                if(resolver == null)
                {
                    resolver = new ComponentAssemblyResolver();
                    disposeResolver = true;
                }

                foreach(string folder in folders)
                    AddAssemblyCatalogs(catalog, folder, searchedFolders, true, resolver, cancellationToken);

                AddAssemblyCatalogs(catalog, ThirdPartyComponentsFolder, searchedFolders, true, resolver, cancellationToken);
                AddAssemblyCatalogs(catalog, CoreComponentsFolder, searchedFolders, true, resolver, cancellationToken);
            }
            finally
            {
                if(disposeResolver)
                    resolver.Dispose();
            }

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
        /// <param name="resolver">A component assembly resolver for finding dependency assemblies</param>
        /// <param name="cancellationToken">An optional cancellation token or null if not supported by the caller.</param>
        /// <remarks>It is done this way to prevent a single assembly that would normally be discovered via a
        /// directory catalog from preventing all assemblies from loading if it cannot be examined when the parts
        /// are composed (i.e. trying to load a Windows Store assembly on Windows 7).</remarks>
        private static void AddAssemblyCatalogs(AggregateCatalog catalog, string folder,
          HashSet<string> searchedFolders, bool includeSubfolders, ComponentAssemblyResolver resolver,
          CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(!String.IsNullOrWhiteSpace(folder) && Directory.Exists(folder) && !searchedFolders.Contains(folder))
            {
                searchedFolders.Add(folder);

                // When debugging components, there may be a copy in the .\obj folder which tends to get found
                // later so it is used rather than the one in the .\bin folder which is actually being debugged.
                // If this is an .\obj folder and a .\bin folder has already been seen in the same location,
                // ignore it.
                if(folder.EndsWith("\\obj", StringComparison.OrdinalIgnoreCase) && searchedFolders.Contains(
                  Path.Combine(Path.GetDirectoryName(folder), "bin")))
                {
                    return;
                }

                bool hadComponents = false;

                foreach(var file in Directory.EnumerateFiles(folder, "*.dll"))
                {
                    if(cancellationToken != CancellationToken.None)
                        cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var asmCat = new AssemblyCatalog(file);

                        // Force MEF to load the assembly and figure out if there are any exports.  Valid
                        // assemblies won't throw any exceptions and will contain parts and will be added to
                        // the catalog.  Use Count() rather than Any() to ensure it touches all parts in case
                        // that makes a difference.
                        if(asmCat.Parts.Count() > 0)
                        {
                            catalog.Catalogs.Add(asmCat);
                            hadComponents = true;
                        }
                        else
                            asmCat.Dispose();

                    }   // Ignore the errors we may expect to see but log them for debugging purposes
                    catch(ArgumentException ex)
                    {
                        // These can occur if it tries to load a foreign framework assembly (i.e. .NETStandard)
                        // In this case, the inner exception will be the bad image format exception.  If not,
                        // report the issue.
                        if(!(ex.InnerException is BadImageFormatException))
                            throw;

                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                    catch(FileNotFoundException ex)
                    {
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

                // Track folders with components so that we can search them for dependencies later if needed
                if(hadComponents)
                    resolver.AddFolder(folder);

                // Enumerate subfolders separately so that we can skip future requests for the same folder
                if(includeSubfolders)
                {
                    try
                    {
                        foreach(string subfolder in Directory.EnumerateDirectories(folder, "*", SearchOption.AllDirectories))
                            AddAssemblyCatalogs(catalog, subfolder, searchedFolders, false, resolver, cancellationToken);
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

            if(allFilters == null)
                throw new ArgumentNullException(nameof(allFilters));

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
                            StringComparison.OrdinalIgnoreCase) == -1));
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
                    sb.AppendFormat(CultureInfo.InvariantCulture, "<generator id=\"{0}\">{1}</generator>\r\n",
                        generator.Id, generator.DefaultConfiguration);
                }
                else
                    sb.AppendFormat(CultureInfo.InvariantCulture, "<generator id=\"{0}\" />\r\n", generator.Id);

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
                sb.AppendFormat(CultureInfo.InvariantCulture, "<language name=\"{0}\" style=\"{1}\" />\r\n",
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

            if(componentContainer == null)
                throw new ArgumentNullException(nameof(componentContainer));

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

        #region XML stream axis methods
        //=====================================================================

        /// <summary>
        /// This provides a fast and efficient way of querying large XML files for a specific element type
        /// </summary>
        /// <param name="xmlFile">The XML file to search</param>
        /// <param name="elementName">The element name for which to search</param>
        /// <returns>An enumerable list of <see cref="XElement"/> instances for each of the found nodes</returns>
        /// <remarks>This version only looks for a single element type which cannot contain nested instances of
        /// the given element.</remarks>
        /// <overloads>
        /// <summary>There are two overloads for this method</summary>
        /// <remarks>Using these methods to search for specific elements avoids having to load the entire file
        /// which can be quite costly in terms of memory when it is extremely large.  It is typically faster as
        /// well since it doesn't have to load and parse the whole file before starting the search.</remarks>
        /// </overloads>
        public static IEnumerable<XElement> XmlStreamAxis(string xmlFile, string elementName)
        {
            using(XmlReader reader = XmlReader.Create(xmlFile, new XmlReaderSettings()))
            {
                while(reader.ReadToFollowing(elementName))
                    yield return (XElement)XNode.ReadFrom(reader);
            }
        }

        /// <summary>
        /// This provides a fast and efficient way of querying large XML files for specific element types which
        /// themselves may contain instances of the given elements.
        /// </summary>
        /// <param name="xmlFile">The XML file to search</param>
        /// <param name="elementNames">An enumerable list of element names for which to search</param>
        /// <returns>An enumerable list of <see cref="XElement"/> instances for each of the found nodes including
        /// any nested instances of elements with those names.</returns>
        /// <remarks>If the element contains nested instances of the elements, the parent is returned first
        /// followed by the nested elements (one level deep only).</remarks>
        public static IEnumerable<XElement> XmlStreamAxis(string xmlFile, IEnumerable<string> elementNames)
        {
            HashSet<string> elements = new HashSet<string>(elementNames);

            using(XmlReader reader = XmlReader.Create(xmlFile, new XmlReaderSettings()))
            {
                reader.MoveToContent();

                while(reader.Read())
                    if(reader.NodeType == XmlNodeType.Element && elementNames.Contains(reader.Name))
                    {
                        var root = (XElement)XNode.ReadFrom(reader);

                        yield return root;

                        // We'll only look one level deep and no further
                        foreach(var d in root.Descendants().Where(d => elements.Contains(d.Name.ToString())))
                            yield return d;
                    }
            }
        }
        #endregion

        #region Deterministic hash code method
        //=====================================================================

        /// <summary>
        /// This returns a deterministic hash code that is the same in the full .NET Framework and in .NET Core
        /// in every session given the same string to hash.
        /// </summary>
        /// <param name="hashString">The string to hash</param>
        /// <returns>The deterministic hash code</returns>
        /// <remarks>The hashing algorithm differs in .NET Core and returns different hash codes for each session.
        /// This was done for security to prevent DoS attacks. For the help file builder, we're just using it to
        /// generate a short filenames or other constant IDs.  As such, we need a deterministic hash code to keep
        /// generating the same hash code for the same IDs in all sessions regardless of platform so that the
        /// filenames and other IDs stay the same for backward compatibility.</remarks>
        public static int GetHashCodeDeterministic(this string hashString)
        {
            if(hashString == null)
                throw new ArgumentNullException(nameof(hashString));

            // This is equivalent to the .NET Framework hashing algorithm but doesn't use unsafe code.  It
            // will generate the same value as the .NET Framework version given the same string.
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                int len = hashString.Length, i = 0, h1, h2;

                while(len > 2)
                {
                    h1 = (hashString[i + 1] << 16) + hashString[i];
                    h2 = 0;

                    if(len >= 3)
                    {
                        if(len >= 4)
                            h2 = hashString[i + 3] << 16;

                        h2 += hashString[i + 2];
                    }

                    hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ h1;
                    hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ h2;

                    i += 4;
                    len -= 4;
                }

                if(len > 0)
                {
                    h1 = 0;

                    if(len >= 2)
                        h1 = hashString[i + 1] << 16;

                    h1 += hashString[i];

                    hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ h1;
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
        #endregion
    }
}
