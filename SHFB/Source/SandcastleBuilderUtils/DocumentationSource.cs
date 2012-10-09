//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : DocumentationSource.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/09/2011
// Note    : Copyright 2006-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing a documentation source such as an
// assembly, an XML comments file, a solution, or a project.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  08/02/2006  EFW  Created the code
// 1.3.2.0  11/10/2006  EFW  Added CommentsOnly property.
// 1.3.4.0  12/31/2006  EFW  Converted path properties to FilePath objects
// 1.6.0.7  04/16/2008  EFW  Added support for wildcards
// 1.8.0.0  06/23/2008  EFW  Rewrote to support the MSBuild project format
// 1.8.0.4  06/05/2010  EFW  Added support for getting build include status and
//                           configuration settings from the solution file.
//=============================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This represents an assembly, an XML comments file, a Visual Studio
    /// Solution (C#, VB.NET, or J#), or a Visual Studio solution containing
    /// one or more C#, VB.NET or J# projects to use for building a help file.
    /// </summary>
    /// <remarks>Wildcards are supported in the <see cref="SourceFile"/>
    /// property.</remarks>
    [DefaultProperty("SourceFile")]
    public class DocumentationSource : PropertyBasedCollectionItem
    {
        #region Private data members
        //=====================================================================

        private FilePath sourceFile;
        private string configuration, platform;
        private bool includeSubFolders;

        // Regular expression used to parse solution files
        private static Regex reExtractProjectGuids = new Regex(
            "^Project\\(\"\\{(" +
            "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC|" +   // C#
            "F184B08F-C81C-45F6-A57F-5ABD9991F28F|" +   // VB.NET
            "8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942|" +   // C++
            "F2A71F9B-5D33-465A-A702-920D77279786|" +   // F#
            "E6FDF86B-F3D1-11D4-8576-0002A516ECE8" +    // J#
            ")\\}\"\\) = \".*?\", \"(?!http)" +
            "(?<Path>.*?proj)\", \"\\{(?<GUID>.*?)\\}\"", RegexOptions.Multiline);
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the project configuration to use when
        /// the source path refers to a Visual Studio solution or project.
        /// </summary>
        /// <value>If not set, the configuration value from the owning help
        /// file project will be used.  This will be ignored for assembly
        /// and XML comments file entries.</value>
        [Category("Project"), Description("The configuration to use for a " +
          "solution or project documentation source.  If blank, the " +
          "configuration from the owning help file project will be used."),
          DefaultValue(null)]
        public string Configuration
        {
            get { return configuration; }
            set
            {
                base.CheckProjectIsEditable();

                if(value != null)
                    value = value.Trim();

                configuration = value;
            }
        }

        /// <summary>
        /// This is used to get or set the project platform to use when the
        /// source path refers to a Visual Studio solution or project.
        /// </summary>
        /// <value>If not set, the platform value from the owning help file
        /// project will be used.  This will be ignored for assembly and XML
        /// comments file entries.</value>
        [Category("Project"), Description("The platform to use for a " +
          "solution or project documentation source.  If blank, the " +
          "platform from the owning help file project will be used."),
          DefaultValue(null)]
        public string Platform
        {
            get { return platform; }
            set
            {
                base.CheckProjectIsEditable();

                if(value != null)
                    value = value.Trim();

                platform = value;
            }
        }

        /// <summary>
        /// This is used to set or get the documentation source file path
        /// </summary>
        /// <value>Wildcards are supported.  If used, all files matching
        /// the wildcard will be included as long as their extension is one of
        /// the following: .exe, .dll, .*proj, .sln.</value>
        [Category("File"), Description("The path to the documentation source file(s)"),
          MergableProperty(false), Editor(typeof(FilePathObjectEditor), typeof(UITypeEditor)),
          RefreshProperties(RefreshProperties.All),
          FileDialog("Select the documentation source",
            "Documentation Sources (*.sln, *.*proj, *.dll, *.exe, *.xml)|*.sln;*.*proj;*.dll;*.exe;*.xml|" +
            "Assemblies and Comments Files (*.dll, *.exe, *.xml)|*.dll;*.exe;*.xml|" +
            "Library Files (*.dll)|*.dll|Executable Files (*.exe)|*.exe|" +
            "XML Comments Files (*.xml)|*.xml|" +
            "Visual Studio Solution Files (*.sln)|*.sln|" +
            "Visual Studio Project Files (*.*proj)|*.*proj|" +
            "All Files (*.*)|*.*", FileDialogType.FileOpen)]
        public FilePath SourceFile
        {
            get { return sourceFile; }
            set
            {
                if(value == null || value.Path.Length == 0)
                    throw new ArgumentException("A file path must be specified",
                        "value");

                base.CheckProjectIsEditable();

                sourceFile = value;
                sourceFile.PersistablePathChanging += new EventHandler(
                    sourceFile_PersistablePathChanging);
            }
        }

        /// <summary>
        /// This is used to get or set whether subfolders are included when
        /// searching for files if the <see cref="SourceFile" /> value
        /// contains wildcards.
        /// </summary>
        /// <value>If set to true and the source file value contains wildcards,
        /// subfolders will be included.  If set to false, the default, or the
        /// source file value does not contain wildcards, only the top-level
        /// folder is included in the search.</value>
        [Category("File"), Description("True to include subfolders in " +
          "wildcard searches or false to only search the top-level folder."),
          DefaultValue(false)]
        public bool IncludeSubFolders
        {
            get { return includeSubFolders; }
            set
            {
                base.CheckProjectIsEditable();
                includeSubFolders = value;
            }
        }

        /// <summary>
        /// This returns a description of the entry suitable for display in a
        /// bound list control.
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

                ext = Path.GetExtension(path).ToLower(CultureInfo.InvariantCulture);

                if((ext.IndexOfAny(wildcards) != -1 || ext == ".sln" ||
                  ext.EndsWith("proj", StringComparison.Ordinal)) &&
                  (!String.IsNullOrEmpty(configuration) ||
                  !String.IsNullOrEmpty(platform)))
                    config = String.Format(CultureInfo.InvariantCulture, " ({0}|{1})",
                        (String.IsNullOrEmpty(configuration)) ? "$(Configuration)" : configuration,
                        (String.IsNullOrEmpty(platform)) ? "$(Platform)" : platform);

                if(path.IndexOfAny(wildcards) != -1 && includeSubFolders)
                    subFolders = " including subfolders";

                return String.Concat(path, config, subFolders);
            }
        }
        #endregion

        #region Private helper methods
        //=====================================================================

        /// <summary>
        /// This is used to handle changes in the <see cref="FilePath" />
        /// properties such that the source path gets stored in the project
        /// file.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void sourceFile_PersistablePathChanging(object sender, EventArgs e)
        {
            base.CheckProjectIsEditable();
        }

        /// <summary>
        /// Extract all project files from the given Visual Studio solution file
        /// </summary>
        /// <param name="solutionFile">The Visual Studio solution from which to
        /// extract the projects.</param>
        /// <param name="configuration">The configuration to use</param>
        /// <param name="platform">The platform to use</param>
        /// <param name="projectFiles">The collection used to return the
        /// extracted projects.</param>
        private static void ExtractProjectsFromSolution(string solutionFile,
          string configuration, string platform, Collection<ProjectFileConfiguration> projectFiles)
        {
            Regex reIsInBuild;
            Match buildMatch;
            string solutionContent, folder = Path.GetDirectoryName(solutionFile);

            using(StreamReader sr = new StreamReader(solutionFile))
            {
                solutionContent = sr.ReadToEnd();
                sr.Close();
            }

            // Only add projects that are likely to contain assemblies
            MatchCollection projects = reExtractProjectGuids.Matches(solutionContent);

            foreach(Match solutionMatch in projects)
            {
                // See if the project is included in the build and get the configuration and platform
                reIsInBuild = new Regex(String.Format(CultureInfo.InvariantCulture,
                    @"\{{{0}\}}\.{1}\|{2}\.Build\.0\s*=\s*(?<Configuration>.*?)\|(?<Platform>.*)",
                    solutionMatch.Groups["GUID"].Value, configuration, platform), RegexOptions.IgnoreCase);

                buildMatch = reIsInBuild.Match(solutionContent);

                // If the platform is "AnyCPU" and it didn't match, try "Any CPU" (with a space)
                if(!buildMatch.Success && platform.Equals("AnyCPU", StringComparison.OrdinalIgnoreCase))
                {
                    reIsInBuild = new Regex(String.Format(CultureInfo.InvariantCulture,
                        @"\{{{0}\}}\.{1}\|Any CPU\.Build\.0\s*=\s*(?<Configuration>.*?)\|(?<Platform>.*)",
                        solutionMatch.Groups["GUID"].Value, configuration), RegexOptions.IgnoreCase);

                    buildMatch = reIsInBuild.Match(solutionContent);
                }

                if(buildMatch.Success)
                    projectFiles.Add(new ProjectFileConfiguration(Path.Combine(folder,
                      solutionMatch.Groups["Path"].Value))
                    {
                        Configuration = buildMatch.Groups["Configuration"].Value.Trim(),
                        Platform = buildMatch.Groups["Platform"].Value.Trim()
                    });
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="filename">The filename of the documentation source</param>
        /// <param name="projConfig">The configuration to use for projects</param>
        /// <param name="projPlatform">The platform to use for projects</param>
        /// <param name="subFolders">True to include subfolders, false to
        /// only search the top-level folder.</param>
        /// <param name="project">The owning project</param>
        internal DocumentationSource(string filename, string projConfig,
          string projPlatform, bool subFolders, SandcastleProject project) :
          base(project)
        {
            sourceFile = new FilePath(filename, project);
            sourceFile.PersistablePathChanging += sourceFile_PersistablePathChanging;
            configuration = projConfig;
            platform = projPlatform;
            includeSubFolders = subFolders;
        }
        #endregion

        #region Equality and ToString methods
        //=====================================================================

        /// <summary>
        /// See if specified item equals this one
        /// </summary>
        /// <param name="obj">The object to compare to this one</param>
        /// <returns>True if equal, false if not</returns>
        /// <remarks>For documentation sources, equality is based solely on
        /// the <see cref="SourceFile" /> value.  The configuration and
        /// platform settings are not considered.</remarks>
        public override bool Equals(object obj)
        {
            DocumentationSource ds = obj as DocumentationSource;

            if(ds == null)
                return false;

            return (this == ds || this.SourceFile == ds.SourceFile);
        }

        /// <summary>
        /// Get a hash code for this item
        /// </summary>
        /// <returns>Returns the hash code for the assembly path and
        /// XML comments path.</returns>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        /// <summary>
        /// Return a string representation of the item
        /// </summary>
        /// <returns>Returns the assembly path and XML comments path separated
        /// by a comma.</returns>
        public override string ToString()
        {
            return this.SourceDescription;
        }
        #endregion

        #region Wildcard expansion methods
        //=====================================================================

        /// <summary>
        /// This returns a collection of assemblies based on the specified
        /// wildcard.
        /// </summary>
        /// <param name="wildcard">The wildcard to use to find assemblies.</param>
        /// <param name="includeSubfolders">If true and the wildcard parameter
        /// includes wildcard characters, subfolders will be searched as well.
        /// If not, only the top-level folder is searched.</param>
        /// <returns>A list of assemblies matching the wildcard</returns>
        public static Collection<string> Assemblies(string wildcard, bool includeSubfolders)
        {
            Collection<string> assemblies = new Collection<string>();
            SearchOption searchOpt = SearchOption.TopDirectoryOnly;
            string dirName;

            dirName = Path.GetDirectoryName(wildcard);

            if(Directory.Exists(dirName))
            {
                if(wildcard.IndexOfAny(new char[] { '*', '?' }) != -1 && includeSubfolders)
                    searchOpt = SearchOption.AllDirectories;

                foreach(string f in Directory.EnumerateFiles(dirName,
                  Path.GetFileName(wildcard), searchOpt).Where(
                  f => f.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                  f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)))
                    assemblies.Add(f);
            }

            return assemblies;
        }

        /// <summary>
        /// This returns a collection of XML comments files based on the
        /// specified wildcard.
        /// </summary>
        /// <param name="wildcard">The wildcard to use to find comments
        /// files.</param>
        /// <param name="includeSubfolders">If true and the wildcard parameter
        /// includes wildcard characters, subfolders will be searched as well.
        /// If not, only the top-level folder is searched.</param>
        /// <returns>A list of XML comments files matching the wildcard</returns>
        public static Collection<string> CommentsFiles(string wildcard, bool includeSubfolders)
        {
            Collection<string> comments = new Collection<string>();
            SearchOption searchOpt = SearchOption.TopDirectoryOnly;
            string dirName;

            dirName = Path.GetDirectoryName(wildcard);

            if(Directory.Exists(dirName))
            {
                if(wildcard.IndexOfAny(new char[] { '*', '?' }) != -1 && includeSubfolders)
                    searchOpt = SearchOption.AllDirectories;

                foreach(string f in Directory.EnumerateFiles(dirName,
                  Path.GetFileName(wildcard), searchOpt).Where(f => f.EndsWith(".xml",
                  StringComparison.OrdinalIgnoreCase)))
                    comments.Add(f);
            }

            return comments;
        }

        /// <summary>
        /// This returns a collection of MSBuild project filenames based on the
        /// specified wildcard.
        /// </summary>
        /// <param name="wildcard">The wildcard to use to find solutions and
        /// projects.</param>
        /// <param name="includeSubfolders">If true and the wildcard parameter
        /// includes wildcard characters, subfolders will be searched as well.
        /// If not, only the top-level folder is searched.</param>
        /// <param name="configuration">The configuration to use</param>
        /// <param name="platform">The platform to use</param>
        /// <returns>A list of projects matching the wildcard.  Any solution
        /// files (.sln) found are returned last, each followed by the projects
        /// extracted from it.</returns>
        public static Collection<ProjectFileConfiguration> Projects(string wildcard,
          bool includeSubfolders, string configuration, string platform)
        {
            List<string> solutions = new List<string>();
            Collection<ProjectFileConfiguration> projects = new Collection<ProjectFileConfiguration>();
            SearchOption searchOpt = SearchOption.TopDirectoryOnly;
            string dirName;

            dirName = Path.GetDirectoryName(wildcard);

            if(Directory.Exists(dirName))
            {
                if(wildcard.IndexOfAny(new char[] { '*', '?' }) != -1 && includeSubfolders)
                    searchOpt = SearchOption.AllDirectories;

                foreach(string f in Directory.EnumerateFiles(dirName, Path.GetFileName(wildcard), searchOpt))
                {
                    if(f.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
                        solutions.Add(f);
                    else
                        if(f.EndsWith("proj", StringComparison.OrdinalIgnoreCase))
                            projects.Add(new ProjectFileConfiguration(f));
                }

                // Add solutions last followed by the projects that they contain.
                // The caller can then set solution specific values in each project
                // related to the solution.
                foreach(string s in solutions)
                {
                    projects.Add(new ProjectFileConfiguration(s));
                    ExtractProjectsFromSolution(s, configuration, platform, projects);
                }
            }

            return projects;
        }

        #endregion
    }
}
