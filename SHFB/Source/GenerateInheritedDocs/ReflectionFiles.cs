//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : ReflectionFiles.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/27/2013
// Note    : Copyright 2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to track the reflection information files and their content.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.7.0  03/27/2013  EFW  Created the code
//===============================================================================================================

using System.Collections.Generic;
using System.Xml.XPath;

namespace SandcastleBuilder.InheritedDocumentation
{
    /// <summary>
    /// This is used to load one or more reflection data files used to look up API information
    /// </summary>
    public class ReflectionFiles
    {
        #region Private data members
        //=====================================================================

        private List<XPathNavigator> apiNodes;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This returns the number of reflection files
        /// </summary>
        public int Count
        {
            get { return apiNodes.Count; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public ReflectionFiles()
        {
            apiNodes = new List<XPathNavigator>();
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Add a reflection file to use for API information
        /// </summary>
        /// <param name="filename">The file to add</param>
        public void AddReflectionFile(string filename)
        {
            XPathDocument xpathDoc = new XPathDocument(filename);
            apiNodes.Add(xpathDoc.CreateNavigator().SelectSingleNode("reflection/apis"));
        }

        /// <summary>
        /// Find the single node that matches the given XPath query
        /// </summary>
        /// <param name="xpath">The XPath query used to find the node</param>
        /// <returns>The node if found or null if not found</returns>
        public XPathNavigator SelectSingleNode(string xpath)
        {
            XPathNavigator result;

            foreach(var nav in apiNodes)
            {
                result = nav.SelectSingleNode(xpath);

                if(result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Find all nodes that matches the given XPath query
        /// </summary>
        /// <param name="xpath">The XPath query used to find nodes</param>
        /// <returns>An enumerable list of matching nodes</returns>
        public IEnumerable<XPathNavigator> Select(string xpath)
        {
            foreach(var nav in apiNodes)
                foreach(XPathNavigator n in nav.Select(xpath))
                    yield return n;
        }
        #endregion
    }
}
