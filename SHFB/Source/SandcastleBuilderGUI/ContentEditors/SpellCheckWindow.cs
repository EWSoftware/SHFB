//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : SpellCheckWindow.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/19/2021
// Note    : Copyright 2013-2021, Eric Woodruff, All rights reserved
//
// This file contains the form used to handle spell checking in the text editor windows
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/11/2013  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using SandcastleBuilder.Gui.Spelling;

using WeifenLuo.WinFormsUI.Docking;

namespace SandcastleBuilder.Gui.ContentEditors
{
    /// <summary>
    /// This form is used to handle spell checking in the text editor windows
    /// </summary>
    /// <remarks>This is rather crude but it works.  It's the best I could do after poking around in the editor
    /// code.  It will do for the time being even if it isn't the most efficient way of doing it.</remarks>
    public partial class SpellCheckWindow : BaseContentEditor
    {
        #region Spelling issue
        //=====================================================================

        /// <summary>
        /// This is used to track spelling issues
        /// </summary>
        private class SpellingIssue
        {
            /// <summary>
            /// True for a misspelling, false for a doubled word
            /// </summary>
            public bool IsMisspelling { get; set; }

            /// <summary>
            /// The word
            /// </summary>
            public string Word { get; set; }

            /// <summary>
            /// The location of the issue
            /// </summary>
            public Point Location { get; set; }

            /// <summary>
            /// The suggestions for misspelled words
            /// </summary>
            public List<string> Suggestions { get; set; }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private DockPane currentParent;
        private TopicEditorWindow currentTopicWindow;
        private GlobalDictionary dictionary;

