//=============================================================================
// System  : Sandcastle Help File Builder
// File    : BaseContentEditor.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/28/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the base content editor form.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  07/26/2008  EFW  Created the code
//=============================================================================

using System;
using System.Windows.Forms;

using WeifenLuo.WinFormsUI.Docking;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This is used as the base class for the Sandcastle Builder content
    /// editors.
    /// </summary>
    /// <remarks>There are probably several different ways to do this that are
    /// better but this is quick and dirty and it works.</remarks>
    public class BaseContentEditor : DockContent
    {
        #region Private and internal data members
        //=====================================================================

        private bool isDirty;

        /// <summary>
        /// This is used to pass a delegate on the clipboard used to obtain
        /// data that is too complex to serialize directly to the clipboard.
        /// </summary>
        /// <returns>The actual object that is to be pasted from the
        /// clipboard.</returns>
        /// <remarks>The method passed in the delegate must be a static method.
        /// If not, the containing type has to be serializable and it probably
        /// isn't cos its too complex and the whole point of this is to work
        /// around that problem.  Yes, it is a hack but it works.</remarks>
        internal delegate object ClipboardDataHandler();
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This returns the dirty state of the document if it supports saving
        /// </summary>
        public virtual bool IsDirty
        {
            get { return isDirty; }
            set
            {
                isDirty = value;

                if(!this.CanSaveContent && isDirty)
                    throw new InvalidOperationException("The content cannot " +
                        "be saved and thus cannot be marked dirty");
            }
        }

        /// <summary>
        /// Returns true if the window can be closed or false if something
        /// prevents it from allowing the close to occur.
        /// </summary>
        public virtual bool CanClose
        {
            get { return true; }
        }

        /// <summary>
        /// This is used to see if the active document allows its content to be
        /// saved.
        /// </summary>
        /// <returns>Returns true if it supports saving its content or false
        /// if it does not.</returns>
        public virtual bool CanSaveContent
        {
            get { return false; }
        }

        /// <summary>
        /// This is used to see if the editor is a document type that should be
        /// saved prior to doing a build.
        /// </summary>
        public virtual bool IsContentDocument
        {
            get { return false; }
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// Save the content of the window using its current filename
        /// </summary>
        /// <returns>True if successful, false if not successful</returns>
        public virtual bool Save()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Save the content of the window using a different filename
        /// </summary>
        /// <returns>True if successful, false if not successful</returns>
        public virtual bool SaveAs()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
