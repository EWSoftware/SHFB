// Change history:
// 12/23/2013 - EFW - Updated the build component to be discoverable via MEF
// 04/27/2014 - EFW - Deprecated this component as it is no longer used.  It will be removed in a future release.
// Presentation styles should now generate all aspects of the topic.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This component applies formatting that makes the help content compatible with
    /// the MS Help Viewer branding package.
    /// </summary>
    /// <remarks>
    /// <note type="warning">PLEASE NOTE: The Branding Component has been deprecated.  Presentation styles are
    /// now responsible for generating all aspects of the topic.  This component will be removed in a future
    /// release.</note>
    /// 
    /// <para>There are three different possible operation modes:</para>
    /// 
    /// <list type="number">
    /// <item>For self-branded MS Help Viewer content, it applies the branding package formatting
    /// that the MS Help Viewer would apply if the content were not self-branded (except for
    /// Microsoft-specific stuff like copyright, logo, feedback, etc.)</item>
    /// <item>For branded MS Help Viewer content, it applies minimal formatting to ensure that
    /// the help content conforms to the MTPS conventions expected by the MS Help Viewer.</item>
    /// <item>For targets other than the MS Help Viewer (eg. Help1 or Web), it performs full
    /// branding package formatting.</item>
    /// </list>
    /// <example>
    /// <code lang="xml" title="Example configuration - MS Help Viewer - default catalog">
    ///    &lt;data self-branded="true"
    ///       branding-content="branding"           The relative path of the branding package transforms
    ///       help-output="MSHelpViewer" /&gt;
    /// </code>
    /// <code lang="xml" title="Example configuration - MS Help Viewer - custom catalog">
    ///    &lt;data self-branded="false"
    ///       branding-content="branding"           The full or relative path to the branding package transforms
    ///       help-output="MSHelpViewer"
    ///       vendor-name="VendorName"              The custom catalog vendor name
    ///       -- Optional elements - leave blank to use the default --
    ///       catalog-product-id="XX"               The custom catalog product id 
    ///       catalog-version="123"                 The custom catalog product version
    ///       locale="en-US"                        The custom catalog locale
    ///       branding-package="MyBrandingPackage"  The custom branding package name (without extension)
    ///       /&gt;
    /// </code>
    /// <code lang="xml" title="Example configuration - other targets">
    ///    &lt;data self-branded="true"
    ///       branding-content="branding"           The full or relative path to the branding package transforms
    ///       help-output="other" /&gt;
    /// </code>
    /// </example>
    /// </remarks>
    public class BrandingComponent : BuildComponentCore
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("Branding Component")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new BrandingComponent(base.BuildAssembler);
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private const string s_configSelfBranded = "self-branded";
        private const string s_configLocale = "locale";
        private const string s_configBrandingContent = "branding-content";
        private const string s_configBrandingPackage = "branding-package";
        private const string s_configHelpOutput = "help-output";
        private const string s_configCatalogProductId = "catalog-product-id";
        private const string s_configCatalogVersion = "catalog-version";
        private const string s_configVendorName = "vendor-name";
        private const string s_codeRemoveElement = "plain-remove-element";
        private const string s_codeRemoveId = "plain-remove-id";
        private const string s_codeRemoveClass = "plain-remove-class";

        private const string s_defaultLocale = "en-US";
        private static string s_defaultBrandingPackage = "Dev10";
        private const string s_defaultHelpOutput = "MSHelpViewer";
        private static string s_defaultCatalogProductId = "VS";
        private static string s_defaultCatalogVersion = "100";
        private static string s_defaultVendorName = "Microsoft";
        private static string s_packageExtension = ".mshc";

        private bool m_selfBranded = true;
        private string m_locale = s_defaultLocale;
        private string m_brandingContent = String.Empty;
        private string m_brandingPackage = s_defaultBrandingPackage;
        private string m_helpOutput = s_defaultHelpOutput;
        private string m_catalogProductId = s_defaultCatalogProductId;
        private string m_catalogVersion = s_defaultCatalogVersion;
        private string m_vendorName = s_defaultVendorName;
        private string m_codeRemoveElement = String.Empty;
        private string m_codeRemoveId = String.Empty;
        private string m_codeRemoveClass = String.Empty;
        private XslCompiledTransform m_brandingTransform = null;
        private XsltArgumentList m_transformArguments = null;

        private const string s_xhtmlNamespace = "http://www.w3.org/1999/xhtml";

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected BrandingComponent(BuildAssemblerCore buildAssembler) : base(buildAssembler)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc/>
        public override void Initialize(XPathNavigator configuration)
        {
            base.WriteMessage(MessageLevel.Warn, "PLEASE NOTE: The Branding Component has been deprecated.  " +
                "Presentation styles are now responsible for generating all aspects of the topic.  This " +
                "component will be removed in a future release.");

            XPathNavigator v_configData = configuration.SelectSingleNode("data");
            String v_configValue;

            if(v_configData != null)
            {
                m_brandingContent = Path.GetFullPath(Environment.ExpandEnvironmentVariables(
                    v_configData.GetAttribute(s_configBrandingContent, String.Empty)));

                if(!String.IsNullOrEmpty(v_configValue = v_configData.GetAttribute(s_configLocale, String.Empty)))
                {
                    m_locale = v_configValue;
                }
                if(!String.IsNullOrEmpty(v_configValue = v_configData.GetAttribute(s_configSelfBranded, String.Empty)))
                {
                    m_selfBranded = bool.Parse(v_configValue);
                }
                if(!String.IsNullOrEmpty(v_configValue = v_configData.GetAttribute(s_configBrandingPackage, String.Empty)))
                {
                    m_brandingPackage = v_configValue;
                }
                if(!String.IsNullOrEmpty(v_configValue = v_configData.GetAttribute(s_configHelpOutput, String.Empty)))
                {
                    m_helpOutput = v_configValue;
                }
                if(!String.IsNullOrEmpty(v_configValue = v_configData.GetAttribute(s_configCatalogProductId, String.Empty)))
                {
                    m_catalogProductId = v_configValue;
                }
                if(!String.IsNullOrEmpty(v_configValue = v_configData.GetAttribute(s_configCatalogVersion, String.Empty)))
                {
                    m_catalogVersion = v_configValue;
                }
                if(!String.IsNullOrEmpty(v_configValue = v_configData.GetAttribute(s_configVendorName, String.Empty)))
                {
                    m_vendorName = v_configValue;
                }
            }

            v_configData = configuration.SelectSingleNode("code");
            if(v_configData != null)
            {
                if(!String.IsNullOrEmpty(v_configValue = v_configData.GetAttribute(s_codeRemoveElement, String.Empty)))
                {
                    m_codeRemoveElement = v_configValue;
                }
                if(!String.IsNullOrEmpty(v_configValue = v_configData.GetAttribute(s_codeRemoveId, String.Empty)))
                {
                    m_codeRemoveId = v_configValue;
                }
                if(!String.IsNullOrEmpty(v_configValue = v_configData.GetAttribute(s_codeRemoveClass, String.Empty)))
                {
                    m_codeRemoveClass = v_configValue;
                }
            }

            // If self-branding is required, force it and issue a warning
            if(!m_selfBranded && String.Compare(m_helpOutput, s_defaultHelpOutput, StringComparison.OrdinalIgnoreCase) != 0)
            {
                WriteMessage(MessageLevel.Warn, "Self-branding is required for {0} help format and it is being enabled.", m_helpOutput);
                m_selfBranded = true;
            }

            if(m_selfBranded)
                LoadBrandingTransform();
        }

        /// <inheritdoc/>
        public override void Apply(XmlDocument document, string key)
        {
            if(String.Compare(m_helpOutput, s_defaultHelpOutput, StringComparison.OrdinalIgnoreCase) == 0)
            {
                if(m_selfBranded)
                    ApplyBranding(document, key);
            }
            else
                ApplyBranding(document, key);

            // EFW - Apply language specific text to syntax sections which the branding transformations miss
            // for some reason.
            ReformatLanguageSpecific(document);
        }
        #endregion

        #region Branding
        //=====================================================================

        /// <summary>
        /// This method applies the branding package transforms.
        /// </summary>
        /// <param name="document">The current document.</param>
        /// <param name="key">The document's unique identifier.</param>
        private void ApplyBranding(XmlDocument document, string key)
        {
            if(m_brandingTransform != null)
            {
#if DEBUG
                WriteMessage(MessageLevel.Info, "  Branding topic {0} ({1}) SelfBranded={2}", key, m_locale, m_selfBranded);
#endif
                try
                {
                    XmlDocument v_tempDocument = new XmlDocument();

                    v_tempDocument.LoadXml(document.OuterXml);

                    // The default xhtml namespace is required for the branding transforms to work,
                    if(String.IsNullOrEmpty(v_tempDocument.DocumentElement.GetAttribute("xmlns")))
                    {
                        v_tempDocument.DocumentElement.SetAttribute("xmlns", s_xhtmlNamespace);
                        v_tempDocument.LoadXml(v_tempDocument.OuterXml);
                    }
                    SetSelfBranding(v_tempDocument, m_selfBranded);
#if DEBUG//_NOT
                    try
                    {
                        String v_tempPath = Path.GetFullPath("PreBranding");
                        if(!Directory.Exists(v_tempPath))
                        {
                            Directory.CreateDirectory(v_tempPath);
                        }
                        v_tempPath = Path.Combine(v_tempPath, key.Replace(':', '_').Replace('.', '_') + ".htm");
                        v_tempDocument.Save(v_tempPath);
                    }
                    catch { }
#endif
#if DEBUG_NOT
                    String v_tempBrandingPath = Path.GetFullPath("TempBranding");
                    if (!Directory.Exists (v_tempBrandingPath))
                    {
                        Directory.CreateDirectory (v_tempBrandingPath);
                    }
                    v_tempBrandingPath = Path.Combine (v_tempBrandingPath, key.Replace (':', '_').Replace ('.', '_') + ".htm");
                    using (FileStream v_stream = new FileStream (v_tempBrandingPath, FileMode.Create, FileAccess.ReadWrite))
#else
                    using(Stream v_stream = new MemoryStream())
#endif
                    {
                        try
                        {
                            XPathNavigator v_navigator = v_tempDocument.CreateNavigator();
                            using(XmlWriter v_writer = XmlWriter.Create(v_stream, m_brandingTransform.OutputSettings))
                            {
                                m_brandingTransform.Transform(v_navigator, m_transformArguments, v_writer);
                            }

                            XmlReaderSettings v_readerSettings = new XmlReaderSettings();
                            v_readerSettings.ConformanceLevel = ConformanceLevel.Fragment;
                            v_readerSettings.CloseInput = true;
                            v_stream.Seek(0, SeekOrigin.Begin);
                            using(XmlReader v_reader = XmlReader.Create(v_stream, v_readerSettings))
                            {
                                v_tempDocument.RemoveAll();
                                v_tempDocument.Load(v_reader);
                            }

                            RemoveUnusedNamespaces(v_tempDocument);

                            using(XmlReader v_reader = new SpecialXmlReader(v_tempDocument.OuterXml, this))
                            {
                                document.RemoveAll();
                                document.Load(v_reader);
                            }
#if DEBUG//_NOT
                            try
                            {
                                String v_tempPath = Path.GetFullPath("PostBranding");
                                if(!Directory.Exists(v_tempPath))
                                {
                                    Directory.CreateDirectory(v_tempPath);
                                }
                                v_tempPath = Path.Combine(v_tempPath, key.Replace(':', '_').Replace('.', '_') + ".htm");
                                document.Save(v_tempPath);
                            }
                            catch { }
#endif
                        }
                        catch(Exception exp)
                        {
                            WriteMessage(key, MessageLevel.Error, exp.Message);
                        }
                    }
                }
                catch(XsltException exp)
                {
                    WriteMessage(key, MessageLevel.Error, "{0} at {1} {2} {3}", exp.Message, exp.SourceUri, exp.LineNumber, exp.LinePosition);
#if DEBUG
                    if(exp.InnerException != null)
                    {
                        WriteMessage(key, MessageLevel.Error, "[{0}] {1}", exp.InnerException.GetType().Name, exp.InnerException.Message);
                    }
#endif
                }
                catch(Exception exp)
                {
                    WriteMessage(key, MessageLevel.Error, exp.Message);
                }
            }
        }

        /// <summary>
        /// Mark the document as SelfBranded for the branding transforms.
        /// </summary>
        /// <param name="document">The current document.</param>
        /// <param name="selfBranded">The SelfBranded setting.</param>
        private void SetSelfBranding(XmlDocument document, bool selfBranded)
        {
            try
            {
                XmlNamespaceManager v_namespaceManager;
                XmlNode v_header;

                v_namespaceManager = new XmlNamespaceManager(document.NameTable);
                v_namespaceManager.AddNamespace("xhtml", s_xhtmlNamespace);

                v_header = document.DocumentElement.SelectSingleNode("xhtml:head", v_namespaceManager);
                if(v_header != null)
                {
					XmlNodeList v_branded = v_header.SelectNodes("xhtml:meta[(@name='SelfBranded') or (@name='Microsoft.Help.SelfBranded')]", v_namespaceManager);

                    if(v_branded.Count == 0)
                    {
                        XmlElement v_meta = document.CreateElement("meta", s_xhtmlNamespace);
                        v_meta.SetAttribute("name", "SelfBranded");
                        v_meta.SetAttribute("content", selfBranded.ToString().ToLowerInvariant());
                        v_header.AppendChild(v_meta);
                    }
                    else
                    {
                        foreach(XmlNode v_brandedNode in v_branded)
                        {
                            XmlAttribute v_Attribute = document.CreateAttribute("meta", String.Empty);
                            v_Attribute.Value = selfBranded.ToString().ToLowerInvariant();
                            v_brandedNode.Attributes.SetNamedItem(v_Attribute);
                        }
                    }
                }
            }
            catch(Exception exp)
            {
                WriteMessage(MessageLevel.Error, exp.Message);
            }
        }

        /// <summary>
        /// Remove extra unused namespace declarations.
        /// </summary>
        /// <param name="document">The current document.</param>
        private void RemoveUnusedNamespaces(XmlDocument document)
        {
            try
            {
                XmlNamespaceManager v_namespaceManager;
                List<XmlNode> v_attribues = new List<XmlNode>();
#if DEBUG_NOT
                WriteMessage (MessageLevel.Info, "  RemoveUnusedNamespaces");
#endif
                v_namespaceManager = new XmlNamespaceManager(document.NameTable);
                v_namespaceManager.AddNamespace("xhtml", s_xhtmlNamespace);

                if((document.DocumentElement != null) && (document.DocumentElement.Attributes != null))
                {
                    foreach(XmlNode v_attribute in document.DocumentElement.Attributes)
                    {
                        if(String.Compare(v_attribute.Prefix, "xmlns", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            v_attribues.Add(v_attribute);
                            if(!v_namespaceManager.HasNamespace(v_attribute.LocalName))
                            {
                                v_namespaceManager.AddNamespace(v_attribute.LocalName, v_attribute.Value);
                            }
                        }
                    }
                    foreach(XmlNode v_attribute in v_attribues)
                    {
#if DEBUG_NOT
                        WriteMessage (MessageLevel.Info, "    Check namespace [{0}] [{1}]", v_attribute.LocalName, v_attribute.Name);
#endif
                        XmlNode v_namespaceUsed = null;

                        if(v_attribute.LocalName.ToLowerInvariant() != "xhtml")
                            v_namespaceUsed = document.SelectSingleNode(String.Format(CultureInfo.InvariantCulture,
                                "(/*//{0}:*) | (/*//*[@{0}:*])", v_attribute.LocalName), v_namespaceManager);

                        if(v_namespaceUsed != null)
                        {
#if DEBUG_NOT
                            WriteMessage(MessageLevel.Info, "      Used [{0}] [{1}] [{2}] [{3}]", v_namespaceUsed.Prefix, v_namespaceUsed.LocalName, v_namespaceUsed.Name, v_namespaceUsed.NamespaceURI);
                            WriteMessage(MessageLevel.Info, "      Used [{0}]", v_namespaceUsed.OuterXml);
#endif
                        }
                        else
                        {
                            document.DocumentElement.Attributes.RemoveNamedItem(v_attribute.Name);
                        }
                    }

                    // The default namespace needs to be removed for future XPath queries to work.
                    // It will be added again if necessary by the SaveComponent.
                    document.DocumentElement.SetAttribute("xmlns", String.Empty);
                }
            }
            catch(Exception exp)
            {
                WriteMessage(MessageLevel.Error, exp.Message);
            }
        }

        /// <summary>
        /// Loads the main branding transform.
        /// <para>The name of the branding transform file is extracted from <c>branding.xml</c> depending
        /// on the current locale.</para>
        /// <para>Any global parameters specified by <c>branding.xml</c> are applied and then specific
        /// parameters are applied according to the component configuration.</para>
        /// </summary>
        private void LoadBrandingTransform()
        {
            try
            {
                if(!String.IsNullOrEmpty(m_brandingContent))
                {
                    if(Directory.Exists(m_brandingContent))
                    {
                        try
                        {
                            String v_brandingTransformName = String.Format(CultureInfo.InvariantCulture,
                                "branding-{0}.xslt", m_locale);

                            XslCompiledTransform v_brandingTransform = new XslCompiledTransform();
                            XmlResolver v_resolver = new XmlUrlResolver();

                            m_transformArguments = new XsltArgumentList();
                            m_transformArguments.XsltMessageEncountered += new XsltMessageEncounteredEventHandler(OnTransformMessageEncountered);
                            LoadBrandingConfig(Path.Combine(m_brandingContent, "branding.xml"), ref v_brandingTransformName);

                            PutTransformParam("catalogProductFamily", m_catalogProductId);
                            PutTransformParam("catalogProductVersion", m_catalogVersion);
                            PutTransformParam("catalogLocale", m_locale);
                            PutTransformParam("content-path", @".\");
                            if(String.Compare(m_helpOutput, s_defaultHelpOutput, StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                PutTransformParam("branding-package", m_brandingPackage + s_packageExtension);
                                PutTransformParam("downscale-browser", true);
                            }
                            else if(String.Compare(m_brandingPackage, s_defaultBrandingPackage, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                PutTransformParam("branding-package", @"content\" + s_defaultVendorName + @"\store\" + m_brandingPackage + s_packageExtension);
                                PutTransformParam("pre-branding", true);
                            }
                            else
                            {
                                PutTransformParam("branding-package", @"content\" + m_vendorName + @"\store\" + m_brandingPackage + s_packageExtension);
                            }

                            if(!String.IsNullOrEmpty(m_codeRemoveElement))
                            {
                                PutTransformParam(s_codeRemoveElement, m_codeRemoveElement);
                            }
                            if(!String.IsNullOrEmpty(m_codeRemoveId))
                            {
                                PutTransformParam(s_codeRemoveId, m_codeRemoveId);
                            }
                            if(!String.IsNullOrEmpty(m_codeRemoveClass))
                            {
                                PutTransformParam(s_codeRemoveClass, m_codeRemoveClass);
                            }

                            WriteMessage(MessageLevel.Info, "Branding Transform \"{0}\" catalogProductFamily={1} catalogProductVersion={2} catalogLocale={3}", Path.Combine(m_brandingContent, v_brandingTransformName), m_catalogProductId, m_catalogVersion, m_locale);
                            v_brandingTransform.Load(Path.Combine(m_brandingContent, v_brandingTransformName), XsltSettings.TrustedXslt, v_resolver);
                            m_brandingTransform = v_brandingTransform;
                        }
                        catch(XsltException exp)
                        {
                            WriteMessage(MessageLevel.Error, "{0} at {1} {2} {3}", exp.Message, exp.SourceUri, exp.LineNumber, exp.LinePosition);
#if DEBUG
                            if(exp.InnerException != null)
                                WriteMessage(MessageLevel.Error, "[{0}] {1}", exp.InnerException.GetType().Name, exp.InnerException.Message);
#endif
                        }
                        catch(Exception exp)
                        {
                            WriteMessage(MessageLevel.Error, exp.Message);
                        }
                    }
                    else
                    {
                        WriteMessage(MessageLevel.Info, "No branding transform loaded.  Branding does not apply in this presentation style.");
                    }
                }
            }
            catch(Exception exp)
            {
                WriteMessage(MessageLevel.Error, exp.Message);
            }
        }

        /// <summary>
        /// Loads the branding transform configuration from <c>branding.xml</c>
        /// </summary>
        /// <param name="configPath">The full path of <c>branding.xml</c></param>
        /// <param name="transformName">The full path of the branding transform file to load.</param>
        private void LoadBrandingConfig(String configPath, ref String transformName)
        {
            try
            {
                XmlDocument v_brandingConfig = new XmlDocument();
                XmlNamespaceManager v_namespaceManager;
                XmlNode v_argumentsNode;
                XmlNode v_transformNode;

                v_brandingConfig.Load(configPath);
                v_namespaceManager = new XmlNamespaceManager(v_brandingConfig.NameTable);
                v_namespaceManager.AddNamespace("branding", "urn:FH-Branding");

                v_argumentsNode = v_brandingConfig.DocumentElement.SelectSingleNode("branding:common-parameters", v_namespaceManager);
                v_transformNode = v_brandingConfig.DocumentElement.SelectSingleNode(String.Format(
                    CultureInfo.InvariantCulture, "branding:transform-parameters[@xml:lang='{0}']",
                    m_locale), v_namespaceManager);

                if(v_argumentsNode != null)
                {
                    foreach(XmlNode v_argumentNode in v_argumentsNode.ChildNodes)
                    {
                        if(String.Compare(v_argumentNode.LocalName, "parameter", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            XmlNode v_argumentName = v_argumentNode.Attributes.GetNamedItem("name");
                            XmlNode v_argumentValue = v_argumentNode.Attributes.GetNamedItem("value");

                            if((v_argumentName != null) && (v_argumentNode != null))
                            {
                                PutTransformParam(v_argumentName.Value, v_argumentValue.Value);
                            }
                        }
                    }
                }
                if(v_transformNode != null)
                {
                    v_transformNode = v_transformNode.Attributes.GetNamedItem("transform");
                }
                if(v_transformNode != null)
                {
                    transformName = v_transformNode.Value;
                }
            }
            catch(Exception ex)
            {
                WriteMessage(MessageLevel.Error, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private void PutTransformParam(String name, Object value)
        {
            if(m_transformArguments.GetParam(name, String.Empty) != null)
            {
                m_transformArguments.RemoveParam(name, String.Empty);
            }
            m_transformArguments.AddParam(name, String.Empty, value);
        }

        void OnTransformMessageEncountered(object sender, XsltMessageEncounteredEventArgs e)
        {
            WriteMessage(MessageLevel.Info, e.Message);
        }

        #endregion

        #region Helper Class
        //=====================================================================

        /// <summary>
        /// This is an override of the default <see cref="T:System.Xml.XmlTextReader"/> that allows
        /// "xmlns" attributes to be skipped.  It's used to remove unused namespaces from an XML file.
        /// </summary>
        private class SpecialXmlReader : XmlTextReader
        {
            BuildComponentCore component;

            public SpecialXmlReader(String fragment, BuildComponentCore component) : base(fragment,
              XmlNodeType.Element, new XmlParserContext(null, null, null, XmlSpace.Default))
            {
                this.component = component;
            }

            public override bool MoveToFirstAttribute()
            {
                bool v_ret = base.MoveToFirstAttribute();
                if(v_ret && (base.Depth <= 2) && (base.NodeType == XmlNodeType.Attribute) && (base.Name == "xmlns"))
                {
#if DEBUG_NOT
                    component.WriteMessage(MessageLevel.Info, "  Skip Attribute [{0}] [{1}] [{2}] [{3}] [{4}]",
                        base.Depth, base.NodeType, base.Name, base.Prefix, base.LocalName);
#endif
                    v_ret = base.MoveToNextAttribute();
                }
                return v_ret;
            }

            public override bool MoveToNextAttribute()
            {
                bool v_ret = base.MoveToNextAttribute();
                if(v_ret && (base.Depth <= 2) && (base.NodeType == XmlNodeType.Attribute) && (base.Name == "xmlns"))
                {
#if DEBUG_NOT
                    component.WriteMessage(MessageLevel.Info, "  Skip Attribute [{0}] [{1}] [{2}] [{3}] [{4}]",
                        base.Depth, base.NodeType, base.Name, base.Prefix, base.LocalName);
#endif
                    v_ret = base.MoveToNextAttribute();
                }
                return v_ret;
            }
        }

        #endregion

        #region LanguageSpecific reformatting
        //=====================================================================

        /// <summary>
        /// Reformats all LanguageSpecific spans to the format used by the MS Help Viewer
        /// </summary>
        /// <param name="document">The current document.</param>
        private static void ReformatLanguageSpecific(XmlDocument document)
        {
            int v_uniqueIdSequence = 0;
            XmlNodeList v_nodeList = document.SelectNodes("//span[@class='languageSpecificText']");

            foreach(XmlNode v_node in v_nodeList)
            {
                XmlNodeList v_partList = v_node.SelectNodes("span[@class]");
                String v_partText = String.Empty;

                if((v_partList.Count > 0) && (v_partList.Count == v_node.ChildNodes.Count))
                {
#if true
                    //
                    //    Option 1 - implement LST as it appears in the final page
                    //
                    String v_uniqueId = String.Format(CultureInfo.InvariantCulture, "IDLST{0:D6}", ++v_uniqueIdSequence);
                    XmlElement v_spanElement;
                    XmlElement v_scriptElement;

                    foreach(XmlNode v_partNode in v_partList)
                    {
                        if(!String.IsNullOrEmpty(v_partText))
                        {
                            v_partText += "|";
                        }
                        v_partText += String.Format(CultureInfo.InvariantCulture, "{0}={1}", v_partNode.Attributes.GetNamedItem("class").Value, v_partNode.InnerText.Trim('\''));
                    }

                    v_spanElement = document.CreateElement("span");
                    v_spanElement.SetAttribute("id", v_uniqueId);
                    v_spanElement.InnerText = "&#160;";
                    v_scriptElement = document.CreateElement("script");
                    v_scriptElement.SetAttribute("type", "text/javascript");
                    v_scriptElement.InnerText = String.Format(CultureInfo.InvariantCulture, "AddLanguageSpecificTextSet(\"{0}?{1}\");", v_uniqueId, v_partText);

                    v_node.ParentNode.InsertAfter(v_scriptElement, v_node);
                    v_node.ParentNode.ReplaceChild(v_spanElement, v_node);
#else
                    //
                    //    Option 2 - implement LST as it appears in the raw page
                    //
                    XmlElement v_lstElement;

                    v_lstElement = document.CreateElement ("mtps:LanguageSpecificText");

                    foreach (XmlNode v_partNode in v_partList)
                    {
                        v_lstElement.SetAttribute (String.Format (CultureInfo.InvariantCulture, "devLang{0}", v_partNode.Attributes.GetNamedItem ("class").Value), v_partNode.InnerText);
                    }
                    v_node.ParentNode.ReplaceChild (v_lstElement, v_node);
#endif
                }
            }
        }
        #endregion
    }
}
