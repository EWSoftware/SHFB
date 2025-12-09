//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : MamlToFlowDocumentConverter.Handlers.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/08/2025
// Note    : Copyright 2012-2025, Eric Woodruff, All rights reserved
//
// This file contains the element handler methods for the MAML to flow document converter class
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/02/2012  EFW  Created the code
// 11/26/2012  EFW  Added support for imported code blocks
// 01/11/2013  EFW  Added support for colorizing code blocks
//===============================================================================================================

// Ignore Spelling: endregion pragma lang nobullet

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace SandcastleBuilder.WPF.Maml;

// This contains the element handlers for the MAML to flow document converter
public partial class MamlToFlowDocumentConverter
{
    #region Helper methods
    //=====================================================================

    /// <summary>
    /// This is used to load a code block from an external file
    /// </summary>
    /// <param name="sourceFile">The source file from which to obtain the code.</param>
    /// <param name="regionName">An optional region name to limit what is imported.</param>
    /// <param name="removeRegionMarkers">True to removed region markers or false to keep them if importing
    /// a region.</param>
    /// <returns>The code block extracted from the file.</returns>
    private static string LoadCodeBlock(string sourceFile, string regionName, bool removeRegionMarkers)
    {
        Regex reFindRegion;
        Match find, m;
        string codeBlock = null, basePath = ImportedCodeBasePath ?? String.Empty;

        if(String.IsNullOrEmpty(sourceFile))
        {
            return "ERROR: A nested <code> tag must contain a \"source\" attribute that specifies the " +
                "source file to import";
        }

        try
        {
            sourceFile = Environment.ExpandEnvironmentVariables(sourceFile);

            if(!Path.IsPathRooted(sourceFile))
                sourceFile = Path.GetFullPath(Path.Combine(basePath, sourceFile));

            using StreamReader sr = new(sourceFile);
            codeBlock = sr.ReadToEnd();
        }
        catch(ArgumentException argEx)
        {
            return String.Format(CultureInfo.InvariantCulture, "Possible invalid path '{0}{1}'.  Error: {2}",
                basePath, sourceFile, argEx.Message);
        }
        catch(IOException ioEx)
        {
            return String.Format(CultureInfo.InvariantCulture, "Unable to load source file '{0}'.  Error: {1}",
                sourceFile, ioEx.Message);
        }

        // If no region is specified, the whole file is included
        if(!String.IsNullOrWhiteSpace(regionName))
        {
            // Find the start of the region.  This gives us an immediate starting match on the second
            // search and we can look for the matching #endregion without caring about the region name.
            // Otherwise, nested regions get in the way and complicate things.  The bit at the end ensures
            // that shorter region names aren't matched in longer ones with the same start that occur before
            // the shorter one.
            reFindRegion = new Regex("\\#(pragma\\s+)?region\\s+\"?" + Regex.Escape(regionName) +
                "\\s*?[\"->]*?\\s*?[\\r\\n]", RegexOptions.IgnoreCase);

            find = reFindRegion.Match(codeBlock);

            if(!find.Success)
            {
                return String.Format(CultureInfo.InvariantCulture, "Unable to locate start of region '{0}' " +
                    "in source file '{1}'", regionName, sourceFile);
            }

            // Find the end of the region taking into account any nested regions
            m = reMatchRegion.Match(codeBlock, find.Index);

            if(!m.Success)
            {
                return String.Format(CultureInfo.InvariantCulture, "Unable to extract region '{0}' in " +
                    "source file '{1}{2}' (missing #endregion?)", regionName, basePath, sourceFile);
            }

            // Extract just the specified region starting after the description
            codeBlock = m.Groups[2].Value.Substring(m.Groups[2].Value.IndexOf('\n') + 1);

            // Strip off the trailing comment characters if present
            if(codeBlock[codeBlock.Length - 1] == ' ')
                codeBlock = codeBlock.TrimEnd();

            // VB commented #End Region statement within a method body
            if(codeBlock[codeBlock.Length - 1] == '\'')
                codeBlock = codeBlock.Substring(0, codeBlock.Length - 1);

            // XML/XAML commented #endregion statement
            if(codeBlock.EndsWith("<!--", StringComparison.Ordinal))
                codeBlock = codeBlock.Substring(0, codeBlock.Length - 4);

            // C or SQL style commented #endregion statement
            if(codeBlock.EndsWith("/*", StringComparison.Ordinal) ||
              codeBlock.EndsWith("--", StringComparison.Ordinal))
            {
                codeBlock = codeBlock.Substring(0, codeBlock.Length - 2);
            }

            // Batch file remark
            if(codeBlock.EndsWith("REM", StringComparison.OrdinalIgnoreCase) && codeBlock.Length > 3 &&
              (codeBlock[codeBlock.Length - 4] == '\r' || codeBlock[codeBlock.Length - 4] == '\n'))
            {
                codeBlock = codeBlock.Substring(0, codeBlock.Length - 3);
            }
        }

        if(removeRegionMarkers)
        {
            codeBlock = reRemoveRegionMarkers.Replace(codeBlock, String.Empty);
            codeBlock = codeBlock.Replace("\r\n\n", "\r\n");
        }

        return StripLeadingWhitespace(codeBlock, 4);
    }

