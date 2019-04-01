//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BindingRedirectSettingsCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/30/2019
// Note    : Copyright 2008-2019, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a collection class used to hold the binding redirect settings information
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/14/2008  EFW  Created the code
//===============================================================================================================

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
        /// <param name="relativePath">True to allow a relative path on <c>importFrom</c> attributes, false to
        /// fully qualify the path.</param>
        public void ToXml(XmlDocument config, XmlNode root, bool relativePath)
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
                node.AppendChild(brs.ToXml(config, node, relativePath));
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
                throw new ArgumentNullException(nameof(navigator));

            iterator = navigator.Select("assemblyBinding/dependentAssembly");

            foreach(XPathNavigator nav in iterator)
                this.Add(BindingRedirectSettings.FromXPathNavigator(pathProvider, nav));
        }
        #endregion
    }
}
