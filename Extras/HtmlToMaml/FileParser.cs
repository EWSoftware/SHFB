//=============================================================================
// System  : HTML to MAML Converter
// File    : FileParse.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/07/2012
// Note    : Copyright 2008-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is used to parse info of of HTML topics
// ready for the conversion to MAML.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  09/12/2008  EFW  Created the code
// 1.0.0.2  08/07/2012  EFW  Incorporated various changes from Dany R
//=============================================================================

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using SandcastleBuilder.Utils;

namespace HtmlToMamlConversion
{
    /// <summary>
    /// This is used to parse an HTML file to extract the various parts for conversion to MAML
    /// </summary>
    public class FileParser
    {
        #region Private data members
        //=====================================================================

        private MSHelpAttrCollection helpAttributes;
        private MSHelpKeywordCollection helpKeywords;

        private Guid topicId;
        private string title, body, topicAbstract, revisionNumber;
        private bool tocExclude, defaultTopic, splitToc;
        private int sortOrder;

        private static Regex reTitle = new Regex(
            @"<\s*title\s*>(.*)<\s*/\s*title\s*>", RegexOptions.IgnoreCase |
            RegexOptions.Singleline);

        private static Regex reTopicId = new Regex(
            @"^\s*<meta\s+name=""id""\s+content=""(.*)""\s*/\s*>\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);  // Multiline to find begin and end markers

        private static Regex reRevisionNumber = new Regex(
            @"^\s*<\s*meta\s+name=""revisionNumber""\s+content=""(.*)""\s*/\s*>\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);  // Multiline to find begin and end markers

        private static Regex reBody = new Regex(
            @"<\s*body(Text)?[^>]*?>(?<Body>.*?)<\s*/\s*body(Text)?\s*>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static Regex reMSHelpAttr = new Regex(
            "<\\s*MSHelp:Attr\\s*Name\\s*=\\s*\"(?<Name>.*?)\"\\s*Value\\s*=" +
            "\\s*\"(?<Value>.*?)\".*?/>", RegexOptions.IgnoreCase |
            RegexOptions.Singleline);

        private static Regex reMSHelpKeyword = new Regex(
            "<\\s*MSHelp:Keyword\\s*Index\\s*=\\s*\"(?<Index>.*?)\"\\s*" +
            "Term\\s*=\\s*\"(?<Term>.*?)\".*?/>", RegexOptions.IgnoreCase |
            RegexOptions.Singleline);

        private static Regex reTocExclude = new Regex(
            @"<!--\s*@TOCExclude\s*-->", RegexOptions.IgnoreCase);

        private static Regex reIsDefaultTopic = new Regex(
            @"<!--\s*@DefaultTopic\s*-->", RegexOptions.IgnoreCase);

        private static Regex reSplitToc = new Regex(
            @"<!--\s*@SplitTOC\s*-->", RegexOptions.IgnoreCase);

        private static Regex reSortOrder = new Regex(@"<!--\s*@SortOrder\s*" +
            @"(?<SortOrder>\d{1,5})\s*-->",
            RegexOptions.IgnoreCase);

        // Regular expressions used for encoding detection and parsing
        private static Regex reXmlEncoding = new Regex(
            "^<\\?xml.*?encoding\\s*=\\s*\"(?<Encoding>.*?)\".*?\\?>");
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the body extract regular expression
        /// </summary>
        /// <exception cref="InvalidOperationException">This is thrown if the regular expression is null
        /// or empty.</exception>
        public static string BodyExtractExpression
        {
            get { return reBody.ToString(); }
            set
            {
                if(String.IsNullOrEmpty(value))
                    throw new InvalidOperationException("The expression cannot be null or empty");

                reBody = new Regex(value, RegexOptions.IgnoreCase |
                    RegexOptions.Singleline);
            }
        }

        /// <summary>
        /// This returns the topic ID parsed from the file
        /// </summary>
        public Guid TopicId
        {
            get { return topicId; }
        }

        /// <summary>
        /// This returns the revision number parsed from the file
        /// </summary>
        public string RevisionNumber
        {
            get { return revisionNumber; }
        }

        /// <summary>
        /// This returns the title parsed from the file
        /// </summary>
        public string Title
        {
            get { return title; }
        }

        /// <summary>
        /// This returns the abstract parsed from the file
        /// </summary>
        public string TopicAbstract
        {
            get { return topicAbstract; }
        }

        /// <summary>
        /// This returns the TOC exclude flag if found in the topic
        /// </summary>
        public bool TocExclude
        {
            get { return tocExclude; }
        }

        /// <summary>
        /// This returns the default topic flag if found in the topic
        /// </summary>
        public bool IsDefaultTopic
        {
            get { return defaultTopic; }
        }

        /// <summary>
        /// This returns the split TOC flag if found in the topic
        /// </summary>
        public bool SplitToc
        {
            get { return splitToc; }
        }

        /// <summary>
        /// This returns the sort order value if found in the topic
        /// </summary>
        public int SortOrder
        {
            get { return sortOrder; }
        }

        /// <summary>
        /// This returns the HTML body parsed from the file
        /// </summary>
        public string Body
        {
            get { return body; }
        }

        /// <summary>
        /// This returns the help attributes parsed from the file
        /// </summary>
        public MSHelpAttrCollection HelpAttributes
        {
            get { return helpAttributes; }
        }

        /// <summary>
        /// This returns the help keywords parsed from the file
        /// </summary>
        public MSHelpKeywordCollection HelpKeywords
        {
            get { return helpKeywords; }
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// Parse the specified HTML file
        /// </summary>
        /// <param name="filename">The file to parse</param>
        /// <remarks>After parsing, the properties can be used to retrieve the information parsed from
        /// the file.</remarks>
        public void ParseFile(string filename)
        {
            Match m;
            Encoding enc = Encoding.Default;
            string content = ReadWithEncoding(filename, ref enc);

            helpAttributes = new MSHelpAttrCollection();
            helpKeywords = new MSHelpKeywordCollection();
            topicId = Guid.Empty;
            title = body = topicAbstract = null;
            revisionNumber = "1";
            tocExclude = defaultTopic = splitToc = false;
            sortOrder = Int32.MaxValue;

            m = reTopicId.Match(content);

            if(m.Success)
                Guid.TryParse(HttpUtility.HtmlDecode(m.Groups[1].Value), out topicId);

            m = reRevisionNumber.Match(content);

            if(m.Success)
                revisionNumber = HttpUtility.HtmlDecode(m.Groups[1].Value);

            m = reTitle.Match(content);

            if(m.Success)
                title = HttpUtility.HtmlDecode(m.Groups[1].Value);

            tocExclude = reTocExclude.IsMatch(content);
            defaultTopic = reIsDefaultTopic.IsMatch(content);
            splitToc = reSplitToc.IsMatch(content);

            m = reSortOrder.Match(content);

            if(m.Success)
                sortOrder = Convert.ToInt32(m.Groups["SortOrder"].Value, CultureInfo.InvariantCulture);

            m = reBody.Match(content);

            if(m.Success)
                body = m.Groups["Body"].Value;

            foreach(Match attr in reMSHelpAttr.Matches(content))
                if(attr.Groups["Name"].Value == "Abstract")
                    topicAbstract = attr.Groups["Value"].Value;
                else
                    helpAttributes.Add(attr.Groups["Name"].Value, attr.Groups["Value"].Value);

            foreach(Match keyword in reMSHelpKeyword.Matches(content))
                helpKeywords.Add(new MSHelpKeyword(keyword.Groups["Index"].Value,
                    keyword.Groups["Term"].Value));
        }

        /// <summary>
        /// This is used to read in a file using an appropriate encoding method
        /// </summary>
        /// <param name="filename">The file to load</param>
        /// <param name="encoding">Pass the default encoding to use.  On return, it contains the actual
        /// encoding for the file.</param>
        /// <returns>The contents of the file.</returns>
        /// <remarks>When reading the file, use the default encoding specified but detect the encoding if
        /// byte order marks are present.  In addition, if the template is an XML file and it contains an
        /// encoding identifier in the XML tag, the file is read using that encoding.</remarks>
        public static string ReadWithEncoding(string filename, ref Encoding encoding)
        {
            Encoding fileEnc;
            string content;

            using(StreamReader sr = new StreamReader(filename, encoding, true))
            {
                content = sr.ReadToEnd();

                // Get the actual encoding used
                encoding = sr.CurrentEncoding;
            }

            Match m = reXmlEncoding.Match(content);

            // Re-read an XML file using the correct encoding?
            if(m.Success)
            {
                fileEnc = Encoding.GetEncoding(m.Groups["Encoding"].Value);

                if(fileEnc != encoding)
                {
                    encoding = fileEnc;

                    using(StreamReader sr = new StreamReader(filename,
                      encoding, true))
                    {
                        content = sr.ReadToEnd();
                    }
                }
            }

            return content;
        }
        #endregion
    }
}
