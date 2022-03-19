//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : NoteElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/19/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle note and alert elements
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/26/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html
{
    /// <summary>
    /// This is used to handle <c>note</c> and <c>alert</c> elements in a topic
    /// </summary>
    public class NoteElement : Element
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the note/alert style
        /// </summary>
        /// <value>The default if not set explicitly is "alert"</value>
        public string NoteAlertStyle { get; set; } = "alert";

        #endregion

        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public NoteElement(string name) : base(name)
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

            // XML comments note elements use type and MAML alert elements use class
            string noteType = element.Attribute("type")?.Value ?? element.Attribute("class")?.Value,
                title, altTitle, image;

            switch(noteType)
            {
                case "tip":
                    title = "alert_title_tip";
                    altTitle = "alert_altText_tip";
                    image = "AlertNote.png";
                    break;

                case "caution":
                case "warning":
                    title = "alert_title_caution";
                    altTitle = "alert_altText_caution";
                    image = "AlertCaution.png";
                    break;

                case "security":
                case "security note":
                    title = "alert_title_security";
                    altTitle = "alert_altText_security";
                    image = "AlertSecurity.png";
                    break;

                case "important":
                    title = "alert_title_important";
                    altTitle = "alert_altText_important";
                    image = "AlertCaution.png";
                    break;

                case "vb":
                case "VB":
                case "VisualBasic":
                case "visual basic note":
                    title = "alert_title_visualBasic";
                    altTitle = "alert_altText_visualBasic";
                    image = "AlertNote.png";
                    break;

                case "cs":
                case "c#":
                case "C#":
                case "CSharp":
                case "visual c# note":
                    title = "alert_title_visualC#";
                    altTitle = "alert_altText_visualC#";
                    image = "AlertNote.png";
                    break;

                case "cpp":
                case "c++":
                case "C++":
                case "CPP":
                case "visual c++ note":
                    title = "alert_title_visualC++";
                    altTitle = "alert_altText_visualC++";
                    image = "AlertNote.png";
                    break;

                case "JSharp":
                case "j#":
                case "J#":
                case "visual j# note":
                    title = "alert_title_visualJ#";
                    altTitle = "alert_altText_visualJ#";
                    image = "AlertNote.png";
                    break;

                case "implement":
                    title = "text_NotesForImplementers";
                    altTitle = "alert_altText_note";
                    image = "AlertNote.png";
                    break;

                case "caller":
                    title = "text_NotesForCallers";
                    altTitle = "alert_altText_note";
                    image = "AlertNote.png";
                    break;

                case "inherit":
                    title = "text_NotesForInheritors";
                    altTitle = "alert_altText_note";
                    image = "AlertNote.png";
                    break;

                default:
                    title = "alert_title_note";
                    altTitle = "alert_altText_note";
                    image = "AlertNote.png";
                    break;
            }

            var contentCell = new XElement("td");
            transformation.RenderChildElements(contentCell, element.Nodes());

            var note = new XElement("div", new XAttribute("class", this.NoteAlertStyle),
                new XElement("table",
                    new XElement("tr",
                        new XElement("th",
                            new XElement("img",
                                new XAttribute("src", transformation.IconPath + image),
                                new XElement("includeAttribute",
                                    new XAttribute("name", "alt"),
                                    new XAttribute("item", altTitle))),
                            NonBreakingSpace,
                            new XElement("include",
                                new XAttribute("item", title)))),
                    new XElement("tr", contentCell)));

            transformation.CurrentElement.Add(note);
        }
        #endregion
    }
}