        private readonly List<SpellingIssue> issues;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public SpellCheckWindow()
        {
            InitializeComponent();

            issues = new List<SpellingIssue>();
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Get the project explorer window
        /// </summary>
        /// <returns>The project explorer window or null if not found</returns>
        private ProjectExplorerWindow FindProjectExplorerWindow()
        {
            ProjectExplorerWindow projectExplorer = null;

            foreach(IDockContent content in this.DockPanel.Contents)
            {
                projectExplorer = content as ProjectExplorerWindow;

                if(projectExplorer != null)
                    break;
            }

            return projectExplorer;
        }

        /// <summary>
        /// Get the active document window
        /// </summary>
        /// <returns>The active topic editor window or null if not found</returns>
        private TopicEditorWindow FindActiveDocumentWindow()
        {
            TopicEditorWindow topicWindow = this.DockPanel.ActiveDocument as TopicEditorWindow;

            if(topicWindow == null)
                foreach(IDockContent content in this.DockPanel.Documents)
                {
                    topicWindow = content as TopicEditorWindow;

                    if(topicWindow != null)
                        break;
                }

            return topicWindow;
        }

        /// <summary>
        /// Update the state of the controls based on the current issue
        /// </summary>
        private void UpdateState()
        {
            btnReplace.Enabled = btnReplaceAll.Enabled = btnIgnoreOnce.Enabled = btnIgnoreAll.Enabled =
                btnAddWord.Enabled = false;
            lblIssue.Text = "Misspelled Word";
            lblMisspelledWord.Text = null;
            lbSuggestions.Items.Clear();

            if(issues.Count == 0)
            {
                lblMisspelledWord.Text = "(No more issues)";
                return;
            }

            var issue = issues[0];

            if(!issue.IsMisspelling)
            {
                lblIssue.Text = "Doubled Word";
                btnReplace.Enabled = btnIgnoreOnce.Enabled = true;
                lbSuggestions.Items.Add("(Delete word)");
            }
            else
            {
                btnIgnoreOnce.Enabled = btnIgnoreAll.Enabled = true;

                if(issue.Suggestions.Count != 0)
                {
                    btnReplace.Enabled = btnReplaceAll.Enabled = btnAddWord.Enabled = true;
                    lbSuggestions.Items.AddRange(issue.Suggestions.ToArray());
                }
                else
                    lbSuggestions.Items.Add("(No suggestions)");
            }

            lblMisspelledWord.Text = issue.Word;
            lbSuggestions.SelectedIndex = 0;

            currentTopicWindow.MoveToPositionAndHighlightText(issue.Location, issue.Word.Length);
        }

        /// <summary>
        /// Adjust the position of issues on the same line as the given issue
        /// </summary>
        /// <param name="line">The line used to match other issues</param>
        /// <param name="offset">The offset by which to adjust issue locations</param>
        private void AdjustIssuePositions(int line, int offset)
        {
            foreach(var issue in issues)
                if(issue.Location.Y == line)
                    issue.Location = new Point(issue.Location.X + offset, issue.Location.Y);
        }
        #endregion

        #region General event handlers
        //=====================================================================

        /// <summary>
        /// Clear the current issues when hidden
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void SpellCheckWindow_VisibleChanged(object sender, EventArgs e)
        {
            this.SpellCheckWindow_CurrentTopicClosed(sender, e);
        }

        /// <summary>
        /// When the parent changes, hook up the activation change event
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void SpellCheckWindow_ParentChanged(object sender, EventArgs e)
        {
            if(currentParent != null)
                currentParent.IsActivatedChanged -= SpellCheckWindow_ParentActivatedChanged;

            currentParent = this.Parent as DockPane;

            if(currentParent != null)
                currentParent.IsActivatedChanged += SpellCheckWindow_ParentActivatedChanged;

            if(currentTopicWindow != null)
                SpellCheckWindow_CurrentTopicClosed(sender, e);
        }

        /// <summary>
        /// When the current topic window closes, disconnect from it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void SpellCheckWindow_CurrentTopicClosed(object sender, EventArgs e)
        {
            if(currentTopicWindow != null)
                currentTopicWindow.FormClosed -= SpellCheckWindow_CurrentTopicClosed;

            currentTopicWindow = null;

            issues.Clear();
            this.UpdateState();

            lblMisspelledWord.Text = null;
        }

        /// <summary>
        /// When the parent is activated, make sure we're still spell checking the same document.  If not,
        /// spell check the new one if there is one.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void SpellCheckWindow_ParentActivatedChanged(object sender, EventArgs e)
        {
            if(!currentParent.IsActivated)
                return;

            // When floating, force the parent window to show the entire control
            if(this.DockState == DockState.Float && (this.Parent.Parent.Size.Width < this.Size.Width ||
              this.Parent.Parent.Size.Height < this.Size.Height))
                this.Parent.Parent.Size = new Size(this.Size.Width + 20, this.Size.Height + 20);

            var activeWindow = this.FindActiveDocumentWindow();

            if(activeWindow != currentTopicWindow)
            {
                issues.Clear();
                this.UpdateState();

                lblMisspelledWord.Text = null;
                currentTopicWindow = activeWindow;

                if(currentTopicWindow != null)
                {
                    currentTopicWindow.FormClosed += SpellCheckWindow_CurrentTopicClosed;

                    if(this.SpellCheckCurrentDocument())
                        this.UpdateState();
                }
            }
        }

        /// <summary>
        /// When an item is double clicked, handle it as a request to replace the misspelling with the selected
        /// word.  If Ctrl is held down, replace all occurrences of the misspelled word.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbSuggestions_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int idx = lbSuggestions.IndexFromPoint(e.Location);

            if(idx != -1)
                if((ModifierKeys & Keys.Control) == 0)
                    btnReplace_Click(sender, e);
                else
                    btnReplaceAll_Click(sender, e);
        }

        /// <summary>
        /// Replace the current misspelled word with the selected word
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnReplace_Click(object sender, EventArgs e)
        {
            if(lbSuggestions.SelectedIndex < 0)
            {
                if(lbSuggestions.Items.Count != 0)
                    lbSuggestions.SelectedIndex = 0;

                return;
            }

            string replacement = issues[0].IsMisspelling ? (string)lbSuggestions.SelectedItem : String.Empty;
            var issue = issues[0];
            
            int length = currentTopicWindow.ReplaceTextAt(issue.Location, issue.Word.Length, replacement);

            issues.RemoveAt(0);

            this.AdjustIssuePositions(issue.Location.Y, replacement.Length - length);
            this.UpdateState();
        }

