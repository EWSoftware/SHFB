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
// 12/30/2012 - EFW - Reworked to use TargetTypeDictionary and share the target data across instances.
// 03/17/2013 - EFW - Added support for the ReferenceLink.RenderAsLink property
// 11/08/2013 - EFW - Applied patch from Stazzz to write out nested XML elements within the link inner text
// 12/24/2013 - EFW - Updated the build component to be discoverable via MEF
// 03/03/2014 - EFW - Added code to default to looking up target IDs if not found in the reflection data but
// they start with "System." or "Microsoft.".  This should help add links to MSDN content for Microsoft SDKs
// without having to add additional reference link data to the help project.
// 03/28/2015 - EFW - Changed the href-format attribute to a nested hrefFormat element.
// 06/05/2015 - EFW - Removed support for the Help 2 Index and LocalOrIndex link types
// 03/24/2017 - EFW - Updated to try and resolve missing overload IDs to an equivalent non-overload method ID
// 08/15/2019 - EFW - Replaced the MSDN resolver with the new Microsoft Docs resolver

// Ignore Spelling: Stazzz noopener noreferrer

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Tools.BuildComponents.Targets;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Sandcastle.Tools.BuildComponents
{
    /// <summary>
    /// This build component is used to resolve links to reference topics
    /// </summary>
    public class ResolveReferenceLinksComponent : BuildComponentCore
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("Resolve Reference Links Component")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new ResolveReferenceLinksComponent(this.BuildAssembler);
            }
        }
        #endregion

        #region Constants
        //=====================================================================

        /// <summary>
        /// This is used as the key name when sharing the URL cache across instances
        /// </summary>
        public const string SharedMemberUrlCacheId = "SharedMemberUrlCache";

        /// <summary>
        /// This is used as the key name when sharing the target dictionaries across instances
        /// </summary>
        public const string SharedReferenceTargetsId = "SharedReferenceTargets";

        #endregion

        #region Private data members
        //=====================================================================

        private static readonly XPathExpression referenceLinkExpression = XPathExpression.Compile("//referenceLink");

        private Dictionary<string, TargetDictionary> sharedTargets;
        private TargetTypeDictionary targets;

        private LinkTextResolver resolver;

        private string linkTarget, memberIdUrlCacheFile;

        // WebDocs target URL formatting (legacy Microsoft stuff)
        private XPathExpression baseUrl;
        private string hrefFormat;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the member ID URL resolver instance
        /// </summary>
        protected IMemberIdUrlResolver UrlResolver { get; private set; }

        /// <summary>
        /// This read-only property returns the target type dictionary
        /// </summary>
        protected TargetTypeDictionary Targets => targets;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected ResolveReferenceLinksComponent(IBuildAssembler buildAssembler) : base(buildAssembler)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            string attrValue, id;

            targets = new TargetTypeDictionary();
            resolver = new LinkTextResolver(targets);

            // Get the shared instances dictionary.  Create it if it doesn't exist.
            if(this.BuildAssembler.Data.TryGetValue(SharedReferenceTargetsId, out object cacheValue))
                sharedTargets = cacheValue as Dictionary<string, TargetDictionary>;

            if(sharedTargets == null)
                this.BuildAssembler.Data[SharedReferenceTargetsId] = sharedTargets = new Dictionary<string, TargetDictionary>();

            // base-url is an xpath expression applied against the current document to pick up the save location of the
            // document. If specified, local links will be made relative to the base-url.
            string baseUrlValue = configuration.GetAttribute("base-url", String.Empty);

            if(!String.IsNullOrWhiteSpace(baseUrlValue))
                baseUrl = XPathExpression.Compile(baseUrlValue);

            // hrefFormat is a string format that is used to format the value of local href attributes. The
            // default is "{0}.htm" if not specified.
            hrefFormat = (string)configuration.Evaluate("string(hrefFormat/@value)");

            if(String.IsNullOrWhiteSpace(hrefFormat))
                hrefFormat = "{0}.htm";

            // The container XPath can be replaced; this is useful
            string containerValue = configuration.GetAttribute("container", String.Empty);

            if(!String.IsNullOrWhiteSpace(containerValue))
                XmlTargetDictionaryUtilities.ContainerExpression = containerValue;

            XPathNodeIterator targetsNodes = configuration.Select("targets");

