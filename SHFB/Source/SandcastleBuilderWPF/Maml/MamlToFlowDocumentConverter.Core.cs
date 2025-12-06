//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : MamlToFlowDocumentConverter.Core.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/03/2025
// Note    : Copyright 2012-2025, Eric Woodruff, All rights reserved
//
// This file contains the core methods of the class used to convert a MAML topic file to a flow document so that
// it can be previewed in the standalone GUI and Visual Studio.
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

// Ignore Spelling: Grande Semibold

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml.Linq;

using ColorizerLibrary;

using Sandcastle.Core.Markdown;

namespace SandcastleBuilder.WPF.Maml;

/// <summary>
/// This class is used to convert a MAML topic file to a flow document
/// </summary>
public partial class MamlToFlowDocumentConverter
{
    #region Private data members
    //=====================================================================

    private readonly Stack<TextElement> parentBlocks;
    private readonly Stack<Span> parentSpans;
    private FlowDocument document;
    private TextElement currentBlock;
    private Span currentSpan;
    private readonly MarkdownToMamlConverter markdownToMaml;

    private static string flowDocumentTemplate, flowDocumentContent;

    #endregion

    #region Properties
    //=====================================================================

    /// <summary>
    /// This read-only property returns the dictionary used to map image IDs to media files
    /// for media link elements.
    /// </summary>
    /// <value>The key is the image ID, the value is the key/value pair where the key is the filename
    /// and the value is the optional alternate text to use as the image tool tip.</value>
    public Dictionary<string, KeyValuePair<string, string>> MediaFiles { get; }

    /// <summary>
    /// This read-only property returns the dictionary used to stored token values that can be
    /// resolved to content that is inserted into the topics.
    /// </summary>
    /// <value>The key is the token name, the value is the token content as an XML fragment.  The
    /// children of the root node of the fragment are inserted into the document when the token is
    /// encountered.</value>
    public Dictionary<string, XElement> Tokens { get; }

    /// <summary>
    /// This read-only property returns the dictionary used to map topic IDs to display titles
    /// for hyperlinks.
    /// </summary>
    /// <value>The key is the topic ID, the value is the display title</value>
    public Dictionary<string, string> TopicTitles { get; }

    /// <summary>
    /// This is used to get or set a code colorizer instance used to colorize code blocks
    /// </summary>
    /// <value>If null, code blocks will not be colorized</value>
    public static CodeColorizer CodeColorizer { get; set; }

    /// <summary>
    /// This is used to set the flow document template used when colorizing code blocks
    /// </summary>
    public static string ColorizerFlowDocumentTemplate
    {
        get => flowDocumentTemplate;
        set
        {
            flowDocumentTemplate = value;

            if(File.Exists(flowDocumentTemplate))
                flowDocumentContent = File.ReadAllText(flowDocumentTemplate);
        }
    }

    /// <summary>
    /// This is used to get or set the base path for imported code blocks
    /// </summary>
    public static string ImportedCodeBasePath { get; set; }

    /// <summary>
    /// This read-only property returns the flow document being created
    /// </summary>
    private FlowDocument Document => document;

    /// <summary>
    /// This returns the current block element to which new elements will be added
    /// </summary>
    /// <value>This returns null if there is no current element.  Setting this value will not push the
    /// current value on to the stack of parent elements.</value>
    private TextElement CurrentBlockElement
    {
        get => currentBlock;
        set => currentBlock = value;
    }

    /// <summary>
    /// This returns the current span element to which new elements will be added
    /// </summary>
    /// <value>This returns null if there is no current element</value>
    private Inline CurrentSpanElement => currentSpan;

    #endregion

    #region Constructor
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    public MamlToFlowDocumentConverter(IEnumerable<string> blockTags, IEnumerable<string> doNotParseTags)
    {
        this.Tokens = new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
        this.TopicTitles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        this.MediaFiles = new Dictionary<string, KeyValuePair<string, string>>(StringComparer.OrdinalIgnoreCase);

        parentBlocks = new Stack<TextElement>();
        parentSpans = new Stack<Span>();

        markdownToMaml = new MarkdownToMamlConverter(true, blockTags, doNotParseTags);
        markdownToMaml.SetUpPipeline();
    }
    #endregion

    #region Conversion methods
    //=====================================================================

