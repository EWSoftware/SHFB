// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 01/30/2012 - EFW - Fixed AddExtensionMethod() so that it doesn't add extension methods to
// enumerations and static classes.
// 03/02/2012 - EFW - Added code to ignore unexposed namespaces and types.  This fixes a crash issue
// caused by certain compiler generated types.
// 07/25/2012 - EFW - Fixed RecordExtensionMethods() to prevent a crash in an odd case

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

using System.Compiler;

using Microsoft.Ddue.Tools.Reflection;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This MRefBuilder add-in adds extension method information to API member nodes
    /// </summary>
    public class ExtensionMethodAddIn : MRefBuilderAddIn
    {
        private Dictionary<TypeNode, List<Method>> index = new Dictionary<TypeNode, List<Method>>();
        private bool isExtensionMethod;
        private ManagedReflectionWriter reflector;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="reflector">The managed reflection writer</param>
        /// <param name="configuration">The configuration to use</param>
        public ExtensionMethodAddIn(ManagedReflectionWriter reflector, XPathNavigator configuration) :
          base(reflector, configuration)
        {
            this.reflector = reflector;

            reflector.RegisterStartTagCallback("apis", new MRefBuilderCallback(RecordExtensionMethods));
            reflector.RegisterEndTagCallback("elements", new MRefBuilderCallback(AddExtensionMethods));
            reflector.RegisterStartTagCallback("apidata", new MRefBuilderCallback(AddExtensionSubsubgroup));
        }

        /// <summary>
        /// Add extension method information to a type
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="type">the current type being extended</param>
        /// <param name="extensionMethodTemplate">A reference to the extension method. For generic methods,
        /// this is a reference to the non-specialized method, e.g. System.Linq.Enumerable.Select``2.
        /// </param>
        /// <param name="specialization">When the current type implements or inherits from a specialization
        /// of a generic type, this parameter has a TypeNode for the type used as apecialization of the
        /// generic type's first template param. 
        /// </param>
        private void AddExtensionMethod(XmlWriter writer, TypeNode type, Method extensionMethodTemplate,
          TypeNode specialization)
        {
            // !EFW - Bug fix
            // Don't add extension method support to enumerations and static classes
            if(type != null && (type.NodeType == NodeType.EnumNode || (type.NodeType == NodeType.Class &&
              type.IsAbstract && type.IsSealed)))
                return;

            // If this is a specialization of a generic method, construct a Method object that describes
            // the specialization.
            Method extensionMethodTemplate2 = extensionMethodTemplate;

            if(extensionMethodTemplate2.IsGeneric && (specialization != null))
            {
                // The specialization type is the first of the method's template arguments
                TypeNodeList templateArgs = new TypeNodeList();
                templateArgs.Add(specialization);

                // Add any additional template arguments
                for(int i = 1; i < extensionMethodTemplate.TemplateParameters.Count; i++)
                    templateArgs.Add(extensionMethodTemplate.TemplateParameters[i]);

                extensionMethodTemplate2 = extensionMethodTemplate.GetTemplateInstance(type, templateArgs);
            }

            TypeNode extensionMethodTemplateReturnType = extensionMethodTemplate2.ReturnType;
            ParameterList extensionMethodTemplateParameters = extensionMethodTemplate2.Parameters;

            ParameterList extensionMethodParameters = new ParameterList();

            for(int i = 1; i < extensionMethodTemplateParameters.Count; i++)
            {
                Parameter extensionMethodParameter = extensionMethodTemplateParameters[i];
                extensionMethodParameters.Add(extensionMethodParameter);
            }

            Method extensionMethod = new Method(extensionMethodTemplate.DeclaringType, new AttributeList(),
                extensionMethodTemplate.Name, extensionMethodParameters, extensionMethodTemplate.ReturnType,
                null);
            extensionMethod.Flags = extensionMethodTemplate.Flags & ~MethodFlags.Static;

            // For generic methods, set the template args and params so the template data is included in
            // the id and the method data.
            if(extensionMethodTemplate2.IsGeneric)
            {
                extensionMethod.IsGeneric = true;

                if(specialization != null)
                {
                    // Set the template args for the specialized generic method
                    extensionMethod.TemplateArguments = extensionMethodTemplate2.TemplateArguments;
                }
                else
                {
                    // Set the generic template params for the non-specialized generic method
                    extensionMethod.TemplateParameters = extensionMethodTemplate2.TemplateParameters;
                }
            }

            // Get the id
            string extensionMethodTemplateId = reflector.ApiNamer.GetMemberName(extensionMethodTemplate);

            // Write the element node
            writer.WriteStartElement("element");
            writer.WriteAttributeString("api", extensionMethodTemplateId);
            writer.WriteAttributeString("source", "extension");

            isExtensionMethod = true;
            reflector.WriteMember(extensionMethod);
            isExtensionMethod = false;
            writer.WriteEndElement();
        }

        private void AddExtensionMethods(XmlWriter writer, Object info)
        {
            MemberDictionary members = info as MemberDictionary;
            if(members == null)
                return;

            TypeNode type = members.Type;

            InterfaceList contracts = type.Interfaces;
            foreach(Interface contract in contracts)
            {
                List<Method> extensionMethods = null;
                if(index.TryGetValue(contract, out extensionMethods))
                {
                    foreach(Method extensionMethod in extensionMethods)
                        if(!IsExtensionMethodHidden(extensionMethod, members))
                            AddExtensionMethod(writer, type, extensionMethod, null);
                }
                if(contract.IsGeneric && (contract.TemplateArguments != null) && (contract.TemplateArguments.Count > 0))
                {
                    Interface templateContract = (Interface)ReflectionUtilities.GetTemplateType(contract);
                    TypeNode specialization = contract.TemplateArguments[0];
                    if(index.TryGetValue(templateContract, out extensionMethods))
                    {
                        foreach(Method extensionMethod in extensionMethods)
                        {
                            if(IsValidTemplateArgument(specialization, extensionMethod.TemplateParameters[0]))
                            {
                                if(!IsExtensionMethodHidden(extensionMethod, members))
                                    AddExtensionMethod(writer, type, extensionMethod, specialization);
                            }
                        }
                    }
                }
            }

            TypeNode comparisonType = type;
            while(comparisonType != null)
            {
                List<Method> extensionMethods = null;
                if(index.TryGetValue(comparisonType, out extensionMethods))
                {
                    foreach(Method extensionMethod in extensionMethods)
                        if(!IsExtensionMethodHidden(extensionMethod, members))
                            AddExtensionMethod(writer, type, extensionMethod, null);
                }
                if(comparisonType.IsGeneric && (comparisonType.TemplateArguments != null) && (comparisonType.TemplateArguments.Count > 0))
                {
                    TypeNode templateType = ReflectionUtilities.GetTemplateType(comparisonType);
                    TypeNode specialization = comparisonType.TemplateArguments[0];
                    if(index.TryGetValue(templateType, out extensionMethods))
                    {
                        foreach(Method extensionMethod in extensionMethods)
                        {
                            if(IsValidTemplateArgument(specialization, extensionMethod.TemplateParameters[0]))
                            {
                                if(!IsExtensionMethodHidden(extensionMethod, members))
                                    AddExtensionMethod(writer, type, extensionMethod, specialization);
                            }
                        }
                    }
                }
                comparisonType = comparisonType.BaseType;
            }
        }

        private void AddExtensionSubsubgroup(XmlWriter writer, Object data)
        {
            if(isExtensionMethod)
                writer.WriteAttributeString("subsubgroup", "extension");
        }

        private bool HasExtensionAttribute(Method method)
        {
            AttributeList attributes = method.Attributes;
            foreach(AttributeNode attribute in attributes)
            {
                if(attribute.Type.FullName == "System.Runtime.CompilerServices.ExtensionAttribute")
                    return (true);
            }
            return (false);
        }

        private bool IsValidTemplateArgument(TypeNode type, TypeNode parameter)
        {
            if(type == null)
                throw new ArgumentNullException("type");
            if(parameter == null)
                throw new ArgumentNullException("parameter");

            // check that the parameter really is a type parameter

            ITypeParameter itp = parameter as ITypeParameter;
            if(itp == null)
                throw new ArgumentException("The 'parameter' argument is null or not an 'ITypeParameter'.");

            // test constraints

            bool reference = ((itp.TypeParameterFlags & TypeParameterFlags.ReferenceTypeConstraint) > 0);
            if(reference && type.IsValueType)
                return (false);

            bool value = ((itp.TypeParameterFlags & TypeParameterFlags.ValueTypeConstraint) > 0);
            if(value && !type.IsValueType)
                return (false);

            bool constructor = ((itp.TypeParameterFlags & TypeParameterFlags.DefaultConstructorConstraint) > 0);


            InterfaceList contracts = parameter.Interfaces;
            if(contracts != null)
            {
                foreach(Interface contract in contracts)
                {
                    if(!type.IsAssignableTo(contract))
                        return (false);
                }
            }

            TypeNode parent = parameter.BaseType;
            if((parent != null) && !type.IsAssignableTo(parent))
                return (false);

            // okay, passed all tests
            return (true);
        }

        private void RecordExtensionMethods(XmlWriter writer, Object info)
        {
            NamespaceList spaces = (NamespaceList)info;

            foreach(Namespace space in spaces)
            {
                // !EFW - Don't bother checking unexposed namespaces
                if(!reflector.ApiFilter.IsExposedNamespace(space))
                    continue;

                TypeNodeList types = space.Types;

                foreach(TypeNode type in types)
                {
                    // !EFW - Don't bother checking unexposed types
                    if(!reflector.ApiFilter.IsExposedType(type))
                        continue;

                    MemberList members = type.Members;

                    // go through the members, looking for fields signaling extension methods
                    foreach(Member member in members)
                    {
                        Method method = member as Method;

                        if(method == null)
                            continue;

                        if(!reflector.ApiFilter.IsExposedMember(method))
                            continue;

                        if(!HasExtensionAttribute(method))
                            continue;

                        ParameterList parameters = method.Parameters;

                        // !EFW - This fix was reported without an example.  Sometimes, there are no parameters.
                        // In such cases, ignore it to prevent a crash.
                        if(parameters == null || parameters.Count == 0)
                            continue;

                        TypeNode extendedType = parameters[0].Type;

                        // recognize generic extension methods where the extended type is a specialization of a generic type,  
                        // and the extended type's specialized template arg is a type parameter declared by the generic extension method 
                        // In this case, we need to save a TypeNode for the non-specialized type in the index, 
                        // because a TypeNode for the specialized type won't match correctly in AddExtensionMethods
                        // Note: we are not interested in extended types that are specialized by a specific type rather than by the extension method's template param.
                        if(method.IsGeneric && (method.TemplateParameters.Count > 0))
                        {
                            if(extendedType.IsGeneric && (extendedType.TemplateArguments != null) && (extendedType.TemplateArguments.Count == 1))
                            {
                                // is the extended type's template arg a template parameter, rather than a specialized type?
                                TypeNode arg = extendedType.TemplateArguments[0];
                                if(arg.IsTemplateParameter)
                                {
                                    // is the template parameter declared on the extension method
                                    ITypeParameter gtp = (ITypeParameter)arg;
                                    if((gtp.DeclaringMember == method) && (gtp.ParameterListIndex == 0))
                                    {
                                        // get a TypeNode for the non-specialized type
                                        extendedType = ReflectionUtilities.GetTemplateType(extendedType);
                                    }
                                }
                            }
                        }

                        List<Method> methods = null;
                        if(!index.TryGetValue(extendedType, out methods))
                        {
                            methods = new List<Method>();
                            index.Add(extendedType, methods);
                        }

                        methods.Add(method);
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether an extension method is hidden by a member that's already defined on the type being extended.
        /// The extension method is hidden if it has the same name, template params, and parameters as a defined method.
        /// </summary>
        /// <param name="extensionMethod">The extension method to compare.</param>
        /// <param name="members">A dictionary of the members defined on the type being extended.</param>
        /// <returns></returns>
        private bool IsExtensionMethodHidden(Method extensionMethod, MemberDictionary members)
        {
            if(!members.MemberNames.Contains(extensionMethod.Name.Name))
                return false;

            // get a list of members with the same name as the extension method
            List<Member> membersList = members[extensionMethod.Name.Name];
            foreach(Member member in membersList)
            {
                // the hiding member must be a method
                if(member.NodeType != NodeType.Method)
                    continue;
                Method method = (Method)member;

                // do the generic template parameters of both methods match?
                if(!method.TemplateParametersMatch(extensionMethod.TemplateParameters))
                    continue;

                // do both methods have the same number of parameters?
                // (not counting the extension method's first param, which identifies the extended type)
                if(method.Parameters.Count != (extensionMethod.Parameters.Count - 1))
                    continue;

                // do the parameter types of both methods match?
                if(DoParameterTypesMatch(extensionMethod.Parameters, method.Parameters))
                    return true;
            }
            return false;
        }

        private bool DoParameterTypesMatch(ParameterList extensionParams, ParameterList methodParams)
        {
            for(int i = 0; i < methodParams.Count; i++)
            {
                if(methodParams[i].Type.FullName != extensionParams[i + 1].Type.FullName)
                    return false;
            }
            return true;
        }
    }
}
