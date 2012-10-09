// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Xml.XPath;
using System.Globalization;

namespace Microsoft.Ddue.Tools {
    /// <summary>
    /// Generates syntax that corresponds to JavaScript that Script# generates.
    /// </summary>
    public class ScriptSharpDeclarationSyntaxGenerator : SyntaxGeneratorTemplate {
        private static readonly XPathExpression typeIsRecordExpression = XPathExpression.Compile("boolean(apidata/@record)");

        private static readonly XPathExpression memberIsGlobalExpression = XPathExpression.Compile("boolean(apidata/@global)");

        /// <summary>
        /// Initializes a new instance of the <c>ScriptSharpDeclarationSyntaxGenerator</c> class.
        /// </summary>
        /// <param name="configuration"></param>
        public ScriptSharpDeclarationSyntaxGenerator(XPathNavigator configuration) : base(configuration) {
		    if (String.IsNullOrEmpty(Language)) Language = "JavaScript";
        }
        
        /// <summary>
        /// Determines whether the feature is unsupported by this language.
        /// </summary>
        /// <param name="reflection"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        private bool IsUnsupported(XPathNavigator reflection, SyntaxWriter writer) {
            if (IsUnsupportedGeneric(reflection, writer)) {
                return true;
            }

            if (IsUnsupportedExplicit(reflection, writer)) {
                return true;
            }

            if (IsUnsupportedUnsafe(reflection, writer)) {
                return true;
            }

            if (HasAttribute(reflection, "System.NonScriptableAttribute")) {
                writer.WriteMessage("UnsupportedType_ScriptSharp");
                return true;
            }

            return false;
        }

        private string ReadNamespaceName(XPathNavigator reflection) {
            return (string)reflection.Evaluate(apiContainingNamespaceNameExpression);
        }

        private string ReadTypeName(XPathNavigator reflection) {
            return (string)reflection.Evaluate(apiNameExpression);
        }

        private string ReadContainingTypeName(XPathNavigator reflection) {
            return (string)reflection.Evaluate(apiContainingTypeNameExpression);
        }

        private string ReadFullTypeName(XPathNavigator reflection) {
            string namespaceName = ReadNamespaceName(reflection);

            string typeName = ReadTypeName(reflection);

            if (String.IsNullOrEmpty(namespaceName) || HasAttribute(reflection, "System.IgnoreNamespaceAttribute")) {
                return typeName;
            }
            else {
                return String.Format("{0}.{1}", namespaceName, typeName);
            }
        }

        private string ReadFullContainingTypeName(XPathNavigator reflection) {
            string namespaceName = ReadNamespaceName(reflection);

            string typeName = ReadContainingTypeName(reflection);

            if (String.IsNullOrEmpty(namespaceName) || HasAttribute(reflection, "System.IgnoreNamespaceAttribute")) {
                return typeName;
            }
            else {
                return String.Format("{0}.{1}", namespaceName, typeName);
            }
        }

        private string ReadMemberName(XPathNavigator reflection) {
            string identifier = (string)reflection.Evaluate(apiNameExpression);

            if (!HasAttribute(reflection, "System.PreserveCaseAttribute")) {
                identifier = CreateCamelCaseName(identifier);
            }

            return identifier;
        }

        private bool HasAttribute(XPathNavigator reflection, string attributeName) {
            attributeName = "T:" + attributeName;

            XPathNodeIterator iterator = (XPathNodeIterator)reflection.Evaluate(apiAttributesExpression);
            foreach (XPathNavigator navigator in iterator) {
                XPathNavigator reference = navigator.SelectSingleNode(attributeTypeExpression);
                if (reference.GetAttribute("api", string.Empty) == attributeName) {
                    return true;
                }
            }

            return false;
        }

        public static string CreateCamelCaseName(string name) {

            if (String.IsNullOrEmpty(name)) {
                return name;
            }
            
            // Some exceptions that simply need to be special cased
            if (name.Equals("ID", StringComparison.Ordinal)) {
                return "id";
            }
            
            bool hasLowerCase = false;
            int conversionLength = 0;
            
            for (int i = 0; i < name.Length; i++) {
                if (Char.IsUpper(name, i)) {
                    conversionLength++;
                } else {
                    hasLowerCase = true;
                    break;
                }
            }
            
            if (((hasLowerCase == false) && (name.Length != 1)) || (conversionLength == 0)) {
                // Name is all upper case, or all lower case; leave it as-is.
                return name;
            }
            
            if (conversionLength > 1) {
                // Convert the leading uppercase segment, except the last character
                // which is assumed to be the first letter of the next word
                return name.Substring(0, conversionLength - 1).ToLower(CultureInfo.InvariantCulture) + name.Substring(conversionLength - 1);
            }

            else if (name.Length == 1) {
                return name.ToLower(CultureInfo.InvariantCulture);
            }

            else {
                // Convert the leading upper case character to lower case
                return Char.ToLower(name[0], CultureInfo.InvariantCulture) + name.Substring(1);
            }
        }
                
