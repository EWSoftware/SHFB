﻿//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : TopicTransformationCore.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/14/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the abstract base class that is used to define the settings and common functionality for a
// specific presentation style topic transformation.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/14/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using Sandcastle.Core.PresentationStyle.Transformation.Elements;
using Sandcastle.Core.PresentationStyle.Transformation.Elements.Html;
using Sandcastle.Core.Reflection;

namespace Sandcastle.Core.PresentationStyle.Transformation
{
    /// <summary>
    /// This abstract base class is used to define the settings and common functionality for a specific
    /// presentation style topic transformation.
    /// </summary>
    /// <remarks>This implements the core processing common to all presentation styles.  While it can render
    /// presentation style neutral elements such as <c>referenceLink</c> and <c>include</c> elements, any
    /// presentation style specific elements such as HTML elements, Open XML elements, or markdown should be
    /// rendered by derived classes in the appropriate overridden methods.</remarks>
    public abstract class TopicTransformationCore
    {
        #region Private data members
        //=====================================================================

        private readonly Dictionary<string, Element> elementHandlers;
        private readonly Dictionary<string, LanguageSpecificText> languageSpecificText;
        private readonly Dictionary<string, TransformationArgument> transformationArguments;
        private readonly List<ApiTopicSectionHandler> apiTopicSections;

        private string bibliographyDataFile;
        private Dictionary<string, XElement> bibliographyData;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the topic key
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// This read-only property returns true if a MAML topic is being generated, false if an API topic is
        /// being generated
        /// </summary>
        public bool IsMamlTopic { get; private set; }

        /// <summary>
        /// This read-only property returns the root <c>document</c> node for the topic being rendered
        /// </summary>
        public XElement DocumentNode { get; private set; }

        /// <summary>
        /// This read-only property returns the <c>metadata</c> node for the topic being rendered
        /// </summary>
        public XElement MetadataNode { get; private set; }

        /// <summary>
        /// This read-only property returns the <c>topic</c> node for the MAML topic being rendered.  This is
        /// not used for API topics.
        /// </summary>
        public XElement TopicNode { get; private set; }

        /// <summary>
        /// This read-only property returns the <c>reference</c> node for the API topic being rendered.  This is
        /// not used for MAML topics.
        /// </summary>
        public XElement ReferenceNode { get; private set; }

        /// <summary>
        /// This read-only property returns the <c>syntax</c> node for the API topic being rendered.  This is
        /// not used for MAML topics.
        /// </summary>
        public XElement SyntaxNode { get; private set; }

        /// <summary>
        /// This read-only property returns the <c>comments</c> node for the API topic being rendered.  This is
        /// not used for MAML topics.
        /// </summary>
        public XElement CommentsNode { get; private set; }

        /// <summary>
        /// This read-only property returns common API member information for the API topic being rendered.  This
        /// is not used for MAML topics.
        /// </summary>
        public ApiMember ApiMember { get; private set; }

        /// <summary>
        /// This is used to get or set the current element to which rendered content is being added
        /// </summary>
        /// <value>If changed, you are responsible for restoring the prior value if necessary after completing
        /// any processing related to the change.</value>
        public XElement CurrentElement { get; set; }

        /// <summary>
        /// This is used to get or set the path used for icons and other images in the presentation style
        /// </summary>
        /// <remarks>Ensure the path is terminated with an appropriate directory separator character</remarks>
        public abstract string IconPath { get; set; }

        /// <summary>
        /// This is used to get or set the path used for style sheets in the presentation style
        /// </summary>
        /// <remarks>Ensure the path is terminated with an appropriate directory separator character</remarks>
        public abstract string StyleSheetPath { get; set; }

        /// <summary>
        /// This is used to get or set the path used for scripts in the presentation style
        /// </summary>
        /// <remarks>Ensure the path is terminated with an appropriate directory separator character</remarks>
        public abstract string ScriptPath { get; set; }

        /// <summary>
        /// This is used to get or set the locale
        /// </summary>
        /// <value>If not set, the default is en-US</value>
        public string Locale { get; set; } = "en-US";

        /// <summary>
        /// This read-only property returns the help file formats supported by the presentation style
        /// </summary>
        public HelpFileFormats SupportedFormats { get; }

        /// <summary>
        /// This is used to get or set the path to the bibliography data file for the <c>bibliography</c>
        /// and <c>cite</c> elements.
        /// </summary>
        /// <value>If not set, the bibliography elements will be ignored</value>
        public string BibliographyDataFile
        {
            get => bibliographyDataFile;
            set
            {
                bibliographyDataFile = value;
                bibliographyData = null;
            }
        }

        /// <summary>
        /// This read-only property returns the content of the bibliography data file if a
        /// <see cref="BibliographyDataFile" /> has been specified.
        /// </summary>
        /// <value>The key is the reference name, the value is the reference element</value>
        public IReadOnlyDictionary<string, XElement> BibliographyData
        {
            get
            {
                // Load the bibliography data on first use
                if(bibliographyData == null)
                {
                    bibliographyData = new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);

                    if(!String.IsNullOrWhiteSpace(bibliographyDataFile) && File.Exists(bibliographyDataFile))
                    {
                        var bd = XDocument.Load(bibliographyDataFile).Root;

                        foreach(var reference in bd.Elements("reference"))
                        {
                            string key = reference.Attribute("name").Value?.NormalizeWhiteSpace();

                            if(!String.IsNullOrWhiteSpace(key) && !bibliographyData.ContainsKey(key))
                                bibliographyData.Add(key, reference);
                        }
                    }
                }

                return bibliographyData;
            }
        }

        /// <summary>
        /// This read-only property returns an enumerable list of the API section handlers in the order that they
        /// will be rendered
        /// </summary>
        protected IEnumerable<ApiTopicSectionHandler> ApiTopicSections => apiTopicSections;

