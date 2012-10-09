//=============================================================================
// System  : Sandcastle Help File Builder
// File    : NodeData.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/01/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to hold tree view node data for the
// project explorer.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  07/28/2008  EFW  Created the code
//=============================================================================

using System;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This is used to hold basic information about the item represented
    /// by the tree view node in the <see cref="ProjectExplorerWindow" />.
    /// </summary>
    public class NodeData
    {
        #region Private data members
        //=====================================================================

        private BuildAction buildAction;
        private object nodeItem, nodeProperties;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// The build action
        /// </summary>
        public BuildAction BuildAction
        {
            get { return buildAction; }
        }

        /// <summary>
        /// The build item for the node
        /// </summary>
        public object Item
        {
            get { return nodeItem; }
        }

        /// <summary>
        /// The properties for the object
        /// </summary>
        /// <value>If a separate properties object is not specified,
        /// this will return the item itself.</value>
        public object Properties
        {
            get
            {
                if(nodeProperties != null)
                    return nodeProperties;

                return nodeItem;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="action">The build action</param>
        /// <param name="item">The node item</param>
        /// <param name="properties">The node properties</param>
        public NodeData(BuildAction action, object item, object properties)
        {
            buildAction = action;
            nodeItem = item;
            nodeProperties = properties;
        }
        #endregion
    }
}
