//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : PassthroughElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/29/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle passthrough elements by writing them out along with any attributes
// and then parsing any child nodes.
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

using Sandcastle.Core.Project;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements;

/// <summary>
/// This handles passthrough elements such as HTML elements by writing them out along with any attributes and
/// then parsing any child nodes.
/// </summary>
public class PassthroughElement : Element
{
    /// <inheritdoc />
    /// <overloads>There are two overloads for the constructor</overloads>
    public PassthroughElement(string name) : base(name)
    {
    }

    /// <inheritdoc />
    public PassthroughElement(string name, bool isBlockElement) : base(name, isBlockElement)
    {
    }

    /// <inheritdoc />
    public override void Render(TopicTransformationCore transformation, XElement element)
    {
        if(transformation == null)
            throw new ArgumentNullException(nameof(transformation));

        if(element == null)
            throw new ArgumentNullException(nameof(element));

        if(this.IsBlockElement && transformation.SupportedFormats == HelpFileFormats.Markdown)
        {
            var lastNode = transformation.CurrentElement.LastNode;

            if(lastNode == null || (lastNode is XText t && !t.Value.EndsWith("\n\n", StringComparison.Ordinal)))
                transformation.CurrentElement.Add("\n");
        }

        var el = new XElement(this.Name);

        foreach(var attr in element.Attributes())
            el.Add(new XAttribute(attr.Name.LocalName, attr.Value));

        transformation.CurrentElement.Add(el);
        transformation.RenderChildElements(el, element.Nodes());

        if(this.IsBlockElement && transformation.SupportedFormats == HelpFileFormats.Markdown)
        {
            if(transformation.CurrentElement.LastNode is not XText lastNode ||
              !lastNode.Value.EndsWith("\n", StringComparison.Ordinal))
            {
                transformation.CurrentElement.Add("\n");
            }
        }
    }
}
