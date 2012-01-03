//=============================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : ContentLayoutEditorControl.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/02/2012
// Note    : Copyright 2011-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the WPF user control used to edit content layout files
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.3  12/12/2011  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.ConceptualContent;
using SandcastleBuilder.WPF.Commands;

namespace SandcastleBuilder.WPF.UserControls
{
    /// <summary>
    /// This user control is used to edit content layout files
    /// </summary>
    public partial class ContentLayoutEditorControl : UserControl
    {
        #region Private data members
        //=====================================================================

        private TopicCollection topics;
        private IEnumerator<Topic> matchEnumerator;
        private Point startDragPoint;

        // Topics are too complex to serialize to the clipboard.  As such, we'll use this as an internal
        // "clipboard" for cut items within the editor instance.
        private Topic clipboardTopic;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the current topic collection including any edits
        /// </summary>
        public TopicCollection Topics
        {
            get { return topics; }
        }

        /// <summary>
        /// This read-only property returns the current topic
        /// </summary>
        /// <value>If no topic is selected, it returns null</value>
        public Topic CurrentTopic
        {
            get
            {
                if(tvContent == null)
                    return null;

                return tvContent.SelectedItem as Topic;
            }
        }
        #endregion

        #region Routed events
        //=====================================================================

        /// <summary>
        /// This registers the <see cref="ContentModified"/> event
        /// </summary>
        public static readonly RoutedEvent ContentModifiedEvent = EventManager.RegisterRoutedEvent(
            "ContentModifiedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler),
            typeof(ContentLayoutEditorControl));

        /// <summary>
        /// This event is used to signal that the content has been modified
        /// </summary>
        public event RoutedEventHandler ContentModified
        {
            add { AddHandler(ContentModifiedEvent, value); }
            remove { RemoveHandler(ContentModifiedEvent, value); }
        }

        /// <summary>
        /// This registers the <see cref="AssociateTopic"/> event
        /// </summary>
        public static readonly RoutedEvent AssociateTopicEvent = EventManager.RegisterRoutedEvent(
            "AssociateTopicEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler),
            typeof(ContentLayoutEditorControl));

