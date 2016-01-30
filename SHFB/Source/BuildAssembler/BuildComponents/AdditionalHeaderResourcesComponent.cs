//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : AdditionalHeaderResourcesComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/23/2015
// Note    : Copyright 2014-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a build component that is used to add additional metadata, style sheet, and script file
// resources to the header of the transformed topics.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/02/2014  EFW  Created the code
//===============================================================================================================

using System;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Microsoft.Ddue.Tools.BuildComponent
{
    /// <summary>
    /// This component is used add additional metadata, style sheet, and script file resources to the header of
    /// the transformed topics.
    /// </summary>
    /// <remarks>The configuration consists of a set of <c>script</c> and <c>stylesheet</c> elements with a
    /// <c>name</c> attribute that specifies the file to add.  The appropriate <c>script</c> and <c>link</c>
    /// elements will be appended to the end of the <c>head</c> element.  In addition, <c>meta</c> elements can
    /// be added.  The metadata will be added to the start of the <c>head</c> element verbatim.</remarks>
    public class AdditionalHeaderResourcesComponent : BuildComponentCore
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("Additional Header Resources Component")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new AdditionalHeaderResourcesComponent(base.BuildAssembler);
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private XPathNodeIterator headerResources;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected AdditionalHeaderResourcesComponent(BuildAssemblerCore buildAssembler) : base(buildAssembler)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            headerResources = configuration.Select("meta|script|stylesheet");
        }

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            XmlElement element, lastMetaElement = null;
            var head = document.SelectSingleNode("//head");

            if(head != null)
                foreach(XPathNavigator node in headerResources)
                    switch(node.LocalName)
                    {
                        case "stylesheet":
                            element = document.CreateElement("link");
                            element.SetAttribute("rel", "stylesheet");
                            element.SetAttribute("type", "text/css");
                            element.SetAttribute("href", node.GetAttribute("name", String.Empty));

                            head.AppendChild(element);
                            break;

                        case "meta":
                            // For metadata, add all of the attributes from the source node and insert them in
                            // the order found at the start of the head element
                            element = document.CreateElement("meta");

                            var attrNav = node.Clone();

                            if(attrNav.MoveToFirstAttribute())
                            {
                                do
                                {
                                    element.SetAttribute(attrNav.Name, attrNav.Value);

                                } while(attrNav.MoveToNextAttribute());
                            }

                            if(lastMetaElement == null)
                                head.InsertBefore(element, head.FirstChild);
                            else
                                head.InsertAfter(element, lastMetaElement);

                            lastMetaElement = element;
                            break;

                        case "script":
                            element = document.CreateElement("script");
                            element.SetAttribute("type", "text/javascript");
                            element.SetAttribute("src", node.GetAttribute("name", String.Empty));

                            // Set the inner text to an empty string so that it is written out with full opening
                            // and closing elements.
                            element.InnerText = String.Empty;

                            head.AppendChild(element);
                            break;
                    }
        }
        #endregion
    }
}