    /// <summary>
    /// Convert the specified MAML file to a flow document
    /// </summary>
    /// <param name="id">The topic ID</param>
    /// <param name="filename">The name of the MAML file to convert</param>
    /// <param name="content">The content to convert or null or an empty string to load it from the
    /// named file</param>
    /// <returns>The converted flow document</returns>
    public FlowDocument ToFlowDocument(string id, string filename, string content)
    {
        XDocument mamlDoc;

        try
        {
            document = CreateFlowDocument();

            parentBlocks.Clear();
            parentSpans.Clear();
            currentBlock = null;
            currentSpan = null;

            if((!String.IsNullOrEmpty(filename) && File.Exists(filename)) || !String.IsNullOrEmpty(content))
            {
                if(Path.GetExtension(filename).Equals(".md", StringComparison.OrdinalIgnoreCase))
                {
                    if(String.IsNullOrEmpty(content))
                        mamlDoc = XDocument.Parse(markdownToMaml.ConvertFromFile(id, filename));
                    else
                        mamlDoc = XDocument.Parse(markdownToMaml.ConvertFromMarkdown(id, content));
                }
                else
                {
                    if(String.IsNullOrEmpty(content))
                        mamlDoc = XDocument.Load(filename, LoadOptions.SetLineInfo);
                    else
                        mamlDoc = XDocument.Parse(content, LoadOptions.SetLineInfo);
                }

                if(IsMamlDocument(mamlDoc))
                {
                    this.ParseChildren(mamlDoc.Nodes());

                    // Have a go at auto-sizing the table columns
                    foreach(var t in document.Blocks.SelectMany(b => b.Flatten().OfType<Table>()))
                        t.AutoSizeTableColumns();
                }
                else
                    document.Blocks.Add(new Section(new Paragraph(new Run("Not a MAML document: " +
                        filename))));
            }
            else
                document.Blocks.Add(new Section(new Paragraph(new Run("Empty container node."))));
        }
        catch(Exception ex)
        {
            // It should exist, but just in case...
            document ??= new FlowDocument();

            var rootSection = new Section();

            if(document.Blocks.Count == 0)
                document.Blocks.Add(rootSection);
            else
                document.Blocks.InsertBefore(document.Blocks.FirstBlock, rootSection);

            Style errorStyle = new(typeof(Section));
            errorStyle.Setters.Add(new Setter(TextElement.FontFamilyProperty,
                new FontFamily("Segoe UI, Lucida Grande, Verdana")));
            errorStyle.Setters.Add(new Setter(TextElement.FontSizeProperty, 12.0));
            errorStyle.Setters.Add(new Setter(Block.PaddingProperty, new Thickness(3)));
            errorStyle.Setters.Add(new Setter(Block.BorderThicknessProperty, new Thickness(1)));
            errorStyle.Setters.Add(new Setter(Block.BorderBrushProperty, new SolidColorBrush(Colors.Red)));

            rootSection.Resources.Add(typeof(Section), errorStyle);
            rootSection.Resources.Add(typeof(Paragraph), document.Resources[typeof(Paragraph)]);
            rootSection.Resources.Add(NamedStyle.Definition, document.Resources[NamedStyle.Definition]);
            rootSection.Resources.Add(NamedStyle.NoTopMargin, document.Resources[NamedStyle.NoTopMargin]);

            var parentSection = new Section();

            parentSection.Blocks.Add(new Paragraph(new Run("Filename: " + filename)));
            parentSection.Blocks.Add(new Paragraph(new Run("Unable to convert topic file.  Reason(s):")));
            rootSection.Blocks.Add(parentSection);

            var section = new Section();

            parentSection.Blocks.Add(section);
            section.SetResourceReference(FrameworkContentElement.StyleProperty, NamedStyle.Definition);

            section.Blocks.Add(new Paragraph(new Run(ex.Message)));

            while(ex.InnerException != null)
            {
                ex = ex.InnerException;
                section.Blocks.Add(new Paragraph(new Run(ex.Message)));
            }

            parentSection.Blocks.Add(new Paragraph(new Run("Parsed content up to the point of " +
                "failure follows.")));
        }

        return document;
    }

    /// <summary>
    /// This converts a MAML address value to a valid XAML element name
    /// </summary>
    /// <param name="name">The MAML address name value to convert</param>
    /// <returns>A hashed ID value that can be used as a XAML element name</returns>
    /// <remarks>Flow document elements are fussy about the value assigned to their Name property
    /// (it's a XAML thing).  This is used to convert the MAML name value to one that is valid for use
    /// as a flow document element name.</remarks>
    public static string ToElementName(string name)
    {
        if(name == null)
            throw new ArgumentNullException(nameof(name));

        return "_" + name.GetHashCode().ToString("X", CultureInfo.InvariantCulture);
    }
    #endregion

