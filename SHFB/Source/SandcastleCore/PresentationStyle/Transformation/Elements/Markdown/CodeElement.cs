//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : CodeElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/28/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle code elements in markdown presentation styles
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/28/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Markdown
{
    /// <summary>
    /// This is used to handle <c>code</c> and <c>snippet</c> elements in a topic for markdown presentation
    /// styles.
    /// </summary>
    public class CodeElement : Element
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The element name</param>
        public CodeElement(string name) : base(name)
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

            string language = element.Attribute("language")?.Value, title = element.Attribute("title")?.Value;

            transformation.CurrentElement.Add("\n\n");

            if(!String.IsNullOrWhiteSpace(title) || (title == null && !String.IsNullOrWhiteSpace(language) &&
              !language.Equals("other", StringComparison.OrdinalIgnoreCase) &&
              !language.Equals("none", StringComparison.OrdinalIgnoreCase)))
            {
                XNode content;

                if(title != null)
                    content = new XText(title);
                else
                {
                    content = new XElement("include",
                        new XAttribute("item", $"devlang_{language}"),
                        new XAttribute("undefined", language));
                }

                transformation.CurrentElement.Add("**", content, "**  \n");
            }

            transformation.CurrentElement.Add("```");

            if(!String.IsNullOrWhiteSpace(language) &&
                !language.Equals("other", StringComparison.OrdinalIgnoreCase) &&
                !language.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                transformation.CurrentElement.Add(" ", new XElement("include",
                    new XAttribute("item", $"devlang_{language}"),
                    new XAttribute("undefined", language)), "\n");
            }

            transformation.RenderChildElements(transformation.CurrentElement, element.Nodes());
            transformation.CurrentElement.Add("\n```\n");
        }
        #endregion
    }
}