#if DEBUG
            this.WriteMessage(MessageLevel.Diagnostic, "Loading reference link target info");

            DateTime startLoad = DateTime.Now;
#endif
            foreach(XPathNavigator targetsNode in targetsNodes)
            {
                // Get target type
                attrValue = targetsNode.GetAttribute("type", String.Empty);

                if(String.IsNullOrWhiteSpace(attrValue))
                    this.WriteMessage(MessageLevel.Error, "Each targets element must have a type attribute " +
                        "that specifies which type of links to create");

                if(!Enum.TryParse<ReferenceLinkType>(attrValue, true, out ReferenceLinkType type))
                    this.WriteMessage(MessageLevel.Error, "'{0}' is not a supported reference link type",
                        attrValue);

                // Check for shared instance by ID.  If not there, create it and add it.
                id = targetsNode.GetAttribute("id", String.Empty);

                if(!sharedTargets.TryGetValue(id, out TargetDictionary newTargets))
                {
                    this.WriteMessage(MessageLevel.Info, "Loading {0} reference link type targets", type);

                    newTargets = this.CreateTargetDictionary(targetsNode);
                    sharedTargets[newTargets.DictionaryId] = newTargets;
                }

                targets.Add(type, newTargets);
            }

#if DEBUG
            TimeSpan loadTime = (DateTime.Now - startLoad);
            this.WriteMessage(MessageLevel.Diagnostic, "Load time: {0} seconds", loadTime.TotalSeconds);

            // Dump targets for comparison to other versions
//            targets.DumpTargetDictionary(Path.GetFullPath("TargetDictionary.xml"));

            // Serialization test
