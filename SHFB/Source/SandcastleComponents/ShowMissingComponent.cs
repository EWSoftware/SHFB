//=============================================================================
// System  : Sandcastle Help File Builder Components
// File    : ShowMissingComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/18/2010
// Note    : Copyright 2007-2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a build component that is used to add "missing" notes
// for missing summary, parameter, returns, value, and remarks tags.  It can
// also add default summary documentation for constructors.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.4.0.0  02/16/2007  EFW  Created the code
// 1.6.0.5  02/25/2008  EFW  Fixed the auto-doc constructor class link
// 1.6.0.6  03/20/2008  EFW  Added auto-doc of constructors on list pages
// 1.6.0.7  03/23/2008  EFW  Added support for ShowMissingTypeParams and
//                           localized the messages.
// 1.8.0.1  01/16/2009  EFW  Added support for missing <include> target docs
// 1.8.0.3  11/19/2009  EFW  Added support for auto-documenting Dispose methods
//=============================================================================

using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This build component is used to add "missing" notes for missing
    /// summary, parameter, returns, value, and remarks tags.  It can also
    /// add default summary documentation for constructors.
    /// </summary>
    /// <example>
    /// <code lang="xml" title="Example configuration">
    /// &lt;!-- Show missing documentation component configuration.  This must
    ///      appear before the TransformComponent. --&gt;
    /// &lt;component type="SandcastleBuilder.Components.ShowMissingComponent"
    ///   assembly="C:\SandcastleBuilder\SandcastleBuilder.Components.dll"&gt;
    ///     &lt;!-- All elements are optional. --&gt;
    ///
    ///     &lt;!-- Auto-document constructors (true by default) --&gt;
    ///     &lt;AutoDocumentConstructors value="true" /&gt;
    ///
    ///     &lt;!-- Auto-document dispose methods (true by default) --&gt;
    ///     &lt;AutoDocumentDisposeMethods value="true" /&gt;
    ///
    ///     &lt;!-- Show missing param tags (true by default) --&gt;
    ///     &lt;ShowMissingParams value="true" /&gt;
    ///
    ///     &lt;!-- Show missing typeparam tags (true by default) --&gt;
    ///     &lt;ShowMissingTypeParams value="true" /&gt;
    ///
    ///     &lt;!-- Show missing remarks tags (false by default) --&gt;
    ///     &lt;ShowMissingRemarks value="false" /&gt;
    ///
    ///     &lt;!-- Show missing returns tags (true by default) --&gt;
    ///     &lt;ShowMissingReturns value="true" /&gt;
    ///
    ///     &lt;!-- Show missing summary tags (true by default) --&gt;
    ///     &lt;ShowMissingSummaries value="true" /&gt;
    ///
    ///     &lt;!-- Show missing value tags (false by default) --&gt;
    ///     &lt;ShowMissingValues value="false" /&gt;
    ///
    ///     &lt;!-- Show missing namespace comments (true by default) --&gt;
    ///     &lt;ShowMissingNamespaces value="true" /&gt;
    ///
    ///     &lt;!-- Show missing include target docs (false by default) --&gt;
    ///     &lt;ShowMissingIncludeTargets value="false" /&gt;
    ///
    ///     &lt;!-- Shared content file containing the localized
    ///          messages (optional) --&gt;
    ///     &lt;contentFile filename="C:\Working\SharedContent.xml" /&gt;
    /// &lt;/component&gt;
    /// </code>
    /// </example>
    public class ShowMissingComponent : BuildComponent
    {
        #region Private data members
        //=====================================================================

        private static Regex reStripWhitespace = new Regex(@"\s");

        private bool autoDocConstructors, autoDocDispose, showMissingParams,
            showMissingTypeParams, showMissingRemarks, showMissingReturns,
            showMissingSummaries, showMissingValues, showMissingNamespaces,
            showMissingIncludeTargets, isEnabled;

        // Auto-documented constructor and "missing" messages
        private string autoDocCtorMsg, autoDocStaticCtorMsg, autoDocDisposeMsg,
            autoDocDisposeBoolMsg, autoDocDisposeParamMsg, missingTagMsg,
            missingParamTagMsg, missingIncludeTargetMsg;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembler">A reference to the build assembler.</param>
        /// <param name="configuration">The configuration information</param>
        /// <remarks>See the <see cref="ShowMissingComponent"/> class topic
        /// for an example of the configuration</remarks>
        /// <exception cref="ConfigurationErrorsException">This is thrown if
        /// an error is detected in the configuration.</exception>
        public ShowMissingComponent(BuildAssembler assembler,
          XPathNavigator configuration) : base(assembler, configuration)
        {
            XPathDocument content;
            XPathNavigator nav, contentNav;
            string value = null;

            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            base.WriteMessage(MessageLevel.Info, String.Format(
                CultureInfo.InvariantCulture,
                "\r\n    [{0}, version {1}]\r\n    Show Missing " +
                "Documentation Component. {2}\r\n    http://SHFB.CodePlex.com",
                fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright));

            // All elements are optional.  If omitted, all properties are
            // true except for showMissingRemarks and showMissingValues;
            autoDocConstructors = autoDocDispose = showMissingParams =
                showMissingTypeParams = showMissingReturns = showMissingSummaries =
                showMissingNamespaces = true;

            nav = configuration.SelectSingleNode("AutoDocumentConstructors");
            if(nav != null)
            {
                value = nav.GetAttribute("value", String.Empty);

                if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value,
                  out autoDocConstructors))
                    throw new ConfigurationErrorsException("You must specify " +
                        "a Boolean value for the <AutoDocumentConstructors> " +
                        "'value' attribute.");
            }

            nav = configuration.SelectSingleNode("AutoDocumentDisposeMethods");
            if(nav != null)
            {
                value = nav.GetAttribute("value", String.Empty);

                if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value,
                  out autoDocDispose))
                    throw new ConfigurationErrorsException("You must specify " +
                        "a Boolean value for the <AutoDocumentDisposeMethods> " +
                        "'value' attribute.");
            }

            nav = configuration.SelectSingleNode("ShowMissingParams");
            if(nav != null)
            {
                value = nav.GetAttribute("value", String.Empty);

                if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value,
                  out showMissingParams))
                    throw new ConfigurationErrorsException("You must specify " +
                        "a Boolean value for the <ShowMissingParams> " +
                        "'value' attribute.");
            }

            nav = configuration.SelectSingleNode("ShowMissingTypeParams");
            if(nav != null)
            {
                value = nav.GetAttribute("value", String.Empty);

                if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value,
                  out showMissingTypeParams))
                    throw new ConfigurationErrorsException("You must specify " +
                        "a Boolean value for the <ShowMissingTypeParams> " +
                        "'value' attribute.");
            }

            nav = configuration.SelectSingleNode("ShowMissingRemarks");
            if(nav != null)
            {
                value = nav.GetAttribute("value", String.Empty);

                if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value,
                  out showMissingRemarks))
                    throw new ConfigurationErrorsException("You must specify " +
                        "a Boolean value for the <ShowMissingRemarks> " +
                        "'value' attribute.");
            }

            nav = configuration.SelectSingleNode("ShowMissingReturns");
            if(nav != null)
            {
                value = nav.GetAttribute("value", String.Empty);

                if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value,
                  out showMissingReturns))
                    throw new ConfigurationErrorsException("You must specify " +
                        "a Boolean value for the <ShowMissingReturns> " +
                        "'value' attribute.");
            }

            nav = configuration.SelectSingleNode("ShowMissingSummaries");
            if(nav != null)
            {
                value = nav.GetAttribute("value", String.Empty);

                if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value,
                  out showMissingSummaries))
                    throw new ConfigurationErrorsException("You must specify " +
                        "a Boolean value for the <ShowMissingSummaries> " +
                        "'value' attribute.");
            }

            nav = configuration.SelectSingleNode("ShowMissingValues");
            if(nav != null)
            {
                value = nav.GetAttribute("value", String.Empty);

                if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value,
                  out showMissingValues))
                    throw new ConfigurationErrorsException("You must specify " +
                        "a Boolean value for the <ShowMissingValues> " +
                        "'value' attribute.");
            }

            nav = configuration.SelectSingleNode("ShowMissingNamespaces");
            if(nav != null)
            {
                value = nav.GetAttribute("value", String.Empty);

                if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value,
                  out showMissingNamespaces))
                    throw new ConfigurationErrorsException("You must specify " +
                        "a Boolean value for the <ShowMissingNamespaces> " +
                        "'value' attribute.");
            }

            nav = configuration.SelectSingleNode("ShowMissingIncludeTargets");
            if(nav != null)
            {
                value = nav.GetAttribute("value", String.Empty);

                if(!String.IsNullOrEmpty(value) && !Boolean.TryParse(value,
                  out showMissingIncludeTargets))
                    throw new ConfigurationErrorsException("You must specify " +
                        "a Boolean value for the <ShowMissingIncludeTargets> " +
                        "'value' attribute.");
            }

            autoDocCtorMsg = "Initializes a new instance of the <see " +
                "cref=\"T:{0}\"/> class";
            autoDocStaticCtorMsg = "Initializes the static fields of the " +
                "<see cref=\"T:{0}\"/> class";

            autoDocDisposeMsg = "Releases all resources used by the <see " +
                "cref=\"T:{0}\"/>";
            autoDocDisposeBoolMsg = "Releases the unmanaged resources used by " +
                "the <see cref=\"T:{0}\"/> and optionally releases the " +
                "managed resources";
            autoDocDisposeParamMsg = "True to release both managed and " +
                "unmanaged resources; false to release only unmanaged resources";

            missingTagMsg = "<p style=\"color: #dc143c; font-size: 8.5pt; " +
                        "font-weight: bold;\">[Missing &lt;{0}&gt; " +
                        "documentation for {1}]</p>";
            missingParamTagMsg = "<p style=\"color: #dc143c; font-size: 8.5pt; " +
                    "font-weight: bold;\">[Missing &lt;{0} name=\"{1}\"/&gt; " +
                    "documentation for \"{2}\"]</p>";
            missingIncludeTargetMsg = "<p style=\"color: #dc143c; font-size: 8.5pt; " +
                    "font-weight: bold;\">[Missing &lt;include&gt; target " +
                    "documentation in '{0}'.  File: '{1}' Path: '{2}']</p>";

            nav = configuration.SelectSingleNode("contentFile");
            if(nav != null)
            {
                value = nav.GetAttribute("filename", String.Empty);

                if(String.IsNullOrEmpty(value) || !File.Exists(value))
                    throw new ConfigurationErrorsException("You must specify " +
                        "a filename value for the <contentFile> 'filename' " +
                        "attribute and it must exist.");

                content = new XPathDocument(value);
                contentNav = content.CreateNavigator();

                nav = contentNav.SelectSingleNode(
                    "content/item[@id='shfbAutoDocConstructor']");
                if(nav != null)
                    autoDocCtorMsg = nav.Value;

                nav = contentNav.SelectSingleNode(
                    "content/item[@id='shfbAutoDocStaticConstructor']");
                if(nav != null)
                    autoDocStaticCtorMsg = nav.Value;

                nav = contentNav.SelectSingleNode(
                    "content/item[@id='shfbAutoDocDispose']");
                if(nav != null)
                    autoDocDisposeMsg = nav.Value;

                nav = contentNav.SelectSingleNode(
                    "content/item[@id='shfbAutoDocDisposeBool']");
                if(nav != null)
                    autoDocDisposeBoolMsg = nav.Value;

                nav = contentNav.SelectSingleNode(
                    "content/item[@id='shfbAutoDocDisposeParam']");
                if(nav != null)
                    autoDocDisposeParamMsg = nav.Value;

                nav = contentNav.SelectSingleNode(
                    "content/item[@id='shfbMissingTag']");
                if(nav != null)
                    missingTagMsg = nav.Value;

                nav = contentNav.SelectSingleNode(
                    "content/item[@id='shfbMissingParamTag']");
                if(nav != null)
                    missingParamTagMsg = nav.Value;

                nav = contentNav.SelectSingleNode(
                    "content/item[@id='shfbMissingIncludeTarget']");
                if(nav != null)
                    missingIncludeTargetMsg = nav.Value;
            }

            isEnabled = (autoDocConstructors || autoDocDispose ||
                showMissingParams || showMissingTypeParams ||
                showMissingRemarks || showMissingReturns ||
                showMissingSummaries || showMissingValues ||
                showMissingNamespaces || showMissingIncludeTargets);

            if(!isEnabled)
                base.WriteMessage(MessageLevel.Info, "  All Show Missing " +
                    "options are disabled.  The component will do nothing.");
        }
        #endregion

        #region Apply the component
        //=====================================================================

        /// <summary>
        /// This is implemented to add the missing documentation tags
        /// </summary>
        /// <param name="document">The XML document with which to work.</param>
        /// <param name="key">The key (member name) of the item being
        /// documented.</param>
        public override void Apply(XmlDocument document, string key)
        {
            XmlNodeList items;
            XmlNode comments, returnsNode;
            string apiKey;

            // Auto-document the constructor(s) on the type's list pages if necessary
            if(isEnabled && autoDocConstructors && (key[0] == 'T' ||
              key.StartsWith("AllMembers", StringComparison.Ordinal) ||
              (key.StartsWith("Overload", StringComparison.Ordinal) &&
              (key.IndexOf(".ctor", StringComparison.Ordinal) != -1 ||
              key.IndexOf(".#ctor", StringComparison.Ordinal) != -1))))
            {
                apiKey = "M:" + key.Substring(key.IndexOf(':') + 1);

                if(key.IndexOf(".ctor", StringComparison.Ordinal) == -1 &&
                  key.IndexOf(".#ctor", StringComparison.Ordinal) == -1)
                    apiKey += ".#ctor";
                else
                    apiKey = apiKey.Replace("..ctor", ".#ctor");

                foreach(XmlNode element in document.SelectNodes(
                  "document/reference/elements//element[starts-with(@api, '" + apiKey + "')]"))
                    this.CheckForMissingText(element, apiKey, "summary");
            }

            // Auto-document the Dispose method(s) on the type's list pages if necessary
            if(isEnabled && autoDocDispose && (key[0] == 'T' ||
              key.StartsWith("AllMembers", StringComparison.Ordinal) ||
              (key.StartsWith("Overload", StringComparison.Ordinal) &&
              key.EndsWith(".Dispose", StringComparison.Ordinal))))
            {
                apiKey = "M:" + key.Substring(key.IndexOf(':') + 1);

                if(!key.EndsWith(".Dispose", StringComparison.Ordinal))
                    apiKey += ".Dispose";

                // Handle IDisposable.Dispose()
                foreach(XmlNode element in document.SelectNodes(
                  "document/reference/elements//element[@api = '" + apiKey + "']"))
                    this.CheckForMissingText(element, apiKey, "summary");

                // Handle the Boolean overload if present
                apiKey += "(System.Boolean)";

                foreach(XmlNode element in document.SelectNodes(
                  "document/reference/elements//element[@api = '" + apiKey + "']"))
                    this.CheckForMissingText(element, apiKey, "summary");
            }

            // Don't bother if there is nothing to add
            if(!isEnabled || key[0] == 'R' || key[1] != ':' || (key[0] == 'N' && !showMissingNamespaces))
                return;

            try
            {
                // Add missing tags based on the type of item it represents
                comments = document.SelectSingleNode("document/comments");

                // All elements can have a summary
                if(showMissingSummaries || (autoDocConstructors &&
                  (key.Contains("#ctor") || key.Contains("#cctor"))) ||
                  (autoDocDispose && (key.EndsWith(".Dispose", StringComparison.Ordinal) ||
                  key.EndsWith(".Dispose(System.Boolean)", StringComparison.Ordinal))))
                    this.CheckForMissingText(comments, key, "summary");

                // All elements can have an include.  We check for this after
                // summary since the "missing" message is appended to the
                // summary and we don't want it to count as the summary.
                if(showMissingIncludeTargets)
                    this.CheckForMissingIncludeTarget(comments, key);

                // All elements can have remarks except namespaces
                if(showMissingRemarks && key[0] != 'N')
                    this.CheckForMissingText(comments, key, "remarks");

                // If it's a property, check for a missing <value> tag
                if(key[0] == 'P' && showMissingValues)
                    this.CheckForMissingText(comments, key, "value");
                else
                {
                    if(showMissingTypeParams && (key[0] == 'T' ||
                      key[0] == 'M'))
                    {
                        items = document.SelectNodes(
                            "document/reference/templates/template");

                        foreach(XmlNode p in items)
                            this.CheckForMissingParameter(comments, key,
                                p.Attributes["name"].Value, "typeparam");
                    }

                    if(key[0] == 'M')
                    {
                        // If it's a member, check for missing <returns>
                        // and <param> tags.
                        if(showMissingReturns)
                        {
                            returnsNode = document.SelectSingleNode("document/reference/returns");

                            if(returnsNode != null)
                                this.CheckForMissingText(comments, key, "returns");
                        }

                        if(showMissingParams || (autoDocDispose &&
                          key.EndsWith(".Dispose(System.Boolean)", StringComparison.Ordinal)))
                        {
                            items = document.SelectNodes("document/reference/parameters/parameter");

                            foreach(XmlNode p in items)
                                this.CheckForMissingParameter(comments, key,
                                    p.Attributes["name"].Value, "param");
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                base.WriteMessage(MessageLevel.Error, "Error adding " +
                    "missing documentation tags: " + ex.Message);
            }
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Check for missing text in the specified documentation tag and, if
        /// it doesn't exist or the text is blank, add a "missing" message as
        /// the documentation tag's text.
        /// </summary>
        /// <param name="comments">The comments node to check.</param>
        /// <param name="key">The key (name) for the current item</param>
        /// <param name="tagName">The tag type for which to check.</param>
        private void CheckForMissingText(XmlNode comments, string key, string tagName)
        {
            string text;
            XmlNode tag = comments.SelectSingleNode(tagName);

            if(tag == null)
            {
                tag = comments.OwnerDocument.CreateNode(XmlNodeType.Element,
                    tagName, null);
                comments.AppendChild(tag);
                text = String.Empty;
            }
            else
                text = reStripWhitespace.Replace(tag.InnerText, String.Empty);

            if(text.Length == 0)
            {
                // Auto document constructor?
                if(tagName == "summary" && autoDocConstructors &&
                  (key.Contains("#ctor") || key.Contains("#cctor")))
                {
                    this.WriteMessage(MessageLevel.Info, "Auto-documenting " +
                        "constructor " + key);

                    if(key.Contains("#cctor"))
                        tag.InnerXml = String.Format(
                            CultureInfo.InvariantCulture, autoDocStaticCtorMsg,
                            HttpUtility.HtmlEncode(key.Substring(2,
                            key.IndexOf(".#cctor", StringComparison.Ordinal) - 2)));
                    else
                        tag.InnerXml = String.Format(
                            CultureInfo.InvariantCulture, autoDocCtorMsg,
                            HttpUtility.HtmlEncode(key.Substring(2,
                            key.IndexOf(".#ctor", StringComparison.Ordinal) - 2)));

                    return;
                }

                // Auto document Dispose method?
                if(tagName == "summary" && (autoDocDispose &&
                  key.EndsWith(".Dispose", StringComparison.Ordinal) ||
                  key.EndsWith(".Dispose(System.Boolean)", StringComparison.Ordinal)))
                {
                    this.WriteMessage(MessageLevel.Info, "Auto-documenting " +
                        "dispose method " + key);

                    if(key.EndsWith(".Dispose", StringComparison.Ordinal))
                        tag.InnerXml = String.Format(
                            CultureInfo.InvariantCulture, autoDocDisposeMsg,
                            HttpUtility.HtmlEncode(key.Substring(2, key.Length - 10)));
                    else
                        tag.InnerXml = String.Format(
                            CultureInfo.InvariantCulture, autoDocDisposeBoolMsg,
                            HttpUtility.HtmlEncode(key.Substring(2, key.Length - 26)));

                    return;
                }

                this.WriteMessage(MessageLevel.Warn, String.Format(
                    CultureInfo.InvariantCulture,
                    "Missing <{0}> documentation for {1}", tagName, key));

                tag.InnerXml = String.Format(CultureInfo.InvariantCulture,
                    missingTagMsg, tagName, HttpUtility.HtmlEncode(key));
            }
        }

        /// <summary>
        /// Check for missing text in the specified &lt;param&gt; or
        /// &lt;typeparam&gt; tag and, if it doesn't exist or the text is
        /// blank, add a "missing" message as the tag's text.
        /// </summary>
        /// <param name="comments">The comments node to check.</param>
        /// <param name="key">The key (name) for the current item</param>
        /// <param name="paramName">The parameter name for which to check.</param>
        /// <param name="tagName">The tag type for which to check.</param>
        private void CheckForMissingParameter(XmlNode comments, string key,
          string paramName, string tagName)
        {
            string text;
            XmlAttribute name;
            XmlNode tag = comments.SelectSingleNode(tagName + "[@name='" +
                paramName + "']");

            if(tag == null)
            {
                tag = comments.OwnerDocument.CreateNode(XmlNodeType.Element,
                    tagName, null);

                name = comments.OwnerDocument.CreateAttribute("name");
                name.Value = paramName;
                tag.Attributes.Append(name);

                comments.AppendChild(tag);
                text = String.Empty;
            }
            else
                text = reStripWhitespace.Replace(tag.InnerText, String.Empty);

            if(text.Length == 0)
            {
                // Auto document Dispose(Bool) parameter?
                if(autoDocDispose && key.EndsWith(".Dispose(System.Boolean)", StringComparison.Ordinal))
                {
                    this.WriteMessage(MessageLevel.Info, "Auto-documenting " +
                        "dispose method parameter for " + key);

                    tag.InnerXml = String.Format(CultureInfo.InvariantCulture,
                        autoDocDisposeParamMsg, HttpUtility.HtmlEncode(
                        key.Substring(2, key.Length - 26)));

                    return;
                }

                this.WriteMessage(MessageLevel.Warn, String.Format(
                    CultureInfo.InvariantCulture,
                    "Missing <{0} name=\"{1}\"/> documentation for {2}",
                    tagName, paramName, key));

                tag.InnerXml = String.Format(CultureInfo.InvariantCulture,
                    missingParamTagMsg, tagName,
                    HttpUtility.HtmlEncode(paramName),
                    HttpUtility.HtmlEncode(key));
            }
        }

        /// <summary>
        /// Check for bad <c>include</c> elements and, if any are found, add a
        /// "missing" message to the summary tag's text.
        /// </summary>
        /// <param name="comments">The comments node to check.</param>
        /// <param name="key">The key (name) for the current item</param>
        private void CheckForMissingIncludeTarget(XmlNode comments, string key)
        {
            XmlNodeList includes = comments.SelectNodes("include");
            XmlNode tag;

            if(includes.Count != 0)
            {
                tag = comments.SelectSingleNode("summary");

                if(tag == null)
                {
                    tag = comments.OwnerDocument.CreateNode(XmlNodeType.Element,
                        "summary", null);
                    comments.AppendChild(tag);
                    tag.InnerXml = String.Empty;
                }

                foreach(XmlNode include in includes)
                {
                    this.WriteMessage(MessageLevel.Warn, String.Format(
                        CultureInfo.InvariantCulture, missingIncludeTargetMsg,
                        key, include.Attributes["file"].Value,
                        include.Attributes["path"].Value));

                    tag.InnerXml += String.Format(CultureInfo.InvariantCulture,
                        missingIncludeTargetMsg, HttpUtility.HtmlEncode(key),
                        HttpUtility.HtmlEncode(include.Attributes["file"].Value),
                        HttpUtility.HtmlEncode(include.Attributes["path"].Value));
                }
            }
        }
        #endregion
    }
}
