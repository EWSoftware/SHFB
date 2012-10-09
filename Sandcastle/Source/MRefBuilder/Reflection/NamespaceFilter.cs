// Copyright (c) Microsoft Corporation.  All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection {

    public class NamespaceFilter
    {

#region Member Variables

        private bool exposed;
        private string name;

        private List < TypeFilter > typeFilters = new List < TypeFilter >();
#endregion

#region Constructors
        public NamespaceFilter(string name, bool exposed) {
            this.name = name;
            this.exposed = exposed;
        }

        public NamespaceFilter(XmlReader configuration) {
            if (configuration.Name != "namespace") throw new InvalidOperationException();
            name = configuration.GetAttribute("name");
            exposed = Convert.ToBoolean(configuration.GetAttribute("expose"));
            XmlReader subtree = configuration.ReadSubtree();
            while (subtree.Read()) {
                if ((subtree.NodeType == XmlNodeType.Element) && (subtree.Name == "type")) {
                    TypeFilter typeFilter = new TypeFilter(subtree);
                    typeFilters.Add(typeFilter);
                }
            }
            subtree.Close();
        }
#endregion

#region Public API

        /// <summary>
        /// Gets the number of type filters
        /// </summary>
        public int TypeFilterCount
        {
            get
            {
                return typeFilters.Count;
            }
        }

        public List < TypeFilter > TypeFilters {
            get {
                return (typeFilters);
                {
                }
            }
        }


        //Find out if any are exposed incase this class is not exposed
        public bool HasExposedMembers(TypeNode type)
        {
            Namespace space = ReflectionUtilities.GetNamespace(type);
            if (IsExposedNamespace(space) != null)
            {
                foreach (TypeFilter typeFilter in typeFilters)
                {
                    bool? result = typeFilter.IsExposedType(type);
                    if (result != null) //matched
                    {
                        return typeFilter.HasExposedMembers(type);
                    }
                }
            }

            return false;
        }

        public bool? IsExposedMember(Member member) {
            //Console.WriteLine("DEBUG: namespaceFilter.isExposedMemeber");
            TypeNode type = ReflectionUtilities.GetTemplateType(member.DeclaringType);
            Namespace space = ReflectionUtilities.GetNamespace(type);
            if (IsExposedNamespace(space) != null) {
                foreach (TypeFilter typeFilter in typeFilters) {
                    bool? result = typeFilter.IsExposedMember(member);
                    if (result != null) return (result);
                }

                //no filters matched this method, check if the type is exposed
                bool? typeIsExposed = IsExposedType(type);
                if (typeIsExposed != null) return typeIsExposed;

                return (exposed); //if the namespace is exposed
            } else {
                return (null);
            }
        }

        public bool? IsExposedNamespace(Namespace space) {
            if (space.Name.Name == name) {
                return (exposed);
            } else {
                return (null);
            }
        }

        public bool? IsExposedType(TypeNode type) {
            Namespace space = ReflectionUtilities.GetNamespace(type);
            if (IsExposedNamespace(space) != null) {
                foreach (TypeFilter typeFilter in typeFilters) {
                    bool? result = typeFilter.IsExposedType(type);
                    if (result != null) return (result);
                }

                //no filter matches for this type, check the parents since it could be nested
                TypeNode parent = type.DeclaringType;
                while (parent != null)
                {
                    bool? parentExposed = IsExposedType(parent);

                    if (parentExposed != null)
                        return parentExposed;

                    parent = type.DeclaringType;
                }

                //no answer for the parents either, the top parent should pass this back above
                return (exposed);
            } else {
                return (null);
            }
        }

#endregion
    }
}
