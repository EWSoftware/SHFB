//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : SectionElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/04/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle general section elements
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
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Sandcastle.Core.PresentationStyle.Transformation.Elements.OpenXml;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements
{
    /// <summary>
    /// This is used to handle general <c>section</c> elements in a topic
    /// </summary>
    public class SectionElement : Element
    {
        #region Private data members
        //=====================================================================

        private static readonly HashSet<XName> possibleAncestors = new HashSet<XName>
        {
            Ddue + "attributesandElements",
            Ddue + "codeExample",
            Ddue + "dotNetFrameworkEquivalent",
            Ddue + "elementInformation",
            Ddue + "exceptions",
            Ddue + "introduction",
            Ddue + "languageReferenceRemarks",
            Ddue + "nextSteps",
            Ddue + "parameters",
            Ddue + "prerequisites",
            Ddue + "procedure",
            Ddue + "relatedTopics",
            Ddue + "remarks",
            Ddue + "requirements",
            Ddue + "schemaHierarchy",
            Ddue + "syntaxSection",
            Ddue + "textValue",
            Ddue + "type",
            Ddue + "section"
        };
        #endregion

        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public SectionElement() : base("section")
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

            XElement title = null, content = null, childContent = element.Element(Ddue + "content");

            if(childContent != null && (childContent.Elements().Any() ||
              childContent.Value.NormalizeWhiteSpace().Length != 0))
            {
                string address = element.Attribute("address")?.Value;
                var titleText = element.Element(Ddue + "title")?.Value.NormalizeWhiteSpace();

                // If nested within any of these ancestor elements, render it as a subsection
                if(element.Ancestors().Any(a => possibleAncestors.Contains(a.Name)))
                {
                    if(!String.IsNullOrWhiteSpace(titleText))
                    {
                        (title, content) = transformation.CreateSubsection(false, titleText);

                        if(!String.IsNullOrWhiteSpace(address))
                        {
                            if(transformation.SupportedFormats != HelpFileFormats.OpenXml)
                                title.Add(new XAttribute("id", address));
                            else
                                OpenXmlElement.AddAddressBookmark(transformation.CurrentElement, address);
                        }
                    }
                }
                else
                {
                    (title, content) = transformation.CreateSection(element.GenerateUniqueId(), false,
                        titleText, address);
                }

                if(title != null)
                    transformation.CurrentElement.Add(title);
                
                if(content != null)
                    transformation.CurrentElement.Add(content);

                // Render this section's content and any subsections
                transformation.RenderChildElements(content ?? transformation.CurrentElement,
                    new[] { childContent }.Concat(element.Elements(Ddue + "sections")));
            }
        }
        #endregion
    }
}
