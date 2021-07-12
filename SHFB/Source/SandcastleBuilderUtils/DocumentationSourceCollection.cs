//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : DocumentationSourceCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/29/2021
// Note    : Copyright 2006-2021, Eric Woodruff, All rights reserved
//
// This file contains a collection class used to hold the documentation sources
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/03/2006  EFW  Created the code
// 05/11/2007  EFW  Added the ability to sort the collection
// 11/10/2007  EFW  Moved CommentFileList to XmlCommentsFileCollection
// 04/16/2008  EFW  Added support for wildcards
// 06/23/2008  EFW  Rewrote to support MSBuild project format
//===============================================================================================================

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This collection class is used to hold the documentation sources and can be used for editing them
    /// </summary>
    /// <remarks>A documentation source is an assembly, an XML comments file, a Visual Studio managed code
    /// project (C#, VB.NET, etc.), or a Visual Studio solution containing one or more managed code projects from
    /// which information is obtained to build a help file.</remarks>
    public class DocumentationSourceCollection : BindingList<DocumentationSource>
    {
        #region Private data members
        //=====================================================================

        private readonly SandcastleProject projectFile;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">The project that owns the collection</param>
        internal DocumentationSourceCollection(SandcastleProject project)
        {
            projectFile = project;

            var docSourcesProperty = project.MSBuildProject.GetProperty("DocumentationSources");

            if(docSourcesProperty != null && !String.IsNullOrWhiteSpace(docSourcesProperty.UnevaluatedValue))
            {
                // The paths in the elements may contain variable references so use final values if
                // requested.
                this.FromXml(project.UsingFinalValues ? docSourcesProperty.EvaluatedValue :
                    docSourcesProperty.UnevaluatedValue);
            }
        }
        #endregion

        #region Read/write doc sources from/to XML
        //=====================================================================

        /// <summary>
        /// This is used to load existing documentation sources from the project file
        /// </summary>
        /// <param name="docSources">The documentation source items</param>
        /// <remarks>The information is stored as an XML fragment</remarks>
        private void FromXml(string docSources)
        {
            string sourceFile, config, platform, targetFramework;
            bool subFolders;

            using(var xr = new XmlTextReader(docSources, XmlNodeType.Element,
              new XmlParserContext(null, null, null, XmlSpace.Default)))
            {
                xr.Namespaces = false;
                xr.MoveToContent();

                while(!xr.EOF)
                {
                    if(xr.NodeType == XmlNodeType.Element && xr.Name == "DocumentationSource")
                    {
                        sourceFile = xr.GetAttribute("sourceFile");
                        config = xr.GetAttribute("configuration");
                        platform = xr.GetAttribute("platform");
                        targetFramework = xr.GetAttribute("targetFramework");
                        subFolders = Convert.ToBoolean(xr.GetAttribute("subFolders"), CultureInfo.InvariantCulture);
                        this.Add(sourceFile, config, platform, targetFramework, subFolders);
                    }

                    xr.Read();
                }
            }
        }

        /// <summary>
        /// This is used to write the documentation source info to an XML fragment ready for storing in the
        /// project file.
        /// </summary>
        /// <returns>The XML fragment containing the documentation sources</returns>
        private string ToXml()
        {
            using(var ms = new MemoryStream(10240))
            {
                using(var xw = new XmlTextWriter(ms, new UTF8Encoding(false)))
                {
                    xw.Formatting = Formatting.Indented;

                    foreach(DocumentationSource ds in this)
                    {
                        xw.WriteStartElement("DocumentationSource");
                        xw.WriteAttributeString("sourceFile", ds.SourceFile.PersistablePath);

                        if(!String.IsNullOrWhiteSpace(ds.Configuration))
                            xw.WriteAttributeString("configuration", ds.Configuration);

                        if(!String.IsNullOrWhiteSpace(ds.Platform))
                            xw.WriteAttributeString("platform", ds.Platform);

                        if(!String.IsNullOrWhiteSpace(ds.TargetFramework))
                            xw.WriteAttributeString("targetFramework", ds.TargetFramework);

                        if(ds.IncludeSubFolders)
                            xw.WriteAttributeString("subFolders", ds.IncludeSubFolders.ToString());

                        xw.WriteEndElement();
                    }

                    xw.Flush();
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Add a new item to the collection
        /// </summary>
        /// <param name="filename">The filename to add</param>
        /// <param name="config">The configuration to use for projects</param>
        /// <param name="platform">The platform to use for projects</param>
        /// <param name="targetFramework">The target framework to use for projects</param>
        /// <param name="subFolders">True to include subfolders, false to only search the top-level folder</param>
        /// <returns>The <see cref="DocumentationSource" /> added to the project or the existing item if the
        /// filename already exists in the collection.</returns>
        /// <remarks>The <see cref="DocumentationSource" /> constructor is internal so that we control creation
        /// of the items and can associate them with the project.</remarks>
        public DocumentationSource Add(string filename, string config, string platform, string targetFramework,
          bool subFolders)
        {
            DocumentationSource item;

            // Make the path relative to the project if possible
            if(Path.IsPathRooted(filename))
                filename = FilePath.AbsoluteToRelativePath(projectFile.BasePath, filename);

            item = new DocumentationSource(filename, config, platform, targetFramework, subFolders, projectFile);

            if(!this.Contains(item))
                this.Add(item);
            else
                item = base[this.IndexOf(item)];

            return item;
        }

        /// <summary>
        /// Save the documentation source collection to the associated project
        /// </summary>
        public void SaveToProject()
        {
            projectFile.MSBuildProject.SetProperty("DocumentationSources", this.ToXml());
        }
        #endregion
    }
}
