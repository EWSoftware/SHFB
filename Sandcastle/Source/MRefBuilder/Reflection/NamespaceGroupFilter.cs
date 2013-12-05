//===============================================================================================================
// System  : Sandcastle MRefBuilder Tool
// File    : NamespaceGroupFilter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/20/2013
// Compiler: Microsoft Visual C#
//
// This file contains a class that is used to enable namespace group filtering
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.   This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 2.7.5.0  11/20/2013  EFW  Merged code from Stazzz to implement namespace grouping support
//===============================================================================================================

using System;
using System.Globalization;
using System.Xml;

namespace Microsoft.Ddue.Tools.Reflection
{
    /// <summary>
    /// This class implements the namespace group filter
    /// </summary>
    public class NamespaceGroupFilter
    {
        #region Private data members
        //=====================================================================

        private string name;
        private bool exposed;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">The XML reader from which to read the configuration</param>
        public NamespaceGroupFilter(XmlReader configuration)
        {
            if(configuration.Name != "namespaceGroup")
                throw new InvalidOperationException("The configuration element must be named 'namespaceGroup'");

            this.name = configuration.GetAttribute("name");
            this.exposed = Convert.ToBoolean(configuration.GetAttribute("expose"), CultureInfo.InvariantCulture);
        }
        #endregion

        #region Methods

        /// <summary>
        /// Check to see if the namespace group is exposed or not by this entry
        /// </summary>
        /// <param name="namespaceGroup">The group namespace to check</param>
        /// <returns>Null if the group namespace is not represented by this entry, true if it is and it is
        /// exposed or false if it is and it is not exposed.</returns>
        public bool? IsExposedNamespaceGroup(String namespaceGroup)
        {
            return (namespaceGroup == name) ? exposed : (bool?)null;
        }
        #endregion
    }
}