    #region Parsing methods
    //=====================================================================

    /// <summary>
    /// This is used to see if the given XML document is a MAML document
    /// </summary>
    /// <param name="doc">The document to check</param>
    /// <returns>True if it is a recognized MAML document type, false if not</returns>
    private static bool IsMamlDocument(XDocument doc)
    {
        if(doc.Root.Name.LocalName != "topic" || !doc.Root.HasElements)
            return false;

        switch(doc.Root.Elements().First().Name.LocalName)
        {
            case "codeEntityDocument":
            case "developerConceptualDocument":
            case "developerErrorMessageDocument":
            case "developerGlossaryDocument":
            case "developerHowToDocument":
            case "developerOrientationDocument":
            case "developerReferenceWithSyntaxDocument":
            case "developerReferenceWithoutSyntaxDocument":
            case "developerSDKTechnologyOverviewArchitectureDocument":
            case "developerSDKTechnologyOverviewCodeDirectoryDocument":
            case "developerSDKTechnologyOverviewOrientationDocument":
            case "developerSDKTechnologyOverviewScenariosDocument":
            case "developerSDKTechnologyOverviewTechnologySummaryDocument":
            case "developerSampleDocument":
            case "developerTroubleshootingDocument":
            case "developerUIReferenceDocument":
            case "developerWalkthroughDocument":
            case "developerWhitePaperDocument":
            case "developerXmlReference":
                return true;

            default:
                break;
        }

        return false;
    }

    /// <summary>
    /// Recursively parse the document elements to create the flow document
    /// </summary>
    /// <param name="nodes">The nodes in the document</param>
    private void ParseChildren(IEnumerable<XNode> nodes)
    {
        ElementProperties elementProps = new(this);
        XText text;
        string innerText;

        foreach(XNode n in nodes)
        {
            elementProps.Element = n as XElement;

            if(elementProps.Element != null)
            {
                elementProps.ReturnToParent = true;
                elementProps.ParseChildren = elementProps.Element.Nodes().Any();

                // If it's an element we recognize, handle it
                if(elementHandlers.TryGetValue(elementProps.Element.Name.LocalName, out Action<ElementProperties> handler))
                    handler(elementProps);
                else
                {
                    System.Diagnostics.Debug.WriteLine("Unhandled element type: " +
                        elementProps.Element.Name.LocalName);

                    // Take a guess at the type (block or inline).  For blocks, we'll just
                    // create a new section to act as a parent to any child elements.
                    if(currentSpan == null && currentBlock is not Paragraph)
                    {
                        if(elementProps.ParseChildren)
                            SectionElement(elementProps);
                        else
                            elementProps.ReturnToParent = false;
                    }
                    else
                    {
                        // If the inline element has no children, insert its name so that it shows up
                        if(!elementProps.ParseChildren)
                        {
                            this.AddInlineToContainer(new Bold(new Run(String.Format(CultureInfo.InvariantCulture,
                                "[{0}]", elementProps.Element.Name.LocalName))));

                            // We just added a bold run so we'll need to return to the parent
                            elementProps.ReturnToParent = true;
                        }
                        else
                            elementProps.ReturnToParent = false;
                    }
                }

                // Store address/id values in the current document element's name for linking.  The value
                // is converted to an internal ID to make sure it is valid for use by the element.
                if(elementProps.Element.Attribute("address") != null)
                {
                    currentSpan?.Name = ToElementName(elementProps.Element.Attribute("address").Value);
                    currentBlock?.Name = ToElementName(elementProps.Element.Attribute("address").Value);
                }
                else
                {
                    if(elementProps.Element.Attribute("id") != null)
                    {
                        currentSpan?.Name = ToElementName(elementProps.Element.Attribute("id").Value);
                        currentBlock?.Name = ToElementName(elementProps.Element.Attribute("id").Value);
                    }
                }

                // Parse the child elements
                if(elementProps.ParseChildren)
                    this.ParseChildren(elementProps.Element.Nodes());

                // Return to the parent element after parsing the child elements?
                if(elementProps.ReturnToParent)
                {
                    if(currentSpan != null)
                    {
                        if(parentSpans.Count != 0)
                            currentSpan = parentSpans.Pop();
                        else
                            currentSpan = null;
                    }
                    else
                    {
                        if(parentBlocks.Count != 0)
                            currentBlock = parentBlocks.Pop();
                        else
                            currentBlock = null;
                    }
                }
            }
            else
            {
                text = n as XText;

                // Whitespace is condensed into a single space to mimic the behavior of text in
                // HTML documents.  Since we are building the document in memory using the objects,
                // they preserve whitespace in the text runs which we don't want.
                if(text != null)
                {
                    innerText = reCondenseWhitespace.Replace(text.Value, " ");

                    // If this is the first child or it's preceded by a line break, trim the leading
                    // whitespace.  This prevents an extra space showing up at the start of paragraph
                    // inner text when it starts on a new line.
                    var prevNode = text.PreviousNode as XElement;

                    if(text.Parent.FirstNode == text || (prevNode != null && prevNode.Name.LocalName == "lineBreak"))
                        innerText = innerText.TrimStart();

                    this.AddInlineToContainer(new Run(innerText));
                }

                // Ignore everything else (comments, processing instructions, etc.)
            }
        }
    }

