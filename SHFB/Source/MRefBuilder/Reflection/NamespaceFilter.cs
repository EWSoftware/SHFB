// Copyright (c) Microsoft Corporation.  All rights reserved.
//

// Change history:
// 11/24/2013 - EFW - Cleaned up the code and removed unused members.  Added support for a "required" attribute.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

using System.Compiler;

namespace Sandcastle.Tools.Reflection
{
    /// <summary>
    /// This class implements the namespace filter
    /// </summary>
    public class NamespaceFilter
    {
        #region Private data members
        //=====================================================================

        private readonly string name;
        private readonly bool exposed;

        private readonly List<TypeFilter> typeFilters;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">The XML reader from which to get the configuration information</param>
        public NamespaceFilter(XmlReader configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if(configuration.Name != "namespace")
                throw new InvalidOperationException("The configuration element must be named 'namespace'");

            typeFilters = [];
            name = configuration.GetAttribute("name");
            exposed = Convert.ToBoolean(configuration.GetAttribute("expose"), CultureInfo.InvariantCulture);

            XmlReader subtree = configuration.ReadSubtree();

            while(subtree.Read())
                if(subtree.NodeType == XmlNodeType.Element && subtree.Name == "type")
                    typeFilters.Add(new TypeFilter(subtree));

            subtree.Close();
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// Check to see if the type is required or not by this entry
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>Null if the type is not within the namespace of this entry, true if it is and it is required
        /// or false if it is and it is not required.</returns>
        public bool? IsRequiredType(TypeNode type)
        {
            if(this.IsExposedNamespace(type.GetNamespace()) != null)
            {
                foreach(TypeFilter typeFilter in typeFilters)
                {
                    bool? result = typeFilter.IsRequiredType(type);

                    if(result != null)
                        return result;
                }

                // No filters match so it's not required
                return false;
            }

            return null;
        }

        /// <summary>
        /// This is used to find out if the given type is in this namespace and has any exposed members
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>True if the type is in this namespace and has exposed members, false if not</returns>
        public bool HasExposedMembers(TypeNode type)
        {
            if(this.IsExposedNamespace(type.GetNamespace()) != null)
                foreach(TypeFilter typeFilter in typeFilters)
                {
                    bool? result = typeFilter.IsExposedType(type);

                    if(result != null)
                        return typeFilter.HasExposedMembers(type);
                }

            return false;
        }

        /// <summary>
        /// Check to see if the namespace is exposed or not by this entry
        /// </summary>
        /// <param name="space">The namespace to check</param>
        /// <returns>Null if the namespace is not represented by this entry, true if it is and it is exposed or
        /// false if it is and it is not exposed.</returns>
        public bool? IsExposedNamespace(Namespace space)
        {
            if(space == null)
                throw new ArgumentNullException(nameof(space));

            return (space.Name != null && space.Name.Name == name) ? exposed : (bool?)null;
        }

        /// <summary>
        /// Check to see if the type is exposed or not by this entry
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>Null if the type is not within the namespace of this entry, true if it is and it is exposed
        /// or false if it is and it is not exposed.</returns>
        public bool? IsExposedType(TypeNode type)
        {
            if(this.IsExposedNamespace(type.GetNamespace()) != null)
            {
                foreach(TypeFilter typeFilter in typeFilters)
                {
                    bool? result = typeFilter.IsExposedType(type);

                    if(result != null)
                        return result;
                }

                // No filter matches for this type, check the parents since it could be nested
                TypeNode parent = type.DeclaringType;

                while(parent != null)
                {
                    bool? parentExposed = this.IsExposedType(parent);

                    if(parentExposed != null)
                        return parentExposed;

                    parent = type.DeclaringType;
                }

                // No filters match for the parents either, use the namespace setting
                return exposed;
            }

            return null;
        }

        /// <summary>
        /// Check to see if the member is exposed or not by this entry
        /// </summary>
        /// <param name="member">The member to check</param>
        /// <returns>Null if the member is not within a type in the namespace of this entry, true if it is and it
        /// is exposed or false if it is and it is not exposed.</returns>
        public bool? IsExposedMember(Member member)
        {
            if(member == null)
                throw new ArgumentNullException(nameof(member));

            TypeNode type = member.DeclaringType.GetTemplateType();

            if(this.IsExposedNamespace(type.GetNamespace()) != null)
            {
                foreach(TypeFilter typeFilter in typeFilters)
                {
                    bool? result = typeFilter.IsExposedMember(member);

                    if(result != null)
                        return result;
                }

                // No filters matched this method, check if the type is exposed.  If no types match, use the
                // namespace setting.
                return (this.IsExposedType(type) ?? exposed);
            }

            return null;
        }
        #endregion
    }
}
