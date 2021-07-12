//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : ContentEditorControl.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/19/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains the derived editor control used to edit conceptual content
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/10/2008  EFW  Created the code
// 07/26/2008  EFW  Reworked for use with the new project format
// 05/11/2013  EFW  Added support for spell checking
//===============================================================================================================

using System;
using System.Drawing;
using System.Windows.Forms;

using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Actions;

using SandcastleBuilder.Gui.Properties;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This is a derived ICSharpCode text editor control that is used to
    /// edit the conceptual content.
    /// </summary>
    public class ContentEditorControl : TextEditorControl
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public ContentEditorControl()
        {
            // Hook up the custom actions for the key presses
            if(!editactions.TryGetValue(Keys.Tab, out IEditAction oldAction))
                oldAction = null;

            editactions[Keys.Tab] = new InsertClosingElement(oldAction);

            if(Settings.Default.EnterMatching)
            {
                editactions[UnsafeNativeMethods.CharToKeys('"')] = new InsertMatchingCharacter("\"\"");
                editactions[UnsafeNativeMethods.CharToKeys('<')] = new InsertMatchingCharacter("<>");
                editactions[UnsafeNativeMethods.CharToKeys('(')] = new InsertMatchingCharacter("()");
                editactions[UnsafeNativeMethods.CharToKeys('[')] = new InsertMatchingCharacter("[]");
                editactions[UnsafeNativeMethods.CharToKeys('{')] = new InsertMatchingCharacter("{}");
            }

            // By default, Ctrl+B does bracket matching.  I want Ctrl+B for legacy bold.  Move bracket matching
            // to Ctrl + ] to match Visual Studio.
            if(editactions.TryGetValue(Keys.Control | Keys.B, out oldAction))
                editactions[Keys.Control | UnsafeNativeMethods.CharToKeys(']')] = oldAction;

            editactions[Keys.Control | Keys.B] = new InsertElement("legacyBold");
            editactions[Keys.Control | Keys.F] = new FindText(this);
            editactions[Keys.Control | Keys.G] = new GotoLine();
            editactions[Keys.Control | Keys.H] = new ReplaceText(this);
            editactions[Keys.Control | Keys.I] = new InsertElement("legacyItalic");
            editactions[Keys.Control | Keys.K] = new InsertElement("codeInline");
            editactions[Keys.Control | Keys.U] = new InsertElement("legacyUnderline");
            editactions[Keys.Control | Keys.V] = new PasteSpecial(this);
            editactions[Keys.Control | Keys.Shift | Keys.K] = new SpellCheck(this);
            editactions[Keys.Shift | Keys.Insert] = new PasteSpecial(this);
        }
        #endregion

        #region Events
        //=====================================================================

        /// <summary>
        /// This event is raised when Ctrl+F is hit to find text
        /// </summary>
        internal event EventHandler PerformFindText;

        /// <summary>
        /// This raises the <see cref="PerformFindText" /> event
        /// </summary>
        /// <param name="e">The event arguments</param>
        internal void OnPerformFindText(EventArgs e)
        {
            PerformFindText?.Invoke(this, e);
        }

        /// <summary>
        /// This event is raised when Ctrl+H is hit to replace text
        /// </summary>
        internal event EventHandler PerformReplaceText;

        /// <summary>
        /// This raises the <see cref="PerformReplaceText" /> event
        /// </summary>
        /// <param name="e">The event arguments</param>
        internal void OnPerformReplaceText(EventArgs e)
        {
            PerformReplaceText?.Invoke(this, e);
        }

        /// <summary>
        /// This event is raised when Ctrl+Shift+K is hit to spell check the text
        /// </summary>
        internal event EventHandler PerformSpellCheck;

        /// <summary>
        /// This raises the <see cref="PerformSpellCheck" /> event
        /// </summary>
        /// <param name="e">The event arguments</param>
        internal void OnPerformSpellCheck(EventArgs e)
        {
            PerformSpellCheck?.Invoke(this, e);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Paste an object from the clipboard (i.e. a topic or image link)
        /// </summary>
        /// <param name="data">The data to paste</param>
        /// <remarks>Since the owning topic editor handles the necessary tasks
        /// in its drag and drop event handler, we'll just invoke it.</remarks>
        internal void PasteSpecial(DataObject data)
        {
            Point p = this.PointToScreen(this.ActiveTextAreaControl.Caret.ScreenPosition);

            this.OnDragDrop(new DragEventArgs(data, 0, p.X, p.Y, DragDropEffects.All, DragDropEffects.All));
        }

        /// <summary>
        /// Insert a string of text into the specified text area at the current
        /// cursor location.
        /// </summary>
        /// <param name="textArea">The text area to use</param>
        /// <param name="text">The text to insert</param>
        public static void InsertString(TextArea textArea, string text)
        {
            if(textArea == null)
                throw new ArgumentNullException(nameof(textArea));

            if(text == null)
                throw new ArgumentNullException(nameof(text));

            int offset = textArea.Caret.Offset;

            textArea.BeginUpdate();

            try
            {
                textArea.Document.UndoStack.StartUndoGroup();

                // If inserted in a selection, replace the selection.
                // Otherwise, clear it.
                if(textArea.SelectionManager.HasSomethingSelected)
                    if(textArea.SelectionManager.SelectionCollection[0].ContainsOffset(offset))
                    {
                        offset = textArea.SelectionManager.SelectionCollection[0].Offset;
                        textArea.SelectionManager.RemoveSelectedText();
                    }
                    else
                        textArea.SelectionManager.ClearSelection();

                textArea.Document.Insert(offset, text);
                textArea.Caret.Position = textArea.Document.OffsetToPosition(offset + text.Length);
                textArea.Refresh();
                textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
            }
            finally
            {
                textArea.Document.UndoStack.EndUndoGroup();
                textArea.EndUpdate();
            }
        }

        /// <summary>
        /// Execute a specified action in the editor
        /// </summary>
        /// <param name="action">The action to execute</param>
        public void Execute(IEditAction action)
        {
            action?.Execute(this.ActiveTextAreaControl.TextArea);
        }
        #endregion
    }
}
