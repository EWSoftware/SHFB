// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 01/30/2012 - EFW - Fixed WriteValue() so that it outputs numeric attribute values.
// 02/09/2012 - EFW - Added support for optional parameters and property getter/setter attributes.
// 02/14/2012 - EFW - Made the unsafe code checks consistent across all syntax generators
// 11/29/2013 - EFW - Added support for metadata based interop attributes
// 12/20/2013 - EFW - Updated the syntax generator to be discoverable via MEF
// 08/01/2014 - EFW - Added support for resource item files containing the localized titles, messages, etc.
// 11/20/2014 - EFW - Added support for writing out method parameter attributes
// 03/14/2021 - EFW - Added support for defaultValue element

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml.XPath;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler.SyntaxGenerator;

namespace Sandcastle.Tools.SyntaxGenerators
{
    /// <summary>
    /// This class generates declaration syntax sections for F#
    /// </summary>
    public sealed class FSharpDeclarationSyntaxGenerator : SyntaxGeneratorTemplate
    {
        #region Syntax generator factory for MEF
        //=====================================================================

        private const string LanguageName = "FSharp", StyleIdName = "fs";

        /// <summary>
        /// This is used to create a new instance of the syntax generator
        /// </summary>
        [SyntaxGeneratorExport("F#", LanguageName, StyleIdName, AlternateIds = "FSharp, fs, fsscript",
          SortOrder = 50, Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
          Description = "Generates F# declaration syntax sections")]
        public sealed class Factory : ISyntaxGeneratorFactory
        {
            /// <inheritdoc />
            public string ResourceItemFileLocation => Path.Combine(ComponentUtilities.AssemblyFolder(
                Assembly.GetExecutingAssembly()), "SyntaxContent");

            /// <inheritdoc />
            public SyntaxGeneratorCore Create()
            {
                return new FSharpDeclarationSyntaxGenerator { Language = LanguageName, StyleId = StyleIdName };
            }
        }
        #endregion

        // namespace: done

        /// <inheritdoc />
        public override void WriteNamespaceSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            string name = reflection.Evaluate(apiNameExpression).ToString();

            writer.WriteKeyword("namespace");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
        }

