// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 03/09/2013 - EFW - Moved the supporting syntax writer classes to the SyntaxComponents assembly project

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This is used to write out syntax for managed code
    /// </summary>
    public class ManagedSyntaxWriter : SyntaxWriter
    {
        XPathNavigator location;
        XmlWriter writer;

        // position along the line
        int position = 0;

        public ManagedSyntaxWriter(XPathNavigator location) : base(location)
        {
            if(location == null)
                throw new ArgumentNullException("location");

            this.location = location;
        }

        public override int Position
        {
            get { return position; }
        }

        public override void WriteStartBlock(string language)
        {
            writer = location.AppendChild();
            writer.WriteStartElement("div");
            writer.WriteAttributeString("codeLanguage", language);
            position = 0;
        }

        public override void WriteStartSubBlock(string classId)
        {
            writer.WriteStartElement("div");
            writer.WriteAttributeString("class", classId);
            position = 0;
        }

        public override void WriteEndSubBlock()
        {
            writer.WriteEndElement();
            position = 0;
        }

        public override void WriteLine()
        {
            base.WriteLine();
            position = 0;
        }

        public override void WriteString(string text)
        {
            writer.WriteString(text);
            position += text.Length;
        }

        public override void WriteStringWithStyle(string text, string style)
        {
            writer.WriteStartElement("span");
            writer.WriteAttributeString("class", style);
            WriteString(text);
            writer.WriteEndElement();
            position += text.Length;
        }

        public override void WriteReferenceLink(string reference)
        {
            writer.WriteStartElement("referenceLink");
            writer.WriteAttributeString("target", reference);
            writer.WriteAttributeString("prefer-overload", "false");
            writer.WriteAttributeString("show-container", "false");
            writer.WriteAttributeString("show-templates", "false");
            writer.WriteAttributeString("show-parameters", "false");
            writer.WriteEndElement();
            position += 10; // approximate
        }

        public override void WriteReferenceLink(string reference, string text)
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

        public override void WriteEndBlock()
        {
            writer.WriteEndElement();
            writer.Close();
            position = 0;
        }

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
    }
}
