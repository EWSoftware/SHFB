﻿//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : TableElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/23/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle the table element based on the topic type
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/14/2022  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: hoverable

using System;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html
{
    /// <summary>
    /// This handles the <c>table</c> element based on the topic type
    /// </summary>
    public class TableElement : Element
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the table caption style
        /// </summary>
        /// <value>The default if not set explicitly is <c>caption</c></value>
        public string TableCaptionStyle { get; set; } = "caption";

        /// <summary>
        /// This is used to get or set the overall table style
        /// </summary>
        /// <value>The default if not set explicitly is <c>table is-hoverable</c></value>
        public string TableStyle { get; set; } = "table is-hoverable";

        #endregion

        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public TableElement() : base("table")
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

            var table = new XElement(this.Name);

            if(!String.IsNullOrWhiteSpace(this.TableStyle) && transformation.SupportedFormats != HelpFileFormats.Markdown)
                table.Add(new XAttribute("class", this.TableStyle));

            if(transformation.IsMamlTopic)
            {
                string title = element.Element(Ddue + "title")?.Value.NormalizeWhiteSpace();

                if((title?.Length ?? 0) != 0)
                {
                    if(transformation.SupportedFormats != HelpFileFormats.Markdown)
                    {
                        transformation.CurrentElement.Add(new XElement("div",
                            new XAttribute("class", this.TableCaptionStyle), title));
                    }
                    else
                    {
                        transformation.CurrentElement.Add(new XElement("p",
                            new XElement("strong", title)));
                    }
                }
            }
            else
            {
                foreach(var attr in element.Attributes())
                {
                    // Don't override explicit styling
                    if(attr.Name.LocalName != "class" || String.IsNullOrWhiteSpace(this.TableStyle))
                        table.Add(new XAttribute(attr.Name.LocalName, attr.Value));
                }
            }

            transformation.CurrentElement.Add(table);
            transformation.RenderChildElements(table, element.Nodes());
        }
        #endregion
    }
}