        /// <inheritdoc />
        public override void WriteClassSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            WriteDotNetObject(reflection, writer, "class");
        }

        /// <inheritdoc />
        public override void WriteStructureSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            WriteDotNetObject(reflection, writer, "struct");
        }

        /// <inheritdoc />
        public override void WriteInterfaceSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            WriteDotNetObject(reflection, writer, "interface");
        }

        /// <inheritdoc />
        public override void WriteDelegateSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            // !EFW - Added unsafe check
            if(IsUnsupportedUnsafe(reflection, writer))
                return;

            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isSerializable = (bool)reflection.Evaluate(apiIsSerializableTypeExpression);

            if(isSerializable)
                WriteAttribute("T:System.SerializableAttribute", true, writer);

            WriteAttributes(reflection, writer);

            writer.WriteKeyword("type");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            writer.WriteString(" = ");
            writer.WriteLine();
            writer.WriteString("    ");
            writer.WriteKeyword("delegate");
            writer.WriteString(" ");
            writer.WriteKeyword("of");
            writer.WriteString(" ");

            WriteParameters(reflection, writer);

            writer.WriteKeyword("->");
            writer.WriteString(" ");
            WriteReturnValue(reflection, writer);

        }

        /// <inheritdoc />
        public override void WriteEnumerationSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isSerializable = (bool)reflection.Evaluate(apiIsSerializableTypeExpression);

            if(isSerializable)
                WriteAttribute("T:System.SerializableAttribute", true, writer);

            WriteAttributes(reflection, writer);
            writer.WriteKeyword("type");
            writer.WriteString(" ");
            WriteVisibility(reflection, writer);
            writer.WriteIdentifier(name);
        }

        /// <inheritdoc />
        public override void WriteConstructorSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            // !EFW - Added unsafe check
            if(IsUnsupportedUnsafe(reflection, writer))
                return;

            string name = (string)reflection.Evaluate(apiContainingTypeNameExpression);

            WriteAttributes(reflection, writer);

            writer.WriteKeyword("new");
            writer.WriteString(" : ");
            WriteParameters(reflection, writer);
            writer.WriteKeyword("->");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);

        }

        /// <inheritdoc />
        public override void WriteNormalMethodSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            // !EFW - Added unsafe check
            if(IsUnsupportedUnsafe(reflection, writer))
                return;

            if(IsUnsupportedVarargs(reflection, writer))
                return;

            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isStatic = (bool)reflection.Evaluate(apiIsStaticExpression);
            bool isVirtual = (bool)reflection.Evaluate(apiIsVirtualExpression) && !(bool)reflection.Evaluate(apiIsAbstractProcedureExpression);
            int iterations = isVirtual ? 2 : 1;

            for(int i = 0; i < iterations; i++)
            {

                WriteAttributes(reflection, writer);

                WriteVisibility(reflection, writer);

                if(isStatic)
                {
                    writer.WriteKeyword("static");
                    writer.WriteString(" ");
                }

                if(isVirtual)
                {
                    if(i == 0)
                    {
                        writer.WriteKeyword("abstract");
                        writer.WriteString(" ");
                    }
                    else
                    {
                        writer.WriteKeyword("override");
                        writer.WriteString(" ");
                    }
                }
                else
                {
                    WriteMemberKeyword(reflection, writer);
                }

                writer.WriteIdentifier(name);
                writer.WriteString(" : ");

                WriteParameters(reflection, writer);

                writer.WriteKeyword("->");
                writer.WriteString(" ");

                WriteReturnValue(reflection, writer);

                writer.WriteString(" ");

                WriteGenericTemplateConstraints(reflection, writer);

                if(i == 0)
                    writer.WriteLine();
            }
        }

        /// <inheritdoc />
        public override void WriteOperatorSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            string name = (string)reflection.Evaluate(apiNameExpression);
            string identifier = null;

            bool isStatic = (bool)reflection.Evaluate(apiIsStaticExpression);

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
                        identifier = "not";
                        break;

                    case "True":
                        identifier = "true";
                        break;

                    case "False":
                        identifier = "false";
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

                    case "Modulus":
                        identifier = "%";
                        break;

                    // binary logical operators
                    case "BitwiseAnd":
                        identifier = "&&&";
                        break;

                    case "BitwiseOr":
                        identifier = "|||";
                        break;

                    case "ExclusiveOr":
                        identifier = "^^^";
                        break;

                    // bit-array operators
                    case "OnesComplement":
                        identifier = null; // No F# equiv.
                        break;

                    case "LeftShift":
                        identifier = "<<<";
                        break;

                    case "RightShift":
                        identifier = ">>>";
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
                writer.WriteMessage("UnsupportedOperator_" + Language);
            else
            {
                if(isStatic)
                {
                    writer.WriteKeyword("static");
                    writer.WriteString(" ");
                }

                writer.WriteKeyword("let");
                writer.WriteString(" ");
                writer.WriteKeyword("inline");
                writer.WriteKeyword(" ");

                writer.WriteString("(");
                writer.WriteIdentifier(identifier);
                writer.WriteString(")");

                WriteParameters(reflection, writer);
                writer.WriteString(" : ");
                WriteReturnValue(reflection, writer);
            }
        }

        /// <inheritdoc />
        public override void WriteCastSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.WriteMessage("UnsupportedCast_" + Language);
        }

        /// <inheritdoc />
        public override void WritePropertySyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            // !EFW - Added unsafe check
            if(IsUnsupportedUnsafe(reflection, writer))
                return;

            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isGettable = (bool)reflection.Evaluate(apiIsReadPropertyExpression);
            bool isSettable = (bool)reflection.Evaluate(apiIsWritePropertyExpression);

            bool isStatic = (bool)reflection.Evaluate(apiIsStaticExpression);
            bool isVirtual = (bool)reflection.Evaluate(apiIsVirtualExpression) && !(bool)reflection.Evaluate(apiIsAbstractProcedureExpression);
            int iterations = isVirtual ? 2 : 1;

            for(int i = 0; i < iterations; i++)
            {
                WriteAttributes(reflection, writer);
                WriteVisibility(reflection, writer);

                if(isStatic)
                {
                    writer.WriteKeyword("static");
                    writer.WriteString(" ");
                }

                if(isVirtual)
                    if(i == 0)
                    {
                        writer.WriteKeyword("abstract");
                        writer.WriteString(" ");
                    }
                    else
                    {
                        writer.WriteKeyword("override");
                        writer.WriteString(" ");
                    }
                else
                {
                    WriteMemberKeyword(reflection, writer);
                }

                writer.WriteIdentifier(name);
                writer.WriteString(" : ");
                WriteReturnValue(reflection, writer);

                writer.WriteString(" ");
                writer.WriteKeyword("with");
                writer.WriteString(" ");

                if(isGettable)
                {
                    // !EFW - Added support for getter/setter attributes
                    XPathNavigator getter = reflection.SelectSingleNode(apiGetterExpression);

                    if(getter != null && getter.HasChildren)
                    {
                        writer.WriteLine();
                        writer.WriteString("\t");
                        this.WriteAttributes(getter, writer, "\t");
                        writer.WriteString("\t");
                    }

                    string getVisibility = (string)reflection.Evaluate(apiGetVisibilityExpression);

                    if(!String.IsNullOrEmpty(getVisibility))
                        WriteVisibility(getVisibility, writer);

                    writer.WriteKeyword("get");
                }

                if(isSettable)
                {
                    if(isGettable)
                        writer.WriteString(", ");

                    // !EFW - Added support for getter/setter attributes
                    XPathNavigator setter = reflection.SelectSingleNode(apiSetterExpression);

                    if(setter != null && setter.HasChildren)
                    {
                        writer.WriteLine();
                        writer.WriteString("\t");
                        this.WriteAttributes(setter, writer, "\t");
                        writer.WriteString("\t");
                    }

                    string setVisibility = (string)reflection.Evaluate(apiSetVisibilityExpression);

                    if(!String.IsNullOrEmpty(setVisibility))
                        WriteVisibility(setVisibility, writer);

                    writer.WriteKeyword("set");
                }

                if(i == 0)
                    writer.WriteLine();
            }
        }

        /// <inheritdoc />
        public override void WriteEventSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            // !EFW - Added unsafe check
            if(IsUnsupportedUnsafe(reflection, writer))
                return;

            string name = (string)reflection.Evaluate(apiNameExpression);
            XPathNavigator handler = reflection.SelectSingleNode(apiHandlerOfEventExpression);
            XPathNavigator args = reflection.SelectSingleNode(apiEventArgsExpression);
            bool isVirtual = (bool)reflection.Evaluate(apiIsVirtualExpression) && !(bool)reflection.Evaluate(apiIsAbstractProcedureExpression);
            int iterations = isVirtual ? 2 : 1;

            for(int i = 0; i < iterations; i++)
            {
                WriteAttributes(reflection, writer);
                WriteVisibility(reflection, writer);

                if(isVirtual)
                {
                    if(i == 0)
                    {
                        writer.WriteKeyword("abstract");
                        writer.WriteString(" ");
                    }
                    else
                    {
                        writer.WriteKeyword("override");
                        writer.WriteString(" ");
                    }
                }
                else
                    WriteMemberKeyword(reflection, writer);

                writer.WriteIdentifier(name);
                writer.WriteString(" : ");
                writer.WriteReferenceLink("T:Microsoft.FSharp.Control.IEvent`2");

                writer.WriteString("<");

                WriteTypeReference(handler, writer);

                writer.WriteString(",");
                writer.WriteLine();
                writer.WriteString("    ");

                if(args == null)
                    writer.WriteReferenceLink("T:System.EventArgs");
                else
                    WriteTypeReference(args, writer);

                writer.WriteString(">");

                if(i == 0)
                    writer.WriteLine();
            }
        }

        /// <inheritdoc />
        public override void WriteFieldSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            // !EFW - Added unsafe check
            if(IsUnsupportedUnsafe(reflection, writer))
                return;

            string name = (string)reflection.Evaluate(apiNameExpression);
            bool isStatic = (bool)reflection.Evaluate(apiIsStaticExpression);
            bool isInitOnly = (bool)reflection.Evaluate(apiIsInitOnlyFieldExpression);
            bool isSerialized = (bool)reflection.Evaluate(apiIsSerializedFieldExpression);

            if(!isSerialized)
                WriteAttribute("T:System.NonSerializedAttribute", true, writer);

            WriteAttributes(reflection, writer);

            if(isStatic)
            {
                writer.WriteKeyword("static");
                writer.WriteString(" ");
            }

            writer.WriteKeyword("val");
            writer.WriteString(" ");

            if(!isInitOnly)
            {
                writer.WriteKeyword("mutable");
                writer.WriteString(" ");
            }

            WriteVisibility(reflection, writer);

            writer.WriteIdentifier(name);

            writer.WriteString(": ");
            WriteReturnValue(reflection, writer);
        }

        private void WriteDotNetObject(XPathNavigator reflection, SyntaxWriter writer, string kind)
        {
            string name = reflection.Evaluate(apiNameExpression).ToString();
            bool isSerializable = (bool)reflection.Evaluate(apiIsSerializableTypeExpression);
            XPathNodeIterator implements = reflection.Select(apiImplementedInterfacesExpression);
            XPathNavigator baseClass = reflection.SelectSingleNode(apiBaseClassExpression);
            bool hasBaseClass = (baseClass != null) && !((bool)baseClass.Evaluate(typeIsObjectExpression));

            // CLR considers interfaces abstract.
            bool isAbstract = (bool)reflection.Evaluate(apiIsAbstractTypeExpression) && kind != "interface";
            bool isSealed = (bool)reflection.Evaluate(apiIsSealedTypeExpression);

            if(isAbstract)
                WriteAttribute("T:Microsoft.FSharp.Core.AbstractClassAttribute", true, writer);
            if(isSealed)
                WriteAttribute("T:Microsoft.FSharp.Core.SealedAttribute", true, writer);

            if(isSerializable)
                WriteAttribute("T:System.SerializableAttribute", true, writer);
            WriteAttributes(reflection, writer);

            writer.WriteKeyword("type");
            writer.WriteString(" ");
            writer.WriteIdentifier(name);
            WriteGenericTemplates(reflection, writer);
            writer.WriteString(" =  ");

            if(hasBaseClass || implements.Count != 0)
            {
                writer.WriteLine();
                writer.WriteString("    ");
            }
            writer.WriteKeyword(kind);

            if(hasBaseClass || implements.Count != 0)
            {
                writer.WriteLine();
            }

            if(hasBaseClass)
            {
                writer.WriteString("        ");
                writer.WriteKeyword("inherit");
                writer.WriteString(" ");
                WriteTypeReference(baseClass, writer);
                writer.WriteLine();
            }


            while(implements.MoveNext())
            {
                XPathNavigator implement = implements.Current;
                writer.WriteString("        ");
                writer.WriteKeyword("interface");
                writer.WriteString(" ");
                WriteTypeReference(implement, writer);
                writer.WriteLine();
            }

            if(hasBaseClass || implements.Count != 0)
            {
                writer.WriteString("    ");
            }
            else
            {
                writer.WriteString(" ");
            }

            writer.WriteKeyword("end");

        }

        // Visibility

        private static void WriteVisibility(XPathNavigator reflection, SyntaxWriter writer)
        {
            string visibility = reflection.Evaluate(apiVisibilityExpression).ToString();
            WriteVisibility(visibility, writer);
        }

        private static readonly Dictionary<string, string> visibilityDictionary = new Dictionary<string, string>()
        {
            { "public", null }, // Default in F#, so unnecessary.
            { "family", null }, // Not supported in F#, section 8.8 in F# spec.
            { "family or assembly", null }, // Not supported in F#, section 8.8 in F# spec. 
            { "family and assembly", null }, // Not supported in F#, section 8.8 in F# spec.
            { "assembly", "internal" },
            { "private", "private" },
        };

        private static void WriteVisibility(string visibility, SyntaxWriter writer)
        {
            if(visibilityDictionary.ContainsKey(visibility) && visibilityDictionary[visibility] != null)
            {
                writer.WriteKeyword(visibilityDictionary[visibility]);
                writer.WriteString(" ");
            }
        }

        // Write member | abstract | override
        private static void WriteMemberKeyword(XPathNavigator reflection, SyntaxWriter writer)
        {
            bool isOverride = (bool)reflection.Evaluate(apiIsOverrideExpression);
            bool isAbstract = (bool)reflection.Evaluate(apiIsAbstractProcedureExpression);

            if(isOverride)
                writer.WriteKeyword("override");
            else
                if(isAbstract)
                writer.WriteKeyword("abstract");
            else
                writer.WriteKeyword("member");

            writer.WriteString(" ");
        }

        // Attributes

        // !EFW - Added newLine parameter
        private static void WriteAttribute(string reference, bool newLine, SyntaxWriter writer)
        {
            writer.WriteString("[<");
            writer.WriteReferenceLink(reference);
            writer.WriteString(">]");

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

                // !EFW - Ignore FixedBufferAttribute, ParamArrayAttribute, IsByRefLikeAttribute, IsReadOnlyAttribute too
                if(type.GetAttribute("api", String.Empty) == "T:System.Runtime.CompilerServices.FixedBufferAttribute" ||
                   type.GetAttribute("api", String.Empty) == "T:System.Runtime.CompilerServices.IsByRefLikeAttribute" ||
                   type.GetAttribute("api", String.Empty) == "T:System.Runtime.CompilerServices.IsReadOnlyAttribute" ||
                   type.GetAttribute("api", String.Empty) == "T:System.ParamArrayAttribute")
                    continue;

                writer.WriteString("[<");
                WriteTypeReference(type, writer);

                XPathNodeIterator arguments = attribute.Select(attributeArgumentsExpression);
                XPathNodeIterator assignments = attribute.Select(attributeAssignmentsExpression);

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

                writer.WriteString(">]");

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

                writer.WriteString("[<");
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
                    writer.WriteString("Pack = ");
                    writer.WriteString(((int)pack).ToString(CultureInfo.InvariantCulture));
                }

                switch(format)
                {
                    case "ansi":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CharSet = CharSet.Ansi");
                        break;

                    case "auto":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CharSet = CharSet.Auto");
                        break;

                    case "unicode":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CharSet = CharSet.Unicode");
                        break;
                }

                writer.WriteString(")");
                writer.WriteString(">]");
                writer.WriteLine();
            }

            double fieldOffset = (double)reflection.Evaluate(apiFieldOffsetFieldExpression);

            if(!Double.IsNaN(fieldOffset))
            {
                writer.WriteString("[<");
                WriteNormalTypeReference("T:System.Runtime.InteropServices.FieldOffsetAttribute", writer);
                writer.WriteString("(");
                writer.WriteString(((int)fieldOffset).ToString(CultureInfo.InvariantCulture));
                writer.WriteString(")");
                writer.WriteString(">]");
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

                writer.WriteString("[<");
                WriteNormalTypeReference("T:System.Runtime.InteropServices.DllImportAttribute", writer);
                writer.WriteString("(\"");
                writer.WriteString(module);
                writer.WriteString("\"");

                if(!String.IsNullOrEmpty(entryPoint))
                {
                    WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                    writer.WriteString("EntryPoint = \"");
                    writer.WriteString(entryPoint);
                    writer.WriteString("\"");
                }

                switch(callingConv)
                {
                    case "cdecl":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CallingConvention = CallingConvention.Cdecl");
                        break;

                    case "fastcall":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CallingConvention = CallingConvention.FastCall");
                        break;

                    case "stdcall":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CallingConvention = CallingConvention.StdCall");
                        break;

                    case "thiscall":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CallingConvention = CallingConvention.ThisCall");
                        break;
                }

                switch(charset)
                {
                    case "ansi":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CharSet = CharSet.Ansi");
                        break;

                    case "auto":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CharSet = CharSet.Auto");
                        break;

                    case "unicode":
                        WriteWithLineBreakIfNeeded(writer, ", ", indent + "\t");
                        writer.WriteString("CharSet = CharSet.Unicode");
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
                writer.WriteString(">]");
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
                    writer.WriteKeyword("null");
                    break;

                case "defaultValue":
                    // I'm making an assumption about syntax here
                    writer.WriteString("new ");
                    this.WriteTypeReference(type, writer);
                    writer.WriteString("()");
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

        // Generics
        private void WriteGenericTemplates(XPathNavigator reflection, SyntaxWriter writer)
        {

            XPathNodeIterator templates = (XPathNodeIterator)reflection.Evaluate(apiTemplatesExpression);

            if(templates.Count == 0)
                return;
            writer.WriteString("<");
            while(templates.MoveNext())
            {
                XPathNavigator template = templates.Current;
                string name = template.GetAttribute("name", String.Empty);
                writer.WriteString("'");
                writer.WriteString(name);
                if(templates.CurrentPosition < templates.Count)
                    writer.WriteString(", ");
            }
            WriteGenericTemplateConstraints(reflection, writer);
            writer.WriteString(">");
        }

        private void WriteGenericTemplateConstraints(XPathNavigator reflection, SyntaxWriter writer)
        {

            XPathNodeIterator templates = reflection.Select(apiTemplatesExpression);

            if(templates.Count == 0)
                return;

            foreach(XPathNavigator template in templates)
            {

                bool constrained = (bool)template.Evaluate(templateIsConstrainedExpression);
                if(constrained)
                {
                    string name = (string)template.Evaluate(templateNameExpression);

                    writer.WriteString(" ");
                    writer.WriteKeyword("when");
                    writer.WriteString(" '");
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
                    writer.WriteKeyword("not struct");
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
                        writer.WriteString(" and ");
                    WriteTypeReference(constraint, writer);
                    previous = true;
                }

            }

        }

        // Parameters

        private void WriteParameters(XPathNavigator reflection, SyntaxWriter writer)
        {
            XPathNodeIterator parameters = reflection.Select(apiParametersExpression);

            if(parameters.Count > 0)
                WriteParameters(parameters, writer);
            else
            {
                writer.WriteKeyword("unit");
                writer.WriteString(" ");
            }
        }

        private void WriteParameters(XPathNodeIterator parameters, SyntaxWriter writer)
        {
            List<KeyValuePair<string, XPathNavigator>> optionalParams = new List<KeyValuePair<string, XPathNavigator>>();
            writer.WriteLine();

            while(parameters.MoveNext())
            {
                XPathNavigator parameter = parameters.Current;

                // !EFW - Added support for optional parameter values
                XPathNavigator argument = parameter.SelectSingleNode(parameterArgumentExpression);
                bool isOptional = (bool)parameter.Evaluate(parameterIsOptionalExpression);

                string name = (string)parameter.Evaluate(parameterNameExpression);
                bool isOut = (bool)parameter.Evaluate(parameterIsOutExpression);
                bool isRef = (bool)parameter.Evaluate(parameterIsRefExpression);
                XPathNavigator type = parameter.SelectSingleNode(parameterTypeExpression);
                writer.WriteString("        ");

                // !EFW - Write out parameter attributes
                WriteAttributes(parameter, writer, null, true);

                // !EFW - Optional indicated by OptionalAttribute?
                if(isOptional)
                    if(argument == null)
                    {
                        WriteAttribute("T:System.Runtime.InteropServices.OptionalAttribute", false, writer);
                        writer.WriteString(" ");
                    }
                    else
                    {
                        writer.WriteString("?");
                        optionalParams.Add(new KeyValuePair<string, XPathNavigator>(name, argument));
                    }

                writer.WriteParameter(name);
                writer.WriteString(" : ");
                WriteTypeReference(type, writer);

                if(isOut || isRef)
                {
                    writer.WriteString(" ");
                    writer.WriteKeyword("byref");
                }

                if(parameters.CurrentPosition != parameters.Count)
                {
                    writer.WriteString(" * ");
                    writer.WriteLine();
                }
                else
                    writer.WriteString(" ");
            }

            // !EFW - Write out the optional value assignments.  F# uses a function to assign defaults so we'll
            // just list them in a "comment" block.  There's probably a better way to do this but I know nothing
            // about F#.
            if(optionalParams.Count != 0)
            {
                writer.WriteLine();
                writer.WriteString("(* Defaults:");
                writer.WriteLine();

                foreach(var kv in optionalParams)
                {
                    writer.WriteString("        ");
                    writer.WriteKeyword("let ");
                    writer.WriteIdentifier("_");
                    writer.WriteIdentifier(kv.Key);
                    writer.WriteString(" = defaultArg ");
                    writer.WriteIdentifier(kv.Key);
                    writer.WriteString(" ");
                    this.WriteValue(kv.Value, writer);
                    writer.WriteLine();
                }

                writer.WriteString("*)");
                writer.WriteLine();
            }
        }

        // Return Value

        private void WriteReturnValue(XPathNavigator reflection, SyntaxWriter writer)
        {
            XPathNavigator type = reflection.SelectSingleNode(apiReturnTypeExpression);

            if(type == null)
                writer.WriteKeyword("unit");
            else
                WriteTypeReference(type, writer);
        }

        // References

        /// <inheritdoc />
        protected override void WriteTypeReference(XPathNavigator reference, SyntaxWriter writer)
        {
            if(reference == null)
                throw new ArgumentNullException(nameof(reference));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

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
                    writer.WriteKeyword("nativeptr");
                    writer.WriteString("<");
                    WriteTypeReference(pointee, writer);
                    writer.WriteString(">");
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
                        WriteTypeReference(typeModifiers.Current, writer);

                    break;
                case "template":
                    string name = reference.GetAttribute("name", String.Empty);
                    writer.WriteString("'");
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

        /// <inheritdoc />
        protected override void WriteNormalTypeReference(string api, SyntaxWriter writer)
        {
            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            switch(api)
            {
                case "T:System.Void":
                    writer.WriteReferenceLink(api, "unit");
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
                    writer.WriteReferenceLink(api, "int16");
                    break;

                case "T:System.Int32":
                    writer.WriteReferenceLink(api, "int");
                    break;

                case "T:System.Int64":
                    writer.WriteReferenceLink(api, "int64");
                    break;

                case "T:System.UInt16":
                    writer.WriteReferenceLink(api, "uint16");
                    break;

                case "T:System.UInt32":
                    writer.WriteReferenceLink(api, "uint32");
                    break;

                case "T:System.UInt64":
                    writer.WriteReferenceLink(api, "uint64");
                    break;

                case "T:System.Single":
                    writer.WriteReferenceLink(api, "float32");
                    break;

                case "T:System.Double":
                    writer.WriteReferenceLink(api, "float");
                    break;

                case "T:System.Decimal":
                    writer.WriteReferenceLink(api, "decimal");
                    break;

                default:
                    writer.WriteReferenceLink(api);
                    break;
            }
        }
    }
}
