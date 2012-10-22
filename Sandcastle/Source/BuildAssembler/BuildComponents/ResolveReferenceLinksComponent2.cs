// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 03/17/2012 - EFW - Added code to suppress output of empty parameter list for unresolved property elements.
// Added code to redirect enum field IDs to their containing enumeration type ID.

using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools
{

    // replace the old component with the new one
    public class ResolveReferenceLinksComponent : ResolveReferenceLinksComponent2
    {

        public ResolveReferenceLinksComponent(BuildAssembler assembler, XPathNavigator configuration) : base(assembler, configuration) { }

    }

    public class ResolveReferenceLinksComponent2 : BuildComponent
    {

        // instantiation logic 
        public ResolveReferenceLinksComponent2(BuildAssembler assembler, XPathNavigator configuration) : base(assembler, configuration)
        {
            // base-url is an xpath expression applied against the current document to pick up the save location of the
            // document. If specified, local links will be made relative to the base-url.
            string baseUrlValue = configuration.GetAttribute("base-url", String.Empty);
            if(!String.IsNullOrEmpty(baseUrlValue))
                baseUrl = XPathExpression.Compile(baseUrlValue);

            // url-format is a string format that is used to format the value of local href attributes. The default is
            // "{0}.htm" for backwards compatibility.
            string hrefFormatValue = configuration.GetAttribute("href-format", String.Empty);
            if(!String.IsNullOrEmpty(hrefFormatValue))
                hrefFormat = hrefFormatValue;

            // the container XPath can be replaced; this is useful
            string containerValue = configuration.GetAttribute("container", String.Empty);
            if(!String.IsNullOrEmpty(containerValue))
                XmlTargetCollectionUtilities.ContainerExpression = containerValue;

            targets = new TargetCollection();
            resolver = new LinkTextResolver(targets);

            XPathNodeIterator targets_nodes = configuration.Select("targets");
            foreach(XPathNavigator targets_node in targets_nodes)
            {

                // get target type
                string typeValue = targets_node.GetAttribute("type", String.Empty);
                if(String.IsNullOrEmpty(typeValue))
                    WriteMessage(MessageLevel.Error, "Each targets element must have a type attribute that specifies which type of links to create.");

                LinkType2 type = LinkType2.None;
                try
                {
                    type = (LinkType2)Enum.Parse(typeof(LinkType2), typeValue, true);
                    if((type == LinkType2.Msdn) && (msdn == null))
                    {
                        WriteMessage(MessageLevel.Info, "Creating MSDN URL resolver.");
                        msdn = new MsdnResolver();
                    }
                }
                catch(ArgumentException)
                {
                    WriteMessage(MessageLevel.Error, String.Format("'{0}' is not a supported reference link type.", typeValue));
                }

                // get base directory
                string baseValue = targets_node.GetAttribute("base", String.Empty);

                // get file pattern
                string filesValue = targets_node.GetAttribute("files", String.Empty);
                if(String.IsNullOrEmpty(filesValue))
                    WriteMessage(MessageLevel.Error, "Each targets element must have a files attribute specifying which target files to load.");

                // determine whether to search recursively
                bool recurse = false;
                string recurseValue = targets_node.GetAttribute("recurse", String.Empty);
                if(!String.IsNullOrEmpty(recurseValue))
                {
                    if(String.Compare(recurseValue, Boolean.TrueString, true) == 0)
                    {
                        recurse = true;
                    }
                    else if(String.Compare(recurseValue, Boolean.FalseString, true) == 0)
                    {
                        recurse = false;
                    }
                    else
                    {
                        WriteMessage(MessageLevel.Error, String.Format("On the targets element, recurse='{0}' is not an allowed value.", recurseValue));
                    }
                }

                // turn baseValue and filesValue into directoryPath and filePattern
                string fullPath;
                if(String.IsNullOrEmpty(baseValue))
                {
                    fullPath = filesValue;
                }
                else
                {
                    fullPath = Path.Combine(baseValue, filesValue);
                }
                fullPath = Environment.ExpandEnvironmentVariables(fullPath);
                string directoryPath = Path.GetDirectoryName(fullPath);
                if(String.IsNullOrEmpty(directoryPath))
                    directoryPath = Environment.CurrentDirectory;
                string filePattern = Path.GetFileName(fullPath);

                // verify that directory exists
                if(!Directory.Exists(directoryPath))
                    WriteMessage(MessageLevel.Error, String.Format("The targets directory '{0}' does not exist.", directoryPath));

                // add the specified targets from the directory
                WriteMessage(MessageLevel.Info, String.Format("Searching directory '{0}' for targets files of the form '{1}'.", directoryPath, filePattern));
                AddTargets(directoryPath, filePattern, recurse, type);

            }

            WriteMessage(MessageLevel.Info, String.Format("Loaded {0} reference targets.", targets.Count));

            string locale_value = configuration.GetAttribute("locale", String.Empty);
            if(!String.IsNullOrEmpty(locale_value) && msdn != null)
                msdn.Locale = locale_value;

            string target_value = configuration.GetAttribute("linkTarget", String.Empty);
            if(!String.IsNullOrEmpty(target_value))
                linkTarget = target_value;
        }

        private void AddTargets(string directory, string filePattern, bool recurse, LinkType2 type)
        {
            string[] files = Directory.GetFiles(directory, filePattern);
            foreach(string file in files)
            {
                AddTargets(file, type);
            }
            if(recurse)
            {
                string[] subdirectories = Directory.GetDirectories(directory);
                foreach(string subdirectory in subdirectories)
                {
                    AddTargets(subdirectory, filePattern, recurse, type);
                }
            }
        }

        private void AddTargets(string file, LinkType2 type)
        {
            try
            {
                XPathDocument document = new XPathDocument(file);
                XmlTargetCollectionUtilities.AddTargets(targets, document.CreateNavigator(), type);
            }
            catch(XmlSchemaException e)
            {
                WriteMessage(MessageLevel.Error, String.Format("The reference targets file '{0}' is not valid. The error message is: {1}", file, BuildComponentUtilities.GetExceptionMessage(e)));
            }
            catch(XmlException e)
            {
                WriteMessage(MessageLevel.Error, String.Format("The reference targets file '{0}' is not well-formed XML. The error message is: {1}", file, BuildComponentUtilities.GetExceptionMessage(e)));
            }
            catch(IOException e)
            {
                WriteMessage(MessageLevel.Error, String.Format("An access error occured while opening the reference targets file '{0}'. The error message is: {1}", file, BuildComponentUtilities.GetExceptionMessage(e)));
            }
        }

        private string linkTarget = "_blank";

        // target information storage

        private TargetCollection targets;

        private LinkTextResolver resolver;

        private static XPathExpression referenceLinkExpression = XPathExpression.Compile("//referenceLink");

        // WebDocs target url formatting

        private XPathExpression baseUrl;

        private string hrefFormat = "{0}.htm";

        // component logic

        public override void Apply(XmlDocument document, string key)
        {
            XPathNodeIterator linkIterator = document.CreateNavigator().Select(referenceLinkExpression);
            XPathNavigator[] linkNodes = BuildComponentUtilities.ConvertNodeIteratorToArray(linkIterator);

            foreach(XPathNavigator linkNode in linkNodes)
            {
                // extract link information
                ReferenceLinkInfo2 link = ReferenceLinkInfo2.Create(linkNode);

                if(link == null)
                {
                    base.WriteMessage(key, MessageLevel.Warn, "Invalid referenceLink element.");
                }
                else
                {

                    // determine target, link type, and display options
                    string targetId = link.Target;
                    DisplayOptions options = link.DisplayOptions;
                    LinkType2 type = LinkType2.None;

                    Target target = targets[targetId];
                    if(target == null)
                    {
                        // no such target known; set link type to none and warn
                        type = LinkType2.None;
                        base.WriteMessage(key, MessageLevel.Warn, "Unknown reference link target '{0}'.", targetId);

                        // !EFW - Turn off the Show Parameter option for unresolved elements except methods.  If
                        // not, it outputs an empty "()" after the member name which looks odd.
                        if(targetId[0] != 'M')
                            options &= ~DisplayOptions.ShowParameters;
                    }
                    else
                    {
                        // if overload is prefered and found, change targetId and make link options hide parameters
                        if(link.PreferOverload)
                        {
                            bool isConversionOperator = false;

                            MethodTarget method = target as MethodTarget;
                            if(method != null)
                            {
                                isConversionOperator = method.conversionOperator;
                            }

                            MemberTarget member = target as MemberTarget;

                            // if conversion operator is found, always link to individual topic.
                            if((member != null) && (!String.IsNullOrEmpty(member.OverloadId)) && !isConversionOperator)
                            {
                                Target overloadTarget = targets[member.OverloadId];
                                if(overloadTarget != null)
                                {
                                    target = overloadTarget;
                                    targetId = overloadTarget.Id;
                                }
                            }

                            // if individual conversion operator is found, always display parameters.
                            if(isConversionOperator && member != null && (!string.IsNullOrEmpty(member.OverloadId)))
                            {
                                options = options | DisplayOptions.ShowParameters;
                            }
                            else
                            {
                                options = options & ~DisplayOptions.ShowParameters;
                            }
                        }

                        // get stored link type
                        type = target.DefaultLinkType;

                        // if link type is local or index, determine which
                        if(type == LinkType2.LocalOrIndex)
                        {
                            if((key != null) && targets.Contains(key) && (target.Container == targets[key].Container))
                            {
                                type = LinkType2.Local;
                            }
                            else
                            {
                                type = LinkType2.Index;
                            }
                        }
                    }

                    // links to this page are not live
                    if(targetId == key)
                    {
                        type = LinkType2.Self;
                    }
                    else if((target != null) && (key != null) && targets.Contains(key) && (target.File == targets[key].File))
                    {
                        type = LinkType2.Self;
                    }

                    // !EFW - Redirect enumeration fields to the containing enumerated type so that we
                    // get a valid link target.  Enum fields don't have a topic to themselves.
                    if(type != LinkType2.None && type != LinkType2.Self && type != LinkType2.Local &&
                      targetId.StartsWith("F:", StringComparison.OrdinalIgnoreCase))
                    {
                        MemberTarget member = target as MemberTarget;

                        if(member != null)
                        {
                            SimpleTypeReference typeRef = member.Type as SimpleTypeReference;

                            if(typeRef != null && targets[typeRef.Id] is EnumerationTarget)
                                targetId = typeRef.Id;
                        }
                    }

                    // get msdn endpoint, if needed
                    string msdnUrl = null;

                    if(type == LinkType2.Msdn)
                    {
                        if((msdn == null) || (msdn.IsDisabled))
                        {
                            // no msdn resolver
                        }
                        else
                        {
                            msdnUrl = msdn.GetMsdnUrl(targetId);
                            if(String.IsNullOrEmpty(msdnUrl))
                            {
                                base.WriteMessage(key, MessageLevel.Warn, "MSDN URL not found for target '{0}'.", targetId);
                            }
                        }
                        if(String.IsNullOrEmpty(msdnUrl))
                            type = LinkType2.None;
                    }

                    // write opening link tag and target info
                    XmlWriter writer = linkNode.InsertAfter();

                    switch(type)
                    {
                        case LinkType2.None:
                            writer.WriteStartElement("span");
                            writer.WriteAttributeString("class", "nolink");
                            break;

                        case LinkType2.Self:
                            writer.WriteStartElement("span");
                            writer.WriteAttributeString("class", "selflink");
                            break;

                        case LinkType2.Local:
                            // format link with prefix and/or postfix
                            string href = String.Format(hrefFormat, target.File);

                            // make link relative, if we have a baseUrl
                            if(baseUrl != null)
                                href = BuildComponentUtilities.GetRelativePath(href, BuildComponentUtilities.EvalXPathExpr(document, baseUrl, "key", key));

                            writer.WriteStartElement("a");
                            writer.WriteAttributeString("href", href);
                            break;

                        case LinkType2.Index:
                            writer.WriteStartElement("mshelp", "link", "http://msdn.microsoft.com/mshelp");
                            writer.WriteAttributeString("keywords", targetId);
                            writer.WriteAttributeString("tabindex", "0");
                            break;

                        case LinkType2.Msdn:
                            writer.WriteStartElement("a");
                            writer.WriteAttributeString("href", msdnUrl);
                            writer.WriteAttributeString("target", linkTarget);
                            break;

                        case LinkType2.Id:
                            string xhelp = String.Format("ms-xhelp:///?Id={0}", targetId);
                            xhelp = xhelp.Replace("#", "%23");
                            writer.WriteStartElement("a");
                            writer.WriteAttributeString("href", xhelp);
                            break;
                    }

                    // write the link text
                    if(String.IsNullOrEmpty(link.DisplayTarget))
                    {
                        if(link.Contents == null)
                        {
                            if(target != null)
                            {
                                resolver.WriteTarget(target, options, writer);
                            }
                            else
                            {
                                //Console.WriteLine("Attemting to create reference");
                                Reference reference = TextReferenceUtilities.CreateReference(targetId);
                                //Console.WriteLine("Returned");
                                if(reference is InvalidReference)
                                    base.WriteMessage(key, MessageLevel.Warn, "Invalid reference link target '{0}'.", targetId);
                                resolver.WriteReference(reference, options, writer);
                            }
                        }
                        else
                        {
                            // write contents to writer
                            link.Contents.WriteSubtree(writer);
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Display target = {0}", link.DisplayTarget);
                        if((String.Compare(link.DisplayTarget, "content", true) == 0) && (link.Contents != null))
                        {
                            // Use the contents as an XML representation of the display target

                            //Console.WriteLine(link.Contents.NodeType);
                            Reference reference = XmlTargetCollectionUtilities.CreateReference(link.Contents);
                            //Console.WriteLine(reference.GetType().FullName);
                            resolver.WriteReference(reference, options, writer);
                        }
                        if((String.Compare(link.DisplayTarget, "format", true) == 0) && (link.Contents != null))
                        {
                            // Use the contents as a format string for the display target

                            string format = link.Contents.OuterXml;
                            //Console.WriteLine("format = {0}", format);

                            string input = null;
                            StringWriter textStore = new StringWriter();
                            try
                            {
                                XmlWriterSettings settings = new XmlWriterSettings();
                                settings.ConformanceLevel = ConformanceLevel.Fragment;

                                XmlWriter xmlStore = XmlWriter.Create(textStore, settings);
                                try
                                {
                                    if(target != null)
                                    {
                                        resolver.WriteTarget(target, options, xmlStore);
                                    }
                                    else
                                    {
                                        Reference reference = TextReferenceUtilities.CreateReference(targetId);
                                        resolver.WriteReference(reference, options, xmlStore);
                                    }
                                }
                                finally
                                {
                                    xmlStore.Close();
                                }
                                input = textStore.ToString();
                            }
                            finally
                            {
                                textStore.Close();
                            }
                            //Console.WriteLine("input = {0}", input);

                            string output = String.Format(format, input);
                            //Console.WriteLine("output = {0}", output);

                            XmlDocumentFragment fragment = document.CreateDocumentFragment();
                            fragment.InnerXml = output;
                            fragment.WriteTo(writer);

                            //writer.WriteRaw(output);
                        }
                        else if((String.Compare(link.DisplayTarget, "extension", true) == 0) && (link.Contents != null))
                        {
                            Reference extMethodReference = XmlTargetCollectionUtilities.CreateExtensionMethodReference(link.Contents);
                            resolver.WriteReference(extMethodReference, options, writer);
                        }
                        else
                        {
                            // Use the display target value as a CER for the display target

                            TextReferenceUtilities.SetGenericContext(key);
                            Reference reference = TextReferenceUtilities.CreateReference(link.DisplayTarget);
                            //Console.WriteLine("Reference is {0}", reference.GetType().FullName);
                            resolver.WriteReference(reference, options, writer);
                        }
                    }

                    // write the closing link tag
                    writer.WriteEndElement();
                    writer.Close();
                }

                // delete the original tag
                linkNode.DeleteSelf();

            }

        }

        // msdn resolver

        private MsdnResolver msdn = null;


    }


    internal class ReferenceLinkInfo2
    {

        // stored data

        private string target;

        private string displayTarget;

        private DisplayOptions options = DisplayOptions.Default;

        private bool preferOverload = false;

        private XPathNavigator contents;

        // data accessors

        public string Target
        {
            get
            {
                return (target);
            }
        }

        public string DisplayTarget
        {
            get
            {
                return (displayTarget);
            }
        }

        public DisplayOptions DisplayOptions
        {
            get
            {
                return (options);
            }
        }

        public bool PreferOverload
        {
            get
            {
                return (preferOverload);
            }
        }

        public XPathNavigator Contents
        {
            get
            {
                return (contents);
            }
        }

        // creation logic

        private ReferenceLinkInfo2() { }

        public static ReferenceLinkInfo2 Create(XPathNavigator element)
        {
            if(element == null)
                throw new ArgumentNullException("element");

            ReferenceLinkInfo2 info = new ReferenceLinkInfo2();

            info.target = element.GetAttribute("target", String.Empty);
            if(String.IsNullOrEmpty(info.target))
                return (null);

            info.displayTarget = element.GetAttribute("display-target", String.Empty);

            string showContainer = element.GetAttribute("show-container", String.Empty);
            if(String.IsNullOrEmpty(showContainer))
                showContainer = element.GetAttribute("qualified", String.Empty);
            if(!String.IsNullOrEmpty(showContainer))
            {
                if(String.Compare(showContainer, Boolean.TrueString, true) == 0)
                {
                    info.options = info.options | DisplayOptions.ShowContainer;
                }
                else if(String.Compare(showContainer, Boolean.FalseString, true) == 0)
                {
                    info.options = info.options & ~DisplayOptions.ShowContainer;
                }
                else
                {
                    return (null);
                }
            }

            string showTemplates = element.GetAttribute("show-templates", String.Empty);
            if(!String.IsNullOrEmpty(showTemplates))
            {
                if(String.Compare(showTemplates, Boolean.TrueString, true) == 0)
                {
                    info.options = info.options | DisplayOptions.ShowTemplates;
                }
                else if(String.Compare(showTemplates, Boolean.FalseString, true) == 0)
                {
                    info.options = info.options & ~DisplayOptions.ShowTemplates;
                }
                else
                {
                    return (null);
                }
            }

            string showParameters = element.GetAttribute("show-parameters", String.Empty);
            if(!String.IsNullOrEmpty(showParameters))
            {
                if(String.Compare(showParameters, Boolean.TrueString, true) == 0)
                {
                    info.options = info.options | DisplayOptions.ShowParameters;
                }
                else if(String.Compare(showParameters, Boolean.FalseString, true) == 0)
                {
                    info.options = info.options & ~DisplayOptions.ShowParameters;
                }
                else
                {
                    return (null);
                }
            }


            string preferOverload = element.GetAttribute("prefer-overload", String.Empty);
            if(String.IsNullOrEmpty(preferOverload))
                preferOverload = element.GetAttribute("auto-upgrade", String.Empty);
            if(!String.IsNullOrEmpty(preferOverload))
            {
                if(String.Compare(preferOverload, Boolean.TrueString, true) == 0)
                {
                    info.preferOverload = true;
                }
                else if(String.Compare(preferOverload, Boolean.FalseString, true) == 0)
                {
                    info.preferOverload = false;
                }
                else
                {
                    return (null);
                }
            }

            info.contents = element.Clone();
            if(!info.contents.MoveToFirstChild())
                info.contents = null;

            return (info);
        }

    }
}
