// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools {


    public class AspNetSyntaxGenerator : SyntaxGenerator {

        public AspNetSyntaxGenerator (XPathNavigator configuration) : base(configuration) {
        }

        protected static XPathExpression nameExpression = XPathExpression.Compile("string(apidata/@name)");
        protected static XPathExpression groupExpression = XPathExpression.Compile("string(apidata/@group)");
        protected static XPathExpression subgroupExpression = XPathExpression.Compile("string(apidata/@subgroup)");

        protected static XPathExpression containingTypeExpression = XPathExpression.Compile("containers/type");
        protected static XPathExpression declaringTypeExpression = XPathExpression.Compile("string(containers/type/@api)");
        protected static XPathExpression propertyTypeExpression = XPathExpression.Compile("string(returns/type/@api)");
        protected static XPathExpression propertyIsSettable = XPathExpression.Compile("boolean(propertydata/@set='true')");
        protected static XPathExpression eventHandlerTypeExpression = XPathExpression.Compile("string(eventhandler/type/@api)");

        protected static XPathExpression typeIsWebControl = XPathExpression.Compile("boolean(family/ancestors/type[@api='T:System.Web.UI.Control'])");

        protected static XPathExpression propertyIsInnerProperty = XPathExpression.Compile("boolean(attributes/attribute[type/@api='T:System.Web.UI.PersistenceModeAttribute' and argument/enumValue/field/@name='InnerProperty'])");

        protected static XPathExpression containingNamespaceExpression = XPathExpression.Compile("string(containers/namespace/@api)");

        private string language = "AspNet";

        public string Language {
            get {
                return (language);
            }
        }

        public override void WriteSyntax (XPathNavigator reflection, SyntaxWriter writer) {

            string group = (string)reflection.Evaluate(groupExpression);
            string subgroup = (string)reflection.Evaluate(subgroupExpression);

            if (group == "type" && subgroup == "class") {
                string prefix = WebControlPrefix(reflection);
                if (!String.IsNullOrEmpty(prefix)) {
                    WriteClassSyntax(reflection, writer, prefix);
                }
            }

            if (group == "member") {

                string prefix = null;
                XPathNavigator containingType = reflection.SelectSingleNode(containingTypeExpression);
                if (containingType != null) prefix = WebControlPrefix(containingType);

                if (!String.IsNullOrEmpty(prefix)) {
                    if (subgroup == "property") {
                        WritePropertySyntax(reflection, writer, prefix);
                    } else if (subgroup == "event") {
                        WriteEventSyntax(reflection, writer, prefix);
                    }
                }
            }


        }

        private string WebControlPrefix (XPathNavigator reflection) {
            if ((bool)reflection.Evaluate(typeIsWebControl)) {
                string name = (string)reflection.Evaluate(nameExpression);
                string space = (string)reflection.Evaluate(containingNamespaceExpression);
                if ((space == "N:System.Web.UI") && ((name == "Page") || (name == "ScriptControl") || (name == "UserControl"))) {
                    return (null);
                } else {
                    if (space == "N:System.Web.UI.MobileControls") {
                        return ("mobile");
                    } else {
                        return ("asp");
                    }
                }
            } else {
                return (null);
            }
        }

        private void WriteClassSyntax (XPathNavigator reflection, SyntaxWriter writer, string prefix) {

            string name = (string)reflection.Evaluate(nameExpression);

            writer.WriteStartBlock(Language);

            writer.WriteString("<");
            writer.WriteString(prefix);
            writer.WriteString(":");
            writer.WriteString(name);
            writer.WriteString(" />");

            writer.WriteEndBlock();

        }

        private void WritePropertySyntax (XPathNavigator reflection, SyntaxWriter writer, string prefix) {

            bool set = (bool) reflection.Evaluate(propertyIsSettable);
            if (!set) return;

            string name = (string) reflection.Evaluate(nameExpression);
            string declaringType = (string) reflection.Evaluate(declaringTypeExpression);
            string propertyType = (string) reflection.Evaluate(propertyTypeExpression);

            bool isInnerProperty = (bool)reflection.Evaluate(propertyIsInnerProperty);

            writer.WriteStartBlock(Language);

            if (isInnerProperty) {

                // inner property logic

                writer.WriteString("<");
                writer.WriteString(prefix);
                writer.WriteString(":");
                writer.WriteReferenceLink(declaringType);
                writer.WriteString(">");

                writer.WriteLine();
                writer.WriteString("\t");

                writer.WriteString("<");
                writer.WriteString(name);
                writer.WriteString(">");
                if (String.IsNullOrEmpty(propertyType)) {
                    writer.WriteParameter("value");
                } else {
                    if (propertyType == "T:System.Boolean") {
                        writer.WriteString("True|False");
                    } else {
                        writer.WriteReferenceLink(propertyType);
                    }
                }
                writer.WriteString("</");
                writer.WriteString(name);
                writer.WriteString(">");

                writer.WriteLine();

                writer.WriteString("</");
                writer.WriteString(prefix);
                writer.WriteString(":");
                writer.WriteReferenceLink(declaringType);
                writer.WriteString(">");

            } else {

                // normal property logic

                writer.WriteString("<");
                writer.WriteString(prefix);
                writer.WriteString(":");
                writer.WriteReferenceLink(declaringType);
                writer.WriteString(" ");
                writer.WriteString(name);
                writer.WriteString("=\"");
                if (String.IsNullOrEmpty(propertyType)) {
                    writer.WriteParameter("value");
                } else {
                    if (propertyType == "T:System.Boolean") {
                        writer.WriteString("True|False");
                    } else {
                        writer.WriteReferenceLink(propertyType);
                    }
                }
                writer.WriteString("\" />");

            }

            writer.WriteEndBlock();

        }

        private void WriteEventSyntax (XPathNavigator reflection, SyntaxWriter writer, string prefix) {

            string name = (string)reflection.Evaluate(nameExpression);
            string declaringType = (string)reflection.Evaluate(declaringTypeExpression);
            string handlerType = (string)reflection.Evaluate(eventHandlerTypeExpression);

            writer.WriteStartBlock(Language);

            writer.WriteString("<");
            writer.WriteString(prefix);
            writer.WriteString(":");
            writer.WriteReferenceLink(declaringType);
            writer.WriteString(" ");
            writer.WriteString("On");
            writer.WriteString(name);
            writer.WriteString("=\"");
            writer.WriteReferenceLink(handlerType);
            writer.WriteString("\" />");

            writer.WriteEndBlock();

        }

    }

}
