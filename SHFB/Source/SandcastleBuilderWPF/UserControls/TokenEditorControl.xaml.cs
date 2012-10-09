//=============================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : TokenEditorControl.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/26/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the WPF user control used to edit token files
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.3  12/21/2011  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.ConceptualContent;

namespace SandcastleBuilder.WPF.UserControls
{
    /// <summary>
    /// This user control is used to edit token files
    /// </summary>
    public partial class TokenEditorControl : UserControl
    {
        #region Private data members
        //=====================================================================

        private TokenCollection tokens;
        private IEnumerator<Token> matchEnumerator;
        private Point startDragPoint;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the current token collection including any edits
        /// </summary>
        public TokenCollection Tokens
        {
            get { return tokens; }
        }
        #endregion

        #region Routed events
        //=====================================================================

        /// <summary>
        /// This registers the <see cref="ContentModified"/> event
        /// </summary>
        public static readonly RoutedEvent ContentModifiedEvent = EventManager.RegisterRoutedEvent(
            "ContentModifiedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler),
            typeof(TokenEditorControl));

        /// <summary>
        /// This event is used to signal that the content has been modified
        /// </summary>
        public event RoutedEventHandler ContentModified
        {
            add { AddHandler(ContentModifiedEvent, value); }
            remove { RemoveHandler(ContentModifiedEvent, value); }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public TokenEditorControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Load a token file for editing
        /// </summary>
        /// <param name="tokenFile">The token file to load</param>
        public void LoadTokenFile(string tokenFile)
        {
            if(tokenFile == null)
                throw new ArgumentNullException("tokenFile", "A token filename must be specified");

            tokens = new TokenCollection(tokenFile);
            tokens.Load();

            tokens.ListChanged += new ListChangedEventHandler(tokens_ListChanged);

            if(tokens.Count != 0)
                tokens[0].IsSelected = true;

            lbTokens.ItemsSource = tokens;

            this.tokens_ListChanged(this, new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        /// <summary>
        /// Get the text to copy as a link to the clipboard
        /// </summary>
        /// <returns>The string to copy to the clipboard or null if there is nothing to copy</returns>
        private string GetTextToCopy()
        {
            Token t = lbTokens.SelectedItem as Token;
            string textToCopy;

            if(t != null)
                textToCopy = t.ToToken();
            else
                textToCopy = null;

            return textToCopy;
        }
        #endregion

        #region General event handlers
        //=====================================================================

        /// <summary>
        /// This is used to mark the file as dirty when the collection changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        void tokens_ListChanged(object sender, ListChangedEventArgs e)
        {
            if(e.PropertyDescriptor != null)
                switch(e.PropertyDescriptor.Name)
                {
                    case "IsSelected":
                        // We don't care about changes to these properties as they are for the
                        // editor and don't affect the state of the token collection.
                        return;

                    default:
                        break;
                }

            if(sender != this)
                base.RaiseEvent(new RoutedEventArgs(ContentModifiedEvent, this));

            // Update control state based on the collection content
            lbTokens.IsEnabled = txtTokenName.IsEnabled = txtTokenValue.IsEnabled =
                (tokens != null && tokens.Count != 0);

            CommandManager.InvalidateRequerySuggested();

            // We must clear the enumerator or it may throw an exception due to collection changes
            if(matchEnumerator != null)
            {
                matchEnumerator.Dispose();
                matchEnumerator = null;
            }
        }

        /// <summary>
        /// Find entities matching the entered text
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            if(tokens == null || tokens.Count == 0)
                return;

            if(txtFindID.Text.Trim().Length == 0)
            {
                if(matchEnumerator != null)
                {
                    matchEnumerator.Dispose();
                    matchEnumerator = null;
                }

                return;
            }

            txtFindID.Text = txtFindID.Text.Trim();

            // If this is the first time, get all matches
            if(matchEnumerator == null)
                matchEnumerator = tokens.Find(t =>
                  (!String.IsNullOrEmpty(t.TokenName) && t.TokenName.IndexOf(txtFindID.Text,
                    StringComparison.CurrentCultureIgnoreCase) != -1) ||
                  (!String.IsNullOrEmpty(t.TokenValue) && t.TokenValue.IndexOf(txtFindID.Text,
                    StringComparison.CurrentCultureIgnoreCase) != -1)).GetEnumerator();

            // Move to the next match
            if(matchEnumerator.MoveNext())
            {
                matchEnumerator.Current.IsSelected = true;
                lbTokens.ScrollIntoView(matchEnumerator.Current);
            }
            else
            {
                if(matchEnumerator != null)
                {
                    matchEnumerator.Dispose();
                    matchEnumerator = null;
                }

                MessageBox.Show("No more matches found", "Token Editor", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Clear the match enumerator when the text changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void txtFindID_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(matchEnumerator != null)
            {
                matchEnumerator.Dispose();
                matchEnumerator = null;
            }
        }

        /// <summary>
        /// Find entities matching the entered text when Enter is hit
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void txtFindID_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter && btnGo.IsEnabled)
            {
                e.Handled = true;
                btnGo_Click(sender, null);
            }
        }
        #endregion

        #region Command event handlers
        //=====================================================================

        /// <summary>
        /// Add a new token to the collection
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdAddItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(tokens != null)
            {
                Token t = new Token { TokenValue = "Add token content here" };
                tokens.Add(t);

                t.IsSelected = true;
                lbTokens.ScrollIntoView(t);

                txtTokenName.SelectAll();
                txtTokenValue.SelectAll();
                txtTokenName.Focus();
            }
        }

        /// <summary>
        /// Determine whether the Delete command can execute
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (lbTokens != null && lbTokens.SelectedItem != null);
        }

