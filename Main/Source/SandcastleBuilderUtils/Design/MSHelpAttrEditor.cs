//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : MSHelpAttrEditor.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/20/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a type editor that displays a dialog box used to edit
// the help attribute items for a project or topic.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.7  03/27/2008  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Security.Permissions;
using System.Windows.Forms;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.ConceptualContent;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This is a type editor that displays the
    /// <see cref="MSHelpAttrEditorDlg"/> to edit the help attribute
    /// items for a project or topic.
    /// </summary>
    [PermissionSet(SecurityAction.LinkDemand, Unrestricted=true),
      PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
    internal sealed class MSHelpAttrEditor : System.Drawing.Design.UITypeEditor
    {
        /// <summary>
        /// This is overridden to edit the value using the
        /// <see cref="MSHelpAttrEditorDlg"/> dialog.
        /// </summary>
        /// <param name="context">The descriptor context</param>
        /// <param name="provider">The provider</param>
        /// <param name="value">The help attribute item collection as an object</param>
        /// <returns>The edited help attribute item collection as an object</returns>
        [RefreshProperties(RefreshProperties.All)]
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context,
          IServiceProvider provider, object value)
        {
            // Get the help attribute item collection
            MSHelpAttrCollection items = value as MSHelpAttrCollection;

            if(context == null || provider == null || context.Instance == null ||
              items == null)
                return base.EditValue(context, provider, value);

            bool isTopic = (context.Instance.GetType() == typeof(Topic));

            using(MSHelpAttrEditorDlg dlg = new MSHelpAttrEditorDlg(items,
              isTopic))
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
