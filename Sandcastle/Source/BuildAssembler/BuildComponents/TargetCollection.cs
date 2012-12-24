// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools
{
    // The basic object model here is this:
    //  * Target objects represent files that can be targeted by a reference link
    //  * Different child objects of Target represent different sorts of API targets: Namespace, Type, Member, etc.
    //  * Targets are stored in a TargetCollection
    // To indicate relationships between targets (e.g. a Method takes a particular type parameter), we
    // introduce another set of classes:
    //  * Reference objects refer to a specific target
    //  * Objects like SpecializedTypeReference and ArrayTypeReference that represent decorated types
    // There are two ways to construct such objects:
    //  * XML from a reflection information file defines Target and Reference objects. XmlUtilities does this.
    //  * Code entity reference strings construct Reference objecs. CerUtilities does this.
    // Finally, we need a way to write the link text corresponding to a reference:
    //  * LinkTextResolver contains routines that, given a reference, writes the corresponding link text

    // All arguments of public methods are verified

    // The fact that the creation methods (via XML or CER strings) for references and their rendering methods
    // are separated from the declarations of the reference types goes against OO principals. (The consequent
    // absence of virtual methods also makes for a lot of ugly casting to figure out what method to call.)
    // But there is a reason for it: I wanted all the code that intrepreted XML together, all the code that
    // intrepreted CER strings together, and all the code that did link text renderig together, and I wanted
    // them all separate from each other. I belive this is extremely important for maintainability. It may
    // be possible to leverage partial classes to do this in a more OO fashion.
#if TEST
    public class Test
    {
        public static void Main(string[] args)
        {
            TargetCollection targets = new TargetCollection();

            XPathDocument document = new XPathDocument(args[0]);
            XPathNavigator node = document.CreateNavigator();
            XmlTargetCollectionUtilities.AddTargets(targets, node, LinkType2.Local);
            Console.WriteLine(targets.Count);

            LinkTextResolver resolver = new LinkTextResolver(targets);

            // test writer
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter writer = XmlWriter.Create(Console.Out, settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("types");
            XPathNodeIterator apiNodes = node.Select("/*/apis/api[not(apidata/@subgroup='enumeration')]//*[@display-api]");
            foreach(XPathNavigator apiNode in apiNodes)
            {
                string api = apiNode.GetAttribute("display-api", String.Empty);
                if(api[1] != ':')
                    continue;

                string id = (string)apiNode.Evaluate("string(ancestor::api[1]/@id)");
                TextReferenceUtilities.SetGenericContext(id);

                Reference reference = TextReferenceUtilities.CreateReference(api);

                writer.WriteStartElement("test");
                writer.WriteAttributeString("api", api);
                writer.WriteAttributeString("context", id);

                if(reference == null)
                    writer.WriteString("NULL REFERENCE");
                else
                    resolver.WriteReference(reference, DisplayOptions.ShowContainer |
                        DisplayOptions.ShowTemplates | DisplayOptions.ShowParameters, writer);

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }
    }
#endif

    // contains a collection of targets

    public class TargetCollection
    {

        private Dictionary<string, Target> index = new Dictionary<string, Target>();

        // read the collection

        public Target this[string id]
        {
            get
            {
                Target result;
                index.TryGetValue(id, out result);
                return (result);
            }
        }

        public bool Contains(string id)
        {
            return (index.ContainsKey(id));
        }

        public IEnumerable<string> Ids
        {
            get
            {
                return (index.Keys);
            }
        }

        public IEnumerable<Target> Targets
        {
            get
            {
                return (index.Values);
            }
        }

        public int Count
        {
            get
            {
                return (index.Count);
            }
        }

        // change the collection

        public void Add(Target target)
        {
            index[target.Id] = target;
            //index.Add(target.Id, target);
        }

        public void Clear()
        {
            index.Clear();
        }

    }

    // targets

    public class Target
    {

        internal string id;

        internal string container;

        internal string file;

        internal LinkType2 type;

        public string Id
        {
            get
            {
                return (id);
            }
        }

        public string Container
        {
            get
            {
                return (container);
            }
        }

        public string File
        {
            get
            {
                return (file);
            }
        }

        internal LinkType2 DefaultLinkType
        {
            get
            {
                return (type);
            }
        }

        public virtual void Add(TargetCollection targets)
        {
            targets.Add(this);
        }

    }

    public class NamespaceTarget : Target
    {

        internal string name;

        public string Name
        {
            get
            {
                return (name);
            }
        }

    }

    public class TypeTarget : Target
    {

        internal string name;

        internal NamespaceReference containingNamespace;

        internal SimpleTypeReference containingType;

        internal string[] templates;

        public string Name
        {
            get
            {
                return (name);
            }
        }

        public NamespaceReference Namespace
        {
            get
            {
                return (containingNamespace);
            }
        }

        public SimpleTypeReference OuterType
        {
            get
            {
                return (containingType);
            }
        }

        public string[] Templates
        {
            get
            {
                return (templates);
            }
        }

    }

    public class EnumerationTarget : TypeTarget
    {

        internal MemberTarget[] elements;

        public override void Add(TargetCollection targets)
        {
            base.Add(targets);

            foreach(MemberTarget element in elements)
            {
                element.Add(targets);
            }
        }

    }

    public class MemberTarget : Target
    {

        internal string name;

        internal SimpleTypeReference containingType;

        internal string overload;

        public string Name
        {
            get
            {
                return (name);
            }
        }

        public TypeReference Type
        {
            get
            {
                return (containingType);
            }
        }

        public string OverloadId
        {
            get
            {
                return (overload);
            }
        }

    }

    public class ConstructorTarget : MemberTarget
    {

        internal Parameter[] parameters;

        public Parameter[] Parameters
        {
            get
            {
                return (parameters);
            }
        }

    }

    public class ProcedureTarget : MemberTarget
    {

        internal bool conversionOperator;

        internal MemberReference explicitlyImplements = null;

        public bool ConversionOperator
        {
            get
            {
                return (conversionOperator);
            }
        }

        public MemberReference ExplicitlyImplements
        {
            get
            {
                return (explicitlyImplements);
            }
        }

    }

    public class EventTarget : ProcedureTarget
    {
    }

    public class PropertyTarget : ProcedureTarget
    {
        internal Parameter[] parameters;

        internal TypeReference returnType;

        public Parameter[] Parameters
        {
            get
            {
                return (parameters);
            }
        }

    }

    public class MethodTarget : ProcedureTarget
    {
        internal Parameter[] parameters;

        internal TypeReference returnType;

        internal string[] templates;

        public Parameter[] Parameters
        {
            get
            {
                return (parameters);
            }
        }

        public string[] Templates
        {
            get
            {
                return (templates);
            }
        }

        // property to hold specialized template arguments (used with extension methods)
        internal TypeReference[] templateArgs;
        public TypeReference[] TemplateArgs
        {
            get
            {
                return (templateArgs);
            }
        }

    }

    public class Parameter
    {

        private string name;

        private TypeReference type;

        public string Name
        {
            get
            {
                return (name);
            }
        }

        public TypeReference Type
        {
            get
            {
                return (type);
            }
        }


        internal Parameter(string name, TypeReference type)
        {
            this.name = name;
            this.type = type;
        }

    }

    // ***** Reference objects *****

    public abstract class Reference { }

    public class NamespaceReference : Reference
    {

        private string namespaceId;

        public string Id
        {
            get
            {
                return (namespaceId);
            }
        }

        public Target Resolve(TargetCollection targets)
        {
            return (targets[namespaceId]);
        }

        internal NamespaceReference(string id)
        {
            this.namespaceId = id;
        }

    }

    public abstract class TypeReference : Reference { }

    public class SimpleTypeReference : TypeReference
    {

        private string typeId;

        public string Id
        {
            get
            {
                return (typeId);
            }
        }

        public Target Resolve(TargetCollection targets)
        {
            return (targets[typeId]);
        }

        internal SimpleTypeReference(string id)
        {
            this.typeId = id;
        }

    }

    public class SpecializedTypeReference : TypeReference
    {

        private Specialization[] specializations;

        public Specialization[] Specializations
        {
            get
            {
                return (specializations);
            }
        }

        internal SpecializedTypeReference(Specialization[] specializations)
        {
            if(specializations == null)
                throw new ArgumentNullException("specializations");
            this.specializations = specializations;
        }

        public Dictionary<IndexedTemplateTypeReference, TypeReference> GetSpecializationDictionary()
        {
            Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary = new Dictionary<IndexedTemplateTypeReference, TypeReference>();
            foreach(Specialization specialization in specializations)
            {
                for(int index = 0; index < specialization.Arguments.Length; index++)
                {
                    IndexedTemplateTypeReference template = new IndexedTemplateTypeReference(specialization.TemplateType.Id, index);
                    dictionary.Add(template, specialization.Arguments[index]);
                }
            }
            return (dictionary);
        }

    }

    public class Specialization
    {

        private SimpleTypeReference template;

        private TypeReference[] arguments;

        public SimpleTypeReference TemplateType
        {
            get
            {
                return (template);
            }
        }

        [CLSCompliant(false)]
        public TypeReference[] Arguments
        {
            get
            {
                return arguments;
            }
        }

        internal Specialization(SimpleTypeReference template, TypeReference[] arguments)
        {
            if(template == null)
                throw new ArgumentNullException("template");
            if(arguments == null)
                throw new ArgumentNullException("arguments");
            this.template = template;
            this.arguments = arguments;
        }

    }

    public abstract class TemplateTypeReference : TypeReference { }

    public class IndexedTemplateTypeReference : TemplateTypeReference
    {

        private string templateId;

        private int index;

        public string TemplateId
        {
            get
            {
                return (templateId);
            }
        }

        public int Index
        {
            get
            {
                return (index);
            }
        }

        internal IndexedTemplateTypeReference(string templateId, int index)
        {
            if(templateId == null)
                throw new ArgumentNullException("templateId");
            if(index < 0)
                throw new ArgumentOutOfRangeException("index");
            this.templateId = templateId;
            this.index = index;
        }

        public override int GetHashCode()
        {
            return (index ^ templateId.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            IndexedTemplateTypeReference other = obj as IndexedTemplateTypeReference;
            if(other == null)
                return (false);
            if((this.index == other.index) && (this.templateId == other.templateId))
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }
    }

    public class NamedTemplateTypeReference : TemplateTypeReference
    {

        private string name;

        public string Name
        {
            get
            {
                return (name);
            }
        }

        public NamedTemplateTypeReference(string name)
        {
            this.name = name;
        }

    }

    public class TypeTemplateTypeReference : TemplateTypeReference
    {

        private SimpleTypeReference template;

        private int position;

        public SimpleTypeReference TemplateType
        {
            get
            {
                return (template);
            }
        }

        public int Position
        {
            get
            {
                return (position);
            }
        }

        internal TypeTemplateTypeReference(SimpleTypeReference template, int position)
        {
            if(template == null)
                throw new ArgumentNullException("template");
            if(position < 0)
                throw new ArgumentOutOfRangeException("position");
            this.template = template;
            this.position = position;
        }

    }

    public class MethodTemplateTypeReference : TemplateTypeReference
    {

        private MemberReference template;

        private int position;

        public MemberReference TemplateMethod
        {
            get
            {
                return (template);
            }
        }

        public int Position
        {
            get
            {
                return (position);
            }
        }

        internal MethodTemplateTypeReference(MemberReference template, int position)
        {
            this.template = template;
            this.position = position;
        }

    }

    public class ArrayTypeReference : TypeReference
    {

        private int rank;

        private TypeReference elementType;

        public int Rank
        {
            get
            {
                return (rank);
            }
        }

        public TypeReference ElementType
        {
            get
            {
                return (elementType);
            }
        }

        internal ArrayTypeReference(TypeReference elementType, int rank)
        {
            if(elementType == null)
                throw new ArgumentNullException("elementType");
            if(rank <= 0)
                throw new ArgumentOutOfRangeException("rank");
            this.elementType = elementType;
            this.rank = rank;
        }

    }

    public class ReferenceTypeReference : TypeReference
    {

        private TypeReference referredToType;

        public TypeReference ReferredToType
        {
            get
            {
                return referredToType;
            }
        }

        internal ReferenceTypeReference(TypeReference referredToType)
        {
            if(referredToType == null)
                throw new ArgumentNullException("referredToType");
            this.referredToType = referredToType;
        }
    }

    public class PointerTypeReference : TypeReference
    {

        private TypeReference pointedToType;

        public TypeReference PointedToType
        {
            get
            {
                return (pointedToType);
            }
        }

        internal PointerTypeReference(TypeReference pointedToType)
        {
            if(pointedToType == null)
                throw new ArgumentNullException("pointedToType");
            this.pointedToType = pointedToType;
        }
    }


    public abstract class MemberReference : Reference { }

    /// <summary>
    /// Contains the information to generate the display string for an extension method link
    /// </summary>
    public class ExtensionMethodReference : Reference
    {
        private string methodName;
        public string Name
        {
            get
            {
                return (methodName);
            }
        }

        private Parameter[] parameters;
        public Parameter[] Parameters
        {
            get
            {
                return (parameters);
            }
        }

        private TypeReference[] templateArgs;
        public TypeReference[] TemplateArgs
        {
            get
            {
                return (templateArgs);
            }
        }

        internal ExtensionMethodReference(string methodName, Parameter[] parameters, TypeReference[] templateArgs)
        {
            if(methodName == null)
                throw new ArgumentNullException("methodName");
            this.methodName = methodName;
            this.parameters = parameters;
            this.templateArgs = templateArgs;
        }
    }


    public class SimpleMemberReference : MemberReference
    {

        private string memberId;

        public string Id
        {
            get
            {
                return (memberId);
            }
        }

        public Target Resolve(TargetCollection targets)
        {
            return (targets[memberId]);
        }

        internal SimpleMemberReference(string id)
        {
            if(id == null)
                throw new ArgumentNullException("id");
            this.memberId = id;
        }

    }


    public class SpecializedMemberReference : MemberReference
    {

        private SimpleMemberReference member;

        private SpecializedTypeReference type;

        public SimpleMemberReference TemplateMember
        {
            get
            {
                return (member);
            }
        }

        public SpecializedTypeReference SpecializedType
        {
            get
            {
                return (type);
            }
        }

        internal SpecializedMemberReference(SimpleMemberReference member, SpecializedTypeReference type)
        {
            if(member == null)
                throw new ArgumentNullException("member");
            if(type == null)
                throw new ArgumentNullException("type");
            this.member = member;
            this.type = type;
        }

    }

    public class SpecializedMemberWithParametersReference : MemberReference
    {

        private string prefix;

        private SpecializedTypeReference type;

        private string member;

        private TypeReference[] parameters;

        public string Prefix
        {
            get
            {
                return (prefix);
            }
        }

        public SpecializedTypeReference SpecializedType
        {
            get
            {
                return (type);
            }
        }

        public string MemberName
        {
            get
            {
                return (member);
            }
        }

        public TypeReference[] ParameterTypes
        {
            get
            {
                return (parameters);
            }
        }

        internal SpecializedMemberWithParametersReference(string prefix, SpecializedTypeReference type, string member, TypeReference[] parameters)
        {
            if(type == null)
                throw new ArgumentNullException("type");
            if(parameters == null)
                throw new ArgumentNullException("parameters");
            this.prefix = prefix;
            this.type = type;
            this.member = member;
            this.parameters = parameters;
        }

    }

    public class InvalidReference : Reference
    {

        private string id;

        public String Id
        {
            get
            {
                return (id);
            }
        }

        internal InvalidReference(string id)
        {
            this.id = id;
        }

    }

    // ***** Logic to construct Target & Reference objects from XML reflection data *****
    // Anything that depends on specifics of the XML reflection data format lives here

    public static class XmlTargetCollectionUtilities
    {
        // XPath expressions for extracting data

        // topic data
        private static XPathExpression topicIdExpression = XPathExpression.Compile("string(@id)");
        private static XPathExpression topicContainerExpression = XPathExpression.Compile("string(containers/library/@assembly)");
        private static XPathExpression topicFileExpression = XPathExpression.Compile("string(file/@name)");

        // api data
        private static XPathExpression apiNameExpression = XPathExpression.Compile("string(apidata/@name)");
        private static XPathExpression apiGroupExpression = XPathExpression.Compile("string(apidata/@group)");
        private static XPathExpression apiSubgroupExpression = XPathExpression.Compile("string(apidata/@subgroup)");

        // member data
        private static XPathExpression apiOverloadIdExpression = XPathExpression.Compile("string(overload/@api | memberdata/@overload)");

        // explicit implmentation data
        private static XPathExpression apiIsExplicitImplementationExpression = XPathExpression.Compile("boolean(memberdata/@visibility='private' and proceduredata/@virtual='true' and boolean(implements/member))");
        private static XPathExpression apiImplementedMembersExpression = XPathExpression.Compile("implements/member");

        // op_explicit and op_implicit data
        private static XPathExpression apiIsConversionOperatorExpression = XPathExpression.Compile("boolean((apidata/@subsubgroup='operator') and (apidata/@name='Explicit' or apidata/@name='Implicit'))");

        // container data
        private static XPathExpression apiContainingNamespaceExpression = XPathExpression.Compile("(containers/namespace)[1]");
        private static XPathExpression apiContainingTypeExpression = XPathExpression.Compile("(containers/type)[1]");

        // reference data
        private static XPathExpression referenceApiExpression = XPathExpression.Compile("string(@api)");

        // template data
        private static XPathExpression apiTemplatesExpression = XPathExpression.Compile("templates/template");
        private static XPathExpression templateNameExpression = XPathExpression.Compile("string(@name)");

        // extension method template data
        private static XPathExpression methodTemplateArgsExpression = XPathExpression.Compile("templates/*");

        // Change the container

        public static string ContainerExpression
        {
            get
            {
                return (topicContainerExpression.Expression);
            }
            set
            {
                topicContainerExpression = XPathExpression.Compile(value);
            }
        }

        // super factory method

        public static void AddTargets(TargetCollection targets, XPathNavigator topicsNode, LinkType2 type)
        {
            XPathNodeIterator topicNodes = topicsNode.Select("/*/apis/api[not(topicdata/@notopic)]");
            foreach(XPathNavigator topicNode in topicNodes)
            {
                Target target = CreateTarget(topicNode, type);
                if(target != null)
                    target.Add(targets);
            }
        }

        // Target factory methods

        public static Target CreateTarget(XPathNavigator topic, LinkType2 type)
        {
            if(topic == null)
                throw new ArgumentNullException("topic");

            bool isApiTarget = (bool)topic.Evaluate("boolean(apidata)");

            Target target;
            if(isApiTarget)
            {
                target = CreateApiTarget(topic, type);
            }
            else
            {
                target = new Target();
            }

            if(target == null)
                throw new XmlSchemaValidationException(String.Format(CultureInfo.InvariantCulture,
                    "The target file '{0}' is not valid.", topic.BaseURI));

            target.id = (string)topic.Evaluate(topicIdExpression);
            if(String.IsNullOrEmpty(target.id))
                throw new XmlSchemaValidationException(String.Format(CultureInfo.InvariantCulture,
                    "The target file '{0}' is not valid.", topic.BaseURI));

            target.container = (string)topic.Evaluate(topicContainerExpression);

            target.file = (string)topic.Evaluate(topicFileExpression);
            if(String.IsNullOrEmpty(target.file))
                throw new XmlSchemaValidationException(String.Format(CultureInfo.InvariantCulture, 
                    "The target file '{0}' is not valid.", topic.BaseURI));

            target.type = type;

            return (target);
        }

        private static Target CreateApiTarget(XPathNavigator api, LinkType2 linkType)
        {
            string subGroup = (string)api.Evaluate(apiGroupExpression);
            if(subGroup == "namespace")
            {
                return (CreateNamespaceTarget(api));
            }
            else if(subGroup == "type")
            {
                return (CreateTypeTarget(api, linkType));
            }
            else if(subGroup == "member")
            {
                return (CreateMemberTarget(api));
            }
            else
            {
                return (null);
            }

        }

        private static NamespaceTarget CreateNamespaceTarget(XPathNavigator api)
        {
            NamespaceTarget target = new NamespaceTarget();
            target.name = (string)api.Evaluate(apiNameExpression);
            if(String.IsNullOrEmpty(target.name))
                target.name = "(Default Namespace)";
            return (target);
        }

        private static TypeTarget CreateTypeTarget(XPathNavigator api, LinkType2 linkType)
        {
            string subgroup = (string)api.Evaluate(apiSubgroupExpression);

            TypeTarget target;
            if(subgroup == "enumeration")
            {
                target = CreateEnumerationTarget(api, linkType);
            }
            else
            {
                target = new TypeTarget();
            }

            target.name = (string)api.Evaluate(apiNameExpression);

            // containing namespace
            XPathNavigator namespaceNode = api.SelectSingleNode(apiContainingNamespaceExpression);
            target.containingNamespace = CreateNamespaceReference(namespaceNode);

            // containing type, if any
            XPathNavigator typeNode = api.SelectSingleNode(apiContainingTypeExpression);
            if(typeNode == null)
            {
                target.containingType = null;
            }
            else
            {
                target.containingType = CreateSimpleTypeReference(typeNode);
            }

            // templates
            target.templates = GetTemplateNames(api);

            return (target);
        }

        private static string[] GetTemplateNames(XPathNavigator api)
        {
            List<string> templates = new List<string>();
            XPathNodeIterator templateNodes = api.Select(apiTemplatesExpression);
            foreach(XPathNavigator templateNode in templateNodes)
            {
                templates.Add((string)templateNode.Evaluate(templateNameExpression));
            }
            return (templates.ToArray());
        }

        private static EnumerationTarget CreateEnumerationTarget(XPathNavigator api, LinkType2 linkType)
        {

            EnumerationTarget enumeration = new EnumerationTarget();

            string typeId = (string)api.Evaluate(topicIdExpression);
            string file = (string)api.Evaluate(topicFileExpression);

            // Create tar
            List<MemberTarget> members = new List<MemberTarget>();
            XPathNodeIterator elementNodes = api.Select("elements/element");
            foreach(XPathNavigator elementNode in elementNodes)
            {
                string memberId = elementNode.GetAttribute("api", String.Empty);

                // try to get name from attribute on element node
                string memberName = elementNode.GetAttribute("name", String.Empty);
                if(String.IsNullOrEmpty(memberName))
                {
                    // if we can't do that, try to get the name by searching the file for the <api> element of that member
                    XPathNavigator memberApi = api.SelectSingleNode(String.Format(CultureInfo.InvariantCulture, 
                        "following-sibling::api[@id='{0}']", memberId));

                    if(memberApi != null)
                    {
                        memberName = (string)memberApi.Evaluate(apiNameExpression);
                    }
                    else
                    {
                        // if all else fails, get the name by parsing the identifier
                        string arguments;
                        string type;
                        TextReferenceUtilities.DecomposeMemberIdentifier(memberId, out type, out memberName, out arguments);
                    }
                }

                MemberTarget member = new MemberTarget();
                member.id = memberId; // get Id from element
                member.file = file; // get file from type file
                member.type = linkType;
                member.name = memberName; // get name from element
                member.containingType = new SimpleTypeReference(typeId); // get containing type from this type
                members.Add(member);
            }

            enumeration.elements = members.ToArray();

            return (enumeration);
        }

        public static MemberTarget CreateMemberTarget(XPathNavigator api)
        {

            string subgroup = (string)api.Evaluate(apiSubgroupExpression);

            MemberTarget target;
            if(subgroup == "method")
            {
                target = CreateMethodTarget(api);
            }
            else if(subgroup == "property")
            {
                target = CreatePropertyTarget(api);
            }
            else if(subgroup == "constructor")
            {
                target = CreateConstructorTarget(api);
            }
            else if(subgroup == "event")
            {
                target = CreateEventTarget(api);
            }
            else
            {
                target = new MemberTarget();
            }

            target.name = (string)api.Evaluate(apiNameExpression);
            target.containingType = CreateSimpleTypeReference(api.SelectSingleNode(apiContainingTypeExpression));
            target.overload = (string)api.Evaluate(apiOverloadIdExpression);

            return (target);
        }

        private static MethodTarget CreateMethodTarget(XPathNavigator api)
        {
            MethodTarget target = new MethodTarget();
            target.parameters = CreateParameterList(api);
            target.returnType = CreateReturnType(api);

            target.conversionOperator = (bool)api.Evaluate(apiIsConversionOperatorExpression);

            if((bool)api.Evaluate(apiIsExplicitImplementationExpression))
            {
                target.explicitlyImplements = CreateMemberReference(api.SelectSingleNode(apiImplementedMembersExpression));
            }

            // this selects templates/template or templates/type, because extension methods can have a mix of generic and specialization
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
            target.templateArgs = templateArgumentReferences;

            // get the short name of each template param
            target.templates = GetTemplateNames(api);

            return (target);
        }

        private static PropertyTarget CreatePropertyTarget(XPathNavigator api)
        {
            PropertyTarget target = new PropertyTarget();
            target.parameters = CreateParameterList(api);
            target.returnType = CreateReturnType(api);

            if((bool)api.Evaluate(apiIsExplicitImplementationExpression))
            {
                target.explicitlyImplements = CreateMemberReference(api.SelectSingleNode(apiImplementedMembersExpression));
            }

            return (target);
        }

        private static EventTarget CreateEventTarget(XPathNavigator api)
        {
            EventTarget target = new EventTarget();
            if((bool)api.Evaluate(apiIsExplicitImplementationExpression))
            {
                target.explicitlyImplements = CreateMemberReference(api.SelectSingleNode(apiImplementedMembersExpression));
            }
            return (target);
        }

        private static ConstructorTarget CreateConstructorTarget(XPathNavigator api)
        {
            ConstructorTarget target = new ConstructorTarget();
            target.parameters = CreateParameterList(api);
            return (target);
        }

        private static Parameter[] CreateParameterList(XPathNavigator api)
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
            return (parameters.ToArray());
        }

        private static TypeReference CreateReturnType(XPathNavigator api)
        {
            XPathNavigator returnTypeNode = api.SelectSingleNode("returns/*[1]");
            if(returnTypeNode == null)
            {
                return (null);
            }
            else
            {
                return (CreateTypeReference(returnTypeNode));
            }
        }

        // reference factory

        public static Reference CreateReference(XPathNavigator node)
        {
            if(node == null)
                throw new ArgumentNullException("node");
            if(node.NodeType == XPathNodeType.Element)
            {
                string tag = node.LocalName;
                if(tag == "namespace")
                    return (CreateNamespaceReference(node));
                if(tag == "member")
                    return (CreateMemberReference(node));
                return (CreateTypeReference(node));
            }
            else
            {
                return (null);
            }
        }

        public static NamespaceReference CreateNamespaceReference(XPathNavigator namespaceElement)
        {
            if(namespaceElement == null)
                throw new ArgumentNullException("namespaceElement");
            string api = (string)namespaceElement.Evaluate(referenceApiExpression);
            NamespaceReference reference = new NamespaceReference(api);
            return (reference);
        }

        public static TypeReference CreateTypeReference(XPathNavigator node)
        {
            if(node == null)
                throw new ArgumentNullException("node");

            string tag = node.LocalName;

            if(tag == "type")
            {
                bool isSpecialized = (bool)node.Evaluate("boolean(.//specialization)");
                if(isSpecialized)
                {
                    return (CreateSpecializedTypeReference(node));
                }
                else
                {
                    return (CreateSimpleTypeReference(node));
                }
            }
            else if(tag == "arrayOf")
            {
                string rankValue = node.GetAttribute("rank", String.Empty);
                XPathNavigator elementNode = node.SelectSingleNode("*[1]");

                return (new ArrayTypeReference(CreateTypeReference(elementNode),
                    Convert.ToInt32(rankValue, CultureInfo.InvariantCulture)));
            }
            else if(tag == "referenceTo")
            {
                XPathNavigator referedToNode = node.SelectSingleNode("*[1]");
                return (new ReferenceTypeReference(CreateTypeReference(referedToNode)));
            }
            else if(tag == "pointerTo")
            {
                XPathNavigator pointedToNode = node.SelectSingleNode("*[1]");
                return (new PointerTypeReference(CreateTypeReference(pointedToNode)));
            }
            else if(tag == "template")
            {
                string nameValue = node.GetAttribute("name", String.Empty);
                string indexValue = node.GetAttribute("index", String.Empty);
                string apiValue = node.GetAttribute("api", String.Empty);

                if(!String.IsNullOrEmpty(apiValue) && !String.IsNullOrEmpty(indexValue))
                    return (new IndexedTemplateTypeReference(apiValue, Convert.ToInt32(indexValue,
                        CultureInfo.InvariantCulture)));

                return new NamedTemplateTypeReference(nameValue);
            }

            throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "INVALID '{0}'", tag));
        }

        public static SimpleTypeReference CreateSimpleTypeReference(XPathNavigator node)
        {
            if(node == null)
                throw new ArgumentNullException("node");
            string api = node.GetAttribute("api", String.Empty);
            SimpleTypeReference reference = new SimpleTypeReference(api);
            return (reference);
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
            SpecializedTypeReference reference = new SpecializedTypeReference(specializations.ToArray());
            return (reference);
        }

        private static Specialization CreateSpecialization(XPathNavigator node)
        {
            SimpleTypeReference template = CreateSimpleTypeReference(node);

            List<TypeReference> arguments = new List<TypeReference>();
            XPathNodeIterator specializationNodes = node.Select("specialization/*");
            foreach(XPathNavigator specializationNode in specializationNodes)
            {
                arguments.Add(CreateTypeReference(specializationNode));
            }

            Specialization specialization = new Specialization(template, arguments.ToArray());
            return (specialization);
        }


        public static MemberReference CreateMemberReference(XPathNavigator node)
        {
            string api = node.GetAttribute("api", String.Empty);
            SimpleMemberReference member = new SimpleMemberReference(api);

            bool isSpecialized = (bool)node.Evaluate("boolean(./type//specialization)");
            if(isSpecialized)
            {
                XPathNavigator typeNode = node.SelectSingleNode("type");
                SpecializedTypeReference type = CreateSpecializedTypeReference(typeNode);
                return (new SpecializedMemberReference(member, type));
            }
            else
            {
                return (member);
            }

        }

        /// <summary>
        /// Create an object to store the information to generate the display string for an extension method
        /// </summary>
        /// <param name="node">xml node containing the extension method data</param>
        /// <returns></returns>
        public static ExtensionMethodReference CreateExtensionMethodReference(XPathNavigator node)
        {
            string methodName = (string)node.Evaluate(apiNameExpression);
            Parameter[] parameters = CreateParameterList(node);
            TypeReference[] templateArgumentReferences = null;
            // List<TemplateName> templateNames = new List<TemplateName>();

            // this selects templates/template or templates/type, because extension methods can have a mix of generic and specialization
            // get the short name of each template param or template arg
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
            ExtensionMethodReference extMethod = new ExtensionMethodReference(methodName, parameters, templateArgumentReferences);
            return extMethod;
        }

    }

    // ***** Logic for constructing references from code entity reference strings *****
    // Anything that depends on the specific form the ID strings lives here

    public static class TextReferenceUtilities
    {

        public static Reference CreateReference(string api)
        {
            if(String.IsNullOrEmpty(api))
                throw new ArgumentException("api cannot be null or empty");

            Reference reference = null;

            char start = api[0];

            if(start == 'N')
            {
                reference = CreateNamespaceReference(api);
            }
            else if(start == 'T')
            {
                reference = CreateTypeReference(api);
            }
            else
                reference = CreateMemberReference(api);

            if(reference == null)
                return (new InvalidReference(api));

            return (reference);
        }

        public static NamespaceReference CreateNamespaceReference(string api)
        {
            if(ValidNamespace.IsMatch(api))
            {
                return (new NamespaceReference(api));
            }
            else
            {
                return (null);
            }
        }

        public static TypeReference CreateTypeReference(string api)
        {
            if(ValidSimpleType.IsMatch(api))
            {
                // this is a reference to a "normal" simple type
                return (CreateSimpleTypeReference(api));
            }
            else if(ValidSpecializedType.IsMatch(api))
            {
                // this is a reference to a specialized type
                return (CreateSpecializedTypeReference(api));
            }
            else if(ValidDecoratedType.IsMatch(api))
            {
                // this is a reference to a type that is decorated or is a template
                // process array, reference, and pointer decorations
                char lastCharacter = api[api.Length - 1];
                if(lastCharacter == ']')
                {
                    // arrays
                    int lastBracketPosition = api.LastIndexOf('[');
                    int rank = api.Length - lastBracketPosition - 1;
                    string elementApi = api.Substring(0, lastBracketPosition);
                    TypeReference elementReference = CreateTypeReference(elementApi);
                    return (new ArrayTypeReference(elementReference, rank));
                }
                else if(lastCharacter == '@')
                {
                    // references
                    string referedApi = api.Substring(0, api.Length - 1);
                    TypeReference referedReference = CreateTypeReference(referedApi);
                    return (new ReferenceTypeReference(referedReference));
                }
                else if(lastCharacter == '*')
                {
                    // pointers
                    string pointedApi = api.Substring(0, api.Length - 1);
                    TypeReference pointedReference = CreateTypeReference(pointedApi);
                    return (new PointerTypeReference(pointedReference));
                }

                // process templates
                if(api.StartsWith("T:``", StringComparison.OrdinalIgnoreCase))
                {
                    int position = Convert.ToInt32(api.Substring(4), CultureInfo.InvariantCulture);

                    if(genericTypeContext == null)
                        return (new NamedTemplateTypeReference("UMP"));

                    return (new IndexedTemplateTypeReference(genericTypeContext.Id, position));
                }
                else if(api.StartsWith("T:`", StringComparison.OrdinalIgnoreCase))
                {
                    int position = Convert.ToInt32(api.Substring(3), CultureInfo.InvariantCulture);

                    if(genericTypeContext == null)
                        return (new NamedTemplateTypeReference("UTP"));

                    return (new IndexedTemplateTypeReference(genericTypeContext.Id, position));
                }

                // we shouldn't get here, because one of those test should have been satisfied if the regex matched
                throw new InvalidOperationException("Could not parse valid type expression");

            }
            else
            {
                return (null);
            }
        }

        private static SimpleTypeReference CreateSimpleTypeReference(string api)
        {
            return (new SimpleTypeReference(api));
        }

        private static SpecializedTypeReference CreateSpecializedTypeReference(string api)
        {

            List<Specialization> specializations = new List<Specialization>();

            string text = String.Copy(api);

            // at the moment we are only handling one specialization; need to iterate

            int specializationStart = text.IndexOf('{');
            int specializationEnd = FindMatchingEndBracket(text, specializationStart);
            string list = text.Substring(specializationStart + 1, specializationEnd - specializationStart - 1);
            string[] types = SeparateTypes(list);
            string template = text.Substring(0, specializationStart) + String.Format(CultureInfo.InvariantCulture,
                "`{0}", types.Length);

            SimpleTypeReference templateReference = CreateSimpleTypeReference(template);
            TypeReference[] argumentReferences = new TypeReference[types.Length];
            for(int i = 0; i < types.Length; i++)
            {
                argumentReferences[i] = CreateTypeReference(types[i]);
            }
            Specialization specialization = new Specialization(templateReference, argumentReferences);

            specializations.Add(specialization);

            // end iteration

            return (new SpecializedTypeReference(specializations.ToArray()));
        }

        public static MemberReference CreateMemberReference(string api)
        {
            if(ValidSimpleMember.IsMatch(api))
            {
                // this is just a normal member of a simple type
                return (new SimpleMemberReference(api));
            }
            else if(ValidSpecializedMember.IsMatch(api))
            {
                // this is a member of a specialized type; we need to extract:
                // (1) the underlying specialized type, (2) the member name, (3) the arguments

                // separate the member prefix
                int colonPosition = api.IndexOf(':');
                string prefix = api.Substring(0, colonPosition);
                string text = api.Substring(colonPosition + 1);

                // get the arguments
                string arguments = String.Empty;
                int startParenthesisPosition = text.IndexOf('(');
                if(startParenthesisPosition > 0)
                {
                    int endParenthesisPosition = text.LastIndexOf(')');
                    arguments = text.Substring(startParenthesisPosition + 1, endParenthesisPosition - startParenthesisPosition - 1);
                    text = text.Substring(0, startParenthesisPosition);
                }

                // separate the type and member name
                int lastDotPosition;
                int firstHashPosition = text.IndexOf('#');
                if(firstHashPosition > 0)
                {
                    // if this is an EII, the boundry is at the last dot before the hash
                    lastDotPosition = text.LastIndexOf('.', firstHashPosition);
                }
                else
                {
                    // otherwise, the boundry is at the last dot
                    lastDotPosition = text.LastIndexOf('.');
                }
                string name = text.Substring(lastDotPosition + 1);
                text = text.Substring(0, lastDotPosition);

                // text now contains a specialized generic type; use it to create a reference
                SpecializedTypeReference type = CreateSpecializedTypeReference("T:" + text);

                // If there are no arguments...
                // we simply create a reference to a member whoose identifier we construct in the specialized type
                if(String.IsNullOrEmpty(arguments))
                {
                    string typeId = type.Specializations[type.Specializations.Length - 1].TemplateType.Id;
                    string memberId = String.Format(CultureInfo.InvariantCulture, "{0}:{1}.{2}", prefix,
                        typeId.Substring(2), name);
                    SimpleMemberReference member = new SimpleMemberReference(memberId);
                    return (new SpecializedMemberReference(member, type));
                }

                // If there are arguments... life is not so simple. We can't be sure we can identify the
                // corresponding member of the template type because any particular type that appears in
                // the argument might have come from the template or it might have come from the specialization.
                // We need to create a special kind of reference to handle this situation.
                string[] parameterTypeCers = SeparateTypes(arguments);
                TypeReference[] parameterTypes = new TypeReference[parameterTypeCers.Length];
                for(int i = 0; i < parameterTypeCers.Length; i++)
                {
                    parameterTypes[i] = CreateTypeReference(parameterTypeCers[i]);
                }
                return (new SpecializedMemberWithParametersReference(prefix, type, name, parameterTypes));

            }
            else
                return null;
        }

        // Template context logic

        private static SimpleTypeReference genericTypeContext = null;

        public static void SetGenericContext(string cer)
        {
            // re-set the context
            genericTypeContext = null;

            // get the new context
            Reference context = CreateReference(cer);
            if(context == null)
                return;

            // if it is a type context, set it to be the type context
            SimpleTypeReference typeContext = context as SimpleTypeReference;
            if(typeContext != null)
            {
                genericTypeContext = typeContext;
                return;
            }

            // if it is a member context, set it to be the member context and use it to obtain a type context, too
            SimpleMemberReference memberContext = context as SimpleMemberReference;
            if(memberContext != null)
            {
                string typeId, memberName, arguments;
                DecomposeMemberIdentifier(memberContext.Id, out typeId, out memberName, out arguments);
                genericTypeContext = CreateSimpleTypeReference(typeId);
                return;
            }

        }

        public static SimpleTypeReference GenericContext
        {
            get
            {
                return (genericTypeContext);
            }
        }

        // Code entity reference validation logic

        // iterate -> specializedTypePattern -> decoratedTypePattern -> decoratedTypeListPattern
        // to get a patterns that enforce the contents of specialization brackets

        static TextReferenceUtilities()
        {

            string namePattern = @"[_a-zA-Z0-9]+";

            // namespace patterns

            string namespacePattern = String.Format(CultureInfo.InvariantCulture, @"({0}\.)*({0})?", namePattern);

            string optionalNamespacePattern = String.Format(CultureInfo.InvariantCulture, @"({0}\.)*", namePattern);

            // type patterns

            string simpleTypePattern = String.Format(CultureInfo.InvariantCulture, @"{0}({1}(`\d+)?\.)*{1}(`\d+)?",
                optionalNamespacePattern, namePattern);

            //string specializedTypePattern = String.Format(CultureInfo.InvariantCulture, @"{0}({1}(\{{.+\}})?\.)*{1}(\{{.+\}})?", optionalNamespacePattern, namePattern);
            string specializedTypePattern = String.Format(CultureInfo.InvariantCulture,
                @"({0}(\{{.+\}})?\.)*{0}(\{{.+\}})?", namePattern);

            //string baseTypePattern = String.Format(CultureInfo.InvariantCulture, @"({0})|({1})", simpleTypePattern, specializedTypePattern);

            string decoratedTypePattern = String.Format(CultureInfo.InvariantCulture,
                @"(({0})|(`\d+)|(``\d+))(@|\*|(\[\]))*", specializedTypePattern);

            string decoratedTypeListPattern = String.Format(CultureInfo.InvariantCulture,
                @"({0},)*{0}", decoratedTypePattern);

            string explicitInterfacePattern = String.Format(CultureInfo.InvariantCulture, 
                @"({0}(\{{[^\.]+\}})?#)*", namePattern);

            // members of non-specialized types

            string simpleFieldPattern = String.Format(CultureInfo.InvariantCulture, @"{0}\.{1}",
                simpleTypePattern, namePattern);

            string simpleEventPattern = String.Format(CultureInfo.InvariantCulture, @"{0}\.{1}{2}",
                simpleTypePattern, explicitInterfacePattern, namePattern);

            string simplePropertyPattern = String.Format(CultureInfo.InvariantCulture, @"{0}\.{1}{2}(\({3}\))?",
                simpleTypePattern, explicitInterfacePattern, namePattern, decoratedTypeListPattern);

            string simpleMethodPattern = String.Format(CultureInfo.InvariantCulture,
                @"{0}\.{1}{2}(``\d+)?(\({3}\))?(~{4})?", simpleTypePattern, explicitInterfacePattern,
                namePattern, decoratedTypeListPattern, decoratedTypePattern);

            string simpleConstructorPattern = String.Format(CultureInfo.InvariantCulture, @"{0}\.#ctor(\({1}\))?",
                simpleTypePattern, decoratedTypeListPattern);

            string simpleOverloadPattern = String.Format(CultureInfo.InvariantCulture, @"{0}\.{1}{2}",
                simpleTypePattern, explicitInterfacePattern, namePattern);

            string simpleConstructorOverloadPattern = String.Format(CultureInfo.InvariantCulture, @"{0}\.#ctor",
                simpleTypePattern);

            // members of specialized types

            string specializedFieldPattern = String.Format(CultureInfo.InvariantCulture, @"{0}\.{1}",
                specializedTypePattern, namePattern);

            string specializedEventPattern = String.Format(CultureInfo.InvariantCulture, @"{0}\.{1}{2}",
                specializedTypePattern, explicitInterfacePattern, namePattern);

            string specializedPropertyPattern = String.Format(CultureInfo.InvariantCulture,
                @"{0}\.{1}{2}(\({3}\))?", specializedTypePattern, explicitInterfacePattern, namePattern,
                decoratedTypeListPattern);

            string specializedMethodPattern = String.Format(CultureInfo.InvariantCulture,
                @"{0}\.{1}{2}(``\d+)?(\({3}\))?(~{4})?", specializedTypePattern, explicitInterfacePattern,
                namePattern, decoratedTypeListPattern, decoratedTypePattern);

            string specializedOverloadPattern = String.Format(CultureInfo.InvariantCulture, @"{0}\.{1}{2}",
                specializedTypePattern, explicitInterfacePattern, namePattern);

            // create regexes using this patterns

            ValidNamespace = new Regex(String.Format(CultureInfo.InvariantCulture, @"^N:{0}$", namespacePattern),
                RegexOptions.Compiled);

            ValidSimpleType = new Regex(String.Format(CultureInfo.InvariantCulture, @"^T:{0}$",
                simpleTypePattern), RegexOptions.Compiled);

            ValidDecoratedType = new Regex(String.Format(CultureInfo.InvariantCulture, @"^T:{0}$",
                decoratedTypePattern), RegexOptions.Compiled);

            ValidSpecializedType = new Regex(String.Format(CultureInfo.InvariantCulture, @"^T:{0}$",
                specializedTypePattern), RegexOptions.Compiled);

            ValidSimpleMember = new Regex(String.Format(CultureInfo.InvariantCulture, 
                @"^((M:{0})|(M:{1})|(P:{2})|(F:{3})|(E:{4})|(Overload:{5})|(Overload:{6}))$",
                simpleMethodPattern, simpleConstructorPattern, simplePropertyPattern, simpleFieldPattern, simpleEventPattern, simpleOverloadPattern, simpleConstructorOverloadPattern));

            ValidSpecializedMember = new Regex(String.Format(CultureInfo.InvariantCulture,
                @"^((M:{0})|(P:{1})|(F:{2})|(E:{3})|(Overload:{4}))$", specializedMethodPattern,
                specializedPropertyPattern, specializedFieldPattern, specializedEventPattern,
                specializedOverloadPattern));
        }

        private static Regex ValidNamespace;

        private static Regex ValidSimpleType;

        private static Regex ValidDecoratedType;

        private static Regex ValidSpecializedType;

        private static Regex ValidSimpleMember;

        private static Regex ValidSpecializedMember;

        // Code entity reference string manipulation utilities

        internal static string[] SeparateTypes(string typelist)
        {
            List<string> types = new List<string>();

            int start = 0;
            int specializationCount = 0;
            for(int index = 0; index < typelist.Length; index++)
            {
                switch(typelist[index])
                {
                    case '{':
                    case '[':
                        specializationCount++;
                        break;
                    case '}':
                    case ']':
                        specializationCount--;
                        break;
                    case ',':
                        if(specializationCount == 0)
                        {
                            types.Add("T:" + typelist.Substring(start, index - start).Trim());
                            start = index + 1;
                        }
                        break;
                }
            }
            types.Add("T:" + typelist.Substring(start).Trim());
            return (types.ToArray());
        }

        internal static void DecomposeMemberIdentifier(string memberCer, out string typeCer, out string memberName, out string arguments)
        {

            // drop the member prefix
            int colonPosition = memberCer.IndexOf(':');
            string text = memberCer.Substring(colonPosition + 1);

            // get the arguments
            arguments = String.Empty;
            int startParenthesisPosition = text.IndexOf('(');
            if(startParenthesisPosition > 0)
            {
                int endParenthesisPosition = text.LastIndexOf(')');
                arguments = text.Substring(startParenthesisPosition + 1, endParenthesisPosition - startParenthesisPosition - 1);
                text = text.Substring(0, startParenthesisPosition);
            }

            // separate the type and member name
            int lastDotPosition;
            int firstHashPosition = text.IndexOf('#');
            if(firstHashPosition > 0)
            {
                // if this is an EII, the boundry is at the last dot before the hash
                lastDotPosition = text.LastIndexOf('.', firstHashPosition);
            }
            else
            {
                // otherwise, the boundry is at the last dot
                lastDotPosition = text.LastIndexOf('.');
            }

            memberName = text.Substring(lastDotPosition + 1);
            typeCer = "T:" + text.Substring(0, lastDotPosition);
        }

        private static int FindMatchingEndBracket(string text, int position)
        {

            if(text == null)
                throw new ArgumentNullException("text");

            if((position < 0) || (position >= text.Length))
                throw new ArgumentOutOfRangeException("position", String.Format(CultureInfo.InvariantCulture,
                    "The position {0} is not within the given text string.", position));

            if(text[position] != '{')
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Position {0} " +
                    "of the string '{1}' does not contain and ending curly bracket.", position, text));

            int count = 1;
            for(int index = position + 1; index < text.Length; index++)
            {
                if(text[index] == '{')
                {
                    count++;
                }
                else if(text[index] == '}')
                {
                    count--;
                }

                if(count == 0)
                    return (index);
            }

            throw new FormatException("No opening brace matches the closing brace.");

        }

        // Writing link text for unresolved simple references

        internal static void WriteNamespaceReference(NamespaceReference space, XmlWriter writer)
        {
            writer.WriteString(space.Id.Substring(2));
        }

        internal static void WriteSimpleTypeReference(SimpleTypeReference type, DisplayOptions options, XmlWriter writer)
        {

            // this logic won't correctly deal with nested types, but type cer strings simply don't include that
            // infomation, so this is out best guess under the assumption of a non-nested type

            string cer = type.Id;

            // get the name
            string name;
            int lastDotPosition = cer.LastIndexOf('.');
            if(lastDotPosition > 0)
            {
                // usually, the name will start after the last dot
                name = cer.Substring(lastDotPosition + 1);
            }
            else
            {
                // but if there is no dot, this is a type in the default namespace and the name is everything after the colon
                name = cer.Substring(2);
            }

            // remove any generic tics from the name
            int tickPosition = name.IndexOf('`');
            if(tickPosition > 0)
                name = name.Substring(0, tickPosition);

            if((options & DisplayOptions.ShowContainer) > 0)
            {
                // work out namespace
            }

            writer.WriteString(name);

            if((options & DisplayOptions.ShowTemplates) > 0)
            {
                // work out templates
            }

        }

        internal static void WriteSimpleMemberReference(SimpleMemberReference member, DisplayOptions options, XmlWriter writer, LinkTextResolver resolver)
        {

            string cer = member.Id;

            string typeCer, memberName, arguments;
            DecomposeMemberIdentifier(cer, out typeCer, out memberName, out arguments);

            if((options & DisplayOptions.ShowContainer) > 0)
            {
                SimpleTypeReference type = CreateSimpleTypeReference(typeCer);
                WriteSimpleTypeReference(type, options & ~DisplayOptions.ShowContainer, writer);
            }

            // change this so that we deal with EII names correctly, too
            writer.WriteString(memberName);

            if((options & DisplayOptions.ShowParameters) > 0)
            {
                string[] parameterTypeCers;
                if(String.IsNullOrEmpty(arguments))
                {
                    Parameter[] parameters = new Parameter[0];
                    resolver.WriteMethodParameters(parameters, writer);
                }
                else
                {
                    parameterTypeCers = SeparateTypes(arguments);
                    Parameter[] parameters = new Parameter[parameterTypeCers.Length];
                    for(int i = 0; i < parameterTypeCers.Length; i++)
                    {
                        TypeReference parameterType = CreateTypeReference(parameterTypeCers[i]);
                        if(parameterType == null)
                        {
                            parameterType = new NamedTemplateTypeReference("UAT");
                        }
                        parameters[i] = new Parameter(String.Empty, parameterType);
                    }
                    resolver.WriteMethodParameters(parameters, writer);
                }
            }

        }

    }

    // ***** Link text writing logic *****

    public class LinkTextResolver
    {

        public LinkTextResolver(TargetCollection targets)
        {
            this.targets = targets;
        }

        private TargetCollection targets;

        public void WriteTarget(Target target, DisplayOptions options, XmlWriter writer)
        {
            if(target == null)
                throw new ArgumentNullException("target");

            if(writer == null)
                throw new ArgumentNullException("writer");

            NamespaceTarget space = target as NamespaceTarget;

            if(space != null)
            {
                WriteNamespaceTarget(space, writer);
                return;
            }

            TypeTarget type = target as TypeTarget;

            if(type != null)
            {
                WriteTypeTarget(type, options, writer);
                return;
            }

            MemberTarget member = target as MemberTarget;

            if(member != null)
            {
                WriteMemberTarget(member, options, writer);
                return;
            }

            throw new InvalidOperationException();
        }

        public static void WriteNamespaceTarget(NamespaceTarget space, XmlWriter writer)
        {
            if(space == null)
                throw new ArgumentNullException("space");

            if(writer == null)
                throw new ArgumentNullException("writer");

            writer.WriteString(space.Name);
        }

        public void WriteTypeTarget(TypeTarget type, DisplayOptions options, XmlWriter writer)
        {
            if(type == null)
                throw new ArgumentNullException("type");

            if(writer == null)
                throw new ArgumentNullException("writer");

            WriteTypeTarget(type, options, true, writer);
        }

        private void WriteTypeTarget(TypeTarget type, DisplayOptions options, bool showOuterType, XmlWriter writer)
        {

            // write namespace, if containers are requested
            if((options & DisplayOptions.ShowContainer) > 0)
            {
                WriteNamespace(type.Namespace, writer);
                WriteSeperator(writer);
            }

            // write outer type, if one exists
            if(showOuterType && (type.OuterType != null))
            {
                WriteSimpleType(type.OuterType, DisplayOptions.Default, writer);
                WriteSeperator(writer);
            }

            // write the type name
            writer.WriteString(type.Name);

            // write if template parameters, if they exist and we are requested
            if((options & DisplayOptions.ShowTemplates) > 0)
            {
                WriteTemplateParameters(type.Templates, writer);
            }
        }

        public void WriteMemberTarget(MemberTarget target, DisplayOptions options, XmlWriter writer)
        {
            WriteMemberTarget(target, options, writer, null);
        }


        private void WriteMemberTarget(MemberTarget target, DisplayOptions options, XmlWriter writer, Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary)
        {
            if(target == null)
                throw new ArgumentNullException("target");
            if(writer == null)
                throw new ArgumentNullException("writer");

            if((options & DisplayOptions.ShowContainer) > 0)
            {
                TypeReference type = target.Type;
                WriteType(type, options & ~DisplayOptions.ShowContainer, writer);
                MethodTarget methodTarget = target as MethodTarget;
                if(methodTarget != null)
                {
                    if(methodTarget.conversionOperator)
                    {
                        writer.WriteString(" ");
                    }
                    else
                    {
                        WriteSeperator(writer);
                    }
                }
                else
                {
                    WriteSeperator(writer);
                }
            }

            // special logic for writing methods
            MethodTarget method = target as MethodTarget;
            if(method != null)
            {
                WriteMethod(method, options, writer, dictionary);
                return;
            }

            // special logic for writing properties
            PropertyTarget property = target as PropertyTarget;
            if(property != null)
            {
                WriteProperty(property, options, writer);
                return;
            }

            // special logic for writing constructors
            ConstructorTarget constructor = target as ConstructorTarget;
            if(constructor != null)
            {
                WriteConstructor(constructor, options, writer);
                return;
            }

            // special logic for writing events
            EventTarget trigger = target as EventTarget;

            if(trigger != null)
            {
                WriteEvent(trigger, writer);
                return;
            }

            // by default, just write name
            writer.WriteString(target.Name);
        }

        public void WriteReference(Reference reference, DisplayOptions options, XmlWriter writer)
        {
            if(reference == null)
                throw new ArgumentNullException("reference");
            if(writer == null)
                throw new ArgumentNullException("writer");

            NamespaceReference space = reference as NamespaceReference;

            if(space != null)
            {
                WriteNamespace(space, writer);
                return;
            }

            TypeReference type = reference as TypeReference;
            if(type != null)
            {
                WriteType(type, options, writer);
                return;
            }

            MemberReference member = reference as MemberReference;
            if(member != null)
            {
                WriteMember(member, options, writer);
                return;
            }

            ExtensionMethodReference extMethod = reference as ExtensionMethodReference;
            if(extMethod != null)
            {
                WriteExtensionMethod(extMethod, options, writer);
                return;
            }

            InvalidReference invalid = reference as InvalidReference;

            if(invalid != null)
            {
                WriteInvalid(invalid, writer);
                return;
            }

            throw new InvalidOperationException();
        }

        public void WriteNamespace(NamespaceReference spaceReference, XmlWriter writer)
        {
            if(spaceReference == null)
                throw new ArgumentNullException("spaceReference");

            if(writer == null)
                throw new ArgumentNullException("writer");

            NamespaceTarget spaceTarget = spaceReference.Resolve(targets) as NamespaceTarget;

            if(spaceTarget != null)
                WriteNamespaceTarget(spaceTarget, writer);
            else
                TextReferenceUtilities.WriteNamespaceReference(spaceReference, writer);
        }

        public void WriteType(TypeReference type, DisplayOptions options, XmlWriter writer)
        {
            WriteType(type, options, writer, null);
        }

        private void WriteType(TypeReference type, DisplayOptions options, XmlWriter writer, Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary)
        {

            if(type == null)
                throw new ArgumentNullException("type");
            if(writer == null)
                throw new ArgumentNullException("writer");

            SimpleTypeReference simple = type as SimpleTypeReference;
            if(simple != null)
            {
                WriteSimpleType(simple, options, writer);
                return;
            }

            SpecializedTypeReference specialized = type as SpecializedTypeReference;
            if(specialized != null)
            {
                WriteSpecializedType(specialized, options, writer);
                return;
            }

            ArrayTypeReference array = type as ArrayTypeReference;
            if(array != null)
            {
                WriteArrayType(array, options, writer, dictionary);
                return;
            }

            ReferenceTypeReference reference = type as ReferenceTypeReference;
            if(reference != null)
            {
                WriteReferenceType(reference, options, writer, dictionary);
                return;
            }

            PointerTypeReference pointer = type as PointerTypeReference;
            if(pointer != null)
            {
                WritePointerType(pointer, options, writer, dictionary);
                return;
            }

            TemplateTypeReference template = type as TemplateTypeReference;
            if(template != null)
            {
                WriteTemplateType(template, options, writer, dictionary);
                return;
            }

            throw new InvalidOperationException("Unknown type reference type");

        }

        public void WriteSimpleType(SimpleTypeReference simple, DisplayOptions options, XmlWriter writer)
        {
            WriteSimpleType(simple, options, true, writer);
        }

        private void WriteSimpleType(SimpleTypeReference simple, DisplayOptions options, bool showOuterType, XmlWriter writer)
        {
            TypeTarget type = simple.Resolve(targets) as TypeTarget;
            if(type != null)
            {
                WriteTypeTarget(type, options, showOuterType, writer);
            }
            else
            {
                TextReferenceUtilities.WriteSimpleTypeReference(simple, options, writer);
            }
        }

        private static void WriteTemplateParameters(string[] templates, XmlWriter writer)
        {

            if(templates.Length == 0)
                return;

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "languageSpecificText");
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cs");
            writer.WriteString("<");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "vb");
            writer.WriteString("(Of ");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cpp");
            writer.WriteString("<");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "nu");
            writer.WriteString("(");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "fs");
            writer.WriteString("<'");
            writer.WriteEndElement();

            writer.WriteEndElement();

            for(int i = 0; i < templates.Length; i++)
            {
                if(i > 0)
                    writer.WriteString(", ");
                writer.WriteString(templates[i]);
            }

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "languageSpecificText");
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cs");
            writer.WriteString(">");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "vb");
            writer.WriteString(")");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cpp");
            writer.WriteString(">");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "nu");
            writer.WriteString(")");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "fs");
            writer.WriteString(">");
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        private void WriteSpecializedType(SpecializedTypeReference special, DisplayOptions options, XmlWriter writer)
        {

            Specialization[] specializations = special.Specializations;
            for(int i = 0; i < specializations.Length; i++)
            {
                if(i == 0)
                {
                    WriteSpecialization(specializations[0], options, writer);
                }
                else
                {
                    WriteSeperator(writer);
                    WriteSpecialization(specializations[i], options & ~DisplayOptions.ShowContainer, writer);
                }

            }

        }

        private void WriteSpecialization(Specialization specialization, DisplayOptions options, XmlWriter writer)
        {
            // write the type itself (without outer types, because those will be written be other calls to this routine)
            WriteSimpleType(specialization.TemplateType, (options & ~DisplayOptions.ShowTemplates), false, writer);

            // then write the template arguments
            WriteTemplateArguments(specialization.Arguments, writer);
        }

        private void WriteTemplateArguments(TypeReference[] specialization, XmlWriter writer)
        {

            if(specialization.Length == 0)
                return;

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "languageSpecificText");
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cs");
            writer.WriteString("<");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "vb");
            writer.WriteString("(Of ");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cpp");
            writer.WriteString("<");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "fs");
            writer.WriteString("<'");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "nu");
            writer.WriteString("(");
            writer.WriteEndElement();

            writer.WriteEndElement();

            for(int i = 0; i < specialization.Length; i++)
            {
                if(i > 0)
                    writer.WriteString(", ");
                WriteType(specialization[i], DisplayOptions.Default, writer);
            }

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "languageSpecificText");
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cs");
            writer.WriteString(">");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "vb");
            writer.WriteString(")");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cpp");
            writer.WriteString(">");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "fs");
            writer.WriteString(">");
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "nu");
            writer.WriteString(")");
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void WriteArrayType(ArrayTypeReference reference, DisplayOptions options, XmlWriter writer, Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary)
        {

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "languageSpecificText");
            // C++ array notation (left)
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cpp");
            writer.WriteString("array<");
            writer.WriteEndElement();
            writer.WriteEndElement(); // end of <span class="languageSpecificText"> element

            // the underlying type
            WriteType(reference.ElementType, options, writer, dictionary);

            // C++ array notation (right)
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "languageSpecificText");
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cpp");

            if(reference.Rank > 1)
                writer.WriteString("," + reference.Rank.ToString(CultureInfo.InvariantCulture));

            writer.WriteString(">");
            writer.WriteEndElement();

            // C# array notation
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cs");
            writer.WriteString("[");

            for(int i = 1; i < reference.Rank; i++)
                writer.WriteString(",");

            writer.WriteString("]");
            writer.WriteEndElement();

            // VB array notation
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "vb");
            writer.WriteString("(");

            for(int i = 1; i < reference.Rank; i++)
                writer.WriteString(",");

            writer.WriteString(")");
            writer.WriteEndElement();

            // neutral array notation
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "nu");
            writer.WriteString("[");

            for(int i = 1; i < reference.Rank; i++)
                writer.WriteString(",");

            writer.WriteString("]");
            writer.WriteEndElement();

            // F# array notation
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "fs");
            writer.WriteString("[");

            for(int i = 1; i < reference.Rank; i++)
                writer.WriteString(",");

            writer.WriteString("]");
            writer.WriteEndElement();

            writer.WriteEndElement(); // end of <span class="languageSpecificText"> element
        }

        private static void WriteSeperator(XmlWriter writer)
        {
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "languageSpecificText");
            // C# seperator
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cs");
            writer.WriteString(".");
            writer.WriteEndElement();

            // VB seperator
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "vb");
            writer.WriteString(".");
            writer.WriteEndElement();

            // C++ seperator
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cpp");
            writer.WriteString("::");
            writer.WriteEndElement();

            // neutral seperator
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "nu");
            writer.WriteString(".");
            writer.WriteEndElement();

            // F# seperator
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "fs");
            writer.WriteString(".");
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        private void WritePointerType(PointerTypeReference pointer, DisplayOptions options, XmlWriter writer, Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary)
        {
            WriteType(pointer.PointedToType, options, writer, dictionary);
            writer.WriteString("*");
        }

        private void WriteReferenceType(ReferenceTypeReference reference, DisplayOptions options, XmlWriter writer, Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary)
        {
            WriteType(reference.ReferredToType, options, writer, dictionary);

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "languageSpecificText");
            // add % in C++
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cpp");
            writer.WriteString("%");
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void WriteTemplateType(TemplateTypeReference template, DisplayOptions options, XmlWriter writer)
        {
            WriteTemplateType(template, options, writer, null);
        }

        private void WriteTemplateType(TemplateTypeReference template, DisplayOptions options, XmlWriter writer, Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary)
        {
            // if we have the name, just write it
            NamedTemplateTypeReference namedTemplate = template as NamedTemplateTypeReference;
            if(namedTemplate != null)
            {
                writer.WriteString(namedTemplate.Name);
                return;
            }

            IndexedTemplateTypeReference indexedTemplate = template as IndexedTemplateTypeReference;
            if(indexedTemplate != null)
            {
                if(dictionary != null && dictionary.ContainsKey(indexedTemplate))
                    WriteType(dictionary[indexedTemplate], options, writer);
                else
                    writer.WriteString(GetTemplateName(indexedTemplate.TemplateId, indexedTemplate.Index));

                return;
            }

            TypeTemplateTypeReference typeTemplate = template as TypeTemplateTypeReference;

            if(typeTemplate != null)
            {

                TypeReference value = null;
                if(dictionary != null)
                {
                    IndexedTemplateTypeReference key = new IndexedTemplateTypeReference(typeTemplate.TemplateType.Id, typeTemplate.Position);

                    if(dictionary.ContainsKey(key))
                        value = dictionary[key];
                }

                if(value == null)
                    writer.WriteString(GetTypeTemplateName(typeTemplate.TemplateType, typeTemplate.Position));
                else
                    WriteType(value, options, writer);

                return;
            }

            throw new InvalidOperationException();
        }

        private string GetTemplateName(string templateId, int position)
        {
            Target target = targets[templateId];

            if(target == null)
            {
                return ("UTT");
            }
            else
            {

                TypeTarget type = target as TypeTarget;
                if(type != null)
                {
                    string[] templates = type.Templates;
                    if(templates.Length > position)
                    {
                        return (templates[position]);
                    }
                    else
                    {
                        return ("UTT");
                    }
                }

                MethodTarget method = target as MethodTarget;
                if(method != null)
                {
                    string[] templates = method.Templates;
                    if(templates.Length > position)
                    {
                        return (templates[position]);
                    }
                    else
                    {
                        return ("UTT");
                    }
                }

                return ("UTT");
            }
        }

        private string GetTypeTemplateName(SimpleTypeReference type, int position)
        {
            TypeTarget target = type.Resolve(targets) as TypeTarget;
            if(target != null)
            {
                string[] templates = target.Templates;
                if(templates.Length > position)
                {
                    return (templates[position]);
                }
                else if(target.OuterType != null)
                {
                    return (GetTypeTemplateName(target.OuterType, position));
                }
                else
                {
                    return ("UTT");
                }
            }
            else
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    "Unknown type reference '{0}'", type.Id));
            }
        }

        public void WriteExtensionMethod(ExtensionMethodReference extMethod, DisplayOptions options, XmlWriter writer)
        {
            if(extMethod == null)
                throw new ArgumentNullException("extMethod");
            if(writer == null)
                throw new ArgumentNullException("writer");

            // write the unqualified method name
            writer.WriteString(extMethod.Name);

            // if this is a generic method, write any template params or args
            if(extMethod.TemplateArgs != null && extMethod.TemplateArgs.Length > 0)
            {
                WriteTemplateArguments(extMethod.TemplateArgs, writer);
            }

            // write parameters
            if((options & DisplayOptions.ShowParameters) > 0)
                WriteMethodParameters(extMethod.Parameters, writer);
        }

        public void WriteMember(MemberReference member, DisplayOptions options, XmlWriter writer)
        {

            if(member == null)
                throw new ArgumentNullException("member");
            if(writer == null)
                throw new ArgumentNullException("writer");

            SimpleMemberReference simple = member as SimpleMemberReference;
            if(simple != null)
            {
                WriteSimpleMember(simple, options, writer);
                return;
            }

            SpecializedMemberReference special = member as SpecializedMemberReference;
            if(special != null)
            {
                WriteSpecializedMember(special, options, writer);
                return;
            }

            SpecializedMemberWithParametersReference ugly = member as SpecializedMemberWithParametersReference;
            if(ugly != null)
            {
                WriteSpecializedMemberWithParameters(ugly, options, writer);
                return;
            }

            throw new InvalidOperationException();

        }

        private void WriteSpecializedMember(SpecializedMemberReference member, DisplayOptions options, XmlWriter writer)
        {

            if((options & DisplayOptions.ShowContainer) > 0)
            {
                WriteType(member.SpecializedType, options & ~DisplayOptions.ShowContainer, writer);
                WriteSeperator(writer);
            }

            Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary = member.SpecializedType.GetSpecializationDictionary();
            WriteSimpleMember(member.TemplateMember, options & ~DisplayOptions.ShowContainer, writer, dictionary);

        }

        private void WriteSimpleMember(SimpleMemberReference member, DisplayOptions options, XmlWriter writer)
        {
            WriteSimpleMember(member, options, writer, null);
        }

        private void WriteSimpleMember(SimpleMemberReference member, DisplayOptions options, XmlWriter writer, Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary)
        {
            MemberTarget target = member.Resolve(targets) as MemberTarget;

            if(target != null)
                WriteMemberTarget(target, options, writer, dictionary);
            else
                TextReferenceUtilities.WriteSimpleMemberReference(member, options, writer, this);
        }

        private void WriteProcedureName(ProcedureTarget target, XmlWriter writer)
        {
            MemberReference implements = target.ExplicitlyImplements;

            if(implements == null)
            {
                if(target.conversionOperator)
                    WriteConversionOperator(target, writer);
                else
                    writer.WriteString(target.Name);
            }
            else
                WriteMember(implements, DisplayOptions.ShowContainer, writer);
        }

        private void WriteMethod(MethodTarget target, DisplayOptions options, XmlWriter writer, Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary)
        {
            WriteProcedureName(target, writer);

            if((options & DisplayOptions.ShowTemplates) > 0)
            {
                // if this is a generic method, write any template params or args
                if(target.TemplateArgs != null && target.TemplateArgs.Length > 0)
                    WriteTemplateArguments(target.TemplateArgs, writer);
            }

            if((options & DisplayOptions.ShowParameters) > 0)
            {
                Parameter[] parameters = target.Parameters;

                if(target.ConversionOperator)
                {
                    TypeReference returns = target.returnType;
                    WriteConversionOperatorParameters(parameters, returns, writer, dictionary);
                }
                else
                    WriteMethodParameters(parameters, writer, dictionary);
            }
        }

        private static void WriteConversionOperator(ProcedureTarget target, XmlWriter writer)
        {
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "languageSpecificText");

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cs");
            writer.WriteString(target.Name);
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "vb");
            if(target.name == "Explicit")
            {
                writer.WriteString("Narrowing");
            }
            else if(target.name == "Implicit")
            {
                writer.WriteString("Widening");
            }
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "cpp");
            writer.WriteString(target.name);
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "nu");
            writer.WriteString(target.name);
            writer.WriteEndElement();

            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "fs");
            writer.WriteString(target.name);
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        internal void WriteMethodParameters(Parameter[] parameters, XmlWriter writer)
        {
            WriteMethodParameters(parameters, writer, null);
        }


        private void WriteMethodParameters(Parameter[] parameters, XmlWriter writer, Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary)
        {
            if(parameters.Length > 0)
            {
                writer.WriteString("(");

                // show parameters
                // we need to deal with type template substitutions!
                for(int i = 0; i < parameters.Length; i++)
                {
                    if(i > 0)
                        writer.WriteString(", ");
                    WriteType(parameters[i].Type, DisplayOptions.Default, writer, dictionary);
                }

                writer.WriteString(")");
            }
            else
            {
                writer.WriteStartElement("span");
                writer.WriteAttributeString("class", "languageSpecificText");
                // when there are no parameters, VB shows no parenthesis

                writer.WriteStartElement("span");
                writer.WriteAttributeString("class", "cs");
                writer.WriteString("()");
                writer.WriteEndElement();

                writer.WriteStartElement("span");
                writer.WriteAttributeString("class", "cpp");
                writer.WriteString("()");
                writer.WriteEndElement();

                writer.WriteStartElement("span");
                writer.WriteAttributeString("class", "nu");
                writer.WriteString("()");
                writer.WriteEndElement();

                writer.WriteStartElement("span");
                writer.WriteAttributeString("class", "fs");
                writer.WriteString("()");
                writer.WriteEndElement();

                writer.WriteEndElement();

            }
        }

        private void WriteConversionOperatorParameters(Parameter[] parameters, TypeReference returns, XmlWriter writer, Dictionary<IndexedTemplateTypeReference, TypeReference> dictionary)
        {
            if(parameters.Length > 0 || returns != null)
                writer.WriteString("(");

            if(parameters.Length > 0)
                WriteType(parameters[0].Type, DisplayOptions.Default, writer, dictionary);

            if(parameters.Length > 0 && returns != null)
                writer.WriteString(" to ");

            if(returns != null)
                WriteType(returns, DisplayOptions.Default, writer, dictionary);

            if(parameters.Length > 0 || returns != null)
                writer.WriteString(")");

            if(parameters.Length == 0 && returns == null)
            {
                writer.WriteStartElement("span");
                writer.WriteAttributeString("class", "languageSpecificText");
                // when there are no parameters, VB shows no parenthesis

                writer.WriteStartElement("span");
                writer.WriteAttributeString("class", "cs");
                writer.WriteString("()");
                writer.WriteEndElement();

                writer.WriteStartElement("span");
                writer.WriteAttributeString("class", "cpp");
                writer.WriteString("()");
                writer.WriteEndElement();

                writer.WriteStartElement("span");
                writer.WriteAttributeString("class", "nu");
                writer.WriteString("()");
                writer.WriteEndElement();

                writer.WriteStartElement("span");
                writer.WriteAttributeString("class", "fs");
                writer.WriteString("()");
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }

        private void WriteProperty(PropertyTarget target, DisplayOptions options, XmlWriter writer)
        {
            WriteProcedureName(target, writer);

            if((options & DisplayOptions.ShowParameters) > 0)
            {
                Parameter[] parameters = target.Parameters;

                // VB only shows parenthesis when there are parameters
                if(parameters.Length > 0)
                {
                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "languageSpecificText");
                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "cs");
                    writer.WriteString("[");
                    writer.WriteEndElement();

                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "vb");
                    writer.WriteString("(");
                    writer.WriteEndElement();

                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "cpp");
                    writer.WriteString("[");
                    writer.WriteEndElement();

                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "nu");
                    writer.WriteString("(");
                    writer.WriteEndElement();

                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "fs");
                    writer.WriteString(" ");
                    writer.WriteEndElement();

                    writer.WriteEndElement();

                    // show parameters
                    // we need to deal with type template substitutions!
                    for(int i = 0; i < parameters.Length; i++)
                    {
                        if(i > 0)
                            writer.WriteString(", ");
                        WriteType(parameters[i].Type, DisplayOptions.Default, writer);
                    }

                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "languageSpecificText");
                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "cs");
                    writer.WriteString("]");
                    writer.WriteEndElement();

                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "vb");
                    writer.WriteString(")");
                    writer.WriteEndElement();

                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "cpp");
                    writer.WriteString("]");
                    writer.WriteEndElement();

                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "nu");
                    writer.WriteString(")");
                    writer.WriteEndElement();

                    writer.WriteStartElement("span");
                    writer.WriteAttributeString("class", "fs");
                    writer.WriteString(" ");
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                }

            }

        }

        private void WriteEvent(EventTarget trigger, XmlWriter writer)
        {
            WriteProcedureName(trigger, writer);
        }

        private void WriteConstructor(ConstructorTarget constructor, DisplayOptions options, XmlWriter writer)
        {


            WriteType(constructor.Type, options & ~DisplayOptions.ShowContainer, writer);

            if((options & DisplayOptions.ShowParameters) > 0)
            {
                Parameter[] parameters = constructor.Parameters;
                WriteMethodParameters(parameters, writer);
            }

        }

        private void WriteSpecializedMemberWithParameters(SpecializedMemberWithParametersReference ugly, DisplayOptions options, XmlWriter writer)
        {

            if((options & DisplayOptions.ShowContainer) > 0)
            {
                WriteSpecializedType(ugly.SpecializedType, options & ~DisplayOptions.ShowContainer, writer);
                WriteSeperator(writer);
            }

            writer.WriteString(ugly.MemberName);

            if((options & DisplayOptions.ShowParameters) > 0)
            {

                writer.WriteString("(");

                TypeReference[] parameterTypes = ugly.ParameterTypes;
                for(int i = 0; i < parameterTypes.Length; i++)
                {
                    if(i > 0)
                        writer.WriteString(", ");

                    WriteType(parameterTypes[i], DisplayOptions.Default, writer);
                }

                writer.WriteString(")");
            }
        }

        private static void WriteInvalid(InvalidReference invalid, XmlWriter writer)
        {
            writer.WriteString("[" + invalid.Id + "]");
        }
    }

    [Flags, Serializable]
    public enum DisplayOptions
    {
        ShowContainer = 1,
        ShowTemplates = 2,
        ShowParameters = 4,
        Default = 6
    }

    [Serializable]
    public enum LinkType2
    {
        None,
        Self,
        Local,
        Index,
        LocalOrIndex,
        Msdn,
        Id
    }
}
