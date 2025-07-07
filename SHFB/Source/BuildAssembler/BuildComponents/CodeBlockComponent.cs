//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : CodeBlockComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/05/2025
// Note    : Copyright 2006-2025, Eric Woodruff, All rights reserved
//
// This file contains a build component that is used to search for <code> XML comment tags and colorize the code
// within them.  It can also include code from an external file or a region within the file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/21/2006  EFW  Created the code
// 01/03/2007  EFW  Added support for VB.NET style #region blocks
// 02/02/2007  EFW  Made changes to support custom presentation styles and new colorizer options
// 06/12/2007  EFW  Added support for nested code blocks
// 06/19/2007  EFW  Various additions and updates for the June CTP
// 06/20/2007  EFW  Fixed bug that caused code blocks with an unknown or unspecified language to always be hidden
// 03/05/2008  EFW  Added support for the keepSeeTags attribute
// 04/05/2008  EFW  Modified to not add language filter elements if the matching language filter is not present.
//                  Updated to support use in conceptual builds.
// 07/22/2008  EFW  Fixed bug related to nested code blocks in conceptual content.  Added option to generate
//                  warnings instead of errors on missing source code.
// 12/02/2008  EFW  Fixed bug that caused <see> tags to go unprocessed due to change in code block handling.
//                  Added support for removeRegionMarkers.
// 06/19/2010  EFW  Added support for MS Help Viewer
// 12/30/2011  EFW  Added support for overriding allowMissingSource option on a case by case basis
// 09/21/2012  EFW  Added support disabling all features except leading whitespace normalization
// 10/17/2012  EFW  Moved the code block insertion code from PostTransformComponent into the new component event
//                  handler in this class.  Moved the title support into the presentation style XSL
//                  transformations.
// 12/26/2013  EFW  Updated the build component to be discoverable via MEF
// 02/27/2014  EFW  Added support for the Open XML help file format
// 04/24/2015  EFW  Added support for the Markdown help file format
// 04/12/2021  EFW  Merged SHFB build components into the main build components assembly
//===============================================================================================================

// Ignore Spelling: de Halleux lt

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

