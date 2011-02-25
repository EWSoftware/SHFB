//=============================================================================
// System  : Sandcastle Help File Builder Components
// File    : MSHelpAttrComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/23/2010
// Note    : Copyright 2008-2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a build component that is used to add additional MS
// Help 2 attributes to the XML data island in each generated API topic.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.7  04/07/2008  EFW  Created the code
// 1.9.0.0  06/19/2010  EFW  Attributes are now optional.  The component will
//                           do nothing if none are specified.
//=============================================================================

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This build component is used to add additional MS Help 2 attributes
    /// to the XML data island in each generated API topic.
    /// </summary>
    /// <example>
    /// <code lang="xml" title="Example configuration">
    /// &lt;!-- MS Help 2 attribute configuration.  This must appear after
    ///      the TransformComponent. --&gt;
    /// &lt;component type="SandcastleBuilder.Components.MSHelpAttrComponent"
    ///   assembly="C:\SandcastleBuilder\SandcastleBuilder.Components.dll"&gt;
    ///     &lt;!-- Additional attributes.  If no attributes are specified,
    ///          the component will do nothing. --&gt;
    ///     &lt;attributes&gt;
    ///         &lt;!-- The "name" attribute is required.  The "value" attribute
    ///              is optional. --&gt;
    ///         &lt;attribute name="DocSet" value="NETFramework" / &gt;
    ///         &lt;attribute name="DocSet" value="ProjectNamespace" / &gt;
    ///         &lt;attribute name="TargetOS" value="Windows" / &gt;
    ///     &lt;/attributes&gt;
    /// &lt;/component&gt;
    /// </code>
    /// </example>
    public class MSHelpAttrComponent : BuildComponent
    {
        #region Private data members
        //=====================================================================
        // Private data members

        private List<KeyValuePair<string, string>> attributes;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembler">A reference to the build assembler.</param>
        /// <param name="configuration">The configuration information</param>
        /// <remarks>See the <see cref="MSHelpAttrComponent"/> class topic
        /// for an example of the configuration</remarks>
        /// <exception cref="ConfigurationErrorsException">This is thrown if
        /// an error is detected in the configuration.</exception>
        public MSHelpAttrComponent(BuildAssembler assembler,
          XPathNavigator configuration) : base(assembler, configuration)
        {
            XPathNodeIterator attrs;
            string name, value;

            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            attributes = new List<KeyValuePair<string, string>>();

            base.WriteMessage(MessageLevel.Info, String.Format(CultureInfo.InvariantCulture,
                "\r\n    [{0}, version {1}]\r\n    MS Help 2 Attribute Component. {2}\r\n    http://SHFB.CodePlex.com",
                fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright));

            // At least one attribute element is required to do anything
            attrs = configuration.Select("attributes/attribute");

            if(attrs.Count == 0)
                base.WriteMessage(MessageLevel.Info, "No additional help attributes found, this component " +
                    "will not do anything.");
            else
            {
                // A name is required.  Value is optional.
                foreach(XPathNavigator nav in attrs)
                {
                    name = nav.GetAttribute("name", String.Empty);
                    value = nav.GetAttribute("value", String.Empty);

                    if(String.IsNullOrEmpty(name))
                        throw new ConfigurationErrorsException(
                            "A 'name' attribute is required on the <attribute> " +
                            "element");

                    attributes.Add(new KeyValuePair<string, string>(name, value));
                }

                base.WriteMessage(MessageLevel.Info, String.Format(CultureInfo.InvariantCulture,
                    "Loaded {0} attributes", attributes.Count));
            }
        }
        #endregion

        #region Apply the component
        /// <summary>
        /// This is implemented to add the attributes to the XML data island.
        /// </summary>
        /// <param name="document">The XML document with which to work.</param>
        /// <param name="key">The key (member name) of the item being documented.</param>
        public override void Apply(XmlDocument document, string key)
        {
            XmlNode dataIsland, node;
            XmlAttribute attr;

            if(attributes.Count != 0)
            {
                XmlNamespaceManager nsm = new XmlNamespaceManager(document.NameTable);
                nsm.AddNamespace("MSHelp", "http://msdn.microsoft.com/mshelp");

                dataIsland = document.SelectSingleNode("html/head/xml");

                if(dataIsland == null)
                    base.WriteMessage(MessageLevel.Warn, "<xml> element was not found.  The additional " +
                        "attributes cannot be added.");
                else
                    foreach(KeyValuePair<string, string> pair in attributes)
                    {
                        node = document.CreateNode(XmlNodeType.Element, "MSHelp:Attr", nsm.LookupNamespace("MSHelp"));

                        attr = document.CreateAttribute("Name");
                        attr.Value = pair.Key;
                        node.Attributes.Append(attr);

                        attr = document.CreateAttribute("Value");
                        attr.Value = pair.Value;
                        node.Attributes.Append(attr);

                        dataIsland.AppendChild(node);
                    }
            }
        }
        #endregion
    }
}
