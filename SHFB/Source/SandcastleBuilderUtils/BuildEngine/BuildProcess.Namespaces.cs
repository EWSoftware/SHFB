//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BuildProcess.Namespaces.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/10/2018
// Note    : Copyright 2006-2018, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using Sandcastle.Core;
using SandcastleBuilder.Utils.BuildComponent;

namespace SandcastleBuilder.Utils.BuildEngine
{
    partial class BuildProcess
    {
        #region Private data members
        //=====================================================================

        private XmlCommentsFileCollection commentsFiles;

        private static Regex reStripWhitespace = new Regex(@"\s");

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the XML comments files collection
        /// </summary>
        public XmlCommentsFileCollection CommentsFiles => commentsFiles;

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

            this.ReportProgress(BuildStep.GenerateNamespaceSummaries,
                "Generating namespace summary information...");

            // Add a dummy file if there are no comments files specified.  This will contain the project and
            // namespace summaries.
            if(commentsFiles.Count == 0)
            {
                nsName = workingFolder + "_ProjNS_.xml";

                using(StreamWriter sw = new StreamWriter(nsName, false, Encoding.UTF8))
                {
                    sw.Write("<?xml version=\"1.0\"?>\r\n<doc>\r\n" +
                        "<assembly>\r\n<name>_ProjNS_</name>\r\n" +
                        "</assembly>\r\n<members/>\r\n</doc>\r\n");
                }

                commentsFiles.Add(new XmlCommentsFile(nsName));
            }

            // Replace any "NamespaceDoc" and "NamespaceGroupDoc" class IDs with their containing namespace.
            // The comments in these then become the comments for the namespaces and namespace groups.
            commentsFiles.ReplaceNamespaceDocEntries();

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
                    member = commentsFiles.FindMember(nsName);
                    this.AddNamespaceComments(member, project.ProjectSummary);
                }

                // Get all the namespace and namespace group nodes from the reflection information file
                var nsElements =
                    ComponentUtilities.XmlStreamAxis(reflectionFile, "api").Where(el =>
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
                        member = commentsFiles.FindMember(nsName);
                        this.AddNamespaceComments(member, summaryText);
                    }
                }
            }
            catch(Exception ex)
            {
                // Eat the error in a partial build so that the user can get into the namespace comments editor
                // to fix it.
                if(this.PartialBuildType != PartialBuildType.None)
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

        #region Get referenced namespaces
        //=====================================================================

        /// <summary>
        /// This is used to get an enumerable list of unique namespaces from the given reflection data file
        /// </summary>
        /// <param name="reflectionInfoFile">The reflection data file to search for namespaces</param>
        /// <param name="validNamespaces">An enumerable list of valid namespaces</param>
        /// <returns>An enumerable list of unique namespaces</returns>
        public static IEnumerable<string> GetReferencedNamespaces(string reflectionInfoFile,
          IEnumerable<string> validNamespaces)
        {
            HashSet<string> seenNamespaces = new HashSet<string>();
            string ns;

            // Find all type references and extract the namespace from them.  This is a rather brute force way
            // of doing it but the type element can appear in various places.  This way we find them all.
            // Examples: ancestors/type/@api, returns/type/@api, parameter/type/@api,
            // parameter/referenceTo/type/@api, attributes/attribute/argument/type/@api,
            // returns/type/specialization/type/@api, containers/type/@api, overrides/member/type/@api
            var typeRefs = ComponentUtilities.XmlStreamAxis(reflectionInfoFile, "type").Select(
                el => el.Attribute("api").Value);

            foreach(string typeName in typeRefs)
                if(typeName.Length > 2 && typeName.IndexOf('.') != -1)
                {
                    ns = typeName.Substring(2, typeName.LastIndexOf('.') - 2);

                    if(validNamespaces.Contains(ns) && !seenNamespaces.Contains(ns))
                    {
                        seenNamespaces.Add(ns);
                        yield return ns;
                    }
                }
        }
        #endregion
    }
}
