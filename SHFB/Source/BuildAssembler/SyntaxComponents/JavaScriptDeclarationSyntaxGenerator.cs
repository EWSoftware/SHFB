//===============================================================================================================
// System  : Sandcastle Build Components
// File    : JavaScriptDeclarationSyntaxGenerator.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/09/2021
// Note    : This is a slightly modified version of the Microsoft ScriptSharpDeclarationSyntaxGenerator
//           (Copyright 2007-2021 Microsoft Corporation).  My changes are indicated by my initials "EFW" in a
//           comment on the changes.
//
// This file contains a JavaScript declaration syntax generator that is used to add a JavaScript Syntax section
// to each generated API topic.  This version differs from the ScriptSharpDeclarationSyntaxGenerator in that it
// looks for a <scriptSharp /> element in the <api> node and, if found, only then will it apply the casing rules
// to the member name.  If not present, no casing rules are applied to the member names thus it is suitable for
// use with regular JavaScript such as that used in AjaxDoc projects.  There are actually only two minor changes
// plus a change to the Script# Reflection File Fixer plug-in.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Change History
// 02/16/2012 - EFW - Merged my changes into the code
// 12/20/2013 - EFW - Updated the syntax generator to be discoverable via MEF
// 08/01/2014 - EFW - Added support for resource item files containing the localized titles, messages, etc.
//===============================================================================================================

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml.XPath;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler.SyntaxGenerator;

namespace Sandcastle.Tools.SyntaxGenerators
{
    /// <summary>
    /// This is a JavaScript declaration syntax generator that is used to add a JavaScript Syntax section to
    /// each generated API topic.
    /// </summary>
    /// <remarks><para>This version differs from the <c>ScriptSharpDeclarationSyntaxGenerator</c> in that it
    /// looks for a <c>&lt;scriptSharp /&gt;</c> element in the <c>&lt;api&gt;</c> node and, if found, only then
    /// will it apply the casing rules to the member name.  If not present, no casing rules are applied to the
    /// member names thus it is suitable for use with regular JavaScript such as that used in AjaxDoc projects.</para>
    /// 
    /// <para>In order to use this script generator with Script# content, the Sandcastle reflection data file
    /// must first be transformed using the Script# Reflection File Fixer plug-in so that the necessary changes
    /// are made to it.</para>
    /// </remarks>
    public sealed class JavaScriptDeclarationSyntaxGenerator : SyntaxGeneratorTemplate
    {
        #region Syntax generator factory for MEF
        //=====================================================================

        private const string LanguageName = "JavaScript", StyleIdName = "js";

        /// <summary>
        /// This is used to create a new instance of the syntax generator
        /// </summary>
        [SyntaxGeneratorExport("JavaScript", LanguageName, StyleIdName, AlternateIds = "js, ecmascript",
          SortOrder = 80, Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
          Description = "Generates JavaScript declaration syntax sections")]
        public sealed class Factory : ISyntaxGeneratorFactory
        {
            /// <inheritdoc />
            public string ResourceItemFileLocation => Path.Combine(ComponentUtilities.AssemblyFolder(
                Assembly.GetExecutingAssembly()), "SyntaxContent");

            /// <inheritdoc />
            public SyntaxGeneratorCore Create()
            {
                return new JavaScriptDeclarationSyntaxGenerator { Language = LanguageName, StyleId = StyleIdName };
            }
        }
        #endregion

        #region Fields
        //=====================================================================

        private static readonly XPathExpression memberIsGlobalExpression =
            XPathExpression.Compile("boolean(apidata/@global)");
        private static readonly XPathExpression typeIsRecordExpression =
            XPathExpression.Compile("boolean(apidata/@record)");

        // EFW - Added XPath expression to locate the scriptSharp element
        private static readonly XPathExpression scriptSharpExpression =
            XPathExpression.Compile("boolean(scriptSharp)");

        #endregion

        #region Private methods
        //=====================================================================

        // EFW - Made most of these methods static per FxCop rule

