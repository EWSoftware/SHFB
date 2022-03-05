﻿//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : TemplatesElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/11/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle templates elements
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

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html
{
    /// <summary>
    /// This is used to handle <c>templates</c> elements
    /// </summary>
    public class TemplatesElement : Element
    {
        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public TemplatesElement() : base("templates")
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

            var (titleElement, contentElement) = transformation.CreateSubsection(true, "title_templates");
            var content = transformation.CurrentElement;

            if(titleElement != null)
                content.Add(titleElement);

            if(contentElement != null)
            {
                content.Add(contentElement);
                content = contentElement;
            }

            var dl = new XElement("dl");

            content.Add(dl);

            foreach(var t in element.Elements("template"))
            {
                dl.Add(new XElement("dt",
                    new XElement("span",
                        transformation.StyleAttributeFor(CommonStyle.Parameter),
                        t.Attribute("name").Value)));

                var dd = new XElement("dd");
                dl.Add(dd);

                var typeParamComments = transformation.CommentsNode.Elements("typeparam").Where(
                    tpc => tpc.Attribute("name")?.Value == t.Attribute("name")?.Value).First();

                if(typeParamComments != null)
                    transformation.RenderChildElements(dd, typeParamComments.Nodes());
            }
        }
        #endregion
    }
}