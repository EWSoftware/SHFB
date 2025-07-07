//===============================================================================================================
// System  : Sandcastle Tools - XML Comments Example
// File    : AttachedEventsAndProperties.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/08/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
//
// This class is used to demonstrate the AttachedEventComments and AttachedPropertyComments XML comments
// elements.  It serves no useful purpose.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/06/2012  EFW  Created the code
//===============================================================================================================

using System;
using System.Windows;
using System.Windows.Controls;

namespace XMLCommentsExamples.DocumentationInheritance
{
    /// <summary>
    /// This class is used to test inherited documentation on attached properties and events
    /// </summary>
    /// <conceptualLink target="3563f000-5677-4cd9-afd7-4e3f2a7fe4fc" />
    /// <conceptualLink target="c0346d23-f376-4948-8f9a-d17b2f1acef3" />
    public static class AttachedEventsAndPropertiesTest
    {
        #region IsBroughtIntoViewWhenSelected attached property

        /// <summary>
        /// This defines the
        /// <see cref="P:XMLCommentsExamples.DocumentationInheritance.AttachedEventsAndPropertiesTest.IsBroughtIntoViewWhenSelected"/>
        /// attached property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>This attached property indicates whether or not a tree view item is brought into
        /// view when selected.
        /// </summary>
        /// <value>The default value is false</value>
        /// <conceptualLink target="c0346d23-f376-4948-8f9a-d17b2f1acef3" />
        /// </AttachedPropertyComments>
        /// <conceptualLink target="c0346d23-f376-4948-8f9a-d17b2f1acef3" />
        public static readonly DependencyProperty IsBroughtIntoViewWhenSelectedProperty =
            DependencyProperty.RegisterAttached(
            "IsBroughtIntoViewWhenSelected",
            typeof(bool),
            typeof(AttachedEventsAndPropertiesTest),
            new UIPropertyMetadata(false, OnIsBroughtIntoViewWhenSelectedChanged));

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
        /// Connect or disconnect the event handler when the selected state changes
        /// </summary>
        /// <param name="depObj">The dependency object</param>
        /// <param name="e">The event arguments</param>
        static void OnIsBroughtIntoViewWhenSelectedChanged(
          DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            if(depObj is not TreeViewItem item)
                return;

            if(e.NewValue is bool == false)
                return;

            if((bool)e.NewValue)
                item.Selected += OnTreeViewItemSelected;
            else
                item.Selected -= OnTreeViewItemSelected;
        }

        /// <summary>
        /// Bring the item into view
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        static void OnTreeViewItemSelected(object sender, RoutedEventArgs e)
        {
            // Only react to the Selected event raised by the TreeViewItem
            // whose IsSelected property was modified. Ignore all ancestors
            // who are merely reporting that a descendant's Selected fired.
            if(!Object.ReferenceEquals(sender, e.OriginalSource))
                return;

            if(e.OriginalSource is TreeViewItem item)
                item.BringIntoView();
        }

        #endregion

        #region ItemActivate attached event

        /// <summary>
        /// This defines the
        /// <see cref="E:XMLCommentsExamples.DocumentationInheritance.AttachedEventsAndPropertiesTest.ItemActivate"/>
        /// attached event.
        /// </summary>
        /// <AttachedEventComments>
        /// <summary>
        /// This attached event is raised when an item is activated
        /// </summary>
        /// <remarks>There's a bit more too it to get the event raised but this is just a
        /// documentation example.</remarks>
        /// <conceptualLink target="3563f000-5677-4cd9-afd7-4e3f2a7fe4fc" />
        /// </AttachedEventComments>
        /// <conceptualLink target="3563f000-5677-4cd9-afd7-4e3f2a7fe4fc" />
        public static readonly RoutedEvent ItemActivateEvent = EventManager.RegisterRoutedEvent(
            "ItemActivate", RoutingStrategy.Bubble, typeof(RoutedEventHandler),
            typeof(AttachedEventsAndPropertiesTest));

        /// <summary>
        /// Add an event handler to an object
        /// </summary>
        /// <param name="o">The dependency object</param>
        /// <param name="handler">The event handler</param>
        public static void AddItemActivateHandler(DependencyObject o, RoutedEventHandler handler)
        {
            ((UIElement)o).AddHandler(AttachedEventsAndPropertiesTest.ItemActivateEvent, handler);
        }

        /// <summary>
        /// Remove an event handler from an object
        /// </summary>
        /// <param name="o">The dependency object</param>
        /// <param name="handler">The event handler</param>
        public static void RemoveItemActivateHandler(DependencyObject o, RoutedEventHandler handler)
        {
            ((UIElement)o).RemoveHandler(AttachedEventsAndPropertiesTest.ItemActivateEvent, handler);
        }
        #endregion
    }
}
