//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : PropertyBasedCollectionItem.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/12/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing a collection item that is stored in
// an MSBuild property element.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  07/01/2008  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This is the base class for collection items that are stored in an
    /// MSBuild property element.
    /// </summary>
    public abstract class PropertyBasedCollectionItem
    {
        #region Private data members
        //=====================================================================

        private SandcastleProject projectFile;
        private bool isDirty;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get the owning project file
        /// </summary>
        [Browsable(false)]
        public SandcastleProject ProjectFile
        {
            get { return projectFile; }
        }

        /// <summary>
        /// This is used to get or set the dirty state of the item
        /// </summary>
        [Browsable(false)]
        public bool IsDirty
        {
            get { return isDirty; }
            set { isDirty = value; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">The owning project</param>
        protected PropertyBasedCollectionItem(SandcastleProject project)
        {
            projectFile = project;
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// This is used to see if the project can be edited.  If not, abort
        /// the change by throwing an exception.
        /// </summary>
        protected void CheckProjectIsEditable()
        {
            // If not associated with a project, allow it by default
            if(projectFile == null)
                isDirty = true;
            else
            {
                CancelEventArgs ce = new CancelEventArgs();
                projectFile.OnQueryEditProjectFile(ce);

                if(ce.Cancel)
                    throw new OperationCanceledException(
                        "Project cannot be edited");

                isDirty = true;
                projectFile.MarkAsDirty();
            }
        }
        #endregion
    }
}
