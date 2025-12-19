//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : MarkdownLinkQuickInfoSource.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/15/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains the class that determines whether or not quick info should be shown for specific Markdown
// elements and what is should contain.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
//===============================================================================================================
// 12/15/2025  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Classification;

namespace SandcastleBuilder.Package.GoToDefinition;

/// <summary>
/// This class is used to determine whether or not quick info should be shown for specific Markdown elements and
/// what it should contain.
/// </summary>
internal sealed class MarkdownLinkQuickInfoSource : IAsyncQuickInfoSource
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
    public MarkdownLinkQuickInfoSource(ITextBuffer buffer, IViewClassifierAggregatorService classifierAggregatorService,
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
    public Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session,
      CancellationToken cancellationToken)
    {
        var triggerPoint = session.GetTriggerPoint(textBuffer.CurrentSnapshot);

        if(triggerPoint == null)
            return Task.FromResult<QuickInfoItem>(null);

        // Markdown doesn't classify the inner text of elements nor our link targets so we have to parse the
        // text to see if it's something we can use.
        var lineSpan = triggerPoint.Value.GetContainingLine();
        string lineText = lineSpan.GetText(), elementName = null, id;
        int tagIndex, endIndex, startIndex = triggerPoint.Value.Position - lineSpan.Start.Position,
            nestedParen = 0;

        if(startIndex >= lineText.Length)
            startIndex = lineText.Length - 1;

        while(startIndex >= 0 && lineText[startIndex] != '@' && lineText[startIndex] != '>')
        {
            if(lineText[startIndex] == '<' || Char.IsWhiteSpace(lineText[startIndex]))
            {
                return Task.FromResult<QuickInfoItem>(null);
            }

            startIndex--;
        }

        endIndex = startIndex + 1;

        while(endIndex < lineText.Length && (nestedParen != 0 || lineText[endIndex] != ')') &&
          lineText[endIndex] != '<')
        {
            if(Char.IsWhiteSpace(lineText[endIndex]))
                return Task.FromResult<QuickInfoItem>(null);

            endIndex++;

            if(endIndex < lineText.Length)
            {
                if(lineText[endIndex] == '(')
                    nestedParen++;
                else
                {
                    if(nestedParen > 0 && lineText[endIndex] == ')')
                    {
                        nestedParen--;
                        endIndex++;
                    }
                }
            }
        }

        if(startIndex < 0)
            startIndex = 0;

        if(lineText[startIndex] == '@')
        {
            if(startIndex < 3 || lineText[startIndex - 1] != '(' || lineText[startIndex - 2] != ']')
                return Task.FromResult<QuickInfoItem>(null);

            startIndex++;

            if(endIndex - startIndex > 2 && lineText[startIndex + 1] == ':')
                elementName = "codeEntityReference";
            else
            {
                tagIndex = startIndex;

                while(tagIndex > 0 && lineText[tagIndex] != '[')
                    tagIndex--;

                if(tagIndex > 0 && lineText[tagIndex - 1] == '!')
                    elementName = "image";
                else
                    elementName = "link";
            }
        }
        else
        {
            if(lineText[startIndex] == '>')
            {
                tagIndex = startIndex;
                startIndex++;

                while(tagIndex > 0 && lineText[tagIndex] != '<')
                    tagIndex--;

                if(tagIndex < 0)
                    return Task.FromResult<QuickInfoItem>(null);

                elementName = lineText.Substring(tagIndex + 1, startIndex - tagIndex - 2);
                tagIndex = elementName.IndexOf(' ');

                if(tagIndex != -1)
                    elementName = elementName.Substring(0, tagIndex);
            }
            else
            {
                if(endIndex < lineText.Length - 2 && lineText[endIndex] == '<' && lineText[endIndex + 1] == '/')
                {
                    tagIndex = endIndex;

                    while(tagIndex < lineText.Length && lineText[tagIndex] != '>')
                        tagIndex++;

                    if(tagIndex >= lineText.Length)
                        return Task.FromResult<QuickInfoItem>(null);

                    elementName = lineText.Substring(endIndex + 2, tagIndex - endIndex - 2);
                }
            }
        }

        id = lineText.Substring(startIndex, endIndex - startIndex);

        if(elementName == null && id.Length > 3 && id[1] == ':')
            elementName = "codeEntityReference";

        if(elementName == null)
            return Task.FromResult<QuickInfoItem>(null);

        var content = this.CreateInfoText(elementName, id);

        if(content != null)
        {
            var applicableToSpan = textBuffer.CurrentSnapshot.CreateTrackingSpan(
                lineSpan.Start + startIndex, endIndex - startIndex, SpanTrackingMode.EdgeExclusive);

            return Task.FromResult(new QuickInfoItem(applicableToSpan, content));
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
            case "image":
            case "link":
                var projectFileSearcher = new ProjectFileSearcher(null);

                var (found, title, _, relativePath) = projectFileSearcher.GetInfoForId(elementName == "image" ?
                    ProjectFileSearcher.IdType.Image : ProjectFileSearcher.IdType.Link, id);

                elements.Add(new ClassifiedTextElement(new ClassifiedTextRun(PredefinedClassificationTypeNames.Other,
                    elementName == "image" ? "Alternate Text: " : "Title: ", ClassifiedTextRunStyle.Bold),
                    new ClassifiedTextRun(PredefinedClassificationTypeNames.Other, title)));
                elements.Add(new ClassifiedTextElement(new ClassifiedTextRun(PredefinedClassificationTypeNames.Other,
                    "Filename: ", ClassifiedTextRunStyle.Bold),
                    new ClassifiedTextRun(PredefinedClassificationTypeNames.Other, relativePath)));

                if(found & ctrlClickEnabled)
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
