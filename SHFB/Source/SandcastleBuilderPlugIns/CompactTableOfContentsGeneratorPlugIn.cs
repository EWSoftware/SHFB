//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : CompactTableOfContentsGeneratorPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/18/2026
// Note    : Copyright 2026, Eric Woodruff, All rights reserved.
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
// 01/16/2026  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

using Sandcastle.Core;
using Sandcastle.Core.BuildEngine;
using Sandcastle.Core.PlugIn;
using Sandcastle.Core.PresentationStyle.Transformation;

namespace SandcastleBuilder.PlugIns;

/// <summary>
/// This plug-in class is used to generate table of contents information for website-based presentation
/// styles.
/// </summary>
/// <remarks>This is a presentation style dependency plug-in and has no configurable elements.  As such, it
/// is hidden.</remarks>
[HelpFileBuilderPlugInExport("Compact Table of Contents Generator", Version = AssemblyInfo.ProductVersion,
  Copyright = AssemblyInfo.Copyright,
  Description = "This plug-in generates table of contents information for website-based presentation styles.",
  IsHidden = true)]
public sealed class CompactTableOfContentsGeneratorPlugIn : IPlugIn
{
    #region TOC entry
    //=====================================================================

    /// <summary>
    /// Information for a table of contents entry
    /// </summary>
    private sealed class TocEntry
    {
        /// <summary>
        /// The topic ID
        /// </summary>
        [JsonIgnore]
        public string TopicId { get; }

        /// <summary>
        /// The file ID (name)
        /// </summary>
        [JsonPropertyName("f")]
        public string FileId { get; set; }

        /// <summary>
        /// The TOC title
        /// </summary>
        [JsonIgnore]
        public string Title { get; set; }

        /// <summary>
        /// The title index
        /// </summary>
        [JsonPropertyName("t")]
        public int TitleIndex { get; set; }

        /// <summary>
        /// The fragment index for child content if the topic has children
        /// </summary>
        [JsonPropertyName("c")]
        public int? FragmentIndex { get; set; }

        /// <summary>
        /// True if this is an empty container node, false if not
        /// </summary>
        [JsonIgnore]
        public bool IsEmptyContainer => this.Title == null;

        /// <summary>
        /// The XML element containing the TOC information
        /// </summary>
        [JsonIgnore]
        public XElement TocElement { get; }

        /// <summary>
        /// The index of the element
        /// </summary>
        [JsonIgnore]
        public int Index { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="t">The XML element containing the TOC entry information</param>
        /// <param name="title">A title for the root node</param>
        public TocEntry(XElement t, string title = null)
        {
            this.TocElement = t;
            this.TopicId = t.Attribute("id")?.Value ?? "Root";
            this.FileId = t.Attribute("file")?.Value;
            this.Title = title ?? t.Attribute("title")?.Value;
        }
    }
    #endregion

    #region Topic TOC info
    //=====================================================================

    /// <summary>
    /// Table of contents fragment
    /// </summary>
    private sealed class TocFragment
    {
        /// <summary>
        /// The fragment index
        /// </summary>
        [JsonIgnore]
        public int Index { get; set; }

        /// <summary>
        /// Breadcrumb topic IDs for the fragment
        /// </summary>
        [JsonPropertyName("b")]
        public List<int> Breadcrumbs { get; } = [];

        /// <summary>
        /// The topics in this fragment
        /// </summary>
        [JsonPropertyName("t")]
        public List<int> Topics { get; } = [];
    }
    #endregion

    #region Table of contents information
    //=====================================================================

    /// <summary>
    /// The table of contents
    /// </summary>
    private sealed class TableOfContents
    {
        /// <summary>
        /// Topic titles
        /// </summary>
        [JsonPropertyName("titles")]
        public List<string> Titles { get; set; }

        /// <summary>
        /// Topics
        /// </summary>
        [JsonPropertyName("topics")]
        public List<TocEntry> Topics { get; set; }

        /// <summary>
        /// TOC fragments
        /// </summary>
        [JsonPropertyName("fragments")]
        public List<TocFragment> Fragments { get; set; }
    }
    #endregion

    #region Private data members
    //=====================================================================

    private IBuildProcess builder;

    private Dictionary<string, TocEntry> tocEntries;
    private List<TocFragment> fragments;

    private static readonly JsonSerializerOptions options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    #endregion

    #region IPlugIn implementation
    //=====================================================================

    /// <inheritdoc/>
    public IEnumerable<ExecutionPoint> ExecutionPoints { get; } =
    [
        new ExecutionPoint(BuildStep.BuildTopics, ExecutionBehaviors.BeforeAndAfter)
    ];

    /// <inheritdoc />
    public void Initialize(IBuildProcess buildProcess, XElement configuration)
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
            builder.ReportProgress("Loading table of contents info for client-side TOC");

            XDocument helpToc = XDocument.Load(Path.Combine(builder.WorkingFolder, "toc.xml"));

            tocEntries = [];
            fragments = [];

            string rootBreadcrumbTitleText = null;

            if(builder.PresentationStyle.TopicTransformation.TransformationArguments.TryGetValue(
              "RootBreadcrumbTitleText", out var btArg))
            {
                rootBreadcrumbTitleText = btArg.Value;
            }

            if(String.IsNullOrWhiteSpace(rootBreadcrumbTitleText))
                rootBreadcrumbTitleText = "Docs";