        /// <summary>
        /// This read-only property returns a dictionary used to contain transformation arguments used by the
        /// presentation style
        /// </summary>
        public IReadOnlyDictionary<string, TransformationArgument> TransformationArguments => transformationArguments;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="supportedFormats">The presentation style's supported help file formats</param>
        protected TopicTransformationCore(HelpFileFormats supportedFormats)
        {
            elementHandlers = new Dictionary<string, Element>();
            languageSpecificText = new Dictionary<string, LanguageSpecificText>();
            apiTopicSections = new List<ApiTopicSectionHandler>();
            transformationArguments = new Dictionary<string, TransformationArgument>(StringComparer.OrdinalIgnoreCase);

            this.SupportedFormats = supportedFormats;
            this.CreateTransformationArguments();
            this.CreateLanguageSpecificText();
            this.CreateElementHandlers();
            this.CreateApiTopicSectionHandlers();
        }
        #endregion

        #region Events
        //=====================================================================

        /// <summary>
        /// This event is raised to notify the user of an unhandled element
        /// </summary>
        /// <remarks>The unhandled element may be incorrectly spelled or may be a custom element for which a new
        /// handler needs to be added.</remarks>
        public event EventHandler<UnhandledElementEventArgs> UnhandledElement;

        /// <summary>
        /// This is used to raise the <see cref="UnhandledElement"/> event
        /// </summary>
        /// <param name="elementName">The unhandled element name</param>
        /// <param name="parentElementName">The parent element name of the unhandled element</param>
        public virtual void OnUnhandledElement(string elementName, string parentElementName)
        {
            this.UnhandledElement?.Invoke(this, new UnhandledElementEventArgs(this.Key, elementName, parentElementName));
        }

        /// <summary>
        /// This event is raised to notify the user of section having been rendered
        /// </summary>
        /// <remarks>This event is raised regardless of whether or not anything was actually rendered for the
        /// section.</remarks>
        public event EventHandler<RenderedSectionEventArgs> SectionRendered;

