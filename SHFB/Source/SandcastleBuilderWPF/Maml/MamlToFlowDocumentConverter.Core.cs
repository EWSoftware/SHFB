//=============================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : MamlToFlowDocumentConverter.Core.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/28/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the core methods of the class used to convert a MAML
// topic file to a flow document so that it can be previewed in the standalone
// GUI and Visual Studio.
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
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace SandcastleBuilder.WPF.Maml
{
    /// <summary>
    /// This class is used to convert a MAML topic file to a flow document
    /// </summary>
    public partial class MamlToFlowDocumentConverter
    {
        #region Private data members
        //=====================================================================

        private Dictionary<string, XElement> tokens;
        private Dictionary<string, string> topicTitles;
        private Dictionary<string, KeyValuePair<string, string>> mediaFiles;
        private Stack<TextElement> parentBlocks;
        private Stack<Span> parentSpans;
        private FlowDocument document;
        private TextElement currentBlock;
        private Span currentSpan;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the dictionary used to map image IDs to media files
        /// for media link elements.
        /// </summary>
        /// <value>The key is the image ID, the value is the key/value pair where the key is the filename
        /// and the value is the optional alternate text to use as the image tooltip.</value>
        public Dictionary<string, KeyValuePair<string, string>> MediaFiles
        {
            get { return mediaFiles; }
        }

        /// <summary>
        /// This read-only property returns the dictionary used to stored token values that can be
        /// resolved to content that is inserted into the topics.
        /// </summary>
        /// <value>The key is the token name, the value is the token content as an XML fragment.  The
        /// children of the root node of the fragment are inserted into the document when the token is
        /// encountered.</value>
        public Dictionary<string, XElement> Tokens
        {
            get { return tokens; }
        }

        /// <summary>
        /// This read-only property returns the dictionary used to map topic IDs to display titles
        /// for hyperlinks.
        /// </summary>
        /// <value>The key is the topic ID, the value is the display title</value>
        public Dictionary<string, string> TopicTitles
        {
            get { return topicTitles; }
        }

        /// <summary>
        /// This is used to map alert classes to their display titles
        /// </summary>
        /// <remarks>The key is the alert class and the value is the display title</remarks>
        public static Dictionary<string, string> AlertTitles
        {
            get { return alertTitles; }
        }

        /// <summary>
        /// This is used to map alert classes to their icons
        /// </summary>
        /// <remarks>The key is the alert class and the value is the icon name</remarks>
        public static Dictionary<string, string> AlertIcons
        {
            get { return alertIcons; }
        }

        /// <summary>
        /// This is used to map named sections to their display titles
        /// </summary>
        /// <remarks>The key is the element name and the value is the display title</remarks>
        public static Dictionary<string, string> NamedSectionTitles
        {
            get { return namedSectionTitles; }
        }

        /// <summary>
        /// This is used to map lanugage IDs to their display titles
        /// </summary>
        /// <remarks>The key is the language ID and the value is the display title</remarks>
        public static Dictionary<string, string> LanguageTitles
        {
            get { return languageTitles; }
        }

        /// <summary>
        /// This read-only property returns the flow document being created
        /// </summary>
        private FlowDocument Document
        {
            get { return document; }
        }

        /// <summary>
        /// This returns the current block element to which new elements will be added
        /// </summary>
        /// <value>This returns null if there is no current element.  Setting this value will not push the
        /// current value on to the stack of parent elements.</value>
        private TextElement CurrentBlockElement
        {
            get { return currentBlock; }
            set { currentBlock = value; }
        }

        /// <summary>
        /// This returns the current span element to which new elements will be added
        /// </summary>
        /// <value>This returns null if there is no current element</value>
        private Inline CurrentSpanElement
        {
            get { return currentSpan; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public MamlToFlowDocumentConverter()
        {
            tokens = new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
            topicTitles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            mediaFiles = new Dictionary<string, KeyValuePair<string, string>>(StringComparer.OrdinalIgnoreCase);
            parentBlocks = new Stack<TextElement>();
            parentSpans = new Stack<Span>();
        }
        #endregion

        #region Conversion methods
        //=====================================================================

        /// <summary>
        /// Convert the specified MAML file to a flow document
        /// </summary>
        /// <param name="filename">The name of the MAML file to convert</param>
        /// <param name="content">The content to convert or null or an empty string to load it from the
        /// named file</param>
        /// <returns>The converted flow document</returns>
        public FlowDocument ToFlowDocument(string filename, string content)
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
                    if(String.IsNullOrEmpty(content))
                        mamlDoc = XDocument.Load(filename, LoadOptions.SetLineInfo);
                    else
                        mamlDoc = XDocument.Parse(content, LoadOptions.SetLineInfo);

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
                if(document == null)
                    document = new FlowDocument();

                var rootSection = new Section();

                if(document.Blocks.Count == 0)
                    document.Blocks.Add(rootSection);
                else
                    document.Blocks.InsertBefore(document.Blocks.FirstBlock, rootSection);

                Style errorStyle = new Style(typeof(Section));
                errorStyle.Setters.Add(new Setter(Section.FontFamilyProperty, new FontFamily("Verdana")));
                errorStyle.Setters.Add(new Setter(Section.FontSizeProperty, 11.0));
                errorStyle.Setters.Add(new Setter(Section.PaddingProperty, new Thickness(3)));
                errorStyle.Setters.Add(new Setter(Section.BorderThicknessProperty, new Thickness(1)));
                errorStyle.Setters.Add(new Setter(Section.BorderBrushProperty,
                    new SolidColorBrush(Colors.Red)));

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
                section.SetResourceReference(Section.StyleProperty, NamedStyle.Definition);

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
            return "_" + name.GetHashCode().ToString("X");
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
            ElementProperties elementProps = new ElementProperties(this);
            Action<ElementProperties> handler;
            XText text;
            string innerText;

            foreach(XNode n in nodes)
            {
                elementProps.Element = n as XElement;

                if(elementProps.Element != null)
                {
                    elementProps.ReturnToParent = true;
                    elementProps.ParseChildren = !elementProps.Element.IsEmpty;

                    // If it's an element we recognize, handle it
                    if(elementHandlers.TryGetValue(elementProps.Element.Name.LocalName, out handler))
                        handler(elementProps);
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Unhandled element type: " +
                            elementProps.Element.Name.LocalName);

                        // Take a guess at the type (block or inline).  For blocks, we'll just
                        // create a new section to act as a parent to any child elements.
                        if(currentSpan == null && !(currentBlock is Paragraph))
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
                                this.AddInlineToContainer(new Bold(new Run(String.Format(
                                    "[{0}]", elementProps.Element.Name.LocalName))));

                                // We just added a bold run so we'll need to return to the parent
                                elementProps.ReturnToParent = true;
                            }
                            else
                                elementProps.ReturnToParent = false;
                        }
                    }

                    // Store address values in the current document element's name for linking.  The value
                    // is converted to an internal ID to make sure it is valid for use by the element.
                    if(elementProps.Element.Attribute("address") != null)
                        if(currentSpan != null)
                            currentSpan.Name = ToElementName(elementProps.Element.Attribute("address").Value);
                        else
                            if(currentBlock != null)
                                currentBlock.Name = ToElementName(elementProps.Element.Attribute("address").Value);

                    // Parse the child elements
                    if(elementProps.ParseChildren)
                        this.ParseChildren(elementProps.Element.Nodes());

                    // Return to the parent element after parsing the child elements?
                    if(elementProps.ReturnToParent)
                        if(currentSpan != null)
                        {
                            if(parentSpans.Count != 0)
                                currentSpan = parentSpans.Pop();
                            else
                                currentSpan = null;
                        }
                        else
                            if(parentBlocks.Count != 0)
                                currentBlock = parentBlocks.Pop();
                            else
                                currentBlock = null;
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

                        // If this is the first child, trim the leading whitespace.  This prevents an extra
                        // space showing up at the start of paragraph inner text when it starts on a new line.
                        if(text.Parent.FirstNode == text)
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

            // Create all of the styles.  Currently it will look like the VS2005 style.
            Style noMargin = new Style(typeof(Paragraph));
            noMargin.Setters.Add(new Setter(Paragraph.MarginProperty, new Thickness(0)));

            Style noTopMargin = new Style(typeof(Paragraph));
            noTopMargin.Setters.Add(new Setter(Paragraph.MarginProperty, new Thickness(Double.NaN, 0,
                Double.NaN, Double.NaN)));

            Style section = new Style(typeof(Section));
            section.Setters.Add(new Setter(Section.FontFamilyProperty, new FontFamily("Verdana")));
            section.Setters.Add(new Setter(Section.FontSizeProperty, 11.0));

            Style codeTitle = new Style(typeof(Paragraph));
            codeTitle.Setters.Add(new Setter(Paragraph.FontWeightProperty, FontWeights.Bold));
            codeTitle.Setters.Add(new Setter(Paragraph.MarginProperty, new Thickness(0)));

            Style codeBlock = new Style(typeof(Section));
            codeBlock.Setters.Add(new Setter(Section.FontFamilyProperty,
                new FontFamily("Consolas, Courier New, Courier")));
            codeBlock.Setters.Add(new Setter(Section.FontSizeProperty, 12.0));
            codeBlock.Setters.Add(new Setter(Section.BackgroundProperty,
                new SolidColorBrush(Color.FromRgb(0xEF, 0xEF, 0xF7))));
            codeBlock.Setters.Add(new Setter(Section.BorderBrushProperty, new SolidColorBrush(Colors.Black)));
            codeBlock.Setters.Add(new Setter(Section.BorderThicknessProperty, new Thickness(0, 0.5, 0, 0)));
            codeBlock.Setters.Add(new Setter(Section.MarginProperty, new Thickness(0, 0, 0, 10)));
            codeBlock.Setters.Add(new Setter(Section.PaddingProperty, new Thickness(5)));
            codeBlock.Resources.Add(typeof(Paragraph), noMargin);

            Style codeInline = new Style(typeof(Span));
            codeInline.Setters.Add(new Setter(Span.FontFamilyProperty,
                new FontFamily("Consolas, Courier New, Courier")));
            codeInline.Setters.Add(new Setter(Span.FontSizeProperty, 12.0));
            codeInline.Setters.Add(new Setter(Span.ForegroundProperty,
                new SolidColorBrush(Color.FromRgb(0, 0, 0x66))));

            Style definedTerm = new Style(typeof(Paragraph));
            definedTerm.Setters.Add(new Setter(Paragraph.FontWeightProperty, FontWeights.Bold));
            definedTerm.Setters.Add(new Setter(Paragraph.MarginProperty, new Thickness(Double.NaN,
                Double.NaN, Double.NaN, 0)));

            Style definition = new Style(typeof(Section));
            definition.Setters.Add(new Setter(Section.MarginProperty, new Thickness(15,
                Double.NaN, Double.NaN, Double.NaN)));
            definition.Resources.Add(typeof(Paragraph), noTopMargin);

            Style list = new Style(typeof(List));
            list.Setters.Add(new Setter(List.MarginProperty, new Thickness(0, 0, 0, 10)));

            Style bottomMargin = new Style(typeof(Paragraph));
            bottomMargin.Setters.Add(new Setter(Paragraph.MarginProperty, new Thickness(0, 0, 0, 10)));

            Style listItem = new Style(typeof(ListItem));
            listItem.Setters.Add(new Setter(ListItem.MarginProperty, new Thickness(10, 10, Double.NaN, Double.NaN)));
            listItem.Resources.Add(typeof(Paragraph), bottomMargin);

            Style title = new Style(typeof(Paragraph));
            title.Setters.Add(new Setter(Paragraph.FontFamilyProperty, new FontFamily("Verdana")));
            title.Setters.Add(new Setter(Paragraph.FontSizeProperty, 14.0));
            title.Setters.Add(new Setter(Paragraph.FontWeightProperty, FontWeights.Bold));
            title.Setters.Add(new Setter(Paragraph.MarginProperty, new Thickness(Double.NaN, 10,
                Double.NaN, 5)));

            Style alertTitle = new Style(typeof(Paragraph));
            alertTitle.Setters.Add(new Setter(Paragraph.FontWeightProperty, FontWeights.Bold));
            alertTitle.Setters.Add(new Setter(Paragraph.BackgroundProperty,
                new SolidColorBrush(Color.FromRgb(0xEF, 0xEF, 0xF7))));
            alertTitle.Setters.Add(new Setter(Paragraph.ForegroundProperty,
                new SolidColorBrush(Color.FromRgb(0, 0, 0x66))));
            alertTitle.Setters.Add(new Setter(Paragraph.MarginProperty, new Thickness(0, 10, 0, 0)));
            alertTitle.Setters.Add(new Setter(Paragraph.PaddingProperty, new Thickness(5, 1, 5, 1)));

            Style alertBody = new Style(typeof(Section));
            alertBody.Setters.Add(new Setter(Section.BackgroundProperty,
                new SolidColorBrush(Color.FromRgb(0xF7, 0xF7, 0xFF))));
            alertBody.Setters.Add(new Setter(Section.BorderBrushProperty, new SolidColorBrush(Colors.Black)));
            alertBody.Setters.Add(new Setter(Section.BorderThicknessProperty, new Thickness(0, 0.5, 0, 0.5)));
            alertBody.Setters.Add(new Setter(Section.MarginProperty, new Thickness(0, 0, 0, 15)));
            alertBody.Setters.Add(new Setter(Section.PaddingProperty, new Thickness(4, 8, 4, 0)));
            alertBody.Resources.Add(typeof(Paragraph), noTopMargin);

            Style tableStyle = new Style(typeof(Table));
            tableStyle.Setters.Add(new Setter(Table.MarginProperty, new Thickness(0, 5, 0, 5)));

            Style tableCell = new Style(typeof(TableCell));
            tableCell.Setters.Add(new Setter(TableCell.PaddingProperty,
                new Thickness(5)));
            tableCell.Setters.Add(new Setter(TableCell.BackgroundProperty,
                new SolidColorBrush(Color.FromRgb(0xF7, 0xF7, 0xFF))));
            tableCell.Setters.Add(new Setter(TableCell.BorderThicknessProperty,
                new Thickness(0, 0.5, 0, 0.5)));
            tableCell.Setters.Add(new Setter(TableCell.BorderBrushProperty,
                new SolidColorBrush(Color.FromRgb(0xD5, 0xD5, 0xD3))));

            Style tableHeaderCell = new Style(typeof(TableCell), tableCell);
            tableHeaderCell.Setters.Add(new Setter(TableCell.BackgroundProperty,
                new SolidColorBrush(Color.FromRgb(0xE7, 0xE7, 0xF7))));

            Style tableHeaderRow = new Style(typeof(TableRowGroup));
            tableHeaderRow.Setters.Add(new Setter(TableRowGroup.BackgroundProperty,
                new SolidColorBrush(Color.FromRgb(0xEF, 0xEF, 0xF7))));
            tableHeaderRow.Setters.Add(new Setter(TableRowGroup.ForegroundProperty,
                new SolidColorBrush(Color.FromRgb(0, 0, 0x66))));
            tableHeaderRow.Setters.Add(new Setter(TableRowGroup.FontWeightProperty, FontWeights.Bold));
            tableHeaderRow.Resources.Add(typeof(TableCell), tableHeaderCell);

            Style tableTitle = new Style(typeof(Paragraph));
            tableTitle.Setters.Add(new Setter(Paragraph.ForegroundProperty,
                new SolidColorBrush(Color.FromRgb(0, 0x33, 0x99))));

            Style math = new Style(typeof(Italic));
            math.Setters.Add(new Setter(Italic.FontFamilyProperty, new FontFamily("Times New Roman")));
            math.Setters.Add(new Setter(Italic.FontSizeProperty, 14.0));

            Style literal = new Style(typeof(Span));
            literal.Setters.Add(new Setter(Span.ForegroundProperty,
                new SolidColorBrush(Color.FromRgb(0x8B, 0, 0))));

            Style quote = new Style(typeof(Paragraph));
            quote.Setters.Add(new Setter(Paragraph.MarginProperty, new Thickness(40, 15, 40, 15)));

            Style glossaryDivisionTitle = new Style(typeof(Paragraph), title);
            glossaryDivisionTitle.Setters.Add(new Setter(Paragraph.FontSizeProperty, 13.0));
            glossaryDivisionTitle.Setters.Add(new Setter(Paragraph.MarginProperty, new Thickness(Double.NaN, 10,
                Double.NaN, 0)));
            glossaryDivisionTitle.Setters.Add(new Setter(Paragraph.BorderThicknessProperty,
                new Thickness(0, 0, 0, 1)));
            glossaryDivisionTitle.Setters.Add(new Setter(Paragraph.BorderBrushProperty,
                new SolidColorBrush(Colors.Black)));

            Style glossaryLetterBar = new Style(typeof(Paragraph));
            glossaryLetterBar.Setters.Add(new Setter(Paragraph.FontSizeProperty, 10.0));

            Style glossaryLetterTitle = new Style(typeof(Paragraph), title);
            glossaryLetterTitle.Setters.Add(new Setter(Paragraph.ForegroundProperty,
                new SolidColorBrush(Colors.Gray)));

            Style glossaryDefinition = new Style(typeof(Section), definition);
            glossaryDefinition.Setters.Add(new Setter(Section.MarginProperty, new Thickness(25,
                Double.NaN, Double.NaN, Double.NaN)));

            Style relatedTopicTitle = new Style(typeof(Paragraph));
            relatedTopicTitle.Setters.Add(new Setter(Paragraph.FontSizeProperty, 12.0));
            relatedTopicTitle.Setters.Add(new Setter(Paragraph.FontWeightProperty, FontWeights.Bold));
            relatedTopicTitle.Setters.Add(new Setter(Paragraph.MarginProperty, new Thickness(Double.NaN, 5,
                Double.NaN, 5)));

            document.Resources.Add(typeof(Section), section);
            document.Resources.Add(typeof(List), list);
            document.Resources.Add(typeof(ListItem), listItem);
            document.Resources.Add(typeof(Table), tableStyle);
            document.Resources.Add(typeof(TableCell), tableCell);

            document.Resources.Add(NamedStyle.AlertTitle, alertTitle);
            document.Resources.Add(NamedStyle.AlertBody, alertBody);
            document.Resources.Add(NamedStyle.CodeTitle, codeTitle);
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
            document.Resources.Add(NamedStyle.Quote, quote);
            document.Resources.Add(NamedStyle.RelatedTopicTitle, relatedTopicTitle);
            document.Resources.Add(NamedStyle.TableHeaderRow, tableHeaderRow);
            document.Resources.Add(NamedStyle.TableTitle, tableTitle);
            document.Resources.Add(NamedStyle.Title, title);

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

                var section = currentBlock as Section;

                if(section != null)
                {
                    section.Blocks.Add((Block)blockElement);
                    parentBlocks.Push(currentBlock);
                    currentBlock = blockElement;
                    return;
                }

                var list = currentBlock as List;

                if(list != null)
                {
                    list.ListItems.Add((ListItem)blockElement);
                    parentBlocks.Push(currentBlock);
                    currentBlock = blockElement;
                    return;
                }

                var listItem = currentBlock as ListItem;

                if(listItem != null)
                {
                    listItem.Blocks.Add((Block)blockElement);
                    parentBlocks.Push(currentBlock);
                    currentBlock = blockElement;
                    return;
                }

                var table = currentBlock as Table;

                if(table != null)
                {
                    table.RowGroups.Add((TableRowGroup)blockElement);
                    parentBlocks.Push(currentBlock);
                    currentBlock = blockElement;
                    return;
                }

                var tableRowGroup = currentBlock as TableRowGroup;

                if(tableRowGroup != null)
                {
                    tableRowGroup.Rows.Add((TableRow)blockElement);
                    parentBlocks.Push(currentBlock);
                    currentBlock = blockElement;
                    return;
                }

                var tableRow = currentBlock as TableRow;

                if(tableRow != null)
                {
                    tableRow.Cells.Add((TableCell)blockElement);
                    parentBlocks.Push(currentBlock);
                    currentBlock = blockElement;
                    return;
                }

                var tableCell = currentBlock as TableCell;

                if(tableCell != null)
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

            var para = currentBlock as Paragraph;

            if(para != null)
            {
                para.Inlines.Add(inlineElement);
                return;
            }

            var listItem = currentBlock as ListItem;

            if(listItem != null)
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

            var tableCell = currentBlock as TableCell;

            if(tableCell != null)
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
            var section = currentBlock as Section;

            if(section != null)
            {
                Paragraph p = new Paragraph(inlineElement);
                section.Blocks.Add(p);

                // This one won't go on the stack
                currentBlock = p;
                return;
            }

            throw new InvalidOperationException("Unknown parent block element encountered for new inline element");
        }
        #endregion
    }
}
