//=============================================================================
// System  : Sandcastle Build Components
// File    : TargetDirectory.cs
// Note    : Copyright 2010-2012 Microsoft Corporation
//
// This file contains the TargetDirectory class used to represent a targets
// directory along with all the assoicated expressions used to find target
// metadata files in it, and extract URLs and link text from those files
// using the ResolveConceptualLinksComponent.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://Sandcastle.CodePlex.com.   This notice and
// all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Change History
// 02/16/2012 - EFW - Merged my changes into the code
//=============================================================================

using System;
using System.IO;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This public enumerated type defines the link types
    /// </summary>
    public enum LinkType
    {
        /// <summary>No links</summary>
        None,
        /// <summary>Local links</summary>
        Local,
        /// <summary>Index links (MS Help 2 only)</summary>
        Index,
        /// <summary>Id links (MS Help Viewer only)</summary>
        Id
    }

    /// <summary>
    /// This represents a targets directory along with all the assoicated
    /// expressions used to find target metadata files in it, and extract URLs
    /// and link text from those files using the <see cref="ResolveConceptualLinksComponent"/>.
    /// </summary>
    public class TargetDirectory
    {
        #region Private data members
        //=====================================================================

        // EFW - Got rid of fileExpression as it wasn't used
        private XPathExpression textExpression, urlExpression, linkTextExpression;
        private LinkType linkType;
        private string directory;
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
        /// <exception cref="ArgumentNullException">This is thrown if the
        /// directory, URL expression, or either text expression is null.</exception>
        public TargetDirectory(string targetDir, XPathExpression urlExp,
          XPathExpression textExp, XPathExpression linkTextExp, LinkType typeOfLink)
        {
            if(targetDir == null)
                throw new ArgumentNullException("targetDir");

            if(urlExp == null)
                throw new ArgumentNullException("urlExp");

            if(textExp == null)
                throw new ArgumentNullException("textExp");

            // EFW - Added support for alternate link text expression
            if(linkTextExp == null)
                throw new ArgumentNullException("linkTextExp");

            directory = targetDir;
            urlExpression = urlExp;
            textExpression = textExp;
            linkTextExpression = linkTextExp;
            linkType = typeOfLink;
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// Get target info for the specified file
        /// </summary>
        /// <param name="file">The file for which to get target info</param>
        /// <returns>A <see cref="TargetInfo" /> object if found or null if
        /// not found.</returns>
        public TargetInfo GetTargetInfo(string file)
        {
            // EFW - Inlined GetDocument()
            string path = Path.Combine(directory, file);

            if(!File.Exists(path))
                return null;

            XPathDocument document = new XPathDocument(path);
            XPathNavigator navigator = document.CreateNavigator();
            string url = navigator.Evaluate(urlExpression).ToString(),
                   text = navigator.Evaluate(textExpression).ToString(),
                   linkText = navigator.Evaluate(linkTextExpression).ToString();

            // EFW - Use the alternate link text if specified
            if(!String.IsNullOrEmpty(linkText))
                text = linkText;

            return new TargetInfo(url, text, linkType);
        }
        #endregion
    }
}