        /// <summary>
        /// This is used to raise the <see cref="SectionRendered"/> event
        /// </summary>
        /// <param name="sectionName">The section name</param>
        /// <param name="customName">The name of the custom section if <c>sectionName</c> is <c>Custom</c></param>
        public virtual void OnSectionRendered(ApiTopicSectionType sectionName, string customName)
        {
            this.SectionRendered?.Invoke(this, new RenderedSectionEventArgs(this.Key, sectionName, customName));
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// This is called to create the transformation arguments that will be used by the transformation
        /// </summary>
        /// <remarks>Transformation arguments are used to pass user-defined values for parts of the rendered
        /// topic such as a logo filename, source code URL, etc.</remarks>
        protected abstract void CreateTransformationArguments();

        /// <summary>
        /// This is called to create the language specific text elements that will be used by the transformation
        /// </summary>
        /// <remarks>Define the common keywords used in language-specific text.  These will be connected to the
        /// language filter in the topic and the appropriate text will be shown based on the selected language.</remarks>
        protected abstract void CreateLanguageSpecificText();

        /// <summary>
        /// This is called to create the element handlers that will be used by the transformation
        /// </summary>
        /// <remarks>Element handlers are used to render the common MAML and API topic elements.</remarks>
        protected abstract void CreateElementHandlers();

        /// <summary>
        /// This is called to create the API topic section handlers that will be used by the transformation
        /// </summary>
        /// <remarks>Unlike MAML topics, API topics are rendered in a fixed order defined by the presentation
        /// style that may vary based on the topic type.</remarks>
        protected abstract void CreateApiTopicSectionHandlers();

        /// <summary>
        /// Add an element that will be transformed when the topic is rendered
        /// </summary>
        /// <param name="element">The element handler</param>
        /// <exception cref="ArgumentException">This is thrown if the element name already has a handler</exception>
        public void AddElement(Element element)
        {
            if(element == null)
                throw new ArgumentNullException(nameof(element));

            if(elementHandlers.ContainsKey(element.Name))
            {
                throw new ArgumentException($"An element handler already exists for element '{element.Name}'.  " +
                    "Use the ReplaceElement method to replace it", nameof(element));
            }

            elementHandlers.Add(element.Name, element);
        }

        /// <summary>
        /// Add a range of elements that will be transformed when the topic is rendered
        /// </summary>
        /// <param name="elements">The element handlers</param>
        /// <exception cref="ArgumentException">This is thrown if an element name already has a handler</exception>
        public void AddElements(IEnumerable<Element> elements)
        {
            if(elements == null)
                throw new ArgumentNullException(nameof(elements));

            foreach(Element element in elements)
                this.AddElement(element);
        }

        /// <summary>
        /// Replace an element handler
        /// </summary>
        /// <param name="element">The element handler for the element to replace</param>
        /// <remarks>If an element handler is not present for the element, it will be added instead</remarks>
        /// <returns>The element handler that was replaced or null if one did not exist</returns>
        public Element ReplaceElement(Element element)
        {
            if(element == null)
                throw new ArgumentNullException(nameof(element));

            if(!elementHandlers.TryGetValue(element.Name, out Element match))
                elementHandlers.Add(element.Name, element);
            else
                elementHandlers[element.Name] = element;

            return match;
        }

        /// <summary>
        /// Replace a range of elements
        /// </summary>
        /// <param name="elements">The element handlers for the elements to replace.  If an element handler is
        /// not present for the element, it will be added instead.</param>
        public void ReplaceElements(IEnumerable<Element> elements)
        {
            if(elements == null)
                throw new ArgumentNullException(nameof(elements));

            foreach(Element element in elements)
                this.ReplaceElement(element);
        }

        /// <summary>
        /// This is used to retrieve the current handler for the given element name
        /// </summary>
        /// <param name="elementName">The element name for which to get the handler</param>
        /// <returns>The element handler if found or null if there isn't one</returns>
        public Element ElementHandlerFor(string elementName)
        {
            if(elementHandlers.TryGetValue(elementName, out Element handler))
                return handler;

            return null;
        }

        /// <summary>
        /// Add a new API topic section handler
        /// </summary>
        /// <param name="sectionHandler">The API topic section handler to add</param>
        /// <exception cref="ArgumentException">This is thrown if a section handler has already been defined for
        /// the given section.</exception>
        public void AddApiTopicSectionHandler(ApiTopicSectionHandler sectionHandler)
        {
            if(sectionHandler == null)
                throw new ArgumentNullException(nameof(sectionHandler));

            var match = apiTopicSections.FirstOrDefault(s => s.SectionType == sectionHandler.SectionType &&
                s.CustomSectionName == sectionHandler.CustomSectionName);

            if(match != null)
            {
                throw new ArgumentException("A section handler has already been defined for " +
                    $"{sectionHandler.SectionType} {sectionHandler.CustomSectionName}");
            }

            apiTopicSections.Add(sectionHandler);
        }

        /// <summary>
        /// Add a range of new API topic section handlers
        /// </summary>
        /// <param name="sectionHandlers">An enumerable list of the API topic section handlers to add</param>
        public void AddApiTopicSectionHandlerRange(IEnumerable<ApiTopicSectionHandler> sectionHandlers)
        {
            if(sectionHandlers == null)
                throw new ArgumentNullException(nameof(sectionHandlers));

            foreach(var s in sectionHandlers)
                this.AddApiTopicSectionHandler(s);
        }

        /// <summary>
        /// Remove an API topic section handler
        /// </summary>
        /// <param name="sectionType">The section type to remove</param>
        /// <param name="customSectionName">If the section type is custom, this defines the custom section name</param>
        /// <returns>The API topic handler that was removed or null if it was not found</returns>
        public ApiTopicSectionHandler RemoveApiTopicSectionHandler(ApiTopicSectionType sectionType, string customSectionName)
        {
            var match = apiTopicSections.FirstOrDefault(s => s.SectionType == sectionType &&
                s.CustomSectionName == customSectionName);

            if(match != null)
                apiTopicSections.Remove(match);

            return match;
        }

        /// <summary>
        /// Replace an API topic section handler with a new one
        /// </summary>
        /// <param name="sectionHandler">The API topic section handler to add</param>
        /// <exception cref="ArgumentException">This is thrown if a section handler has not been defined for
        /// the given section.</exception>
        /// <returns>The API topic handler that was replaced</returns>
        public ApiTopicSectionHandler ReplaceApiTopicSectionHandler(ApiTopicSectionHandler sectionHandler)
        {
            if(sectionHandler == null)
                throw new ArgumentNullException(nameof(sectionHandler));

            var match = apiTopicSections.FirstOrDefault(s => s.SectionType == sectionHandler.SectionType &&
                s.CustomSectionName == sectionHandler.CustomSectionName);

            if(match == null)
            {
                throw new ArgumentException("No section handler has been defined for " +
                    $"{sectionHandler.SectionType} {sectionHandler.CustomSectionName}");
            }

            apiTopicSections.Insert(apiTopicSections.IndexOf(match), sectionHandler);
            apiTopicSections.Remove(match);

            return match;
        }

        /// <summary>
        /// Insert an API topic section handler before the given section handler
        /// </summary>
        /// <param name="sectionHandler">The API topic section handler to insert.</param>
        /// <param name="insertBeforeSectionHandler">The API topic section handler before which the given
        /// handler is inserted.</param>
        /// <remarks>If the section handler already exists, it is removed before inserting it in the new location.</remarks>
        public void InsertApiTopicSectionHandlerBefore(ApiTopicSectionHandler sectionHandler,
          ApiTopicSectionHandler insertBeforeSectionHandler)
        {
            if(sectionHandler == null)
                throw new ArgumentNullException(nameof(sectionHandler));

            if(insertBeforeSectionHandler == null)
                throw new ArgumentNullException(nameof(insertBeforeSectionHandler));

            if(sectionHandler != insertBeforeSectionHandler)
            {
                var match = apiTopicSections.FirstOrDefault(s => s.SectionType == sectionHandler.SectionType &&
                    s.CustomSectionName == sectionHandler.CustomSectionName);

                if(match != null)
                    apiTopicSections.Remove(match);

                match = apiTopicSections.FirstOrDefault(s => s.SectionType == insertBeforeSectionHandler.SectionType &&
                    s.CustomSectionName == insertBeforeSectionHandler.CustomSectionName);

                if(match == null)
                    throw new ArgumentException("Insert Before handler not found", nameof(insertBeforeSectionHandler));

                apiTopicSections.Insert(apiTopicSections.IndexOf(match), sectionHandler);
            }
        }

        /// <summary>
        /// Insert an API topic section handler after the given section handler
        /// </summary>
        /// <param name="sectionHandler">The API topic section handler to insert.</param>
        /// <param name="insertAfterSectionHandler">The API topic section handler after which the given
        /// handler is inserted.</param>
        /// <remarks>If the section handler already exists, it is removed before inserting it in the new location.</remarks>
        public void InsertApiTopicSectionHandlerAfter(ApiTopicSectionHandler sectionHandler,
          ApiTopicSectionHandler insertAfterSectionHandler)
        {
            if(sectionHandler == null)
                throw new ArgumentNullException(nameof(sectionHandler));

            if(insertAfterSectionHandler == null)
                throw new ArgumentNullException(nameof(insertAfterSectionHandler));

            if(sectionHandler != insertAfterSectionHandler)
            {
                var match = apiTopicSections.FirstOrDefault(s => s.SectionType == sectionHandler.SectionType &&
                    s.CustomSectionName == sectionHandler.CustomSectionName);

                if(match != null)
                    apiTopicSections.Remove(match);

                match = apiTopicSections.FirstOrDefault(s => s.SectionType == insertAfterSectionHandler.SectionType &&
                    s.CustomSectionName == insertAfterSectionHandler.CustomSectionName);

                if(match == null)
                    throw new ArgumentException("Insert After handler not found", nameof(insertAfterSectionHandler));

                apiTopicSections.Insert(apiTopicSections.IndexOf(match) + 1, sectionHandler);
            }
        }

        /// <summary>
        /// This is used to retrieve the current API topic section handler for the given section
        /// </summary>
        /// <param name="sectionType">The section type</param>
        /// <param name="customSectionName">If the section type is <c>CustomSection</c>, this should refer to the
        /// custom section name</param>
        /// <returns>The API topic section handler if found or null if there isn't one</returns>
        public ApiTopicSectionHandler ApiTopicSectionHandlerFor(ApiTopicSectionType sectionType, string customSectionName)
        {
            return apiTopicSections.FirstOrDefault(s => s.SectionType == sectionType &&
                s.CustomSectionName == customSectionName);
        }

        /// <summary>
        /// Add a language specific text entry
        /// </summary>
        /// <param name="keyword">The language specific text entry for the keyword</param>
        /// <remarks>Entries will be indexed for each of the given language keywords in the entry</remarks>
        /// <exception cref="ArgumentException">This is thrown if an entry already exists for the keyword</exception>
        public void AddLanguageSpecificText(LanguageSpecificText keyword)
        {
            if(keyword == null)
                throw new ArgumentNullException(nameof(keyword));

            foreach(var lst in keyword.Text)
            {
                if(!String.IsNullOrWhiteSpace(lst.Keyword))
                {
                    if(languageSpecificText.ContainsKey(lst.Keyword))
                    {
                        throw new ArgumentException("A language specific text entry already exists for " +
                            $"keyword '{lst.Keyword}'.  Use the ReplaceLanguageSpecificText method to replace it",
                            nameof(keyword));
                    }

                    languageSpecificText.Add(lst.Keyword, keyword);
                }
            }
        }

        /// <summary>
        /// Replace a language specific text entry
        /// </summary>
        /// <param name="keyword">The language specific text entry for the keyword to replace</param>
        /// <remarks>If an entry is not present for the keyword, it will be added instead</remarks>
        public void ReplaceLanguageSpecificText(LanguageSpecificText keyword)
        {
            if(keyword == null)
                throw new ArgumentNullException(nameof(keyword));

            foreach(var lst in keyword.Text)
            {
                if(!String.IsNullOrWhiteSpace(lst.Keyword))
                {
                    if(!languageSpecificText.ContainsKey(lst.Keyword))
                        languageSpecificText.Add(lst.Keyword, keyword);
                    else
                        languageSpecificText[lst.Keyword] = keyword;
                }
            }
        }

        /// <summary>
        /// Add a range of language specific text entries
        /// </summary>
        /// <param name="keywords">The language specific text entries to add</param>
        /// <remarks>Entries will be indexed for each of the given language keywords in the entry</remarks>
        /// <exception cref="ArgumentException">This is thrown if a keyword already has an entry</exception>
        public void AddLanguageSpecificTextRange(IEnumerable<LanguageSpecificText> keywords)
        {
            if(keywords == null)
                throw new ArgumentNullException(nameof(keywords));

            foreach(LanguageSpecificText kw in keywords)
                this.AddLanguageSpecificText(kw);
        }

        /// <summary>
        /// Replace a range of language specific text entries
        /// </summary>
        /// <param name="keywords">The language specific text entries to replace.  If an entry is not present for
        /// the keyword, it will be added instead.</param>
        public void ReplaceLanguageSpecificTextRange(IEnumerable<LanguageSpecificText> keywords)
        {
            if(keywords == null)
                throw new ArgumentNullException(nameof(keywords));

            foreach(LanguageSpecificText kw in keywords)
                this.ReplaceLanguageSpecificText(kw);
        }

        /// <summary>
        /// This is used to retrieve the language specific text for a given keyword
        /// </summary>
        /// <param name="keyword">The keyword for which to get language specific text</param>
        /// <returns>The language specific text entry if found for the given keyword or null if there isn't one</returns>
        public LanguageSpecificText LanguageSpecificTextFor(string keyword)
        {
            if(languageSpecificText.TryGetValue(keyword, out LanguageSpecificText lst))
                return lst;

            return null;
        }

        /// <summary>
        /// Add a new transformation argument
        /// </summary>
        /// <param name="argument">The transformation argument information</param>
        /// <exception cref="ArgumentException">This is thrown if a transformation argument already exists for
        /// the new argument's name.</exception>
        public void AddTransformationArgument(TransformationArgument argument)
        {
            if(argument == null)
                throw new ArgumentNullException(nameof(argument));

            transformationArguments.Add(argument.Key, argument);
        }

        /// <summary>
        /// Add a range of new transformation arguments
        /// </summary>
        /// <param name="arguments">An enumerable list of the transformation arguments to add</param>
        /// <exception cref="ArgumentException">This is thrown if a transformation argument already exists for
        /// any of the new argument names.</exception>
        public void AddTransformationArgumentRange(IEnumerable<TransformationArgument> arguments)
        {
            if(arguments == null)
                throw new ArgumentNullException(nameof(arguments));

            foreach(var a in arguments)
                transformationArguments.Add(a.Key, a);
        }

        /// <summary>
        /// This is used to parse the topic data and render the topic output for the presentation style
        /// </summary>
        /// <param name="key">The topic key</param>
        /// <param name="topic">The topic to render</param>
        /// <returns>The rendered document content</returns>
        public XDocument Render(string key, XDocument topic)
        {
            if(topic == null)
                throw new ArgumentNullException(nameof(topic));

            // Get references to the most commonly used nodes and values for the commonly accessed attributes
            this.Key = key ?? throw new ArgumentNullException(nameof(key));
            this.DocumentNode = topic.Root;
            this.MetadataNode = this.DocumentNode.Element("metadata");
            this.IsMamlTopic = (this.DocumentNode.Attribute("type")?.Value.Equals("MAML", StringComparison.Ordinal) ?? false);

            if(this.MetadataNode == null)
                throw new InvalidOperationException("The metadata node was not found");

            if(this.IsMamlTopic)
            {
                this.TopicNode = this.DocumentNode.Element("topic");
                this.ReferenceNode = this.SyntaxNode = this.CommentsNode = null;
                this.ApiMember = null;

                if(this.TopicNode == null)
                    throw new InvalidOperationException("The topic node was not found");
            }
            else
            {
                this.TopicNode = null;
                this.ReferenceNode = this.DocumentNode.Element("reference");
                this.SyntaxNode = this.DocumentNode.Element("syntax");
                this.CommentsNode = this.DocumentNode.Element("comments");

                if(this.ReferenceNode == null)
                    throw new InvalidOperationException("The reference node was not found");
                else
                    this.ApiMember = new ApiMember(this.ReferenceNode, this.Key);

                if(this.SyntaxNode == null)
                    throw new InvalidOperationException("The syntax node was not found");

                if(this.CommentsNode == null)
                    throw new InvalidOperationException("The comments node was not found");
            }

            return this.RenderTopic();
        }

        /// <summary>
        /// This is used to render a reference link based on the given type information
        /// </summary>
        /// <param name="content">The content element to which the link is added</param>
        /// <param name="typeInfo">An element containing the type information for the reference link</param>
        /// <param name="qualified">True to fully qualify the type, false to show the type name alone</param>
        /// <remarks>This method will call itself recursively if necessary to render type specializations,
        /// template parameter types, array types, etc.</remarks>
        public abstract void RenderTypeReferenceLink(XElement content, XElement typeInfo, bool qualified);

        /// <summary>
        /// Render the topic content based on the topic type
        /// </summary>
        /// <returns>The current topic rendered in the presentation style format</returns>
        protected abstract XDocument RenderTopic();

        /// <summary>
        /// Render the given XML node based on its node type
        /// </summary>
        /// <param name="node">The node to render</param>
        public virtual void RenderNode(XNode node)
        {
            switch(node)
            {
                case XText text:
                    this.CurrentElement.Add(new XText(text.Value));
                    break;

                case XElement element:
                    // If it has no handler, add a default handler and treat it like a non-rendered parent element
                    if(!elementHandlers.TryGetValue(element.Name.LocalName, out Element handler))
                    {
                        // Raise an event to flag unhandled elements
                        this.OnUnhandledElement(element.Name.LocalName, element.Parent.Name.LocalName);

                        handler = new NonRenderedParentElement(element.Name.LocalName);
                        elementHandlers.Add(handler.Name, handler);
                    }

                    handler.Render(this, element);
                    break;

                case XComment _:
                    break;

                default:
                    if(node != null)
                    {
                        Debug.WriteLine("Unhandled node type: {0}", node.NodeType);

                        if(Debugger.IsAttached)
                            Debugger.Break();
                    }
                    break;
            }
        }

        /// <summary>
        /// This makes the given parent element the current element, renders the list of children to it and then
        /// resets the current element to the prior current element.
        /// </summary>
        /// <param name="parent">The parent element to which the children are rendered</param>
        /// <param name="children">An enumerable list of child elements to render</param>
        public void RenderChildElements(XElement parent, IEnumerable<XNode> children)
        {
            if(parent == null)
                throw new ArgumentNullException(nameof(parent));

            if(children != null && children.Any())
            {
                var priorCurrentElement = this.CurrentElement;
                this.CurrentElement = parent;

                foreach(var child in children)
                    this.RenderNode(child);

                this.CurrentElement = priorCurrentElement;
            }
        }

        /// <summary>
        /// Create a topic section with a title
        /// </summary>
        /// <param name="uniqueId">A unique ID that can be used to identify the section</param>
        /// <param name="localizedTitle">True if <paramref name="title"/> is a localized include item, false if
        /// it is literal title text.</param>
        /// <param name="title">The item id if <paramref name="localizedTitle"/> is true, or the literal title
        /// text if it is false.</param>
        /// <param name="linkId">An optional link ID for the section.  If there is no title, this will be
        /// ignored.</param>
        /// <returns>A tuple containing a reference to the title element if a title was created, null if not,
        /// and a reference to the content element into which any additional content can be rendered.  Both
        /// elements should be added to the topic if created.</returns>
        public abstract (XElement Title, XElement Content) CreateSection(string uniqueId, bool localizedTitle,
          string title, string linkId);

        /// <summary>
        /// Create a topic subsection with a title
        /// </summary>
        /// <param name="localizedTitle">True if <paramref name="title"/> is a localized include item, false if
        /// it is literal title text.</param>
        /// <param name="title">The item id if <paramref name="localizedTitle"/> is true, or the literal title
        /// text if it is false.</param>
        /// <returns>A tuple containing a reference to the title element if a title was created, null if not,
        /// and a reference to the content element into which any additional content can be rendered or null
        /// if it should be rendered into the current topic element.  Both elements should be added to the topic
        /// if created.</returns>
        public abstract (XElement Title, XElement Content) CreateSubsection(bool localizedTitle, string title);

        /// <summary>
        /// Get the title for a MAML topic
        /// </summary>
        /// <returns>The XML content representing the current topic's title</returns>
        protected virtual XNode MamlTopicTitle()
        {
            if(!this.IsMamlTopic)
                throw new InvalidOperationException("Not a MAML topic");

            return new XText((this.MetadataNode.Element("title") ??
                this.TopicNode.Descendants(Element.Ddue + "title").FirstOrDefault())?.Value);
        }

        /// <summary>
        /// Get the title for an API topic
        /// </summary>
        /// <param name="qualifyMembers">True to qualify members with their namespace, false if not</param>
        /// <param name="plainText">True if it should be in plain text (metadata and table of contents title) or
        /// decorated with language-specific text elements (page title).</param>
        /// <returns>The XML content representing the current topic's title</returns>
        protected virtual XNode ApiTopicTitle(bool qualifyMembers, bool plainText)
        {
            if(this.IsMamlTopic)
                throw new InvalidOperationException("Not an API topic");

            string titleItem;

            switch(this.ApiMember)
            {
                case var t when t.TopicGroup == ApiMemberGroup.Api:
                    // API topic titles.  The subsubgroup, subgroup, or group determines the title.
                    if(t.ApiSubSubgroup != ApiMemberGroup.None)
                    {
                        if(t.ApiSubSubgroup == ApiMemberGroup.Operator &&
                          (t.Name.Equals("Explicit", StringComparison.Ordinal) ||
                           t.Name.Equals("Implicit", StringComparison.Ordinal)))
                        {
                            titleItem = "typeConversion";
                        }
                        else
                            titleItem = t.ApiSubSubgroup.ToString();
                    }
                    else
                    {
                        if(t.ApiSubgroup != ApiMemberGroup.None)
                            titleItem = t.ApiSubgroup.ToString();
                        else
                            titleItem = t.ApiGroup.ToString();
                    }
                    break;

                case var t when t.TopicSubgroup == ApiMemberGroup.Overload:
                    // Overload topic titles
                    if(t.ApiSubSubgroup == ApiMemberGroup.Operator)
                    {
                        if(t.Name.Equals("Explicit", StringComparison.Ordinal) ||
                           t.Name.Equals("Implicit", StringComparison.Ordinal))
                        {
                            titleItem = "conversionOperator";
                        }
                        else
                            titleItem = t.ApiSubSubgroup.ToString();
                    }
                    else
                        titleItem = t.ApiSubgroup.ToString();
                    break;

                case var t when t.TopicGroup == ApiMemberGroup.List:
                    // List topic titles.  The topic subgroup (e.g. "Methods") determines the title.
                    if(t.TopicSubgroup == ApiMemberGroup.Operators)
                    {
                        int operatorCount = this.ReferenceNode.Element("elements").Elements("element").Where(
                            el => !(el.Element("apidata")?.Attribute("name")?.Value ?? String.Empty).Equals(
                                "Explicit", StringComparison.Ordinal) &&
                                !(el.Element("apidata")?.Attribute("name")?.Value ?? String.Empty).Equals(
                                "Implicit", StringComparison.Ordinal)).Count();
                        int conversionCount = this.ReferenceNode.Element("elements").Elements("element").Where(
                            el => (el.Element("apidata")?.Attribute("name")?.Value ?? String.Empty).Equals(
                                "Explicit", StringComparison.Ordinal) ||
                                (el.Element("apidata")?.Attribute("name")?.Value ?? String.Empty).Equals(
                                "Implicit", StringComparison.Ordinal)).Count();

                        if(operatorCount > 0 && conversionCount > 0)
                            titleItem = "OperatorsAndTypeConversions";
                        else
                        {
                            if(operatorCount == 0 && conversionCount > 0)
                                titleItem = "TypeConversions";
                            else
                                titleItem = t.TopicSubgroup.ToString();
                        }
                    }
                    else
                        titleItem = t.TopicSubgroup.ToString();
                    break;

                case var t when t.TopicGroup == ApiMemberGroup.Root || t.TopicGroup == ApiMemberGroup.RootGroup:
                    // Root titles
                    titleItem = "root";
                    break;

                default:
                    // We shouldn't get here so if this item appears as a missing content item, there's an issue
                    titleItem = "Unknown";
                    break;
            }

            XElement titleParam = new XElement("parameter"), memberParams = new XElement("parameter"),
                includeItem = new XElement("include", new XAttribute("item", "topicTitle_" + titleItem),
                    titleParam, memberParams);

            if(plainText)
                titleParam.Add(this.ApiTopicShortNamePlainText(qualifyMembers));
            else
                titleParam.Add(this.ApiTopicShortNameDecorated());

            // Only show parameters for operators and overloaded members
            if(this.ApiMember.ApiSubSubgroup == ApiMemberGroup.Operator &&
              (this.ApiMember.Name.Equals("Explicit", StringComparison.Ordinal) ||
               this.ApiMember.Name.Equals("Implicit", StringComparison.Ordinal)))
            {
                memberParams.Add(this.ApiTopicOperatorTypes(plainText));
            }
            else
            {
                if(!String.IsNullOrWhiteSpace(this.ApiMember.OverloadTopicId))
                {
                    memberParams.Add(this.ApiTopicParameterTypes(plainText));
                }
            }

            return includeItem;
        }

        /// <summary>
        /// Get the short type/member name for an API topic in plain text.  This is used for metadata values and
        /// the table of contents title.
        /// </summary>
        /// <param name="qualifyMembers">True to qualify members with their namespace, false if not</param>
        /// <returns>An XML element representing the current topic's type/member name in plain text</returns>
        protected virtual XNode ApiTopicShortNamePlainText(bool qualifyMembers)
        {
            XElement content = null;
            var sb = new StringBuilder(1024);

            switch(this.ApiMember)
            {
                case var t when (t.TopicGroup == ApiMemberGroup.Api && t.ApiGroup == ApiMemberGroup.Type) ||
                  (t.TopicGroup == ApiMemberGroup.List && t.TopicSubgroup != ApiMemberGroup.Overload):
                    // Type overview pages and member list pages get the type name
                    this.ApiTypeNamePlainText(sb, this.ReferenceNode);
                    break;

                case var t when (t.TopicGroup == ApiMemberGroup.Api && t.ApiSubgroup == ApiMemberGroup.Constructor) ||
                  (t.TopicSubgroup == ApiMemberGroup.Overload && t.ApiSubgroup == ApiMemberGroup.Constructor):
                    // Constructors and member list pages also use the type name
                    this.ApiTypeNamePlainText(sb, this.ReferenceNode.Element("containers").Element("type"));
                    break;

                case var t when (t.TopicGroup == ApiMemberGroup.Api && t.ApiGroup == ApiMemberGroup.Member) ||
                  (t.TopicSubgroup == ApiMemberGroup.Overload && t.ApiGroup == ApiMemberGroup.Member):
                    // Member pages use the member name, qualified if the qualified flag is set
                    if(qualifyMembers)
                    {
                        this.ApiTypeNamePlainText(sb, this.ReferenceNode.Element("containers").Element("type"));

                        if(t.ApiSubSubgroup == ApiMemberGroup.Operator &&
                          (t.Name.Equals("Explicit", StringComparison.Ordinal) ||
                           t.Name.Equals("Implicit", StringComparison.Ordinal)))
                        {
                            sb.Append(' ');
                        }
                        else
                            sb.Append('.');
                    }

                    // EII names are InterfaceName.InterfaceMemberName, not MemberName
                    if(t.IsExplicitlyImplemented)
                    {
                        var member = this.ReferenceNode.Element("implements").Element("member");

                        this.ApiTypeNamePlainText(sb, member.Element("type"));
                        
                        sb.Append('.');

                        // If the API element is not present (unresolved type), show the type name from the type element
                        if(!String.IsNullOrWhiteSpace(t.Name))
                            sb.Append(t.Name);
                        else
                        {
                            string name = member.Attribute("api")?.Value;

                            if(name != null)
                            {
                                int pos = name.LastIndexOf('.');

                                if(pos != -1)
                                    name = name.Substring(pos + 1);

                                sb.Append(name);
                            }
                        }

                        var templates = member.Element("templates");

                        if(templates != null)
                            this.ApiTypeNamePlainText(sb, templates);
                    }
                    else
                    {
                        // Other members use the name with templates if any.
                        var templates = this.ReferenceNode.Element("templates");

                        sb.Append(t.Name);

                        // Just use the plain, unadorned API name for list pages (overloads pages)
                        if(templates != null && t.TopicGroup != ApiMemberGroup.List)
                            this.ApiTypeNamePlainText(sb, templates);
                    }
                    break;

                case var t when (String.IsNullOrWhiteSpace(t.Name)):
                    // Default namespace
                    content = new XElement("include", new XAttribute("item", "defaultNamespace"));
                    break;

                default:
                    // Namespaces and other members just use the name
                    sb.Append(this.ApiMember.Name);
                    break;
            }

            return (content != null) ? (XNode)content : new XText(sb.ToString());
        }

        /// <summary>
        /// Get the short type/member name for an API topic decorated with language-specific text.  This is used
        /// for page title.
        /// </summary>
        /// <returns>An enumerable list of XML elements representing the current topic's type/member name</returns>
        protected abstract IEnumerable<XNode> ApiTopicShortNameDecorated();

        /// <summary>
        /// Get parameter and return types for an operator API member topic (e.g. <c>Int32</c> to <c>Decimal</c>)
        /// </summary>
        /// <param name="plainText">True if it should be in plain text (metadata and table of contents title) or
        /// decorated with language-specific text elements (page title).</param>
        /// <returns>An enumerable list of one or more XML nodes representing the parameter and return types</returns>
        protected virtual IEnumerable<XNode> ApiTopicOperatorTypes(bool plainText)
        {
            var parameters = this.ReferenceNode.Element("parameters").Elements();
            var returns = this.ReferenceNode.Element("returns").Elements();

            if(plainText)
            {
                var sb = new StringBuilder(1024);

                if(parameters.Count() == 1 || returns.Count() == 1)
                    sb.Append('(');

                if(parameters.Count() == 1)
                    this.ApiTypeNamePlainText(sb, parameters.First().Elements().First());

                if(parameters.Count() == 1 || returns.Count() == 1)
                    sb.Append(" to ");

                if(returns.Count() == 1)
                    this.ApiTypeNamePlainText(sb, returns.First());

                if(parameters.Count() == 1 || returns.Count() == 1)
                    sb.Append(')');

                return new[] { new XText(sb.ToString()) };
            }

            // This isn't returned, just its content
            var opsElement = new XElement("parameters");

            if(parameters.Count() == 1 || returns.Count() == 1)
                opsElement.Add("(");

            if(parameters.Count() == 1)
                this.ApiTypeNameDecorated(opsElement, parameters.First().Elements().First());

            if(parameters.Count() == 1 || returns.Count() == 1)
                opsElement.Add(" to ");

            if(returns.Count() == 1)
                this.ApiTypeNameDecorated(opsElement, returns.First());

            if(parameters.Count() == 1 || returns.Count() == 1)
                opsElement.Add(")");

            return opsElement.Nodes();
        }

        /// <summary>
        /// Get parameter types for an API member topic in plain text format
        /// </summary>
        /// <param name="plainText">True if it should be in plain text (metadata and table of contents title) or
        /// decorated with language-specific text elements (page title).</param>
        /// <returns>An enumerable list of one or more XML nodes representing the parameter types or null if
        /// there aren't any.</returns>
        protected virtual IEnumerable<XNode> ApiTopicParameterTypes(bool plainText)
        {
            var parameters = this.ReferenceNode.Element("parameters");
            bool first = true;

            if(parameters == null)
                return null;

            if(plainText)
            {
                var sb = new StringBuilder("(", 1024);

                foreach(var p in parameters.Elements("parameter"))
                {
                    if(!first)
                        sb.Append(", ");

                    this.ApiTypeNamePlainText(sb, p.Elements().First());
                    first = false;
                }

                if(this.ReferenceNode.Element("proceduredata")?.Attribute("varargs") != null)
                    sb.Append(", ...");

                sb.Append(')');

                return new[] { new XText(sb.ToString()) };
            }

            // This isn't returned, just its content
            var paramElement = new XElement("parameters", "(");

            foreach(var p in parameters.Elements("parameter"))
            {
                if(!first)
                    paramElement.Add(", ");

                this.ApiTypeNameDecorated(paramElement, p.Elements().First());
                first = false;
            }

            if(this.ReferenceNode.Element("proceduredata")?.Attribute("varargs") != null)
                paramElement.Add(", ...");

            paramElement.Add(")");

            return paramElement.Nodes();
        }

        /// <summary>
        /// This is used to get a type name in plain text based on the given type information
        /// </summary>
        /// <param name="memberName">A string builder to which the name elements are added</param>
        /// <param name="typeInfo">An element containing the type information for the reference link</param>
        protected void ApiTypeNamePlainText(StringBuilder memberName, XElement typeInfo)
        {
            if(memberName == null)
                throw new ArgumentNullException(nameof(memberName));

            if(typeInfo == null)
                throw new ArgumentNullException(nameof(typeInfo));

            var specialization = typeInfo.Element("specialization");
            var templates = typeInfo.Element("templates");
            string name = typeInfo.Attribute("name")?.Value, api = typeInfo.Attribute("api")?.Value,
                apiDataName = typeInfo.Element("apidata")?.Attribute("name")?.Value;
            bool first = true;

            switch(typeInfo.Name.LocalName)
            {
                case "reference":
                case "type":
                    if(typeInfo.Name.LocalName == "reference")
                    {
                        // Don't show the type for list pages except for overload topics
                        if(this.ApiMember.TopicGroup != ApiMemberGroup.List ||
                          this.ApiMember.TopicSubgroup == ApiMemberGroup.Overload)
                        {
                            var typeNode = typeInfo.Element("type");

                            if(typeNode == null)
                            {
                                typeNode = typeInfo.Element("containers")?.Element("type");

                                if(typeNode != null)
                                {
                                    this.ApiTypeNamePlainText(memberName, typeNode);
                                    memberName.Append('.');
                                }
                            }
                        }
                    }
                    else
                    {
                        // Add nested type name if necessary
                        var nestedType = typeInfo.Element("type") ?? typeInfo.Element("container")?.Element("type");

                        if(nestedType != null)
                        {
                            this.ApiTypeNamePlainText(memberName, nestedType);
                            memberName.Append('.');
                        }
                    }

                    // If the API element is not present (unresolved type), show the type name from the type element
                    if(!String.IsNullOrWhiteSpace(apiDataName))
                        memberName.Append(apiDataName);
                    else
                    {
                        if(api != null)
                        {
                            int pos = api.LastIndexOf('.');

                            if(pos != -1)
                                api = api.Substring(pos + 1);

                            memberName.Append(api);
                        }
                    }

                    if(specialization != null)
                        this.ApiTypeNamePlainText(memberName, specialization);
                    else
                    {
                        if(templates != null)
                            this.ApiTypeNamePlainText(memberName, templates);
                    }
                    break;

                case "specialization":
                case "templates":
                    memberName.Append('(');

                    foreach(var t in typeInfo.Elements())
                    {
                        if(!first)
                            memberName.Append(", ");

                        this.ApiTypeNamePlainText(memberName, t);

                        first = false;
                    }

                    memberName.Append(')');
                    break;

                case "template":
                    memberName.Append(name);
                    break;

                case "arrayOf":
                    this.ApiTypeNamePlainText(memberName, typeInfo.Elements().First());

                    memberName.Append('[');

                    if(Int32.TryParse(typeInfo.Attribute("rank")?.Value, out int rank) && rank > 1)
                        memberName.Append(',');

                    memberName.Append(']');
                    break;

                case "pointerTo":
                    this.ApiTypeNamePlainText(memberName, typeInfo.Elements().First());
                    memberName.Append('*');
                    break;

                case "referenceTo":
                    this.ApiTypeNamePlainText(memberName, typeInfo.Elements().First());
                    break;

                case "member":
                    this.ApiTypeNamePlainText(memberName, typeInfo.Elements().First());

                    string member = typeInfo.Element("apidata")?.Attribute("name")?.Value;

                    if(!String.IsNullOrWhiteSpace(member))
                    {
                        memberName.Append('.');
                        memberName.Append(member);
                    }
                    break;

                default:
                    Debug.WriteLine("Unhandled type element: {0}", typeInfo.Name.LocalName);

                    if(Debugger.IsAttached)
                        Debugger.Break();
                    break;
            }
        }

        /// <summary>
        /// This is used to get a type name in decorated with language-specific text elements based on the given
        /// type information.
        /// </summary>
        /// <param name="memberName">An XML element to which the name elements are added</param>
        /// <param name="typeInfo">An element containing the type information for the reference link</param>
        protected abstract void ApiTypeNameDecorated(XElement memberName, XElement typeInfo);

        /// <summary>
        /// This is used to get the type name with the template count if any (e.g. TypeName`2)
        /// </summary>
        /// <param name="typeInfo">The element containing the type information</param>
        /// <returns>The type name with the template count if any</returns>
        public static string ApiTypeNameWithTemplateCount(XElement typeInfo)
        {
            if(typeInfo == null)
                throw new ArgumentNullException(nameof(typeInfo));

            string typeName = null;

            foreach(var t in typeInfo.Elements("type").Concat(
              typeInfo.Element("containers")?.Elements("type") ?? Enumerable.Empty<XElement>()))
            {
                typeName += ApiTypeNameWithTemplateCount(t) + ".";
            }

            typeName += typeInfo.Element("apidata").Attribute("name").Value;

            int count = typeInfo.Element("templates")?.Elements("template").Count() ?? 0;

            if(count != 0)
                typeName += $"`{count}";

            return typeName;
        }

        /// <summary>
        /// This returns an enumerable list of plain text API member names with language-specific text for use
        /// in help index entries.
        /// </summary>
        /// <param name="typeInfo">The element containing the type information</param>
        /// <returns>One or more versions of the API member name.  If there is no language-specific text,
        /// a single entry is returned.  If language-specific text is present, a copy is returned for C# and
        /// another for Visual Basic.</returns>
        public IEnumerable<string> LanguageSpecificApiNames(XElement typeInfo)
        {
            StringBuilder sb = new StringBuilder(1024);

            this.ApiTypeNamePlainText(sb, typeInfo);

            string apiName = sb.ToString();

            if(apiName.IndexOf('(') == -1)
                yield return apiName;
            else
            {
                sb.Replace(",", "%2C").Replace("(", "%3C").Replace(")", "%3E");
                yield return sb.ToString();

                sb.Replace("%3C", "(Of ").Replace("%3E", ")");
                yield return sb.ToString();
            }
        }

        /// <summary>
        /// This is used to get the language ID for the given language from a syntax section or code example
        /// </summary>
        /// <param name="codeLanguage">The code language for which to get the ID</param>
        /// <returns>The language ID if it could be determined or the code language if not or it belongs to
        /// a syntax generator.</returns>
        public static string LanguageIdFor(string codeLanguage)
        {
            if(String.IsNullOrWhiteSpace(codeLanguage))
                return "none";

            string language = codeLanguage.ToLowerInvariant();

            // Languages without a syntax generator.  The presentation style content files will contain any
            // required resource items for these (i.e.devlang_HTML)
            switch(language)
            {
                // None/other.  No resource items are needed for these.
                case "none":
                case "other":
                    return language;

                case "htm":
                case "html":
                    return "HTML";

                case "bat":
                case "batch":
                    return "batch";

                case "ps1":
                case "pshell":
                case "powershell":
                    return "PShell";

                case "py":
                    return "Python";

                case "sql":
                case "sqlserver":
                case "sql server":
                    return "SQL";

                case "vbs":
                case "vbscript":
                    return "VBScript";

                case "vb-c#":
                case "visualbasicandcsharp":
                    return "VisualBasicAndCSharp";

                case "xml":
                case "xmllang":
                case "xsl":
                    return "XML";

                case "xaml":
                case "xamlusage":
                    // Special case for XAML.  It has a syntax generator but we treat the code elements
                    // differently and must use a common ID.
                    return "XAML";

                default:
                    // If none of the above, assume it is a language with a syntax generator.  The syntax
                    // generator content files will contain any required resource items for the language.
                    return codeLanguage;
            }
        }

        /// <summary>
        /// This is used to return an <c>XAttribute</c> containing the style to apply to an element
        /// </summary>
        /// <param name="style">The common style to convert to an attribute</param>
        /// <returns>The common style in the form of an XML attribute (e.g. <c>new XAttribute("class", "keyword")</c></returns>
        public abstract XAttribute StyleAttributeFor(CommonStyle style);

        /// <summary>
        /// This is used to return an style name for the given common style
        /// </summary>
        /// <param name="style">The common style to convert to an attribute</param>
        /// <returns>The common style name in the form of an string</returns>
        public abstract string StyleNameFor(CommonStyle style);

        #endregion
    }
}
