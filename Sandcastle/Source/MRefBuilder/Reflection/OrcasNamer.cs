// Copyright (c) Microsoft Corporation.  All rights reserved.
//

// Change history:
// 03/29/2012 - EFW - Fixed WriteTemplate() so that it uses the correct template parameter names which
// don't always match the base class's template parameter names (i.e. Collection<TControl> vs Collection<T>).

using System;
using System.IO;

using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection
{

    public class OrcasNamer : ApiNamer
    {

        public override string GetMemberName(Member member)
        {

            using(TextWriter writer = new StringWriter())
            {

                switch(member.NodeType)
                {
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

        public override string GetNamespaceName(Namespace space)
        {
            using(TextWriter writer = new StringWriter())
            {
                writer.Write("N:");
                WriteNamespace(space, writer);
                return (writer.ToString());
            }
        }

        public override string GetTypeName(TypeNode type)
        {
            using(TextWriter writer = new StringWriter())
            {
                writer.Write("T:");
                WriteType(type, writer);
                return (writer.ToString());
            }
        }


        private static string GetName(Member entity)
        {

            using(TextWriter writer = new StringWriter())
            {

                TypeNode type = entity as TypeNode;
                if(type != null)
                {
                    writer.Write("T:");
                    WriteType(type, writer);
                    return (writer.ToString());
                }

                switch(entity.NodeType)
                {
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

        private static void WriteConstructor(InstanceInitializer constructor, TextWriter writer)
        {
            WriteType(constructor.DeclaringType, writer);
            writer.Write(".#ctor");
            WriteParameters(constructor.Parameters, writer);
        }

        private static void WriteEvent(Event trigger, TextWriter writer)
        {
            WriteType(trigger.DeclaringType, writer);

            Event eiiTrigger = null;
            if(trigger.IsPrivate && trigger.IsVirtual)
            {
                Event[] eiiTriggers = ReflectionUtilities.GetImplementedEvents(trigger);
                if(eiiTriggers.Length > 0)
                    eiiTrigger = eiiTriggers[0];
            }

            if(eiiTrigger != null)
            {
                TypeNode eiiType = eiiTrigger.DeclaringType;
                TextWriter eiiWriter = new StringWriter();

                if(eiiType != null && eiiType.Template != null)
                {
                    writer.Write(".");
                    WriteTemplate(eiiType, writer);
                }
                else
                {
                    WriteType(eiiType, eiiWriter);
                    writer.Write(".");
                    writer.Write(eiiWriter.ToString().Replace('.', '#'));
                }

                writer.Write("#");
                writer.Write(eiiTrigger.Name.Name);
            }
            else
            {
                writer.Write(".{0}", trigger.Name.Name);
            }
        }

        private static void WriteField(Field field, TextWriter writer)
        {
            WriteType(field.DeclaringType, writer);
            writer.Write(".{0}", field.Name.Name);
        }

        private static void WriteMethod(Method method, TextWriter writer)
        {
            string name = method.Name.Name;
            WriteType(method.DeclaringType, writer);

            Method eiiMethod = null;
            if(method.IsPrivate && method.IsVirtual)
            {
                MethodList eiiMethods = method.ImplementedInterfaceMethods;
                if(eiiMethods.Count > 0)
                    eiiMethod = eiiMethods[0];
            }
            if(eiiMethod != null)
            { //explicitly implemented interface
                TypeNode eiiType = eiiMethod.DeclaringType;
                TextWriter eiiWriter = new StringWriter();


                //we need to keep the param names instead of turning them into numbers
                //get the template to the right format
                if(eiiType != null && eiiType.Template != null)
                {
                    writer.Write(".");
                    WriteTemplate(eiiType, writer);
                }
                else //revert back to writing the type the old way if there is no template
                {
                    WriteType(eiiType, eiiWriter);
                    writer.Write(".");
                    writer.Write(eiiWriter.ToString().Replace('.', '#'));
                }

                writer.Write("#");
                writer.Write(eiiMethod.Name.Name);
            }
            else
            {
                writer.Write(".{0}", name);
            }
            if(method.IsGeneric)
            {
                TypeNodeList genericParameters = method.TemplateParameters;
                if(genericParameters != null)
                {
                    writer.Write("``{0}", genericParameters.Count);
                }
            }
            WriteParameters(method.Parameters, writer);
            // add ~ for conversion operators
            if((name == "op_Implicit") || (name == "op_Explicit"))
            {
                writer.Write("~");
                WriteType(method.ReturnType, writer);
            }

        }

        // The actual logic to construct names

        private static void WriteNamespace(Namespace space, TextWriter writer)
        {
            writer.Write(space.Name);
        }

        private static void WriteParameters(ParameterList parameters, TextWriter writer)
        {
            if((parameters == null) || (parameters.Count == 0))
                return;
            writer.Write("(");
            for(int i = 0; i < parameters.Count; i++)
            {
                if(i > 0)
                    writer.Write(",");
                WriteType(parameters[i].Type, writer);
            }
            writer.Write(")");
        }

        private static void WriteProperty(Property property, TextWriter writer)
        {
            WriteType(property.DeclaringType, writer);
            //Console.WriteLine( "{0}::{1}", property.DeclaringType.FullName, property.Name );

            Property eiiProperty = null;
            if(property.IsPrivate && property.IsVirtual)
            {
                Property[] eiiProperties = ReflectionUtilities.GetImplementedProperties(property);
                if(eiiProperties.Length > 0)
                    eiiProperty = eiiProperties[0];
            }



            if(eiiProperty != null)
            {
                TypeNode eiiType = eiiProperty.DeclaringType;
                TextWriter eiiWriter = new StringWriter();


                if(eiiType != null && eiiType.Template != null)
                {
                    writer.Write(".");
                    WriteTemplate(eiiType, writer);
                }
                else
                {
                    WriteType(eiiType, eiiWriter);
                    writer.Write(".");
                    writer.Write(eiiWriter.ToString().Replace('.', '#'));
                }

                writer.Write("#");
                writer.Write(eiiProperty.Name.Name);
            }
            else
            {
                writer.Write(".{0}", property.Name.Name);
            }
            ParameterList parameters = property.Parameters;
            WriteParameters(parameters, writer);
        }

        private static void WriteStaticConstructor(StaticInitializer constructor, TextWriter writer)
        {
            WriteType(constructor.DeclaringType, writer);
            writer.Write(".#cctor");
            WriteParameters(constructor.Parameters, writer);
        }

        /// <summary>
        /// Used for explicitly implemented interfaces to convert the template to the
        /// format used in the comments file.
        /// </summary>
        /// <param name="type">EII Type</param>
        /// <param name="writer"></param>
        private static void WriteTemplate(TypeNode eiiType, TextWriter writer)
        {
//            string eiiClean = eiiType.Template.ToString();
            // !EFW Use this instead as the type paramater may be different in the user's code
            // (i.e. Collection<TControl> instead of Collection<T>.
            string eiiClean = eiiType.GetFullUnmangledNameWithTypeParameters();

            eiiClean = eiiClean.Replace('.', '#');
            eiiClean = eiiClean.Replace(',', '@'); //change the seperator between params
            eiiClean = eiiClean.Replace('<', '{'); //change the parameter brackets
            eiiClean = eiiClean.Replace('>', '}');
            writer.Write(eiiClean);
        }

        private static void WriteType(TypeNode type, TextWriter writer)
        {
            switch(type.NodeType)
            {
                case NodeType.ArrayType:
                    ArrayType array = type as ArrayType;
                    WriteType(array.ElementType, writer);
                    writer.Write("[");
                    if(array.Rank > 1)
                    {
                        for(int i = 0; i < array.Rank; i++)
                        {
                            if(i > 0)
                                writer.Write(",");
                            writer.Write("0:");
                        }
                    }
                    writer.Write("]");
                    break;
                case NodeType.Reference:
                    Reference reference = type as Reference;
                    TypeNode referencedType = reference.ElementType;
                    WriteType(referencedType, writer);
                    writer.Write("@");
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
                    if(type.IsTemplateParameter)
                    {
                        ITypeParameter gtp = (ITypeParameter)type;
                        if(gtp.DeclaringMember is TypeNode)
                        {
                            writer.Write("`");
                        }
                        else if(gtp.DeclaringMember is Method)
                        {
                            writer.Write("``");
                        }
                        else
                        {
                            throw new InvalidOperationException("Generic parameter not on type or method.");
                        }
                        writer.Write(gtp.ParameterListIndex);
                    }
                    else
                    {
                        // namespace
                        TypeNode declaringType = type.DeclaringType;
                        if(declaringType != null)
                        {
                            // names of nested types begin with outer type name
                            WriteType(declaringType, writer);
                            writer.Write(".");
                        }
                        else
                        {
                            // otherwise just prepend the namespace
                            Identifier space = type.Namespace;
                            if((space != null) && !String.IsNullOrEmpty(space.Name))
                            {
                                //string space = type.Namespace.Name;
                                //if (space != null && space.Length > 0) {
                                writer.Write(space.Name);
                                writer.Write(".");
                            }
                        }
                        // name
                        writer.Write(type.GetUnmangledNameWithoutTypeParameters());
                        // generic parameters
                        if(type.IsGeneric)
                        {
                            // number of parameters
                            TypeNodeList parameters = type.TemplateParameters;
                            if(parameters != null)
                            {
                                writer.Write("`{0}", parameters.Count);
                            }
                            // arguments
                            TypeNodeList arguments = type.TemplateArguments;
                            if((arguments != null) && (arguments.Count > 0))
                            {
                                writer.Write("{");
                                for(int i = 0; i < arguments.Count; i++)
                                {
                                    TypeNode argument = arguments[i];
                                    if(i > 0)
                                        writer.Write(",");
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
