// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 01/30/2012 - EFW - Fixed WriteValue() so that it outputs numeric attribute values.
// 02/09/2012 - EFW - Added support for optional parameters and property getter/setter attributes.
// 02/14/2012 - EFW - Added support for fixed keyword

using System;
using System.Globalization;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools
{
    public sealed class CSharpDeclarationSyntaxGenerator : SyntaxGeneratorTemplate
    {
        public CSharpDeclarationSyntaxGenerator(XPathNavigator configuration) : base(configuration)
        {
            if(String.IsNullOrEmpty(Language))
                Language = "CSharp";
        }

        // namespace: done
        public override void WriteNamespaceSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string name = reflection.Evaluate(apiNameExpression).ToString();

            writer.WriteKeyword("namespace");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
        }

        // class: done
        public override void WriteClassSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            string name = reflection.Evaluate(apiNameExpression).ToString();
            bool isAbstract = (bool)reflection.Evaluate(apiIsAbstractTypeExpression);
            bool isSealed = (bool)reflection.Evaluate(apiIsSealedTypeExpression);
            bool isSerializable = (bool)reflection.Evaluate(apiIsSerializableTypeExpression);

            if(isSerializable)
                WriteAttribute("T:System.SerializableAttribute", true, writer);
            WriteAttributes(reflection, writer);
            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            if(isAbstract)
            {
                if(isSealed)
                {
                    writer.WriteKeyword("static");
                }
                else
                {
                    writer.WriteKeyword("abstract");
                }
                writer.WriteString(" ");
            }
            else
            {
                if(isSealed)
                {
                    writer.WriteKeyword("sealed");
                    writer.WriteString(" ");
                }
            }
            writer.WriteKeyword("class");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            WriteGenericTemplates(reflection, writer, false);
            WriteBaseClassAndImplementedInterfaces(reflection, writer);
            WriteGenericTemplateConstraints(reflection, writer);

        }


        // structure: done
        public override void WriteStructureSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isSerializable = (bool)reflection.Evaluate(apiIsSerializableTypeExpression);

            if(isSerializable)
                WriteAttribute("T:System.SerializableAttribute", true, writer);
            WriteAttributes(reflection, writer);
            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            writer.WriteKeyword("struct");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            WriteGenericTemplates(reflection, writer, false);
            WriteImplementedInterfaces(reflection, writer);
            WriteGenericTemplateConstraints(reflection, writer);

        }

        // interface: done
        public override void WriteInterfaceSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            string name = (string)reflection.Evaluate(apiNameExpression);

            WriteAttributes(reflection, writer);
            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            writer.WriteKeyword("interface");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            WriteGenericTemplates(reflection, writer, true); // interfaces need co/contravariance info
            WriteImplementedInterfaces(reflection, writer);
            WriteGenericTemplateConstraints(reflection, writer);

        }

        // delegate: done
        public override void WriteDelegateSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isSerializable = (bool)reflection.Evaluate(apiIsSerializableTypeExpression);

            if(isSerializable)
                WriteAttribute("T:System.SerializableAttribute", true, writer);
            WriteAttributes(reflection, writer);
            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            writer.WriteKeyword("delegate");
            writer.WriteString(" ");
            WriteReturnValue(reflection, writer);
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            WriteGenericTemplates(reflection, writer, true); // delegates need co/contravariance info
            WriteMethodParameters(reflection, writer);
            WriteGenericTemplateConstraints(reflection, writer);

        }

        // enumeration: still need to handle non-standard base
        public override void WriteEnumerationSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isSerializable = (bool)reflection.Evaluate(apiIsSerializableTypeExpression);

            if(isSerializable)
                WriteAttribute("T:System.SerializableAttribute", true, writer);
            WriteAttributes(reflection, writer);
            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            writer.WriteKeyword("enum");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
        }

        // constructor: done
        public override void WriteConstructorSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            string name = (string)reflection.Evaluate(apiContainingTypeNameExpression);
            bool isStatic = (bool)reflection.Evaluate(apiIsStaticExpression);

            WriteAttributes(reflection, writer);
            if(isStatic)
            {
                writer.WriteKeyword("static");
            }
            else
            {
                WriteVisibility(reflection, writer);
            }
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            WriteMethodParameters(reflection, writer);

        }

        // normal method: done
        public override void WriteNormalMethodSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(IsUnsupportedVarargs(reflection, writer))
                return;

            string name = (string)reflection.Evaluate(apiNameExpression);

            bool isExplicit = (bool)reflection.Evaluate(apiIsExplicitImplementationExpression);

            WriteAttributes(reflection, writer);
            if(!isExplicit)
                WriteProcedureModifiers(reflection, writer);
            WriteReturnValue(reflection, writer);
            writer.WriteString(" ");

            if(isExplicit)
            {
                XPathNavigator member = reflection.SelectSingleNode(apiImplementedMembersExpression);
                //string memberName = (string) member.Evaluate(nameExpression);
                //string id = member.GetAttribute("api", String.Empty);
                XPathNavigator contract = member.SelectSingleNode(memberDeclaringTypeExpression);
                WriteTypeReference(contract, writer);
                writer.WriteString(".");
                WriteMemberReference(member, writer);
                //writer.WriteReferenceLink(id);
            }
            else
            {
                writer.WriteIdentifier(name);
            }
            WriteGenericTemplates(reflection, writer, false);
            WriteMethodParameters(reflection, writer);
            WriteGenericTemplateConstraints(reflection, writer);

        }

        // operator: done
        public override void WriteOperatorSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string name = (string)reflection.Evaluate(apiNameExpression);
            string identifier = null;

            if(!(bool)reflection.Evaluate(apiIsUdtReturnExpression))
            {
                switch(name)
                {
                    // unary math operators
                    case "UnaryPlus":
                        identifier = "+";
                        break;
                    case "UnaryNegation":
                        identifier = "-";
                        break;
                    case "Increment":
                        identifier = "++";
                        break;
                    case "Decrement":
                        identifier = "--";
                        break;
                    // unary logical operators
                    case "LogicalNot":
                        identifier = "!";
                        break;
                    case "True":
                        identifier = "true";
                        break;
                    case "False":
                        identifier = "false";
                        break;
                    // binary comparison operators
                    case "Equality":
                        identifier = "==";
                        break;
                    case "Inequality":
                        identifier = "!=";
                        break;
                    case "LessThan":
                        identifier = "<";
                        break;
                    case "GreaterThan":
                        identifier = ">";
                        break;
                    case "LessThanOrEqual":
                        identifier = "<=";
                        break;
                    case "GreaterThanOrEqual":
                        identifier = ">=";
                        break;
                    // binary math operators
                    case "Addition":
                        identifier = "+";
                        break;
                    case "Subtraction":
                        identifier = "-";
                        break;
                    case "Multiply":
                        identifier = "*";
                        break;
                    case "Division":
                        identifier = "/";
                        break;
                    case "Modulus":
                        identifier = "%";
                        break;
                    // binary logical operators
                    case "BitwiseAnd":
                        identifier = "&";
                        break;
                    case "BitwiseOr":
                        identifier = "|";
                        break;
                    case "ExclusiveOr":
                        identifier = "^";
                        break;
                    // bit-array operators
                    case "OnesComplement":
                        identifier = "~";
                        break;
                    case "LeftShift":
                        identifier = "<<";
                        break;
                    case "RightShift":
                        identifier = ">>";
                        break;
                    case "Assign":
                        identifier = "=";
                        break;
                    // unrecognized operator
                    default:
                        identifier = null;
                        break;
                }
            }
            if(identifier == null)
            {
                writer.WriteMessage("UnsupportedOperator_" + Language);
            }
            else
            {
                WriteProcedureModifiers(reflection, writer);
                WriteReturnValue(reflection, writer);
                writer.WriteString(" ");
                writer.WriteKeyword("operator");
                writer.WriteString(" ");
                writer.WriteIdentifier(identifier);
                WriteMethodParameters(reflection, writer);
            }
        }

        // cast: done
        public override void WriteCastSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            string name = (string)reflection.Evaluate(apiNameExpression);

            WriteProcedureModifiers(reflection, writer);
            if(name == "Implicit")
            {
                writer.WriteKeyword("implicit operator");
            }
            else if(name == "Explicit")
            {
                writer.WriteKeyword("explicit operator");
            }
            else
            {
                throw new InvalidCastException("Invalid cast.");
            }
            writer.WriteString(" ");
            WriteReturnValue(reflection, writer);
            writer.WriteString(" ");
            WriteMethodParameters(reflection, writer);

        }

        public override void WritePropertySyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isDefault = (bool)reflection.Evaluate(apiIsDefaultMemberExpression);
            bool isGettable = (bool)reflection.Evaluate(apiIsReadPropertyExpression);
            bool isSettable = (bool)reflection.Evaluate(apiIsWritePropertyExpression);
            bool isExplicit = (bool)reflection.Evaluate(apiIsExplicitImplementationExpression);
            XPathNodeIterator parameters = reflection.Select(apiParametersExpression);

            WriteAttributes(reflection, writer);
            if(!isExplicit)
                WriteProcedureModifiers(reflection, writer);
            WriteReturnValue(reflection, writer);
            writer.WriteString(" ");

            if(isExplicit)
            {
                XPathNavigator member = reflection.SelectSingleNode(apiImplementedMembersExpression);
                XPathNavigator contract = member.SelectSingleNode(memberDeclaringTypeExpression);
                WriteTypeReference(contract, writer);
                writer.WriteString(".");
                if(parameters.Count > 0)
                {
                    // In C#, EII property with parameters is an indexer; use 'this' instead of the property's name
                    writer.WriteKeyword("this");
                }
                else
                {
                    WriteMemberReference(member, writer);
                }
            }
            else
            {
                // In C#, any property with parameters is an indexer, which is declared using 'this' instead of the property's name
                if(isDefault || parameters.Count > 0)
                {
                    writer.WriteKeyword("this");
                }
                else
                {
                    writer.WriteIdentifier(name);
                }
            }

            WritePropertyParameters(reflection, writer);
            writer.WriteString(" {");

            if(isGettable)
            {
                // !EFW - Added support for getter/setter attributes
                XPathNavigator getter = reflection.SelectSingleNode(apiGetterExpression);

                if(getter.HasChildren)
                {
                    writer.WriteLine();
                    this.WriteAttributes(getter, writer, "\t");
                    writer.WriteString("\t");
                }
                else
                    writer.WriteString(" ");

                string getVisibility = (string)reflection.Evaluate(apiGetVisibilityExpression);

                if(!String.IsNullOrEmpty(getVisibility))
                {
                    WriteVisibility(getVisibility, writer);
                    writer.WriteString(" ");
                }

                writer.WriteKeyword("get");
                writer.WriteString(";");

                if(getter.HasChildren)
                    writer.WriteLine();
            }

            if(isSettable)
            {
                // !EFW - Added support for getter/setter attributes
                XPathNavigator setter = reflection.SelectSingleNode(apiSetterExpression);

                if(setter.HasChildren)
                {
                    if(!isGettable)
                        writer.WriteLine();

                    this.WriteAttributes(setter, writer, "\t");
                    writer.WriteString("\t");
                }
                else
                    writer.WriteString(" ");

                string setVisibility = (string)reflection.Evaluate(apiSetVisibilityExpression);

                if(!String.IsNullOrEmpty(setVisibility))
                {
                    WriteVisibility(setVisibility, writer);
                    writer.WriteString(" ");
                }

                writer.WriteKeyword("set");
                writer.WriteString(";");

                if(setter.HasChildren)
                    writer.WriteLine();
            }

            writer.WriteString(" }");
        }

        public override void WriteEventSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string name = (string)reflection.Evaluate(apiNameExpression);
            XPathNavigator handler = reflection.SelectSingleNode(apiHandlerOfEventExpression);
            bool isExplicit = (bool)reflection.Evaluate(apiIsExplicitImplementationExpression);

            WriteAttributes(reflection, writer);
            if(!isExplicit)
                WriteProcedureModifiers(reflection, writer);
            writer.WriteString("event");
            writer.WriteString(" ");
            WriteTypeReference(handler, writer);
            writer.WriteString(" ");

            if(isExplicit)
            {
                XPathNavigator member = reflection.SelectSingleNode(apiImplementedMembersExpression);
                //string id = (string) member.GetAttribute("api", String.Empty);
                XPathNavigator contract = member.SelectSingleNode(memberDeclaringTypeExpression);
                WriteTypeReference(contract, writer);
                writer.WriteString(".");
                WriteMemberReference(member, writer);
                //writer.WriteReferenceLink(id);
                // writer.WriteIdentifier(memberName);
            }
            else
            {
                writer.WriteIdentifier(name);
            }
        }

        private static void WriteProcedureModifiers(XPathNavigator reflection, SyntaxWriter writer)
        {
            // interface members don't get modified
            string typeSubgroup = (string)reflection.Evaluate(apiContainingTypeSubgroupExpression);

            if(typeSubgroup == "interface")
                return;

            bool isStatic = (bool)reflection.Evaluate(apiIsStaticExpression);
            bool isVirtual = (bool)reflection.Evaluate(apiIsVirtualExpression);
            bool isAbstract = (bool)reflection.Evaluate(apiIsAbstractProcedureExpression);
            bool isFinal = (bool)reflection.Evaluate(apiIsFinalExpression);
            bool isOverride = (bool)reflection.Evaluate(apiIsOverrideExpression);

            WriteVisibility(reflection, writer);
            writer.WriteString(" ");

            if(isStatic)
            {
                writer.WriteKeyword("static");
                writer.WriteString(" ");
            }
            else
            {
                if(isVirtual)
                {
                    if(isAbstract)
                    {
                        writer.WriteKeyword("abstract");
                        writer.WriteString(" ");
                    }
                    else if(isOverride)
                    {
                        writer.WriteKeyword("override");
                        writer.WriteString(" ");
                        if(isFinal)
                        {
                            writer.WriteKeyword("sealed");
                            writer.WriteString(" ");
                        }
                    }
                    else
                    {
                        if(!isFinal)
                        {
                            writer.WriteKeyword("virtual");
                            writer.WriteString(" ");
                        }
                    }
                }
            }
        }

        public override void WriteFieldSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isStatic = (bool)reflection.Evaluate(apiIsStaticExpression);
            bool isLiteral = (bool)reflection.Evaluate(apiIsLiteralFieldExpression);
            bool isInitOnly = (bool)reflection.Evaluate(apiIsInitOnlyFieldExpression);
            bool isSerialized = (bool)reflection.Evaluate(apiIsSerializedFieldExpression);

            // !EFW - Added support for fixed keyword
            XPathNavigator fixedAttribute = reflection.SelectSingleNode(apiFixedAttribute);

            if(!isSerialized)
                WriteAttribute("T:System.NonSerializedAttribute", true, writer);

            WriteAttributes(reflection, writer);
            WriteVisibility(reflection, writer);
            writer.WriteString(" ");

            if(isStatic)
            {
                if(isLiteral)
                    writer.WriteKeyword("const");
                else
                    writer.WriteKeyword("static");

                writer.WriteString(" ");
            }

            if(isInitOnly)
            {
                writer.WriteKeyword("readonly");
                writer.WriteString(" ");
            }

            if(fixedAttribute != null)
            {
                writer.WriteKeyword("fixed");
                writer.WriteString(" ");

                XPathNavigator bufferType = fixedAttribute.SelectSingleNode(apiFixedBufferType),
                    bufferSize = fixedAttribute.SelectSingleNode(apiFixedBufferSize);

                if(bufferType != null)
                {
                    WriteTypeReference(bufferType, writer);
                    writer.WriteString(" ");
                }

                writer.WriteIdentifier(name);

                if(bufferSize != null)
                {
                    writer.WriteString("[");
                    writer.WriteString(bufferSize.Value);
                    writer.WriteString("]");
                }
            }
            else
            {
                WriteReturnValue(reflection, writer);
                writer.WriteString(" ");
                writer.WriteIdentifier(name);
            }
        }

        // Visibility

        private static void WriteVisibility(XPathNavigator reflection, SyntaxWriter writer)
        {
            string visibility = reflection.Evaluate(apiVisibilityExpression).ToString();
            WriteVisibility(visibility, writer);
        }

        private static void WriteVisibility(string visibility, SyntaxWriter writer)
        {
            switch(visibility)
            {
                case "public":
                    writer.WriteKeyword("public");
                    break;
                case "family":
                    writer.WriteKeyword("protected");
                    break;
                case "family or assembly":
                    writer.WriteKeyword("protected internal");
                    break;
                case "assembly":
                    writer.WriteKeyword("internal");
                    break;
                case "private":
                    writer.WriteKeyword("private");
                    break;
                case "family and assembly":
                    // this isn't handled in C#
                    break;
            }
        }

        // Attributes

        // !EFW - Added newLine parameter
        private static void WriteAttribute(string reference, bool newLine, SyntaxWriter writer)
        {
            writer.WriteString("[");
            writer.WriteReferenceLink(reference);
            writer.WriteString("]");

            if(newLine)
                writer.WriteLine();
        }

        // !EFW - Added indent parameter for property getter/setter attributes
        private void WriteAttributes(XPathNavigator reflection, SyntaxWriter writer, string indent = null)
        {
            XPathNodeIterator attributes = (XPathNodeIterator)reflection.Evaluate(apiAttributesExpression);

            foreach(XPathNavigator attribute in attributes)
            {
                XPathNavigator type = attribute.SelectSingleNode(attributeTypeExpression);

                // !EFW - Ignore FixedBufferAttribute too
                if(type.GetAttribute("api", String.Empty) == "T:System.Runtime.CompilerServices.ExtensionAttribute" ||
                  type.GetAttribute("api", String.Empty) == "T:System.Runtime.CompilerServices.FixedBufferAttribute")
                    continue;

                if(!String.IsNullOrEmpty(indent))
                    writer.WriteString(indent);

                writer.WriteString("[");
                WriteTypeReference(type, writer);

                XPathNodeIterator arguments = (XPathNodeIterator)attribute.Select(attributeArgumentsExpression);
                XPathNodeIterator assignments = (XPathNodeIterator)attribute.Select(attributeAssignmentsExpression);

                if((arguments.Count > 0) || (assignments.Count > 0))
                {
                    writer.WriteString("(");
                    while(arguments.MoveNext())
                    {
                        XPathNavigator argument = arguments.Current;
                        if(arguments.CurrentPosition > 1)
                        {
                            writer.WriteString(", ");

                            if(writer.Position > maxPosition)
                            {
                                writer.WriteLine();

                                if(!String.IsNullOrEmpty(indent))
                                    writer.WriteString(indent);

                                writer.WriteString("\t");
                            }
                        }
                        WriteValue(argument, writer);
                    }
                    if((arguments.Count > 0) && (assignments.Count > 0))
                        writer.WriteString(", ");
                    while(assignments.MoveNext())
                    {
                        XPathNavigator assignment = assignments.Current;

                        if(assignments.CurrentPosition > 1)
                        {
                            writer.WriteString(", ");

                            if(writer.Position > maxPosition)
                            {
                                writer.WriteLine();

                                if(!String.IsNullOrEmpty(indent))
                                    writer.WriteString(indent);

                                writer.WriteString("\t");
                            }
                        }

                        writer.WriteString((string)assignment.Evaluate(assignmentNameExpression));
                        writer.WriteString(" = ");
                        WriteValue(assignment, writer);

                    }
                    writer.WriteString(")");
                }

                writer.WriteString("]");
                writer.WriteLine();
            }
        }

        private void WriteValue(XPathNavigator parent, SyntaxWriter writer)
        {
            XPathNavigator type = parent.SelectSingleNode(attributeTypeExpression);
            XPathNavigator value = parent.SelectSingleNode(valueExpression);

            switch(value.LocalName)
            {
                case "nullValue":
                    writer.WriteKeyword("null");
                    break;

                case "typeValue":
                    writer.WriteKeyword("typeof");
                    writer.WriteString("(");
                    WriteTypeReference(value.SelectSingleNode(typeExpression), writer);
                    writer.WriteString(")");
                    break;

                case "enumValue":
                    XPathNodeIterator fields = value.SelectChildren(XPathNodeType.Element);
                    while(fields.MoveNext())
                    {
                        string name = fields.Current.GetAttribute("name", String.Empty);
                        if(fields.CurrentPosition > 1)
                            writer.WriteString("|");
                        WriteTypeReference(type, writer);
                        writer.WriteString(".");
                        writer.WriteString(name);
                    }
                    break;

                case "value":
                    string text = value.Value;
                    string typeId = type.GetAttribute("api", String.Empty);

                    switch(typeId)
                    {
                        case "T:System.String":
                            writer.WriteString("\"");
                            writer.WriteString(text);
                            writer.WriteString("\"");
                            break;

                        case "T:System.Boolean":
                            writer.WriteKeyword(Convert.ToBoolean(text, CultureInfo.InvariantCulture) ?
                                "true" : "false");
                            break;

                        case "T:System.Char":
                            writer.WriteString("'");
                            writer.WriteString(text);
                            writer.WriteString("'");
                            break;

                        // !EFW - Bug fix.  Support numeric values and other potential types.
                        // Note that decimal isn't supported as an attribute value so it is omitted.
                        case "T:System.Byte":
                        case "T:System.Double":
                        case "T:System.SByte":
                        case "T:System.Int16":
                        case "T:System.Int64":
                        case "T:System.Int32":
                        case "T:System.UInt16":
                        case "T:System.UInt32":
                        case "T:System.UInt64":
                            writer.WriteString(text);
                            break;

                        case "T:System.Single":
                            writer.WriteString(text);
                            writer.WriteString("f");
                            break;

                        default:
                            // If not a recognized type, just write out the value so that something shows
                            if(type.LocalName != "arrayOf")
                                writer.WriteString(text);
                            else
                            {
                                // It's an array.  We don't get the values so just write out the type.
                                writer.WriteString("new ");
                                this.WriteTypeReference(type, writer);
                                writer.WriteString(" { ... }");
                            }
                            break;
                    }
                    break;
            }
        }

        // Interfaces

        private void WriteImplementedInterfaces(XPathNavigator reflection, SyntaxWriter writer)
        {

            XPathNodeIterator implements = reflection.Select(apiImplementedInterfacesExpression);

            if(implements.Count == 0)
                return;
            writer.WriteString(" : ");
            while(implements.MoveNext())
            {
                XPathNavigator implement = implements.Current;
                WriteTypeReference(implement, writer);
                if(implements.CurrentPosition < implements.Count)
                {
                    writer.WriteString(", ");
                    if(writer.Position > maxPosition)
                    {
                        writer.WriteLine();
                        writer.WriteString("\t");
                    }
                }
            }

        }

        private void WriteBaseClassAndImplementedInterfaces(XPathNavigator reflection, SyntaxWriter writer)
        {

            XPathNavigator baseClass = reflection.SelectSingleNode(apiBaseClassExpression);
            XPathNodeIterator implements = reflection.Select(apiImplementedInterfacesExpression);

            bool hasBaseClass = (baseClass != null) && !((bool)baseClass.Evaluate(typeIsObjectExpression));
            bool hasImplementedInterfaces = (implements.Count > 0);

            if(hasBaseClass || hasImplementedInterfaces)
            {

                writer.WriteString(" : ");
                if(hasBaseClass)
                {
                    WriteTypeReference(baseClass, writer);
                    if(hasImplementedInterfaces)
                    {
                        writer.WriteString(", ");
                        if(writer.Position > maxPosition)
                        {
                            writer.WriteLine();
                            writer.WriteString("\t");
                        }
                    }
                }

                while(implements.MoveNext())
                {
                    XPathNavigator implement = implements.Current;
                    WriteTypeReference(implement, writer);
                    if(implements.CurrentPosition < implements.Count)
                    {
                        writer.WriteString(", ");
                        if(writer.Position > maxPosition)
                        {
                            writer.WriteLine();
                            writer.WriteString("\t");
                        }
                    }
                }

            }

        }

        // Generics

        private static void WriteGenericTemplates(XPathNavigator reflection, SyntaxWriter writer, bool writeVariance)
        {
            XPathNodeIterator templates = (XPathNodeIterator)reflection.Evaluate(apiTemplatesExpression);

            if(templates.Count == 0)
                return;
            writer.WriteString("<");
            while(templates.MoveNext())
            {
                XPathNavigator template = templates.Current;
                if(writeVariance)
                {
                    bool contravariant = (bool)template.Evaluate(templateIsContravariantExpression);
                    bool covariant = (bool)template.Evaluate(templateIsCovariantExpression);

                    if(contravariant)
                    {
                        writer.WriteKeyword("in");
                        writer.WriteString(" ");
                    }
                    if(covariant)
                    {
                        writer.WriteKeyword("out");
                        writer.WriteString(" ");
                    }
                }
                string name = template.GetAttribute("name", String.Empty);
                writer.WriteString(name);
                if(templates.CurrentPosition < templates.Count)
                    writer.WriteString(", ");
            }
            writer.WriteString(">");

        }

        private void WriteGenericTemplateConstraints(XPathNavigator reflection, SyntaxWriter writer)
        {

            XPathNodeIterator templates = reflection.Select(apiTemplatesExpression);

            if(templates.Count == 0)
                return;

            writer.WriteLine();
            foreach(XPathNavigator template in templates)
            {

                bool constrained = (bool)template.Evaluate(templateIsConstrainedExpression);
                if(constrained)
                {
                    string name = (string)template.Evaluate(templateNameExpression);

                    writer.WriteKeyword("where");
                    writer.WriteString(" ");
                    writer.WriteString(name);
                    writer.WriteString(" : ");
                }
                else
                {
                    continue;
                }

                bool value = (bool)template.Evaluate(templateIsValueTypeExpression);
                bool reference = (bool)template.Evaluate(templateIsReferenceTypeExpression);
                bool constructor = (bool)template.Evaluate(templateIsConstructableExpression);
                XPathNodeIterator constraints = template.Select(templateConstraintsExpression);

                // keep track of whether there is a previous constraint, so we know whether to put a comma
                bool previous = false;

                if(value)
                {
                    if(previous)
                        writer.WriteString(", ");
                    writer.WriteKeyword("struct");
                    previous = true;
                }

                if(reference)
                {
                    if(previous)
                        writer.WriteString(", ");
                    writer.WriteKeyword("class");
                    previous = true;
                }

                if(constructor)
                {
                    if(previous)
                        writer.WriteString(", ");
                    writer.WriteKeyword("new");
                    writer.WriteString("()");
                    previous = true;
                }

                foreach(XPathNavigator constraint in constraints)
                {
                    if(previous)
                        writer.WriteString(", ");
                    WriteTypeReference(constraint, writer);
                    previous = true;
                }
                writer.WriteLine();
            }

        }

        // Parameters

        private void WriteMethodParameters(XPathNavigator reflection, SyntaxWriter writer)
        {

            XPathNodeIterator parameters = reflection.Select(apiParametersExpression);

            writer.WriteString("(");
            if(parameters.Count > 0)
            {
                writer.WriteLine();
                WriteParameters(parameters, reflection, writer);
            }
            writer.WriteString(")");

        }

        private void WritePropertyParameters(XPathNavigator reflection, SyntaxWriter writer)
        {

            XPathNodeIterator parameters = reflection.Select(apiParametersExpression);

            if(parameters.Count == 0)
                return;

            writer.WriteString("[");
            writer.WriteLine();
            WriteParameters(parameters, reflection, writer);
            writer.WriteString("]");

        }


        private void WriteParameters(XPathNodeIterator parameters, XPathNavigator reflection, SyntaxWriter writer)
        {
            bool isExtension = (bool)reflection.Evaluate(apiIsExtensionMethod);

            while(parameters.MoveNext())
            {
                XPathNavigator parameter = parameters.Current;

                // !EFW - Added support for optional parameter values
                XPathNavigator argument = (XPathNavigator)parameter.SelectSingleNode(parameterArgumentExpression);
                bool isOptional = (bool)parameter.Evaluate(parameterIsOptionalExpression);

                string name = (string)parameter.Evaluate(parameterNameExpression);
                XPathNavigator type = parameter.SelectSingleNode(parameterTypeExpression);
                bool isOut = (bool)parameter.Evaluate(parameterIsOutExpression);
                bool isRef = (bool)parameter.Evaluate(parameterIsRefExpression);
                bool isParamArray = (bool)parameter.Evaluate(parameterIsParamArrayExpression);

                writer.WriteString("\t");

                if(isExtension && parameters.CurrentPosition == 1)
                {
                    writer.WriteKeyword("this");
                    writer.WriteString(" ");
                }

                // !EFW - Optional indicated by OptionalAttribute?
                if(isOptional && argument == null)
                {
                    WriteAttribute("T:System.Runtime.InteropServices.OptionalAttribute", false, writer);
                    writer.WriteString(" ");
                }

                if(isRef)
                {
                    if(isOut)
                    {
                        writer.WriteKeyword("out");
                    }
                    else
                    {
                        writer.WriteKeyword("ref");
                    }
                    writer.WriteString(" ");
                }

                if(isParamArray)
                {
                    writer.WriteKeyword("params");
                    writer.WriteString(" ");
                }

                WriteTypeReference(type, writer);
                writer.WriteString(" ");
                writer.WriteParameter(name);

                // !EFW - Write optional value if present
                if(argument != null)
                {
                    writer.WriteString(" = ");
                    this.WriteValue(argument, writer);
                }

                if(parameters.CurrentPosition < parameters.Count)
                    writer.WriteString(",");

                writer.WriteLine();
            }
        }

        // Return Value

        private void WriteReturnValue(XPathNavigator reflection, SyntaxWriter writer)
        {
            XPathNavigator type = reflection.SelectSingleNode(apiReturnTypeExpression);

            if(type == null)
            {
                writer.WriteKeyword("void");
            }
            else
            {
                WriteTypeReference(type, writer);
            }
        }

        // References

        private void WriteTypeReference(XPathNavigator reference, SyntaxWriter writer)
        {
            switch(reference.LocalName)
            {
                case "arrayOf":
                    int rank = Convert.ToInt32(reference.GetAttribute("rank", String.Empty),
                        CultureInfo.InvariantCulture);

                    XPathNavigator element = reference.SelectSingleNode(typeExpression);
                    WriteTypeReference(element, writer);
                    writer.WriteString("[");

                    for(int i = 1; i < rank; i++)
                        writer.WriteString(",");

                    writer.WriteString("]");
                    break;

                case "pointerTo":
                    XPathNavigator pointee = reference.SelectSingleNode(typeExpression);
                    WriteTypeReference(pointee, writer);
                    writer.WriteString("*");
                    break;
                case "referenceTo":
                    XPathNavigator referee = reference.SelectSingleNode(typeExpression);
                    WriteTypeReference(referee, writer);
                    break;
                case "type":
                    string id = reference.GetAttribute("api", String.Empty);
                    WriteNormalTypeReference(id, writer);
                    XPathNodeIterator typeModifiers = reference.Select(typeModifiersExpression);
                    while(typeModifiers.MoveNext())
                    {
                        WriteTypeReference(typeModifiers.Current, writer);
                    }
                    break;
                case "template":
                    string name = reference.GetAttribute("name", String.Empty);
                    writer.WriteString(name);
                    XPathNodeIterator modifiers = reference.Select(typeModifiersExpression);
                    while(modifiers.MoveNext())
                    {
                        WriteTypeReference(modifiers.Current, writer);
                    }
                    break;
                case "specialization":
                    writer.WriteString("<");
                    XPathNodeIterator arguments = reference.Select(specializationArgumentsExpression);
                    while(arguments.MoveNext())
                    {
                        if(arguments.CurrentPosition > 1)
                            writer.WriteString(", ");
                        WriteTypeReference(arguments.Current, writer);
                    }
                    writer.WriteString(">");
                    break;
            }
        }

        private static void WriteNormalTypeReference(string api, SyntaxWriter writer)
        {
            switch(api)
            {
                case "T:System.Void":
                    writer.WriteReferenceLink(api, "void");
                    break;
                case "T:System.String":
                    writer.WriteReferenceLink(api, "string");
                    break;
                case "T:System.Boolean":
                    writer.WriteReferenceLink(api, "bool");
                    break;
                case "T:System.Byte":
                    writer.WriteReferenceLink(api, "byte");
                    break;
                case "T:System.SByte":
                    writer.WriteReferenceLink(api, "sbyte");
                    break;
                case "T:System.Char":
                    writer.WriteReferenceLink(api, "char");
                    break;
                case "T:System.Int16":
                    writer.WriteReferenceLink(api, "short");
                    break;
                case "T:System.Int32":
                    writer.WriteReferenceLink(api, "int");
                    break;
                case "T:System.Int64":
                    writer.WriteReferenceLink(api, "long");
                    break;
                case "T:System.UInt16":
                    writer.WriteReferenceLink(api, "ushort");
                    break;
                case "T:System.UInt32":
                    writer.WriteReferenceLink(api, "uint");
                    break;
                case "T:System.UInt64":
                    writer.WriteReferenceLink(api, "ulong");
                    break;
                case "T:System.Single":
                    writer.WriteReferenceLink(api, "float");
                    break;
                case "T:System.Double":
                    writer.WriteReferenceLink(api, "double");
                    break;
                case "T:System.Decimal":
                    writer.WriteReferenceLink(api, "decimal");
                    break;
                default:
                    writer.WriteReferenceLink(api);
                    break;
            }
        }

        private static void WriteMemberReference(XPathNavigator member, SyntaxWriter writer)
        {
            string api = member.GetAttribute("api", String.Empty);
            writer.WriteReferenceLink(api);
        }
    }
}
