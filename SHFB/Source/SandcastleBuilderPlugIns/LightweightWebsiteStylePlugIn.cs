//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : LightweightWebsiteStylePlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)  Based on code by Sam Harwell
// Updated : 02/05/2016
// Note    : Copyright 2014-2016, Eric Woodruff, All rights reserved.
//           Portions Copyright 2014-2016, Sam Harwell, All rights reserved.
// Compiler: Microsoft Visual C#
//
// This file contains a plug-in that is used to add elements for the lightweight website style such as a search
// box and a table of contents in the topics similar to the current MSDN lightweight style.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/04/2014  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class is used to add elements for the lightweight website style such as a search box and a
    /// table of contents in the topics similar to the current MSDN lightweight style.
    /// </summary>
    /// <remarks>This is a presentation style dependency plug-in and has no configurable elements.  As such, it
    /// is hidden.</remarks>
    [HelpFileBuilderPlugInExport("Lightweight Website Style", Version = AssemblyInfo.ProductVersion,
      Copyright = AssemblyInfo.Copyright + "\r\nPortions Copyright \xA9 2014, Sam Harwell, All Rights Reserved",
      Description = "This plug-in embeds elements for the lightweight website style such as a search box and " +
        "a table of contents in the topics similar to the current MSDN lightweight style.", IsHidden = true)]
    public sealed class LightweightWebsiteStylePlugIn : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private List<ExecutionPoint> executionPoints;
        private BuildProcess builder;
        private string resizeToolTip;

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
                        new ExecutionPoint(BuildStep.GenerateSharedContent, ExecutionBehaviors.After),
                        new ExecutionPoint(BuildStep.GenerateFullTextIndex, ExecutionBehaviors.After)
                    };

                return executionPoints;
            }
        }

        /// <inheritdoc />
        public string ConfigurePlugIn(SandcastleProject project, string currentConfig)
        {
            MessageBox.Show("This plug-in has no configurable settings", "Lightweight Website Style Plug-In",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            return currentConfig;
        }

        /// <inheritdoc />
        public void Initialize(BuildProcess buildProcess, XPathNavigator configuration)
        {
            builder = buildProcess;

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright);
        }

        /// <inheritdoc />
        public void Execute(ExecutionContext context)
        {
            // Look up the resize tool tip in the shared content resource items file
            if(context.BuildStep == BuildStep.GenerateSharedContent)
            {
                string sharedContentFile = Directory.EnumerateFiles(builder.PresentationStyleResourceItemsFolder,
                    "shared*content*", SearchOption.AllDirectories).FirstOrDefault();

                if(sharedContentFile != null)
                {
                    XDocument doc = XDocument.Load(sharedContentFile);
                    var toolTip = doc.XPathSelectElement("content/item[@id='resizeToolTip']");

                    if(toolTip != null)
                        resizeToolTip = toolTip.Value;
                }

                if(String.IsNullOrWhiteSpace(resizeToolTip))
                {
                    builder.ReportWarning("LWW0001", "Unable to locate resizeToolTip resource item.  " +
                        "Using default text.");
                    resizeToolTip = "Click or drag to resize";
                }

                return;
            }

            builder.ReportProgress("Adding lightweight search and TOC elements to each topic...");

            // Load the web TOC generated by SandcastleHtmlExtract
            XDocument webtoc = XDocument.Load(Path.Combine(builder.WorkingFolder, "WebTOC.xml"));

            // Remove the Id attribute from all nodes that contain a Url attribute
            foreach(XElement element in webtoc.XPathSelectElements("//node()[@Id and @Url]"))
                element.Attribute("Id").Remove();

            // Generate the TOC fragments
            Directory.CreateDirectory(Path.Combine(builder.WorkingFolder, "Output", "Website", "toc"));
            List<XElement> elements = new List<XElement>(webtoc.XPathSelectElements("//node()"));

            // Work around the problem of the root node only showing one type if there are no conceptual topics
            // or they are listed after the namespaces.  In such cases, expand the root node so that all
            // namespaces are listed on the default page.  This avoids confusion caused by only seeing one
            // namespace when the root node is collapsed by default.
            var firstElement = webtoc.Root.Elements().First();

            Parallel.ForEach(elements,
              new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 20 }, element =>
            {
                XDocument pageChildren = new XDocument(new XDeclaration("1.0", "utf-8", null));
                XElement copy = new XElement(element);

                pageChildren.Add(copy);

                foreach(XElement child in copy.Elements())
                {
                    if(!child.HasElements)
                        continue;

                    child.SetAttributeValue("HasChildren", true);
                    child.RemoveNodes();
                }

                string uri = null;

                if(copy.Attribute("Url") != null)
                    uri = copy.Attribute("Url").Value;
                else
                    if(copy.Attribute("Id") != null)
                        uri = copy.Attribute("Id").Value;
                    else
                        if(copy.Name.LocalName == "HelpTOC")
                            uri = "roottoc.html";
                        else
                            throw new NotImplementedException();

                string fileId = Path.GetFileNameWithoutExtension(uri.Substring(uri.LastIndexOf('/') + 1));

                if(element.HasElements)
                {
                    using(XmlWriter writer = XmlWriter.Create(Path.Combine(builder.WorkingFolder, "Output",
                      "Website", "toc", fileId + ".xml")))
                    {
                        pageChildren.WriteTo(writer);
                    }
                }

                if(string.IsNullOrEmpty((string)copy.Attribute("Url")))
                    return;

                // Generate the lightweight TOC pane
                XElement current = element;
                IEnumerable<XElement> parents = current.XPathSelectElements("parent::HelpTOCNode/ancestor::HelpTOCNode");
                XElement parent = current.XPathSelectElement("parent::HelpTOCNode");
                IEnumerable<XElement> siblings = current.Parent.Elements("HelpTOCNode");
                IEnumerable<XElement> children = current.Elements("HelpTOCNode");

                XElement tocNav = new XElement("div", new XAttribute("id", "tocNav"));

                // The documentation root
                tocNav.Add(GenerateTocRoot(parent == null && !children.Any()));

                // All the ancestors *except* the immediate parent, always collapsed by default
                foreach(XElement ancestor in parents)
                    tocNav.Add(GenerateTocAncestor(ancestor, 0, false));

                // The immediate parent is expanded if the current node has no children
                if(parent != null)
                {
                    bool expanded = !current.HasElements;
                    int level = expanded ? 1 : 0;
                    tocNav.Add(GenerateTocAncestor(parent, level, expanded));
                }

                // The siblings of the current node are shown if the parent is expanded, otherwise only the
                // current node is shown.
                foreach(XElement sibling in siblings)
                {
                    bool showSiblings;
                    int level;

                    if(parent == null && !current.HasElements)
                    {
                        showSiblings = true;
                        level = 1;
                    }
                    else
                    {
                        showSiblings = !current.HasElements;
                        level = current.HasElements || parent == null ? 1 : 2;
                    }

                    tocNav.Add(GenerateTocSibling(current, sibling, level, showSiblings));
                }

                // The children of the current node, if any exist, are always shown by default
                foreach(XElement child in children)
                    tocNav.Add(GenerateTocChild(child));

                XElement resizableBar =
                    new XElement("div",
                        new XAttribute("id", "tocResizableEW"),
                        new XAttribute("onmousedown", "OnMouseDown(event);"),
                    // Add empty text to force full start/end element.  This allows for proper display in the
                    // browser while still allowing XHTML output that's valid for post-processing.
                        new XText(String.Empty));

                XElement resizeUi =
                    new XElement("div",
                        new XAttribute("id", "TocResize"),
                        new XAttribute("class", "tocResize"),
                        new XElement("img",
                            new XAttribute("id", "ResizeImageIncrease"),
                            new XAttribute("src", "../icons/TocOpen.gif"),
                            new XAttribute("onclick", "OnIncreaseToc()"),
                            new XAttribute("alt", resizeToolTip),
                            new XAttribute("title", resizeToolTip)),
                        new XElement("img",
                            new XAttribute("id", "ResizeImageReset"),
                            new XAttribute("src", "../icons/TocClose.gif"),
                            new XAttribute("style", "display:none"),
                            new XAttribute("onclick", "OnResetToc()"),
                            new XAttribute("alt", resizeToolTip),
                            new XAttribute("title", resizeToolTip)));

                XElement leftNav =
                    new XElement("div",
                        new XAttribute("class", "leftNav"),
                        new XAttribute("id", "leftNav"),
                        tocNav,
                        resizableBar,
                        resizeUi);

                string path = Path.Combine(builder.WorkingFolder, @"Output\Website", current.Attribute("Url").Value);
                string outputFile = File.ReadAllText(path, Encoding.UTF8);

                // Search box
                int pos = outputFile.IndexOf("<div class=\"pageHeader\"", StringComparison.Ordinal);

                if(pos != -1)
                {
                    pos = outputFile.IndexOf("</div>", pos, StringComparison.Ordinal);

                    if(pos != -1)
                        outputFile = outputFile.Insert(pos, "<form id=\"SearchForm\" method=\"get\" " +
                            "action=\"#\" onsubmit=\"javascript:TransferToSearchPage(); return false;\">" +
                            "<input id=\"SearchTextBox\" type=\"text\" maxlength=\"200\" />" +
                            "<button id=\"SearchButton\" type=\"submit\"></button>" +
                            "</form>");
                }

                // Left nav
                pos = outputFile.IndexOf("<div class=\"topicContent\"", StringComparison.Ordinal);

                if(pos != -1)
                    outputFile = outputFile.Insert(pos, leftNav.ToString(SaveOptions.DisableFormatting));

                if(element == firstElement && firstElement.HasElements)
                {
                    outputFile = outputFile.Replace("</html>", @"<script type=""text/javascript"">
<!--
    var tocNav = document.getElementById(""tocNav"");
    var anchor = tocNav.children[0].children[0];
    Toggle(anchor);
    tocNav.children[0].className += "" current"";
-->
</script>
</html>");
                }

                File.WriteAllText(path, outputFile, Encoding.UTF8);
            });
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

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Generate the root TOC node
        /// </summary>
        /// <param name="expanded">True if expanded false if not</param>
        /// <returns>The root TOC node</returns>
        private XElement GenerateTocRoot(bool expanded)
        {
            XElement result =
                new XElement("div",
                    new XAttribute("class", "toclevel0"),
                    new XAttribute("data-toclevel", "0"),
                    new XElement("a",
                        new XAttribute("class", expanded ? "tocExpanded" : "tocCollapsed"),
                        new XAttribute("onclick", "javascript: Toggle(this);"),
                        new XAttribute("href", "#!")),
                    new XElement("a",
                        new XAttribute("data-tochassubtree", "true"),
                        new XAttribute("href", Path.GetFileName(builder.DefaultTopicFile)),
                        new XAttribute("title", builder.ResolvedHelpTitle),
                        new XAttribute("tocid", "roottoc"),
                        new XText(builder.SubstitutionTags.TransformText(builder.CurrentProject.HelpTitle))));

            if(expanded)
                result.SetAttributeValue("data-childrenloaded", "true");

            return result;
        }

        /// <summary>
        /// Generate a TOC ancestor node
        /// </summary>
        /// <param name="ancestor">The TOC ancestor</param>
        /// <param name="level">The level of the node</param>
        /// <param name="expanded">True if expanded, false if not</param>
        /// <returns>The TOC ancestor node</returns>
        private static XElement GenerateTocAncestor(XElement ancestor, int level, bool expanded)
        {
            string file, tocid, tocTitle;

            if(ancestor.Attribute("Url") != null)
            {
                file = "../" + ancestor.Attribute("Url").Value;
                tocid = Path.GetFileNameWithoutExtension(file);
            }
            else
            {
                // Probably an empty container node.  Use the first child with a URL as the target
                var targetChild = ancestor.Descendants("HelpTOCNode").FirstOrDefault(n => n.Attribute("Url") != null);

                if(targetChild != null)
                    file = "../" + ancestor.Attribute("Url").Value;
                else
                    file = "#!";

                if(ancestor.Attribute("Id") != null)
                    tocid = ancestor.Attribute("Id").Value;
                else
                    tocid = Path.GetFileNameWithoutExtension(file);
            }

            tocTitle = ancestor.Attribute("Title").Value;

            XElement result =
                new XElement("div",
                    new XAttribute("class", "toclevel" + level),
                    new XAttribute("data-toclevel", level),
                    new XElement("a",
                        new XAttribute("class", expanded ? "tocExpanded" : "tocCollapsed"),
                        new XAttribute("onclick", "javascript: Toggle(this);"),
                        new XAttribute("href", "#!")),
                    new XElement("a",
                        new XAttribute("data-tochassubtree", "true"),
                        new XAttribute("href", file),
                        new XAttribute("title", tocTitle),
                        new XAttribute("tocid", tocid),
                        new XText(tocTitle)));

            if(expanded)
                result.SetAttributeValue("data-childrenloaded", true);

            return result;
        }

        /// <summary>
        /// Generate a TOC sibling node
        /// </summary>
        /// <param name="current">The current node</param>
        /// <param name="sibling">The sibling node</param>
        /// <param name="level">The level of the node</param>
        /// <param name="showSiblings">True to show siblings, false to keep them hidden</param>
        /// <returns>The TOC sibling node</returns>
        private static XElement GenerateTocSibling(XElement current, XElement sibling, int level, bool showSiblings)
        {
            XElement glyphElement;
            string targetId, targetTocId, currentId, file, tocTitle, styleClassSuffix;

            if(sibling.Attribute("Url") != null)
            {
                targetId = sibling.Attribute("Url").Value;
                targetTocId = Path.GetFileNameWithoutExtension(targetId);
            }
            else
            {
                // Probably an empty container node.  Use the first child with a URL as the target
                var targetChild = sibling.Descendants("HelpTOCNode").FirstOrDefault(n => n.Attribute("Url") != null);

                if(targetChild != null)
                    targetId = targetChild.Attribute("Url").Value;
                else
                    targetId = "#!";

                if(sibling.Attribute("Id") != null)
                    targetTocId = sibling.Attribute("Id").Value;
                else
                    targetTocId = "#!";
            }

            if(current.Attribute("Url") != null)
                currentId = current.Attribute("Url").Value;
            else
                currentId = "#!";

            file = Path.GetFileName(targetId);
            tocTitle = sibling.Attribute("Title").Value;
            styleClassSuffix = (targetId == currentId) ? " current" : string.Empty;

            if(targetId != currentId && !showSiblings)
                return null;

            if(sibling.HasElements)
            {
                glyphElement =
                    new XElement("a",
                        new XAttribute("class", (targetId == currentId) ? "tocExpanded" : "tocCollapsed"),
                        new XAttribute("onclick", "javascript: Toggle(this);"),
                        new XAttribute("href", "#!"));
            }
            else
                glyphElement = null;

            XElement result =
                new XElement("div",
                    new XAttribute("class", "toclevel" + level + styleClassSuffix),
                    new XAttribute("data-toclevel", level),
                    glyphElement,
                    new XElement("a",
                        new XAttribute("data-tochassubtree", sibling.HasElements),
                        new XAttribute("href", file),
                        new XAttribute("title", tocTitle),
                        new XAttribute("tocid", targetTocId),
                        new XText(tocTitle)));

            if(sibling.HasElements && targetId == currentId)
                result.SetAttributeValue("data-childrenloaded", true);

            return result;
        }

        /// <summary>
        /// Generate a TOC child node
        /// </summary>
        /// <param name="child">The child node</param>
        /// <returns>The TOC child node</returns>
        private static XElement GenerateTocChild(XElement child)
        {
            XElement glyphElement;
            string file, tocid, tocTitle;
            int level = 2;

            // Some items in the TOC do not have actual pages associated with them
            if(child.Attribute("Url") != null)
            {
                file = Path.GetFileName(child.Attribute("Url").Value);
                tocid = Path.GetFileNameWithoutExtension(file);
            }
            else
            {
                // Probably an empty container node.  Use the first child with a URL as the target
                var targetChild = child.Descendants("HelpTOCNode").FirstOrDefault(n => n.Attribute("Url") != null);

                if(targetChild != null)
                    file = Path.GetFileName(targetChild.Attribute("Url").Value);
                else
                    file = "#!";

                if(child.Attribute("Id") != null)
                    tocid = child.Attribute("Id").Value;
                else
                    tocid = "#!";
            }

            tocTitle = child.Attribute("Title").Value;

            if(child.HasElements)
            {
                glyphElement =
                    new XElement("a",
                        new XAttribute("class", "tocCollapsed"),
                        new XAttribute("onclick", "javascript: Toggle(this);"),
                        new XAttribute("href", "#!"));
            }
            else
                glyphElement = null;

            XElement result =
                new XElement("div",
                    new XAttribute("class", "toclevel" + level),
                    new XAttribute("data-toclevel", level),
                    glyphElement,
                    new XElement("a",
                        new XAttribute("data-tochassubtree", child.HasElements),
                        new XAttribute("href", file),
                        new XAttribute("title", tocTitle),
                        new XAttribute("tocid", tocid),
                        new XText(tocTitle)));

            return result;
        }
        #endregion
    }
}
