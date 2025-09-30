//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : MamlLinkQuickInfoSource.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/29/2025
// Note    : Copyright 2014-2025, Eric Woodruff, All rights reserved
//
// This file contains the class that determines whether or not quick info should be shown for specific MAML
// elements and what is should contain.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
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
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Classification;

namespace SandcastleBuilder.Package.GoToDefinition;

/// <summary>
/// This class is used to determine whether or not quick info should be shown for specific MAML elements and
/// what it should contain.
/// </summary>
internal sealed class MamlLinkQuickInfoSource : IAsyncQuickInfoSource
{
    #region Private data members
    //=====================================================================

    private readonly ITextBuffer textBuffer;
    private readonly IViewClassifierAggregatorService classifierAggregatorService;
    private readonly bool ctrlClickEnabled;

    #endregion

    #region Constructor
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="buffer">The buffer to use</param>
    /// <param name="classifierAggregatorService">The classifier aggregator service to use</param>
    /// <param name="ctrlClickEnabled">True if Ctrl+Click on definition is enabled, false if not</param>
    public MamlLinkQuickInfoSource(ITextBuffer buffer, IViewClassifierAggregatorService classifierAggregatorService,
      bool ctrlClickEnabled)
    {
        this.textBuffer = buffer;
        this.classifierAggregatorService = classifierAggregatorService;
        this.ctrlClickEnabled = ctrlClickEnabled;
    }
    #endregion

    #region IAsyncQuickInfoSource implementation
    //=====================================================================

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose of for this one
    }

    /// <inheritdoc />
    public async Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session,
      CancellationToken cancellationToken)
    {
        var triggerPoint = session.GetTriggerPoint(textBuffer.CurrentSnapshot);

        if(triggerPoint == null)
            return null;

        var lineSpan = triggerPoint.Value.GetContainingLine();
        string elementName = null, attrName = null, name, spanText;
        ContainerElement content = null;
        ITrackingSpan applicableToSpan;

        // Getting the classifier needs to run on the UI thread
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        var classifier = classifierAggregatorService.GetClassifier(session.TextView);
        var classificationSpans = classifier.GetClassificationSpans(new SnapshotSpan(lineSpan.Start, lineSpan.End));

        foreach(var curSpan in classificationSpans)
        {
            name = curSpan.ClassificationType.Classification.ToLowerInvariant();
            var tagSpan = curSpan.Span;

            switch(name)
            {
                case "xml name":
                    elementName = tagSpan.GetText();
                    break;

                case "xml attribute":
                    attrName = tagSpan.GetText();
                    break;

                case "xml attribute value":
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

                                return new QuickInfoItem(applicableToSpan, content);
                            }
                        }

                        return null;
                    }
                    break;

                case "xml text":
                    if(tagSpan.Contains(triggerPoint.Value))
                    {
                        spanText = tagSpan.GetText().Trim();

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
                                // We only get info about the current line so we may just get some XML text if
                                // the starting tag is on a prior line.  In such cases, see if the text looks
                                // like an entity reference or a code reference ID.  If so, offer it as a
                                // clickable link.  If not, ignore it.
                                if(String.IsNullOrWhiteSpace(elementName))
                                {
                                    if(spanText.IsCodeEntityReference())
                                        content = this.CreateInfoText("codeEntityReference", tagSpan.GetText());
                                    else
                                    {
                                        if(spanText.IsCodeReferenceId())
                                            content = this.CreateInfoText("codeReference", tagSpan.GetText());
                                    }
                                }
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

                            return new QuickInfoItem(applicableToSpan, content);
                        }

                        return null;
                    }
                    break;

                default:
                    break;
            }
        }

        return null;
    }

    /// <summary>
    /// This is used to get the content to add to the quick info by looking up the given topic ID to get its
    /// title and filename if possible.
    /// </summary>
    /// <param name="elementName">The element name for which to create content</param>
    /// <param name="id">The ID to look up if necessary</param>
    /// <returns>The content to add to the quick info (a text block element containing the additional info
    /// about the element)</returns>
    private ContainerElement CreateInfoText(string elementName, string id)
    {
        if(String.IsNullOrWhiteSpace(elementName) || String.IsNullOrWhiteSpace(id))
            return null;

        var elements = new List<ClassifiedTextElement>();

        switch(elementName)
        {
            case "image":
            case "link":
            case "topic":
                var projectFileSearcher = new ProjectFileSearcher(null);

                var (found, title, _, relativePath) = projectFileSearcher.GetInfoForId(elementName == "image" ?
                    ProjectFileSearcher.IdType.Image : ProjectFileSearcher.IdType.Link, id);

                elements.Add(new ClassifiedTextElement(new ClassifiedTextRun(PredefinedClassificationTypeNames.Other,
                    elementName == "image" ? "Alternate Text: " : "Title: ", ClassifiedTextRunStyle.Bold),
                    new ClassifiedTextRun(PredefinedClassificationTypeNames.Other, title)));
                elements.Add(new ClassifiedTextElement(new ClassifiedTextRun(PredefinedClassificationTypeNames.Other,
                    "Filename: ",ClassifiedTextRunStyle.Bold),
                    new ClassifiedTextRun(PredefinedClassificationTypeNames.Other, relativePath)));

                if(elementName != "topic" && found & ctrlClickEnabled)
                {
                    elements.Add(new ClassifiedTextElement(new ClassifiedTextRun(
                        PredefinedClassificationTypeNames.Other, "Ctrl+Click to open the file")));
                }
                break;

            case "codeEntityReference":
                if(!ctrlClickEnabled)
                    return null;

                elements.Add(new ClassifiedTextElement(new ClassifiedTextRun(
                    PredefinedClassificationTypeNames.Other, "Ctrl+Click to go to definition (within solution only)")));
                break;

            default:
                if(!ctrlClickEnabled)
                    return null;

                elements.Add(new ClassifiedTextElement(new ClassifiedTextRun(
                    PredefinedClassificationTypeNames.Other, "Ctrl+Click to open the containing file")));
                break;
        }

        return new ContainerElement(ContainerElementStyle.Stacked, elements);
    }
    #endregion
}
