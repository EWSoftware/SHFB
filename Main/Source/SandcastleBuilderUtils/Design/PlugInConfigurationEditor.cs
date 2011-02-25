//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : PlugInConfigurationEditor.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/10/2007
// Note    : Copyright 2007, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a type editor that displays a dialog box used to edit
// the configurations for build process plug-ins.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.2.0  09/10/2007  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Security.Permissions;
using System.Windows.Forms;

using SandcastleBuilder.Utils.PlugIn;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This is a type editor that displays the
    /// <see cref="PlugInConfigurationEditorDlg"/> to edit the build process
    /// plug-in configurations.
    /// </summary>
    [PermissionSet(SecurityAction.LinkDemand, Unrestricted = true),
      PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
    internal sealed class PlugInConfigurationEditor : System.Drawing.Design.UITypeEditor
    {
        /// <summary>
        /// This is overridden to edit the value using the
        /// <see cref="PlugInConfigurationEditorDlg"/> dialog.
        /// </summary>
        /// <param name="context">The descriptor context</param>
        /// <param name="provider">The provider</param>
        /// <param name="value">The plug-in configuration dictionary as an
        /// object</param>
        /// <returns>The edited plug-in configuration dictionary as an
        /// object</returns>
        [RefreshProperties(RefreshProperties.All)]
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context,
          IServiceProvider provider, object value)
        {
            // Get the plug-in configuration dictionary
            PlugInConfigurationDictionary items = value as
                PlugInConfigurationDictionary;

            if(context == null || provider == null || context.Instance == null ||
              items == null)
                return base.EditValue(context, provider, value);

            using(PlugInConfigurationEditorDlg dlg =
              new PlugInConfigurationEditorDlg(items))
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
