//===============================================================================================================
// System  : HTML to MAML Converter
// File    : ImageReference.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/08/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains a class that represents an image reference.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/12/2008  EFW  Created the code
//===============================================================================================================

using System;
using System.Globalization;
using System.IO;

using SandcastleBuilder.Utils;

namespace HtmlToMamlConversion
{
    /// <summary>
    /// This represents an image reference
    /// </summary>
    public class ImageReference
    {

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get the source image filename
        /// </summary>
        public FilePath SourceFile { get; set; }

        /// <summary>
        /// This returns the image's unique ID
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Get or set the image's alternate text
        /// </summary>
        public string AlternateText { get; set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">The source image file</param>
        public ImageReference(string source)
        {
            this.Id = Path.GetFileNameWithoutExtension(source);
            this.SourceFile = new FilePath(source, HtmlToMaml.PathProvider);
        }
        #endregion

        #region ToString
        //=====================================================================

        /// <summary>
        /// Convert to string for debugging purposes
        /// </summary>
        /// <returns>The string representation of the topic</returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}", this.SourceFile.PersistablePath,
                this.Id, this.AlternateText);
        }
        #endregion
    }
}
