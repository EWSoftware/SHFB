//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ConvertibleElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/23/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains a class that handles elements that are converted to a different element name and have an
// optional style attribute.
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
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements
{
    /// <summary>
    /// This handles elements that are converted to a different element name and have an optional style
    /// attribute.
    /// </summary>
    public class ConvertibleElement : Element
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the element name that will be rendered in the topic
        /// </summary>
        public string RenderedName { get; }

        /// <summary>
        /// This read-only property returns the style name if specified
        /// </summary>
        /// <value>If null, no style attribute will be rendered</value>
        public string StyleName { get; }

        /// <summary>
        /// This read-only property returns true if the element is addressable (it can have an address attribute
        /// that serves as a link target).
        /// </summary>
        public bool Addressable { get; }

        /// <summary>
        /// This read-only property returns the name of the attribute value from which to get the value for the
        /// converted element's content.  If null, the elements nodes are used instead.
        /// </summary>
        public string ValueAttributeName { get; }

        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The element name</param>
        /// <param name="renderedElement">The element name to use in the rendered topic</param>
        /// <overloads>There are three overloads for the constructor</overloads>
        public ConvertibleElement(string name, string renderedElement) : base(name)
        {
            this.RenderedName = renderedElement;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The element name</param>
        /// <param name="renderedElement">The element name to use in the rendered topic</param>
        /// <param name="addressable">True if it can have an address attribute, false if not</param>
        /// <overloads>There are three overloads for the constructor</overloads>
        public ConvertibleElement(string name, string renderedElement, bool addressable) : base(name)
        {
            this.RenderedName = renderedElement;
            this.Addressable = addressable;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The element name</param>
        /// <param name="renderedElement">The element name to use in the rendered topic</param>
        /// <param name="styleName">The style name to use in the style attribute</param>
        /// <overloads>There are two overloads for the constructor</overloads>
        public ConvertibleElement(string name, string renderedElement, string styleName) : this(name, renderedElement)
        {
            this.StyleName = styleName;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The element name</param>
        /// <param name="valueAttributeName">The attribute name from which to get the value to render in the content</param>
        /// <param name="renderedElement">The element name to use in the rendered topic</param>
        /// <param name="styleName">The style name to use in the style attribute</param>
        /// <overloads>There are two overloads for the constructor</overloads>
        public ConvertibleElement(string name, string valueAttributeName, string renderedElement,
          string styleName) : this(name, renderedElement)
        {
            this.StyleName = styleName;
            this.ValueAttributeName = valueAttributeName;
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

            var el = new XElement(this.RenderedName);

            if(!String.IsNullOrWhiteSpace(this.StyleName))
                el.Add(new XAttribute("class", this.StyleName));

            if(this.Addressable)
            {
                string address = element.Attribute("address")?.Value;

                if(!String.IsNullOrWhiteSpace(address))
                {
                    switch(transformation.SupportedFormats)
                    {
                        case HelpFileFormats.OpenXml:
                            OpenXml.OpenXmlElement.AddAddressBookmark(transformation.CurrentElement, address);
                            break;

                        case HelpFileFormats.Markdown:
                            Markdown.MarkdownElement.AddAddressBookmark(transformation.CurrentElement, address);
                            break;

                        default:
                            el.Add(new XAttribute("id", address));
                            break;
                    }
                }
            }

            transformation.CurrentElement.Add(el);

            if(String.IsNullOrWhiteSpace(this.ValueAttributeName))
                transformation.RenderChildElements(el, element.Nodes());
            else
                el.Add(element.Attribute(this.ValueAttributeName)?.Value);
        }
        #endregion
    }
}
