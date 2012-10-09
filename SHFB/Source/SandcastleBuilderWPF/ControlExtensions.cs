//=============================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : ControlExtensions.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/28/2012
// Note    : Copyright 2011-2012, Eric Woodruff, All rights reserved
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
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

        #region Flow document and document element extension methods
        //=====================================================================

        /// <summary>
        /// Find elements of the given type that match the given predicate
        /// </summary>
        /// <typeparam name="T">The object type to find</typeparam>
        /// <param name="document">The document to search</param>
        /// <param name="predicate">The predicate to match</param>
        /// <returns>A list of all document elements of the given type that match the predicate</returns>
        public static IEnumerable<T> Find<T>(this FlowDocument document, Predicate<T> predicate)
        {
            return document.Blocks.SelectMany(b => b.Flatten()).OfType<T>().Where(e => predicate(e));
        }

        /// <summary>
        /// This is used to find all elements in a flow document with the given name
        /// </summary>
        /// <param name="document">The flow document to search</param>
        /// <param name="name">The element name to find</param>
        /// <returns>An enumerable list of elements with the given name</returns>
        public static IEnumerable<TextElement> FindByName(this FlowDocument document, string name)
        {
            return document.Blocks.SelectMany(b => b.Flatten()).Where(e => (name == null && e.Name == null) ||
                (name != null && e.Name != null && name.Equals(e.Name)));
        }

        /// <summary>
        /// This is used to flatten a flow document element hierarchy for searching
        /// </summary>
        /// <param name="element">The starting element to flatten</param>
        /// <returns>A flattened enumerable list containing the element and all child elements and
        /// their children recursively.</returns>
        public static IEnumerable<TextElement> Flatten(this TextElement element)
        {
            var table = element as Table;

            if(table != null)
                return (new[] { table }).Concat(table.RowGroups.SelectMany(g => g.Rows).SelectMany(
                    r => r.Cells).SelectMany(c => c.Flatten()));

            var tableCell = element as TableCell;

            if(tableCell != null)
                return (new[] { tableCell }).Concat(tableCell.Blocks.SelectMany(b => b.Flatten()));

            var section = element as Section;

            if(section != null)
                return (new[] { section }).Concat(section.Blocks.SelectMany(b => b.Flatten()));

            var list = element as List;

            if(list != null)
                return (new[] { list }).Concat(list.ListItems.Concat(list.ListItems.SelectMany(i => i.Flatten())));

            var listItem = element as ListItem;

            if(listItem != null)
                return (new[] { listItem }).Concat(listItem.Blocks.SelectMany(b => b.Flatten()));

            var para = element as Paragraph;

            if(para != null)
                return (new[] { element }).Concat(para.Inlines.SelectMany(i => i.Flatten()));

            var anchoredBlock = element as AnchoredBlock;

            if(anchoredBlock != null)
                return (new[] { anchoredBlock }).Concat(anchoredBlock.Blocks.SelectMany(b => b.Flatten()));

            var span = element as Span;

            if(span != null)
                return (new[] { span }).Concat(span.Inlines.SelectMany(i => i.Flatten()));

            return new[] { element };
        }
        #endregion

        #region Flow document table column sizing methods
        //=====================================================================

        /// <summary>
        /// This method is used to auto-size the columns in a table
        /// </summary>
        /// <param name="table">The table in which to auto-size the columns</param>
        /// <remarks>This is a very simple, brute force method of sizing the columns based purely on the
        /// length of the text in each cell.  It's far from perfect but does a decent job in most cases.</remarks>
        public static void AutoSizeTableColumns(this Table table)
        {
            int column, minWidth;
            double multiplier;

            var maxWidths = table.ColumnWidths().ToArray();

            // Add column definitions to the table if not there already
            if(table.Columns.Count == 0)
                for(column = 0; column < maxWidths.Length; column++)
                    table.Columns.Add(new TableColumn());

            // Get the minimum width
            minWidth = maxWidths.Min(w => w);
            column = 0;

            // Distribute the width across all columns based on how wide they are compared to the smallest
            // column.  Again, very simplistic but it does fairly well in most cases.
            foreach(var c in table.Columns)
            {
                multiplier = (double)maxWidths[column] / (double)minWidth;

                // Limit the width to a maxmimum of five times the smallest column so that we don't squash
                // the smaller columns.
                if(multiplier > 5.0)
                    multiplier = 5.0;

                c.Width = new GridLength(multiplier, GridUnitType.Star);
                column++;
            }
        }

        /// <summary>
        /// This is used to get the width of each column in a table measured in characters
        /// </summary>
        /// <param name="table">The table for which to get the column widths</param>
        /// <returns>An enumerable list of column width</returns>
        /// <remarks>Nested images and tables within cells are also taken into account</remarks>
        private static IEnumerable<int> ColumnWidths(this Table table)
        {
            int width, column, columnCount = table.RowGroups.Max(g => g.Rows.Max(r => r.Cells.Count));

            // Make sure all rows have the same number of columns.  If not, that's an error worth noting.
            if(columnCount != table.RowGroups.Min(g => g.Rows.Min(r => r.Cells.Count)))
                throw new InvalidOperationException("The column count is not consistent across all rows in a table");

            int[] maxWidths = new int[columnCount];

            // Figure out the width of each column based on the length of the text in each cell.  This is very
            // simplistic but does a good enough job in most cases.
            foreach(var row in table.RowGroups.SelectMany(g => g.Rows))
            {
                column = 0;

                foreach(var cell in row.Cells)
                {
                    // Look at the text within each individual paragraph.  Note that this will include all
                    // paragraphs in the cell, even those in nested block elements such as tables.  The
                    // check below will determine if the nested tables as a whole are longer than any
                    // one paragraph.
                    foreach(var para in cell.Blocks.SelectMany(b => b.Flatten().OfType<Paragraph>()))
                        foreach(var r in para.Inlines.Select(b => b.Flatten().OfType<Run>().Aggregate("",
                          (s, r) => s + r.Text)))
                        {
                            if(r.Length > maxWidths[column])
                                maxWidths[column] = r.Length;
                        }

                    // Check for nested images.  Figure out a rough cell width in characters based on the
                    // image width and font size.
                    foreach(var image in cell.Blocks.SelectMany(b => b.Flatten().OfType<BlockUIContainer>()).Where(
                      b => b.Child is Image).Select(b => (Image)b.Child))
                    {
                        width = (int)(image.Width / table.FontSize);

                        if(width > maxWidths[column])
                            maxWidths[column] = width;
                    }

                    // If there are nested tables, check to see if any of them are wider when all columns
                    // in each one are considered together.
                    foreach(var t in cell.Blocks.OfType<Table>())
                    {
                        width = t.ColumnWidths().Sum(w => w);

                        if(width > maxWidths[column])
                            maxWidths[column] = width;
                    }

                    column++;
                }
            }

            // Enforce a minimum width so that we don't lose columns
            for(column = 0; column < columnCount; column++)
                if(maxWidths[column] < 10)
                    maxWidths[column] = 10;

            return maxWidths;
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
