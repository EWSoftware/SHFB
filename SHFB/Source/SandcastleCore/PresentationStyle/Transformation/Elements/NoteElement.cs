//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : NoteElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/30/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using Sandcastle.Core.PresentationStyle.Conversion;
using Sandcastle.Core.Project;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements;

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
    public NoteElement(string name) : base(name, true)
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

        bool convertingMarkdown = transformation is MarkdownConversionTransformation;

        // Resolve unset paths on first use
        if(!convertingMarkdown && String.IsNullOrWhiteSpace(this.NoteAlertTemplatePath))
        {
            this.NoteAlertTemplatePath = transformation.ResolvePath(
                Path.Combine("Templates", "NoteAlertTemplate.html"));
        }

        if(!convertingMarkdown && String.IsNullOrWhiteSpace(this.CautionAlertTemplatePath))
        {
            this.CautionAlertTemplatePath = transformation.ResolvePath(
                Path.Combine("Templates", "CautionAlertTemplate.html"));
        }

        if(!convertingMarkdown && String.IsNullOrWhiteSpace(this.SecurityAlertTemplatePath))
        {
            this.SecurityAlertTemplatePath = transformation.ResolvePath(
                Path.Combine("Templates", "SecurityAlertTemplate.html"));
        }

        if(!convertingMarkdown && String.IsNullOrWhiteSpace(this.LanguageAlertTemplatePath))
        {
            this.LanguageAlertTemplatePath = transformation.ResolvePath(
                Path.Combine("Templates", "LanguageAlertTemplate.html"));
        }

        if(!convertingMarkdown && String.IsNullOrWhiteSpace(this.ToDoAlertTemplatePath))
        {
            this.ToDoAlertTemplatePath = transformation.ResolvePath(
                Path.Combine("Templates", "ToDoAlertTemplate.html"));
        }

        // XML comments note elements use type and MAML alert elements use class
        string noteType = element.Attribute("type")?.Value ?? element.Attribute("class")?.Value,
            userDefinedTitle = element.Attribute("title")?.Value;

        if(!convertingMarkdown)
        {
            string title, altTitle, template;

            switch(noteType)
            {
                case "note":
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

            var noteTemplate = TopicTransformationCore.LoadTemplateFile(template,
            [
                ("{@IconPath}", transformation.IconPath),
                ("{@AlertAltTextItemId}", altTitle),
                ("{@AlertTitleItemId}", title)
            ]).Root;

            if(!String.IsNullOrWhiteSpace(userDefinedTitle))
            {
                var titleInclude = noteTemplate.Descendants("include").FirstOrDefault(
                    i => i.Attribute("item").Value == title);

                titleInclude?.ReplaceWith(userDefinedTitle);
            }

            var contentCell = noteTemplate.Descendants().FirstOrDefault(d => d.Attribute("id")?.Value == "content") ??
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
        else
        {
            // Alerts can be complicated as they may contain more than simple paragraphs.  As long as it only
            // contains text or paragraphs with text and inline elements, we'll do our best to convert it to
            // Markdown.  If not, pass it through in a MAML alert element wrapper.
            XElement firstElement = element.Nodes().FirstOrDefault() as XElement;

            if(!element.Nodes().All(n => n is XText || n is XComment ||
              (n is XElement e && e.Name.LocalName == "para")))
            {
                transformation.CurrentElement.Add("\n");

                var el = new XElement(this.Name);

                foreach(var attr in element.Attributes())
                    el.Add(new XAttribute(attr.Name.LocalName, attr.Value));

                transformation.CurrentElement.Add(el);
                transformation.RenderChildElements(el, element.Nodes());
            }
            else
            {
                transformation.CurrentElement.Add($"\n&#62; [!{noteType.ToUpperInvariant()}");

                if(!String.IsNullOrWhiteSpace(userDefinedTitle))
                    transformation.CurrentElement.Add($", {userDefinedTitle.NormalizeWhiteSpace()}");

                transformation.CurrentElement.Add("]\n&#62; ");

                // This is a bit tricky as we have to prefix each line start with "> ".  So, render it to
                // a temporary element and then copy all of the text out prefixing as we go.
                var el = new XElement(this.Name);

                transformation.RenderChildElements(el, element.Nodes());

                // First move the content of paragraphs into the parent element.
                foreach(var para in el.Elements().Where(e => e.Name.LocalName == "para").ToList())
                {
                    var priorNode = para.PreviousNode;

                    foreach(var node in para.Nodes().ToList())
                    {
                        node.Remove();
                        para.AddAfterSelf(node);
                    }

                    para.Remove();
                }

                // Prefix each line with "> ".
                StringBuilder sb = new(10240);
                bool isFirst = true;

                foreach(var b in el.Nodes())
                {
                    if(b is XText textBlock)
                    {
                        sb.Clear();
                        sb.Append(textBlock.Value);

                        int idx = 0;

                        do
                        {
                            while(idx < sb.Length && (sb[idx] == '\r' || sb[idx] == '\n'))
                            {
                                if(isFirst)
                                    sb.Remove(idx, 1);
                                else
                                {
                                    if(idx > 0 && sb[idx - 1] == '\n')
                                        break;

                                    idx++;
                                }
                            }

                            // Don't insert a prefix if there is an inline prior to the text with no line break after it
                            if(!isFirst && (textBlock.PreviousNode is not XElement || idx != 0))
                                sb.Insert(idx, "&#62; ");
                            else
                                isFirst = false;

                            while(idx < sb.Length && sb[idx] != '\r' && sb[idx] != '\n')
                                idx++;

                        } while(idx < sb.Length);

                        // Remove any trailing spaces if this is the last text node in the block
                        if(textBlock.NextNode is null)
                        {
                            idx--;

                            while(idx >= 0 && sb[idx] == ' ')
                            {
                                sb.Remove(idx, 1);
                                idx--;
                            }
                        }

                        textBlock.Value = sb.ToString();
                    }
                }

                // And finally, move the nodes to the current element.
                foreach(var n in el.Nodes().ToList())
                {
                    n.Remove();
                    transformation.CurrentElement.Add(n);
                }
            }

            transformation.CurrentElement.Add("\n");
        }
    }
    #endregion
}
