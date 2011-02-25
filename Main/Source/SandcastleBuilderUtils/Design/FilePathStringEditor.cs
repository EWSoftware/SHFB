//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : FilePathStringEditor.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/29/2006
// Note    : Copyright 2006, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a type editor that can display a file dialog to allow
// selection of a file path at design time.  It is used in conjunction with
// the FileDialogAttribute to specify the file dialog title, filter, and type.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.3.4.0  12/29/2006  EFW  Created the code
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
    /// This is a type editor that can display a file dialog to allow
    /// selection of a file path at design time for a string object.
    /// </summary>
    /// <remarks>It is used in conjunction with the
    /// <see cref="FileDialogAttribute" /> to specify the file dialog title,
    /// filter, and type</remarks>
    [PermissionSet(SecurityAction.LinkDemand, Unrestricted=true),
      PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
    public class FilePathStringEditor : System.Drawing.Design.UITypeEditor
    {
        /// <summary>
        /// This is overridden to edit the value using a file dialog.
        /// </summary>
        /// <param name="context">The descriptor context</param>
        /// <param name="provider">The provider</param>
        /// <param name="value">The file path as an object</param>
        /// <returns>The selected file path as an object</returns>
        [RefreshProperties(RefreshProperties.All)]
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context,
          IServiceProvider provider, object value)
        {
            FileDialog dlg = null;
            string filename, filePath, currentFilename;

            if(context == null || provider == null ||
              context.Instance == null || (value != null && !(value is string)))
                return base.EditValue(context, provider, value);

            // Get the filename and path
            currentFilename = (string)value;

            if(!String.IsNullOrEmpty(currentFilename))
            {
                filename = Path.GetFileName(currentFilename);
                filePath = Path.GetDirectoryName(currentFilename);
            }
            else
                filename = filePath = String.Empty;

            try
            {
                // Figure out what type of file dialog to show and set
                // its properties.
                FileDialogAttribute fda = context.PropertyDescriptor.Attributes[
                    typeof(FileDialogAttribute)] as FileDialogAttribute;

                if(fda != null && fda.DialogType == FileDialogType.FileSave)
                    dlg = new SaveFileDialog();
                else
                    dlg = new OpenFileDialog();

                dlg.RestoreDirectory = true;
                dlg.FileName = filename;
                dlg.InitialDirectory = filePath;

                if(fda != null)
                {
                    dlg.Title = fda.Title;
                    dlg.Filter = fda.Filter;
                }
                else
                {
                    dlg.Title = "Select " + context.PropertyDescriptor.DisplayName;
                    dlg.Filter = "All files (*.*)|*.*";
                }

                // If selected, set the new filename
                if(dlg.ShowDialog() == DialogResult.OK)
                    value = dlg.FileName;
            }
            finally
            {
                if(dlg != null)
                    dlg.Dispose();
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
