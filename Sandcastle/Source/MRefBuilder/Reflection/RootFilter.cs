// Copyright (c) Microsoft Corporation.  All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection {

    public class RootFilter
    {

#region Member Variables
        private bool exposed;

        private List < NamespaceFilter > namespaceFilters = new List < NamespaceFilter >();
#endregion

#region Constructors

        public RootFilter(bool exposed) {
            this.exposed = exposed;
        }

        public RootFilter(XmlReader configuration) {
            exposed = Convert.ToBoolean(configuration.GetAttribute("expose"));
            XmlReader subtree = configuration.ReadSubtree();
            while (subtree.Read()) {
                if ((subtree.NodeType == XmlNodeType.Element) && (subtree.Name == "namespace")) {
                    NamespaceFilter namespaceFilter = new NamespaceFilter(subtree);
                    namespaceFilters.Add(namespaceFilter);
                }
            }
            subtree.Close();
        }
#endregion

#region Public API

        /// <summary>
        /// Gets the exposed value from the config
        /// </summary>
        public bool ExposedFilterSetting
        {
            get
            {
                return exposed;
            }
        }

        /// <summary>
        /// Gets the number of namespace filters
        /// </summary>
        public int NamespaceFilterCount
        {
            get
            {
                return namespaceFilters.Count;
            }
        }

        public List < NamespaceFilter > NamespaceFilters {
            get {
                return (namespaceFilters);
            }
        }

        //Find out if any are exposed incase this class is not exposed
        public bool HasExposedMembers(TypeNode type)
        {
            foreach (NamespaceFilter namespaceFilter in namespaceFilters)
            {
                bool? result = namespaceFilter.IsExposedType(type);
                if (result != null)
                {
                    return namespaceFilter.HasExposedMembers(type);
                }
            }
            return false;
        }

        public bool IsExposedApi(Member api) {

            Namespace space = api as Namespace;
            if (space != null) return (IsExposedNamespace(space));

            TypeNode type = api as TypeNode;
            if (type != null) return (IsExposedType(type));

            return (IsExposedMember(api));

        }


        public bool IsExposedMember(Member member) {
            //Console.WriteLine("DEBUG: root.IsExposedMember");
            foreach (NamespaceFilter namespaceFilter in namespaceFilters) {
                bool? result = namespaceFilter.IsExposedMember(member);
                if (result != null) return ((bool)result);
            }
            return (exposed);
        }

        public bool IsExposedNamespace(Namespace space) {
            foreach (NamespaceFilter namespaceFilter in namespaceFilters) {
                bool? result = namespaceFilter.IsExposedNamespace(space);
                if (result != null) return ((bool)result);
            }
            return (exposed);
        }

        public bool IsExposedType(TypeNode type) {
            foreach (NamespaceFilter namespaceFilter in namespaceFilters) {
                bool? result = namespaceFilter.IsExposedType(type);
                if (result != null) return ((bool)result);
            }

            return (exposed);
        }

#endregion
    }
}
