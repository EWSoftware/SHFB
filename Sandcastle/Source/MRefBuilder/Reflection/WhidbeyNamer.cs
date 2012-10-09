// Copyright (c) Microsoft Corporation.  All rights reserved.
//

using System;
using System.Collections.Generic;
using System.IO;

using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection {

    public class WhidbeyNamer : ApiNamer {

        public override string GetMemberName(Member member) {

            using (TextWriter writer = new StringWriter()) {

                switch (member.NodeType) {
                    case NodeType.Field:
                        writer.Write("F:");
                        WriteField((Field)member, writer);
                        break;
                    case NodeType.Property:
                        writer.Write("P:");
                        WriteProperty((Property)member, writer);
                        break;
                    case NodeType.Method:
                        writer.Write("M:");
                        WriteMethod((Method)member, writer);
                        break;
                    case NodeType.InstanceInitializer:
                        writer.Write("M:");
                        WriteConstructor((InstanceInitializer)member, writer);
                        break;
                    case NodeType.StaticInitializer:
                        writer.Write("M:");
                        WriteStaticConstructor((StaticInitializer)member, writer);
                        break;
                    case NodeType.Event:
                        writer.Write("E:");
                        WriteEvent((Event)member, writer);
                        break;
                }

                return (writer.ToString());

            }

        }

        public override string GetNamespaceName(Namespace space) {
            using (TextWriter writer = new StringWriter()) {
                writer.Write("N:");
                WriteNamespace(space, writer);
                return (writer.ToString());
            }
        }

        public override string GetTypeName(TypeNode type) {
            using (TextWriter writer = new StringWriter()) {
                writer.Write("T:");
                WriteType(type, writer);
                return (writer.ToString());
            }
        }


        private static string GetName(Member entity) {

            using (TextWriter writer = new StringWriter()) {

                TypeNode type = entity as TypeNode;
                if (type != null) {
                    writer.Write("T:");
                    WriteType(type, writer);
                    return (writer.ToString());
                }

                switch (entity.NodeType) {
                    case NodeType.Namespace:
                        writer.Write("N:");
                        WriteNamespace(entity as Namespace, writer);
                        break;
                    case NodeType.Field:
                        writer.Write("F:");
                        WriteField(entity as Field, writer);
                        break;
                    case NodeType.Property:
                        writer.Write("P:");
                        WriteProperty(entity as Property, writer);
                        break;
                    case NodeType.Method:
                        writer.Write("M:");
                        WriteMethod(entity as Method, writer);
                        break;
                    case NodeType.InstanceInitializer:
                        writer.Write("M:");
                        WriteConstructor(entity as InstanceInitializer, writer);
                        break;
                    case NodeType.StaticInitializer:
                        writer.Write("M:");
                        WriteStaticConstructor(entity as StaticInitializer, writer);
                        break;
                    case NodeType.Event:
                        writer.Write("E:");
                        WriteEvent(entity as Event, writer);
                        break;
                }

                return (writer.ToString());

            }

        }

        private static void WriteConstructor(InstanceInitializer constructor, TextWriter writer) {
            WriteType(constructor.DeclaringType, writer);
            writer.Write(".#ctor");
            WriteParameters(constructor.Parameters, writer);
        }

        private static void WriteEvent(Event trigger, TextWriter writer) {
            WriteType(trigger.DeclaringType, writer);
            writer.Write(".{0}", trigger.Name.Name);
        }

        private static void WriteField(Field field, TextWriter writer) {
            WriteType(field.DeclaringType, writer);
            writer.Write(".{0}", field.Name.Name);
        }

        private static void WriteMethod(Method method, TextWriter writer) {
            string name = method.Name.Name;
            WriteType(method.DeclaringType, writer);
            writer.Write(".{0}", name);
            if (method.IsGeneric) {
                TypeNodeList genericParameters = method.TemplateParameters;
                if (genericParameters != null) {
                    writer.Write("``{0}", genericParameters.Count);
                }
            }
            WriteParameters(method.Parameters, writer);
            // add ~ for conversion operators
            if ((name == "op_Implicit") || (name == "op_Explicit")) {
                writer.Write("~");
                WriteType(method.ReturnType, writer);
            }

        }

        // The actual logic to construct names

        private static void WriteNamespace(Namespace space, TextWriter writer) {
            writer.Write(space.Name);
        }

        private static void WriteParameters(ParameterList parameters, TextWriter writer) {
            if ((parameters == null) || (parameters.Count == 0)) return;
            writer.Write("(");
            for (int i = 0; i < parameters.Count; i++) {
                if (i > 0) writer.Write(",");
                WriteType(parameters[i].Type, writer);
            }
            writer.Write(")");
        }

        private static void WriteProperty(Property property, TextWriter writer) {
            WriteType(property.DeclaringType, writer);
            writer.Write(".{0}", property.Name.Name);
            ParameterList parameters = property.Parameters;
            WriteParameters(parameters, writer);
        }

        private static void WriteStaticConstructor(StaticInitializer constructor, TextWriter writer) {
            WriteType(constructor.DeclaringType, writer);
            writer.Write(".#cctor");
            WriteParameters(constructor.Parameters, writer);
        }

        private static void WriteType(TypeNode type, TextWriter writer) {
            switch (type.NodeType) {
                case NodeType.ArrayType:
                    ArrayType array = type as ArrayType;
                    WriteType(array.ElementType, writer);
                    writer.Write("[");
                    for (int i = 1; i < array.Rank; i++) writer.Write(",");
                    writer.Write("]");
                    break;
                case NodeType.Reference:
                    Reference reference = type as Reference;
                    TypeNode referencedType = reference.ElementType;
                    WriteType(referencedType, writer);

                    // DocStudio fails to add @ to template parameters or arrays of template
                    // parameters, so we have to mirror this bizarre behavior here
                    bool writeAt = true;
                    if (referencedType.IsTemplateParameter) writeAt = false;
                    if (referencedType.NodeType == NodeType.ArrayType) {
                        ArrayType referencedArray = referencedType as ArrayType;
                        if (referencedArray.ElementType.IsTemplateParameter) writeAt = false;
                    }
                    if (writeAt) writer.Write("@");
                    break;
                case NodeType.Pointer:
                    Pointer pointer = type as Pointer;
                    WriteType(pointer.ElementType, writer);
                    writer.Write("*");
                    break;
                case NodeType.OptionalModifier:
                    TypeModifier optionalModifierClause = type as TypeModifier;
                    WriteType(optionalModifierClause.ModifiedType, writer);
                    writer.Write("!");
                    WriteType(optionalModifierClause.Modifier, writer);
                    break;
                case NodeType.RequiredModifier:
                    TypeModifier requiredModifierClause = type as TypeModifier;
                    WriteType(requiredModifierClause.ModifiedType, writer);
                    writer.Write("|");
                    WriteType(requiredModifierClause.Modifier, writer);
                    break;
                default:
                    if (type.IsTemplateParameter) {
                        ITypeParameter gtp = (ITypeParameter)type;
                        if (gtp.DeclaringMember is TypeNode) {
                            writer.Write("`");
                        } else if (gtp.DeclaringMember is Method) {
                            writer.Write("``");
                        } else {
                            throw new InvalidOperationException("Generic parameter not on type or method.");
                        }
                        writer.Write(gtp.ParameterListIndex);
                    } else {
                        // namespace
                        TypeNode declaringType = type.DeclaringType;
                        if (declaringType != null) {
                            // names of nested types begin with outer type name
                            WriteType(declaringType, writer);
                            writer.Write(".");
                        } else {
                            // otherwise just prepend the namespace
                            string space = type.Namespace.Name;
                            if (space != null && space.Length > 0) {
                                writer.Write(space);
                                writer.Write(".");
                            }
                        }
                        // name
                        writer.Write(type.GetUnmangledNameWithoutTypeParameters());
                        // generic parameters
                        if (type.IsGeneric) {
                            // number of parameters
                            TypeNodeList parameters = type.TemplateParameters;
                            if (parameters != null) {
                                writer.Write("`{0}", parameters.Count);
                            }
                            // arguments
                            TypeNodeList arguments = type.TemplateArguments;
                            if ((arguments != null) && (arguments.Count > 0)) {
                                writer.Write("{");
                                for (int i = 0; i < arguments.Count; i++) {
                                    TypeNode argument = arguments[i];
                                    if (i > 0) writer.Write(",");
                                    WriteType(arguments[i], writer);
                                }
                                writer.Write("}");
                            }
                        }
                    }
                    break;
            }
        }

    }

}
