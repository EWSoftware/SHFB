// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 01/30/2012 - EFW - Fixed WriteValue() so that it outputs numeric attribute values.
// 02/09/2012 - EFW - Added support for optional parameters and property getter/setter attributes.
// 02/14/2012 - EFW - Added support for fixed keyword
// 03/30/2012 - EFW - Added support for interior_ptr<T>
// 11/29/2013 - EFW - Added support for metadata based interop attributes
// 12/20/2013 - EFW - Updated the syntax generator to be discoverable via MEF
// 08/01/2014 - EFW - Added support for resource item files containing the localized titles, messages, etc.
// 11/20/2014 - EFW - Added support for writing out method parameter attributes

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.SyntaxGenerator;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This class generates declaration syntax sections for Managed C++
    /// </summary>
    public sealed class ManagedCPlusPlusDeclarationSyntaxGenerator : SyntaxGeneratorTemplate
    {
        #region Syntax generator factory for MEF
        //=====================================================================

        private const string LanguageName = "ManagedCPlusPlus", StyleIdName = "cpp";

        /// <summary>
        /// This is used to create a new instance of the syntax generator
        /// </summary>
        [SyntaxGeneratorExport("Managed C++", LanguageName, StyleIdName,
          AlternateIds = "CPlusPlus, ManagedCPlusPlus, cpp, cpp#, c++, kblangcpp", SortOrder = 40,
          Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
          Description = "Generates Managed C++ declaration syntax sections")]
        public sealed class Factory : ISyntaxGeneratorFactory
        {
            /// <inheritdoc />
            public string ResourceItemFileLocation
            {
                get
                {
                    return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SyntaxContent");
                }
            }

            /// <inheritdoc />
            public SyntaxGeneratorCore Create()
            {
                return new ManagedCPlusPlusDeclarationSyntaxGenerator { Language = LanguageName, StyleId = StyleIdName };
            }
        }
        #endregion

        // namespace: done

        /// <inheritdoc />
        public override void WriteNamespaceSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string name = reflection.Evaluate(apiNameExpression).ToString();

            writer.WriteKeyword("namespace");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
        }

        /// <inheritdoc />
        public override void WriteClassSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string name = reflection.Evaluate(apiNameExpression).ToString();
            bool isAbstract = (bool)reflection.Evaluate(apiIsAbstractTypeExpression);
            bool isSealed = (bool)reflection.Evaluate(apiIsSealedTypeExpression);
            bool isSerializable = (bool)reflection.Evaluate(apiIsSerializableTypeExpression);

            if(isSerializable)
                WriteAttribute("T:System.SerializableAttribute", true, writer);

            WriteAttributes(reflection, writer);

            WriteGenericTemplates(reflection, writer);

            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            writer.WriteKeyword("ref class");
            writer.WriteString(" ");

            writer.WriteIdentifier(name);

            if(isAbstract)
            {
                writer.WriteString(" ");
                writer.WriteKeyword("abstract");
            }
            if(isSealed)
            {
                writer.WriteString(" ");
                writer.WriteKeyword("sealed");
            }

            WriteBaseClassAndImplementedInterfaces(reflection, writer);

        }

        // structure: add base type declaration

        /// <inheritdoc />
        public override void WriteStructureSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isSerializable = (bool)reflection.Evaluate(apiIsSerializableTypeExpression);

            if(isSerializable)
                WriteAttribute("T:System.SerializableAttribute", true, writer);
            WriteAttributes(reflection, writer);

            WriteGenericTemplates(reflection, writer);

            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            writer.WriteKeyword("value class");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            WriteImplementedInterfaces(reflection, writer);

        }

        /// <inheritdoc />
        public override void WriteInterfaceSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            string name = (string)reflection.Evaluate(apiNameExpression);

            WriteAttributes(reflection, writer);

            WriteGenericTemplates(reflection, writer);

            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            writer.WriteKeyword("interface class");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            WriteImplementedInterfaces(reflection, writer);

        }

        // delegate: done

        /// <inheritdoc />
        public override void WriteDelegateSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isSerializable = (bool)reflection.Evaluate(apiIsSerializableTypeExpression);

            if(isSerializable)
                WriteAttribute("T:System.SerializableAttribute", true, writer);
            WriteAttributes(reflection, writer);

            WriteGenericTemplates(reflection, writer);

            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            writer.WriteKeyword("delegate");
            writer.WriteString(" ");
            WriteReturnValue(reflection, writer);
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            WriteMethodParameters(reflection, writer);

        }

        /// <inheritdoc />
        public override void WriteEnumerationSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isSerializable = (bool)reflection.Evaluate(apiIsSerializableTypeExpression);

            if(isSerializable)
                WriteAttribute("T:System.SerializableAttribute", true, writer);
            WriteAttributes(reflection, writer);
            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            writer.WriteKeyword("enum class");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);

            // *** ENUM BASE ***
        }

        /// <inheritdoc />
        public override void WriteConstructorSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            string name = (string)reflection.Evaluate(apiContainingTypeNameExpression);
            bool isStatic = (bool)reflection.Evaluate(apiIsStaticExpression);

            // !EFW - Write visibility before attributes
            WriteVisibility(reflection, writer);
            writer.WriteString(":");
            writer.WriteLine();

            WriteAttributes(reflection, writer);

            if(isStatic)
            {
                writer.WriteKeyword("static");
                writer.WriteString(" ");
            }

            writer.WriteIdentifier(name);

            WriteMethodParameters(reflection, writer);

        }

        /// <inheritdoc />
        public override void WriteNormalMethodSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string name = (string)reflection.Evaluate(apiNameExpression);
            string typeSubgroup = (string)reflection.Evaluate(apiContainingTypeSubgroupExpression);

            if(typeSubgroup == "interface")
            {
                WriteAttributes(reflection, writer);
                WriteGenericTemplates(reflection, writer);
                WriteReturnValue(reflection, writer);
                writer.WriteString(" ");
                writer.WriteIdentifier(name);
                WriteMethodParameters(reflection, writer);
            }
            else
            {
                // EFW - Write visibility before attributes
                WriteProcedureVisibility(reflection, writer);

                WriteAttributes(reflection, writer);
                WriteGenericTemplates(reflection, writer);
                WritePrefixProcedureModifiers(reflection, writer);
                WriteReturnValue(reflection, writer);
                writer.WriteString(" ");
                writer.WriteIdentifier(name);
                WriteMethodParameters(reflection, writer);
                WritePostfixProcedureModifiers(reflection, writer);
            }

            WriteExplicitImplementations(reflection, writer);
        }

        private void WriteExplicitImplementations(XPathNavigator reflection, SyntaxWriter writer)
        {
            bool isExplicit = (bool)reflection.Evaluate(apiIsExplicitImplementationExpression);
            if(isExplicit)
            {
                writer.WriteString(" = ");

                XPathNodeIterator implements = reflection.Select(apiImplementedMembersExpression);
                while(implements.MoveNext())
                {
                    XPathNavigator implement = implements.Current;
                    //string id = (string)implement.GetAttribute("api", String.Empty);
                    XPathNavigator contract = implement.SelectSingleNode(memberDeclaringTypeExpression);

                    if(implements.CurrentPosition > 1)
                        writer.WriteString(", ");
                    WriteTypeReference(contract, false, writer);
                    writer.WriteString("::");
                    WriteMemberReference(implement, writer);
                    // writer.WriteReferenceLink(id);
                }
            }
        }

        /// <inheritdoc />
        public override void WriteOperatorSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string name = (string)reflection.Evaluate(apiNameExpression);

            string identifier;
            switch(name)
            {
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
                // others
                case "Comma":
                    identifier = ",";
                    break;
                case "MemberSelection":
                    identifier = "->";
                    break;
                case "AddressOf":
                    identifier = "&";
                    break;
                case "PointerDereference":
                    identifier = "*";
                    break;
                case "Assign":
                    identifier = "=";
                    break;
                // unrecognized operator
                default:
                    identifier = null;
                    break;
            }

            if(identifier == null)
            {
                writer.WriteMessage("UnsupportedOperator_" + Language);
            }
            else
            {

                WriteProcedureVisibility(reflection, writer);
                WritePrefixProcedureModifiers(reflection, writer);
                WriteReturnValue(reflection, writer);
                writer.WriteString(" ");
                writer.WriteKeyword("operator");
                writer.WriteString(" ");
                writer.WriteIdentifier(identifier);
                WriteMethodParameters(reflection, writer);
            }

        }

        /// <inheritdoc />
        public override void WriteCastSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string name = (string)reflection.Evaluate(apiNameExpression);

            WritePrefixProcedureModifiers(reflection, writer);
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
                throw new InvalidOperationException("invalid cast type: " + name);
            }
            writer.WriteString(" ");
            WriteReturnValue(reflection, writer);
            writer.WriteString(" ");
            WriteMethodParameters(reflection, writer);

        }

        /// <inheritdoc />
        public override void WritePropertySyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isDefault = (bool)reflection.Evaluate(apiIsDefaultMemberExpression);
            bool hasGetter = (bool)reflection.Evaluate(apiIsReadPropertyExpression);
            bool hasSetter = (bool)reflection.Evaluate(apiIsWritePropertyExpression);
            bool isExplicit = (bool)reflection.Evaluate(apiIsExplicitImplementationExpression);
            XPathNodeIterator parameters = reflection.Select(apiParametersExpression);

            // !EFW - Write visibility before attributes
            WriteProcedureVisibility(reflection, writer);

            WriteAttributes(reflection, writer);
            WritePrefixProcedureModifiers(reflection, writer);
            writer.WriteKeyword("property");
            writer.WriteString(" ");
            WriteReturnValue(reflection, writer);
            writer.WriteString(" ");

            // if is default member, write default, otherwise write name
            if(isDefault)
            {
                writer.WriteKeyword("default");
            }
            else
            {
                writer.WriteIdentifier(name);
            }

            if(parameters.Count > 0)
            {
                writer.WriteString("[");
                WriteParameters(parameters.Clone(), false, writer);
                writer.WriteString("]");
            }

            writer.WriteString(" {");
            writer.WriteLine();

            if(hasGetter)
            {
                // !EFW - Added support for getter/setter attributes
                XPathNavigator getter = reflection.SelectSingleNode(apiGetterExpression);

                if(getter != null && !getter.HasChildren)
                    writer.WriteString("\t");

                // write the get visibility
                string getVisibility = (string)reflection.Evaluate(apiGetVisibilityExpression);

                if(!String.IsNullOrEmpty(getVisibility))
                {
                    WriteVisibility(getVisibility, writer);
                    writer.WriteString(":");

                    if(!getter.HasChildren)
                        writer.WriteString(" ");
                    else
                        writer.WriteLine();
                }

                if(getter != null && getter.HasChildren)
                {
                    writer.WriteString("\t");
                    this.WriteAttributes(getter, writer, "\t");
                    writer.WriteString("\t");
                }

                WriteReturnValue(reflection, writer);

                writer.WriteString(" ");
                writer.WriteKeyword("get");
                writer.WriteString(" ");

                writer.WriteString("(");
                WriteParameters(parameters.Clone(), false, writer);
                writer.WriteString(")");

                WritePostfixProcedureModifiers(reflection, writer);

                if(isExplicit)
                {
                    XPathNavigator implement = reflection.SelectSingleNode(apiImplementedMembersExpression);
                    XPathNavigator contract = implement.SelectSingleNode(memberDeclaringTypeExpression);
                    //string id = (string) implement.GetAttribute("api", String.Empty);

                    writer.WriteString(" = ");
                    WriteTypeReference(contract, false, writer);
                    writer.WriteString("::");
                    WriteMemberReference(implement, writer);
                    //writer.WriteReferenceLink(id);
                    writer.WriteString("::");
                    writer.WriteKeyword("get");
                }

                writer.WriteString(";");
                writer.WriteLine();
            }

            if(hasSetter)
            {
                // !EFW - Added support for getter/setter attributes
                XPathNavigator setter = reflection.SelectSingleNode(apiSetterExpression);

                if(setter != null && !setter.HasChildren)
                    writer.WriteString("\t");

                // Write the set visibility
                string setVisibility = (string)reflection.Evaluate(apiSetVisibilityExpression);

                if(!String.IsNullOrEmpty(setVisibility))
                {
                    WriteVisibility(setVisibility, writer);
                    writer.WriteString(":");

                    if(!setter.HasChildren)
                        writer.WriteString(" ");
                    else
                        writer.WriteLine();
                }

                if(setter != null && setter.HasChildren)
                {
                    writer.WriteString("\t");
                    this.WriteAttributes(setter, writer, "\t");
                    writer.WriteString("\t");
                }

                writer.WriteKeyword("void");
                writer.WriteString(" ");
                writer.WriteKeyword("set");
                writer.WriteString(" ");

                writer.WriteString("(");

                if(parameters.Count > 0)
                {
                    WriteParameters(parameters.Clone(), false, writer);
                    writer.WriteString(", ");
                }

                WriteReturnValue(reflection, writer);
                writer.WriteString(" ");
                writer.WriteParameter("value");
                writer.WriteString(")");

                WritePostfixProcedureModifiers(reflection, writer);

                if(isExplicit)
                {
                    XPathNavigator implement = reflection.SelectSingleNode(apiImplementedMembersExpression);
                    XPathNavigator contract = implement.SelectSingleNode(memberDeclaringTypeExpression);

                    writer.WriteString(" = ");
                    WriteTypeReference(contract, false, writer);
                    writer.WriteString("::");
                    WriteMemberReference(implement, writer);
                    writer.WriteString("::");
                    writer.WriteKeyword("set");
                }

                writer.WriteString(";");
                writer.WriteLine();
            }

            writer.WriteString("}");
        }

        /// <inheritdoc />
        public override void WriteEventSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string name = (string)reflection.Evaluate(apiNameExpression);
            XPathNavigator handler = reflection.SelectSingleNode(apiHandlerOfEventExpression);

            // !EFW - Write visibility before attributes
            WriteProcedureVisibility(reflection, writer);

            WriteAttributes(reflection, writer);
            WritePrefixProcedureModifiers(reflection, writer);
            writer.WriteString(" ");
            writer.WriteKeyword("event");
            writer.WriteString(" ");
            WriteTypeReference(handler, writer);
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            writer.WriteString(" {");
            writer.WriteLine();

            writer.WriteString("\t");
            writer.WriteKeyword("void");
            writer.WriteString(" ");
            writer.WriteKeyword("add");
            writer.WriteString(" (");
            WriteTypeReference(handler, writer);
            writer.WriteString(" ");
            writer.WriteParameter("value");
            writer.WriteString(")");
            writer.WriteString(";");
            writer.WriteLine();

            writer.WriteString("\t");
            writer.WriteKeyword("void");
            writer.WriteString(" ");
            writer.WriteKeyword("remove");
            writer.WriteString(" (");
            WriteTypeReference(handler, writer);
            writer.WriteString(" ");
            writer.WriteParameter("value");
            writer.WriteString(")");
            writer.WriteString(";");
            writer.WriteLine();


            writer.WriteString("}");

        }

        /// <inheritdoc />
        public override void WriteFieldSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isStatic = (bool)reflection.Evaluate(apiIsStaticExpression);
            bool isLiteral = (bool)reflection.Evaluate(apiIsLiteralFieldExpression);
            bool isInitOnly = (bool)reflection.Evaluate(apiIsInitOnlyFieldExpression);
            bool isSerialized = (bool)reflection.Evaluate(apiIsSerializedFieldExpression);

            // !EFW - Added support for fixed keyword
            XPathNavigator fixedAttribute = reflection.SelectSingleNode(apiFixedAttribute);

            // !EFW - Write visibility before attributes
            WriteVisibility(reflection, writer);
            writer.WriteString(":");
            writer.WriteLine();

            if(!isSerialized)
                WriteAttribute("T:System.NonSerializedAttribute", true, writer);
            WriteAttributes(reflection, writer);

            if(isStatic)
            {
                if(isLiteral)
                {
                    writer.WriteKeyword("literal");
                }
                else
                {
                    writer.WriteKeyword("static");
                }
                writer.WriteString(" ");
            }
            if(isInitOnly)
            {
                writer.WriteKeyword("initonly");
                writer.WriteString(" ");
            }

            if(fixedAttribute != null)
            {
                // EFW - My Managed C++ skills are non-existent but I think this syntax is okay.
                // Feel free to correct me if I'm wrong.
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

        private static void WritePrefixProcedureModifiers(XPathNavigator reflection, SyntaxWriter writer)
        {
            // interface members don't get modified
            string typeSubgroup = (string)reflection.Evaluate(apiContainingTypeSubgroupExpression);

            if(typeSubgroup == "interface")
                return;

            bool isStatic = (bool)reflection.Evaluate(apiIsStaticExpression);
            bool isVirtual = (bool)reflection.Evaluate(apiIsVirtualExpression);

            if(isStatic)
            {
                writer.WriteKeyword("static");
                writer.WriteString(" ");
            }
            else if(isVirtual)
            {
                writer.WriteKeyword("virtual");
                writer.WriteString(" ");
            }
        }

        private static void WritePostfixProcedureModifiers(XPathNavigator reflection, SyntaxWriter writer)
        {
            // interface members don't get modified
            string typeSubgroup = (string)reflection.Evaluate(apiContainingTypeSubgroupExpression);

            if(typeSubgroup == "interface")
                return;

            bool isVirtual = (bool)reflection.Evaluate(apiIsVirtualExpression);
            bool isAbstract = (bool)reflection.Evaluate(apiIsAbstractProcedureExpression);
            bool isFinal = (bool)reflection.Evaluate(apiIsFinalExpression);
            bool isOverride = (bool)reflection.Evaluate(apiIsOverrideExpression);

            if(isVirtual)
            {
                if(isAbstract)
                {
                    writer.WriteString(" ");
                    writer.WriteKeyword("abstract");
                }
                if(isOverride)
                {
                    writer.WriteString(" ");
                    writer.WriteKeyword("override");
                }
                if(isFinal)
                {
                    writer.WriteString(" ");
                    writer.WriteKeyword("sealed");
                }
            }
        }

        // Visibility

        private static void WriteProcedureVisibility(XPathNavigator reflection, SyntaxWriter writer)
        {
            string typeSubgroup = (string)reflection.Evaluate(apiContainingTypeSubgroupExpression);

            if(typeSubgroup != "interface")
            {
                WriteVisibility(reflection, writer);
                writer.WriteString(":");
                writer.WriteLine();
            }
        }

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
                    writer.WriteKeyword("protected public");
                    break;
                case "family and assembly":
                    writer.WriteKeyword("protected private");
                    break;
                case "assembly":
                    writer.WriteKeyword("internal");
                    break;
                case "private":
                    writer.WriteKeyword("private");
                    break;
            }
        }

        // Generics

        private void WriteGenericTemplates(XPathNavigator reflection, SyntaxWriter writer)
        {
            XPathNodeIterator templateNodes = (XPathNodeIterator)reflection.Evaluate(apiTemplatesExpression);

            if(templateNodes.Count == 0)
                return;

            XPathNavigator[] templates = templateNodes.ToArray();

            if(templates.Length == 0)
                return;

            // generic declaration
            writer.WriteKeyword("generic");
            writer.WriteString("<");

            for(int i = 0; i < templates.Length; i++)
            {
                XPathNavigator template = templates[i];
                string name = (string)template.Evaluate(templateNameExpression);
                if(i > 0)
                    writer.WriteString(", ");
                writer.WriteKeyword("typename");
                writer.WriteString(" ");
                writer.WriteString(name);
            }

            writer.WriteString(">");
            writer.WriteLine();

            // generic constraints
            foreach(XPathNavigator template in templates)
            {
                bool constrained = (bool)template.Evaluate(templateIsConstrainedExpression);
                if(!constrained)
                    continue;

                string name = (string)template.Evaluate(templateNameExpression);

                writer.WriteKeyword("where");
                writer.WriteString(" ");
                writer.WriteString(name);
                writer.WriteString(" : ");

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
                    writer.WriteKeyword("value class");
                    previous = true;
                }

                if(reference)
                {
                    if(previous)
                        writer.WriteString(", ");
                    writer.WriteKeyword("ref class");
                    previous = true;
                }

                if(constructor)
                {
                    if(previous)
                        writer.WriteString(", ");
                    writer.WriteKeyword("gcnew");
                    writer.WriteString("()");
                    previous = true;
                }

                foreach(XPathNavigator constraint in constraints)
                {
                    if(previous)
                        writer.WriteString(", ");
                    WriteTypeReference(constraint, false, writer);
                    previous = true;
                }

                writer.WriteLine();

            }

        }

        // Interfaces

        private void WriteImplementedInterfaces(XPathNavigator reflection, SyntaxWriter writer)
        {
            XPathNodeIterator implements = reflection.Select(apiImplementedInterfacesExpression);

            if(implements.Count != 0)
            {
                writer.WriteString(" : ");

                while(implements.MoveNext())
                {
                    XPathNavigator implement = implements.Current;
                    WriteTypeReference(implement, false, writer);

                    if(implements.CurrentPosition < implements.Count)
                        WriteWithLineBreakIfNeeded(writer, ", ", "\t");
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
                    writer.WriteKeyword("public");
                    writer.WriteString(" ");
                    WriteTypeReference(baseClass, false, writer);

                    if(hasImplementedInterfaces)
                        WriteWithLineBreakIfNeeded(writer, ", ", "\t");
                }

                while(implements.MoveNext())
                {
                    XPathNavigator implement = implements.Current;
                    WriteTypeReference(implement, false, writer);

                    if(implements.CurrentPosition < implements.Count)
                        WriteWithLineBreakIfNeeded(writer, ", ", "\t");
                }
            }
        }

        // Return Value

        private void WriteReturnValue(XPathNavigator reflection, SyntaxWriter writer)
        {

            XPathNavigator type = reflection.SelectSingleNode(apiReturnTypeExpression);

            if(type == null)
                writer.WriteKeyword("void");
            else
                WriteTypeReference(type, writer);
        }

        private void WriteMethodParameters(XPathNavigator reflection, SyntaxWriter writer)
        {

            XPathNodeIterator parameters = reflection.Select(apiParametersExpression);

            writer.WriteString("(");
            if(parameters.Count > 0)
            {
                writer.WriteLine();
                WriteParameters(parameters, true, writer);
            }
            writer.WriteString(")");

        }

        private void WriteParameters(XPathNodeIterator parameters, bool multiline, SyntaxWriter writer)
        {
            bool isVarargs = (bool)parameters.Current.Evaluate(apiIsVarargsExpression);

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

                if(multiline)
                    writer.WriteString("\t");

                // !EFW - Optional indicated by OptionalAttribute?
                if(isOptional && argument == null)
                {
                    WriteAttribute("T:System.Runtime.InteropServices.OptionalAttribute", false, writer);
                    writer.WriteString(" ");
                }

                if(isIn)
                {
                    WriteAttribute("T:System.Runtime.InteropServices.InAttribute", false, writer);
                    writer.WriteString(" ");
                }

                if(isOut)
                {
                    WriteAttribute("T:System.Runtime.InteropServices.OutAttribute", false, writer);
                    writer.WriteString(" ");
                }

                // !EFW - Write out parameter attributes
                WriteAttributes(parameter, writer, null, true);

                if(isParamArray)
                    writer.WriteString("... ");

                WriteTypeReference(type, writer);
                writer.WriteString(" ");
                writer.WriteParameter(name);

                // !EFW - Write optional value if present
                if(argument != null)
                {
                    writer.WriteString(" = ");
                    this.WriteValue(argument, writer);
                }

                if(parameters.CurrentPosition < parameters.Count || isVarargs)
                    writer.WriteString(", ");

                if(multiline)
                    writer.WriteLine();
            }
            if(isVarargs)
            {
                if(multiline)
                    writer.WriteString("\t");
                writer.WriteString("...");
                if(multiline)
                    writer.WriteLine();
            }


        }

        // Type references

        private void WriteTypeReference(XPathNavigator reference, SyntaxWriter writer)
        {
            WriteTypeReference(reference, true, writer);
        }

        private void WriteTypeReference(XPathNavigator reference, bool handle, SyntaxWriter writer)
        {
            switch(reference.LocalName)
            {
                case "arrayOf":
                    XPathNavigator element = reference.SelectSingleNode(typeExpression);
                    int rank = Convert.ToInt32(reference.GetAttribute("rank", String.Empty),
                        CultureInfo.InvariantCulture);

                    writer.WriteKeyword("array");
                    writer.WriteString("<");
                    WriteTypeReference(element, writer);

                    if(rank > 1)
                    {
                        writer.WriteString(",");
                        writer.WriteString(rank.ToString(CultureInfo.InvariantCulture));
                    }

                    writer.WriteString(">");

                    if(handle)
                        writer.WriteString("^");
                    break;

                case "pointerTo":
                    XPathNavigator pointee = reference.SelectSingleNode(typeExpression);
                    WriteTypeReference(pointee, writer);
                    writer.WriteString("*");
                    break;

                case "referenceTo":
                    XPathNavigator referee = reference.SelectSingleNode(typeExpression);
                    
                    // !EFW - Convert explicit dereference to interior_ptr<T>
                    bool isExplicitDereference = (bool)reference.Evaluate(explicitDereferenceExpression);

                    if(isExplicitDereference)
                        writer.WriteString("interior_ptr<");

                    WriteTypeReference(referee, writer);

                    if(isExplicitDereference)
                        writer.WriteString(">");
                    else
                        writer.WriteString("%");
                    break;

                case "type":
                    string id = reference.GetAttribute("api", String.Empty);
                    bool isRef = (reference.GetAttribute("ref", String.Empty) == "true");
                    WriteNormalTypeReference(id, writer);
                    XPathNodeIterator typeModifiers = reference.Select(typeModifiersExpression);

                    while(typeModifiers.MoveNext())
                        WriteTypeReference(typeModifiers.Current, writer);

                    if(handle && isRef)
                        writer.WriteString("^");

                    break;

                case "template":
                    string name = reference.GetAttribute("name", String.Empty);
                    writer.WriteString(name);
                    XPathNodeIterator modifiers = reference.Select(typeModifiersExpression);

                    while(modifiers.MoveNext())
                        WriteTypeReference(modifiers.Current, writer);

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

        private static void WriteNormalTypeReference(string reference, SyntaxWriter writer)
        {
            switch(reference)
            {
                case "T:System.Void":
                    writer.WriteReferenceLink(reference, "void");
                    break;
                case "T:System.Boolean":
                    writer.WriteReferenceLink(reference, "bool");
                    break;
                case "T:System.Byte":
                    writer.WriteReferenceLink(reference, "unsigned char");
                    break;
                case "T:System.SByte":
                    writer.WriteReferenceLink(reference, "signed char");
                    break;
                case "T:System.Char":
                    writer.WriteReferenceLink(reference, "wchar_t");
                    break;
                case "T:System.Int16":
                    writer.WriteReferenceLink(reference, "short");
                    break;
                case "T:System.Int32":
                    writer.WriteReferenceLink(reference, "int");
                    break;
                case "T:System.Int64":
                    writer.WriteReferenceLink(reference, "long long");
                    break;
                case "T:System.UInt16":
                    writer.WriteReferenceLink(reference, "unsigned short");
                    break;
                case "T:System.UInt32":
                    writer.WriteReferenceLink(reference, "unsigned int");
                    break;
                case "T:System.UInt64":
                    writer.WriteReferenceLink(reference, "unsigned long long");
                    break;
                case "T:System.Single":
                    writer.WriteReferenceLink(reference, "float");
                    break;
                case "T:System.Double":
                    writer.WriteReferenceLink(reference, "double");
                    break;
                default:
                    writer.WriteReferenceLink(reference);
                    break;
            }
        }

        // Attributes

        private static void WriteAttribute(string reference, bool newline, SyntaxWriter writer)
        {
            writer.WriteString("[");
            writer.WriteReferenceLink(reference);
            writer.WriteString("]");

            if(newline)
                writer.WriteLine();
        }

        // !EFW - Added indent parameter for property getter/setter attributes.  Added parameterAttributes to
        // suppress line feeds for method parameter attributes.
        private void WriteAttributes(XPathNavigator reflection, SyntaxWriter writer, string indent = null,
            bool parameterAttributes = false)
        {
            // Handle interop attributes first as they are output in metadata
            if(!parameterAttributes)
                WriteInteropAttributes(reflection, writer, indent);

            // Add the standard attributes
            XPathNodeIterator attributes = (XPathNodeIterator)reflection.Evaluate(apiAttributesExpression);

            foreach(XPathNavigator attribute in attributes)
            {
                XPathNavigator type = attribute.SelectSingleNode(typeExpression);

                // !EFW - Ignore FixedBufferAttribute and ParamArrayAttribute
                if(type.GetAttribute("api", String.Empty) == "T:System.Runtime.CompilerServices.FixedBufferAttribute" ||
                  type.GetAttribute("api", String.Empty) == "T:System.ParamArrayAttribute")
                    continue;

                writer.WriteString("[");
                WriteTypeReference(type, false, writer);

                XPathNodeIterator arguments = (XPathNodeIterator)attribute.Select(attributeArgumentsExpression);
                XPathNodeIterator assignments = (XPathNodeIterator)attribute.Select(attributeAssignmentsExpression);

                if((arguments.Count > 0) || (assignments.Count > 0))
                {
                    writer.WriteString("(");
                    while(arguments.MoveNext())
                    {
                        XPathNavigator argument = arguments.Current;

                        if(arguments.CurrentPosition > 1)
                            WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");

                        WriteValue(argument, writer);
                    }

                    if(arguments.Count > 0 && assignments.Count > 0)
                        writer.WriteString(", ");

                    while(assignments.MoveNext())
                    {
                        XPathNavigator assignment = assignments.Current;

                        if(assignments.CurrentPosition > 1)
                            WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");

                        writer.WriteString((string)assignment.Evaluate(assignmentNameExpression));
                        writer.WriteString(" = ");
                        WriteValue(assignment, writer);

                    }

                    writer.WriteString(")");
                }

                writer.WriteString("]");

                if(!parameterAttributes)
                    writer.WriteLine();
            }

            if(parameterAttributes && attributes.Count != 0)
                writer.WriteString(" ");
        }

        // EFW - Added support for interop attributes stored in metadata
        private static void WriteInteropAttributes(XPathNavigator reflection, SyntaxWriter writer, string indent = null)
        {
            if((bool)reflection.Evaluate(apiComImportTypeExpression))
                WriteAttribute("T:System.Runtime.InteropServices.ComImportAttribute", true, writer);

            string layout = (string)reflection.Evaluate(apiStructLayoutTypeExpression);

            if(!String.IsNullOrEmpty(layout))
            {
                double size = (double)reflection.Evaluate(apiStructLayoutSizeTypeExpression),
                    pack = (double)reflection.Evaluate(apiStructLayoutPackTypeExpression);
                string format = (string)reflection.Evaluate(apiStructLayoutFormatTypeExpression);

                writer.WriteString("[");
                WriteNormalTypeReference("T:System.Runtime.InteropServices.StructLayoutAttribute", writer);
                writer.WriteString("(");

                switch(layout)
                {
                    case "explicit":
                        writer.WriteString("LayoutKind::Explicit");
                        break;

                    case "sequential":
                        writer.WriteString("LayoutKind::Sequential");
                        break;

                    default:
                        writer.WriteString("LayoutKind::Auto");
                        break;
                }

                if(!Double.IsNaN(size) && size != 0)
                {
                    WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                    writer.WriteString("Size = ");
                    writer.WriteString(((int)size).ToString(CultureInfo.InvariantCulture));
                }

                if(!Double.IsNaN(pack) && pack != 0)
                {
                    WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                    writer.WriteString("Pack = ");
                    writer.WriteString(((int)pack).ToString(CultureInfo.InvariantCulture));
                }

                switch(format)
                {
                    case "ansi":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CharSet = CharSet::Ansi");
                        break;

                    case "auto":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CharSet = CharSet::Auto");
                        break;

                    case "unicode":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CharSet = CharSet::Unicode");
                        break;
                }

                writer.WriteString(")");
                writer.WriteString("]");
                writer.WriteLine();
            }

            double fieldOffset = (double)reflection.Evaluate(apiFieldOffsetFieldExpression);

            if(!Double.IsNaN(fieldOffset))
            {
                writer.WriteString("[");
                WriteNormalTypeReference("T:System.Runtime.InteropServices.FieldOffsetAttribute", writer);
                writer.WriteString("(");
                writer.WriteString(((int)fieldOffset).ToString(CultureInfo.InvariantCulture));
                writer.WriteString(")");
                writer.WriteString("]");
                writer.WriteLine();
            }

            bool preserveSig = (bool)reflection.Evaluate(apiPreserveSigProcedureExpression);
            string module = (string)reflection.Evaluate(apiModuleProcedureExpression);

            if(!String.IsNullOrEmpty(module))
            {
                string entryPoint = (string)reflection.Evaluate(apiEntryPointProcedureExpression),
                    callingConv = (string)reflection.Evaluate(apiCallingConvProcedureExpression),
                    charset = (string)reflection.Evaluate(apiCharSetProcedureExpression),
                    bestFitMapping = (string)reflection.Evaluate(apiBestFitMappingProcedureExpression);

                bool exactSpelling = (bool)reflection.Evaluate(apiExactSpellingProcedureExpression),
                    throwOnUnmappableChar = (bool)reflection.Evaluate(apiUnmappableCharProcedureExpression),
                    setLastError = (bool)reflection.Evaluate(apiSetLastErrorProcedureExpression);

                writer.WriteString("[");
                WriteNormalTypeReference("T:System.Runtime.InteropServices.DllImportAttribute", writer);
                writer.WriteString("(L\"");
                writer.WriteString(module);
                writer.WriteString("\"");

                if(!String.IsNullOrEmpty(entryPoint))
                {
                    WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                    writer.WriteString("EntryPoint = L\"");
                    writer.WriteString(entryPoint);
                    writer.WriteString("\"");
                }

                switch(callingConv)
                {
                    case "cdecl":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CallingConvention = CallingConvention::Cdecl");
                        break;

                    case "fastcall":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CallingConvention = CallingConvention::FastCall");
                        break;

                    case "stdcall":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CallingConvention = CallingConvention::StdCall");
                        break;

                    case "thiscall":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CallingConvention = CallingConvention::ThisCall");
                        break;
                }

                switch(charset)
                {
                    case "ansi":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CharSet = CharSet::Ansi");
                        break;

                    case "auto":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CharSet = CharSet::Auto");
                        break;

                    case "unicode":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CharSet = CharSet::Unicode");
                        break;
                }

                if(!String.IsNullOrEmpty(bestFitMapping) && !Convert.ToBoolean(bestFitMapping, CultureInfo.InvariantCulture))
                {
                    WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                    writer.WriteString("BestFitMapping = false");
                }

                if(exactSpelling)
                {
                    WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                    writer.WriteString("ExactSpelling = true");
                }

                if(throwOnUnmappableChar)
                {
                    WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                    writer.WriteString("ThrowOnUnmappableChar = true");
                }

                if(!preserveSig)
                {
                    WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                    writer.WriteString("PreserveSig = false");
                }

                if(setLastError)
                {
                    WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                    writer.WriteString("SetLastError = true");
                }

                writer.WriteString(")");
                writer.WriteString("]");
                writer.WriteLine();
            }
            else
                if(preserveSig)
                    WriteAttribute("T:System.Runtime.InteropServices.PreserveSigAttribute", true, writer);
        }

        private void WriteValue(XPathNavigator parent, SyntaxWriter writer)
        {
            XPathNavigator type = parent.SelectSingleNode(attributeTypeExpression);
            XPathNavigator value = parent.SelectSingleNode(valueExpression);

            switch(value.LocalName)
            {
                case "nullValue":
                    writer.WriteKeyword("nullptr");
                    break;

                case "typeValue":
                    writer.WriteKeyword("typeof");
                    writer.WriteString("(");
                    WriteTypeReference(value.SelectSingleNode(typeExpression), false, writer);
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
                        writer.WriteString("::");
                        writer.WriteString(name);
                    }
                    break;

                case "value":
                    string text = value.Value;
                    string typeId = type.GetAttribute("api", String.Empty);

                    switch(typeId)
                    {
                        case "T:System.String":
                            writer.WriteString("L\"");
                            writer.WriteString(text);
                            writer.WriteString("\"");
                            break;

                        case "T:System.Boolean":
                            writer.WriteKeyword(Convert.ToBoolean(text, CultureInfo.InvariantCulture) ?
                                "true" : "false");
                            break;

                        case "T:System.Char":
                            writer.WriteString(@"L'");
                            writer.WriteString(text);
                            writer.WriteString(@"'");
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
                                // The syntax here probably isn't perfect but at least it conveys the type.
                                writer.WriteString("__gc new ");
                                this.WriteTypeReference(type, writer);
                                writer.WriteString(" { ... }");
                            }
                            break;
                    }
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
