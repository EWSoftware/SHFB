//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : NoteElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/05/2022
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

// Ignore Spelling: todo

using System;
using System.Linq;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements
{
    /// <summary>
    /// This is used to handle <c>note</c> and <c>alert</c> elements in a topic
    /// </summary>
    public class NoteElement : Element
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the Note alert template file path
        /// </summary>
        /// <value>If not set by the owning transformation or something else, the element will try to resolve
        /// the default path on first use.</value>
        public string NoteAlertTemplatePath { get; set; }

        /// <summary>
        /// This is used to get or set the Caution alert template file path
        /// </summary>
        /// <value>This must be set by the owning transformation</value>
        /// <value>If not set by the owning transformation or something else, the element will try to resolve
        /// the default path on first use.</value>
        public string CautionAlertTemplatePath { get; set; }

        /// <summary>
        /// This is used to get or set the Security alert template file path
        /// </summary>
        /// <value>This must be set by the owning transformation</value>
        /// <value>If not set by the owning transformation or something else, the element will try to resolve
        /// the default path on first use.</value>
        public string SecurityAlertTemplatePath { get; set; }

        /// <summary>
        /// This is used to get or set the Language alert template file path
        /// </summary>
        /// <value>This must be set by the owning transformation</value>
        /// <value>If not set by the owning transformation or something else, the element will try to resolve
        /// the default path on first use.</value>
        public string LanguageAlertTemplatePath { get; set; }

        /// <summary>
        /// This is used to get or set the To Do alert template file path
        /// </summary>
        /// <value>This must be set by the owning transformation</value>
        /// <value>If not set by the owning transformation or something else, the element will try to resolve
        /// the default path on first use.</value>
        public string ToDoAlertTemplatePath { get; set; }

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

            // Resolve unset paths on first use
            if(String.IsNullOrWhiteSpace(this.NoteAlertTemplatePath))
                this.NoteAlertTemplatePath = transformation.ResolvePath(@"Templates\NoteAlertTemplate.html");

            if(String.IsNullOrWhiteSpace(this.CautionAlertTemplatePath))
                this.CautionAlertTemplatePath = transformation.ResolvePath(@"Templates\CautionAlertTemplate.html");

            if(String.IsNullOrWhiteSpace(this.SecurityAlertTemplatePath))
                this.SecurityAlertTemplatePath = transformation.ResolvePath(@"Templates\SecurityAlertTemplate.html");

            if(String.IsNullOrWhiteSpace(this.LanguageAlertTemplatePath))
                this.LanguageAlertTemplatePath = transformation.ResolvePath(@"Templates\LanguageAlertTemplate.html");

            if(String.IsNullOrWhiteSpace(this.ToDoAlertTemplatePath))
                this.ToDoAlertTemplatePath = transformation.ResolvePath(@"Templates\ToDoAlertTemplate.html");

            // XML comments note elements use type and MAML alert elements use class
            string noteType = element.Attribute("type")?.Value ?? element.Attribute("class")?.Value,
                title, altTitle, template, userDefinedTitle = element.Attribute("title")?.Value;

            switch(noteType)
            {
                case "tip":
                    title = "alert_title_tip";
                    altTitle = "alert_altText_tip";
                    template = this.NoteAlertTemplatePath;
                    break;

                case "caution":
                case "warning":
                    title = "alert_title_caution";
                    altTitle = "alert_altText_caution";
                    template = this.CautionAlertTemplatePath;
                    break;

                case "security":
                case "security note":
                    title = "alert_title_security";
                    altTitle = "alert_altText_security";
                    template = this.SecurityAlertTemplatePath;
                    break;

                case "important":
                    title = "alert_title_important";
                    altTitle = "alert_altText_important";
                    template = this.CautionAlertTemplatePath;
                    break;

                case "vb":
                case "VB":
                case "VisualBasic":
                case "visual basic note":
                    title = "alert_title_visualBasic";
                    altTitle = "alert_altText_visualBasic";
                    template = this.LanguageAlertTemplatePath;
                    break;

                case "cs":
                case "c#":
                case "C#":
                case "CSharp":
                case "visual c# note":
                    title = "alert_title_visualC#";
                    altTitle = "alert_altText_visualC#";
                    template = this.LanguageAlertTemplatePath;
                    break;

                case "cpp":
                case "c++":
                case "C++":
                case "CPP":
                case "visual c++ note":
                    title = "alert_title_visualC++";
                    altTitle = "alert_altText_visualC++";
                    template = this.LanguageAlertTemplatePath;
                    break;

                case "JSharp":
                case "j#":
                case "J#":
                case "visual j# note":
                    title = "alert_title_visualJ#";
                    altTitle = "alert_altText_visualJ#";
                    template = this.LanguageAlertTemplatePath;
                    break;

                case "implement":
                    title = "text_NotesForImplementers";
                    altTitle = "alert_altText_note";
                    template = this.NoteAlertTemplatePath;
                    break;

                case "caller":
                    title = "text_NotesForCallers";
                    altTitle = "alert_altText_note";
                    template = this.NoteAlertTemplatePath;
                    break;

                case "inherit":
                    title = "text_NotesForInheritors";
                    altTitle = "alert_altText_note";
                    template = this.NoteAlertTemplatePath;
                    break;

                case "todo":
                    title = "alert_title_todo";
                    altTitle = "alert_altText_todo";
                    template = this.ToDoAlertTemplatePath;
                    break;

                default:
                    title = "alert_title_note";
                    altTitle = "alert_altText_note";
                    template = this.NoteAlertTemplatePath;
                    break;
            }

            var noteTemplate = TopicTransformationCore.LoadTemplateFile(template, new[]
            {
                ("{@IconPath}", transformation.IconPath),
                ("{@AlertAltTextItemId}", altTitle),
                ("{@AlertTitleItemId}", title)
            }).Root;

            if(!String.IsNullOrWhiteSpace(userDefinedTitle))
            {
                var titleInclude = noteTemplate.Descendants("include").FirstOrDefault(
                    i => i.Attribute("item").Value == title);

                if(titleInclude != null)
                    titleInclude.ReplaceWith(userDefinedTitle);
            }

            var contentCell = noteTemplate.Descendants().FirstOrDefault(d => d.Attribute("id")?.Value == "content");

            if(contentCell == null)
                throw new InvalidOperationException("Unable to locate the 'content' element in the note template");

            contentCell.Attribute("id").Remove();

            transformation.RenderChildElements(contentCell, element.Nodes());

            if(transformation.SupportedFormats != HelpFileFormats.OpenXml &&
              transformation.SupportedFormats != HelpFileFormats.Markdown)
            {
                transformation.CurrentElement.Add(noteTemplate);
            }
            else
            {
                foreach(var n in noteTemplate.Nodes())
                    transformation.CurrentElement.Add(n);
            }
        }
        #endregion
    }
}
