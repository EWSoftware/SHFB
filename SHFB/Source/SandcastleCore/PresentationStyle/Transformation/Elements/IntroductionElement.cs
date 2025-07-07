//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : IntroductionElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle introduction elements
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

using Sandcastle.Core.PresentationStyle.Transformation.Elements.OpenXml;
using Sandcastle.Core.Project;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements
{
    /// <summary>
    /// This is used to handle <c>introduction</c> elements in a topic
    /// </summary>
    public class IntroductionElement : Element
    {
        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public IntroductionElement() : base("introduction")
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

            if(element.Elements().Any() || element.Value.NormalizeWhiteSpace().Length != 0)
            {
                XElement intro;
                string address = element.Attribute("address")?.Value;

                switch(transformation.SupportedFormats)
                {
                    case HelpFileFormats.OpenXml:
                        intro = transformation.CurrentElement;

                        if(!String.IsNullOrWhiteSpace(address))
                            OpenXmlElement.AddAddressBookmark(intro, address);
                        break;

                    case HelpFileFormats.Markdown:
                        intro = transformation.CurrentElement;

                        // Special case.  Links to this address will get converted to a link to the page header
                        // since the introduction has no title itself.
                        if(!String.IsNullOrWhiteSpace(address))
                            Markdown.MarkdownElement.AddAddressBookmark(intro, "@pageHeader_" + address);
                        break;

                    default:
                        intro = new XElement("div");

                        if(!String.IsNullOrWhiteSpace(address))
                            intro.Add(new XAttribute("id", address));

                        transformation.CurrentElement.Add(intro);
                        break;
                }

                transformation.RenderChildElements(intro, element.Nodes());
            }
        }
        #endregion
    }
}
