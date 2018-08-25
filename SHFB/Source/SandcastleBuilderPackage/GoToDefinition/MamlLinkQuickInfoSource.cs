//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : MamlLinkQuickInfoSource.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/06/2015
// Note    : Copyright 2014-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that determines whether or not quick info should be shown for specific MAML
// elements and what is should contain.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
//===============================================================================================================
// 12/01/2014  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: xlink

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace SandcastleBuilder.Package.GoToDefinition
{
    /// <summary>
    /// This class is used to determine whether or not quick info should be shown for specific MAML elements and
    /// what it should contain.
    /// </summary>
    internal sealed class MamlLinkQuickInfoSource : IQuickInfoSource
    {
        #region Private data members
        //=====================================================================

        private SVsServiceProvider serviceProvider;
        private ITextBuffer textBuffer;
        private MamlLinkQuickInfoSourceProvider provider;
        private bool ctrlClickEnabled;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceProvider">The service provider to use</param>
        /// <param name="buffer">The buffer to use</param>
        /// <param name="provider">The quick info source provider to use</param>
        /// <param name="ctrlClickEnabled">True if Ctrl+Click on definition is enabled, false if not</param>
        public MamlLinkQuickInfoSource(SVsServiceProvider serviceProvider, ITextBuffer buffer,
          MamlLinkQuickInfoSourceProvider provider, bool ctrlClickEnabled)
        {
            this.serviceProvider = serviceProvider;
            this.textBuffer = buffer;
            this.provider = provider;
            this.ctrlClickEnabled = ctrlClickEnabled;
        }
        #endregion

        #region IQuickInfoSource implementation
        //=====================================================================

        /// <inheritdoc />
        public void Dispose()
        {
            // Nothing to dispose of for this one
        }

        /// <inheritdoc />
        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent,
          out ITrackingSpan applicableToSpan)
        {
            applicableToSpan = null;

            var tagAggregator = provider.AggregatorFactory.CreateTagAggregator<IClassificationTag>(session.TextView);
            var triggerPoint = session.GetTriggerPoint(textBuffer.CurrentSnapshot);

            if(triggerPoint != null)
            {
                SnapshotSpan tagSpan;
                var lineSpan = triggerPoint.Value.GetContainingLine();
                string elementName = null, attrName = null, name, spanText;
                UIElement content;

                // Get the tags for the line containing the mouse point
                foreach(IMappingTagSpan<IClassificationTag> curTag in tagAggregator.GetTags(
                  new SnapshotSpan(lineSpan.Start, lineSpan.End)))
                {
                    name = curTag.Tag.ClassificationType.Classification.ToLowerInvariant();

                    switch(name)
                    {
                        case "xml name":
                            tagSpan = curTag.Span.GetSpans(textBuffer).First();
                            elementName = tagSpan.GetText();
                            break;

                        case "xml attribute":
                            tagSpan = curTag.Span.GetSpans(textBuffer).First();
                            attrName = tagSpan.GetText();
                            break;

                        case "xml attribute value":
                            tagSpan = curTag.Span.GetSpans(textBuffer).First();

                            if(tagSpan.Contains(triggerPoint.Value))
                            {
                                if(((elementName == "image" || elementName == "link") && attrName == "xlink:href") ||
                                  (elementName == "topic" && attrName == "id"))
                                {
                                    content = this.CreateInfoText(elementName, tagSpan.GetText());

                                    if(content != null)
                                    {
                                        applicableToSpan = textBuffer.CurrentSnapshot.CreateTrackingSpan(tagSpan,
                                            SpanTrackingMode.EdgeExclusive);

                                        quickInfoContent.Add(content);
                                    }
                                }

                                return;
                            }
                            break;

                        case "xml text":
                            tagSpan = curTag.Span.GetSpans(textBuffer).First();

                            if(tagSpan.Contains(triggerPoint.Value))
                            {
                                spanText = tagSpan.GetText().Trim();
                                content = null;

                                switch(elementName)
                                {
                                    case "codeEntityReference":
                                        if(spanText.IsCodeEntityReference())
                                            content = this.CreateInfoText(elementName, tagSpan.GetText());
                                        break;

                                    case "codeReference":
                                        if(spanText.IsCodeReferenceId())
                                            content = this.CreateInfoText(elementName, tagSpan.GetText());
                                        break;

                                    case "token":
                                        content = this.CreateInfoText(elementName, tagSpan.GetText());
                                        break;

                                    default:
                                        // We only get info about the current line so we may just get some XML
                                        // text if the starting tag is on a prior line.  In such cases, see if
                                        // the text looks like an entity reference or a code reference ID.  If
                                        // so, offer it as a clickable link.  If not, ignore it.
                                        if(String.IsNullOrWhiteSpace(elementName))
                                            if(spanText.IsCodeEntityReference())
                                                content = this.CreateInfoText("codeEntityReference", tagSpan.GetText());
                                            else
                                                if(spanText.IsCodeReferenceId())
                                                    content = this.CreateInfoText("codeReference", tagSpan.GetText());
                                        break;
                                }

                                if(content != null)
                                {
                                    // Ignore any leading whitespace on the span so that the pop-up open directly
                                    // under the text.
                                    spanText = tagSpan.GetText();

                                    int offset = spanText.Length - spanText.TrimStart().Length;

                                    applicableToSpan = textBuffer.CurrentSnapshot.CreateTrackingSpan(
                                        tagSpan.Start + offset, tagSpan.Length - offset, SpanTrackingMode.EdgeExclusive);

                                    quickInfoContent.Add(content);
                                }
                                return;
                            }
                            break;

                        default:
                            break;
                    }
                } 
            }
        }

        /// <summary>
        /// This is used to get the content to add to the quick info by looking up the given topic ID to get its
        /// title and filename if possible.
        /// </summary>
        /// <param name="elementName">The element name for which to create content</param>
        /// <param name="id">The ID to look up if necessary</param>
        /// <returns>The content to add to the quick info (a text block element containing the additional info
        /// about the element)</returns>
        private UIElement CreateInfoText(string elementName, string id)
        {
            if(String.IsNullOrWhiteSpace(elementName) || String.IsNullOrWhiteSpace(id))
                return null;

            var textBlock = new TextBlock();

            switch(elementName)
            {
                case "image":
                case "link":
                case "topic":
                    var projectFileSearcher = new ProjectFileSearcher(serviceProvider, null);
                    string title, filename, relativePath;

                    bool found = projectFileSearcher.GetInfoFor(elementName == "image" ?
                        ProjectFileSearcher.IdType.Image : ProjectFileSearcher.IdType.Link, id,
                        out title, out filename, out relativePath);

                    textBlock.Inlines.AddRange(new Inline[] {
                        new Bold(new Run(elementName == "image" ? "Alternate Text: " : "Title: ")),
                        new Run(title),
                        new LineBreak(),
                        new Bold(new Run("Filename: ")),
                        new Run(relativePath)
                    });

                    if(elementName != "topic" && found & ctrlClickEnabled)
                        textBlock.Inlines.AddRange(new Inline[] {
                            new LineBreak(),
                            new Run("Ctrl+Click to open the file")
                        });
                    break;

                case "codeEntityReference":
                    if(!ctrlClickEnabled)
                        return null;

                    textBlock.Inlines.Add(new Run("Ctrl+Click to go to definition (within solution only)"));
                    break;

                default:
                    if(!ctrlClickEnabled)
                        return null;

                    textBlock.Inlines.Add(new Run("Ctrl+Click to open the containing file"));
                    break;
            }

            // Set the styles in order to support other themes
            textBlock.SetResourceReference(TextBlock.BackgroundProperty, EnvironmentColors.ToolTipBrushKey);
            textBlock.SetResourceReference(TextBlock.ForegroundProperty, EnvironmentColors.ToolTipTextBrushKey);

            return textBlock;
        }
        #endregion
    }
}