        /// <summary>
        /// Check to see if the given attribute exists on the entry
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <param name="attributeName">The attribute for which to search</param>
        /// <returns>True if found or false if not found</returns>
        private static bool HasAttribute(XPathNavigator reflection, string attributeName)
        {
            attributeName = "T:" + attributeName;
            XPathNodeIterator iterator = (XPathNodeIterator)reflection.Evaluate(apiAttributesExpression);

            foreach(XPathNavigator navigator in iterator)
            {
                if(navigator.SelectSingleNode(attributeTypeExpression).GetAttribute(
                  "api", String.Empty) == attributeName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check to see if the element is unsupported
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <param name="writer">The writer to which the "unsupported"
        /// message is written.</param>
        /// <returns>True if unsupported or false if it is supported</returns>
        private bool IsUnsupported(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(this.IsUnsupportedGeneric(reflection, writer) || this.IsUnsupportedExplicit(reflection, writer) ||
              this.IsUnsupportedUnsafe(reflection, writer))
            {
                return true;
            }

            if(HasAttribute(reflection, "System.NonScriptableAttribute"))
            {
                writer.WriteMessage("UnsupportedType_ScriptSharp");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Convert the given name to camel case
        /// </summary>
        /// <param name="name ">The name to modify</param>
        /// <returns>The name converted to camel case</returns>
        private static string CreateCamelCaseName(string name)
        {
            if(String.IsNullOrEmpty(name))
                return name;

            // Some exceptions that simply need to be special cased
            if(name.Equals("ID", StringComparison.Ordinal))
                return "id";

            bool hasLowerCase = false;
            int conversionLength = 0;

            for(int i = 0; i < name.Length; i++)
                if(Char.IsUpper(name, i))
                    conversionLength++;
                else
                {
                    hasLowerCase = true;
                    break;
                }

            // If name is all upper case or all lower case, leave it as-is
            if((!hasLowerCase && name.Length != 1) || conversionLength == 0)
                return name;

            if(conversionLength > 1)
            {
                // Convert the leading uppercase segment, except the last character
                // which is assumed to be the first letter of the next word
                return name.Substring(0, conversionLength - 1).ToLowerInvariant() +
                    name.Substring(conversionLength - 1);
            }
            else if(name.Length == 1)
            {
                return name.ToLowerInvariant();
            }
            else
            {
                // Convert the leading upper case character to lower case
                return Char.ToLower(name[0], CultureInfo.InvariantCulture) + name.Substring(1);
            }
        }

        /// <summary>
        /// Read the containing type name from the entry
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <returns>The containing type name if found or null if not found</returns>
        private static string ReadContainingTypeName(XPathNavigator reflection)
        {
            return (string)reflection.Evaluate(apiContainingTypeNameExpression);
        }

        /// <summary>
        /// Read the full containing type name from the entry
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <returns>The full containing type name prefixed with its namespace
        /// or null if not found</returns>
        private static string ReadFullContainingTypeName(XPathNavigator reflection)
        {
            string nameSpace = ReadNamespaceName(reflection);
            string typeName = ReadContainingTypeName(reflection);

            if(String.IsNullOrEmpty(nameSpace) || HasAttribute(reflection, "System.IgnoreNamespaceAttribute"))
                return typeName;

            return String.Concat(nameSpace, ".", typeName);
        }

        /// <summary>
        /// Read the full type name from the entry
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <returns>The full type name prefixed with its namespace or null if
        /// not found</returns>
        private static string ReadFullTypeName(XPathNavigator reflection)
        {
            string nameSpace = ReadNamespaceName(reflection);
            string typeName = ReadTypeName(reflection);

            if(String.IsNullOrEmpty(nameSpace) || HasAttribute(reflection, "System.IgnoreNamespaceAttribute"))
                return typeName;

            return String.Concat(nameSpace, ".", typeName);
        }

        /// <summary>
        /// Read the member name from the entry
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <returns>The member name</returns>
        /// <remarks>If a <c>&lt;scriptSharp /&gt;</c> element exists in the
        /// entry, the casing rule is applied to the member name.  If not
        /// present, it is returned as-is.  The casing rule will convert the
        /// name to camel case unless the member is marked with
        /// <c>System.PreserveCaseAttribute</c>.</remarks>
        private static string ReadMemberName(XPathNavigator reflection)
        {
            string identifier = (string)reflection.Evaluate(apiNameExpression);

            // EFW - Don't apply the rule if <scriptSharp /> isn't found
            // or the PreserveCaseAttribute is found.
            if((bool)reflection.Evaluate(scriptSharpExpression) &&
              !HasAttribute(reflection, "System.PreserveCaseAttribute"))
            {
                identifier = CreateCamelCaseName(identifier);
            }

            return identifier;
        }

        /// <summary>
        /// Read the namespace name from the entry
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <returns>The namespace name</returns>
        private static string ReadNamespaceName(XPathNavigator reflection)
        {
            return (string)reflection.Evaluate(apiContainingNamespaceNameExpression);
        }

        /// <summary>
        /// Read the type name from the entry
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <returns>The type name</returns>
        private static string ReadTypeName(XPathNavigator reflection)
        {
            return (string)reflection.Evaluate(apiNameExpression);
        }

        /// <summary>
        /// Write an indented new line
        /// </summary>
        /// <param name="writer">The syntax writer to which it is written</param>
        private static void WriteIndentedNewLine(SyntaxWriter writer)
        {
            writer.WriteString(",");
            writer.WriteLine();
            writer.WriteString("\t");
        }

        /// <inheritdoc />
        protected override void WriteNormalTypeReference(string api, SyntaxWriter writer)
        {
            if(api == null)
                throw new ArgumentNullException(nameof(api));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            switch(api)
            {
                case "T:System.Byte":
                    writer.WriteReferenceLink(api, "Byte");
                    break;

                case "T:System.SByte":
                    writer.WriteReferenceLink(api, "SByte");
                    break;

                case "T:System.Char":
                    writer.WriteReferenceLink(api, "Char");
                    break;

                case "T:System.Int16":
                    writer.WriteReferenceLink(api, "Int16");
                    break;

                case "T:System.Int32":
                    writer.WriteReferenceLink(api, "Int32");
                    break;

                case "T:System.Int64":
                    writer.WriteReferenceLink(api, "Int64");
                    break;

                case "T:System.UInt16":
                    writer.WriteReferenceLink(api, "UInt16");
                    break;

                case "T:System.UInt32":
                    writer.WriteReferenceLink(api, "UInt32");
                    break;

                case "T:System.UInt64":
                    writer.WriteReferenceLink(api, "UInt64");
                    break;

                case "T:System.Single":
                    writer.WriteReferenceLink(api, "Single");
                    break;

                case "T:System.Double":
                    writer.WriteReferenceLink(api, "Double");
                    break;

                case "T:System.Decimal":
                    writer.WriteReferenceLink(api, "Decimal");
                    break;

                case "T:System.Boolean":
                    writer.WriteReferenceLink(api, "Boolean");
                    break;

                default:
                    {
                        string text = api.Substring(2);

                        if(text.StartsWith("System.", StringComparison.Ordinal))
                        {
                            int num = text.LastIndexOf('.');
                            text = text.Substring(num + 1);
                        }

                        writer.WriteReferenceLink(api, text);
                        break;
                    }
            }
        }

        /// <summary>
        /// Write out a parameter
        /// </summary>
        /// <param name="parameter">The parameter information</param>
        /// <param name="writer">The syntax writer to which it is written</param>
        private static void WriteParameter(XPathNavigator parameter, SyntaxWriter writer)
        {
            string paramName = (string)parameter.Evaluate(parameterNameExpression);

            if((bool)parameter.Evaluate(parameterIsParamArrayExpression))
            {
                writer.WriteString("... ");
            }

            writer.WriteParameter(paramName);
        }

        /// <summary>
        /// Write out a parameter list
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <param name="writer">The syntax writer to which it is written</param>
        private static void WriteParameterList(XPathNavigator reflection, SyntaxWriter writer)
        {
            XPathNodeIterator iterator = reflection.Select(apiParametersExpression);

            writer.WriteString("(");

            while(iterator.MoveNext())
            {
                XPathNavigator current = iterator.Current;
                WriteParameter(current, writer);

                if(iterator.CurrentPosition < iterator.Count)
                    writer.WriteString(", ");
            }

            writer.WriteString(")");
        }

        /// <summary>
        /// Write out the record constructor syntax
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <param name="writer">The syntax writer to which it is written</param>
        private static void WriteRecordConstructorSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string nameSpace = ReadNamespaceName(reflection);
            string containingType = ReadContainingTypeName(reflection);

            writer.WriteString(nameSpace);
            writer.WriteString(".$create_");
            writer.WriteString(containingType);
            writer.WriteString(" = ");
            writer.WriteKeyword("function");
            WriteParameterList(reflection, writer);
            writer.WriteString(";");
        }

        /// <summary>
        /// Write out the record syntax
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <param name="writer">The syntax writer to which it is written</param>
        private static void WriteRecordSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string nameSpace = ReadNamespaceName(reflection);
            string typeName = ReadTypeName(reflection);

            writer.WriteString(nameSpace);
            writer.WriteString(".$create_");
            writer.WriteString(typeName);
            writer.WriteString(" = ");
            writer.WriteKeyword("function");
            writer.WriteString("();");
        }

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
                    int rank = Convert.ToInt32(reference.GetAttribute("rank", String.Empty), CultureInfo.InvariantCulture);
                    XPathNavigator navigator = reference.SelectSingleNode(typeExpression);

                    WriteTypeReference(navigator, writer);
                    writer.WriteString("[");

                    for(int i = 1; i < rank; i++)
                        writer.WriteString(",");

                    writer.WriteString("]");
                    break;

                case "type":
                    string id = reference.GetAttribute("api", String.Empty);
                    WriteNormalTypeReference(id, writer);
                    break;

                case "pointerTo":
                case "referenceTo":
                case "template":
                case "specialization":
                    // Not supported
                    break;
            }
        }
        #endregion

        #region Public methods
        //=====================================================================

        /// <summary>
        /// Not used by this syntax generator
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <param name="writer">The syntax writer to which it is written</param>
        public override void WriteAttachedEventSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
        }

