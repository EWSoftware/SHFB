// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 03/09/2013 - EFW - Moved the supporting syntax writer classes to the SyntaxComponents assembly project
// 03/17/2013 - EFW - Added support for the RenderReferenceLinks property
// 12/21/2013 - EFW - Moved class to Sandcastle.Core assembly

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace Sandcastle.Core.BuildAssembler.SyntaxGenerator
{
    /// <summary>
    /// This is used to write out syntax for managed code
    /// </summary>
    public class ManagedSyntaxWriter : SyntaxWriter
    {
        #region Private data members
        //=====================================================================

        private XPathNavigator location;
        private XmlWriter writer;

        // position along the line
        private int position;

        #endregion

        #region Properties
        //=====================================================================

        /// <inheritdoc />
        public override int Position
        {
            get { return position; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="location">The location in which to write the output</param>
        public ManagedSyntaxWriter(XPathNavigator location)
        {
            if(location == null)
                throw new ArgumentNullException("location");

            this.location = location;
        }
        #endregion

        #region Abstract method implementation
        //=====================================================================

        /// <inheritdoc />
        public override void WriteStartBlock(string codeLanguage, string styleId)
        {
            writer = location.AppendChild();
            writer.WriteStartElement("div");
            writer.WriteAttributeString("codeLanguage", codeLanguage);
            writer.WriteAttributeString("style", styleId);
            position = 0;
        }

        /// <inheritdoc />
        public override void WriteStartSubBlock(string classId)
        {
            writer.WriteStartElement("div");
            writer.WriteAttributeString("class", classId);
            position = 0;
        }

        /// <inheritdoc />
        public override void WriteEndBlock()
        {
            writer.WriteEndElement();
            writer.Close();
            position = 0;
        }

        /// <inheritdoc />
        public override void WriteEndSubBlock()
        {
            writer.WriteEndElement();
            position = 0;
        }

        /// <inheritdoc />
        public override void WriteLine()
        {
            base.WriteLine();
            position = 0;
        }

        /// <inheritdoc />
        public override void WriteString(string text)
        {
            writer.WriteString(text);
            position += text.Length;
        }

        /// <inheritdoc />
        public override void WriteStringWithStyle(string text, string style)
        {
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", style);
            WriteString(text);
            writer.WriteEndElement();
            position += text.Length;
        }

        /// <inheritdoc />
        public override void WriteReferenceLink(string reference)
        {
            writer.WriteStartElement("referenceLink");
            writer.WriteAttributeString("target", reference);
            writer.WriteAttributeString("prefer-overload", "false");
            writer.WriteAttributeString("show-container", "false");
            writer.WriteAttributeString("show-templates", "false");
            writer.WriteAttributeString("show-parameters", "false");

            // Since we have no inner text, it will be up to the reference link component to render the link
            // accordingly.
            if(!base.RenderReferenceLinks)
                writer.WriteAttributeString("renderAsLink", "false");

            writer.WriteEndElement();
            position += 10; // approximate
        }

        /// <inheritdoc />
        public override void WriteReferenceLink(string reference, string text)
        {
            if(base.RenderReferenceLinks)
            {
                writer.WriteStartElement("referenceLink");
                writer.WriteAttributeString("target", reference);
                writer.WriteAttributeString("prefer-overload", "false");
                writer.WriteAttributeString("show-container", "false");
                writer.WriteAttributeString("show-templates", "false");
                writer.WriteAttributeString("show-parameters", "false");
                writer.WriteString(text);
                writer.WriteEndElement();

                position += text.Length;
            }
            else
                base.WriteIdentifier(text);
        }

        /// <inheritdoc />
        public override void WriteMessage(string message, IEnumerable<string> parameters)
        {
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", "message");
            writer.WriteStartElement("include");
            writer.WriteAttributeString("item", message);

            if(parameters != null)
                foreach(string parameter in parameters)
                {
                    writer.WriteStartElement("parameter");
                    writer.WriteRaw(parameter);
                    writer.WriteEndElement();
                }

            writer.WriteEndElement();
            writer.WriteEndElement();
        }
        #endregion
    }
}
