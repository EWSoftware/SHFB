//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : CodeSnippet.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/05/2025
// Note    : Copyright 2014-2025, Eric Woodruff, All rights reserved
//
// This file contains a class that is used to track a single code snippet in the SyntaxComponent
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/27/2014  EFW  Created the code
//===============================================================================================================

using System;
using System.Xml;

namespace Sandcastle.Tools.BuildComponents.Snippets
{
    /// <summary>
    /// This is used to track a single code snippet in the <see cref="SyntaxComponent"/>
    /// </summary>
    public sealed class CodeSnippet
    {
        #region Private data members
        //=====================================================================

        private readonly XmlElement code;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the code element
        /// </summary>
        public XmlElement CodeElement => code;

        /// <summary>
        /// This read-only property returns the title for the snippet if one is defined
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// This read-only property returns the language for the snippet
        /// </summary>
        public string Language { get; }

        /// <summary>
        /// This is used to get or set the language element name
        /// </summary>
        public string LanguageElementName { get; set; }

        /// <summary>
        /// This is used to get the keyword style parameter
        /// </summary>
        public string KeywordStyleParameter { get; set; }

        /// <summary>
        /// This is used to get or set the sort order
        /// </summary>
        public int SortOrder { get; set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="code">The code element</param>
        public CodeSnippet(XmlElement code)
        {
            this.code = code ?? throw new ArgumentNullException(nameof(code));

            var attr = code.Attributes["title"];

            if(attr != null)
                this.Title = attr.Value;

            attr = code.Attributes["language"] ?? code.Attributes["lang"] ?? code.Attributes["codeLanguage"];

            this.Language = attr?.Value ?? "Other";
        }
        #endregion
    }
}
