//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ImplementsElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle the implements
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/11/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Linq;
using System.Xml.Linq;

using Sandcastle.Core.Project;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements
{
    /// <summary>
    /// This handles the <c>implements</c> element in a syntax section
    /// </summary>
    public class ImplementsElement : Element
    {
        /// <inheritdoc />
        public ImplementsElement() : base("implements")
        {
        }

        /// <inheritdoc />
        public override void Render(TopicTransformationCore transformation, XElement element)
        {
            if(transformation == null)
                throw new ArgumentNullException(nameof(transformation));

            if(element == null)
                throw new ArgumentNullException(nameof(element));

            var memberElements = element.Elements("member");

            if(memberElements.Any())
            {
                var (titleElement, contentElement) = transformation.CreateSubsection(true, "title_implements");
                var content = transformation.CurrentElement;

                if(titleElement != null)
                    content.Add(titleElement);

                if(contentElement != null)
                {
                    content.Add(contentElement);
                    content = contentElement;
                }

                if(transformation.SupportedFormats == HelpFileFormats.OpenXml)
                {
                    // For Open XML, the links are wrapped in a paragraph
                    var para = new XElement(OpenXml.OpenXmlElement.WordProcessingML + "p");
                    content.Add(para);
                    content = para;
                }

                foreach(var m in memberElements)
                {
                    content.Add(new XElement("referenceLink",
                            new XAttribute("target", m.Attribute("api").Value),
                            new XAttribute("qualified", "true")));

                    if(transformation.SupportedFormats != HelpFileFormats.Markdown)
                        content.Add(new XElement("br"));
                    else
                        content.Add("  \n");
                }
            }
        }
    }
}
