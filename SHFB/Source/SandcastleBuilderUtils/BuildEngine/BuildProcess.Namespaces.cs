//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BuildProcess.Namespaces.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/02/2012
// Note    : Copyright 2006-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the code used to generate the namespace summary file and
// to purge the unwanted namespaces from the reflection information file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.2.0.0  09/04/2006  EFW  Created the code
// 1.3.1.0  09/29/2006  EFW  Reworked to insert "missing summary" note for all
//                           namespaces without a summary.
// 1.3.2.0  11/10/2006  EFW  Reworked to support external XML comments files
// 1.4.0.2  05/11/2007  EFW  Missing namespace messages are now optional
// 1.5.0.2  07/19/2007  EFW  Namespace removal is now handled by the MRefBuilder
//                           ripping feature.
// 1.5.2.0  09/13/2007  EFW  Added support for calling plug-ins
// 1.6.0.6  03/08/2008  EFW  Added support for NamespaceDoc classes
//=============================================================================

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

using SandcastleBuilder.Utils.PlugIn;

namespace SandcastleBuilder.Utils.BuildEngine
{
    partial class BuildProcess
    {
        #region Private data members
        //=====================================================================
        // Private data members

        private XmlCommentsFileCollection commentsFiles;

        private static Regex reStripWhitespace = new Regex(@"\s");
        #endregion

        #region Private data members
        //=====================================================================
        // Private data members

        /// <summary>
        /// This read-only property returns the XML comments files collection
        /// </summary>
        public XmlCommentsFileCollection CommentsFiles
        {
            get { return commentsFiles; }
        }
        #endregion

        /// <summary>
        /// This is called to generate the namespace summary file
        /// </summary>
        private void GenerateNamespaceSummaries()
        {
            XmlNodeList nsElements;
            XmlNode member;
            NamespaceSummaryItem nsi;
            string nsName = null, summaryText;
            bool isDocumented;

            this.ReportProgress(BuildStep.GenerateNamespaceSummaries,
                "Generating namespace summary information...");

            // Add a dummy file if there are no comments files specified.
            // This will contain the project and namespace summaries.
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

            // Replace any "NamespaceDoc" class IDs with their containing
            // namespace.  The comments in these then become the comments
            // for the namespace.
            commentsFiles.ReplaceNamespaceDocEntries();

            if(this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                return;

            this.ExecutePlugIns(ExecutionBehaviors.Before);

            // XML comments do not support summaries on namespace elements by default.  However, if
            // Sandcastle finds them, it will add them to the help file.  The same holds true for
            // project comments on the root namespaces page (R:Project_[HtmlHelpName]).  We can
            // accomplish this by adding elements to the first comments file or by supplying them
            // in an external XML comments file.
            try
            {
                // Add the project comments if specified
                if(project.ProjectSummary.Length != 0)
                {
                    // Set the name in case it isn't valid
                    nsName = "R:Project_" + project.HtmlHelpName.Replace(" ", "_");
                    member = commentsFiles.FindMember(nsName);
                    this.AddNamespaceComments(member, project.ProjectSummary);
                }

                // Get all the namespace nodes
                nsElements = apisNode.SelectNodes("api[starts-with(@id, 'N:')]");

                // Add the namespace summaries
                foreach(XmlNode n in nsElements)
                {
                    nsName = n.Attributes["id"].Value;
                    nsi = project.NamespaceSummaries[nsName.Substring(2)];

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

                    // As of 1.5.0.2, namespace removal is handled by
                    // the MRefBuilder ripping feature so we don't have
                    // to deal with it here anymore.

                    // MRefBuilder bug, June CTP and prior.  If ripped, an
                    // empty node still appears in the reflection file that
                    // needs to go away when using /internal+.
                    if(n.SelectSingleNode("elements").ChildNodes.Count == 0)
                        n.ParentNode.RemoveChild(n);
                    else
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
                // Eat the error in a partial build so that the user can
                // get into the namespace comments editor to fix it.
                if(!isPartialBuild)
                    throw new BuilderException("BE0012", String.Format(
                        CultureInfo.InvariantCulture, "Error generating " +
                        "namespace summaries (Namespace = {0}): {1}", nsName,
                        ex.Message), ex);
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
                tag = member.OwnerDocument.CreateNode(XmlNodeType.Element,
                    "summary", null);
                member.AppendChild(tag);
            }

            text = reStripWhitespace.Replace(tag.InnerText, String.Empty);

            // NOTE: The text is not HTML encoded as it can contain HTML
            //       tags.  As such, entities such as "&" should be entered
            //       in encoded form in the text (i.e. &amp;).
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

                    // The ShowMissingComponent handles adding the "missing"
                    // error message to the topic.
        }
    }
}
