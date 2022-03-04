//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : LanguageKeywordElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/28/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle languageKeyword elements
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/25/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html
{
    /// <summary>
    /// This handles <c>languageKeyword</c> elements
    /// </summary>
    public class LanguageKeywordElement : Element
    {
        /// <inheritdoc />
        public LanguageKeywordElement() : base("languageKeyword")
        {
        }

        /// <inheritdoc />
        public override void Render(TopicTransformationCore transformation, XElement element)
        {
            if(transformation == null)
                throw new ArgumentNullException(nameof(transformation));

            if(element == null)
                throw new ArgumentNullException(nameof(element));

            string keyword = element.Value?.NormalizeWhiteSpace();

            // If there is a slash, separate the keywords and render each one individually
            bool first = true;

            foreach(string k in keyword.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string kw = k.Trim();

                if(!String.IsNullOrWhiteSpace(kw))
                {
                    if(!first)
                        transformation.CurrentElement.Add('/');

                    var lst = transformation.LanguageSpecificTextFor(kw);

                    if(lst != null)
                        transformation.CurrentElement.Add(lst.Render());
                    else
                        transformation.CurrentElement.Add(LanguageSpecificText.RenderKeyword(keyword));

                    first = false;
                }
            }
        }
    }
}
