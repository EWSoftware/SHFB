//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : TocExcludePlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/16/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains a plug-in that can be used to exclude API members from the table of contents via the
// <tocexclude /> XML comment tag.  The excluded items are still accessible in the help file via other topic
// links.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/13/2008  EFW  Created the code
// 12/17/2013  EFW  Updated to use MEF for the plug-ins
//===============================================================================================================

// Ignore Spelling: tocexclude

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class can be used to exclude API members from the table of contents via the
    /// <c>&lt;tocexclude /&gt;</c> XML comment tag.  The excluded items are still accessible in the help file
    /// via other topic links.
    /// </summary>
    [HelpFileBuilderPlugInExport("Table of Contents Exclusion", Version = AssemblyInfo.ProductVersion,
      Copyright = AssemblyInfo.Copyright, Description = "This plug-in can be used to exclude API members from " +
        "the table of contents via the <tocexclude /> XML comment tag.  The excluded items are still accessible " +
        "in the help file via other topic links.")]
    public sealed class TocExcludePlugIn : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private List<ExecutionPoint> executionPoints;
        private BuildProcess builder;
        private List<string> exclusionList;

        #endregion

        #region IPlugIn implementation
        //=====================================================================

        /// <summary>
        /// This read-only property returns a collection of execution points that define when the plug-in should
        /// be invoked during the build process.
        /// </summary>
        public IEnumerable<ExecutionPoint> ExecutionPoints
        {
            get
            {
                if(executionPoints == null)
                    executionPoints = new List<ExecutionPoint>
                    {
                        // This one has a slightly higher priority as it removes stuff that the other plug-ins
                        // don't need to see.
                        new ExecutionPoint(BuildStep.GenerateIntermediateTableOfContents,
                            ExecutionBehaviors.After, 1500)
                    };

                return executionPoints;
            }
        }

        /// <summary>
        /// This method is used to initialize the plug-in at the start of the build process
        /// </summary>
        /// <param name="buildProcess">A reference to the current build process</param>
        /// <param name="configuration">The configuration data that the plug-in should use to initialize itself</param>
        public void Initialize(BuildProcess buildProcess, XElement configuration)
        {
            builder = buildProcess;

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright);

            exclusionList = new List<string>();
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            XmlDocument toc;
            XPathNavigator root, navToc, tocEntry, tocParent;
            bool hasParent;

            // Scan the XML comments files.  The files aren't available soon after this step and by now
            // everything that other plug-ins may have added to the collection should be there.
            builder.ReportProgress("Searching for comment members containing <tocexclude />...");

            foreach(XmlCommentsFile f in builder.CommentsFiles)
                foreach(XmlNode member in f.Members.SelectNodes("member[count(.//tocexclude) > 0]/@name"))
                    exclusionList.Add(member.Value);

            builder.ReportProgress("Found {0} members to exclude from the TOC", exclusionList.Count);

            if(exclusionList.Count == 0)
                return;

            builder.ReportProgress("Removing members from the TOC");

            toc = new XmlDocument();
            toc.Load(Path.Combine(builder.WorkingFolder, "toc.xml"));
            navToc = toc.CreateNavigator();

            // If a root namespace container node is present, we need to look in it rather than the document root
            // node.
            root = navToc.SelectSingleNode("topics/topic[starts-with(@id, 'R:')]");

            if(root == null)
                root = navToc.SelectSingleNode("topics");

            foreach(string id in exclusionList)
            {
                tocEntry = root.SelectSingleNode("//topic[@id='" + id + "']");

                // Ignore if null, it was probably excluded by the API filter
                if(tocEntry != null)
                {
                    // Remove the entry.  If this results in the parent being an empty node, remove it too.
                    do
                    {
                        tocParent = tocEntry.Clone();
                        hasParent = tocParent.MoveToParent();

                        builder.ReportProgress("    Removing '{0}'", tocEntry.GetAttribute("id", String.Empty));
                        tocEntry.DeleteSelf();

                    } while(hasParent && !tocParent.HasChildren);
                }
            }

            toc.Save(Path.Combine(builder.WorkingFolder, "toc.xml"));
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <summary>
        /// This implements the Dispose() interface to properly dispose of the plug-in object
        /// </summary>
        public void Dispose()
        {
            // Nothing to dispose of in this one
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
