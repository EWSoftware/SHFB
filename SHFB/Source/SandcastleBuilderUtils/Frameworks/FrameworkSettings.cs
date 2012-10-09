//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : FrameworkSettings.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/10/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is used to contain settings information for a specific .NET Framework version
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.   This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.5.0  09/09/2012  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SandcastleBuilder.Utils.Frameworks
{
    /// <summary>
    /// This class is used to contain settings for a specific .NET Framework version
    /// </summary>
    public sealed class FrameworkSettings
    {
        #region Private data members
        //=====================================================================

        private List<AssemblyLocation> assemblyLocations;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the framework title
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// This read-only property returns the platform associated with the framework
        /// </summary>
        public string Platform { get; private set; }

        /// <summary>
        /// This read-only property returns the version number associated with the framework
        /// </summary>
        public Version Version { get; private set; }

        /// <summary>
        /// This read-only property returns the framework version to use for redirection if this version is not
        /// present.
        /// </summary>
        public string Redirect { get; private set; }

        /// <summary>
        /// This read-only property returns an enumerable list of assembly locations
        /// </summary>
        public IEnumerable<AssemblyLocation> AssemblyLocations
        {
            get { return assemblyLocations; }
        }

        /// <summary>
        /// This read-only property can be used to determine if the framework is present on the current system
        /// </summary>
        /// <returns>True if the core framework folder exists and contains the first assembly, false if not.  If
        /// the first assembly is present, it is assumed that all of them are.</returns>
        public bool IsFrameworkPresent
        {
            get
            {
                AssemblyLocation al = assemblyLocations.FirstOrDefault(l => l.IsCoreLocation);

                return (al != null && al.Assemblies.Count() != 0 && File.Exists(al.Assemblies.First().Filename));
            }
        }

        /// <summary>
        /// This read-only property returns an enumerable list of all assemblies in the framework version
        /// </summary>
        public IEnumerable<AssemblyDetails> AllAssemblies
        {
            get
            {
                foreach(var al in assemblyLocations)
                    foreach(var ad in al.Assemblies)
                        yield return ad;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Private constructor
        /// </summary>
        private FrameworkSettings()
        {
            assemblyLocations = new List<AssemblyLocation>();
        }
        #endregion

        #region Methods used to convert to/from XML
        //=====================================================================

        /// <summary>
        /// This is used to load the settings for a .NET Framework version from an XML element
        /// </summary>
        /// <param name="framework">The XML element containing the settings</param>
        /// <returns>The new framework settings item</returns>
        internal static FrameworkSettings FromXml(XElement framework)
        {
            FrameworkSettings fs = new FrameworkSettings
            {
                Title = framework.Attribute("Title").Value,
                Platform = framework.Attribute("Platform").Value,
                Version = new Version(framework.Attribute("Version").Value),
                Redirect = (string)framework.Attribute("Redirect")
            };

            foreach(var location in framework.Descendants("Location"))
                fs.assemblyLocations.Add(AssemblyLocation.FromXml(location));

            return fs;
        }

        /// <summary>
        /// This is used to convert the framework settings to an XML element
        /// </summary>
        /// <returns>The framework settings as an XML element</returns>
        internal XElement ToXml()
        {
            XElement framework = new XElement("Framework", new[] {
                new XAttribute("Title", this.Title),
                new XAttribute("Platform", this.Platform),
                new XAttribute("Version", this.Version),
                String.IsNullOrEmpty(this.Redirect) ? null : new XAttribute("Redirect", this.Redirect)
            });

            XElement locations = new XElement("AssemblyLocations");
            locations.Add(this.AssemblyLocations.Select(l => l.ToXml()));

            framework.Add(locations);

            return framework;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to return an enumerable list of comments file locations
        /// </summary>
        /// <param name="language">An optional language to use when searching for XML comments files, or
        /// null to use the current language.</param>
        /// <returns>An enumerable list of file paths to search for XML comments files</returns>
        public IEnumerable<string> CommentsFileLocations(CultureInfo language)
        {
            string path;

            foreach(var l in assemblyLocations)
                if(this.Platform != PlatformType.DotNetPortable || this.Version.Major != 4 || this.Version.Minor > 0)
                {
                    // If localized, the comments files will be in a sub-folder based on the language
                    path = null;

                    // Check for localized versions using the specified language
                    if(language != null)
                        path = CultureSpecificCommentsFileFolder(l, language);

                    // If not found, check for localized versions using the current language
                    if(path == null && (language == null || language.Name != CultureInfo.CurrentCulture.Name))
                        path = CultureSpecificCommentsFileFolder(l, CultureInfo.CurrentCulture);

                    // If not found, check for localized versions using the default English language
                    if(path == null && !CultureInfo.CurrentCulture.Name.StartsWith("en",
                      StringComparison.OrdinalIgnoreCase))
                        path = CultureSpecificCommentsFileFolder(l, CultureInfo.GetCultureInfo("en"));

                    // If no culture-specific folder was found, try the same location as the assemblies
                    if(path == null && l.Assemblies.Any(a => File.Exists(Path.ChangeExtension(a.Filename, ".xml"))))
                        path = l.Path;

                    if(path != null)
                        yield return Path.Combine(path, "*.xml");
                }
                else
                {
                    // The .NET Portable Library Framework 4.0 duplicates most of its comments files across all
                    // of the profile folders.  To minimize the duplication, we'll find the folder with the most
                    // files and return it followed by an entry for each unique file in subsequent folders.
                    var commentGroups = Directory.EnumerateFiles(l.Path, "*.xml",
                        SearchOption.AllDirectories).Where(d => File.Exists(
                            Path.ChangeExtension(d, ".dll"))).GroupBy(d =>
                                Path.GetDirectoryName(d)).OrderByDescending(d => d.Count());

                    HashSet<string> commentsFiles = new HashSet<string>(commentGroups.First().Select(
                        f => Path.GetFileName(f)));

                    yield return Path.Combine(commentGroups.First().Key, "*.xml");

                    foreach(var g in commentGroups)
                        foreach(var f in g.Where(f => !commentsFiles.Contains(Path.GetFileName(f))).ToList())
                            yield return Path.Combine(g.Key, Path.GetFileName(f));
                }
        }

        /// <summary>
        /// This is used to see if any comments files exist in a culture-specific framework location folder
        /// </summary>
        /// <param name="location">The framework location</param>
        /// <param name="language">The language used to check for a culture-specific folder</param>
        /// <returns>True if files were</returns>
        private static string CultureSpecificCommentsFileFolder(AssemblyLocation location, CultureInfo language)
        {
            string path = location.Path;

            if(language == null)
                return null;

            if(Directory.Exists(Path.Combine(path, language.Name)))
                path = Path.Combine(path, language.Name);
            else
                if(Directory.Exists(Path.Combine(path, language.TwoLetterISOLanguageName)))
                    path = Path.Combine(path, language.TwoLetterISOLanguageName);
                else
                    path = null;

            if(path != null && !location.Assemblies.Any(a => File.Exists(Path.ChangeExtension(
              Path.Combine(path, Path.GetFileName(a.Filename)), ".xml"))))
                path = null;

            return path;
        }

        /// <summary>
        /// This is used to see if the framework contains an assembly that uses the specified name
        /// </summary>
        /// <param name="assemblyName">The assembly name without a path or extension or a strong name value.
        /// If a strong name value is specified, only the name part is used to determine if the assembly is
        /// present in the framework.  Assembly names are compare case-insensitively.</param>
        /// <returns>True if the framework contains the named assembly, false if not.</returns>
        public bool ContainsAssembly(string assemblyName)
        {
            if(assemblyName.IndexOf(',') != 0)
                assemblyName = assemblyName.Split(',')[0].Trim();

            return assemblyLocations.Any(al => al.Assemblies.Any(a => a.Name.Equals(assemblyName,
                StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// This is used to find an assembly by name
        /// </summary>
        /// <param name="assemblyName">The assembly name without a path or extension or a strong name value.
        /// If a strong name value is specified, only the name part is used to determine if the assembly is
        /// present in the framework.  Assembly names are compare case-insensitively.</param>
        /// <returns>The assembly if found or null if not found</returns>
        public AssemblyDetails FindAssembly(string assemblyName)
        {
            if(assemblyName.IndexOf(',') != 0)
                assemblyName = assemblyName.Split(',')[0].Trim();

            foreach(var al in assemblyLocations)
            {
                var ad = al.Assemblies.FirstOrDefault(a => a.Name.Equals(assemblyName,
                    StringComparison.OrdinalIgnoreCase));

                if(ad != null)
                    return ad;
            }

            return null;
        }
        #endregion
    }
}
