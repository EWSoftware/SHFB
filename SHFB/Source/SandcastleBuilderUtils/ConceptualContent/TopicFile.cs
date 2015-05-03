//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : TopicFile.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/02/2013
// Note    : Copyright 2008-2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing a conceptual content topic file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
//===============================================================================================================
// 1.8.0.0  08/07/2008  EFW  Created the code
// 1.9.7.0  01/02/2013  EFW  Added method to get referenced namespaces
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.Utils.ConceptualContent
{
    /// <summary>
    /// This class represents a conceptual content topic file
    /// </summary>
    public class TopicFile
    {
        #region Private data members
        //=====================================================================

        private static Regex reMeta = new Regex("\\<meta\\s*name\\s*=" +
            "\\s*\"(?<Name>\\w*?)\"\\s*content\\s*=\\s*\"(?<Content>.*?)\"",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private FileItem fileItem;
        private DocumentType docType;
        private string id, errorMessage;
        private int revision;
        private bool contentParsed;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the file build item
        /// </summary>
        public FileItem FileItem
        {
            get { return fileItem; }
            set
            {
                fileItem = value;
                contentParsed = false;
                docType = DocumentType.None;
            }
        }

        /// <summary>
        /// Get the name of the file without the path
        /// </summary>
        public string Name
        {
            get { return fileItem.Name; }
        }

        /// <summary>
        /// Get the full path to the file
        /// </summary>
        public string FullPath
        {
            get { return fileItem.FullPath; }
        }

        /// <summary>
        /// This is used to get the unique ID of the topic
        /// </summary>
        public string Id
        {
            get
            {
                this.ParseContent(false);
                return id;
            }
        }

        /// <summary>
        /// This is used to get the topic's revision number
        /// </summary>
        public int RevisionNumber
        {
            get
            {
                this.ParseContent(false);
                return revision;
            }
        }

        /// <summary>
        /// This read-only property is used to get the document type
        /// </summary>
        public DocumentType DocumentType
        {
            get
            {
                this.ParseContent(false);
                return docType;
            }
        }

        /// <summary>
        /// This read-only property is used to return the error message if
        /// <see cref="DocumentType" /> returns <b>Invalid</b>.
        /// </summary>
        public string ErrorMessage
        {
            get { return errorMessage; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="file">The file build item from the project</param>
        /// <exception cref="ArgumentNullException">This is thrown if the file
        /// item is null.</exception>
        public TopicFile(FileItem file)
        {
            if(file == null)
                throw new ArgumentNullException("file");

            fileItem = file;
            revision = 1;
        }
        #endregion

        #region Parsing methods
        //=====================================================================

        /// <summary>
        /// This will parse the file content and extract the document type,
        /// unique ID, and revision number.
        /// </summary>
        /// <param name="reparse">If false and the file has already been
        /// parsed, the method just returns.  If true, the file is reparsed
        /// to refresh the information.</param>
        public void ParseContent(bool reparse)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            XmlReader xr = null;
            string attrValue, ext;
            int rev;

            if(!reparse && contentParsed)
                return;

            contentParsed = false;
            docType = DocumentType.None;
            id = errorMessage = null;
            revision = 1;

            if(!File.Exists(fileItem.FullPath))
            {
                docType = DocumentType.NotFound;
                return;
            }

            // Don't bother parsing HTML files but support them for passing through stuff like title pages which
            // may not need to look like the API topics.
            ext = Path.GetExtension(fileItem.FullPath).ToLowerInvariant();

            if(ext == ".htm" || ext == ".html")
            {
                docType = DocumentType.Html;

                contentParsed = true;
                this.ParseIdFromHtml();
                return;
            }

            try
            {
                settings.CloseInput = true;
                settings.IgnoreComments = true;
                settings.IgnoreProcessingInstructions = true;
                settings.IgnoreWhitespace = true;

                xr = XmlReader.Create(fileItem.FullPath, settings);
                xr.MoveToContent();

                while(!xr.EOF)
                    if(xr.NodeType != XmlNodeType.Element)
                        xr.Read();
                    else
                        switch(xr.Name)
                        {
                            case "topic":
                                // If a <topic> element is found, parse the ID
                                // and revision number from it.
                                attrValue = xr.GetAttribute("id");

                                // The ID is required
                                if(attrValue != null && attrValue.Trim().Length != 0)
                                    id = attrValue;
                                else
                                    throw new XmlException("<topic> element " +
                                        "is missing the 'id' attribute");

                                // This is optional
                                attrValue = xr.GetAttribute("revisionNumber");

                                if(attrValue != null && Int32.TryParse(attrValue, out rev))
                                    revision = rev;

                                xr.Read();
                                break;

                            case "developerConceptualDocument":
                            case "developerErrorMessageDocument":
                            case "developerGlossaryDocument":
                            case "developerHowToDocument":
                            case "developerOrientationDocument":
                            case "codeEntityDocument":
                            case "developerReferenceWithSyntaxDocument":
                            case "developerReferenceWithoutSyntaxDocument":
                            case "developerSampleDocument":
                            case "developerSDKTechnologyOverviewArchitectureDocument":
                            case "developerSDKTechnologyOverviewCodeDirectoryDocument":
                            case "developerSDKTechnologyOverviewOrientationDocument":
                            case "developerSDKTechnologyOverviewScenariosDocument":
                            case "developerSDKTechnologyOverviewTechnologySummaryDocument":
                            case "developerTroubleshootingDocument":
                            case "developerUIReferenceDocument":
                            case "developerWalkthroughDocument":
                            case "developerWhitePaperDocument":
                            case "developerXmlReference":
                                docType = (DocumentType)Enum.Parse(typeof(DocumentType), xr.Name, true);
                                xr.Read();
                                break;

                            default:    // Ignore it
                                xr.Skip();
                                break;
                        }
            }
            catch(Exception ex)
            {
                docType = DocumentType.Invalid;
                errorMessage = ex.Message;
            }
            finally
            {
                if(xr != null)
                    xr.Close();

                contentParsed = true;
            }
        }

        /// <summary>
        /// This is used to parse the ID and revision number from an HTML file
        /// </summary>
        private void ParseIdFromHtml()
        {
            Encoding enc = Encoding.Default;
            int rev;

            string content = BuildProcess.ReadWithEncoding(fileItem.FullPath,
                ref enc);

            MatchCollection matches = reMeta.Matches(content);

            foreach(Match m in matches)
                if(m.Groups[1].Value == "id")
                    id = m.Groups[2].Value;
                else
                    if(m.Groups[1].Value == "revisionNumber" &&
                      Int32.TryParse(m.Groups[2].Value, out rev))
                        revision = rev;
        }

        /// <summary>
        /// This is used to get an enumerable list of unique namespaces from the given reflection data file
        /// </summary>
        /// <param name="reflectionDataPath">The reflection data path containing the valid namespace files</param>
        /// <returns>An enumerable list of unique namespaces in the topic</returns>
        public IEnumerable<string> GetReferencedNamespaces(string reflectionDataPath)
        {
            XPathDocument doc = new XPathDocument(fileItem.FullPath);
            XPathNavigator nav = doc.CreateNavigator();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(nav.NameTable);
            nsMgr.AddNamespace("ddue", "http://ddue.schemas.microsoft.com/authoring/2003/5"); 

            HashSet<string> seenNamespaces = new HashSet<string>();
            string ns;

            HashSet<string> validNamespaces = new HashSet<string>(Directory.EnumerateFiles(reflectionDataPath,
                "*.xml", SearchOption.AllDirectories).Select(f => Path.GetFileNameWithoutExtension(f)));

            // Find all code entity references
            var nodes = nav.Select("//ddue:codeEntityReference", nsMgr);

            foreach(XPathNavigator n in nodes)
                if(n.Value.Length > 2 && n.Value.IndexOfAny(new[] { '.', '(' }) != -1)
                {
                    ns = n.Value.Trim();

                    // Strip off member name?
                    if(!ns.StartsWith("R:", StringComparison.OrdinalIgnoreCase) &&
                      !ns.StartsWith("N:", StringComparison.OrdinalIgnoreCase) &&
                      !ns.StartsWith("T:", StringComparison.OrdinalIgnoreCase))
                    {
                        if(ns.IndexOf('(') != -1)
                            ns = ns.Substring(0, ns.IndexOf('('));

                        if(ns.IndexOf('.') != -1)
                            ns = ns.Substring(0, ns.LastIndexOf('.'));
                    }

                    if(ns.IndexOf('.') != -1)
                        ns = ns.Substring(2, ns.LastIndexOf('.') - 2);
                    else
                        ns = ns.Substring(2);

                    if(validNamespaces.Contains(ns) && !seenNamespaces.Contains(ns))
                    {
                        seenNamespaces.Add(ns);
                        yield return ns;
                    }
                }
        }
        #endregion
    }
}