    /// <summary>
    /// Recursively parse the document elements and add them to the given parent block
    /// </summary>
    /// <param name="parent">The parent block to which the document elements are added</param>
    /// <param name="nodes">The nodes in the document</param>
    /// <remarks>This temporarily makes the given parent element the current block to which new child
    /// elements are added.</remarks>
    private void ParseChildren(Block parent, IEnumerable<XNode> nodes)
    {
        parentBlocks.Push(currentBlock);
        currentBlock = parent;

        this.ParseChildren(nodes);
        currentBlock = parentBlocks.Pop();
    }
    #endregion

    #region Flow document element creation methods
    //=====================================================================

    /// <summary>
    /// This creates a flow document with the necessary styles
    /// </summary>
    /// <returns></returns>
    private static FlowDocument CreateFlowDocument()
    {
        // We used left justified text to mimic the behavior of the HTML layout
        var document = new FlowDocument
        {
            PagePadding = new Thickness(10, 15, 10, 15),
            TextAlignment = TextAlignment.Left
        };

        // Add the icons used for alert element titles
        document.Resources.Add("AlertCaution", SharedResources.CautionIcon);
        document.Resources.Add("AlertNote", SharedResources.NoteIcon);
        document.Resources.Add("AlertSecurity", SharedResources.SecurityIcon);

        // Create all of the styles.  Currently it will look like the VS2013 style.
        Style noMargin = new(typeof(Paragraph));
        noMargin.Setters.Add(new Setter(Block.MarginProperty, new Thickness(0)));

        Style noTopMargin = new(typeof(Paragraph));
        noTopMargin.Setters.Add(new Setter(Block.MarginProperty, new Thickness(Double.NaN, 0,
            Double.NaN, Double.NaN)));

        Style section = new(typeof(Section));
        section.Setters.Add(new Setter(TextElement.FontFamilyProperty,
            new FontFamily("Segoe UI, Lucida Grande, Verdana")));
        section.Setters.Add(new Setter(TextElement.FontSizeProperty, 12.0));

        Style para = new(typeof(Paragraph));
        para.Setters.Add(new Setter(TextElement.FontSizeProperty, 12.0));
        para.Setters.Add(new Setter(Block.PaddingProperty, new Thickness(0, 0, 0, 10)));
        para.Setters.Add(new Setter(Block.MarginProperty, new Thickness(0)));

        Style codeBlock = new(typeof(Section));
        codeBlock.Setters.Add(new Setter(TextElement.FontFamilyProperty,
            new FontFamily("Consolas, Courier New, Courier")));
        codeBlock.Setters.Add(new Setter(TextElement.FontSizeProperty, 12.0));
        codeBlock.Setters.Add(new Setter(Block.BorderBrushProperty, new SolidColorBrush(Colors.Black)));
        codeBlock.Setters.Add(new Setter(Block.BorderThicknessProperty, new Thickness(0.5)));
        codeBlock.Setters.Add(new Setter(Block.MarginProperty, new Thickness(0, 0, 0, 10)));
        codeBlock.Setters.Add(new Setter(Block.PaddingProperty, new Thickness(5)));
        codeBlock.Resources.Add(typeof(Paragraph), noMargin);

        Style codeInline = new(typeof(Span));
        codeInline.Setters.Add(new Setter(TextElement.FontFamilyProperty,
            new FontFamily("Consolas, Courier New, Courier")));
        codeInline.Setters.Add(new Setter(TextElement.FontSizeProperty, 12.0));
        codeInline.Setters.Add(new Setter(TextElement.ForegroundProperty,
            new SolidColorBrush(Color.FromRgb(0, 0, 0x66))));

        Style definedTerm = new(typeof(Paragraph));
        definedTerm.Setters.Add(new Setter(TextElement.FontWeightProperty, FontWeights.Bold));
        definedTerm.Setters.Add(new Setter(Block.MarginProperty, new Thickness(Double.NaN,
            Double.NaN, Double.NaN, 0)));

        Style definition = new(typeof(Section));
        definition.Setters.Add(new Setter(Block.MarginProperty, new Thickness(15,
            Double.NaN, Double.NaN, Double.NaN)));
        definition.Resources.Add(typeof(Paragraph), noTopMargin);

        Style list = new(typeof(List));
        list.Setters.Add(new Setter(Block.MarginProperty, new Thickness(0, 0, 0, 20)));
        list.Setters.Add(new Setter(Block.PaddingProperty, new Thickness(30, Double.NaN, Double.NaN, Double.NaN)));

        Style bottomMargin = new(typeof(Paragraph));
        bottomMargin.Setters.Add(new Setter(Block.MarginProperty, new Thickness(0, 0, 0, 10)));

        Style listItem = new(typeof(ListItem));
        listItem.Setters.Add(new Setter(ListItem.MarginProperty, new Thickness(10, 10, Double.NaN, Double.NaN)));
        listItem.Resources.Add(typeof(Paragraph), bottomMargin);

        Style heading1 = new(typeof(Paragraph));
        heading1.Setters.Add(new Setter(TextElement.FontFamilyProperty,
            new FontFamily("Segoe UI, Lucida Grande, Verdana")));
        heading1.Setters.Add(new Setter(TextElement.FontWeightProperty, FontWeights.SemiBold));
        heading1.Setters.Add(new Setter(TextElement.FontSizeProperty, 20.0));
        heading1.Setters.Add(new Setter(Block.MarginProperty, new Thickness(Double.NaN, 10,
            Double.NaN, 10)));

        Style heading2 = new(typeof(Paragraph));
        heading2.Setters.Add(new Setter(TextElement.FontFamilyProperty,
            new FontFamily("Segoe UI, Lucida Grande, Verdana")));
        heading2.Setters.Add(new Setter(TextElement.FontWeightProperty, FontWeights.SemiBold));
        heading2.Setters.Add(new Setter(TextElement.FontSizeProperty, 18.0));
        heading2.Setters.Add(new Setter(Block.MarginProperty, new Thickness(Double.NaN, 10,
            Double.NaN, 10)));

        Style heading3 = new(typeof(Paragraph));
        heading3.Setters.Add(new Setter(TextElement.FontFamilyProperty,
            new FontFamily("Segoe UI, Lucida Grande, Verdana")));
        heading3.Setters.Add(new Setter(TextElement.FontWeightProperty, FontWeights.SemiBold));
        heading3.Setters.Add(new Setter(TextElement.FontSizeProperty, 16.0));
        heading3.Setters.Add(new Setter(Block.MarginProperty, new Thickness(Double.NaN, 10,
            Double.NaN, 10)));

        Style heading4 = new(typeof(Paragraph));
        heading4.Setters.Add(new Setter(TextElement.FontFamilyProperty,
            new FontFamily("Segoe UI, Lucida Grande, Verdana")));
        heading4.Setters.Add(new Setter(TextElement.FontWeightProperty, FontWeights.SemiBold));
        heading4.Setters.Add(new Setter(TextElement.FontSizeProperty, 14.0));
        heading4.Setters.Add(new Setter(Block.MarginProperty, new Thickness(Double.NaN, 10,
            Double.NaN, 10)));

        Style heading5 = new(typeof(Paragraph));
        heading5.Setters.Add(new Setter(TextElement.FontFamilyProperty,
            new FontFamily("Segoe UI, Lucida Grande, Verdana")));
        heading5.Setters.Add(new Setter(TextElement.FontWeightProperty, FontWeights.SemiBold));
        heading5.Setters.Add(new Setter(TextElement.FontSizeProperty, 12.0));
        heading5.Setters.Add(new Setter(Block.MarginProperty, new Thickness(Double.NaN, 10,
            Double.NaN, 10)));

        Style heading6 = new(typeof(Paragraph));
        heading6.Setters.Add(new Setter(TextElement.FontFamilyProperty,
            new FontFamily("Segoe UI, Lucida Grande, Verdana")));
        heading6.Setters.Add(new Setter(TextElement.FontWeightProperty, FontWeights.SemiBold));
        heading6.Setters.Add(new Setter(TextElement.FontSizeProperty, 10.0));
        heading6.Setters.Add(new Setter(Block.MarginProperty, new Thickness(Double.NaN, 10,
            Double.NaN, 10)));

        Style alertTitle = new(typeof(Paragraph));
        alertTitle.Setters.Add(new Setter(TextElement.FontWeightProperty, FontWeights.Bold));
        alertTitle.Setters.Add(new Setter(TextElement.BackgroundProperty,
            new SolidColorBrush(Color.FromRgb(0xED, 0xED, 0xED))));
        alertTitle.Setters.Add(new Setter(TextElement.ForegroundProperty,
            new SolidColorBrush(Color.FromRgb(0x63, 0x63, 0x63))));
        alertTitle.Setters.Add(new Setter(Block.MarginProperty, new Thickness(0, 10, 0, 0)));
        alertTitle.Setters.Add(new Setter(Block.PaddingProperty, new Thickness(5)));

        Style alertBody = new(typeof(Section));
        alertBody.Setters.Add(new Setter(Block.BorderBrushProperty,
            new SolidColorBrush(Color.FromRgb(0xDB, 0xDB, 0xDB))));
        alertBody.Setters.Add(new Setter(Block.BorderThicknessProperty, new Thickness(0, 0.5, 0, 0.5)));
        alertBody.Setters.Add(new Setter(Block.MarginProperty, new Thickness(0, 0, 0, 15)));
        alertBody.Setters.Add(new Setter(Block.PaddingProperty, new Thickness(4, 8, 4, 0)));
        alertBody.Resources.Add(typeof(Paragraph), noTopMargin);

        Style tableStyle = new(typeof(Table));
        tableStyle.Setters.Add(new Setter(Block.MarginProperty, new Thickness(0, 5, 0, 5)));

        Style tableCell = new(typeof(TableCell));
        tableCell.Setters.Add(new Setter(TableCell.PaddingProperty, new Thickness(5)));
        tableCell.Setters.Add(new Setter(TableCell.BorderThicknessProperty, new Thickness(0, 0, 0, 0.5)));
        tableCell.Setters.Add(new Setter(TableCell.BorderBrushProperty,
            new SolidColorBrush(Color.FromRgb(0xDB, 0xDB, 0xDB))));

        Style tableHeaderCell = new(typeof(TableCell), tableCell);
        tableHeaderCell.Setters.Add(new Setter(TextElement.BackgroundProperty,
            new SolidColorBrush(Color.FromRgb(0xED, 0xED, 0xED))));

        Style tableHeaderRow = new(typeof(TableRowGroup));
        tableHeaderRow.Setters.Add(new Setter(TextElement.BackgroundProperty,
            new SolidColorBrush(Color.FromRgb(0xED, 0xED, 0xED))));
        tableHeaderRow.Setters.Add(new Setter(TextElement.ForegroundProperty,
            new SolidColorBrush(Color.FromRgb(0x63, 0x63, 0x63))));
        tableHeaderRow.Setters.Add(new Setter(TextElement.FontWeightProperty, FontWeights.Bold));
        tableHeaderRow.Resources.Add(typeof(TableCell), tableHeaderCell);

        Style tableTitle = new(typeof(Paragraph));
        tableTitle.Setters.Add(new Setter(TextElement.FontSizeProperty, 12.0));
        tableTitle.Setters.Add(new Setter(TextElement.FontWeightProperty, FontWeights.Bold));
        tableTitle.Setters.Add(new Setter(TextElement.ForegroundProperty,
            new SolidColorBrush(Color.FromRgb(0, 0x33, 0x99))));
        tableTitle.Setters.Add(new Setter(Block.PaddingProperty, new Thickness(0)));

        Style math = new(typeof(Italic));

        Style literal = new(typeof(Span));
        literal.Setters.Add(new Setter(TextElement.ForegroundProperty,
            new SolidColorBrush(Color.FromRgb(0xCC, 0, 0))));

        Style quote = new(typeof(Paragraph));
        quote.Setters.Add(new Setter(Block.MarginProperty, new Thickness(40, 15, 40, 15)));

        Style blockQuote = new(typeof(Section));
        blockQuote.Setters.Add(new Setter(Block.MarginProperty, new Thickness(40, 15, 40, 15)));

        Style glossaryDivisionTitle = new(typeof(Paragraph), heading1);
        glossaryDivisionTitle.Setters.Add(new Setter(TextElement.FontSizeProperty, 12.0));
        glossaryDivisionTitle.Setters.Add(new Setter(Block.MarginProperty, new Thickness(Double.NaN, 10,
            Double.NaN, 0)));
        glossaryDivisionTitle.Setters.Add(new Setter(Block.BorderThicknessProperty,
            new Thickness(0, 0, 0, 1)));
        glossaryDivisionTitle.Setters.Add(new Setter(Block.BorderBrushProperty,
            new SolidColorBrush(Colors.Black)));

        Style glossaryLetterBar = new(typeof(Paragraph));
        glossaryLetterBar.Setters.Add(new Setter(TextElement.FontSizeProperty, 12.0));

        Style glossaryLetterTitle = new(typeof(Paragraph), heading1);
        glossaryLetterTitle.Setters.Add(new Setter(TextElement.FontSizeProperty, 14.0));
        glossaryLetterTitle.Setters.Add(new Setter(TextElement.ForegroundProperty,
            new SolidColorBrush(Colors.Gray)));

        Style glossaryDefinition = new(typeof(Section), definition);
        glossaryDefinition.Setters.Add(new Setter(Block.MarginProperty, new Thickness(25,
            Double.NaN, Double.NaN, Double.NaN)));

        Style relatedTopicTitle = new(typeof(Paragraph));
        relatedTopicTitle.Setters.Add(new Setter(TextElement.FontFamilyProperty,
            new FontFamily("Segoe UI Semibold, Segoe UI, Lucida Grande, Verdana")));
        relatedTopicTitle.Setters.Add(new Setter(TextElement.FontSizeProperty, 13.0));
        relatedTopicTitle.Setters.Add(new Setter(TextElement.FontWeightProperty, FontWeights.Bold));
        relatedTopicTitle.Setters.Add(new Setter(Block.MarginProperty, new Thickness(Double.NaN, 5,
            Double.NaN, 5)));

        document.Resources.Add(typeof(Section), section);
        document.Resources.Add(typeof(Paragraph), para);
        document.Resources.Add(typeof(List), list);
        document.Resources.Add(typeof(ListItem), listItem);
        document.Resources.Add(typeof(Table), tableStyle);
        document.Resources.Add(typeof(TableCell), tableCell);

        document.Resources.Add(NamedStyle.AlertTitle, alertTitle);
        document.Resources.Add(NamedStyle.AlertBody, alertBody);
        document.Resources.Add(NamedStyle.CodeBlock, codeBlock);
        document.Resources.Add(NamedStyle.CodeInline, codeInline);
        document.Resources.Add(NamedStyle.DefinedTerm, definedTerm);
        document.Resources.Add(NamedStyle.Definition, definition);
        document.Resources.Add(NamedStyle.GlossaryDefinition, glossaryDefinition);
        document.Resources.Add(NamedStyle.GlossaryDivisionTitle, glossaryDivisionTitle);
        document.Resources.Add(NamedStyle.GlossaryLetterBar, glossaryLetterBar);
        document.Resources.Add(NamedStyle.GlossaryLetterTitle, glossaryLetterTitle);
        document.Resources.Add(NamedStyle.Literal, literal);
        document.Resources.Add(NamedStyle.Math, math);
        document.Resources.Add(NamedStyle.NoMargin, noMargin);
        document.Resources.Add(NamedStyle.NoTopMargin, noTopMargin);
        document.Resources.Add(NamedStyle.BlockQuote, blockQuote);
        document.Resources.Add(NamedStyle.Quote, quote);
        document.Resources.Add(NamedStyle.RelatedTopicTitle, relatedTopicTitle);
        document.Resources.Add(NamedStyle.TableHeaderRow, tableHeaderRow);
        document.Resources.Add(NamedStyle.TableTitle, tableTitle);
        document.Resources.Add(NamedStyle.Heading1, heading1);
        document.Resources.Add(NamedStyle.Heading2, heading2);
        document.Resources.Add(NamedStyle.Heading3, heading3);
        document.Resources.Add(NamedStyle.Heading4, heading4);
        document.Resources.Add(NamedStyle.Heading5, heading5);
        document.Resources.Add(NamedStyle.Heading6, heading6);

        return document;
    }

