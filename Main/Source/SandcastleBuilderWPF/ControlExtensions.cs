//=============================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : ControlExtensions.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/23/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class with WPF control extension methods.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.3  12/17/2011  EFW  Created the code
//=============================================================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SandcastleBuilder.WPF
{
    /// <summary>
    /// This class contains various extension methods for WPF controls
    /// </summary>
    public static class ControlExtensions
    {
        #region General control extension methods
        //=====================================================================

        /// <summary>
        /// From the given starting element, this will work up the visual tree until it finds a parent
        /// element of the given type.
        /// </summary>
        /// <typeparam name="T">The parent element type to find</typeparam>
        /// <param name="element">The starting element</param>
        /// <returns>The parent element of the given type if found or null if not found</returns>
        public static T ParentElementOfType<T>(this FrameworkElement element) where T : FrameworkElement
        {
            while(element != null && !(element is T))
                element = VisualTreeHelper.GetParent(element) as FrameworkElement;

            return element as T;
        }
        #endregion

        #region Commit changes to data source extension method
        //=====================================================================

        /// <summary>
        /// This is used to commit pending changes to the data source from
        /// a bound object and its children.
        /// </summary>
        /// <param name="parent">The parent object</param>
        public static void CommitChanges(this DependencyObject parent)
        {
            var localValues = parent.GetLocalValueEnumerator();

            while(localValues.MoveNext())
            {
                var entry = localValues.Current;

                if(BindingOperations.IsDataBound(parent, entry.Property))
                {
                    var binding = BindingOperations.GetBindingExpression(parent, entry.Property);

                    if(binding != null)
                        binding.UpdateSource();
                }
            }

            for(int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                VisualTreeHelper.GetChild(parent, i).CommitChanges();
        }
        #endregion

        #region TreeView extension methods
        //=====================================================================

        /// <summary>
        /// This <see cref="TreeView"/> extension method auto-scrolls the tree view if the mouse position
        /// is within a few display units of the top or bottom of the control.
        /// </summary>
        /// <param name="treeView">The tree view to use</param>
        /// <param name="mousePosition">The current position of the mouse</param>
        /// <remarks>This is useful for scrolling during drag and drop operations</remarks>
        public static void AutoScrollIfNeeded(this TreeView treeView, Point mousePosition)
        {
            ScrollViewer scrollViewer = null;
            double scrollOffset = 0;

            var border = VisualTreeHelper.GetChild(treeView, 0);

            if(border != null)
                scrollViewer = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;

            if(scrollViewer != null)
            {
                // If within range of the top or bottom border, scroll it
                if(scrollViewer.ViewportHeight - mousePosition.Y < 10) 
                    scrollOffset = 5; 
                else
                    if(mousePosition.Y < 10) 
                        scrollOffset = -5;

                if(scrollOffset != 0.0) 
                { 
                    scrollOffset += scrollViewer.VerticalOffset; 

                    if(scrollOffset < 0.0) 
                        scrollOffset = 0.0; 
                    else
                        if(scrollOffset > scrollViewer.ScrollableHeight) 
                            scrollOffset = scrollViewer.ScrollableHeight; 

                    scrollViewer.ScrollToVerticalOffset(scrollOffset); 
                }
            } 
        }
        #endregion
    }
}
