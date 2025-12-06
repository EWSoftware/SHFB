//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ParagraphElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/22/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle paragraph elements in Open XML output
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/04/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Linq;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.OpenXml;

/// <summary>
/// This handles paragraph elements in Open XML output
/// </summary>
/// <para>Note that unlike HTML, self-closing and empty paragraphs will be rendered in the document and will
/// consume space.  However, we can't remove them as it could then combine text into a single paragraph that
/// is not intended to be combined.  It is best to let the user sort it out later.  The fix is to wrap the
/// text in the paragraph elements and not use self-closing paragraphs.</para>
public class ParagraphElement : OpenXmlElement
{
    /// <inheritdoc />
    public ParagraphElement(string name) : base(name, true)
    {
    }

    /// <inheritdoc />
    public override void Render(TopicTransformationCore transformation, XElement element)
    {
        if(transformation == null)
            throw new ArgumentNullException(nameof(transformation));

        if(element == null)
            throw new ArgumentNullException(nameof(element));

        var para = new XElement(WordProcessingML + "p");

        // Apply styles based on the parent element if necessary
        switch(element)
        {
            case var p when p.Parent.Name.LocalName == "blockquote":
                para.Add(new XElement(WordProcessingML + "pPr",
                    new XElement(WordProcessingML + "pStyle",
                        new XAttribute(WordProcessingML + "val", "Quote"))));
                break;

            case var p when p.Parent.Name.LocalName == "definition":
                para.Add(new XElement(WordProcessingML + "pPr",
                    new XElement(WordProcessingML + "ind",
                        new XAttribute(WordProcessingML + "left", "432"))));
                break;

            case var p when p.Parent.Name.LocalName == "th" || p.Ancestors(Ddue + "tableHeader").Any():
                // In table header cells, keep them together with the next row to avoid splitting them across
                // pages.
                para.Add(new XElement(WordProcessingML + "pPr", new XElement(WordProcessingML + "keepNext")));
                break;

            default:
                break;
        }

        transformation.CurrentElement.Add(para);
        transformation.RenderChildElements(para, element.Nodes());
    }
}