        /// <summary>
        /// Write out attached property syntax
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <param name="writer">The syntax writer to which it is written</param>
        public override void WriteAttachedPropertySyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            string containingTypeName = ReadContainingTypeName(reflection);
            string memberName = ReadMemberName(reflection);
            string fullName = String.Concat(containingTypeName, ".", memberName.Substring(3));

            if(memberName.StartsWith("Get", StringComparison.OrdinalIgnoreCase))
            {
                writer.WriteKeyword("var");
                writer.WriteString(" value = obj['");
                writer.WriteString(fullName);
                writer.WriteString("'];");
            }
            else
                if(memberName.StartsWith("Set", StringComparison.OrdinalIgnoreCase))
            {
                writer.WriteString("obj['");
                writer.WriteString(fullName);
                writer.WriteString("'] = value;");
            }
        }

        /// <summary>
        /// Write out class syntax
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <param name="writer">The syntax writer to which it is written</param>
        public override void WriteClassSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            if(this.IsUnsupported(reflection, writer))
                return;

            if(HasAttribute(reflection, "System.RecordAttribute"))
            {
                WriteRecordSyntax(reflection, writer);
                return;
            }

            string identifier = ReadFullTypeName(reflection);

            writer.WriteIdentifier(identifier);
            writer.WriteString(" = ");
            writer.WriteKeyword("function");
            writer.WriteString("();");
            writer.WriteLine();
            writer.WriteLine();
            writer.WriteIdentifier("Type");
            writer.WriteString(".createClass(");
            writer.WriteLine();
            writer.WriteString("\t'");
            writer.WriteString(identifier);
            writer.WriteString("'");

