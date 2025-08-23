//===============================================================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : DocumentationSource.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/22/2025
// Note    : Copyright 2006-2025, Eric Woodruff, All rights reserved
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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using Sandcastle.Core;
using Sandcastle.Core.Project;

namespace SandcastleBuilder.MSBuild.HelpProject
{
    /// <summary>
    /// This represents an assembly, an XML comments file, a Visual Studio managed code project (C#, VB.NET,
    /// etc.), or a Visual Studio solution containing one or more managed code projects from which information
    /// is obtained to build a help file.
    /// </summary>
    /// <remarks>Wildcards are supported in the <see cref="SourceFile"/> property.</remarks>
    [DefaultProperty("SourceFile")]
    public class DocumentationSource : IDocumentationSource, INotifyPropertyChanged
    {
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
            get;
            set
            {
                field = (value ?? String.Empty).Trim();
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
            get;
            set
            {
                field = (value ?? String.Empty).Trim();
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
            get;
            set
            {
                field = (value ?? String.Empty).Trim();
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// This is used to set or get the documentation source file path
        /// </summary>
        /// <value>Wildcards are supported.  If used, all files matching the wildcard will be included as long as
        /// their extension is one of the following: .exe, .dll, .winmd, .*proj, .sln.</value>
        [Category("File"), Description("The path to the documentation source file(s)"), MergableProperty(false),
          RefreshProperties(RefreshProperties.All)]
        public FilePath SourceFile
        {
            get;
            set
            {
                if(value == null || value.Path.Length == 0)
                    throw new ArgumentException("A file path must be specified", nameof(value));

                field = value;
                field.PersistablePathChanging += (s, e) => this.OnPropertyChanged(nameof(SourceFile));
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
            get;
            set
            {
                field = value;
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

                path = Path.GetFileName(this.SourceFile.PersistablePath);

                if(path[0] == '*' && path[1] == '.')
                    path = this.SourceFile.PersistablePath;

                ext = Path.GetExtension(path).ToLowerInvariant();

                if((ext.IndexOfAny(FilePath.Wildcards) != -1 || ext == ".sln" || ext == ".slnx" ||
                  ext.EndsWith("proj", StringComparison.Ordinal)) &&
                  (!String.IsNullOrWhiteSpace(this.Configuration) || !String.IsNullOrWhiteSpace(this.Platform) ||
                  !String.IsNullOrWhiteSpace(this.TargetFramework)))
                {
                    string[] parts = [this.Configuration, this.Platform, this.TargetFramework];

                    if(String.IsNullOrWhiteSpace(parts[0]) && !String.IsNullOrWhiteSpace(parts[1]))
                        parts[0] = "$(Configuration)";

                    if(!String.IsNullOrWhiteSpace(parts[0]) && String.IsNullOrWhiteSpace(parts[1]))
                        parts[1] = "$(Platform)";

                    config = " (" + String.Join("|", parts.Where(p => !String.IsNullOrWhiteSpace(p))) + ")";
                }

                if(path.IndexOfAny(FilePath.Wildcards) != -1 && this.IncludeSubFolders)
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
            this.IncludeSubFolders = includeSubfolders;
            this.Configuration = configuration;
            this.Platform = platform;
            this.TargetFramework = targetFramework;
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
            return obj is DocumentationSource ds && (this == ds || this.SourceFile == ds.SourceFile);
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
                string wildcard = this.SourceFile, dirName = Path.GetDirectoryName(wildcard);

                if(Directory.Exists(dirName))
                {
                    if(wildcard.IndexOfAny(FilePath.Wildcards) != -1 && this.IncludeSubFolders)
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
                string wildcard = this.SourceFile, dirName = Path.GetDirectoryName(wildcard);

                if(Directory.Exists(dirName))
                {
                    if(wildcard.IndexOfAny(FilePath.Wildcards) != -1 && this.IncludeSubFolders)
                        searchOpt = SearchOption.AllDirectories;

                    foreach(string filename in Directory.EnumerateFiles(dirName, Path.GetFileName(wildcard), searchOpt).Where(
                      f => f.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)))
                    {
                        yield return filename;
                    }
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
            List<string> solutions = [];
            SearchOption searchOpt = SearchOption.TopDirectoryOnly;
            string dirName, wildcard = this.SourceFile;

            if(configurationName == null)
                throw new ArgumentNullException(nameof(configurationName));

            if(platformName == null)
                throw new ArgumentNullException(nameof(platformName));

            dirName = Path.GetDirectoryName(wildcard);

            if(Directory.Exists(dirName))
            {
                if(wildcard.IndexOfAny(FilePath.Wildcards) != -1 && this.IncludeSubFolders)
                    searchOpt = SearchOption.AllDirectories;

                foreach(string filename in Directory.EnumerateFiles(dirName, Path.GetFileName(wildcard), searchOpt))
                {
                    if(filename.EndsWith(".sln", StringComparison.OrdinalIgnoreCase) ||
                      filename.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase))
                    {
                        solutions.Add(filename);
                    }
                    else
                    {
                        if(filename.EndsWith("proj", StringComparison.OrdinalIgnoreCase))
                            yield return new ProjectFileConfiguration(filename);
                    }
                }

                // Add solutions last followed by the projects that they contain.  The caller can then set
                // solution specific values in each project related to the solution.
                foreach(string s in solutions)
                {
                    yield return new ProjectFileConfiguration(s);

                    var sf = new SolutionFile(s);

                    foreach(var p in sf.EnumerateProjectFiles())
                    {
                        if(sf.WillBuild(p, configurationName, platformName))
                            yield return p;
                    }
                }
            }
        }
        #endregion
    }
}
