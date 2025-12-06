//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : EditorCommands.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/21/2025
// Note    : Copyright 2011-2025, Eric Woodruff, All rights reserved
//
// This file contains a class for the help file builder's routed UI commands for file editors.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/17/2011  EFW  Created the code
//===============================================================================================================

using System.Windows.Input;

namespace SandcastleBuilder.WPF.Commands;

/// <summary>
/// This class contains the help file builder's routed UI commands for file editors
/// </summary>
public static class EditorCommands
{
    #region Edit command
    //=====================================================================

    /// <summary>
    /// Open an item for editing
    /// </summary>
    /// <remarks>The default key binding is Ctrl+E</remarks>
    public static RoutedUICommand Edit { get; } = new RoutedUICommand("Edit", "Edit", typeof(EditorCommands),
        new InputGestureCollection(new[] { new KeyGesture(Key.E, ModifierKeys.Control, "Ctrl+E") }));

    #endregion

    #region Move up command
    //=====================================================================

    /// <summary>
    /// Move an item up within its collection
    /// </summary>
    /// <remarks>The default key binding is Ctrl+Up</remarks>
    public static RoutedUICommand MoveUp { get; } = new RoutedUICommand("Move Up", "MoveUp",
        typeof(EditorCommands), new InputGestureCollection(new[] {
            new KeyGesture(Key.Up, ModifierKeys.Control, "Ctrl+Up") }));

    #endregion

    #region Move down command
    //=====================================================================

    /// <summary>
    /// Move an item down within its collection
    /// </summary>
    /// <remarks>The default key binding is Ctrl+Down</remarks>
    public static RoutedUICommand MoveDown { get; } = new RoutedUICommand("Move Down", "MoveDown",
        typeof(EditorCommands), new InputGestureCollection(new[] {
            new KeyGesture(Key.Down, ModifierKeys.Control, "Ctrl+Down") }));

    #endregion

    #region Sort command
    //=====================================================================

    /// <summary>
    /// Sort a collection of items
    /// </summary>
    /// <remarks>The default key binding is Ctrl+Shift+T</remarks>
    public static RoutedUICommand Sort { get; } = new RoutedUICommand("Sort", "Sort", typeof(EditorCommands),
        new InputGestureCollection(new[] { new KeyGesture(Key.T, ModifierKeys.Control | ModifierKeys.Shift,
            "Ctrl+Shift+T") }));

    #endregion

    #region Paste as child command
    //=====================================================================

    /// <summary>
    /// Paste an item as a child of the selected item
    /// </summary>
    /// <remarks>The default key bindings are Ctrl+Shift+V and Ctrl+Shift+Insert</remarks>
    public static RoutedUICommand PasteAsChild { get; } = new RoutedUICommand("Paste as Child", "PasteAsChild",
        typeof(EditorCommands), new InputGestureCollection(new[]
        {
            new KeyGesture(Key.V, ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+V"),
            new KeyGesture(Key.Insert, ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+Insert"),
        }));

    #endregion

    #region Collapse all command
    //=====================================================================

    /// <summary>
    /// Collapse all items in a collection
    /// </summary>
    /// <remarks>The default key binding is Ctrl+Shift+Left</remarks>
    public static RoutedUICommand CollapseAll { get; } = new RoutedUICommand("Collapse All", "CollapseAll",
        typeof(EditorCommands), new InputGestureCollection(new[] { new KeyGesture(Key.Left,
            ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+Left") }));

    #endregion

    #region Expand all command
    //=====================================================================

    /// <summary>
    /// Expand all items in a collection
    /// </summary>
    /// <remarks>The default key binding is Ctrl+Shift+Right</remarks>
    public static RoutedUICommand ExpandAll { get; } = new RoutedUICommand("Expand All", "ExpandAll",
        typeof(EditorCommands), new InputGestureCollection(new[] { new KeyGesture(Key.Right,
            ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+Right") }));

    #endregion

    #region Collapse current item command
    //=====================================================================

    /// <summary>
    /// Collapse the current item and its children
    /// </summary>
    /// <remarks>The default key binding is Shift+Left</remarks>
    public static RoutedUICommand CollapseCurrent { get; } = new RoutedUICommand("Collapse Current Item",
        "CollapseCurrent", typeof(EditorCommands), new InputGestureCollection(new[] {
            new KeyGesture(Key.Left, ModifierKeys.Shift, "Shift+Left") }));

    #endregion

    #region Expand current item command
    //=====================================================================

    /// <summary>
    /// Expand the current item and its children
    /// </summary>
    /// <remarks>The default key binding is Shift+Right</remarks>
    public static RoutedUICommand ExpandCurrent { get; } = new RoutedUICommand("Expand Current Item",
        "ExpandCurrent", typeof(EditorCommands), new InputGestureCollection(new[] {
            new KeyGesture(Key.Right, ModifierKeys.Shift, "Shift+Right") }));

    #endregion

    #region Add item command
    //=====================================================================

    /// <summary>
    /// Add an item to the collection
    /// </summary>
    /// <remarks>This command has no default key binding</remarks>
    public static RoutedUICommand AddItem { get; } = new RoutedUICommand("Add an Item", "AddItem",
        typeof(EditorCommands));

    #endregion

    #region Convert to Markdown command
    //=====================================================================

    /// <summary>
    /// Convert a topic file from MAML to Markdown
    /// </summary>
    /// <remarks>This command has no default key binding</remarks>
    public static RoutedUICommand ConvertToMarkdown { get; } = new RoutedUICommand("Convert to Markdown",
        "ConvertToMarkdown", typeof(EditorCommands));

    #endregion
}
