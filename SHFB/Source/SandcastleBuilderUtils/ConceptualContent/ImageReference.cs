//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ImageReference.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/06/2010
// Note    : Copyright 2008-2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing a conceptual content image that can
// be used to insert a reference to an image in a topic.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.7  04/24/2008  EFW  Created the code
// 1.8.0.0  07/25/2008  EFW  Reworked to support new MSBuild project format
//=============================================================================

using System;
using System.ComponentModel;
using System.Globalization;

namespace SandcastleBuilder.Utils.ConceptualContent
{
    /// <summary>
    /// This represents a conceptual content image that can be used to insert
    /// a reference to an image in a topic.
    /// </summary>
    /// <remarks>This class is serializable so that it can be copied to the
    /// clipboard.</remarks>
    [DefaultProperty("ImageFile")]
    public class ImageReference
    {
        #region Private data members
        //=====================================================================

        private FileItem fileItem;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get the image filename
        /// </summary>
        [Category("File"), Description("The image filename")]
        public string Filename
        {
            get { return fileItem.Name; }
        }

        /// <summary>
        /// This is used to get the full path to the image file
        /// </summary>
        [Category("File"), Description("The full path to the image file")]
        public string FullPath
        {
            get { return fileItem.FullPath; }
        }

        /// <summary>
        /// This is used to get the unique ID of the image
        /// </summary>
        [Category("Metadata"), Description("The unique ID of the image"),
          DefaultValue(null)]
        public string Id
        {
            get { return fileItem.ImageId; }
            set
            {
                if(value == null || value.Trim().Length == 0)
                    value = Guid.NewGuid().ToString();

                fileItem.ImageId = value;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not to copy the image to the
        /// <b>.\Output\[HelpFormat]\media</b> folder if it is not referenced
        /// in a media link.
        /// </summary>
        /// <value>The default is false and the image will not be copied
        /// unless it is referenced in a media link.  If set to true, the
        /// image will be copied even if it is not referenced.  This is useful
        /// for forcing the copy of images referenced in external links
        /// which are not handled by the art reference build component.</value>
        [Category("Metadata"), Description("If true, the image is always " +
          "copied to the output media folder.  If false, it will only be " +
          "copied if referenced in a media link."), DefaultValue(false)]
        public bool CopyToMedia
        {
            get { return fileItem.CopyToMedia; }
            set { fileItem.CopyToMedia = value; }
        }

        /// <summary>
        /// This is used to get or set the optional alternate text for the image
        /// </summary>
        [Category("Metadata"), Description("The optional alternate text for the image"),
          DefaultValue(null)]
        public string AlternateText
        {
            get { return fileItem.AlternateText; }
            set { fileItem.AlternateText = value; }
        }

        /// <summary>
        /// This read-only property is used to get a title for display
        /// (i.e. in the designer).
        /// </summary>
        /// <value>If there is <see cref="AlternateText" /> specified, it is
        /// returned along with the filename and the image ID in parentheses.
        /// If not, the filename is returned along with the image ID in
        /// parentheses.</value>
        [Browsable(false)]
        public string DisplayTitle
        {
            get
            {
                if(!String.IsNullOrEmpty(fileItem.AlternateText))
                    return String.Format(CultureInfo.CurrentCulture,
                        "{0} ({1}, {2})", fileItem.AlternateText,
                        fileItem.Name, fileItem.ImageId);

                return fileItem.Name + " (" + fileItem.ImageId + ")";
            }
        }

        /// <summary>
        /// This is used to get the file item associated with the image
        /// reference.
        /// </summary>
        [Browsable(false)]
        public FileItem FileItem
        {
            get { return fileItem; }
        }
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildItem">The build item to associate with the
        /// image reference.</param>
        public ImageReference(FileItem buildItem)
        {
            fileItem = buildItem;
        }
        #endregion

        #region Convert to link element format
        //=====================================================================

        /// <summary>
        /// Convert the image reference to a <c>&lt;mediaLink&gt;</c> element.
        /// </summary>
        /// <returns>The image in its <c>&lt;mediaLink&gt;</c> element form</returns>
        public string ToMediaLink()
        {
            string caption;

            if(!String.IsNullOrEmpty(fileItem.AlternateText))
                caption = String.Concat("<caption>", fileItem.AlternateText,
                    "</caption>\r\n");
            else
                caption = String.Empty;

            return String.Format(CultureInfo.CurrentCulture,
                "<mediaLink>\r\n{0}<image xlink:href=\"{1}\"/>\r\n" +
                "</mediaLink>", caption, fileItem.ImageId);
        }

        /// <summary>
        /// Convert the image reference to a <c>&lt;mediaLinkInline&gt;</c>
        /// element.
        /// </summary>
        /// <returns>The image in its <c>&lt;mediaLinkInline&gt;</c> element form</returns>
        public string ToMediaLinkInline()
        {
            return String.Format(CultureInfo.CurrentCulture,
                "<mediaLinkInline><image xlink:href=\"{0}\"/>" +
                "</mediaLinkInline>", fileItem.ImageId);
        }

        /// <summary>
        /// Convert the image reference to an <c>&lt;externalLink&gt;</c> element.
        /// </summary>
        /// <returns>The image in its <c>&lt;externalLink&gt;</c> element form</returns>
        public string ToExternalLink()
        {
            string linkAltText, linkText;

            if(!String.IsNullOrEmpty(fileItem.AlternateText))
            {
                linkText = String.Concat("<linkText>", fileItem.AlternateText,
                    "</linkText>\r\n");
                linkAltText = String.Concat("<linkAlternateText>",
                    fileItem.AlternateText, "</linkAlternateText>\r\n");
            }
            else
            {
                linkText = String.Concat("<linkText>", fileItem.ImageId,
                    "</linkText>\r\n");
                linkAltText = String.Empty;
            }

            return String.Format(CultureInfo.CurrentCulture,
                "<externalLink>\r\n{0}{1}<linkUri>../Media/{2}" +
                "</linkUri>\r\n<linkTarget>_self</linkTarget>\r\n" +
                "</externalLink>", linkText, linkAltText, fileItem.Name);
        }

        /// <summary>
        /// Convert the image reference to an <c>&lt;img&gt;</c> element.
        /// </summary>
        /// <returns>The image in its <c>&lt;img&gt;</c> element form</returns>
        public string ToImageLink()
        {
            string linkAltText;

            if(!String.IsNullOrEmpty(fileItem.AlternateText))
                linkAltText = fileItem.AlternateText;
            else
                linkAltText = String.Empty;

            return String.Format(CultureInfo.CurrentCulture,
                "<img src=\"../Media/{0}\" alt=\"{1}\" />", fileItem.Name,
                linkAltText);
        }
        #endregion
    }
}