        /// <summary>
        /// This event is used to request the owner associate a file with the current topic
        /// </summary>
        /// <remarks>If handled, the event's <c>Handled</c> property should be set to True</remarks>
        public event RoutedEventHandler AssociateTopic
        {
            add { AddHandler(AssociateTopicEvent, value); }
            remove { RemoveHandler(AssociateTopicEvent, value); }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public ContentLayoutEditorControl()
        {
            // Merge the shared resources
            this.Resources.MergedDictionaries.Add(SharedResources.SplitButtonStyleResources);

            InitializeComponent();

            cboApiParentMode.ItemsSource = (new Dictionary<ApiParentMode, string>
            {
                { ApiParentMode.None, "None" },
                { ApiParentMode.InsertAfter, "Insert after this topic" },
                { ApiParentMode.InsertBefore, "Insert before this topic" },
                { ApiParentMode.InsertAsChild, "Insert as child of this topic" }
            }).ToList();

            dgcboIndex.ItemsSource = MSHelpKeyword.ValidIndexNames;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Load a content layout file for editing
        /// </summary>
        /// <param name="contentLayoutFile">The content layout file item to load</param>
        public void LoadContentLayoutFile(FileItem contentLayoutFile)
        {
            if(contentLayoutFile == null)
                throw new ArgumentNullException("contentLayoutFile",
                    "A content layout file item must be specified");

            topics = new TopicCollection(contentLayoutFile);
            topics.Load();
            topics.ListChanged += new ListChangedEventHandler(topics_ListChanged);

            if(topics.Count !=0 && topics.Find(t => t.IsSelected, false).Count() == 0)
                topics[0].IsSelected = true;

            tvContent.ItemsSource = topics;

            this.topics_ListChanged(this, new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        /// <summary>
        /// Get the text to copy as a link to the clipboard
        /// </summary>
        /// <returns>The string to copy to the clipboard or null if there is nothing to copy</returns>
        private string GetTextToCopy()
        {
            Topic t = tvContent.SelectedItem as Topic;
            string textToCopy;

            if(t != null && !String.IsNullOrEmpty(t.Id))
                textToCopy = String.Format(CultureInfo.InvariantCulture, "<link xlink:href=\"{0}\" />", t.Id);
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
        void topics_ListChanged(object sender, ListChangedEventArgs e)
        {
            Topic selectedTopic = tvContent.SelectedItem as Topic;

            if(e.PropertyDescriptor != null)
                switch(e.PropertyDescriptor.Name)
                {
                    case "DisplayTitle":
                    case "IsExpanded":
                    case "IsSelected":
                        // We don't care about changes to these properties as they are for the
                        // editor and don't affect the state of the topic collection.
                        return;

                    case "ApiParentMode":
                        // There can be only one API content parent
                        if(selectedTopic != null && selectedTopic.ApiParentMode != ApiParentMode.None)
                            foreach(var match in topics.Find(
                              t => t.ApiParentMode != ApiParentMode.None && t != selectedTopic, false))
                                match.ApiParentMode = ApiParentMode.None;
                        break;

                    case "IsDefaultTopic":
                        // There can be only one default topic
                        if(selectedTopic != null && selectedTopic.IsDefaultTopic)
                            foreach(var match in topics.Find(t => t.IsDefaultTopic && t != selectedTopic, false))
                                match.IsDefaultTopic = false;
                        break;

                    case "IsMSHVRootContentContainer":
                        // There can be only one MSHV root container
                        if(selectedTopic != null && selectedTopic.IsMSHVRootContentContainer)
                        {
                            // It can't have any visible children as they won't show up
                            if(selectedTopic.Subtopics.Find(t => t.Visible, false).Any())
                            {
                                MessageBox.Show("The root container cannot contain any visible sub-topics",
                                    "Content Layout Editor", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                                selectedTopic.IsMSHVRootContentContainer = false;
                                return;
                            }

                            foreach(var match in topics.Find(t => t.IsMSHVRootContentContainer &&
                              t != selectedTopic, false))
                                match.IsMSHVRootContentContainer = false;
                        }
                        break;

                    default:
                        break;
                }

            if(sender != this)
                base.RaiseEvent(new RoutedEventArgs(ContentModifiedEvent, this));

            // Update control state based on the collection content
            tvContent.IsEnabled = expFileProps.IsEnabled = expTopicProps.IsEnabled =
                expIndexKeywords.IsEnabled = expHelpAttributes.IsEnabled =
                (topics != null && topics.Count != 0);

            CommandManager.InvalidateRequerySuggested();

            // We must clear the enumerator or it may throw an exception due to collection changes
            if(matchEnumerator != null)
            {
                matchEnumerator.Dispose();
                matchEnumerator = null;
            }
        }

        /// <summary>
        /// This is used to set the command parameter on the Add Child Topic sub-menu items so that
        /// the command event handlers know how to add the topics.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void AddChildTopic_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            ItemCollection items;

            if(sender is MenuItem)
                items = ((MenuItem)sender).Items;
            else
                items = ((ContextMenu)sender).Items;

            foreach(MenuItem mi in items.OfType<MenuItem>())
                mi.CommandParameter = true;
        }

        /// <summary>
        /// Find entities matching the entered text
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            if(topics == null || topics.Count == 0)
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
                matchEnumerator = topics.Find(t =>
                  (!String.IsNullOrEmpty(t.Id) && t.Id.IndexOf(txtFindID.Text,
                    StringComparison.CurrentCultureIgnoreCase) != -1) ||
                  (!String.IsNullOrEmpty(t.DisplayTitle) && t.DisplayTitle.IndexOf(txtFindID.Text,
                    StringComparison.CurrentCultureIgnoreCase) != -1), true).GetEnumerator();

            // Move to the next match
            if(matchEnumerator.MoveNext())
                matchEnumerator.Current.IsSelected = true;
            else
            {
                if(matchEnumerator != null)
                {
                    matchEnumerator.Dispose();
                    matchEnumerator = null;
                }

                MessageBox.Show("No more matches found", "Content Layout Editor", MessageBoxButton.OK,
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

        /// <summary>
        /// Select the item under the cursor if possible so that the context menu appears over the
        /// correct item.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvContent_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;

            if(item != null)
            {
                item.IsSelected = true;
                item.Focus();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Edit the selected topic when the tree view item is double clicked
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvContent_TreeViewItemMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;

            // Only execute this if it's the selected node.  An odd side-effect of how we have to hook up
            // the event handler is that it fires for the selected item and all of its parents up to the
            // root of the tree even if the event is marked as handled.
            if(item != null && item.IsSelected)
                EditorCommands.Edit.Execute(null, item);
        }
        #endregion

        #region Command event handlers
        //=====================================================================

        /// <summary>
        /// Add an empty container topic to the collection that is not associated with any file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdAddItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Topic currentTopic = this.CurrentTopic,
                newTopic = new Topic
                {
                    Title = "Table of Contents Container",
                    TopicFile = null,   // Assign a default GUID
                };

            // If the command parameter is null, add it as a sibling.  If not, add it as a child.
            if(e.Parameter == null || currentTopic == null)
            {
                if(currentTopic == null || topics.Count == 0)
                    topics.Add(newTopic);
                else
                    currentTopic.Parent.Insert(currentTopic.Parent.IndexOf(currentTopic) + 1, newTopic);
            }
            else
            {
                currentTopic.Subtopics.Add(newTopic);
                currentTopic.IsExpanded = true;
            }

            newTopic.IsSelected = true;
        }

        /// <summary>
        /// Determine whether the Collapse/Expand commands can execute
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdExpandCollapse_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (topics != null && topics.Count != 0 &&
                ((e.Command != EditorCommands.CollapseCurrent && e.Command != EditorCommands.ExpandCurrent) ||
                (this.CurrentTopic != null && this.CurrentTopic.Subtopics.Count != 0)));
        }

        /// <summary>
        /// Collapse or expand all topics with children
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdExpandCollapseAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            bool expand = (e.Command == EditorCommands.ExpandAll);

            if(topics != null)
                foreach(var topic in topics.Find(t => t.Subtopics.Count != 0, false))
                    topic.IsExpanded = expand;
        }

        /// <summary>
        /// Collapse or expand the current topic and its children
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdExpandCollapseCurrent_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            bool expand = (e.Command == EditorCommands.ExpandCurrent);

            if(this.CurrentTopic != null && this.CurrentTopic.Subtopics.Count != 0)
            {
                this.CurrentTopic.IsExpanded = expand;

                foreach(var topic in this.CurrentTopic.Subtopics.Find(t => t.Subtopics.Count != 0, false))
                    topic.IsExpanded = expand;
            }
        }

        /// <summary>
        /// Determine whether the Move Up command can execute
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdMoveUp_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (this.CurrentTopic != null && this.CurrentTopic.Parent.IndexOf(this.CurrentTopic) > 0);
        }

        /// <summary>
        /// Move the selected topic up within its parent collection
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdMoveUp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Topic t = tvContent.SelectedItem as Topic;
            TopicCollection parent;
            int idx;

            if(t != null)
            {
                parent = t.Parent;
                idx = parent.IndexOf(t);

                parent.Remove(t);
                parent.Insert(idx - 1, t);
                t.IsSelected = true;
                tvContent.Focus();
            }
        }

        /// <summary>
        /// Determine whether the Move Down command can execute
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdMoveDown_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (this.CurrentTopic != null && this.CurrentTopic.Parent.IndexOf(
                this.CurrentTopic) < this.CurrentTopic.Parent.Count - 1);
        }