using ColorizerLibrary;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Sandcastle.Tools.BuildComponents
{
    /// <summary>
    /// This build component is used to search for &lt;code&gt; XML comment tags and colorize the code within
    /// them.  It can also include code from an external file or a region within the file.
    /// </summary>
    /// <remarks>The colorizer files are only copied once and only if code is actually colorized.  If the files
    /// already exist (i.e. additional content has replaced them), they are not copied either.  That way, you
    /// can customize the color style sheet as you see fit without modifying the default style sheet.</remarks>
    /// <example>
    /// <code language="xml" title="Example configuration">
    /// &lt;!-- Code block component configuration.  This must appear before
    ///      the TransformComponent. --&gt;
    /// &lt;component id="Code Block Component"&gt;
    ///     &lt;!-- Base path for relative filenames in source
    ///          attributes (optional). --&gt;
    ///     &lt;basePath value="..\SandcastleComponents" /&gt;
    ///
    ///     &lt;!-- Base output paths for the files (required).  These should
    ///          match the parent folder of the output path of the HTML files
    ///          used in the SaveComponent instances. --&gt;
    ///     &lt;outputPaths&gt;
    ///       &lt;path value="Output\HtmlHelp1\" /&gt;
    ///       &lt;path value="Output\MSHelpViewer\" /&gt;
    ///       &lt;path value="Output\Website\" /&gt;
    ///     &lt;/outputPaths&gt;
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
    ///         XSLT style sheet file (required)
    ///         CSS style sheet file (required)
    ///         Script file (required)
    ///         Disabled (optional, leading whitespace normalization only)
    ///         Default language (optional)
    ///         Enable line numbering (optional)
    ///         Enable outlining (optional)
    ///         Keep XML comment "see" tags within the code (optional)
    ///         Tab size for unknown languages (optional, 0 = use default)
    ///         Use language name as default title (optional) --&gt;
    ///     &lt;colorizer syntaxFile="highlight.xml" styleFile="highlight.xsl"
    ///       stylesheet="highlight.css" scriptFile="highlight.js"
    ///       disabled="false" language="cs" numberLines="false" outlining="false"
    ///       keepSeeTags="false" tabSize="0" defaultTitle="true" /&gt;
    /// &lt;/component&gt;
    /// </code>
    ///
    /// <code language="xml" title="Examples as used in XML comments.">
    /// &lt;example&gt;
    /// A basic code block that uses the configuration defaults:
    /// &lt;code&gt;
    /// /// Code to colorize
    /// &lt;/code&gt;
    ///
    /// Override options with block-specific options:
    /// &lt;code language="xml" numberLines="true" outlining="false" tabSize="8" &gt;
    ///     &amp;lt;XmlTags/&amp;gt;
    /// &lt;/code&gt;
    ///
    /// An entire external file or a delimited region from it can be
    /// included.  This allows you to compile your example code externally
    /// to ensure that it is still valid and saves you from maintaining it
    /// in two places.
    ///
    /// Retrieve all code from an external file.  Use VB.NET syntax.
    /// &lt;code source="..\Examples\WholeDemo.vb" language="vbnet"/&gt;
    ///
    /// Retrieve a specific #region from an external file.
    /// &lt;code source="..\Examples\SeveralExamples.vb"
    ///     region="Example 1" language="vbnet"/&gt;
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
    public class CodeBlockComponent : BuildComponentCore
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component used to colorize code blocks
        /// </summary>
        [BuildComponentExport("Code Block Component", IsVisible = true, Version = AssemblyInfo.ProductVersion,
          Copyright = AssemblyInfo.Copyright, Description = "This build component is used to search for <code> " +
            "elements within reference XML comments and conceptual content topics and colorize the code within " +
            "them.  It can also include code from an external file or a region within the file.")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public Factory()
            {
                this.ReferenceBuildPlacement = new ComponentPlacement(PlacementAction.Before,
                    "Transform Component");
                this.ConceptualBuildPlacement = new ComponentPlacement(PlacementAction.Before,
                    "Transform Component");
            }

            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new CodeBlockComponent(this.BuildAssembler);
            }

            /// <inheritdoc />
            public override string DefaultConfiguration =>
@"<!-- Base path for relative filenames in source attributes (optional) -->
<basePath value=""{@HtmlEncProjectFolder}"" />

<!-- Base output paths for the files (required).  These should match the parent folder of the output path
	 of the HTML files (see each of the SaveComponent instances in the configuration files). -->
<outputPaths>
	{@HelpFormatOutputPaths}
</outputPaths>

<!-- Allow missing source files (Optional).  If omitted, it will generate errors if referenced source files
	 are missing. -->
<allowMissingSource value=""false"" />

<!-- Remove region markers from imported code blocks.  If omitted, region markers in imported code blocks
	 are left alone. -->
<removeRegionMarkers value=""false"" />

<!-- Code colorizer options (required).
	 Attributes:
		Language syntax configuration file (required)
		XSLT style sheet file (required)
		CSS style sheet file (required)
		Script file (required)
		Disabled (optional, leading whitespace normalization only)
		Default language (optional)
		Enable line numbering (optional)
		Enable outlining (optional)
		Keep XML comment ""see"" tags within the code (optional)
		Tab size override (optional, 0 = Use syntax file setting)
		Use language name as default title (optional) -->
<colorizer syntaxFile=""{@CoreComponentsFolder}Colorizer\highlight.xml""
	styleFile=""{@CoreComponentsFolder}Colorizer\highlight.xsl""
	stylesheet=""{@CoreComponentsFolder}Colorizer\highlight.css""
	scriptFile=""{@CoreComponentsFolder}Colorizer\highlight.js""
	disabled=""{@DisableCodeBlockComponent}"" language=""cs"" numberLines=""false"" outlining=""false""
	keepSeeTags=""false"" tabSize=""0"" defaultTitle=""true"" />";
        }
        #endregion

        #region Private data members
        //=====================================================================

        // Colorized code dictionary used by the OnComponent event handler
        private Dictionary<string, Dictionary<string, XmlNode>> topicCodeBlocks;

        private bool hasColorizedCodeBlocks;

        // Output folder paths
        private List<string> outputPaths;

        private CodeColorizer colorizer;    // The code colorizer

        // The style sheet, script, and image files to include and the output path
        private string stylesheet, scriptFile, stylesheetAttrPath, scriptFileAttrPath;

        // Line numbering, outlining, keep see tags, remove region markers, disabled, files copied, Open XML,
        // and Markdown flags.
        private bool numberLines, outliningEnabled, keepSeeTags, removeRegionMarkers, isDisabled,
            isOpenXml, isMarkdown;

        // The base path to use for file references with relative paths and the default language
        private string basePath, defaultLanguage;

        // The message level for missing source errors
        private MessageLevel messageLevel;

        // Uh, yeah.  Don't ask me to explain this.  Just accept that it works (I hope :)).  It uses balancing
        // groups to extract #region to #endregion accounting for any nested regions within it.  If you want to
        // know all of the mind-bending details, Google for the terms: regex "balancing group".
        private static readonly Regex reMatchRegion = new(
            @"\#(pragma\s+)?region\s+(.*?(((?<Open>\#(pragma\s+)?region\s+).*?)+" +
            @"((?<Close-Open>\#(pragma\s+)?end\s?region).*?)+)*(?(Open)(?!)))" +
            @"\#(pragma\s+)?end\s?region", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // This is used to remove unwanted region markers from imported code
        private static readonly Regex reRemoveRegionMarkers = new(@"^.*?#(pragma\s+)?(region|end\s?region).*?$",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

        // XPath queries
        private XPathExpression referenceRoot, referenceCode, conceptualRoot, conceptualCode, nestedRefCode,
            nestedConceptCode;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected CodeBlockComponent(IBuildAssembler buildAssembler) : base(buildAssembler)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>See the <see cref="CodeBlockComponent"/> class topic for an example of the configuration and
        /// usage.</remarks>
        /// <exception cref="ArgumentException">This is thrown if an error is detected in the
        /// configuration.</exception>
        public override void Initialize(XPathNavigator configuration)
        {
            XPathNavigator nav;
            string value = null, syntaxFile, styleFile;
            bool allowMissingSource = false, useDefaultTitle = false;
            int defaultTabSize = 8;

            outputPaths = [];
            topicCodeBlocks = [];

            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            this.WriteMessage(MessageLevel.Info, "[{0}, version {1}]\r\n    Code Block Component.  " +
                "{2}.\r\n    Portions copyright (c) 2003-2015, Jonathan de Halleux, All rights reserved.\r\n" +
                "    https://GitHub.com/EWSoftware/SHFB", fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright);

            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            // The <basePath> element is optional.  If not set, it will assume the current folder as the base
            // path for source references with relative paths.
            nav = configuration.SelectSingleNode("basePath");

            if(nav != null)
                basePath = nav.GetAttribute("value", String.Empty);

            if(String.IsNullOrEmpty(basePath))
                basePath = Directory.GetCurrentDirectory();

            // Get the output paths
            foreach(XPathNavigator path in configuration.Select("outputPaths/path"))
            {
                value = path.GetAttribute("value", String.Empty);

                if(!Directory.Exists(value))
                    Directory.CreateDirectory(value);

                outputPaths.Add(value);

                // The Open XML format doesn't support all features and requires a custom transformation
                if(value.IndexOf("OpenXML", StringComparison.OrdinalIgnoreCase) != -1)
                    isOpenXml = true;

                // The Markdown format doesn't support any features
                if(value.IndexOf("Markdown", StringComparison.OrdinalIgnoreCase) != -1)
                    isMarkdown = true;
            }

            if(outputPaths.Count == 0)
            {
                throw new ArgumentException("You must specify at least one <path> element in the " +
                    "<outputPaths> element.  You may need to delete and re-add the component to the project " +
                    "to obtain updated configuration settings.", nameof(configuration));
            }

            // The <allowMissingSource> element is optional.  If not set, missing source files generate an error.
            nav = configuration.SelectSingleNode("allowMissingSource");

            if(nav != null)
            {
                value = nav.GetAttribute("value", String.Empty);

                if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value, out allowMissingSource))
                {
                    throw new ArgumentException("You must specify a Boolean value for the " +
                        "<allowMissingSource> 'value' attribute.", nameof(configuration));
                }
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
                {
                    throw new ArgumentException("You must specify a Boolean value for the " +
                        "<removeRegionMarkers> 'value' attribute.", nameof(configuration));
                }
            }

            // The <colorizer> element is required and defines the defaults for the code colorizer
            nav = configuration.SelectSingleNode("colorizer");

            if(nav == null)
            {
                throw new ArgumentException("You must specify a <colorizer> element to define the code " +
                    "colorizer options.", nameof(configuration));
            }

            // The file and URL values are all required
            syntaxFile = nav.GetAttribute("syntaxFile", String.Empty);
            styleFile = nav.GetAttribute("styleFile", String.Empty);
            stylesheet = nav.GetAttribute("stylesheet", String.Empty);
            scriptFile = nav.GetAttribute("scriptFile", String.Empty);

            if(String.IsNullOrEmpty(syntaxFile))
            {
                throw new ArgumentException("You must specify a 'syntaxFile' attribute on the " +
                    "<colorizer> element.", nameof(configuration));
            }

            if(String.IsNullOrEmpty(styleFile))
            {
                throw new ArgumentException("You must specify a 'styleFile' attribute on the " +
                    "<colorizer> element.", nameof(configuration));
            }

            if(String.IsNullOrEmpty(stylesheet) && !isOpenXml && !isMarkdown)
            {
                throw new ArgumentException("You must specify a 'stylesheet' attribute on the " +
                    "<colorizer> element", nameof(configuration));
            }

            if(String.IsNullOrEmpty(scriptFile) && !isOpenXml && !isMarkdown)
            {
                throw new ArgumentException("You must specify a 'scriptFile' attribute on the " +
                    "<colorizer> element", nameof(configuration));
            }

            // The syntax and style files must also exist.  The "copy" image URL is just a location and it
            // doesn't have to exist yet.
            syntaxFile = Path.GetFullPath(syntaxFile);
            styleFile = Path.GetFullPath(styleFile);

            if(!File.Exists(syntaxFile))
            {
                throw new ArgumentException("The specified syntax file could not be found: " + syntaxFile,
                    nameof(configuration));
            }

            if(!File.Exists(styleFile))
            {
                throw new ArgumentException("The specified style file could not be found: " + styleFile,
                    nameof(configuration));
            }

            if(!isOpenXml && !isMarkdown)
            {
                stylesheet = Path.GetFullPath(stylesheet);
                scriptFile = Path.GetFullPath(scriptFile);

                if(!File.Exists(stylesheet))
                    throw new ArgumentException("Could not find style sheet file: " + stylesheet, nameof(configuration));

                if(!File.Exists(scriptFile))
                    throw new ArgumentException("Could not find script file: " + scriptFile, nameof(configuration));

                stylesheetAttrPath = Path.Combine(this.BuildAssembler.TopicTransformation.StyleSheetPath,
                    Path.GetFileName(stylesheet));
                scriptFileAttrPath = Path.Combine(this.BuildAssembler.TopicTransformation.ScriptPath,
                    Path.GetFileName(scriptFile));
            }

            // Optional attributes
            defaultLanguage = nav.GetAttribute("language", String.Empty);

            if(String.IsNullOrEmpty(defaultLanguage))
                defaultLanguage = "none";

            value = nav.GetAttribute("numberLines", String.Empty);

            if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value, out numberLines))
            {
                throw new ArgumentException("You must specify a Boolean value for the " +
                    "'numberLines' attribute.", nameof(configuration));
            }

            value = nav.GetAttribute("outlining", String.Empty);

            if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value, out outliningEnabled))
            {
                throw new ArgumentException("You must specify a Boolean value for the 'outlining' attribute.",
                    nameof(configuration));
            }

            value = nav.GetAttribute("keepSeeTags", String.Empty);

            if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value, out keepSeeTags))
            {
                throw new ArgumentException("You must specify a Boolean value for the 'keepSeeTags' attribute.",
                    nameof(configuration));
            }

            value = nav.GetAttribute("tabSize", String.Empty);

            if(!String.IsNullOrEmpty(value) && !Int32.TryParse(value, out defaultTabSize))
            {
                throw new ArgumentException("You must specify an integer value for the 'tabSize' attribute.",
                    nameof(configuration));
            }

            value = nav.GetAttribute("defaultTitle", String.Empty);

            if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value, out useDefaultTitle))
            {
                throw new ArgumentException("You must specify a Boolean value for the 'defaultTitle' attribute.",
                    nameof(configuration));
            }

            if(!isMarkdown)
            {
                value = nav.GetAttribute("disabled", String.Empty);

                if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value, out isDisabled))
                {
                    throw new ArgumentException("You must specify a Boolean value for the 'disabled' attribute.",
                        nameof(configuration));
                }
            }
            else
                isDisabled = true;      // Markdown doesn't support anything so it is always disabled

            if(isOpenXml)
            {
                numberLines = outliningEnabled = false;

                // If the default transform is specified, switch to the Open XML version.  This can happen if
                // the user adds the code block component to their project to override the default settings.
                string defaultTransform = Path.Combine(Path.GetDirectoryName(asm.Location),
                    @"Colorizer\highlight.xsl");

                if(styleFile.Equals(defaultTransform, StringComparison.OrdinalIgnoreCase))
                {
                    styleFile = Path.Combine(Path.GetDirectoryName(defaultTransform), "highlight_openxml.xsl");

                    if(!File.Exists(styleFile))
                    {
                        throw new ArgumentException("The specified style file could not be found: " + styleFile,
                            nameof(configuration));
                    }
                }
            }

            // Initialize the code colorizer
            colorizer = new CodeColorizer(syntaxFile, styleFile)
            {
                UseDefaultTitle = useDefaultTitle,
                TabSize = defaultTabSize
            };

            // Share the language ID mappings so that other components like the Syntax Component can get titles
            // for languages it doesn't know about.
            this.BuildAssembler.Data["LanguageIds"] = colorizer.FriendlyNames;

            // Create the XPath queries
            var context = new CustomContext();

            context.AddNamespace("ddue", "http://ddue.schemas.microsoft.com/authoring/2003/5");

            referenceRoot = XPathExpression.Compile("document/comments");
            referenceCode = XPathExpression.Compile("//code");
            nestedRefCode = XPathExpression.Compile("code");
            conceptualRoot = XPathExpression.Compile("document/topic");
            conceptualCode = XPathExpression.Compile("//ddue:code");
            conceptualCode.SetContext(context);
            nestedConceptCode = XPathExpression.Compile("ddue:code");
            nestedConceptCode.SetContext(context);

            // Hook up the event handler to complete the process after the topic is transformed to HTML
            this.BuildAssembler.ComponentEvent += TransformComponent_TopicTransformed;
        }

        /// <summary>
        /// This is implemented to perform the code colorization.
        /// </summary>
        /// <param name="document">The XML document with which to work.</param>
        /// <param name="key">The key (member name) of the item being documented.</param>
        public override void Apply(XmlDocument document, string key)
        {
            if(document == null)
                throw new ArgumentNullException(nameof(document));

            XPathNavigator root, navDoc = document.CreateNavigator();
            XPathNavigator[] codeList;
            XPathExpression nestedCode;
            XmlAttribute attr;
            XmlNode code, preNode;
            XmlElement refLink;

            string language, codeBlock;
            bool nbrLines, outline, seeTags;
            int tabSize, start, end, id = 1;
            MessageLevel msgLevel;

            // Select all code nodes.  The location depends on the build type.
            root = navDoc.SelectSingleNode(referenceRoot);

            // If not null, it's a reference (API) build.  If null, it's a conceptual build.
            if(root != null)
            {
                codeList = root.Select(referenceCode).ToArray();
                nestedCode = nestedRefCode;
            }
            else
            {
                root = navDoc.SelectSingleNode(conceptualRoot);
                nestedCode = nestedConceptCode;

                if(root == null)
                {
                    this.WriteMessage(key, MessageLevel.Warn, "Root content node not found.  Cannot colorize code.");
                    return;
                }

                codeList = root.Select(conceptualCode).ToArray();
            }

            var colorizedCodeBlocks = new Dictionary<string, XmlNode>();

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
                tabSize = 0;
                msgLevel = messageLevel;

                // Allow the "missing source" option to be overridden locally.  However, if false, it will
                // inherit the global setting.
                if(code.Attributes["allowMissingSource"] != null)
                {
                    msgLevel = Convert.ToBoolean(code.Attributes["allowMissingSource"].Value,
                        CultureInfo.InvariantCulture) ? MessageLevel.Warn : messageLevel;
                }

                // If there are nested code blocks, load them.  Source and region attributes will be ignored on
                // the parent.  All other attributes will be applied to the combined block of code.  If there are
                // no nested blocks, source and region will be used to load the code if found.  Otherwise, the
                // existing inner XML is used for the code.
                if(navCode.SelectSingleNode(nestedCode) != null)
                    codeBlock = this.LoadNestedCodeBlocks(key, navCode, nestedCode, msgLevel);
                else
                {
                    if(code.Attributes["source"] != null)
                        codeBlock = this.LoadCodeBlock(key, code, msgLevel);
                    else
                        codeBlock = code.InnerXml;
                }

                // Check for option overrides
                if(code.Attributes["numberLines"] != null)
                    nbrLines = Convert.ToBoolean(code.Attributes["numberLines"].Value, CultureInfo.InvariantCulture);

                if(code.Attributes["outlining"] != null)
                    outline = Convert.ToBoolean(code.Attributes["outlining"].Value, CultureInfo.InvariantCulture);

                if(code.Attributes["keepSeeTags"] != null)
                    seeTags = Convert.ToBoolean(code.Attributes["keepSeeTags"].Value, CultureInfo.InvariantCulture);

                if(code.Attributes["tabSize"] != null)
                    tabSize = Convert.ToInt32(code.Attributes["tabSize"].Value, CultureInfo.InvariantCulture);

                // If either language option is set to "none" or an unknown language, it just strips excess
                // leading whitespace and optionally numbers the lines and adds outlining based on the other
                // settings.
                if(code.Attributes["lang"] != null)
                {
                    language = code.Attributes["lang"].Value;

                    // The transformations consistently use "language" so change the attribute name
                    attr = document.CreateAttribute("language");
                    attr.Value = language;
                    code.Attributes.Remove(code.Attributes["lang"]);
                    code.Attributes.Append(attr);
                }
                else
                {
                    if(code.Attributes["language"] != null)
                        language = code.Attributes["language"].Value;
                    else
                    {
                        // Add the attribute with the default language
                        attr = document.CreateAttribute("language");
                        attr.Value = language;
                        code.Attributes.Append(attr);
                    }
                }

                if(isOpenXml)
                    nbrLines = outline = false;

                // If disabled, we'll just normalize the leading whitespace and let the Sandcastle transformation
                // handle it.  The language ID is passed to use the appropriate tab size if not overridden.
                if(isDisabled)
                {
                    // Pass through the default line numbering option if enabled
                    if(nbrLines && code.Attributes["numberLines"] == null)
                    {
                        attr = document.CreateAttribute("numberLines");
                        attr.Value = "true";
                        code.Attributes.Append(attr);
                    }

                    code.InnerXml = colorizer.ProcessAndHighlightText(String.Format(CultureInfo.InvariantCulture,
                        "<code lang=\"{0}\" tabSize=\"{1}\" disabled=\"true\">{2}</code>", language, tabSize,
                        codeBlock));

                    continue;
                }

                // Process the code.  The colorizer is built to highlight <pre> tags in an HTML file so we'll
                // wrap the code in a <pre> tag with the settings.
                codeBlock = colorizer.ProcessAndHighlightText(String.Format(CultureInfo.InvariantCulture,
                    "<pre lang=\"{0}\" numberLines=\"{1}\" outlining=\"{2}\" keepSeeTags=\"{3}\" " +
                    "tabSize=\"{4}\">{5}</pre>", language, nbrLines, outline, seeTags, tabSize, codeBlock));

                // Non-breaking spaces are replaced with a space entity.  If not, they disappear in the rendered
                // HTML.  Seems to be an XML or XSLT thing.
                codeBlock = codeBlock.Replace("&nbsp;", "&#x20;");

                // Get the location of the actual code excluding the title div and pre elements
                start = codeBlock.IndexOf('>', codeBlock.IndexOf("</div>", StringComparison.Ordinal) + 6) + 1;
                end = codeBlock.LastIndexOf('<');

                preNode = document.CreateNode(XmlNodeType.Element, "pre", null);
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

                // Replace the code with a placeholder ID.  The OnComponent event handler will replace it with
                // the code from the container node.
                code.InnerXml = "@@_SHFB_" + id.ToString(CultureInfo.InvariantCulture);

                // Add the container to the code block dictionary
                colorizedCodeBlocks.Add(code.InnerXml, preNode);
                id++;
            }

            if(colorizedCodeBlocks.Count != 0)
                topicCodeBlocks.Add(key, colorizedCodeBlocks);
        }

        /// <summary>
        /// At disposal, copy the script and style files if any topics with code blocks were encountered
        /// </summary>
        /// <param name="disposing">Pass true to dispose of the managed and unmanaged resources or false to just
        /// dispose of the unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            string destStylesheet, destScriptFile;

            if(disposing && hasColorizedCodeBlocks)
            {
                foreach(string outputPath in outputPaths)
                {
                    destStylesheet = Path.Combine(outputPath, stylesheetAttrPath.Replace("../", String.Empty));
                    destScriptFile = Path.Combine(outputPath, scriptFileAttrPath.Replace("../", String.Empty));

                    if(Path.DirectorySeparatorChar != '/')
                    {
                        destStylesheet = destStylesheet.Replace('/', Path.DirectorySeparatorChar);
                        destScriptFile = destScriptFile.Replace('/', Path.DirectorySeparatorChar);
                    }

                    if(!Directory.Exists(Path.GetDirectoryName(destStylesheet)))
                        Directory.CreateDirectory(Path.GetDirectoryName(destStylesheet));

                    if(!Directory.Exists(Path.GetDirectoryName(destScriptFile)))
                        Directory.CreateDirectory(Path.GetDirectoryName(destScriptFile));

                    // Don't copy if already there (i.e. overridden by a copy in the project or copied by another
                    // instance).
                    if(!File.Exists(destStylesheet))
                    {
                        File.Copy(stylesheet, destStylesheet);

                        // All attributes are turned off so that we can delete it later
                        File.SetAttributes(destStylesheet, FileAttributes.Normal);
                    }

                    // Raise an event to indicate that a file was created
                    OnComponentEvent(new FileCreatedEventArgs(this.GroupId, "Code Block Component", null,
                        destStylesheet, true));

                    if(!File.Exists(destScriptFile))
                    {
                        File.Copy(scriptFile, destScriptFile);
                        File.SetAttributes(destScriptFile, FileAttributes.Normal);
                    }

                    OnComponentEvent(new FileCreatedEventArgs(this.GroupId, "Code Block Component", null,
                        destScriptFile, true));
                }
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to load a set of nested code blocks from external files
        /// </summary>
        /// <param name="key">The topic key</param>
        /// <param name="navCode">The node in which to replace the nested code blocks</param>
        /// <param name="nestedCode">The XPath expression used to locate the nested code blocks.</param>
        /// <param name="msgLevel">The message level for missing source code</param>
        /// <returns>The HTML encoded blocks extracted from the files as a single code block</returns>
        /// <remarks>Only source and region attributes are used.  All other attributes are obtained from the
        /// parent code block.  Text nodes are created to replace the nested code tags so that any additional
        /// text in the parent code block is also retained.</remarks>
        private string LoadNestedCodeBlocks(string key, XPathNavigator navCode, XPathExpression nestedCode,
          MessageLevel msgLevel)
        {
            foreach(XPathNavigator codeElement in navCode.Select(nestedCode).ToArray())
                codeElement.ReplaceSelf("\r\n" + this.LoadCodeBlock(key, ((IHasXmlNode)codeElement).GetNode(),
                    msgLevel));

            return navCode.InnerXml;
        }

        /// <summary>
        /// This is used to load a code block from an external file.
        /// </summary>
        /// <param name="key">The topic key</param>
        /// <param name="code">The node containing the attributes</param>
        /// <param name="msgLevel">The message level for missing source code</param>
        /// <returns>The HTML encoded block extracted from the file.</returns>
        private string LoadCodeBlock(string key, XmlNode code, MessageLevel msgLevel)
        {
            Regex reFindRegion;
            Match find, m;
            bool removeRegions = removeRegionMarkers;
            string sourceFile = null, codeBlock = null;

            XmlAttribute srcFile = code.Attributes["source"];

            if(srcFile != null)
                sourceFile = srcFile.Value;

            if(String.IsNullOrWhiteSpace(sourceFile))
            {
                this.WriteMessage(key, msgLevel, "A nested <code> tag must contain a \"source\" attribute " +
                    "that specifies the source file to import");
                return "!ERROR: See log file!";
            }

            try
            {
                sourceFile = Environment.ExpandEnvironmentVariables(sourceFile);

                if(!Path.IsPathRooted(sourceFile))
                    sourceFile = Path.GetFullPath(Path.Combine(basePath, sourceFile));

                using StreamReader sr = new(sourceFile);
                codeBlock = sr.ReadToEnd();
            }
            catch(ArgumentException argEx)
            {
                this.WriteMessage(key, msgLevel, "Possible invalid path '{0}{1}'.  Cause: {2}", basePath,
                    sourceFile, argEx.Message);
                return "!ERROR: See log file!";
            }
            catch(IOException ioEx)
            {
                this.WriteMessage(key, msgLevel, "Unable to load source file '{0}'.  Cause: {1}", sourceFile,
                    ioEx.Message);
                return "!ERROR: See log file!";
            }

            // If no region is specified, the whole file is included
            if(code.Attributes["region"] != null)
            {
                string region = code.Attributes["region"].Value;

                // Find the start of the region.  This gives us an immediate starting match on the second
                // search and we can look for the matching #endregion without caring about the region name.
                // Otherwise, nested regions get in the way and complicate things.  The bit at the end ensures
                // that shorter region names aren't matched in longer ones with the same start that occur before
                // the shorter one.
                reFindRegion = new Regex("\\#(pragma\\s+)?region\\s+\"?" + Regex.Escape(region) +
                    "\\s*?[\"->]*?\\s*?[\\r\\n]", RegexOptions.IgnoreCase);

                find = reFindRegion.Match(codeBlock);

                if(!find.Success)
                {
                    this.WriteMessage(key, msgLevel, "Unable to locate start of region '{0}' in source file '{1}'",
                        region, sourceFile);
                    return "!ERROR: See log file!";
                }

                // Find the end of the region taking into account any nested regions
                m = reMatchRegion.Match(codeBlock, find.Index);

                if(!m.Success)
                {
                    this.WriteMessage(key, msgLevel, "Unable to extract region '{0}' in source file '{1}{2}' " +
                        "(missing #endregion?)", region, basePath, sourceFile);
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

                // C, F#, or SQL style commented #endregion statement
                if(codeBlock.EndsWith("/*", StringComparison.Ordinal) ||
                  codeBlock.EndsWith("//", StringComparison.Ordinal) ||
                  codeBlock.EndsWith("(*", StringComparison.Ordinal) ||
                  codeBlock.EndsWith("--", StringComparison.Ordinal))
                {
                    codeBlock = codeBlock.Substring(0, codeBlock.Length - 2);
                }

                // Batch file remark
                if(codeBlock.EndsWith("REM", StringComparison.OrdinalIgnoreCase) && codeBlock.Length > 3 &&
                  (codeBlock[codeBlock.Length - 4] == '\r' || codeBlock[codeBlock.Length - 4] == '\n'))
                {
                    codeBlock = codeBlock.Substring(0, codeBlock.Length - 3);
                }
            }

            if(code.Attributes["removeRegionMarkers"] != null &&
              !Boolean.TryParse(code.Attributes["removeRegionMarkers"].Value, out removeRegions))
            {
                this.WriteMessage(key, MessageLevel.Warn, "Invalid removeRegionMarkers attribute value.  " +
                    "Option ignored.");
            }

            if(removeRegions)
            {
                codeBlock = reRemoveRegionMarkers.Replace(codeBlock, String.Empty);
                codeBlock = codeBlock.Replace("\r\n\n", "\r\n");
            }

            // Return the HTML encoded block
            return WebUtility.HtmlEncode(codeBlock);
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// This is used to complete the process by inserting the colorized code within the topic after it has
        /// been transformed to HTML.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        /// <remarks>A two-phase approach is needed as the HTML for the colorized code wouldn't make it through
        /// the conceptual content XSL transformations.</remarks>
        private void TransformComponent_TopicTransformed(object sender, EventArgs e)
        {
            XmlNode head, node;
            XmlAttribute attr;

            // Don't bother if not a transform event, not in our group, or if the topic contained no code blocks
            if(e is not AppliedChangesEventArgs ac || ac.GroupId != this.GroupId ||
              ac.ComponentId != "Transform Component" ||
              !topicCodeBlocks.TryGetValue(ac.Key, out Dictionary<string, XmlNode> colorizedCodeBlocks))
            {
                return;
            }

            topicCodeBlocks.Remove(ac.Key);

            if(!isOpenXml && !isMarkdown)
            {
                // Note topics with colorized code blocks so that we can copy the supporting files when disposed
                hasColorizedCodeBlocks = true;

                // Find the <head> section
                head = ac.Document.SelectSingleNode("html/head");

                if(head == null)
                {
                    this.WriteMessage(ac.Key, MessageLevel.Error, "<head> section not found!  Could not insert links.");
                    return;
                }

                // Add the link to the style sheet
                node = ac.Document.CreateNode(XmlNodeType.Element, "link", null);

                attr = ac.Document.CreateAttribute("type");
                attr.Value = "text/css";
                node.Attributes.Append(attr);

                attr = ac.Document.CreateAttribute("rel");
                attr.Value = "stylesheet";
                node.Attributes.Append(attr);

                attr = ac.Document.CreateAttribute("href");
                attr.Value = stylesheetAttrPath;
                node.Attributes.Append(attr);

                head.AppendChild(node);

                // Add the link to the script
                node = ac.Document.CreateNode(XmlNodeType.Element, "script", null);

                attr = ac.Document.CreateAttribute("type");
                attr.Value = "text/javascript";
                node.Attributes.Append(attr);

                attr = ac.Document.CreateAttribute("src");
                attr.Value = scriptFileAttrPath;
                node.Attributes.Append(attr);

                // Script tags cannot be self-closing so set their inner text to a space so that they render as
                // an opening and a closing tag.
                node.InnerXml = " ";

                head.AppendChild(node);
            }

            // The "local-name()" part of the query is for Open XML style which add a namespace to the element.
            // I could have created a context for the namespace but this is quick and it works for all cases.
            foreach(XmlNode codeContainer in ac.Document.SelectNodes(
              "//pre[starts-with(.,'@@_SHFB_')]|//*[(local-name() = 'pre' or local-name() = 't') and starts-with(.,'@@_SHFB_')]"))
            {
                XmlNode placeholder = codeContainer;

                if(colorizedCodeBlocks.TryGetValue(placeholder.InnerText, out XmlNode codeBlock))
                {
                    if(!isOpenXml)
                    {
                        // Make sure spacing is preserved
                        if(placeholder.Attributes["xml:space"] == null)
                        {
                            attr = ac.Document.CreateAttribute("xml:space");
                            attr.Value = "preserve";
                            placeholder.Attributes.Append(attr);
                        }
                    }
                    else
                    {
                        // Replace the w:t element's parent paragraph content with the code block.  The Open XML
                        // file builder task will fix up the formatting.
                        placeholder = placeholder.ParentNode.ParentNode;
                    }

                    placeholder.InnerXml = codeBlock.InnerXml;

                    // The <span> tags cannot be self-closing if empty.  The colorizer renders them correctly but
                    // when written out as XML, they get converted to self-closing tags which breaks them.  To fix
                    // them, store an empty string in each empty span so that it renders as an opening and closing
                    // tag.  Note that if null, InnerText returns an empty string by default.  As such, this looks
                    // redundant but it really isn't.
                    foreach(XmlNode span in placeholder.SelectNodes(".//span"))
                        if(span.InnerText.Length == 0)
                            span.InnerText = String.Empty;
                }
                else
                    this.WriteMessage(ac.Key, MessageLevel.Warn, "Unable to locate colorized code for placeholder: " +
                        placeholder.InnerText);
            }
        }
        #endregion
    }
}
