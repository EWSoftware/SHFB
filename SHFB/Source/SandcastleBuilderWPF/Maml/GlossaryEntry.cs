//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : GlossaryEntry.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/17/2021
// Note    : Copyright 2012-2021, Eric Woodruff, All rights reserved
//
// This file contains the class used hold information about a glossary entry
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/16/2012  EFW  Created the code
//===============================================================================================================

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SandcastleBuilder.WPF.Maml
{
    /// <summary>
    /// This class is used to hold information about a glossary entry
    /// </summary>
    internal class GlossaryEntry
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the term dictionary
        /// </summary>
        /// <value>The key is the term, the value is an optional term ID used for linking</value>
        public Dictionary<string, string> Terms { get; } = new Dictionary<string, string>();

        /// <summary>
        /// This read-only property returns an optional list of related entries
        /// </summary>
        public List<string> RelatedEntries { get; } = new List<string>();

        /// <summary>
        /// This is used to get or set the parent division of the entry
        /// </summary>
        public XElement Parent { get; set; }

        /// <summary>
        /// This is used to get or set the element containing the definition
        /// </summary>
        public XElement Definition { get; set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="glossaryEntry">The glossary entry element to represent</param>
        public GlossaryEntry(XElement glossaryEntry)
        {
            XNamespace ddue = MamlToFlowDocumentConverter.ddue;

            this.Parent = glossaryEntry.Parent;

            foreach(var t in glossaryEntry.Descendants(ddue + "term"))
                this.Terms.Add(t.Value.Trim(), t.Attribute("termId")?.Value.Trim());

            this.Definition = glossaryEntry.Descendants(ddue + "definition").First();
            this.RelatedEntries.AddRange(glossaryEntry.Descendants(
                ddue + "relatedEntry").Attributes("termId").Select(a => a.Value));
        }
        #endregion
    }
}