    /// <summary>
    /// This is used to add a block element to the current block container element.  The added
    /// block element becomes the new parent block for subsequent elements.
    /// </summary>
    /// <param name="blockElement">The block element to add</param>
    private void AddToBlockContainer(TextElement blockElement)
    {
        try
        {
            if(currentBlock == null)
            {
                document.Blocks.Add((Block)blockElement);
                currentBlock = blockElement;
                return;
            }

            if(currentBlock is Section section)
            {
                section.Blocks.Add((Block)blockElement);
                parentBlocks.Push(currentBlock);
                currentBlock = blockElement;
                return;
            }

            if(currentBlock is List list)
            {
                list.ListItems.Add((ListItem)blockElement);
                parentBlocks.Push(currentBlock);
                currentBlock = blockElement;
                return;
            }

            if(currentBlock is ListItem listItem)
            {
                listItem.Blocks.Add((Block)blockElement);
                parentBlocks.Push(currentBlock);
                currentBlock = blockElement;
                return;
            }

            if(currentBlock is Table table)
            {
                table.RowGroups.Add((TableRowGroup)blockElement);
                parentBlocks.Push(currentBlock);
                currentBlock = blockElement;
                return;
            }

            if(currentBlock is TableRowGroup tableRowGroup)
            {
                tableRowGroup.Rows.Add((TableRow)blockElement);
                parentBlocks.Push(currentBlock);
                currentBlock = blockElement;
                return;
            }

            if(currentBlock is TableRow tableRow)
            {
                tableRow.Cells.Add((TableCell)blockElement);
                parentBlocks.Push(currentBlock);
                currentBlock = blockElement;
                return;
            }

            if(currentBlock is TableCell tableCell)
            {
                tableCell.Blocks.Add((Block)blockElement);
                parentBlocks.Push(currentBlock);
                currentBlock = blockElement;
                return;
            }
        }
        catch(InvalidCastException ex)
        {
            throw new InvalidOperationException("Unexpected block element type.  Ill-formed document?", ex);
        }

        throw new InvalidOperationException("Unknown parent block element encountered for new block element");
    }

