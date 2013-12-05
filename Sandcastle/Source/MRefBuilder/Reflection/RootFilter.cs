// Copyright (c) Microsoft Corporation.  All rights reserved.
//

// Change history:
// 11/20/2013 - EFW - Merged code from Stazzz to implement namespace grouping support.  Cleaned up the code and
// removed unused members.  Added support for a "required" attribute.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection
{
    public class RootFilter
    {
        #region Private data members
        //=====================================================================

        private bool exposed;

        private List<NamespaceFilter> namespaceFilters;
        private List<NamespaceGroupFilter> namespaceGroupFilters;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property gets the number of namespace filters
        /// </summary>
        public int NamespaceFilterCount
        {
            get { return namespaceFilters.Count; }
        }
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Default constructor.  All items are exposed by default.
        /// </summary>
        /// <overloads>There are two overloads for the constructor</overloads>
        public RootFilter()
        {
            exposed = true;
            namespaceFilters = new List<NamespaceFilter>();
            namespaceGroupFilters = new List<NamespaceGroupFilter>();
        }

        /// <summary>
        /// Constructor with configuration read in from an XML reader
        /// </summary>
        /// <param name="configuration">The XML reader from which to get the configuration</param>
        public RootFilter(XmlReader configuration) : this()
        {
            exposed = Convert.ToBoolean(configuration.GetAttribute("expose"), CultureInfo.InvariantCulture);
            XmlReader subtree = configuration.ReadSubtree();

            while(subtree.Read())
                if(subtree.NodeType == XmlNodeType.Element)
                    if(subtree.Name == "namespace")
                    {
                        NamespaceFilter namespaceFilter = new NamespaceFilter(subtree);
                        namespaceFilters.Add(namespaceFilter);
                    }
                    else
                        if(subtree.Name == "namespaceGroup")
                            namespaceGroupFilters.Add(new NamespaceGroupFilter(subtree));

            subtree.Close();
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// This is used to find out if the given type is required
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>True if the type is required, false if not</returns>
        public bool IsRequiredType(TypeNode type)
        {
            foreach(NamespaceFilter namespaceFilter in namespaceFilters)
            {
                bool? result = namespaceFilter.IsRequiredType(type);

                if(result != null)
                    return result.Value;
            }

            return false;
        }

        /// <summary>
        /// This is used to find out if the given type has any exposed members
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>True if the type has exposed members, false if not</returns>
        public bool HasExposedMembers(TypeNode type)
        {
            foreach(NamespaceFilter namespaceFilter in namespaceFilters)
            {
                bool? result = namespaceFilter.IsExposedType(type);

                if(result != null)
                    return namespaceFilter.HasExposedMembers(type);
            }

            return false;
        }

        /// <summary>
        /// This is used to find out if the given namespace group is exposed
        /// </summary>
        /// <param name="type">The namespace group to check</param>
        /// <returns>True if the namespace group is exposed, false if not</returns>
        public bool IsExposedNamespaceGroup(string space)
        {
            foreach(NamespaceGroupFilter namespaceFilter in namespaceGroupFilters)
            {
                bool? result = namespaceFilter.IsExposedNamespaceGroup(space);

                if(result != null)
                    return result.Value;
            }

            return exposed;
        }

        /// <summary>
        /// Check to see if the type is exposed or not
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>True if it is, false if it is not</returns>
        public bool IsExposedType(TypeNode type)
        {
            foreach(NamespaceFilter namespaceFilter in namespaceFilters)
            {
                bool? result = namespaceFilter.IsExposedType(type);

                if(result != null)
                    return result.Value;
            }

            return exposed;
        }

        /// <summary>
        /// Check to see if the member is exposed or not
        /// </summary>
        /// <param name="member">The member to check</param>
        /// <returns>True if it is, false if it is not</returns>
        public bool IsExposedMember(Member member)
        {
            foreach(NamespaceFilter namespaceFilter in namespaceFilters)
            {
                bool? result = namespaceFilter.IsExposedMember(member);

                if(result != null)
                    return result.Value;
            }

            return exposed;
        }
        #endregion
    }
}