        /// <summary>
        /// Move the selected topic down within its parent collection
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdMoveDown_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Topic t = tvContent.SelectedItem as Topic;
            TopicCollection parent;
            int idx;

            if(t != null)
            {
                parent = t.Parent;
                idx = parent.IndexOf(t);

                parent.Remove(t);
                parent.Insert(idx + 1, t);
                t.IsSelected = true;
                tvContent.Focus();
            }
        }

        /// <summary>
        /// Determine whether the Sort command can execute
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdSort_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (this.CurrentTopic != null && this.CurrentTopic.Parent.Count > 1);
        }

        /// <summary>
        /// Sort the topics in the current topic's parent collection by display title
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdSort_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Topic t = tvContent.SelectedItem as Topic;

            if(t != null)
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;

                    t.Parent.Sort();
                    t.IsSelected = true;
                    tvContent.Focus();
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
        }

        /// <summary>
        /// Determine whether the Delete command can execute
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (tvContent != null && tvContent.SelectedItem != null);
        }

        /// <summary>
        /// Delete the selected topic and all of its children
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Topic t = tvContent.SelectedItem as Topic;

            if(t != null && MessageBox.Show(String.Format(CultureInfo.CurrentCulture, "Are you sure you " +
              "want to delete the topic '{0}' and all of its sub-topics?", t.DisplayTitle),
              "Content Layout Editor", MessageBoxButton.YesNo, MessageBoxImage.Question,
              MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                t.Parent.Remove(t);
                tvContent.Focus();
            }
        }

        /// <summary>
        /// Determine if the Copy and Cut commands can be executed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdCopyCut_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (tvContent != null && tvContent.SelectedItem != null);
        }

        /// <summary>
        /// Copy the selected item to the clipboard as a link
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdCopy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string textToCopy;

            if(tvContent.SelectedItem != null)
            {
                textToCopy = this.GetTextToCopy();

                if(textToCopy != null)
                    Clipboard.SetText(textToCopy);
            }
        }

        /// <summary>
        /// Cut the selected topic to the internal clipboard
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdCut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            clipboardTopic = this.CurrentTopic;

            if(clipboardTopic != null)
            {
                clipboardTopic.Parent.Remove(clipboardTopic);
                tvContent.Focus();
            }
        }

        /// <summary>
        /// Determine if the Paste command can be executed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdPaste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (topics != null && clipboardTopic != null);
        }

        /// <summary>
        /// Paste the selected topic as a sibling or child of the selected topic
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Topic targetTopic = this.CurrentTopic, newTopic = clipboardTopic;

            if(newTopic != null)
            {
                // Don't allow pasting multiple copies of the same item in here as the IDs must be unique
                clipboardTopic = null;

                if(targetTopic == null)
                    topics.Add(newTopic);
                else
                {
                    if(e.Command == EditorCommands.PasteAsChild)
                    {
                        targetTopic.Subtopics.Add(newTopic);
                        targetTopic.IsExpanded = true;
                    }
                    else
                        targetTopic.Parent.Insert(targetTopic.Parent.IndexOf(targetTopic) + 1, newTopic);
                }

                newTopic.IsSelected = true;
                tvContent.Focus();
            }
        }

        /// <summary>
        /// View help for this editor
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Utility.ShowHelpTopic("54e3dc97-5125-441e-8e84-7f9303e95f26");
        }

        /// <summary>
        /// Associate a topic file with the selected node
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAssociateTopic_Click(object sender, RoutedEventArgs e)
        {
            Topic t = tvContent.SelectedItem as Topic;

            if(t != null)
            {
                // Let the caller prompt for the filename and add it to the project if necessary
                RoutedEventArgs args = new RoutedEventArgs(AssociateTopicEvent, this);
                base.RaiseEvent(args);

                // If associated, refresh the bindings
                if(args.Handled)
                {
                    txtID.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                    txtRevNumber.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                    txtTopicFilename.GetBindingExpression(TextBox.TextProperty).UpdateTarget();

                    this.topics_ListChanged(tvContent, new ListChangedEventArgs(ListChangedType.Reset, -1));
                }
            }
        }

        /// <summary>
        /// Clear the topic associated with the selected node
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnClearTopic_Click(object sender, RoutedEventArgs e)
        {
            Topic t = tvContent.SelectedItem as Topic;

            if(t != null && MessageBox.Show("Do you want to clear the file associated with this topic?",
              "Content Layout Editor", MessageBoxButton.YesNo, MessageBoxImage.Question,
              MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                t.TopicFile = null;

                txtID.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                txtRevNumber.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                txtTopicFilename.GetBindingExpression(TextBox.TextProperty).UpdateTarget();

                this.topics_ListChanged(tvContent, new ListChangedEventArgs(ListChangedType.Reset, -1));
            }
        }

        /// <summary>
        /// Refresh the topic file associations to reflect changes made to the project elsewhere (i.e. in the
        /// Solution Explorer).
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnRefreshAssociations_Click(object sender, RoutedEventArgs e)
        {
            topics.MatchProjectFilesToTopics();

            txtID.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            txtRevNumber.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            txtTopicFilename.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
        }
        #endregion

        #region Tree view drag and drop event handlers
        //=====================================================================

        /// <summary>
        /// This is used to note the starting mouse position in order to trigger drag and drop operations
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvContent_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startDragPoint = e.GetPosition(null);
        }

        /// <summary>
        /// Allow drag and drop of the items.  The items are converted to their text form to allow dragging
        /// and dropping in topic files.  They can also be dragged and dropped as topics within the tree view
        /// to rearrange the topics.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvContent_MouseMove(object sender, MouseEventArgs e)
        {
            DataObject data = new DataObject();
            Topic currentTopic = this.CurrentTopic;
            Point currentPosition = e.GetPosition(null);
            string textToCopy;

            if(e.LeftButton == MouseButtonState.Pressed &&
              (Math.Abs(currentPosition.X - startDragPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
              Math.Abs(currentPosition.Y - startDragPoint.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                // Make sure we are actually within a tree view item
                var item = (e.OriginalSource as FrameworkElement).ParentElementOfType<TreeViewItem>();

                // Make sure the items match to prevent inadvertent drag and drops if the mouse is clicked and
                // dragged outside of an item into an item.
                if(item != null && (item.Header as Topic) == currentTopic)
                {
                    textToCopy = this.GetTextToCopy();

                    if(textToCopy != null)
                        data.SetText(textToCopy);

                    data.SetData(typeof(Topic), currentTopic);

                    DragDrop.DoDragDrop(tvContent, data, DragDropEffects.All);

                    // Make sure the drag source is selected when done.  This keeps the item selected to make
                    // it easier to go back to the same location when dragging topics into a file editor to
                    // create a link.
                    currentTopic.IsSelected = true;
                }
                else
                    startDragPoint = currentPosition;
            }
        }

        /// <summary>
        /// This validates the drop target during the drag operation
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvContent_DragOver(object sender, DragEventArgs e)
        {
            FrameworkElement sourceElement = e.OriginalSource as FrameworkElement;
            Topic dragSource, dropTarget;
            
            e.Effects = DragDropEffects.None;
            e.Handled = true;

            if(e.Data.GetDataPresent(typeof(Topic)))
            {
                tvContent.AutoScrollIfNeeded(e.GetPosition(tvContent));

                // Make sure we are actually within an item in the source's tree view
                var tree = sourceElement.ParentElementOfType<TreeView>();

                if(tree == tvContent)
                {
                    var item = sourceElement.ParentElementOfType<TreeViewItem>();

                    if(item != null)
                    {
                        item.IsSelected = true;

                        dropTarget = this.CurrentTopic;
                        dragSource = (Topic)e.Data.GetData(typeof(Topic));

                        // Check that the dragged item is not the drop target and also that the drop target is
                        // not a child of the dragged item.  Either condition makes for an invalid drop target.
                        if(dropTarget != null && dragSource != null && dropTarget != dragSource &&
                          !dragSource.Subtopics.Find(t => t == dropTarget, false).Any())
                            e.Effects = DragDropEffects.Move;
                    }
                }
            }
        }

        /// <summary>
        /// This handles the drop operation for the tree view
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvContent_Drop(object sender, DragEventArgs e)
        {
            Topic dragSource, dropTarget = this.CurrentTopic;
            int offset;

            if(e.Data.GetDataPresent(typeof(Topic)) && dropTarget != null)
            {
                dragSource = (Topic)e.Data.GetData(typeof(Topic));

                if(dragSource != null && dragSource != dropTarget)
                {
                    // If the drag source is the next sibling of the drop target, insert it in the drop target's
                    // location (swap them).
                    offset = (dropTarget.Parent.Contains(dragSource) && dropTarget.Parent.IndexOf(dragSource) ==
                        dropTarget.Parent.IndexOf(dropTarget) + 1) ? 0 : 1;

                    dragSource.Parent.Remove(dragSource);

                    // If Shift is not held down, make it a sibling of the drop target.
                    // If Shift is help down, make it a child of the drop target.
                    if((e.KeyStates & DragDropKeyStates.ShiftKey) == 0)
                        dropTarget.Parent.Insert(dropTarget.Parent.IndexOf(dropTarget) + offset, dragSource);
                    else
                    {
                        dropTarget.Subtopics.Add(dragSource);
                        dropTarget.IsExpanded = true;
                    }

                    dragSource.IsSelected = true;
                }

                // Mark it as handled or Visual Studio tries to use it and fails
                e.Handled = true;
            }
        }
        #endregion
    }
}