    /// <summary>
    /// This is used to add a block element to the current block container element without
    /// making it the new current parent block.
    /// </summary>
    /// <param name="blockElement">The block element to add</param>
    private void AddNonParentToBlockContainer(TextElement blockElement)
    {
        this.AddToBlockContainer(blockElement);

        if(parentBlocks.Count != 0)
            currentBlock = parentBlocks.Pop();
        else
            currentBlock = null;
    }

    /// <summary>
    /// This is used to add an inline element to the current block or inline container element
    /// </summary>
    /// <param name="inlineElement">The element to add to the current block container</param>
    private void AddInlineToContainer(Inline inlineElement)
    {
        bool addToBlock = true;

        if(currentBlock == null)
            throw new InvalidOperationException("No current block element.  Ill-formed document?");

        var span = inlineElement as Span;

        if(span != null || currentSpan != null)
        {
            if(currentSpan != null)
            {
                currentSpan.Inlines.Add(inlineElement);
                addToBlock = false;
            }

            if(span != null)
            {
                parentSpans.Push(currentSpan);
                currentSpan = span;
            }

            if(!addToBlock)
                return;
        }

        if(currentBlock is Paragraph para)
        {
            para.Inlines.Add(inlineElement);
            return;
        }

        if(currentBlock is ListItem listItem)
        {
            // A list item should have a paragraph.  If we get here, the user didn't add one so we'll
            // do it for them.  This is a common occurrence in older documents where IntelliSense was not
            // available to point out the issue.
            Paragraph p = listItem.Blocks.OfType<Paragraph>().FirstOrDefault();

            if(p == null)
                listItem.Blocks.Add(new Paragraph(inlineElement));
            else
                p.Inlines.Add(inlineElement);

            return;
        }

        if(currentBlock is TableCell tableCell)
        {
            // As above, the same is true of entry elements in table rows
            Paragraph p = tableCell.Blocks.OfType<Paragraph>().FirstOrDefault();

            if(p == null)
                tableCell.Blocks.Add(new Paragraph(inlineElement));
            else
                p.Inlines.Add(inlineElement);

            return;
        }

        // It's probably an ill-formed document.  Add a paragraph to contain the element.  It may not render
        // properly but that's a clue that something needs to be fixed.
        if(currentBlock is Section section)
        {
            Paragraph p = new(inlineElement);
            section.Blocks.Add(p);

            // This one won't go on the stack
            currentBlock = p;
            return;
        }

        throw new InvalidOperationException("Unknown parent block element encountered for new inline element");
    }
    #endregion
}
