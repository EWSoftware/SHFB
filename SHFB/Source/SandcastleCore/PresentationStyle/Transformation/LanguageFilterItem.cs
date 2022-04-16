//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : LanguageFilterItem.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/11/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains a class used to define the language filter items for presentation styles that support a
// language filter.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/11/2022  EFW  Created the code
//===============================================================================================================

namespace Sandcastle.Core.PresentationStyle.Transformation
{
    /// <summary>
    /// This defines the shared content item ID and keyword style values used for the language filter options
    /// </summary>
    public class LanguageFilterItem
    {
        /// <summary>
        /// This read-only property is used to get the shared content item ID to use for the language text
        /// </summary>
        public string SharedContentItemId { get; }

        /// <summary>
        /// This read-only property is used to get the keyword style to use for the language filter selection
        /// </summary>
        public string KeywordStyle { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sharedContentItemId">The shared content item ID</param>
        /// <param name="keywordStyle">The keyword style</param>
        public LanguageFilterItem(string sharedContentItemId, string keywordStyle)
        {
            this.SharedContentItemId = sharedContentItemId;
            this.KeywordStyle = keywordStyle;
        }
    }
}
