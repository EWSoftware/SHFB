//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : SyntaxFilterValueEditor.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/15/2010
// Note    : Copyright 2009-2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a type editor that displays a checked list box as the
// drop-down editor for a string containing a list of language syntax filter
// IDs.  This makes it easy to select multiple values with the drop-down.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.3  11/10/2009  EFW  Created the code
//=============================================================================

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using SandcastleBuilder.Utils.BuildComponent;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This is a type editor that displays a checked list box as the drop-down
    /// editor for a string containing a list of language syntax filter IDs.
    /// This makes it easy to select multiple values with the drop-down.
    /// </summary>
    internal sealed class SyntaxFilterValueEditor : System.Drawing.Design.UITypeEditor
    {
        /// <summary>
        /// This is overridden to edit the value using a checked list box
        /// control as the drop-down editor.
        /// </summary>
        /// <param name="context">The descriptor context</param>
        /// <param name="provider">The provider</param>
        /// <param name="value">The enumerated type object to edit</param>
        /// <returns>The edited enumerated type object</returns>
        [RefreshProperties(RefreshProperties.All),
         PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context,
          IServiceProvider provider, object value)
        {
            Collection<SyntaxFilterInfo> allFilters, definedFilters;
            IWindowsFormsEditorService editorService;
            CheckedListBox ckbListBox;
            string filterIds;

            // Only use the editor if we have a valid context
            if(context == null || provider == null || context.Instance == null ||
              value == null)
                return base.EditValue(context, provider, value);

            editorService = (IWindowsFormsEditorService)provider.GetService(
                typeof(IWindowsFormsEditorService));

            if(editorService == null)
                return base.EditValue(context, provider, value);

            allFilters = BuildComponentManager.SyntaxFiltersFrom("All");
            definedFilters = BuildComponentManager.SyntaxFiltersFrom(value.ToString());

            using(ckbListBox = new CheckedListBox())
            {
                ckbListBox.BorderStyle = BorderStyle.None;
                ckbListBox.CheckOnClick = true;

                // Tahoma 8pt prevents it from clipping the bottom of each item
                ckbListBox.Font = new Font("Tahoma", 8.0f);

                // Load the values into the checked list box
                foreach(SyntaxFilterInfo info in allFilters)
                    ckbListBox.Items.Add(info.Id, definedFilters.Contains(info));

                // Adjust the height of the list box to show all items or
                // at most twelve of them.
                if(ckbListBox.Items.Count < 12)
                    ckbListBox.Height = ckbListBox.Items.Count *
                        ckbListBox.ItemHeight;
                else
                    ckbListBox.Height = ckbListBox.Items.Count * 12;

                // Display it and let the user edit the value
                editorService.DropDownControl(ckbListBox);

                // Get the selected syntax filter ID values and return them
                // as a comma-separated string.  This also checks for common
                // short-hand values and uses them if appropriate.
                if(ckbListBox.CheckedItems.Count == 0)
                    value = "None";
                else
                {
                    filterIds = String.Join(", ", ckbListBox.CheckedItems.Cast<string>().ToArray());
                    definedFilters = BuildComponentManager.SyntaxFiltersFrom(filterIds);

                    // Convert to All, AllButUsage, or Standard?
                    if(definedFilters.Count == allFilters.Count)
                        filterIds = "All";
                    else
                        if(definedFilters.Count == allFilters.Count(
                          af => af.Id.IndexOf("usage", StringComparison.OrdinalIgnoreCase) == -1))
                            filterIds = "AllButUsage";
                        else
                            if(definedFilters.Count == 3 && (definedFilters.All(f => f.Id == "CSharp" ||
                              f.Id == "VisualBasic" || f.Id == "CPlusPlus")))
                                filterIds = "Standard";

                    value = filterIds;
                }
            }

            return value;
        }

        /// <summary>
        /// This is overridden to specify the editor's edit style
        /// </summary>
        /// <param name="context">The descriptor context</param>
        /// <returns>Always returns <b>DropDown</b> as long as there is a
        /// context and an instance.  Otherwise, it returns <b>None</b>.</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public override UITypeEditorEditStyle GetEditStyle(
          System.ComponentModel.ITypeDescriptorContext context)
        {
            if(context != null && context.Instance != null)
                return UITypeEditorEditStyle.DropDown;

            return UITypeEditorEditStyle.None;
        }
    }
}
