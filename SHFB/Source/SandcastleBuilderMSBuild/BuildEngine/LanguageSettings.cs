//===============================================================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : LanguageSettings.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/21/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the language settings for use by the HTML extract build step
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/02/2022  EFW  Moved the language settings into this class from the configuration file
//===============================================================================================================

namespace SandcastleBuilder.MSBuild.BuildEngine
{
    /// <summary>
    /// This class is used to contain the language settings for the HTML extract build step
    /// </summary>
    public class LanguageSettings
    {
        /// <summary>
        /// This is used to get or set the locale ID (LCID) for the language
        /// </summary>
        public int LocaleId { get; set; }

        /// <summary>
        /// This is used to get or set the code page to use when determining the encoding for the files based on
        /// the <see cref="LocaleId" />.
        /// </summary>
        public int CodePage { get; set; }

        /// <summary>
        /// This is used to get or set the character set value that will be written to the HTML files in place of
        /// the UTF-8 value when localizing the files for use with the HTML Help 1 compiler.
        /// </summary>
        public string CharacterSet { get; set; }
    }
}