            tocEntries.Add("Root", new TocEntry(helpToc.Root, rootBreadcrumbTitleText) { FragmentIndex = 0 });

            int idx = 1, fragmentIndex = 1;

            foreach(var t in helpToc.Descendants("topic"))
            {
                // There's an odd case were we can get a duplicate TOC entry if using the wrong framework
                // type to document assemblies from a different framework type (e.g. using .NET Framework for
                // a .NET Standard assembly).  These are typically solved by choosing the proper framework
                // version in the documentation project.
                string id = t.Attribute("id").Value;

                if(!tocEntries.ContainsKey(id))
                {
                    var te = new TocEntry(t)
                    {
                        Index = idx++,
                        FragmentIndex = t.HasElements ? fragmentIndex++ : null
                    };

                    tocEntries.Add(id, te);
                }
                else
                {
                    builder.ReportWarning("CTOC0001", "Duplicate key found in TOC.  This was unexpected.  " +
                        "Do you have the right framework version selected in the documentation project?  " +
                        "Duplicate key: {0}", id);
                }
            }

            // Register the script to load the initial page TOC on startup and also add the TOC file metadata
            // when the topic starts to be rendered.
            builder.PresentationStyle.TopicTransformation.RenderStarting += this.TopicTransformation_RenderStarting;
            return;
        }

        builder.ReportProgress("Generating website TOC information");

        // The title isn't available until now as the static parts are resolved from shared content items.
        // API member titles can be duplicated frequently so we'll just store the unique titles to reduce the
        // TOC file size.
        Dictionary<string, int> uniqueTitles = [];
        int titleIdx = 0;

        foreach(var t in tocEntries.Values)
        {
            if(t.FileId != null)
            {
                string topicFile = Path.Combine(builder.WorkingFolder, "Output", "Website", "html", t.FileId + ".htm");

                var head = ComponentUtilities.XmlStreamAxis(topicFile, "head").FirstOrDefault() ??
                    throw new InvalidOperationException($"Head element not found in topic file {topicFile}");

                var tocTitle = head.Elements("meta").Where(m => m.Attribute("name")?.Value == "Title").FirstOrDefault();

                t.Title = tocTitle?.Attribute("content")?.Value ?? head.Element("title")?.Value ??
                    throw new InvalidOperationException("Unable to determine topic title");
            }
            else
            {
                // For empty container nodes, link to the first child with a topic
                var firstChild = t.TocElement.Descendants("topic").FirstOrDefault(n => n.Attribute("file") != null);

                if(firstChild != null)
                    t.FileId = firstChild.Attribute("file").Value;
                else
                {
                    // Unlikely, but if this happens, we've just got an empty container node or one with
                    // children that are all empty as well.
                    t.FileId = "#";
                }
            }

            if(!uniqueTitles.TryGetValue(t.Title, out int ti))
            {
                ti = titleIdx++;
                uniqueTitles.Add(t.Title, ti);
            }

            t.TitleIndex = ti;
        }

        // After all of the topics have been generated, create the TOC fragments
        foreach(var parentTopic in tocEntries.Values.Where(t => t.FragmentIndex != null))
        {
            TocFragment tocFragment = new() { Index = parentTopic.FragmentIndex.Value };

            fragments.Add(tocFragment);

            foreach(var child in parentTopic.TocElement.Elements("topic"))
                tocFragment.Topics.Add(tocEntries[child.Attribute("id").Value].Index);

            // Add the breadcrumb links
            if(parentTopic.TocElement.Parent != null)
            {
                var te = tocEntries[parentTopic.TocElement.Attribute("id").Value];
                tocFragment.Breadcrumbs.Add(te.Index);

                XElement parent = parentTopic.TocElement.Parent;

                while(parent.Parent != null)
                {
                    tocFragment.Breadcrumbs.Add(tocEntries[parent.Attribute("id").Value].Index);
                    parent = parent.Parent;
                }
            }

            // A reference to the root will be added by the script so we don't need to add it here
            tocFragment.Breadcrumbs.Reverse();
        }

        var toc = new TableOfContents
        {
            Titles = [.. uniqueTitles.OrderBy(kv => kv.Value).Select(kv => kv.Key)],
            Topics = [.. tocEntries.Values.OrderBy(t => t.Index)],
            Fragments = [.. fragments.OrderBy(f => f.Index)]
        };

        File.WriteAllText(Path.Combine(builder.WorkingFolder, "Output", "Website", "toc.json"),
            JsonSerializer.Serialize(toc, options));
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
    private void TopicTransformation_RenderStarting(object sender, RenderTopicEventArgs e)
    {
        // MS Help Viewer root content topics will not be in the help file TOC so ignore them
        if(tocEntries.TryGetValue(e.Key, out TocEntry te))
        {
            var head = e.TopicContent.Descendants("head").FirstOrDefault() ??
                throw new InvalidOperationException("Rendered topic did not contain a head element");

            TocEntry parent = tocEntries[te.TocElement.Parent?.Attribute("id")?.Value ?? "Root"];

            head.Add(new XElement("meta",
                new XAttribute("name", "tocParentId"),
                new XAttribute("content", parent.FragmentIndex)));

            builder.PresentationStyle.TopicTransformation.RegisterStartupScript(100, "LoadToc();");
        }
    }
    #endregion
}