            bool hasBaseClass = false;
            XPathNavigator reference = reflection.SelectSingleNode(apiBaseClassExpression);

            if(!(reference == null || (bool)reference.Evaluate(typeIsObjectExpression)))
            {
                WriteIndentedNewLine(writer);
                this.WriteTypeReference(reference, writer);
                hasBaseClass = true;
            }

            XPathNodeIterator iterator = reflection.Select(apiImplementedInterfacesExpression);

            if(iterator.Count != 0)
            {
                if(!hasBaseClass)
                {
                    WriteIndentedNewLine(writer);
                    writer.WriteString("null");
                }

                WriteIndentedNewLine(writer);

                while(iterator.MoveNext())
                {
                    XPathNavigator current = iterator.Current;
                    this.WriteTypeReference(current, writer);

                    if(iterator.CurrentPosition < iterator.Count)
                        WriteIndentedNewLine(writer);
                }
            }

            writer.WriteString(");");
        }

        /// <summary>
        /// Write out constructor syntax
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <param name="writer">The syntax writer to which it is written</param>
        public override void WriteConstructorSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            if(!this.IsUnsupported(reflection, writer))
            {
                if((bool)reflection.Evaluate(typeIsRecordExpression))
                    WriteRecordConstructorSyntax(reflection, writer);
                else
                {
                    string identifier = ReadFullContainingTypeName(reflection);

                    writer.WriteIdentifier(identifier);
                    writer.WriteString(" = ");
                    writer.WriteKeyword("function");
                    WriteParameterList(reflection, writer);
                    writer.WriteString(";");
                }
            }
        }

        /// <summary>
        /// Write out delegate syntax
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <param name="writer">The syntax writer to which it is written</param>
        public override void WriteDelegateSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.WriteKeyword("function");
            WriteParameterList(reflection, writer);
            writer.WriteString(";");
        }

        /// <summary>
        /// Write out enumeration syntax
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <param name="writer">The syntax writer to which it is written</param>
        public override void WriteEnumerationSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            string identifier = ReadFullTypeName(reflection);

            writer.WriteIdentifier(identifier);
            writer.WriteString(" = ");
            writer.WriteKeyword("function");
            writer.WriteString("();");
            writer.WriteLine();
            writer.WriteIdentifier(identifier);
            writer.WriteString(".createEnum('");
            writer.WriteIdentifier(identifier);
            writer.WriteString("', ");
            writer.WriteString(HasAttribute(reflection, "System.FlagsAttribute") ? "true" : "false");
            writer.WriteString(");");
        }

        /// <summary>
        /// Write out event syntax
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <param name="writer">The syntax writer to which it is written</param>
        public override void WriteEventSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            if(!this.IsUnsupported(reflection, writer))
            {
                if(reflection.Select(apiParametersExpression).Count > 0)
                    writer.WriteMessage("UnsupportedIndex_" + this.Language);
                else
                {
                    string identifier = ReadMemberName(reflection);

                    writer.WriteKeyword("function");
                    writer.WriteString(" add_");
                    writer.WriteIdentifier(identifier);
                    writer.WriteString("(");
                    writer.WriteParameter("value");
                    writer.WriteString(");");
                    writer.WriteLine();
                    writer.WriteKeyword("function");
                    writer.WriteString(" remove_");
                    writer.WriteIdentifier(identifier);
                    writer.WriteString("(");
                    writer.WriteParameter("value");
                    writer.WriteString(");");
                }
            }
        }

        /// <summary>
        /// Write out field syntax
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <param name="writer">The syntax writer to which it is written</param>
        public override void WriteFieldSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            if(!this.IsUnsupported(reflection, writer))
            {
                string identifier = ReadMemberName(reflection);

                // EFW - Added "var" keyword before field name
                writer.WriteKeyword("var");
                writer.WriteString(" ");

                if((bool)reflection.Evaluate(apiIsStaticExpression))
                {
                    writer.WriteIdentifier(ReadFullContainingTypeName(reflection));
                    writer.WriteString(".");
                }

                writer.WriteIdentifier(identifier);
            }
        }

        /// <summary>
        /// Write out interface syntax
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <param name="writer">The syntax writer to which it is written</param>
        public override void WriteInterfaceSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            if(!this.IsUnsupported(reflection, writer))
            {
                string identifier = ReadFullTypeName(reflection);

                writer.WriteIdentifier(identifier);
                writer.WriteString(" = ");
                writer.WriteKeyword("function");
                writer.WriteString("();");
                writer.WriteLine();
                writer.WriteIdentifier(identifier);
                writer.WriteString(".createInterface('");
                writer.WriteIdentifier(identifier);
                writer.WriteString("');");
            }
        }

        /// <summary>
        /// Write out namespace syntax
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <param name="writer">The syntax writer to which it is written</param>
        public override void WriteNamespaceSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            string identifier = reflection.Evaluate(apiNameExpression).ToString();

            writer.WriteString("Type.createNamespace('");
            writer.WriteIdentifier(identifier);
            writer.WriteString("');");
        }

        /// <summary>
        /// Write out normal method syntax
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <param name="writer">The syntax writer to which it is written</param>
        public override void WriteNormalMethodSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            if(this.IsUnsupported(reflection, writer))
                return;

            if(HasAttribute(reflection, "System.AttachedPropertyAttribute"))
            {
                this.WriteAttachedPropertySyntax(reflection, writer);
                return;
            }

            string identifier = ReadMemberName(reflection);
            bool isStatic = (bool)reflection.Evaluate(apiIsStaticExpression);
            bool isGlobal = (bool)reflection.Evaluate(memberIsGlobalExpression);

            if(isStatic && !isGlobal)
            {
                writer.WriteIdentifier(ReadFullContainingTypeName(reflection));
                writer.WriteString(".");
                writer.WriteIdentifier(identifier);
                writer.WriteString(" = ");
                writer.WriteKeyword("function");
            }
            else
            {
                writer.WriteKeyword("function");
                writer.WriteString(" ");
                writer.WriteIdentifier(identifier);
            }

            WriteParameterList(reflection, writer);
            writer.WriteString(";");
        }

        /// <summary>
        /// Operator syntax is unsupported
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <param name="writer">The syntax writer to which it is written</param>
        public override void WriteOperatorSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.WriteMessage("UnsupportedOperator_" + this.Language);
        }

        /// <summary>
        /// Cast syntax is unsupported
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <param name="writer">The syntax writer to which it is written</param>
        public override void WriteCastSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.WriteMessage("UnsupportedCast_" + this.Language);
        }

        /// <summary>
        /// Write out property syntax if supported
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <param name="writer">The syntax writer to which it is written</param>
        public override void WritePropertySyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            if(this.IsUnsupported(reflection, writer))
                return;

            if(HasAttribute(reflection, "System.IntrinsicPropertyAttribute"))
            {
                this.WriteFieldSyntax(reflection, writer);
                return;
            }

            string identifier = ReadMemberName(reflection);
            bool isStatic = (bool)reflection.Evaluate(apiIsStaticExpression);
            bool isReadProp = (bool)reflection.Evaluate(apiIsReadPropertyExpression);
            bool isWriteProp = (bool)reflection.Evaluate(apiIsWritePropertyExpression);

            if(isReadProp)
            {
                if(isStatic)
                {
                    writer.WriteIdentifier(ReadFullContainingTypeName(reflection));
                    writer.WriteString(".");
                    writer.WriteString("get_");
                    writer.WriteIdentifier(identifier);
                    writer.WriteString(" = ");
                    writer.WriteKeyword("function");
                }
                else
                {
                    writer.WriteKeyword("function");
                    writer.WriteString(" ");
                    writer.WriteString("get_");
                    writer.WriteIdentifier(identifier);
                }

                WriteParameterList(reflection, writer);
                writer.WriteString(";");
                writer.WriteLine();
            }

            if(isWriteProp)
            {
                if(isStatic)
                {
                    writer.WriteIdentifier(ReadFullContainingTypeName(reflection));
                    writer.WriteString(".");
                    writer.WriteString("set_");
                    writer.WriteIdentifier(identifier);
                    writer.WriteString(" = ");
                    writer.WriteKeyword("function");
                }
                else
                {
                    writer.WriteKeyword("function");
                    writer.WriteString(" ");
                    writer.WriteString("set_");
                    writer.WriteIdentifier(identifier);
                }

                writer.WriteString("(");
                writer.WriteParameter("value");
                writer.WriteString(");");
            }
        }

        /// <summary>
        /// Structure syntax is unsupported
        /// </summary>
        /// <param name="reflection">The reflection information</param>
        /// <param name="writer">The syntax writer to which it is written</param>
        public override void WriteStructureSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            if(!this.IsUnsupported(reflection, writer))
                writer.WriteMessage("UnsupportedStructure_" + this.Language);
        }
        #endregion
    }
}
