//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : CopyrightElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/22/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle copyright elements
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

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements;

/// <summary>
/// This handles <c>copyright</c> elements
/// </summary>
public class CopyrightElement : Element
{
    /// <inheritdoc />
    public CopyrightElement() : base("copyright", true)
    {
    }

    /// <inheritdoc />
    public override void Render(TopicTransformationCore transformation, XElement element)
    {
        if(transformation == null)
            throw new ArgumentNullException(nameof(transformation));

        if(element == null)
            throw new ArgumentNullException(nameof(element));

        string years = String.Join(", ", element.Elements(Ddue + "year").Select(y => y.Value.NormalizeWhiteSpace())),
            holders = String.Join(", ", element.Elements(Ddue + "holder").Select(y => y.Value.NormalizeWhiteSpace()));

        if(!String.IsNullOrWhiteSpace(years))
            years = " " + years;

        if(!String.IsNullOrWhiteSpace(holders))
            holders = " " + holders;

        transformation.CurrentElement.Add(new XElement("p",
            new XElement("include",
                new XAttribute("item", "boilerplate_copyrightNotice"),
            new XElement("parameter", element.Element(Ddue + "trademark")?.Value.NormalizeWhiteSpace()),
            new XElement("parameter", years),
            new XElement("parameter", holders))));
    }
}
