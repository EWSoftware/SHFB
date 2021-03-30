//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : DocumentationSource.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/29/2021
// Note    : Copyright 2006-2021, Eric Woodruff, All rights reserved
//
// This file contains a class representing a documentation source such as an assembly, an XML comments file, a
// solution, or a project.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/02/2006  EFW  Created the code
// 11/10/2006  EFW  Added CommentsOnly property.
// 12/31/2006  EFW  Converted path properties to FilePath objects
// 04/16/2008  EFW  Added support for wildcards
// 06/23/2008  EFW  Rewrote to support the MSBuild project format
// 06/05/2010  EFW  Added support for getting build include status and configuration settings from solution file
// 10/22/2012  EFW  Added support for .winmd documentation sources
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This represents an assembly, an XML comments file, a Visual Studio managed code project (C#, VB.NET,
    /// etc.), or a Visual Studio solution containing one or more managed code projects from which information
    /// is obtained to build a help file.
    /// </summary>
    /// <remarks>Wildcards are supported in the <see cref="SourceFile"/> property.</remarks>
    [DefaultProperty("SourceFile")]
    public class DocumentationSource : INotifyPropertyChanged
    {
        #region Private data members
        //=====================================================================

        private FilePath sourceFile;
        private string configuration, platform, targetFramework;
        private bool includeSubFolders;

        // Regular expression used to parse solution files
        private static readonly Regex reExtractProjectGuids = new Regex(
            "^Project\\(\"\\{(" +
            "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC|" +   // C#
            "F184B08F-C81C-45F6-A57F-5ABD9991F28F|" +   // VB.NET
            "8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942|" +   // C++
            "F2A71F9B-5D33-465A-A702-920D77279786|" +   // F#
            "E6FDF86B-F3D1-11D4-8576-0002A516ECE8|" +   // J#
            "9A19103F-16F7-4668-BE54-9A1E7A4F7556|" +   // C# - .NET Standard/Core project
            "778DAE3C-4631-46EA-AA77-85C1314464D9|" +   // VB.NET - .NET Standard/Core project
            "6EC3EE1D-3C4E-46DD-8F32-0CC8E7565705" +    // F# - .NET Standard/Core project
            ")\\}\"\\) = \".*?\", \"(?!http)" +
            "(?<Path>.*?proj)\", \"\\{(?<GUID>.*?)\\}\"", RegexOptions.Multiline);

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the project configuration to use when the source path refers to a Visual
        /// Studio solution or project.
        /// </summary>
        /// <value>If not set, the configuration value from the owning help file project will be used.  This will
        /// be ignored for assembly and XML comments file entries.</value>
        [Category("Project"), Description("The configuration to use for a solution or project documentation " +
          "source.  If blank, the configuration from the owning help file project will be used."),
          DefaultValue(null)]
        public string Configuration
        {
            get => configuration;
            set
            {
                configuration = (value ?? String.Empty).Trim();
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// This is used to get or set the project platform to use when the source path refers to a Visual Studio
        /// solution or project.
        /// </summary>
        /// <value>If not set, the platform value from the owning help file project will be used.  This will be
        /// ignored for assembly and XML comments file entries.</value>
        [Category("Project"), Description("The platform to use for a solution or project documentation " +
          "source.  If blank, the platform from the owning help file project will be used."), DefaultValue(null)]
        public string Platform
        {
            get => platform;
            set
            {
                platform = (value ?? String.Empty).Trim();
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// This is used to get or set the project target framework to use when the source path refers to a
        /// Visual Studio solution or project.
        /// </summary>
        /// <value>This only applies if the project uses multi-targeting.  If not set, the first target framework
        /// will be used.  This will be ignored for assembly and XML comments file entries.</value>
        [Category("Project"), Description("The target framework to use for project documentation sources that " +
          "use multi-targeting.  If blank, the first target framework will be used."), DefaultValue(null)]
        public string TargetFramework
        {
            get => targetFramework;
            set
            {
                targetFramework = (value ?? String.Empty).Trim();
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// This is used to set or get the documentation source file path
        /// </summary>
        /// <value>Wildcards are supported.  If used, all files matching the wildcard will be included as long as
        /// their extension is one of the following: .exe, .dll, .winmd, .*proj, .sln.</value>
        [Category("File"), Description("The path to the documentation source file(s)"),
          MergableProperty(false), Editor(typeof(FilePathObjectEditor), typeof(UITypeEditor)),
          RefreshProperties(RefreshProperties.All),
          FileDialog("Select the documentation source",
            "Documentation Sources (*.sln, *.*proj, *.dll, *.exe, *.winmd, *.xml)|*.sln;*.*proj;*.dll;*.exe;*.winmd;*.xml|" +
            "Assemblies and Comments Files (*.dll, *.exe, *.winmd, *.xml)|*.dll;*.exe;*.winmd;*.xml|" +
            "Library Files (*.dll, *.winmd)|*.dll;*.winmd|Executable Files (*.exe)|*.exe|" +
            "XML Comments Files (*.xml)|*.xml|" +
            "Visual Studio Solution Files (*.sln)|*.sln|" +
            "Visual Studio Project Files (*.*proj)|*.*proj|" +
            "All Files (*.*)|*.*", FileDialogType.FileOpen)]
        public FilePath SourceFile
        {
            get => sourceFile;
            set
            {
                if(value == null || value.Path.Length == 0)
                    throw new ArgumentException("A file path must be specified", nameof(value));

                sourceFile = value;
                sourceFile.PersistablePathChanging += (s, e) => this.OnPropertyChanged(nameof(SourceFile));
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// This is used to get or set whether subfolders are included when searching for files if the
        /// <see cref="SourceFile" /> value contains wildcards.
        /// </summary>
        /// <value>If set to true and the source file value contains wildcards, subfolders will be included.  If
        /// set to false, the default, or the source file value does not contain wildcards, only the top-level
        /// folder is included in the search.</value>
        [Category("File"), Description("True to include subfolders in wildcard searches or false to only " +
          "search the top-level folder."), DefaultValue(false)]
        public bool IncludeSubFolders
        {
            get => includeSubFolders;
            set
            {
                includeSubFolders = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// This returns a description of the entry suitable for display in a bound list control or property grid
        /// </summary>
        [Browsable(false)]
        public string SourceDescription
        {
            get
            {
                string path, config = null, subFolders = null, ext;
                char[] wildcards = new char[] { '*', '?' };

                path = Path.GetFileName(sourceFile.PersistablePath);

                if(path[0] == '*' && path[1] == '.')
                    path = sourceFile.PersistablePath;

                ext = Path.GetExtension(path).ToLowerInvariant();

                if((ext.IndexOfAny(wildcards) != -1 || ext == ".sln" ||
                  ext.EndsWith("proj", StringComparison.Ordinal)) &&
                  (!String.IsNullOrWhiteSpace(configuration) || !String.IsNullOrWhiteSpace(platform) ||
                  !String.IsNullOrWhiteSpace(targetFramework)))
                {
                    string[] parts = new[] { configuration, platform, targetFramework };

                    if(String.IsNullOrWhiteSpace(parts[0]) && !String.IsNullOrWhiteSpace(parts[1]))
                        parts[0] = "$(Configuration)";

                    if(!String.IsNullOrWhiteSpace(parts[0]) && String.IsNullOrWhiteSpace(parts[1]))
                        parts[1] = "$(Platform)";

                    config = " (" + String.Join("|", parts.Where(p => !String.IsNullOrWhiteSpace(p))) + ")";
                }

                if(path.IndexOfAny(wildcards) != -1 && this.IncludeSubFolders)
                    subFolders = " including subfolders";

                return String.Concat(path, config, subFolders);
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="filename">The filename of the documentation source</param>
        /// <param name="configuration">The configuration to use for projects</param>
        /// <param name="platform">The platform to use for projects</param>
        /// <param name="targetFramework">The target framework to use for projects</param>
        /// <param name="includeSubfolders">True to include subfolders, false to only search the top-level folder</param>
        /// <param name="basePathProvider">The base path provider</param>
        internal DocumentationSource(string filename, string configuration, string platform,
          string targetFramework, bool includeSubfolders, IBasePathProvider basePathProvider)
        {
            this.includeSubFolders = includeSubfolders;
            this.configuration = configuration;
            this.platform = platform;
            this.targetFramework = targetFramework;
            this.SourceFile = new FilePath(filename, basePathProvider);
        }
        #endregion

        #region INotifyPropertyChanged Members
        //=====================================================================

        /// <summary>
        /// The property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This raises the <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="propertyName">The property name that changed</param>
        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Equality and ToString methods
        //=====================================================================

        /// <summary>
        /// See if specified item equals this one
        /// </summary>
        /// <param name="obj">The object to compare to this one</param>
        /// <returns>True if equal, false if not</returns>
        /// <remarks>For documentation sources, equality is based solely on the <see cref="SourceFile" /> value.
        /// The configuration and platform settings are not considered.</remarks>
        public override bool Equals(object obj)
        {
            if(!(obj is DocumentationSource ds))
                return false;

            return (this == ds || this.SourceFile == ds.SourceFile);
        }

        /// <summary>
        /// Get a hash code for this item
        /// </summary>
        /// <returns>Returns the hash code for the assembly path and XML comments path.</returns>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        /// <summary>
        /// Return a string representation of the item
        /// </summary>
        /// <returns>Returns the assembly path and XML comments path separated by a comma.</returns>
        public override string ToString()
        {
            return this.SourceDescription;
        }
        #endregion

        #region Wildcard expansion properties and methods
        //=====================================================================

        /// <summary>
        /// This read-only property returns an enumerable list of assemblies based on the current settings
        /// </summary>
        /// <returns>An enumerable list of assemblies matching the <see cref="SourceFile"/> path.  Sub-folders
        /// are only included if <see cref="IncludeSubFolders"/> is set to true.</returns>
        public IEnumerable<string> Assemblies
        {
            get
            {
                SearchOption searchOpt = SearchOption.TopDirectoryOnly;
                string wildcard = sourceFile, dirName = Path.GetDirectoryName(wildcard);

                if(Directory.Exists(dirName))
                {
                    if(wildcard.IndexOfAny(new char[] { '*', '?' }) != -1 && includeSubFolders)
                        searchOpt = SearchOption.AllDirectories;

                    foreach(string filename in Directory.EnumerateFiles(dirName, Path.GetFileName(wildcard), searchOpt).Where(
                      f => f.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                      f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
                      f.EndsWith(".winmd", StringComparison.OrdinalIgnoreCase)))
                    {
                        yield return filename;
                    }
                }
            }
        }

        /// <summary>
        /// This read-only property returns an enumerable list of XML comments files based on the current settings
        /// </summary>
        /// <returns>An enumerable list of XML comments files matching the <see cref="SourceFile"/> path.
        /// Sub-folders are only included if <see cref="IncludeSubFolders"/> is set to true.</returns>
        public IEnumerable<string> CommentsFiles
        {
            get
            {
                SearchOption searchOpt = SearchOption.TopDirectoryOnly;
                string wildcard = sourceFile, dirName = Path.GetDirectoryName(wildcard);

                if(Directory.Exists(dirName))
                {
                    if(wildcard.IndexOfAny(new char[] { '*', '?' }) != -1 && includeSubFolders)
                        searchOpt = SearchOption.AllDirectories;

                    foreach(string filename in Directory.EnumerateFiles(dirName, Path.GetFileName(wildcard), searchOpt).Where(
                      f => f.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)))
                        yield return filename;
                }
            }
        }

        /// <summary>
        /// This returns an enumerable list of MSBuild project file configurations based on the current settings
        /// and the given configuration and platform.
        /// </summary>
        /// <param name="configurationName">The configuration to use</param>
        /// <param name="platformName">The platform to use</param>
        /// <returns>An enumerable list of project configurations matching the <see cref="SourceFile"/> path.
        /// Sub-folders are only included if <see cref="IncludeSubFolders"/> is set to true.  Any solution files
        /// (.sln) found are returned last, each followed by the projects extracted from them.</returns>
        public IEnumerable<ProjectFileConfiguration> Projects(string configurationName, string platformName)
        {
            List<string> solutions = new List<string>();
            SearchOption searchOpt = SearchOption.TopDirectoryOnly;
            string dirName, wildcard = sourceFile;

            dirName = Path.GetDirectoryName(wildcard);

            if(Directory.Exists(dirName))
            {
                if(wildcard.IndexOfAny(new char[] { '*', '?' }) != -1 && includeSubFolders)
                    searchOpt = SearchOption.AllDirectories;

                foreach(string filename in Directory.EnumerateFiles(dirName, Path.GetFileName(wildcard), searchOpt))
                {
                    if(filename.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
                        solutions.Add(filename);
                    else
                        if(filename.EndsWith("proj", StringComparison.OrdinalIgnoreCase))
                            yield return new ProjectFileConfiguration(filename);
                }

                // Add solutions last followed by the projects that they contain.  The caller can then set
                // solution specific values in each project related to the solution.
                foreach(string s in solutions)
                {
                    yield return new ProjectFileConfiguration(s);

                    foreach(var config in ExtractProjectsFromSolution(s, configurationName, platformName))
                        yield return config;
                }
            }
        }

        /// <summary>
        /// This is used to get a list of all projects in a solution file regardless of configuration and
        /// platform.
        /// </summary>
        /// <param name="solutionFile">The solution filename from which to get the project names</param>
        /// <returns>An enumerable list of the projects within the solution regardless of configuration or
        /// platform build combinations in which they are enabled.</returns>
        public static IEnumerable<string> ProjectsIn(string solutionFile)
        {
            string solutionContent;

            using(StreamReader sr = new StreamReader(solutionFile))
            {
                solutionContent = sr.ReadToEnd();
            }

            // Only add projects that are likely to contain assemblies
            MatchCollection projects = reExtractProjectGuids.Matches(solutionContent);

            foreach(Match solutionMatch in projects)
                yield return solutionMatch.Groups["Path"].Value;
        }

        /// <summary>
        /// Extract all project files from the given Visual Studio solution file
        /// </summary>
        /// <param name="solutionFile">The Visual Studio solution from which to extract the projects.</param>
        /// <param name="configuration">The configuration to use</param>
        /// <param name="platform">The platform to use</param>
        /// <returns>An enumerable list of project configurations that were extracted from the solution</returns>
        private static IEnumerable<ProjectFileConfiguration> ExtractProjectsFromSolution(string solutionFile,
          string configuration, string platform)
        {
            string solutionContent, folder = Path.GetDirectoryName(solutionFile);

            using(StreamReader sr = new StreamReader(solutionFile))
            {
                solutionContent = sr.ReadToEnd();
            }

            // Only add projects that are likely to contain assemblies
            MatchCollection projects = reExtractProjectGuids.Matches(solutionContent);

            foreach(Match solutionMatch in projects)
            {
                // See if the project is included in the build and get the configuration and platform
                var reIsInBuild = new Regex(String.Format(CultureInfo.InvariantCulture,
                    @"\{{{0}\}}\.{1}\|{2}\.Build\.0\s*=\s*(?<Configuration>.*?)\|(?<Platform>.*)",
                    solutionMatch.Groups["GUID"].Value, configuration, platform), RegexOptions.IgnoreCase);

                var buildMatch = reIsInBuild.Match(solutionContent);

                // If the platform is "AnyCPU" and it didn't match, try "Any CPU" (with a space)
                if(!buildMatch.Success && platform.Equals("AnyCPU", StringComparison.OrdinalIgnoreCase))
                {
                    reIsInBuild = new Regex(String.Format(CultureInfo.InvariantCulture,
                        @"\{{{0}\}}\.{1}\|Any CPU\.Build\.0\s*=\s*(?<Configuration>.*?)\|(?<Platform>.*)",
                        solutionMatch.Groups["GUID"].Value, configuration), RegexOptions.IgnoreCase);

                    buildMatch = reIsInBuild.Match(solutionContent);
                }

                if(buildMatch.Success)
                    yield return new ProjectFileConfiguration(Path.Combine(folder, solutionMatch.Groups["Path"].Value))
                    {
                        Configuration = buildMatch.Groups["Configuration"].Value.Trim(),
                        Platform = buildMatch.Groups["Platform"].Value.Trim()
                    };
            }
        }
        #endregion
    }
}
