//=============================================================================
// System  : HTML to MAML Converter
// File    : ImageReference.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/18/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that represents an image reference.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: https://GitHub.com/EWSoftware/SHFB.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  09/12/2008  EFW  Created the code
//=============================================================================

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
        #region Private data members
        //=====================================================================

        private FilePath sourceFile;
        private string id, altText;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get the source image filename
        /// </summary>
        public FilePath SourceFile
        {
            get { return sourceFile; }
            set { sourceFile = value; }
        }

        /// <summary>
        /// This returns the image's unique ID
        /// </summary>
        public string Id
        {
            get { return id; }
        }

        /// <summary>
        /// Get or set the image's alternate text
        /// </summary>
        public string AlternateText
        {
            get { return altText; }
            set { altText = value; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">The source image file</param>
        public ImageReference(string source)
        {
            id = Path.GetFileNameWithoutExtension(source);
            sourceFile = new FilePath(source, HtmlToMaml.PathProvider);
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
            return String.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}",
                sourceFile.PersistablePath, id, altText);
        }
        #endregion
    }
}
