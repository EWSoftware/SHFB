//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : MSHelpKeywordCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2008-2025, Eric Woodruff, All rights reserved
//
// This file contains a collection class used to hold the help index keyword information
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

using System;
using System.ComponentModel;
using System.Linq;
using System.Xml;

namespace Sandcastle.Core.ConceptualContent
{
    /// <summary>
    /// This collection class is used to hold the help index keyword items for a project
    /// </summary>
    public class MSHelpKeywordCollection : BindingList<MSHelpKeyword>
    {
        #region Read/write as XML
        //=====================================================================

        /// <summary>
        /// This is used to load the keyword collection from the project file
        /// </summary>
        /// <param name="xr">The XML text reader from which the information is loaded.</param>
        internal void ReadXml(XmlReader xr)
        {
            while(!xr.EOF && xr.NodeType != XmlNodeType.EndElement)
            {
                if(xr.NodeType == XmlNodeType.Element && xr.Name == "HelpKeyword")
                    if(MSHelpKeyword.ValidIndexNames.Contains(xr.GetAttribute("index")))
                        this.Add(new MSHelpKeyword(xr.GetAttribute("index"), xr.GetAttribute("term")));

                xr.Read();
            }
        }

        /// <summary>
        /// This is used to save the keyword collection to the project file
        /// </summary>
        /// <param name="xw">The XML text writer to which the information is written</param>
        public void WriteXml(XmlWriter xw)
        {
            if(xw == null)
                throw new ArgumentNullException(nameof(xw));

            if(this.Count > 0)
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
    }
}
