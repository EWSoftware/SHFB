//=============================================================================
// System  : Sandcastle Help File Builder Package
// File    : DocumentationSourceNodeProperties.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/14/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that exposes the properties for the
// DocumentationSourceNode object.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.0  03/30/2011  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;

using SandcastleBuilder.Package.Properties;
using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Package.Nodes
{
    /// <summary>
    /// This is used to expose the properties for <see cref="DocumentationSourceNode" /> objects.
    /// </summary>
    [CLSCompliant(false), ComVisible(true), Guid("246CA3DC-D2F8-4380-8511-CE46632574F2")]
    public sealed class DocumentationSourceNodeProperties : NodeProperties, IBasePathProvider
    {
        #region Private data members
        //=====================================================================

        private XElement documentationSource;
        private FilePath sourceFile;
        private string configuration, platform;
        private bool includeSubFolders;

        // These are used to convert MSBuild variable references to normal
        // environment variable references.
        private static Regex reMSBuildVar = new Regex(@"\$\((.*?)\)");

        private MatchEvaluator buildVarMatchEval;
        #endregion

        #region Properties
        //=====================================================================

        // NOTE: If you apply a type converter attribute to any property, it must be of the type
        //       PropertyPageTypeConverterAttribute or it will be ignored.

        /// <summary>
        /// This is used to get or set the project configuration to use when
        /// the source path refers to a Visual Studio solution or project.
        /// </summary>
        /// <value>If not set, the configuration value from the owning help
        /// file project will be used.  This will be ignored for assembly
        /// and XML comments file entries.</value>
        [Category("Project"), Description("The configuration to use for a solution or project " +
          "documentation source.  If blank, the configuration from the owning help file project " +
          "will be used."), DefaultValue(null), DisplayName("Configuration")]
        public string Configuration
        {
            get { return configuration; }
            set
            {
                this.CheckProjectIsEditable();

                if(value != null)
                    value = value.Trim();

                configuration = value;
                this.StoreDocumentationSourceChanges();
            }
        }

        /// <summary>
        /// This is used to get or set the project platform to use when the
        /// source path refers to a Visual Studio solution or project.
        /// </summary>
        /// <value>If not set, the platform value from the owning help file
        /// project will be used.  This will be ignored for assembly and XML
        /// comments file entries.</value>
        [Category("Project"), Description("The platform to use for a solution or project documentation " +
          "source.  If blank, the platform from the owning help file project will be used."),
          DefaultValue(null), DisplayName("Platform")]
        public string Platform
        {
            get { return platform; }
            set
            {
                this.CheckProjectIsEditable();

                if(value != null)
                    value = value.Trim();

                platform = value;
                this.StoreDocumentationSourceChanges();
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
          RefreshProperties(RefreshProperties.All), PropertyPageTypeConverter(typeof(FilePathTypeConverter)),
          FileDialog("Select the documentation source",
            "Documentation Sources (*.sln, *.*proj, *.dll, *.exe, *.xml)|*.sln;*.*proj;*.dll;*.exe;*.xml|" +
            "Assemblies and Comments Files (*.dll, *.exe, *.xml)|*.dll;*.exe;*.xml|" +
            "Library Files (*.dll)|*.dll|Executable Files (*.exe)|*.exe|" +
            "XML Comments Files (*.xml)|*.xml|" +
            "Visual Studio Solution Files (*.sln)|*.sln|" +
            "Visual Studio Project Files (*.*proj)|*.*proj|" +
            "All Files (*.*)|*.*", FileDialogType.FileOpen), DisplayName("Source File")]
        public FilePath SourceFile
        {
            get { return sourceFile; }
            set
            {
                if(value == null || value.Path.Length == 0)
                    throw new ArgumentException("A file path must be specified", "value");

                this.CheckProjectIsEditable();

                sourceFile = value;
                sourceFile.PersistablePathChanging += sourceFile_PersistablePathChanging;
                sourceFile.PersistablePathChanged += sourceFile_PersistablePathChanged;

                this.StoreDocumentationSourceChanges();
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
          DefaultValue(false), DisplayName("Include Subfolders")]
        public bool IncludeSubFolders
        {
            get { return includeSubFolders; }
            set
            {
                this.CheckProjectIsEditable();
                includeSubFolders = value;

                this.StoreDocumentationSourceChanges();
            }
        }
        #endregion

        #region Private helper methods
        //=====================================================================

        /// <summary>
        /// This is used to see if the project is editable
        /// </summary>
        private void CheckProjectIsEditable()
        {
            if(!base.Node.ProjectMgr.QueryEditProjectFile(false))
                throw Marshal.GetExceptionForHR(VSConstants.OLE_E_PROMPTSAVECANCELLED);
        }

        /// <summary>
        /// This is used to push the changes into the DocumentationSources project property
        /// </summary>
        private void StoreDocumentationSourceChanges()
        {
            documentationSource.Attribute("sourceFile").Value = sourceFile.PersistablePath;

            if(documentationSource.Attribute("configuration") != null)
            {
                if(!String.IsNullOrEmpty(configuration))
                    documentationSource.Attribute("configuration").Value = configuration;
                else
                    documentationSource.Attributes("configuration").Remove();
            }
            else
                if(!String.IsNullOrEmpty(configuration))
                    documentationSource.Add(new XAttribute("configuration", configuration));

            if(documentationSource.Attribute("platform") != null)
            {
                if(!String.IsNullOrEmpty(platform))
                    documentationSource.Attribute("platform").Value = platform;
                else
                    documentationSource.Attributes("platform").Remove();
            }
            else
                if(!String.IsNullOrEmpty(platform))
                    documentationSource.Add(new XAttribute("platform", platform));

            if(documentationSource.Attribute("subFolders") != null)
            {
                if(includeSubFolders)
                    documentationSource.Attribute("subFolders").Value = "true";
                else
                    documentationSource.Attributes("subFolders").Remove();
            }
            else
                if(includeSubFolders)
                    documentationSource.Add(new XAttribute("subFolders", "true"));

            ((DocumentationSourcesContainerNode)base.Node.Parent).StoreDocumentationSources();

            this.Node.ReDraw(UIHierarchyElement.Caption);
        }

        /// <summary>
        /// This is used to handle changes in the <see cref="FilePath" /> properties such that the
        /// source path gets stored in the project file.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void sourceFile_PersistablePathChanging(object sender, EventArgs e)
        {
            this.CheckProjectIsEditable();
        }

        /// <summary>
        /// This is used to handle changes in the <see cref="FilePath" /> properties such that the
        /// source path gets stored in the project file.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void sourceFile_PersistablePathChanged(object sender, EventArgs e)
        {
            this.StoreDocumentationSourceChanges();
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">The node that contains the properties to expose
        /// via the Property Browser.</param>
        public DocumentationSourceNodeProperties(DocumentationSourceNode node) : base(node)
        {
            buildVarMatchEval = new MatchEvaluator(this.OnBuildVarMatch);

            documentationSource = node.DocumentationSource;

            sourceFile = new FilePath(documentationSource.Attribute("sourceFile").Value, this);
            sourceFile.PersistablePathChanging += sourceFile_PersistablePathChanging;
            sourceFile.PersistablePathChanged += sourceFile_PersistablePathChanged;

            if(documentationSource.Attribute("configuration") != null)
                configuration = documentationSource.Attribute("configuration").Value;

            if(documentationSource.Attribute("platform") != null)
                platform = documentationSource.Attribute("platform").Value;

            if(documentationSource.Attribute("subFolders") != null)
                includeSubFolders = documentationSource.Attribute("subFolders").Value.Equals(
                    "true", StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// Get the class name for the node
        /// </summary>
        /// <returns>The documentation source class name</returns>
        public override string GetClassName()
        {
            return Resources.DocSourceProperties;
        }
        #endregion

        #region IBasePathProvider Members
        //=====================================================================

        /// <inheritdoc />
        [Browsable(false)]
        string IBasePathProvider.BasePath
        {
            get
            {
                return Path.GetDirectoryName(this.Node.ProjectMgr.BuildProject.FullPath);
            }
        }

        /// <summary>
        /// This method resolves any MSBuild environment variables in the
        /// path objects.
        /// </summary>
        /// <param name="path">The path to use</param>
        /// <returns>A copy of the path after performing any custom resolutions</returns>
        string IBasePathProvider.ResolvePath(string path)
        {
            return reMSBuildVar.Replace(path, buildVarMatchEval);
        }

        /// <summary>
        /// Resolve references to MSBuild variables in a path value
        /// </summary>
        /// <param name="match">The match that was found</param>
        /// <returns>The string to use as the replacement</returns>
        private string OnBuildVarMatch(Match match)
        {
            return (this.Node.ProjectMgr.GetProjectProperty(match.Groups[1].Value) ?? String.Empty);
        }
        #endregion
    }
}
