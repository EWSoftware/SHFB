//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : HtmlBlockMarkdownExtension.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/04/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains a class used to parse nested HTML blocks for Markdown that needs to be rendered as HTML
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/20/2025  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using Markdig;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax;

using Sandcastle.Core.Markdown.Parsers;
using Sandcastle.Core.Markdown.Renderers;

namespace Sandcastle.Core.Markdown.Extensions;

/// <summary>
/// This class is a Markdown extension that post-processes the parsed document in order to parse the inner HTML
/// content as Markdown and splices the resulting blocks into the document in place of the original <c>HtmlBlock</c>.
/// </summary>
/// <remarks>Typically, Markdown does not do this.  However, since we're parsing and rendering MAML topics, it
/// is necessary to do so in order to ensure that all Markdown, even if nested within HTML or MAML elements, is
/// properly processed.</remarks>
public class HtmlBlockMarkdownExtension : IMarkdownExtension
{
    #region Private data members
    //=====================================================================

    private MarkdownPipeline currentPipeline;
    private readonly HashSet<string> blockTags, doNotParseTags;
    private bool isParsing;

    #endregion

    #region Constructor
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="blockTags">An enumerable list of tags that should be treated as blocks</param>
    /// <param name="doNotParseTags">An enumerable list of tags that should not be parsed for Markdown
    /// content (code, scripts, pre, style, textarea, etc.).</param>
    public HtmlBlockMarkdownExtension(IEnumerable<string> blockTags, IEnumerable<string> doNotParseTags)
    {
        this.blockTags = new HashSet<string>(blockTags, StringComparer.OrdinalIgnoreCase);
        this.doNotParseTags = new HashSet<string>(doNotParseTags, StringComparer.OrdinalIgnoreCase);
    }
    #endregion

    #region IMarkdownExtension implementation
    //=====================================================================

