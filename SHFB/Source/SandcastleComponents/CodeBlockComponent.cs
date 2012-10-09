//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : CodeBlockComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/21/2012
// Note    : Copyright 2006-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a build component that is used to search for <code> XML comment tags and colorize the code
// within them.  It can also include code from an external file or a region within the file.  Note that this
// component must be paired with the PostTransformComponent.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.   This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.3.3.0  11/21/2006  EFW  Created the code
// 1.3.4.0  01/03/2007  EFW  Added support for VB.NET style #region blocks
// 1.4.0.0  02/02/2007  EFW  Made changes to support custom presentation styles and new colorizer options
// 1.4.0.2  06/12/2007  EFW  Added support for nested code blocks
// 1.5.0.0  06/19/2007  EFW  Various additions and updates for the June CTP
// 1.6.0.3  06/20/2007  EFW  Fixed bug that caused code blocks with an unknown or unspecified language to always
//                           be hidden.
// 1.6.0.5  03/05/2008  EFW  Added support for the keepSeeTags attribute
// 1.6.0.7  04/05/2008  EFW  Modified to not add language filter elements if the matching language filter is not
//                           present.  Updated to support use in conceptual builds.
// 1.8.0.0  07/22/2008  EFW  Fixed bug related to nested code blocks in conceptual content.  Added option to
//                           generate warnings instead of errors on missing source code.
// 1.8.0.1  12/02/2008  EFW  Fixed bug that caused <see> tags to go unprocessed due to change in code block
//                           handling.  Added support for removeRegionMarkers.
// 1.9.0.1  06/19/2010  EFW  Added support for MS Help Viewer
// 1.9.3.3  12/30/2011  EFW  Added support for overriding allowMissingSource option on a case by case basis
// 1.9.5.0  09/21/2012  EFW  Added support disabling all features except leading whitespace normalization
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools;

