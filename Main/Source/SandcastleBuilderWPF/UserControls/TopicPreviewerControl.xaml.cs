//=============================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : TopicPreviewerControl.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/05/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to preview MAML topic files.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.4  01/02/2012  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.ConceptualContent;
using SandcastleBuilder.WPF.Commands;
using SandcastleBuilder.WPF.Maml;

namespace SandcastleBuilder.WPF.UserControls
{
    /// <summary>
    /// This control is used to preview MAML topic files
    /// </summary>
    public partial class TopicPreviewerControl : UserControl
    {
        #region Private data members
        //=====================================================================

        private SandcastleProject currentProject;
        private TocEntryCollection tableOfContents;
        private MamlToFlowDocumentConverter converter;
        private List<Uri> browserHistory;
        private int historyLocation;
        private bool isNavigating;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to set or get the current project
        /// </summary>
        public SandcastleProject CurrentProject
        {
            get { return currentProject; }
            set
            {
                currentProject = value;

                tableOfContents = null;

                if(this.IsLoaded)
                    this.LoadTableOfContentsInfo();
            }
        }

        /// <summary>
        /// This read-only property returns the currently selected topic
        /// </summary>
        public TocEntry CurrentTopic
        {
            get { return tvContent.SelectedItem as TocEntry; }
        }
        #endregion

        #region Routed events
        //=====================================================================

        /// <summary>
        /// This registers the <see cref="FileContentNeeded"/> event
        /// </summary>
        public static readonly RoutedEvent FileContentNeededEvent = EventManager.RegisterRoutedEvent(
            "FileContentNeededEvent", RoutingStrategy.Bubble, typeof(EventHandler<FileContentNeededEventArgs>),
            typeof(TopicPreviewerControl));

        /// <summary>
        /// This event is used to get information for token, content layout, and site map files that are
        /// open in editors so that current information is displayed.
        /// </summary>
        public event EventHandler<FileContentNeededEventArgs> FileContentNeeded
        {
            add { AddHandler(FileContentNeededEvent, value); }
            remove { RemoveHandler(FileContentNeededEvent, value); }
        }

        /// <summary>
        /// This registers the <see cref="TopicContentNeeded"/> event
        /// </summary>
        public static readonly RoutedEvent TopicContentNeededEvent = EventManager.RegisterRoutedEvent(
            "TopicContentNeededEvent", RoutingStrategy.Bubble, typeof(EventHandler<TopicContentNeededEventArgs>),
            typeof(TopicPreviewerControl));