        /// <summary>
        /// Delete the selected token
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Token t = lbTokens.SelectedItem as Token;

            if(t != null && MessageBox.Show(String.Format(CultureInfo.CurrentCulture, "Are you sure you " +
              "want to delete the token '{0}'?", t.TokenName), "Token Editor", MessageBoxButton.YesNo,
              MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                tokens.Remove(t);
                lbTokens.Focus();
            }
        }

        /// <summary>
        /// Determine if the Copy commands can be executed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdCopy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (lbTokens != null && lbTokens.SelectedItem != null);
        }

        /// <summary>
        /// Copy the selected item to the clipboard as a link
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdCopy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string textToCopy;

            if(lbTokens.SelectedItem != null)
            {
                textToCopy = this.GetTextToCopy();

                if(textToCopy != null)
                    Clipboard.SetText(textToCopy);
            }
        }

        /// <summary>
        /// View help for this editor
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Utility.ShowHelpTopic("ed6870bb-772d-4596-9fc1-5638ae6d621b");
        }
        #endregion

        #region List box drag and drop event handlers
        //=====================================================================

        /// <summary>
        /// This is used to note the starting mouse position in order to trigger drag and drop operations
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbTokens_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startDragPoint = e.GetPosition(null);
        }

        /// <summary>
        /// Allow drag and drop of the items.  The items are converted to their text form to allow dragging
        /// and dropping in topic files.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbTokens_MouseMove(object sender, MouseEventArgs e)
        {
            Token currentToken = lbTokens.SelectedItem as Token;
            Point currentPosition = e.GetPosition(null);
            string textToCopy;

            if(e.LeftButton == MouseButtonState.Pressed &&
              (Math.Abs(currentPosition.X - startDragPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
              Math.Abs(currentPosition.Y - startDragPoint.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                // Make sure we are actually within a token item
                var item = (e.OriginalSource as FrameworkElement).ParentElementOfType<ListBoxItem>();

                // Make sure the items match to prevent inadvertent drag and drops if the mouse is clicked and
                // dragged outside of an item into an item.
                if(item != null && (item.Content as Token) == currentToken)
                {
                    textToCopy = this.GetTextToCopy();

                    if(textToCopy != null)
                        DragDrop.DoDragDrop(lbTokens, textToCopy, DragDropEffects.Copy);

                    // Make sure the drag source is selected when done.  This keeps the item selected to make
                    // it easier to go back to the same location when dragging tokens into a file editor to
                    // create a link.
                    currentToken.IsSelected = true;
                    lbTokens.ScrollIntoView(currentToken);
                }
                else
                    startDragPoint = currentPosition;
            }
        }
        #endregion
    }
}
