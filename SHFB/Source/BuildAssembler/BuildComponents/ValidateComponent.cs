// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/21/2012 - EFW - Updated the warning message to include the document key
// 12/24/2013 - EFW - Updated the build component to be discoverable via MEF

using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This component serves as a debugging aid.  It is used to validate the generated document against one
    /// or more XML schemas.
    /// </summary>
    public class ValidateComponent : BuildComponentCore
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("XML Schema Validation Component")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new ValidateComponent(base.BuildAssembler);
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private XmlSchemaSet schemas = new XmlSchemaSet();
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected ValidateComponent(BuildAssemblerCore buildAssembler) : base(buildAssembler)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>The configuration should contain one or more <c>schema</c> elements with a <c>file</c>
        /// attribute that specifies the XSD schema file to use.</remarks>
        public override void Initialize(XPathNavigator configuration)
        {
            foreach(XPathNavigator schema_node in configuration.Select("schema"))
                schemas.Add(null, schema_node.GetAttribute("file", String.Empty));
        }

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
