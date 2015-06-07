//===============================================================================================================
// System  : Sandcastle Build Components
// File    : ResolveConceptualLinksComponent.cs
// Note    : Copyright 2010-2015 Microsoft Corporation
//
// This file contains a modified version of the original ResolveConceptualLinksComponent that allows the use of
// inner text from the <link> tag and also allows the use of anchor references (#anchorName) in the link target.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Change History
// 02/16/2012 - EFW - Merged my changes into the code
// 12/26/2012 - EFW - Minor updates to processing.  As with the SharedContentComponent, this one doesn't load
// enough info to warrant trying to share the common data across all instances.
// 10/03/2013 - EFW - Applied patch from gfraiteur to remove the GUID topic ID requirement.  Bear in mind that
// GUIDs are still preferred as they are guaranteed to be unique which is important for Help 2 and MS Help
// Viewer content.  Duplicate IDs across multiple sets of content would cause linking issues in the collections.
// 12/24/2013 - EFW - Updated the build component to be discoverable via MEF
// 06/05/2015 - EFW - Removed support for the Help 2 Index link type
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools.Targets;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This is a modified version of the original <c>ResolveConceptualLinksComponent</c> that is used to resolve
    /// links to conceptual topics.
    /// </summary>
    /// <remarks>This version contains the following improvements and fixes:
    /// <list type="bullet">
    ///   <item>Broken links use the <b>None</b> style rather than the <c>Index</c> style so that it is apparent
    /// that they do not work.</item>
    ///   <item>The inner text from the conceptual link is used if specified.</item>
    ///   <item>On broken links, when the <c>showBrokenLinkText</c> option is true and there is no inner text,
    /// the target value is displayed.</item>
    ///   <item>Conceptual link targets can include an optional anchor name from within the target such as
    /// "#Name" (see examples below).</item>
    ///   <item>Unnecessary whitespace is removed from the link text.</item>
    ///   <item>If the companion file contains a <c>&lt;linkText&gt;</c> element and no inner text is specified,
    /// its value will be used for the link text rather than the title.  This allows for a shorter title or
    /// description to use as the default link text.</item>
    /// </list></remarks>
    /// <example>
    /// On links without inner text, if the companion file contains a <c>linkText</c> element, that text will be
    /// used.  If not, the title is used.
    ///
    /// <code lang="xml" title="Example Links">
    /// <![CDATA[<!-- Link with inner text -->
    /// <link xlink:href="3ab3113f-984b-19ac-7812-990192aca5b0">Click Here</link>
    /// <!-- Link with anchor reference -->
    /// <link xlink:href="3ab3113f-984b-19ac-7812-990192aca5b1#SubTopic" />
    /// <!-- Link with inner text and an anchor reference -->
    /// <link xlink:href="3ab3113f-984b-19ac-7812-990192aca5b1#PropA">PropertyA</link>]]>
    /// </code>
    /// 
    /// <code lang="xml" title="Example configuration">
    /// &lt;!-- Resolve conceptual links --&gt;
    /// &lt;component id="Resolve Conceptual Links Component">
    ///     &lt;showBrokenLinkText value="true" /&gt;
    ///     &lt;targets base="xmlComp" type="local" /&gt;
    /// &lt;/component&gt;
    /// </code>
    /// </example>
    public class ResolveConceptualLinksComponent : BuildComponentCore
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("Resolve Conceptual Links Component")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new ResolveConceptualLinksComponent(base.BuildAssembler);
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private Dictionary<string, TargetInfo> cache;
        private TargetDirectoryCollection targetDirectories;
        private bool showBrokenLinkText;

        private static XPathExpression conceptualLinks = XPathExpression.Compile("//conceptualLink");

        private const int CacheSize = 1000;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected ResolveConceptualLinksComponent(BuildAssemblerCore buildAssembler) : base(buildAssembler)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            TargetDirectory targetDirectory;
            XPathExpression urlExp, textExp, linkTextExp;
            ConceptualLinkType linkType = ConceptualLinkType.None;
            string attribute, basePath;

            targetDirectories = new TargetDirectoryCollection();

            // This is a simple cache.  If the cache size limit is reached, it clears the cache and starts over
            cache = new Dictionary<string, TargetInfo>(CacheSize);

            attribute = (string)configuration.Evaluate("string(showBrokenLinkText/@value)");

            if(!String.IsNullOrWhiteSpace(attribute))
                showBrokenLinkText = Convert.ToBoolean(attribute, CultureInfo.InvariantCulture);

            foreach(XPathNavigator navigator in configuration.Select("targets"))
            {
                basePath = navigator.GetAttribute("base", String.Empty);

                if(String.IsNullOrEmpty(basePath))
                    base.WriteMessage(MessageLevel.Error, "Every targets element must have a base attribute " +
                        "that specifies the path to a directory of target metadata files.");

                basePath = Environment.ExpandEnvironmentVariables(basePath);

                if(!Directory.Exists(basePath))
                    base.WriteMessage(MessageLevel.Error, "The specified target metadata directory '{0}' " +
                        "does not exist.", basePath);

                attribute = navigator.GetAttribute("url", String.Empty);

                if(String.IsNullOrEmpty(attribute))
                    urlExp = XPathExpression.Compile("concat(/metadata/topic/@id,'.htm')");
                else
                    urlExp = this.CompileXPathExpression(attribute);

                attribute = navigator.GetAttribute("text", String.Empty);

                if(String.IsNullOrEmpty(attribute))
                    textExp = XPathExpression.Compile("string(/metadata/topic/title)");
                else
                    textExp = this.CompileXPathExpression(attribute);

                // EFW - Added support for linkText option
                attribute = navigator.GetAttribute("linkText", String.Empty);

                if(String.IsNullOrEmpty(attribute))
                    linkTextExp = XPathExpression.Compile("string(/metadata/topic/linkText)");
                else
                    linkTextExp = this.CompileXPathExpression(attribute);

                attribute = navigator.GetAttribute("type", String.Empty);

                if(String.IsNullOrEmpty(attribute))
                    base.WriteMessage(MessageLevel.Error, "Every targets element must have a type attribute " +
                        "that specifies what kind of link to create to targets found in that directory.");

                if(!Enum.TryParse<ConceptualLinkType>(attribute, true, out linkType))
                    base.WriteMessage(MessageLevel.Error, "'{0}' is not a valid link type.", attribute);

                targetDirectory = new TargetDirectory(basePath, urlExp, textExp, linkTextExp, linkType);
                targetDirectories.Add(targetDirectory);
            }

            base.WriteMessage(MessageLevel.Info, "Collected {0} targets directories.", targetDirectories.Count);
        }

        /// <summary>
        /// This is implemented to resolve the conceptual links
        /// </summary>
        /// <param name="document">The XML document with which to work.</param>
        /// <param name="key">The key (member name) of the item being documented.</param>
        public override void Apply(XmlDocument document, string key)
        {
            ConceptualLinkInfo info;
            TargetInfo targetInfo;
            ConceptualLinkType linkType;
            string url, text;

            foreach(XPathNavigator navigator in document.CreateNavigator().Select(conceptualLinks).ToArray())
            {
                info = new ConceptualLinkInfo(navigator);
                url = text = null;
                linkType = ConceptualLinkType.None;

                targetInfo = this.GetTargetInfoFromCache(info.Target);

                if(targetInfo == null)
                {
                    // EFW - Removed linkType = Index, broken links should use the None style.
                    text = this.BrokenLinkDisplayText(info.Target, info.Text);
                    base.WriteMessage(key, MessageLevel.Warn, "Unknown conceptual link target '{0}'.", info.Target);
                }
                else
                {
                    url = targetInfo.Url;

                    // EFW - Append the anchor if one was specified
                    if(!String.IsNullOrEmpty(info.Anchor))
                        url += info.Anchor;

                    // EFW - Use the link text if specified
                    if(!String.IsNullOrEmpty(info.Text))
                        text = info.Text;
                    else
                        text = targetInfo.Text;

                    linkType = targetInfo.LinkType;
                }

                XmlWriter writer = navigator.InsertAfter();

                switch(linkType)
                {
                    case ConceptualLinkType.None:
                        writer.WriteStartElement("span");
                        writer.WriteAttributeString("class", "nolink");
                        break;

                    case ConceptualLinkType.Local:
                        writer.WriteStartElement("a");
                        writer.WriteAttributeString("href", url);
                        break;

                    case ConceptualLinkType.Id:
                        writer.WriteStartElement("a");
                        writer.WriteAttributeString("href", "ms-xhelp:///?Id=" + info.Target);
                        break;
                }

                writer.WriteString(text);
                writer.WriteEndElement();
                writer.Close();

                navigator.DeleteSelf();
            }
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Determine what to display for broken links
        /// </summary>
        /// <param name="target">The target key</param>
        /// <param name="text">The link text</param>
        /// <returns>The text to display for the broken link</returns>
        private string BrokenLinkDisplayText(string target, string text)
        {
            // EFW - If true but text is empty, use the target
            if(showBrokenLinkText && !String.IsNullOrEmpty(text))
                return text;

            return String.Concat("[", target, "]");
        }

        /// <summary>
        /// Compile an XPath expression and report an error if it fails
        /// </summary>
        /// <param name="xpath">The XPath expression to compile.</param>
        /// <returns>The compiled XPath expression.</returns>
        private XPathExpression CompileXPathExpression(string xpath)
        {
            XPathExpression expression = null;

            try
            {
                expression = XPathExpression.Compile(xpath);
            }
            catch(ArgumentException argEx)
            {
                base.WriteMessage(MessageLevel.Error, "'{0}' is not a valid XPath expression. The error " +
                    "message is: {1}", xpath, argEx.Message);
            }
            catch(XPathException xpathEx)
            {
                base.WriteMessage(MessageLevel.Error, "'{0}' is not a valid XPath expression. The error " +
                    "message is: {1}", xpath, xpathEx.Message);
            }

            return expression;
        }

        /// <summary>
        /// Get target info
        /// </summary>
        /// <param name="target">The target for which to get info</param>
        /// <returns>The target info object if found or null if not found</returns>
        private TargetInfo GetTargetInfoFromCache(string target)
        {
            TargetInfo targetInfo;

            if(!cache.TryGetValue(target, out targetInfo))
            {
                targetInfo = targetDirectories[target];

                if(cache.Count >= CacheSize)
                    cache.Clear();

                cache.Add(target, targetInfo);
            }

            return targetInfo;
        }
        #endregion
    }
}
