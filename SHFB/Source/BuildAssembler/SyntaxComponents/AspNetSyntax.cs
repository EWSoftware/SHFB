// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 12/20/2013 - EFW - Updated the syntax generator to be discoverable via MEF
// 08/01/2014 - EFW - Added support for resource item files containing the localized titles, messages, etc.

using System;
using System.IO;
using System.Reflection;
using System.Xml.XPath;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler.SyntaxGenerator;

namespace Sandcastle.Tools.SyntaxGenerators
{
    /// <summary>
    /// This class generates declaration syntax sections for ASP.NET
    /// </summary>
    public sealed class AspNetSyntaxGenerator : SyntaxGeneratorCore
    {
        #region Syntax generator factory for MEF
        //=====================================================================

        private const string LanguageName = "AspNet", StyleIdName = "asp";

        /// <summary>
        /// This is used to create a new instance of the syntax generator
        /// </summary>
        [SyntaxGeneratorExport("ASP.NET", LanguageName, StyleIdName, AlternateIds = "AspNet, asp", SortOrder = 100,
          Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
          Description = "Generates ASP.NET declaration syntax sections")]
        public sealed class Factory : ISyntaxGeneratorFactory
        {
            /// <inheritdoc />
            public string ResourceItemFileLocation => Path.Combine(ComponentUtilities.AssemblyFolder(
                Assembly.GetExecutingAssembly()), "SyntaxContent");

            /// <inheritdoc />
            public SyntaxGeneratorCore Create()
            {
                return new AspNetSyntaxGenerator();
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private static readonly XPathExpression nameExpression = XPathExpression.Compile("string(apidata/@name)");
        private static readonly XPathExpression groupExpression = XPathExpression.Compile("string(apidata/@group)");
        private static readonly XPathExpression subgroupExpression = XPathExpression.Compile("string(apidata/@subgroup)");

        private static readonly XPathExpression containingTypeExpression = XPathExpression.Compile("containers/type");
        private static readonly XPathExpression declaringTypeExpression = XPathExpression.Compile("string(containers/type/@api)");
        private static readonly XPathExpression propertyTypeExpression = XPathExpression.Compile("string(returns/type/@api)");
        private static readonly XPathExpression propertyIsSettable = XPathExpression.Compile("boolean(propertydata/@set='true')");
        private static readonly XPathExpression eventHandlerTypeExpression = XPathExpression.Compile("string(eventhandler/type/@api)");

        private static readonly XPathExpression typeIsWebControl = XPathExpression.Compile(
            "boolean(family/ancestors/type[@api='T:System.Web.UI.Control'])");

        private static readonly XPathExpression propertyIsInnerProperty = XPathExpression.Compile(
            "boolean(attributes/attribute[type/@api='T:System.Web.UI.PersistenceModeAttribute' and argument/enumValue/field/@name='InnerProperty'])");

        private static readonly XPathExpression containingNamespaceExpression = XPathExpression.Compile("string(containers/namespace/@api)");

        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// Initialize the syntax generator
        /// </summary>
        /// <param name="configuration">The syntax generator configuration</param>
        /// <remarks>This component has no configurable elements</remarks>
        public override void Initialize(XPathNavigator configuration)
        {
        }

        /// <inheritdoc />
        public override void WriteSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            if(reflection == null)
                throw new ArgumentNullException(nameof(reflection));

            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            string group = (string)reflection.Evaluate(groupExpression);
            string subgroup = (string)reflection.Evaluate(subgroupExpression);

            if(group == "type" && subgroup == "class")
            {
                string prefix = WebControlPrefix(reflection);

                if(!String.IsNullOrEmpty(prefix))
                    WriteClassSyntax(reflection, writer, prefix);
            }

            if(group == "member")
            {
                string prefix = null;
                XPathNavigator containingType = reflection.SelectSingleNode(containingTypeExpression);

                if(containingType != null)
                    prefix = WebControlPrefix(containingType);

                if(!String.IsNullOrEmpty(prefix))
                    if(subgroup == "property")
                        WritePropertySyntax(reflection, writer, prefix);
                    else
                        if(subgroup == "event")
                        WriteEventSyntax(reflection, writer, prefix);
            }
        }

