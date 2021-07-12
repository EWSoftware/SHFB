﻿//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : TopicPreviewerControl.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/22/2021
// Note    : Copyright 2012-2021, Eric Woodruff, All rights reserved
//
// This file contains the class used to preview MAML topic files.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/02/2012  EFW  Created the code
// 01/12/2012  EFW  Added support for colorized code blocks
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;

using ColorizerLibrary;

using Sandcastle.Core;

using Sandcastle.Platform.Windows;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;
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
        private readonly MamlToFlowDocumentConverter converter;
        private readonly List<Uri> browserHistory;
        private int historyLocation;
        private bool isNavigating;

        private static readonly Regex reRemoveLineNumbers = new Regex(@"^\s*\d+\| ", RegexOptions.Multiline);

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to set or get the current project
        /// </summary>
        public SandcastleProject CurrentProject
        {
            get => currentProject;
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
        public TocEntry CurrentTopic => tvContent.SelectedItem as TocEntry;

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
            if(MamlToFlowDocumentConverter.CodeColorizer == null)
                try
                {
                    string colorizerPath = Path.Combine(ComponentUtilities.CoreComponentsFolder, "Colorizer");

                    MamlToFlowDocumentConverter.CodeColorizer = new CodeColorizer(Path.Combine(colorizerPath,
                      "highlight.xml"), Path.Combine(colorizerPath, "highlight_flowDoc.xsl"))
                    {
                        OutputFormat = OutputFormat.FlowDocument
                    };

                    MamlToFlowDocumentConverter.ColorizerFlowDocumentTemplate = Path.Combine(colorizerPath,
                        "DocumentTemplate.xaml");
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Unable to create code block colorizer.  Reason: " + ex.Message,
                        Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

            converter = new MamlToFlowDocumentConverter();

            browserHistory = new List<Uri>();
            historyLocation = -1;

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
        /// This loads the tree view with table of contents file entries from the project
        /// </summary>
        /// <remarks>Token information is also loaded here and passed on to the converter.</remarks>
        private void LoadTableOfContentsInfo()
        {
            List<ITableOfContents> tocFiles;

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

            // Make sure the base path is set for imported code blocks
            this.SetImportedCodeBasePath();

            // Get content from open file editors
            var args = new FileContentNeededEventArgs(FileContentNeededEvent, this);
            this.RaiseEvent(args);

            lblCurrentProject.Text = currentProject.Filename;
            browserHistory.Clear();
            historyLocation = -1;
            tableOfContents = new TocEntryCollection();

            try
            {
                converter.MediaFiles.Clear();

                // Get the image files.  This information is used to resolve media link elements in the
                // topic files.
                foreach(var file in currentProject.ImagesReferences)
                    converter.MediaFiles[file.Id] = new KeyValuePair<string, string>(file.FullPath, file.AlternateText);
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

                // Get the token files.  This information is used to resolve token elements in the topic files.
                foreach(var file in currentProject.ContentFiles(BuildAction.Tokens).OrderBy(f => f.LinkPath))
                {
                    // If open in an editor, use the edited values
                    if(!args.TokenFiles.TryGetValue(file.FullPath, out TokenCollection tokens))
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
                converter.TopicTitles.Clear();

                // Load the content layout files.  Site maps are ignored as we don't support rendering them.
                tocFiles = new List<ITableOfContents>();

                foreach(var contentFile in currentProject.ContentFiles(BuildAction.ContentLayout))
                {
                    // If open in an editor, use the edited values
                    if(!args.ContentLayoutFiles.TryGetValue(contentFile.FullPath, out TopicCollection contentLayout))
                    {
                        contentLayout = new TopicCollection(contentFile);
                        contentLayout.Load();
                    }

                    tocFiles.Add(contentLayout);
                }

                tocFiles.Sort((x, y) =>
                {
                    ContentFile fx = x.ContentLayoutFile, fy = y.ContentLayoutFile;

                    if(fx.SortOrder < fy.SortOrder)
                        return -1;

                    if(fx.SortOrder > fy.SortOrder)
                        return 1;

                    return String.Compare(fx.Filename, fy.Filename, StringComparison.OrdinalIgnoreCase);
                });

                // Create the merged TOC.  For the purpose of adding links, we'll include everything even topics
                // marked as invisible.
                foreach(ITableOfContents file in tocFiles)
                    file.GenerateTableOfContents(tableOfContents, true);

                // Pass the topic IDs and titles on to the converter for use in hyperlinks
                foreach(var t in tableOfContents.All())
                    if(!String.IsNullOrEmpty(t.Id))
                        converter.TopicTitles[t.Id] = t.LinkText;
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
        /// This is used to set the base path for imported code regions
        /// </summary>
        private void SetImportedCodeBasePath()
        {
            string cfgPath, basePath = Path.GetDirectoryName(Path.GetFullPath(currentProject.Filename));

            try
            {
                // If there is a code block component configuration, get the base path from it
                if(currentProject.ComponentConfigurations.TryGetValue("Code Block Component",
                  out BuildComponentConfiguration compConfig) && compConfig.Enabled)
                {
                    var cfg = XElement.Parse(compConfig.Configuration);
                    var basePathCfg = cfg.Descendants("basePath").FirstOrDefault();

                    if(basePathCfg != null)
                    {
                        cfgPath = basePathCfg.Attribute("value").Value.Replace("{@HtmlEncProjectFolder}",
                            FolderPath.TerminatePath(basePath));

                        if(Directory.Exists(cfgPath))
                            basePath = cfgPath;
                    }
                }
            }
            catch
            {
                // Ignore exceptions, if we can't figure it out, we'll use the project's folder
            }

            MamlToFlowDocumentConverter.ImportedCodeBasePath = basePath;
        }

        /// <summary>
        /// This is used to adjust the page width to accommodate images wider than the available
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
                    {
                        topic.IsSelected = true;

                        // We need to force the SelectedItemChanged event to run so that the document instance is
                        // updated before we get to the fragment check below.  Haven't found a better way to do
                        // this but it works.
                        if(Application.Current != null && Application.Current.Dispatcher != null)
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                new ThreadStart(delegate { }));
                    }

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
                    // Scroll the element into view.  BringIntoView() doesn't work on the flow document element
                    // so we need to find the scroll viewer that contains it and scroll that instead.
                    DependencyObject child = fdViewer;

                    if(VisualTreeHelper.GetChildrenCount(child) != 0)
                    {
                        while(!(child is ScrollViewer))
                            child = VisualTreeHelper.GetChild(child as Visual, 0);

                        ((ScrollViewer)child).ScrollToVerticalOffset(element.ContentStart.GetCharacterRect(
                            LogicalDirection.Forward).Top);
                    }

                    wasFound = true;
                }
                else
                    wasFound = false;
            }

            return wasFound;
        }

        /// <summary>
        /// Copy code from a code block element
        /// </summary>
        /// <param name="link">The hyperlink associated with the code block.  Note that this makes an assumption
        /// about the structure of the document</param>
        private static void CopyCode(Hyperlink link)
        {
            try
            {
                var fe = link.Parent as FrameworkElement;

                fe = fe.Parent as FrameworkElement;

                if(fe.Parent is BlockUIContainer b)
                {
                    if(b.NextBlock is Section sec)
                    {
                        if(sec.Blocks.FirstBlock is Paragraph para)
                        {
                            TextRange r = new TextRange(para.ContentStart, para.ContentEnd);

                            // Remove line numbers from the front of each line if present and copy the text
                            // to the clipboard.
                            Clipboard.SetText(reRemoveLineNumbers.Replace(r.Text, String.Empty));
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to copy code to clipboard.  Reason: " + ex.Message, Constants.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            if(sender is TreeViewItem item)
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
            // Only execute this if it's the selected node.  An odd side-effect of how we have to hook up
            // the event handler is that it fires for the selected item and all of its parents up to the
            // root of the tree even if the event is marked as handled.
            if(sender is TreeViewItem item && item.IsSelected)
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
                    Mouse.OverrideCursor = Cursors.Wait;

                    // If the file is open in an editor, use its current content
                    var args = new TopicContentNeededEventArgs(TopicContentNeededEvent, this, t.SourceFile);

                    if(!String.IsNullOrEmpty(t.SourceFile))
                        this.RaiseEvent(args);

                    txtTitle.Text = t.PreviewerTitle;
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
                finally
                {
                    Mouse.OverrideCursor = null;
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
        /// Resize the page width as needed to accommodate images wider than the display area
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
            string path;

            if(!(e.OriginalSource is Hyperlink link))
                return;

            // Convert relative URIs to absolute URIs
            if(!link.NavigateUri.IsAbsoluteUri)
            {
                path = link.NavigateUri.OriginalString;

                if(path.Length > 1 && path[0] == '/')
                    path = path.Substring(1);

                if(!Uri.TryCreate(Path.Combine(Path.GetDirectoryName(currentProject.Filename), path),
                  UriKind.RelativeOrAbsolute, out Uri absoluteUri))
                {
                    absoluteUri = new Uri("link://INVALID_LINK_URI");
                }

                link.NavigateUri = absoluteUri;
            }

            // If not a known ID, tell the user
            if(link.NavigateUri.Scheme.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Unknown link target: " + link.NavigateUri.Host, Constants.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // Copy code?
            if(link.NavigateUri.Scheme.Equals("copy", StringComparison.OrdinalIgnoreCase))
            {
                CopyCode(link);
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
                        MessageBox.Show("Unknown link target: " + link.NavigateUri.Host + link.NavigateUri.Fragment,
                            Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);

                    MessageBox.Show("Unknown link target: " + link.NavigateUri.Host + link.NavigateUri.Fragment,
                        Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
                MessageBox.Show(String.Format(CultureInfo.InvariantCulture, "Unable to launch URL: {0}\r\n\r\n" +
                    "Reason: {1}", link.NavigateUri.Host, ex.Message), Constants.AppName, MessageBoxButton.OK,
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
            UiUtility.ShowHelpTopic("d3c7584d-73c0-4725-87f8-51e4ad956694");
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
