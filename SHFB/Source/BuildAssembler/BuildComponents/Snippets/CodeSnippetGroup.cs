//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : CodeSnippetGroup.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/04/2014
// Note    : Copyright 2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is used to track a group of related code snippets in the SyntaxComponent
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/27/2014  EFW  Created the code
//===============================================================================================================

using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Ddue.Tools.Snippets
{
    /// <summary>
    /// This is used to track a group of related code snippets in the <see cref="SyntaxComponent"/>
    /// </summary>
    public sealed class CodeSnippetGroup
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get the containing group element
        /// </summary>
        /// <remarks>This serves as a place holder during the grouping and sorting operations and it becomes the
        /// parent of the code snippets in the final topic.</remarks>
        public XmlElement SnippetGroupElement { get; private set; }

        /// <summary>
        /// This read-only property is used to get the list of related code snippets
        /// </summary>
        public List<CodeSnippet> CodeSnippets { get; private set; }

        /// <summary>
        /// This is used to get or set whether or not the group is a single, standalone snippet
        /// </summary>
        public bool IsStandalone { get; set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="snippetGroupElement">The snippet group element used to contain the snippets</param>
        public CodeSnippetGroup(XmlElement snippetGroupElement)
        {
            this.SnippetGroupElement = snippetGroupElement;
            this.CodeSnippets = new List<CodeSnippet>();
        }
        #endregion
    }
}
