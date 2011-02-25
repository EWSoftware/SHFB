//=============================================================================
// System  : Sandcastle Help File Builder
// File    : GenerateInheritedDocs.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/27/2010
// Note    : Copyright 2008-2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the console mode tool that scans XML comments files for
// <inheritdoc /> tags and produces a new XML comments file containing the
// inherited documentation for use by Sandcastle.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.5  02/27/2008  EFW  Created the code
// 1.8.0.0  07/14/2008  EFW  Added support for running as an MSBuild task
//=============================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using SandcastleBuilder.Utils.InheritedDocumentation;

namespace SandcastleBuilder.InheritedDocumentation
{
    /// <summary>
    /// This class represents the tool that scans XML comments files for
    /// <b>&lt;inheritdoc /&gt;</b> tags and produces a new XML comments
    /// file containing the inherited documentation for use by Sandcastle.
    /// </summary>
    public class GenerateInheritedDocs : Task
    {
        #region MSBuild task interface
        //=====================================================================

        private string configFile;

        /// <summary>
        /// This is used to set the configuration file to use from the MSBuild
        /// project file
        /// </summary>
        [Required]
        public string ConfigurationFile
        {
            get { return configFile; }
            set { configFile = value; }
        }

        /// <summary>
        /// This is used to execute the task and generate the inherited
        /// documentation.
        /// </summary>
        /// <returns>True on success or false on failure.</returns>
        public override bool Execute()
        {
            return (Main(new string[] { configFile }) == 0);
        }
        #endregion

        #region Private data members
        //=====================================================================

        // Configuration stuff
        private static XPathNavigator apisNode;
        private static string inheritedDocsFilename;
        private static IndexedCommentsCache commentsCache;
        private static Collection<XPathNavigator> commentsFiles;
        private static bool showDupWarning;

        private static XmlDocument inheritedDocs;
        private static XmlNode docMemberList;
        private static Stack<string> memberStack;
        #endregion

        #region Main program entry point
        //=====================================================================

