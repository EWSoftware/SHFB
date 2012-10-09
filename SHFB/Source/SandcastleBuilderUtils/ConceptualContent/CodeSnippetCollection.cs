//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : CodeSnippetCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/07/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a collection class used to hold the conceptual content
// code snippet information from a code snippet file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  08/07/2008  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.Utils.ConceptualContent
{
    /// <summary>
    /// This collection class is used to hold the conceptual content code
    /// snippet information for an associated code snippets file.
    /// </summary>
    public class CodeSnippetCollection : BindingList<CodeReference>
    {
        #region Private data members
        //=====================================================================

        private FileItem fileItem;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get the build item related to the code snippet file
        /// containing the collection items.
        /// </summary>
        public FileItem CodeSnippetFile
        {
            get { return fileItem; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="file">The code snippets file associated with the
        /// collection.</param>
        /// <remarks>Code snippets are not loaded until the <see cref="Load" />
        /// method is called.</remarks>
        public CodeSnippetCollection(FileItem file)
        {
            fileItem = file;
        }
        #endregion

        #region Sort collection
        //=====================================================================
        // Sort the collection

        /// <summary>
        /// This is used to sort the collection
        /// </summary>
        /// <remarks>Values are sorted by ID.  Comparisons are case-sensitive.</remarks>
        public void Sort()
        {
            ((List<CodeReference>)base.Items).Sort(
                delegate(CodeReference x, CodeReference y)
                {
                    return String.Compare(x.Id, y.Id, StringComparison.Ordinal);
                });
        }
        #endregion

        #region Read/write the token file
        //=====================================================================

        /// <summary>
        /// Load the collection from the related file
        /// </summary>
        /// <remarks>This will be done automatically at constructor.  This can
        /// be called to reload the collection if needed.</remarks>
        public void Load()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            XmlReader xr = null;

            try
            {
                this.Clear();
                settings.CloseInput = true;

                xr = XmlReader.Create(fileItem.FullPath, settings);
                xr.MoveToContent();

                while(!xr.EOF)
                {
                    if(xr.NodeType == XmlNodeType.Element && xr.Name == "item")
                        this.Add(new CodeReference(xr.GetAttribute("id")));

                    xr.Read();
                }

                this.Sort();
            }
            finally
            {
                if(xr != null)
                    xr.Close();
            }
        }
        #endregion
    }
}
