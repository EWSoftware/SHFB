//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : OpenXmlElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/09/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the abstract base class used to create rendered Open XML elements in API and MAML topics
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/26/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements.OpenXml
{
    /// <summary>
    /// This abstract base class is used to create rendered Open XML elements in API and MAML topics
    /// </summary>
    public abstract class OpenXmlElement : Element
    {
        #region Constants
        //=====================================================================

        /// <summary>
        /// The Word Processing markup language namespace (w)
        /// </summary>
        public static readonly XNamespace WordProcessingML = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        /// <summary>
        /// The Office namespace (o)
        /// </summary>
        public static readonly XNamespace Office = "urn:schemas-microsoft-com:office:office";

        /// <summary>
        /// The VML namespace (v)
        /// </summary>
        public static readonly XNamespace Vml = "urn:schemas-microsoft-com:vml";

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The element name</param>
        protected OpenXmlElement(string name) : base(name)
        {
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Add a bookmark for an address attribute
        /// </summary>
        /// <param name="content">The content element to which the bookmark is added</param>
        /// <param name="uniqueId">The unique ID to use for the bookmark</param>
        /// <remarks>Open XML does not support ID attributes like HTML.  Instead, it renders bookmarks with the
        /// unique IDs that will be used as the link targets.  The Open XML file builder task will reformat the
        /// bookmark name and ID to ensure that they are all unique.</remarks>
        public static void AddAddressBookmark(XElement content, string uniqueId)
        {
            if(content == null)
                throw new ArgumentNullException(nameof(content));

            content.Add(new XElement(WordProcessingML + "bookmarkStart",
                    new XAttribute(WordProcessingML + "name", $"_{uniqueId}"),
                    new XAttribute(WordProcessingML + "id", "0")),
                new XElement(WordProcessingML + "bookmarkEnd",
                    new XAttribute(WordProcessingML + "id", "0")));
        }
        #endregion
    }
}
