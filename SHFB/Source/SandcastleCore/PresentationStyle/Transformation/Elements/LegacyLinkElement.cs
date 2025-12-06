//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : LegacyLinkElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/21/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle legacyLink elements
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/23/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Linq;
using System.Xml.Linq;

using Sandcastle.Core.PresentationStyle.Conversion;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements;

/// <summary>
/// This handles <c>legacyLink</c> elements
/// </summary>
public class LegacyLinkElement : Element
{
    /// <inheritdoc />
    public LegacyLinkElement() : base("legacyLink")
    {
    }

    /// <inheritdoc />
    public override void Render(TopicTransformationCore transformation, XElement element)
    {
        if(transformation == null)
            throw new ArgumentNullException(nameof(transformation));

        if(element == null)
            throw new ArgumentNullException(nameof(element));

        // If converting and it has no inner text, just render the URL so that it's handled as an auto-link
        if(transformation is MarkdownConversionTransformation && !element.Nodes().Any())
            transformation.CurrentElement.Add(element.Attribute(Xlink + "href")?.Value);
        else
        {
            var link = new XElement("a",
                new XAttribute("href", element.Attribute(Xlink + "href")?.Value),
                new XAttribute("rel", "noopener noreferrer"));

            transformation.CurrentElement.Add(link);

            if(element.Nodes().Any())
                transformation.RenderChildElements(link, element.Nodes());
            else
                link.Add(element.Attribute(Xlink + "href")?.Value);
        }
    }
}
