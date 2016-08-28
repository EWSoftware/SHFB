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
// 11/25/2013 - EFW - Cleaned up the code and removed unused members
// 06/16/2015 - EFW - Fixed the extension method parameter comparisons so that it doesn't add extension methods
// to types with matching method signatures.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

using System.Compiler;

using Microsoft.Ddue.Tools.Reflection;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This add-in is used to add extension method information to API member nodes
    /// </summary>
    public class ExtensionMethodAddIn : MRefBuilderAddIn
    {
        #region Private data members
        //=====================================================================

        private Dictionary<TypeNode, List<Method>> index;
        private bool isExtensionMethod;
        private ManagedReflectionWriter mrw;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="writer">The API visitor and reflection data writer</param>
        /// <param name="configuration">This add-in has no configurable options</param>
        public ExtensionMethodAddIn(ManagedReflectionWriter writer, XPathNavigator configuration) :
          base(writer, configuration)
        {
            index = new Dictionary<TypeNode, List<Method>>();

            this.mrw = writer;

            writer.RegisterStartTagCallback("apis", RecordExtensionMethods);
            writer.RegisterStartTagCallback("apidata", AddExtensionSubsubgroup);
            writer.RegisterEndTagCallback("elements", AddExtensionMethods);
        }
        #endregion

        #region Add-in callback methods
        //=====================================================================

        /// <summary>
        /// This finds all extension methods, adds information about them to the types, and tracks
        /// them for adding to the reflection data later in the other callbacks.
        /// </summary>
        /// <param name="writer">The reflection data XML writer</param>
        /// <param name="info">For this callback, the information object is a namespace list</param>
        private void RecordExtensionMethods(XmlWriter writer, object info)
        {
            NamespaceList spaces = (NamespaceList)info;

            foreach(Namespace space in spaces)
            {
                // !EFW - Don't bother checking unexposed namespaces
                if(!mrw.ApiFilter.IsExposedNamespace(space))
                    continue;

                TypeNodeList types = space.Types;

                foreach(TypeNode type in types)
                {
                    // !EFW - Don't bother checking unexposed types
                    if(!mrw.ApiFilter.IsExposedType(type))
                        continue;

                    // Go through the members looking for fields signaling extension methods.  Members may be
                    // added so convert to a list first to avoid enumeration issues.
                    foreach(Member member in type.Members.ToList())
                    {
                        Method method = member as Method;

                        if(method == null || !mrw.ApiFilter.IsExposedMember(method) ||
                          !method.Attributes.Any(a => a.Type.FullName == "System.Runtime.CompilerServices.ExtensionAttribute"))
                            continue;

                        ParameterList parameters = method.Parameters;

                        // !EFW - This fix was reported without an example.  Sometimes, there are no parameters.
                        // In such cases, ignore it to prevent a crash.
                        if(parameters == null || parameters.Count == 0)
                            continue;

                        TypeNode extendedType = parameters[0].Type;

                        // Recognize generic extension methods where the extended type is a specialization of a
                        // generic type and the extended type's specialized template argument is a type parameter
                        // declared by the generic extension method.  In this case, we need to save a TypeNode
                        // for the non-specialized type in the index because a TypeNode for the specialized type
                        // won't match correctly in AddExtensionMethods().  NOTE: we are not interested in
                        // extended types that are specialized by a specific type rather than by the extension
                        // method's template parameter.
                        if(method.IsGeneric && method.TemplateParameters.Count > 0)
                            if(extendedType.IsGeneric && extendedType.TemplateArguments != null &&
                              extendedType.TemplateArguments.Count == 1)
                            {
                                // Is the extended type's template argument a template parameter rather than a
                                // specialized type?
                                TypeNode arg = extendedType.TemplateArguments[0];

                                if(arg.IsTemplateParameter)
                                {
                                    // Is the template parameter declared on the extension method
                                    ITypeParameter gtp = (ITypeParameter)arg;

                                    if(gtp.DeclaringMember == method && gtp.ParameterListIndex == 0)
                                    {
                                        // Get a TypeNode for the non-specialized type
                                        extendedType = extendedType.GetTemplateType();
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
        /// This is used to add a <c>subsubgroup</c> attribute that identifies the extension method
        /// </summary>
        /// <param name="writer">The reflection data XML writer</param>
        /// <param name="info">Not used here</param>
        private void AddExtensionSubsubgroup(XmlWriter writer, object data)
        {
            if(isExtensionMethod)
                writer.WriteAttributeString("subsubgroup", "extension");
        }

        /// <summary>
        /// This is used to add the extension method elements to the type's member list
        /// </summary>
        /// <param name="writer">The reflection data XML writer</param>
        /// <param name="info">For this callback, this is a member dictionary</param>
        private void AddExtensionMethods(XmlWriter writer, object info)
        {
            MemberDictionary members = info as MemberDictionary;

            if(members == null)
                return;

            TypeNode type = members.Type;

            foreach(Interface contract in type.Interfaces)
            {
                List<Method> extensionMethods = null;

                if(index.TryGetValue(contract, out extensionMethods))
                    foreach(Method extensionMethod in extensionMethods)
                        if(!IsExtensionMethodHidden(extensionMethod, members))
                            AddExtensionMethod(writer, type, extensionMethod, null);

                if(contract.IsGeneric && contract.TemplateArguments != null && contract.TemplateArguments.Count > 0)
                {
                    Interface templateContract = (Interface)contract.GetTemplateType();
                    TypeNode specialization = contract.TemplateArguments[0];

                    if(index.TryGetValue(templateContract, out extensionMethods))
                        foreach(Method extensionMethod in extensionMethods)
                            if(IsValidTemplateArgument(specialization, extensionMethod.TemplateParameters[0]))
                                if(!IsExtensionMethodHidden(extensionMethod, members))
                                    AddExtensionMethod(writer, type, extensionMethod, specialization);
                }
            }

            TypeNode comparisonType = type;

            while(comparisonType != null)
            {
                List<Method> extensionMethods = null;

                if(index.TryGetValue(comparisonType, out extensionMethods))
                    foreach(Method extensionMethod in extensionMethods)
                        if(!IsExtensionMethodHidden(extensionMethod, members))
                            AddExtensionMethod(writer, type, extensionMethod, null);

                if(comparisonType.IsGeneric && comparisonType.TemplateArguments != null &&
                  comparisonType.TemplateArguments.Count > 0)
                {
                    TypeNode templateType = comparisonType.GetTemplateType();
                    TypeNode specialization = comparisonType.TemplateArguments[0];

                    if(index.TryGetValue(templateType, out extensionMethods))
                        foreach(Method extensionMethod in extensionMethods)
                            if(IsValidTemplateArgument(specialization, extensionMethod.TemplateParameters[0]))
                                if(!IsExtensionMethodHidden(extensionMethod, members))
                                    AddExtensionMethod(writer, type, extensionMethod, specialization);
                }

                comparisonType = comparisonType.BaseType;
            }
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Add extension method information to a type
        /// </summary>
        /// <param name="writer">The reflection data XML writer</param>
        /// <param name="type">the current type being extended</param>
        /// <param name="extensionMethodTemplate">A reference to the extension method. For generic methods,
        /// this is a reference to the non-specialized method, e.g. System.Linq.Enumerable.Select``2.</param>
        /// <param name="specialization">When the current type implements or inherits from a specialization
        /// of a generic type, this parameter has a TypeNode for the type used as a specialization of the
        /// generic type's first template parameter.</param>
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

            if(extensionMethodTemplate2.IsGeneric && specialization != null)
            {
                // The specialization type is the first of the method's template arguments
                TypeNodeList templateArgs = new TypeNodeList();
                templateArgs.Add(specialization);

                // Add any additional template arguments
                for(int i = 1; i < extensionMethodTemplate.TemplateParameters.Count; i++)
                    templateArgs.Add(extensionMethodTemplate.TemplateParameters[i]);

                extensionMethodTemplate2 = extensionMethodTemplate.GetTemplateInstance(type, templateArgs);
            }

            ParameterList extensionMethodTemplateParameters = extensionMethodTemplate2.Parameters;

            ParameterList extensionMethodParameters = new ParameterList();

            for(int i = 1; i < extensionMethodTemplateParameters.Count; i++)
            {
                Parameter extensionMethodParameter = extensionMethodTemplateParameters[i];
                extensionMethodParameters.Add(extensionMethodParameter);
            }

            Method extensionMethod = new Method(extensionMethodTemplate.DeclaringType, new AttributeList(),
                extensionMethodTemplate.Name, extensionMethodParameters, extensionMethodTemplate2.ReturnType,
                null);
            extensionMethod.Flags = extensionMethodTemplate.Flags & ~MethodFlags.Static;

            // For generic methods, set the template arguments and parameters so the template data is included in
            // the ID and the method data.
            if(extensionMethodTemplate2.IsGeneric)
            {
                extensionMethod.IsGeneric = true;

                if(specialization != null)
                {
                    // Set the template arguments for the specialized generic method
                    extensionMethod.TemplateArguments = extensionMethodTemplate2.TemplateArguments;
                }
                else
                {
                    // Set the generic template parameters for the non-specialized generic method
                    extensionMethod.TemplateParameters = extensionMethodTemplate2.TemplateParameters;
                }
            }

            // Get the ID
            string extensionMethodTemplateId = mrw.ApiNamer.GetMemberName(extensionMethodTemplate);

            // Write the element node
            writer.WriteStartElement("element");
            writer.WriteAttributeString("api", extensionMethodTemplateId);
            writer.WriteAttributeString("source", "extension");

            isExtensionMethod = true;
            mrw.WriteMember(extensionMethod, false);
            isExtensionMethod = false;

            writer.WriteEndElement();
        }

        /// <summary>
        /// This is used to see if a type and parameter are valid template arguments
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <param name="parameter">The parameter to check</param>
        /// <returns>True if it is valid, false if not</returns>
        private static bool IsValidTemplateArgument(TypeNode type, TypeNode parameter)
        {
            if(type == null)
                throw new ArgumentNullException("type");

            // Check that the parameter really is a type parameter
            ITypeParameter itp = parameter as ITypeParameter;

            if(itp == null)
                throw new ArgumentException("The 'parameter' argument is null or not an 'ITypeParameter'.");

            // Test constraints
            bool reference = ((itp.TypeParameterFlags & TypeParameterFlags.ReferenceTypeConstraint) > 0);

            if(reference && type.IsValueType)
                return (false);

            bool value = ((itp.TypeParameterFlags & TypeParameterFlags.ValueTypeConstraint) > 0);

            if(value && !type.IsValueType)
                return (false);

            InterfaceList contracts = parameter.Interfaces;

            if(contracts != null)
                foreach(Interface contract in contracts)
                    if(!type.IsAssignableTo(contract))
                        return false;

            TypeNode parent = parameter.BaseType;

            if(parent != null && !type.IsAssignableTo(parent))
                return false;

            // Okay, passed all tests
            return true;
        }

        /// <summary>
        /// Determines whether an extension method is hidden by a member that's already defined on the type being
        /// extended.  The extension method is hidden if it has the same name, template parameters, and
        /// parameters as a defined method.
        /// </summary>
        /// <param name="extensionMethod">The extension method to compare</param>
        /// <param name="members">A dictionary of the members defined on the type being extended</param>
        /// <returns>True if hidden, false if not</returns>
        private static bool IsExtensionMethodHidden(Method extensionMethod, MemberDictionary members)
        {
            if(!members.MemberNames.Contains(extensionMethod.Name.Name))
                return false;

            // Get a list of members with the same name as the extension method
            foreach(Member member in members[extensionMethod.Name.Name])
            {
                // The hiding member must be a method
                if(member.NodeType != NodeType.Method)
                    continue;

                Method method = (Method)member;

                // Do the generic template parameters of both methods match?
                if(!method.TemplateParametersMatch(extensionMethod.TemplateParameters))
                    continue;

                // !EFW - Don't exclude the first parameter, it does need to be included.
                // Do both methods have the same number of parameters?
                if(method.Parameters.Count != extensionMethod.Parameters.Count)
                    continue;

                // Do the parameter types of both methods match?
                if(DoParameterTypesMatch(extensionMethod.Parameters, method.Parameters))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// This is used to see if the extension parameter types match the method parameter types
        /// </summary>
        /// <param name="extensionParams">The extension parameters</param>
        /// <param name="methodParams">The method parameters</param>
        /// <returns>True if they match, false if not</returns>
        private static bool DoParameterTypesMatch(ParameterList extensionParams, ParameterList methodParams)
        {
            // !EFW - Don't ignore the first extension method parameter, it does need to be included.
            for(int i = 0; i < methodParams.Count; i++)
                if(methodParams[i].Type.FullName != extensionParams[i].Type.FullName)
                    return false;

            return true;
        }
        #endregion
    }
}