    /// <summary>
    /// This is used to strip a common amount of leading whitespace on all  lines of a text block and to
    /// convert tabs to a consistent number of spaces.
    /// </summary>
    /// <param name="text">The text containing the lines to clean up</param>
    /// <param name="tabSize">The number of spaces to which tab characters are converted</param>
    private static string StripLeadingWhitespace(string text, int tabSize)
    {
        string[] lines;
        string currentLine, tabsToSpaces = new(' ', tabSize);
        int minSpaces, spaceCount, line;

        if(String.IsNullOrEmpty(text))
            return text;

        // Replace "\r\n" with "\n" first for consistency
        lines = text.Replace("\r\n", "\n").Split(['\n']);

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
                {
                    if(currentLine[spaceCount] != ' ')
                        break;
                }

                if(spaceCount == currentLine.Length)
                    currentLine = String.Empty;
                else
                {
                    if(spaceCount < minSpaces)
                        minSpaces = spaceCount;
                }

                lines[line] = currentLine;
            }
        }

        // Unlikely, but it could happen...
        if(minSpaces == Int32.MaxValue)
            minSpaces = 0;

        // The second pass joins them back together less the determined amount of leading whitespace
        StringBuilder sb = new(text.Length);

        for(line = 0; line < lines.Length; line++)
        {
            currentLine = lines[line];

            if(currentLine.Length != 0)
                sb.AppendLine(currentLine.Substring(minSpaces));
            else
            {
                if(sb.Length > 0)   // Skip leading blank lines
                    sb.AppendLine();
            }
        }

        // Trim off trailing blank lines too
        return sb.ToString().TrimEnd([' ', '\r', '\n']);
    }

    /// <summary>
    /// This is used to add a block of colorized code to the flow document
    /// </summary>
    /// <param name="code">The code to colorize</param>
    /// <param name="language">The language to use when colorizing the code</param>
    /// <param name="numberLines">True to number lines, false if not</param>
    /// <param name="container">The container for the colorized code elements</param>
    private static void ColorizeCode(string code, string language, bool numberLines, Paragraph container)
    {
        try
        {
            // Colorize the code
            CodeColorizer.NumberLines = numberLines;

            string colorizedContent = CodeColorizer.ColorizePlainText(code, language);

            // Insert the colorized code into the flow document template
            colorizedContent = flowDocumentContent.Replace("@CONTENT@", colorizedContent);

            // The following steps can take a few seconds for large documents with complex coloring such
            // as large XML files.
            if(XamlReader.Parse(colorizedContent) is FlowDocument fd)
            {
                using MemoryStream stream = new();

                // Flow document elements are attached to their parent document.  To break the bond we
                // need to stream it out and then back in again before adding them to the current
                // document.  A side effect of this is that it converts the named styles into literal
                // style elements so we don't need the named styles added to the end document.
                Block b = fd.Blocks.FirstBlock;

                TextRange range = new(b.ContentStart, b.ContentEnd);
                range.Save(stream, DataFormats.XamlPackage);

                range = new(container.ContentEnd, container.ContentEnd);
                range.Load(stream, DataFormats.XamlPackage);
            }
        }
        catch(Exception ex)
        {
            container.Inlines.Add(new Run("Unable to colorize code: " + ex.Message));
        }
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

        // Map the class name to a title
        attribute = props.Element.Attribute("class");

        if(attribute == null || !alertTitles.TryGetValue(attribute.Value, out string title))
            title = "Note";

        if(attribute == null || !alertIcons.TryGetValue(attribute.Value, out string icon))
            icon = "AlertNote";

        attribute = props.Element.Attribute("title");

        if(attribute != null)
            title = attribute.Value;

        Section alert = new();
        props.Converter.AddToBlockContainer(alert);

        Paragraph p = new();
        alert.Blocks.Add(p);

        // Set the icon based on the alert type
        var iconImage = (ImageSource)props.Converter.Document.Resources[icon];

        p.Inlines.Add(new InlineUIContainer(new Image
        {
            Source = iconImage,
            Height = iconImage.Height / 2,
            Width = iconImage.Width / 2,
            ToolTip = title,
            Margin = new Thickness(0, 0, 5, 0),
            VerticalAlignment = VerticalAlignment.Center
        }));

        p.Inlines.Add(new Run(title));
        p.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.AlertTitle);

        Section alertBody = new();
        alert.Blocks.Add(alertBody);

        // We want the body section to be the current parent here rather than the containing section
        props.Converter.CurrentBlockElement = alertBody;

        alertBody.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.AlertBody);
    }

    /// <summary>
    /// Handle code elements
    /// </summary>
    /// <param name="props">The element properties</param>
    private static void CodeElement(ElementProperties props)
    {
        var codeElement = props.Element;

        // For code elements with no attributes, treat them like a codeInline element
        if(!codeElement.HasAttributes && codeElement.Name.LocalName == "code")
        {
            if(props.Converter.CurrentBlockElement is not Paragraph)
                props.Converter.AddToBlockContainer(new Paragraph());

            InlineCodeElement(props);
            return;
        }

        XAttribute attribute;
        Section code = new();
        props.Converter.AddToBlockContainer(code);
        string language = "none", title, sourceFile, region;
        bool removeRegionMarkers;

        if(codeElement.Name.LocalName != "codeReference")
        {
            attribute = codeElement.Attribute("lang") ?? codeElement.Attribute("language") ??
                codeElement.Attribute("class");

            if(attribute != null)
                language = attribute.Value;

            // If there is a title attribute, use that for the title.  If not, map the language ID to
            // a display title.
            attribute = codeElement.Attribute("title");

            // For a blank title, a single space is required.  If empty, use the language title.
            if(attribute != null && attribute.Value.Length != 0)
                title = attribute.Value;
            else
            {
                if(CodeColorizer == null || !CodeColorizer.FriendlyNames.TryGetValue(language, out title))
                    title = String.Empty;
            }

            // If there are nested code blocks, import the code and replace them with their content
            foreach(var nestedBlock in codeElement.Descendants(ddue + "code").ToList())
            {
                attribute = nestedBlock.Attribute("source");

                if(attribute != null)
                {
                    sourceFile = attribute.Value;
                    attribute = nestedBlock.Attribute("region");

                    if(attribute != null)
                        region = attribute.Value;
                    else
                        region = null;

                    attribute = nestedBlock.Attribute("removeRegionMarkers");

                    if(attribute == null || !Boolean.TryParse(attribute.Value, out removeRegionMarkers))
                        removeRegionMarkers = false;

                    nestedBlock.Value = LoadCodeBlock(sourceFile, region, removeRegionMarkers);
                }
            }
        }
        else
            title = "Code Reference";

        // Create the title and Copy link elements.  These will reside in a grid in a block UI container.
        Grid g = new();

        g.ColumnDefinitions.Add(new ColumnDefinition());
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        TextBlock tb = new()
        {
            TextAlignment = TextAlignment.Left,
            FontWeight = FontWeights.Bold,
            Text = title
        };

        Grid.SetColumn(tb, 0);
        g.Children.Add(tb);

        Hyperlink l = new(new Run("Copy"))
        {
            // The URI signals the generic handler to copy code rather than launch the URL
            NavigateUri = new Uri("copy://code", UriKind.RelativeOrAbsolute),
            ToolTip = "Copy Code",
            FontSize = 10
        };

        tb = new TextBlock(l) { TextAlignment = TextAlignment.Right };
        Grid.SetColumn(tb, 1);
        g.Children.Add(tb);

        BlockUIContainer buic = new(g) { Margin = new Thickness(0, 3, 0, 3) };
        code.Blocks.Add(buic);

        // Create the section that will hold the code block
        Section codeBlock = new();
        code.Blocks.Add(codeBlock);
        codeBlock.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.CodeBlock);

        Paragraph p = new();
        codeBlock.Blocks.Add(p);

        // See if lines are to be numbered
        attribute = codeElement.Attribute("numberLines");

        if(attribute == null || !Boolean.TryParse(attribute.Value, out bool numberLines))
            numberLines = false;

        // If importing from an external file, include that info in the content
        attribute = codeElement.Attribute("source");

        if(attribute != null)
        {
            sourceFile = attribute.Value;
            attribute = codeElement.Attribute("region");

            if(attribute != null)
                region = attribute.Value;
            else
                region = null;

            attribute = codeElement.Attribute("removeRegionMarkers");

            if(attribute == null || !Boolean.TryParse(attribute.Value, out removeRegionMarkers))
                removeRegionMarkers = false;

            if(CodeColorizer != null)
                ColorizeCode(LoadCodeBlock(sourceFile, region, removeRegionMarkers), language, numberLines, p);
            else
                p.Inlines.Add(new Run(LoadCodeBlock(sourceFile, region, removeRegionMarkers)));
        }
        else
        {
            if(codeElement.Name.LocalName != "codeReference" && CodeColorizer != null)
                ColorizeCode(codeElement.Value, language, numberLines, p);
            else
                p.Inlines.Add(new Run(StripLeadingWhitespace(codeElement.Value, 4)));
        }

        props.ParseChildren = false;
    }

    /// <summary>
    /// Handle general paragraph type elements
    /// </summary>
    /// <param name="props">The element properties</param>
    private static void ParagraphElement(ElementProperties props)
    {
        // If empty, ignore it to match Sandcastle's behavior
        if(props.ParseChildren && (props.Element.HasElements || !String.IsNullOrEmpty(props.Element.Value)))
            props.Converter.AddToBlockContainer(new Paragraph());
        else
            props.ReturnToParent = false;
    }

    /// <summary>
    /// Handle figure elements
    /// </summary>
    /// <param name="props">The element properties</param>
    /// <remarks>If the section is empty, it will be omitted</remarks>
    private static void FigureElement(ElementProperties props)
    {
        if(props.ParseChildren && props.Element.HasElements)
        {
            Section s = new();

            var attribute = props.Element.Attribute("placement")?.Value ??
                props.Element.Element(ddue + "figcaption")?.Attribute("placement")?.Value;

            switch(attribute)
            {
                case "center":
                    s.TextAlignment = TextAlignment.Center;
                    break;

                case "far":
                    s.TextAlignment = TextAlignment.Right;
                    break;

                default:
                    break;
            }

            // Ensure paragraphs created in this section inherit the alignment by default
            Style paraStyle = new(typeof(Paragraph));
            paraStyle.Setters.Add(new Setter(Block.TextAlignmentProperty, s.TextAlignment));
            s.Resources.Add(typeof(Paragraph), paraStyle);
            props.Converter.AddToBlockContainer(s);
        }
        else
            props.ParseChildren = props.ReturnToParent = false;
    }

    /// <summary>
    /// Handle figure caption elements
    /// </summary>
    /// <param name="props">The element properties</param>
    private static void FigureCaptionElement(ElementProperties props)
    {
        // If empty, ignore it to match Sandcastle's behavior
        if(props.ParseChildren && (props.Element.HasElements || !String.IsNullOrEmpty(props.Element.Value)))
        {
            Paragraph p = new();

            // The placement may be on the parent figure element or a another figcaption element above the image
            // if this one is placed after the image.
            var attribute = props.Element.Parent?.Attribute("placement")?.Value ??
                props.Element.Parent?.Element(ddue + "figcaption")?.Attribute("placement")?.Value;

            switch(attribute)
            {
                case "center":
                    p.TextAlignment = TextAlignment.Center;
                    break;

                case "far":
                    p.TextAlignment = TextAlignment.Right;
                    break;

                default:
                    break;
            }

            props.Converter.AddToBlockContainer(p);
        }
        else
            props.ReturnToParent = false;
    }

    /// <summary>
    /// Handle quote elements
    /// </summary>
    /// <param name="props">The element properties</param>
    private static void QuoteElement(ElementProperties props)
    {
        if(props.Element.Name == "quote")
        {
            Paragraph p = new();
            props.Converter.AddToBlockContainer(p);
            p.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.Quote);
        }
        else
        {
            Section s = new();
            props.Converter.AddToBlockContainer(s);
            s.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.BlockQuote);
        }
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
            Section s = new();

            // If this is a named section, add the standard title
            if(namedSectionTitles.TryGetValue(props.Element.Name.LocalName, out string title))
            {
                Paragraph p = new(new Run(title));
                s.Blocks.Add(p);
                p.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.Heading2);
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

        if(abstractAttr == null || !Boolean.TryParse(abstractAttr.Value, out bool excluded))
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
            int headingLevel = (int?)props.Element.Attribute("level") ?? 0;

            // Default to a heading level of 2 if not specified since the topic title is level 1
            if(headingLevel < 1)
                headingLevel = 2;
            else
            {
                if(headingLevel > 6)
                    headingLevel = 6;
            }

            Paragraph p = new(new Run(reCondenseWhitespace.Replace(
                props.Element.Value.Trim(), " ")));
            props.Converter.AddToBlockContainer(p);
            p.SetResourceReference(FrameworkContentElement.StyleProperty, headingStyles[headingLevel - 1]);
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
        Paragraph p = new();
        props.Converter.AddToBlockContainer(p);
        p.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.DefinedTerm);
    }

    /// <summary>
    /// Handle definition elements
    /// </summary>
    /// <param name="props">The element properties</param>
    private static void DefinitionElement(ElementProperties props)
    {
        Section s = new();
        props.Converter.AddToBlockContainer(s);
        s.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.Definition);
    }

    /// <summary>
    /// Handle list elements
    /// </summary>
    /// <param name="props">The element properties</param>
    private static void ListElement(ElementProperties props)
    {
        TextMarkerStyle markerStyle = TextMarkerStyle.Disc;
        int startIndex = 1;

        switch(props.Element.Name.LocalName)
        {
            case "list":
            case "steps":
            case "ul":
                switch(props.Element.Attribute("class")?.Value.ToLowerInvariant())
                {
                    case "ordered":
                        markerStyle = TextMarkerStyle.Decimal;
                        startIndex = (int?)props.Element.Attribute("start") ?? 1;
                        break;

                    case "nobullet":
                    case "noBullet":    // Markdown uses the actual style name
                        markerStyle = TextMarkerStyle.None;
                        break;

                    default:
                        break;
                }
                break;

            case "ol":
                markerStyle = TextMarkerStyle.Decimal;
                startIndex = (int?)props.Element.Attribute("start") ?? 1;
                break;

            default:
                break;
        }

        props.Converter.AddToBlockContainer(new List { MarkerStyle = markerStyle, StartIndex = startIndex });
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
        List<XElement> tasks = [], reference = [], concepts = [], otherResources = [], tokenContent = [];
        XAttribute attribute;
        string linkType;

        if(!props.ParseChildren || !props.Element.HasElements)
        {
            props.ParseChildren = props.ReturnToParent = false;
            return;
        }

        // All links are handled here
        props.ParseChildren = false;

        // A name is added here for use by autoOutline elements
        Section s = new() { Name = ToElementName("seeAlsoSection") };
        props.Converter.AddToBlockContainer(s);

        // Add the section title
        Paragraph p = new(new Run(namedSectionTitles[props.Element.Name.LocalName]));
        s.Blocks.Add(p);
        p.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.Heading2);

        // Expand tokens first
        foreach(var link in props.Element.Nodes().OfType<XElement>().Where(n => n.Name.LocalName == "token"))
        {
            if(props.Converter.Tokens.TryGetValue(link.Value.Trim(), out XElement token))
                tokenContent.AddRange(token.Nodes().OfType<XElement>());
        }

        // Group the elements by type or topic ID
        foreach(var link in props.Element.Nodes().OfType<XElement>().Concat(tokenContent))
        {
            linkType = link.Name.LocalName;
            attribute = link.Attribute("topicType_id");

            if(attribute == null || !Guid.TryParse(attribute.Value, out Guid topicId))
                topicId = Guid.Empty;

            attribute = link.Attribute(xlink + "href");

            if(attribute == null || !Guid.TryParse(attribute.Value, out Guid href))
                href = Guid.Empty;

            if(href != Guid.Empty && (linkType == "link" || linkType == "legacyLink") && (
              topicId == HowToId || topicId == WalkthroughId || topicId == SampleId ||
              topicId == TroubleshootingId))
            {
                tasks.Add(link);
            }
            else
            {
                if(linkType == "codeEntityReference" || ((linkType == "link" || linkType == "legacyLink") && (
                  href == Guid.Empty || topicId == ReferenceWithoutSyntaxId ||
                  topicId == ReferenceWithSyntaxId || topicId == XmlReferenceId ||
                  topicId == ErrorMessageId || topicId == UIReferenceId)))
                {
                    reference.Add(link);
                }
                else
                {
                    if(href != Guid.Empty && (linkType == "link" || linkType == "legacyLink") && (
                      topicId == ConceptualId || topicId == SdkTechnologyOverviewArchitectureId ||
                      topicId == SdkTechnologyOverviewCodeDirectoryId ||
                      topicId == SdkTechnologyOverviewScenariosId ||
                      topicId == SdkTechnologyOverviewTechnologySummaryId))
                    {
                        concepts.Add(link);
                    }
                    else
                    {
                        if(linkType == "externalLink" || ((linkType == "link" || linkType == "legacyLink") &&
                          href != Guid.Empty && (topicId == Guid.Empty || topicId == OrientationId ||
                          topicId == WhitePaperId || topicId == CodeEntityId || topicId == GlossaryId ||
                          topicId == SDKTechnologyOverviewOrientationId)))
                        {
                            otherResources.Add(link);
                        }
                    }
                }
            }
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
            p.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.RelatedTopicTitle);

            p = new Paragraph();
            s.Blocks.Add(p);
            p.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.NoTopMargin);

            foreach(var link in links)
            {
                if(isFirst)
                    isFirst = false;
                else
                    p.Inlines.Add(new LineBreak());

                converter.ParseChildren(p, [link]);
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
        BlockUIContainer imageBlock = new();
        XElement imageElement = props.Element.Descendants(ddue + "image").FirstOrDefault(),
            captionElement = props.Element.Descendants(ddue + "caption").FirstOrDefault();
        XAttribute attribute;
        HorizontalAlignment alignment = HorizontalAlignment.Left;
        KeyValuePair<string, string> imageInfo = new(null, null);
        Image image = new();
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
            {
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
            var bm = new BitmapImage();

            // Cache on load to prevent it locking the image and ignore the cache so that changes to the
            // image are reflected in the topic when reloaded.
            bm.BeginInit();
            bm.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bm.CacheOption = BitmapCacheOption.OnLoad;
            bm.UriSource = new Uri(imageInfo.Key);
            bm.EndInit();

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
                StackPanel sp = new()
                {
                    HorizontalAlignment = alignment
                };

                if(captionAfter)
                    sp.Children.Add(image);

                StackPanel spChild = new()
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = alignment
                };

                if(!String.IsNullOrEmpty(leadIn))
                {
                    spChild.Children.Add(new TextBlock
                    {
                        Text = leadIn + ": ",
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(Color.FromRgb(0, 0x33, 0x99))
                    });
                }

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
        {
            imageBlock.Child = new TextBlock
            {
                Text = String.Format(CultureInfo.InvariantCulture, "[INVALID IMAGE ID: {0}]", id),
                Background = new SolidColorBrush(Colors.Red),
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = alignment
            };
        }

        props.Converter.AddToBlockContainer(imageBlock);
        props.ParseChildren = false;
    }

    /// <summary>
    /// Handle media link inline elements
    /// </summary>
    /// <param name="props">The element properties</param>
    private static void MediaLinkInlineElement(ElementProperties props)
    {
        InlineUIContainer inlineImage = new();
        XElement imageElement = props.Element.Descendants(ddue + "image").FirstOrDefault();
        XAttribute href;
        Image image = new();
        KeyValuePair<string, string> imageInfo = new(null, null);
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
            var bm = new BitmapImage();

            // Cache on load to prevent it locking the image and ignore the cache so that changes to the
            // image are reflected in the topic when reloaded.
            bm.BeginInit();
            bm.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bm.CacheOption = BitmapCacheOption.OnLoad;
            bm.UriSource = new Uri(imageInfo.Key);
            bm.EndInit();

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
        {
            inlineImage.Child = new TextBlock
            {
                Text = String.Format(CultureInfo.InvariantCulture, "[INVALID IMAGE ID: {0}]", id),
                Background = new SolidColorBrush(Colors.Red),
                Foreground = new SolidColorBrush(Colors.White)
            };
        }

        props.Converter.AddInlineToContainer(inlineImage);

        props.ReturnToParent = props.ParseChildren = false;
    }

    /// <summary>
    /// Handle HTML image elements
    /// </summary>
    /// <param name="props">The element properties</param>
    private static void ImageElement(ElementProperties props)
    {
        UIElement child;
        string id = props.Element.Attribute("src")?.Value ?? props.Element.Attribute("target")?.Value ?? "???",
            altText = props.Element.Attribute("alt")?.Value ?? props.Element.Attribute("altText")?.Value;
        KeyValuePair<string, string> imageInfo = new(null, null);
        HorizontalAlignment alignment = HorizontalAlignment.Left;
        var attribute = props.Element.Parent?.Parent?.Attribute("placement")?.Value ??
            props.Element.Parent?.Parent?.Element(ddue + "figcaption")?.Attribute("placement")?.Value;

        switch(attribute)
        {
            case "center":
                alignment = HorizontalAlignment.Center;
                break;

            case "far":
                alignment = HorizontalAlignment.Right;
                break;

            default:
                break;
        }

        if(id.Length > 0 && (id[0] == '@' || props.Element.Name.LocalName == "artLink"))
        {
            if(id[0] == '@')
                id = id.Substring(1);

            if(!props.Converter.MediaFiles.TryGetValue(id, out imageInfo))
                imageInfo = new KeyValuePair<string, string>(null, null);
        }

        if(!String.IsNullOrEmpty(imageInfo.Key) && File.Exists(imageInfo.Key))
        {
            var bm = new BitmapImage();

            // Cache on load to prevent it locking the image and ignore the cache so that changes to the
            // image are reflected in the topic when reloaded.
            bm.BeginInit();
            bm.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bm.CacheOption = BitmapCacheOption.OnLoad;
            bm.UriSource = new Uri(imageInfo.Key);
            bm.EndInit();

            // Use the actual image size to mimic the HTML layout
            child = new Image()
            {
                Height = bm.Height,
                Width = bm.Width,
                ToolTip = !String.IsNullOrEmpty(altText) ? altText :
                    !String.IsNullOrEmpty(imageInfo.Value) ? imageInfo.Value :
                        "ID: " + id + "\nFilename: " + imageInfo.Key,
                Margin = new Thickness(5, 0, 5, 0),
                HorizontalAlignment = alignment,
                Source = bm
            };
        }
        else
        {
            child = new TextBlock
            {
                Text = String.Format(CultureInfo.InvariantCulture, "[INVALID IMAGE ID: {0}]", id),
                Background = new SolidColorBrush(Colors.Red),
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = alignment
            };
        }

        if(props.Converter.CurrentBlockElement is Paragraph)
        {
            InlineUIContainer imageInline = new() { Child = child };
            props.Converter.AddInlineToContainer(imageInline);
            props.ReturnToParent = false;
        }
        else
        {
            BlockUIContainer imageBlock = new() { Child = child };
            props.Converter.AddToBlockContainer(imageBlock);
            props.ParseChildren = false;
            props.ReturnToParent = true;
        }
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
            Paragraph p = new(new Run(reCondenseWhitespace.Replace(title.Value.Trim(), " ")));
            props.Converter.AddNonParentToBlockContainer(p);

            p.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.TableTitle);
        }

        props.Converter.AddToBlockContainer(new Table());
    }

    /// <summary>
    /// Handle table header elements
    /// </summary>
    /// <param name="props">The element properties</param>
    private static void TableHeaderElement(ElementProperties props)
    {
        TableRowGroup g = new();
        props.Converter.AddToBlockContainer(g);
        g.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.TableHeaderRow);
    }

    /// <summary>
    /// Handle table row elements
    /// </summary>
    /// <param name="props">The element properties</param>
    private static void RowElement(ElementProperties props)
    {
        // If not already in a table row group, add the group first
        if(props.Converter.CurrentBlockElement is not TableRowGroup)
        {
            TableRowGroup group = new();

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
        Span s = new();
        props.Converter.AddInlineToContainer(s);
        s.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.CodeInline);
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
    /// Handle line break elements
    /// </summary>
    /// <param name="props">The element properties</param>
    /// <remarks>This inserts a line break element</remarks>
    private static void LineBreakElement(ElementProperties props)
    {
        props.Converter.AddInlineToContainer(new LineBreak());
        props.ParseChildren = props.ReturnToParent = false;
    }

    /// <summary>
    /// Handle literal elements
    /// </summary>
    /// <param name="props">The element properties</param>
    private static void LiteralElement(ElementProperties props)
    {
        Span s = new();
        props.Converter.AddInlineToContainer(s);
        s.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.Literal);
    }

    /// <summary>
    /// Handle math elements
    /// </summary>
    /// <param name="props">The element properties</param>
    private static void MathElement(ElementProperties props)
    {
        Italic i = new();
        props.Converter.AddInlineToContainer(i);
        i.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.Math);
    }

    /// <summary>
    /// Handle elements with generic strikethrough styling
    /// </summary>
    /// <param name="props">The element properties</param>
    private static void StrikethroughElement(ElementProperties props)
    {
        Span s = new(new Run(reCondenseWhitespace.Replace(props.Element.Value.Trim(), " ")))
        {
            TextDecorations = TextDecorations.Strikethrough
        };

        props.Converter.AddInlineToContainer(new Span(s));
        props.ParseChildren = false;
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
        Span s = new(new Run(reCondenseWhitespace.Replace(props.Element.Value.Trim(), " ")))
        {
            BaselineAlignment = BaselineAlignment.Subscript
        };

        s.FontSize *= 0.75;

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
        Span s = new(new Run(reCondenseWhitespace.Replace(props.Element.Value.Trim(), " ")))
        {
            BaselineAlignment = BaselineAlignment.Superscript
        };

        s.FontSize *= 0.75;

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
    /// Handle HTML anchor link elements which may be a code entity reference, a topic link, or an external link
    /// </summary>
    /// <param name="props">The element properties</param>
    /// <remarks>Anchor links in Markdown content may actually be one of the other link types so we have to
    /// handle all of them here based on the <c>href</c> value.</remarks>
    private static void AnchorLinkElement(ElementProperties props)
    {
        Hyperlink l;
        string href = props.Element.Attribute("href")?.Value ?? "MISSING_LINK_HREF",
            title = props.Element.Attribute("title")?.Value, target = props.Element.Attribute("target")?.Value,
            linkText = reCondenseWhitespace.Replace(props.Element.Value?.Trim() ?? String.Empty, " ");

        if(href[0] == '@' || href[0] == '#')
        {
            if(href[0] == '@')
                href = href.Substring(1);

            if(href.Length > 2 && href[1] == ':')
            {
                if(String.IsNullOrWhiteSpace(linkText))
                {
                    href = WebUtility.UrlDecode(href);

                    // Code entity reference
                    bool qualifyHint = (bool?)props.Element.Attribute("show-container") ?? false;

                    // Remove parameters from the ID.  We'll ignore the show-parameters option for previewing as we
                    // can't convert them to their more human readable form.
                    int pos = href.IndexOf('(');
                    char prefix = 'N';

                    if(pos != -1)
                        href = href.Substring(0, pos);

                    // Remove the type prefix
                    if(href.Length > 2 && href[1] == ':')
                    {
                        prefix = href[0];
                        href = href.Substring(2);
                    }

                    string[] parts = href.Split(['.'], StringSplitOptions.RemoveEmptyEntries);

                    // If qualified, add the appropriate parts
                    if(qualifyHint)
                    {
                        if(prefix != 'N' && prefix != 'T')
                            href = String.Join(".", parts, parts.Length - 2, 2);
                    }
                    else
                        href = parts[parts.Length - 1];

                    if(parts.Length > 2)
                    {
                        href = href.Replace("#ctor", parts[parts.Length - 2]).Replace("#cctor",
                            parts[parts.Length - 2]);
                    }
                }
                else
                    href = linkText;

                props.Converter.AddInlineToContainer(new Bold(new Run(href)));
                props.ParseChildren = false;
            }
            else
            {
                // Topic link
                props.Converter.AddInlineToContainer(new Hyperlink { NavigateUri = new Uri("link://" + href) });

                // Add the topic title as the link text if there is no inner text
                if(!props.ParseChildren)
                {
                    int pos = href.IndexOf('#');

                    if(pos != -1)
                        href = href.Substring(0, pos);

                    if(!props.Converter.TopicTitles.TryGetValue(href, out title))
                        title = "[UNKNOWN TOPIC ID: " + href + "]";

                    props.Converter.AddInlineToContainer(new Run(title));
                }
                else
                {
                    props.Converter.AddInlineToContainer(new Run(reCondenseWhitespace.Replace(
                        props.Element.Value.Trim(), " ")));
                    props.ParseChildren = false;
                }
            }
        }
        else
        {
            // External link
            try
            {
                l = new Hyperlink { NavigateUri = new Uri(href, UriKind.RelativeOrAbsolute) };
            }
            catch(UriFormatException)
            {
                l = new Hyperlink
                {
                    NavigateUri = new Uri("none://UNABLE_TO_CONVERT_URL_TO_URI",
                    UriKind.RelativeOrAbsolute)
                };
            }

            l.ToolTip = title;
            l.TargetName = target ?? "_blank";

            if(!props.Element.HasElements && !String.IsNullOrWhiteSpace(linkText))
            {
                l.Inlines.Add(new Run(String.IsNullOrWhiteSpace(linkText) ? href : linkText));
                props.ParseChildren = false;
            }

            props.Converter.AddInlineToContainer(l);
        }
    }

    /// <summary>
    /// Handle code entity reference elements
    /// </summary>
    /// <param name="props">The element properties</param>
    private static void CodeEntityReferenceElement(ElementProperties props)
    {
        string linkText, memberId = (props.Element.Value ?? String.Empty).Trim();
        string[] parts;
        char prefix = 'N';
        int pos;

        linkText = (string)props.Element.Attribute("linkText");

        if(String.IsNullOrWhiteSpace(linkText))
        {
            bool qualifyHint = (bool?)props.Element.Attribute("qualifyHint") ?? false;

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

            parts = memberId.Split(['.'], StringSplitOptions.RemoveEmptyEntries);

            // If qualified, add the appropriate parts
            if(qualifyHint)
            {
                if(prefix != 'N' && prefix != 'T')
                    memberId = String.Join(".", parts, parts.Length - 2, 2);
            }
            else
                memberId = parts[parts.Length - 1];

            if(parts.Length > 2)
            {
                memberId = memberId.Replace("#ctor", parts[parts.Length - 2]).Replace("#cctor",
                    parts[parts.Length - 2]);
            }
        }
        else
            memberId = linkText;

        props.Converter.AddInlineToContainer(new Bold(new Run(memberId)));
        props.ParseChildren = false;
    }

    /// <summary>
    /// Handle external link elements
    /// </summary>
    /// <param name="props">The element properties</param>
    private static void ExternalLinkElement(ElementProperties props)
    {
        Hyperlink l;
        string linkText = props.Element.Element(ddue + "linkText")?.Value,
            linkAltText = props.Element.Element(ddue + "linkAlternateText")?.Value,
            linkUri = props.Element.Element(ddue + "linkUri")?.Value ?? "none://MISSING_LINKURI_ELEMENT",
            linkTarget = props.Element.Element(ddue + "linkTarget")?.Value;

        if(linkText != null)
            linkText = reCondenseWhitespace.Replace(linkText.Trim(), " ");
        else
            linkText = linkUri;

        try
        {
            l = new Hyperlink { NavigateUri = new Uri(linkUri, UriKind.RelativeOrAbsolute) };
        }
        catch(UriFormatException)
        {
            l = new Hyperlink
            {
                NavigateUri = new Uri("none://UNABLE_TO_CONVERT_URL_TO_URI",
                UriKind.RelativeOrAbsolute)
            };
        }

        l.ToolTip = linkAltText;
        l.TargetName = linkTarget;
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
        string id = props.Element.Attribute(xlink + "href")?.Value ?? "MISSING_LINK_HREF";

        props.Converter.AddInlineToContainer(new Hyperlink { NavigateUri = new Uri("link://" + id) });

        // Add the topic title as the link text if there is no inner text
        if(!props.ParseChildren)
        {
            int pos = id.IndexOf('#');

            if(pos != -1)
                id = id.Substring(0, pos);

            if(!props.Converter.TopicTitles.TryGetValue(id, out string title))
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

    #region Miscellaneous element handlers
    //=====================================================================

    /// <summary>
    /// Handle auto-outline elements
    /// </summary>
    /// <param name="props">The element properties</param>
    private static void AutoOutlineElement(ElementProperties props)
    {
        XElement parent = props.Element.Parent;
        bool isInIntro = parent.Name.LocalName == "introduction";
        string leadText = (props.Element.Attribute("lead")?.Value ?? String.Empty).Trim();

        if(leadText == "none")
            leadText = null;

        if(leadText != null && leadText.Length == 0)
        {
            if(isInIntro)
                leadText = "This topic contains the following sections.";
            else
                leadText = "This section contains the following subsections.";
        }

        if(!Boolean.TryParse(props.Element.Attribute("excludeRelatedTopics")?.Value, out bool excludeRelatedTopics))
            excludeRelatedTopics = false;

        if(String.IsNullOrEmpty(props.Element.Value) || !Int32.TryParse(props.Element.Value, out int maxDepth))
            maxDepth = 0;

        // If there are title elements with a level, it's a Markdown topic so we have to build it differently
        bool isMarkdown = props.Element.Document.Descendants(ddue + "title").Any(
            t => t.Attribute("level") != null);

        if(!isMarkdown)
        {
            if(isInIntro)
            {
                parent = parent.Parent;

                if(!excludeRelatedTopics)
                {
                    var relatedTopics = parent.Element(ddue + "relatedTopics");

                    if(relatedTopics == null || !relatedTopics.HasElements)
                        excludeRelatedTopics = true;
                }
            }
            else
            {
                parent = parent.Parent.Elements(ddue + "sections").FirstOrDefault();
                excludeRelatedTopics = true;
            }
        }

        Section s = new();
        props.Converter.AddNonParentToBlockContainer(s);

        if(leadText != null)
        {
            Paragraph p = new();
            p.Inlines.Add(new Run(leadText));
            s.Blocks.Add(p);
        }

        if(parent != null)
        {
            List list;

            if(!isMarkdown)
                list = InsertAutoOutline(parent, 0, maxDepth);
            else
            {
                list = InsertAutoOutlineFromMarkdown(props.Element.Document, isInIntro ? null : parent.Parent,
                    maxDepth, excludeRelatedTopics);
            }

            if(list != null)
            {
                // If not excluding related topics, add a link to the See Also section
                if(!isMarkdown && !excludeRelatedTopics)
                {
                    list.ListItems.Add(new ListItem(new Paragraph(new Hyperlink(new Run("See Also"))
                    {
                        NavigateUri = new Uri("link://#seeAlsoSection")
                    })));
                }

                s.Blocks.Add(list);
            }
        }

        props.ParseChildren = props.ReturnToParent = false;
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
        List list = new();

        foreach(var section in parent.Elements(ddue + "section"))
        {
            var titleElement = section.Element(ddue + "title");

            if(titleElement == null || titleElement.Value.Trim().Length == 0)
                continue;

            string title = reCondenseWhitespace.Replace(titleElement.Value, " ").Trim();

            var address = section.Attribute("address");

            var listItem = new ListItem();
            list.ListItems.Add(listItem);

            var p = new Paragraph();
            listItem.Blocks.Add(p);

            if(address == null)
                p.Inlines.Add(new Run(title));
            else
            {
                p.Inlines.Add(new Hyperlink(new Run(title))
                {
                    NavigateUri = new Uri("link://#" + address.Value.Trim())
                });
            }

            if(depth < maxDepth)
            {
                foreach(var subsection in section.Elements(ddue + "sections"))
                {
                    var subList = InsertAutoOutline(subsection, depth + 1, maxDepth);

                    if(subList != null)
                        listItem.Blocks.Add(subList);
                }
            }
        }

        return (list.ListItems.Count != 0) ? list : null;
    }

    /// <summary>
    /// This is used to add auto-outline links from a Markdown topic
    /// </summary>
    /// <param name="document">The document from which to get the sections</param>
    /// <param name="start">Null for a top-level outline, or the starting section for a sub-section outline</param>
    /// <param name="maxDepth">The maximum depth of nested sections</param>
    /// <param name="excludeRelatedTopics">True to exclude the related topics link, false to include it</param>
    /// <returns>A list element containing the outline or null if no valid sections where found</returns>
    private static List InsertAutoOutlineFromMarkdown(XDocument document, XElement start, int maxDepth,
      bool excludeRelatedTopics)
    {
        List list = new();
        int idx, level;

        // Get all title elements with level attributes from the section
        var titleElements = document.DescendantNodes().OfType<XElement>().Where(
            e => e.Name.LocalName == "title" && e.Attribute("level") != null).ToList();

        // If we have starting element, it's a sub-section outline for that element so remove it and everything
        // before it and all elements after it that aren't at a higher level.
        if(start != null)
        {
            while(titleElements.Count != 0 && titleElements[0].Parent != start)
                titleElements.RemoveAt(0);

            idx = 0;
            level = (int?)start.Attribute("level") ?? 2;

            if(titleElements.Count != 0)
                titleElements.RemoveAt(0);

            while(idx < titleElements.Count &&
              ((int?)titleElements[idx].Attribute("level") ?? 2) > level)
            {
                idx++;
            }

            while(idx < titleElements.Count)
                titleElements.RemoveAt(idx);
        }
        else
        {
            var seeAlsoSection = titleElements.FirstOrDefault(
                t => t.Parent?.Attribute("id")?.Value == "see-also");

            if(seeAlsoSection != null)
            {
                idx = titleElements.IndexOf(seeAlsoSection);
                level = (int?)seeAlsoSection.Attribute("level") ?? 2;

                // If not removing the See Also section, just exclude any child elements after it
                if(!excludeRelatedTopics)
                    idx++;
                else
                    titleElements.RemoveAt(idx);

                while(idx < titleElements.Count &&
                  ((int?)titleElements[idx].Attribute("level") ?? 2) > level)
                {
                    titleElements.RemoveAt(idx);
                }
            }
        }

        if(titleElements.Count == 0)
            return null;

        // Heading levels can vary so we'll track depth independent of heading level
        int baseDepth = 0;

        AddMarkdownTitlesToList(list, titleElements, 0, 0, baseDepth, maxDepth);

        return (list.ListItems.Count != 0) ? list : null;
    }

    /// <summary>
    /// Recursively add title elements to the list based on their level
    /// </summary>
    /// <param name="currentList">The current list to add items to</param>
    /// <param name="titleElements">All title elements</param>
    /// <param name="startIndex">The starting index in the title elements list</param>
    /// <param name="currentDepth">The current depth being processed</param>
    /// <param name="currentLevel">The current heading level</param>
    /// <param name="maxDepth">The maximum depth to process</param>
    /// <returns>The index of the next unprocessed title element</returns>
    private static int AddMarkdownTitlesToList(List currentList, List<XElement> titleElements,
        int startIndex, int currentLevel, int currentDepth, int maxDepth)
    {
        int i = startIndex;

        while(i < titleElements.Count)
        {
            var titleElement = titleElements[i];
            int level = (int?)titleElement.Attribute("level") ?? 2;

            // If this title is at a lower level, return to parent
            if(level < currentLevel)
                return i;

            string title = reCondenseWhitespace.Replace(titleElement.Value, " ").Trim();

            if(String.IsNullOrEmpty(title))
            {
                i++;
                continue;
            }

            var address = titleElement.Parent?.Attribute("id");
            var listItem = new ListItem();
            currentList.ListItems.Add(listItem);

            var p = new Paragraph();
            listItem.Blocks.Add(p);

            if(address == null)
                p.Inlines.Add(new Run(title));
            else
            {
                p.Inlines.Add(new Hyperlink(new Run(title))
                {
                    NavigateUri = new Uri("link://#" + address.Value.Trim())
                });
            }

            i++;

            // Check if the next title is at a higher level (nested)
            if(i < titleElements.Count)
            {
                int nextLevel = (int?)titleElements[i].Attribute("level") ?? 2;

                if(nextLevel > level)
                {
                    if(currentDepth + 1 < maxDepth)
                    {
                        // Create nested list for higher level titles
                        List nestedList = new();

                        i = AddMarkdownTitlesToList(nestedList, titleElements, i, nextLevel, currentDepth + 1, maxDepth);

                        listItem.Blocks.Add(nestedList);
                    }
                    else
                    {
                        // Skip titles at a higher level
                        while(i < titleElements.Count)
                        {
                            nextLevel = (int?)titleElements[i].Attribute("level") ?? 2;
                            
                            if(nextLevel <= level)
                                break;

                            i++;
                        }
                    }
                }
            }
        }

        return i;
    }

    /// <summary>
    /// Handle ignored elements
    /// </summary>
    /// <param name="props">The element properties</param>
    /// <remarks>For ignored elements, we don't return to the parent element after parsing
    /// this one's children as there may be other sibling elements after it.</remarks>
    private static void IgnoredElement(ElementProperties props)
    {
        props.ReturnToParent = false;
    }

    /// <summary>
    /// Handle ignored elements with ignored child elements
    /// </summary>
    /// <param name="props">The element properties</param>
    /// <remarks>This ignores all child elements too.  For ignored elements, we don't return to the parent
    /// element as there may be other sibling elements after it.</remarks>
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
        if(props.Converter.CurrentSpanElement is Span span)
        {
            Run r = new()
            {
                Text = reCondenseWhitespace.Replace(content, " "),
                Background = new SolidColorBrush(Color.FromRgb(0xEF, 0xEF, 0xF7))
            };

            span.Inlines.Add(r);
            props.ReturnToParent = false;
        }
        else
        {
            if(props.Converter.CurrentBlockElement is Paragraph p)
            {
                Run r = new()
                {
                    Text = reCondenseWhitespace.Replace(content, " "),
                    Background = new SolidColorBrush(Color.FromRgb(0xEF, 0xEF, 0xF7))
                };

                p.Inlines.Add(r);
                props.ReturnToParent = false;
            }
            else
            {
                Section s = new();
                props.Converter.AddToBlockContainer(s);

                p = new Paragraph();
                s.Blocks.Add(p);
                p.Inlines.Add(new Run(StripLeadingWhitespace(content, 4)));

                s.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.CodeBlock);
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
        if(props.Converter.Tokens.TryGetValue(props.Element.Value.Trim(), out XElement token))
        {
            props.Converter.ParseChildren(token.Nodes());

            // Add elements after the token to the current parent element
            props.ReturnToParent = false;
        }
        else
        {
            props.Converter.AddInlineToContainer(new Bold(new Run(String.Format(CultureInfo.InvariantCulture,
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
        Section glossary = new();
        Paragraph p = null;
        XElement titleElement;
        XAttribute addressAttr;
        List<XElement> divisions;
        Dictionary<XElement, (string Id, string Title)> divisionIds = [];
        List<GlossaryEntry> entries = [];
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

            p.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.Heading2);
        }

        // See if there are divisions.  If so, add one section for each division.  If not, lump all entries
        // into one untitled division.
        divisions = [.. props.Element.Descendants(ddue + "glossaryDiv")];

        // If there are multiple divisions, add a link to each one provided we have an address and a title
        if(divisions.Count == 0)
        {
            divisions = [props.Element];
            divisionIds.Add(props.Element, (null, null));
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

                divisionIds.Add(d, (id, title));

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
        entries.AddRange(props.Element.Descendants(ddue + "glossaryEntry").Select(g => new GlossaryEntry(g)));

        // Render each division
        foreach(var d in divisions)
        {
            var titleAndId = divisionIds[d];

            // Add a title if there is one
            if(!String.IsNullOrEmpty(titleAndId.Title))
            {
                id = titleAndId.Id;
                p = new Paragraph(new Run(titleAndId.Title)) { Name = id };
                glossary.Blocks.Add(p);
                p.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.GlossaryDivisionTitle);
            }
            else
            {
                id = "__GlossaryDiv" + autoId.ToString(CultureInfo.InvariantCulture);
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
        Paragraph p = new();
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
            {
                p.Inlines.Add(new Hyperlink(new Run(ch.ToString()))
                {
                    NavigateUri = new Uri("link://#" + divisionId + "_" + ch.ToString())
                });
            }
        }

        converter.AddNonParentToBlockContainer(p);
        p.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.GlossaryLetterBar);

        foreach(var g in groupLetters)
        {
            p = new Paragraph(new Run(g.Key.ToString())
            {
                Name = ToElementName(divisionId + "_" + g.Key.ToString())
            });

            converter.AddNonParentToBlockContainer(p);
            p.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.GlossaryLetterTitle);

            foreach(var entry in g)
            {
                s = new Section();

                converter.AddNonParentToBlockContainer(s);
                s.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.GlossaryDefinition);

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
                p.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.NoMargin);

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

                        var related = entries.SelectMany(e => e.Terms).FirstOrDefault(t => t.Value == r);

                        if(related.Key != null)
                        {
                            p.Inlines.Add(new Hyperlink(new Run(related.Key))
                            {
                                NavigateUri = new Uri("link://#" + r)
                            });
                        }
                    }

                    p.Inlines.Add(new LineBreak());
                    s.Blocks.Add(p);
                }
            }
        }
    }
    #endregion
}
