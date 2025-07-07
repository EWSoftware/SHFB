//===============================================================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : BuildProcess.Namespaces.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/22/2025
// Note    : Copyright 2006-2025, Eric Woodruff, All rights reserved
//
// This file contains the code used to generate the namespace summary file and to purge the unwanted namespaces
// from the reflection information file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/04/2006  EFW  Created the code
// 09/29/2006  EFW  Reworked to insert "missing summary" note for all namespaces without a summary
// 11/10/2006  EFW  Reworked to support external XML comments files
// 05/11/2007  EFW  Missing namespace messages are now optional
// 07/19/2007  EFW  Namespace removal is now handled by the MRefBuilder ripping feature
// 09/13/2007  EFW  Added support for calling plug-ins
// 03/08/2008  EFW  Added support for NamespaceDoc classes
// 12/14/2013  EFW  Added support for namespace grouping
// 09/18/2014  EFW  Added support for NamespaceGroupDoc classes
//===============================================================================================================

// Ignore Spelling: topicdata

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core;
using Sandcastle.Core.BuildEngine;
using Sandcastle.Core.PlugIn;
using Sandcastle.Core.Project;

namespace SandcastleBuilder.MSBuild.BuildEngine
{
    public partial class BuildProcess
    {
        #region Private data members
        //=====================================================================

        private static readonly Regex reStripWhitespace = new(@"\s");

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the XML comments files collection
        /// </summary>
        public XmlCommentsFileCollection CommentsFiles { get; private set; }

        #endregion

        #region Namespace summary methods
        //=====================================================================

