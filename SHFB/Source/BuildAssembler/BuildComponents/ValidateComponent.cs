// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/21/2012 - EFW - Updated the warning message to include the document key

using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This component serves as a debugging aid.  It is used to validate the generated document against one
    /// or more XML schemas.
    /// </summary>
    public class ValidateComponent : BuildComponent
    {
        #region Private data members
        //=====================================================================

        private XmlSchemaSet schemas = new XmlSchemaSet();
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembler">The build assembler reference</param>
        /// <param name="configuration">The component configuration</param>
        /// <remarks>The configuration should contain one or more <c>schema</c> elements with a <c>file</c>
        /// attribute that specifies the XSD schema file to use.</remarks>
        public ValidateComponent(BuildAssembler assembler, XPathNavigator configuration) :
          base(assembler, configuration)
        {
            foreach(XPathNavigator schema_node in configuration.Select("schema"))
                schemas.Add(null, schema_node.GetAttribute("file", String.Empty));
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            // Set the validation schemas
            document.Schemas = schemas;

            // Validate the document
            document.Validate((sender, e) =>
            {
                this.WriteMessage(key, MessageLevel.Warn, e.Message);
            });
        }
        #endregion
    }
}
