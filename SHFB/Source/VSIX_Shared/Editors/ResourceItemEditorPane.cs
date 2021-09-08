﻿//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : ResourceItemFileEditorPane.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/02/2018
// Note    : Copyright 2011-2018, Eric Woodruff, All rights reserved
//
// This file contains a class used to host the resource item  file editor control
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/26/2011  EFW  Created the code
//===============================================================================================================

using System;

using Microsoft.VisualStudio.Shell.Interop;

using SandcastleBuilder.Utils;
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
#pragma warning disable VSTHRD010
                project = SandcastleBuilderPackage.CurrentSandcastleProject;
#pragma warning restore VSTHRD010

                if(project == null)
                {
                    // If there is no current project, create a dummy project to use
                    disposeOfProject = true;
                    project = new SandcastleProject("__TempProject__.shfbproj", false, false);
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
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

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