        /// <summary>
        /// This is called to generate the namespace summary file
        /// </summary>
        private void GenerateNamespaceSummaries()
        {
            XmlNode member;
            NamespaceSummaryItem nsi;
            string nsName = null, summaryText;
            bool isDocumented;

            this.ReportProgress(BuildStep.GenerateNamespaceSummaries, "Generating namespace summary information...");

            // Add a dummy file if there are no comments files specified.  This will contain the project and
            // namespace summaries.
            if(this.CommentsFiles.Count == 0)
            {
                nsName = this.WorkingFolder + "_ProjNS_.xml";

                using(StreamWriter sw = new(nsName, false, Encoding.UTF8))
                {
                    sw.Write("<?xml version=\"1.0\"?>\r\n<doc>\r\n" +
                        "<assembly>\r\n<name>_ProjNS_</name>\r\n" +
                        "</assembly>\r\n<members/>\r\n</doc>\r\n");
                }

                this.CommentsFiles.Add(new XmlCommentsFile(nsName));
            }

            // Replace any "NamespaceDoc" and "NamespaceGroupDoc" class IDs with their containing namespace.
            // The comments in these then become the comments for the namespaces and namespace groups.
            this.CommentsFiles.ReplaceNamespaceDocEntries();

            if(this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                return;

            this.ExecutePlugIns(ExecutionBehaviors.Before);

            // XML comments do not support summaries on namespace elements by default.  However, if Sandcastle
            // finds them, it will add them to the help file.  The same holds true for project comments on the
            // root namespaces page (R:Project_[HtmlHelpName]).  We can accomplish this by adding elements to
            // the first comments file or by supplying them in an external XML comments file.
            try
            {
                // Add the project comments if specified
                if(project.ProjectSummary.Length != 0)
                {
                    // Set the name in case it isn't valid
                    nsName = "R:Project_" + this.ResolvedHtmlHelpName.Replace(" ", "_");
                    member = this.CommentsFiles.FindMember(nsName);
                    this.AddNamespaceComments(member, project.ProjectSummary);
                }

                // Get all the namespace and namespace group nodes from the reflection information file
                var nsElements =
                    ComponentUtilities.XmlStreamAxis(this.ReflectionInfoFilename, "api").Where(el =>
                    {
                        string id = el.Attribute("id").Value;

                        return (id.Length > 1 && id[1] == ':' && (id[0] == 'N' || id[0] == 'G') &&
                            el.Element("topicdata").Attribute("group").Value != "rootGroup");
                    }).Select(el => el.Attribute("id").Value);

                // Add the namespace summaries
                foreach(var n in nsElements)
                {
                    nsName = n;

                    nsi = project.NamespaceSummaries[nsName.StartsWith("N:", StringComparison.Ordinal) ?
                        nsName.Substring(2) : nsName.Substring(2) + " (Group)"];

                    if(nsi != null)
                    {
                        isDocumented = nsi.IsDocumented;
                        summaryText = nsi.Summary;
                    }
                    else
                    {
                        // The global namespace is not documented by default
                        isDocumented = (nsName != "N:");
                        summaryText = String.Empty;
                    }

                    if(isDocumented)
                    {
                        // If documented, add the summary text
                        member = this.CommentsFiles.FindMember(nsName);
                        this.AddNamespaceComments(member, summaryText);
                    }
                }
            }
            catch(Exception ex)
            {
                throw new BuilderException("BE0012", String.Format(CultureInfo.CurrentCulture,
                    "Error generating namespace summaries (Namespace = {0}): {1}", nsName, ex.Message), ex);
            }

            this.ExecutePlugIns(ExecutionBehaviors.After);
        }

        /// <summary>
        /// Add project or namespace comments
        /// </summary>
        /// <param name="member">The member node to modify.</param>
        /// <param name="summaryText">The summary text to add.</param>
        private void AddNamespaceComments(XmlNode member, string summaryText)
        {
            string text;
            XmlNode tag = member.SelectSingleNode("summary");

            if(tag == null)
            {
                tag = member.OwnerDocument.CreateNode(XmlNodeType.Element, "summary", null);
                member.AppendChild(tag);
            }

            text = reStripWhitespace.Replace(tag.InnerText, String.Empty);

            // NOTE: The text is not HTML encoded as it can contain HTML tags.  As such, entities such as "&"
            // should be entered in encoded form in the text (i.e. &amp;).
            if(!String.IsNullOrEmpty(text))
            {
                if(!String.IsNullOrEmpty(summaryText))
                    tag.InnerXml += "<p/>" + summaryText;
            }
            else
                if(!String.IsNullOrEmpty(summaryText))
                    tag.InnerXml = summaryText;
                else
                    if(!project.ShowMissingNamespaces)
                        tag.InnerXml = "&#xA0;";

            // The ShowMissingComponent handles adding the "missing" error message to the topic.
        }
        #endregion

        #region Namespace grouping methods
        //=====================================================================

        /// <summary>
        /// This processes the namespaces in the reflection information file and adds the group entries
        /// </summary>
        /// <returns>True on success, false on failure</returns>
        private void AddNamespaceGroupEntries()
        {
            List<string> namespaces = [];
            XPathNavigator projectRoot = null;
            int groupCount = 0;

            this.ReportProgress(BuildStep.AddNamespaceGroups, "Adding namespace group entries...");

            if(this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                return;

            this.ExecutePlugIns(ExecutionBehaviors.Before);

            string ungroupedFilename = Path.ChangeExtension(this.ReflectionInfoFilename, ".ungrouped");

            File.Move(this.ReflectionInfoFilename, ungroupedFilename);

            using(var reader = XmlReader.Create(ungroupedFilename, new XmlReaderSettings { CloseInput = true }))
            {
                XPathDocument source = new(reader);

                using XmlWriter xw = XmlWriter.Create(this.ReflectionInfoFilename, new XmlWriterSettings
                {
                    Indent = true,
                    CloseOutput = true
                });
                
                // Copy the reflection element
                xw.WriteStartDocument();
                xw.WriteStartElement("reflection");

                var reflection = source.CreateNavigator().SelectSingleNode("reflection");

                // Copy assembly elements
                reflection.SelectSingleNode("assemblies")?.WriteSubtree(xw);

                // Copy the API elements and track all of the namespace elements
                xw.WriteStartElement("apis");

                foreach(XPathNavigator api in reflection.Select("apis/api"))
                {
                    string id = (string)api.Evaluate("string(@id)");

                    if(id != null && id.Length > 1 && id[1] == ':' && (id[0] == 'N' || id[0] == 'R'))
                    {
                        if(id.StartsWith("N:", StringComparison.Ordinal))
                        {
                            namespaces.Add(id);
                            api.WriteSubtree(xw);
                        }
                        else
                            projectRoot = api;      // Project root node gets replaced if present
                    }
                    else
                        api.WriteSubtree(xw);
                }

                // Group namespaces and write out the group entries
                foreach(var group in GroupNamespaces(namespaces, projectRoot != null))
                {
                    if(group.Namespace.Length == 0)
                    {
                        // If the namespace is blank, it's the root group.  If a project root element was
                        // specified, replace its element list with the one from this group.  If no project
                        // root element was found, write the root group out as a placeholder for the TOC
                        // transformation so that it can determine the root level content.
                        if(projectRoot != null)
                        {
                            xw.WriteStartElement("api");
                            xw.WriteAttributeString("id", projectRoot.GetAttribute("id", String.Empty));

                            projectRoot.MoveToChild("topicdata", String.Empty);
                            projectRoot.WriteSubtree(xw);

                            xw.WriteStartElement("elements");

                            foreach(string child in group.Children.OrderBy(n => n.Substring(2)))
                            {
                                xw.WriteStartElement("element");
                                xw.WriteAttributeString("api", child);
                                xw.WriteEndElement();
                            }

                            xw.WriteEndElement();   // elements
                            xw.WriteEndElement();   // api
                        }
                        else
                        {
                            // This one does not generate a topic
                            xw.WriteStartElement("api");
                            xw.WriteAttributeString("id", "G:");

                            xw.WriteStartElement("topicdata");
                            xw.WriteAttributeString("group", "rootGroup");
                            xw.WriteAttributeString("notopic", String.Empty);
                            xw.WriteEndElement();

                            xw.WriteStartElement("elements");

                            foreach(string child in group.Children.OrderBy(n => n.Substring(2)))
                            {
                                xw.WriteStartElement("element");
                                xw.WriteAttributeString("api", child);
                                xw.WriteEndElement();
                            }

                            xw.WriteEndElement();   // elements
                            xw.WriteEndElement();   // api
                        }
                    }
                    else
                    {
                        groupCount++;

                        xw.WriteStartElement("api");
                        xw.WriteAttributeString("id", group.Namespace);

                        xw.WriteStartElement("topicdata");
                        xw.WriteAttributeString("group", "api");
                        xw.WriteEndElement();

                        xw.WriteStartElement("apidata");
                        xw.WriteAttributeString("name", group.Namespace.Substring(2));
                        xw.WriteAttributeString("group", "namespaceGroup");
                        xw.WriteEndElement();

                        xw.WriteStartElement("elements");

                        foreach(string child in group.Children.OrderBy(n => n.Substring(2)))
                        {
                            xw.WriteStartElement("element");
                            xw.WriteAttributeString("api", child);
                            xw.WriteEndElement();
                        }

                        xw.WriteEndElement();   // elements
                        xw.WriteEndElement();   // api
                    }
                }

                xw.WriteEndElement();   // apis
                xw.WriteEndElement();   // reflection
                xw.WriteEndDocument();
            }

            this.ReportProgress("Added {0} namespace group entries", groupCount);

            this.ExecutePlugIns(ExecutionBehaviors.After);
        }

        /// <summary>
        /// This is used to group the namespaces based on their common root
        /// </summary>
        /// <param name="namespaces">An enumerable list of namespaces to group.</param>
        /// <param name="hasRootNamespaceContainer">True if the project has a root namespace container, false
        /// if not.  This controls whether or not the root group is retained.</param>
        /// <returns>An enumerable list of the grouped namespaces</returns>
        private IEnumerable<NamespaceGroup> GroupNamespaces(List<string> namespaces,
          bool hasRootNamespaceContainer)
        {
            Dictionary<string, NamespaceGroup> groups = [];
            string[] parts;
            string root;
            int partCount;
            int maxParts = this.CurrentProject.MaximumGroupParts;

            // If the shortest namespace has more than one part, increase the maximum number of parts to account
            // for the base length.
            if(namespaces.Count != 0)
                maxParts += namespaces.Min(n => n.Split('.').Length) - 1;

            // This serves as the root group.  If a project element is present in the reflection file, its
            // list of elements will be replaced by the list in this group.  If not, this group is written to
            // the file to serve as a place holder for the TOC XSL transformation.
            groups.Add(String.Empty, new NamespaceGroup { Namespace = String.Empty });

            // Iterate over the namespaces so that we can figure out the common root namespaces
            foreach(string space in namespaces.AsEnumerable().Reverse())
            {
                parts = space.Split('.');
                partCount = parts.Length >= maxParts ? maxParts : parts.Length - 1;

                if(parts.Length - 1 <= partCount)
                    root = String.Join(".", parts, 0, parts.Length - 1);
                else
                    root = String.Join(".", parts, 0, partCount);

                // Create a new group to represent the namespace if there are child namespaces and it is not
                // there already.  A group is only created if it will contain more than one namespace. Namespaces
                // without any children will end up in their parent as a standard namespace entry.
                if(root.Length > 2 && !groups.ContainsKey(root) && namespaces.Count(n => n.StartsWith(root,
                  StringComparison.Ordinal)) > 1)
                {
                    groups.Add(root, new NamespaceGroup { Namespace = root });
                }
            }

            // Now place the namespaces in the appropriate group.  Include the group keys as they may not be
            // represented by an actual namespace.
            foreach(string space in namespaces.Concat(groups.Keys.Where(k => k.Length != 0)).GroupBy(
              n => n).Select(n => n.Key))
            {
                parts = space.Split('.');
                partCount = parts.Length >= maxParts ? maxParts : parts.Length - 1;

                while(partCount > -1)
                {
                    if(parts.Length - 1 <= partCount)
                        root = String.Join(".", parts, 0, parts.Length - 1);
                    else
                        root = String.Join(".", parts, 0, partCount);

                    if(groups.TryGetValue(root, out NamespaceGroup match))
                    {
                        match.Children.Add(space);
                        break;
                    }

                    // If not found (group key with no namespace), remove the last part and try again.  If all
                    // else fails, it'll end up in the root group.
                    partCount--;
                }
            }

            // Make a pass through the groups.  Convert each child that is a group key to a group reference.
            foreach(var kv in groups)
            {
                var children = kv.Value.Children;

                for(int idx = 0; idx < children.Count; idx++)
                    if(groups.ContainsKey(children[idx]))
                        children[idx] = "G" + children[idx].Substring(1);
            }

            // If a group only contains one group entry, pull that sub-group up into the parent and remove
            // the sub-group.  Don't do it for the root namespace container or if the namespace itself will
            // appear with the group entry.
            foreach(var group in groups.ToList())
            {
                if((group.Key.Length != 0 || hasRootNamespaceContainer) && group.Value.Children.Count == 1 &&
                  group.Value.Children[0][0] == 'G' && !namespaces.Contains(group.Value.Namespace))
                {
                    root = "N" + group.Value.Children[0].Substring(1);

                    // Ignore it if already removed
                    if(groups.ContainsKey(group.Key))
                    {
                        if(namespaces.Contains(root))
                            group.Value.Children.Add(root);

                        group.Value.Children.RemoveAt(0);
                        group.Value.Children.AddRange(groups[root].Children);
                        groups.Remove(root);
                    }
                }
            }

            var rootGroup = groups[String.Empty];

            if(rootGroup.Children.Count != 0)
                root = "N" + rootGroup.Children[0].Substring(1);
            else
                root = "N:";

            if(groups.Count > 1 && rootGroup.Children.Count == 1 && hasRootNamespaceContainer)
            {
                // Special case.  If we've got a root group with only one child and there is a root namespace
                // container, pull the content for the group into the root and remove the child.
                if(namespaces.Contains(root))
                    rootGroup.Children.Add(root);

                rootGroup.Children.RemoveAt(0);
                rootGroup.Children.AddRange(groups[root].Children);
                groups.Remove(root);
            }
            else
            {
                if(groups.Count > 1 && rootGroup.Children.Count == 1 && !namespaces.Contains(root))
                {
                    // Special case.  If the root group doesn't exist as a namespace and only has one child, pull
                    // the content for the group into the root and remove the child.  However, if the group has
                    // more than one namespace starting with the first namespace, keep the group and name it
                    // after the first entry.
                    var childGroup = groups[root];

                    string name = childGroup.Children[0] + ".";

                    if(!childGroup.Children.Skip(1).All(c => c.StartsWith(name, StringComparison.Ordinal)))
                    {
                        rootGroup.Children.RemoveAt(0);
                        rootGroup.Children.AddRange(childGroup.Children);
                        groups.Remove(root);
                    }
                    else
                    {
                        childGroup.Namespace = name.Substring(0, name.Length - 1);
                        rootGroup.Children.RemoveAt(0);
                        rootGroup.Children.Add("G" + childGroup.Namespace.Substring(1));
                    }
                }
            }

            // In the final pass, for each group key that is a namespace, add the namespace to its children.
            // Also change the name of the namespace group.  Once done, return it to the caller.
            foreach(var group in groups.Values)
            {
                if(namespaces.Contains(group.Namespace) && !group.Children.Contains(group.Namespace))
                    group.Children.Add(group.Namespace);

                if(group.Namespace.Length != 0)
                    group.Namespace = "G" + group.Namespace.Substring(1);

                yield return group;
            }
        }
        #endregion
    }
}
