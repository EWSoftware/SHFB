//=============================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : MamlToFlowDocumentConverter.Handlers.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/28/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the element handler methods for the MAML to flow document
// converter class.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.4  01/02/2012  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace SandcastleBuilder.WPF.Maml
{
    // This contains the element handlers for the MAML to flow document converter
    partial class MamlToFlowDocumentConverter
	{
        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to strip a common amount of leading whitespace on all  lines of a text block and to
        /// convert tabs to a consistent number of spaces.
        /// </summary>
        /// <param name="text">The text containing the lines to clean up</param>
        /// <param name="tabSize">The number of spaces to which tab characters are converted</param>
        private static string StripLeadingWhitespace(string text, int tabSize)
        {
            string[] lines;
            string currentLine, tabsToSpaces = new String(' ', tabSize);
            int minSpaces, spaceCount, line;

            if(String.IsNullOrEmpty(text))
                return text;

            // Replace "\r\n" with "\n" first for consistency
            lines = text.Replace("\r\n", "\n").Split(new char[] { '\n' });

            // If only one line, just trim it and convert tabs to spaces
            if(lines.Length == 1)
                return lines[0].Trim().Replace("\t", tabsToSpaces);

            minSpaces = Int32.MaxValue;

            // The first pass is used to determine the minimum number of spaces and to truncate blank lines
            for(line = 0; line < lines.Length; line++)
            {
                currentLine = lines[line].Replace("\t", tabsToSpaces);

                if(currentLine.Length != 0)
                {
                    for(spaceCount = 0; spaceCount < currentLine.Length; spaceCount++)
                        if(currentLine[spaceCount] != ' ')
                            break;

                    if(spaceCount == currentLine.Length)
                        currentLine = String.Empty;
                    else
                        if(spaceCount < minSpaces)
                            minSpaces = spaceCount;

                    lines[line] = currentLine;
                }
            }

            // Unlikely, but it could happen...
            if(minSpaces == Int32.MaxValue)
                minSpaces = 0;

            // The second pass joins them back together less the determined amount of leading whitespace
            StringBuilder sb = new StringBuilder(text.Length);

            for(line = 0; line < lines.Length; line++)
            {
                currentLine = lines[line];

                if(currentLine.Length != 0)
                    sb.AppendLine(currentLine.Substring(minSpaces));
                else
                    if(sb.Length > 0)   // Skip leading blank lines
                        sb.AppendLine();
            }

            // Trim off trailing blank lines too
            return sb.ToString().TrimEnd(new char[] { ' ', '\r', '\n' });
        }
        #endregion

        #region General section and formatted block element handlers
        //=====================================================================

        /// <summary>
        /// Handle alert elements
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void AlertElement(ElementProperties props)
        {
            XAttribute attribute;
            string title = null, icon = null;

            // Map the class name to a title
            attribute = props.Element.Attribute("class");

            if(attribute == null || !AlertTitles.TryGetValue(attribute.Value, out title))
                title = "Note";

            if(attribute == null || !AlertIcons.TryGetValue(attribute.Value, out icon))
                icon = "AlertNote";

            Section alert = new Section();
            props.Converter.AddToBlockContainer(alert);

            Paragraph p = new Paragraph();
            alert.Blocks.Add(p);

            // Set the icon based on the alert type
            var iconImage = (ImageSource)props.Converter.Document.Resources[icon];

            p.Inlines.Add(new InlineUIContainer(new Image
            {
                Source = iconImage,
                Height = iconImage.Height,
                Width = iconImage.Width,
                ToolTip = title,
                Margin = new Thickness(0, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center
            }));

            p.Inlines.Add(new Run(title));
            p.SetResourceReference(Paragraph.StyleProperty, NamedStyle.AlertTitle);

            Section alertBody = new Section();
            alert.Blocks.Add(alertBody);

            // We want the body section to be the current parent here rather than the containing section
            props.Converter.CurrentBlockElement = alertBody;

            alertBody.SetResourceReference(Section.StyleProperty, NamedStyle.AlertBody);
        }

        /// <summary>
        /// Handle code elements
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void CodeElement(ElementProperties props)
        {
            XAttribute attribute;
            Section code = new Section();
            props.Converter.AddToBlockContainer(code);
            string language = "none", title;

            if(props.Element.Name.LocalName != "codeReference")
            {
                attribute = props.Element.Attribute("lang");

                if(attribute == null)
                    attribute = props.Element.Attribute("language");

                if(attribute != null)
                    language = attribute.Value;

                // If there is a title attribute, use that for the title.  If not, map the language ID to
                // a display title.
                attribute = props.Element.Attribute("title");

                // For a blank title, a single space is required.  If empty, use the language title.
                if(attribute != null && attribute.Value.Length != 0)
                    title = attribute.Value;
                else
                    if(!LanguageTitles.TryGetValue(language, out title))
                        title = language;
            }
            else
                title = "Code Reference";

            Paragraph p = new Paragraph();
            code.Blocks.Add(p);
            p.Inlines.Add(new Run(title));
            p.SetResourceReference(Paragraph.StyleProperty, NamedStyle.CodeTitle);

            Section codeBlock = new Section();
            code.Blocks.Add(codeBlock);
            codeBlock.SetResourceReference(Section.StyleProperty, NamedStyle.CodeBlock);

            p = new Paragraph();
            codeBlock.Blocks.Add(p);

            // If importing from an external file, include that info in the content
            attribute = props.Element.Attribute("source");

            if(attribute != null)
            {
                p.Inlines.Add(new Run(String.Format("Import code from {0}", attribute.Value)));

                attribute = props.Element.Attribute("region");

                if(attribute != null)
                    p.Inlines.Add(new Run(String.Format(" limited to the region named '{0}'", attribute.Value)));

                p.Inlines.Add(new LineBreak());
            }

            p.Inlines.Add(new Run(StripLeadingWhitespace(props.Element.Value, 4)));

            props.ParseChildren = false;
        }

        /// <summary>
        /// Handle general paragraph type elements
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void ParagraphElement(ElementProperties props)
        {
            // If empty, ignore it to match the Sandcastle's behavior
            if(props.ParseChildren && props.Element.FirstNode != null && !String.IsNullOrEmpty(props.Element.Value))
                props.Converter.AddToBlockContainer(new Paragraph());
            else
                props.ReturnToParent = false;
        }

        /// <summary>
        /// Handle quote elements
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void QuoteElement(ElementProperties props)
        {
            Paragraph p = new Paragraph();
            props.Converter.AddToBlockContainer(p);
            p.SetResourceReference(Paragraph.StyleProperty, NamedStyle.Quote);
        }

        /// <summary>
        /// Handle general and named section elements
        /// </summary>
        /// <param name="props">The element properties</param>
        /// <remarks>If the section is empty, it will be omitted</remarks>
        private static void SectionElement(ElementProperties props)
        {
            if(props.ParseChildren && props.Element.HasElements)
            {
                Section s = new Section();
                string title;

                // If this is a named section, add the standard title
                if(NamedSectionTitles.TryGetValue(props.Element.Name.LocalName, out title))
                {
                    Paragraph p = new Paragraph(new Run(title));
                    s.Blocks.Add(p);
                    p.SetResourceReference(Paragraph.StyleProperty, NamedStyle.Title);
                }

                props.Converter.AddToBlockContainer(s);
            }
            else
                props.ParseChildren = props.ReturnToParent = false;
        }

        /// <summary>
        /// Handle summary elements
        /// </summary>
        /// <param name="props">The element properties</param>
        /// <remarks>If the abstract attribute is set to true, this element is skipped</remarks>
        private static void SummaryElement(ElementProperties props)
        {
            XAttribute abstractAttr = props.Element.Attribute("abstract");
            bool excluded;

            if(abstractAttr == null || !Boolean.TryParse(abstractAttr.Value, out excluded))
                excluded = false;

            if(!excluded)
                props.Converter.AddToBlockContainer(new Section());
            else
                props.ParseChildren = props.ReturnToParent = false;
        }

        /// <summary>
        /// Handle title elements
        /// </summary>
        /// <param name="props">The element properties</param>
        /// <remarks>If the title element is inside a table, it is skipped as the table will already have
        /// added it outside of itself.</remarks>
        private static void TitleElement(ElementProperties props)
        {
            if(props.Converter.CurrentBlockElement is Table)
                props.ReturnToParent = false;
            else
            {
                Paragraph p = new Paragraph(new Run(reCondenseWhitespace.Replace(
                    props.Element.Value.Trim(), " ")));
                props.Converter.AddToBlockContainer(p);
                p.SetResourceReference(Paragraph.StyleProperty, NamedStyle.Title);
            }

            props.ParseChildren = false;
        }
        #endregion

        #region List element handlers
        //=====================================================================

        /// <summary>
        /// Handle defined term elements
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void DefinedTermElement(ElementProperties props)
        {
            Paragraph p = new Paragraph();
            props.Converter.AddToBlockContainer(p);
            p.SetResourceReference(Paragraph.StyleProperty, NamedStyle.DefinedTerm);
        }

        /// <summary>
        /// Handle definition elements
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void DefinitionElement(ElementProperties props)
        {
            Section s = new Section();
            props.Converter.AddToBlockContainer(s);
            s.SetResourceReference(Section.StyleProperty, NamedStyle.Definition);
        }

        /// <summary>
        /// Handle list elements
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void ListElement(ElementProperties props)
        {
            XAttribute listClass = props.Element.Attribute("class");
            TextMarkerStyle markerStyle = TextMarkerStyle.Disc;

            if(listClass != null)
                switch(listClass.Value)
                {
                    case "ordered":
                        markerStyle = TextMarkerStyle.Decimal;
                        break;

                    case "nobullet":
                        markerStyle = TextMarkerStyle.None;
                        break;

                    default:
                        break;
                }

            props.Converter.AddToBlockContainer(new List { MarkerStyle = markerStyle });
        }

        /// <summary>
        /// Handle list item elements
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void ListItemElement(ElementProperties props)
        {
            props.Converter.AddToBlockContainer(new ListItem());
        }

        /// <summary>
        /// Handle related topics elements
        /// </summary>
        /// <param name="props">The element properties</param>
        /// <remarks>If empty, the related topics section is omitted</remarks>
        private static void RelatedTopicsElement(ElementProperties props)
        {
            List<XElement> tasks = new List<XElement>(), reference = new List<XElement>(),
                concepts = new List<XElement>(), otherResources = new List<XElement>(),
                tokenContent = new List<XElement>();
            XElement token;
            XAttribute attribute;
            Guid topicId, href;
            string linkType;

            if(!props.ParseChildren || !props.Element.HasElements)
            {
                props.ParseChildren = props.ReturnToParent = false;
                return;
            }

            // All links are handled here
            props.ParseChildren = false;

            // A name is added here for use by autoOutline elements
            Section s = new Section { Name = ToElementName("seeAlsoSection") };
            props.Converter.AddToBlockContainer(s);

            // Add the section title
            Paragraph p = new Paragraph(new Run(NamedSectionTitles[props.Element.Name.LocalName]));
            s.Blocks.Add(p);
            p.SetResourceReference(Paragraph.StyleProperty, NamedStyle.Title);

            // Expand tokens first
            foreach(var link in props.Element.Nodes().OfType<XElement>().Where(n => n.Name.LocalName == "token"))
                if(props.Converter.Tokens.TryGetValue(props.Element.Value.Trim(), out token))
                    tokenContent.AddRange(token.Nodes().OfType<XElement>());

            // Group the elements by type or topic ID
            foreach(var link in props.Element.Nodes().OfType<XElement>().Concat(tokenContent))
            {
                linkType = link.Name.LocalName;
                attribute = link.Attribute("topicType_id");

                if(attribute == null || !Guid.TryParse(attribute.Value, out topicId))
                    topicId = Guid.Empty;

                attribute = link.Attribute(xlink + "href");

                if(attribute == null || !Guid.TryParse(attribute.Value, out href))
                    href = Guid.Empty;

                if(href != Guid.Empty && (linkType == "link" || linkType == "legacyLink") && (
                  topicId == HowToId || topicId == WalkthroughId || topicId == SampleId ||
                  topicId == TroubleshootingId))
                {
                    tasks.Add(link);
                }
                else
                    if(linkType == "codeEntityReference" || ((linkType == "link" || linkType == "legacyLink") && (
                      href == Guid.Empty || topicId == ReferenceWithoutSyntaxId ||
                      topicId == ReferenceWithSyntaxId || topicId == XmlReferenceId ||
                      topicId == ErrorMessageId || topicId == UIReferenceId)))
                    {
                        reference.Add(link);
                    }
                    else
                        if(href != Guid.Empty && (linkType == "link" || linkType == "legacyLink") && (
                          topicId == ConceptualId || topicId == SdkTechnologyOverviewArchitectureId ||
                          topicId == SdkTechnologyOverviewCodeDirectoryId ||
                          topicId == SdkTechnologyOverviewScenariosId ||
                          topicId == SdkTechnologyOverviewTechnologySummaryId))
                            concepts.Add(link);
                        else
                            if(linkType == "externalLink" || ((linkType == "link" || linkType == "legacyLink") &&
                              href != Guid.Empty && (topicId == Guid.Empty || topicId == OrientationId ||
                              topicId == WhitePaperId || topicId == CodeEntityId || topicId == GlossaryId ||
                              topicId == SDKTechnologyOverviewOrientationId)))
                                otherResources.Add(link);
            }

            // Add each set
            AddRelatedTopicLinks(props.Converter, s, tasks, "Tasks");
            AddRelatedTopicLinks(props.Converter, s, reference, "Reference");
            AddRelatedTopicLinks(props.Converter, s, concepts, "Concepts");
            AddRelatedTopicLinks(props.Converter, s, otherResources, "Other Resources");
        }

        /// <summary>
        /// This is used to add the related topic links
        /// </summary>
        /// <param name="converter">The converter used to add the elements</param>
        /// <param name="s">The section used to contain the links</param>
        /// <param name="links">The list of links to add</param>
        /// <param name="sectionTitle">The section title</param>
        private static void AddRelatedTopicLinks(MamlToFlowDocumentConverter converter, Section s,
          List<XElement> links, string sectionTitle)
        {
            Paragraph p;
            bool isFirst = true;

            if(links.Count != 0)
            {
                p = new Paragraph(new Run(sectionTitle));
                s.Blocks.Add(p);
                p.SetResourceReference(Paragraph.StyleProperty, NamedStyle.RelatedTopicTitle);

                p = new Paragraph();
                s.Blocks.Add(p);
                p.SetResourceReference(Paragraph.StyleProperty, NamedStyle.NoTopMargin);

                foreach(var link in links)
                {
                    if(isFirst)
                        isFirst = false;
                    else
                        p.Inlines.Add(new LineBreak());

                    converter.ParseChildren(p, new[] { link });
                }
            }
        }
        #endregion

        #region Media element handlers
        //=====================================================================

        /// <summary>
        /// Handle media link elements
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void MediaLinkElement(ElementProperties props)
        {
            BlockUIContainer imageBlock = new BlockUIContainer();
            XElement imageElement = props.Element.Descendants(ddue + "image").FirstOrDefault(),
                captionElement = props.Element.Descendants(ddue + "caption").FirstOrDefault();
            XAttribute attribute;
            HorizontalAlignment alignment = HorizontalAlignment.Left;
            KeyValuePair<string, string> imageInfo = new KeyValuePair<string, string>(null, null);
            Image image = new Image();
            string id = "???", caption = null, leadIn = null;
            bool captionAfter = false;

            if(imageElement != null)
            {
                attribute = imageElement.Attribute(xlink + "href");

                if(attribute != null)
                {
                    id = attribute.Value;

                    if(!props.Converter.MediaFiles.TryGetValue(id, out imageInfo))
                        imageInfo = new KeyValuePair<string, string>(null, null);
                }

                attribute = imageElement.Attribute("placement");

                if(attribute != null)
                    switch(attribute.Value)
                    {
                        case "center":
                            alignment = HorizontalAlignment.Center;
                            break;

                        case "far":
                            alignment = HorizontalAlignment.Right;
                            break;
                    }
            }

            if(captionElement != null)
            {
                caption = reCondenseWhitespace.Replace(captionElement.Value.Trim(), " ");

                attribute = captionElement.Attribute("placement");

                if(attribute != null && attribute.Value == "after")
                    captionAfter = true;

                attribute = captionElement.Attribute("lead");

                if(attribute != null)
                    leadIn = attribute.Value;
            }

            if(!String.IsNullOrEmpty(imageInfo.Key) && File.Exists(imageInfo.Key))
            {
                var bm = new BitmapImage(new Uri(imageInfo.Key));

                // Use the actual image size to mimic the HTML layout
                image.Height = bm.Height;
                image.Width = bm.Width;
                image.ToolTip = !String.IsNullOrEmpty(imageInfo.Value) ? imageInfo.Value :
                    "ID: " + id + "\nFilename: " + imageInfo.Key;
                image.Margin = new Thickness(10);
                image.HorizontalAlignment = alignment;

                image.Source = bm;

                if(String.IsNullOrEmpty(caption))
                    imageBlock.Child = image;
                else
                {
                    StackPanel sp = new StackPanel();
                    sp.HorizontalAlignment = alignment;

                    if(captionAfter)
                        sp.Children.Add(image);

                    StackPanel spChild = new StackPanel { Orientation = Orientation.Horizontal };
                    spChild.HorizontalAlignment = alignment;

                    if(!String.IsNullOrEmpty(leadIn))
                        spChild.Children.Add(new TextBlock
                        {
                            Text = leadIn + ": ",
                            FontWeight = FontWeights.Bold,
                            Foreground = new SolidColorBrush(Color.FromRgb(0, 0x33, 0x99))
                        });

                    spChild.Children.Add(new TextBlock
                    {
                        Text = caption,
                        Foreground = new SolidColorBrush(Color.FromRgb(0, 0x33, 0x99))
                    });

                    sp.Children.Add(spChild);

                    if(!captionAfter)
                        sp.Children.Add(image);

                    imageBlock.Child = sp;
                }
            }
            else
                imageBlock.Child = new TextBlock
                {
                    Text = String.Format("[INVALID IMAGE ID: {0}]", id),
                    Background = new SolidColorBrush(Colors.Red),
                    Foreground = new SolidColorBrush(Colors.White),
                    HorizontalAlignment = alignment
                };

            props.Converter.AddToBlockContainer(imageBlock);
            props.ParseChildren = false;
        }

        /// <summary>
        /// Handle media link inline elements
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void MediaLinkInlineElement(ElementProperties props)
        {
            InlineUIContainer inlineImage = new InlineUIContainer();
            XElement imageElement = props.Element.Descendants(ddue + "image").FirstOrDefault();
            XAttribute href;
            Image image = new Image();
            KeyValuePair<string, string> imageInfo = new KeyValuePair<string, string>(null, null);
            string id = "???";

            if(imageElement != null)
            {
                href = imageElement.Attribute(xlink + "href");

                if(href != null)
                {
                    id = href.Value;

                    if(!props.Converter.MediaFiles.TryGetValue(id, out imageInfo))
                        imageInfo = new KeyValuePair<string, string>(null, null);
                }
            }

            if(!String.IsNullOrEmpty(imageInfo.Key) && File.Exists(imageInfo.Key))
            {
                var bm = new BitmapImage(new Uri(imageInfo.Key));

                // Use the actual image size to mimic the HTML layout
                image.Height = bm.Height;
                image.Width = bm.Width;
                image.ToolTip = !String.IsNullOrEmpty(imageInfo.Value) ? imageInfo.Value :
                    "ID: " + id + "\nFilename: " + imageInfo.Key;
                image.Margin = new Thickness(5, 0, 5, 0);

                image.Source = bm;
                inlineImage.Child = image;
            }
            else
                inlineImage.Child = new TextBlock
                {
                    Text = String.Format("[INVALID IMAGE ID: {0}]", id),
                    Background = new SolidColorBrush(Colors.Red),
                    Foreground = new SolidColorBrush(Colors.White)
                };

            props.Converter.AddInlineToContainer(inlineImage);

            props.ReturnToParent = props.ParseChildren = false;
        }
        #endregion

        #region Table element handlers
        //=====================================================================

        /// <summary>
        /// Handle table elements
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void TableElement(ElementProperties props)
        {
            // If there is a title element, add a title using its content right before the table.  The title
            // element handler will ignore it since it will have this table as its parent.
            XElement title = props.Element.Element(ddue + "title");

            if(title != null)
            {
                Paragraph p = new Paragraph(new Run(reCondenseWhitespace.Replace(title.Value.Trim(), " ")));
                props.Converter.AddNonParentToBlockContainer(p);

                p.SetResourceReference(Paragraph.StyleProperty, NamedStyle.TableTitle);
            }

            props.Converter.AddToBlockContainer(new Table());
        }

        /// <summary>
        /// Handle table header elements
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void TableHeaderElement(ElementProperties props)
        {
            var g = new TableRowGroup();
            props.Converter.AddToBlockContainer(g);
            g.SetResourceReference(TableRowGroup.StyleProperty, NamedStyle.TableHeaderRow);
        }

        /// <summary>
        /// Handle table row elements
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void RowElement(ElementProperties props)
        {
            // If not already in a table row group, add the group first
            if(!(props.Converter.CurrentBlockElement is TableRowGroup))
            {
                var group = new TableRowGroup();

                // We'll use this group as the parent for all child elements but we don't want to
                // push it on the stack.  The current element should be a table.
                ((Table)props.Converter.CurrentBlockElement).RowGroups.Add(group);
                props.Converter.CurrentBlockElement = group;
            }

            props.Converter.AddToBlockContainer(new TableRow());
        }

        /// <summary>
        /// Handle table cell entry elements
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void EntryElement(ElementProperties props)
        {
            props.Converter.AddToBlockContainer(new TableCell());
        }
        #endregion

        #region Formatted inline element handlers
        //=====================================================================

        /// <summary>
        /// Handle elements with generic bold styling
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void BoldElement(ElementProperties props)
        {
            props.Converter.AddInlineToContainer(new Bold());
        }

        /// <summary>
        /// Handle elements with inline code styling
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void InlineCodeElement(ElementProperties props)
        {
            Span s = new Span();
            props.Converter.AddInlineToContainer(s);
            s.SetResourceReference(Run.StyleProperty, NamedStyle.CodeInline);
        }

        /// <summary>
        /// Handle elements with generic italic styling
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void ItalicElement(ElementProperties props)
        {
            props.Converter.AddInlineToContainer(new Italic());
        }

        /// <summary>
        /// Handle literal elements
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void LiteralElement(ElementProperties props)
        {
            Span s = new Span();
            props.Converter.AddInlineToContainer(s);
            s.SetResourceReference(Span.StyleProperty, NamedStyle.Literal);
        }

        /// <summary>
        /// Handle math elements
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void MathElement(ElementProperties props)
        {
            Italic i = new Italic();
            props.Converter.AddInlineToContainer(i);
            i.SetResourceReference(Italic.StyleProperty, NamedStyle.Math);
        }

        /// <summary>
        /// Handle elements with generic subscript styling
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void SubscriptElement(ElementProperties props)
        {
            // Not all fonts support the superscript and subscript variants and WPF 4.0 has some bugs in
            // it's handling of digits with those variants.  As such, we'll use the baseline alignment and
            // a smaller font size to achieve a similar result.
            Span s = new Span(new Run(reCondenseWhitespace.Replace(props.Element.Value.Trim(), " ")));
            s.BaselineAlignment = BaselineAlignment.Subscript;
            s.FontSize = s.FontSize * 0.75;

            props.Converter.AddInlineToContainer(new Span(s));
            props.ParseChildren = false;
        }

        /// <summary>
        /// Handle elements with generic superscript styling
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void SuperscriptElement(ElementProperties props)
        {
            // Not all fonts support the superscript and subscript variants and WPF 4.0 has some bugs in
            // it's handling of digits with those variants.  As such, we'll use the baseline alignment and
            // a smaller font size to achieve a similar result.
            Span s = new Span(new Run(reCondenseWhitespace.Replace(props.Element.Value.Trim(), " ")));
            s.BaselineAlignment = BaselineAlignment.Superscript;
            s.FontSize = s.FontSize * 0.75;

            props.Converter.AddInlineToContainer(s);
            props.ParseChildren = false;
        }

        /// <summary>
        /// Handle elements with generic underline styling
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void UnderlineElement(ElementProperties props)
        {
            props.Converter.AddInlineToContainer(new Underline());
        }
        #endregion

        #region Link element handlers
        //=====================================================================

        /// <summary>
        /// Handle code entity reference elements
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void CodeEntityReferenceElement(ElementProperties props)
        {
            XAttribute hint;
            string memberId = (props.Element.Value ?? String.Empty).Trim();
            string[] parts;
            bool qualifyHint;
            char prefix = 'N';
            int pos;

            hint = props.Element.Attribute("qualifyHint");

            if(hint == null || !Boolean.TryParse(hint.Value, out qualifyHint))
                qualifyHint = false;

            // Remove parameters from the ID
            pos = memberId.IndexOf('(');

            if(pos != -1)
                memberId = memberId.Substring(0, pos);

            // Remove the type prefix
            if(memberId.Length > 2 && memberId[1] == ':')
            {
                prefix = memberId[0];
                memberId = memberId.Substring(2);
            }

            parts = memberId.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            // If qualified, add the appropriate parts
            if(qualifyHint)
            {
                if(prefix != 'N' && prefix != 'T')
                    memberId = String.Join(".", parts, parts.Length - 2, 2);
            }
            else
                memberId = parts[parts.Length - 1];

            if(parts.Length > 2)
                memberId = memberId.Replace("#ctor", parts[parts.Length - 2]).Replace(
                    "#cctor", parts[parts.Length - 2]);

            props.Converter.AddInlineToContainer(new Bold(new Run(memberId)));
            props.ParseChildren = false;
        }

        /// <summary>
        /// Handle external link elements
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void ExternalLinkElement(ElementProperties props)
        {
            XElement linkTextElement = props.Element.Descendants(ddue + "linkText").FirstOrDefault(),
                linkAltTextElement = props.Element.Descendants(ddue + "linkAlternateText").FirstOrDefault(),
                linkUriElement = props.Element.Descendants(ddue + "linkUri").FirstOrDefault(),
                linkTargetElement = props.Element.Descendants(ddue + "linkTarget").FirstOrDefault();
            string linkText, linkUri;

            if(linkUriElement != null)
                linkUri = linkUriElement.Value;
            else
                linkUri = "none://MISSING_LINKURI_ELEMENT";

            if(linkTextElement != null)
                linkText = reCondenseWhitespace.Replace(linkTextElement.Value.Trim(), " ");
            else
                linkText = linkUri;

            Hyperlink l = new Hyperlink { NavigateUri = new Uri(linkUri, UriKind.RelativeOrAbsolute) };

            if(linkAltTextElement != null)
                l.ToolTip = linkAltTextElement.Value;

            if(linkTargetElement != null)
                l.TargetName = linkTargetElement.Value;

            l.Inlines.Add(new Run(linkText));

            props.Converter.AddInlineToContainer(l);

            props.ParseChildren = false;
        }

        /// <summary>
        /// Handle link elements
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void LinkElement(ElementProperties props)
        {
            XAttribute href;
            string id, title;
            int pos;

            href = props.Element.Attribute(xlink + "href");

            if(href != null)
                id = href.Value;
            else
                id = "[MISSING LINK HREF]";

            props.Converter.AddInlineToContainer(new Hyperlink { NavigateUri = new Uri("link://" + id) });

            // Add the topic title as the link text if there is no inner text
            if(!props.ParseChildren)
            {
                pos = id.IndexOf('#');

                if(pos != -1)
                    id = id.Substring(0, pos);

                if(!props.Converter.TopicTitles.TryGetValue(id, out title))
                    title = "[UNKNOWN TOPIC ID: " + id + "]";

                props.Converter.AddInlineToContainer(new Run(title));
            }
            else
            {
                props.Converter.AddInlineToContainer(new Run(reCondenseWhitespace.Replace(
                    props.Element.Value.Trim(), " ")));
                props.ParseChildren = false;
            }
        }
        #endregion

        #region Misellaneous element handlers
        //=====================================================================

        /// <summary>
        /// Handle auto-outline elements
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void AutoOutlineElement(ElementProperties props)
        {
            XElement parent;
            XAttribute attribute;
            List list;
            string leadText = String.Empty;
            bool excludeRelatedTopics, isInIntro = (props.Element.Parent.Name.LocalName == "introduction");
            int maxDepth;

            props.ParseChildren = props.ReturnToParent = false;

            attribute = props.Element.Attribute("lead");

            if(attribute != null)
            {
                leadText = attribute.Value.Trim();

                if(leadText == "none")
                    leadText = null;
            }

            if(leadText != null && leadText.Length == 0)
                if(isInIntro)
                    leadText = "This topic contains the following sections.";
                else
                    leadText = "This section contains the following subsections.";

            if(isInIntro)
            {
                parent = props.Element.Parent.Parent;
                attribute = props.Element.Attribute("excludeRelatedTopics");

                if(attribute == null || !Boolean.TryParse(attribute.Value, out excludeRelatedTopics))
                    excludeRelatedTopics = false;

                if(!excludeRelatedTopics)
                {
                    var relatedTopics = parent.Element(ddue + "relatedTopics");

                    if(relatedTopics == null || !relatedTopics.HasElements)
                        excludeRelatedTopics = true;
                }
            }
            else
            {
                parent = props.Element.Parent.Parent.Elements(ddue + "sections").FirstOrDefault();
                excludeRelatedTopics = true;
            }

            if(String.IsNullOrEmpty(props.Element.Value) || !Int32.TryParse(props.Element.Value, out maxDepth))
                maxDepth = 0;

            Section s = new Section();
            props.Converter.AddNonParentToBlockContainer(s);

            if(leadText != null)
            {
                Paragraph p = new Paragraph();
                p.Inlines.Add(new Run(leadText));
                s.Blocks.Add(p);
            }

            if(parent != null)
            {
                list = InsertAutoOutline(parent, 0, maxDepth);

                if(list != null)
                {
                    // If not exluding related topics, add a link to the See Also section
                    if(!excludeRelatedTopics)
                        list.ListItems.Add(new ListItem(new Paragraph(new Hyperlink(new Run("See Also"))
                        {
                            NavigateUri = new Uri("link://#seeAlsoSection")
                        })));

                    s.Blocks.Add(list);
                }
            }
        }

        /// <summary>
        /// This is used to recursively add auto-outline links
        /// </summary>
        /// <param name="parent">The parent element from which to extract sections</param>
        /// <param name="depth">The current depth of the outline</param>
        /// <param name="maxDepth">The maximum depth to recurse for section titles</param>
        /// <returns>A list element containing the outline or null if no valid sections where found</returns>
        private static List InsertAutoOutline(XElement parent, int depth, int maxDepth)
        {
            List list = new List(), subList;
            ListItem listItem;
            Paragraph p;
            XElement titleElement;
            XAttribute address;
            string title;

            foreach(var section in parent.Elements(ddue + "section"))
            {
                titleElement = section.Element(ddue + "title");

                if(titleElement == null || titleElement.Value.Trim().Length == 0)
                    continue;

                title = reCondenseWhitespace.Replace(titleElement.Value, " ").Trim();
                address = section.Attribute("address");

                listItem = new ListItem();
                list.ListItems.Add(listItem);
                p = new Paragraph();
                listItem.Blocks.Add(p);

                if(address == null)
                    p.Inlines.Add(new Run(title));
                else
                    p.Inlines.Add(new Hyperlink(new Run(title))
                    {
                        NavigateUri = new Uri("link://#" + address.Value.Trim())
                    });

                if(depth < maxDepth)
                    foreach(var subsection in section.Elements(ddue + "sections"))
                    {
                        subList = InsertAutoOutline(subsection, depth + 1, maxDepth);

                        if(subList != null)
                            listItem.Blocks.Add(subList);
                    }
            }

            return (list.ListItems.Count != 0) ? list : null;
        }

        /// <summary>
        /// Handle ignored elements
        /// </summary>
        /// <param name="props">The element properties</param>
        /// <remarks>For ignored elements, we don't return to the parent element after parsing
        /// this one's children as there may be other sibiling elements after it.</remarks>
        private static void IgnoredElement(ElementProperties props)
        {
            props.ReturnToParent = false;
        }

        /// <summary>
        /// Handle ignored elements with ignored child elements
        /// </summary>
        /// <param name="props">The element properties</param>
        /// <remarks>This ignores all child elements too.  For ignored elements, we don't return to the parent
        /// element as there may be other sibiling elements after it.</remarks>
        private static void IgnoredElementWithChildren(ElementProperties props)
        {
            props.ParseChildren = props.ReturnToParent = false;
        }

        /// <summary>
        /// Handle markup elements
        /// </summary>
        /// <param name="props">The element properties</param>
        /// <remarks>Markup elements can contain anything so no attempt is made to parse the content.
        /// Its is added as an inline or block element depending on the current context.</remarks>
        private static void MarkupElement(ElementProperties props)
        {
            // Get the content with all formatting preserved
            string content = reRemoveNamespace.Replace(
                props.Element.Nodes().Aggregate("", (c, node) => c += node.ToString()), String.Empty);

            // This can be a block or inline element depending on the parent element
            Span span = props.Converter.CurrentSpanElement as Span;

            if(span != null)
            {
                Run r = new Run
                {
                    Text = reCondenseWhitespace.Replace(content, " "),
                    Background = new SolidColorBrush(Color.FromRgb(0xEF, 0xEF, 0xF7))
                };

                span.Inlines.Add(r);
                props.ReturnToParent = false;
            }
            else
            {
                Paragraph p = props.Converter.CurrentBlockElement as Paragraph;

                if(p != null)
                {
                    Run r = new Run
                    {
                        Text = reCondenseWhitespace.Replace(content, " "),
                        Background = new SolidColorBrush(Color.FromRgb(0xEF, 0xEF, 0xF7))
                    };

                    p.Inlines.Add(r);
                    props.ReturnToParent = false;
                }
                else
                {
                    Section s = new Section();
                    props.Converter.AddToBlockContainer(s);

                    p = new Paragraph();
                    s.Blocks.Add(p);
                    p.Inlines.Add(new Run(StripLeadingWhitespace(content, 4)));

                    s.SetResourceReference(Section.StyleProperty, NamedStyle.CodeBlock);
                }
            }

            props.ParseChildren = false;
        }

        /// <summary>
        /// Handle token elements
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void TokenElement(ElementProperties props)
        {
            XElement token;

            if(props.Converter.Tokens.TryGetValue(props.Element.Value.Trim(), out token))
            {
                props.Converter.ParseChildren(token.Nodes());

                // Add elements after the token to the current parent element
                props.ReturnToParent = false;
            }
            else
            {
                props.Converter.AddInlineToContainer(new Bold(new Run(String.Format(
                    "[MISSING TOKEN: {0}]", props.Element.Value.Trim()))));

                // We just added a bold span so it will need to go back to the last parent
                // span if any on return.
            }

            props.ParseChildren = false;
        }
        #endregion

        #region Glossary element handlers
        //=====================================================================

        /// <summary>
        /// Handle all aspects of glossary element and sub-element formatting
        /// </summary>
        /// <param name="props">The element properties</param>
        private static void GlossaryElement(ElementProperties props)
        {
            Section glossary = new Section();
            Paragraph p = null;
            XElement titleElement;
            XAttribute addressAttr;
            List<XElement> divisions;
            Dictionary<XElement, Tuple<string, string>> divisionIds = new Dictionary<XElement, Tuple<string, string>>();
            List<GlossaryEntry> entries = new List<GlossaryEntry>();
            string address, id, title;
            bool isFirst = true;
            int autoId = 1;

            // All elements are handled here
            props.ParseChildren = false;
            props.Converter.AddToBlockContainer(glossary);

            // If there is a title element, add a title using its content
            titleElement = props.Element.Element(ddue + "title");

            if(titleElement != null)
            {
                p = new Paragraph(new Run(reCondenseWhitespace.Replace(titleElement.Value.Trim(), " ")));
                glossary.Blocks.Add(p);

                p.SetResourceReference(Paragraph.StyleProperty, NamedStyle.Title);
            }

            // See if there are divisions.  If so, add one section for each division.  If not, lump all entries
            // into one untitled division.
            divisions = props.Element.Descendants(ddue + "glossaryDiv").ToList();

            // If there are multiple divisions, add a link to each one provided we have an address and a title
            if(divisions.Count == 0)
            {
                divisions = new List<XElement>() { props.Element };
                divisionIds.Add(props.Element, Tuple.Create<string, string>(null, null));
            }
            else
            {
                foreach(var d in divisions)
                {
                    addressAttr = d.Attribute("address");
                    titleElement = d.Element(ddue + "title");

                    if(addressAttr != null)
                    {
                        address = addressAttr.Value;
                        id = ToElementName(address);
                    }
                    else
                        address = id = null;

                    if(titleElement != null)
                        title = reCondenseWhitespace.Replace(titleElement.Value.Trim(), " ");
                    else
                        title = null;

                    divisionIds.Add(d, Tuple.Create(id, title));

                    if(!String.IsNullOrEmpty(title))
                    {
                        if(isFirst)
                        {
                            p = new Paragraph();
                            glossary.Blocks.Add(p);
                            isFirst = false;
                        }
                        else
                            p.Inlines.Add(new Run(" | "));

                        if(!String.IsNullOrEmpty(address))
                            p.Inlines.Add(new Hyperlink(new Run(title)) { NavigateUri = new Uri("link://#" + address) });
                        else
                            p.Inlines.Add(new Bold(new Run(title)));
                    }
                }

                p.Inlines.Add(new LineBreak());
            }

            // Extract all glossary entries for use in creating the divisions.  Entries may refer to related
            // entries in other divisions so we need to get them all up front.
            entries.AddRange(props.Element.Descendants(
                MamlToFlowDocumentConverter.ddue + "glossaryEntry").Select(g => new GlossaryEntry(g)));

            // Render each division
            foreach(var d in divisions)
            {
                var titleAndId = divisionIds[d];

                // Add a title if there is one
                if(!String.IsNullOrEmpty(titleAndId.Item2))
                {
                    id = titleAndId.Item1;
                    p = new Paragraph(new Run(titleAndId.Item2)) { Name = id };
                    glossary.Blocks.Add(p);
                    p.SetResourceReference(Paragraph.StyleProperty, NamedStyle.GlossaryDivisionTitle);
                }
                else
                {
                    id = "__GlossaryDiv" + autoId.ToString();
                    autoId++;
                }

                RenderGlossaryDivision(d, id, entries, props.Converter);

                glossary.Blocks.Add(new Paragraph());
            }
        }

        /// <summary>
        /// This is used to render a glossary division
        /// </summary>
        /// <param name="d">The root glossary division element</param>
        /// <param name="divisionId">The division ID used as a prefix for each letter section in the division</param>
        /// <param name="entries">An enumerable list of all glossary entries</param>
        /// <param name="converter">The converter to which the glossary elements will be added</param>
        /// <returns>An enumerable list of blocks that define the glossary division in the document</returns>
        private static void RenderGlossaryDivision(XElement d, string divisionId,
          IEnumerable<GlossaryEntry> entries, MamlToFlowDocumentConverter converter)
        {
            Section s;
            Paragraph p = new Paragraph();
            bool isFirst = true;

            // Group all entries in this division by the first letter of the first term
            var groupLetters = entries.Where(g => g.Parent == d).GroupBy(
                g => Char.ToUpperInvariant(g.Terms.First().Key[0])).OrderBy(g => g.Key);

            // Generate the letter bar for the division
            for(char ch = 'A'; ch <= 'Z'; ch++)
            {
                if(isFirst)
                    isFirst = false;
                else
                    p.Inlines.Add(new Run(" | "));

                if(!groupLetters.Any(g => g.Key == ch))
                    p.Inlines.Add(new Bold(new Run(ch.ToString())));
                else
                    p.Inlines.Add(new Hyperlink(new Run(ch.ToString()))
                    {
                        NavigateUri = new Uri("link://#" + divisionId + "_" + ch.ToString())
                    });
            }

            converter.AddNonParentToBlockContainer(p);
            p.SetResourceReference(Paragraph.StyleProperty, NamedStyle.GlossaryLetterBar);

            foreach(var g in groupLetters)
            {
                p = new Paragraph(new Run(g.Key.ToString())
                {
                    Name = ToElementName(divisionId + "_" + g.Key.ToString())
                });

                converter.AddNonParentToBlockContainer(p);
                p.SetResourceReference(Paragraph.StyleProperty, NamedStyle.GlossaryLetterTitle);

                foreach(var entry in g)
                {
                    s = new Section();

                    converter.AddNonParentToBlockContainer(s);
                    s.SetResourceReference(Section.StyleProperty, NamedStyle.GlossaryDefinition);

                    p = new Paragraph();
                    isFirst = true;

                    foreach(var t in entry.Terms)
                    {
                        if(isFirst)
                            isFirst = false;
                        else
                            p.Inlines.Add(new Run(", "));

                        p.Inlines.Add(new Bold(new Run(t.Key))
                        {
                            Name = (t.Value == null) ? String.Empty : ToElementName(t.Value)
                        });
                    }

                    s.Blocks.Add(p);
                    p.SetResourceReference(Paragraph.StyleProperty, NamedStyle.NoMargin);

                    converter.ParseChildren(s, entry.Definition.Nodes());

                    if(entry.RelatedEntries.Count != 0)
                    {
                        p = new Paragraph();
                        p.Inlines.Add(new Run("See also: "));
                        isFirst = true;

                        foreach(var r in entry.RelatedEntries)
                        {
                            if(isFirst)
                                isFirst = false;
                            else
                                p.Inlines.Add(new Run(", "));

                            var related = entries.SelectMany(e => e.Terms).First(t => t.Value == r);

                            p.Inlines.Add(new Hyperlink(new Run(related.Key))
                            {
                                NavigateUri = new Uri("link://#" + r)
                            });
                        }

                        p.Inlines.Add(new LineBreak());
                        s.Blocks.Add(p);
                    }
                }
            }
        }
        #endregion
    }
}
