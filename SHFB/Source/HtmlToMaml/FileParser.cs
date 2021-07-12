//===============================================================================================================
// System  : HTML to MAML Converter
// File    : FileParse.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/07/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains a class that is used to parse info out of HTML topics ready for the conversion to MAML.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/12/2008  EFW  Created the code
// 08/07/2012  EFW  Incorporated various changes from Dany R
//===============================================================================================================

// Ignore Spelling: Dany xml Attr

using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

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

        private static readonly Regex reTitle = new Regex(@"<\s*title\s*>(.*)<\s*/\s*title\s*>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static readonly Regex reTopicId = new Regex(
            @"^\s*<meta\s+name=""id""\s+content=""(.*)""\s*/\s*>\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);  // Multi-line to find begin and end markers

        private static readonly Regex reRevisionNumber = new Regex(
            @"^\s*<\s*meta\s+name=""revisionNumber""\s+content=""(.*)""\s*/\s*>\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);  // Multi-line to find begin and end markers

        private static Regex reBody = new Regex(
            @"<\s*body(Text)?[^>]*?>(?<Body>.*?)<\s*/\s*body(Text)?\s*>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static readonly Regex reMSHelpAttr = new Regex(
            "<\\s*MSHelp:Attr\\s*Name\\s*=\\s*\"(?<Name>.*?)\"\\s*Value\\s*=\\s*\"(?<Value>.*?)\".*?/>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static readonly Regex reMSHelpKeyword = new Regex(
            "<\\s*MSHelp:Keyword\\s*Index\\s*=\\s*\"(?<Index>.*?)\"\\s*Term\\s*=\\s*\"(?<Term>.*?)\".*?/>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static readonly Regex reTocExclude = new Regex(@"<!--\s*@TOCExclude\s*-->",
            RegexOptions.IgnoreCase);

        private static readonly Regex reIsDefaultTopic = new Regex(@"<!--\s*@DefaultTopic\s*-->",
            RegexOptions.IgnoreCase);

        private static readonly Regex reSplitToc = new Regex(@"<!--\s*@SplitTOC\s*-->", RegexOptions.IgnoreCase);

        private static readonly Regex reSortOrder = new Regex(@"<!--\s*@SortOrder\s*(?<SortOrder>\d{1,5})\s*-->",
            RegexOptions.IgnoreCase);

        // Regular expressions used for encoding detection and parsing
        private static readonly Regex reXmlEncoding = new Regex(
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
            get => reBody.ToString();
            set
            {
                if(String.IsNullOrEmpty(value))
                    throw new InvalidOperationException("The expression cannot be null or empty");

                reBody = new Regex(value, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }
        }

        /// <summary>
        /// This returns the topic ID parsed from the file
        /// </summary>
        public Guid TopicId => topicId;

        /// <summary>
        /// This returns the revision number parsed from the file
        /// </summary>
        public string RevisionNumber => revisionNumber;

        /// <summary>
        /// This returns the title parsed from the file
        /// </summary>
        public string Title => title;

        /// <summary>
        /// This returns the abstract parsed from the file
        /// </summary>
        public string TopicAbstract => topicAbstract;

        /// <summary>
        /// This returns the TOC exclude flag if found in the topic
        /// </summary>
        public bool TocExclude => tocExclude;

        /// <summary>
        /// This returns the default topic flag if found in the topic
        /// </summary>
        public bool IsDefaultTopic => defaultTopic;

        /// <summary>
        /// This returns the split TOC flag if found in the topic
        /// </summary>
        public bool SplitToc => splitToc;

        /// <summary>
        /// This returns the sort order value if found in the topic
        /// </summary>
        public int SortOrder => sortOrder;

        /// <summary>
        /// This returns the HTML body parsed from the file
        /// </summary>
        public string Body => body;

        /// <summary>
        /// This returns the help attributes parsed from the file
        /// </summary>
        public MSHelpAttrCollection HelpAttributes => helpAttributes;

        /// <summary>
        /// This returns the help keywords parsed from the file
        /// </summary>
        public MSHelpKeywordCollection HelpKeywords => helpKeywords;

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
                _ = Guid.TryParse(WebUtility.HtmlDecode(m.Groups[1].Value), out topicId);

            m = reRevisionNumber.Match(content);

            if(m.Success)
                revisionNumber = WebUtility.HtmlDecode(m.Groups[1].Value);

            m = reTitle.Match(content);

            if(m.Success)
                title = WebUtility.HtmlDecode(m.Groups[1].Value);

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
                helpKeywords.Add(new MSHelpKeyword(keyword.Groups["Index"].Value, keyword.Groups["Term"].Value));
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

                    using(StreamReader sr = new StreamReader(filename, encoding, true))
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
