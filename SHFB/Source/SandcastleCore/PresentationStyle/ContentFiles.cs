//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ContentFiles.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/06/2021
// Note    : Copyright 2012-2021, Eric Woodruff, All rights reserved
//
// This file contains a class that is used to contain content files for a presentation style that are to be
// embedded in the compiled help file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/24/2012  EFW  Created the code
// 06/21/2013  EFW  Added support for format-specific help content files
// 01/04/2014  EFW  Moved the code into Sandcastle.Core
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sandcastle.Core.PresentationStyle
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
        public HelpFileFormats HelpFileFormats { get; }

        /// <summary>
        /// This read-only property returns the base path used for the source path files
        /// </summary>
        /// <value>If null, the presentation style base path is used</value>
        public string BasePath { get; }

        /// <summary>
        /// This read-only property returns the source path wildcard used to copy files
        /// </summary>
        public string SourcePathWildcard { get; }

        /// <summary>
        /// This read-only property returns the destination folder of the content files in the compiled help file
        /// </summary>
        public string DestinationFolder { get; }

        /// <summary>
        /// This read-only property returns an enumerable list of file extensions that should be treated as
        /// template files that need substitution tags replaced at build time.
        /// </summary>
        public IEnumerable<string> TemplateFileExtensions { get; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="helpFileFormats">The help file formats to which the files apply</param>
        /// <param name="sourcePath">The source path</param>
        /// <overloads>There are two overloads for the constructor</overloads>
        /// <remarks>The files from the source path will be copied to a like named folder in the build output</remarks>
        public ContentFiles(HelpFileFormats helpFileFormats, string sourcePath) : this(helpFileFormats, null,
          sourcePath, null, Enumerable.Empty<string>())
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="helpFileFormats">The help file formats to which the files apply</param>
        /// <param name="sourcePath">The source path</param>
        /// <param name="destination">The destination path to use in the build output</param>
        public ContentFiles(HelpFileFormats helpFileFormats, string sourcePath, string destination) :
          this(helpFileFormats, null, sourcePath, destination, Enumerable.Empty<string>())
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="helpFileFormats">The help file formats to which the files apply</param>
        /// <param name="basePath">An alternate base path or null to use the presentation style base path</param>
        /// <param name="sourcePath">The source path</param>
        /// <param name="destination">The destination path</param>
        /// <param name="templateFileExtensions">An enumerable list of file extensions to treat as template files</param>
        public ContentFiles(HelpFileFormats helpFileFormats, string basePath, string sourcePath,
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
            this.TemplateFileExtensions = templateFileExtensions.ToList();
        }
        #endregion
    }
}
