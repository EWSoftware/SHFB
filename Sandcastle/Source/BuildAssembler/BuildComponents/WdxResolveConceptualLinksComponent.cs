// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Text.RegularExpressions;
using System.Web;


namespace Microsoft.Ddue.Tools {

    /// <summary>
    /// WdxResolveConceptualLinksComponent handles conceptual links where the target is a GUID. All other kinds
    /// of targets are considered invalid. NOTE: This is an experimental version for WebDocs.
    /// </summary>
	public class WdxResolveConceptualLinksComponent : BuildComponent {

        const int MaxTargetCacheSize = 1000;

        TargetSetList targetSets = new TargetSetList();
        XPathExpression baseUrl;
        string invalidLinkFormat = "<span class='nolink'>{1}</span>";
        string brokenLinkFormat = "<a href='http://msdn2.microsoft.com/en-us/library/{0}'>{1}</a>";
        string defaultFormat = "<a href='{0}'>{1}</a>";

		public WdxResolveConceptualLinksComponent (BuildAssembler assembler, XPathNavigator configuration) : base(assembler, configuration) {
            //System.Diagnostics.Debugger.Break();

            string av; // temporary attribute values

            // base-url is an xpath expression that is used to lookup the url that relative links need to be
            // relative to. The lookup is done against the current document. This attribute is needed only if 
            // one of the targets uses relative links that are not in the current directory. If not specified,
            // the target uses the url from the meta data unchanged.
            av = configuration.GetAttribute("base-url", String.Empty);
            if (!String.IsNullOrEmpty(av))
                baseUrl = CompileXPathExpression(av);

            // invalid-link-format specifies a string format to be used for invalid (target is not a valid GUID)
            // links. The string formatter is called with parameter {0} set to the target attribute of the link,
            // and parameter {1} set to the tag content from the source document. A reasonable default is used
            // if the value is not specified.
            av = configuration.GetAttribute("invalid-link-format", String.Empty);
            if (!String.IsNullOrEmpty(av))
                invalidLinkFormat = av;

            // broken-link-format specifies a string format to be used for broken links (target GUID lookup
            // failed in all targets). The string formatter is called with parameter {0} set to the target attribute
            // of the link, and parameter {1} set to the tag content from the source document. A reasonable
            // default is used if the value is not specified.
            av = configuration.GetAttribute("broken-link-format", String.Empty);
            if (!String.IsNullOrEmpty(av))
                brokenLinkFormat = av;

            // <targets> specifies a lookup solution for each possible set of link targets. Each target must
            // specify either a lookup file or error condition (invalid-link, broken-link).
            XPathNodeIterator targetsNodes = configuration.Select("targets");
			foreach (XPathNavigator targetsNode in targetsNodes) {

                // lookup-file specifies the meta data file used for looking up URLs and titles. The value will
                // go through environment variable expansion during setup and then through string formatting after
                // computing the url, with parameter {0} set to the link target GUID. This attribute is required.
                string lookupFile = targetsNode.GetAttribute("lookup-file", String.Empty);
                if (string.IsNullOrEmpty(lookupFile))
                    WriteMessage(MessageLevel.Error, "Each target must have a lookup-file attribute.");
                else
                    lookupFile = Environment.ExpandEnvironmentVariables(lookupFile);
                
                // check-file-exists if specified ensures that the link target file exists; if it doesn't exist we
                // take the broken link action.
                string checkFileExists = targetsNode.GetAttribute("check-file-exists", String.Empty);
                if (!String.IsNullOrEmpty(checkFileExists))
                    checkFileExists = Environment.ExpandEnvironmentVariables(checkFileExists);

                // url is an xpath expression that is used to lookup the link url in the meta data file. The default
                // value can be used to lookup the url in .cmp.xml files.
                av = targetsNode.GetAttribute("url", String.Empty);
                XPathExpression url = String.IsNullOrEmpty(av) ? 
                    XPathExpression.Compile("concat(/metadata/topic/@id,'.htm')") :
                    XPathExpression.Compile(av);

                // text is an xpath expression that is used to lookup the link text in the meta data file. The default
                // value can be used to lookup the link text in .cmp.xml files.
                av = targetsNode.GetAttribute("text", string.Empty);
                XPathExpression text = String.IsNullOrEmpty(av) ?
                    XPathExpression.Compile("string(/metadata/topic/title)") :
                    XPathExpression.Compile(av);

                // relative-url determines whether the links from this target set are relative to the current page
                // and need to be adjusted to the base directory.
                av = targetsNode.GetAttribute("relative-url", String.Empty);
                bool relativeUrl = String.IsNullOrEmpty(av) ? false : Convert.ToBoolean(av);;

                // format is a format string that is used to generate the link. Parameter {0} is the url;
                // parameter {1} is the text. The default creates a standard HTML link.
                string format = targetsNode.GetAttribute("format", String.Empty);
                if (String.IsNullOrEmpty(format))
                    format = defaultFormat;
                
                // target looks OK
                targetSets.Add(new TargetSet(lookupFile, checkFileExists, url, text, relativeUrl, format));
            }

            WriteMessage(MessageLevel.Info, String.Format("Collected {0} targets directories.", targetSets.Count));	
		}

