// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 01/30/2012 - EFW - Fixed WriteValue() so that it outputs numeric attribute values.
// 02/09/2012 - EFW - Added support for optional parameters and property getter/setter attributes.
// 02/14/2012 - EFW - Made the unsafe code checks consistent across all syntax generators

using System;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools
{
    public class VisualBasicDeclarationSyntaxGenerator : SyntaxGeneratorTemplate
    {
        public VisualBasicDeclarationSyntaxGenerator(XPathNavigator configuration) : base(configuration)
        {
            if(String.IsNullOrEmpty(Language))
                Language = "VisualBasic";
        }

        // namespace: done
        public override void WriteNamespaceSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string name = (string)reflection.Evaluate(apiNameExpression);

            writer.WriteKeyword("Namespace");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
        }

        // class: done
        public override void WriteClassSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isAbstract = (bool)reflection.Evaluate(apiIsAbstractTypeExpression);
            bool isSealed = (bool)reflection.Evaluate(apiIsSealedTypeExpression);
            bool isSerializable = (bool)reflection.Evaluate(apiIsSerializableTypeExpression);

            if(isSerializable)
                WriteAttribute("T:System.SerializableAttribute", writer);
            WriteAttributes(reflection, writer);
            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            if(isAbstract)
            {
                if(isSealed)
                {
                    // static -- VB doesn't really handle this case
                    writer.WriteKeyword("NotInheritable");
                    writer.WriteString(" ");
                }
                else
                {
                    writer.WriteKeyword("MustInherit");
                    writer.WriteString(" ");
                }
            }
            else if(isSealed)
            {
                writer.WriteKeyword("NotInheritable");
                writer.WriteString(" ");
            }
            writer.WriteKeyword("Class");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            WriteGenericTemplates(reflection, writer);
            WriteBaseClass(reflection, writer);
            WriteImplementedInterfaces(reflection, writer);
        }

        // structure: add base type
        public override void WriteStructureSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isSerializable = (bool)reflection.Evaluate(apiIsSerializableTypeExpression);

            if(isSerializable)
                WriteAttribute("T:System.SerializableAttribute", writer);
            WriteAttributes(reflection, writer);
            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            writer.WriteKeyword("Structure");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            WriteGenericTemplates(reflection, writer);
            WriteImplementedInterfaces(reflection, writer);
        }

        // interface: done
        public override void WriteInterfaceSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            string name = (string)reflection.Evaluate(apiNameExpression);

            WriteAttributes(reflection, writer);
            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            writer.WriteKeyword("Interface");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            WriteGenericTemplates(reflection, writer, true); // Need to write variance info for interfaces and delegates
            WriteImplementedInterfaces(reflection, writer);
        }

        // delegate: done
        public override void WriteDelegateSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(IsUnsupportedUnsafe(reflection, writer))
                return;

            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isSerializable = (bool)reflection.Evaluate(apiIsSerializableTypeExpression);
            XPathNavigator type = reflection.SelectSingleNode(apiReturnTypeExpression);

            if(isSerializable)
                WriteAttribute("T:System.SerializableAttribute", writer);
            WriteAttributes(reflection, writer);
            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            writer.WriteKeyword("Delegate");
            writer.WriteString(" ");
            if(type == null)
            {
                writer.WriteKeyword("Sub");
            }
            else
            {
                writer.WriteKeyword("Function");
            }
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            WriteGenericTemplates(reflection, writer, true); // Need to write variance info for interfaces and delegates
            WriteParameters(reflection, writer);
            if(type != null)
            {
                writer.WriteString(" ");
                writer.WriteKeyword("As");
                writer.WriteString(" ");
                WriteTypeReference(type, writer);
            }

        }

        // enumeration: done
        public override void WriteEnumerationSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isSerializable = (bool)reflection.Evaluate(apiIsSerializableTypeExpression);

            if(isSerializable)
                WriteAttribute("T:System.SerializableAttribute", writer);
            WriteAttributes(reflection, writer);
            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            writer.WriteKeyword("Enumeration");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
        }

        // constructor: done
        public override void WriteConstructorSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(IsUnsupportedUnsafe(reflection, writer))
                return;

            bool isStatic = (bool)reflection.Evaluate(apiIsStaticExpression);

            WriteAttributes(reflection, writer);
            if(isStatic)
            {
                writer.WriteKeyword("Shared");
            }
            else
            {
                WriteVisibility(reflection, writer);
            }
            writer.WriteString(" ");
            writer.WriteKeyword("Sub");
            writer.WriteString(" ");
            writer.WriteIdentifier("New");
            WriteParameters(reflection, writer);

        }


        public override void WriteNormalMethodSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            // !EFW - Added unsafe check
            if(IsUnsupportedUnsafe(reflection, writer))
                return;

            if(IsUnsupportedVarargs(reflection, writer))
                return;

            string name = (string)reflection.Evaluate(apiNameExpression);
            XPathNavigator type = reflection.SelectSingleNode(apiReturnTypeExpression);
            bool isExplicit = (bool)reflection.Evaluate(apiIsExplicitImplementationExpression);

            WriteAttributes(reflection, writer);
            WriteProcedureModifiers(reflection, writer);
            if(type == null)
            {
                writer.WriteKeyword("Sub");
            }
            else
            {
                writer.WriteKeyword("Function");
            }
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            WriteGenericTemplates(reflection, writer);
            WriteParameters(reflection, writer);
            if(type != null)
            {
                writer.WriteString(" ");
                writer.WriteKeyword("As");
                writer.WriteString(" ");
                WriteTypeReference(type, writer);
            }
            if(isExplicit)
            {
                if(writer.Position > maxPosition)
                {
                    writer.WriteLine();
                    writer.WriteString("\t");
                }
                else
                {
                    writer.WriteString(" ");
                }
                writer.WriteKeyword("Implements");
                writer.WriteString(" ");
                XPathNodeIterator implementations = reflection.Select(apiImplementedMembersExpression);
                while(implementations.MoveNext())
                {
                    XPathNavigator implementation = implementations.Current;
                    XPathNavigator contract = implementation.SelectSingleNode(attributeTypeExpression);
                    // string id = implementation.GetAttribute("api", String.Empty);
                    if(implementations.CurrentPosition > 1)
                        writer.WriteString(", ");
                    WriteTypeReference(contract, writer);
                    writer.WriteString(".");
                    WriteMemberReference(implementation, writer);
                }
            }

        }

        public override void WriteOperatorSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            string name = (string)reflection.Evaluate(apiNameExpression);
            XPathNavigator type = reflection.SelectSingleNode(apiReturnTypeExpression);

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
                        identifier = "Not";
                        break;
                    case "True":
                        identifier = "IsTrue";
                        break;
                    case "False":
                        identifier = "IsFalse";
                        break;
                    // binary comparison operators
                    case "Equality":
                        identifier = "=";
                        break;
                    case "Inequality":
                        identifier = "<>";
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
                    case "Exponent":
                        identifier = "^";
                        break;
                    case "Modulus":
                        identifier = "Mod";
                        break;
                    case "IntegerDivision":
                        identifier = @"\";
                        break;
                    // binary logical operators
                    case "BitwiseAnd":
                        identifier = "And";
                        break;
                    case "BitwiseOr":
                        identifier = "Or";
                        break;
                    case "ExclusiveOr":
                        identifier = "Xor";
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
                    // concatenation
                    case "Concatenate":
                        identifier = "&";
                        break;
                    // casting operators
                    case "Implicit":
                    case "Explicit":
                        identifier = "CType";
                        break;
                    case "Assign":
                        identifier = "=";
                        break;
                    // didn't recognize an operator
                    default:
                        identifier = null;
                        break;
                }
            }
            if(identifier == null)
            {
                writer.WriteMessage("UnsupportedOperator_" + Language);
                return;
            }
            WriteProcedureModifiers(reflection, writer);
            if(name == "Implicit")
            {
                writer.WriteKeyword("Widening");
                writer.WriteString(" ");
            }
            else if(name == "Explicit")
            {
                writer.WriteKeyword("Narrowing");
                writer.WriteString(" ");
            }
            writer.WriteKeyword("Operator");
            writer.WriteString(" ");
            writer.WriteIdentifier(identifier);
            WriteParameters(reflection, writer);
            writer.WriteString(" ");
            writer.WriteKeyword("As");
            writer.WriteString(" ");
            WriteTypeReference(type, writer);
        }

        public override void WriteCastSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            WriteOperatorSyntax(reflection, writer);
        }

        public override void WritePropertySyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(IsUnsupportedUnsafe(reflection, writer))
                return;

            string name = (string)reflection.Evaluate(apiNameExpression);
            bool hasGetter = (bool)reflection.Evaluate(apiIsReadPropertyExpression);
            bool hasSetter = (bool)reflection.Evaluate(apiIsWritePropertyExpression);
            bool isDefault = (bool)reflection.Evaluate(apiIsDefaultMemberExpression);
            bool isExplicit = (bool)reflection.Evaluate(apiIsExplicitImplementationExpression);
            XPathNavigator type = reflection.SelectSingleNode(apiReturnTypeExpression);

            WriteAttributes(reflection, writer);
            WriteProcedureModifiers(reflection, writer);

            if(hasGetter && !hasSetter)
            {
                writer.WriteKeyword("ReadOnly");
                writer.WriteString(" ");
            }
            else if(hasSetter && !hasGetter)
            {
                writer.WriteKeyword("WriteOnly");
                writer.WriteString(" ");
            }

            if(isDefault)
            {
                writer.WriteKeyword("Default");
                writer.WriteString(" ");
            }

            writer.WriteKeyword("Property");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            WriteParameters(reflection, writer);
            writer.WriteString(" ");
            writer.WriteKeyword("As");
            writer.WriteString(" ");
            WriteTypeReference(type, writer);

            if(isExplicit)
            {
                if(writer.Position > maxPosition)
                {
                    writer.WriteLine();
                    writer.WriteString("\t");
                }
                else
                {
                    writer.WriteString(" ");
                }
                writer.WriteKeyword("Implements");
                writer.WriteString(" ");
                XPathNodeIterator implementations = reflection.Select(apiImplementedMembersExpression);
                while(implementations.MoveNext())
                {
                    XPathNavigator implementation = implementations.Current;
                    XPathNavigator contract = implementation.SelectSingleNode(memberDeclaringTypeExpression);
                    //string id = implementation.GetAttribute("api", String.Empty);
                    if(implementations.CurrentPosition > 1)
                        writer.WriteString(", ");
                    WriteTypeReference(contract, writer);
                    writer.WriteString(".");
                    WriteMemberReference(implementation, writer);
                    //writer.WriteReferenceLink(id);	
                }
            }

            if(hasGetter)
            {
                writer.WriteLine();

                // !EFW - Added support for getter/setter attributes
                XPathNavigator getter = reflection.SelectSingleNode(apiGetterExpression);

                if(getter.HasChildren)
                {
                    writer.WriteString("\t");
                    this.WriteAttributes(getter, writer, "\t");
                }

                writer.WriteString("\t");

                string getVisibility = (string)reflection.Evaluate(apiGetVisibilityExpression);

                if(!String.IsNullOrEmpty(getVisibility))
                {
                    WriteVisibility(getVisibility, writer);
                    writer.WriteString(" ");
                }

                writer.WriteKeyword("Get");
            }

            if(hasSetter)
            {
                writer.WriteLine();

                // !EFW - Added support for getter/setter attributes
                XPathNavigator setter = reflection.SelectSingleNode(apiSetterExpression);

                if(setter.HasChildren)
                {
                    writer.WriteString("\t");
                    this.WriteAttributes(setter, writer, "\t");
                }

                writer.WriteString("\t");

                string setVisibility = (string)reflection.Evaluate(apiSetVisibilityExpression);

                if(!String.IsNullOrEmpty(setVisibility))
                {
                    WriteVisibility(setVisibility, writer);
                    writer.WriteString(" ");
                }

                writer.WriteKeyword("Set");
            }
        }

        public override void WriteEventSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            // !EFW - Added unsafe check
            if(IsUnsupportedUnsafe(reflection, writer))
                return;

            string name = (string)reflection.Evaluate(apiNameExpression);
            XPathNavigator handler = reflection.SelectSingleNode(apiHandlerOfEventExpression);

            WriteAttributes(reflection, writer);
            WriteProcedureModifiers(reflection, writer);
            writer.WriteString("Event");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            writer.WriteString(" ");
            writer.WriteKeyword("As");
            writer.WriteString(" ");
            WriteTypeReference(handler, writer);
            WriteExplicitImplementations(reflection, writer);
        }

        private void WriteExplicitImplementations(XPathNavigator reflection, SyntaxWriter writer)
        {
            bool isExplicit = (bool)reflection.Evaluate(apiIsExplicitImplementationExpression);
            if(isExplicit)
            {
                if(writer.Position > maxPosition)
                {
                    writer.WriteLine();
                    writer.WriteString("\t");
                }
                else
                {
                    writer.WriteString(" ");
                }
                writer.WriteKeyword("Implements");
                writer.WriteString(" ");
                XPathNodeIterator implementations = reflection.Select(apiImplementedMembersExpression);
                while(implementations.MoveNext())
                {
                    XPathNavigator implementation = implementations.Current;
                    XPathNavigator contract = implementation.SelectSingleNode(memberDeclaringTypeExpression);
                    //string id = implementation.GetAttribute("api", String.Empty);
                    if(implementations.CurrentPosition > 1)
                        writer.WriteString(", ");
                    WriteTypeReference(contract, writer);
                    writer.WriteString(".");
                    WriteMemberReference(implementation, writer);
                    //writer.WriteReferenceLink(id);	
                }
            }
        }

        private void WriteProcedureModifiers(XPathNavigator reflection, SyntaxWriter writer)
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
                writer.WriteKeyword("Shared");
                writer.WriteString(" ");
            }
            else
            {
                if(isVirtual)
                {
                    if(isAbstract)
                    {
                        writer.WriteKeyword("MustOverride");
                        writer.WriteString(" ");
                    }
                    else if(isOverride)
                    {
                        writer.WriteKeyword("Overrides");
                        writer.WriteString(" ");
                        if(isFinal)
                        {
                            writer.WriteKeyword("NotOverridable");
                            writer.WriteString(" ");
                        }
                    }
                    else
                    {
                        if(!isFinal)
                        {
                            writer.WriteKeyword("Overridable");
                            writer.WriteString(" ");
                        }
                    }
                }
            }

        }

        public override void WriteFieldSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(IsUnsupportedUnsafe(reflection, writer))
                return;

            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isStatic = (bool)reflection.Evaluate(apiIsStaticExpression);
            bool isLiteral = (bool)reflection.Evaluate(apiIsLiteralFieldExpression);
            bool isInitOnly = (bool)reflection.Evaluate(apiIsInitOnlyFieldExpression);
            bool isSerialized = (bool)reflection.Evaluate(apiIsSerializedFieldExpression);
            XPathNavigator type = reflection.SelectSingleNode(apiReturnTypeExpression);

            if(!isSerialized)
                WriteAttribute("T:System.NonSerializedAttribute", writer);
            WriteAttributes(reflection, writer);
            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            if(isStatic)
            {
                if(isLiteral)
                {
                    writer.WriteKeyword("Const");
                }
                else
                {
                    writer.WriteKeyword("Shared");
                }
                writer.WriteString(" ");
            }
            if(isInitOnly)
            {
                writer.WriteKeyword("ReadOnly");
                writer.WriteString(" ");
            }
            writer.WriteIdentifier(name);
            writer.WriteString(" ");
            writer.WriteKeyword("As");
            writer.WriteString(" ");
            WriteTypeReference(type, writer);

        }

        // Visibility
        private void WriteVisibility(XPathNavigator reflection, SyntaxWriter writer)
        {

            string visibility = (string)reflection.Evaluate(apiVisibilityExpression);
            WriteVisibility(visibility, writer);
        }

        private void WriteVisibility(string visibility, SyntaxWriter writer)
        {

            switch(visibility)
            {
                case "public":
                    writer.WriteKeyword("Public");
                    break;
                case "family":
                    writer.WriteKeyword("Protected");
                    break;
                case "family or assembly":
                    writer.WriteKeyword("Protected Friend");
                    break;
                case "family and assembly":
                    writer.WriteKeyword("Friend"); // not right, but same outside assembly
                    break;
                case "assembly":
                    writer.WriteKeyword("Friend");
                    break;
                case "private":
                    writer.WriteKeyword("Private");
                    break;
            }

        }

        // Attributes

        private void WriteAttribute(string reference, SyntaxWriter writer)
        {
            WriteAttribute(reference, true, writer);
        }

        private void WriteAttribute(string reference, bool newLine, SyntaxWriter writer)
        {
            writer.WriteString("<");
            writer.WriteReferenceLink(reference);
            writer.WriteString(">");
            if(newLine)
            {
                writer.WriteString(" _");
                writer.WriteLine();
            }
        }

        // !EFW - Added indent parameter for property getter/setter attributes
        private void WriteAttributes(XPathNavigator reflection, SyntaxWriter writer, string indent = null)
        {
            XPathNodeIterator attributes = (XPathNodeIterator)reflection.Evaluate(apiAttributesExpression);

            foreach(XPathNavigator attribute in attributes)
            {
                XPathNavigator type = attribute.SelectSingleNode(attributeTypeExpression);

                // !EFW - Ignore FixedBufferAttribute
                if(type.GetAttribute("api", String.Empty) == "T:System.Runtime.CompilerServices.FixedBufferAttribute")
                    continue;

                XPathNodeIterator arguments = (XPathNodeIterator)attribute.Select(attributeArgumentsExpression);
                XPathNodeIterator assignments = (XPathNodeIterator)attribute.Select(attributeAssignmentsExpression);

                writer.WriteString("<");
                WriteTypeReference(type, writer);

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
                                writer.WriteString(" _");
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
                                writer.WriteString(" _");
                                writer.WriteLine();

                                if(!String.IsNullOrEmpty(indent))
                                    writer.WriteString(indent);

                                writer.WriteString("\t");
                            }
                        }
                        writer.WriteString((string)assignment.Evaluate(assignmentNameExpression));
                        writer.WriteString(" := ");
                        WriteValue(assignment, writer);

                    }
                    writer.WriteString(")");
                }

                writer.WriteString("> ");
                writer.WriteString("_");
                writer.WriteLine();
            }

        }

        private void WriteValue(XPathNavigator parent, SyntaxWriter writer)
        {
            XPathNavigator type = parent.SelectSingleNode(attributeTypeExpression);
            XPathNavigator value = parent.SelectSingleNode(valueExpression);

            if(value == null)
                Console.WriteLine("null value");

            switch(value.LocalName)
            {
                case "nullValue":
                    writer.WriteKeyword("Nothing");
                    break;

                case "typeValue":
                    writer.WriteKeyword("GetType");
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
                        {
                            writer.WriteString(" ");
                            writer.WriteKeyword("Or");
                            writer.WriteString(" ");
                        }
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
                            bool bool_value = Convert.ToBoolean(text);
                            if(bool_value)
                            {
                                writer.WriteKeyword("True");
                            }
                            else
                            {
                                writer.WriteKeyword("False");
                            }
                            break;

                        case "T:System.Char":
                            writer.WriteString("\"");
                            writer.WriteString(text);
                            writer.WriteString("\"C");
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
                            writer.WriteString("F");
                            break;

                        default:
                            // If not a recognized type, just write out the value so that something shows
                            if(type.LocalName != "arrayOf")
                                writer.WriteString(text);
                            else
                            {
                                // It's an array.  We don't get the values so just write out the type.
                                writer.WriteString("New ");
                                this.WriteTypeReference(type, writer);
                                writer.WriteString(" { ... }");
                            }
                            break;
                    }
                    break;
            }
        }

        // Interfaces
        private void WriteBaseClass(XPathNavigator reflection, SyntaxWriter writer)
        {

            XPathNavigator baseClass = reflection.SelectSingleNode(apiBaseClassExpression);

            if((baseClass != null) && !((bool)baseClass.Evaluate(typeIsObjectExpression)))
            {
                writer.WriteString(" _");
                writer.WriteLine();
                writer.WriteString("\t");
                writer.WriteKeyword("Inherits");
                writer.WriteString(" ");
                WriteTypeReference(baseClass, writer);
            }

        }

        private void WriteImplementedInterfaces(XPathNavigator reflection, SyntaxWriter writer)
        {

            XPathNodeIterator implements = reflection.Select(apiImplementedInterfacesExpression);

            if(implements.Count == 0)
                return;

            writer.WriteString(" _");
            writer.WriteLine();
            writer.WriteString("\t");
            string subgroup = (string)reflection.Evaluate(apiSubgroupExpression);
            if(subgroup == "interface")
                writer.WriteKeyword("Inherits");
            else
                writer.WriteKeyword("Implements");
            writer.WriteString(" ");
            while(implements.MoveNext())
            {
                XPathNavigator implement = implements.Current;
                if(implements.CurrentPosition > 1)
                {
                    writer.WriteString(", ");
                    if(writer.Position > maxPosition)
                    {
                        writer.WriteString(" _");
                        writer.WriteLine();
                        writer.WriteString("\t");
                    }
                }
                WriteTypeReference(implement, writer);
            }

        }

        // Generics

        private void WriteGenericTemplates(XPathNavigator reflection, SyntaxWriter writer)
        {
            WriteGenericTemplates(reflection, writer, false);
        }

        private void WriteGenericTemplates(XPathNavigator reflection, SyntaxWriter writer, bool writeVariance)
        {

            XPathNodeIterator templates = reflection.Select(apiTemplatesExpression);

            if(templates.Count == 0)
                return;
            writer.WriteString("(");
            writer.WriteKeyword("Of");
            writer.WriteString(" ");
            while(templates.MoveNext())
            {
                XPathNavigator template = templates.Current;
                if(templates.CurrentPosition > 1)
                    writer.WriteString(", ");

                if(writeVariance)
                {
                    bool contravariant = (bool)template.Evaluate(templateIsContravariantExpression);
                    bool covariant = (bool)template.Evaluate(templateIsCovariantExpression);

                    if(contravariant)
                    {
                        writer.WriteKeyword("In");
                        writer.WriteString(" ");
                    }
                    if(covariant)
                    {
                        writer.WriteKeyword("Out");
                        writer.WriteString(" ");
                    }
                }

                string name = template.GetAttribute("name", String.Empty);
                writer.WriteString(name);

                // write out constraints

                bool constrained = (bool)template.Evaluate(templateIsConstrainedExpression);
                if(constrained)
                {
                    writer.WriteString(" ");
                    writer.WriteKeyword("As");
                    writer.WriteString(" ");

                    bool value = (bool)template.Evaluate(templateIsValueTypeExpression);
                    bool reference = (bool)template.Evaluate(templateIsReferenceTypeExpression);
                    bool constructor = (bool)template.Evaluate(templateIsConstructableExpression);
                    XPathNodeIterator constraints = template.Select(templateConstraintsExpression);

                    int count = constraints.Count;
                    if(value)
                        count++;
                    if(reference)
                        count++;
                    if(constructor)
                        count++;

                    if(count > 1)
                        writer.WriteString("{");

                    int index = 0;
                    if(value)
                    {
                        if(index > 0)
                            writer.WriteString(", ");
                        writer.WriteKeyword("Structure");
                        index++;
                    }
                    if(reference)
                    {
                        if(index > 0)
                            writer.WriteString(", ");
                        writer.WriteKeyword("Class");
                        index++;
                    }
                    if(constructor)
                    {
                        if(index > 0)
                            writer.WriteString(", ");
                        writer.WriteKeyword("New");
                        index++;
                    }
                    foreach(XPathNavigator constraint in constraints)
                    {
                        if(index > 0)
                            writer.WriteString(", ");
                        WriteTypeReference(constraint, writer);
                        index++;
                    }

                    if(count > 1)
                        writer.WriteString("}");

                }

            }
            writer.WriteString(")");
        }

        // Parameters

        private void WriteParameters(XPathNavigator reflection, SyntaxWriter writer)
        {
            XPathNodeIterator parameters = reflection.Select(apiParametersExpression);

            if(parameters.Count == 0)
                return;

            writer.WriteString(" ( _");
            writer.WriteLine();

            while(parameters.MoveNext())
            {
                XPathNavigator parameter = parameters.Current;

                // !EFW - Added support for optional parameter values
                XPathNavigator argument = (XPathNavigator)parameter.SelectSingleNode(parameterArgumentExpression);
                bool isOptional = (bool)parameter.Evaluate(parameterIsOptionalExpression);

                XPathNavigator type = parameter.SelectSingleNode(parameterTypeExpression);
                string name = (string)parameter.Evaluate(parameterNameExpression);
                bool isIn = (bool)parameter.Evaluate(parameterIsInExpression);
                bool isOut = (bool)parameter.Evaluate(parameterIsOutExpression);
                bool isParamArray = (bool)parameter.Evaluate(parameterIsParamArrayExpression);
                bool isByRef = (bool)parameter.Evaluate(parameterIsRefExpression);

                writer.WriteString("\t");

                // !EFW - Optional indicated by OptionalAttribute?
                if(isOptional && argument == null)
                {
                    this.WriteAttribute("T:System.Runtime.InteropServices.OptionalAttribute", false, writer);
                    writer.WriteString(" ");
                }

                if(isOut)
                {
                    WriteAttribute("T:System.Runtime.InteropServices.OutAttribute", false, writer);
                    writer.WriteString(" ");
                }

                // !EFW - Write optional value if present
                if(argument != null)
                    writer.WriteString("Optional ");

                if(isParamArray)
                {
                    writer.WriteKeyword("ParamArray");
                    writer.WriteString(" ");
                }

                if(isByRef)
                {
                    writer.WriteKeyword("ByRef");
                    writer.WriteString(" ");
                }

                writer.WriteParameter(name);
                writer.WriteString(" ");
                writer.WriteKeyword("As");
                writer.WriteString(" ");
                WriteTypeReference(type, writer);

                // !EFW - Write optional value if present
                if(argument != null)
                {
                    writer.WriteString(" = ");
                    this.WriteValue(argument, writer);
                }

                if(parameters.CurrentPosition < parameters.Count)
                    writer.WriteString(",");

                writer.WriteString(" _");
                writer.WriteLine();
            }

            writer.WriteString(")");
        }

        // References

        private void WriteTypeReference(XPathNavigator reference, SyntaxWriter writer)
        {
            switch(reference.LocalName)
            {
                case "arrayOf":
                    int rank = Convert.ToInt32(reference.GetAttribute("rank", String.Empty));
                    XPathNavigator element = reference.SelectSingleNode(typeExpression);
                    WriteTypeReference(element, writer);
                    writer.WriteString("(");
                    for(int i = 1; i < rank; i++) { writer.WriteString(","); }
                    writer.WriteString(")");
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
                    writer.WriteString("(");
                    writer.WriteKeyword("Of");
                    writer.WriteString(" ");
                    XPathNodeIterator arguments = reference.Select(specializationArgumentsExpression);
                    while(arguments.MoveNext())
                    {
                        if(arguments.CurrentPosition > 1)
                            writer.WriteString(", ");
                        WriteTypeReference(arguments.Current, writer);
                    }
                    writer.WriteString(")");
                    break;
            }
        }

        private void WriteNormalTypeReference(string reference, SyntaxWriter writer)
        {
            switch(reference)
            {
                case "T:System.Int16":
                    writer.WriteReferenceLink(reference, "Short");
                    break;
                case "T:System.Int32":
                    writer.WriteReferenceLink(reference, "Integer");
                    break;
                case "T:System.Int64":
                    writer.WriteReferenceLink(reference, "Long");
                    break;
                case "T:System.UInt16":
                    writer.WriteReferenceLink(reference, "UShort");
                    break;
                case "T:System.UInt32":
                    writer.WriteReferenceLink(reference, "UInteger");
                    break;
                case "T:System.UInt64":
                    writer.WriteReferenceLink(reference, "ULong");
                    break;
                default:
                    writer.WriteReferenceLink(reference);
                    break;
            }
        }

        private void WriteMemberReference(XPathNavigator member, SyntaxWriter writer)
        {
            string api = member.GetAttribute("api", String.Empty);
            writer.WriteReferenceLink(api);
        }

    }

}
