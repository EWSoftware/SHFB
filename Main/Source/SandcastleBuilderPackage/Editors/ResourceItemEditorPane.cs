//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : ResourceItemFileEditorPane.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/31/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used to host the resource item  file editor
// control.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.3  12/26/2011  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.Shell.Interop;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.ConceptualContent;
using SandcastleBuilder.WPF;
using SandcastleBuilder.WPF.UserControls;

namespace SandcastleBuilder.Package.Editors
{
    /// <summary>
    /// This is used to host the resource item file editor control
    /// </summary>
    public class ResourceItemEditorPane : SimpleEditorPane<ResourceItemEditorFactory, ResourceItemEditorControl>
    {
        #region Private data members
        //=====================================================================

        private string resourceItemFilename;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public ResourceItemEditorPane()
        {
            base.UIControl.ContentModified += ucResourceItemEditor_ContentModified;
        }
        #endregion

        #region Abstract method implementations
        //=====================================================================

        /// <inheritdoc />
        protected override string GetFileExtension()
        {
            return ".items";
        }

        /// <inheritdoc />
        protected override Guid GetCommandSetGuid()
        {
            return Guid.Empty;
        }

        /// <inheritdoc />
        protected override void LoadFile(string fileName)
        {
            SandcastleProject project = null;
            bool disposeOfProject = false;

            resourceItemFilename = fileName;

            try
            {
                // Get the current project so that the editor knows what presentation style items to load
                project = SandcastleBuilderPackage.CurrentSandcastleProject;

                if(project == null)
                {
                    // If there is no current project, create a dummy project to use
                    disposeOfProject = true;
                    project = new SandcastleProject("__TempProject__.shfbproj", false);
                }

                base.UIControl.LoadResourceItemsFile(fileName, project);
            }
            finally
            {
                if(disposeOfProject)
                    project.Dispose();
            }
        }

        /// <inheritdoc />
        protected override void SaveFile(string fileName)
        {
            Utility.GetServiceFromPackage<IVsUIShell, SVsUIShell>(true).SetWaitCursor();

            base.UIControl.CommitChanges();

            if(base.IsDirty || !fileName.Equals(resourceItemFilename, StringComparison.OrdinalIgnoreCase))
            {
                resourceItemFilename = fileName;
                base.UIControl.Save(fileName);
            }
        }
        #endregion

        #region General event handlers
        //=====================================================================

        /// <summary>
        /// This is used to mark the file as dirty when the collection changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ucResourceItemEditor_ContentModified(object sender, System.Windows.RoutedEventArgs e)
        {
            base.OnContentChanged();
        }
        #endregion
    }
}
