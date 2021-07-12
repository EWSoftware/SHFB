//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : MSHelpAttrCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/08/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains a collection class used to hold the help attribute information.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/25/2008  EFW  Created the code
// 07/03/2008  EFW  Rewrote to support MSBuild project format
//===============================================================================================================

using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Xml;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This collection class is used to hold the help attribute items for a
    /// project.
    /// </summary>
    public class MSHelpAttrCollection : BindingList<MSHelpAttr>
    {
        #region Private data members
        //=====================================================================

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
                foreach(MSHelpAttr attr in this)
                    if(attr.IsDirty)
                        return true;

                return isDirty;
            }
            set
            {
                foreach(MSHelpAttr attr in this)
                    attr.IsDirty = value;

                isDirty = value;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public MSHelpAttrCollection()
        {
        }
        #endregion

        #region Sort collection
        //=====================================================================
        // Sort the collection

        /// <summary>
        /// This is used to sort the collection
        /// </summary>
        /// <remarks>Values are sorted by attribute name and value</remarks>
        public void Sort()
        {
            ((List<MSHelpAttr>)base.Items).Sort(
                delegate(MSHelpAttr x, MSHelpAttr y)
                {
                    return Comparer<MSHelpAttr>.Default.Compare(x, y);
                });
        }
        #endregion

        #region Read/write help attribute items from/to XML
        //=====================================================================

        /// <summary>
        /// Write the collection to the given XML text writer
        /// </summary>
        /// <param name="xw">The XML text writer to which the information
        /// is written.</param>
        /// <param name="includeContainer">True to write out the containing <b>HelpAttributes</b> element, false
        /// to exclude it.</param>
        public void WriteXml(XmlWriter xw, bool includeContainer)
        {
            if(includeContainer)
                xw.WriteStartElement("HelpAttributes");

            foreach(MSHelpAttr attr in this)
            {
                xw.WriteStartElement("HelpAttribute");
                xw.WriteAttributeString("name", attr.AttributeName);
                xw.WriteAttributeString("value", attr.AttributeValue);
                xw.WriteEndElement();
            }

            if(includeContainer)
                xw.WriteEndElement();
        }
        #endregion

        #region Add/create a new help attribute item
        //=====================================================================

        /// <summary>
        /// Add a new item to the collection
        /// </summary>
        /// <param name="name">The attribute name</param>
        /// <param name="value">The attribute value</param>
        /// <returns>The <see cref="MSHelpAttr" /> added to the project.  If the item already exists in the
        /// collection, the existing item is returned.</returns>
        /// <remarks>The <see cref="MSHelpAttr" /> constructor is internal so that we control creation of the
        /// items and can associate them with the project.</remarks>
        public MSHelpAttr Add(string name, string value)
        {
            MSHelpAttr item = new MSHelpAttr(name, value);

            if(!this.Contains(item))
                base.Add(item);

            return item;
        }
        #endregion

        #region Convert the collection to a string for the configuration file
        //=====================================================================

        /// <summary>
        /// Convert the collection to its string form for use in the <b>sandcastle.config</b> file.
        /// </summary>
        /// <returns>The help attribute collection in string form ready for use in the Sandcastle BuildAssembler
        /// configuration file.</returns>
        public string ToConfigurationString()
        {
            StringBuilder sb = new StringBuilder("<attributes>\r\n", 1024);

            foreach(MSHelpAttr ha in this)
                sb.AppendFormat(CultureInfo.InvariantCulture, "  <attribute name=\"{0}\" value=\"{1}\" />",
                    ha.AttributeName, ha.AttributeValue);

            sb.Append("</attributes>\r\n");

            return sb.ToString();
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// This is used to mark the collection as changed when there is no associated project.
        /// </summary>
        public void MarkAsDirty()
        {
            isDirty = true;
            this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, -1));
        }

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
