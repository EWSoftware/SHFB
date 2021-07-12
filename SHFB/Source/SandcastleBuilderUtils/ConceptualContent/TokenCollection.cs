//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : TokenCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/14/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains a collection class used to hold the conceptual content token information from a token file
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/24/2008  EFW  Created the code
// 07/25/2008  EFW  Reworked to support new MSBuild project format
// 12/22/2011  EFW  Updated for use with the new token file editor
//===============================================================================================================

// Ignore Spelling: xlink

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;

namespace SandcastleBuilder.Utils.ConceptualContent
{
    /// <summary>
    /// This collection class is used to hold the conceptual content tokens
    /// for an associated token file.
    /// </summary>
    public class TokenCollection : BindingList<Token>
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filename">The token file associated with the collection.</param>
        /// <remarks>Tokens are not loaded until the <see cref="Load" /> method
        /// is called.</remarks>
        public TokenCollection(string filename)
        {
            this.TokenFilePath = filename;
        }
        #endregion

        #region Properties
        //=====================================================================
        
        /// <summary>
        /// This is used to get or set the token file path
        /// </summary>
        public string TokenFilePath { get; set; }

        #endregion

        #region Sort and find methods
        //=====================================================================

        /// <summary>
        /// This is used to sort the collection
        /// </summary>
        /// <remarks>Values are sorted by token name and value.  Comparisons
        /// are case-sensitive.</remarks>
        public void Sort()
        {
            ((List<Token>)this.Items).Sort((x, y) =>
            {
                int result = String.Compare(x.TokenName, y.TokenName, StringComparison.Ordinal);

                if(result == 0)
                    result = String.Compare(x.TokenValue, y.TokenValue, StringComparison.Ordinal);

                return result;
            });
        }

        /// <summary>
        /// This is used to find all tokens that match the specified predicate
        /// </summary>
        /// <param name="match">The match predicate</param>
        /// <returns>An enumerable list of all matches</returns>
        public IEnumerable<Token> Find(Predicate<Token> match)
        {
            if(match == null)
                throw new ArgumentNullException(nameof(match));

            foreach(var t in this)
                if(match(t))
                    yield return t;
        }
        #endregion

        #region Read/write the token file
        //=====================================================================

        /// <summary>
        /// Load the collection from the related file
        /// </summary>
        /// <remarks>This will be done automatically at constructor.  This can
        /// be called to reload the collection if needed.</remarks>
        public void Load()
        {
            this.Clear();

            using(var xr = XmlReader.Create(this.TokenFilePath, new XmlReaderSettings { CloseInput = true }))
            {
                xr.MoveToContent();

                while(!xr.EOF)
                {
                    if(xr.NodeType == XmlNodeType.Element && xr.Name == "item")
                        this.Add(new Token(xr.GetAttribute("id"), xr.ReadInnerXml()));

                    xr.Read();
                }
            }

            this.Sort();
        }

        /// <summary>
        /// Save the token collection to its related file ready for use by
        /// <b>BuildAssembler</b>.
        /// </summary>
        public void Save()
        {
            using(var writer = XmlWriter.Create(this.TokenFilePath, new XmlWriterSettings { Indent = true,
              CloseOutput = true }))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("content");
                writer.WriteAttributeString("xml", "space", null, "preserve");
                writer.WriteAttributeString("xmlns", "ddue", null,
                    "http://ddue.schemas.microsoft.com/authoring/2003/5");
                writer.WriteAttributeString("xmlns", "xlink", null, "http://www.w3.org/1999/xlink");

                foreach(Token t in this)
                {
                    writer.WriteStartElement("item");
                    writer.WriteAttributeString("id", t.TokenName);

                    // The value is written as raw text to preserve any XML
                    // within it.  The token value is also trimmed to remove
                    // unnecessary whitespace that might affect the layout.
                    writer.WriteRaw(t.TokenValue.Trim());
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();   // </content>
                writer.WriteEndDocument();
            }
        }
        #endregion
    }
}
