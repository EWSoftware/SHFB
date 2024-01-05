// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace
// 12/28/2012 - EFW - Renamed XmlTargetDictionaryUtilities to reflect its usage with TargetDictionary

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Sandcastle.Tools.BuildComponents.Targets
{
    /// <summary>
    /// The logic to construct Target and Reference objects from XML reflection data.  Anything that depends on
    /// specifics of the XML reflection data format lives here.
    /// </summary>
    public static class XmlTargetDictionaryUtilities
    {
        // XPath expressions for extracting data

        // Topic data
        private static readonly XPathExpression topicIdExpression = XPathExpression.Compile("string(@id)");
        private static XPathExpression topicContainerExpression = XPathExpression.Compile("string(containers/library/@assembly)");
        private static readonly XPathExpression topicFileExpression = XPathExpression.Compile("string(file/@name)");

        // API data
        private static readonly XPathExpression apiNameExpression = XPathExpression.Compile("string(apidata/@name)");
        private static readonly XPathExpression apiGroupExpression = XPathExpression.Compile("string(apidata/@group)");
        private static readonly XPathExpression apiSubgroupExpression = XPathExpression.Compile("string(apidata/@subgroup)");

        // Member data
        private static readonly XPathExpression apiOverloadIdExpression = XPathExpression.Compile("string(overload/@api | memberdata/@overload)");

        // explicit implementation data
        private static readonly XPathExpression apiIsExplicitImplementationExpression = XPathExpression.Compile(
            "boolean(proceduredata/@eii='true' and boolean(implements/member))");
        private static readonly XPathExpression apiImplementedMembersExpression = XPathExpression.Compile("implements/member");

        // op_explicit and op_implicit data
        private static readonly XPathExpression apiIsConversionOperatorExpression = XPathExpression.Compile(
            "boolean((apidata/@subsubgroup='operator') and (apidata/@name='Explicit' or apidata/@name='Implicit'))");

        // Container data
        private static readonly XPathExpression apiContainingNamespaceExpression = XPathExpression.Compile("(containers/namespace)[1]");
        private static readonly XPathExpression apiContainingTypeExpression = XPathExpression.Compile("(containers/type)[1]");

        // Reference data
        private static readonly XPathExpression referenceApiExpression = XPathExpression.Compile("string(@api)");

        // Template data
        private static readonly XPathExpression apiTemplatesExpression = XPathExpression.Compile("templates/template");
        private static readonly XPathExpression templateNameExpression = XPathExpression.Compile("string(@name)");

        // Extension method template data
        private static readonly XPathExpression methodTemplateArgsExpression = XPathExpression.Compile("templates/*");

        // Change the container

        /// <summary>
        /// This is used to get or set the container expression
        /// </summary>
        public static string ContainerExpression
        {
            get => topicContainerExpression.Expression;
            set => topicContainerExpression = XPathExpression.Compile(value);
        }

        // super factory method

        /// <summary>
        /// This is used to enumerate targets
        /// </summary>
        /// <param name="topicsNode">The topics node from which to get the target information</param>
        /// <returns>An enumerable list of targets</returns>
        public static IEnumerable<Target> EnumerateTargets(XPathNavigator topicsNode)
        {
            if(topicsNode == null)
                throw new ArgumentNullException(nameof(topicsNode));

            XPathNodeIterator topicNodes = topicsNode.Select("/*/apis/api[not(topicdata/@notopic)]");

            foreach(XPathNavigator topicNode in topicNodes)
            {
                Target target = CreateTarget(topicNode);

                if(target != null)
                    yield return target;
            }
        }

        // Target factory methods

        /// <summary>
        /// Create a target
        /// </summary>
        /// <param name="topic">The topic from which to get the target information</param>
        /// <returns>The target</returns>
        public static Target CreateTarget(XPathNavigator topic)
        {
            if(topic == null)
                throw new ArgumentNullException(nameof(topic));

            bool isApiTarget = (bool)topic.Evaluate("boolean(apidata)");

            Target target;

            if(isApiTarget)
                target = CreateApiTarget(topic);
            else
                target = new Target();

            if(target == null)
                throw new XmlSchemaValidationException(String.Format(CultureInfo.InvariantCulture,
                    "The target file '{0}' is not valid (unknown API target subgroup).", topic.BaseURI));

            target.Id = (string)topic.Evaluate(topicIdExpression);

            if(String.IsNullOrEmpty(target.Id))
                throw new XmlSchemaValidationException(String.Format(CultureInfo.InvariantCulture,
                    "The target file '{0}' is not valid (no target ID specified).", topic.BaseURI));

            target.Container = (string)topic.Evaluate(topicContainerExpression);

            target.File = (string)topic.Evaluate(topicFileExpression);

            // Reflection data sets won't have a filename so just use the ID as the distinct name
            if(String.IsNullOrEmpty(target.File))
                target.File = target.Id;

            return target;
        }

        private static Target CreateApiTarget(XPathNavigator api)
        {
            string subGroup = (string)api.Evaluate(apiGroupExpression);

            if(subGroup == "namespace" || subGroup == "namespaceGroup" )
                return CreateNamespaceTarget(api);

            if(subGroup == "type")
                return CreateTypeTarget(api);

            if(subGroup == "member")
                return CreateMemberTarget(api);

            return null;
        }

        private static NamespaceTarget CreateNamespaceTarget(XPathNavigator api)
        {
            string name = (string)api.Evaluate(apiNameExpression);

            if(String.IsNullOrEmpty(name))
                name = "(Default Namespace)";

            return new NamespaceTarget(name);
        }

        private static TypeTarget CreateTypeTarget(XPathNavigator api)
        {
            string subgroup = (string)api.Evaluate(apiSubgroupExpression);

            TypeTarget target;

            if(subgroup == "enumeration")
                target = CreateEnumerationTarget(api);
            else
                target = new TypeTarget();

            target.Name = (string)api.Evaluate(apiNameExpression);

            // Containing namespace
            XPathNavigator namespaceNode = api.SelectSingleNode(apiContainingNamespaceExpression);
            target.ContainingNamespace = CreateNamespaceReference(namespaceNode);

            // Containing type, if any
            XPathNavigator typeNode = api.SelectSingleNode(apiContainingTypeExpression);

            if(typeNode == null)
                target.ContainingType = null;
            else
                target.ContainingType = CreateSimpleTypeReference(typeNode);

            // Templates
            target.Templates = GetTemplateNames(api);

            return target;
        }

        private static IList<string> GetTemplateNames(XPathNavigator api)
        {
            List<string> templates = new List<string>();

            XPathNodeIterator templateNodes = api.Select(apiTemplatesExpression);

            foreach(XPathNavigator templateNode in templateNodes)
                templates.Add((string)templateNode.Evaluate(templateNameExpression));

            return templates;
        }

        private static EnumerationTarget CreateEnumerationTarget(XPathNavigator api)
        {
            string typeId = (string)api.Evaluate(topicIdExpression);
            string file = (string)api.Evaluate(topicFileExpression);

            // Create target
            List<MemberTarget> members = new List<MemberTarget>();
            XPathNodeIterator elementNodes = api.Select("elements/element");

            foreach(XPathNavigator elementNode in elementNodes)
            {
                string memberId = elementNode.GetAttribute("api", String.Empty);

                // Try to get name from attribute on element node
                string memberName = elementNode.GetAttribute("name", String.Empty);

                if(String.IsNullOrEmpty(memberName))
                {
                    // If we can't do that, try to get the name by searching the file for the <api> element of
                    // that member.
                    XPathNavigator memberApi = api.SelectSingleNode(String.Format(CultureInfo.InvariantCulture,
                        "following-sibling::api[@id='{0}']", memberId));

                    if(memberApi != null)
                        memberName = (string)memberApi.Evaluate(apiNameExpression);
                    else
                    {
                        // If all else fails, get the name by parsing the identifier
                        TextReferenceUtilities.DecomposeMemberIdentifier(memberId, out _, out memberName, out _);
                    }
                }

                MemberTarget member = new MemberTarget
                {
                    Id = memberId,       // Get Id from element
                    File = file,         // Get file from type file
                    Name = memberName,   // Get name from element
                    ContainingType = new SimpleTypeReference(typeId) // Get containing type from this type
                };

                members.Add(member);
            }

            return new EnumerationTarget(members);
        }

        /// <summary>
        /// Create a member target
        /// </summary>
        /// <param name="api">The node from which to get the target information</param>
        /// <returns>The member target</returns>
        public static MemberTarget CreateMemberTarget(XPathNavigator api)
        {
            if(api == null)
                throw new ArgumentNullException(nameof(api));

            string subgroup = (string)api.Evaluate(apiSubgroupExpression);

            MemberTarget target;

            if(subgroup == "method")
                target = CreateMethodTarget(api);
            else
                if(subgroup == "property")
                    target = CreatePropertyTarget(api);
                else
                    if(subgroup == "constructor")
                        target = new ConstructorTarget(CreateParameterList(api));
                    else
                        if(subgroup == "event")
                            target = CreateEventTarget(api);
                        else
                            target = new MemberTarget();

            target.Name = (string)api.Evaluate(apiNameExpression);
            target.ContainingType = CreateSimpleTypeReference(api.SelectSingleNode(apiContainingTypeExpression));
            target.OverloadId = (string)api.Evaluate(apiOverloadIdExpression);

            return target;
        }

        private static MethodTarget CreateMethodTarget(XPathNavigator api)
        {
            MethodTarget target = new MethodTarget(CreateParameterList(api), CreateReturnType(api))
            {
                IsConversionOperator = (bool)api.Evaluate(apiIsConversionOperatorExpression)
            };

            if((bool)api.Evaluate(apiIsExplicitImplementationExpression))
                target.ExplicitlyImplements = CreateMemberReference(api.SelectSingleNode(apiImplementedMembersExpression));

            // This selects templates/template or templates/type, because extension methods can have a mix of
            // generic and specialization.
            XPathNodeIterator templateArgNodes = api.Select(methodTemplateArgsExpression);
            TypeReference[] templateArgumentReferences = null;

            if(templateArgNodes != null && templateArgNodes.Count > 0)
            {
                templateArgumentReferences = new TypeReference[templateArgNodes.Count];
                int i = 0;

                foreach(XPathNavigator templateArgNode in templateArgNodes)
                {
                    templateArgumentReferences[i] = CreateTypeReference(templateArgNode);
                    i++;
                }
            }

            target.TemplateArgs = templateArgumentReferences;

            // Get the short name of each template param
            target.Templates = GetTemplateNames(api);

            return target;
        }

        private static PropertyTarget CreatePropertyTarget(XPathNavigator api)
        {
            PropertyTarget target = new PropertyTarget(CreateParameterList(api), CreateReturnType(api));

            if((bool)api.Evaluate(apiIsExplicitImplementationExpression))
                target.ExplicitlyImplements = CreateMemberReference(api.SelectSingleNode(apiImplementedMembersExpression));

            return target;
        }

        private static EventTarget CreateEventTarget(XPathNavigator api)
        {
            EventTarget target = new EventTarget();

            if((bool)api.Evaluate(apiIsExplicitImplementationExpression))
                target.ExplicitlyImplements = CreateMemberReference(api.SelectSingleNode(apiImplementedMembersExpression));

            return target;
        }

        private static IList<Parameter> CreateParameterList(XPathNavigator api)
        {
            List<Parameter> parameters = new List<Parameter>();
            XPathNodeIterator parameterNodes = api.Select("parameters/parameter");

            foreach(XPathNavigator parameterNode in parameterNodes)
            {
                string name = parameterNode.GetAttribute("name", String.Empty);
                XPathNavigator type = parameterNode.SelectSingleNode("*[1]");
                Parameter parameter = new Parameter(name, CreateTypeReference(type));
                parameters.Add(parameter);
            }

            return parameters;
        }

        private static TypeReference CreateReturnType(XPathNavigator api)
        {
            XPathNavigator returnTypeNode = api.SelectSingleNode("returns/*[1]");

            if(returnTypeNode == null)
                return null;

            return CreateTypeReference(returnTypeNode);
        }

        // reference factory

        /// <summary>
        /// Create a reference
        /// </summary>
        /// <param name="node">The node from which to get the target information</param>
        /// <returns>The reference</returns>
        public static Reference CreateReference(XPathNavigator node)
        {
            if(node == null)
                throw new ArgumentNullException(nameof(node));

            if(node.NodeType == XPathNodeType.Element)
            {
                string tag = node.LocalName;

                if(tag == "namespace")
                    return CreateNamespaceReference(node);

                if(tag == "member")
                    return CreateMemberReference(node);

                return CreateTypeReference(node);
            }

            return null;
        }

        /// <summary>
        /// Create a namespace reference
        /// </summary>
        /// <param name="namespaceElement">The namespace element from which to get the target information</param>
        /// <returns>The namespace reference</returns>
        public static NamespaceReference CreateNamespaceReference(XPathNavigator namespaceElement)
        {
            if(namespaceElement == null)
                throw new ArgumentNullException(nameof(namespaceElement));

            string api = (string)namespaceElement.Evaluate(referenceApiExpression);

            return new NamespaceReference(api);
        }

        /// <summary>
        /// Create a type reference
        /// </summary>
        /// <param name="node">The node from which to get the target information</param>
        /// <returns>The type reference</returns>
        public static TypeReference CreateTypeReference(XPathNavigator node)
        {
            if(node == null)
                throw new ArgumentNullException(nameof(node));

            string tag = node.LocalName;

            if(tag == "type")
            {
                bool isSpecialized = (bool)node.Evaluate("boolean(.//specialization)");

                if(isSpecialized)
                    return CreateSpecializedTypeReference(node);

                return CreateSimpleTypeReference(node);
            }

            if(tag == "arrayOf")
            {
                string rankValue = node.GetAttribute("rank", String.Empty);
                XPathNavigator elementNode = node.SelectSingleNode("*[1]");

                return new ArrayTypeReference(CreateTypeReference(elementNode),
                    Convert.ToInt32(rankValue, CultureInfo.InvariantCulture));
            }
            
            if(tag == "referenceTo")
            {
                XPathNavigator referedToNode = node.SelectSingleNode("*[1]");
                return new ReferenceTypeReference(CreateTypeReference(referedToNode));
            }

            if(tag == "pointerTo")
            {
                XPathNavigator pointedToNode = node.SelectSingleNode("*[1]");
                return new PointerTypeReference(CreateTypeReference(pointedToNode));
            }

            if(tag == "template")
            {
                string nameValue = node.GetAttribute("name", String.Empty);
                string indexValue = node.GetAttribute("index", String.Empty);
                string apiValue = node.GetAttribute("api", String.Empty);

                if(!String.IsNullOrEmpty(apiValue) && !String.IsNullOrEmpty(indexValue))
                    return new IndexedTemplateTypeReference(apiValue, Convert.ToInt32(indexValue,
                        CultureInfo.InvariantCulture));

                return new NamedTemplateTypeReference(nameValue);
            }

            throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "INVALID '{0}'", tag));
        }

        /// <summary>
        /// Create a simple type reference
        /// </summary>
        /// <param name="node">The node from which to get the target information</param>
        /// <returns>The simple type reference</returns>
        public static SimpleTypeReference CreateSimpleTypeReference(XPathNavigator node)
        {
            if(node == null)
                throw new ArgumentNullException(nameof(node));

            string api = node.GetAttribute("api", String.Empty);

            return new SimpleTypeReference(api);
        }

        private static SpecializedTypeReference CreateSpecializedTypeReference(XPathNavigator node)
        {
            Stack<Specialization> specializations = new Stack<Specialization>();
            XPathNavigator typeNode = node.Clone();

            while(typeNode != null)
            {
                specializations.Push(CreateSpecialization(typeNode));
                typeNode = typeNode.SelectSingleNode("type");
            }

            return new SpecializedTypeReference(specializations.ToArray());
        }

        private static Specialization CreateSpecialization(XPathNavigator node)
        {
            SimpleTypeReference template = CreateSimpleTypeReference(node);

            List<TypeReference> arguments = new List<TypeReference>();
            XPathNodeIterator specializationNodes = node.Select("specialization/*");

            foreach(XPathNavigator specializationNode in specializationNodes)
                arguments.Add(CreateTypeReference(specializationNode));

            return new Specialization(template, arguments);
        }

        /// <summary>
        /// Create a member reference
        /// </summary>
        /// <param name="node">The node from which to get the target information</param>
        /// <returns>The member reference</returns>
        public static MemberReference CreateMemberReference(XPathNavigator node)
        {
            if(node == null)
                throw new ArgumentNullException(nameof(node));

            string api = node.GetAttribute("api", String.Empty);
            SimpleMemberReference member = new SimpleMemberReference(api);

            bool isSpecialized = (bool)node.Evaluate("boolean(./type//specialization)");

            if(isSpecialized)
            {
                XPathNavigator typeNode = node.SelectSingleNode("type");
                SpecializedTypeReference type = CreateSpecializedTypeReference(typeNode);

                return new SpecializedMemberReference(member, type);
            }

            return member;
        }

        /// <summary>
        /// Create an object to store the information to generate the display string for an extension method
        /// </summary>
        /// <param name="node">xml node containing the extension method data</param>
        /// <returns>The extension method reference</returns>
        public static ExtensionMethodReference CreateExtensionMethodReference(XPathNavigator node)
        {
            if(node == null)
                throw new ArgumentNullException(nameof(node));

            string methodName = (string)node.Evaluate(apiNameExpression);
            IList<Parameter> parameters = CreateParameterList(node);
            TypeReference[] templateArgumentReferences = null;

            // This selects templates/template or templates/type, because extension methods can have a mix of
            // generic and specialization.  Get the short name of each template param or template arg.
            XPathNodeIterator templateNodes = node.Select(methodTemplateArgsExpression);

            if(templateNodes != null && templateNodes.Count > 0)
            {
                templateArgumentReferences = new TypeReference[templateNodes.Count];
                int i = 0;

                foreach(XPathNavigator templateNode in templateNodes)
                {
                    templateArgumentReferences[i] = CreateTypeReference(templateNode);
                    i++;
                }
            }

            return new ExtensionMethodReference(methodName, parameters, templateArgumentReferences);
        }
    }
}
