//=============================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : TreeViewItemBehavior.cs
// Author  : Josh Smith
// Updated : 12/04/2011
// Source  : http://www.codeproject.com/KB/WPF/AttachedBehaviors.aspx
// Note    : Copyright 2008-2011, Josh Smith, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that exposes attached behaviors that can be
// applied to TreeViewItem objects.
//
// This code is published under the Code Project Open License (CPOL).  A copy
// of the license can be found at http://www.codeproject.com/info/cpol10.aspx.
// This notice, the author's name, and all copyright notices must remain intact
// in all applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.3  12/04/2011  EFW  Added the code to the project
//=============================================================================

using System;
using System.Windows;
using System.Windows.Controls;

namespace SandcastleBuilder.WPF.Behaviors
{
    /// <summary>
    /// Exposes attached behaviors that can be applied to TreeViewItem objects.
    /// </summary>
    public static class TreeViewItemBehavior
    {
        #region IsBroughtIntoViewWhenSelected

        /// <summary>
        /// Get the property value
        /// </summary>
        /// <param name="treeViewItem">The tree view item</param>
        /// <returns>The property value</returns>
        public static bool GetIsBroughtIntoViewWhenSelected(TreeViewItem treeViewItem)
        {
            return (bool)treeViewItem.GetValue(IsBroughtIntoViewWhenSelectedProperty);
        }

        /// <summary>
        /// Sets the property value
        /// </summary>
        /// <param name="treeViewItem">The tree view item</param>
        /// <param name="value">The property value</param>
        public static void SetIsBroughtIntoViewWhenSelected(
          TreeViewItem treeViewItem, bool value)
        {
            treeViewItem.SetValue(IsBroughtIntoViewWhenSelectedProperty, value);
        }

        /// <summary>
        /// Dependency property
        /// </summary>
        public static readonly DependencyProperty IsBroughtIntoViewWhenSelectedProperty =
            DependencyProperty.RegisterAttached(
            "IsBroughtIntoViewWhenSelected",
            typeof(bool),
            typeof(TreeViewItemBehavior),
            new UIPropertyMetadata(false, OnIsBroughtIntoViewWhenSelectedChanged));

        static void OnIsBroughtIntoViewWhenSelectedChanged(
          DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            TreeViewItem item = depObj as TreeViewItem;
            if(item == null)
                return;

            if(e.NewValue is bool == false)
                return;

            if((bool)e.NewValue)
                item.Selected += OnTreeViewItemSelected;
            else
                item.Selected -= OnTreeViewItemSelected;
        }

        static void OnTreeViewItemSelected(object sender, RoutedEventArgs e)
        {
            // Only react to the Selected event raised by the TreeViewItem
            // whose IsSelected property was modified. Ignore all ancestors
            // who are merely reporting that a descendant's Selected fired.
            if(!Object.ReferenceEquals(sender, e.OriginalSource))
                return;

            TreeViewItem item = e.OriginalSource as TreeViewItem;
            if(item != null)
                item.BringIntoView();
        }

        #endregion // IsBroughtIntoViewWhenSelected
    }
}
