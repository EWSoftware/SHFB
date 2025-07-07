// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 03/09/2013 - EFW - Moved the class into the Snippets namespace and made it public

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sandcastle.Tools.BuildComponents.Snippets
{
    /// <summary>
    /// This represents a snippet identifier
    /// </summary>
    public readonly struct SnippetIdentifier
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the example ID
        /// </summary>
        public string ExampleId { get; }

        /// <summary>
        /// This read-only property returns the snippet ID
        /// </summary>
        public string SnippetId { get; }

        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor.  This version takes an example ID and a snippet ID
        /// </summary>
        /// <param name="exampleId">The example ID</param>
        /// <param name="snippetId">The snippet ID</param>
        /// <overloads>There are two overloads for the constructor</overloads>
        public SnippetIdentifier(string exampleId, string snippetId)
        {
            if(exampleId == null)
                throw new ArgumentNullException(nameof(exampleId));

            if(snippetId == null)
                throw new ArgumentNullException(nameof(snippetId));

            this.ExampleId = exampleId.ToLowerInvariant();
            this.SnippetId = snippetId.ToLowerInvariant();
        }

        /// <summary>
        /// Constructor.  This parses the example and snippet IDs from the given identifier
        /// </summary>
        /// <param name="identifier">The identifier to use.  This should contain the example ID and the
        /// snippet ID in that order separated by a hash character (#).</param>
        public SnippetIdentifier(string identifier)
        {
            if(identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            int index = identifier.LastIndexOf('#');

            this.ExampleId = identifier.Substring(0, index).ToLowerInvariant();
            this.SnippetId = identifier.Substring(index + 1).ToLowerInvariant();
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// This returns the snippet identifier
        /// </summary>
        /// <returns>The example ID and snippet ID separated by a hash character (#)</returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}#{1}", this.ExampleId, this.SnippetId);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to parse a snippet identifier containing an example ID and one or more snippet IDs
        /// in a comma-separated list (i.e. ExampleID#SnippetID or ExampleID#SnippetID1,SnippetID2).
        /// </summary>
        /// <param name="reference">The reference to parse</param>
        /// <returns>An enumerable list of snippet identifiers</returns>
        public static IEnumerable<SnippetIdentifier> ParseReference(string reference)
        {
            if(reference == null)
                throw new ArgumentNullException(nameof(reference));

            int index = reference.IndexOf('#');

            if(index > -1)
            {
                string example = reference.Substring(0, index);

                foreach(string id in reference.Substring(index + 1).Split([','],
                  StringSplitOptions.RemoveEmptyEntries))
                {
                    yield return new SnippetIdentifier(example, id.Trim());
                }
            }
        }
        #endregion
    }
}
