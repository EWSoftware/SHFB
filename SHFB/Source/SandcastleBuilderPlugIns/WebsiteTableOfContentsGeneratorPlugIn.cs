//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : WebsiteTableOfContentsGeneratorPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)  Based on code by Sam Harwell
// Updated : 04/16/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved.
//
// This file contains a plug-in that is used to generate table of contents information for website-based
// presentation styles.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/27/2022  EFW  Created the code
//===============================================================================================================

// Ignore spelling: fa wbr

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Sandcastle.Core;
using Sandcastle.Core.PresentationStyle.Transformation;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class is used to generate table of contents information for website-based presentation
    /// styles.
    /// </summary>
    /// <remarks>This is a presentation style dependency plug-in and has no configurable elements.  As such, it
    /// is hidden.</remarks>
    [HelpFileBuilderPlugInExport("Website Table of Contents Generator", Version = AssemblyInfo.ProductVersion,
      Copyright = AssemblyInfo.Copyright,
      Description = "This plug-in generates table of contents information for website-based presentation styles.",
      IsHidden = true)]
    public sealed class WebsiteTableOfContentsGeneratorPlugIn : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private List<ExecutionPoint> executionPoints;
        private BuildProcess builder;

        private Dictionary<string, XElement> tocEntries;
        private List<XElement> parentTopics;
        private string tocPath, rootBreadcrumbTitleText;

        #endregion

        #region IPlugIn implementation
        //=====================================================================

        /// <inheritdoc/>
        public IEnumerable<ExecutionPoint> ExecutionPoints
        {
            get
            {
                if(executionPoints == null)
                    executionPoints = new List<ExecutionPoint>
                    {
                        new ExecutionPoint(BuildStep.BuildTopics, ExecutionBehaviors.BeforeAndAfter)
                    };

                return executionPoints;
            }
        }

        /// <inheritdoc />
        public void Initialize(BuildProcess buildProcess, XElement configuration)
        {
            builder = buildProcess;

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright);
        }

        /// <inheritdoc />
        public void Execute(ExecutionContext context)
        {
            if(context == null)
                throw new ArgumentNullException(nameof(context));

            if(context.Behavior == ExecutionBehaviors.Before)
            {
                builder.ReportProgress("Loading TOC info and adding unique IDs to parent nodes");

                XDocument helpToc = XDocument.Load(Path.Combine(builder.WorkingFolder, "toc.xml"));

                // Add a unique ID to each topic that has children.  These will have a TOC fragment file
                // generated for them containing the child elements.
                tocEntries = helpToc.Descendants("topic").ToDictionary(k => k.Attribute("id").Value, v => v);
                parentTopics = new List<XElement>
                {
                    helpToc.Root
                };

                helpToc.Root.Add(new XAttribute("parentId", "Root"));

                foreach(var t in tocEntries.Values.Where(te => te.HasElements))
                {
                    t.Add(new XAttribute("parentId", t.GenerateUniqueId()));
                    parentTopics.Add(t);
                }

                tocPath = builder.PresentationStyle.TopicTranformation.ScriptPath;

                int pathEnd = tocPath.Substring(0, tocPath.Length - 1).LastIndexOf('/') + 1;

                tocPath = tocPath.Substring(0, pathEnd) + "toc/";

                if(builder.PresentationStyle.TopicTranformation.TransformationArguments.TryGetValue("RootBreadcrumbTitleText",
                  out var btArg))
                {
                    rootBreadcrumbTitleText = btArg.Value;
                }

                if(String.IsNullOrWhiteSpace(rootBreadcrumbTitleText))
                    rootBreadcrumbTitleText = "Docs";

                // Register the script to load the initial page TOC on startup and also add the TOC file metadata
                // when the topic starts to be rendered.
                builder.PresentationStyle.TopicTranformation.RenderStarting += this.TopicTranformation_RenderStarting;
                return;
            }

            builder.ReportProgress("Generating TOC fragment files");

            string file, title, subtopicId, fileUrl, tocFolder = Path.Combine(builder.WorkingFolder, "Output",
                "Website", "toc");

            if(!Directory.Exists(tocFolder))
                Directory.CreateDirectory(tocFolder);

            var topicTitles = new Dictionary<string, string>();

            foreach(var t in tocEntries.Values)
            {
                file = t.Attribute("file")?.Value;

                if(file != null)
                {
                    // The title isn't available until now as the static parts are resolved from shared content items
                    string topicFile = Path.Combine(builder.WorkingFolder, @"Output\Website\html", file + ".htm");

                    var head = ComponentUtilities.XmlStreamAxis(topicFile, "head").FirstOrDefault();

                    if(head == null)
                        throw new InvalidOperationException($"Head element not found in topic file {topicFile}");

                    var tocTitle = head.Elements("meta").Where(m => m.Attribute("name")?.Value == "Title").FirstOrDefault();
                    title = tocTitle?.Attribute("content")?.Value ?? head.Element("title")?.Value;

                    if(title == null)
                        throw new InvalidOperationException("Unable to determine topic title");

                    topicTitles.Add(file, title);
                }
            }

            // After all of the topics have been generated, create the TOC fragment files
            foreach(var parentNode in parentTopics)
            {
                XElement tocItems = new XElement("tocItems"), breadcrumbs = new XElement("breadcrumbs");
                XDocument tocFragment = new XDocument(new XDeclaration("1.0", "utf-8", null),
                    new XElement("toc", breadcrumbs, tocItems));
                string id = parentNode.Attribute("parentId").Value;

                foreach(var child in parentNode.Elements("topic"))
                {
                    subtopicId = child.Attribute("parentId")?.Value;
                    file = child.Attribute("file")?.Value;

                    if(file == null)
                    {
                        title = child.Attribute("title").Value;

                        // For empty container nodes, link to the first child with a topic
                        var firstChild = child.Descendants("topic").FirstOrDefault(n => n.Attribute("file") != null);

                        if(firstChild != null)
                            file = firstChild.Attribute("file").Value;
                        else
                        {
                            // Unlikely, but if this happens, we've just got an empty container node or one with
                            // children that are all empty as well.
                            file = "#";
                        }
                    }
                    else
                        title = topicTitles[file];

                    fileUrl = $"{file}.htm";

                    if(subtopicId == null)
                    {
                        // Just a topic, no children
                        tocItems.Add(new XElement("li",
                            new XElement("a", new XAttribute("id", file),
                            new XAttribute("href", fileUrl), title.InsertWordBreakOpportunities())));
                    }
                    else
                    {
                        // This is a parent node with children.  They will be loaded on demand.  Use a space on
                        // the empty elements to prevent the browser converting them to self-closing elements
                        // when loaded which messes up the layout.
                        tocItems.Add(new XElement("li",
                            new XElement("a",
                                    new XAttribute("id", file),
                                    new XAttribute("class", "has-submenu"),
                                    new XAttribute("href", fileUrl),
                                new XElement("span",
                                    new XAttribute("data-tocFile", $"{tocPath}{subtopicId}.xml"),
                                    new XAttribute("class", "icon toggle"),
                                    new XAttribute("onclick", "ToggleExpandCollapse(this); return false;"),
                                        new XElement("i",
                                            new XAttribute("class", "fa fa-angle-right"), " ")),
                                title.InsertWordBreakOpportunities()),
                            new XElement("ul", new XAttribute("class", "toc-menu is-hidden"), " ")));
                    }
                }

                // Add the breadcrumb links
                if(parentNode.Parent != null)
                {
                    file = parentNode.Attribute("file")?.Value;

                    if(file == null)
                    {
                        // Empty container node
                        title = parentNode.Attribute("title").Value;
                        breadcrumbs.Add(new XElement("li", new XElement("p", title.InsertWordBreakOpportunities())));
                    }
                    else
                    {
                        title = topicTitles[file];
                        fileUrl = $"{file}.htm";
                        breadcrumbs.Add(new XElement("li",
                            new XElement("a", new XAttribute("href", fileUrl), title.InsertWordBreakOpportunities())));
                    }

                    XElement rootNode = parentTopics[0], parent = parentNode.Parent;

                    while(parent != rootNode)
                    {
                        file = parent.Attribute("file")?.Value;

                        if(file == null)
                        {
                            // Empty container node
                            title = parent.Attribute("title").Value;
                            breadcrumbs.AddFirst(new XElement("li", new XElement("p", title.InsertWordBreakOpportunities())));
                        }
                        else
                            title = topicTitles[file];

                        fileUrl = $"{file}.htm";

                        breadcrumbs.AddFirst(new XElement("li",
                            new XElement("a", new XAttribute("href", fileUrl), title.InsertWordBreakOpportunities())));

                        parent = parent.Parent;
                    }
                }

                // Add a breadcrumb link to the first root topic
                file = parentTopics[0].Descendants("topic").FirstOrDefault(
                    n => n.Attribute("file") != null)?.Attribute("file")?.Value;

                if(file == null)
                    fileUrl = "#";
                else
                    fileUrl = $"{file}.htm";

                breadcrumbs.AddFirst(new XElement("li",
                    new XElement("a", new XAttribute("href", fileUrl), rootBreadcrumbTitleText)));

                tocFragment.Save(Path.Combine(tocFolder, id + ".xml"),
                    builder.CurrentProject.IndentHtml ? SaveOptions.None : SaveOptions.DisableFormatting);
            };
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <inheritdoc/>
        public void Dispose()
        {
            // Nothing to dispose of in this one
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// This is used to add the TOC file metadata element and register the startup script when the document
        /// starts to be rendered.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void TopicTranformation_RenderStarting(object sender, RenderTopicEventArgs e)
        {
            // MS Help Viewer root content topics will not be in the help file TOC so ignore them
            if(tocEntries.TryGetValue(e.Key, out XElement tocEntry))
            {
                var head = e.TopicContent.Descendants("head").FirstOrDefault();

                if(head == null)
                    throw new InvalidOperationException("Rendered topic did not contain a head element");

                string tocFile;

                if(tocEntry.Parent == tocEntry.Document.Root)
                    tocFile = "Root";
                else
                    tocFile = tocEntry.Parent.Attribute("parentId").Value;

                head.Add(new XElement("meta",
                    new XAttribute("name", "tocFile"),
                    new XAttribute("content", $"{tocPath}{tocFile}.xml")));

                builder.PresentationStyle.TopicTranformation.RegisterStartupScript(100, "LoadTocFile(null, null);");
            }
        }
        #endregion
    }
}
