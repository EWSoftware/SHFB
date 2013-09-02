//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ContentFiles.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/21/2013
// Note    : Copyright 2012-2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is used to contain content files for a presentation style that are to be
// embedded in the compiled help file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.6.0  10/24/2012  EFW  Created the code
// 1.9.8.0  06/21/2013  EFW  Added support for format-specific help content files
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;

namespace SandcastleBuilder.Utils.PresentationStyle
{
    /// <summary>
    /// This class is used to contain content files for a presentation style that are to be embedded in the
    /// compiled help file.
    /// </summary>
    public class ContentFiles
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the help file formats to which these files apply
        /// </summary>
        public HelpFileFormat HelpFileFormats { get; private set; }

        /// <summary>
        /// This read-only property returns the base path used for the source path files
        /// </summary>
        /// <value>If null, the presentation style base path is used</value>
        public string BasePath { get; private set; }

        /// <summary>
        /// This read-only property returns the source path wildcard used to copy files
        /// </summary>
        public string SourcePathWildcard { get; private set; }

        /// <summary>
        /// This read-only property returns the destination folder of the content files in the compiled help file
        /// </summary>
        public string DestinationFolder { get; private set; }

        /// <summary>
        /// This read-only property returns an enumerable list of file extensions that should be treated as
        /// template files that need substitution tags replaced at build time.
        /// </summary>
        public IEnumerable<string> TemplateFileExtensions { get; private set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="helpFileFormats">The help file formats to which the files apply</param>
        /// <param name="basePath">An alternate base path or null to use the presentation style base path</param>
        /// <param name="sourcePath">The source path</param>
        /// <param name="destination">The destination path</param>
        /// <param name="templateFileExtensions">An enumerable list of file extensions to treat as template files</param>
        internal ContentFiles(HelpFileFormat helpFileFormats, string basePath, string sourcePath,
          string destination, IEnumerable<string> templateFileExtensions)
        {
            if(Path.IsPathRooted(sourcePath))
                throw new InvalidOperationException("Content source path must be relative");

            if(!String.IsNullOrEmpty(destination))
            {
                if(Path.IsPathRooted(destination))
                    throw new InvalidOperationException("Content destination path must be relative");

                if(destination.IndexOfAny(new[] { '*', '?' }) != -1)
                    throw new InvalidOperationException("Content destination must be a path only");
            }

            this.HelpFileFormats = helpFileFormats;
            this.BasePath = basePath;
            this.SourcePathWildcard = sourcePath;
            this.DestinationFolder = destination;
            this.TemplateFileExtensions = templateFileExtensions;
        }
        #endregion
    }
}