        public override void Apply(XmlDocument document, string key) {
            // Run through all conceptual nodes, make sure the target is a valid GUID, attempt to resolve the target
            // to a link using one of the TargetSet definitions, and then replace the node with the result. Errors
            // will be dealt with as follows: 1) bad target (GUID) -> output invalidLinkFormat; 2) broken link (cannot
            // resolve target or the URL is empty) -> output brokenLinkFormat; 3) missing text -> just delete the node
            // and don't output anything (presumably there's no point in creating a link that nobody can see). In all
            // three cases we'll log the problem as a warning.
            string docBaseUrl = baseUrl == null ? null : BuildComponentUtilities.EvalXPathExpr(document, baseUrl, "key", key);
            XPathNavigator[] linkNodes = BuildComponentUtilities.ConvertNodeIteratorToArray(document.CreateNavigator().Select(conceptualLinks));
            foreach (XPathNavigator node in linkNodes) {
                string targetGuid = node.GetAttribute("target", String.Empty);
                string url = targetGuid;
                string text = node.ToString();
                string format;
                if (validGuid.IsMatch(url)) {
                    format = brokenLinkFormat;
                    Target t = targetSets.Lookup(targetGuid);
                    if (t == null) {
                        WriteMessage(MessageLevel.Warn, String.Format("Conceptual link not found in target sets; target={0}", targetGuid));
                    }
                    else {
                        if (!String.IsNullOrEmpty(t.Url)) {
                            format = t.TargetSet.Format;
                            url = (docBaseUrl != null && t.TargetSet.RelativeUrl) ? 
                                BuildComponentUtilities.GetRelativePath(t.Url, docBaseUrl) : t.Url;
                            if (!String.IsNullOrEmpty(t.Text))
                                text = t.Text;
                        }
                        else
                            WriteMessage(MessageLevel.Warn, String.Format("Conceptual link found in target set, but meta data does not specify a url; target={0}", targetGuid));
                    }
                }
                else
                    format = invalidLinkFormat;

                if (String.IsNullOrEmpty(text)) {
                    node.DeleteSelf();
                    WriteMessage(MessageLevel.Warn, String.Format("Skipping conceptual link without text; target={0}", url));
                }
                else {
                    node.OuterXml = String.Format(format, url, text);
                }
            }
		}

        private static XPathExpression conceptualLinks = XPathExpression.Compile("//conceptualLink");
        private static Regex validGuid = new Regex(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", RegexOptions.Compiled);


        #region HelperFunctions

        public XPathExpression CompileXPathExpression(string xpath) {
            XPathExpression expression = null;
            try {
                expression = XPathExpression.Compile(xpath);
            }
            catch (ArgumentException e) {
                WriteMessage(MessageLevel.Error, String.Format("'{0}' is not a valid XPath expression. The error message is: {1}", xpath, e.Message));
            }
            catch (XPathException e) {
                WriteMessage(MessageLevel.Error, String.Format("'{0}' is not a valid XPath expression. The error message is: {1}", xpath, e.Message));
            }
            return (expression);
        }

        #endregion HelperFunctions


        #region DataStructures

        //
        //  Internal data structures to support WdxResolveConceptualLinksComponent
        //

        class TargetSet {
            string lookupFile;
            string checkFileExists;
            XPathExpression url;
            XPathExpression text;

            bool relativeUrl;
            public bool RelativeUrl {
                get { return relativeUrl; }
            }

            string format;
            public string Format {
                get { return format; }
            }

            public TargetSet(string lookupFile, string checkFileExists, XPathExpression url, XPathExpression text, bool relativeUrl, string format) {
                if (lookupFile == null)
                    throw new ArgumentNullException("lookupFile");
                this.lookupFile = lookupFile;

                this.checkFileExists = checkFileExists;

                if (url == null)
                    throw new ArgumentNullException("url");
                this.url = url;

                if (text == null)
                    throw new ArgumentNullException("text");
                this.text = text;

                this.relativeUrl = relativeUrl;

                if (format == null)
                    throw new ArgumentNullException("format");
                this.format = format;
            }

            public Target Lookup(string targetGuid) {
                string lookupFilePathName = String.Format(lookupFile, targetGuid);
                if (File.Exists(lookupFilePathName) &&
                    (checkFileExists == null || File.Exists(String.Format(checkFileExists, targetGuid)))) {
                    XPathDocument document = new XPathDocument(lookupFilePathName);
                    return new Target(this, 
                        (string)document.CreateNavigator().Evaluate(url), 
                        (string)document.CreateNavigator().Evaluate(text));
                }
                return null;
            }
        }

        class TargetSetList {
            List<TargetSet> targetSets = new List<TargetSet>();
            Dictionary<string, Target> targetCache = new Dictionary<string, Target>();

            public TargetSetList() {
            }

            public void Add(TargetSet targetSet) {
                if (targetSet == null)
                    throw new ArgumentNullException("targetSet");
                this.targetSets.Add(targetSet);
            }

            public int Count {
                get { return targetSets.Count; }
            }

            public Target Lookup(string targetGuid) {
                Target t = null;
                if (!targetCache.TryGetValue(targetGuid, out t)) {
                    foreach (TargetSet ts in targetSets) {
                        t = ts.Lookup(targetGuid);
                        if (t != null) {
                            if (targetCache.Count >= MaxTargetCacheSize)
                                targetCache.Clear();
                            targetCache.Add(targetGuid, t);
                            break;
                        }
                    }
                }
                return t;
            }
        }

        class Target {
            TargetSet targetSet;
            public TargetSet TargetSet {
                get { return targetSet; }
            }

            string url;
            public string Url {
                get { return url; }
            }

            string text;
            public string Text {
                get { return text; }
            }

            public Target(TargetSet targetSet, string url, string text) {
                if (targetSet == null)
                    throw new ArgumentNullException("targetSet");
                this.targetSet = targetSet;
                this.url = url;
                this.text = text;
            }
        }

       #endregion DataStructures
    }
}