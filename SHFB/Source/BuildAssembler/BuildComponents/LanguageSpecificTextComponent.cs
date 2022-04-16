﻿//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : LanguageSpecificTextComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/28/2022
// Note    : Copyright 2014-2022, Eric Woodruff, All rights reserved
//
// This file contains a build component that is used to convert span style language-specific text elements to
// the script style elements used in HTML-based presentation styles.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/23/2014  EFW  Created the code
//===============================================================================================================

using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Sandcastle.Tools.BuildComponents
{
    /// <summary>
    /// This component is used to convert the span style language-specific text elements to the script style
    /// elements used in the HTML-based presentation styles.
    /// </summary>
    /// <remarks>This is handled after topic transformation as other components such as the Resolve Reference
    /// Links component may insert additional language-specific text elements after transformation has
    /// occurred.</remarks>
    public class LanguageSpecificTextComponent : BuildComponentCore
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("Language-Specific Text Component")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new LanguageSpecificTextComponent(this.BuildAssembler);
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected LanguageSpecificTextComponent(IBuildAssembler buildAssembler) : base(buildAssembler)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            // This component has no configuration settings
        }

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            if(document == null)
                throw new ArgumentNullException(nameof(document));

            if(key == null)
                throw new ArgumentNullException(nameof(key));

            string idRoot = $"LST{key.GetHashCodeDeterministic():X}_";
            int sequence = 0;

            foreach(XmlNode lstNode in document.SelectNodes("//span[@class='languageSpecificText']"))
            {
                XmlNodeList langList = lstNode.SelectNodes("span[@class]");

                // The language span count should match the total element count.  If not, we'll let it pass
                // through as is.
                if(langList.Count > 0 && langList.Count == lstNode.ChildNodes.Count)
                {
                    string uniqueId = idRoot + sequence.ToString(CultureInfo.InvariantCulture),
                        langParam = String.Join("|", langList.Cast<XmlNode>().Select(
                            n => String.Concat(n.Attributes["class"].Value, "=", n.InnerText)));

                    XmlElement spanElement = document.CreateElement("span");
                    spanElement.SetAttribute("id", uniqueId);
                    spanElement.SetAttribute("data-languageSpecificText", langParam);

                    // Set the inner text to an empty string so that it is written out with full opening and
                    // closing elements.
                    spanElement.InnerText = String.Empty;

                    lstNode.ParentNode.ReplaceChild(spanElement, lstNode);

                    sequence++;
                }
            }
        }
        #endregion
    }
}
