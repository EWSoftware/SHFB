//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : MarkupElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/05/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle the markup element, a parent element that does not itself have any
// rendered representation.  It just passes through clones of its child elements without any namespaces.
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
    /// This handles the <c>markup</c> element, a parent element that does not itself have any rendered
    /// representation.  It just clones the child nodes, removes any XML namespaces, and passes them through
    /// as-is.
    /// </summary>
    /// <remarks><para>This will allow build components and topic authors to add HTML or other elements such as
    /// <c>include</c> elements for localized shared content to a pre-transformed document.  This prevents it
    /// from being removed as unrecognized content by the transformations.</para>
    /// 
    /// <para>An optional <c>contentType</c> attribute is supported that defines the type of content (Html,
    /// OpenXml, or Markdown).  This allows rendering of content based on the content type supported by the
    /// presentation style.</para>
    /// 
    /// <para>When specified, presentation styles that only support Open XML will only render markup element
    /// content with a content type of "OpenXml".  Presentation styles that only support Markdown will only
    /// render markup element content with a content type of "Html" or "Markdown".  All others will only render
    /// the content if the type is "Html".  If the attribute is omitted, the content will be rendered regardless
    /// of the presentation style's formats whether or not they actually support it.</para></remarks>
    public class MarkupElement : Element
    {
        /// <inheritdoc />
        public MarkupElement() : base("markup")
        {
        }

        /// <inheritdoc />
        public override void Render(TopicTransformationCore transformation, XElement element)
        {
            if(transformation == null)
                throw new ArgumentNullException(nameof(transformation));

            if(element == null)
                throw new ArgumentNullException(nameof(element));

            var contentType = element.Attribute("contentType")?.Value;

            // If a content type is specified, ignore the element in unsupported formats
            if(contentType != null)
            {
                switch(transformation.SupportedFormats)
                {
                    case HelpFileFormats.OpenXml:
                        if(!contentType.Equals("OpenXml", StringComparison.OrdinalIgnoreCase))
                            return;
                        break;

                    case HelpFileFormats.Markdown:
                        if(!contentType.Equals("Markdown", StringComparison.OrdinalIgnoreCase) &&
                           !contentType.Equals("Html", StringComparison.OrdinalIgnoreCase))
                        {
                            return;
                        }
                        break;

                    default:
                        if(!contentType.Equals("Html", StringComparison.OrdinalIgnoreCase))
                            return;
                        break;
                }
            }

            XElement clone = new XElement(element);

            clone.RemoveNamespaces();

            foreach(var child in clone.Nodes())
                transformation.CurrentElement.Add(child);
        }
    }
}
