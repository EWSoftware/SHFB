//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : NamespaceSummaryItemCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2006-2025, Eric Woodruff, All rights reserved
//
// This file contains a collection class used to hold the namespace summary item information
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/04/2006  EFW  Created the code
// 06/30/2008  EFW  Rewrote to support MSBuild project format
// 04/07/2011  EFW  Made the constructor and from/to XML members public so that it can be used from the
//                  VSPackage.
// 11/30/2013  EFW  Merged changes from Stazzz to support namespace grouping
//===============================================================================================================

// Ignore Spelling: Stazzz

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Sandcastle.Core.Project
{
    /// <summary>
    /// This collection class is used to hold the namespace summary items for a project
    /// </summary>
    /// <remarks>Namespaces that appear in the assemblies but not in this list are documented by default and
    /// will appear without a namespace summary.</remarks>
    public class NamespaceSummaryItemCollection : List<NamespaceSummaryItem>
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set a reference to the project that owns the collection
        /// </summary>
        /// <remarks>This is used by collection editors to get a reference to the owning project</remarks>
        public ISandcastleProject Project { get; set; }

        /// <summary>
        /// Indexer.  This can be used to retrieve the summary information for the specified namespace
        /// </summary>
        /// <param name="name">The namespace for which to search</param>
        /// <returns>The namespace summary information if found or null if not found</returns>
        public NamespaceSummaryItem this[string name]
        {
            get
            {
                if(String.IsNullOrWhiteSpace(name))
                    name = "(global)";

                foreach(NamespaceSummaryItem nsi in this)
                    if(nsi.Name == name)
                        return nsi;

                return null;
            }
        }
        #endregion

        #region Read/write namespace summary items from/to XML
        //=====================================================================

        /// <summary>
        /// This is used to load existing namespace summary items from the project file
        /// </summary>
        /// <param name="namespaceSummaries">The namespace summary items</param>
        /// <remarks>The information is stored as an XML fragment</remarks>
        public void FromXml(string namespaceSummaries)
        {
            string name, groupValue;
            bool isDocumented, isGroup;

            using var xr = new XmlTextReader(namespaceSummaries, XmlNodeType.Element,
              new XmlParserContext(null, null, null, XmlSpace.Default));
            
            xr.MoveToContent();

            while(!xr.EOF)
            {
                if(xr.NodeType == XmlNodeType.Element && xr.Name == "NamespaceSummaryItem")
                {
                    name = xr.GetAttribute("name");
                    isDocumented = Convert.ToBoolean(xr.GetAttribute("isDocumented"),
                        CultureInfo.InvariantCulture);

                    groupValue = xr.GetAttribute("isGroup");
                    isGroup = !String.IsNullOrWhiteSpace(groupValue) && Convert.ToBoolean(groupValue,
                       CultureInfo.InvariantCulture);

                    this.Add(name, isGroup, isDocumented, xr.ReadString());
                }

                xr.Read();
            }
        }

        /// <summary>
        /// This is used to write the namespace summary info to an XML fragment ready for storing in the project
        /// file.
        /// </summary>
        /// <returns>The XML fragment containing the namespace summary info</returns>
        public string ToXml()
        {
            using var ms = new MemoryStream(10240);
            using var xw = new XmlTextWriter(ms, new UTF8Encoding(false));
            
            xw.Formatting = Formatting.Indented;

            foreach(NamespaceSummaryItem nsi in this)
            {
                xw.WriteStartElement("NamespaceSummaryItem");
                xw.WriteAttributeString("name", nsi.Name);

                if(nsi.IsGroup)
                    xw.WriteAttributeString("isGroup", nsi.IsGroup.ToString());

                xw.WriteAttributeString("isDocumented", nsi.IsDocumented.ToString());
                xw.WriteString(nsi.Summary);
                xw.WriteEndElement();
            }

            xw.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }
        #endregion

        #region Add/create a new namespace summary item
        //=====================================================================

        /// <summary>
        /// Add a new item to the collection
        /// </summary>
        /// <param name="name">The namespace name</param>
        /// <param name="isGroup">True if this is a grouping namespace, false if this is normal namespace</param>
        /// <param name="isDocumented">True for documented, false for not documented</param>
        /// <param name="summary">The summary text</param>
        /// <returns>The <see cref="NamespaceSummaryItem" /> added to the project.  If the namespace already
        /// exists in the collection, the existing item is returned.</returns>
        /// <remarks>The <see cref="NamespaceSummaryItem" /> constructor is internal so that we control creation
        /// of the items and can associate them with the project.</remarks>
        public NamespaceSummaryItem Add(string name, bool isGroup, bool isDocumented, string summary)
        {
            NamespaceSummaryItem item = this[name];

            if(item == null)
            {
                item = new NamespaceSummaryItem(name, isGroup, isDocumented, summary);
                this.Add(item);
            }

            return item;
        }

        /// <summary>
        /// Create a temporary item that isn't part of the project
        /// </summary>
        /// <param name="name">The namespace name</param>
        /// <param name="isGroup">True if this is a grouping namespace, false if this is normal namespace</param>
        /// <returns>The <see cref="NamespaceSummaryItem" /> that can later be added to the project if necessary</returns>
        /// <exception cref="ArgumentException">This is thrown if the given namespace already exists in the
        /// collection.</exception>
        public NamespaceSummaryItem CreateTemporaryItem(string name, bool isGroup)
        {
            NamespaceSummaryItem item = this[name];

            if(item != null)
            {
                throw new ArgumentException("The given namespace exists in the collection and cannot be a " +
                    "temporary item", nameof(name));
            }

            item = new NamespaceSummaryItem(name, isGroup, !String.IsNullOrEmpty(name), String.Empty);

            return item;
        }
        #endregion
    }
}
