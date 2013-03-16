// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 03/09/2013 - EFW - Moved the supporting syntax writer classes to the SyntaxComponents assembly project

using System.Xml.XPath;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This is the abstract base class for syntax generators
    /// </summary>
    public abstract class SyntaxGenerator
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">The syntax generator configuration</param>
        protected SyntaxGenerator(XPathNavigator configuration)
        {
        }

        /// <summary>
        /// This is implemented to write the syntax
        /// </summary>
        /// <param name="reflection">The reflection data used to generate the syntax</param>
        /// <param name="writer">The writer to which the syntax information is written</param>
        public abstract void WriteSyntax(XPathNavigator reflection, SyntaxWriter writer);
    }
}
