//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : ComponentConfigurationEditor.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/01/2007
// Note    : Copyright 2006, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a type editor that displays a dialog box used to edit
// the configurations for custom build components.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.3.3.0  11/24/2006  EFW  Created the code
// 1.6.0.2  11/01/2007  EFW  Reworked to support better handling of components
//=============================================================================

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Security.Permissions;
using System.Windows.Forms;

using SandcastleBuilder.Utils.BuildComponent;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This is a type editor that displays the
    /// <see cref="ComponentConfigurationEditorDlg"/> to edit the build
    /// component configurations.
    /// </summary>
    [PermissionSet(SecurityAction.LinkDemand, Unrestricted = true),
      PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
    internal sealed class ComponentConfigurationEditor : System.Drawing.Design.UITypeEditor
    {
        /// <summary>
        /// This is overridden to edit the value using the
        /// <see cref="ComponentConfigurationEditorDlg"/> dialog.
        /// </summary>
        /// <param name="context">The descriptor context</param>
        /// <param name="provider">The provider</param>
        /// <param name="value">The component configuration dictionary as an
        /// object</param>
        /// <returns>The edited component configuration dictionary as an
        /// object</returns>
        [RefreshProperties(RefreshProperties.All)]
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context,
          IServiceProvider provider, object value)
        {
            // Get the component configuration dictionary
            ComponentConfigurationDictionary items = value as
                ComponentConfigurationDictionary;

            if(context == null || provider == null || context.Instance == null ||
              items == null)
                return base.EditValue(context, provider, value);

            using(ComponentConfigurationEditorDlg dlg =
              new ComponentConfigurationEditorDlg(items))
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
