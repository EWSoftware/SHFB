//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : MSHelpKeywordCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/08/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains a collection class used to hold the help index keyword information.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/25/2008  EFW  Created the code
//===============================================================================================================

using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This collection class is used to hold the help index keyword items for a project
    /// </summary>
    public class MSHelpKeywordCollection : BindingList<MSHelpKeyword>
    {
        #region Sort collection
        //=====================================================================

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
        /// This is used to save the keyword collection to the project file
        /// </summary>
        /// <param name="xw">The XML text writer to which the information is written.</param>
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
        /// This is used to mark the collection as changed when there is no associated project
        /// </summary>
        public void MarkAsDirty()
        {
            this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, -1));
        }
        #endregion
    }
}
