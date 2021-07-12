//===============================================================================================================
// System  : Sandcastle Build Components
// File    : TargetDirectory.cs
// Note    : Copyright 2010-2021 Microsoft Corporation
//
// This file contains the TargetDirectory class used to represent a targets directory along with all the
// associated expressions used to find target metadata files in it, and extract URLs and link text from those
// files using the ResolveConceptualLinksComponent.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Change History
// 02/16/2012 - EFW - Merged my changes into the code
// 12/26/2012 - EFW - Moved the class into the Targets namespace and move LinkType to its own file
//===============================================================================================================

using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace Sandcastle.Tools.BuildComponents.Targets
{
    /// <summary>
    /// This represents a targets directory along with all the associated expressions used to find target
    /// metadata files in it, and extract URLs and link text from those files using the
    /// <see cref="ResolveConceptualLinksComponent"/>.
    /// </summary>
    public class TargetDirectory
    {
        #region Private data members
        //=====================================================================

        // EFW - Got rid of fileExpression as it wasn't used
        private readonly XPathExpression textExpression, urlExpression, linkTextExpression;
        private readonly ConceptualLinkType linkType;
        private readonly string directory;

        #endregion

        // EFW - Removed unused properties

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="targetDir">The target directory</param>
        /// <param name="urlExp">The URL expression</param>
        /// <param name="textExp">The text (title) expression</param>
        /// <param name="linkTextExp">The alternate link text expression</param>
        /// <param name="typeOfLink">The link type</param>
        /// <exception cref="ArgumentNullException">This is thrown if the directory, URL expression, or either
        /// text expression is null.</exception>
        public TargetDirectory(string targetDir, XPathExpression urlExp, XPathExpression textExp,
          XPathExpression linkTextExp, ConceptualLinkType typeOfLink)
        {
            // EFW - Added support for alternate link text expression
            directory = targetDir ?? throw new ArgumentNullException(nameof(targetDir));
            urlExpression = urlExp ?? throw new ArgumentNullException(nameof(urlExp));
            textExpression = textExp ?? throw new ArgumentNullException(nameof(textExp));
            linkTextExpression = linkTextExp ?? throw new ArgumentNullException(nameof(linkTextExp));
            linkType = typeOfLink;
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// Get target info for the specified file
        /// </summary>
        /// <param name="targetId">The target ID for which to get target info.  This is expected to be in the
        /// form of a GUID.</param>
        /// <returns>A <see cref="TargetInfo" /> object if found or null if not found</returns>
        /// <remarks>The target ID is expected to be in the form of a GUID.  The target filename is expected to
        /// be in the format <b>[GUID].cmp.xml</b>.</remarks>
        public TargetInfo GetTargetInfo(string targetId)
        {
            // EFW - Inlined GetDocument()
            string path = Path.Combine(directory, targetId + ".cmp.xml");

            if(!File.Exists(path))
                return null;

            using(var reader = XmlReader.Create(path, new XmlReaderSettings { CloseInput = true }))
            {
                XPathDocument document = new XPathDocument(reader);
                XPathNavigator navigator = document.CreateNavigator();
                string url = navigator.Evaluate(urlExpression).ToString(),
                       text = navigator.Evaluate(textExpression).ToString(),
                       linkText = navigator.Evaluate(linkTextExpression).ToString();

                // EFW - Use the alternate link text if specified
                if(!String.IsNullOrEmpty(linkText))
                    text = linkText;

                return new TargetInfo(url, text, linkType);
            }
        }
        #endregion
    }
}
