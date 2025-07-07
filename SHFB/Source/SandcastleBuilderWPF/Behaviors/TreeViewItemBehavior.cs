//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : TreeViewItemBehavior.cs
// Author  : Josh Smith
// Updated : 07/04/2025
// Source  : https://www.codeproject.com/Articles/28959/Introduction-to-Attached-Behaviors-in-WPF
// Note    : Copyright 2008-2025, Josh Smith, All rights reserved
//
// This file contains a class that exposes attached behaviors that can be applied to TreeViewItem objects
//
// This code is published under the Code Project Open License (CPOL).  A copy of the license can be found at
// http://www.codeproject.com/info/cpol10.aspx.  This notice, the author's name, and all copyright notices must
// remain intact in all applications, documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/04/2011  EFW  Added the code to the project
//===============================================================================================================

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
        /// <summary>
        /// This defines the <see cref="P:SandcastleBuilder.WPF.Behaviors.TreeViewItemBehavior.IsBroughtIntoViewWhenSelected"/>
        /// attached property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// This attached property indicates whether or not a tree view item is brought into view when selected
        /// </summary>
        /// <value>The default value is false</value>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty IsBroughtIntoViewWhenSelectedProperty =
            DependencyProperty.RegisterAttached("IsBroughtIntoViewWhenSelected", typeof(bool),
                typeof(TreeViewItemBehavior), new UIPropertyMetadata(false, OnIsBroughtIntoViewWhenSelectedChanged));

        /// <summary>
        /// Get the property value
        /// </summary>
        /// <param name="treeViewItem">The tree view item</param>
        /// <returns>The property value</returns>
        public static bool GetIsBroughtIntoViewWhenSelected(TreeViewItem treeViewItem)
        {
            if(treeViewItem == null)
                throw new ArgumentNullException(nameof(treeViewItem));

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
            if(treeViewItem == null)
                throw new ArgumentNullException(nameof(treeViewItem));

            treeViewItem.SetValue(IsBroughtIntoViewWhenSelectedProperty, value);
        }

        /// <summary>
        /// This attaches and detaches the event handler to the tree view items
        /// </summary>
        /// <param name="depObj">The dependency object</param>
        /// <param name="e">The event arguments</param>
        private static void OnIsBroughtIntoViewWhenSelectedChanged(
          DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            if(depObj is not TreeViewItem item || e.NewValue is not bool)
                return;

            if((bool)e.NewValue)
                item.Selected += OnTreeViewItemSelected;
            else
                item.Selected -= OnTreeViewItemSelected;
        }

        /// <summary>
        /// This brings the tree view item into view when selected
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private static void OnTreeViewItemSelected(object sender, RoutedEventArgs e)
        {
            // Only react to the Selected event raised by the TreeViewItem
            // whose IsSelected property was modified. Ignore all ancestors
            // who are merely reporting that a descendant's Selected fired.
            if(!ReferenceEquals(sender, e.OriginalSource))
                return;

            if(e.OriginalSource is TreeViewItem item)
                item.BringIntoView();
        }
    }
}
