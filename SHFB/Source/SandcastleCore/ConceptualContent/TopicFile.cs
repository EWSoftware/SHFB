//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : TopicFile.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/05/2025
// Note    : Copyright 2008-2025, Eric Woodruff, All rights reserved
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
using System.Text.RegularExpressions;
using System.Xml;

using Sandcastle.Core.Markdown;

namespace Sandcastle.Core.ConceptualContent;

/// <summary>
/// This class represents a conceptual content topic file
/// </summary>
public class TopicFile
{
    #region Private data members
    //=====================================================================

    private DocumentType docType;
    private string id, alternateId, errorMessage;
    private bool contentParsed;
    private MarkdownFile markdownFile;
    private static readonly Regex reCodeEntityRef = new(@"\]\(@(?<Ref>[NTFEPM]:.*?)\)");

    #endregion

    #region Properties
    //=====================================================================

    /// <summary>
    /// This is used to get or set the file build item
    /// </summary>
    public ContentFile ContentFile { get; }

    /// <summary>
    /// Get the name of the file without the path
    /// </summary>
    public string Name => this.ContentFile.Filename;

    /// <summary>
    /// Get the full path to the file
    /// </summary>
    public string FullPath => this.ContentFile.FullPath;

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
    /// This is used to get the unique alternate ID of the topic
    /// </summary>
    public string AlternateId
    {
        get
        {
            this.ParseContent(false);
            return alternateId;
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
    /// This read-only property is used to return the Markdown file metadata if the topic is associated with one
    /// </summary>
    public MarkdownFile MarkdownFile
    {
        get
        {
            this.ParseContent(false);
            return markdownFile;
        }
    }

    /// <summary>
    /// This read-only property is used to return the error message if <see cref="DocumentType" /> returns
    /// <c>Invalid</c>.
    /// </summary>
    public string ErrorMessage => errorMessage;

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
        this.ContentFile = contentFile ?? throw new ArgumentNullException(nameof(contentFile));
    }
    #endregion

    #region Parsing methods
    //=====================================================================

    /// <summary>
    /// This will parse the file content and extract the document type and unique ID
    /// </summary>
    /// <param name="reparse">If false and the file has already been parsed, the method just returns.  If
    /// true, the file is reparsed to refresh the information.</param>
    public void ParseContent(bool reparse)
    {
        if(!reparse && contentParsed)
            return;

        contentParsed = false;
        docType = DocumentType.None;
        id = errorMessage = null;
        markdownFile = null;

        if(!File.Exists(this.ContentFile.FullPath))
        {
            docType = DocumentType.NotFound;
            return;
        }

        try
        {
            if(Path.GetExtension(this.ContentFile.Filename).Equals(".md", StringComparison.OrdinalIgnoreCase))
            {
                markdownFile = new MarkdownFile(this.ContentFile.FullPath);
                docType = DocumentType.Markdown;
                id = markdownFile.UniqueId;
                alternateId = markdownFile.AlternateId;
            }
            else
            {
                var settings = new XmlReaderSettings
                {
                    CloseInput = true,
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true,
                    IgnoreWhitespace = true
                };

                using var xr = XmlReader.Create(this.ContentFile.FullPath, settings);

                xr.MoveToContent();

                while(!xr.EOF)
                {
                    if(xr.NodeType != XmlNodeType.Element)
                        xr.Read();
                    else
                    {
                        switch(xr.Name)
                        {
                            case "topic":
                                // If a topic element is found, parse the required ID from it
                                string attrValue = xr.GetAttribute("id");

                                if(attrValue != null && attrValue.Trim().Length != 0)
                                    id = attrValue;
                                else
                                    throw new XmlException("<topic> element is missing the 'id' attribute");

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
                }
            }
        }
        catch(Exception ex)
        {
            docType = DocumentType.Invalid;
            errorMessage = ex.Message;
        }
        finally
        {
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
        List<string> entityRefs = [];
        HashSet<string> seenNamespaces = [];
        string ns;

        if(this.DocumentType == DocumentType.Markdown)
        {
            entityRefs = [.. reCodeEntityRef.Matches(File.ReadAllText(this.ContentFile.FullPath)).Cast<Match>().Select(
                m => m.Groups["Ref"].Value)];
        }
        else
        {
            entityRefs = [.. ComponentUtilities.XmlStreamAxis(
               this.ContentFile.FullPath, "codeEntityReference").Select(el => el.Value)];
        }

        foreach(var refName in entityRefs)
        {
            if(refName.Length > 2 && refName.IndexOfAny(['.', '(']) != -1)
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
    }
    #endregion
}
