//=============================================================================
// System  : Sandcastle Help File Builder
// File    : TopicEditorWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/08/2012
// Note    : Copyright 2008-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to edit the conceptual topic files.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.7  05/10/2008  EFW  Created the code
// 1.8.0.0  07/26/2008  EFW  Reworked for use with the new project format
// 1.9.3.3  12/11/2011  EFW  Simplified the drag and drop code to work with
//                           the new Entity References WPF user control.
// 1.9.3.4  01/20/2012  EFW  Added property to allow retrieval of the text
//=============================================================================

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

using SandcastleBuilder.Gui.Properties;
using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.ConceptualContent;

using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Actions;
using WeifenLuo.WinFormsUI.Docking;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This form is used to edit a conceptual topic file.
    /// </summary>
    public partial class TopicEditorWindow : BaseContentEditor
    {
        #region Private data members
        //=====================================================================

        private ToolStripMenuItem lastAction;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the filename
        /// </summary>
        public string Filename
        {
            get { return this.ToolTipText; }
        }

        /// <summary>
        /// This read-only property returns the current file content
        /// </summary>
        public string FileContent
        {
            get { return editor.Document.TextContent; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filename">The filename to load</param>
        public TopicEditorWindow(string filename)
        {
            string ext;

            InitializeComponent();

            // Connect the drag and drop events to the text area
            TextArea area = editor.ActiveTextAreaControl.TextArea;
            area.AllowDrop = true;
            area.DragEnter += editor_DragEnter;
            area.DragOver += editor_DragEnter;
            area.DragDrop += editor_DragDrop;
            editor.PerformFindText += editor_PerformFindText;
            editor.PerformReplaceText += editor_PerformReplaceText;

            editor.TextEditorProperties.Font = Settings.Default.TextEditorFont;
            editor.TextEditorProperties.ShowLineNumbers = Settings.Default.ShowLineNumbers;

            try
            {
                editor.LoadFile(filename);

                ext = Path.GetExtension(filename).ToLowerInvariant();

                if(ext == ".aml" || ext == ".topic" || ext == ".snippets" || ext == ".tokens" ||
                  ext == ".content" || ext == ".xamlcfg")
                    editor.SetHighlighting("XML");

                editor.TextChanged += editor_TextChanged;

                this.Text = Path.GetFileName(filename);
                this.ToolTipText = filename;
            }
            catch(Exception ex)
            {
                this.Text = this.ToolTipText = "N/A";
                MessageBox.Show("Unable to load file '" + filename + "'. Reason: " + ex.Message, "Topic Editor",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// Return the string used to store the editor state
        /// </summary>
        /// <returns>A string containing the type name and filename</returns>
        protected override string GetPersistString()
        {
            return GetType().ToString() + "," + this.ToolTipText;
        }

        /// <inheritdoc />
        public override bool CanClose
        {
            get
            {
                if(!this.IsDirty)
                    return true;

                DialogResult dr = MessageBox.Show("Do you want to save your changes to '" + this.ToolTipText +
                    "?  Click YES to to save them, NO to discard them, or CANCEL to stay here and make " +
                    "further changes.", "Topic Editor", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button3);

                if(dr == DialogResult.Cancel)
                    return false;

                if(dr == DialogResult.Yes)
                {
                    this.Save();

                    if(this.IsDirty)
                        return false;
                }
                else
                    this.IsDirty = false;    // Don't ask again

                return true;
            }
        }

        /// <inheritdoc />
        public override bool CanSaveContent
        {
            get { return true; }
        }

        /// <inheritdoc />
        public override bool IsContentDocument
        {
            get { return true; }
        }

        /// <inheritdoc />
        public override bool Save()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if(this.IsDirty)
                {
                    editor.SaveFile(this.ToolTipText);
                    this.Text = Path.GetFileName(this.ToolTipText);
                    this.IsDirty = false;
                }

                return true;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to save file.  Reason: " + ex.Message, "Topic Editor",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <inheritdoc />
        public override bool SaveAs()
        {
            using(SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Save Content File As";
                dlg.Filter = "Project Files (*.aml, *.htm*, *.css, *.js, *.content, *.sitemap, " +
                    "*.snippets, *.tokens, *.items)|*.aml;*.htm*;*.css;*.js;*.content;*.sitemap;*.tokens;" +
                    "*.snippets;*.items|Content Files (*.aml, *.htm*)|*.aml;*.htm*|Content Layout Files " +
                    "(*.content, *.sitemap)|*.content;*.sitemap|All Files (*.*)|*.*";
                dlg.DefaultExt = Path.GetExtension(this.ToolTipText);
                dlg.InitialDirectory = Directory.GetCurrentDirectory();

                if(dlg.ShowDialog() == DialogResult.OK)
                    return this.Save(dlg.FileName);
            }

            return false;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Update the font used based on the selected user settings
        /// </summary>
        public void UpdateFont()
        {
            editor.TextEditorProperties.Font = Settings.Default.TextEditorFont;
            editor.TextEditorProperties.ShowLineNumbers = Settings.Default.ShowLineNumbers;
        }

        /// <summary>
        /// Track the last used insert action and update the toolbar button
        /// </summary>
        /// <param name="element">The last used element</param>
        private void TrackLastInsertedElement(ToolStripMenuItem element)
        {
            if(element == null)
                element = miAlert;

            if(element != lastAction)
            {
                lastAction = element;
                tsbInsertElement.Text = lastAction.Text;
                tsbInsertElement.ToolTipText = lastAction.ToolTipText;
            }
        }

        /// <summary>
        /// Insert a link to a topic
        /// </summary>
        /// <param name="extension">The extension of the file in which the
        /// link is being inserted.</param>
        /// <param name="topic">The topic for which to create a link</param>
        /// <remarks>If dropped inside some selected text, the link will
        /// wrap the selected text.</remarks>
        private void InsertTopicLink(string extension, Topic topic)
        {
            TextArea textArea = editor.ActiveTextAreaControl.TextArea;
            int offset = textArea.Caret.Offset;
            string selectedText;

            if(textArea.SelectionManager.HasSomethingSelected &&
              textArea.SelectionManager.SelectionCollection[0].ContainsOffset(offset))
                selectedText = textArea.SelectionManager.SelectionCollection[0].SelectedText;
            else
                selectedText = String.Empty;

            if(extension == ".htm" || extension == ".html")
                ContentEditorControl.InsertString(textArea,
                    topic.ToAnchor(selectedText));
            else
                ContentEditorControl.InsertString(textArea,
                    topic.ToLink(selectedText));
        }

        /// <summary>
        /// Insert a link to a site map table of contents entry (HTML only)
        /// </summary>
        /// <param name="extension">The extension of the file in which the
        /// link is being inserted.</param>
        /// <param name="tocEntry">The TOC entry for which to create a link</param>
        /// <remarks>If dropped inside some selected text, the link will
        /// wrap the selected text.</remarks>
        private void InsertTocLink(string extension, TocEntry tocEntry)
        {
            TextArea textArea = editor.ActiveTextAreaControl.TextArea;
            int offset = textArea.Caret.Offset;
            string selectedText;

            if(textArea.SelectionManager.HasSomethingSelected &&
              textArea.SelectionManager.SelectionCollection[0].ContainsOffset(offset))
                selectedText = textArea.SelectionManager.SelectionCollection[0].SelectedText;
            else
                selectedText = String.Empty;

            if(extension == ".htm" || extension == ".html")
                ContentEditorControl.InsertString(textArea,
                    tocEntry.ToAnchor(selectedText));
            else
                ContentEditorControl.InsertString(textArea,
                    tocEntry.Title);    // Not supported in MAML topics
        }

        /// <summary>
        /// Save the topic to a new filename
        /// </summary>
        /// <param name="filename">The new filename</param>
        /// <returns>True if saved successfully, false if not</returns>
        /// <overloads>There are two overloads for this method</overloads>
        public bool Save(string filename)
        {
            this.Text = Path.GetFileName(filename);
            this.ToolTipText = filename;
            this.IsDirty = true;
            return this.Save();
        }

        /// <summary>
        /// Paste text from the clipboard into the editor
        /// </summary>
        public void PasteFromClipboard()
        {
            editor.Execute(new PasteSpecial(editor));
        }
        #endregion

        #region General event handlers
        //=====================================================================

        /// <summary>
        /// Mark the file as dirty when the text changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void editor_TextChanged(object sender, EventArgs e)
        {
            if(!this.IsDirty)
            {
                this.IsDirty = true;
                this.Text += "*";
            }
        }

        /// <summary>
        /// This is overriden to prompt to save changes if necessary
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = !this.CanClose;
            base.OnClosing(e);
        }
        #endregion

        #region Editor drag and drop handlers
        //=====================================================================

        /// <summary>
        /// This displays the drop cursor when the mouse drags into
        /// the editor.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void editor_DragEnter(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        /// <summary>
        /// This handles the drop operation for the editor
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void editor_DragDrop(object sender, DragEventArgs e)
        {
            TextArea textArea = editor.ActiveTextAreaControl.TextArea;
            DataObject data = e.Data as DataObject;
            Topic topic;
            TocEntry tocEntry;
            string extension = Path.GetExtension(this.ToolTipText).ToLower(CultureInfo.InvariantCulture);

            if(data == null)
                return;

            if(data.GetDataPresent(typeof(Topic)))
            {
                topic = data.GetData(typeof(Topic)) as Topic;

                // Topic links can wrap selected text
                if(topic != null)
                    this.InsertTopicLink(extension, topic);
            }
            else
                if(data.GetDataPresent(typeof(TocEntry)))
                {
                    tocEntry = data.GetData(typeof(TocEntry)) as TocEntry;

                    // Topic links can wrap selected text
                    if(tocEntry != null)
                        this.InsertTocLink(extension, tocEntry);
                }
                else
                    if(data.GetDataPresent(DataFormats.Text))
                        ContentEditorControl.InsertString(textArea, data.GetText());
        }
        #endregion

        #region Toolbar event handlers
        //=====================================================================

        // NOTE: Each method calls editor.Focus() to refocus the editor control.  If not done, the cursor
        // occasionally disappears if the toolbar buttons are clicked or double-clicked in a certain way.
        // Usually it's after using the "para" toolbar dropdown.

        /// <summary>
        /// Insert a basic element such as legacyBold, legacyItalic, etc.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event element</param>
        private void insertElement_Click(object sender, EventArgs e)
        {
            ToolStripItem item = sender as ToolStripItem;

            editor.Execute(new InsertElement(item.Text));
            editor.Focus();
            this.TrackLastInsertedElement(sender as ToolStripMenuItem);
        }

        /// <summary>
        /// Insert a list element
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void insertList_Click(object sender, EventArgs e)
        {
            TextArea textArea = editor.ActiveTextAreaControl.TextArea;
            string style;

            if(sender == tsbListBullet)
                style = "bullet";
            else
                style = "ordered";

            ContentEditorControl.InsertString(textArea, "\r\n<list class=\"" + style + "\">\r\n" +
                "  <listItem><para>Item 1</para></listItem>\r\n  <listItem><para>Item 2</para>" +
                "</listItem>\r\n</list>\r\n");
            editor.Focus();
        }

        /// <summary>
        /// Insert a table element
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tsbTable_Click(object sender, EventArgs e)
        {
            TextArea textArea = editor.ActiveTextAreaControl.TextArea;

            ContentEditorControl.InsertString(textArea,
                "\r\n<table>\r\n  <tableHeader>\r\n    " +
                "<row>\r\n      <entry><para>Header 1</para></entry>\r\n      <entry>" +
                "<para>Header 2</para></entry>\r\n    </row>\r\n  </tableHeader>\r\n  " +
                "<row>\r\n    <entry><para>Column 1</para></entry>\r\n    <entry>" +
                "<para>Column 2</para></entry>\r\n  </row>\r\n</table>\r\n");
            editor.Focus();
        }

        /// <summary>
        /// Insert a link to an in-page address attribute value
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tsbLocalLink_Click(object sender, EventArgs e)
        {
            TextArea textArea = editor.ActiveTextAreaControl.TextArea;
            int offset = textArea.Caret.Offset;
            string selectedText;

            if(textArea.SelectionManager.HasSomethingSelected &&
              textArea.SelectionManager.SelectionCollection[0].ContainsOffset(offset))
                selectedText = textArea.SelectionManager.SelectionCollection[0].SelectedText;
            else
                selectedText = "inner text";

            ContentEditorControl.InsertString(textArea,
                "<link xlink:href=\"#addr\">" + selectedText + "</link>");
            textArea.Caret.Column -= 7;
            editor.Focus();
        }

        /// <summary>
        /// Insert an external link
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tsbExternalLink_Click(object sender, EventArgs e)
        {
            TextArea textArea = editor.ActiveTextAreaControl.TextArea;
            int offset = textArea.Caret.Offset;
            string selectedText;

            if(textArea.SelectionManager.HasSomethingSelected &&
              textArea.SelectionManager.SelectionCollection[0].ContainsOffset(offset))
                selectedText = textArea.SelectionManager.SelectionCollection[0].SelectedText;
            else
                selectedText = "link text";

            ContentEditorControl.InsertString(textArea, "<externalLink>\r\n<linkText>" + selectedText +
                "</linkText>\r\n<linkAlternateText>Optional alternate text</linkAlternateText>\r\n" +
                "<linkUri>http://www.url.com</linkUri>\r\n<linkTarget>_blank</linkTarget>\r\n" +
                "</externalLink>\r\n");
            editor.Focus();
        }

        /// <summary>
        /// Perform the action associated with the last insert item used
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tsbInsertElement_ButtonClick(object sender, EventArgs e)
        {
            if(lastAction != null)
                lastAction.PerformClick();
            else
                miAlert.PerformClick();
        }

        /// <summary>
        /// Insert an alert element
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miAlert_Click(object sender, EventArgs e)
        {
            TextArea textArea = editor.ActiveTextAreaControl.TextArea;
            int offset = textArea.Caret.Offset;
            string selectedText;

            if(textArea.SelectionManager.HasSomethingSelected &&
              textArea.SelectionManager.SelectionCollection[0].ContainsOffset(offset))
                selectedText = textArea.SelectionManager.SelectionCollection[0].SelectedText;
            else
                selectedText = "Alert text";

            ContentEditorControl.InsertString(textArea, "\r\n<alert class=\"note\">\r\n  <para>" + selectedText +
                "</para>\r\n</alert>\r\n");
            editor.Focus();
            this.TrackLastInsertedElement(sender as ToolStripMenuItem);
        }

        /// <summary>
        /// Insert a code element
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miCode_Click(object sender, EventArgs e)
        {
            TextArea textArea = editor.ActiveTextAreaControl.TextArea;

            ContentEditorControl.InsertString(textArea, "\r\n<code language=\"cs\">\r\n/// Code\r\n</code>\r\n");
            editor.Focus();
            this.TrackLastInsertedElement(sender as ToolStripMenuItem);
        }

        /// <summary>
        /// Insert a definition table
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miDefinitionTable_Click(object sender, EventArgs e)
        {
            TextArea textArea = editor.ActiveTextAreaControl.TextArea;

            ContentEditorControl.InsertString(textArea, "\r\n<definitionTable>\r\n" +
                "  <definedTerm>Term 1</definedTerm>\r\n  <definition>" +
                "<para>Definition 1</para></definition>\r\n  <definedTerm>Term 2" +
                "</definedTerm>\r\n  <definition><para>Definition 2</para></definition>" +
                "\r\n</definitionTable>");
            editor.Focus();
            this.TrackLastInsertedElement(sender as ToolStripMenuItem);
        }

        /// <summary>
        /// Insert a section
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void miSection_Click(object sender, EventArgs e)
        {
            TextArea textArea = editor.ActiveTextAreaControl.TextArea;

            ContentEditorControl.InsertString(textArea, "\r\n<section address=\"optionalAddress\">\r\n" +
                "  <title>Title</title>\r\n  <content>\r\n    <para>Content goes here</para>\r\n" +
                "  </content>\r\n</section>\r\n");
            editor.Focus();
            this.TrackLastInsertedElement(sender as ToolStripMenuItem);
        }

        /// <summary>
        /// HTML encode any currently selected text
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tsbHtmlEncode_Click(object sender, EventArgs e)
        {
            editor.Execute(new HtmlEncode());
            editor.Focus();
        }

        /// <summary>
        /// Cut text
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tsbCutText_Click(object sender, EventArgs e)
        {
            editor.Execute(new Cut());
            editor.Focus();
        }

        /// <summary>
        /// Cut text
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tsbCopyText_Click(object sender, EventArgs e)
        {
            editor.Execute(new Copy());
            editor.Focus();
        }

        /// <summary>
        /// Paste text
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tsbPasteText_Click(object sender, EventArgs e)
        {
            this.PasteFromClipboard();
            editor.Focus();
        }

        /// <summary>
        /// Undo editor change
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tsbUndo_Click(object sender, EventArgs e)
        {
            editor.Undo();
            editor.Focus();
        }

        /// <summary>
        /// Redo editor change
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tsbRedo_Click(object sender, EventArgs e)
        {
            editor.Redo();
            editor.Focus();
        }
        #endregion

        #region Find and Replace methods
        //=====================================================================

        /// <summary>
        /// This handles the Perform Find Text event
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void editor_PerformFindText(object sender, EventArgs e)
        {
            FindAndReplaceWindow findWindow = null;

            foreach(IDockContent content in this.DockPanel.Contents)
            {
                findWindow = content as FindAndReplaceWindow;

                if(findWindow != null)
                    break;
            }

            if(findWindow != null && findWindow.Visible)
            {
                if(!findWindow.ShowReplaceControls(false) &&
                  !String.IsNullOrEmpty(findWindow.FindText))
                {
                    if(!this.FindText(findWindow.FindText, findWindow.CaseSensitive))
                        MessageBox.Show("The specified text was not found", Constants.AppName,
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                    findWindow.Activate();
            }
            else
                if(findWindow == null)
                {
                    findWindow = new FindAndReplaceWindow();
                    findWindow.Show(this.DockPanel);
                }
                else
                {
                    findWindow.Activate();
                    findWindow.ShowReplaceControls(false);
                }
        }

        /// <summary>
        /// This handles the Perform Replace Text event
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void editor_PerformReplaceText(object sender, EventArgs e)
        {
            FindAndReplaceWindow findWindow = null;

            foreach(IDockContent content in this.DockPanel.Contents)
            {
                findWindow = content as FindAndReplaceWindow;

                if(findWindow != null)
                    break;
            }

            if(findWindow != null && findWindow.Visible)
            {
                if(findWindow.ShowReplaceControls(true) && !String.IsNullOrEmpty(findWindow.FindText))
                    this.ReplaceText(findWindow.FindText, findWindow.ReplaceWith, findWindow.CaseSensitive);
                else
                    findWindow.Activate();
            }
            else
                if(findWindow == null)
                {
                    findWindow = new FindAndReplaceWindow();
                    findWindow.Show(this.DockPanel);
                    findWindow.ShowReplaceControls(true);
                }
                else
                {
                    findWindow.Activate();
                    findWindow.ShowReplaceControls(true);
                }
        }

        /// <summary>
        /// Find the specified text in the editor
        /// </summary>
        /// <param name="textToFind">The text to find</param>
        /// <param name="caseSensitive">True to do a case-sensitive search
        /// or false to do a case-insensitive search</param>
        /// <returns>True if the text is found, false if not</returns>
        public bool FindText(string textToFind, bool caseSensitive)
        {
            TextLocation start, end;
            TextArea textArea = editor.ActiveTextAreaControl.TextArea;
            StringComparison comparisonType = (caseSensitive) ? StringComparison.CurrentCulture :
                StringComparison.CurrentCultureIgnoreCase;
            string text = textArea.Document.GetText(0, textArea.Document.TextLength);
            int pos, offset = textArea.Caret.Offset;

            if(textToFind == null)
                textToFind = String.Empty;

            pos = text.IndexOf(textToFind, offset, comparisonType);

            if(pos == -1 && offset != 0)
                pos = text.IndexOf(textToFind, 0, offset, comparisonType);

            if(pos != -1)
            {
                start = textArea.Document.OffsetToPosition(pos);
                end = textArea.Document.OffsetToPosition(pos + textToFind.Length);
                textArea.Caret.Position = end;
                textArea.SelectionManager.SetSelection(start, end);
                this.Activate();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Find and replace the next occurrence of the specified text in the
        /// editor.
        /// </summary>
        /// <param name="textToFind">The text to find</param>
        /// <param name="replaceWith">The replacement text</param>
        /// <param name="caseSensitive">True to do a case-sensitive search
        /// or false to do a case-insensitive search</param>
        /// <returns>True if the text is found and replaced, false if not</returns>
        public bool ReplaceText(string textToFind, string replaceWith, bool caseSensitive)
        {
            TextArea textArea = editor.ActiveTextAreaControl.TextArea;
            StringComparison comparisonType = (caseSensitive) ? StringComparison.CurrentCulture :
                StringComparison.CurrentCultureIgnoreCase;
            string text = textArea.Document.GetText(0, textArea.Document.TextLength);
            int offset;

            if(textToFind == null)
                textToFind = String.Empty;

            if(replaceWith == null)
                replaceWith = String.Empty;

            // If the cursor is sitting at the end of the find text, replace
            // the instance at the cursor.
            offset = textArea.Caret.Offset - textToFind.Length;

            if(offset >= 0 && String.Compare(text, offset, textToFind, 0, textToFind.Length, comparisonType) == 0)
            {
                textArea.Document.Replace(offset, textToFind.Length, replaceWith);
                textArea.Caret.Position = textArea.Document.OffsetToPosition(offset + replaceWith.Length);
            }

            // Find the next occurence, if any
            return this.FindText(textToFind, caseSensitive);
        }

        /// <summary>
        /// Find and replace all occurrences of the specified text in the
        /// editor.
        /// </summary>
        /// <param name="textToFind">The text to find</param>
        /// <param name="replaceWith">The replacement text</param>
        /// <param name="caseSensitive">True to do a case-sensitive search
        /// or false to do a case-insensitive search</param>
        /// <returns>True if replacements were made, false if not</returns>
        public bool ReplaceAll(string textToFind, string replaceWith, bool caseSensitive)
        {
            TextArea textArea = editor.ActiveTextAreaControl.TextArea;
            bool nextFound;
            int offset;

            textArea.Caret.Position = textArea.Document.OffsetToPosition(0);

            if(!this.FindText(textToFind, caseSensitive))
                return false;

            offset = textArea.Caret.Offset;

            do
            {
                nextFound = this.ReplaceText(textToFind, replaceWith, caseSensitive);

            } while(offset < textArea.Caret.Offset && nextFound);

            return true;
        }
        #endregion
    }
}
