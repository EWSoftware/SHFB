//===============================================================================================================
// System  : Sandcastle Tools - Windows platform specific code
// File    : FilePathObjectEditor.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2006-2025, Eric Woodruff, All rights reserved
//
// This file contains a type editor that can display a file dialog to allow selection of a file path at design
// time.  It is used in conjunction with the FileDialogAttribute to specify the file dialog title, filter, and
// type.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/29/2006  EFW  Created the code
// 05/16/2021  EFW  Moved the code to the Windows platform assembly from SandcastleBuilder.Utils
//===============================================================================================================

using System;
using System.ComponentModel;

using Sandcastle.Core;

namespace Sandcastle.Platform.Windows.Design
{
    /// <summary>
    /// This is a type editor that can display a file dialog to allow selection of a file path at design time
    /// for a <see cref="FilePath"/> object.
    /// </summary>
    /// <remarks>It is used in conjunction with the <see cref="FileDialogAttribute" /> to specify the file dialog
    /// title, filter, and type</remarks>
    public class FilePathObjectEditor : FilePathStringEditor
    {
        /// <summary>
        /// This is overridden to edit the value using a file dialog.
        /// </summary>
        /// <param name="context">The descriptor context</param>
        /// <param name="provider">The provider</param>
        /// <param name="value">The file path as an object</param>
        /// <returns>The selected file path as an object</returns>
        [RefreshProperties(RefreshProperties.All)]
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            FilePath filePath = value as FilePath;

            if(filePath == null)
                return base.EditValue(context, provider, value);

            string path = (string)base.EditValue(context, provider, filePath.ToString());

            if(path != filePath.Path)
                value = new FilePath(path, filePath.IsFixedPath, filePath.BasePathProvider);

            return value;
        }
    }
}
