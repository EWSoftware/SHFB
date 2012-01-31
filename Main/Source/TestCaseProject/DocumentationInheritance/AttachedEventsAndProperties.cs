using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace TestDoc.DocumentationInheritance
{
    /// <summary>
    /// This class is used to test inherited documentation on attached properties and events
    /// </summary>
    public static class AttachedEventsAndPropertiesTest
    {
        #region IsBroughtIntoViewWhenSelected attached property

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
        /// This defines the <see cref="P:TestDoc.DocumentationInheritance.AttachedEventsAndPropertiesTest.IsBroughtIntoViewWhenSelected"/>
        /// attached property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>This attached property indicates whether or not a tree view item is brought into
        /// view when selected.
        /// </summary>
        /// <value>The default value is false</value>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty IsBroughtIntoViewWhenSelectedProperty =
            DependencyProperty.RegisterAttached(
            "IsBroughtIntoViewWhenSelected",
            typeof(bool),
            typeof(AttachedEventsAndPropertiesTest),
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

        #region ItemActivate attached event
        //=====================================================================

        /// <summary>
        /// This defines the <see cref="E:TestDoc.DocumentationInheritance.AttachedEventsAndPropertiesTest.ItemActivate"/>
        /// attached event.
        /// </summary>
        /// <AttachedEventComments>
        /// <summary>
        /// This attached event is raised when an item is activated
        /// </summary>
        /// <remarks>There's a bit more too it to get the event raised but this is just a
        /// documentation example.</remarks>
        /// </AttachedEventComments>
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
