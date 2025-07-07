//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : EditorActions.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/05/2025
// Note    : Copyright 2008-2025, Eric Woodruff, All rights reserved
//
// This file contains various custom actions for the topic editor
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/24/2008  EFW  Created the code
// 11/23/2012  EFW  Changed HTML encoding action so that it doesn't encode single and double quotes
// 05/11/2013  EFW  Added support for spell checking
//===============================================================================================================

using System;
using System.Globalization;
using System.Net;
using System.Windows.Forms;

using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Actions;
using ICSharpCode.TextEditor.Document;

namespace SandcastleBuilder.Gui.ContentEditors
{
    #region Goto line
    //=====================================================================

    /// <summary>
    /// Get a line number and position the cursor on it
    /// </summary>
    internal sealed class GotoLine : AbstractEditAction
    {
        /// <summary>
        /// Execute the Goto Line action
        /// </summary>
        /// <param name="textArea">The text area in which to perform the
        /// action</param>
        public override void Execute(TextArea textArea)
        {
            using GotoLineDlg dlg = new();
            
            if(dlg.ShowDialog() == DialogResult.OK)
                textArea.Caret.Line = dlg.LineNumber - 1;
        }
    }
    #endregion

    #region Fire the find text event
    //=====================================================================

    /// <summary>
    /// Fire the PerformFindText event
    /// </summary>
    internal sealed class FindText : AbstractEditAction
    {
        private readonly ContentEditorControl editor;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">The parent editor control</param>
        public FindText(ContentEditorControl parent)
        {
            editor = parent;
        }

        /// <summary>
        /// Execute the Find Text action
        /// </summary>
        /// <param name="textArea">The text area in which to perform the
        /// action</param>
        public override void Execute(TextArea textArea)
        {
            editor.OnPerformFindText(EventArgs.Empty);
        }
    }
    #endregion

    #region Fire the replace text event
    //=====================================================================

    /// <summary>
    /// Fire the PerformReplaceText event
    /// </summary>
    internal sealed class ReplaceText : AbstractEditAction
    {
        private readonly ContentEditorControl editor;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">The parent editor control</param>
        public ReplaceText(ContentEditorControl parent)
        {
            editor = parent;
        }

        /// <summary>
        /// Execute the Find Text action
        /// </summary>
        /// <param name="textArea">The text area in which to perform the
        /// action</param>
        public override void Execute(TextArea textArea)
        {
            editor.OnPerformReplaceText(EventArgs.Empty);
        }
    }
    #endregion

    #region Fire the spell check event
    //=====================================================================

    /// <summary>
    /// Fire the SpellCheck event
    /// </summary>
    internal sealed class SpellCheck : AbstractEditAction
    {
        private readonly ContentEditorControl editor;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">The parent editor control</param>
        public SpellCheck(ContentEditorControl parent)
        {
            editor = parent;
        }

        /// <summary>
        /// Execute the Spell Check action
        /// </summary>
        /// <param name="textArea">The text area in which to perform the action</param>
        public override void Execute(TextArea textArea)
        {
            editor.OnPerformSpellCheck(EventArgs.Empty);
        }
    }
    #endregion

    #region Paste Special
    //=====================================================================

    /// <summary>
    /// This handles pasting data from <see cref="BaseContentEditor.ClipboardDataHandler" />
    /// objects if present.
    /// </summary>
    internal sealed class PasteSpecial : Paste
    {
        private readonly ContentEditorControl editor;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">The parent editor control</param>
        public PasteSpecial(ContentEditorControl parent)
        {
            editor = parent;
        }

        /// <summary>
        /// Handle the paste operation
        /// </summary>
        /// <param name="textArea">The text area in which to perform the
        /// action</param>
        public override void Execute(TextArea textArea)
        {
            BaseContentEditor.ClipboardDataHandler getData =
                (BaseContentEditor.ClipboardDataHandler)Clipboard.GetDataObject().GetData(
                typeof(BaseContentEditor.ClipboardDataHandler));
            object pasteData = null;
            DataObject data;

            if(getData != null)
                pasteData = getData();

            if(pasteData != null)
            {
                data = new DataObject();
                data.SetData(pasteData.GetType(), pasteData);
                editor.PasteSpecial(data);
            }
            else
                base.Execute(textArea);
        }
    }
    #endregion

    #region HTML encode the selected text
    //=====================================================================

    /// <summary>
    /// This will HTML encode the currently selected block of text.
    /// </summary>
    /// <remarks>If no text is selected, nothing will happen.</remarks>
    internal sealed class HtmlEncode : AbstractEditAction
    {
        /// <summary>
        /// Execute the HTML Encode action
        /// </summary>
        /// <param name="textArea">The text area in which to perform the action</param>
        public override void Execute(TextArea textArea)
        {
            int offset = textArea.Caret.Offset;
            string selectedText;

            if(textArea.SelectionManager.HasSomethingSelected &&
              textArea.SelectionManager.SelectionCollection[0].ContainsOffset(offset))
            {
                selectedText = textArea.SelectionManager.SelectionCollection[0].SelectedText;

                // HTML encode everything but single and double quotes as they're fine as-is
                ContentEditorControl.InsertString(textArea,
                    WebUtility.HtmlEncode(selectedText).Replace("&quot;", "\"").Replace("&#39;", "'"));
            }
        }
    }
    #endregion

