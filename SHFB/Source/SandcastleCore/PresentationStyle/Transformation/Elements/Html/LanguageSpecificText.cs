//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : LanguageSpecificText.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/11/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to define language specific text used by a presentation style
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/25/2022  EFW  Created the code
//===============================================================================================================

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.Html
{
    /// <summary>
    /// This class is used to define language specific text used by a presentation style
    /// </summary>
    /// <remarks>These elements are translated to script calls by the Language Specific Text build component in
    /// HTML-based presentation styles.  In those that don't support script such as Open XML, the build task that
    /// generates the final content replaces them with the neutral text equivalent or removes them.</remarks>
    public sealed class LanguageSpecificText
    {
        #region Private data members
        //=====================================================================

        private readonly List<(string LanguageId, string Keyword)> languageText;

        #endregion

        #region Constants
        //=====================================================================

        /// <summary>
        /// The style name to use for language specific text.  This is used by the build component.
        /// </summary>
        public const string LanguageSpecificTextStyleName = "languageSpecificText";

        /// <summary>
        /// This represents the C++ language identifier
        /// </summary>
        public const string CPlusPlus = "cpp";

        /// <summary>
        /// This represents the C# language identifier
        /// </summary>
        public const string CSharp = "cs";

        /// <summary>
        /// This represents the Visual Basic language identifier
        /// </summary>
        public const string VisualBasic = "vb";

        /// <summary>
        /// This represents the F# language identifier
        /// </summary>
        public const string FSharp = "fs";

        /// <summary>
        /// This represents the neutral language identifier
        /// </summary>
        /// <value>The text for this value will be used if it doesn't have a specific match in the topic's
        /// language filter selection.  Typically, this is used when the same keyword can be used for multiple
        /// languages.</value>
        public const string Neutral = "nu";

        /// <summary>
        /// This represents the common name separators
        /// </summary>
        public static readonly LanguageSpecificText NameSeparator = new LanguageSpecificText(false, new[]
        {
            (CPlusPlus, "::"),
            (Neutral, "."),
        });

        /// <summary>
        /// This represents the common generic type specialization opening characters (&lt;T&gt;)
        /// </summary>
        public static readonly LanguageSpecificText TypeSpecializationOpening = new LanguageSpecificText(false, new[]
        {
            (CPlusPlus, "<"),
            (CSharp, "<"),
            (FSharp, "<"),
            (VisualBasic, "(Of "),
            (Neutral, "("),
        });

        /// <summary>
        /// This represents the common generic type specialization closing characters (&lt;T&gt;)
        /// </summary>
        public static readonly LanguageSpecificText TypeSpecializationClosing = new LanguageSpecificText(false, new[]
        {
            (CPlusPlus, ">"),
            (CSharp, ">"),
            (FSharp, ">"),
            (VisualBasic, ")"),
            (Neutral, ")"),
        });

        /// <summary>
        /// This represents the "array of" opening text.  Closing text is rendered dynamically due to the need
        /// to show rank if defined.
        /// </summary>
        public static readonly LanguageSpecificText ArrayOfOpening = new LanguageSpecificText(false, new[]
        {
            (CPlusPlus, "array<")
        });

        /// <summary>
        /// This represents the closing text for "reference to" characters
        /// </summary>
        public static readonly LanguageSpecificText ReferenceTo = new LanguageSpecificText(false, new[]
        {
            (CPlusPlus, "%")
        });
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns true if the <see cref="KeywordStyleName" /> will be applied to the
        /// rendered language-specific text or false if not.
        /// </summary>
        public bool ApplyKeywordStyle { get; }

        /// <summary>
        /// This read-only property returns an enumerable list of the language specific text
        /// </summary>
        /// <value>The key is the language identifier and the value is the text to show for the related language</value>
        public IEnumerable<(string LanguageId, string Keyword)> Text => languageText;

        /// <summary>
        /// This is used to get or set the keyword style name
        /// </summary>
        public static string KeywordStyleName { get; set; } = "keyword";

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="applyKeywordStyle">True to apply the <see cref="KeywordStyleName" /> to the rendered
        /// language-specific text, false to not apply it.</param>
        /// <param name="text">An enumerable list of tuples containing the language ID and related text to display</param>
        public LanguageSpecificText(bool applyKeywordStyle, IEnumerable<(string LanguageId, string Keyword)> text)
        {
            this.ApplyKeywordStyle = applyKeywordStyle;

            languageText = new List<(string LanguageId, string Keyword)>(text);
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// Render the language specific text
        /// </summary>
        /// <returns>An XML element containing the language specific text for the keyword</returns>
        public XElement Render()
        {
            var span = new XElement("span", new XAttribute("class", LanguageSpecificTextStyleName),
                this.Text.Select(t => new XElement("span", new XAttribute("class", t.LanguageId), t.Keyword)));

            if(!this.ApplyKeywordStyle)
                return span;

            return new XElement("span", new XAttribute("class", KeywordStyleName), span);
        }

        /// <summary>
        /// This can be used to render an unrecognized language keyword with an appropriate style
        /// </summary>
        /// <param name="keyword">The keyword to render</param>
        /// <returns>An XML element containing the rendered keyword</returns>
        public static XElement RenderKeyword(string keyword)
        {
            return new XElement("span", new XAttribute("class", KeywordStyleName), keyword);
        }
        #endregion
    }
}
