//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ImageReference.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/03/2025
// Note    : Copyright 2008-2025, Eric Woodruff, All rights reserved
//
// This file contains a class representing a conceptual content image that can be used to insert a reference to
// an image in a topic.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/24/2008  EFW  Created the code
// 07/25/2008  EFW  Reworked to support new MSBuild project format
//===============================================================================================================

using System;
using System.IO;

namespace Sandcastle.Core.ConceptualContent;

/// <summary>
/// This represents a conceptual content image that can be used to insert a reference to an image in a topic
/// </summary>
public class ImageReference
{
    #region Properties
    //=====================================================================

    /// <summary>
    /// This read-only property is used to get the image filename without the path
    /// </summary>
    public string Filename => Path.GetFileName(this.FullPath);

    /// <summary>
    /// This is used to get or set the full path to the image file
    /// </summary>
    public string FullPath { get; set; }

    /// <summary>
    /// This is used to get or set the unique ID of the image
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// This is used to get or set whether or not to copy the image to the
    /// <strong>.\Output\[HelpFormat]\media</strong> folder if it is not referenced in a media link.
    /// </summary>
    /// <value>The default is false and the image will not be copied unless it is referenced in a media link.
    /// If set to true, the image will be copied even if it is not referenced.  This is useful for forcing
    /// the copying of images referenced in external links which are not handled by the art reference build
    /// component.</value>
    public bool CopyToMedia { get; set; }

    /// <summary>
    /// This is used to get or set the optional alternate text for the image
    /// </summary>
    public string AlternateText { get; set; }

    /// <summary>
    /// This read-only property is used to get a title for display (i.e. in the designer)
    /// </summary>
    /// <value>If there is <see cref="AlternateText" /> specified, it is returned along with the filename
    /// and the image ID in parentheses.  If not, the filename is returned along with the image ID in
    /// parentheses.</value>
    public string DisplayTitle
    {
        get
        {
            if(!String.IsNullOrEmpty(this.AlternateText))
                return $"{this.AlternateText} ({this.Filename}, {this.Id})";

            return $"{this.Filename} ({this.Id})";
        }
    }
    #endregion

    #region Constructor
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fullPath">The full path to the image file</param>
    /// <param name="id">The ID of the image</param>
    public ImageReference(string fullPath, string id)
    {
        if(String.IsNullOrWhiteSpace(fullPath))
            throw new ArgumentException("A full path to the image is required", nameof(fullPath));

        if(String.IsNullOrWhiteSpace(id))
            throw new ArgumentException("An image ID is required", nameof(id));

        this.FullPath = Path.GetFullPath(fullPath);
        this.Id = id;
    }
    #endregion

    #region Convert to link element format
    //=====================================================================

    /// <summary>
    /// Convert the image reference to a Markdown image link
    /// </summary>
    /// <returns>The image in its Markdown image link form</returns>
    public string ToMarkdownLink()
    {
        return $"![{this.AlternateText?.Trim()}](@{this.Id})\r\n";
    }

    /// <summary>
    /// Convert the image reference to a <c>&lt;mediaLink&gt;</c> element
    /// </summary>
    /// <returns>The image in its <c>&lt;mediaLink&gt;</c> element form</returns>
    public string ToMediaLink()
    {
        string caption = !String.IsNullOrWhiteSpace(this.AlternateText) ?
            String.Concat("<caption>", this.AlternateText, "</caption>\r\n") : String.Empty;

        return $"<mediaLink>\r\n{caption}<image xlink:href=\"{this.Id}\"/>\r\n</mediaLink>";
    }

    /// <summary>
    /// Convert the image reference to a <c>&lt;mediaLinkInline&gt;</c> element
    /// </summary>
    /// <returns>The image in its <c>&lt;mediaLinkInline&gt;</c> element form</returns>
    public string ToMediaLinkInline()
    {
        return $"<mediaLinkInline><image xlink:href=\"{this.Id}\"/></mediaLinkInline>";
    }

    /// <summary>
    /// Convert the image reference to an <c>&lt;externalLink&gt;</c> element
    /// </summary>
    /// <returns>The image in its <c>&lt;externalLink&gt;</c> element form</returns>
    public string ToExternalLink()
    {
        string linkAltText, linkText;

        if(!String.IsNullOrWhiteSpace(this.AlternateText))
        {
            linkText = String.Concat("<linkText>", this.AlternateText, "</linkText>\r\n");
            linkAltText = String.Concat("<linkAlternateText>", this.AlternateText, "</linkAlternateText>\r\n");
        }
        else
        {
            linkText = String.Concat("<linkText>", this.Id, "</linkText>\r\n");
            linkAltText = String.Empty;
        }

        return $"<externalLink>\r\n{linkText}{linkAltText}<linkUri>../Media/{this.Filename}</linkUri>\r\n" +
            "<linkTarget>_self</linkTarget>\r\n</externalLink>";
    }

    /// <summary>
    /// Convert the image reference to an <c>&lt;img&gt;</c> element
    /// </summary>
    /// <returns>The image in its <c>&lt;img&gt;</c> element form</returns>
    public string ToImageLink()
    {
        return $"<img src=\"../Media/{this.Filename}\" alt=\"{this.AlternateText?.Trim()}\" />";
    }
    #endregion
}
