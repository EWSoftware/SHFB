// Copyright (c) Microsoft Corporation.  All rights reserved.
//

// Change history:
// 11/24/2013 - EFW - Cleaned up the code and removed unused members.  Added support for a "required" attribute
// that indicates a type is required and should always be exposed.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;

using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection
{
    public class TypeFilter
    {
        #region Private data members
        //=====================================================================

        private string name;
        private bool exposed, required;

        private List<MemberFilter> memberFilters;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">The XML reader from which to get the configuration</param>
        public TypeFilter(XmlReader configuration)
        {
            if(configuration.NodeType != XmlNodeType.Element || configuration.Name != "type")
                throw new InvalidOperationException("The configuration element must be named 'type'");

            memberFilters = new List<MemberFilter>();
            name = configuration.GetAttribute("name");
            exposed = Convert.ToBoolean(configuration.GetAttribute("expose"), CultureInfo.InvariantCulture);

            // If not exposed, check for a required attribute which forces it to be exposed.  This allows a
            // way to expose it and indicate that it should always be exposed in the configuration file.
            if(!exposed)
            {
                required = Convert.ToBoolean(configuration.GetAttribute("required"), CultureInfo.InvariantCulture);

                if(required)
                    exposed = true;
            }

            XmlReader subtree = configuration.ReadSubtree();

            while(subtree.Read())
                if(subtree.NodeType == XmlNodeType.Element && subtree.Name == "member")
                    memberFilters.Add(new MemberFilter(subtree));

            subtree.Close();
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// This is used to find out if the given type has any exposed members
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>True if the type has exposed members, false if not</returns>
        public bool HasExposedMembers(TypeNode type)
        {
            return type.Members.Any(member => memberFilters.Any(filter => filter.IsExposedMember(member) == true));
        }

        /// <summary>
        /// Check to see if the type is marked as required or not by this entry
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>Null if the type is not represented by this entry, true if it is and it is required or false
        /// if it is and it is not required.</returns>
        /// <remarks>This compares the given type to itself.  If this filter contains a '.' designating it as
        /// for a nested class it will skip non-nested classes.  Classes with no declaring types will not be
        /// compared to filters with a '.'.</remarks>
        public bool? IsRequiredType(TypeNode type)
        {
            bool? typeIsRequired = null;

            // Check to see if the type is nested
            if(type.DeclaringType == null && type.Name.Name == name)
                typeIsRequired = required;
            else
                if(name != null && name.IndexOf('.') != -1)
                {
                    // If we are nested, check if this filter is for a nested class.

                    // Get a stack of declaring type names
                    Stack<string> parentNames = new Stack<string>();
                    parentNames.Push(type.Name.Name);

                    TypeNode parent = type.DeclaringType;

                    while(parent != null)
                    {
                        parentNames.Push(parent.Name.Name);
                        parent = parent.DeclaringType;
                    }

                    // Put them back in the correct order and check the name
                    if(name == String.Join(".", parentNames))
                        typeIsRequired = required;
                }

            return typeIsRequired;
        }

        /// <summary>
        /// Check to see if the type is exposed or not by this entry
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>Null if the type is not represented by this entry, true if it is and it is exposed or false
        /// if it is and it is not exposed.</returns>
        /// <remarks>This compares the given type to itself.  If this filter contains a '.' designating it as
        /// for a nested class it will skip non-nested classes.  Classes with no declaring types will not be
        /// compared to filters with a '.'.</remarks>
        public bool? IsExposedType(TypeNode type)
        {
            bool? typeIsExposed = null;

            // Check to see if the type is nested
            if(type.DeclaringType == null && type.Name.Name == name)
                typeIsExposed = exposed;
            else
                if(name != null && name.IndexOf('.') != -1)
                {
                    // If we are nested, check if this filter is for a nested class.

                    // Get a stack of declaring type names
                    Stack<string> parentNames = new Stack<string>();
                    parentNames.Push(type.Name.Name);

                    TypeNode parent = type.DeclaringType;

                    while(parent != null)
                    {
                        parentNames.Push(parent.Name.Name);
                        parent = parent.DeclaringType;
                    }

                    // Put them back in the correct order and check the name
                    if(name == String.Join(".", parentNames))
                        typeIsExposed = exposed;
                }

            return typeIsExposed;
        }

        /// <summary>
        /// Check to see if the member is exposed or not by this entry
        /// </summary>
        /// <param name="member">The member to check</param>
        /// <returns>Null if the member is not within the type of this entry, true if it is and it is exposed or
        /// false if it is and it is not exposed.</returns>
        public bool? IsExposedMember(Member member)
        {
            TypeNode type = member.DeclaringType.GetTemplateType();

            if(this.IsExposedType(type) != null)
            {
                foreach(MemberFilter memberFilter in memberFilters)
                {
                    bool? result = memberFilter.IsExposedMember(member);

                    if(result != null)
                        return result;
                }

                // No filters matched so use the type setting
                return exposed;
            }

            return null;
        }
        #endregion
    }
}
