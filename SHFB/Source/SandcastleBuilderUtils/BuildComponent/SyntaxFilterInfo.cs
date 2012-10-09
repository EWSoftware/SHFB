//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : SyntaxFilterInfo.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/10/2009
// Note    : Copyright 2009, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to provide information about the loaded
// language syntax filter build components.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.3  11/10/2009  EFW  Created the code
//=============================================================================

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml.XPath;

namespace SandcastleBuilder.Utils.BuildComponent
{
    /// <summary>
    /// This class contains information about the loaded language syntax filter
    /// build components.
    /// </summary>
    public class SyntaxFilterInfo
    {
        #region Private data members
        //=====================================================================

        private string id, generatorNode, languageNode;
        private int sortOrder;
        private Collection<string> alternateNames;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the ID of the syntax filter
        /// </summary>
        public string Id
        {
            get { return id; }
        }

        /// <summary>
        /// This read-only property returns the sort order of the syntax filter
        /// </summary>
        public int SortOrder
        {
            get { return sortOrder; }
        }

        /// <summary>
        /// This read-only property returns an optional list of alternate names
        /// that will map to this syntax filter.
        /// </summary>
        public Collection<string> AlternateNames
        {
            get { return alternateNames; }
        }

        /// <summary>
        /// This read-only property returns the XML to use for the generator
        /// XML node in the build component configuration file.
        /// </summary>
        public string GeneratorXml
        {
            get { return generatorNode; }
        }

        /// <summary>
        /// This read-only property returns the XML to use for the language
        /// XML node in the build component configuration file.
        /// </summary>
        public string LanguageXml
        {
            get { return languageNode; }
        }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="component">The XPath navigator containing the syntax
        /// filter component's configuration information</param>
        internal SyntaxFilterInfo(XPathNavigator component)
        {
            XPathNavigator item;
            string attrValue;

            id = component.GetAttribute("id", String.Empty);

            if(String.IsNullOrEmpty(id))
                throw new InvalidOperationException("Syntax filter configuration contains no 'id' attribute");

            attrValue = component.GetAttribute("sortOrder", String.Empty);

            // Sort order is optional
            if(String.IsNullOrEmpty(attrValue))
                sortOrder = Int32.MaxValue;
            else
                sortOrder = Convert.ToInt32(attrValue, CultureInfo.InvariantCulture);

            // Alternate names are optional
            alternateNames = new Collection<string>();
            attrValue = component.GetAttribute("alternateNames", String.Empty);

            if(!String.IsNullOrEmpty(attrValue))
                foreach(string n in attrValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    attrValue = n.Trim();

                    if(attrValue.Length != 0)
                        alternateNames.Add(attrValue.ToLowerInvariant());
                }

            item = component.SelectSingleNode("generator");
            if(item == null)
                throw new InvalidOperationException("Syntax filter configuration contains no 'generator' node");

            generatorNode = item.OuterXml;

            item = component.SelectSingleNode("language");
            if(item == null)
                throw new InvalidOperationException("Syntax filter configuration contains no 'generator' node");

            languageNode = item.OuterXml;
        }
    }
}
