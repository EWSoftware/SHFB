//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : VersionSettingsCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/07/2010
// Note    : Copyright 2007-2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a collection class used to hold the version settings
// information.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.3  12/01/2007  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This collection class is used to hold the version settings information
    /// for the <see cref="VersionBuilderPlugIn"/>.
    /// </summary>
    public class VersionSettingsCollection : BindingList<VersionSettings>
    {
        #region Convert to/from XML
        //=====================================================================

        /// <summary>
        /// This is used to save the version settings list to the configuration
        /// settings.
        /// </summary>
        /// <param name="config">The XML configuration document.</param>
        /// <param name="root">The root configuration node</param>
        public void ToXml(XmlDocument config, XmlNode root)
        {
            XmlNode node;

            node = root.SelectSingleNode("versions");

            if(node == null)
            {
                node = config.CreateNode(XmlNodeType.Element, "versions", null);
                root.AppendChild(node);
            }
            else
                node.RemoveAll();

            foreach(VersionSettings vs in this)
                node.AppendChild(vs.ToXml(config, node));
        }

        /// <summary>
        /// This is used to load the version settings list from the
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

            iterator = navigator.Select("versions/version");
            foreach(XPathNavigator nav in iterator)
                this.Add(VersionSettings.FromXPathNavigator(pathProvider, nav));
        }
        #endregion

        #region Sort the collection
        //=====================================================================

        /// <summary>
        /// This is used to sort the collection by framework label in ascending
        /// order and by version in descending order.
        /// </summary>
        /// <remarks>The collection must be sorted in descending order by
        /// version in order for the version builder tool to output the correct
        /// information.</remarks>
        public void Sort()
        {
            ((List<VersionSettings>)base.Items).Sort((x, y) =>
            {
                int result;

                result = String.Compare(x.FrameworkLabel, y.FrameworkLabel,
                    false, CultureInfo.CurrentCulture);

                if(result == 0)
                    result = String.Compare(x.Version, y.Version,
                        false, CultureInfo.CurrentCulture) * -1;

                return result;
            });
        }
        #endregion
    }
}
