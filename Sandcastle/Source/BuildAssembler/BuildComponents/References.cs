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

    public interface ISimpleReference {

        string Id { get; }

    }

    public abstract partial class Reference { }

    // Namespace

    public partial class NamespaceReference : Reference, ISimpleReference {

        internal NamespaceReference (string id) {
            this.namespaceId = id;
        }

        private string namespaceId;

        public string Id {
            get {
                return (namespaceId);
            }
        }

    }

    // Type

    public abstract partial class TypeReference : Reference { }

    public partial class SimpleTypeReference : TypeReference {

        internal SimpleTypeReference (string id) {
            this.typeId = id;
        }

        private string typeId;

        public string Id {
            get {
                return (typeId);
            }
        }


    }

    public partial class ReferenceTypeReference : TypeReference {

        private TypeReference referedToType;

        public TypeReference ReferedToType {
            get {
                return (referedToType);
            }
        }

        internal ReferenceTypeReference (TypeReference referedToType) {
            if (referedToType == null) throw new ArgumentNullException("referedToType");
            this.referedToType = referedToType;
        }
    }

    public partial class PointerTypeReference : TypeReference {

        internal PointerTypeReference (TypeReference pointedToType) {
            if (pointedToType == null) throw new ArgumentNullException("pointedToType");
            this.pointedToType = pointedToType;
        }

        private TypeReference pointedToType;

        public TypeReference PointedToType {
            get {
                return (pointedToType);
            }
        }

    }

    public partial class ArrayTypeReference : TypeReference {

        internal ArrayTypeReference (TypeReference elementType, int rank) {
            if (elementType == null) throw new ArgumentNullException("elementType");
            if (rank <= 0) throw new ArgumentOutOfRangeException("rank");
            this.elementType = elementType;
            this.rank = rank;
        }

        private int rank;

        private TypeReference elementType;

        public int Rank {
            get {
                return (rank);
            }
        }

        public TypeReference ElementType {
            get {
                return (elementType);
            }
        }

    }

    public class TemplateTypeReference : TypeReference {

        internal TemplateTypeReference (string templateId, int position) {
            if (template == null) throw new ArgumentNullException("template");
            if (position < 0) throw new ArgumentOutOfRangeException("position");
            this.template = null;   // fix this: create a reference
            this.position = position;
        }

        internal TemplateTypeReference (ISimpleReference template, int position) {
            if (template == null) throw new ArgumentNullException("template");
            if (position < 0) throw new ArgumentOutOfRangeException("position");
            this.template = template;
            this.position = position;
        }

        private ISimpleReference template;

        private int position;
    }

    public abstract partial class MemberReference : Reference {}

    public partial class SimpleMemberReference : MemberReference, ISimpleReference {

        internal SimpleMemberReference (string id) {
            if (id == null) throw new ArgumentNullException("id");
            this.memberId = id;
        }

        private string memberId;

        public string Id {
            get {
                return(memberId);
            }
        }



    }




    // ***** XML PARSING ****

    public partial class Reference {

        public static Reference Create (XmlReader element) {
            if (element == null) throw new ArgumentNullException("element");
            switch (element.LocalName) {
                case "namespace":
                    return (NamespaceReference.Create(element));
                case "member":
                return (MemberReference.Create(element));
                default:
                    return (TypeReference.Create(element));
            }
        }

        public static Reference Create (XPathNavigator node) {
            if (node == null) throw new ArgumentNullException("node");
            if (node.NodeType == XPathNodeType.Element) {
                string tag = node.LocalName;
                if (tag == "namespace") return (NamespaceReference.Create(node));
                //if (tag == "member") return (MemberReference.Create(node));
                return (TypeReference.Create(node));
            } else {
                return (null);
            }
        }

        protected static  XPathExpression referenceApiExpression = XPathExpression.Compile("string(@api)");


    }

    public partial class NamespaceReference {

        public static new NamespaceReference Create (XmlReader space) {
            if (space == null) throw new ArgumentNullException("space");

            string api = space.GetAttribute("api");

            return(new NamespaceReference(api));

        }

        public static new NamespaceReference Create (XPathNavigator space) {
            if (space == null) throw new ArgumentNullException("space");
            string api = (string) space.Evaluate(referenceApiExpression);
            return(new NamespaceReference(api));
        }

    }

    public partial class TypeReference {

        public static new TypeReference Create (XmlReader element) {
            if (element == null) throw new ArgumentNullException("element");
            switch (element.LocalName) {
                case "type":
                    // also handle specialization!
                    return(SimpleTypeReference.Create(element));
                case "referenceTo":
                    return(ReferenceTypeReference.Create(element));
                case "pointerTo":
                    return(PointerTypeReference.Create(element));
                case "arrayOf":
                    return(ArrayTypeReference.Create(element));

            }
            return (null);
        }

        public static new TypeReference Create (XPathNavigator element) {
            if (element == null) throw new ArgumentNullException("element");
            string tag = element.LocalName;
            if (tag == "type") {
                bool isSpecialized = (bool)element.Evaluate("boolean(.//specialization)");
                if (isSpecialized) {
                    // deal with specialization!
                    // return (CreateSpecializedTypeReference(element));
                    return (SimpleTypeReference.Create(element));
                } else {
                    return (SimpleTypeReference.Create(element));
                }
            } else if (tag == "arrayOf") {
                string rankValue = element.GetAttribute("rank", String.Empty);
                XPathNavigator elementNode = element.SelectSingleNode("*[1]");
                return (new ArrayTypeReference(TypeReference.Create(elementNode), Convert.ToInt32(rankValue)));
            } else if (tag == "referenceTo") {
                XPathNavigator referedToNode = element.SelectSingleNode("*[1]");
                return (new ReferenceTypeReference(TypeReference.Create(referedToNode)));
            } else if (tag == "pointerTo") {
                XPathNavigator pointedToNode = element.SelectSingleNode("*[1]");
                return (new PointerTypeReference(TypeReference.Create(pointedToNode)));
            } else if (tag == "template") {
                //string nameValue = element.GetAttribute("name", String.Empty);
                string indexValue = element.GetAttribute("index", String.Empty);
                string apiValue = element.GetAttribute("api", String.Empty);
                if ((!String.IsNullOrEmpty(apiValue)) && (!String.IsNullOrEmpty(indexValue))) {
                    return (new TemplateTypeReference(apiValue, Convert.ToInt32(indexValue)));
                    // return (new IndexedTemplateTypeReference(apiValue, Convert.ToInt32(indexValue)));
                } else {
                    throw new InvalidOperationException();
                    // return (new NamedTemplateTypeReference(nameValue));
                }
            }

            throw new InvalidOperationException(String.Format("INVALID '{0}'", tag));
        }

    }

    public partial class SimpleTypeReference {

        public static new SimpleTypeReference Create (XmlReader type) {
            if (type == null) throw new ArgumentNullException("type");
            string api = type.GetAttribute("api");
            return (new SimpleTypeReference(api));
        }

        public static new SimpleTypeReference Create  (XPathNavigator type) {
            if (type == null) throw new ArgumentNullException("type");
            string api = (string)type.Evaluate(referenceApiExpression);
            return(new SimpleTypeReference(api));
        }

    }

    public partial class ReferenceTypeReference {

        public static new ReferenceTypeReference Create (XmlReader referenceTo) {
            if (referenceTo == null) throw new ArgumentException("refernceTo");
            referenceTo.Read();
            TypeReference referedToType = TypeReference.Create(referenceTo);
            return (new ReferenceTypeReference(referedToType));
        }

        public static new ReferenceTypeReference Create (XPathNavigator referenceTo) {
            XPathNavigator referedToNode = referenceTo.SelectSingleNode("*[1]");
            TypeReference referedToType = TypeReference.Create(referedToNode);
            return (new ReferenceTypeReference(referedToType));
        }
    }

    public partial class PointerTypeReference {

        public static new PointerTypeReference Create (XmlReader pointerTo) {
            if (pointerTo == null) throw new ArgumentNullException("pointerTo");
            pointerTo.Read();
            TypeReference pointedToType = TypeReference.Create(pointerTo);
            return (new PointerTypeReference(pointedToType));
        }

        public static new PointerTypeReference Create (XPathNavigator pointerTo) {
            XPathNavigator pointedToNode = pointerTo.SelectSingleNode("*[1]");
            TypeReference pointedToType = TypeReference.Create(pointedToNode);
            return (new PointerTypeReference(pointedToType));
        }
    }

    public partial class ArrayTypeReference {

        public static new ArrayTypeReference Create (XmlReader arrayOf) {
            if (arrayOf == null) throw new ArgumentNullException("arrayOf");

            int rank = 1;
            string rankText = arrayOf.GetAttribute("rank");
            if (!String.IsNullOrEmpty(rankText)) rank = Convert.ToInt32(rankText);

            arrayOf.Read();
            TypeReference elementType = TypeReference.Create(arrayOf);

            return (new ArrayTypeReference(elementType, rank));
        }
    }
}
