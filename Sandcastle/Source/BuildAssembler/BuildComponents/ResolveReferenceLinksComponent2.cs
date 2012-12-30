// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 03/17/2012 - EFW - Added code to suppress output of empty parameter list for unresolved property elements.
// Added code to redirect enum field IDs to their containing enumeration type ID.
// 12/21/2012 - EFW - Removed obsolete ResolveReferenceLinksComponent class
// 12/28/2012 - EFW - General code clean-up.  Added code to report the reason for MSDN service failures.
// Exposed the target collection and MSDN resolver via properties.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

using Microsoft.Ddue.Tools.Targets;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This build component is used to resolve links to reference topics
    /// </summary>
    public class ResolveReferenceLinksComponent2 : BuildComponent
    {
        #region Constants
        //=====================================================================

        /// <summary>
        /// This is used a the key name when sharing the MSDN content ID cache across instances
        /// </summary>
        public const string SharedMsdnCacheId = "SharedMsdnCache";
        #endregion

        #region Private data members
        //=====================================================================

        private static XPathExpression referenceLinkExpression = XPathExpression.Compile("//referenceLink");

        private string linkTarget, msdnIdCacheFile;

        private TargetCollection targets;

        private LinkTextResolver resolver;

        private MsdnResolver msdnResolver;

        // WebDocs target url formatting (legacy Microsoft stuff)
        private XPathExpression baseUrl;
        private string hrefFormat;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the MSDN resolver instance
        /// </summary>
        protected MsdnResolver MsdnResolver
        {
            get { return msdnResolver; }
        }

        /// <summary>
        /// This read-only property returns the target dictionary
        /// </summary>
        protected TargetCollection Targets
        {
            get { return targets; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public ResolveReferenceLinksComponent2(BuildAssembler assembler, XPathNavigator configuration) :
          base(assembler, configuration)
        {
            Target target;
            ReferenceLinkType type;

            // base-url is an xpath expression applied against the current document to pick up the save location of the
            // document. If specified, local links will be made relative to the base-url.
            string baseUrlValue = configuration.GetAttribute("base-url", String.Empty);

            if(!String.IsNullOrEmpty(baseUrlValue))
                baseUrl = XPathExpression.Compile(baseUrlValue);

            // url-format is a string format that is used to format the value of local href attributes. The default is
            // "{0}.htm" for backwards compatibility.
            hrefFormat = configuration.GetAttribute("href-format", String.Empty);

            if(String.IsNullOrWhiteSpace(hrefFormat))
                hrefFormat = "{0}.htm";

            // The container XPath can be replaced; this is useful
            string containerValue = configuration.GetAttribute("container", String.Empty);

            if(!String.IsNullOrEmpty(containerValue))
                XmlTargetCollectionUtilities.ContainerExpression = containerValue;

            targets = new TargetCollection();
            resolver = new LinkTextResolver(targets);

            XPathNodeIterator targets_nodes = configuration.Select("targets");

#if DEBUG
            base.WriteMessage(MessageLevel.Diagnostic, "Loading reference link target info");

            DateTime startLoad = DateTime.Now;
#endif
            foreach(XPathNavigator targets_node in targets_nodes)
            {
                // Get target type
                string typeValue = targets_node.GetAttribute("type", String.Empty);

                if(String.IsNullOrEmpty(typeValue))
                    base.WriteMessage(MessageLevel.Error, "Each targets element must have a type attribute " +
                        "that specifies which type of links to create.");

                if(!Enum.TryParse<ReferenceLinkType>(typeValue, true, out type))
                    base.WriteMessage(MessageLevel.Error, "'{0}' is not a supported reference link type.", typeValue);

                if(type == ReferenceLinkType.Msdn && msdnResolver == null)
                {
                    base.WriteMessage(MessageLevel.Info, "Creating MSDN URL resolver.");
                    msdnResolver = this.CreateMsdnResolver(configuration);
                }

                // Get base directory
                string baseValue = targets_node.GetAttribute("base", String.Empty);

                // Get file pattern
                string filesValue = targets_node.GetAttribute("files", String.Empty);

                if(String.IsNullOrEmpty(filesValue))
                    base.WriteMessage(MessageLevel.Error, "Each targets element must have a files attribute " +
                        "specifying which target files to load.");

                // Determine whether to search recursively
                bool recurse = false;
                string recurseValue = targets_node.GetAttribute("recurse", String.Empty);

                if(!String.IsNullOrEmpty(recurseValue) && !Boolean.TryParse(recurseValue, out recurse))
                    base.WriteMessage(MessageLevel.Error, "On the targets element, recurse='{0}' is not an " +
                        "allowed value.", recurseValue);

                // Turn baseValue and filesValue into directoryPath and filePattern
                string fullPath;

                if(String.IsNullOrEmpty(baseValue))
                    fullPath = filesValue;
                else
                    fullPath = Path.Combine(baseValue, filesValue);

                fullPath = Environment.ExpandEnvironmentVariables(fullPath);

                string directoryPath = Path.GetDirectoryName(fullPath);

                if(String.IsNullOrEmpty(directoryPath))
                    directoryPath = Environment.CurrentDirectory;

                string filePattern = Path.GetFileName(fullPath);

                // Verify that the directory exists
                if(!Directory.Exists(directoryPath))
                    base.WriteMessage(MessageLevel.Error, "The targets directory '{0}' does not exist.", directoryPath);

                // Add the specified targets from the directory
                base.WriteMessage(MessageLevel.Info, "Searching directory '{0}' for targets files of the " +
                    "form '{1}'.", directoryPath, filePattern);

                foreach(string file in Directory.EnumerateFiles(directoryPath, filePattern, recurse ?
                  SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        XPathDocument document = new XPathDocument(file);
                        XmlTargetCollectionUtilities.AddTargets(targets, document.CreateNavigator(), type);
                    }
                    catch(XmlSchemaException e)
                    {
                        base.WriteMessage(MessageLevel.Error, "The reference targets file '{0}' is not valid. " +
                            "The error message is: {1}", file, e.GetExceptionMessage());
                    }
                    catch(XmlException e)
                    {
                        base.WriteMessage(MessageLevel.Error, "The reference targets file '{0}' is not " +
                            "well-formed XML.  The error message is: {1}", file, e.GetExceptionMessage());
                    }
                    catch(IOException e)
                    {
                        base.WriteMessage(MessageLevel.Error, "An access error occured while opening the " +
                            "reference targets file '{0}'. The error message is: {1}", file,
                            e.GetExceptionMessage());
                    }
                }
            }

            // If we have an MSDN resolver with cached entries, update targets with a null content ID to use a
            // link type of None so that we don't waste time looking them up again.
            if(msdnResolver != null && msdnResolver.MsdnContentIdCache.Count != 0)
                foreach(var kv in msdnResolver.MsdnContentIdCache)
                    if(kv.Value == null && targets.TryGetValue(kv.Key, out target))
                        target.DefaultLinkType = ReferenceLinkType.None;

#if DEBUG
            TimeSpan loadTime = (DateTime.Now - startLoad);
            base.WriteMessage(MessageLevel.Diagnostic, "Load time: {0} seconds", loadTime.TotalSeconds);

            // Dump targets for comparison to other versions
//            targets.DumpTargetCollection(Path.GetFullPath("TargetCollection.xml"));
#endif

            base.WriteMessage(MessageLevel.Info, "Loaded {0} reference targets.", targets.Count);

            string locale_value = configuration.GetAttribute("locale", String.Empty);

            if(!String.IsNullOrEmpty(locale_value) && msdnResolver != null)
                msdnResolver.Locale = locale_value;

            linkTarget = configuration.GetAttribute("linkTarget", String.Empty);

            if(String.IsNullOrWhiteSpace(linkTarget))
                linkTarget = "_blank";
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            Target keyTarget;
            string msdnUrl = null;

            foreach(XPathNavigator linkNode in document.CreateNavigator().Select(referenceLinkExpression).ToArray())
            {
                // Extract link information
                ReferenceLinkInfo link = new ReferenceLinkInfo(linkNode);

                // Determine target, link type, and display options
                string targetId = link.Target;
                DisplayOptions options = link.DisplayOptions;
                ReferenceLinkType type = ReferenceLinkType.None;

                Target target = targets[targetId];

                if(target == null)
                {
                    // No such target known; set link type to none and warn
                    type = ReferenceLinkType.None;
                    base.WriteMessage(key, MessageLevel.Warn, "Unknown reference link target '{0}'.", targetId);

                    // !EFW - Turn off the Show Parameters option for unresolved elements except methods.  If
                    // not, it outputs an empty "()" after the member name which looks odd.
                    if(targetId[0] != 'M')
                        options &= ~DisplayOptions.ShowParameters;
                }
                else
                {
                    // If overload is prefered and found, change targetId and make link options hide parameters
                    if(link.PreferOverload)
                    {
                        bool isConversionOperator = false;

                        MethodTarget method = target as MethodTarget;

                        if(method != null)
                            isConversionOperator = method.conversionOperator;

                        MemberTarget member = target as MemberTarget;

                        // If conversion operator is found, always link to individual topic
                        if(member != null && !String.IsNullOrEmpty(member.OverloadId) && !isConversionOperator)
                        {
                            Target overloadTarget = targets[member.OverloadId];

                            if(overloadTarget != null)
                            {
                                target = overloadTarget;
                                targetId = overloadTarget.Id;
                            }
                        }

                        // If individual conversion operator is found, always display parameters
                        if(isConversionOperator && member != null && !String.IsNullOrEmpty(member.OverloadId))
                            options = options | DisplayOptions.ShowParameters;
                        else
                            options = options & ~DisplayOptions.ShowParameters;
                    }

                    // Get stored link type
                    type = target.DefaultLinkType;

                    // If link type is Local or Index, determine which
                    if(type == ReferenceLinkType.LocalOrIndex)
                        if(targets.TryGetValue(key, out keyTarget) && target.Container == keyTarget.Container)
                            type = ReferenceLinkType.Local;
                        else
                            type = ReferenceLinkType.Index;
                }

                // Links to this page are not live
                if(targetId == key)
                    type = ReferenceLinkType.Self;
                else
                    if(target != null && targets.TryGetValue(key, out keyTarget) && target.File == keyTarget.File)
                        type = ReferenceLinkType.Self;

                // !EFW - Redirect enumeration fields to the containing enumerated type so that we
                // get a valid link target.  Enum fields don't have a topic to themselves.
                if(type != ReferenceLinkType.None && type != ReferenceLinkType.Self && type != ReferenceLinkType.Local &&
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

                // Get MSDN endpoint if needed
                if(type == ReferenceLinkType.Msdn)
                    if(msdnResolver != null && !msdnResolver.IsDisabled)
                    {
                        msdnUrl = msdnResolver.GetMsdnUrl(targetId);

                        if(String.IsNullOrEmpty(msdnUrl))
                        {
                            // If the web service failed, report the reason
                            if(msdnResolver.IsDisabled)
                                base.WriteMessage(key, MessageLevel.Warn, "MSDN web service failed.  No " +
                                    "further look ups will be performed for this run.\r\nReason: {0}",
                                    msdnResolver.DisabledReason);
                            else
                                base.WriteMessage(key, MessageLevel.Warn, "MSDN URL not found for target '{0}'.",
                                    targetId);

                            type = ReferenceLinkType.None;
                        }
                    }
                    else
                        type = ReferenceLinkType.None;

                // Write opening link tag and target info
                XmlWriter writer = linkNode.InsertAfter();

                switch(type)
                {
                    case ReferenceLinkType.None:
                        writer.WriteStartElement("span");
                        writer.WriteAttributeString("class", "nolink");
                        break;

                    case ReferenceLinkType.Self:
                        writer.WriteStartElement("span");
                        writer.WriteAttributeString("class", "selflink");
                        break;

                    case ReferenceLinkType.Local:
                        // Format link with prefix and/or postfix
                        string href = String.Format(CultureInfo.InvariantCulture, hrefFormat, target.File);

                        // Make link relative, if we have a baseUrl
                        if(baseUrl != null)
                            href = href.GetRelativePath(document.EvalXPathExpr(baseUrl, "key", key));

                        writer.WriteStartElement("a");
                        writer.WriteAttributeString("href", href);
                        break;

                    case ReferenceLinkType.Index:
                        writer.WriteStartElement("mshelp", "link", "http://msdn.microsoft.com/mshelp");
                        writer.WriteAttributeString("keywords", targetId);
                        writer.WriteAttributeString("tabindex", "0");
                        break;

                    case ReferenceLinkType.Msdn:
                        writer.WriteStartElement("a");
                        writer.WriteAttributeString("href", msdnUrl);
                        writer.WriteAttributeString("target", linkTarget);
                        break;

                    case ReferenceLinkType.Id:
                        writer.WriteStartElement("a");
                        writer.WriteAttributeString("href", ("ms-xhelp:///?Id=" + targetId).Replace("#", "%23"));
                        break;
                }

                // Write the link text
                if(String.IsNullOrEmpty(link.DisplayTarget))
                {
                    if(link.Contents == null)
                    {
                        if(target != null)
                            resolver.WriteTarget(target, options, writer);
                        else
                        {
                            Reference reference = TextReferenceUtilities.CreateReference(targetId);

                            if(reference is InvalidReference)
                                base.WriteMessage(key, MessageLevel.Warn,
                                    "Invalid reference link target '{0}'.", targetId);

                            resolver.WriteReference(reference, options, writer);
                        }
                    }
                    else
                    {
                        // Write contents to writer
                        link.Contents.WriteSubtree(writer);
                    }
                }
                else
                {
                    if(link.DisplayTarget.Equals("content", StringComparison.OrdinalIgnoreCase)  &&
                        link.Contents != null)
                    {
                        // Use the contents as an XML representation of the display target
                        Reference reference = XmlTargetCollectionUtilities.CreateReference(link.Contents);
                        resolver.WriteReference(reference, options, writer);
                    }

                    if(link.DisplayTarget.Equals("format", StringComparison.OrdinalIgnoreCase) &&
                        link.Contents != null)
                    {
                        // Use the contents as a format string for the display target
                        string format = link.Contents.OuterXml;
                        string input = null;

                        using(StringWriter textStore = new StringWriter(CultureInfo.InvariantCulture))
                        {
                            XmlWriterSettings settings = new XmlWriterSettings();
                            settings.ConformanceLevel = ConformanceLevel.Fragment;

                            XmlWriter xmlStore = XmlWriter.Create(textStore, settings);

                            try
                            {
                                if(target != null)
                                    resolver.WriteTarget(target, options, xmlStore);
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

                        string output = String.Format(CultureInfo.InvariantCulture, format, input);

                        XmlDocumentFragment fragment = document.CreateDocumentFragment();
                        fragment.InnerXml = output;
                        fragment.WriteTo(writer);
                    }
                    else if(link.DisplayTarget.Equals("extension", StringComparison.OrdinalIgnoreCase) &&
                        link.Contents != null)
                    {
                        Reference extMethodReference = XmlTargetCollectionUtilities.CreateExtensionMethodReference(link.Contents);
                        resolver.WriteReference(extMethodReference, options, writer);
                    }
                    else
                    {
                        // Use the display target value as a CER for the display target
                        TextReferenceUtilities.SetGenericContext(key);
                        Reference reference = TextReferenceUtilities.CreateReference(link.DisplayTarget);
                        resolver.WriteReference(reference, options, writer);
                    }
                }

                // Write the closing link tag
                writer.WriteEndElement();
                writer.Close();

                // Delete the original tag
                linkNode.DeleteSelf();
            }
        }

        /// <summary>
        /// This is overriden to save the updated MSDN URL cache, if used, when disposed
        /// </summary>
        /// <param name="disposing">Pass true to dispose of the managed and unmanaged resources or false to just
        /// dispose of the unmanaged resources.</param>
        /// <remarks>Derived classes that override <see cref="CreateMsdnResolver"/> should also override this
        /// method if necessary to persist the cache if it was updated.</remarks>
        protected override void Dispose(bool disposing)
        {
            if(disposing && msdnIdCacheFile != null && msdnResolver != null && msdnResolver.CacheItemsAdded)
            {
                base.WriteMessage(MessageLevel.Info, "MSDN URL cache updated.  Saving new information to " +
                    msdnIdCacheFile);

                try
                {
                    using(FileStream fs = new FileStream(msdnIdCacheFile, FileMode.Create, FileAccess.ReadWrite,
                      FileShare.None))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(fs, msdnResolver.MsdnContentIdCache);

                        base.WriteMessage(MessageLevel.Info, "New MSDN URL cache size: {0} entries",
                            msdnResolver.MsdnContentIdCache.Count);
                    }
                }
                catch(IOException ex)
                {
                    // Most likely it couldn't access the file.  We'll issue a warning but will continue with
                    // the build.
                    base.WriteMessage(MessageLevel.Warn, "Unable to create MSDN URL cache file.  It will be " +
                        "created or updated on a subsequent build: " + ex.Message);
                }
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to create an MSDN resolver for the reference link to use in looking up MSDN content iDs
        /// </summary>
        /// <param name="configuration">The component configuration</param>
        /// <returns>An MSDN resolver instance</returns>
        /// <remarks>This can be overridden in derived classes to provide persistent caches with backing stores
        /// other than the default dictionary serialized to a binary file.  It also allows sharing the cache
        /// across instances by placing it in the <see cref="BuildComponent.Data"/> dictionary using the key
        /// name <c>SharedMsdnUrlCacheID</c>.
        /// 
        /// <para>If overridden, the <see cref="Dispose"/> method may also need to be overridden to persist
        /// changes to the cache as needed.</para></remarks>
        public virtual MsdnResolver CreateMsdnResolver(XPathNavigator configuration)
        {
            MsdnResolver resolver;
            IDictionary<string, string> cache = null;

            if(BuildComponent.Data.ContainsKey(SharedMsdnCacheId))
                cache = BuildComponent.Data[SharedMsdnCacheId] as IDictionary<string, string>;

            // If the shared cache already exists, return an instance that uses it.  It is assumed that all
            // subsequent instances will use the same cache.
            if(cache != null)
                return new MsdnResolver(cache);

            // If a <cache> element is not specified, we'll use the standard resolver without a persistent cache.
            // We will share it across all instances though.
            XPathNavigator node = configuration.SelectSingleNode("msdnUrlCache");

            if(node == null)
                resolver = new MsdnResolver();
            else
            {
                // Keep the filename.  If we own it, we'll update the cache file when disposed.
                msdnIdCacheFile = node.GetAttribute("path", String.Empty);

                if(String.IsNullOrWhiteSpace(msdnIdCacheFile))
                    base.WriteMessage(MessageLevel.Error, "You must specify a path attribute value on the " +
                        "msdnUrlCache element.");

                // Create the folder if it doesn't exist
                msdnIdCacheFile = Path.GetFullPath(Environment.ExpandEnvironmentVariables(msdnIdCacheFile));
                string path = Path.GetDirectoryName(msdnIdCacheFile);

                if(!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                // Load the cache if it exists
                if(!File.Exists(msdnIdCacheFile))
                {
                    // Logged as a diagnostic message since looking up all IDs can significantly slow the build
                    base.WriteMessage(MessageLevel.Diagnostic, "The MSDN URL cache '" + msdnIdCacheFile +
                        "' does not exist yet.  All IDs will be looked up in this build which will slow it down.");

                    resolver = new MsdnResolver();
                }
                else
                    using(FileStream fs = new FileStream(msdnIdCacheFile, FileMode.Open, FileAccess.Read,
                      FileShare.Read))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        resolver = new MsdnResolver((IDictionary<string, string>)bf.Deserialize(fs));

                        base.WriteMessage(MessageLevel.Info, "Loaded {0} cached MSDN URL entries",
                            resolver.MsdnContentIdCache.Count);
                    }
            }

            BuildComponent.Data[SharedMsdnCacheId] = resolver.MsdnContentIdCache;

            return resolver;
        }
        #endregion
    }
}
