//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : TopicFile.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/27/2015
// Note    : Copyright 2008-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing a conceptual content topic file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/07/2008  EFW  Created the code
// 01/02/2013  EFW  Added method to get referenced namespaces
// 05/08/2015  EFW  Removed support for raw HTML files
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core;

namespace SandcastleBuilder.Utils.ConceptualContent
{
    /// <summary>
    /// This class represents a conceptual content topic file
    /// </summary>
    public class TopicFile
    {
        #region Private data members
        //=====================================================================

        private ContentFile contentFile;
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
        public ContentFile ContentFile
        {
            get { return contentFile; }
            set
            {
                contentFile = value;
                contentParsed = false;
                docType = DocumentType.None;
            }
        }

        /// <summary>
        /// Get the name of the file without the path
        /// </summary>
        public string Name
        {
            get { return contentFile.Filename; }
        }

        /// <summary>
        /// Get the full path to the file
        /// </summary>
        public string FullPath
        {
            get { return contentFile.FullPath; }
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
        /// This read-only property is used to return the error message if <see cref="DocumentType" /> returns
        /// <c>Invalid</c>.
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
        /// <param name="contentFile">The content file from the project</param>
        /// <exception cref="ArgumentNullException">This is thrown if the content file is null</exception>
        public TopicFile(ContentFile contentFile)
        {
            if(contentFile == null)
                throw new ArgumentNullException("contentFile");

            this.contentFile = contentFile;

            revision = 1;
        }
        #endregion

        #region Parsing methods
        //=====================================================================

        /// <summary>
        /// This will parse the file content and extract the document type, unique ID, and revision number
        /// </summary>
        /// <param name="reparse">If false and the file has already been parsed, the method just returns.  If
        /// true, the file is reparsed to refresh the information.</param>
        public void ParseContent(bool reparse)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            XmlReader xr = null;
            string attrValue;
            int rev;

            if(!reparse && contentParsed)
                return;

            contentParsed = false;
            docType = DocumentType.None;
            id = errorMessage = null;
            revision = 1;

            if(!File.Exists(contentFile.FullPath))
            {
                docType = DocumentType.NotFound;
                return;
            }

            try
            {
                settings.CloseInput = true;
                settings.IgnoreComments = true;
                settings.IgnoreProcessingInstructions = true;
                settings.IgnoreWhitespace = true;

                xr = XmlReader.Create(contentFile.FullPath, settings);
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
        /// This is used to get an enumerable list of unique namespaces referenced in the topic
        /// </summary>
        /// <param name="validNamespaces">An enumerable list of valid framework namespaces</param>
        /// <returns>An enumerable list of unique namespaces in the topic</returns>
        public IEnumerable<string> GetReferencedNamespaces(IEnumerable<string> validNamespaces)
        {
            HashSet<string> seenNamespaces = new HashSet<string>();
            string ns;

            // Find all code entity references
            var entityRefs = ComponentUtilities.XmlStreamAxis(contentFile.FullPath, "codeEntityReference").Select(el => el.Value);

            foreach(var refName in entityRefs)
                if(refName.Length > 2 && refName.IndexOfAny(new[] { '.', '(' }) != -1)
                {
                    ns = refName.Trim();

                    // Strip off member name?
                    if(!ns.StartsWith("R:", StringComparison.OrdinalIgnoreCase) &&
                      !ns.StartsWith("G:", StringComparison.OrdinalIgnoreCase) &&
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
