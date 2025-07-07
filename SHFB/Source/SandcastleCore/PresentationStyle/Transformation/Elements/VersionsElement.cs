//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : NamedSectionElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle versions elements
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/13/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Linq;
using System.Xml.Linq;

using Sandcastle.Core.PresentationStyle.Transformation.Elements.OpenXml;
using Sandcastle.Core.Project;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements
{
    /// <summary>
    /// This is used to handle <c>versions</c> elements
    /// </summary>
    public class VersionsElement : Element
    {
        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public VersionsElement() : base("versions")
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

            if(element.Elements().Any())
            {
                var (title, contentElement) = transformation.CreateSubsection(true, "title_versions");
                var content = transformation.CurrentElement;

                if(title != null)
                    content.Add(title);

                if(contentElement != null)
                {
                    content.Add(contentElement);
                    content = contentElement;
                }

                foreach(var v in element.Elements("versions"))
                {
                    if(v.HasElements)
                    {
                        switch(transformation.SupportedFormats)
                        {
                            case HelpFileFormats.OpenXml:
                                content.Add(new XElement(OpenXmlElement.WordProcessingML + "p",
                                    new XElement(OpenXmlElement.WordProcessingML + "pPr",
                                        new XElement(OpenXmlElement.WordProcessingML + "pStyle",
                                            new XAttribute(OpenXmlElement.WordProcessingML + "val", "Heading4"))),
                                    new XElement(OpenXmlElement.WordProcessingML + "r",
                                        new XElement(OpenXmlElement.WordProcessingML + "t",
                                            new XElement("include",
                                                new XAttribute("item", v.Attribute("name").Value))))));
                                break;

                            case HelpFileFormats.Markdown:
                                content.Add("#### ", new XElement("include",
                                        new XAttribute("item", v.Attribute("name").Value)), "  \n");
                                break;

                            default:
                                content.Add(new XElement("h4",
                                    new XElement("include",
                                        new XAttribute("item", v.Attribute("name").Value))));
                                break;
                        }

                        RenderVersionInfo(v, content, transformation.SupportedFormats == HelpFileFormats.Markdown);
                    }
                }
            }
        }

        /// <summary>
        /// This is used to render the information for each version entry
        /// </summary>
        /// <param name="version">The version element</param>
        /// <param name="content">The content element to which the information is added</param>
        /// <param name="isMarkdown">True if rendering for markdown, false if not</param>
        private static void RenderVersionInfo(XElement version, XElement content, bool isMarkdown)
        {
            // Show the versions in which the api is supported, if any
            var notObsolete = version.Elements().Where(v => v.Attribute("obsolete") == null).ToList();

            if(notObsolete.Count != 0)
            {
                var include = new XElement("include", new XAttribute("item", $"supportedIn_{notObsolete.Count}"));
                content.Add(include);

                foreach(var v in notObsolete)
                {
                    include.Add(new XElement("parameter",
                        new XElement("include",
                            new XAttribute("item", v.Attribute("name").Value))));
                }

                if(isMarkdown)
                    content.Add("  \n");
                else
                    content.Add(new XElement("br"));
            }

            // Show the versions in which the API is obsolete with a compiler warning, if any
            foreach(var v in version.Elements().Where(v => v.Attribute("obsolete")?.Value == "warning"))
            {
                content.Add(new XElement("include",
                    new XAttribute("item", "obsoleteWarning"),
                    new XElement("parameter",
                        new XElement("include",
                            new XAttribute("item", v.Attribute("name").Value)))));

                if(isMarkdown)
                    content.Add("  \n");
                else
                    content.Add(new XElement("br"));
            }

            // Show the version in which the API is obsolete and does not compile, if any
            var obsoleteError = version.Elements().Where(v => v.Attribute("obsolete")?.Value == "error").LastOrDefault();

            if(obsoleteError != null)
            {
                content.Add(new XElement("include",
                    new XAttribute("item", "obsoleteError"),
                    new XElement("parameter",
                        new XElement("include",
                            new XAttribute("item", obsoleteError.Attribute("name").Value)))));

                if(isMarkdown)
                    content.Add("  \n");
                else
                    content.Add(new XElement("br"));
            }
        }
        #endregion
    }
}