        /// <summary>
        /// This is used to get the web control prefix based on the given type
        /// </summary>
        /// <param name="reflection">An XPath navigator containing the type information</param>
        /// <returns>The web control prefix or null if the type is not a web control</returns>
        private static string WebControlPrefix(XPathNavigator reflection)
        {
            if((bool)reflection.Evaluate(typeIsWebControl))
            {
                string name = (string)reflection.Evaluate(nameExpression);
                string space = (string)reflection.Evaluate(containingNamespaceExpression);

                if(space == "N:System.Web.UI" && (name == "Page" || name == "ScriptControl" || name == "UserControl"))
                    return null;

                if(space == "N:System.Web.UI.MobileControls")
                    return "mobile";

                return "asp";
            }

            return null;
        }

        /// <summary>
        /// Write out class syntax
        /// </summary>
        /// <param name="reflection">An XPath navigator containing the member information</param>
        /// <param name="writer">The syntax writer to which the information is written</param>
        /// <param name="prefix">The web control prefix to use</param>
        private static void WriteClassSyntax(XPathNavigator reflection, SyntaxWriter writer, string prefix)
        {
            string name = (string)reflection.Evaluate(nameExpression);

            writer.WriteStartBlock(LanguageName, StyleIdName);

            writer.WriteString("<");
            writer.WriteString(prefix);
            writer.WriteString(":");
            writer.WriteString(name);
            writer.WriteString(" />");

            writer.WriteEndBlock();
        }

        /// <summary>
        /// Write out property syntax
        /// </summary>
        /// <param name="reflection">An XPath navigator containing the member information</param>
        /// <param name="writer">The syntax writer to which the information is written</param>
        /// <param name="prefix">The web control prefix to use</param>
        private static void WritePropertySyntax(XPathNavigator reflection, SyntaxWriter writer, string prefix)
        {
            bool set = (bool)reflection.Evaluate(propertyIsSettable);

            if(!set)
                return;

            string name = (string)reflection.Evaluate(nameExpression);
            string declaringType = (string)reflection.Evaluate(declaringTypeExpression);
            string propertyType = (string)reflection.Evaluate(propertyTypeExpression);

            bool isInnerProperty = (bool)reflection.Evaluate(propertyIsInnerProperty);

            writer.WriteStartBlock(LanguageName, StyleIdName);

            if(isInnerProperty)
            {
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

                if(String.IsNullOrEmpty(propertyType))
                    writer.WriteParameter("value");
                else
                    if(propertyType == "T:System.Boolean")
                    writer.WriteString("True|False");
                else
                    writer.WriteReferenceLink(propertyType);

                writer.WriteString("</");
                writer.WriteString(name);
                writer.WriteString(">");

                writer.WriteLine();

                writer.WriteString("</");
                writer.WriteString(prefix);
                writer.WriteString(":");
                writer.WriteReferenceLink(declaringType);
                writer.WriteString(">");
            }
            else
            {
                // normal property logic
                writer.WriteString("<");
                writer.WriteString(prefix);
                writer.WriteString(":");
                writer.WriteReferenceLink(declaringType);
                writer.WriteString(" ");
                writer.WriteString(name);
                writer.WriteString("=\"");

                if(String.IsNullOrEmpty(propertyType))
                    writer.WriteParameter("value");
                else
                    if(propertyType == "T:System.Boolean")
                    writer.WriteString("True|False");
                else
                    writer.WriteReferenceLink(propertyType);

                writer.WriteString("\" />");
            }

            writer.WriteEndBlock();
        }

        /// <summary>
        /// Write out event syntax
        /// </summary>
        /// <param name="reflection">An XPath navigator containing the member information</param>
        /// <param name="writer">The syntax writer to which the information is written</param>
        /// <param name="prefix">The web control prefix to use</param>
        private static void WriteEventSyntax(XPathNavigator reflection, SyntaxWriter writer, string prefix)
        {
            string name = (string)reflection.Evaluate(nameExpression);
            string declaringType = (string)reflection.Evaluate(declaringTypeExpression);
            string handlerType = (string)reflection.Evaluate(eventHandlerTypeExpression);

            writer.WriteStartBlock(LanguageName, StyleIdName);

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
        #endregion
    }
}
