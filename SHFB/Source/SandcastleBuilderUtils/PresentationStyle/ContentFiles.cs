//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ContentFiles.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/24/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is used to contain content files for a presentation style that are to be
// embedded in the compiled help file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.   This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.6.0  10/24/2012  EFW  Created the code
//===============================================================================================================

using System;
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
        /// This read-only property returns the source path wildcard used to copy files
        /// </summary>
        public string SourcePathWildcard { get; private set; }

        /// <summary>
        /// This read-only property returns the destination folder of the content files in the compiled help file
        /// </summary>
        public string DestinationFolder { get; private set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sourcePath">The source path</param>
        /// <param name="destination">The destination path</param>
        internal ContentFiles(string sourcePath, string destination)
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

            this.SourcePathWildcard = sourcePath;
            this.DestinationFolder = destination;
        }
        #endregion
    }
}
