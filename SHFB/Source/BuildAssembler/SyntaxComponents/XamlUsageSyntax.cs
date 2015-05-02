// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 03/28/2012 - EFW - Fixed WritePropertySyntax() so that it generates syntax for properties with
// abstract return types as long as there is a type converter for it (i.e. Brush).
// 12/23/2012 - EFW - Made the xamlAssemblies dictionary use case-insensitive key comparisons
// 12/20/2013 - EFW - Updated the syntax generator to be discoverable via MEF
// 08/01/2014 - EFW - Added support for resource item files containing the localized titles, messages, etc.
// 12/03/2014 - EFW - Fixed up some problems that were causing it to skip or add XAML info incorrectly

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml.XPath;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler.SyntaxGenerator;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This class generates usage syntax sections for XAML
    /// </summary>
    public sealed class XamlUsageSyntaxGenerator : SyntaxGeneratorTemplate
    {
        #region Syntax generator factory for MEF
        //=====================================================================

        private const string LanguageName = "XAML", StyleIdName = "xaml";

        /// <summary>
        /// This is used to create a new instance of the syntax generator
        /// </summary>
        [SyntaxGeneratorExport("XAML Usage", LanguageName, StyleIdName, AlternateIds = "XamlUsage, xaml",
          SortOrder = 90, Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
          Description = "Generates XAML usage syntax sections",
          DefaultConfiguration="<filter files=\"{@SHFBFolder}PresentationStyles\\Shared\\configuration\\xamlSyntax.config\" />\r\n" +
			"{@XamlConfigFiles}")]
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
                return new XamlUsageSyntaxGenerator { Language = LanguageName, StyleId = StyleIdName };
            }
        }
        #endregion

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            base.Initialize(configuration);

            this.LoadConfigNode(configuration);
        }

        /// <inheritdoc />
        public override void WriteSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            writer.WriteStartBlock(this.Language, this.StyleId);

            // Check the list of assemblies for which to generate XAML syntax
            string assemblyName = (string)reflection.Evaluate(apiContainingAssemblyExpression);
            string namespaceName = (string)reflection.Evaluate(apiContainingNamespaceNameExpression);

            if(!xamlAssemblies.ContainsKey(assemblyName))
            {
                WriteXamlBoilerplate(XamlBoilerplateID.nonXamlAssemblyBoilerplate, writer);
            }
            else
            {
                string group = (string)reflection.Evaluate(apiGroupExpression);
                switch(group)
                {
                    case "namespace":
                        WriteNamespaceSyntax(reflection, writer);
                        break;
                    case "type":
                        WriteTypeSyntax(reflection, writer);
                        break;
                    case "member":
                        WriteMemberSyntax(reflection, writer);
                        break;
                }
                WriteXamlXmlnsUri(assemblyName, namespaceName, writer);
            }

            writer.WriteEndBlock();
        }

        private void WriteXamlXmlnsUri(string assemblyName, string namespaceName, SyntaxWriter writer)
        {
            Dictionary<string, List<string>> clrNamespaces;

            if(xamlAssemblies.TryGetValue(assemblyName, out clrNamespaces))
            {
                List<string> xmlnsUriList;
                if(clrNamespaces.TryGetValue(namespaceName, out xmlnsUriList))
                {
                    foreach(string xmlnsUri in xmlnsUriList)
                    {
                        // start the syntax block
                        writer.WriteStartSubBlock("xamlXmlnsUri");
                        writer.WriteString(xmlnsUri);
                        writer.WriteEndSubBlock();
                    }
                }
            }
        }

        // list of classes whose subclasses do NOT get XAML syntax
        private List<string> excludedAncestorList = new List<string>();

        // List of assemblies whose members get XAML syntax.  The assembly name key is compared case-insensitively.
        // The nested dictionary is a list of assembly namespaces that have one or more xmlns uris for xaml.
        private Dictionary<string, Dictionary<string, List<string>>> xamlAssemblies =
            new Dictionary<string, Dictionary<string, List<string>>>(StringComparer.OrdinalIgnoreCase);

        private void LoadConfigNode(XPathNavigator configuration)
        {
            // get the filter files
            XPathNodeIterator filterNodes = configuration.Select("filter");

            if(filterNodes.Count == 0)
            {
                LoadConfiguration(configuration);
                return;
            }

            foreach(XPathNavigator filterNode in filterNodes)
            {
                string filterFiles = filterNode.GetAttribute("files", String.Empty);

                if((filterFiles == null) || (filterFiles.Length == 0))
                    throw new InvalidOperationException("The XamlUsageSyntaxGenerator filter/@files attribute must specify a path.");

                ParseDocuments(filterFiles);
            }
        }

        private void LoadConfiguration(XPathNavigator configuration)
        {
            // get the list of excluded ancestor classes
            foreach(XPathNavigator excludedClass in configuration.Select("xamlExcludedAncestors/class"))
            {
                string apiId = excludedClass.GetAttribute("api", string.Empty);
                if(apiId.Length > 0 && !excludedAncestorList.Contains(apiId))
                    excludedAncestorList.Add(apiId);
            }

            // get the list of XAML assemblies; members in other assemblies get no xaml syntax, just 'not applicable' boilerplate
            foreach(XPathNavigator xamlAssembly in configuration.Select("xamlAssemblies/assembly"))
            {
                string assemblyName = xamlAssembly.GetAttribute("name", string.Empty);
                if(string.IsNullOrEmpty(assemblyName))
                    continue; // should emit warning message

                Dictionary<string, List<string>> clrNamespaces;
                if(!xamlAssemblies.TryGetValue(assemblyName, out clrNamespaces))
                {
                    clrNamespaces = new Dictionary<string, List<string>>();
                    xamlAssemblies.Add(assemblyName, clrNamespaces);
                }

                foreach(XPathNavigator xmlnsNode in xamlAssembly.Select("xmlns[@uri][clrNamespace]"))
                {
                    string xmlnsUri = xmlnsNode.GetAttribute("uri", string.Empty);
                    if(string.IsNullOrEmpty(xmlnsUri))
                        continue; // should emit warning message

                    foreach(XPathNavigator clrNamespaceNode in xmlnsNode.Select("clrNamespace[@name]"))
                    {
                        string namespaceName = clrNamespaceNode.GetAttribute("name", string.Empty);
                        if(string.IsNullOrEmpty(namespaceName))
                            continue; // should emit warning message

                        List<string> xmlnsUriList;
                        if(!clrNamespaces.TryGetValue(namespaceName, out xmlnsUriList))
                        {
                            xmlnsUriList = new List<string>();
                            clrNamespaces.Add(namespaceName, xmlnsUriList);
                        }
                        if(!xmlnsUriList.Contains(xmlnsUri))
                            xmlnsUriList.Add(xmlnsUri);
                    }
                }
            }
        }

        /// <summary>
        /// This is used to parse configuration files
        /// </summary>
        /// <param name="wildcardPath">The path used to find the configuration files</param>
        public void ParseDocuments(string wildcardPath)
        {
            string filterFiles = Environment.ExpandEnvironmentVariables(wildcardPath);

            if(filterFiles == null || filterFiles.Length == 0)
                throw new InvalidOperationException("The XamlUsageSyntaxGenerator filter path is an empty string.");

            string directoryPart = Path.GetDirectoryName(filterFiles);

            if(String.IsNullOrEmpty(directoryPart))
                directoryPart = Environment.CurrentDirectory;

            directoryPart = Path.GetFullPath(directoryPart);
            string filePart = Path.GetFileName(filterFiles);

            foreach(string file in Directory.EnumerateFiles(directoryPart, filePart))
                ParseDocument(file);
        }

        private void ParseDocument(string file)
        {
            try
            {
                XPathDocument document = new XPathDocument(file);

                XPathNavigator xamlSyntaxNode = document.CreateNavigator().SelectSingleNode("/*");
                LoadConfiguration(xamlSyntaxNode);
            }
            catch(Exception e)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    "Exception parsing XamlUsageSyntaxGenerator filter file: {0}. Exception message: {1}", file,
                    e.Message));
            }
        }

        /// <inheritdoc />
        public override void WriteNamespaceSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            // empty xaml syntax for namespace topics
        }

        private void WriteXamlBoilerplate(XamlBoilerplateID bpID, SyntaxWriter writer)
        {
            WriteXamlBoilerplate(bpID, null, writer);
        }

        private void WriteXamlBoilerplate(XamlBoilerplateID bpID, XPathNavigator typeReflection, SyntaxWriter writer)
        {
            string xamlBlockId = System.Enum.GetName(typeof(XamlBoilerplateID), bpID);
            if(xamlBlockId != null)
            {
                writer.WriteStartSubBlock(xamlBlockId);
                if(typeReflection != null)
                    WriteTypeReference(typeReflection, writer);
                writer.WriteEndSubBlock();
            }
        }

        /// <inheritdoc />
        public override void WriteClassSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            bool isAbstract = (bool)reflection.Evaluate(apiIsAbstractTypeExpression);
            bool isSealed = (bool)reflection.Evaluate(apiIsSealedTypeExpression);

            if(isAbstract && !isSealed)
            {
                // Output boilerplate for abstract class 
                WriteXamlBoilerplate(XamlBoilerplateID.classXamlSyntax_abstract, writer);
            }
            else if(!HasDefaultConstructor(reflection))
            {
                if(HasTypeConverterAttribute(reflection))
                    WriteXamlBoilerplate(XamlBoilerplateID.classXamlSyntax_noDefaultCtorWithTypeConverter, writer);
                else
                    WriteXamlBoilerplate(XamlBoilerplateID.classXamlSyntax_noDefaultCtor, writer);
            }
            else if(IsExcludedSubClass(reflection))
            {
                WriteXamlBoilerplate(XamlBoilerplateID.classXamlSyntax_excludedSubClass, writer);
            }
            else
            {
                // show the default XAML syntax for classes
                // Note: skipped the test for TypeConverterAttribute shown in the flowchart because same syntax either way
                ObjectElementUsageForClassStruct(reflection, writer);
            }
        }

        private void ObjectElementUsageForClassStruct(XPathNavigator reflection, SyntaxWriter writer)
        {
            string typeName = (string)reflection.Evaluate(apiNameExpression);
            bool isGeneric = (bool)reflection.Evaluate(apiIsGenericExpression);
            string xamlBlockId = System.Enum.GetName(typeof(XamlHeadingID), XamlHeadingID.xamlObjectElementUsageHeading);

            string contentPropertyId = (string)reflection.Evaluate(contentPropertyIdExpression);

            if(String.IsNullOrEmpty(contentPropertyId))
                contentPropertyId = (string)reflection.Evaluate(ancestorContentPropertyIdExpression);

            // start the syntax block
            writer.WriteStartSubBlock(xamlBlockId);

            writer.WriteString("<");

            if(isGeneric)
            {
                writer.WriteIdentifier(typeName);

                // for generic types show the type arguments
                XPathNodeIterator templates = (XPathNodeIterator)reflection.Evaluate(apiTemplatesExpression);

                if(templates.Count > 0)
                {
                    writer.WriteString(" x:TypeArguments=\"");

                    while(templates.MoveNext())
                    {
                        XPathNavigator template = templates.Current;
                        string name = template.GetAttribute("name", String.Empty);
                        writer.WriteString(name);

                        if(templates.CurrentPosition < templates.Count)
                            writer.WriteString(",");
                    }

                    writer.WriteString("\"");
                }
            }
            else
            {
                // for non-generic types just show the name
                writer.WriteIdentifier(typeName);
            }

            if(String.IsNullOrEmpty(contentPropertyId))
                writer.WriteString(" .../>");
            else
            {
                // close the start tag
                writer.WriteString(">");

                // the inner xml of the Object Element syntax for a type with a content property
                // is a link to the content property
                writer.WriteLine();
                writer.WriteString("  ");
                writer.WriteReferenceLink(contentPropertyId);
                writer.WriteLine();

                // write the end tag
                writer.WriteString("</");
                writer.WriteIdentifier(typeName);
                writer.WriteString(">");
            }

            // end the sub block
            writer.WriteEndSubBlock();
        }

        /// <inheritdoc />
        public override void WriteStructureSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            bool notWriteable = (bool)reflection.Evaluate(noSettablePropertiesExpression);

            if(notWriteable)
            {
                // Output boilerplate for struct with no writeable properties 
                WriteXamlBoilerplate(XamlBoilerplateID.structXamlSyntax_nonXaml, writer);
            }
            else
            {
                // All writeable structs in XAML assemblies are usable in XAML
                // always show the Object Element Usage syntax
                ObjectElementUsageForClassStruct(reflection, writer);

                // For structs with TypeConverterAttribute,
                // if we can show multiple syntax blocks, also output AttributeUsage boilerplate
                if(HasTypeConverterAttribute(reflection))
                    WriteXamlBoilerplate(XamlBoilerplateID.structXamlSyntax_attributeUsage, writer);
            }
        }

        /// <inheritdoc />
        public override void WriteInterfaceSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            WriteXamlBoilerplate(XamlBoilerplateID.interfaceOverviewXamlSyntax, writer);
        }

        /// <inheritdoc />
        public override void WriteDelegateSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            WriteXamlBoilerplate(XamlBoilerplateID.delegateOverviewXamlSyntax, writer);
        }

        /// <inheritdoc />
        public override void WriteEnumerationSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            WriteXamlBoilerplate(XamlBoilerplateID.enumerationOverviewXamlSyntax, writer);
        }

        /// <inheritdoc />
        public override void WriteConstructorSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            WriteXamlBoilerplate(XamlBoilerplateID.constructorOverviewXamlSyntax, writer);
        }

        /// <inheritdoc />
        public override void WriteMethodSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            WriteXamlBoilerplate(XamlBoilerplateID.methodOverviewXamlSyntax, writer);
        }

        /// <inheritdoc />
        public override void WriteAttachedPropertySyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string propertyName = (string)reflection.Evaluate(apiNameExpression);
            string containingTypeName = (string)reflection.Evaluate(apiContainingTypeNameExpression);
            bool isSettable = (bool)reflection.Evaluate(apiIsWritePropertyExpression);
            XPathNavigator returnType = reflection.SelectSingleNode(apiReturnTypeExpression);
            if(!isSettable)
            {
                WriteXamlBoilerplate(XamlBoilerplateID.propertyXamlSyntax_readOnly, writer);
            }
            else
            {
                // xaml syntax block for attached property
                string xamlBlockId = System.Enum.GetName(typeof(XamlHeadingID), XamlHeadingID.xamlAttributeUsageHeading);
                writer.WriteStartSubBlock(xamlBlockId);
                writer.WriteString("<");
                writer.WriteParameter("object ");
                writer.WriteIdentifier(containingTypeName + "." + propertyName);
                writer.WriteString("=\"");
                WriteTypeReference(returnType, writer);
                writer.WriteString("\" .../>");
                writer.WriteEndSubBlock();
            }
        }

        /// <inheritdoc />
        public override void WritePropertySyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            bool isSettable = (bool)reflection.Evaluate(apiIsWritePropertyExpression);
            bool isSetterPublic = (bool)reflection.Evaluate(apiIsSetterPublicExpression);
            bool isAbstract = (bool)reflection.Evaluate(apiIsAbstractProcedureExpression);
            string propertyVisibility = (string)reflection.Evaluate(apiVisibilityExpression);
            XPathNodeIterator parameters = reflection.Select(apiParametersExpression);

            XPathNavigator returnType = reflection.SelectSingleNode(apiReturnTypeExpression);
            bool notWriteableReturnType = (bool)returnType.Evaluate(noSettablePropertiesExpression);
            string returnTypeId = returnType.GetAttribute("api", string.Empty);
            string returnTypeSubgroup = (string)returnType.Evaluate(apiSubgroupExpression);
            bool returnTypeIsAbstract = (bool)returnType.Evaluate(apiIsAbstractTypeExpression);
            bool returnTypeIsReadonlyStruct = (returnTypeSubgroup == "structure" && notWriteableReturnType &&
                !IsPrimitiveType(returnTypeId));

            XPathNavigator containingType = reflection.SelectSingleNode(apiContainingTypeExpression);
            string containingTypeSubgroup = (string)containingType.Evaluate(apiSubgroupExpression);

            // an ordinary property, not an attached prop
            if(containingTypeSubgroup == "interface")
            {
                WriteXamlBoilerplate(XamlBoilerplateID.propertyXamlSyntax_noXamlSyntaxForInterfaceMembers, writer);
            }
            else if((bool)containingType.Evaluate(apiIsAbstractTypeExpression) &&
                    (bool)containingType.Evaluate(apiIsSealedTypeExpression))
            {
                // the property's containing type is static if it's abstract and sealed
                // members of a static class cannot be used in XAML.
                WriteXamlBoilerplate(XamlBoilerplateID.propertyXamlSyntax_nonXamlParent, writer);
            }
            else if(IsExcludedSubClass(containingType))
            {
                WriteXamlBoilerplate(XamlBoilerplateID.propertyXamlSyntax_parentIsExcludedSubClass, writer);
            }
            else if(!DoesParentSupportXaml(reflection))
            {
                WriteXamlBoilerplate(XamlBoilerplateID.propertyXamlSyntax_nonXamlParent, writer);
            }
            else if(propertyVisibility != "public")
            {
                WriteXamlBoilerplate(XamlBoilerplateID.propertyXamlSyntax_notPublic, writer);
            }
            else if(isAbstract)
            {
                WriteXamlBoilerplate(XamlBoilerplateID.propertyXamlSyntax_abstract, writer);
            }
            else if(parameters.Count > 0)
            {
                // per DDUERELTools bug 1373: indexer properties cannot be used in XAML
                WriteXamlBoilerplate(XamlBoilerplateID.propertyXamlSyntax_nonXaml, writer);
            }
            else if(IsContentProperty(reflection) && !returnTypeIsReadonlyStruct)
            {
                PropertyContentElementUsageSimple(reflection, writer);
            }
            else if(!isSettable || !isSetterPublic)
            {
                WriteXamlBoilerplate(XamlBoilerplateID.propertyXamlSyntax_readOnly, writer);
            }
            else if(returnTypeIsAbstract && !HasTypeConverterAttribute(returnType))    // !EFW - Allow it if there's a type converter
            {
                WriteXamlBoilerplate(XamlBoilerplateID.propertyXamlSyntax_abstractType, returnType, writer);
            }
            else if(IsPrimitiveType(returnTypeId))
            {
                PropertyAttributeUsage(reflection, writer);
            }
            else if(returnTypeSubgroup == "enumeration")
            {
                PropertyAttributeUsage(reflection, writer);
            }
            else
            {
                bool hasDefaultConstructor = HasDefaultConstructor(returnType);
                if(HasTypeConverterAttribute(returnType))
                {
                    if(hasDefaultConstructor && !returnTypeIsReadonlyStruct)
                    {
                        PropertyElementUsageGrande(reflection, writer);
                    }
                    PropertyAttributeUsage(reflection, writer);
                }
                else if(hasDefaultConstructor && !returnTypeIsReadonlyStruct)
                {
                    PropertyElementUsageGrande(reflection, writer);
                }
                else
                {
                    WriteXamlBoilerplate(XamlBoilerplateID.propertyXamlSyntax_nonXaml, writer);
                }
            }
        }

        // A simple Property Element Usage block for a content property
        // syntax looks like: 
        //   <object>
        //     <linkToType .../>
        //   </object>
        private void PropertyContentElementUsageSimple(XPathNavigator propertyReflection, SyntaxWriter writer)
        {
            string xamlBlockId = System.Enum.GetName(typeof(XamlHeadingID), XamlHeadingID.xamlContentElementUsageHeading);
            XPathNavigator returnType = propertyReflection.SelectSingleNode(apiReturnTypeExpression);

            // start the syntax block
            writer.WriteStartSubBlock(xamlBlockId);

            //   <object>
            writer.WriteString("<");
            writer.WriteParameter("object");
            writer.WriteString(">");
            writer.WriteLine();
            //       <linkToType .../>
            writer.WriteString("  <");
            WriteTypeReference(returnType, writer);
            writer.WriteString(" .../>");
            writer.WriteLine();
            //   </object>
            writer.WriteString("</");
            writer.WriteParameter("object");
            writer.WriteString(">");

            writer.WriteEndSubBlock();
        }

        // A grandiose Property Element Usage block
        // syntax looks like: 
        //   <object>
        //     <object.PropertyName>
        //       <linkToType .../>
        //     </object.PropertyName>
        //   </object>
        private void PropertyElementUsageGrande(XPathNavigator propertyReflection, SyntaxWriter writer)
        {
            string xamlBlockId = System.Enum.GetName(typeof(XamlHeadingID), XamlHeadingID.xamlPropertyElementUsageHeading);
            string propertyName = (string)propertyReflection.Evaluate(apiNameExpression);
            XPathNavigator returnType = propertyReflection.SelectSingleNode(apiReturnTypeExpression);

            // start the syntax block
            writer.WriteStartSubBlock(xamlBlockId);

            //   <object>
            writer.WriteString("<");
            writer.WriteParameter("object");
            writer.WriteString(">");
            writer.WriteLine();
            //     <object.PropertyName>
            writer.WriteString("  <");
            writer.WriteParameter("object");
            writer.WriteString(".");
            writer.WriteIdentifier(propertyName);
            writer.WriteString(">");
            writer.WriteLine();
            //       <linkToType .../>
            writer.WriteString("    <");
            WriteTypeReference(returnType, writer);
            writer.WriteString(" .../>");
            writer.WriteLine();
            //     </object.PropertyName>
            writer.WriteString("  </");
            writer.WriteParameter("object");
            writer.WriteString(".");
            writer.WriteIdentifier(propertyName);
            writer.WriteString(">");
            writer.WriteLine();
            //   </object>
            writer.WriteString("</");
            writer.WriteParameter("object");
            writer.WriteString(">");

            writer.WriteEndSubBlock();
        }

        // An Attribute Usage block
        private void PropertyAttributeUsage(XPathNavigator propertyReflection, SyntaxWriter writer)
        {
            string xamlBlockId = System.Enum.GetName(typeof(XamlHeadingID), XamlHeadingID.xamlAttributeUsageHeading);
            string propertyName = (string)propertyReflection.Evaluate(apiNameExpression);
            XPathNavigator returnType = propertyReflection.SelectSingleNode(apiReturnTypeExpression);

            // start the syntax block
            writer.WriteStartSubBlock(xamlBlockId);

            // syntax looks like: 
            //   <object PropertyName="linkToType" .../>
            writer.WriteString("<");
            writer.WriteParameter("object ");
            writer.WriteIdentifier(propertyName);
            writer.WriteString("=\"");
            WriteTypeReference(returnType, writer);
            writer.WriteString("\" .../>");

            writer.WriteEndSubBlock();
        }

        /// <inheritdoc />
        public override void WriteEventSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string eventName = (string)reflection.Evaluate(apiNameExpression);
            string eventVisibility = (string)reflection.Evaluate(apiVisibilityExpression);
            bool isAbstract = (bool)reflection.Evaluate(apiIsAbstractProcedureExpression);
            XPathNavigator eventHandler = reflection.SelectSingleNode(apiHandlerOfEventExpression);

            XPathNavigator containingType = reflection.SelectSingleNode(apiContainingTypeExpression);
            string containingTypeSubgroup = (string)containingType.Evaluate(apiSubgroupExpression);
            bool containingTypeIsAbstract = (bool)containingType.Evaluate(apiIsAbstractTypeExpression);
            bool containingTypeIsSealed = (bool)containingType.Evaluate(apiIsSealedTypeExpression);

            if(containingTypeSubgroup == "interface")
            {
                WriteXamlBoilerplate(XamlBoilerplateID.eventXamlSyntax_noXamlSyntaxForInterfaceMembers, writer);
            }
            else if(containingTypeIsAbstract && containingTypeIsSealed)
            {
                // the event's containing type is static if it's abstract and sealed
                // members of a static class cannot be used in XAML.
                WriteXamlBoilerplate(XamlBoilerplateID.eventXamlSyntax_nonXamlParent, writer);
            }
            else if(IsExcludedSubClass(containingType))
            {
                WriteXamlBoilerplate(XamlBoilerplateID.eventXamlSyntax_parentIsExcludedSubClass, writer);
            }
            else if(!DoesParentSupportXaml(reflection))
            {
                WriteXamlBoilerplate(XamlBoilerplateID.eventXamlSyntax_nonXamlParent, writer);
            }
            else if(eventVisibility != "public")
            {
                WriteXamlBoilerplate(XamlBoilerplateID.eventXamlSyntax_notPublic, writer);
            }
            else if(isAbstract)
            {
                WriteXamlBoilerplate(XamlBoilerplateID.eventXamlSyntax_abstract, writer);
            }
            else
            {
                // start the syntax block
                string xamlBlockId = System.Enum.GetName(typeof(XamlHeadingID), XamlHeadingID.xamlAttributeUsageHeading);
                writer.WriteStartSubBlock(xamlBlockId);

                // syntax looks like: 
                //   <object eventName="eventHandlerLink" .../>
                writer.WriteString("<");
                writer.WriteParameter("object ");
                writer.WriteIdentifier(eventName);
                writer.WriteString("=\"");
                WriteTypeReference(eventHandler, writer);
                writer.WriteString("\" .../>");

                writer.WriteEndSubBlock();
            }
        }

        /// <inheritdoc />
        public override void WriteAttachedEventSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string eventName = (string)reflection.Evaluate(apiNameExpression);
            string containingTypeName = (string)reflection.Evaluate(apiContainingTypeNameExpression);
            XPathNavigator eventHandler = reflection.SelectSingleNode(apiHandlerOfEventExpression);

            // xaml syntax block for attached event
            string xamlBlockId = System.Enum.GetName(typeof(XamlHeadingID), XamlHeadingID.xamlAttributeUsageHeading);
            writer.WriteStartSubBlock(xamlBlockId);

            writer.WriteString("<");
            writer.WriteParameter("object ");
            writer.WriteIdentifier(containingTypeName + "." + eventName);
            writer.WriteString("=\"");
            WriteTypeReference(eventHandler, writer);
            writer.WriteString("\" .../>");

            writer.WriteEndSubBlock();
        }

        /// <inheritdoc />
        public override void WriteFieldSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            WriteXamlBoilerplate(XamlBoilerplateID.fieldOverviewXamlSyntax, writer);
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

                    XPathNavigator outerTypeReference = reference.SelectSingleNode(typeOuterTypeExpression);
                    if(outerTypeReference != null)
                    {
                        WriteTypeReference(outerTypeReference, writer);
                        writer.WriteString(".");
                    }

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
                case "T:System.String":
                    writer.WriteReferenceLink(reference, "string");
                    break;
                case "T:System.Boolean":
                    writer.WriteReferenceLink(reference, "bool");
                    break;
                case "T:System.Byte":
                    writer.WriteReferenceLink(reference, "byte");
                    break;
                case "T:System.SByte":
                    writer.WriteReferenceLink(reference, "sbyte");
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
                case "T:System.UInt16":
                    writer.WriteReferenceLink(reference, "ushort");
                    break;
                case "T:System.UInt32":
                    writer.WriteReferenceLink(reference, "uint");
                    break;
                case "T:System.UInt64":
                    writer.WriteReferenceLink(reference, "ulong");
                    break;
                case "T:System.Single":
                    writer.WriteReferenceLink(reference, "float");
                    break;
                case "T:System.Double":
                    writer.WriteReferenceLink(reference, "double");
                    break;
                case "T:System.Decimal":
                    writer.WriteReferenceLink(reference, "decimal");
                    break;
                default:
                    writer.WriteReferenceLink(reference);
                    break;
            }
        }

        // utility routines

        // A default constructor is a a parameterless, public constructor method
        // This is called for:
        //  a class
        //  the declaring type of a member
        //  the type of a property
        private bool HasDefaultConstructor(XPathNavigator typeReflection)
        {
            // all structs have implicit default constructors
            string subgroup = (string)typeReflection.Evaluate(apiSubgroupExpression);
            if(subgroup == "structure")
                return true;

            return (bool)typeReflection.Evaluate(hasDefaultConstructorExpression);
        }

        // This is called to check for a "TypeConverterAttribute" on:
        //   a class or structure topic
        //   the declaring type of a property or event member
        //   the type of a property
        private bool HasTypeConverterAttribute(XPathNavigator typeReflection)
        {
            return (bool)typeReflection.Evaluate(hasTypeConverterAttributeExpression);
        }

        // Get the id of the content property, if any, for the property's containing type
        // return true if the content property id matches the current property's id 
        private bool IsContentProperty(XPathNavigator propertyReflection)
        {
            string propertyName = (string)propertyReflection.Evaluate(apiNameExpression);
            XPathNavigator containingType = propertyReflection.SelectSingleNode(apiContainingTypeExpression);
            string containingTypeName = (string)containingType.Evaluate(apiNameExpression);
            string namespaceId = (string)propertyReflection.Evaluate(apiContainingNamespaceIdExpression);
            string propertyId = string.Concat("P:", namespaceId.Substring(2), ".", string.Concat(containingTypeName, ".", propertyName));
            string contentPropertyId = (string)containingType.Evaluate(contentPropertyIdExpression);
            if(propertyId == contentPropertyId)
                return true;
            else
                return false;
        }

        // Check the list of subclasses to exclude
        // This is called to check the class ancestors of
        //   a class
        //   the declaring type of a property or event member
        private bool IsExcludedSubClass(XPathNavigator typeReflection)
        {
            XPathNodeIterator ancestors = (XPathNodeIterator)typeReflection.Evaluate(apiAncestorsExpression);

            // Check the type itself as well
            string ancestorId = typeReflection.GetAttribute("api", string.Empty);

            if(!String.IsNullOrWhiteSpace(ancestorId) && excludedAncestorList.Contains(ancestorId))
                return true;

            foreach(XPathNavigator ancestor in ancestors)
            {
                ancestorId = ancestor.GetAttribute("api", string.Empty);

                if(excludedAncestorList.Contains(ancestorId))
                    return true;
            }

            return false;
        }

        // Check the parent type of a property or event. 
        // Does it have the necessary characteristics so the property or event can be used in XAML? 
        // Is PARENT CLASS abstract OR does it have a default ctor OR a class-level TypeConverter attribute?
        private bool DoesParentSupportXaml(XPathNavigator memberReflection)
        {
            XPathNavigator containingType = memberReflection.SelectSingleNode(apiContainingTypeExpression);
            if((bool)containingType.Evaluate(apiIsAbstractTypeExpression))
                return true;

            if(HasDefaultConstructor(containingType))
                return true;

            if(HasTypeConverterAttribute(containingType))
                return true;

            // A property that returns a primitive type doesn't need a TypeConverterAttribute, so return true here
            XPathNavigator returnType = memberReflection.SelectSingleNode(apiReturnTypeExpression);
            if(returnType != null)
            {
                string returnTypeId = returnType.GetAttribute("api", string.Empty);

                if(IsPrimitiveType(returnTypeId))
                    return true;
            }

            return false;
        }

        private static bool IsPrimitiveType(string typeId)
        {
            // The primitive types are Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, IntPtr, Char, Double, and Single.
            switch(typeId)
            {
                case "T:System.Boolean":
                case "T:System.Byte":
                case "T:System.SByte":
                case "T:System.Int16":
                case "T:System.UInt16":
                case "T:System.Int32":
                case "T:System.UInt32":
                case "T:System.Int64":
                case "T:System.UInt64":
                case "T:System.IntPtr":
                case "T:System.Char":
                case "T:System.Double":
                case "T:System.Single":
                case "T:System.String": // String is not a primitive but is treated as one for this XAML purpose
                    return true;

                default:
                    return false;
            }
        }

        private XPathExpression hasTypeConverterAttributeExpression = XPathExpression.Compile("boolean(attributes/attribute/type[@api='T:System.ComponentModel.TypeConverterAttribute'])");

        private XPathExpression hasDefaultConstructorExpression = XPathExpression.Compile("boolean(typedata/@defaultConstructor)");

        private XPathExpression contentPropertyIdExpression = XPathExpression.Compile("string(typedata/@contentProperty)");
        private XPathExpression ancestorContentPropertyIdExpression = XPathExpression.Compile("string(family/ancestors/type/@contentProperty)");

        private XPathExpression noSettablePropertiesExpression = XPathExpression.Compile("boolean(typedata/@noSettableProperties)");

        private XPathExpression apiIsSetterPublicExpression = XPathExpression.Compile("boolean((memberdata[@visibility='public'] and not(propertydata[@set-visibility!='public'])) or propertydata[@set-visibility='public'])");
    }

    /// <summary>
    /// This enumerated type defines XAML boilerplate resource item IDs
    /// </summary>
    internal enum XamlBoilerplateID
    {
        // boilerplate for classes in xaml assemblies
        classXamlSyntax_abstract,
        classXamlSyntax_excludedSubClass,
        classXamlSyntax_noDefaultCtor,
        classXamlSyntax_noDefaultCtorWithTypeConverter,
        // boilerplate for structs in xaml assemblies
        structXamlSyntax_nonXaml,
        structXamlSyntax_attributeUsage,
        // boilerplate for events in xaml assemblies
        eventXamlSyntax_parentIsExcludedSubClass,
        eventXamlSyntax_noXamlSyntaxForInterfaceMembers,
        eventXamlSyntax_nonXamlParent,
        eventXamlSyntax_notPublic,
        eventXamlSyntax_abstract,
        eventXamlSyntax_nonXaml,
        // boilerplate for properties in xaml assemblies
        propertyXamlSyntax_parentIsExcludedSubClass,
        propertyXamlSyntax_noXamlSyntaxForInterfaceMembers,
        propertyXamlSyntax_nonXamlParent,
        propertyXamlSyntax_notPublic,
        propertyXamlSyntax_abstract,
        propertyXamlSyntax_readOnly,
        propertyXamlSyntax_abstractType,
        propertyXamlSyntax_nonXaml,
        // syntax used with all enums in xaml assemblies
        enumerationOverviewXamlSyntax,
        // boilerplate used with all method, field, etc. in xaml assemblies
        delegateOverviewXamlSyntax,
        interfaceOverviewXamlSyntax,
        constructorOverviewXamlSyntax,
        fieldOverviewXamlSyntax,
        methodOverviewXamlSyntax,
        // boilerplate used with all types and members in all non-xaml assemblies
        nonXamlAssemblyBoilerplate
    }

    /// <summary>
    /// This enumerated type defines XAML heading resource item IDs
    /// </summary>
    internal enum XamlHeadingID
    {
        xamlAttributeUsageHeading,
        xamlObjectElementUsageHeading,
        xamlPropertyElementUsageHeading,
        xamlContentElementUsageHeading,
        xamlSyntaxBoilerplateHeading
    }
}
