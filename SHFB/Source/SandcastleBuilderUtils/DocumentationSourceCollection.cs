//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : DocumentationSourceCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/04/2008
// Note    : Copyright 2006-2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a collection class used to hold the documentation
// sources.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  08/03/2006  EFW  Created the code
// 1.4.0.2  05/11/2007  EFW  Added the ability to sort the collection
// 1.6.0.2  11/10/2007  EFW  Moved CommentFileList to XmlCommentsFileCollection
// 1.6.0.7  04/16/2008  EFW  Added support for wildcards
// 1.8.0.0  06/23/2008  EFW  Rewrote to support MSBuild project format
//=============================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This collection class is used to hold the documentation sources
    /// </summary>
    /// <remarks>A documentation source is an assembly, an XML comments file,
    /// a Visual Studio project (C#, VB.NET, or J#), or a Visual Studio
    /// solution containing one or more C#, VB.NET or J# projects.</remarks>
    public class DocumentationSourceCollection : BindingList<DocumentationSource>
    {
        #region Private data members
        //=====================================================================

        private SandcastleProject projectFile;
        private bool isDirty;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the dirty state of the collection
        /// </summary>
        public bool IsDirty
        {
            get
            {
                foreach(DocumentationSource ds in this)
                    if(ds.IsDirty)
                        return true;

                return isDirty;
            }
            set
            {
                foreach(DocumentationSource ds in this)
                    ds.IsDirty = value;

                isDirty = value;
            }
        }

        /// <summary>
        /// This read-only property returns a list of assemblies in the
        /// collection.
        /// </summary>
        public Collection<string> Assemblies
        {
            get
            {
                Collection<string> assemblies = new Collection<string>();

                foreach(DocumentationSource ds in this)
                    foreach(string file in DocumentationSource.Assemblies(
                      ds.SourceFile, ds.IncludeSubFolders))
                        assemblies.Add(file);

                return assemblies;
            }
        }

        /// <summary>
        /// This read-only property returns a list of XML comments files in the
        /// collection.
        /// </summary>
        public Collection<string> CommentsFiles
        {
            get
            {
                Collection<string> comments = new Collection<string>();

                foreach(DocumentationSource ds in this)
                    foreach(string file in DocumentationSource.CommentsFiles(
                      ds.SourceFile, ds.IncludeSubFolders))
                        comments.Add(file);

                return comments;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="project">The project that owns the collection</param>
        internal DocumentationSourceCollection(SandcastleProject project)
        {
            projectFile = project;
        }
        #endregion

        #region Sort the collection
        //=====================================================================

        /// <summary>
        /// This is used to sort the collection in ascending order.
        /// </summary>
        public void Sort()
        {
            ((List<DocumentationSource>)base.Items).Sort(
                delegate(DocumentationSource x, DocumentationSource y)
                {
                    return String.Compare(x.SourceDescription,
                        y.SourceDescription,
                        StringComparison.CurrentCultureIgnoreCase);
                });

            base.OnListChanged(new ListChangedEventArgs(
                ListChangedType.Reset, -1));
        }
        #endregion

        #region Read/write doc sources from/to XML
        //=====================================================================

        /// <summary>
        /// This is used to load existing documentation sources from the
        /// project file.
        /// </summary>
        /// <param name="docSources">The documentation source items</param>
        /// <remarks>The information is stored as an XML fragment</remarks>
        internal void FromXml(string docSources)
        {
            XmlTextReader xr = null;
            string sourceFile, config, platform;
            bool subFolders;

            try
            {
                xr = new XmlTextReader(docSources, XmlNodeType.Element,
                    new XmlParserContext(null, null, null, XmlSpace.Default));
                xr.Namespaces = false;
                xr.MoveToContent();

                while(!xr.EOF)
                {
                    if(xr.NodeType == XmlNodeType.Element &&
                      xr.Name == "DocumentationSource")
                    {
                        sourceFile = xr.GetAttribute("sourceFile");
                        config = xr.GetAttribute("configuration");
                        platform = xr.GetAttribute("platform");
                        subFolders = Convert.ToBoolean(xr.GetAttribute(
                            "subFolders"), CultureInfo.InvariantCulture);
                        this.Add(sourceFile, config, platform, subFolders);
                    }

                    xr.Read();
                }
            }
            finally
            {
                if(xr != null)
                    xr.Close();

                isDirty = false;
            }
        }

        /// <summary>
        /// This is used to write the documentation source info to an XML
        /// fragment ready for storing in the project file.
        /// </summary>
        /// <returns>The XML fragment containing the documentation sources</returns>
        internal string ToXml()
        {
            MemoryStream ms = new MemoryStream(10240);
            XmlTextWriter xw = null;

            try
            {
                xw = new XmlTextWriter(ms, new UTF8Encoding(false));
                xw.Formatting = Formatting.Indented;

                foreach(DocumentationSource ds in this)
                {
                    xw.WriteStartElement("DocumentationSource");
                    xw.WriteAttributeString("sourceFile",
                        ds.SourceFile.PersistablePath);

                    if(!String.IsNullOrEmpty(ds.Configuration))
                        xw.WriteAttributeString("configuration", ds.Configuration);

                    if(!String.IsNullOrEmpty(ds.Platform))
                        xw.WriteAttributeString("platform", ds.Platform);

                    if(ds.IncludeSubFolders)
                        xw.WriteAttributeString("subFolders",
                            ds.IncludeSubFolders.ToString());

                    xw.WriteEndElement();
                }

                xw.Flush();
                return Encoding.UTF8.GetString(ms.ToArray());
            }
            finally
            {
                if(xw != null)
                    xw.Close();

                ms.Dispose();
            }
        }
        #endregion

        #region Add a new doc source to the project
        //=====================================================================

        /// <summary>
        /// Add a new item to the collection
        /// </summary>
        /// <param name="filename">The filename to add</param>
        /// <param name="config">The configuration to use for projects</param>
        /// <param name="platform">The platform to use for projects</param>
        /// <param name="subFolders">True to include subfolders, false to
        /// only search the top-level folder.</param>
        /// <returns>The <see cref="DocumentationSource" /> added to the
        /// project or the existing item if the filename already exists in the
        /// collection.</returns>
        /// <remarks>The <see cref="DocumentationSource" /> constructor is
        /// internal so that we control creation of the items and can
        /// associate them with the project.</remarks>
        public DocumentationSource Add(string filename, string config,
          string platform, bool subFolders)
        {
            DocumentationSource item;

            // Make the path relative to the project if possible
            if(Path.IsPathRooted(filename))
                filename = FilePath.AbsoluteToRelativePath(projectFile.BasePath,
                    filename);

            item = new DocumentationSource(filename, config, platform,
                subFolders, projectFile);

            if(!base.Contains(item))
                base.Add(item);
            else
                item = base[base.IndexOf(item)];

            return item;
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// This is overridden to mark the collection as dirty when it changes
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnListChanged(ListChangedEventArgs e)
        {
            isDirty = true;
            base.OnListChanged(e);
        }
        #endregion
    }
}
