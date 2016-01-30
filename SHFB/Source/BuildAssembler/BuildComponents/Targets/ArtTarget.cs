//===============================================================================================================
// System  : Sandcastle Build Components
// File    : ArtTarget.cs
//
// This file contains a class that holds art target information used to resolve art links by the
// ResolveArtLinksComponent.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Change History
// 12/24/2012 - EFW - Moved the class into its own file and separate namespace, made it public, and added
// getters and setters.
//===============================================================================================================

using System.Xml.XPath;

using Microsoft.Ddue.Tools.BuildComponent;

namespace Microsoft.Ddue.Tools.Targets
{
    /// <summary>
    /// This class holds art target information used to resolve art links by the <see cref="ResolveArtLinksComponent"/>
    /// </summary>
    public class ArtTarget
    {
        /// <summary>
        /// This is used to get or set the ID used to identify the file
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// This is used to get or set the input path where the source file is located
        /// </summary>
        public string InputPath { get; set; }

        /// <summary>
        /// This is used to get or set the base output path for all content
        /// </summary>
        public string BaseOutputPath { get; set; }

        /// <summary>
        /// This is used to get or set the XPath expression used to determine the output path for the art file
        /// </summary>
        public XPathExpression OutputXPath { get; set; }

        /// <summary>
        /// This is used to get or set the link path to use in reference links
        /// </summary>
        public string LinkPath { get; set; }

        /// <summary>
        /// This is used to set the alternate text for reference links
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// This is used to get or set the name of the file
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// This is used to get or set the XPath expression used to determine the reference link path format
        /// </summary>
        public XPathExpression FormatXPath { get; set; }

        /// <summary>
        /// This is used to get or set the XPath expression used to determine the relative reference link path
        /// </summary>
        public XPathExpression RelativeToXPath { get; set; }
    }
}
