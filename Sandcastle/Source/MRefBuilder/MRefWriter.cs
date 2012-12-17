// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 02/09/2012 - EFW - Updated WriteParameter() so that it notes optional parameters indicated by
// OptionalAttribute alone (no default value).
// 11/30/2012 - EFW - Added updates based on changes submitted by ComponentOne to fix crashes caused by
// obfuscated member names.

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Xml;

using System.Compiler;
using Microsoft.Ddue.Tools.Reflection;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// Write out information gained from managed reflection
    /// </summary>
    public class ManagedReflectionWriter : ApiVisitor
    {
        private const string implement = "implement";

        // Implementations

        private const string implements = "implements";

        private Dictionary<string, Object> assemblyNames = new Dictionary<string, Object>();

        // Inheritence

        // Keep track of descendents

        private Dictionary<TypeNode, List<TypeNode>> descendentIndex = new Dictionary<TypeNode, List<TypeNode>>();

        private Dictionary<string, List<MRefBuilderCallback>> endTagCallbacks = new Dictionary<string, List<MRefBuilderCallback>>();

        // Keep track of interface implementors

        private Dictionary<Interface, List<TypeNode>> implementorIndex = new Dictionary<Interface, List<TypeNode>>();

        // private ApiFilter memberFilter = new ExternalDocumentedFilter();

        private bool includeNamespaces = true;

        private ApiNamer namer;

        private List<Member> parsedMembers = new List<Member>();

        private List<Namespace> parsedNamespaces = new List<Namespace>();

        private List<TypeNode> parsedTypes = new List<TypeNode>();

        // add-in callbacks

        private Dictionary<string, List<MRefBuilderCallback>> startTagCallbacks = new Dictionary<string, List<MRefBuilderCallback>>();

        // Stored data

        private XmlWriter writer;

        // Constructor

        public ManagedReflectionWriter(TextWriter output) : this(output, new ExternalTopicFilter()) { }

        public ManagedReflectionWriter(TextWriter output, ApiFilter filter) : this(output, filter, new OrcasNamer()) { }

        public ManagedReflectionWriter(TextWriter output, ApiNamer namer) : this(output, new ExternalTopicFilter(), namer) { }

        public ManagedReflectionWriter(TextWriter output, ApiFilter filter, ApiNamer namer)
            : base(filter)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            writer = XmlWriter.Create(output, settings);

            this.namer = namer;
        }

        // Exposed data

        public ApiNamer ApiNamer
        {
            get
            {
                return (namer);
            }
            set
            {
                namer = value;
            }
        }

        public bool IncludeNamespaces
        {
            get
            {
                return (includeNamespaces);
            }
            set
            {
                includeNamespaces = value;
            }
        }

        public Member[] Members
        {
            get
            {
                return (parsedMembers.ToArray());
            }
        }

        public Namespace[] Namespaces
        {
            get
            {
                return (parsedNamespaces.ToArray());
            }
        }


        public TypeNode[] Types
        {
            get
            {
                return (parsedTypes.ToArray());
            }
        }

        // disposal

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                writer.Close();
            }
            base.Dispose(disposing);
        }

        public void RegisterEndTagCallback(string name, MRefBuilderCallback callback)
        {
            List<MRefBuilderCallback> current;
            if(!endTagCallbacks.TryGetValue(name, out current))
            {
                current = new List<MRefBuilderCallback>();
                endTagCallbacks.Add(name, current);
            }
            current.Add(callback);
        }

        public void RegisterStartTagCallback(string name, MRefBuilderCallback callback)
        {
            List<MRefBuilderCallback> current;
            if(!startTagCallbacks.TryGetValue(name, out current))
            {
                current = new List<MRefBuilderCallback>();
                startTagCallbacks.Add(name, current);
            }
            current.Add(callback);
        }

        public void WriteMember(Member member)
        {
            //Console.WriteLine("Write Member {0} [{1}]", member.FullName, member.DeclaringType.DeclaringModule.Name);
            WriteMember(member, member.DeclaringType);
        }

        public void WriteMemberReference(Member member)
        {
            if(member == null)
                throw new ArgumentNullException("member");
            writer.WriteStartElement("member");
            Member template = ReflectionUtilities.GetTemplateMember(member);

            // !EFW - Change from ComponentOne
            writer.WriteAttributeString("api", namer.GetMemberName(template).TranslateToValidXmlValue());

            if(!member.DeclaringType.IsStructurallyEquivalentTo(template.DeclaringType))
            {
                // !EFW - Change from ComponentOne
                writer.WriteAttributeString("display-api", namer.GetMemberName(member).TranslateToValidXmlValue());
            }

            WriteTypeReference(member.DeclaringType);
            writer.WriteEndElement();
        }

        public void WriteTypeReference(TypeNode type)
        {
            if(type == null)
                throw new ArgumentNullException("type");
            WriteStartTypeReference(type);
            writer.WriteEndElement();
        }

        protected override void VisitMember(Member member)
        {
            //Console.WriteLine("Member: {0}", member.Name);
            parsedMembers.Add(member);

            writer.WriteStartElement("api");

            // !EFW - Change from ComponentOne
            writer.WriteAttributeString("id", namer.GetMemberName(member).TranslateToValidXmlValue());

            StartElementCallbacks("api", member);
            WriteMember(member);
            EndElementCallbacks("api", member);
            writer.WriteEndElement();
        }

        protected override void VisitNamespace(Namespace space)
        {
            parsedNamespaces.Add(space);
            WriteNamespace(space);
            base.VisitNamespace(space);
        }

        // visitation logic

        protected override void VisitNamespaces(NamespaceList spaces)
        {

            // construct a sorted assembly catalog
            foreach(AssemblyNode assembly in this.Assemblies)
            {
                assemblyNames.Add(assembly.StrongName, null);
            }

            // catalog type hierarchy and interface implementors
            for(int i = 0; i < spaces.Count; i++)
            {
                TypeNodeList types = spaces[i].Types;
                for(int j = 0; j < types.Count; j++)
                {
                    TypeNode type = types[j];
                    if(ApiFilter.IsExposedType(type))
                    {
                        if(type.NodeType == NodeType.Class)
                            PopulateDescendentIndex(type);
                        PopulateImplementorIndex(type);
                    }
                }
            }

            // start the document
            writer.WriteStartDocument();
            writer.WriteStartElement("reflection");

            // write assembly info
            writer.WriteStartElement("assemblies");
            foreach(AssemblyNode assembly in this.Assemblies)
            {
                WriteAssembly(assembly);
            }
            writer.WriteEndElement();

            // start api info
            writer.WriteStartElement("apis");
            StartElementCallbacks("apis", spaces);

            // write it
            WriteNamespaces(spaces);
            base.VisitNamespaces(spaces);

            // finish api info
            EndElementCallbacks("apis", spaces);
            writer.WriteEndElement();

            // finish document
            writer.WriteEndElement();
            writer.WriteEndDocument();

        }

        protected override void VisitType(TypeNode type)
        {
            //Console.WriteLine("Type: {0}", type.FullName);
            parsedTypes.Add(type);
            WriteType(type);
            base.VisitType(type);
        }

        // Attributes

        protected void WriteAttributes(AttributeList attributes, SecurityAttributeList securityAttributes)
        {
            AttributeNode[] exposed = GetExposedAttributes(attributes, securityAttributes);
            if(exposed.Length == 0)
                return;
            writer.WriteStartElement("attributes");
            for(int i = 0; i < exposed.Length; i++)
            {
                AttributeNode attribute = exposed[i];
                writer.WriteStartElement("attribute");

                TypeNode type = attribute.Type;
                WriteTypeReference(attribute.Type);
                // WriteStringAttribute("type", namer.GetApiName(attribute.Type));

                ExpressionList expressions = attribute.Expressions;
                for(int j = 0; j < expressions.Count; j++)
                {
                    WriteExpression(expressions[j]);
                }

                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        protected void WriteExpression(Expression expression)
        {
            if(expression.NodeType == NodeType.Literal)
            {
                Literal argument = (Literal)expression;
                writer.WriteStartElement("argument");
                WriteLiteral(argument);
                writer.WriteEndElement();
            }
            else if(expression.NodeType == NodeType.NamedArgument)
            {
                NamedArgument assignment = (NamedArgument)expression;
                Literal value = (Literal)assignment.Value;
                writer.WriteStartElement("assignment");
                WriteStringAttribute("name", assignment.Name.Name);
                WriteLiteral(value);
                writer.WriteEndElement();
            }
        }

        private static FieldList GetAppliedFields(EnumNode enumeration, long value)
        {
            // if a single field matches, return it;
            // otherwise return all fields that are in value
            FieldList list = new FieldList();
            MemberList members = enumeration.Members;
            for(int i = 0; i < members.Count; i++)
            {
                if(members[i].NodeType != NodeType.Field)
                    continue;
                Field field = (Field)members[i];
                if(field.DefaultValue == null)
                    continue;
                long fieldValue = Convert.ToInt64(field.DefaultValue.Value);
                if(fieldValue == value)
                {
                    return (new FieldList(new Field[1] { field }));
                }
                else if((fieldValue & value) == fieldValue)
                {
                    list.Add(field);
                }
            }
            return (list);
        }

        // Static utility functions

        private static Namespace GetNamespace(TypeNode type)
        {
            if(type.DeclaringType != null)
            {
                return (GetNamespace(type.DeclaringType));
            }
            else
            {
                return (new Namespace(type.Namespace));
            }
        }

        private static string GetVisibility(Member api)
        {
            if(api == null)
                throw new ArgumentNullException("api");
            if(api.IsPublic)
            {
                return ("public");
            }
            else if(api.IsAssembly)
            {
                return ("assembly");
            }
            else if(api.IsFamilyOrAssembly)
            {
                return ("family or assembly");
            }
            else if(api.IsFamily)
            {
                return ("family");
            }
            else if(api.IsFamilyAndAssembly)
            {
                return ("family and assembly");
            }
            else if(api.IsPrivate)
            {
                return ("private");
            }
            else
            {
                throw new InvalidOperationException(String.Format("Unknown access level for {0}", api.FullName));
            }
        }

        private static bool IsValidXmlChar(char c)
        {
            if(c < 0x20)
            {
                return ((c == 0x9) || (c == 0xa));
            }
            else
            {
                return ((c <= 0xd7ff) || ((0xe000 <= c) && (c <= 0xfffd)));
            }
        }

        private static bool IsValidXmlText(string text)
        {
            foreach(char c in text)
            {
                if(!IsValidXmlChar(c))
                    return (false);
            }
            return (true);
        }

        private void EndElementCallbacks(string name, Object info)
        {
            List<MRefBuilderCallback> callbacks;
            if(endTagCallbacks.TryGetValue(name, out callbacks))
            {
                foreach(MRefBuilderCallback callback in callbacks)
                    callback.Invoke(writer, info);
            }
        }

        private AttributeNode[] GetExposedAttributes(AttributeList attributes, SecurityAttributeList securityAttributes)
        {
            if(attributes == null)
                Console.WriteLine("null attribute list");
            if(securityAttributes == null)
                Console.WriteLine("null security attribute list");
            List<AttributeNode> exposedAttributes = new List<AttributeNode>();
            for(int i = 0; i < attributes.Count; i++)
            {
                AttributeNode attribute = attributes[i];
                if(attribute == null)
                    Console.WriteLine("null attribute");
                if(this.ApiFilter.IsExposedAttribute(attribute))
                    exposedAttributes.Add(attribute);
            }
            for(int i = 0; i < securityAttributes.Count; i++)
            {
                SecurityAttribute securityAttribute = securityAttributes[i];
                if(securityAttribute == null)
                    Console.WriteLine("null security attribute");
                AttributeList permissionAttributes = securityAttribute.PermissionAttributes;
                //if (permissionAttributes == null) Console.WriteLine("null permission attribute list");
                if(permissionAttributes == null)
                    continue;
                for(int j = 0; j < permissionAttributes.Count; j++)
                {
                    AttributeNode permissionAttribute = permissionAttributes[j];
                    //if (permissionAttribute == null) Console.WriteLine("null permission attribute");
                    // saw an example where this was null; ildasm shows no permission attribute, so skip it
                    if(permissionAttribute == null)
                        continue;
                    if(this.ApiFilter.IsExposedAttribute(permissionAttribute))
                        exposedAttributes.Add(permissionAttribute);
                }
            }
            return (exposedAttributes.ToArray());
        }

        private Member[] GetExposedImplementedMembers(IEnumerable<Member> members)
        {
            List<Member> exposedImplementedMembers = new List<Member>();
            foreach(Member member in members)
            {
                if(this.ApiFilter.IsExposedMember(member))
                {
                    exposedImplementedMembers.Add(member);
                }
            }
            return (exposedImplementedMembers.ToArray());
        }

        private Interface[] GetExposedInterfaces(InterfaceList contracts)
        {
            List<Interface> exposedContracts = new List<Interface>();
            for(int i = 0; i < contracts.Count; i++)
            {
                Interface contract = contracts[i];
                if(this.ApiFilter.IsDocumentedInterface(contract))
                {
                    // if generic, check whether specialization types are exposed
                    exposedContracts.Add(contract);
                }
            }
            return (exposedContracts.ToArray());
        }

        private AttributeNode GetParamArrayAttribute(Parameter param)
        {
            AttributeList attributes = param.Attributes;
            for(int i = 0, n = attributes == null ? 0 : attributes.Count; i < n; i++)
            {
                AttributeNode attr = attributes[i];
                if(attr == null)
                    continue;
                if(attr.Type.FullName == "System.ParamArrayAttribute")
                    return attr;
            }
            return null;
        }

        private void PopulateDescendentIndex(TypeNode child)
        {

            // get the parent of the type in question
            TypeNode parent = child.BaseType;
            if(parent == null)
                return;

            // un-specialize the parent so we see specialized types as children
            parent = ReflectionUtilities.GetTemplateType(parent);

            // get the list of children for that parent (i.e. the sibling list)
            List<TypeNode> siblings;
            if(!descendentIndex.TryGetValue(parent, out siblings))
            {
                siblings = new List<TypeNode>();
                descendentIndex[parent] = siblings;
            }

            // add the type in question to the sibling list
            siblings.Add(child);

        }

        private void PopulateImplementorIndex(TypeNode type)
        {

            // get the list of interfaces exposed by the type
            Interface[] contracts = GetExposedInterfaces(type.Interfaces);

            // for each implemented interface...
            for(int i = 0; i < contracts.Length; i++)
            {

                // get the unspecialized form of the interface
                Interface contract = contracts[i];
                if(contract.IsGeneric)
                    contract = (Interface)ReflectionUtilities.GetTemplateType(contract);

                // get the list of implementors
                List<TypeNode> implementors;
                if(!implementorIndex.TryGetValue(contract, out implementors))
                {
                    implementors = new List<TypeNode>();
                    implementorIndex[contract] = implementors;
                }

                // and add the type to it
                implementors.Add(type);
            }
        }

        private void StartElementCallbacks(string name, Object info)
        {
            List<MRefBuilderCallback> callbacks;
            if(startTagCallbacks.TryGetValue(name, out callbacks))
            {
                foreach(MRefBuilderCallback callback in callbacks)
                    callback.Invoke(writer, info);
            }
        }


        // API data for all entities

        private void WriteApiData(Member api)
        {
            writer.WriteStartElement("apidata");

            string name = api.Name.Name;

            string group = null;
            string subgroup = null;
            string subsubgroup = null;

            if(api.NodeType == NodeType.Namespace)
            {
                group = "namespace";
            }
            else if(api is TypeNode)
            {
                group = "type";

                TypeNode type = (TypeNode)api;
                name = type.GetUnmangledNameWithoutTypeParameters();

                switch(api.NodeType)
                {
                    case NodeType.Class:
                        subgroup = "class";
                        break;
                    case NodeType.Struct:
                        subgroup = "structure";
                        break;
                    case NodeType.Interface:
                        subgroup = "interface";
                        break;
                    case NodeType.EnumNode:
                        subgroup = "enumeration";
                        break;
                    case NodeType.DelegateNode:
                        subgroup = "delegate";
                        break;
                }
            }
            else
            {
                group = "member";

                switch(api.NodeType)
                {
                    case NodeType.Field:
                        subgroup = "field";
                        break;
                    case NodeType.Property:
                        subgroup = "property";
                        break;
                    case NodeType.InstanceInitializer:
                    case NodeType.StaticInitializer:
                        subgroup = "constructor";
                        // name = api.DeclaringType.GetUnmangledNameWithoutTypeParameters();
                        break;
                    case NodeType.Method:
                        subgroup = "method";
                        if((api.IsSpecialName) && (name.StartsWith("op_")))
                        {
                            subsubgroup = "operator";
                            name = name.Substring(3);
                        }
                        break;
                    case NodeType.Event:
                        subgroup = "event";
                        break;
                }

                // Name of EIIs is just interface member name
                int dotIndex = name.LastIndexOf(".");
                if(dotIndex > 0)
                    name = name.Substring(dotIndex + 1);

            }


            WriteStringAttribute("name", name);
            WriteStringAttribute("group", group);
            if(subgroup != null)
                WriteStringAttribute("subgroup", subgroup);
            if(subsubgroup != null)
                WriteStringAttribute("subsubgroup", subsubgroup);

            StartElementCallbacks("apidata", api);

            // WriteStringAttribute("file", GetGuid(namer.GetApiName(api)).ToString());

            EndElementCallbacks("apidata", api);
            writer.WriteEndElement();
        }

        // writing logic

        private void WriteAssembly(AssemblyNode assembly)
        {
            // if (assembly == null) Console.WriteLine("null assembly");
            // Console.WriteLine("assembly: {0}", assembly.Name);
            writer.WriteStartElement("assembly");
            // if (assembly.Name == null) Console.WriteLine("null assembly name");
            WriteStringAttribute("name", assembly.Name);
            // if (assembly.Version == null) Console.WriteLine("null assembly version");

            // basic assembly data
            writer.WriteStartElement("assemblydata");
            WriteStringAttribute("version", assembly.Version.ToString());
            WriteStringAttribute("culture", assembly.Culture.ToString());
            byte[] key = assembly.PublicKeyOrToken;
            writer.WriteStartAttribute("key");
            writer.WriteBinHex(key, 0, key.Length);
            writer.WriteEndAttribute();
            WriteStringAttribute("hash", assembly.HashAlgorithm.ToString());
            writer.WriteEndElement();

            // assembly attribute data
            WriteAttributes(assembly.Attributes, assembly.SecurityAttributes);

            writer.WriteEndElement();
        }

        private void WriteAssignment(NamedArgument assignment)
        {
            string name = assignment.Name.Name;
            Literal value = (Literal)assignment.Value;
            writer.WriteStartElement("assignment");
            WriteStringAttribute("name", name);
            WriteLiteral(value);
            writer.WriteEndElement();
        }

        // utilities used to write attributes

        private void WriteBooleanAttribute(string attribute, bool value)
        {
            if(value)
            {
                writer.WriteAttributeString(attribute, "true");
            }
            else
            {
                writer.WriteAttributeString(attribute, "false");
            }
        }

        private void WriteBooleanAttribute(string attribute, bool value, bool defaultValue)
        {
            if(value != defaultValue)
            {
                WriteBooleanAttribute(attribute, value);
            }
        }

        private void WriteEnumerationData(EnumNode enumeration)
        {
            TypeNode underlying = enumeration.UnderlyingType;
            if(underlying.FullName != "System.Int32")
            {
                writer.WriteStartElement("enumerationbase");
                WriteTypeReference(enumeration.UnderlyingType);
                writer.WriteEndElement();
            }
        }

        private void WriteEventData(Event trigger)
        {

            Method adder = trigger.HandlerAdder;
            Method remover = trigger.HandlerRemover;
            Method caller = trigger.HandlerCaller;

            WriteProcedureData(adder, trigger.OverriddenMember);

            writer.WriteStartElement("eventdata");
            if(adder != null)
                WriteBooleanAttribute("add", true);
            if(remover != null)
                WriteBooleanAttribute("remove", true);
            if(caller != null)
                WriteBooleanAttribute("call", true);
            writer.WriteEndElement();

            if(adder != null)
            {
                writer.WriteStartElement("adder");
                WriteStringAttribute("name", string.Format("add_{0}", trigger.Name.Name));

                WriteAttributes(adder.Attributes, adder.SecurityAttributes);
                writer.WriteEndElement();
            }
            if(remover != null)
            {
                writer.WriteStartElement("remover");
                WriteStringAttribute("name", string.Format("remove_{0}", trigger.Name.Name));

                WriteAttributes(remover.Attributes, remover.SecurityAttributes);
                writer.WriteEndElement();
            }

            writer.WriteStartElement("eventhandler");
            WriteTypeReference(trigger.HandlerType);
            writer.WriteEndElement();

            // handlers should always be elegates, but I have seen a case where one is not, so check for this
            DelegateNode handler = trigger.HandlerType as DelegateNode;
            if(handler != null)
            {
                ParameterList parameters = handler.Parameters;

                if((parameters != null) && (parameters.Count == 2) && (parameters[0].Type.FullName == "System.Object"))
                {
                    writer.WriteStartElement("eventargs");
                    WriteTypeReference(parameters[1].Type);
                    writer.WriteEndElement();
                }
            }

        }


        private void WriteFieldData(Field field)
        {
            writer.WriteStartElement("fielddata");
            WriteBooleanAttribute("literal", field.IsLiteral);
            WriteBooleanAttribute("initonly", field.IsInitOnly);
            WriteBooleanAttribute("volatile", field.IsVolatile, false);
            WriteBooleanAttribute("serialized", (field.Flags & FieldFlags.NotSerialized) == 0);
            writer.WriteEndElement();
        }

        private void WriteGenericParameter(TypeNode templateParameter)
        {

            ITypeParameter itp = (ITypeParameter)templateParameter;

            writer.WriteStartElement("template");

            // !EFW - Change from ComponentOne
            writer.WriteAttributeString("name", templateParameter.Name.Name.TranslateToValidXmlValue());

            // evaluate constraints
            bool reference = ((itp.TypeParameterFlags & TypeParameterFlags.ReferenceTypeConstraint) > 0);
            bool value = ((itp.TypeParameterFlags & TypeParameterFlags.ValueTypeConstraint) > 0);
            bool constructor = ((itp.TypeParameterFlags & TypeParameterFlags.DefaultConstructorConstraint) > 0);
            bool contravariant = ((itp.TypeParameterFlags & TypeParameterFlags.Contravariant) > 0);
            bool covariant = ((itp.TypeParameterFlags & TypeParameterFlags.Covariant) > 0);
            InterfaceList interfaces = templateParameter.Interfaces;
            TypeNode parent = templateParameter.BaseType;

            // no need to show inheritance from ValueType if value flag is set
            if(value && (parent != null) && (parent.FullName == "System.ValueType"))
                parent = null;

            if((parent != null) || (interfaces.Count > 0) || reference || value || constructor)
            {
                writer.WriteStartElement("constrained");
                if(reference)
                    WriteBooleanAttribute("ref", true);
                if(value)
                    WriteBooleanAttribute("value", true);
                if(constructor)
                    WriteBooleanAttribute("ctor", true);
                if(parent != null)
                    WriteTypeReference(parent);
                WriteInterfaces(interfaces);
                writer.WriteEndElement();
            }
            if(covariant || contravariant)
            {
                writer.WriteStartElement("variance");
                if(contravariant)
                    WriteBooleanAttribute("contravariant", true);
                if(covariant)
                    WriteBooleanAttribute("covariant", true);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private void WriteSpecializedTemplateArguments(TypeNodeList templateArguments)
        {
            if(templateArguments == null)
                return;
            if(templateArguments.Count == 0)
                return;
            writer.WriteStartElement("templates");
            for(int i = 0; i < templateArguments.Count; i++)
            {
                WriteTypeReference(templateArguments[i]);
            }
            writer.WriteEndElement();
        }

        // Generic Parameters

        private void WriteGenericParameters(TypeNodeList templateParameters)
        {
            if(templateParameters == null)
                return;
            if(templateParameters.Count == 0)
                return;
            writer.WriteStartElement("templates");
            for(int i = 0; i < templateParameters.Count; i++)
            {
                WriteGenericParameter(templateParameters[i]);
            }
            writer.WriteEndElement();
        }

        private void WriteHierarchy(TypeNode type)
        {

            writer.WriteStartElement("family");

            // write ancestors
            writer.WriteStartElement("ancestors");
            for(TypeNode ancestor = type.BaseType; ancestor != null; ancestor = ancestor.BaseType)
            {
                WriteTypeReference(ancestor);
            }
            writer.WriteEndElement();

            // write descendents
            if(descendentIndex.ContainsKey(type))
            {
                List<TypeNode> descendents = descendentIndex[type];
                writer.WriteStartElement("descendents");
                foreach(TypeNode descendent in descendents)
                    WriteTypeReference(descendent);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();

        }

        private void WriteImplementedMembers(Member[] members)
        {
            if((members == null) || (members.Length == 0))
                return;
            Member[] exposedMembers = GetExposedImplementedMembers(members);
            if((exposedMembers == null) || (exposedMembers.Length == 0))
                return;
            writer.WriteStartElement(implements);
            for(int i = 0; i < exposedMembers.Length; i++)
            {
                Member member = exposedMembers[i];
                //TypeNode type = member.DeclaringType;

                WriteMemberReference(member);

            }
            writer.WriteEndElement();
        }

        // Interfaces

        private void WriteImplementors(Interface contract)
        {
            List<TypeNode> implementors;
            if(!implementorIndex.TryGetValue(contract, out implementors))
                return;
            if((implementors == null) || (implementors.Count == 0))
                return;
            writer.WriteStartElement("implementors");
            StartElementCallbacks("implementors", implementors);
            foreach(TypeNode implementor in implementors)
            {
                WriteTypeReference(implementor);
            }
            writer.WriteEndElement();
            EndElementCallbacks("implementors", implementors);
        }

        private void WriteInterface(Interface contract)
        {
            // writer.WriteStartElement("implement");
            WriteTypeReference(contract);
            // writer.WriteAttributeString("interface", namer.GetTypeName(contract));
            // writer.WriteEndElement();
        }

        private void WriteInterfaces(InterfaceList contracts)
        {
            Interface[] implementedContracts = GetExposedInterfaces(contracts);
            if(implementedContracts.Length == 0)
                return;
            writer.WriteStartElement("implements");
            StartElementCallbacks("implements", implementedContracts);
            for(int i = 0; i < implementedContracts.Length; i++)
            {
                WriteInterface(implementedContracts[i]);
            }
            writer.WriteEndElement();
            EndElementCallbacks("implements", implementedContracts);
        }

        private void WriteLibraryReference(Module module)
        {
            AssemblyNode assembly = module.ContainingAssembly;
            writer.WriteStartElement("library");
            WriteStringAttribute("assembly", assembly.Name);
            WriteStringAttribute("module", module.Name);
            WriteStringAttribute("kind", module.Kind.ToString());
            writer.WriteEndElement();
        }

        private void WriteLiteral(Literal literal)
        {
            WriteLiteral(literal, true);
        }

        private void WriteLiteral(Literal literal, bool showType)
        {
            TypeNode type = literal.Type;
            Object value = literal.Value;
            if(showType)
                WriteTypeReference(type);
            if(value == null)
            {
                writer.WriteElementString("nullValue", String.Empty);
            }
            else
            {
                if(type.NodeType == NodeType.EnumNode)
                {
                    EnumNode enumeration = (EnumNode)type;
                    FieldList fields = GetAppliedFields(enumeration, Convert.ToInt64(value));
                    writer.WriteStartElement("enumValue");
                    for(int i = 0; i < fields.Count; i++)
                    {
                        writer.WriteStartElement("field");

                        // !EFW - Change from ComponentOne
                        writer.WriteAttributeString("name", fields[i].Name.Name.TranslateToValidXmlValue());
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
                else if(type.FullName == "System.Type")
                {
                    writer.WriteStartElement("typeValue");
                    WriteTypeReference((TypeNode)value);
                    writer.WriteEndElement();
                }
                else
                {
                    string text = value.ToString();
                    if(!IsValidXmlText(text))
                        text = String.Empty;
                    writer.WriteElementString("value", text);
                }
            }
        }


        private void WriteMember(Member member, TypeNode type)
        {
            //writer.WriteStartElement("api");
            //writer.WriteAttributeString("id", namer.GetMemberName(member));
            //Console.WriteLine("member: {0}", namer.GetMemberName(member));

            WriteApiData(member);
            WriteMemberData(member);

            SecurityAttributeList securityAttributes = new SecurityAttributeList();
            switch(member.NodeType)
            {
                case NodeType.Field:
                    Field field = (Field)member;
                    WriteFieldData(field);
                    WriteValue(field.Type);
                    // write enumeration field values; expand later to all literal field values?
                    if(field.DeclaringType.NodeType == NodeType.EnumNode)
                    {
                        WriteLiteral(new Literal(field.DefaultValue.Value, ((EnumNode)field.DeclaringType).UnderlyingType), false);
                    }
                    break;
                case NodeType.Method:
                    Method method = (Method)member;
                    WriteMethodData(method);

                    // write the templates node with either the generic template params or the specialized template arguments
                    if(method.TemplateArguments != null)
                        WriteSpecializedTemplateArguments(method.TemplateArguments);
                    else
                        WriteGenericParameters(method.TemplateParameters);

                    WriteParameters(method.Parameters);
                    WriteValue(method.ReturnType);
                    WriteImplementedMembers(ReflectionUtilities.GetImplementedMethods(method));

                    if(method.SecurityAttributes != null)
                        securityAttributes = method.SecurityAttributes;
                    break;
                case NodeType.Property:
                    Property property = (Property)member;
                    WritePropertyData(property);
                    WriteParameters(property.Parameters);
                    WriteValue(property.Type);
                    WriteImplementedMembers(ReflectionUtilities.GetImplementedProperties(property));
                    break;
                case NodeType.Event:
                    Event trigger = (Event)member;
                    WriteEventData(trigger);
                    WriteImplementedMembers(ReflectionUtilities.GetImplementedEvents(trigger));
                    break;
                case NodeType.InstanceInitializer:
                case NodeType.StaticInitializer:
                    Method constructor = (Method)member;
                    WriteParameters(constructor.Parameters);
                    break;

            }

            WriteMemberContainers(member, type);

            WriteAttributes(member.Attributes, securityAttributes);

            //writer.WriteEndElement();
        }

        private void WriteMemberContainers(Member member, TypeNode type)
        {
            writer.WriteStartElement("containers");
            WriteLibraryReference(type.DeclaringModule);
            WriteNamespaceReference(GetNamespace(type));
            WriteTypeReference(type);
            writer.WriteEndElement();
        }

        // Member data

        private void WriteMemberData(Member member)
        {

            writer.WriteStartElement("memberdata");

            WriteStringAttribute("visibility", GetVisibility(member));
            WriteBooleanAttribute("static", member.IsStatic, false);
            WriteBooleanAttribute("special", member.IsSpecialName, false);

            // check overload status 
            // don't do this anymore: overload is a doc model concept and may be need to be tweaked afer versioning

            WriteBooleanAttribute("default", ReflectionUtilities.IsDefaultMember(member), false);

            StartElementCallbacks("memberdata", member);

            EndElementCallbacks("memberdata", member);
            writer.WriteEndElement();
        }

        private void WriteMethodData(Method method)
        {

            WriteProcedureData(method, method.OverriddenMember);

            // writer.WriteStartElement("methoddata");
            // writer.WriteEndElement();
        }

        private void WriteNamespace(Namespace space)
        {
            writer.WriteStartElement("api");

            // !EFW - Change from ComponentOne
            writer.WriteAttributeString("id", namer.GetNamespaceName(space).TranslateToValidXmlValue());
            StartElementCallbacks("api", space);

            WriteApiData(space);
            WriteNamespaceElements(space);

            EndElementCallbacks("api", space);
            writer.WriteEndElement();
        }

        // Members

        private void WriteNamespaceElements(Namespace space)
        {
            TypeNodeList types = space.Types;
            if(types.Count == 0)
                return;
            writer.WriteStartElement("elements");
            for(int i = 0; i < types.Count; i++)
            {
                TypeNode type = types[i];
                //skip hidden types, but if a type is not exposed and has exposed members we must add it
                if(!ApiFilter.IsExposedType(type) && !ApiFilter.HasExposedMembers(type))
                    continue;
                writer.WriteStartElement("element");

                // !EFW - Change from ComponentOne
                writer.WriteAttributeString("api", namer.GetTypeName(type).TranslateToValidXmlValue());

                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private void WriteNamespaceReference(Namespace space)
        {
            writer.WriteStartElement("namespace");

            // !EFW - Change from ComponentOne
            writer.WriteAttributeString("api", namer.GetNamespaceName(space).TranslateToValidXmlValue());

            writer.WriteEndElement();
        }

        private void WriteNamespaces(NamespaceList spaces)
        {
            // This is a part of the doc model; don't do this anymore
        }

        private void WriteParameter(Parameter parameter)
        {
            writer.WriteStartElement("parameter");

            // !EFW - Change from ComponentOne
            writer.WriteAttributeString("name", parameter.Name.Name.TranslateToValidXmlValue());

            if(parameter.IsIn)
                WriteBooleanAttribute("in", true);

            if(parameter.IsOut)
                WriteBooleanAttribute("out", true);

            if(GetParamArrayAttribute(parameter) != null)
                WriteBooleanAttribute("params", true);

            // !EFW - Support optional parameters noted by OptionalAttribute alone (no default value)
            if(parameter.IsOptional)
                WriteBooleanAttribute("optional", true);

            WriteTypeReference(parameter.Type);

            if(parameter.IsOptional && parameter.DefaultValue != null)
                WriteExpression(parameter.DefaultValue);

            writer.WriteEndElement();
        }

        // Parameters

        private void WriteParameters(ParameterList parameters)
        {
            if(parameters.Count == 0)
                return;
            writer.WriteStartElement("parameters");
            for(int i = 0; i < parameters.Count; i++)
            {
                WriteParameter(parameters[i]);
            }
            writer.WriteEndElement();
        }

        private void WriteProcedureData(Method method, Member overrides)
        {
            writer.WriteStartElement("proceduredata");

            WriteBooleanAttribute("abstract", method.IsAbstract, false);
            WriteBooleanAttribute("virtual", method.IsVirtual);
            WriteBooleanAttribute("final", method.IsFinal, false);
            WriteBooleanAttribute("varargs", method.CallingConvention == CallingConventionFlags.VarArg, false);


            if(method.IsPrivate && method.IsVirtual)
                WriteBooleanAttribute("eii", true);

            writer.WriteEndElement();

            if(overrides != null)
            {
                writer.WriteStartElement("overrides");
                WriteMemberReference(overrides);
                // WriteStringAttribute("overrides", namer.GetMemberName(overrides));
                writer.WriteEndElement();
            }

        }

        private void WritePropertyData(Property property)
        {

            string property_visibility = GetVisibility(property);

            Method getter = property.Getter;
            Method setter = property.Setter;

            Method accessor = getter;
            if(accessor == null)
                accessor = setter;

            // procedure data
            WriteProcedureData(accessor, property.OverriddenMember);

            // property data
            writer.WriteStartElement("propertydata");
            if(getter != null)
            {
                WriteBooleanAttribute("get", true);
                string getter_visibility = GetVisibility(getter);

                if(getter_visibility != property_visibility)
                    WriteStringAttribute("get-visibility", getter_visibility);
            }
            if(setter != null)
            {
                WriteBooleanAttribute("set", true);
                string setter_visibility = GetVisibility(setter);
                if(setter_visibility != property_visibility)
                    WriteStringAttribute("set-visibility", setter_visibility);
            }
            writer.WriteEndElement();

            if(getter != null)
            {
                writer.WriteStartElement("getter");
                WriteStringAttribute("name", string.Format("get_{0}", property.Name.Name));

                WriteAttributes(getter.Attributes, getter.SecurityAttributes);
                writer.WriteEndElement();
            }
            if(setter != null)
            {
                writer.WriteStartElement("setter");
                WriteStringAttribute("name", string.Format("set_{0}", property.Name.Name.ToString()));

                WriteAttributes(setter.Attributes, setter.SecurityAttributes);
                writer.WriteEndElement();
            }
        }

        private void WriteStartTypeReference(TypeNode type)
        {
            switch(type.NodeType)
            {
                case NodeType.ArrayType:
                    ArrayType array = type as ArrayType;
                    writer.WriteStartElement("arrayOf");
                    writer.WriteAttributeString("rank", array.Rank.ToString());
                    WriteTypeReference(array.ElementType);
                    break;

                case NodeType.Reference:
                    Reference reference = type as Reference;
                    writer.WriteStartElement("referenceTo");
                    WriteTypeReference(reference.ElementType);
                    break;

                case NodeType.Pointer:
                    Pointer pointer = type as Pointer;
                    writer.WriteStartElement("pointerTo");
                    WriteTypeReference(pointer.ElementType);
                    break;

                case NodeType.OptionalModifier:
                    TypeModifier optionalModifierClause = type as TypeModifier;
                    WriteStartTypeReference(optionalModifierClause.ModifiedType);
                    writer.WriteStartElement("optionalModifier");
                    WriteTypeReference(optionalModifierClause.Modifier);
                    writer.WriteEndElement();
                    break;

                case NodeType.RequiredModifier:
                    TypeModifier requiredModifierClause = type as TypeModifier;
                    WriteStartTypeReference(requiredModifierClause.ModifiedType);
                    writer.WriteStartElement("requiredModifier");
                    WriteTypeReference(requiredModifierClause.Modifier);
                    writer.WriteEndElement();
                    break;

                default:
                    if(type.IsTemplateParameter)
                    {
                        ITypeParameter gtp = (ITypeParameter)type;
                        writer.WriteStartElement("template");

                        // !EFW - Change from ComponentOne
                        writer.WriteAttributeString("name", type.Name.Name.TranslateToValidXmlValue());
                        writer.WriteAttributeString("index", gtp.ParameterListIndex.ToString());
                        writer.WriteAttributeString("api", namer.GetApiName(gtp.DeclaringMember).TranslateToValidXmlValue());
                    }
                    else
                    {
                        writer.WriteStartElement("type");

                        if(type.IsGeneric)
                        {
                            TypeNode template = ReflectionUtilities.GetTemplateType(type);

                            // !EFW - Change from ComponentOne
                            writer.WriteAttributeString("api", namer.GetTypeName(template).TranslateToValidXmlValue());
                            WriteBooleanAttribute("ref", !template.IsValueType);

                            // record specialization							
                            TypeNodeList arguments = type.TemplateArguments;
                            if((arguments != null) && (arguments.Count > 0))
                            {
                                writer.WriteStartElement("specialization");

                                for(int i = 0; i < arguments.Count; i++)
                                {
                                    WriteTypeReference(arguments[i]);
                                }
                                writer.WriteEndElement();
                            }

                        }
                        else
                        {
                            // !EFW - Change from ComponentOne
                            writer.WriteAttributeString("api", namer.GetTypeName(type).TranslateToValidXmlValue());
                            WriteBooleanAttribute("ref", !type.IsValueType);
                        }

                        // record outer types (because they may be specialized, and otherwise that information is lost)
                        if(type.DeclaringType != null)
                            WriteTypeReference(type.DeclaringType);

                    }
                    break;
            }
        }

        private void WriteStringAttribute(string attribute, string value)
        {
            // !EFW - Change from ComponentOne
            writer.WriteAttributeString(attribute, value.TranslateToValidXmlValue());
        }

        private void WriteType(TypeNode type)
        {
            writer.WriteStartElement("api");

            // !EFW - Change from ComponentOne
            writer.WriteAttributeString("id", namer.GetTypeName(type).TranslateToValidXmlValue());
            StartElementCallbacks("api", type);

            WriteApiData(type);
            WriteTypeData(type);

            switch(type.NodeType)
            {
                case NodeType.Class:
                case NodeType.Struct:
                    WriteGenericParameters(type.TemplateParameters);
                    WriteInterfaces(type.Interfaces);
                    WriteTypeElements(type);
                    break;
                case NodeType.Interface:
                    WriteGenericParameters(type.TemplateParameters);
                    WriteInterfaces(type.Interfaces);
                    WriteImplementors((Interface)type);
                    WriteTypeElements(type);
                    break;
                case NodeType.DelegateNode:
                    DelegateNode handler = (DelegateNode)type;
                    WriteGenericParameters(handler.TemplateParameters);
                    WriteParameters(handler.Parameters);
                    WriteValue(handler.ReturnType);
                    break;
                case NodeType.EnumNode:
                    WriteEnumerationData((EnumNode)type);
                    WriteTypeElements(type);
                    break;
            }

            WriteTypeContainers(type);

            WriteAttributes(type.Attributes, type.SecurityAttributes);

            EndElementCallbacks("api", type);
            writer.WriteEndElement();

        }

        private void WriteTypeContainers(TypeNode type)
        {

            writer.WriteStartElement("containers");
            WriteLibraryReference(type.DeclaringModule);
            WriteNamespaceReference(GetNamespace(type));

            // for nested types, record outer types
            TypeNode outer = type.DeclaringType;
            if(outer != null)
                WriteTypeReference(outer);

            writer.WriteEndElement();

        }

        // Type data

        private void WriteTypeData(TypeNode type)
        {
            writer.WriteStartElement("typedata");

            // data for all types
            WriteStringAttribute("visibility", GetVisibility(type));
            WriteBooleanAttribute("abstract", type.IsAbstract, false);
            WriteBooleanAttribute("sealed", type.IsSealed, false);
            WriteBooleanAttribute("serializable", (type.Flags & TypeFlags.Serializable) != 0);

            // interop data
            TypeFlags layout = type.Flags & TypeFlags.LayoutMask;
            switch(layout)
            {
                case TypeFlags.AutoLayout:
                    WriteStringAttribute("layout", "auto");
                    break;
                case TypeFlags.SequentialLayout:
                    WriteStringAttribute("layout", "sequential");
                    break;
                case TypeFlags.ExplicitLayout:
                    WriteStringAttribute("layout", "explicit");
                    break;
            }
            TypeFlags format = type.Flags & TypeFlags.StringFormatMask;
            switch(format)
            {
                case TypeFlags.AnsiClass:
                    WriteStringAttribute("format", "ansi");
                    break;
                case TypeFlags.UnicodeClass:
                    WriteStringAttribute("format", "unicode");
                    break;
                case TypeFlags.AutoClass:
                    WriteStringAttribute("format", "auto");
                    break;
            }
            // also import

            StartElementCallbacks("typedata", type);


            EndElementCallbacks("typedata", type);
            writer.WriteEndElement();

            // for classes, recored base type
            if(type is Class)
            {
                WriteHierarchy(type);
                // TypeNode parent = type.BaseType;
                // if (parent != null) WriteStringAttribute("parent", namer.GetTypeName(parent));
            }

        }

        private void WriteTypeElements(TypeNode type)
        {
            // collect members
            MemberDictionary members = new MemberDictionary(type, this.ApiFilter);

            if(members.Count == 0)
                return;

            writer.WriteStartElement("elements");
            StartElementCallbacks("elements", members);

            foreach(Member member in members)
            {
                writer.WriteStartElement("element");

                Member template = ReflectionUtilities.GetTemplateMember(member);

                // !EFW - Change from ComponentOne
                writer.WriteAttributeString("api", namer.GetMemberName(template).TranslateToValidXmlValue());

                bool write = false;

                // inherited, specialized generics get a displayed target different from the target
                // we also write out their info, since it can't be looked up anywhere
                if(!member.DeclaringType.IsStructurallyEquivalentTo(template.DeclaringType))
                {
                    // !EFW - Change from ComponentOne
                    writer.WriteAttributeString("display-api", namer.GetMemberName(member).TranslateToValidXmlValue());
                    write = true;
                }

                // if a member is from a type in a dependency assembly, write out its info, since it can't be looked up in this file
                if(!assemblyNames.ContainsKey(member.DeclaringType.DeclaringModule.ContainingAssembly.StrongName))
                    write = true;
                // if (Array.BinarySearch(assemblyNames, member.DeclaringType.DeclaringModule.ContainingAssembly.Name) < 0) write = true;

                if(write)
                    WriteMember(member);

                writer.WriteEndElement();
            }

            EndElementCallbacks("elements", members);
            writer.WriteEndElement();

        }

        // Return value or property value or field value
        private void WriteValue(TypeNode type)
        {
            if(type.FullName == "System.Void")
                return;
            writer.WriteStartElement("returns");
            WriteTypeReference(type);
            // writer.WriteAttributeString("type", namer.GetTypeName(type));
            writer.WriteEndElement();
        }
    }
}
