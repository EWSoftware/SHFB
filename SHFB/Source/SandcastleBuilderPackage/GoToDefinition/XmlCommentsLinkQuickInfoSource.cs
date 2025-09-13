//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : XmlCommentsLinkQuickInfoSource.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/12/2025
// Note    : Copyright 2014-2025, Eric Woodruff, All rights reserved
//
// This file contains the class that determines whether or not quick info should be shown for specific XML
// comments elements and what is should contain.
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;

namespace SandcastleBuilder.Package.GoToDefinition;

/// <summary>
/// This class is used to determine whether or not quick info should be shown for specific XML comments
/// link elements and what it should contain.
/// </summary>
internal sealed class XmlCommentsLinkQuickInfoSource : IAsyncQuickInfoSource
{
    #region Private data members
    //=====================================================================

    private readonly ITextBuffer textBuffer;
    private readonly IViewTagAggregatorFactoryService aggregatorFactory;
    private readonly bool ctrlClickEnabled;

    #endregion

    #region Constructor
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="buffer">The buffer to use</param>
    /// <param name="aggregatorFactory">The aggregator factory to use</param>
    /// <param name="ctrlClickEnabled">True if Ctrl+Click on definition is enabled, false if not</param>
    public XmlCommentsLinkQuickInfoSource(ITextBuffer buffer, IViewTagAggregatorFactoryService aggregatorFactory,
      bool ctrlClickEnabled)
    {
        this.textBuffer = buffer;
        this.aggregatorFactory = aggregatorFactory;
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
    public Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session,
      CancellationToken cancellationToken)
    {
        var triggerPoint = session.GetTriggerPoint(textBuffer.CurrentSnapshot);

        if(triggerPoint == null)
            return Task.FromResult<QuickInfoItem>(null);

        using var tagAggregator = aggregatorFactory.CreateTagAggregator<IClassificationTag>(session.TextView);
        var lineSpan = triggerPoint.Value.GetContainingLine();
        string elementName = null, attrName = null, identifier = null, name;
        ContainerElement content;
        ITrackingSpan applicableToSpan;

        foreach(IMappingTagSpan<IClassificationTag> curTag in tagAggregator.GetTags(
          new SnapshotSpan(lineSpan.Start, lineSpan.End)))
        {
            name = curTag.Tag.ClassificationType.Classification.ToLowerInvariant();
            var tagSpan = curTag.Span.GetSpans(textBuffer).First();

            if(name.IndexOf("identifier", StringComparison.Ordinal) != -1)
                name = "identifier";

            switch(name)
            {
                case "xml doc comment - name":
                    elementName = tagSpan.GetText().Trim();
                    break;

                case "xml doc comment - attribute name":
                    attrName = tagSpan.GetText().Trim();
                    identifier = null;

                    if(attrName == "cref" || attrName == "name")
                        attrName = null;
                    break;

                case "xml doc comment - attribute value":
                    if(elementName == "conceptualLink" && attrName == "target" &&
                        tagSpan.Contains(triggerPoint.Value) && tagSpan.Length > 1)
                    {
                        content = this.CreateInfoText(elementName, tagSpan.GetText());

                        if(content != null)
                        {
                            applicableToSpan = textBuffer.CurrentSnapshot.CreateTrackingSpan(tagSpan,
                                SpanTrackingMode.EdgeExclusive);

                            return Task.FromResult(new QuickInfoItem(applicableToSpan, content));
                        }

                        return Task.FromResult<QuickInfoItem>(null);
                    }
                    break;

                case "identifier":
                case "keyword":
                case "operator":
                    if(attrName != null)
                    {
                        identifier += tagSpan.GetText();

                        if(name == "keyword")
                            identifier += " ";
                    }
                    break;

                case "punctuation":
                    if(identifier != null)
                        identifier += tagSpan.GetText();
                    break;

                case "xml doc comment - attribute quotes":
                    if(identifier != null)
                    {
                        // Set the span to that of the identifier
                        var span = new SnapshotSpan(tagSpan.Snapshot, tagSpan.Start - identifier.Length,
                            identifier.Length);

                        if(span.Contains(triggerPoint.Value) && span.Length > 1)
                        {
                            content = this.CreateInfoText("cref", span.GetText());

                            if(content != null)
                            {
                                applicableToSpan = textBuffer.CurrentSnapshot.CreateTrackingSpan(span,
                                    SpanTrackingMode.EdgeExclusive);

                                return Task.FromResult(new QuickInfoItem(applicableToSpan, content));
                            }
                        }

                        return Task.FromResult<QuickInfoItem>(null);
                    }
                    break;

                case "xml doc comment - text":
                    if(elementName == "token" && tagSpan.Contains(triggerPoint.Value) && tagSpan.Length > 1)
                    {
                        content = this.CreateInfoText(elementName, tagSpan.GetText());

                        if(content != null)
                        {
                            applicableToSpan = textBuffer.CurrentSnapshot.CreateTrackingSpan(tagSpan,
                                SpanTrackingMode.EdgeExclusive);

                            return Task.FromResult(new QuickInfoItem(applicableToSpan, content));
                        }

                        return Task.FromResult<QuickInfoItem>(null);
                    }
                    break;

                default:
                    break;
            }
        }

        return Task.FromResult<QuickInfoItem>(null);
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
            case "conceptualLink":
                var projectFileSearcher = new ProjectFileSearcher(null);

                var (found, title, _, relativePath) = projectFileSearcher.GetInfoForId(
                    ProjectFileSearcher.IdType.Link, id);

                elements.Add(new ClassifiedTextElement(new ClassifiedTextRun(PredefinedClassificationTypeNames.Other,
                    "Title: ", ClassifiedTextRunStyle.Bold),
                    new ClassifiedTextRun(PredefinedClassificationTypeNames.Other, title)));
                elements.Add(new ClassifiedTextElement(new ClassifiedTextRun(PredefinedClassificationTypeNames.Other,
                    "Filename: ", ClassifiedTextRunStyle.Bold),
                    new ClassifiedTextRun(PredefinedClassificationTypeNames.Other, relativePath)));

                if(found && ctrlClickEnabled)
                {
                    elements.Add(new ClassifiedTextElement(new ClassifiedTextRun(
                        PredefinedClassificationTypeNames.Other, "Ctrl+Click to open the file")));
                }
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
