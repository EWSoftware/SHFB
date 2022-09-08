//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : CodeEntityReferenceElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/08/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle codeEntityReference elements
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
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements
{
    /// <summary>
    /// This handles <c>codeEntityReference</c> elements
    /// </summary>
    public class CodeEntityReferenceElement : Element
    {
        /// <inheritdoc />
        public CodeEntityReferenceElement() : base("codeEntityReference")
        {
        }

        /// <inheritdoc />
        public override void Render(TopicTransformationCore transformation, XElement element)
        {
            if(transformation == null)
                throw new ArgumentNullException(nameof(transformation));

            if(element == null)
                throw new ArgumentNullException(nameof(element));

            string linkTarget = element.Value.NormalizeWhiteSpace(),
                linkText = element.Attribute("linkText")?.Value.NormalizeWhiteSpace();
            bool qualifyHint = element.Attribute("qualifyHint").ToBoolean(),
                autoUpgrade = element.Attribute("autoUpgrade").ToBoolean();

            var link = new XElement("referenceLink",
                new XAttribute("target", linkTarget));

            if(qualifyHint)
                link.Add(new XAttribute("show-container", "true"), new XAttribute("show-parameters", "true"));

            if(autoUpgrade)
                link.Add(new XAttribute("prefer-overload", "true"));

            if(!String.IsNullOrWhiteSpace(linkText))
                link.Add(linkText);
            else
            {
                if(linkTarget.StartsWith("R:", StringComparison.Ordinal))
                    link.Add(new XElement("include", new XAttribute("item", "topicTitle_Root")));
            }

            transformation.CurrentElement.Add(link);
        }
    }
}
