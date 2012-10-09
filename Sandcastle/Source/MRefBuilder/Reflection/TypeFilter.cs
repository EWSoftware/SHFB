// Copyright (c) Microsoft Corporation.  All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection {

    public class TypeFilter
    {

#region Member Variables

        private bool exposed;

        private List < MemberFilter > memberFilters = new List < MemberFilter >();

        private string name;

#endregion

#region Constructors
        public TypeFilter(string name, bool exposed) {
            if (name == null) throw new ArgumentNullException("name");
            this.name = name;
            this.exposed = exposed;
        }

        public TypeFilter(XmlReader configuration) {
            if ((configuration.NodeType != XmlNodeType.Element) || (configuration.Name != "type")) throw new InvalidOperationException();
            name = configuration.GetAttribute("name");
            exposed = Convert.ToBoolean(configuration.GetAttribute("expose"));
            XmlReader subtree = configuration.ReadSubtree();
            while (subtree.Read()) {
                if ((subtree.NodeType == XmlNodeType.Element) && (subtree.Name == "member")) {
                    MemberFilter memberFilter = new MemberFilter(subtree);
                    memberFilters.Add(memberFilter);
                }
            }
            subtree.Close();
        }
#endregion

#region Public API

        //Find out if any are exposed incase this class is not exposed
        public bool HasExposedMembers(TypeNode type)
        {
            foreach (Member member in type.Members)
                foreach (MemberFilter memberFilter in memberFilters)
                    if (memberFilter.IsExposedMember(member) == true)
                        return true;

            return false;
        }


        public bool? IsExposedMember(Member member) {
            //Console.WriteLine("DEBUG: typeFilter.IsExposedMember");
            TypeNode type = ReflectionUtilities.GetTemplateType(member.DeclaringType);
            if (IsExposedType(type) != null) {
                foreach (MemberFilter memberFilter in memberFilters) {
                    bool? result = memberFilter.IsExposedMember(member);
                    if (result != null) return (result);
                }

                return (exposed); //return the type's exposed setting
            } else {
                return (null);
            }
        }

        /**
         * <summary>IsExposedType compares the given type to itself. If this filter
         * contains a '.' designating it as for a nested class it will skip
         * non-nested classes. Classes with no declaring types will not be compared to 
         * filters with a '.'
         * </summary>
         */
        public bool? IsExposedType(TypeNode type)
        {
            bool? typeIsExposed = null;

            //check if the type was nested
            if (type.DeclaringType == null)
            {
                if (type.Name.Name == name)
                    typeIsExposed = exposed;
            }
                //if we are nested then check if this filter is for a nested class
                //check that nothing used is null also. 
                //if the name attribute is not there in the config name can be null here.
            else if (name != null && name.Contains("."))
            {
                //Get a stack of declaring type names
                Stack < string > parentNames = new Stack < string >();
                parentNames.Push(type.Name.Name); //start with this one

                TypeNode parent = type.DeclaringType;
                while (parent != null)
                {
                    parentNames.Push(parent.Name.Name);
                    parent = parent.DeclaringType;
                }

                //put them back in the correct order and check the name
                if (name.Equals(String.Join(".", parentNames.ToArray())))
                    typeIsExposed = exposed;
            }

            return typeIsExposed;
        }

#endregion
    }
}
