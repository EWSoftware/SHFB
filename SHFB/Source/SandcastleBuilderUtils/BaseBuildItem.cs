//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BaseBuildItem.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/08/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a base wrapper class for a build item in the project.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  06/23/2008  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This is a base wrapper class for a build item in the project
    /// </summary>
    public abstract class BaseBuildItem
    {
        #region Private data members
        //=====================================================================

        private ProjectElement projElement;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This returns the project element associated with the instance
        /// </summary>
        [Browsable(false)]
        public ProjectElement ProjectElement
        {
            get { return projElement; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="element">The project element associated with the
        /// instance</param>
        protected BaseBuildItem(ProjectElement element)
        {
            projElement = element;
        }
        #endregion
    }
}
