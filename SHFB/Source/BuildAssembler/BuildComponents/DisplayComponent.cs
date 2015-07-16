// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/21/2012 - EFW - Updated to output the content as a diagnostic message using the WriteMessage() method
// rather than just dumping it to the console.
// 12/23/2013 - EFW - Updated the build component to be discoverable via MEF

using System;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Microsoft.Ddue.Tools.BuildComponent
{
    /// <summary>
    /// This component serves as a debugging aid.  It dumps the current document to the message log
    /// </summary>
    public class DisplayComponent : BuildComponentCore
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("Display Component")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new DisplayComponent(base.BuildAssembler);
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private string xpathFormat = "/";
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected DisplayComponent(BuildAssemblerCore buildAssembler) : base(buildAssembler)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>If an <c>xpath</c> element is found in the configuration, it's inner text will be used as
        /// a filter expression to select elements to dump.  If not found, the entire document is dumped.   The
        /// expression can have a single replacement parameter (<c>{0}</c>).  If present, it will be replaced
        /// with the current document key.</remarks>
        public override void Initialize(XPathNavigator configuration)
        {
            XPathNavigator xpathFormatNode = configuration.SelectSingleNode("xpath");

            if(xpathFormatNode != null)
                xpathFormat = xpathFormatNode.Value;
        }

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            object result = document.CreateNavigator().Evaluate(String.Format(CultureInfo.InvariantCulture,
                xpathFormat, key));

            if(result == null)
            {
                this.WriteMessage(key, MessageLevel.Diagnostic, "Null result");
                return;
            }

            XPathNodeIterator nodes = result as XPathNodeIterator;

            if(nodes != null)
            {
                foreach(XPathNavigator node in nodes)
                    this.WriteMessage(key, MessageLevel.Diagnostic, "\r\n" + node.OuterXml);

                return;
            }

            this.WriteMessage(key, MessageLevel.Diagnostic, "\r\n" + result.ToString());
        }
        #endregion
    }
}
