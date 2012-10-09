//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : WildcardReferenceSettingsCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/17/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a collection class used to hold the wildcard reference
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
// 1.9.2.0  01/17/2011  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.Xml.Linq;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This collection class is used to hold the wildcard reference settings
    /// information for the <see cref="WildcardReferencesPlugIn"/>.
    /// </summary>
    public class WildcardReferenceSettingsCollection : BindingList<WildcardReferenceSettings>
    {
        #region Convert to/from XML
        //=====================================================================

        /// <summary>
        /// This is used to save the wildcard reference settings list to the
        /// configuration settings.
        /// </summary>
        /// <param name="root">The root configuration node</param>
        public void ToXml(XElement root)
        {
            XElement node;

            if(root == null)
                throw new ArgumentNullException("root");

            node = root.Element("references");

            if(node == null)
            {
                node = new XElement("references");
                root.Add(node);
            }
            else
                node.RemoveAll();

            foreach(WildcardReferenceSettings wr in this)
                node.Add(wr.ToXml());
        }

        /// <summary>
        /// This is used to load the wildcard reference settings list from the
        /// configuration settings.
        /// </summary>
        /// <param name="pathProvider">The base path provider object</param>
        /// <param name="root">The XElement from which the information is loaded.</param>
        public void FromXml(IBasePathProvider pathProvider, XElement root)
        {
            if(root == null)
                throw new ArgumentNullException("root");

            foreach(var r in root.Element("references").Elements())
                this.Add(WildcardReferenceSettings.FromXPathNavigator(pathProvider, r));
        }
        #endregion
    }
}
