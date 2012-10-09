//=============================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : EditorCommands.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/29/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class for the help file builder's routed UI commands
// for file editors.
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

using System.Windows.Input;

namespace SandcastleBuilder.WPF.Commands
{
    /// <summary>
    /// This class contains the help file builder's routed UI commands for file editors
    /// </summary>
    public static class EditorCommands
    {
        #region Edit command
        //=====================================================================

        private static RoutedUICommand edit;

        /// <summary>
        /// Open an item for editing
        /// </summary>
        /// <remarks>The default key binding is Ctrl+E</remarks>
        public static RoutedUICommand Edit
        {
            get
            {
                if(edit == null)
                    edit = new RoutedUICommand("Edit", "Edit", typeof(EditorCommands),
                        new InputGestureCollection(new[] { new KeyGesture(Key.E, ModifierKeys.Control,
                            "Ctrl+E") }));

                return edit;
            }
        }
        #endregion

        #region Move up command
        //=====================================================================

        private static RoutedUICommand moveUp;

        /// <summary>
        /// Move an item up within its collection
        /// </summary>
        /// <remarks>The default key binding is Ctrl+Up</remarks>
        public static RoutedUICommand MoveUp
        {
            get
            {
                if(moveUp == null)
                    moveUp = new RoutedUICommand("Move Up", "MoveUp", typeof(EditorCommands),
                        new InputGestureCollection(new[] { new KeyGesture(Key.Up, ModifierKeys.Control,
                            "Ctrl+Up") }));

                return moveUp;
            }
        }
        #endregion

        #region Move down command
        //=====================================================================

        private static RoutedUICommand moveDown;

        /// <summary>
        /// Move an item down within its collection
        /// </summary>
        /// <remarks>The default key binding is Ctrl+Down</remarks>
        public static RoutedUICommand MoveDown
        {
            get
            {
                if(moveDown == null)
                    moveDown = new RoutedUICommand("Move Down", "MoveDown", typeof(EditorCommands),
                        new InputGestureCollection(new[] { new KeyGesture(Key.Down, ModifierKeys.Control,
                            "Ctrl+Down") }));

                return moveDown;
            }
        }
        #endregion

        #region Sort command
        //=====================================================================

        private static RoutedUICommand sort;

        /// <summary>
        /// Sort a collection of items
        /// </summary>
        /// <remarks>The default key binding is Ctrl+Shift+T</remarks>
        public static RoutedUICommand Sort
        {
            get
            {
                if(sort == null)
                    sort = new RoutedUICommand("Sort", "Sort", typeof(EditorCommands),
                        new InputGestureCollection(new[] { new KeyGesture(Key.T, ModifierKeys.Control |
                            ModifierKeys.Shift, "Ctrl+Shift+T") }));

                return sort;
            }
        }
        #endregion

        #region Paste as child command
        //=====================================================================

        private static RoutedUICommand pasteAsChild;

        /// <summary>
        /// Paste an item as a child of the selected item
        /// </summary>
        /// <remarks>The default key bindings are Ctrl+Shift+V and Ctrl+Shift+Insert</remarks>
        public static RoutedUICommand PasteAsChild
        {
            get
            {
                if(pasteAsChild == null)
                    pasteAsChild = new RoutedUICommand("Paste as Child", "PasteAsChild",
                        typeof(EditorCommands), new InputGestureCollection(new[]
                        {
                            new KeyGesture(Key.V, ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+V"),
                            new KeyGesture(Key.Insert, ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+Insert"),
                        }));

                return pasteAsChild;
            }
        }
        #endregion

        #region Collapse all command
        //=====================================================================

        private static RoutedUICommand collapseAll;

        /// <summary>
        /// Collapse all items in a collection
        /// </summary>
        /// <remarks>The default key binding is Ctrl+Shift+Left</remarks>
        public static RoutedUICommand CollapseAll
        {
            get
            {
                if(collapseAll == null)
                    collapseAll = new RoutedUICommand("Collapse All", "CollapseAll", typeof(EditorCommands),
                        new InputGestureCollection(new[] { new KeyGesture(Key.Left,
                            ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+Left") }));

                return collapseAll;
            }
        }
        #endregion

        #region Expand all command
        //=====================================================================

        private static RoutedUICommand expandAll;

        /// <summary>
        /// Expand all items in a collection
        /// </summary>
        /// <remarks>The default key binding is Ctrl+Shift+Right</remarks>
        public static RoutedUICommand ExpandAll
        {
            get
            {
                if(expandAll == null)
                    expandAll = new RoutedUICommand("Expand All", "ExpandAll", typeof(EditorCommands),
                        new InputGestureCollection(new[] { new KeyGesture(Key.Right,
                            ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+Right") }));

                return expandAll;
            }
        }
        #endregion

        #region Collapse current item command
        //=====================================================================

        private static RoutedUICommand collapseCurrent;

        /// <summary>
        /// Collapse the current item and its children
        /// </summary>
        /// <remarks>The default key binding is Shift+Left</remarks>
        public static RoutedUICommand CollapseCurrent
        {
            get
            {
                if(collapseCurrent == null)
                    collapseCurrent = new RoutedUICommand("Collapse Current Item", "CollapseCurrent",
                        typeof(EditorCommands),
                        new InputGestureCollection(new[] { new KeyGesture(Key.Left, ModifierKeys.Shift,
                            "Shift+Left") }));

                return collapseCurrent;
            }
        }
        #endregion

        #region Expand current item command
        //=====================================================================

        private static RoutedUICommand expandCurrent;

        /// <summary>
        /// Expand the current item and its children
        /// </summary>
        /// <remarks>The default key binding is Shift+Right</remarks>
        public static RoutedUICommand ExpandCurrent
        {
            get
            {
                if(expandCurrent == null)
                    expandCurrent = new RoutedUICommand("Expand Current Item", "ExpandCurrent",
                        typeof(EditorCommands),
                        new InputGestureCollection(new[] { new KeyGesture(Key.Right, ModifierKeys.Shift,
                            "Shift+Right") }));

                return expandCurrent;
            }
        }
        #endregion

        #region Add item command
        //=====================================================================

        private static RoutedUICommand addItem;

        /// <summary>
        /// Add an item to the collection
        /// </summary>
        /// <remarks>This command has no default key binding</remarks>
        public static RoutedUICommand AddItem
        {
            get
            {
                if(addItem == null)
                    addItem = new RoutedUICommand("Add an Item", "AddItem",
                        typeof(EditorCommands));

                return addItem;
            }
        }
        #endregion
    }
}
