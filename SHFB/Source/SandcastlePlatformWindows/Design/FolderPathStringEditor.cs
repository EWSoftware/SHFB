//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : FolderPathStringEditor.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/16/2021
// Note    : Copyright 2006-2021, Eric Woodruff, All rights reserved
//
// This file contains a type editor that can display a folder browser dialog to allow selection of a folder path
// at design time.  This can be used in conjunction with the FolderDialogAttribute to specify the folder browser
// dialog's properties.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/02/2006  EFW  Created the code
// 05/16/2021  EFW  Moved the code to the Windows platform assembly from SandcastleBuilder.Utils
//===============================================================================================================

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;

namespace Sandcastle.Platform.Windows.Design
{
    /// <summary>
    /// This is a type editor that can display a folder browser dialog to allow selection of a folder path at
    /// design time.
    /// </summary>
    /// <remarks>This can be used in conjunction with the <see cref="FolderDialogAttribute" /> to specify the
    /// folder browser dialog's properties.</remarks>
    public class FolderPathStringEditor : UITypeEditor
    {
        /// <summary>
        /// This is overridden to edit the value using a folder browser dialog.
        /// </summary>
        /// <param name="context">The descriptor context</param>
        /// <param name="provider">The provider</param>
        /// <param name="value">The folder path as an object</param>
        /// <returns>The selected folder path as an object</returns>
        [RefreshProperties(RefreshProperties.All)]
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            string currentFolder;

            if(context == null || provider == null || context.Instance == null || (value != null && !(value is string)))
                return base.EditValue(context, provider, value);

            currentFolder = (string)value;

            if(String.IsNullOrEmpty(currentFolder))
                currentFolder = String.Empty;

            using(FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                // See if we have been supplied a title
                if(context.PropertyDescriptor.Attributes[typeof(FolderDialogAttribute)] is FolderDialogAttribute fda)
                {
                    dlg.Description = fda.Description;
                    dlg.ShowNewFolderButton = fda.ShowNewFolderButton;
                    dlg.RootFolder = fda.RootFolder;
                    dlg.SelectedPath = Environment.GetFolderPath(
                        fda.DefaultFolder);
                }
                else
                    dlg.Description = "Select " + context.PropertyDescriptor.DisplayName;

                if(currentFolder.Length != 0)
                    dlg.SelectedPath = Path.GetFullPath(currentFolder);

                // If selected, set the new filename
                if(dlg.ShowDialog() == DialogResult.OK)
                    value = dlg.SelectedPath;
            }

            return value;
        }

        /// <summary>
        /// This is overridden to specify the editor's edit style
        /// </summary>
        /// <param name="context">The descriptor context</param>
        /// <returns>Always returns <b>Modal</b> as long as there is a context and an instance.  Otherwise, it
        /// returns <c>None</c>.</returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            if(context != null && context.Instance != null)
                return UITypeEditorEditStyle.Modal;

            return UITypeEditorEditStyle.None;
        }
    }
}
