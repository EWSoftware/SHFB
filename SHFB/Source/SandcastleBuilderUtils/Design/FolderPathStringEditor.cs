//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : FolderPathStringEditor.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/18/2007
// Note    : Copyright 2006-2007, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a type editor that can display a folder browser dialog
// to allow selection of a folder path at design time.  This can be used in
// conjunction with the FolderDialogAttribute to specify the folder browser
// dialog's properties.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  08/02/2006  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Security.Permissions;
using System.Windows.Forms;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This is a type editor that can display a folder browser dialog to
    /// allow selection of a folder path at design time.
    /// </summary>
    /// <remarks>This can be used in conjunction with the
    /// <see cref="FolderDialogAttribute" /> to specify the folder browser
    /// dialog's properties.</remarks>
    [PermissionSet(SecurityAction.LinkDemand, Unrestricted=true),
      PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
    public class FolderPathStringEditor : System.Drawing.Design.UITypeEditor
    {
        /// <summary>
        /// This is overridden to edit the value using a folder browser dialog.
        /// </summary>
        /// <param name="context">The descriptor context</param>
        /// <param name="provider">The provider</param>
        /// <param name="value">The folder path as an object</param>
        /// <returns>The selected folder path as an object</returns>
        [RefreshProperties(RefreshProperties.All)]
        public override object EditValue(ITypeDescriptorContext context,
          IServiceProvider provider, object value)
        {
            string currentFolder;

            if(context == null || provider == null ||
              context.Instance == null || (value != null && !(value is string)))
                return base.EditValue(context, provider, value);

            currentFolder = (string)value;

            if(String.IsNullOrEmpty(currentFolder))
                currentFolder = String.Empty;

            using(FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                // See if we have been supplied a title
                FolderDialogAttribute fda = context.PropertyDescriptor.Attributes[
                    typeof(FolderDialogAttribute)] as FolderDialogAttribute;

                if(fda != null)
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
        /// <returns>Always returns <b>Modal</b> as long as there is a context
        /// and an instance.  Otherwise, it returns <b>None</b>.</returns>
        public override UITypeEditorEditStyle GetEditStyle(
          System.ComponentModel.ITypeDescriptorContext context)
        {
            if(context != null && context.Instance != null)
                return UITypeEditorEditStyle.Modal;

            return UITypeEditorEditStyle.None;
        }
    }
}
