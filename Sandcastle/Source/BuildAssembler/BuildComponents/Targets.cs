// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.Xml.XPath;

namespace BuildComponents {

    public partial class Target {

        internal string id;

        public string Id {
            get {
                return (id);
            }
        }
    }

    // Namespace

    public partial class NamespaceTarget : Target {

        internal string name;

        public string Name {
            get {
                return (name);
            }
        }

    }

    // Type

    public partial class TypeTarget : Target {

        // apidata

        protected string subgroup;

        // containers

        protected NamespaceReference containingNamespace;

        protected SimpleTypeReference containingType;

        protected string containingAssembly;

        // other

        public NamespaceReference Namespace {
            get {
                return (containingNamespace);
            }
        }

        public SimpleTypeReference OuterType {
            get {
                return (containingType);
            }
        }

    }

    // Construction of targets from Xml

    public partial class Target {

        public static Target Create (XmlReader api) {

            string id = api.GetAttribute("id");

            Target target = null;
            api.ReadToFollowing("apidata");
            string group = api.GetAttribute("group");
            switch (group) {
                case "namespace":
                    target = NamespaceTarget.Create(api);
                break;
            }

            target.id = id;

            return (target);
        }

        protected static XPathExpression apiNameExpression = XPathExpression.Compile("string(apidata/@name)");


    }

    public partial class NamespaceTarget {

        public static new NamespaceTarget Create (XmlReader apidata) {
            NamespaceTarget target = new NamespaceTarget();
            string name = apidata.GetAttribute("name");

            // This is not locale-independent.
            if (String.IsNullOrEmpty(target.name)) name = "(Default Namespace)";

            target.name = name;

            return (target);
        }

        public static NamespaceTarget Create (XPathNavigator api) {

            NamespaceTarget target = new NamespaceTarget();
            target.name = (string)api.Evaluate(apiNameExpression);

            // This is not locale-independent.
            if (String.IsNullOrEmpty(target.name)) target.name = "(Default Namespace)";

            return (target);
        }

    }


    public partial class TypeTarget {

        public static new TypeTarget Create (XmlReader api) {

            api.ReadToFollowing("apidata");
            //string subgroup = api.GetAttribute("subgroup");

            api.ReadToFollowing("typedata");
            //string visibilityValue = api.GetAttribute("visibility");
            //string abstractValue = api.GetAttribute("abstract");
            //string sealedValue = api.GetAttribute("sealed");
            //string serializableValue = api.GetAttribute("serealizable");

            api.ReadToFollowing("library");
            string containingAssemblyValue = api.GetAttribute("assembly");

            api.ReadToFollowing("namespace");
            NamespaceReference containingNamespace = NamespaceReference.Create(api);

            TypeTarget target = new TypeTarget();
            target.containingAssembly = containingAssemblyValue;
            target.containingNamespace = containingNamespace;

            return (target);

        }
 
    }

}
