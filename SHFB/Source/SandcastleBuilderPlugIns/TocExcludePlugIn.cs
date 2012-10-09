//=============================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : TocExcludePlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/13/2010
// Note    : Copyright 2008-2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a plug-in that can be used to exclude API members from
// the table of contents via the <tocexclude /> XML comment tag.  The excluded
// items are still accessible in the help file via other topic links.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.6  03/13/2008  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildEngine;
using SandcastleBuilder.Utils.PlugIn;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class can be used to exclude API members from the table
    /// of contents via the <c>&lt;tocexclude /&gt;</c> XML comment tag.  The
    /// excluded items are still accessible in the help file via other topic
    /// links.
    /// </summary>
    public class TocExcludePlugIn : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private ExecutionPointCollection executionPoints;

        private BuildProcess builder;

        private List<string> exclusionList;
        #endregion

        #region IPlugIn implementation
        //=====================================================================

        /// <summary>
        /// This read-only property returns a friendly name for the plug-in
        /// </summary>
        public string Name
        {
            get { return "Table of Contents Exclusion"; }
        }

        /// <summary>
        /// This read-only property returns the version of the plug-in
        /// </summary>
        public Version Version
        {
            get
            {
                // Use the assembly version
                Assembly asm = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

                return new Version(fvi.ProductVersion);
            }
        }

        /// <summary>
        /// This read-only property returns the copyright information for the
        /// plug-in.
        /// </summary>
        public string Copyright
        {
            get
            {
                // Use the assembly copyright
                Assembly asm = Assembly.GetExecutingAssembly();
                AssemblyCopyrightAttribute copyright = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(
                    asm, typeof(AssemblyCopyrightAttribute));

                return copyright.Copyright;
            }
        }

        /// <summary>
        /// This read-only property returns a brief description of the plug-in
        /// </summary>
        public string Description
        {
            get
            {
                return "This plug-in can be used to exclude API members " +
                    "from the table of contents via the <tocexclude /> XML " +
                    "comment tag.  The excluded items are still accessible " +
                    "in the help file via other topic links.";
            }
        }

        /// <summary>
        /// This plug-in does not run in partial builds
        /// </summary>
        public bool RunsInPartialBuild
        {
            get { return false; }
        }

        /// <summary>
        /// This read-only property returns a collection of execution points
        /// that define when the plug-in should be invoked during the build
        /// process.
        /// </summary>
        public ExecutionPointCollection ExecutionPoints
        {
            get
            {
                if(executionPoints == null)
                    executionPoints = new ExecutionPointCollection
                    {
                        new ExecutionPoint(BuildStep.CopyStandardContent, ExecutionBehaviors.After),

                        // This one has a slightly higher priority as it removes
                        // stuff that the other plug-ins don't need to see.
                        new ExecutionPoint(BuildStep.GenerateIntermediateTableOfContents, ExecutionBehaviors.After, 1500)
                    };

                return executionPoints;
            }
        }

        /// <summary>
        /// This method is used by the Sandcastle Help File Builder to let the
        /// plug-in perform its own configuration.
        /// </summary>
        /// <param name="project">A reference to the active project</param>
        /// <param name="currentConfig">The current configuration XML fragment</param>
        /// <returns>A string containing the new configuration XML fragment</returns>
        /// <remarks>The configuration data will be stored in the help file
        /// builder project.</remarks>
        public string ConfigurePlugIn(SandcastleProject project, string currentConfig)
        {
            MessageBox.Show("This plug-in has no configurable settings",
                "Table of Contents Exclusion Plug-In", MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return currentConfig;
        }

        /// <summary>
        /// This method is used to initialize the plug-in at the start of the
        /// build process.
        /// </summary>
        /// <param name="buildProcess">A reference to the current build
        /// process.</param>
        /// <param name="configuration">The configuration data that the plug-in
        /// should use to initialize itself.</param>
        public void Initialize(BuildProcess buildProcess, XPathNavigator configuration)
        {
            builder = buildProcess;

            builder.ReportProgress("{0} Version {1}\r\n{2}", this.Name, this.Version, this.Copyright);

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

            // Scan the XML comments files.  The files aren't available soon
            // after this step and by now everything that other plug-ins may
            // have added to the collection should be there.
            if(context.BuildStep == BuildStep.CopyStandardContent)
            {
                builder.ReportProgress("Searching for comment members containing <tocexclude />...");

                foreach(XmlCommentsFile f in builder.CommentsFiles)
                    foreach(XmlNode member in f.Members.SelectNodes("member[count(.//tocexclude) > 0]/@name"))
                        exclusionList.Add(member.Value);

                builder.ReportProgress("Found {0} members to exclude from the TOC", exclusionList.Count);
                return;
            }

            if(exclusionList.Count == 0)
            {
                builder.ReportProgress("No members found to exclude");
                return;
            }

            builder.ReportProgress("Removing members from the TOC");

            toc = new XmlDocument();
            toc.Load(builder.WorkingFolder + "toc.xml");
            navToc = toc.CreateNavigator();

            // If a root namespace container node is present, we need to look
            // in it rather than the document root node.
            root = navToc.SelectSingleNode("topics/topic[starts-with(@id, 'R:')]");

            if(root == null)
                root = navToc.SelectSingleNode("topics");

            foreach(string id in exclusionList)
            {
                tocEntry = root.SelectSingleNode("//topic[@id='" + id + "']");

                // Ignore if null, it was probably excluded by the API filter
                if(tocEntry != null)
                {
                    // Remove the entry.  If this results in the parent
                    // being an empty node, remove it too.
                    do
                    {
                        tocParent = tocEntry.Clone();
                        hasParent = tocParent.MoveToParent();

                        builder.ReportProgress("    Removing '{0}'", tocEntry.GetAttribute("id", String.Empty));
                        tocEntry.DeleteSelf();

                    } while(hasParent && !tocParent.HasChildren);
                }
            }

            toc.Save(builder.WorkingFolder + "toc.xml");
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <summary>
        /// This handles garbage collection to ensure proper disposal of the
        /// plug-in if not done explicity with <see cref="Dispose()"/>.
        /// </summary>
        ~TocExcludePlugIn()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// This implements the Dispose() interface to properly dispose of
        /// the plug-in object.
        /// </summary>
        /// <overloads>There are two overloads for this method.</overloads>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This can be overridden by derived classes to add their own
        /// disposal code if necessary.
        /// </summary>
        /// <param name="disposing">Pass true to dispose of the managed
        /// and unmanaged resources or false to just dispose of the
        /// unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Nothing to dispose of in this one
        }
        #endregion
    }
}
