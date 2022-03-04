//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : NonRenderedParentElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/21/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle parent elements that do not themselves have any rendered
// representation.
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

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html
{
    /// <summary>
    /// This handles parent elements that do not themselves have any rendered representation.  It simply parses
    /// each of the child nodes in the given element if it has any and renders those as needed.  Support is
    /// included for an optional title specified through a title element or a localized include item.
    /// </summary>
    public class NonRenderedParentElement : Element
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns true if the element has a title or false if not
        /// </summary>
        public bool HasTitle { get; }

        /// <summary>
        /// This read-only property returns the localized include item name to use for the title or null
        /// if it has not title or has an optional title element.
        /// </summary>
        public string TitleIncludeItem { get; }

        /// <summary>
        /// This read-only property returns the header element name for the title
        /// </summary>
        public string HeaderElementName { get; }

        /// <summary>
        /// This read-only property returns the style name to use for the header element or null if there isn't one
        /// </summary>
        public string StyleName { get; }

        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>This element only contains child elements and has no title.  If it contains a title
        /// element, it will be ignored.</remarks>
        /// <param name="name">The element name</param>
        /// <overloads>There are two overloads for the constructor</overloads>
        public NonRenderedParentElement(string name) : base(name)
        {
        }

        /// <summary>
        /// This constructor is for non-rendered parent elements that will have a title either specified with
        /// the named localized include item or a child title element.
        /// </summary>
        /// <param name="name">The element name</param>
        /// <param name="headerLevel">The header element level for the title(1 = h1, 2 = h2, etc.)</param>
        /// <param name="styleName">The style name to use in the style attribute or null for no style</param>
        /// <param name="titleIncludeItem">The include item name to use for the localized title or null to
        /// look for an optional title element.</param>
        public NonRenderedParentElement(string name, int headerLevel, string styleName,
          string titleIncludeItem) : base(name)
        {
            this.HasTitle = true;
            this.StyleName = styleName;
            this.TitleIncludeItem = titleIncludeItem;

            if(headerLevel < 1)
                headerLevel = 1;
            else
            {
                if(headerLevel > 6)
                    headerLevel = 6;
            }

            this.HeaderElementName = "h" + (char)(headerLevel + 48);
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

            if(this.HasTitle)
            {
                XElement titleElement = null;

                if(String.IsNullOrWhiteSpace(this.TitleIncludeItem))
                {
                    string title = element.Element(Ddue + "title")?.Value.NormalizeWhiteSpace();

                    if((title?.Length ?? 0) != 0)
                        titleElement = new XElement(this.HeaderElementName, title);
                }
                else
                {
                    titleElement = new XElement(this.HeaderElementName, new XElement("include",
                        new XAttribute("item", this.TitleIncludeItem)));
                }

                if(titleElement != null)
                {
                    if(!String.IsNullOrWhiteSpace(this.StyleName))
                        titleElement.Add(new XAttribute("class", this.StyleName));

                    transformation.CurrentElement.Add(titleElement);
                }
            }

            foreach(var child in element.Nodes())
                transformation.RenderNode(child);
        }
        #endregion
    }
}
