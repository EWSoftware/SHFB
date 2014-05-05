// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 03/09/2013 - EFW - Moved the supporting syntax writer classes to the SyntaxComponents assembly project
// 03/17/2013 - EFW - Added the RenderReferenceLinks property to control whether or not reference links are
// written as identifiers or actual reference links.
// 12/21/2013 - EFW - Moved class to Sandcastle.Core assembly
// 04/26/2014 - Changed WriteStartBlock to add a style ID to each syntax element for better handling within the
// XSL transformations.

using System.Collections.Generic;

namespace Sandcastle.Core.BuildAssembler.SyntaxGenerator
{
    /// <summary>
    /// This is used as the abstract base class for syntax writers
    /// </summary>
    public abstract class SyntaxWriter
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        protected SyntaxWriter()
        {
        }
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// The current position of the writer
        /// </summary>
        public virtual int Position
        {
            get { return -1; }
        }

        /// <summary>
        /// This is used to indicate whether or not the syntax writer should render reference links as actual
        /// links.
        /// </summary>
        /// <value>The default is false to render reference link elements as identifiers instead.  If set to
        /// true, it will render them as actual clickable links if the target can be resolved.</value>
        public bool RenderReferenceLinks { get; set; }
        #endregion

        #region Abstract methods
        //=====================================================================

        /// <summary>
        /// Write a start block
        /// </summary>
        /// <param name="codeLanguage">The code language of the block</param>
        /// <param name="styleId">The style ID of the block</param>
        public abstract void WriteStartBlock(string codeLanguage, string styleId);

        /// <summary>
        /// Write a start sub-block
        /// </summary>
        /// <param name="classId">The style class ID of the sub-block</param>
        public abstract void WriteStartSubBlock(string classId);

        /// <summary>
        /// Write an end block
        /// </summary>
        public abstract void WriteEndBlock();

        /// <summary>
        /// Write an end sub-block
        /// </summary>
        public abstract void WriteEndSubBlock();

        /// <summary>
        /// Write a string value
        /// </summary>
        /// <param name="text">The string to write</param>
        public abstract void WriteString(string text);

        /// <summary>
        /// Write a string value with a style
        /// </summary>
        /// <param name="text">The string to write</param>
        /// <param name="style">The style class ID to use</param>
        public abstract void WriteStringWithStyle(string text, string style);

        /// <summary>
        /// Write a reference link
        /// </summary>
        /// <param name="reference">The reference link ID</param>
        /// <overloads>There are two overloads for this method</overloads>
        public abstract void WriteReferenceLink(string reference);

        /// <summary>
        /// Write a reference link with inner text
        /// </summary>
        /// <param name="reference">The reference link ID</param>
        /// <param name="text">The inner text of the link</param>
        public abstract void WriteReferenceLink(string reference, string text);

        /// <summary>
        /// Write a message include item
        /// </summary>
        /// <param name="message">The message include item ID</param>
        /// <param name="parameters">Optional parameters to add to the message include item</param>
        public abstract void WriteMessage(string message, IEnumerable<string> parameters);

        #endregion

        #region Virtual methods
        //=====================================================================

        /// <summary>
        /// Write out a blank line
        /// </summary>
        public virtual void WriteLine()
        {
            this.WriteString("\n");
        }

        /// <summary>
        /// Write out a keyword
        /// </summary>
        /// <param name="keyword">The keyword to write</param>
        public virtual void WriteKeyword(string keyword)
        {
            this.WriteStringWithStyle(keyword, "keyword");
        }

        /// <summary>
        /// Write out a parameter
        /// </summary>
        /// <param name="parameter">The parameter to write</param>
        public virtual void WriteParameter(string parameter)
        {
            this.WriteStringWithStyle(parameter, "parameter");
        }

        /// <summary>
        /// Write out an identifier
        /// </summary>
        /// <param name="identifier">The identifier to write</param>
        public virtual void WriteIdentifier(string identifier)
        {
            this.WriteStringWithStyle(identifier, "identifier");
        }

        /// <summary>
        /// Write out a literal
        /// </summary>
        /// <param name="literal">The literal to write</param>
        public virtual void WriteLiteral(string literal)
        {
            this.WriteStringWithStyle(literal, "literal");
        }

        /// <summary>
        /// Write out a message
        /// </summary>
        /// <param name="message">The message include item ID to write</param>
        public virtual void WriteMessage(string message)
        {
            this.WriteMessage(message, null);
        }
        #endregion
    }
}