        /// <summary>
        /// Main program entry point
        /// </summary>
        /// <param name="args">The command line arguments.  There should be
        /// single configuration filename specified.</param>
        /// <returns>Zero on success, non-zero on failure</returns>
        public static int Main(string[] args)
        {
            bool success = true;

            Assembly asm = Assembly.GetExecutingAssembly();

            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            Console.WriteLine("{0}, version {1}\r\n{2}\r\nE-Mail: Eric@EWoodruff.us\r\n",
                fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright);

            if(args.Length != 1)
            {
                Console.WriteLine("Specify the name of the configuration " +
                    "file as the only command line argument.");
                return 1;
            }

            try
            {
                commentsFiles = new Collection<XPathNavigator>();
                inheritedDocs = new XmlDocument();
                inheritedDocs.PreserveWhitespace = true;
                inheritedDocs.LoadXml(@"<doc>
  <assembly>
    <name>_InheritedDocs_</name>
  </assembly>
  <members>
  </members>
</doc>");
                docMemberList = inheritedDocs.SelectSingleNode("doc/members");
                memberStack = new Stack<string>();

                LoadConfiguration(args[0]);
                ScanCommentsFiles();
                inheritedDocs.Save(inheritedDocsFilename);

                Console.WriteLine("\r\nDocumentation merged successfully");
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                success = false;

                Console.WriteLine("SHFB: Error GID0001: Unexpected error " +
                    "while generating inherited documentation:\r\n{0}",
                    ex.ToString());
            }

#if DEBUG
            if(System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Hit ENTER to exit...");
                Console.ReadLine();
            }
#endif
            return (success) ? 0 : 2;
        }
        #endregion

        #region Duplicate warning event handler
        //=====================================================================

        /// <summary>
        /// Report a duplicate key warning
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private static void commentsCache_ReportWarning(object sender, CommentsCacheEventArgs e)
        {
            if(showDupWarning)
                Console.WriteLine(e.Message);
        }
        #endregion

        #region Load the configuration file
        //=====================================================================

        /// <summary>
        /// Load the configuration file
        /// </summary>
        /// <param name="configFile">The configuration file to load</param>
        /// <exception cref="ArgumentException">This is thrown if the
        /// configuration file does not exist.</exception>
        /// <exception cref="ConfigurationErrorsException">This is thrown if
        /// the configuration file is not valid.</exception>
        private static void LoadConfiguration(string configFile)
        {
            XPathDocument config, xpathDoc;
            XPathNavigator navConfig, navComments, element, refInfo;
            string path, wildcard, attrValue;
            bool recurse;
            int cacheSize = 100;

            if(!File.Exists(configFile))
                throw new ArgumentException("Configuration file not found: " + configFile, "configFile");

            config = new XPathDocument(configFile);
            navConfig = config.CreateNavigator();

            // Show duplicate key warnings?
            showDupWarning = (navConfig.SelectSingleNode("configuration/showDuplicateWarning") != null);

            // Get the reflection information file
            element = navConfig.SelectSingleNode("configuration/reflectionInfo/@file");

            if(element == null || !File.Exists(element.Value))
                throw new ConfigurationErrorsException("The reflectionFile " +
                    "element is missing or the specified file does not exist");

            Console.WriteLine("Reflection information will be retrieved from '{0}'", element.Value);
            xpathDoc = new XPathDocument(element.Value);
            refInfo = xpathDoc.CreateNavigator();
            apisNode = refInfo.SelectSingleNode("reflection/apis");

            // Get the inherited documentation filename
            element = navConfig.SelectSingleNode("configuration/inheritedDocs/@file");

            if(element == null)
                throw new ConfigurationErrorsException("The inheritedDocs " +
                    "element does not exist or is not valid");

            inheritedDocsFilename = element.Value;
            Console.WriteLine("Inherited documentation will be written to '{0}'", inheritedDocsFilename);

            // Load the comments file information
            navComments = navConfig.SelectSingleNode("configuration/commentsFiles");

            attrValue = navComments.GetAttribute("cacheSize", String.Empty);

            if(attrValue.Length != 0)
                cacheSize = Convert.ToInt32(attrValue, CultureInfo.InvariantCulture);

            commentsCache = new IndexedCommentsCache(cacheSize);
            commentsCache.ReportWarning += commentsCache_ReportWarning;

            foreach(XPathNavigator nav in navComments.Select("*"))
            {
                path = nav.GetAttribute("path", String.Empty);
                wildcard = nav.GetAttribute("file", String.Empty);
                attrValue = nav.GetAttribute("recurse", String.Empty);

                if(path.Length == 0)
                {
                    path = Path.GetDirectoryName(wildcard);
                    wildcard = Path.GetFileName(wildcard);
                }

                path = Environment.ExpandEnvironmentVariables(path);

                if(wildcard.Length != 0)
                    wildcard = Environment.ExpandEnvironmentVariables(wildcard);
                else
                    wildcard = "*.xml";

                if(attrValue.Length == 0)
                    recurse = false;
                else
                    recurse = Convert.ToBoolean(attrValue, CultureInfo.InvariantCulture);

                Console.WriteLine("Indexing {0}\\{1}", path, wildcard);
                commentsCache.IndexCommentsFiles(path, wildcard, recurse, (nav.Name == "scan") ? commentsFiles : null);
            }

            if(commentsCache.FilesIndexed == 0 || commentsCache.IndexCount == 0)
                throw new ConfigurationErrorsException("No comments files were specified or they did not " +
                    "contain valid information to index");

            if(commentsFiles.Count == 0)
                throw new ConfigurationErrorsException("No comments files were specified to scan for " +
                    "<inheritdoc /> tags.");

            Console.WriteLine("Indexed {0} members in {1} file(s).  {2} file(s) to scan for <inheritdoc /> tags.",
                commentsCache.IndexCount, commentsCache.FilesIndexed, commentsFiles.Count);
        }
        #endregion

        #region Root level documenation inheritance methods
        //=====================================================================

        /// <summary>
        /// This scans the XML comments files for &lt;inheritdoc /&gt; tags and
        /// adds the inherited documentation.
        /// </summary>
        private static void ScanCommentsFiles()
        {
            XmlNode node;
            Dictionary<string, XPathNavigator> members = new Dictionary<string, XPathNavigator>();
            string name;

            // Get a list of all unique members containing an <inheritdoc />
            // tag.  This will include entries with occurrences at the root
            // level as well as those nested within other tags.  Root level
            // stuff will get handled first followed by any nested tags.
            foreach(XPathNavigator file in commentsFiles)
                foreach(XPathNavigator tag in file.Select("//inheritdoc"))
                {
                    while(tag.Name != "member")
                        tag.MoveToParent();

                    name = tag.GetAttribute("name", String.Empty);

                    if(!members.ContainsKey(name))
                        members.Add(name, tag);
                }

            // For each one, create a new XML node that can be edited
            foreach(XPathNavigator nav in members.Values)
            {
                node = inheritedDocs.CreateDocumentFragment();
                node.InnerXml = nav.OuterXml;
                docMemberList.AppendChild(node);
            }

            // Add explicit interface implementations that do not have
            // member comments already.
            foreach(XPathNavigator api in apisNode.Select(
              "api[memberdata/@visibility='private' and proceduredata/@virtual='true']/@id"))
                if(commentsCache.GetComments(api.Value) == null && !members.ContainsKey(api.Value))
                {
                    node = inheritedDocs.CreateDocumentFragment();
                    node.InnerXml = String.Format(CultureInfo.InvariantCulture,
                        "<member name=\"{0}\"><inheritdoc /></member>", api.Value);
                    docMemberList.AppendChild(node);
                }

            Console.WriteLine("Merging inherited documentation...\r\n");

            // Inherit the root level documentation
            foreach(XmlNode n in docMemberList.SelectNodes("*"))
                InheritDocumentation(n);
        }

        /// <summary>
        /// This is used to generate the inherited documentation for the
        /// given member.  Only tags at the root level are processed here.
        /// </summary>
        /// <param name="member">The member for which to inherit documentation</param>
        /// <remarks>This will recursively expand documentation if a base
        /// member's comments are present in the generation list.</remarks>
        private static void InheritDocumentation(XmlNode member)
        {
            List<string> sources = new List<string>();
            XPathNavigator apiNode, baseMember;
            XmlNode copyMember;
            XmlAttribute cref, filter;
            string name, ctorName, baseMemberName;
            bool commentsFound;

            name = member.SelectSingleNode("@name").Value;

            // Check for a circular reference
            if(memberStack.Contains(name))
            {
                StringBuilder sb = new StringBuilder("Circular reference detected.\r\n" +
                    "Documentation inheritance stack:\r\n");

                sb.AppendFormat("    {0}: {1}", memberStack.Count + 1, name);
                sb.Append("\r\n");

                while(memberStack.Count != 0)
                    sb.AppendFormat("    {0}: {1}\r\n", memberStack.Count, memberStack.Pop());

                throw new InheritedDocsException(sb.ToString());
            }

            memberStack.Push(name);

            // Handle root level stuff first
            foreach(XmlNode inheritTag in member.SelectNodes("inheritdoc"))
            {
                // Get rid of the tag
                inheritTag.ParentNode.RemoveChild(inheritTag);

                // Apply a selection filter?
                filter = inheritTag.Attributes["select"];

                // Inherit from a member other than the base?
                cref = inheritTag.Attributes["cref"];

                // Is the base explicitly specified?
                if(cref != null)
                {
                    // Is it in the list of members for which to generate
                    // documentation?
                    copyMember = docMemberList.SelectSingleNode("member[@name='" + cref.Value + "']");

                    // If so, expand its tags now and use it.  If not, try
                    // to get it from the comments cache.
                    if(copyMember != null)
                    {
                        InheritDocumentation(copyMember);
                        baseMember = copyMember.CreateNavigator();
                    }
                    else
                        baseMember = commentsCache.GetComments(cref.Value);

                    if(baseMember != null)
                        MergeComments(baseMember, member.CreateNavigator(), (filter == null) ? "*" : filter.Value);
                    else
                        Console.WriteLine("SHFB: Warning GID0002: No comments found for cref '{0}' on member '{1}'",
                            cref.Value, name);

                    continue;
                }

                // Get a list of sources from which to merge comments
                commentsFound = false;
                sources.Clear();
                apiNode = apisNode.SelectSingleNode("api[@id='" + name + "']");

                if(apiNode == null)
                {
                    Console.WriteLine("SHFB: Warning GID0003: Unable to locate API ID '{0}'", name);
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
                }
                else
                {
                    // Constructors aren't like normal overrides.  They can call base copies that take the
                    // same arguments but the overrides aren't listed in the reflection info.  We'll just
                    // search all base types for a matching signature.
                    if(name.IndexOf("#ctor", StringComparison.Ordinal) != -1 ||
                      name.IndexOf("#cctor", StringComparison.Ordinal) != -1)
                    {
                        if(name.IndexOf("#ctor", StringComparison.Ordinal) != -1)
                            ctorName = name.Substring(name.IndexOf("#ctor", StringComparison.Ordinal));
                        else
                            ctorName = name.Substring(name.IndexOf("#cctor", StringComparison.Ordinal));

                        baseMember = apiNode.SelectSingleNode("containers/type/@api");
                        apiNode = apisNode.SelectSingleNode("api[@id='" + baseMember.Value + "']");

                        foreach(XPathNavigator baseType in apiNode.Select("family/ancestors/type/@api"))
                        {
                            baseMemberName = String.Format(CultureInfo.InvariantCulture, "M:{0}.{1}",
                                baseType.Value.Substring(2), ctorName);

                            if(commentsCache.GetComments(baseMemberName) != null)
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

                        // Then hit implementations.  There should only be one
                        // but just in case, look for more.
                        foreach(XPathNavigator baseType in apiNode.Select("implements/member/@api"))
                            sources.Add(baseType.Value);
                    }
                }

                // Inherit documentation from each base member
                foreach(string baseName in sources)
                {
                    // Is it in the list of members for which to generate
                    // documentation?
                    copyMember = docMemberList.SelectSingleNode("member[@name='" + baseName + "']");

                    // If so, expand its tags now and use it.  If not, try
                    // to get it from the comments cache.
                    if(copyMember != null)
                    {
                        InheritDocumentation(copyMember);
                        baseMember = copyMember.CreateNavigator();
                    }
                    else
                        baseMember = commentsCache.GetComments(baseName);

                    if(baseMember != null)
                    {
                        MergeComments(baseMember, member.CreateNavigator(), (filter == null) ? "*" : filter.Value);
                        commentsFound = true;
                    }
                }

                if(!commentsFound)
                    Console.WriteLine("SHFB: Warning GID0004: No comments found for member '{0}'", name);
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
        private static void MergeComments(XPathNavigator fromMember, XPathNavigator toMember, string filter)
        {
            XPathNavigator duplicate;
            string[] dupAttrs = new string[] { "cref", "href", "name", "vref", "xref" };
            string attrValue;

            if(String.IsNullOrEmpty(filter))
                filter = "*";

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
        #endregion

        #region Nested documentation inheritance methods
        //=====================================================================

        /// <summary>
        /// This is used to generate the inherited documentation for the
        /// given member.  Only tags at the root level are processed here.
        /// </summary>
        /// <param name="member">The member for which to inherit documentation</param>
        /// <remarks>This will recursively expand documentation if a base
        /// member's comments are present in the generation list.</remarks>
        private static void InheritNestedDocumentation(XmlNode member)
        {
            StringBuilder sb = new StringBuilder(256);
            XPathNavigator baseMember;
            XmlNode copyMember, content, newNode;
            XmlAttribute cref, filter;
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
                    if(filter.Value[0] != '/')
                        sb.AppendFormat("/{0}", filter.Value);
                    else
                    {
                        // If the filter is rooted, ignore the parent tags
                        // and use it as is.  This allows nesting inheritdoc
                        // within other tags that you don't want automatically
                        // included in the filter.
                        sb.Remove(0, sb.Length);
                        sb.Append(filter.Value.Substring(1));
                    }

                // Inherit from a member other than the base?
                cref = inheritTag.Attributes["cref"];

                baseMember = LocateBaseDocumentation(name, (cref != null) ? cref.Value : null);

                if(baseMember != null)
                {
                    content = inheritedDocs.CreateDocumentFragment();

                    // Merge the content
                    foreach(XPathNavigator element in baseMember.Select(sb.ToString()))
                    {
                        newNode = inheritedDocs.CreateDocumentFragment();

                        // If there's no filter, we don't want the tag
                        if(filter != null)
                            newNode.InnerXml = element.OuterXml;
                        else
                            newNode.InnerXml = element.InnerXml;

                        content.AppendChild(newNode);
                    }

                    // Replace the tag with the content
                    inheritTag.ParentNode.ReplaceChild(content, inheritTag);
                }
                else
                {
                    inheritTag.ParentNode.RemoveChild(inheritTag);

                    if(cref != null)
                        Console.WriteLine("SHFB: Warning GID0005: No comments found for cref '{0}' on member " +
                            "'{1}' in '{2}'", cref.Value, name, sb);
                    else
                        Console.WriteLine("SHFB: Warning GID0006: No comments found for member '{0}' in '{1}'",
                            name, sb);
                }
            }
        }

        /// <summary>
        /// Locate and merge the documentation from the base member(s)
        /// </summary>
        /// <param name="name">The member name</param>
        /// <param name="cref">An optional member name from which to inherit
        /// the documentation.</param>
        private static XPathNavigator LocateBaseDocumentation(string name, string cref)
        {
            List<string> sources = new List<string>();
            XPathNavigator apiNode, baseMember;
            XmlNode copyMember;
            string ctorName, baseMemberName;

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
                    baseMember = commentsCache.GetComments(cref);

                return baseMember;
            }

            // Get a list of sources from which to merge comments
            apiNode = apisNode.SelectSingleNode("api[@id='" + name + "']");

            if(apiNode == null)
            {
                Console.WriteLine("SHFB: Warning GID0003: Unable to locate API ID '{0}'", name);
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
                if(name.IndexOf("#ctor", StringComparison.Ordinal) != -1 ||
                  name.IndexOf("#cctor", StringComparison.Ordinal) != -1)
                {
                    if(name.IndexOf("#ctor", StringComparison.Ordinal) != -1)
                        ctorName = name.Substring(name.IndexOf("#ctor", StringComparison.Ordinal));
                    else
                        ctorName = name.Substring(name.IndexOf("#cctor", StringComparison.Ordinal));

                    baseMember = apiNode.SelectSingleNode("containers/type/@api");
                    apiNode = apisNode.SelectSingleNode("api[@id='" + baseMember.Value + "']");

                    foreach(XPathNavigator baseType in apiNode.Select("family/ancestors/type/@api"))
                    {
                        baseMemberName = String.Format(CultureInfo.InvariantCulture, "M:{0}.{1}",
                            baseType.Value.Substring(2), ctorName);

                        if(commentsCache.GetComments(baseMemberName) != null)
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

                    // Then hit implementations.  There should only be one
                    // but just in case, look for more.
                    foreach(XPathNavigator baseType in apiNode.Select("implements/member/@api"))
                        sources.Add(baseType.Value);
                }
            }

            baseMember = null;

            // Find the first member with comments
            foreach(string baseName in sources)
            {
                // Is it in the list of members for which to generate
                // documentation?
                copyMember = docMemberList.SelectSingleNode("member[@name='" + baseName + "']");

                // If so, expand its tags now and use it.  If not, try
                // to get it from the comments cache.
                if(copyMember != null)
                {
                    InheritDocumentation(copyMember);
                    baseMember = copyMember.CreateNavigator();
                }
                else
                    baseMember = commentsCache.GetComments(baseName);

                if(baseMember != null)
                    break;
            }

            return baseMember;
        }
        #endregion
    }
}