using ColorizerLibrary;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This build component is used to search for &lt;code&gt; XML comment tags and colorize the code within
    /// them.  It can also include code from an external file or a region within the file.
    /// </summary>
    /// <remarks>Note that this component must be paired with the
    /// <see cref="PostTransformComponent"/>.</remarks>
    /// <example>
    /// <code lang="xml" title="Example configuration">
    /// &lt;!-- Code block component configuration.  This must appear before
    ///      the TransformComponent.  See also: PostTransformComponent. --&gt;
    /// &lt;component type="SandcastleBuilder.Components.CodeBlockComponent"
    ///   assembly="C:\SandcastleBuilder\SandcastleBuilder.Components.dll"&gt;
    ///     &lt;!-- Base path for relative filenames in source
    ///          attributes (optional). --&gt;
    ///     &lt;basePath value="..\SandcastleComponents" /&gt;
    ///
    ///     &lt;!-- Connect to language filter (optional).  If omitted,
    ///          language filtering is enabled by default. --&gt;
    ///     &lt;languageFilter value="true" /&gt;
    ///
    ///     &lt;!-- Allow missing source files (Optional).  If omitted,
    ///          it will generate errors if referenced source files
    ///          are missing. --&gt;
    ///     &lt;allowMissingSource value="false" /&gt;
    /// 
    ///     &lt;!-- Remove region markers from imported code blocks.  If omitted,
    ///          region markers in imported code blocks are left alone. --&gt;
    ///     &lt;removeRegionMarkers value="false" /&gt;
    ///
    ///     &lt;!-- Code colorizer options (required).
    ///       Attributes:
    ///         Language syntax configuration file (required)
    ///         XSLT style file (required)
    ///         "Copy" image file URL (required)
    ///         Default language (optional)
    ///         Enable line numbering (optional)
    ///         Enable outlining (optional)
    ///         Keep XML comment "see" tags within the code (optional)
    ///         Tab size for unknown languages (optional, 0 = use default)
    ///         Use language name as default title (optional)
    ///         Disabled (optional, normalize leading whitespace only) --&gt;
    ///     &lt;colorizer syntaxFile="highlight.xml" styleFile="highlight.xsl"
    ///       copyImageUrl="CopyCode.gif" language="cs" numberLines="false"
    ///       outlining="false" keepSeeTags="false" tabSize="0"
    ///       defaultTitle="true" disabled="false" /&gt;
    /// &lt;/component&gt;
    /// </code>
    ///
    /// <code lang="xml" title="Examples as used in XML comments.">
    /// &lt;example&gt;
    /// A basic code block that uses the configuration defaults:
    /// &lt;code&gt;
    /// /// Code to colorize
    /// &lt;/code&gt;
    ///
    /// Override options with block-specific options:
    /// &lt;code lang="xml" numberLines="true" outlining="false" tabSize="8" &gt;
    ///     &amp;lt;XmlTags/&amp;gt;
    /// &lt;/code&gt;
    ///
    /// An entire external file or a delimited region from it can be
    /// included.  This allows you to compile your example code externally
    /// to ensure that it is still valid and saves you from maintaining it
    /// in two places.
    ///
    /// Retrieve all code from an external file.  Use VB.NET syntax.
    /// &lt;code source="..\Examples\WholeDemo.vb" lang="vbnet"/&gt;
    ///
    /// Retrieve a specific #region from an external file.
    /// &lt;code source="..\Examples\SeveralExamples.vb"
    ///     region="Example 1" lang="vbnet"/&gt;
    /// 
    /// Keep &lt;see&gt; tags within comments so that they are converted to
    /// links to the help topics.
    /// &lt;code keepSeeTags="true"&gt;
    /// int x = this.&lt;see cref="CountStuff"&gt;CountStuff&lt;/see&gt;(true);
    /// 
    /// string value = this.&lt;see cref="System.Object.ToString"&gt;
    /// &lt;code&gt;
    ///
    /// &lt;example&gt;
    /// </code>
    /// </example>
    public class CodeBlockComponent : BuildComponent
    {
        #region Private data members
        //=====================================================================

        // Colorized code dictionary and image location used by PostTransformComponent
        private static Dictionary<string, XmlNode> colorizedCodeBlocks = new Dictionary<string, XmlNode>();
        private static string copyImageLocation, copyText;

        private CodeColorizer colorizer;    // The code colorizer

        // Line numbering, outlining, keep see tags, remove region markers, and disabled flags
        private bool numberLines, outliningEnabled, keepSeeTags, removeRegionMarkers, isDisabled;

        // The base path to use for file references with relative paths, the syntax and style filenames, and the
        // default language.
        private string basePath, syntaxFile, styleFile, copyImage, defaultLanguage;

        // The message level for missing source errors
        private MessageLevel messageLevel;

        // Connect code blocks to the language filter if true
        private bool languageFilter;
        private List<string> filtersPresent;

        private int defaultTabSize;     // Default tab size

        // Uh, yeah.  Don't ask me to explain this.  Just accept that it works (I hope :)).  It uses balancing
        // groups to extract #region to #endregion accounting for any nested regions within it.  If you want to
        // know all of the mind-bending details, Google for the terms: regex "balancing group".
        private static Regex reMatchRegion = new Regex(
            @"\#(pragma\s+)?region\s+(.*?(((?<Open>\#(pragma\s+)?region\s+).*?)+" +
            @"((?<Close-Open>\#(pragma\s+)?end\s?region).*?)+)*(?(Open)(?!)))" +
            @"\#(pragma\s+)?end\s?region", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // This is used to remove unwanted region markers from imported code
        private static Regex reRemoveRegionMarkers = new Regex(@"^.*?#(pragma\s+)?(region|end\s?region).*?$",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

        // XPath queries
        private XmlNamespaceManager context;
        private XPathExpression referenceRoot, referenceCode, conceptualRoot, conceptualCode, nestedRefCode,
            nestedConceptCode;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used by the <see cref="PostTransformComponent"/> to insert the colorized code blocks
        /// </summary>
        /// <remarks>The colorized code blocks contain HTML which isn't supported by conceptual topics
        /// pre-transformation.  As such, we hold onto them here and use a placeholder ID.  The post-transform
        /// component will replace the placeholder ID with the actual colorized code block.</remarks>
        public static Dictionary<string, XmlNode> ColorizedCodeBlocks
        {
            get { return colorizedCodeBlocks; }
        }

        /// <summary>
        /// This is used by the <see cref="PostTransformComponent"/> to get the destination location and filename
        /// of the "Copy" image.
        /// </summary>
        public static string CopyImageLocation
        {
            get { return copyImageLocation; }
        }

        /// <summary>
        /// This is used by the <see cref="PostTransformComponent"/> to get the "Copy" text so that it can be
        /// replaced with an include item.
        /// </summary>
        /// <remarks>We can't do it here as the TransformComponent strips the &lt;include&gt; tag as it isn't one
        /// it recognizes.</remarks>
        public static string CopyText
        {
            get { return copyText; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembler">A reference to the build assembler.</param>
        /// <param name="configuration">The configuration information</param>
        /// <remarks><b>NOTE:</b>  This component must be paired with the <see cref="PostTransformComponent"/>.
        /// See the <see cref="CodeBlockComponent"/> class topic for an example of the configuration and usage.</remarks>
        /// <exception cref="ConfigurationErrorsException">This is thrown if an error is detected in the
        /// configuration.</exception>
        public CodeBlockComponent(BuildAssembler assembler, XPathNavigator configuration) :
          base(assembler, configuration)
        {
            XPathNavigator nav;
            string value = null;
            bool allowMissingSource = false, useDefaultTitle = false;

            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            base.WriteMessage(MessageLevel.Info, String.Format(CultureInfo.InvariantCulture,
                "\r\n    [{0}, version {1}]\r\n    Code Block Component.  {2}.\r\n    Portions copyright (c) " +
                "2003, Jonathan de Halleux, All rights reserved.\r\n    http://SHFB.CodePlex.com",
                fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright));

            // The <basePath> element is optional.  If not set, it will assume the current folder as the base
            // path for source references with relative paths.
            nav = configuration.SelectSingleNode("basePath");

            if(nav != null)
                basePath = nav.GetAttribute("value", String.Empty);

            if(String.IsNullOrEmpty(basePath))
                basePath = Directory.GetCurrentDirectory();

            if(basePath[basePath.Length - 1] != '\\')
                basePath += @"\";

            // The <languageFilter> element is optional.  If not set, language filtering is enabled by default.
            nav = configuration.SelectSingleNode("languageFilter");

            if(nav != null)
            {
                value = nav.GetAttribute("value", String.Empty);

                if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value, out languageFilter))
                    throw new ConfigurationErrorsException("You must specify a Boolean value for the " +
                        "<languageFilter> 'value' attribute.");
            }
            else
                languageFilter = true;

            // The <allowMissingSource> element is optional.  If not set, missing source files generate an error.
            nav = configuration.SelectSingleNode("allowMissingSource");

            if(nav != null)
            {
                value = nav.GetAttribute("value", String.Empty);

                if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value, out allowMissingSource))
                    throw new ConfigurationErrorsException("You must specify a Boolean value for the " +
                        "<allowMissingSource> 'value' attribute.");
            }

            if(!allowMissingSource)
                messageLevel = MessageLevel.Error;
            else
                messageLevel = MessageLevel.Warn;

            // The <removeRegionMarkers> element is optional.  If not set, region markers in imported code are
            // left alone.
            nav = configuration.SelectSingleNode("removeRegionMarkers");

            if(nav != null)
            {
                value = nav.GetAttribute("value", String.Empty);

                if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value, out removeRegionMarkers))
                    throw new ConfigurationErrorsException("You must specify a Boolean value for the " +
                        "<removeRegionMarkers> 'value' attribute.");
            }

            // The <colorizer> element is required and defines the defaults for the code colorizer
            nav = configuration.SelectSingleNode("colorizer");

            if(nav == null)
                throw new ConfigurationErrorsException("You must specify a <colorizer> element to define the " +
                    "code colorizer options.");

            // The syntax configuration file and XSLT style file are required
            syntaxFile = nav.GetAttribute("syntaxFile", String.Empty);
            styleFile = nav.GetAttribute("styleFile", String.Empty);
            copyImage = nav.GetAttribute("copyImageUrl", String.Empty);

            if(String.IsNullOrEmpty(syntaxFile))
                throw new ConfigurationErrorsException("You must specify a 'syntaxFile' attribute on the " +
                    "<colorizer> element.");

            if(String.IsNullOrEmpty(styleFile))
                throw new ConfigurationErrorsException("You must specify a 'styleFile' attribute on the " +
                    "<colorizer> element.");

            if(String.IsNullOrEmpty(copyImage))
                throw new ConfigurationErrorsException("You must specify a 'copyImageUrl' attribute on the " +
                    "<colorizer> element.");

            // The syntax and style files must also exist.  The "copy" image is just a location and it doesn't
            // have to exist yet.
            if(!File.Exists(syntaxFile))
                throw new ConfigurationErrorsException("The specified syntax file could not be found: " +
                    syntaxFile);

            if(!File.Exists(styleFile))
                throw new ConfigurationErrorsException("The specified style file could not be found: " +
                    styleFile);

            // Optional attributes
            defaultLanguage = nav.GetAttribute("language", String.Empty);

            if(String.IsNullOrEmpty(defaultLanguage))
                defaultLanguage = "none";

            value = nav.GetAttribute("numberLines", String.Empty);

            if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value, out numberLines))
                throw new ConfigurationErrorsException("You must specify a Boolean value for the " +
                    "'numberLines' attribute.");

            value = nav.GetAttribute("outlining", String.Empty);

            if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value, out outliningEnabled))
                throw new ConfigurationErrorsException("You must specify a Boolean value for the " +
                    "'outlining' attribute.");

            value = nav.GetAttribute("keepSeeTags", String.Empty);

            if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value, out keepSeeTags))
                throw new ConfigurationErrorsException("You must specify a Boolean value for the " +
                    "'keepSeeTags' attribute.");

            value = nav.GetAttribute("tabSize", String.Empty);

            if(!String.IsNullOrEmpty(value) && !Int32.TryParse(value, out defaultTabSize))
                throw new ConfigurationErrorsException("You must specify an integer value for the 'tabSize' " +
                    "attribute.");

            value = nav.GetAttribute("defaultTitle", String.Empty);

            if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value, out useDefaultTitle))
                throw new ConfigurationErrorsException("You must specify a Boolean value for the " +
                    "'defaultTitle' attribute.");

            value = nav.GetAttribute("disabled", String.Empty);

            if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value, out isDisabled))
                throw new ConfigurationErrorsException("You must specify a Boolean value for the " +
                    "'disabled' attribute.");

            // Initialize the code colorizer
            colorizer = new CodeColorizer(syntaxFile, styleFile);
            colorizer.UseDefaultTitle = useDefaultTitle;
            colorizer.CopyImageUrl = copyImageLocation = copyImage;
            colorizer.TabSize = defaultTabSize;
            copyText = colorizer.CopyText;

            // Get the language filters in effect.  If a code block uses a language without a filter, it won't
            // have the attributes added to it so that it stays visible all the time.
            nav = configuration.Clone();
            nav.MoveToParent();
            filtersPresent = new List<string>();

            foreach(XPathNavigator filter in nav.Select("//language/@name"))
                filtersPresent.Add(filter.Value);

            // Create the XPath queries
            context = new CustomContext();
            context.AddNamespace("ddue", "http://ddue.schemas.microsoft.com/authoring/2003/5");
            referenceRoot = XPathExpression.Compile("document/comments");
            referenceCode = XPathExpression.Compile("//code");
            nestedRefCode = XPathExpression.Compile("code");
            conceptualRoot = XPathExpression.Compile("document/topic");
            conceptualCode = XPathExpression.Compile("//ddue:code");
            conceptualCode.SetContext(context);
            nestedConceptCode = XPathExpression.Compile("ddue:code");
            nestedConceptCode.SetContext(context);
        }
        #endregion

        #region Apply the component
        //=====================================================================

        /// <summary>
        /// This is implemented to perform the code colorization.
        /// </summary>
        /// <param name="document">The XML document with which to work.</param>
        /// <param name="key">The key (member name) of the item being documented.</param>
        public override void Apply(XmlDocument document, string key)
        {
            XPathNavigator root, navDoc = document.CreateNavigator();
            XPathNavigator[] codeList;
            XPathExpression nestedCode;
            XmlAttribute attr;
            XmlNode code, titleDiv, preNode, container, refLink;

            string language, title, codeBlock;
            bool nbrLines, outline, seeTags, filter;
            int tabSize, start, end, blockId = 1, id = 1;
            MessageLevel msgLevel;

            // Clear the dictionary
            colorizedCodeBlocks.Clear();

            // Select all code nodes.  The location depends on the build type.
            root = navDoc.SelectSingleNode(referenceRoot);

            // If not null, it's a reference (API) build.  If null, it's a conceptual build.
            if(root != null)
            {
                codeList = BuildComponentUtilities.ConvertNodeIteratorToArray(root.Select(referenceCode));
                nestedCode = nestedRefCode;
            }
            else
            {
                root = navDoc.SelectSingleNode(conceptualRoot);
                nestedCode = nestedConceptCode;

                if(root == null)
                {
                    base.WriteMessage(MessageLevel.Warn, "Root content node not found.  Cannot colorize code.");
                    return;
                }

                codeList = BuildComponentUtilities.ConvertNodeIteratorToArray(root.Select(conceptualCode));
            }

            foreach(XPathNavigator navCode in codeList)
            {
                code = ((IHasXmlNode)navCode).GetNode();

                // If the parent is null, it was a nested node and has already been handled
                if(code.ParentNode == null)
                    continue;

                // Set the defaults
                language = defaultLanguage;
                nbrLines = numberLines;
                outline = outliningEnabled;
                seeTags = keepSeeTags;
                filter = languageFilter;
                tabSize = 0;
                title = String.Empty;
                msgLevel = messageLevel;

                // Allow the "missing source" option to be overridden locally.  However, if false, it will
                // inherit the global setting.
                if(code.Attributes["allowMissingSource"] != null)
                    msgLevel = Convert.ToBoolean(code.Attributes["allowMissingSource"].Value,
                        CultureInfo.InvariantCulture) ? MessageLevel.Warn : messageLevel;

                // If there are nested code blocks, load them.  Source and region attributes will be ignored on
                // the parent.  All other attributes will be applied to the combined block of code.  If there are
                // no nested blocks, source and region will be used to load the code if found.  Otherwise, the
                // existing inner XML is used for the code.
                if(navCode.SelectSingleNode(nestedCode) != null)
                    codeBlock = this.LoadNestedCodeBlocks(navCode, nestedCode, msgLevel);
                else
                    if(code.Attributes["source"] != null)
                        codeBlock = this.LoadCodeBlock(code, msgLevel);
                    else
                        codeBlock = code.InnerXml;

                // Check for option overrides
                if(code.Attributes["numberLines"] != null)
                    nbrLines = Convert.ToBoolean(code.Attributes["numberLines"].Value,
                        CultureInfo.InvariantCulture);

                if(code.Attributes["outlining"] != null)
                    outline = Convert.ToBoolean(code.Attributes["outlining"].Value,
                        CultureInfo.InvariantCulture);

                if(code.Attributes["keepSeeTags"] != null)
                    seeTags = Convert.ToBoolean(code.Attributes["keepSeeTags"].Value,
                        CultureInfo.InvariantCulture);

                if(code.Attributes["filter"] != null)
                    filter = Convert.ToBoolean(code.Attributes["filter"].Value,
                        CultureInfo.InvariantCulture);

                if(code.Attributes["tabSize"] != null)
                    tabSize = Convert.ToInt32(code.Attributes["tabSize"].Value,
                        CultureInfo.InvariantCulture);

                // If either language option is set to "none" or an unknown language, it just strips excess
                // leading whitespace and optionally numbers the lines and adds outlining based on the other
                // settings.
                if(code.Attributes["lang"] != null)
                    language = code.Attributes["lang"].Value;
                else
                    if(code.Attributes["language"] != null)
                        language = code.Attributes["language"].Value;

                // Use the title if one is supplied.
                if(code.Attributes["title"] != null)
                    title = HttpUtility.HtmlEncode(code.Attributes["title"].Value);

                // If disabled, we'll just normalize the leading whitespace and let the Sandcastle transformation
                // handle it.  The language ID is passed to use the appropriate tab size if not overridden.
                if(isDisabled)
                {
                    code.InnerXml = colorizer.ProcessAndHighlightText(String.Format(CultureInfo.InvariantCulture,
                        "<code lang=\"{0}\" tabSize=\"{1}\" disabled=\"true\">{2}</code>", language, tabSize,
                        codeBlock));

                    continue;
                }

                // Process the code.  The colorizer is built to highlight <pre> tags in an HTML file so we'll
                // wrap the code in a <pre> tag with the settings.
                codeBlock = colorizer.ProcessAndHighlightText(String.Format(CultureInfo.InvariantCulture,
                    "<pre lang=\"{0}\" numberLines=\"{1}\" outlining=\"{2}\" " +
                    "keepSeeTags=\"{3}\" tabSize=\"{4}\" {5}>{6}</pre>",
                    language, nbrLines, outline, seeTags, tabSize,
                    (title.Length != 0) ? "title=\"" + title + "\"" : String.Empty, codeBlock));

                // Non-breaking spaces are replaced with a space entity.  If not, they disappear in the rendered
                // HTML.  Seems to be an XML or XSLT thing.
                codeBlock = codeBlock.Replace("&nbsp;", "&#x20;");

                // Move the title above the code block so that it doesn't interfere with the stuff generated
                // by the transformation component.  The post-transform component will perform some clean-up
                // to get rid of similar stuff added by it.
                title = codeBlock.Substring(0, codeBlock.IndexOf("</div>", StringComparison.Ordinal) + 6);
                codeBlock = codeBlock.Substring(title.Length);

                titleDiv = document.CreateDocumentFragment();
                titleDiv.InnerXml = title;
                titleDiv = titleDiv.ChildNodes[0];

                // Remove the colorizer's <pre> tag.  We'll add our own below.
                start = codeBlock.IndexOf('>') + 1;
                end = codeBlock.LastIndexOf('<');

                // We need to add the xml:space="preserve" attribute on the <pre> element so that MS Help Viewer
                // doesn't remove significant whitespace between colorized elements.
                preNode = document.CreateNode(XmlNodeType.Element, "pre", null);
                attr = document.CreateAttribute("xml:space");
                attr.Value = "preserve";
                preNode.Attributes.Append(attr);
                preNode.InnerXml = codeBlock.Substring(start, end - start);

                // Convert <see> tags to <referenceLink> or <a> tags.  We need to do this so that the Resolve
                // Links component can do its job further down the line.  The code blocks are not present when
                // the transformations run so that the colorized HTML doesn't get stripped out in conceptual
                // builds.  This could be redone to use a <markup> element if the Sandcastle transformations
                // ever support it natively.
                foreach(XmlNode seeTag in preNode.SelectNodes("//see"))
                {
                    if(seeTag.Attributes["cref"] != null)
                        refLink = document.CreateElement("referenceLink");
                    else
                        refLink = document.CreateElement("a");

                    foreach(XmlAttribute seeAttr in seeTag.Attributes)
                    {
                        if(seeAttr.Name == "cref")
                            attr = document.CreateAttribute("target");
                        else
                            attr = (XmlAttribute)seeAttr.Clone();

                        attr.Value = seeAttr.Value;
                        refLink.Attributes.Append(attr);
                    }

                    if(seeTag.HasChildNodes)
                        refLink.InnerXml = seeTag.InnerXml;

                    seeTag.ParentNode.ReplaceChild(refLink, seeTag);
                }

                // The <span> tags cannot be self-closing if empty.  The colorizer renders them correctly but
                // when written out as XML, they get converted to self-closing tags which breaks them.  To fix
                // them, store an empty string in each empty span so that it renders as an opening and closing
                // tag.  Note that if null, InnerText returns an empty string by default.  As such, this looks
                // redundant but it really isn't (see note above).
                foreach(XmlNode span in preNode.SelectNodes("//span"))
                    if(span.InnerText.Length == 0)
                        span.InnerText = String.Empty;

                // Add language filter stuff if needed or just the title if there is one and we aren't
                // using the language filter.
                if(filter)
                {
                    // If necessary, map the language ID to one we will recognize
                    language = language.ToLower(CultureInfo.InvariantCulture);

                    if(colorizer.AlternateIds.ContainsKey(language))
                        language = colorizer.AlternateIds[language];

                    container = this.AddLanguageFilter(titleDiv, preNode, language, blockId++);
                }
                else
                {
                    container = document.CreateNode(XmlNodeType.Element, "span", null);
                    container.AppendChild(titleDiv);
                    container.AppendChild(preNode);
                }

                // Replace the code with a placeholder ID.  The post-transform
                // component will relace it with the code from the container.
                code.InnerXml = "@@_SHFB_" + id.ToString(CultureInfo.InvariantCulture);

                // Add the container to the code block dictionary
                colorizedCodeBlocks.Add(code.InnerXml, container);
                id++;
            }
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to load a set of nested code blocks from external files
        /// </summary>
        /// <param name="navCode">The node in which to replace the nested code blocks</param>
        /// <param name="nestedCode">The XPath expression used to locate the nested code blocks.</param>
        /// <param name="msgLevel">The message level for missing source code</param>
        /// <returns>The HTML encoded blocks extracted from the files as a single code block</returns>
        /// <remarks>Only source and region attributes are used.  All other attributes are obtained from the
        /// parent code block.  Text nodes are created to replace the nested code tags so that any additional
        /// text in the parent code block is also retained.</remarks>
        private string LoadNestedCodeBlocks(XPathNavigator navCode, XPathExpression nestedCode,
          MessageLevel msgLevel)
        {
            XPathNavigator[] codeList = BuildComponentUtilities.ConvertNodeIteratorToArray(
                navCode.Select(nestedCode));

            foreach(XPathNavigator codeElement in codeList)
                codeElement.ReplaceSelf("\r\n" + this.LoadCodeBlock(((IHasXmlNode)codeElement).GetNode(),
                    msgLevel));

            return navCode.InnerXml;
        }

        /// <summary>
        /// This is used to load a code block from an external file.
        /// </summary>
        /// <param name="code">The node containing the attributes</param>
        /// <param name="msgLevel">The message level for missing source code</param>
        /// <returns>The HTML encoded block extracted from the file.</returns>
        private string LoadCodeBlock(XmlNode code, MessageLevel msgLevel)
        {
            XmlNode srcFile;
            Regex reFindRegion;
            Match find, m;
            bool removeRegions = removeRegionMarkers;
            string sourceFile = null, region = null, codeBlock = null;

            srcFile = code.Attributes["source"];

            if(srcFile != null)
                sourceFile = srcFile.Value;

            if(String.IsNullOrEmpty(sourceFile))
            {
                base.WriteMessage(msgLevel, "A nested <code> tag must contain a \"source\" attribute " +
                    "that specifies the source file to import");
                return "!ERROR: See log file!";
            }

            try
            {
                sourceFile = Environment.ExpandEnvironmentVariables(sourceFile);

                if(!Path.IsPathRooted(sourceFile))
                    sourceFile = Path.GetFullPath(basePath + sourceFile);

                using(StreamReader sr = new StreamReader(sourceFile))
                {
                    codeBlock = sr.ReadToEnd();
                }
            }
            catch(ArgumentException argEx)
            {
                base.WriteMessage(msgLevel, String.Format(CultureInfo.InvariantCulture,
                    "Possible invalid path '{0}{1}'.  Error: {2}", basePath, sourceFile, argEx.Message));
                return "!ERROR: See log file!";
            }
            catch(IOException ioEx)
            {
                base.WriteMessage(msgLevel, String.Format(CultureInfo.InvariantCulture,
                    "Unable to load source file '{0}'.  Error: {1}", sourceFile, ioEx.Message));
                return "!ERROR: See log file!";
            }

            // If no region is specified, the whole file is included
            if(code.Attributes["region"] != null)
            {
                region = code.Attributes["region"].Value;

                // Find the start of the region.  This gives us an immediate starting match on the second
                // search and we can look for the matching #endregion without caring about the region name.
                // Otherwise, nested regions get in the way and complicate things.
                reFindRegion = new Regex("\\#(pragma\\s+)?region\\s+\"?" + Regex.Escape(region),
                    RegexOptions.IgnoreCase);

                find = reFindRegion.Match(codeBlock);

                if(!find.Success)
                {
                    base.WriteMessage(msgLevel, String.Format(CultureInfo.InvariantCulture,
                        "Unable to locate start of region '{0}' in source file '{1}'", region, sourceFile));
                    return "!ERROR: See log file!";
                }

                // Find the end of the region taking into account any nested regions
                m = reMatchRegion.Match(codeBlock, find.Index);

                if(!m.Success)
                {
                    base.WriteMessage(msgLevel, String.Format(CultureInfo.InvariantCulture,
                        "Unable to extract region '{0}' in source file '{1}{2}' (missing #endregion?)",
                        region, basePath, sourceFile));
                    return "!ERROR: See log file!";
                }

                // Extract just the specified region starting after the description
                codeBlock = m.Groups[2].Value.Substring(m.Groups[2].Value.IndexOf('\n') + 1);

                // Strip off the trailing comment characters if present
                if(codeBlock[codeBlock.Length - 1] == ' ')
                    codeBlock = codeBlock.TrimEnd();

                // VB commented #End Region statement within a method body
                if(codeBlock[codeBlock.Length - 1] == '\'')
                    codeBlock = codeBlock.Substring(0, codeBlock.Length - 1);

                // XML/XAML commented #endregion statement
                if(codeBlock.EndsWith("<!--", StringComparison.Ordinal))
                    codeBlock = codeBlock.Substring(0, codeBlock.Length - 4);

                // C or SQL style commented #endregion statement
                if(codeBlock.EndsWith("/*", StringComparison.Ordinal) ||
                  codeBlock.EndsWith("--", StringComparison.Ordinal))
                    codeBlock = codeBlock.Substring(0, codeBlock.Length - 2);
            }

            if(code.Attributes["removeRegionMarkers"] != null &&
              !Boolean.TryParse(code.Attributes["removeRegionMarkers"].Value, out removeRegions))
                base.WriteMessage(MessageLevel.Warn, "Invalid removeRegionMarkers attribute value.  " +
                    "Option ignored.");

            if(removeRegions)
            {
                codeBlock = reRemoveRegionMarkers.Replace(codeBlock, String.Empty);
                codeBlock = codeBlock.Replace("\r\n\n", "\r\n");
            }

            // Return the HTML encoded block
            return HttpUtility.HtmlEncode(codeBlock);
        }

        /// <summary>
        /// This is used to add language filter IDs for the Prototype, VS2005, and Hana styles so that the code
        /// block is shown or hidden based on the language filter selection.
        /// </summary>
        /// <param name="title">The title node if used</param>
        /// <param name="code">The code node</param>
        /// <param name="language">The language to use as the filter</param>
        /// <param name="blockId">A unique ID number for the code block</param>
        /// <returns>The span node containing the title and code nodes</returns>
        /// <remarks>The <see cref="PostTransformComponent"/> adds the script necessary to register the code
        /// blocks and set their initial visible state in the Prototype and Hana styles.</remarks>
        private XmlNode AddLanguageFilter(XmlNode title, XmlNode code, string language, int blockId)
        {
            XmlDocument document = code.OwnerDocument;
            XmlNode span;
            XmlAttribute idAttr, xlangAttr, codeLangAttr;

            // Add the language filter attribute
            span = document.CreateNode(XmlNodeType.Element, "span", null);
            idAttr = document.CreateAttribute("id");
            xlangAttr = document.CreateAttribute("x-lang"); // Prototype / Hana
            codeLangAttr = document.CreateAttribute("codeLanguage"); // VS2005

            switch(language)
            {
                case "cs":
                    xlangAttr.Value = codeLangAttr.Value = "CSharp";
                    break;

                case "vbnet":
                    xlangAttr.Value = codeLangAttr.Value = "VisualBasic";
                    break;

                case "cpp":
                    xlangAttr.Value = codeLangAttr.Value = "ManagedCPlusPlus";
                    break;

                case "javascript":
                    xlangAttr.Value = codeLangAttr.Value = "JavaScript";
                    break;

                case "jsharp":
                    xlangAttr.Value = codeLangAttr.Value = "JSharp";
                    break;

                case "jscriptnet":
                    xlangAttr.Value = codeLangAttr.Value = "JScript";
                    break;

                case "xaml":
                    xlangAttr.Value = codeLangAttr.Value = "XAML";
                    break;

                default:
                    // Unknown, don't apply the filter.  It'll show up for all languages.  Just add the title if
                    // needed.
                    xlangAttr.Value = String.Empty;
                    codeLangAttr.Value = language;
                    break;
            }

            // If a filter for the language isn't there, don't connect it.  The code block will always be visible.
            if(xlangAttr.Value.Length != 0 && !filtersPresent.Contains(codeLangAttr.Value))
            {
                xlangAttr.Value = String.Empty;
                codeLangAttr.Value = "none";
            }

            if(xlangAttr.Value.Length != 0)
            {
                // The post-transform component does all the hard work if it finds spans with an ID attribute
                // starting with "cbc_".
                idAttr.Value = "cbc_" + blockId.ToString(CultureInfo.InvariantCulture);

                span.Attributes.Append(idAttr);
                span.Attributes.Append(xlangAttr);
                span.Attributes.Append(codeLangAttr);

                // Add the title
                span.AppendChild(title);

                // The Prototype style doesn't need the enclosing span but it doesn't hurt anything.  With the
                // introduction of custom styles in v1.4.0.0, it needs to be generic so elements are added to
                // cover all styles.
                span.AppendChild(code.Clone());
            }
            else
            {
                span.AppendChild(title);
                span.AppendChild(code.Clone());
            }

            return span;
        }
        #endregion

        #region Static configuration method for use with SHFB
        //=====================================================================

        /// <summary>
        /// This static method is used by the Sandcastle Help File Builder to let the component perform its own
        /// configuration.
        /// </summary>
        /// <param name="currentConfig">The current configuration XML fragment</param>
        /// <returns>A string containing the new configuration XML fragment</returns>
        public static string ConfigureComponent(string currentConfig)
        {
            using(CodeBlockConfigDlg dlg = new CodeBlockConfigDlg(currentConfig))
            {
                if(dlg.ShowDialog() == DialogResult.OK)
                    currentConfig = dlg.Configuration;
            }

            return currentConfig;
        }
        #endregion
    }
}