        private void WriteIndentedNewLine(SyntaxWriter writer) {
            writer.WriteString(",");
            writer.WriteLine();
            writer.WriteString("\t");
        }

        private void WriteParameterList(XPathNavigator reflection, SyntaxWriter writer) {
            XPathNodeIterator parameters = reflection.Select(apiParametersExpression);
            writer.WriteString("(");

            while (parameters.MoveNext()) {
                XPathNavigator parameter = parameters.Current;

                WriteParameter(parameter, writer);
                if (parameters.CurrentPosition < parameters.Count) {
                    writer.WriteString(", ");
                }
            }
            writer.WriteString(")");
        }

        private void WriteParameter(XPathNavigator parameter, SyntaxWriter writer) {
            string text = (string)parameter.Evaluate(parameterNameExpression);
            XPathNavigator reference = parameter.SelectSingleNode(parameterTypeExpression);
            if ((bool)parameter.Evaluate(parameterIsParamArrayExpression)) {
                writer.WriteString("... ");
            }
            writer.WriteParameter(text);
        }

        private void WriteTypeReference(XPathNavigator reference, SyntaxWriter writer)
        {
            switch (reference.LocalName)
            {
                case "arrayOf":
                        int rank = Convert.ToInt32(reference.GetAttribute("rank", string.Empty));
                        XPathNavigator navigator = reference.SelectSingleNode(typeExpression);
                        WriteTypeReference(navigator, writer);
                        writer.WriteString("[");
                        for (int i = 1; i < rank; i++) { writer.WriteString(","); }
                        writer.WriteString("]");
                        break;
                case "type":
                        string id = reference.GetAttribute("api", string.Empty);
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

        private void WriteNormalTypeReference(string api, SyntaxWriter writer) {
            switch (api) {
                case "T:System.Byte":
                    writer.WriteReferenceLink(api, "Byte");
                    return;

                case "T:System.SByte":
                    writer.WriteReferenceLink(api, "SByte");
                    return;

                case "T:System.Char":
                    writer.WriteReferenceLink(api, "Char");
                    return;

                case "T:System.Int16":
                    writer.WriteReferenceLink(api, "Int16");
                    return;

                case "T:System.Int32":
                    writer.WriteReferenceLink(api, "Int32");
                    return;

                case "T:System.Int64":
                    writer.WriteReferenceLink(api, "Int64");
                    return;

                case "T:System.UInt16":
                    writer.WriteReferenceLink(api, "UInt16");
                    return;

                case "T:System.UInt32":
                    writer.WriteReferenceLink(api, "UInt32");
                    return;

                case "T:System.UInt64":
                    writer.WriteReferenceLink(api, "UInt64");
                    return;

                case "T:System.Single":
                    writer.WriteReferenceLink(api, "Single");
                    return;

                case "T:System.Double":
                    writer.WriteReferenceLink(api, "Double");
                    return;

                case "T:System.Decimal":
                    writer.WriteReferenceLink(api, "Decimal");
                    return;

                case "T:System.Boolean":
                    writer.WriteReferenceLink(api, "Boolean");
                    return;
            }

            // Remove 'T:'
            string name = api.Substring(2);

            // Strip System namespaces
            if (name.StartsWith("System.")) {
                int idx = name.LastIndexOf('.');
                name = name.Substring(idx + 1);
            }

            writer.WriteReferenceLink(api, name);
        }

        private void WriteRecordSyntax(XPathNavigator reflection, SyntaxWriter writer) {
            string namespaceName = ReadNamespaceName(reflection);

            string typeName = ReadTypeName(reflection);

            writer.WriteString(namespaceName);
            writer.WriteString(".$create_");
            writer.WriteString(typeName);
            writer.WriteString(" = ");
            writer.WriteKeyword("function");
            writer.WriteString("();");
        }

        private void WriteRecordConstructorSyntax(XPathNavigator reflection, SyntaxWriter writer) {
            string namespaceName = ReadNamespaceName(reflection);

            string typeName = ReadContainingTypeName(reflection);

            writer.WriteString(namespaceName);
            writer.WriteString(".$create_");
            writer.WriteString(typeName);
            writer.WriteString(" = ");
            writer.WriteKeyword("function");
            WriteParameterList(reflection, writer);
            writer.WriteString(";");
        }
 
        public override void WriteClassSyntax(XPathNavigator reflection, SyntaxWriter writer) {
            if (IsUnsupported(reflection, writer)) return;
            
            if (HasAttribute(reflection, "System.RecordAttribute")) {
                WriteRecordSyntax(reflection, writer);
                return;
            }

            string typeName = ReadFullTypeName(reflection);

            writer.WriteIdentifier(typeName);
            writer.WriteString(" = ");
            writer.WriteKeyword("function");
            writer.WriteString("();");

            writer.WriteLine();
            writer.WriteLine();

            writer.WriteIdentifier("Type");
            writer.WriteString(".createClass(");
            writer.WriteLine();
            writer.WriteString("\t'");
            writer.WriteString(typeName);
            writer.WriteString("'");

            bool hasBaseClass = false;

            // Write the base class.
            XPathNavigator reference = reflection.SelectSingleNode(apiBaseClassExpression);
            if (!((reference == null) || ((bool)reference.Evaluate(typeIsObjectExpression)))) {
                WriteIndentedNewLine(writer);
                WriteTypeReference(reference, writer);
                hasBaseClass = true;
            }

            // Write the interfaces.
            XPathNodeIterator iterator = reflection.Select(apiImplementedInterfacesExpression);
            if (iterator.Count != 0) {
                if (!hasBaseClass) {
                    WriteIndentedNewLine(writer);
                    writer.WriteString("null");
                }

                WriteIndentedNewLine(writer);

                while (iterator.MoveNext()) {
                    XPathNavigator interfaceRef = iterator.Current;
                    WriteTypeReference(interfaceRef, writer);
                    if (iterator.CurrentPosition < iterator.Count) {
                        WriteIndentedNewLine(writer);
                    }
                }
            }

            writer.WriteString(");");
        }

        public override void WriteConstructorSyntax(XPathNavigator reflection, SyntaxWriter writer) {
            if (IsUnsupported(reflection, writer)) {
                return;
            }

            bool isRecord = (bool)reflection.Evaluate(typeIsRecordExpression);

            if (isRecord) {
                WriteRecordConstructorSyntax(reflection, writer);
                return;
            }

            string typeName = ReadFullContainingTypeName(reflection);

            writer.WriteIdentifier(typeName);
            writer.WriteString(" = ");
            writer.WriteKeyword("function");
            WriteParameterList(reflection, writer);
            writer.WriteString(";");
        }

        public override void WriteNormalMethodSyntax(XPathNavigator reflection, SyntaxWriter writer) {
            if (IsUnsupported(reflection, writer)) return;
            
            if (HasAttribute(reflection, "System.AttachedPropertyAttribute")) {
                WriteAttachedPropertySyntax(reflection, writer);
                return;
            }

            string memberName = ReadMemberName(reflection);

            bool isStatic = (bool)reflection.Evaluate(apiIsStaticExpression);
            bool isGlobal = (bool)reflection.Evaluate(memberIsGlobalExpression);

            if (isStatic && !isGlobal) {
                writer.WriteIdentifier(ReadFullContainingTypeName(reflection));
                writer.WriteString(".");
                writer.WriteIdentifier(memberName);
                writer.WriteString(" = ");
                writer.WriteKeyword("function");
            }
            else {
                writer.WriteKeyword("function");
                writer.WriteString(" ");
                writer.WriteIdentifier(memberName);
            }

            WriteParameterList(reflection, writer);
            writer.WriteString(";");
        }

        public override void WriteDelegateSyntax(XPathNavigator reflection, SyntaxWriter writer) {
            writer.WriteKeyword("function");
            WriteParameterList(reflection, writer);
            writer.WriteString(";");
        }

        public override void WriteEnumerationSyntax(XPathNavigator reflection, SyntaxWriter writer) {
            string typeName = ReadFullTypeName(reflection);

            writer.WriteIdentifier(typeName);
            writer.WriteString(" = ");
            writer.WriteKeyword("function");
            writer.WriteString("();");

            writer.WriteLine();

            writer.WriteIdentifier(typeName);
            writer.WriteString(".createEnum('");
            writer.WriteIdentifier(typeName);
            writer.WriteString("', ");
            writer.WriteString(HasAttribute(reflection, "System.FlagsAttribute") ? "true" : "false");
            writer.WriteString(");");
        }

        public override void WriteEventSyntax(XPathNavigator reflection, SyntaxWriter writer) {
            if (IsUnsupported(reflection, writer)) return;
            
            if (reflection.Select(apiParametersExpression).Count > 0) {
                writer.WriteMessage("UnsupportedIndex_" + Language);
                return;
            }

            string memberName = ReadMemberName(reflection);

            writer.WriteKeyword("function");
            writer.WriteString(" add_");
            writer.WriteIdentifier(memberName);
            writer.WriteString("(");
            writer.WriteParameter("value");
            writer.WriteString(");");

            writer.WriteLine();

            writer.WriteKeyword("function");
            writer.WriteString(" remove_");
            writer.WriteIdentifier(memberName);
            writer.WriteString("(");
            writer.WriteParameter("value");
            writer.WriteString(");");
        }

        public override void WriteFieldSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if (IsUnsupported(reflection, writer)) {
                return;
            }

            string memberName = ReadMemberName(reflection);

            bool isStatic = (bool)reflection.Evaluate(apiIsStaticExpression);
            if (isStatic) {
                string typeName = ReadFullContainingTypeName(reflection);

                writer.WriteIdentifier(typeName);
                writer.WriteString(".");
            }

            writer.WriteIdentifier(memberName);
        }

        public override void WriteInterfaceSyntax(XPathNavigator reflection, SyntaxWriter writer) {
            if (IsUnsupported(reflection, writer)) return;
            
            string typeName = ReadFullTypeName(reflection);

            writer.WriteIdentifier(typeName);
            writer.WriteString(" = ");
            writer.WriteKeyword("function");
            writer.WriteString("();");

            writer.WriteLine();

            writer.WriteIdentifier(typeName);
            writer.WriteString(".createInterface('");
            writer.WriteIdentifier(typeName);
            writer.WriteString("');");
        }

        public override void WriteAttachedEventSyntax(XPathNavigator reflection, SyntaxWriter writer) {
            // Not supported
        }

        public override void WriteAttachedPropertySyntax(XPathNavigator reflection, SyntaxWriter writer) {
            string typeName = ReadContainingTypeName(reflection);

            string methodName = ReadMemberName(reflection);

            string propertyName = String.Format("{0}.{1}", typeName, methodName.Substring(3));

            if (methodName.StartsWith("Get", StringComparison.OrdinalIgnoreCase)) {
                writer.WriteKeyword("var");
                writer.WriteString(" value = obj['");
                writer.WriteString(propertyName);
                writer.WriteString("'];");
            }
            else if (methodName.StartsWith("Set", StringComparison.OrdinalIgnoreCase)) {
                writer.WriteString("obj['");
                writer.WriteString(propertyName);
                writer.WriteString("'] = value;");                
            }
        }

        public override void WriteOperatorSyntax(XPathNavigator reflection, SyntaxWriter writer) {
            writer.WriteMessage("UnsupportedOperator_" + Language);
        }

        public override void WriteCastSyntax(XPathNavigator reflection, SyntaxWriter writer) {
            writer.WriteMessage("UnsupportedCast_" + Language);
        }

        public override void WriteNamespaceSyntax(XPathNavigator reflection, SyntaxWriter writer) {
            string name = reflection.Evaluate(apiNameExpression).ToString();

            writer.WriteString("Type.createNamespace('");
            writer.WriteIdentifier(name);
            writer.WriteString("');");
        }

        public override void WritePropertySyntax(XPathNavigator reflection, SyntaxWriter writer) {
            if (IsUnsupported(reflection, writer)) return;
            
            if (HasAttribute(reflection, "System.IntrinsicPropertyAttribute")) {
                WriteFieldSyntax(reflection, writer);
                return;
            }

            string memberName = ReadMemberName(reflection);

            bool isStatic = (bool)reflection.Evaluate(apiIsStaticExpression);

            bool isGetter = (bool)reflection.Evaluate(apiIsReadPropertyExpression);
            bool isSetter = (bool)reflection.Evaluate(apiIsWritePropertyExpression);

            XPathNavigator reference = reflection.SelectSingleNode(apiReturnTypeExpression);

            if (isGetter) {
                if (isStatic) {
                    writer.WriteIdentifier(ReadFullContainingTypeName(reflection));
                    writer.WriteString(".");

                    writer.WriteString("get_");
                    writer.WriteIdentifier(memberName);
                    writer.WriteString(" = ");
                    writer.WriteKeyword("function");
                }
                else {
                    writer.WriteKeyword("function");
                    writer.WriteString(" ");

                    writer.WriteString("get_");
                    writer.WriteIdentifier(memberName);
                }

                WriteParameterList(reflection, writer);
                writer.WriteString(";");
                writer.WriteLine();
            }

            if (isSetter) {
                if (isStatic) {
                    writer.WriteIdentifier(ReadFullContainingTypeName(reflection));
                    writer.WriteString(".");

                    writer.WriteString("set_");
                    writer.WriteIdentifier(memberName);
                    writer.WriteString(" = ");
                    writer.WriteKeyword("function");
                }
                else {
                    writer.WriteKeyword("function");
                    writer.WriteString(" ");

                    writer.WriteString("set_");
                    writer.WriteIdentifier(memberName);
                }

                writer.WriteString("(");
                writer.WriteParameter("value");
                writer.WriteString(");");
            }
        }

        public override void WriteStructureSyntax(XPathNavigator reflection, SyntaxWriter writer) {
            if (IsUnsupported(reflection, writer)) return;
            writer.WriteMessage("UnsupportedStructure_" + Language);
        }
   }
}