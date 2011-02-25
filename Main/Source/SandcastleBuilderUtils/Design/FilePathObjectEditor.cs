//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : FilePathObjectEditor.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/19/2008
// Note    : Copyright 2006-2008, Eric Woodruff, All rights reserved
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
    /// selection of a file path at design time for a <see cref="FilePath"/>
    /// object.
    /// </summary>
    /// <remarks>It is used in conjunction with the
    /// <see cref="FileDialogAttribute" /> to specify the file dialog title,
    /// filter, and type</remarks>
    public class FilePathObjectEditor : FilePathStringEditor
    {
        /// <summary>
        /// This is overridden to edit the value using a file dialog.
        /// </summary>
        /// <param name="context">The descriptor context</param>
        /// <param name="provider">The provider</param>
        /// <param name="value">The file path as an object</param>
        /// <returns>The selected file path as an object</returns>
        [RefreshProperties(RefreshProperties.All),
         PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context,
          IServiceProvider provider, object value)
        {
            FilePath filePath = value as FilePath;

            if(filePath == null)
                return base.EditValue(context, provider, value);

            string path = (string)base.EditValue(context, provider,
                filePath.ToString());

            if(path != filePath.Path)
                value = new FilePath(path, filePath.IsFixedPath,
                    filePath.BasePathProvider);

            return value;
        }
    }
}