    #region Insert closing element action
    //=====================================================================

    /// <summary>
    /// This is used to insert a closing element when Tab is hit and the cursor
    /// is within an opening tag.
    /// </summary>
    /// <remarks>This is a really simple form of auto-completion.  This may be
    /// replaced in a later release with real auto-completion support that
    /// includes a pop-up with available options.</remarks>
    internal sealed class InsertClosingElement : AbstractEditAction
    {
        private readonly IEditAction defaultAction;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="oldAction">The old action to call if this action
        /// doesn't do anything.</param>
        public InsertClosingElement(IEditAction oldAction)
        {
            defaultAction = oldAction;
        }

        /// <summary>
        /// Execute the Insert Closing Element action
        /// </summary>
        /// <param name="textArea">The text area in which to perform the
        /// action</param>
        public override void Execute(TextArea textArea)
        {
            IDocument doc = textArea.Document;
            int endTag = -1, open = -1, close = -1, offset = textArea.Caret.Offset;
            char c;

            if(offset > 0 && doc.GetCharAt(offset - 1) == '>')
                offset--;

            // Find end of the element
            for(int i = offset; i < doc.TextLength; i++)
            {
                c = doc.GetCharAt(i);

                if(c == '<' || c== '>')
                {
                    if(c == '>')
                    {
                        endTag = close = i;

                        // Find start of tag
                        for(int j = close - 1; j >= 0; j--)
                        {
                            c = doc.GetCharAt(j);

                            if(c == '<' || c == '>')
                            {
                                if(c == '<')
                                    open = j;

                                break;
                            }

                            // On whitespace, assume we just moved past
                            // an attribute.
                            if(Char.IsWhiteSpace(c))
                                endTag = j;
                        }
                    }

                    break;
                }
            }

            // Execute the default action if we couldn't find an element
            // or it's a comment.
            if(open < 0 || close < 0 || doc.GetText(open + 1, 3) == "!--")
            {
                if(defaultAction != null)
                    defaultAction.Execute(textArea);
                else
                    textArea.InsertChar('\t');
            }
            else
            {
                open++;
                string tag = doc.GetText(open, endTag - open);

                close++;
                doc.Insert(close, "</" + tag + ">");
                textArea.Caret.Position = doc.OffsetToPosition(close);
            }
        }
    }
    #endregion

    #region Insert element
    //=====================================================================

    /// <summary>
    /// Insert an element of the given type optionally wrapping any selected
    /// text in which the cursor is located.
    /// </summary>
    internal sealed class InsertElement : AbstractEditAction
    {
        private readonly string elementName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="element">The element name to insert</param>
        public InsertElement(string element)
        {
            if(String.IsNullOrEmpty(element))
                throw new ArgumentException("An element name must be specified", nameof(element));

            elementName = element;
        }

        /// <summary>
        /// Execute the Insert Element action
        /// </summary>
        /// <param name="textArea">The text area in which to perform the
        /// action</param>
        public override void Execute(TextArea textArea)
        {
            int offset = textArea.Caret.Offset;
            string selectedText;

            if(textArea.SelectionManager.HasSomethingSelected &&
              textArea.SelectionManager.SelectionCollection[0].ContainsOffset(offset))
                selectedText = textArea.SelectionManager.SelectionCollection[0].SelectedText;
            else
                selectedText = String.Empty;

            ContentEditorControl.InsertString(textArea, String.Format(
                CultureInfo.CurrentCulture, "<{0}>{1}</{0}>", elementName,
                selectedText));

            textArea.Caret.Column -= elementName.Length + 3;
        }
    }
    #endregion

    #region Insert matching character
    //=====================================================================

    /// <summary>
    /// Insert a matching character when the first of the pair is entered
    /// </summary>
    internal sealed class InsertMatchingCharacter : AbstractEditAction
    {
        private readonly string matchedChars;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="match">The character(s) to insert</param>
        public InsertMatchingCharacter(string match)
        {
            if(String.IsNullOrEmpty(match))
                throw new ArgumentException("Character(s) to insert cannot be null or empty", nameof(match));

            matchedChars = match;
        }

        /// <summary>
        /// Execute the Insert Matching action
        /// </summary>
        /// <param name="textArea">The text area in which to perform the
        /// action</param>
        public override void Execute(TextArea textArea)
        {
            int col = textArea.Caret.Column;

            textArea.InsertString(matchedChars);
            textArea.Caret.Column = col + 1;
        }
    }
    #endregion
}
