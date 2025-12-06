//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : MarkdownElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/29/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle rendering of elements enclosed in markdown syntax
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/04/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Markdown;

/// <summary>
/// This handles parent elements that are enclosed in markdown syntax
/// </summary>
public class MarkdownElement : Element
{
    #region Properties
    //=====================================================================

    /// <summary>
    /// This read-only property returns the name of the attribute value from which to get the value for the
    /// converted element's content.  If null, the elements nodes are used instead.
    /// </summary>
    public string ValueAttributeName { get; }

    /// <summary>
    /// This read-only property returns the prefix Markdown syntax
    /// </summary>
    public string PrefixSyntax { get; }

    /// <summary>
    /// This read-only property returns the suffix Markdown syntax
    /// </summary>
    public string SuffixSyntax { get; }

    /// <summary>
    /// This read-only property returns the fallback HTML element to use if a block parent HTML element is
    /// detected that will cause the markdown syntax to be ignored.
    /// </summary>
    public string FallbackElement { get; }

    #endregion

    #region Constructors
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">The element name</param>
    /// <param name="prefixSyntax">The markdown prefix syntax character(s) to use before the content or null
    /// if there isn't any</param>
    /// <param name="suffixSyntax">The markdown suffix syntax character(s) to use after the content or
    /// null if there isn't any</param>
    /// <param name="fallbackElement">The fallback HTML element to use if a block parent HTML element is
    /// detected that will cause the markdown syntax to be ignored.</param>
    /// <param name="isBlockElement">True if the element is a block element, false if it is not</param>
    /// <overloads>There are two overloads for the constructor</overloads>
    public MarkdownElement(string name, string prefixSyntax, string suffixSyntax,
      string fallbackElement, bool isBlockElement = false) : base(name, isBlockElement)
    {
        this.PrefixSyntax = prefixSyntax;
        this.SuffixSyntax = suffixSyntax;
        this.FallbackElement = fallbackElement;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">The element name</param>
    /// <param name="valueAttributeName">The attribute name from which to get the value to render in the content</param>
    /// <param name="prefixSyntax">The markdown prefix syntax character(s) to use before the content or null
    /// if there isn't any</param>
    /// <param name="suffixSyntax">The markdown suffix syntax character(s) to use after the content or
    /// null if there isn't any</param>
    /// <param name="fallbackElement">The fallback HTML element to use if a block parent HTML element is
    /// detected that will cause the markdown syntax to be ignored.</param>
    public MarkdownElement(string name, string valueAttributeName, string prefixSyntax, string suffixSyntax,
      string fallbackElement) : base(name)
    {
        this.ValueAttributeName = valueAttributeName;
        this.PrefixSyntax = prefixSyntax;
        this.SuffixSyntax = suffixSyntax;
        this.FallbackElement = fallbackElement;
    }
    #endregion

    #region Methods
    //=====================================================================

    /// <inheritdoc />
    public override void Render(TopicTransformationCore transformation, XElement element)
    {
        if(transformation == null)
            throw new ArgumentNullException(nameof(transformation));

        if(element == null)
            throw new ArgumentNullException(nameof(element));

        // Markdown typically doesn't get rendered if nested within another HTML element.  In some cases it does
        // but only if the opening element is not immediately followed by a new line.  If a parent element other
        // than the document is found, use the fallback HTML element instead.
        if(!String.IsNullOrWhiteSpace(this.FallbackElement) && transformation.CurrentElement.Parent != null)
        {
            var el = new XElement(this.FallbackElement);

            transformation.CurrentElement.Add(el);

            if(String.IsNullOrWhiteSpace(this.ValueAttributeName))
                transformation.RenderChildElements(el, element.Nodes());
            else
                el.Add(element.Attribute(this.ValueAttributeName)?.Value);
        }
        else
        {
            if(this.IsBlockElement && this.PrefixSyntax != "\n")
            {
                var lastNode = transformation.CurrentElement.LastNode;

                if(lastNode == null || (lastNode is XText t && !t.Value.EndsWith("\n\n", StringComparison.Ordinal)))
                    transformation.CurrentElement.Add("\n");
            }

            if(this.PrefixSyntax != null)
                transformation.CurrentElement.Add(this.PrefixSyntax);

            if(String.IsNullOrWhiteSpace(this.ValueAttributeName))
            {
                foreach(var child in element.Nodes())
                    transformation.RenderNode(child);
            }
            else
                transformation.CurrentElement.Add(element.Attribute(this.ValueAttributeName)?.Value);

            if(this.SuffixSyntax != null)
                transformation.CurrentElement.Add(this.SuffixSyntax);

            if(this.IsBlockElement)
            {
                if(transformation.CurrentElement.LastNode is not XText lastNode ||
                  !lastNode.Value.EndsWith("\n", StringComparison.Ordinal) ||
                  this.SuffixSyntax == "\n")
                {
                    transformation.CurrentElement.Add("\n");
                }
            }
        }
    }

    /// <summary>
    /// Add a bookmark for an address attribute
    /// </summary>
    /// <param name="content">The content element to which the bookmark is added</param>
    /// <param name="uniqueId">The unique ID to use for the bookmark</param>
    /// <remarks>Open XML does not support ID attributes like HTML.  Instead, it renders bookmarks with the
    /// unique IDs that will be used as the link targets.  The Open XML file builder task will reformat the
    /// bookmark name and ID to ensure that they are all unique.</remarks>
    public static void AddAddressBookmark(XElement content, string uniqueId)
    {
        if(content == null)
            throw new ArgumentNullException(nameof(content));

        content.Add(new XElement("span", new XAttribute("id", $"{uniqueId}"), String.Empty), "  \n");
    }
    #endregion
}