    /// <inheritdoc />
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        pipeline.DocumentProcessed -= PipelineOnDocumentProcessed;
        pipeline.DocumentProcessed += PipelineOnDocumentProcessed;
        pipeline.BlockParsers.Replace<HtmlBlockParser>(new ExtendedHtmlBlockParser(blockTags, doNotParseTags));
    }

    /// <inheritdoc />
    /// <remarks>This captures the built pipeline when the pipeline setup for the renderer is called.  The
    /// <c>DocumentProcessed</c> delegate only receives a <c>MarkdownDocument</c>, so we need a reference to the
    /// pipeline in order to parse inner HTML content using the same pipeline configuration.</remarks>
    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        currentPipeline = pipeline;
    }
    #endregion

    #region Methods
    //=====================================================================

    /// <summary>
    /// Searches the document for HTML blocks and attempts to parse their inner text as Markdown
    /// </summary>
    /// <param name="document">The document to parse</param>
    private void PipelineOnDocumentProcessed(MarkdownDocument document)
    {
        if(isParsing || currentPipeline is null || document is null)
            return;

        try
        {
            // Prevent going recursive if inner parsing also triggers DocumentProcessed
            isParsing = true;

            using var sw = new StringWriter();
            HtmlBlockRenderer htmlRenderer = new(sw);
            List<string> modifiedLines = [];
            StringBuilder blockText = new(10240);

            currentPipeline.Setup(htmlRenderer);

            for(int i = 0; i < document.Count; i++)
            {
                var block = document[i];

                if(block is not HtmlBlock htmlBlock)
                    continue;

                var linesGroup = htmlBlock.Lines;
                string reconstructedRaw = null;
                bool hasChanged;

                try
                {
                    modifiedLines.Clear();
                    blockText.Clear();
                    hasChanged = false;

                    for(int li = 0; li < linesGroup.Count; li++)
                    {
                        var slice = linesGroup.Lines[li];
                        blockText.AppendLine(slice.ToString());

                        // Try to parse the line as XML/HTML fragment.  The block may be spread over several
                        // lines.  So, if it's not well-formed, keep adding lines until it is or we run out
                        // of lines.
                        XDocument xdoc;

                        try
                        {
                            // NOTE: We assume here that we'll only get a single element.  However, if the text
                            // is structured such that an opening tag appears immediately after a closing tag
                            // with no intervening line break and the second element's content spans lines
                            // (e.g. ...</div><div>...\n), we'll get two elements which fails to parse because it
                            // needs a single root element.  There's nothing we can do about that here.  The fix
                            // is to edit the source document and split the second block element's opening tag
                            // onto a new line.
                            //
                            // Catching the exception can be slow while debugging since it still logs the
                            // exception to the debug window.  It runs much faster when not debugging.
                            xdoc = XDocument.Parse(blockText.ToString(), LoadOptions.PreserveWhitespace);
                        }
                        catch
                        {
                            // If we're out of lines, just keep the original text
                            if(li == linesGroup.Count - 1)
                                modifiedLines.Add(blockText.ToString());

                            continue;
                        }

                        // Find all text nodes within the parsed document
                        var textNodes = xdoc.DescendantNodes().OfType<XText>().ToList();

                        if(textNodes.Count == 0)
                        {
                            modifiedLines.Add(blockText.ToString());
                            blockText.Clear();
                            continue;
                        }

                        blockText.Clear();

                        foreach(var t in textNodes)
                        {
                            // Ignore blocks that should not be parsed and empty blocks
                            if(t.Parent == null || doNotParseTags.Contains(t.Parent.Name.LocalName) ||
                              String.IsNullOrWhiteSpace(t.Value))
                            {
                                continue;
                            }

                            // Parse the inner text as Markdown using the captured pipeline.  Render the
                            // resulting MarkdownDocument to HTML, then parse that HTML into XElement(s) so
                            // they can replace the original XText node.
                            MarkdownDocument md;

                            try
                            {
                                md = MarkdownParser.Parse(t.Value, currentPipeline);
                            }
                            catch
                            {
                                if(System.Diagnostics.Debugger.IsAttached)
                                    System.Diagnostics.Debugger.Break();

                                continue;
                            }

                            // Render the markdown document to HTML using the same pipeline configuration
                            string renderedHtml;

                            try
                            {
                                htmlRenderer.Render(md);
                                htmlRenderer.Writer.Flush();

                                renderedHtml = htmlRenderer.Writer.ToString()!;

                                htmlRenderer.ResetRenderer();
                            }
                            catch
                            {
                                if(System.Diagnostics.Debugger.IsAttached)
                                    System.Diagnostics.Debugger.Break();

                                continue;
                            }

                            // Remove the containing paragraph element and add back the leading and trailing
                            // whitespace if in a non-block element or another paragraph.
                            if((t.Parent!.Name.LocalName == "p" || !blockTags.Contains(t.Parent!.Name.LocalName)) &&
                              renderedHtml.StartsWith("<p>", StringComparison.OrdinalIgnoreCase))
                            {
                                blockText.Append(renderedHtml);
                                
                                int end = blockText.Length - 1;

                                while(end > 0 && renderedHtml[end] != '<')
                                    end--;

                                blockText.Remove(end, renderedHtml.Length - end);
                                blockText.Remove(0, 3);

                                end = 0;
                                
                                while(end < t.Value.Length && Char.IsWhiteSpace(t.Value[end]))
                                {
                                    blockText.Insert(0, t.Value[end]);
                                    end++;
                                }

                                if(end < t.Value.Length)
                                {
                                    end = t.Value.Length - 1;
                                    
                                    while(end >= 0 && Char.IsWhiteSpace(t.Value[end]))
                                        end--;

                                    if(end < t.Value.Length - 1)
                                        blockText.Append(t.Value.Substring(end + 1));
                                }

                                renderedHtml = blockText.ToString();
                                blockText.Clear();
                            }

                            if(String.IsNullOrEmpty(renderedHtml))
                                continue;

                            // Parse the rendered HTML fragment into XML nodes by wrapping in a root element
                            XDocument fragDoc;

                            try
                            {
                                fragDoc = XDocument.Parse("<root>" + renderedHtml + "</root>", LoadOptions.PreserveWhitespace);
                            }
                            catch
                            {
                                if(System.Diagnostics.Debugger.IsAttached)
                                    System.Diagnostics.Debugger.Break();

                                continue;
                            }

                            // Clone the nodes to be inserted
                            var replacementNodes = fragDoc.Root!.Nodes().Select<XNode, XNode>(
                                node =>
                                {
                                    return node switch
                                    {
                                        XElement el => new XElement(el),
                                        XText tx => new XText(tx.Value),
                                        XComment cm => new XComment(cm.Value),
                                        _ => null
                                    };

                                }).Where(n => n is not null).Cast<XNode>().ToArray();

                            if(replacementNodes.Length == 0)
                                continue;

                            // Replace the original text node with the rendered nodes
                            t.ReplaceWith(replacementNodes);
                            hasChanged = true;
                        }

                        // Serialize the modified XDocument back to a single-line representation for the block line
                        modifiedLines.Add(xdoc.ToString(SaveOptions.DisableFormatting));
                    }

                    if(hasChanged)
                        reconstructedRaw = String.Join(Environment.NewLine, modifiedLines);
                }
                catch
                {
                    if(System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();

                    reconstructedRaw = null;
                }

                if(String.IsNullOrWhiteSpace(reconstructedRaw))
                    continue;

                try
                {
                    // Parse the inner Markdown using the same pipeline
                    var innerDoc = MarkdownParser.Parse(reconstructedRaw, currentPipeline);

                    if(innerDoc == null || innerDoc.Count == 0)
                        continue;

                    if(htmlBlock.Parent is ContainerBlock parentContainer)
                    {
                        parentContainer.RemoveAt(i);

                        int insertIndex = i;

                        // Transfer blocks from the inner document to the parent container
                        while(innerDoc.Count > 0)
                        {
                            var blockToMove = innerDoc[0];
                            innerDoc.RemoveAt(0);
                            parentContainer.Insert(insertIndex, blockToMove);
                            insertIndex++;
                        }

                        i = insertIndex - 1;
                    }
                }
                catch
                {
                    if(System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();

                    continue;
                }
            }
        }
        finally
        {
            isParsing = false;
        }
    }
    #endregion
}
