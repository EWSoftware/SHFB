//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : DocumentationSourceWrapper.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/23/2021
// Note    : Copyright 2021, Eric Woodruff, All rights reserved
//
// This file contains the class used to wrap a documentation source so that it can be edited in the project
// explorer tool window's property pane.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/23/2021  EFW  Created the code
//===============================================================================================================

using System.ComponentModel;
using System.Drawing.Design;

using Sandcastle.Platform.Windows.Design;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This is used to wrap a documentation source so that it can be edited in the project explorer tool
    /// window's property pane.
    /// </summary>
    public class DocumentationSourceWrapper
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the underlying documentation source
        /// </summary>
        [Browsable(false)]
        public DocumentationSource DocumentationSource { get; }

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
            get => this.DocumentationSource.Configuration;
            set => this.DocumentationSource.Configuration = value;
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
            get => this.DocumentationSource.Platform;
            set => this.DocumentationSource.Platform = value;
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
            get => this.DocumentationSource.TargetFramework;
            set => this.DocumentationSource.TargetFramework = value;
        }

        /// <summary>
        /// This is used to set or get the documentation source file path
        /// </summary>
        /// <value>Wildcards are supported.  If used, all files matching the wildcard will be included as long as
        /// their extension is one of the following: .exe, .dll, .winmd, .*proj, .sln.</value>
        [Category("File"), Description("The path to the documentation source file(s)"),
          MergableProperty(false),
          RefreshProperties(RefreshProperties.All),
          Editor(typeof(FilePathObjectEditor), typeof(UITypeEditor)),
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
            get => this.DocumentationSource.SourceFile;
            set => this.DocumentationSource.SourceFile = value;
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
            get => this.DocumentationSource.IncludeSubFolders;
            set => this.DocumentationSource.IncludeSubFolders = value;
        }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ds">The documentation source to wrap</param>
        public DocumentationSourceWrapper(DocumentationSource ds)
        {
            this.DocumentationSource = ds;
        }
        #endregion
    }
}
