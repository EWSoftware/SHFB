//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BindingRedirectSettingsCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/30/2012
// Note    : Copyright 2008-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a collection class used to hold the binding redirect
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
// 1.8.0.1  11/14/2008  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This collection class is used to hold the binding redirect settings information for the
    /// <see cref="BindingRedirectResolverPlugIn"/>.
    /// </summary>
    public class BindingRedirectSettingsCollection : BindingList<BindingRedirectSettings>
    {
        #region Convert to/from XML
        //=====================================================================

        /// <summary>
        /// This is used to save the binding redirect settings list to the configuration settings
        /// </summary>
        /// <param name="config">The XML configuration document.</param>
        /// <param name="root">The root configuration node</param>
        public void ToXml(XmlDocument config, XmlNode root)
        {
            XmlNode node;

            node = root.SelectSingleNode("assemblyBinding");

            if(node == null)
            {
                node = config.CreateNode(XmlNodeType.Element, "assemblyBinding", null);
                root.AppendChild(node);
            }
            else
                node.RemoveAll();

            foreach(BindingRedirectSettings brs in this)
                node.AppendChild(brs.ToXml(config, node));
        }

        /// <summary>
        /// This is used to load the binding redirect settings list from the configuration settings
        /// </summary>
        /// <param name="pathProvider">The base path provider object</param>
        /// <param name="navigator">The XPath navigator from which the information is loaded</param>
        public void FromXml(IBasePathProvider pathProvider, XPathNavigator navigator)
        {
            XPathNodeIterator iterator;

            if(navigator == null)
                throw new ArgumentNullException("navigator");

            iterator = navigator.Select("assemblyBinding/dependentAssembly");

            foreach(XPathNavigator nav in iterator)
                this.Add(BindingRedirectSettings.FromXPathNavigator(pathProvider, nav));
        }
        #endregion
    }
}
