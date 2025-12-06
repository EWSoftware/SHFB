//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : StyledParagraphElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/05/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle elements rendered as a paragraph element with a specific style in
// Open XML output.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/09/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Linq;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.OpenXml;

/// <summary>
/// This handles elements rendered as paragraphs with a specific style in Open XML output
/// </summary>
public class StyledParagraphElement : OpenXmlElement
{
    #region Properties
    //=====================================================================

    /// <summary>
    /// The paragraph style to use
    /// </summary>
    public string ParagraphStyle { get; }

    #endregion

    #region Constructor
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">The element name</param>
    /// <param name="styleName">The paragraph style to use</param>
    /// <param name="isBlockElement">True if the element is a block element, false if it is not</param>
    public StyledParagraphElement(string name, string styleName, bool isBlockElement = false) : base(name, isBlockElement)
    {
        this.ParagraphStyle = styleName;
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

        XElement content;

        // Don't apply the style if the element itself contains paragraphs.  The style will be applied by
        // the paragraph element handler.
        if(!element.Elements(Ddue + "p").Any() && !element.Elements(Ddue + "para").Any())
        {
            content = new XElement(WordProcessingML + "p",
                new XElement(WordProcessingML + "pPr",
                    new XElement(WordProcessingML + "pStyle",
                        new XAttribute(WordProcessingML + "val", this.ParagraphStyle))));

            transformation.CurrentElement.Add(content);
        }
        else
            content = transformation.CurrentElement;

        transformation.RenderChildElements(content, element.Nodes());
    }
    #endregion
}
