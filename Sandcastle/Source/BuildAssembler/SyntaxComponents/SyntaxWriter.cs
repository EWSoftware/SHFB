// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 03/09/2013 - EFW - Moved the supporting syntax writer classes to the SyntaxComponents assembly project

using System.Collections.Generic;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This is used as the abstract base class for syntax writers
    /// </summary>
    public abstract class SyntaxWriter
    {
        protected SyntaxWriter(XPathNavigator location)
        {
        }

        // Syntax block APIs

        public virtual int Position
        {
            get { return -1; }
        }

        public abstract void WriteStartBlock(string language);

        public abstract void WriteStartSubBlock(string classId);

        public abstract void WriteEndSubBlock();

        public abstract void WriteString(string text);

        public abstract void WriteStringWithStyle(string text, string style);

        public abstract void WriteReferenceLink(string reference);

        public abstract void WriteReferenceLink(string reference, string text);

        public virtual void WriteLine()
        {
            WriteString("\n");
        }

        public virtual void WriteKeyword(string keyword)
        {
            WriteStringWithStyle(keyword, "keyword");
        }

        public virtual void WriteParameter(string parameter)
        {
            WriteStringWithStyle(parameter, "parameter");
        }

        public virtual void WriteIdentifier(string identifier)
        {
            WriteStringWithStyle(identifier, "identifier");
        }

        public virtual void WriteLiteral(string literal)
        {
            WriteStringWithStyle(literal, "literal");
        }

        public virtual void WriteMessage(string message)
        {
            WriteMessage(message, null);
        }

        public abstract void WriteMessage(string message, IEnumerable<string> parameters);

        public abstract void WriteEndBlock();
    }
}
