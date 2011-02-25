//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ApiFilter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/08/2008
// Note    : Copyright 2007-2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing an API entry that is to be removed
// from the reflection information using MRefBuilder's namespace ripping
// feature.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.0.2  07/16/2007  EFW  Created the code
// 1.8.0.0  07/08/2008  EFW  Rewrote to support MSBuild project format
//=============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using System.Xml;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This represents an API entry that is to be removed from the reflection
    /// information using MRefBuilder's namespace ripping feature.
    /// </summary>
    public class ApiFilter : IComparable<ApiFilter>, ICloneable
    {
        #region Private data members
        //=====================================================================
        // Private data members

        private ApiEntryType entryType;
        private string fullName, filterName;
        private bool isExposed, isProjectExclude;

        private ApiFilterCollection children;
        #endregion

        #region Properties
        //=====================================================================
        // Properties

        /// <summary>
        /// This is used to get or set the API entry type
        /// </summary>
        public ApiEntryType EntryType
        {
            get { return entryType; }
            set { entryType = value; }
        }

        /// <summary>
        /// This is used to get the fully qualified name of the API entry
        /// </summary>
        public string FullName
        {
            get { return fullName; }
            set { fullName = value; }
        }

        /// <summary>
        /// This is used to get the API filter name
        /// </summary>
        /// <value>For API entry types other than namespaces, this is the name
        /// without the namespace and, for methods properties, etc. the
        /// type.</value>
        public string FilterName
        {
            get { return filterName; }
            set
            {
                if(value == null)
                    value = String.Empty;

                // For use in the filter, we need to translate some characters
                filterName = value.Replace('#', '.');
#if DEBUG
                // There shouldn't be any parameter info as the ripping
                // feature doesn't support it.
                if(filterName.IndexOf('(') != -1 ||
                  filterName.IndexOf("``", StringComparison.Ordinal) != -1)
                    throw new ArgumentException("Filter name contains " +
                        "parameter information which isn't supported", "value");
#endif
            }
        }

        /// <summary>
        /// This is used to get or set whether or not the entry is exposed
        /// </summary>
        public bool IsExposed
        {
            get { return isExposed; }
            set { isExposed = value; }
        }

        /// <summary>
        /// This is used to get or set whether or not the entry is excluded
        /// via the project (i.e. via the SHFB Namespaces option or an
        /// <code>&lt;exclude /&gt;</code> tag.
        /// </summary>
        public bool IsProjectExclude
        {
            get { return isProjectExclude; }
            set { isProjectExclude = value; }
        }

        /// <summary>
        /// This returns the child API filter collection for this entry
        /// </summary>
        /// <value>For namespaces and types, if there are children, they
        /// represent the specific entries within the namespace or type
        /// to hide or expose.</value>
        public ApiFilterCollection Children
        {
            get { return children; }
        }
        #endregion

        #region IComparable<ApiFilter> Members
        /// <summary>
        /// Compares this instance to another instance and returns an
        /// indication of their relative values.
        /// </summary>
        /// <param name="other">An ApiFilter object to compare</param>
        /// <returns>Returns -1 if this instance is less than the
        /// value, 0 if they are equal, or 1 if this instance is
        /// greater than the value or the value is null.</returns>
        /// <remarks>Entries are sorted by API entry type and full name</remarks>
        public int CompareTo(ApiFilter other)
        {
            int result = 0;

            if(other == null)
                return 1;

            // For types, treat them as equal and sort by name
            if(entryType < ApiEntryType.Class || entryType >
              ApiEntryType.Delegate || other.EntryType < ApiEntryType.Class ||
              other.EntryType > ApiEntryType.Delegate)
                result = (int)entryType - (int)other.EntryType;

            if(result == 0)
                result = String.Compare(fullName, other.FullName,
                    StringComparison.CurrentCulture);

            return result;
        }
        #endregion

        #region ICloneable Members
        //=====================================================================
        // ICloneable implementation

        /// <summary>
        /// Clone the API filter
        /// </summary>
        /// <returns>A clone of the filter</returns>
        public object Clone()
        {
            ApiFilter clone = new ApiFilter(entryType, fullName, isExposed);

            clone.FilterName = filterName;
            clone.IsProjectExclude = isProjectExclude;

            foreach(ApiFilter child in children)
                clone.Children.Add((ApiFilter)child.Clone());

            return clone;
        }
        #endregion

        //=====================================================================
        // Methods, etc.

        #region Constructors
        //=====================================================================
        // Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <overloads>There are two overloads for the constructor</overloads>
        internal ApiFilter()
        {
            // Child collections do not contain a project reference
            children = new ApiFilterCollection(null);
            isExposed = true;
        }

        /// <summary>
        /// Constructor.  This takes the API type and the full name
        /// </summary>
        /// <param name="apiType">The API entry type</param>
        /// <param name="name">The fully qualified name</param>
        /// <param name="exposed">True to expose it, false to hide it</param>
        internal ApiFilter(ApiEntryType apiType, string name, bool exposed) :
          this()
        {
            int pos;

            if(name == null)
                name = String.Empty;

            entryType = apiType;
            fullName = name;
            isExposed = exposed;

            // By default, we'll use the last part as the filter name
            // unless it's a namespace.
            pos = name.LastIndexOf('.');

            if(entryType == ApiEntryType.Namespace || pos == -1)
                this.FilterName = name;
            else
                this.FilterName = name.Substring(pos + 1);
        }
        #endregion

        #region ConvertToString method
        //=====================================================================
        // ConvertToString and Equals methods

        /// <summary>
        /// This is used to convert the entry and its children to a string
        /// ready for use in the MRefBuilder config file.
        /// </summary>
        /// <param name="sb">The string builder to which the information is
        /// appended.</param>
        internal void ConvertToString(StringBuilder sb)
        {
            string endTag = null;

            // Invalid entries are ignored
            if(entryType != ApiEntryType.None)
            {
                switch(entryType)
                {
                    case ApiEntryType.Namespace:
                        sb.Append("  <namespace name=\"");
                        endTag = "  </namespace>\r\n";
                        break;

                    case ApiEntryType.Class:
                    case ApiEntryType.Structure:
                    case ApiEntryType.Interface:
                    case ApiEntryType.Enumeration:
                    case ApiEntryType.Delegate:
                        sb.Append("    <type name=\"");
                        endTag = "    </type>\r\n";
                        break;

                    default:
                        sb.Append("      <member name=\"");
                        break;
                }

                sb.Append(HttpUtility.HtmlEncode(filterName));

                sb.AppendFormat("\" expose=\"{0}\"",
                    isExposed.ToString(CultureInfo.InvariantCulture).ToLower(
                        CultureInfo.InvariantCulture));

                if(children.Count == 0)
                    sb.Append("/>\r\n");
                else
                {
                    sb.Append(">\r\n");

                    foreach(ApiFilter child in children)
                        child.ConvertToString(sb);

                    sb.Append(endTag);
                }
            }
        }
        #endregion

        #region Read/write as XML methods
        //=====================================================================
        // These are used to read and write the settings to the project file

        /// <summary>
        /// This is used to load the content item information from the project
        /// file.
        /// </summary>
        /// <param name="xr">The XML text reader from which the information
        /// is loaded.</param>
        internal void FromXml(XmlTextReader xr)
        {
            ApiFilter filter;
            string attrValue;

            this.EntryType = (ApiEntryType)Enum.Parse(typeof(ApiEntryType),
                xr.GetAttribute("entryType"), true);
            this.FullName = xr.GetAttribute("fullName");

            attrValue = xr.GetAttribute("filterName");

            if(!String.IsNullOrEmpty(attrValue))
                this.FilterName = attrValue;
            else
                this.FilterName = fullName;

            this.IsExposed = Convert.ToBoolean(xr.GetAttribute(
                "isExposed"), CultureInfo.InvariantCulture);

            if(!xr.IsEmptyElement)
                while(!xr.EOF)
                {
                    xr.Read();

                    if(xr.NodeType == XmlNodeType.EndElement &&
                      xr.Name == "Filter")
                        break;

                    if(xr.NodeType == XmlNodeType.Element &&
                      xr.Name == "Filter")
                    {
                        filter = new ApiFilter();
                        filter.FromXml(xr);
                        this.Children.Add(filter);
                    }
                }
        }

        /// <summary>
        /// This is used to save the content item information to the project
        /// file.
        /// </summary>
        /// <param name="xw">The XML text writer to which the information
        /// is written.</param>
        internal void ToXml(XmlTextWriter xw)
        {
            xw.WriteStartElement("Filter");
            xw.WriteAttributeString("entryType", entryType.ToString());
            xw.WriteAttributeString("fullName", fullName);

            if(filterName != fullName)
                xw.WriteAttributeString("filterName", filterName);

            xw.WriteAttributeString("isExposed", isExposed.ToString());

            if(children.Count != 0)
                foreach(ApiFilter filter in children)
                    filter.ToXml(xw);

            xw.WriteEndElement();
        }

        #endregion

        #region Static helper methods
        //=====================================================================
        // Static helper methods

        /// <summary>
        /// This is used to convert an API type letter to an
        /// <see cref="ApiEntryType"/> enumerated value.
        /// </summary>
        /// <param name="apiType">The letter to convert</param>
        /// <returns>The <b>ApiEntryType</b> represented by the letter</returns>
        public static ApiEntryType ApiEntryTypeFromLetter(char apiType)
        {
            switch(apiType)
            {
                case 'N':
                    return ApiEntryType.Namespace;

                case 'T':
                    // It could be a class, structure, interface, enumeration,
                    // or a delegate.  They all use "T" so we'll pick Class.
                    return ApiEntryType.Class;

                case 'M':
                    return ApiEntryType.Method;

                case 'P':
                    return ApiEntryType.Property;

                case 'E':
                    return ApiEntryType.Event;

                case 'F':
                    return ApiEntryType.Field;

                default:
                    return ApiEntryType.None;
            }
        }
        #endregion
    }
}
