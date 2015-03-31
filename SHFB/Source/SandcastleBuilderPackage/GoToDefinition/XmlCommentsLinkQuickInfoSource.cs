//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : XmlCommentsLinkQuickInfoSource.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/09/2015
// Note    : Copyright 2014-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that determines whether or not quick info should be shown for specific XML
// comments elements and what is should contain.
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace SandcastleBuilder.Package.GoToDefinition
{
    /// <summary>
    /// This class is used to determine whether or not quick info should be shown for specific XML comments
    /// link elements and what it should contain.
    /// </summary>
    internal sealed class XmlCommentsLinkQuickInfoSource : IQuickInfoSource
    {
        #region Private data members
        //=====================================================================

        private SVsServiceProvider serviceProvider;
        private ITextBuffer textBuffer;
        private XmlCommentsLinkQuickInfoSourceProvider provider;
        private bool ctrlClickEnabled, enableInCRef;
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
        /// <param name="enableInCRef">True to enable in <c>cref</c> targets, false if not</param>
        public XmlCommentsLinkQuickInfoSource(SVsServiceProvider serviceProvider, ITextBuffer buffer,
          XmlCommentsLinkQuickInfoSourceProvider provider, bool ctrlClickEnabled, bool enableInCRef)
        {
            this.serviceProvider = serviceProvider;
            this.textBuffer = buffer;
            this.provider = provider;
            this.ctrlClickEnabled = ctrlClickEnabled;
            this.enableInCRef = enableInCRef;
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
                string elementName = null, attrName = null, identifier = null, name;
                UIElement content;

                // Get the tags for the line containing the mouse point
                foreach(IMappingTagSpan<IClassificationTag> curTag in tagAggregator.GetTags(
                  new SnapshotSpan(lineSpan.Start, lineSpan.End)))
                {
                    name = curTag.Tag.ClassificationType.Classification.ToLowerInvariant();
                    tagSpan = curTag.Span.GetSpans(textBuffer).First();

                    if(name.IndexOf("identifier", StringComparison.Ordinal) != -1)
                        name = "identifier";

                    switch(name)
                    {
                        case "xml doc tag":
                            // Track the last seen element or attribute.  The classifier in VS2013 and earlier does
                            // not break up the XML comments into elements and attributes so we may get a mix of text
                            // in the "tag".
                            attrName = tagSpan.GetText();

                            // If it contains "cref", tne next XML doc attribute value will be the target
                            if(attrName.IndexOf("cref=", StringComparison.Ordinal) != -1 && enableInCRef)
                                attrName = "cref";

                            // As above, for conceptualLink, the next XML doc attribute will be the target
                            if(attrName.StartsWith("<conceptualLink", StringComparison.Ordinal))
                                attrName = "conceptualLink";

                            // For token, the next XML doc comment will contain the token name
                            if(attrName == "<token>")
                                attrName = "token";
                            break;

                        case "xml doc attribute":
                            if((attrName == "cref" || attrName == "conceptualLink") &&
                              tagSpan.Contains(triggerPoint.Value) && tagSpan.Length > 2)
                            {
                                // Drop the quotes from the span
                                var span = new SnapshotSpan(tagSpan.Snapshot, tagSpan.Start + 1,
                                    tagSpan.Length - 2);

                                content = this.CreateInfoText(attrName, span.GetText());

                                if(content != null)
                                {
                                    applicableToSpan = textBuffer.CurrentSnapshot.CreateTrackingSpan(span,
                                        SpanTrackingMode.EdgeExclusive);

                                    quickInfoContent.Add(content);
                                }

                                return;
                            }
                            break;

                        case "xml doc comment":
                            if(attrName == "token" && tagSpan.Contains(triggerPoint.Value) && tagSpan.Length > 1)
                            {
                                content = this.CreateInfoText(attrName, tagSpan.GetText());

                                if(content != null)
                                {
                                    applicableToSpan = textBuffer.CurrentSnapshot.CreateTrackingSpan(tagSpan,
                                        SpanTrackingMode.EdgeExclusive);

                                    quickInfoContent.Add(content);
                                }

                                return;
                            }
                            break;

                        // VS2015 is more specific in its classifications
                        case "xml doc comment - name":
                            elementName = tagSpan.GetText().Trim();
                            break;

                        case "xml doc comment - attribute name":
                            attrName = tagSpan.GetText().Trim();
                            identifier = null;

                            if(attrName == "cref" && !enableInCRef)
                                attrName = null;
                            break;

                        case "xml doc comment - attribute value":
                            if((attrName == "cref" || (elementName == "conceptualLink" && attrName == "target")) &&
                              tagSpan.Contains(triggerPoint.Value) && tagSpan.Length > 1)
                            {
                                content = this.CreateInfoText((attrName == "cref") ? attrName : elementName,
                                    tagSpan.GetText());

                                if(content != null)
                                {
                                    applicableToSpan = textBuffer.CurrentSnapshot.CreateTrackingSpan(tagSpan,
                                        SpanTrackingMode.EdgeExclusive);

                                    quickInfoContent.Add(content);
                                }

                                return;
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

                                        quickInfoContent.Add(content);
                                    }
                                }

                                return;
                            }
                            break;

                        case "xml doc comment - text":
                            if(elementName == "token" && tagSpan.Contains(triggerPoint.Value) &&
                              tagSpan.Length > 1)
                            {
                                content = this.CreateInfoText(elementName, tagSpan.GetText());

                                if(content != null)
                                {
                                    applicableToSpan = textBuffer.CurrentSnapshot.CreateTrackingSpan(tagSpan,
                                        SpanTrackingMode.EdgeExclusive);

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
                case "conceptualLink":
                    var projectFileSearcher = new ProjectFileSearcher(serviceProvider, null);
                    string title, filename, relativePath;

                    bool found = projectFileSearcher.GetInfoFor(ProjectFileSearcher.IdType.Link, id,
                        out title, out filename, out relativePath);

                    textBlock.Inlines.AddRange(new Inline[] {
                        new Bold(new Run("Title: ")),
                        new Run(title),
                        new LineBreak(),
                        new Bold(new Run("Filename: ")),
                        new Run(relativePath)
                    });

                    if(found && ctrlClickEnabled)
                        textBlock.Inlines.AddRange(new Inline[] {
                            new LineBreak(),
                            new Run("Ctrl+Click to open the file")
                        });
                    break;

                case "cref":
                    if(!ctrlClickEnabled)
                        return null;

                    if(!IntelliSense.RoslynHacks.RoslynUtilities.IsFinalRoslyn)
                        textBlock.Inlines.Add(new Run("Ctrl+Click to go to definition (within solution only)"));
                    else
                        textBlock.Inlines.Add(new Run("Ctrl+Click to go to definition"));
                    break;

                default:
                    if(!ctrlClickEnabled)
                        return null;

                    textBlock.Inlines.Add(new Run("Ctrl+Click to open the containing file"));
                    break;
            }

            // Set the styles in order to support other themes in VS 2012 and later
            object themeKey = Utility.GetThemeKey("ToolTipBrushKey", SystemColors.ControlLightBrushKey);

            if(themeKey != null)
            {
                textBlock.SetResourceReference(TextBlock.BackgroundProperty, themeKey);
                textBlock.SetResourceReference(TextBlock.ForegroundProperty,
                    Utility.GetThemeKey("ToolTipTextBrushKey", SystemColors.ControlTextBrushKey));
            }

            return textBlock;
        }
        #endregion
    }
}
