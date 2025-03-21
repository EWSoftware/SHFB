//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : GenerateInheritedDocs.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/20/2025
// Note    : Copyright 2008-2025, Eric Woodruff, All rights reserved
//
// This file contains the build task that scans XML comments files for <inheritdoc /> tags and produces a new
// XML comments file containing the inherited documentation for use by Sandcastle.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/27/2008  EFW  Created the code
// 07/14/2008  EFW  Added support for running as an MSBuild task
// 01/23/2012  EFW  Added support for auto-documenting attached properties and events.  Also added support for
//                  utilizing AttachedPropertyComments and AttachedEventComments elements to provide comments
//                  for attached properties and events that differ from the comments on the backing fields.
// 09/12/2024  EFW  Moved the code into the build engine and removed the task
//===============================================================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.Utils.InheritedDocumentation
{
    /// <summary>
    /// This class scans XML comments files for <strong>&lt;inheritdoc /&gt;</strong> tags and produces a new XML
    /// comments file containing the inherited documentation for use by Sandcastle.
    /// </summary>
    public class GenerateInheritedDocs
    {
        #region Private data members
        //=====================================================================

        private readonly BuildProcess buildProcess;

        private readonly ReflectionFiles reflectionFiles;
        private readonly IndexedCommentsCache commentsCache;
        private readonly ConcurrentBag<XPathNavigator> commentsFiles;

        private readonly XmlDocument inheritedDocs;
        private readonly XmlNode docMemberList;
        private readonly Stack<string> memberStack;

        // For base type/interface inheritance, these classes should be used as a last resort in the given order.
        // Typically, we don't want to inherit from these but from something more useful in the hierarchy.
        private readonly List<string> lastResortInheritableTypes = new List<string>
        {
            "T:System.IDisposable",
            "T:System.Object"
        };
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the reflection file information
        /// </summary>
        public ReflectionFiles ReflectionFiles => reflectionFiles;
        
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildProcess">The build process to use</param>
        public GenerateInheritedDocs(BuildProcess buildProcess)
        {
            this.buildProcess = buildProcess ?? throw new ArgumentNullException(nameof(buildProcess));

            commentsCache = new IndexedCommentsCache(100);
            commentsCache.ReportWarning += (s, e) => buildProcess.ReportProgress(e.Message);

            reflectionFiles = new ReflectionFiles();

            commentsFiles = new ConcurrentBag<XPathNavigator>();
            inheritedDocs = new XmlDocument { PreserveWhitespace = true };
            inheritedDocs.LoadXml(@"<doc>
  <assembly>
    <name>_InheritedDocs_</name>
  </assembly>
  <members>
  </members>
</doc>");
            docMemberList = inheritedDocs.SelectSingleNode("doc/members");
            memberStack = new Stack<string>();
        }
        #endregion

        #region Execute methods
        //=====================================================================

        /// <summary>
        /// This is used to execute the task and generate the inherited documentation
        /// </summary>
        /// <returns>True on success or false on failure.</returns>
        public bool Execute()
        {
            bool success = true;

            try
            {
                buildProcess.ReportProgress("Reflection information will be retrieved from '{0}'",
                    buildProcess.ReflectionInfoFilename);
                reflectionFiles.AddReflectionFile(buildProcess.ReflectionInfoFilename);

                var dataSet = buildProcess.FrameworkReflectionData;
                string folder, wildcard;

                // Index the framework comments files
                foreach(string location in dataSet.CommentsFileLocations(buildProcess.CurrentProject.Language))
                {
                    folder = Path.GetDirectoryName(location);
                    wildcard = Path.GetFileName(location);

                    buildProcess.ReportProgress("Indexing {0}", location);
                    commentsCache.IndexCommentsFiles(folder, wildcard, false, null);
                }

                // Index the project comments files.  These will be scanned for inherited documentation
                foreach(XmlCommentsFile f in buildProcess.CommentsFiles.Where(cf => cf.IsValid))
                {
                    string path = f.SourcePath;

                    // The path is not altered if the file is already in or under the working folder (i.e. files
                    // added by plug-ins).
                    if(!f.SourcePath.StartsWith(buildProcess.WorkingFolder, StringComparison.OrdinalIgnoreCase))
                        path = Path.Combine(buildProcess.WorkingFolder, Path.GetFileName(path));

                    folder = Path.GetDirectoryName(path);
                    wildcard = Path.GetFileName(path);

                    buildProcess.ReportProgress("Indexing {0}", path);
                    commentsCache.IndexCommentsFiles(folder, wildcard, false, commentsFiles);
                }

                if(commentsCache.FilesIndexed == 0 || commentsCache.IndexCount == 0)
                {
                    throw new InvalidOperationException("No comments files were specified or they did not " +
                        "contain valid information to index");
                }

                if(commentsFiles.IsEmpty)
                {
                    throw new InvalidOperationException("No comments files were specified to scan for " +
                        "<inheritdoc /> tags.");
                }

                buildProcess.ReportProgress("Indexed {0} members in {1} file(s).  {2} file(s) to scan for <inheritdoc /> tags.",
                    commentsCache.IndexCount, commentsCache.FilesIndexed, commentsFiles.Count);

                ScanCommentsFiles();

                string inheritedDocsFile = Path.Combine(buildProcess.WorkingFolder, "_InheritedDocs_.xml");

                inheritedDocs.Save(inheritedDocsFile);

                buildProcess.ReportProgress("Documentation merged successfully.  Merged comments were saved in '{0}'.",
                    inheritedDocsFile);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                success = false;

                buildProcess.ReportError(buildProcess.CurrentBuildStep, "GID0001",
                    " Unexpected error while generating inherited documentation:\r\n{0}", ex.ToString());
            }

            return success;
        }
        #endregion

        #region Root level documentation inheritance methods
        //=====================================================================

        /// <summary>
        /// This scans the XML comments files for &lt;inheritdoc /&gt; tags and adds the inherited documentation
        /// </summary>
        private void ScanCommentsFiles()
        {
            XPathNavigator apiId, apiField, fieldComments, attachedComments;
            XmlDocumentFragment node;
            Dictionary<string, XPathNavigator> members = new Dictionary<string, XPathNavigator>();
            string name;

            // Get a list of all unique members containing an <inheritdoc /> tag.  This will include entries with
            // occurrences at the root level as well as those nested within other tags.  Root level stuff will
            // get handled first followed by any nested tags.
            foreach(XPathNavigator file in commentsFiles)
            {
                foreach(XPathNavigator tag in file.Select("//inheritdoc"))
                {
                    while(tag.Name != "member")
                        tag.MoveToParent();

                    name = tag.GetAttribute("name", String.Empty);

                    // Ignore members that are not going to be documented.  This avoids a lot of GID0003 warnings
                    // for missing comments on undocumented members.
                    apiId = reflectionFiles.SelectSingleNode("api[@id='" + name + "']");

                    if(apiId != null && !members.ContainsKey(name))
                        members.Add(name, tag);
                }
            }

            // For each one, create a new XML node that can be edited
            foreach(XPathNavigator nav in members.Values)
            {
                node = inheritedDocs.CreateDocumentFragment();
                node.InnerXml = nav.OuterXml;
                docMemberList.AppendChild(node);
            }

            // Add explicit interface implementations that do not have member comments already
            foreach(XPathNavigator api in reflectionFiles.Select("api[proceduredata/@eii='true']/@id"))
            {
                if(commentsCache[api.Value] == null && !members.ContainsKey(api.Value))
                {
                    node = inheritedDocs.CreateDocumentFragment();

                    // The C++ compiler can generate invalid IDs.  If found, the invalid characters are encoded.
                    node.InnerXml = String.Format(CultureInfo.InvariantCulture,
                        "<member name=\"{0}\"><inheritdoc /></member>",
                        (api.Value.IndexOf('<') != -1) ? WebUtility.HtmlEncode(api.Value) : api.Value);

                    docMemberList.AppendChild(node);
                }
            }

            // Add attached property and attached event entries that inherit comments from their
            // related fields that do not have member comments already.
            foreach(XPathNavigator api in reflectionFiles.Select(
              "api[apidata/@subsubgroup='attachedEvent' or apidata/@subsubgroup='attachedProperty']"))
            {
                apiId = api.SelectSingleNode("@id");

                if(commentsCache[apiId.Value] == null && !members.ContainsKey(apiId.Value))
                {
                    if(apiId.Value[0] == 'E')
                        apiField = api.SelectSingleNode("attachedeventdata/field/member/@api");
                    else
                        apiField = api.SelectSingleNode("attachedpropertydata/field/member/@api");

                    if(apiField != null)
                    {
                        fieldComments = commentsCache[apiField.Value];

                        if(fieldComments == null)
                            attachedComments = null;
                        else
                        {
                            if(apiId.Value[0] == 'E')
                                attachedComments = fieldComments.SelectSingleNode("AttachedEventComments");
                            else
                                attachedComments = fieldComments.SelectSingleNode("AttachedPropertyComments");
                        }

                        if(fieldComments == null || attachedComments == null)
                        {
                            // If no attached property/events comments are defined, inherit the field's comments
                            // so that we don't get a "missing comments" error.  However, if comments do not
                            // exist for the field, don't bother and let it generate a "missing comments" error
                            // if needed.  This prevents false GID0002 errors for inherited attached events and
                            // properties that never actually appear in the documentation.
                            if(docMemberList.SelectSingleNode($"member[@name='{apiField.Value}']") != null ||
                              commentsCache[apiField.Value] != null)
                            {
                                node = inheritedDocs.CreateDocumentFragment();
                                node.InnerXml = String.Format(CultureInfo.InvariantCulture,
                                    "<member name=\"{0}\"><inheritdoc cref=\"{1}\" /></member>", apiId.Value,
                                    apiField.Value);
                                docMemberList.AppendChild(node);
                            }
                        }
                        else
                        {
                            // Create an entry for the field comments without the attached comments
                            node = inheritedDocs.CreateDocumentFragment();
                            node.InnerXml = String.Format(CultureInfo.InvariantCulture,
                                "<member name=\"{0}\">{1}</member>", apiField.Value, fieldComments.InnerXml);
                            node.SelectSingleNode("member").RemoveChild(
                                node.SelectSingleNode("member/" + attachedComments.Name));
                            docMemberList.AppendChild(node);

                            // Add the comments for the attached member
                            node = inheritedDocs.CreateDocumentFragment();
                            node.InnerXml = String.Format(CultureInfo.InvariantCulture,
                                "<member name=\"{0}\">{1}</member>", apiId.Value, attachedComments.InnerXml);
                            docMemberList.AppendChild(node);
                        }
                    }
                }
            }

            buildProcess.ReportProgress("Merging inherited documentation...\r\n");

            // Inherit the root level documentation
            foreach(XmlNode n in docMemberList.SelectNodes("*"))
                InheritDocumentation(n);
        }

        /// <summary>
        /// This is used to generate the inherited documentation for the given member.  Only tags at the root
        /// level are processed here.
        /// </summary>
        /// <param name="member">The member for which to inherit documentation</param>
        /// <remarks>This will recursively expand documentation if a base member's comments are present in the
        /// generation list.</remarks>
        private void InheritDocumentation(XmlNode member)
        {
            List<string> sources = new List<string>();
            XPathNavigator apiNode, baseMember;
            XmlNode copyMember;
            XmlAttribute cref, filter;
            string name, ctorName, baseMemberName;
            bool commentsFound;
            int idx, namePosition;

            name = member.SelectSingleNode("@name").Value;

            // Check for a circular reference.  If found, issue a warning and return the comments as-is.
            // Typically, this is a problem but it may be legitimate if someone inherits comments from another
            // element within the same member comments (i.e. inheriting a span or paragraph from the summary
            // in the remarks element).
            if(memberStack.Contains(name))
            {
                StringBuilder sb = new StringBuilder("Circular reference detected: ");

                idx = memberStack.Count;
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}: {1}", idx + 1, name);

                foreach(var m in memberStack.ToArray())
                    sb.AppendFormat(CultureInfo.InvariantCulture, " <-- {0}: {1}", idx--, m);

                // NOTE: Use an explicit conversion for the string builder or it picks the wrong overload here
                buildProcess.ReportWarning("GID0009", sb.ToString());
                return;
            }

            memberStack.Push(name);

            // Handle root level stuff first
            foreach(XmlNode inheritTag in member.SelectNodes("inheritdoc"))
            {
                // Get rid of the tag
                inheritTag.ParentNode.RemoveChild(inheritTag);

                // Apply a selection filter?
                filter = inheritTag.Attributes["select"];

                if(filter != null)
                {
                    // Deprecated as of Nov 2019 to match Visual Studio 2019 usage
                    buildProcess.ReportWarning("GID0010", "'{0}' uses the deprecated inheritdoc 'select' " +
                        "attribute.  Use the equivalent 'path' attribute instead.", name);
                }
                else
                    filter = inheritTag.Attributes["path"];

                // Inherit from a member other than the base?
                cref = inheritTag.Attributes["cref"];

                // Is the base explicitly specified?
                if(cref != null)
                {
                    // Is it in the list of members for which to generate documentation?
                    copyMember = docMemberList.SelectSingleNode("member[@name='" + cref.Value + "']");

                    // If so, expand its tags now and use it.  If not, try to get it from the comments cache.
                    if(copyMember != null)
                    {
                        InheritDocumentation(copyMember);
                        baseMember = copyMember.CreateNavigator();
                    }
                    else
                        baseMember = commentsCache[cref.Value];

                    if(baseMember != null)
                        MergeComments(baseMember, member.CreateNavigator(), (filter == null) ? "*" : filter.Value);
                    else
                    {
                        buildProcess.ReportWarning("GID0002", "No comments found for cref '{0}' on member " +
                            "'{1}'", cref.Value, name);
                    }

                    continue;
                }

                // Get a list of sources from which to merge comments
                commentsFound = false;
                sources.Clear();
                apiNode = reflectionFiles.SelectSingleNode("api[@id='" + name + "']");

                if(apiNode == null)
                {
                    buildProcess.ReportWarning("GID0003", "Unable to locate API ID '{0}'", name);
                    continue;
                }

                if(name[0] == 'T')
                {
                    // Give precedence to base types
                    foreach(XPathNavigator baseType in apiNode.Select("family/ancestors/type/@api"))
                        sources.Add(baseType.Value);

                    // Then hit the interface implementations
                    foreach(XPathNavigator baseType in apiNode.Select("implements/type/@api"))
                        sources.Add(baseType.Value);

                    // Move last resort types to the end of the list in the given order since we typically don't
                    // want to inherit from these but something higher up in the hierarchy that may have come
                    // alphabetically later or a type may have taken precedence over an interface.  For example,
                    // a user-defined type only inherits from a user-defined interface.  Its base type of
                    // System.Object would take precedence over the user-defined interface.  This fixes it so
                    // that the user-defined interface is used first.  In the unlikely event that this still
                    // picks the wrong type, an explicit cref attribute can be used to specify the proper one.
                    foreach(string lastResortType in lastResortInheritableTypes)
                    {
                        sources.Remove(lastResortType);
                        sources.Add(lastResortType);
                    }
                }
                else
                {
                    // Constructors aren't like normal overrides.  They can call base copies that take the
                    // same arguments but the overrides aren't listed in the reflection info.  We'll just
                    // search all base types for a matching signature.
                    namePosition = name.IndexOf("#ctor", StringComparison.Ordinal);

                    if(namePosition == -1)
                        namePosition = name.IndexOf("#cctor", StringComparison.Ordinal);

                    if(namePosition != -1)
                    {
                        ctorName = name.Substring(namePosition);
                        baseMember = apiNode.SelectSingleNode("containers/type/@api");
                        apiNode = reflectionFiles.SelectSingleNode("api[@id='" + baseMember.Value + "']");

                        foreach(XPathNavigator baseType in apiNode.Select("family/ancestors/type/@api"))
                        {
                            baseMemberName = String.Format(CultureInfo.InvariantCulture, "M:{0}.{1}",
                                baseType.Value.Substring(2), ctorName);

                            if(commentsCache[baseMemberName] != null)
                            {
                                sources.Add(baseMemberName);
                                break;
                            }
                        }
                    }
                    else
                    {
                        // Give precedence to an override
                        baseMember = apiNode.SelectSingleNode("overrides/member/@api");

                        if(baseMember != null)
                            sources.Add(baseMember.Value);

                        // Then hit implementations.  There should only be one but just in case, look for more.
                        foreach(XPathNavigator baseType in apiNode.Select("implements/member/@api"))
                            sources.Add(baseType.Value);
                    }
                }

                // Inherit documentation from each base member
                foreach(string baseName in sources)
                {
                    // Is it in the list of members for which to generate documentation?
                    copyMember = docMemberList.SelectSingleNode("member[@name='" + baseName + "']");

                    // If so, expand its tags now and use it.  If not, try to get it from the comments cache.
                    if(copyMember != null)
                    {
                        InheritDocumentation(copyMember);
                        baseMember = copyMember.CreateNavigator();
                    }
                    else
                        baseMember = commentsCache[baseName];

                    if(baseMember != null)
                    {
                        MergeComments(baseMember, member.CreateNavigator(), (filter == null) ? "*" : filter.Value);
                        commentsFound = true;
                    }
                }

                if(!commentsFound)
                    buildProcess.ReportWarning("GID0004", "No comments found for member '{0}'", name);
            }

            // Now merge documentation nested within other tags
            InheritNestedDocumentation(member);

            memberStack.Pop();
        }

        /// <summary>
        /// Merge the XML comments from one member into another
        /// </summary>
        /// <param name="fromMember">The member from which to merge comments</param>
        /// <param name="toMember">The member into which the comments merged</param>
        /// <param name="filter">The selection filter</param>
        private void MergeComments(XPathNavigator fromMember, XPathNavigator toMember, string filter)
        {
            XPathNavigator duplicate;
            string[] dupAttrs = new string[] { "cref", "href", "name", "vref", "xref" };
            string attrValue;

            if(String.IsNullOrEmpty(filter))
                filter = "*";

            try
            {
                // Merge based on the element name
                foreach(XPathNavigator element in fromMember.Select(filter))
                    switch(element.Name)
                    {
                        case "example":     // Ignore if already present
                        case "exclude":
                        case "filterpriority":
                        case "preliminary":
                        case "summary":
                        case "remarks":
                        case "returns":
                        case "threadsafety":
                        case "value":
                            if(toMember.SelectSingleNode(element.Name) == null)
                                toMember.AppendChild(element);
                            break;

                        case "overloads":
                        case "revisionHistory":
                            // Ignore completely.  We only need one.
                            break;

                        default:
                            if(!element.HasAttributes)
                                toMember.AppendChild(element);
                            else
                            {
                                // Ignore if there is a duplicate by attribute
                                // name and value.
                                duplicate = null;

                                foreach(string attrName in dupAttrs)
                                {
                                    attrValue = element.GetAttribute(attrName, String.Empty);

                                    if(!String.IsNullOrEmpty(attrValue))
                                    {
                                        duplicate = toMember.SelectSingleNode(String.Format(CultureInfo.InvariantCulture,
                                            "{0}[@{1}='{2}']", element.Name, attrName, attrValue));

                                        if(duplicate != null)
                                            break;
                                    }
                                }

                                if(duplicate == null)
                                    toMember.AppendChild(element);
                            }
                            break;
                    }
            }
            catch(XPathException xpe)
            {
                // If the XPath query causes an error, include the member ID and the expression to help
                // locate the problem.
                throw new XPathException("An XPath exception occurred inheriting comments for:\r\n" +
                    $"    Member ID: {memberStack.Peek()}\r\n   Expression: {filter}\r\n", xpe);
            }
        }
        #endregion

        #region Nested documentation inheritance methods
        //=====================================================================

        /// <summary>
        /// This is used to generate the inherited documentation nested within other documentation elements of
        /// the given member.  Only nested tags are processed here.
        /// </summary>
        /// <param name="member">The member for which to inherit documentation</param>
        /// <remarks>Unlike root level elements, if the inherited nested documentation contains <c>inheritdoc</c>
        /// tags, they will not be handled recursively.  Note that common elements such as <c>param</c> are
        /// inherited automatically at the root level so there's no need to use <c>inheritdoc</c> within them
        /// unless you want to include something specific using a filter.</remarks>
        private void InheritNestedDocumentation(XmlNode member)
        {
            StringBuilder sb = new StringBuilder(256);
            XPathNavigator baseMember;
            XmlNode copyMember;
            XmlAttribute cref, filter, autoFilter;
            string name;

            foreach(XmlNode inheritTag in member.SelectNodes(".//inheritdoc"))
            {
                copyMember = inheritTag;
                sb.Remove(0, sb.Length);

                // Build the filter
                while(copyMember.ParentNode.Name != "member")
                {
                    copyMember = copyMember.ParentNode;

                    if(sb.Length != 0)
                        sb.Insert(0, '/');

                    sb.Insert(0, copyMember.Name);
                }

                name = copyMember.ParentNode.Attributes["name"].Value;

                // Apply a selection filter?
                filter = inheritTag.Attributes["select"];

                if(filter != null)
                {
                    // Deprecated as of Nov 2019 to match Visual Studio 2019 usage
                    buildProcess.ReportWarning("GID0010", "'{0}' uses the deprecated inheritdoc 'select' " +
                        "attribute.  Use the equivalent 'path' attribute instead.", name);
                }
                else
                    filter = inheritTag.Attributes["path"];

                if(filter != null)
                {
                    if(filter.Value[0] != '/')
                        sb.AppendFormat(CultureInfo.InvariantCulture, "/{0}", filter.Value);
                    else
                    {
                        // If the filter is rooted, ignore the parent tags and use it as is.  This allows
                        // nesting inheritdoc within other tags that you don't want automatically included in
                        // the filter.
                        sb.Remove(0, sb.Length);
                        sb.Append(filter.Value.Substring(1));
                    }
                }
                else
                {
                    if(inheritTag.ParentNode.Name != "member")
                    {
                        // If nested within an element that has a cref or name attribute, apply that as an
                        // automatic filter.  If not, we may end up pulling in comments from unrelated elements
                        // such as other parameters.
                        autoFilter = inheritTag.ParentNode.Attributes["cref"];

                        if(autoFilter == null)
                            autoFilter = inheritTag.ParentNode.Attributes["name"];

                        if(autoFilter != null)
                            sb.AppendFormat(CultureInfo.InvariantCulture, "[@{0}=\"{1}\"]", autoFilter.Name, autoFilter.Value);
                    }
                }

                // Inherit from a member other than the base?
                cref = inheritTag.Attributes["cref"];

                baseMember = LocateBaseDocumentation(name, cref?.Value);

                if(baseMember != null && sb.Length != 0)
                {
                    var content = inheritedDocs.CreateDocumentFragment();

                    // Merge the content
                    try
                    {
                        foreach(XPathNavigator element in baseMember.Select(sb.ToString()))
                        {
                            var newNode = inheritedDocs.CreateDocumentFragment();

                            // If there's no filter, we don't want the tag
                            if(filter != null)
                                newNode.InnerXml = element.OuterXml;
                            else
                                newNode.InnerXml = element.InnerXml;

                            content.AppendChild(newNode);
                        }
                    }
                    catch(XPathException xpe)
                    {
                        // If the XPath query causes an error, include the member ID and the expression to help
                        // locate the problem.
                        throw new XPathException("An XPath exception occurred inheriting comments for:\r\n" +
                            $"    Member ID: {name}\r\n   Expression: {sb}\r\n", xpe);
                    }

                    // Replace the tag with the content
                    inheritTag.ParentNode.ReplaceChild(content, inheritTag);
                }
                else
                {
                    inheritTag.ParentNode.RemoveChild(inheritTag);

                    // If this is empty, we've just tried to inherit documentation from a base member in a
                    // reference assembly that wasn't included for documentation.  The Additional Reference
                    // Links plug-in can help resolve this issue.
                    if(sb.Length == 0)
                        sb.Append("[Base member from reference assembly not included for documentation]");

                    if(cref != null)
                    {
                        buildProcess.ReportWarning("GID0005", "No comments found for cref '{0}' on member " +
                            "'{1}' in '{2}'", cref.Value, name, sb);
                    }
                    else
                    {
                        buildProcess.ReportWarning("GID0006", "No comments found for member '{0}' in '{1}'",
                            name, sb);
                    }
                }
            }
        }

        /// <summary>
        /// Locate and merge the documentation from the base member(s)
        /// </summary>
        /// <param name="name">The member name</param>
        /// <param name="cref">An optional member name from which to inherit
        /// the documentation.</param>
        private XPathNavigator LocateBaseDocumentation(string name, string cref)
        {
            List<string> sources = new List<string>();
            XPathNavigator apiNode, baseMember;
            XmlNode copyMember;
            string ctorName, baseMemberName;
            int namePosition;

            // Is the base explicitly specified?
            if(cref != null)
            {
                // Is it in the list of members for which to generate
                // documentation?
                copyMember = docMemberList.SelectSingleNode("member[@name='" + cref + "']");

                // If so, expand its tags now and use it.  If not, try
                // to get it from the comments cache.
                if(copyMember != null)
                {
                    InheritDocumentation(copyMember);
                    baseMember = copyMember.CreateNavigator();
                }
                else
                    baseMember = commentsCache[cref];

                return baseMember;
            }

            // Get a list of sources from which to merge comments
            apiNode = reflectionFiles.SelectSingleNode("api[@id='" + name + "']");

            if(apiNode == null)
            {
                buildProcess.ReportWarning("GID0003", "Unable to locate API ID '{0}'", name);
                return null;
            }

            if(name[0] == 'T')
            {
                // Give precedence to base types
                foreach(XPathNavigator baseType in apiNode.Select("family/ancestors/type/@api"))
                    sources.Add(baseType.Value);

                // Then hit the interface implementations
                foreach(XPathNavigator baseType in apiNode.Select("implements/type/@api"))
                    sources.Add(baseType.Value);
            }
            else
            {
                // Constructors aren't like normal overrides.  They can call base copies that take the same
                // arguments but the overrides aren't listed in the reflection info.  We'll just search all
                // base types for a matching signature.
                namePosition = name.IndexOf("#ctor", StringComparison.Ordinal);

                if(namePosition == -1)
                    namePosition = name.IndexOf("#cctor", StringComparison.Ordinal);

                if(namePosition != -1)
                {
                    ctorName = name.Substring(namePosition);
                    baseMember = apiNode.SelectSingleNode("containers/type/@api");
                    apiNode = reflectionFiles.SelectSingleNode("api[@id='" + baseMember.Value + "']");

                    foreach(XPathNavigator baseType in apiNode.Select("family/ancestors/type/@api"))
                    {
                        baseMemberName = String.Format(CultureInfo.InvariantCulture, "M:{0}.{1}",
                            baseType.Value.Substring(2), ctorName);

                        if(commentsCache[baseMemberName] != null)
                        {
                            sources.Add(baseMemberName);
                            break;
                        }
                    }
                }
                else
                {
                    // Give precedence to an override
                    baseMember = apiNode.SelectSingleNode("overrides/member/@api");

                    if(baseMember != null)
                        sources.Add(baseMember.Value);

                    // Then hit implementations.  There should only be one but just in case, look for more.
                    foreach(XPathNavigator baseType in apiNode.Select("implements/member/@api"))
                        sources.Add(baseType.Value);
                }
            }

            baseMember = null;

            // Find the first member with comments
            foreach(string baseName in sources)
            {
                // Is it in the list of members for which to generate documentation?
                copyMember = docMemberList.SelectSingleNode("member[@name='" + baseName + "']");

                // If so, expand its tags now and use it.  If not, try to get it from the comments cache.
                if(copyMember != null)
                {
                    InheritDocumentation(copyMember);
                    baseMember = copyMember.CreateNavigator();
                }
                else
                    baseMember = commentsCache[baseName];

                if(baseMember != null)
                    break;
            }

            return baseMember;
        }
        #endregion
    }
}
