//===============================================================================================================
// System  : Sandcastle MRefBuilder Tool
// File    : WindowsStoreAndPhoneNamer.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/11/2014
// Note    : Copyright 2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a modified version of the original OrcasNamer that renames certain core framework types in
// assemblies compiled against the Windows Store or Windows Phone frameworks.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/16/2014  EFW  Created the code
//===============================================================================================================

using System;
using System.Linq;
using System.Text;

using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection
{
    /// <summary>
    /// This is the API member namer for assemblies built against the Windows Store or Windows Phone frameworks
    /// </summary>
    /// <remarks>These frameworks rename certain types when compiled but the XML comments still refer to the
    /// conventional names used in other frameworks.  As such, we'll rename them back to the expected names when
    /// generating the reflection information.</remarks>
    public class WindowsStoreAndPhoneNamer : ApiNamer
    {
        #region ApiNamer implementation
        //=====================================================================

        /// <inheritdoc />
        public override string GetNamespaceName(Namespace space)
        {
            return "N:" + space.Name;
        }

        /// <inheritdoc />
        public override string GetTypeName(TypeNode type)
        {
            StringBuilder sb = new StringBuilder("T:");

            WriteType(type, sb);

            return sb.ToString();
        }

        /// <inheritdoc />
        public override string GetMemberName(Member member)
        {
            StringBuilder sb = new StringBuilder();

            switch(member.NodeType)
            {
                case NodeType.Field:
                    sb.Append("F:");
                    WriteField((Field)member, sb);
                    break;

                case NodeType.Property:
                    sb.Append("P:");
                    WriteProperty((Property)member, sb);
                    break;

                case NodeType.Event:
                    sb.Append("E:");
                    WriteEvent((Event)member, sb);
                    break;

                case NodeType.Method:
                    sb.Append("M:");
                    WriteMethod((Method)member, sb);
                    break;

                case NodeType.InstanceInitializer:
                    sb.Append("M:");
                    WriteConstructor((InstanceInitializer)member, sb);
                    break;

                case NodeType.StaticInitializer:
                    sb.Append("M:");
                    WriteStaticConstructor((StaticInitializer)member, sb);
                    break;
            }

            return sb.ToString();
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Write out a type name
        /// </summary>
        /// <param name="type">The type for which to write out the name</param>
        /// <param name="sb">The string builder to which the name is written</param>
        private static void WriteType(TypeNode type, StringBuilder sb)
        {
            switch(type.NodeType)
            {
                case NodeType.ArrayType:
                    ArrayType array = (ArrayType)type;
                    WriteType(array.ElementType, sb);

                    sb.Append("[");

                    if(array.Rank > 1)
                        for(int i = 0; i < array.Rank; i++)
                        {
                            if(i > 0)
                                sb.Append(",");

                            sb.Append("0:");
                        }

                    sb.Append("]");
                    break;

                case NodeType.Reference:
                    Reference reference = (Reference)type;
                    WriteType(reference.ElementType, sb);
                    sb.Append("@");
                    break;

                case NodeType.Pointer:
                    Pointer pointer = (Pointer)type;
                    WriteType(pointer.ElementType, sb);
                    sb.Append("*");
                    break;

                case NodeType.OptionalModifier:
                    TypeModifier optionalModifierClause = (TypeModifier)type;
                    WriteType(optionalModifierClause.ModifiedType, sb);
                    sb.Append("!");
                    WriteType(optionalModifierClause.Modifier, sb);
                    break;

                case NodeType.RequiredModifier:
                    TypeModifier requiredModifierClause = (TypeModifier)type;
                    WriteType(requiredModifierClause.ModifiedType, sb);
                    sb.Append("|");
                    WriteType(requiredModifierClause.Modifier, sb);
                    break;

                default:
                    if(type.IsTemplateParameter)
                    {
                        ITypeParameter gtp = (ITypeParameter)type;

                        if(gtp.DeclaringMember is TypeNode)
                            sb.Append("`");
                        else
                            if(gtp.DeclaringMember is Method)
                                sb.Append("``");
                            else
                                throw new InvalidOperationException("Generic parameter not on type or method");

                        sb.Append(gtp.ParameterListIndex);
                    }
                    else
                    {
                        // Namespace
                        TypeNode declaringType = type.DeclaringType;

                        if(declaringType != null)
                        {
                            // Names of nested types begin with outer type name
                            WriteType(declaringType, sb);
                            sb.Append(".");
                        }
                        else
                        {
                            // Otherwise just prefix with the namespace
                            Identifier space = type.Namespace;

                            if(space != null && !String.IsNullOrEmpty(space.Name))
                            {
                                sb.Append(space.Name);
                                sb.Append(".");
                            }
                        }

                        // Name
                        sb.Append(type.GetUnmangledNameWithoutTypeParameters());

                        RenameTypeIfNecessary(sb);

                        // Generic parameters
                        if(type.IsGeneric)
                        {
                            // Number of parameters
                            TypeNodeList parameters = type.TemplateParameters;

                            if(parameters != null)
                            {
                                sb.Append('`');
                                sb.Append(parameters.Count);
                            }

                            // Arguments
                            TypeNodeList arguments = type.TemplateArguments;

                            if(arguments != null && arguments.Count > 0)
                            {
                                sb.Append("{");

                                for(int i = 0; i < arguments.Count; i++)
                                {
                                    if(i > 0)
                                        sb.Append(",");

                                    WriteType(arguments[i], sb);
                                }

                                sb.Append("}");
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// This is used to rename the type if necessary
        /// </summary>
        /// <param name="sb">The string builder containing the current type name</param>
        private static void RenameTypeIfNecessary(StringBuilder sb)
        {
            string typeNameInCode = null;

            // These are all of the currently known cases that need renaming
            switch(sb.ToString())
            {
                case "T:Windows.Foundation.Collections.IIterable":
                    typeNameInCode = "T:System.Collections.Generic.IEnumerable";
                    break;

                case "T:Windows.Foundation.Collections.IMap":
                    typeNameInCode = "T:System.Collections.Generic.IDictionary";
                    break;

                case "T:Windows.Foundation.Collections.IMapView":
                    typeNameInCode = "T:System.Collections.Generic.IReadOnlyDictionary";
                    break;

                case "T:Windows.Foundation.Collections.IVector":
                    typeNameInCode = "T:System.Collections.Generic.IList";
                    break;

                case "T:Windows.Foundation.Collections.IVectorView":
                    typeNameInCode = "T:System.Collections.Generic.IReadOnlyList";
                    break;

                case "T:Windows.Foundation.Collections.KeyValuePair":
                    typeNameInCode = "T:System.Collections.Generic.KeyValuePair";
                    break;

                case "T:Windows.Foundation.DateTime":
                    typeNameInCode = "T:System.DateTimeOffset";
                    break;

                case "T:Windows.Foundation.EventHandler":
                    typeNameInCode = "T:System.EventHandler";
                    break;

                case "T:Windows.Foundation.IReference":
                    typeNameInCode = "T:System.Nullable";
                    break;

                case "T:Windows.Foundation.TimeSpan":
                    typeNameInCode = "T:System.TimeSpan";
                    break;

                case "T:Windows.Foundation.Uri":
                    typeNameInCode = "T:System.Uri";
                    break;

                default:
                    break;
            }

            if(typeNameInCode != null)
            {
                sb.Clear();
                sb.Append(typeNameInCode);
            }
        }

        /// <summary>
        /// Write out a field name
        /// </summary>
        /// <param name="field">The field for which to write out the name</param>
        /// <param name="sb">The string builder to which the name is written</param>
        private static void WriteField(Field field, StringBuilder sb)
        {
            WriteType(field.DeclaringType, sb);
            sb.Append('.');
            sb.Append(field.Name.Name);
        }

        /// <summary>
        /// Write out a property name
        /// </summary>
        /// <param name="property">The property for which to write out the name</param>
        /// <param name="sb">The string builder to which the name is written</param>
        private static void WriteProperty(Property property, StringBuilder sb)
        {
            WriteType(property.DeclaringType, sb);

            Property eiiProperty = null;

            if(property.IsPrivate && property.IsVirtual)
                eiiProperty = property.GetImplementedProperties().FirstOrDefault();

            if(eiiProperty != null)
            {
                TypeNode eiiType = eiiProperty.DeclaringType;

                if(eiiType != null)
                    if(eiiType.Template != null)
                    {
                        sb.Append(".");
                        WriteTemplate(eiiType, sb);
                    }
                    else
                    {
                        StringBuilder eiiName = new StringBuilder();

                        WriteType(eiiType, eiiName);
                        sb.Append(".");
                        sb.Append(eiiName.ToString().Replace('.', '#'));
                    }

                sb.Append("#");
                sb.Append(eiiProperty.Name.Name);
            }
            else
            {
                sb.Append('.');
                sb.Append(property.Name.Name);
            }

            WriteParameters(property.Parameters, sb);
        }

        /// <summary>
        /// Write out an event name
        /// </summary>
        /// <param name="trigger">The event for which to write out the name</param>
        /// <param name="sb">The string builder to which the name is written</param>
        private static void WriteEvent(Event trigger, StringBuilder sb)
        {
            WriteType(trigger.DeclaringType, sb);

            Event eiiTrigger = null;

            if(trigger.IsPrivate && trigger.IsVirtual)
                eiiTrigger = trigger.GetImplementedEvents().FirstOrDefault();

            if(eiiTrigger != null)
            {
                TypeNode eiiType = eiiTrigger.DeclaringType;

                if(eiiType != null)
                    if(eiiType.Template != null)
                    {
                        sb.Append(".");
                        WriteTemplate(eiiType, sb);
                    }
                    else
                    {
                        StringBuilder eiiName = new StringBuilder();

                        WriteType(eiiType, eiiName);
                        sb.Append(".");
                        sb.Append(eiiName.ToString().Replace('.', '#'));
                    }

                sb.Append("#");
                sb.Append(eiiTrigger.Name.Name);
            }
            else
            {
                sb.Append('.');
                sb.Append(trigger.Name.Name);
            }
        }

        /// <summary>
        /// Write out a constructor name
        /// </summary>
        /// <param name="constructor">The constructor for which to write out the name</param>
        /// <param name="sb">The string builder to which the name is written</param>
        private static void WriteConstructor(InstanceInitializer constructor, StringBuilder sb)
        {
            WriteType(constructor.DeclaringType, sb);
            sb.Append(".#ctor");
            WriteParameters(constructor.Parameters, sb);
        }

        /// <summary>
        /// Write out a static constructor name
        /// </summary>
        /// <param name="constructor">The static constructor for which to write out the name</param>
        /// <param name="sb">The string builder to which the name is written</param>
        private static void WriteStaticConstructor(StaticInitializer constructor, StringBuilder sb)
        {
            WriteType(constructor.DeclaringType, sb);
            sb.Append(".#cctor");
            WriteParameters(constructor.Parameters, sb);
        }

        /// <summary>
        /// Write out a method name
        /// </summary>
        /// <param name="method">The method for which to write out the name</param>
        /// <param name="sb">The string builder to which the name is written</param>
        private static void WriteMethod(Method method, StringBuilder sb)
        {
            string name = method.Name.Name;

            WriteType(method.DeclaringType, sb);

            Method eiiMethod = null;

            if(method.IsPrivate && method.IsVirtual)
                eiiMethod = method.ImplementedInterfaceMethods.FirstOrDefault();

            if(eiiMethod != null)
            {
                TypeNode eiiType = eiiMethod.DeclaringType;

                if(eiiType != null)
                    if(eiiType.Template != null)
                    {
                        sb.Append(".");
                        WriteTemplate(eiiType, sb);
                    }
                    else
                    {
                        StringBuilder eiiName = new StringBuilder();

                        WriteType(eiiType, eiiName);
                        sb.Append(".");
                        sb.Append(eiiName.ToString().Replace('.', '#'));
                    }

                sb.Append("#");
                sb.Append(eiiMethod.Name.Name);
            }
            else
            {
                sb.Append('.');
                sb.Append(name);
            }

            if(method.IsGeneric)
            {
                TypeNodeList genericParameters = method.TemplateParameters;

                if(genericParameters != null)
                {
                    sb.Append("``");
                    sb.Append(genericParameters.Count);
                }
            }

            WriteParameters(method.Parameters, sb);

            // Add ~ for conversion operators
            if(name == "op_Implicit" || name == "op_Explicit")
            {
                sb.Append("~");
                WriteType(method.ReturnType, sb);
            }
        }

        /// <summary>
        /// This is used for explicitly implemented interfaces to convert the template to the format used in the
        /// comments file.
        /// </summary>
        /// <param name="type">The explicitly implemented interface type</param>
        /// <param name="sb">The string builder to which the name is written</param>
        private static void WriteTemplate(TypeNode eiiType, StringBuilder sb)
        {
            // !EFW Use this instead of the template name as the type parameter may be different in the user's
            // code (i.e. Collection<TControl> instead of Collection<T>.
            string eiiClean = eiiType.GetFullUnmangledNameWithTypeParameters();

            eiiClean = eiiClean.Replace('.', '#');
            eiiClean = eiiClean.Replace('+', '#');  // !EFW - Treat nested class separators like periods
            eiiClean = eiiClean.Replace(',', '@');  // Change the separator between parameters
            eiiClean = eiiClean.Replace('<', '{');  // Change the parameter brackets
            eiiClean = eiiClean.Replace('>', '}');

            sb.Append(eiiClean);
        }

        /// <summary>
        /// This is used to write out the parameter types for a member
        /// </summary>
        /// <param name="parameters">The list of parameters to write out</param>
        /// <param name="sb">The string builder to which the parameter names are written</param>
        private static void WriteParameters(ParameterList parameters, StringBuilder sb)
        {
            if(parameters != null && parameters.Count != 0)
            {
                sb.Append("(");

                for(int i = 0; i < parameters.Count; i++)
                {
                    if(i > 0)
                        sb.Append(",");

                    WriteType(parameters[i].Type, sb);
                }

                sb.Append(")");
            }
        }
        #endregion
    }
}
