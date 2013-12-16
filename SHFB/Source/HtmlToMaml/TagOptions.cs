//=============================================================================
// System  : HTML to MAML Converter
// File    : TagOptions.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/17/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is used to contain options for tag entries
// from the conversion rules file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  09/17/2008  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Text.RegularExpressions;

namespace HtmlToMamlConversion
{
    /// <summary>
    /// This is used to contain the options and state for a <c>Tag</c> entry
    /// from the conversion rules file.
    /// </summary>
    public class TagOptions
    {
        #region Private data members
        //=====================================================================

        private XPathNodeIterator matchRules;
        private Stack<string> endTags;

        private string startTag, endTag, tagAttributes, tag, attributes, closing;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This returns the replacement tag to use based on the last
        /// evaluation.
        /// </summary>
        public string Tag
        {
            get { return tag; }
        }

        /// <summary>
        /// This returns the attributes to use based on the last evaluation
        /// </summary>
        public string Attributes
        {
            get { return attributes; }
        }

        /// <summary>
        /// This returns the closing "/" if the element is an end tag
        /// </summary>
        public string Closing
        {
            get { return closing; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tag">The tag node</param>
        public TagOptions(XPathNavigator tag)
        {
            startTag = tag.GetAttribute("startTag", String.Empty);
            endTag = tag.GetAttribute("endTag", String.Empty);
            tagAttributes = tag.GetAttribute("attributes", String.Empty);
            matchRules = tag.Select("Match");
            endTags = new Stack<string>();
        }
        #endregion

        #region Methods, etc.
        //=====================================================================

        /// <summary>
        /// Evaluate the match and determine the properties to use for the
        /// replacement.
        /// </summary>
        /// <param name="match">The regular expression match to evaluate</param>
        public void Evaluate(Match match)
        {
            Regex reRule;
            string tagAttrs;

            closing = match.Groups["Closing"].Value;
            tagAttrs = match.Groups["Attributes"].Value;

            tag = startTag;
            attributes = tagAttributes;

            // If it's a closing tag, process it
            if(!String.IsNullOrEmpty(closing))
            {
                attributes = String.Empty;

                if(endTags.Count != 0)
                    tag = endTags.Pop();
                else
                    if(!String.IsNullOrEmpty(endTag))
                        tag = endTag;

                return;
            }

            // It's an opening or self-closing tag.  Check for a match rule.
            if(matchRules.Count != 0)
                foreach(XPathNavigator rule in matchRules)
                {
                    reRule = new Regex(rule.GetAttribute("expression",
                        String.Empty), RegexOptions.IgnoreCase |
                        RegexOptions.Singleline);

                    if(reRule.IsMatch(match.Value))
                    {
                        tag = rule.GetAttribute("startTag", String.Empty);
                        attributes = rule.GetAttribute("attributes", String.Empty);

                        if(!String.IsNullOrEmpty(rule.GetAttribute("endTag", String.Empty)))
                            endTags.Push(rule.GetAttribute("endTag", String.Empty));
                        else
                            endTags.Push(tag);

                        break;
                    }
                }

            if(attributes == "@Preserve")
            {
                if(!String.IsNullOrEmpty(tagAttrs))
                    attributes = " " + tagAttrs.Trim();
                else
                    attributes = String.Empty;
            }
            else
                if(tagAttrs == "/")
                    attributes = tagAttrs;
                else
                    if(!String.IsNullOrEmpty(attributes))
                        attributes = " " + attributes;
        }
        #endregion
    }
}
