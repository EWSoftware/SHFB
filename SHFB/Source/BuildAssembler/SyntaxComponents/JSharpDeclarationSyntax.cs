// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 01/30/2012 - EFW - Fixed WriteValue() so that it outputs numeric attribute values.
// 02/09/2012 - EFW - Added support for optional parameters and property getter/setter attributes.
// 02/14/2012 - EFW - Made the unsafe code checks consistent across all syntax generators
// 12/23/2012 - EFW - Changed base class to SyntaxGeneratorTemplate as it was identical with the exception of
// the static WriteVisibility() method.
// 11/29/2013 - EFW - Added support for metadata based interop attributes
// 12/20/2013 - EFW - Updated the syntax generator to be discoverable via MEF

using System;
using System.Globalization;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler.SyntaxGenerator;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This class generates declaration syntax sections for J#
    /// </summary>
    public sealed class JSharpDeclarationSyntaxGenerator : SyntaxGeneratorTemplate
    {
        #region Syntax generator factory for MEF
        //=====================================================================

        private const string LanguageName = "JSharp", StyleIdName = "jsh";

        /// <summary>
        /// This is used to create a new instance of the syntax generator
        /// </summary>
        [SyntaxGeneratorExport("J#", LanguageName, StyleIdName, AlternateIds = "JSharp, jsh", SortOrder = 60,
          Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
          Description = "Generates JSharp declaration syntax sections")]
        public sealed class Factory : ISyntaxGeneratorFactory
        {
            /// <inheritdoc />
            public SyntaxGeneratorCore Create()
            {
                return new JSharpDeclarationSyntaxGenerator { Language = LanguageName, StyleId = StyleIdName };
            }
        }
        #endregion

        /// <inheritdoc />
        public override void WriteNamespaceSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string name = reflection.Evaluate(apiNameExpression).ToString();

            writer.WriteKeyword("package");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
        }

        /// <inheritdoc />
        public override void WriteClassSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(IsUnsupportedGeneric(reflection, writer))
                return;

            string name = reflection.Evaluate(apiNameExpression).ToString();
            bool isAbstract = (bool)reflection.Evaluate(apiIsAbstractTypeExpression);
            bool isSealed = (bool)reflection.Evaluate(apiIsSealedTypeExpression);
            bool isSerializable = (bool)reflection.Evaluate(apiIsSerializableTypeExpression);

            if(isSerializable)
                WriteAttribute("T:System.SerializableAttribute", writer);
            WriteAttributes(reflection, writer);
            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            if(isSealed)
            {
                writer.WriteKeyword("final");
                writer.WriteString(" ");
            }
            else if(isAbstract)
            {
                writer.WriteKeyword("abstract");
                writer.WriteString(" ");
            }
            writer.WriteKeyword("class");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);

            XPathNavigator baseClass = reflection.SelectSingleNode(apiBaseClassExpression);
            if((baseClass != null) && !((bool)baseClass.Evaluate(typeIsObjectExpression)))
            {
                writer.WriteString(" ");
                writer.WriteKeyword("extends");
                writer.WriteString(" ");
                WriteTypeReference(baseClass, writer);
            }

            WriteImplementedInterfaces(reflection, writer);

        }

        /// <inheritdoc />
        public override void WriteStructureSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            if(IsUnsupportedGeneric(reflection, writer))
                return;

            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isSerializable = (bool)reflection.Evaluate(apiIsSerializableTypeExpression);

            if(isSerializable)
                WriteAttribute("T:System.SerializableAttribute", writer);
            WriteAttributes(reflection, writer);
            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            writer.WriteKeyword("final");
            writer.WriteString(" ");
            writer.WriteKeyword("class");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);

            writer.WriteString(" ");
            writer.WriteKeyword("extends");
            writer.WriteString(" ");
            writer.WriteReferenceLink("T:System.ValueType");

            WriteImplementedInterfaces(reflection, writer);
        }

        /// <inheritdoc />
        public override void WriteInterfaceSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {

            if(IsUnsupportedGeneric(reflection, writer))
                return;

            string name = (string)reflection.Evaluate(apiNameExpression);

            WriteAttributes(reflection, writer);
            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            writer.WriteKeyword("interface");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            WriteImplementedInterfaces("extends", reflection, writer);
        }

        /// <inheritdoc />
        public override void WriteDelegateSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(IsUnsupportedUnsafe(reflection, writer))
                return;
            if(IsUnsupportedGeneric(reflection, writer))
                return;

            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isSerializable = (bool)reflection.Evaluate(apiIsSerializableTypeExpression);

            writer.WriteString("/** @delegate */");
            writer.WriteLine();

            if(isSerializable)
                WriteAttribute("T:System.SerializableAttribute", writer);
            WriteAttributes(reflection, writer);
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
                WriteAttribute("T:System.SerializableAttribute", writer);
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
            if(IsUnsupportedUnsafe(reflection, writer))
                return;

            string name = (string)reflection.Evaluate(apiContainingTypeNameExpression);
            bool isStatic = (bool)reflection.Evaluate(apiIsStaticExpression);

            if(isStatic)
            {
                // no static constructors in Java
                writer.WriteMessage("UnsupportedStaticConstructor_" + Language);
                return;
            }

            WriteAttributes(reflection, writer);
            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            WriteMethodParameters(reflection, writer);

        }

        /// <inheritdoc />
        public override void WriteMethodSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            bool isSpecialName = (bool)reflection.Evaluate(apiIsSpecialExpression);

            if(isSpecialName)
            {
                writer.WriteMessage("UnsupportedOperator_" + Language);
            }
            else
            {
                WriteNormalMethodSyntax(reflection, writer);
            }
        }

        /// <inheritdoc />
        public override void WriteNormalMethodSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(IsUnsupportedUnsafe(reflection, writer))
                return;
            if(IsUnsupportedGeneric(reflection, writer))
                return;
            if(IsUnsupportedExplicit(reflection, writer))
                return;

            string name = (string)reflection.Evaluate(apiNameExpression);

            WriteAttributes(reflection, writer);
            WriteProcedureModifiers(reflection, writer);
            WriteReturnValue(reflection, writer);
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            WriteMethodParameters(reflection, writer);
        }

        /// <inheritdoc />
        public override void WritePropertySyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(IsUnsupportedUnsafe(reflection, writer))
                return;

            if(IsUnsupportedExplicit(reflection, writer))
                return;

            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isGettable = (bool)reflection.Evaluate(apiIsReadPropertyExpression);
            bool isSettable = (bool)reflection.Evaluate(apiIsWritePropertyExpression);

            if(isGettable)
            {
                writer.WriteString("/** @property */");
                writer.WriteLine();

                // !EFW - Added support for getter/setter attributes
                XPathNavigator getter = reflection.SelectSingleNode(apiGetterExpression);

                if(getter != null && getter.HasChildren)
                    this.WriteAttributes(getter, writer);

                // write getter method
                WriteAttributes(reflection, writer);
                WriteProcedureModifiers(reflection, writer);
                WriteReturnValue(reflection, writer);
                writer.WriteString(" ");
                writer.WriteIdentifier("get_" + name);
                WriteMethodParameters(reflection, writer);
                writer.WriteLine();
            }

            if(isSettable)
            {
                writer.WriteString("/** @property */");
                writer.WriteLine();

                // !EFW - Added support for getter/setter attributes
                XPathNavigator setter = reflection.SelectSingleNode(apiSetterExpression);

                if(setter != null && setter.HasChildren)
                    this.WriteAttributes(setter, writer);

                // write setter method
                WriteAttributes(reflection, writer);
                WriteProcedureModifiers(reflection, writer);
                writer.WriteKeyword("void");
                writer.WriteString(" ");
                writer.WriteIdentifier("set_" + name);

                // parameters
                writer.WriteString("(");
                WriteReturnValue(reflection, writer);
                writer.WriteString(" ");
                writer.WriteParameter("value");
                writer.WriteString(")");

                // end parameters
                writer.WriteLine();
            }
        }

        /// <inheritdoc />
        public override void WriteEventSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(IsUnsupportedUnsafe(reflection, writer))
                return;

            string name = (string)reflection.Evaluate(apiNameExpression);
            XPathNavigator handler = reflection.SelectSingleNode(apiHandlerOfEventExpression);

            writer.WriteString("/** @event */");
            writer.WriteLine();
            // add_ method declaration
            WriteAttributes(reflection, writer);
            WriteProcedureModifiers(reflection, writer);
            WriteReturnValue(reflection, writer);
            writer.WriteString(" ");
            writer.WriteIdentifier("add_" + name);
            writer.WriteString(" (");
            WriteTypeReference(handler, writer);
            writer.WriteString(" ");
            writer.WriteParameter("value");
            writer.WriteString(")");
            writer.WriteLine();

            writer.WriteString("/** @event */");
            writer.WriteLine();
            // remove_ method declaration
            WriteAttributes(reflection, writer);
            WriteProcedureModifiers(reflection, writer);
            WriteReturnValue(reflection, writer);
            writer.WriteString(" ");
            writer.WriteIdentifier("remove_" + name);
            writer.WriteString(" (");
            WriteTypeReference(handler, writer);
            writer.WriteString(" ");
            writer.WriteParameter("value");
            writer.WriteString(")");
            writer.WriteLine();

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
                    else if(isFinal)
                    {
                        writer.WriteKeyword("final");
                        writer.WriteString(" ");
                    }
                }
            }
        }

        /// <inheritdoc />
        public override void WriteFieldSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            // !EFW - Added unsafe check
            if(IsUnsupportedUnsafe(reflection, writer))
                return;

            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isStatic = (bool)reflection.Evaluate(apiIsStaticExpression);
            bool isLiteral = (bool)reflection.Evaluate(apiIsLiteralFieldExpression);
            bool isInitOnly = (bool)reflection.Evaluate(apiIsInitOnlyFieldExpression);
            bool isSerialized = (bool)reflection.Evaluate(apiIsSerializedFieldExpression);

            if(!isSerialized)
                WriteAttribute("T:System.NonSerializedAttribute", writer);
            WriteAttributes(reflection, writer);
            WriteVisibility(reflection, writer);
            writer.WriteString(" ");
            // Java doesn't support literals as distinct from static initonly
            if(isStatic)
            {
                writer.WriteKeyword("static");
                writer.WriteString(" ");
            }
            if(isLiteral || isInitOnly)
            {
                writer.WriteKeyword("final");
                writer.WriteString(" ");
            }
            WriteReturnValue(reflection, writer);
            writer.WriteString(" ");
            writer.WriteIdentifier(name);

        }

        // Visibility

        private static void WriteVisibility(XPathNavigator reflection, SyntaxWriter writer)
        {
            string visibility = reflection.Evaluate(apiVisibilityExpression).ToString();

            switch(visibility)
            {
                case "public":
                    writer.WriteKeyword("public");
                    break;
                case "family":
                    // in Java, protected = family or assembly
                    writer.WriteKeyword("protected");
                    break;
                case "family or assembly":
                    writer.WriteKeyword("protected");
                    break;
                case "assembly":
                    // no assembly-only access in Java
                    break;
                case "private":
                    writer.WriteKeyword("private");
                    break;
            }
        }

        // Attributes

        private static void WriteAttribute(string reference, SyntaxWriter writer)
        {
            WriteAttribute(reference, writer, true);
        }

        private static void WriteAttribute(string reference, SyntaxWriter writer, bool newline)
        {
            writer.WriteString("/** @attribute ");
            writer.WriteReferenceLink(reference);
            writer.WriteString(" */ ");

            if(newline)
                writer.WriteLine();
        }

        private void WriteAttributes(XPathNavigator reflection, SyntaxWriter writer)
        {
            // Handle interop attributes first as they are output in metadata
            WriteInteropAttributes(reflection, writer);

            // Add the standard attributes
            XPathNodeIterator attributes = (XPathNodeIterator)reflection.Evaluate(apiAttributesExpression);

            foreach(XPathNavigator attribute in attributes)
            {
                XPathNavigator type = attribute.SelectSingleNode(attributeTypeExpression);

                // !EFW - Ignore FixedBufferAttribute
                if(type.GetAttribute("api", String.Empty) == "T:System.Runtime.CompilerServices.FixedBufferAttribute")
                    continue;

                writer.WriteString("/** @attribute ");
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
                            WriteWithLineBreakIfNeeded(writer, ", ", "\t");

                        WriteValue(argument, writer);
                    }

                    if(arguments.Count > 0 && assignments.Count > 0)
                        writer.WriteString(", ");

                    while(assignments.MoveNext())
                    {
                        XPathNavigator assignment = assignments.Current;

                        if(assignments.CurrentPosition > 1)
                            WriteWithLineBreakIfNeeded(writer, ", ", "\t");

                        writer.WriteString((string)assignment.Evaluate(assignmentNameExpression));
                        writer.WriteString(" = ");
                        WriteValue(assignment, writer);
                    }

                    writer.WriteString(")");
                }

                writer.WriteString(" */");
                writer.WriteLine();
            }
        }

        // EFW - Added support for interop attributes stored in metadata
        private static void WriteInteropAttributes(XPathNavigator reflection, SyntaxWriter writer)
        {
            if((bool)reflection.Evaluate(apiComImportTypeExpression))
                WriteAttribute("T:System.Runtime.InteropServices.ComImportAttribute", writer, true);

            string layout = (string)reflection.Evaluate(apiStructLayoutTypeExpression);

            if(!String.IsNullOrEmpty(layout))
            {
                double size = (double)reflection.Evaluate(apiStructLayoutSizeTypeExpression),
                    pack = (double)reflection.Evaluate(apiStructLayoutPackTypeExpression);
                string format = (string)reflection.Evaluate(apiStructLayoutFormatTypeExpression);

                writer.WriteString("/** @attribute ");
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
                    WriteWithLineBreakIfNeeded(writer, ", ", "\t");
                    writer.WriteString("Size = ");
                    writer.WriteString(((int)size).ToString(CultureInfo.InvariantCulture));
                }

                if(!Double.IsNaN(pack) && pack != 0)
                {
                    WriteWithLineBreakIfNeeded(writer, ", ", "\t");
                    writer.WriteString("Pack = ");
                    writer.WriteString(((int)pack).ToString(CultureInfo.InvariantCulture));
                }

                switch(format)
                {
                    case "ansi":
                        WriteWithLineBreakIfNeeded(writer, ", ", "\t");
                        writer.WriteString("CharSet = CharSet.Ansi");
                        break;

                    case "auto":
                        WriteWithLineBreakIfNeeded(writer, ", ", "\t");
                        writer.WriteString("CharSet = CharSet.Auto");
                        break;

                    case "unicode":
                        WriteWithLineBreakIfNeeded(writer, ", ", "\t");
                        writer.WriteString("CharSet = CharSet.Unicode");
                        break;
                }

                writer.WriteString(")");
                writer.WriteString(" */");
                writer.WriteLine();
            }

            double fieldOffset = (double)reflection.Evaluate(apiFieldOffsetFieldExpression);

            if(!Double.IsNaN(fieldOffset))
            {
                writer.WriteString("/** @attribute ");
                WriteNormalTypeReference("T:System.Runtime.InteropServices.FieldOffsetAttribute", writer);
                writer.WriteString("(");
                writer.WriteString(((int)fieldOffset).ToString(CultureInfo.InvariantCulture));
                writer.WriteString(")");
                writer.WriteString(" */");
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

                writer.WriteString("/** @attribute ");
                WriteNormalTypeReference("T:System.Runtime.InteropServices.DllImportAttribute", writer);
                writer.WriteString("(\"");
                writer.WriteString(module);
                writer.WriteString("\"");

                if(!String.IsNullOrEmpty(entryPoint))
                {
                    WriteWithLineBreakIfNeeded(writer, ", ", "\t");
                    writer.WriteString("EntryPoint = \"");
                    writer.WriteString(entryPoint);
                    writer.WriteString("\"");
                }

                switch(callingConv)
                {
                    case "cdecl":
                        WriteWithLineBreakIfNeeded(writer, ", ", "\t");
                        writer.WriteString("CallingConvention = CallingConvention.Cdecl");
                        break;

                    case "fastcall":
                        WriteWithLineBreakIfNeeded(writer, ", ", "\t");
                        writer.WriteString("CallingConvention = CallingConvention.FastCall");
                        break;

                    case "stdcall":
                        WriteWithLineBreakIfNeeded(writer, ", ", "\t");
                        writer.WriteString("CallingConvention = CallingConvention.StdCall");
                        break;

                    case "thiscall":
                        WriteWithLineBreakIfNeeded(writer, ", ", "\t");
                        writer.WriteString("CallingConvention = CallingConvention.ThisCall");
                        break;
                }

                switch(charset)
                {
                    case "ansi":
                        WriteWithLineBreakIfNeeded(writer, ", ", "\t");
                        writer.WriteString("CharSet = CharSet.Ansi");
                        break;

                    case "auto":
                        WriteWithLineBreakIfNeeded(writer, ", ", "\t");
                        writer.WriteString("CharSet = CharSet.Auto");
                        break;

                    case "unicode":
                        WriteWithLineBreakIfNeeded(writer, ", ", "\t");
                        writer.WriteString("CharSet = CharSet.Unicode");
                        break;
                }

                if(!String.IsNullOrEmpty(bestFitMapping) && !Convert.ToBoolean(bestFitMapping, CultureInfo.InvariantCulture))
                {
                    WriteWithLineBreakIfNeeded(writer, ", ", "\t");
                    writer.WriteString("BestFitMapping = false");
                }

                if(exactSpelling)
                {
                    WriteWithLineBreakIfNeeded(writer, ", ", "\t");
                    writer.WriteString("ExactSpelling = true");
                }

                if(throwOnUnmappableChar)
                {
                    WriteWithLineBreakIfNeeded(writer, ", ", "\t");
                    writer.WriteString("ThrowOnUnmappableChar = true");
                }

                if(!preserveSig)
                {
                    WriteWithLineBreakIfNeeded(writer, ", ", "\t");
                    writer.WriteString("PreserveSig = false");
                }

                if(setLastError)
                {
                    WriteWithLineBreakIfNeeded(writer, ", ", "\t");
                    writer.WriteString("SetLastError = true");
                }

                writer.WriteString(")");
                writer.WriteString(" */");
                writer.WriteLine();
            }
            else
                if(preserveSig)
                    WriteAttribute("T:System.Runtime.InteropServices.PreserveSigAttribute", writer, true);
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
                    // this isn't really supported in J#; there is no compile-time way to get a type representation
                    // writer.WriteKeyword("typeof");
                    // writer.WriteString("(");
                    WriteTypeReference(value.SelectSingleNode(typeExpression), writer);
                    // writer.WriteString(")");
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
            WriteImplementedInterfaces("implements", reflection, writer);
        }

        private void WriteImplementedInterfaces(string keyword, XPathNavigator reflection, SyntaxWriter writer)
        {
            XPathNodeIterator implements = reflection.Select(apiImplementedInterfacesExpression);

            if(implements.Count != 0)
            {
                writer.WriteString(" ");
                writer.WriteKeyword(keyword);
                writer.WriteString(" ");

                while(implements.MoveNext())
                {
                    XPathNavigator implement = implements.Current;
                    WriteTypeReference(implement, writer);

                    if(implements.CurrentPosition < implements.Count)
                        WriteWithLineBreakIfNeeded(writer, ", ", "\t");
                }
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
                WriteParameters(parameters, writer);
            }
            writer.WriteString(")");

        }

        private void WriteParameters(XPathNodeIterator parameters, SyntaxWriter writer)
        {
            while(parameters.MoveNext())
            {
                XPathNavigator parameter = parameters.Current;

                // !EFW - Added support for optional parameter values
                XPathNavigator argument = (XPathNavigator)parameter.SelectSingleNode(parameterArgumentExpression);
                bool isOptional = (bool)parameter.Evaluate(parameterIsOptionalExpression);

                string name = (string)parameter.Evaluate(parameterNameExpression);
                XPathNavigator type = parameter.SelectSingleNode(parameterTypeExpression);
                bool isIn = (bool)parameter.Evaluate(parameterIsInExpression);
                bool isOut = (bool)parameter.Evaluate(parameterIsOutExpression);
                bool isRef = (bool)parameter.Evaluate(parameterIsRefExpression);

                writer.WriteString("\t");

                // !EFW - Optional indicated by OptionalAttribute?
                if(isOptional && argument == null)
                    WriteAttribute("T:System.Runtime.InteropServices.OptionalAttribute", writer, false);

                if(isIn)
                    WriteAttribute("T:System.Runtime.InteropServices.InAttribute", writer, false);

                if(isOut)
                    WriteAttribute("T:System.Runtime.InteropServices.OutAttribute", writer, false);

                if(isRef)
                    writer.WriteString("/** @ref */");

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
                writer.WriteKeyword("void");
            else
                WriteTypeReference(type, writer);
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

        private static void WriteNormalTypeReference(string reference, SyntaxWriter writer)
        {
            switch(reference)
            {
                case "T:System.Void":
                    writer.WriteReferenceLink(reference, "void");
                    break;
                case "T:System.Boolean":
                    writer.WriteReferenceLink(reference, "boolean");
                    break;
                case "T:System.Byte":
                    writer.WriteReferenceLink(reference, "byte");
                    break;
                case "T:System.Char":
                    writer.WriteReferenceLink(reference, "char");
                    break;
                case "T:System.Int16":
                    writer.WriteReferenceLink(reference, "short");
                    break;
                case "T:System.Int32":
                    writer.WriteReferenceLink(reference, "int");
                    break;
                case "T:System.Int64":
                    writer.WriteReferenceLink(reference, "long");
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
    }
}
