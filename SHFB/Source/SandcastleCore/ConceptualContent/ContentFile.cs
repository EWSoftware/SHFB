//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ContentFile.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2015-2025, Eric Woodruff, All rights reserved
//
// This file contains a class representing a content file such as a token file, code snippet file, image, etc.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/13/2015  EFW  Created the code
//===============================================================================================================

using System;
using System.IO;
using System.Globalization;

namespace Sandcastle.Core.ConceptualContent
{
    /// <summary>
    /// This represents a content file such as a token file, code snippet file, image, etc.
    /// </summary>
    public class ContentFile
    {
        #region Private data members
        //=====================================================================

        private readonly FilePath filePath;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get the base path provider
        /// </summary>
        public IBasePathProvider BasePathProvider => filePath.BasePathProvider;

        /// <summary>
        /// This read-only property is used to get the content filename without the path
        /// </summary>
        public string Filename => Path.GetFileName(filePath);

        /// <summary>
        /// This is used to get or set the full path to the content file
        /// </summary>
        /// <remarks>This returns the path to the file's true location.  For linked items, this path will differ
        /// from the <see cref="LinkPath"/> which returns the project-relative location.</remarks>
        public string FullPath => filePath;

        /// <summary>
        /// This read-only property is used to get the persistable path to the content item (the path relative
        /// to the project folder)
        /// </summary>
        public string PersistablePath => filePath.PersistablePath;

        /// <summary>
        /// This is used to get or set the link path to the content file (the project-relative location)
        /// </summary>
        /// <remarks>For linked items, this will be the location of the file within the project.  For files
        /// outside the project folder, this will not match the <see cref="FullPath"/> property value.</remarks>
        public string LinkPath
        {
            get
            {
                if(String.IsNullOrWhiteSpace(field))
                    return filePath;

                return field;
            }
            set => field = value;
        }

        /// <summary>
        /// This is used to get or set the sort order for site map and content layout files
        /// </summary>
        /// <value>For other file types, this will always return zero</value>
        public int SortOrder { get; set; }

        /// <summary>
        /// This read-only property returns the language of the content file
        /// </summary>
        /// <value>The language is determined by looking at the suffix on the filename (Filename_LangSuffix.xxx)
        /// or the filename itself without the extension.  If the suffix or filename is a valid language code,
        /// this returns it.  If not valid, null is returned and the file is assumed to be language neutral.</value>
        public CultureInfo Language { get; }

        /// <summary>
        /// This is used to get or set a provider that can be used to obtain content files from a project or some
        /// other source.
        /// </summary>
        public IContentFileProvider ContentFileProvider { get; set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filePath">The full path to the content file</param>
        public ContentFile(FilePath filePath)
        {
            if(filePath == null || filePath.Path.Length == 0)
                throw new ArgumentException("A full path to the content file is required", nameof(filePath));

            this.filePath = filePath;

            // Set the language based on the filename suffix or the filename itself if possible
            string name = Path.GetFileNameWithoutExtension(filePath);
            int pos = name.LastIndexOf('_');

            try
            {
                this.Language = new CultureInfo(name.Substring(pos + 1));

                // If it's unknown, ignore it
                if(this.Language.ThreeLetterWindowsLanguageName == "ZZZ")
                    this.Language = null;
            }
            catch
            {
                // Ignore invalid values and assume it's language neutral.
            }
        }
        #endregion
    }
}
