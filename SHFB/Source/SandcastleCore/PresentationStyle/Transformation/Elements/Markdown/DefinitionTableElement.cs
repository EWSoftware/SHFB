//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : DefinitionTableElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/30/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle MAML definition table elements for conversion to markdown topics
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/26/2025  EFW  Created the code
//===============================================================================================================

using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Markdown;

/// <summary>
/// This handles <c>list</c> elements based on the topic type
/// </summary>
public class DefinitionTableElement : Element
{
    #region Constructor
    //=====================================================================

    /// <inheritdoc />
    public DefinitionTableElement() : base("definitionTable", true)
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

        // Definition tables can be complicated as they may contain more than simple paragraphs.  As long as
        // its not nested within another element and each defined term and definition only contains text or
        // paragraphs with text and inline elements, we'll do our best to convert it to Markdown.  If not,
        // pass it through as is.
        var listItems = element.Elements().ToList();

        if(transformation.CurrentElement.Parent == null && 
          !listItems.All(li => !li.Nodes().All(n => n is XText || n is XComment ||
          (n is XElement e && e.Name.LocalName == "para"))))
        {
            transformation.CurrentElement.Add("\n");

            foreach(var li in listItems)
            {
                if(li.Name.LocalName != "definition")
                {
                    transformation.RenderChildElements(transformation.CurrentElement, li.Nodes());
                    transformation.CurrentElement.Add("\n");
                    continue;
                }

                // This is a bit tricky as we have to prefix the first line with ":   " and each subsequent line
                // with four spaces.  So, render it to a temporary element and then copy all of the text out
                // prefixing as we go.
                var dd = new XElement(this.Name);

                transformation.RenderChildElements(dd, li.Nodes());

                // First move the content of paragraphs into the parent element.
                foreach(var para in dd.Elements().Where(e => e.Name.LocalName == "para").ToList())
                {
                    var priorNode = para.PreviousNode;

                    foreach(var node in para.Nodes().ToList())
                    {
                        node.Remove();
                        para.AddAfterSelf(node);
                    }

                    para.Remove();
                }

                // Prefix each line with as needed
                StringBuilder sb = new(10240);
                bool isFirst = true;

                foreach(var b in dd.Nodes())
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
                                sb.Insert(idx, "    ");
                            else
                            {
                                sb.Insert(idx, ":   ");
                                isFirst = false;
                            }

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
                foreach(var n in dd.Nodes().ToList())
                {
                    n.Remove();
                    transformation.CurrentElement.Add(n);
                }

                transformation.CurrentElement.Add("\n");
            }
        }
        else
        {
            var lastNode = transformation.CurrentElement.LastNode;

            if(lastNode is XText t && !t.Value.EndsWith("\n\n", StringComparison.Ordinal))
                transformation.CurrentElement.Add("\n");

            var el = new XElement(this.Name);

            foreach(var attr in element.Attributes())
                el.Add(new XAttribute(attr.Name.LocalName, attr.Value));

            transformation.CurrentElement.Add(el);
            transformation.RenderChildElements(el, element.Nodes());

            if(transformation.CurrentElement.LastNode is not XText lastNode2 ||
                !lastNode2.Value.EndsWith("\n", StringComparison.Ordinal))
            {
                transformation.CurrentElement.Add("\n");
            }
        }
    }
    #endregion
}
