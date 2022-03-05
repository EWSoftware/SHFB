//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : SourceContextElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/11/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle the sourceContext element
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
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html
{
    /// <summary>
    /// This handles the <c>sourceContext</c> element in a syntax section
    /// </summary>
    public class SourceContextElement : Element
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the Request Example URL transformation argument name
        /// </summary>
        public string RequestExampleUrlArgName { get; }

        /// <summary>
        /// This read-only property returns the base source code URL transformation argument name
        /// </summary>
        public string BaseSourceCodeUrlArgName { get; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requestExampleUrlArgName">The Request Example URL transformation argument name to use or
        /// null if there isn't one.</param>
        /// <param name="baseSourceCodeUrlArgName">The base source code URL transformation argument name to use
        /// or null if there isn't one.</param>
        public SourceContextElement(string requestExampleUrlArgName, string baseSourceCodeUrlArgName) : base("sourceContext")
        {
            this.RequestExampleUrlArgName = requestExampleUrlArgName;
            this.BaseSourceCodeUrlArgName = baseSourceCodeUrlArgName;
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

            string requestExampleUrl = null, baseSourceCodeUrl = null;

            if(!String.IsNullOrWhiteSpace(this.RequestExampleUrlArgName))
                requestExampleUrl = transformation.TransformationArguments[this.RequestExampleUrlArgName].Value;

            if(!String.IsNullOrWhiteSpace(this.BaseSourceCodeUrlArgName))
                baseSourceCodeUrl = transformation.TransformationArguments[this.BaseSourceCodeUrlArgName].Value;

            if(!String.IsNullOrWhiteSpace(requestExampleUrl))
            {
                transformation.CurrentElement.Add(new XElement("include",
                    new XAttribute("item", "requestExample"),
                    new XElement("parameter", requestExampleUrl)));
            }

            if(!String.IsNullOrWhiteSpace(baseSourceCodeUrl))
            {
                string file = baseSourceCodeUrl + element.Attribute("file").Value,
                    lineNumber = element.Attribute("startLine")?.Value;

                if(!String.IsNullOrWhiteSpace(lineNumber))
                    file += "#L" + lineNumber;

                transformation.CurrentElement.Add(new XElement("a",
                        new XAttribute("target", "_blank"),
                        transformation.StyleAttributeFor(CommonStyle.Button),
                        new XAttribute("href", file),
                        new XAttribute("rel", "noopener noreferrer"),
                    new XElement("includeAttribute",
                        new XAttribute("name", "title"),
                        new XAttribute("item", "sourceCodeLinkTitle")),
                    new XElement("include",
                        new XAttribute("item", "sourceCodeLinkText"))));
            }
        }
        #endregion
    }
}