        /// <summary>
        /// This event is used to get the content of a topic file that is open in an editor so that current
        /// information is displayed.
        /// </summary>
        public event EventHandler<TopicContentNeededEventArgs> TopicContentNeeded
        {
            add { AddHandler(TopicContentNeededEvent, value); }
            remove { RemoveHandler(TopicContentNeededEvent, value); }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public TopicPreviewerControl()
        {
            // Load the language block titles on first use
            if(MamlToFlowDocumentConverter.LanguageTitles.Count == 0)
                LoadLanguageTitles();

            converter = new MamlToFlowDocumentConverter();
            browserHistory = new List<Uri>();
            historyLocation = -1;

            // Merge the shared resources
            this.Resources.MergedDictionaries.Add(SharedResources.SplitButtonStyleResources);

            InitializeComponent();
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Refresh the table of contents and the current topic to show any changes made to them
        /// </summary>
        /// <param name="reloadLastTopic">True to reload the last topic, false to ignore it</param>
        public void Refresh(bool reloadLastTopic)
        {
            TocEntry t = this.CurrentTopic;

            if(t != null)
            {
                this.LoadTableOfContentsInfo();

                // Go back to the last selected topic if it is still there and wanted
                if(reloadLastTopic)
                    this.FindAndDisplay(t.SourceFile);
            }
        }

        /// <summary>
        /// This is used to find a topic by filename and display it
        /// </summary>
        /// <param name="filename">The filename of the topic to display</param>
        public void FindAndDisplay(string filename)
        {
            if(tableOfContents == null)
            {
                if(this.IsVisible)
                    this.LoadTableOfContentsInfo();

                if(tableOfContents == null)
                    return;
            }

            if(!String.IsNullOrEmpty(filename))
            {
                var t = tableOfContents.Find(filename);

                if(t != null && !String.IsNullOrEmpty(t.Id))
                    this.NavigateToTopic(new Uri("link://" + t.Id));
            }
        }

        /// <summary>
        /// This is used to load the language block titles on first use
        /// </summary>
        private static void LoadLanguageTitles()
        {
            try
            {
                string highlighterFile = Path.Combine(Environment.ExpandEnvironmentVariables("%SHFBROOT%"),
                    @"Colorizer\highlight.xml");

                if(File.Exists(highlighterFile))
                {
                    XDocument doc = XDocument.Load(highlighterFile);
                    var langTitles = MamlToFlowDocumentConverter.LanguageTitles;

                    // Get the variations first
                    foreach(var lang in doc.Descendants("map").Descendants("language"))
                        langTitles.Add(lang.Attribute("from").Value, lang.Attribute("to").Value);

                    // Get the main language IDs and titles and also assign them to the variations
                    foreach(var lang in doc.Descendants("languages").Descendants("language"))
                    {
                        langTitles.Add(lang.Attribute("id").Value, lang.Attribute("name").Value);

                        var variations = langTitles.Where(kw => kw.Value.Equals(lang.Attribute("id").Value,
                            StringComparison.OrdinalIgnoreCase)).ToArray();

                        foreach(var v in variations)
                            langTitles[v.Key] = lang.Attribute("name").Value;
                    }

                    // Map "none" to an empty title
                    langTitles["none"] = " ";
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to load language titles for code blocks.  Reason: " + ex.Message,
                    Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// This loads the tree view with table of contents file entries from the project
        /// </summary>
        /// <remarks>Token information is also loaded here and passed on to the converter.</remarks>
        private void LoadTableOfContentsInfo()
        {
            FileItemCollection imageFiles, tokenFiles, contentLayoutFiles;
            List<ITableOfContents> tocFiles;
            TopicCollection contentLayout;
            TokenCollection tokens;

            tvContent.ItemsSource = null;
            tableOfContents = null;
            lblCurrentProject.Text = null;
            browserHistory.Clear();
            historyLocation = -1;

            if(currentProject == null)
            {
                lblCurrentProject.Text = "None - Select a help file builder project in the Solution Explorer";
                return;
            }

            // Get content from open file editors
            var args = new FileContentNeededEventArgs(FileContentNeededEvent, this);
            base.RaiseEvent(args);

            currentProject.EnsureProjectIsCurrent(false);
            lblCurrentProject.Text = currentProject.Filename;
            browserHistory.Clear();
            historyLocation = -1;
            tableOfContents = new TocEntryCollection();

            try
            {
                converter.MediaFiles.Clear();

                // Get the image files.  This information is used to resolve media link elements in the
                // topic files.
                imageFiles = new FileItemCollection(currentProject, BuildAction.Image);

                foreach(FileItem file in imageFiles)
                    if(!String.IsNullOrEmpty(file.ImageId))
                        converter.MediaFiles[file.ImageId] = new KeyValuePair<string, string>(file.FullPath,
                            file.AlternateText);
            }
            catch(Exception ex)
            {
                tableOfContents.Add(new TocEntry(currentProject)
                {
                    Title = "ERROR: Unable to load media info: " + ex.Message
                });
            }

            try
            {
                converter.Tokens.Clear();

                // Get the token files.  This information is used to resolve token elements in the
                // topic files.
                tokenFiles = new FileItemCollection(currentProject, BuildAction.Tokens);

                foreach(FileItem file in tokenFiles)
                {
                    // If open in an editor, use the edited values
                    if(!args.TokenFiles.TryGetValue(file.FullPath, out tokens))
                    {
                        tokens = new TokenCollection(file.FullPath);
                        tokens.Load();
                    }

                    // Store the tokens as XElements so that they can be parsed inline with the topic
                    foreach(var t in tokens)
                        converter.Tokens.Add(t.TokenName, XElement.Parse("<token>" + t.TokenValue +
                            "</token>"));
                }
            }
            catch(Exception ex)
            {
                tableOfContents.Add(new TocEntry(currentProject)
                {
                    Title = "ERROR: Unable to load token info: " + ex.Message
                });
            }

            try
            {
                // Get the content layout files.  Site maps are ignored.  We don't support rendering them.
                contentLayoutFiles = new FileItemCollection(currentProject, BuildAction.ContentLayout);
                tocFiles = new List<ITableOfContents>();

                // Add the conceptual content layout files
                foreach(FileItem file in contentLayoutFiles)
                {
                    // If open in an editor, use the edited values
                    if(!args.ContentLayoutFiles.TryGetValue(file.FullPath, out contentLayout))
                    {
                        contentLayout = new TopicCollection(file);
                        contentLayout.Load();
                    }

                    // For the purpose of adding links, make sure everything is visible
                    contentLayout.Find(t => !t.Visible, false).ToList().ForEach(t => t.Visible = true);

                    tocFiles.Add(contentLayout);
                }

                // Sort the files
                tocFiles.Sort((x, y) =>
                {
                    FileItem fx = x.ContentLayoutFile, fy = y.ContentLayoutFile;

                    if(fx.SortOrder < fy.SortOrder)
                        return -1;

                    if(fx.SortOrder > fy.SortOrder)
                        return 1;

                    return String.Compare(fx.Name, fy.Name, StringComparison.OrdinalIgnoreCase);
                });

                // Create the merged TOC
                foreach(ITableOfContents file in tocFiles)
                    file.GenerateTableOfContents(tableOfContents, currentProject);

                // Pass the topic IDs and titles on to the converter for use in hyperlinks
                foreach(var t in tableOfContents.All())
                    if(!String.IsNullOrEmpty(t.Id))
                        converter.TopicTitles[t.Id] = t.Title;
            }
            catch(Exception ex)
            {
                tableOfContents.Add(new TocEntry(currentProject)
                {
                    Title = "ERROR: Unable to load TOC info: " + ex.Message
                });
            }

            if(tableOfContents.Count != 0)
            {
                foreach(var t in tableOfContents.All())
                    t.IsSelected = false;

                tableOfContents[0].IsSelected = true;
            }

            tvContent.ItemsSource = tableOfContents;
        }

        /// <summary>
        /// This is used to adjust the page width to accomodate images wider than the available
        /// display area.
        /// </summary>
        /// <remarks>If not adjusted, the images get clipped.</remarks>
        private void AdjustPageWidth()
        {
            // Find the widest image, if any
            var maxImageWidth = fdViewer.Document.Find<BlockUIContainer>(b => b.Child is Image).Max(
                b => (double?)((Image)b.Child).Width);

            if(maxImageWidth != null)
            {
                // Add triple the padding or it tends to clip the image before triggering the scrollbar.
                // NOTE: Use an actual double or it ignores the addition for some reason (compiler bug?).
                double pageWidth = maxImageWidth.Value + ((fdViewer.Document.PagePadding.Left +
                    fdViewer.Document.PagePadding.Right) * 3.0);

                if(fdViewer.ActualWidth < pageWidth)
                    fdViewer.Document.PageWidth = pageWidth;
                else
                    fdViewer.Document.PageWidth = Double.NaN;
            }
        }

        /// <summary>
        /// This is used to record browser history
        /// </summary>
        /// <param name="link">The link to add to the history</param>
        private void RecordHistory(Uri link)
        {
            if(historyLocation > 0)
                browserHistory.RemoveRange(historyLocation + 1, browserHistory.Count - historyLocation - 1);

            // If we've only got a fragment, convert it to a full link
            if(String.IsNullOrEmpty(link.Host))
            {
                var t = this.CurrentTopic;

                if(t != null)
                    link = new Uri("link://" + t.Id + link.Fragment);
            }

            browserHistory.Add(link);
            historyLocation++;
        }

        /// <summary>
        /// This is used to locate a topic by ID and display it
        /// </summary>
        /// <param name="link">The link to the topic</param>
        /// <returns>True if found and displayed, false if not</returns>
        private bool NavigateToTopic(Uri link)
        {
            bool wasFound = false;

            // If there's a topic ID, try to find it
            if(!String.IsNullOrEmpty(link.Host))
            {
                var topics = tableOfContents.Find(
                    t => t.Id.Equals(link.Host, StringComparison.OrdinalIgnoreCase), true);

                if(topics.Any())
                {
                    var topic = topics.First();

                    // If already selected, force it to reload to reflect changes made to the topic
                    if(topic.IsSelected)
                        tvContent_SelectedItemChanged(this, new RoutedPropertyChangedEventArgs<object>(null, null));
                    else
                        topic.IsSelected = true;

                    wasFound = true;
                }
            }

            // If there's a fragment, try to find the named element and bring it into view
            if(!String.IsNullOrEmpty(link.Fragment) && link.Fragment.Length > 1)
            {
                var element = fdViewer.Document.FindByName(MamlToFlowDocumentConverter.ToElementName(
                    link.Fragment.Substring(1))).FirstOrDefault();

                if(element != null)
                {
                    element.BringIntoView();
                    wasFound = true;
                }
                else
                    wasFound = false;
            }

            return wasFound;
        }
        #endregion

        #region General event handlers
        //=====================================================================

        /// <summary>
        /// This loads the content when the control is first made visible
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ucTopicPreviewer_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(this.IsVisible && currentProject != null && tableOfContents == null)
                this.LoadTableOfContentsInfo();
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

        /// <summary>
        /// Load the topic and display it when selected
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvContent_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TocEntry t = tvContent.SelectedItem as TocEntry;

            if(t != null)
            {
                try
                {
                    // If the file is open in an editor, use its current content
                    var args = new TopicContentNeededEventArgs(TopicContentNeededEvent, this, t.SourceFile);

                    if(!String.IsNullOrEmpty(t.SourceFile))
                        base.RaiseEvent(args);

                    txtTitle.Text = t.Title;
                    fdViewer.Document = converter.ToFlowDocument(t.SourceFile, args.TopicContent);

                    this.AdjustPageWidth();

                    // Scroll to the top if not the first time shown
                    DependencyObject child = fdViewer;

                    if(VisualTreeHelper.GetChildrenCount(child) != 0)
                    {
                        while(!(child is ScrollViewer))
                            child = VisualTreeHelper.GetChild(child as Visual, 0);

                        ((ScrollViewer)child).ScrollToHome();
                    }
                }
                catch(Exception ex)
                {
                    // If we get here, something went really wrong
                    MessageBox.Show("Unable to convert topic.  Possible converter error.  Reason: " +
                        ex.Message, Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if(!isNavigating && !String.IsNullOrEmpty(t.Id))
                    this.RecordHistory(new Uri("link://" + t.Id));
            }
            else
            {
                txtTitle.Text = null;
                fdViewer.Document = null;
            }
        }

        /// <summary>
        /// Resize the page width as needed to accomodate images wider than the display area
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void fdViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(e.WidthChanged && fdViewer.Document != null)
                this.AdjustPageWidth();
        }

        /// <summary>
        /// Handle hyperlink clicks within the currently displayed topic
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void fdViewer_LinkClicked(object sender, RoutedEventArgs e)
        {
            Hyperlink link = e.OriginalSource as Hyperlink;
            Uri absoluteUri;
            string path;

            if(link == null)
                return;

            // Convert relative URIs to absolute URIs
            if(!link.NavigateUri.IsAbsoluteUri)
            {
                path = link.NavigateUri.OriginalString;

                if(path.Length > 1 && path[0] == '/')
                    path = path.Substring(1);

                if(!Uri.TryCreate(Path.Combine(Path.GetDirectoryName(currentProject.Filename), path),
                  UriKind.RelativeOrAbsolute, out absoluteUri))
                    absoluteUri = new Uri("link://INVALID_LINK_URI");

                link.NavigateUri = absoluteUri;
            }

            // If not a known ID, tell the user
            if(link.NavigateUri.Scheme.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Unknown link target: " + link.NavigateUri.Host, "Topic Previewer",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // If it's a local topic link, find it and show it
            if(link.NavigateUri.Scheme.Equals("link", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    isNavigating = true;

                    if(this.NavigateToTopic(link.NavigateUri))
                    {
                        this.RecordHistory(link.NavigateUri);
                        fdViewer.Focus();
                    }
                    else
                        MessageBox.Show("Unknown link target: " + link.NavigateUri.Host +
                            link.NavigateUri.Fragment, "Topic Previewer", MessageBoxButton.OK,
                            MessageBoxImage.Exclamation);
                }
                finally
                {
                    isNavigating = false;
                }

                return;
            }

            // It looks like an external link so try to launch it.  We don't handle link target so it
            // will always be launched in an external window.
            try
            {
                System.Diagnostics.Process.Start(link.NavigateUri.AbsoluteUri);
            }
            catch(Exception ex)
            {
                MessageBox.Show(String.Format("Unable to launch URL: {0}\r\n\r\nReason: {1}",
                    link.NavigateUri.Host, ex.Message), "Topic Previewer", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
        }
        #endregion

        #region Command event handlers
        //=====================================================================

        /// <summary>
        /// This is used to see if the Browse Back command can be executed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdBrowseBack_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (historyLocation > 0);
        }

        /// <summary>
        /// This handles the Browse Back command
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdBrowseBack_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(historyLocation > 0)
            {
                historyLocation--;

                try
                {
                    isNavigating = true;
                    this.NavigateToTopic(browserHistory[historyLocation]);
                }
                finally
                {
                    isNavigating = false;
                }
            }
        }

        /// <summary>
        /// This is used to see if the Browse Forward command can be executed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdBrowseForward_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (historyLocation < browserHistory.Count - 1);
        }

        /// <summary>
        /// This handles the Browse Forward command
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdBrowseForward_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(historyLocation < browserHistory.Count - 1)
            {
                historyLocation++;

                try
                {
                    isNavigating = true;
                    this.NavigateToTopic(browserHistory[historyLocation]);
                }
                finally
                {
                    isNavigating = false;
                }
            }
        }

        /// <summary>
        /// Determine whether the Collapse/Expand commands can execute
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdExpandCollapse_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (tableOfContents != null && tableOfContents.Count != 0 &&
                ((e.Command != EditorCommands.CollapseCurrent && e.Command != EditorCommands.ExpandCurrent) ||
                (this.CurrentTopic != null && this.CurrentTopic.Children.Count != 0)));
        }

        /// <summary>
        /// Collapse or expand all topics with children
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdExpandCollapseAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            bool expand = (e.Command == EditorCommands.ExpandAll);

            if(tableOfContents != null)
                foreach(var topic in tableOfContents.Find(t => t.Children.Count != 0, false))
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

            if(this.CurrentTopic != null && this.CurrentTopic.Children.Count != 0)
            {
                this.CurrentTopic.IsExpanded = expand;

                foreach(var topic in this.CurrentTopic.Children.Find(t => t.Children.Count != 0, false))
                    topic.IsExpanded = expand;
            }
        }

        /// <summary>
        /// View help for this tool window
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Utility.ShowHelpTopic("d3c7584d-73c0-4725-87f8-51e4ad956694");
        }

        /// <summary>
        /// Refresh the currently information
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdRefresh_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Refresh(true);
        }
        #endregion
    }
}
