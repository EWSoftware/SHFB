//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : MSHelpKeywordCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/16/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a collection class used to hold the help index keyword
// information.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.7  03/25/2008  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Text;
using System.Xml;

using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This collection class is used to hold the help index keyword items for
    /// a project.
    /// </summary>
    [TypeConverter(typeof(MSHelpKeywordCollectionTypeConverter)),
      Editor(typeof(MSHelpKeywordEditor), typeof(UITypeEditor)), Serializable]
    public class MSHelpKeywordCollection : BindingList<MSHelpKeyword>
    {
        #region Sort collection
        //=====================================================================
        // Sort the collection

        /// <summary>
        /// This is used to sort the collection
        /// </summary>
        /// <remarks>Values are sorted by index name and term</remarks>
        public void Sort()
        {
            ((List<MSHelpKeyword>)base.Items).Sort(
                delegate(MSHelpKeyword x, MSHelpKeyword y)
                {
                    return Comparer<MSHelpKeyword>.Default.Compare(x, y);
                });
        }
        #endregion

        #region Read/write as XML
        //=====================================================================

        /// <summary>
        /// This is used to load the keyword collection from the project
        /// file.
        /// </summary>
        /// <param name="xr">The XML text reader from which the information
        /// is loaded.</param>
        internal void ReadXml(XmlReader xr)
        {
            MSHelpKeyword kw;

            while(!xr.EOF && xr.NodeType != XmlNodeType.EndElement)
            {
                if(xr.NodeType == XmlNodeType.Element &&
                  xr.Name == "HelpKeyword")
                {
                    kw = new MSHelpKeyword(xr.GetAttribute("index"),
                        xr.GetAttribute("term"));

                    if(!String.IsNullOrEmpty(kw.Index))
                        this.Add(kw);
                }

                xr.Read();
            }
        }

        /// <summary>
        /// This is used to save the keyword collection to the project
        /// file.
        /// </summary>
        /// <param name="xw">The XML text writer to which the information
        /// is written.</param>
        public void WriteXml(XmlWriter xw)
        {
            if(base.Count > 0)
            {
                xw.WriteStartElement("HelpKeywords");

                foreach(MSHelpKeyword kw in this)
                {
                    xw.WriteStartElement("HelpKeyword");
                    xw.WriteAttributeString("index", kw.Index);
                    xw.WriteAttributeString("term", kw.Term);
                    xw.WriteEndElement();
                }

                xw.WriteEndElement();
            }
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to mark the collection as changed when there is no
        /// associated project.
        /// </summary>
        public void MarkAsDirty()
        {
            this.OnListChanged(new ListChangedEventArgs(
                ListChangedType.ItemChanged, -1));
        }
        #endregion
    }
}
