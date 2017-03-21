// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 01/30/2012 - EFW - Fixed WriteValue() so that it outputs numeric attribute values.
// 02/09/2012 - EFW - Added support for optional parameters and property getter/setter attributes.
// 02/14/2012 - EFW - Added support for fixed keyword
// 11/29/2013 - EFW - Added support for metadata based interop attributes
// 12/20/2013 - EFW - Updated the syntax generator to be discoverable via MEF
// 08/01/2014 - EFW - Added support for resource item files containing the localized titles, messages, etc.
// 11/20/2014 - EFW - Added support for writing out method parameter attributes
// 10/08/2015 - EFW - Added support for writing out the value of constant fields

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml.XPath;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler.SyntaxGenerator;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This class generates declaration syntax sections for X#
    /// </summary>
    public sealed class XSharpDeclarationSyntaxGenerator : SyntaxGeneratorTemplate
    {
        #region Syntax generator factory for MEF
        //=====================================================================

        private const string LanguageName = "XSharp", StyleIdName = "xs";
        private bool InFunctionsClass = false;
        /// <summary>
        /// This is used to create a new instance of the syntax generator
        /// </summary>
        [SyntaxGeneratorExport("X#", LanguageName, StyleIdName, AlternateIds = "XSharp, xs",
          SortOrder = 100, Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
          Description = "Generates X# declaration syntax sections")]
        public sealed class Factory : ISyntaxGeneratorFactory
        {
            /// <inheritdoc />
            public string ResourceItemFileLocation
            {
                get
                {
                    return Path.Combine(ComponentUtilities.AssemblyFolder(Assembly.GetExecutingAssembly()), "SyntaxContent");
                }
            }

            /// <inheritdoc />
            public SyntaxGeneratorCore Create()
            {
                return new XSharpDeclarationSyntaxGenerator { Language = LanguageName, StyleId = StyleIdName };
            }
        }
        #endregion

        /// <inheritdoc />
        public override void WriteNamespaceSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string name = reflection.Evaluate(apiNameExpression).ToString();

            writer.WriteKeyword("BEGIN NAMESPACE");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
        }

        /// <inheritdoc />
        public override void WriteClassSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            //System.Diagnostics.Debugger.Launch();
            string name = reflection.Evaluate(apiNameExpression).ToString();
            InFunctionsClass = (string.Compare(name, "Functions", true) == 0);
            bool isAbstract = (bool)reflection.Evaluate(apiIsAbstractTypeExpression);
            bool isSealed = (bool)reflection.Evaluate(apiIsSealedTypeExpression);
            bool isSerializable = (bool)reflection.Evaluate(apiIsSerializableTypeExpression);
            if(isSerializable)
                WriteAttribute("T:System.SerializableAttribute", true, writer);
            WriteAttributes(reflection, writer);
            WriteVisibility(reflection, writer);
            //System.Diagnostics.Debugger.Launch();
            writer.WriteString(" ");
            if(isAbstract)
            {
                if(isSealed)
                {
                    writer.WriteKeyword("STATIC");
                }
                else
                {
                    writer.WriteKeyword("ABSTRACT");
                }
                writer.WriteString(" ");
            }
            else
            {
                if(isSealed)
                {
                    writer.WriteKeyword("SEALED");
                    writer.WriteString(" ");
                }
            }
            writer.WriteKeyword("CLASS");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            WriteGenericTemplates(reflection, writer, false);
            WriteBaseClassAndImplementedInterfaces(reflection, writer);
            WriteGenericTemplateConstraints(reflection, writer);

        }

        /// <inheritdoc />
        public override void WriteStructureSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isSerializable = (bool)reflection.Evaluate(apiIsSerializableTypeExpression);

            if(isSerializable)
                WriteAttribute("T:System.SerializableAttribute", true, writer);
            WriteAttributes(reflection, writer);
            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            writer.WriteKeyword("STRUCTURE");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            WriteGenericTemplates(reflection, writer, false);
            WriteImplementedInterfaces(reflection, writer);
            WriteGenericTemplateConstraints(reflection, writer);

        }

        /// <inheritdoc />
        public override void WriteInterfaceSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            string name = (string)reflection.Evaluate(apiNameExpression);

            WriteAttributes(reflection, writer);
            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            writer.WriteKeyword("INTERFACE");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            WriteGenericTemplates(reflection, writer, true); // interfaces need co/contravariance info
            WriteImplementedInterfaces(reflection, writer);
            WriteGenericTemplateConstraints(reflection, writer);

        }

        /// <inheritdoc />
        public override void WriteDelegateSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isSerializable = (bool)reflection.Evaluate(apiIsSerializableTypeExpression);

            if(isSerializable)
                WriteAttribute("T:System.SerializableAttribute", true, writer);
            WriteAttributes(reflection, writer);
            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            writer.WriteKeyword("DELEGATE");
            writer.WriteString(" ");
            WriteReturnValue(reflection, writer);
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            WriteGenericTemplates(reflection, writer, true); // delegates need co/contravariance info
            WriteMethodParameters(reflection, writer);
            WriteGenericTemplateConstraints(reflection, writer);

        }

        // enumeration: still need to handle non-standard base
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
            writer.WriteKeyword("enum");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
        }

        /// <inheritdoc />
        public override void WriteConstructorSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            string name = (string)reflection.Evaluate(apiContainingTypeNameExpression);
            bool isStatic = (bool)reflection.Evaluate(apiIsStaticExpression);

            WriteAttributes(reflection, writer);
            if(isStatic)
            {
                writer.WriteKeyword("STATIC");
            }
            else
            {
                WriteVisibility(reflection, writer);
            }
            writer.WriteString(" ");
            writer.WriteKeyword("CONSTRUCTOR");
            WriteMethodParameters(reflection, writer);

        }

        /// <inheritdoc />
        public override void WriteNormalMethodSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(IsUnsupportedVarargs(reflection, writer))
                return;

            string name = (string)reflection.Evaluate(apiNameExpression);
            XPathNavigator type = reflection.SelectSingleNode(apiReturnTypeExpression);

            bool isExplicit = (bool)reflection.Evaluate(apiIsExplicitImplementationExpression);

            WriteAttributes(reflection, writer);
            if(!isExplicit)
                WriteProcedureModifiers(reflection, writer);
            if (InFunctionsClass)
                writer.WriteKeyword("FUNCTION");
            else
                writer.WriteKeyword("METHOD");
            writer.WriteString(" ");

            if (isExplicit)
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
            if (type != null)
            {
                writer.WriteString(" ");
                writer.WriteKeyword("AS");
                writer.WriteString(" ");
                WriteTypeReference(type, writer);
            }
            else
            {
                writer.WriteString(" ");
                writer.WriteKeyword("AS");
                writer.WriteString(" ");
                writer.WriteKeyword("VOID");
            }
        }

        /// <summary>
        /// Write out a normal type reference
        /// </summary>
        /// <param name="api">The type reference to output</param>
        /// <param name="writer">The syntax writer to which the type reference is written</param>
        protected override void WriteNormalTypeReference(string api, SyntaxWriter writer)
        {
            switch (api)
            {
                case "T:System.Void":
                    writer.WriteReferenceLink(api, "VOID");
                    break;

                case "T:System.String":
                    writer.WriteReferenceLink(api, "STRING");
                    break;

                case "T:System.Boolean":
                    writer.WriteReferenceLink(api, "LOGIC");
                    break;

                case "T:System.Byte":
                    writer.WriteReferenceLink(api, "BYTE");
                    break;

                case "T:System.SByte":
                    writer.WriteReferenceLink(api, "System.SByte");
                    break;

                case "T:System.Char":
                    writer.WriteReferenceLink(api, "CHAR");
                    break;

                case "T:System.Int16":
                    writer.WriteReferenceLink(api, "SHORT");
                    break;

                case "T:System.Int32":
                    writer.WriteReferenceLink(api, "LONG");
                    break;

                case "T:System.Int64":
                    writer.WriteReferenceLink(api, "INT64");
                    break;

                case "T:System.UInt16":
                    writer.WriteReferenceLink(api, "WORD");
                    break;

                case "T:System.UInt32":
                    writer.WriteReferenceLink(api, "DWORD");
                    break;

                case "T:System.UInt64":
                    writer.WriteReferenceLink(api, "UINT64");
                    break;

                case "T:System.Single":
                    writer.WriteReferenceLink(api, "REAL4");
                    break;

                case "T:System.Double":
                    writer.WriteReferenceLink(api, "REAL8");
                    break;

                case "T:System.Decimal":
                    writer.WriteReferenceLink(api, "DECIMAL");
                    break;

                case "T:Vulcan.__Array":
                    writer.WriteReferenceLink(api, "ARRAY");
                    break;

                case "T:Vulcan.__Usual":
                    writer.WriteReferenceLink(api, "USUAL");
                    break;

                case "T:Vulcan.__Psz":
                    writer.WriteReferenceLink(api, "PSZ");
                    break;

                case "T:Vulcan.__VOFloat":
                    writer.WriteReferenceLink(api, "FLOAT");
                    break;

                case "T:Vulcan.__VODate":
                    writer.WriteReferenceLink(api, "DATE");
                    break;

                case "T:Vulcan.__Symbol":
                    writer.WriteReferenceLink(api, "SYMBOL");
                    break;

                case "T:Vulcan.__WinBool":
                    writer.WriteReferenceLink(api, "LOGIC");
                    break;

                default:
                    writer.WriteReferenceLink(api);
                    break;
            }
        }
        /// <inheritdoc />
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
                        identifier = "TRUE";
                        break;
                    case "False":
                        identifier = "FALSE";
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
                        identifier = ":=";
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
                writer.WriteString(" ");
                writer.WriteKeyword("OPERATOR");
                writer.WriteString(" ");
                writer.WriteIdentifier(identifier);
                WriteMethodParameters(reflection, writer);
                writer.WriteString(" ");
                writer.WriteKeyword("AS");
                writer.WriteString(" ");
                WriteReturnValue(reflection, writer);
            }
        }

        /// <inheritdoc />
        public override void WriteCastSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            string name = (string)reflection.Evaluate(apiNameExpression);

            WriteProcedureModifiers(reflection, writer);
            if(name == "Implicit")
            {
                writer.WriteKeyword("OPERATOR IMPLICIT");
            }
            else if(name == "Explicit")
            {
                writer.WriteKeyword("OPERATOR EXPLICIT");
            }
            else
            {
                throw new InvalidCastException("Invalid cast.");
            }
            writer.WriteString(" ");
            WriteMethodParameters(reflection, writer);
            writer.WriteString(" ");
            writer.WriteKeyword("AS");
            writer.WriteString(" ");
            WriteReturnValue(reflection, writer);

        }

        /// <inheritdoc />
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
            writer.WriteKeyword("PROPERTY");

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
                    writer.WriteKeyword("SELF");
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
                    writer.WriteKeyword("SELF");
                }
                else
                {
                    writer.WriteIdentifier(name);
                }
            }
            WritePropertyParameters(reflection, writer);
            writer.WriteString(" ");
            writer.WriteKeyword("AS");
            writer.WriteString(" ");
            WriteReturnValue(reflection, writer);

            if(isGettable)
            {
                // !EFW - Added support for getter/setter attributes
                XPathNavigator getter = reflection.SelectSingleNode(apiGetterExpression);

                if(getter != null && getter.HasChildren)
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

                writer.WriteKeyword("GET");

                if(getter != null && getter.HasChildren)
                    writer.WriteLine();
            }

            if(isSettable)
            {
                // !EFW - Added support for getter/setter attributes
                XPathNavigator setter = reflection.SelectSingleNode(apiSetterExpression);

                if(setter != null && setter.HasChildren)
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

                writer.WriteKeyword("SET");

                if(setter != null && setter.HasChildren)
                    writer.WriteLine();
            }

            writer.WriteString(" ");
        }

        /// <inheritdoc />
        public override void WriteEventSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string name = (string)reflection.Evaluate(apiNameExpression);
            XPathNavigator handler = reflection.SelectSingleNode(apiHandlerOfEventExpression);
            bool isExplicit = (bool)reflection.Evaluate(apiIsExplicitImplementationExpression);

            WriteAttributes(reflection, writer);
            if(!isExplicit)
                WriteProcedureModifiers(reflection, writer);
            writer.WriteString("EVENT");
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
            writer.WriteString(" ");
            writer.WriteKeyword("AS");
            writer.WriteString(" ");
            WriteReturnValue(reflection, writer);
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
                if (!InFunctionsClass) 
                {
                    writer.WriteKeyword("STATIC");
                    writer.WriteString(" ");
                }
            } else
            {
                if(isVirtual)
                {
                    if(isAbstract)
                    {
                        writer.WriteKeyword("ABSTRACT");
                        writer.WriteString(" ");
                    }
                    else if(isOverride)
                    {
                        writer.WriteKeyword("VIRTUAL");
                        writer.WriteString(" ");
                        if(isFinal)
                        {
                            writer.WriteKeyword("SEALED");
                            writer.WriteString(" ");
                        }
                    }
                    else
                    {
                        if(!isFinal)
                        {
                            writer.WriteKeyword("VIRTUAL");
                            writer.WriteString(" ");
                        }
                    }
                }
            }
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

            if(!isSerialized)
                WriteAttribute("T:System.NonSerializedAttribute", true, writer);

            WriteAttributes(reflection, writer);
            WriteVisibility(reflection, writer);
            writer.WriteString(" ");

            if(isStatic)
            {
                if (InFunctionsClass) {
                    if(isLiteral)
                        writer.WriteKeyword("DEFINE");
                    else
                        writer.WriteKeyword("GLOBAL");

                }
                else
                    if(isLiteral) {
                    writer.WriteKeyword("CONST");
                } else {
                    writer.WriteKeyword("STATIC");
                }

                writer.WriteString(" ");
            }

            if(isInitOnly)
            {
                writer.WriteKeyword("INITONLY");
                writer.WriteString(" ");
            }

            if(fixedAttribute != null)
            {
                writer.WriteKeyword("DIM");
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
                writer.WriteIdentifier(name);

                if(isStatic && isLiteral)
                {
                    writer.WriteString(" := ");
                    this.WriteConstantValue(reflection, writer);
                }
                writer.WriteString(" ");
                writer.WriteKeyword("AS");
                writer.WriteString(" ");
                WriteReturnValue(reflection, writer);

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
                //case "public":
                //    writer.WriteKeyword("PUBLIC");
                //    break;
                case "family":
                    writer.WriteKeyword("PROTECTED");
                    break;
                case "family or assembly":
                    writer.WriteKeyword("PROTECTED INTERNAL");
                    break;
                case "assembly":
                    writer.WriteKeyword("INTERNAL");
                    break;
                case "private":
                    writer.WriteKeyword("PRIVATE");
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
                XPathNavigator type = attribute.SelectSingleNode(attributeTypeExpression);

                // !EFW - Ignore FixedBufferAttribute and ParamArrayAttribute too
                if(type.GetAttribute("api", String.Empty) == "T:System.Runtime.CompilerServices.ExtensionAttribute" ||
                  type.GetAttribute("api", String.Empty) == "T:System.Runtime.CompilerServices.FixedBufferAttribute" ||
                  type.GetAttribute("api", String.Empty) == "T:System.ParamArrayAttribute")
                    continue;

                if (type.GetAttribute("api", String.Empty) == "T:Vulcan.Internal.ClipperCallingConventionAttribute")
                    continue;

                if (!String.IsNullOrEmpty(indent))
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
                        writer.WriteString(" := ");
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
        private void WriteInteropAttributes(XPathNavigator reflection, SyntaxWriter writer, string indent = null)
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
                        writer.WriteString("LayoutKind.Explicit");
                        break;

                    case "sequential":
                        writer.WriteString("LayoutKind.Sequential");
                        break;

                    default:
                        writer.WriteString("LayoutKind.Auto");
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
                    writer.WriteString("Pack := ");
                    writer.WriteString(((int)pack).ToString(CultureInfo.InvariantCulture));
                }

                switch(format)
                {
                    case "ansi":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CharSet := CharSet.Ansi");
                        break;

                    case "auto":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CharSet := CharSet.Auto");
                        break;

                    case "unicode":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CharSet := CharSet.Unicode");
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
                writer.WriteString("(\"");
                writer.WriteString(module);
                writer.WriteString("\"");

                if(!String.IsNullOrEmpty(entryPoint))
                {
                    WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                    writer.WriteString("EntryPoint := \"");
                    writer.WriteString(entryPoint);
                    writer.WriteString("\"");
                }

                switch(callingConv)
                {
                    case "cdecl":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CallingConvention := CallingConvention.Cdecl");
                        break;

                    case "fastcall":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CallingConvention := CallingConvention.FastCall");
                        break;

                    case "stdcall":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CallingConvention := CallingConvention.StdCall");
                        break;

                    case "thiscall":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CallingConvention := CallingConvention.ThisCall");
                        break;
                }

                switch(charset)
                {
                    case "ansi":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CharSet := CharSet.Ansi");
                        break;

                    case "auto":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CharSet := CharSet.Auto");
                        break;

                    case "unicode":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CharSet := CharSet.Unicode");
                        break;
                }

                if(!String.IsNullOrEmpty(bestFitMapping) && !Convert.ToBoolean(bestFitMapping, CultureInfo.InvariantCulture))
                {
                    WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                    writer.WriteString("BestFitMapping := FALSE");
                }

                if(exactSpelling)
                {
                    WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                    writer.WriteString("ExactSpelling := TRUE");
                }

                if(throwOnUnmappableChar)
                {
                    WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                    writer.WriteString("ThrowOnUnmappableChar := TRUE");
                }

                if(!preserveSig)
                {
                    WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                    writer.WriteString("PreserveSig := FALSE");
                }

                if(setLastError)
                {
                    WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                    writer.WriteString("SetLastError := TRUE");
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
                    writer.WriteKeyword("NULL");
                    break;

                case "typeValue":
                    writer.WriteKeyword("TYPEOF");
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
                                "TRUE" : "FALSE");
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

            if(implements.Count != 0)
            {
                writer.WriteString(" ");
                writer.WriteKeyword("IMPLEMENTS");
                writer.WriteString(" ");

                while (implements.MoveNext())
                {
                    XPathNavigator implement = implements.Current;
                    WriteTypeReference(implement, writer);

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

                if(hasBaseClass)
                {
                    writer.WriteString(" ");
                    writer.WriteKeyword("INHERIT");
                    writer.WriteString(" ");
                    WriteTypeReference(baseClass, writer);

                    if(hasImplementedInterfaces)
                        WriteWithLineBreakIfNeeded(writer, ", ", "\t");
                }
                if (hasImplementedInterfaces)
                {
                    writer.WriteString(" ");
                    writer.WriteKeyword("IMPLEMENTS");
                    writer.WriteString(" ");
                }

                while (implements.MoveNext())
                {
                    XPathNavigator implement = implements.Current;
                    WriteTypeReference(implement, writer);

                    if(implements.CurrentPosition < implements.Count)
                        WriteWithLineBreakIfNeeded(writer, ", ", "\t");
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
                        writer.WriteKeyword("IN");
                        writer.WriteString(" ");
                    }
                    if(covariant)
                    {
                        writer.WriteKeyword("OUT");
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

                    writer.WriteKeyword("WHERE");
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
                    writer.WriteKeyword("STRUCT");
                    previous = true;
                }

                if(reference)
                {
                    if(previous)
                        writer.WriteString(", ");
                    writer.WriteKeyword("CLASS");
                    previous = true;
                }

                if(constructor)
                {
                    if(previous)
                        writer.WriteString(", ");
                    writer.WriteKeyword("NEW");
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

                // !EFW - Optional indicated by OptionalAttribute?
                if(isOptional && argument == null)
                {
                    WriteAttribute("T:System.Runtime.InteropServices.OptionalAttribute", false, writer);
                    writer.WriteString(" ");
                }

                // !EFW - Write out parameter attributes
                WriteAttributes(parameter, writer, null, true);

                if(isExtension && parameters.CurrentPosition == 1)
                {
                    writer.WriteKeyword("SELF");
                    writer.WriteString(" ");
                }

                writer.WriteParameter(name);
                writer.WriteString(" ");
                if(isParamArray) 
                {
                    writer.WriteKeyword("PARAMS");
                }
                else if(isRef)
                {
                    if(isOut)
                    {
                        writer.WriteKeyword("OUT");
                    }
                    else
                    {
                        writer.WriteKeyword("REF");
                    }
                }
                else
                {
                    writer.WriteKeyword("AS");
                }
                writer.WriteString(" ");
                WriteTypeReference(type, writer);

                // !EFW - Write optional value if present
                if(argument != null)
                {
                    writer.WriteString(" := ");
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
                writer.WriteKeyword("VOID");
            else
                WriteTypeReference(type, writer);
        }

        // References

        private static void WriteMemberReference(XPathNavigator member, SyntaxWriter writer)
        {
            string api = member.GetAttribute("api", String.Empty);
            writer.WriteReferenceLink(api);
        }
        /// <inheritdoc />
        protected override void WriteTypeReference(XPathNavigator reference, SyntaxWriter writer)
        {
            switch (reference.LocalName)
            {
                case "arrayOf":
                    int rank = Convert.ToInt32(reference.GetAttribute("rank", String.Empty),
                        CultureInfo.InvariantCulture);

                    XPathNavigator element = reference.SelectSingleNode(typeExpression);
                    WriteTypeReference(element, writer);
                    writer.WriteString("[");

                    for (int i = 1; i < rank; i++)
                        writer.WriteString(",");

                    writer.WriteString("]");
                    break;

                case "pointerTo":
                    XPathNavigator pointee = reference.SelectSingleNode(typeExpression);
                    string idPointee = pointee.LocalName;
                    string namePointee = pointee.GetAttribute("api", String.Empty);
                    if (idPointee == "type" && namePointee == "T:System.Void")
                    {
                        writer.WriteKeyword("PTR");
                    }
                    else
                    {
                        WriteTypeReference(pointee, writer);
                        writer.WriteString("*");
                    }
                    break;

                case "referenceTo":
                    XPathNavigator referee = reference.SelectSingleNode(typeExpression);
                    WriteTypeReference(referee, writer);
                    break;

                case "type":
                    string id = reference.GetAttribute("api", String.Empty);
                    WriteNormalTypeReference(id, writer);
                    XPathNodeIterator typeModifiers = reference.Select(typeModifiersExpression);

                    while (typeModifiers.MoveNext())
                        WriteTypeReference(typeModifiers.Current, writer);

                    break;

                case "template":
                    string name = reference.GetAttribute("name", String.Empty);
                    writer.WriteString(name);
                    XPathNodeIterator modifiers = reference.Select(typeModifiersExpression);

                    while (modifiers.MoveNext())
                        WriteTypeReference(modifiers.Current, writer);

                    break;

                case "specialization":
                    writer.WriteString("<");
                    XPathNodeIterator arguments = reference.Select(specializationArgumentsExpression);

                    while (arguments.MoveNext())
                    {
                        if (arguments.CurrentPosition > 1)
                            writer.WriteString(", ");

                        WriteTypeReference(arguments.Current, writer);
                    }

                    writer.WriteString(">");
                    break;
            }
        }

    }
}
