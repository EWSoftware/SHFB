//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : NodeData.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/22/2025
// Note    : Copyright 2008-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to hold tree view node data for the project explorer
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/28/2008  EFW  Created the code
//===============================================================================================================

using Sandcastle.Core.Project;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This is used to hold basic information about the item represented by the tree view node in the
    /// <see cref="ProjectExplorerWindow" />.
    /// </summary>
    public class NodeData
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// The build action
        /// </summary>
        public BuildAction BuildAction { get; }

        /// <summary>
        /// The build item for the node
        /// </summary>
        public object Item { get; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="action">The build action</param>
        /// <param name="item">The node item</param>
        public NodeData(BuildAction action, object item)
        {
            this.BuildAction = action;
            this.Item = item;
        }
        #endregion
    }
}
