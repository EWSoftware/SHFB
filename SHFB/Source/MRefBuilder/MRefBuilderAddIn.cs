// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System.Xml.XPath;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This serves as the base class for MRefBuilder add-ins
    /// </summary>
    public class MRefBuilderAddIn
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="writer">The API visitor and reflection data writer</param>
        /// <param name="configuration">An XPath navigator containing the add-in configuration</param>
        /// <remarks>For this base class, this does nothing</remarks>
        protected MRefBuilderAddIn(ManagedReflectionWriter writer, XPathNavigator configuration)
        {
        }
    }
}