        /// <summary>
        /// Replace all occurrences of the misspelled word with the selected word
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnReplaceAll_Click(object sender, EventArgs e)
        {
            if(lbSuggestions.SelectedIndex < 0)
            {
                lbSuggestions.SelectedIndex = 0;
                return;
            }

            string word = issues[0].Word, replacement = (string)lbSuggestions.SelectedItem;
            int length;

            foreach(var issue in issues.Where(i => i.Word.Equals(word, StringComparison.OrdinalIgnoreCase)).ToList())
            {
                // Match the case of the first letter if necessary
                if(replacement.Length > 1 && (Char.IsUpper(replacement[0]) != Char.IsUpper(replacement[1]) ||
                  (Char.IsLower(replacement[0]) && Char.IsLower(replacement[1]))))
                    if(Char.IsUpper(issue.Word[0]) && !Char.IsUpper(replacement[0]))
                    {
                        replacement = replacement.Substring(0, 1).ToUpper(
                            SpellCheckerConfiguration.DefaultLanguage) + replacement.Substring(1);
                    }
                    else
                        if(Char.IsLower(issue.Word[0]) && !Char.IsLower(replacement[0]))
                            replacement = replacement.Substring(0, 1).ToLower(
                                SpellCheckerConfiguration.DefaultLanguage) + replacement.Substring(1);

                length = currentTopicWindow.ReplaceTextAt(issue.Location, word.Length, replacement);

                issues.Remove(issue);

                this.AdjustIssuePositions(issue.Location.Y, replacement.Length - length);
            }

            this.UpdateState();
        }

        /// <summary>
        /// Ignore just the current occurrence of the misspelled word
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnIgnoreOnce_Click(object sender, EventArgs e)
        {
            issues.RemoveAt(0);
            this.UpdateState();
        }

        /// <summary>
        /// Ignore all occurrences of the misspelled word
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnIgnoreAll_Click(object sender, EventArgs e)
        {
            string word = issues[0].Word;

            if(sender == btnIgnoreAll)
                dictionary.IgnoreWord(word);

            foreach(var issue in issues.Where(i => i.Word.Equals(word, StringComparison.OrdinalIgnoreCase)).ToList())
                issues.Remove(issue);

            this.UpdateState();
        }

        /// <summary>
        /// Add the word to the global dictionary
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAddWord_Click(object sender, EventArgs e)
        {
            dictionary.AddWordToDictionary(issues[0].Word);

            this.btnIgnoreAll_Click(sender, e);
        }
        #endregion

        #region Spell checking methods and event handlers
        //=====================================================================

        /// <summary>
        /// Spell check the current document and start showing suggestions for replacement
        /// </summary>
        private bool SpellCheckCurrentDocument()
        {
            bool success = false;

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                // Use the language from the current project to do the spell checking
                var projectExplorer = this.FindProjectExplorerWindow();

                if(dictionary == null || (projectExplorer != null &&
                  projectExplorer.CurrentProject.Language != dictionary.Language))
                {
                    dictionary = GlobalDictionary.CreateGlobalDictionary(projectExplorer?.CurrentProject.Language);
                }

                if(dictionary == null)
                    lblMisspelledWord.Text = "Unable to create dictionary!";
                else
                {
                    var speller = new FileSpellChecker(dictionary);

                    speller.MisspelledWord += new EventHandler<SpellingEventArgs>(speller_MisspelledWord);
                    speller.DoubledWord += new EventHandler<SpellingEventArgs>(speller_DoubledWord);

                    string text = currentTopicWindow.GetAllText();

                    string ext = Path.GetExtension(currentTopicWindow.Filename).ToLowerInvariant();

                    switch(ext)
                    {
                        case ".aml":
                        case ".axml":
                        case ".ascx":
                        case ".asp":
                        case ".aspx":
                        case ".config":
                        case ".content":
                        case ".htm":
                        case ".html":
                        case ".items":
                        case ".sitemap":
                        case ".snippets":
                        case ".tokens":
                        case ".xaml":
                        case ".xml":
                        case ".xsl":
                        case ".xslt":
                        case ".xamlcfg":
                            using(var ms = new MemoryStream(Encoding.UTF8.GetBytes(text)))
                            using(var reader = XmlReader.Create(ms, new XmlReaderSettings {
                              DtdProcessing = DtdProcessing.Ignore }))
                            {
                                speller.SpellCheckXmlReader(reader);
                            }
                            break;

                        default:
                            speller.SpellCheckText(text);
                            break;
                    }

                    success = true;
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                lblMisspelledWord.Text = "Unable to spell check file: " + ex.Message;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            return success;
        }

        /// <summary>
        /// Record the location of a misspelled word
        /// </summary>
        private void speller_MisspelledWord(object sender, SpellingEventArgs e)
        {
            issues.Add(new SpellingIssue
            {
                IsMisspelling = true,
                Word = e.Word,
                Location = e.Position,
                Suggestions = dictionary.SuggestCorrections(e.Word).ToList()
            });
        }

        /// <summary>
        /// Record the location of a doubled word
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void speller_DoubledWord(object sender, SpellingEventArgs e)
        {
            issues.Add(new SpellingIssue
            {
                Word = e.Word,
                Location = e.Position,
            });
        }
        #endregion
    }
}
