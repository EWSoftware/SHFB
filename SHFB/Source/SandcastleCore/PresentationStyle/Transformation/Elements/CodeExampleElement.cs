//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : CodeExampleElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/28/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle codeExample elements
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
/// This is used to handle <c>codeExample</c> elements in a topic
/// </summary>
/// <remarks>The first <c>codeExample</c> element encountered will create a section and all other
/// <c>codeExample</c> elements within the parent element will be included in the section and will not be
/// processed separately.</remarks>
public class CodeExampleElement : Element
{
    #region Constructor
    //=====================================================================

    /// <inheritdoc />
    public CodeExampleElement() : base("codeExample", true)
    {
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

        if(!element.PrecedingSiblings(element.Name).Any())
        {
            var (title, content) = transformation.CreateSection(element.GenerateUniqueId(), true,
                "title_example", null, 0);

            if(title != null)
                transformation.CurrentElement.Add(title);

            if(content != null)
                transformation.CurrentElement.Add(content);
            else
                content = transformation.CurrentElement;

            transformation.RenderChildElements(content, element.Nodes());

            foreach(var otherExample in element.FollowingSiblings(element.Name))
                transformation.RenderChildElements(content, otherExample.Nodes());
        }
    }
    #endregion
}
