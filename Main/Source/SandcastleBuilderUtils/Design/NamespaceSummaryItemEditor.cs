//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : NamespaceSummaryItemEditor.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/27/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a type editor that displays a dialog box used to edit
// the namespace summaries for a project.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  07/27/2008  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Security.Permissions;
using System.Windows.Forms;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This is a type editor that displays the
    /// <see cref="NamespaceSummaryItemEditorDlg"/> to edit the namespace
    /// summaries for a project.
    /// </summary>
    [PermissionSet(SecurityAction.LinkDemand, Unrestricted=true),
      PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
    internal sealed class NamespaceSummaryItemEditor : System.Drawing.Design.UITypeEditor
    {
        /// <summary>
        /// This is overridden to edit the value using the
        /// <see cref="NamespaceSummaryItemEditorDlg"/> dialog.
        /// </summary>
        /// <param name="context">The descriptor context</param>
        /// <param name="provider">The provider</param>
        /// <param name="value">The namespace summary item collection as an
        /// object</param>
        /// <returns>The edited namespace summary item collection as an object</returns>
        [RefreshProperties(RefreshProperties.All)]
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context,
          IServiceProvider provider, object value)
        {
            // Get the namespace summary item collection
            NamespaceSummaryItemCollection items = value as NamespaceSummaryItemCollection;

            if(context == null || provider == null || context.Instance == null ||
              items == null)
                return base.EditValue(context, provider, value);

            using(NamespaceSummaryItemEditorDlg dlg =
              new NamespaceSummaryItemEditorDlg(items))
            {
                dlg.ShowDialog();
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