//            targets.SerializeDictionary(Directory.GetCurrentDirectory());
#endif
            // Getting the count from a database cache can be expensive so only report it if it will be seen
            if(this.BuildAssembler.VerbosityLevel == MessageLevel.Info)
                this.WriteMessage(MessageLevel.Info, "{0} total reference link targets", targets.Count);

            if(targets.NeedsMemberIdUrlResolver)
            {
                this.WriteMessage(MessageLevel.Info, "Creating member ID URL resolver");

                this.UrlResolver = this.CreateMemberIdResolver(configuration);

                string localeValue = (string)configuration.Evaluate("string(locale/@value)");

                if(this.UrlResolver != null && !String.IsNullOrWhiteSpace(localeValue))
                    this.UrlResolver.Locale = localeValue;
            }

            linkTarget = (string)configuration.Evaluate("string(linkTarget/@value)");

            if(String.IsNullOrWhiteSpace(linkTarget))
                linkTarget = "_blank";
        }

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            if(document == null)
                throw new ArgumentNullException(nameof(document));

            Target target = null;
            string memberUrl = null;

            foreach(XPathNavigator linkNode in document.CreateNavigator().Select(referenceLinkExpression).ToArray())
            {
                // Extract link information
                ReferenceLinkInfo link = new ReferenceLinkInfo(linkNode);

                // Determine target, link type, and display options
                string targetId = link.Target;
                DisplayOptions options = link.DisplayOptions;
                ReferenceLinkType type = ReferenceLinkType.None;

                if(String.IsNullOrWhiteSpace(targetId) || targetId == "!:")
                {
                    this.WriteMessage(key, MessageLevel.Warn, "The target attribute is missing or has no " +
                        "value.  You have most likely omitted a cref attribute or left it blank on an XML " +
                        "comments element such as see, seealso, or exception.");

                    linkNode.DeleteSelf();
                    continue;
                }

                bool targetFound = targets.TryGetValue(targetId, out target, out type);

                // If it's an overload ID that wasn't found, it's possible that the overloads got excluded.
                // As such, see if we can find a match for a method using the same ID regardless of any
                // parameters.
                if(!targetFound && targetId.StartsWith("Overload:", StringComparison.Ordinal) ||
                  targetId.StartsWith("O:", StringComparison.Ordinal))
                {
                    string methodTargetId = "M:" + targetId.Substring(targetId.IndexOf(':') + 1);
                    methodTargetId = targets.Keys.FirstOrDefault(k => k.StartsWith(methodTargetId,
                        StringComparison.Ordinal));

                    if(methodTargetId != null)
                    {
                        targetId = methodTargetId;
                        targetFound = targets.TryGetValue(targetId, out target, out type);
                        options |= DisplayOptions.ShowParameters;

                        // Don't use the content as it may not be appropriate for the method.  The default is
                        // "{0} Overload" which no longer applies.  Instead we'll show the method name and
                        // its parameters.
                        if(link.DisplayTarget.Equals("format", StringComparison.OrdinalIgnoreCase))
                        {
                            link.DisplayTarget = null;
                            link.Contents = null;
                        }
                    }
                }

                // If not found and it starts with "System." or "Microsoft." we'll go with the assumption that
                // it's part of a Microsoft class library that is not part of the core framework but does have
                // documentation available online.  Worst case it doesn't and we get an unresolved link warning
                // instead of an unknown reference target warning.
                if(!targetFound && ((targetId.Length > 9 && targetId.Substring(2).StartsWith("System.",
                  StringComparison.Ordinal)) || (targetId.Length > 12 && targetId.Substring(2).StartsWith(
                  "Microsoft.", StringComparison.Ordinal))))
                {
                    // Use the same link type as a core framework class
                    targetFound = targets.TryGetValue("T:System.Object", out target, out type);

                    // We don't have a target in this case so links to overloads pages won't work.  Also note
                    // that the link text will be generated from the ID which shouldn't make much of a difference
                    // in most cases.  If either case is an issue, the Additional Reference Links SHFB plug-in
                    // can be used to generate valid link target data.
                    target = null;
                }

                if(!targetFound)
                {
                    // If not being rendered as a link, don't report a warning
                    if(link.RenderAsLink && targetId != key)
                        this.WriteMessage(key, MessageLevel.Warn, "Unknown reference link target '{0}'.", targetId);

                    // !EFW - Turn off the Show Parameters option for unresolved elements except methods.  If
                    // not, it outputs an empty "()" after the member name which looks odd.
                    if(targetId[0] != 'M')
                        options &= ~DisplayOptions.ShowParameters;
                }
                else
                {
                    // If overload is preferred and found, change targetId and make link options hide parameters
                    if(link.PreferOverload && target != null)
                    {
                        bool isConversionOperator = false;

                        if(target is MethodTarget method)
                            isConversionOperator = method.IsConversionOperator;

                        MemberTarget member = target as MemberTarget;

                        // If conversion operator is found, always link to individual topic
                        if(member != null && !String.IsNullOrWhiteSpace(member.OverloadId) && !isConversionOperator)
                        {
                            Target overloadTarget = targets[member.OverloadId];

                            if(overloadTarget != null)
                            {
                                target = overloadTarget;
                                targetId = overloadTarget.Id;
                            }
                        }

                        // If individual conversion operator is found, always display parameters
                        if(isConversionOperator && member != null && !String.IsNullOrWhiteSpace(member.OverloadId))
                            options |= DisplayOptions.ShowParameters;
                        else
                            options &= ~DisplayOptions.ShowParameters;
                    }
                }

                // Suppress the link if so requested.  Links to this page are not live.
                if(!link.RenderAsLink)
                    type = ReferenceLinkType.None;
                else
                {
                    if(targetId == key)
                        type = ReferenceLinkType.Self;
                    else
                    {
                        if(target != null && targets.TryGetValue(key, out Target keyTarget) && target.File == keyTarget.File)
                            type = ReferenceLinkType.Self;
                    }
                }

                // !EFW - Redirect enumeration fields to the containing enumerated type so that we
                // get a valid link target.  Enum fields don't have a topic to themselves.
                if(type != ReferenceLinkType.None && type != ReferenceLinkType.Self && type != ReferenceLinkType.Local &&
                  targetId.StartsWith("F:", StringComparison.Ordinal))
                {
                    if(target is MemberTarget member && member.ContainingType is SimpleTypeReference typeRef &&
                      targets[typeRef.Id] is EnumerationTarget)
                    {
                        targetId = typeRef.Id;
                    }
                }

                // Get the URL for the member ID if needed
                if(type == ReferenceLinkType.Msdn)
                {
                    if(this.UrlResolver != null && !this.UrlResolver.IsDisabled)
                    {
                        memberUrl = this.UrlResolver.ResolveUrlForId(targetId);

                        // The Microsoft Docs cross reference service doesn't appear to support overloads links
                        // so try to convert to one of the overload members and use that as the target instead.
                        if(String.IsNullOrWhiteSpace(memberUrl) && !this.UrlResolver.IsDisabled &&
                          targetId.StartsWith("Overload:", StringComparison.Ordinal) ||
                          targetId.StartsWith("O:", StringComparison.Ordinal))
                        {
                            string methodTargetId = "M:" + targetId.Substring(targetId.IndexOf(':') + 1);
                            methodTargetId = targets.Keys.FirstOrDefault(k => k.StartsWith(methodTargetId,
                                StringComparison.Ordinal));

                            if(methodTargetId != null)
                            {
                                memberUrl = this.UrlResolver.ResolveUrlForId(methodTargetId);

                                if(memberUrl != null)
                                    this.UrlResolver.CachedUrls[targetId] = memberUrl;
                            }
                        }

                        if(String.IsNullOrWhiteSpace(memberUrl))
                        {
                            // If the web service failed, report the reason
                            if(this.UrlResolver.IsDisabled)
                            {
                                this.WriteMessage(key, MessageLevel.Warn, "Member ID URL resolver service failed.  " +
                                    "No further look ups will be performed for this build.\r\nReason: {0}",
                                    this.UrlResolver.DisabledReason);
                            }
                            else
                                this.WriteMessage(key, MessageLevel.Warn, "Member ID URL not found for target '{0}'.",
                                    targetId);

                            type = ReferenceLinkType.None;
                        }
                    }
                    else
                        type = ReferenceLinkType.None;
                }

                // Write opening link tag and target info
                XmlWriter writer = linkNode.InsertAfter();

                switch(type)
                {
                    case ReferenceLinkType.None:
                        writer.WriteStartElement("span");

                        // If the link was intentionally suppressed, write it out as an identifier (i.e. links
                        // in the syntax section).
                        if(link.RenderAsLink)
                            writer.WriteAttributeString("class", "noLink");
                        else
                            writer.WriteAttributeString("class", "identifier");
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

                    case ReferenceLinkType.Msdn:
                        writer.WriteStartElement("a");
                        writer.WriteAttributeString("href", memberUrl);
                        writer.WriteAttributeString("target", linkTarget);
                        writer.WriteAttributeString("rel", "noopener noreferrer");
                        break;

                    case ReferenceLinkType.Id:
                        writer.WriteStartElement("a");
                        writer.WriteAttributeString("href", ("ms-xhelp:///?Id=" + targetId).Replace("#", "%23"));
                        break;
                }

                // Write the link text
                if(String.IsNullOrWhiteSpace(link.DisplayTarget))
                {
                    if(link.Contents == null)
                    {
                        if(target != null)
                            resolver.WriteTarget(target, options, writer);
                        else
                        {
                            Reference reference = TextReferenceUtilities.CreateReference(targetId);

                            if(reference is InvalidReference)
                                this.WriteMessage(key, MessageLevel.Warn,
                                    "Invalid reference link target '{0}'.", targetId);

                            resolver.WriteReference(reference, options, writer);
                        }
                    }
                    else
                    {
                        do
                        {
                            link.Contents.WriteSubtree(writer);

                        } while(link.Contents.MoveToNext());
                    }
                }
                else
                {
                    if(link.DisplayTarget.Equals("content", StringComparison.OrdinalIgnoreCase)  &&
                        link.Contents != null)
                    {
                        // Use the contents as an XML representation of the display target
                        Reference reference = XmlTargetDictionaryUtilities.CreateReference(link.Contents);
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
                            XmlWriterSettings settings = new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Fragment };

                            using(XmlWriter xmlStore = XmlWriter.Create(textStore, settings))
                            {
                                if(target != null)
                                    resolver.WriteTarget(target, options, xmlStore);
                                else
                                {
                                    Reference reference = TextReferenceUtilities.CreateReference(targetId);
                                    resolver.WriteReference(reference, options, xmlStore);
                                }
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
                        Reference extMethodReference = XmlTargetDictionaryUtilities.CreateExtensionMethodReference(link.Contents);
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
        /// This is overridden to save the updated cache information and dispose of target information
        /// </summary>
        /// <param name="disposing">Pass true to dispose of the managed and unmanaged resources or false to just
        /// dispose of the unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if(disposing && this.UrlResolver != null && !this.UrlResolver.IsDisposed)
            {
                this.UpdateUrlCache();
                this.UrlResolver.Dispose();
                this.UrlResolver = null;
            }

            if(targets != null)
            {
                targets.Dispose();

                foreach(var td in sharedTargets.Values.ToList())
                    if(td.IsDisposed)
                        sharedTargets.Remove(td.DictionaryId);
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to create a member ID URL resolver for the component to use in looking up help website
        /// URLs.
        /// </summary>
        /// <param name="configuration">The component configuration</param>
        /// <returns>An <see cref="IMemberIdUrlResolver"/> instance</returns>
        /// <remarks>This can be overridden in derived classes to provide persistent caches with backing stores
        /// other than the default dictionary serialized to a binary file.  It also allows sharing the cache
        /// across instances by placing it in the <see cref="IBuildAssembler.Data"/> dictionary using the key
        /// name <see cref="SharedMemberUrlCacheId"/>.
        /// 
        /// <para>If overridden, the <see cref="UpdateUrlCache"/> method should also be overridden to
        /// persist changes to the cache if needed.</para></remarks>
        /// <overloads>There are two overloads for this method</overloads>
        protected virtual IMemberIdUrlResolver CreateMemberIdResolver(XPathNavigator configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            IMemberIdUrlResolver newResolver;
            IDictionary<string, string> cache = null;

            if(this.BuildAssembler.Data.TryGetValue(SharedMemberUrlCacheId, out object cacheValue))
                cache = cacheValue as IDictionary<string, string>;

            // If the shared cache already exists, return an instance that uses it.  It is assumed that all
            // subsequent instances will use the same cache.
            if(cache != null)
                return this.CreateMemberIdResolver(cache, true);

            // If a <cache> element is not specified, we'll use the standard resolver without a persistent cache.
            // We will share it across all instances though.
            XPathNavigator node = configuration.SelectSingleNode("memberIdUrlCache");

            if(node == null)
                newResolver = this.CreateMemberIdResolver(null, false);
            else
            {
                // Keep the filename.  If we own it, we'll update the cache file when disposed.
                memberIdUrlCacheFile = node.GetAttribute("path", String.Empty);

                if(String.IsNullOrWhiteSpace(memberIdUrlCacheFile))
                    this.WriteMessage(MessageLevel.Error, "You must specify a path attribute value on the " +
                        "memberIdUrlCache element.");

                // Create the folder if it doesn't exist
                memberIdUrlCacheFile = Path.GetFullPath(Environment.ExpandEnvironmentVariables(memberIdUrlCacheFile));
                string path = Path.GetDirectoryName(memberIdUrlCacheFile);

                if(!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                // Load the cache if it exists
                if(!File.Exists(memberIdUrlCacheFile))
                {
                    // Logged as a diagnostic message since looking up all IDs can significantly slow the build
                    this.WriteMessage(MessageLevel.Diagnostic, "The member ID URL cache '" + memberIdUrlCacheFile +
                        "' does not exist yet.  All IDs will be looked up in this build which will slow it down.");

                    newResolver = this.CreateMemberIdResolver(null, false);
                }
                else
                    using(FileStream fs = new FileStream(memberIdUrlCacheFile, FileMode.Open, FileAccess.Read,
                      FileShare.Read))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        newResolver = this.CreateMemberIdResolver((IDictionary<string, string>)bf.Deserialize(fs), false);

                        this.WriteMessage(MessageLevel.Info, "Loaded {0} cached member ID URL entries",
                            newResolver.CachedUrls.Count);
                    }
            }

            this.BuildAssembler.Data[SharedMemberUrlCacheId] = newResolver.CachedUrls;

            return newResolver;
        }

        /// <summary>
        /// This is used to create a member ID URL resolver for the component to use in looking up help website
        /// URLs.
        /// </summary>
        /// <param name="cache">A cache of existing URLs or null to use the default cache.</param>
        /// <param name="isShared">True if the cache is shared, false if not.  If not shared, the cache will
        /// be disposed of when the instance is disposed of.  If <paramref name="cache"/> is null, this parameter
        /// is ignored.</param>
        /// <returns>An <see cref="IMemberIdUrlResolver"/> instance</returns>
        protected virtual IMemberIdUrlResolver CreateMemberIdResolver(IDictionary<string, string> cache, bool isShared)
        {
            if(cache != null)
                return new MicrosoftDocsXRefServiceResolver(cache, isShared);

            return new MicrosoftDocsXRefServiceResolver();
        }

        /// <summary>
        /// This is used to update the URL cache file
        /// </summary>
        /// <remarks>The default implementation serializes the standard dictionary to a file using binary
        /// serialization if new entries were added and it loaded the cache file.</remarks>
        public virtual void UpdateUrlCache()
        {
            if(memberIdUrlCacheFile != null && this.UrlResolver != null && this.UrlResolver.CacheItemsAdded != 0)
            {
                this.WriteMessage(MessageLevel.Info, "Member ID URL cache updated.  Saving new information to " +
                    memberIdUrlCacheFile);

                try
                {
                    using(FileStream fs = new FileStream(memberIdUrlCacheFile, FileMode.Create, FileAccess.ReadWrite,
                      FileShare.None))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(fs, this.UrlResolver.CachedUrls);
                    }

                    this.WriteMessage(MessageLevel.Diagnostic, "{0} entries added to the member ID URL cache.  " +
                        "New cache size: {1} entries", this.UrlResolver.CacheItemsAdded,
                        this.UrlResolver.CachedUrls.Count);
                }
                catch(IOException ex)
                {
                    // Most likely it couldn't access the file.  We'll issue a warning but will continue with
                    // the build.
                    this.WriteMessage(MessageLevel.Warn, "Unable to create member ID URL cache file.  It will " +
                        "be created or updated on a subsequent build: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// This is used to create a <see cref="TargetDictionary"/> used to store reference link targets
        /// </summary>
        /// <param name="configuration">The configuration element for the target dictionary</param>
        /// <returns>A default <see cref="InMemoryTargetDictionary"/> instance containing the reference link
        /// targets</returns>
        /// <remarks>This can be overridden in derived classes to provide persistent caches with backing stores
        /// other than the default <see cref="Dictionary{TKey, TValue}"/></remarks>.
        public virtual TargetDictionary CreateTargetDictionary(XPathNavigator configuration)
        {
            TargetDictionary d = null;

            try
            {
                d = new InMemoryTargetDictionary(this, configuration);
            }
            catch(Exception ex)
            {
                this.WriteMessage(MessageLevel.Error, BuildComponentUtilities.GetExceptionMessage(ex));
            }

            return d;
        }
        #endregion
    }
}
