//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ReferenceLinkSettingsCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/13/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a collection class used to hold the reference link
// settings information.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.5  02/25/2008  EFW  Created the code
// 1.8.0.0  08/13/2008  EFW  Updated to support the new project format
//=============================================================================

using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This collection class is used to hold the reference link settings
    /// information for the <see cref="AdditionalReferenceLinksPlugIn"/>.
    /// </summary>
    public class ReferenceLinkSettingsCollection : BindingList<ReferenceLinkSettings>
    {
        #region Convert to/from XML
        //=====================================================================

        /// <summary>
        /// This is used to save the reference link settings list to the
        /// configuration settings.
        /// </summary>
        /// <param name="config">The XML configuration document.</param>
        /// <param name="root">The root configuration node</param>
        public void ToXml(XmlDocument config, XmlNode root)
        {
            XmlNode node;

            node = root.SelectSingleNode("targets");

            if(node == null)
            {
                node = config.CreateNode(XmlNodeType.Element, "targets", null);
                root.AppendChild(node);
            }
            else
                node.RemoveAll();

            foreach(ReferenceLinkSettings rl in this)
                node.AppendChild(rl.ToXml(config, node));
        }

        /// <summary>
        /// This is used to load the reference link settings list from the
        /// configuration settings.
        /// </summary>
        /// <param name="pathProvider">The base path provider object</param>
        /// <param name="navigator">The XPath navigator from which the
        /// information is loaded.</param>
        public void FromXml(IBasePathProvider pathProvider,
          XPathNavigator navigator)
        {
            XPathNodeIterator iterator;

            if(navigator == null)
                throw new ArgumentNullException("navigator");

            iterator = navigator.Select("targets/target");
            foreach(XPathNavigator nav in iterator)
                this.Add(ReferenceLinkSettings.FromXPathNavigator(pathProvider,
                    nav));
        }
        #endregion
    }
}
