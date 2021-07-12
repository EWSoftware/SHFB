// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System.Xml;

namespace Sandcastle.Tools
{
    /// <summary>
    /// This delegate is used for MRefBuilder callback methods
    /// </summary>
    /// <param name="writer">The reflection data XML writer</param>
    /// <param name="info">An object containing information for the callback (context dependent)</param>
    public delegate void MRefBuilderCallback(XmlWriter writer, object info);
}
