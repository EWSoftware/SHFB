//==========================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : EntityReferencesControl.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/10/2012
// Note    : Copyright 2011-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the WPF user control used to look up code entity references, code snippets, tokens,
// images, and table of content entries and allows them to be dragged and dropped into a topic editor window.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.   This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// =========================================================================================================
// 1.9.3.3  12/04/2011  EFW  Created the code
//==========================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.XPath;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildEngine;
using SandcastleBuilder.Utils.ConceptualContent;
using SandcastleBuilder.Utils.Design;
using SandcastleBuilder.Utils.Frameworks;
using SandcastleBuilder.Utils.InheritedDocumentation;
using SandcastleBuilder.Utils.MSBuild;

namespace SandcastleBuilder.WPF.UserControls
{
    /// <summary>
    /// This user control is used to look up code entity references, code snippets, tokens, images, and
    /// table of content entries and allows them to be dragged and dropped into a topic editor window.
    /// </summary>
    public partial class EntityReferencesControl : UserControl
    {
        #region Private data members
        //=====================================================================

        private SandcastleProject currentProject;

        private List<EntityReference> tokens, images, tableOfContents, codeSnippets;
        private IEnumerator<EntityReference> matchEnumerator;
        private string[] codeEntities;

        private CancellationTokenSource indexTokenSource;
        private Task<IndexedCommentsCache> indexTask;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to enable or disable the animated GIF behavior
        /// </summary>
        /// <remarks>This is a hack to work around a problem when the control is hosted in a Windows
        /// Forms application via a <c>HostElement</c>.  If set in the XAML, it causes an exception
        /// related to the <c>CurrentFrameIndex</c> property already being registered.  By only enabling
        /// it at runtime, it works around the problem.</remarks>
        public bool AllowAnimatedGif
        {
            get
            {
                return Behaviors.SupportAnimatedGIFBehavior.GetSupportAnimatedGif(imgSpinner);
            }
            set
            {
                Behaviors.SupportAnimatedGIFBehavior.SetSupportAnimatedGif(imgSpinner, value);
            }
        }

        /// <summary>
        /// This is used to set or get the current project
        /// </summary>
        public SandcastleProject CurrentProject
        {
            get { return currentProject; }
            set
            {
                currentProject = value;

                tokens = images = tableOfContents = codeSnippets = null;
                codeEntities = null;

                if(this.IsLoaded)
                {
                    tvEntities.ItemsSource = null;
                    txtFindName.IsReadOnly = true;
                    tvEntities.IsEnabled = btnGo.IsEnabled = false;

                    this.DisposeOfTask();
                    this.cboEntityType_SelectionChanged(this, null);
                }
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public EntityReferencesControl()
        {
            InitializeComponent();

            spImageFormats.Visibility = spTocFormats.Visibility = spCodeEntityRefFormats.Visibility =
                spIndexingPanel.Visibility = Visibility.Collapsed;
            txtFindName.IsReadOnly = true;
            tvEntities.IsEnabled = btnGo.IsEnabled = false;
        }
        #endregion

        #region Routed events
        //=====================================================================

        /// <summary>
        /// This registers the <see cref="FileContentNeeded"/> event
        /// </summary>
        public static readonly RoutedEvent FileContentNeededEvent = EventManager.RegisterRoutedEvent(
            "FileContentNeededEvent", RoutingStrategy.Bubble, typeof(EventHandler<FileContentNeededEventArgs>),
            typeof(EntityReferencesControl));

        /// <summary>
        /// This event is used to get information for token, content layout, and site map files that are
        /// open in editors so that current information is displayed.
        /// </summary>
        public event EventHandler<FileContentNeededEventArgs> FileContentNeeded
        {
            add { AddHandler(FileContentNeededEvent, value); }
            remove { RemoveHandler(FileContentNeededEvent, value); }
        }
        #endregion

        #region Load token info
        //=====================================================================

        /// <summary>
        /// This loads the tree view with token file entries from the project
        /// </summary>
        private List<EntityReference> LoadTokenInfo()
        {
            FileItemCollection tokenFiles;
            EntityReference tokenFileEntity = null;
            TokenCollection tokenColl;

            if(tokens != null)
                return tokens;

            tokens = new List<EntityReference>();

            currentProject.EnsureProjectIsCurrent(false);
            tokenFiles = new FileItemCollection(currentProject, BuildAction.Tokens);

            // Get content from open file editors
            var args = new FileContentNeededEventArgs(FileContentNeededEvent, this);
            base.RaiseEvent(args);

            foreach(FileItem tokenFile in tokenFiles)
                try
                {
                    if(File.Exists(tokenFile.FullPath))
                    {
                        tokenFileEntity = new EntityReference
                        {
                            EntityType = EntityType.File,
                            Id = tokenFile.FullPath,
                            Label = Path.GetFileName(tokenFile.FullPath),
                            ToolTip = tokenFile.FullPath
                        };

                        tokens.Add(tokenFileEntity);

                        // If open in an editor, use the edited values
                        if(!args.TokenFiles.TryGetValue(tokenFile.FullPath, out tokenColl))
                        {
                            tokenColl = new TokenCollection(tokenFile.FullPath);
                            tokenColl.Load();
                        }

                        foreach(Token t in tokenColl)
                            tokenFileEntity.SubEntities.Add(new EntityReference
                            {
                                EntityType = EntityType.Token,
                                Id = t.TokenName,
                                Label = t.TokenName,
                                ToolTip = t.TokenName,
                                Tag = t
                            });
                    }

                    tokenFileEntity = null;
                }
                catch(Exception ex)
                {
                    if(tokenFileEntity == null)
                        tokens.Add(new EntityReference
                        {
                            EntityType = EntityType.File,
                            Label = "Unable to load file '" + tokenFile.FullPath +
                                "'.  Reason: " + ex.Message,
                            ToolTip = "Error"
                        });
                    else
                        tokens.Add(new EntityReference
                        {
                            EntityType = EntityType.File,
                            Label = "Unable to load file: " + ex.Message,
                            ToolTip = "Error"
                        });
                }

            if(tokens.Count != 0)
            {
                tokens[0].IsSelected = true;

                if(tokens[0].SubEntities.Count != 0)
                    tokens[0].IsExpanded = true;
            }

            return tokens;
        }
        #endregion

        #region Load image information
        //=====================================================================

        /// <summary>
        /// This loads the tree view with image file entries from the project
        /// </summary>
        private List<EntityReference> LoadImageInfo()
        {
            ImageReferenceCollection imagesRefs;

            if(images != null)
                return images;

            images = new List<EntityReference>();

            currentProject.EnsureProjectIsCurrent(false);
            imagesRefs = new ImageReferenceCollection(currentProject);

            foreach(ImageReference ir in imagesRefs)
                if(!String.IsNullOrEmpty(ir.Id))
                    images.Add(new EntityReference
                    {
                        EntityType = EntityType.Image,
                        Id = ir.Id,
                        Label = ir.DisplayTitle,
                        ToolTip = String.Format(CultureInfo.CurrentCulture, "ID: {0}\nFile: {1}",
                            ir.Id, ir.FullPath),
                        Tag = ir
                    });

            if(images.Count != 0)
                images[0].IsSelected = true;

            return images;
        }
        #endregion

        #region Load table of contents info
        //=====================================================================

        /// <summary>
        /// This loads the tree view with table of contents file entries from the project
        /// </summary>
        private List<EntityReference> LoadTableOfContentsInfo()
        {
            FileItemCollection contentLayoutFiles, siteMapFiles;
            List<ITableOfContents> tocFiles;
            TopicCollection contentLayout;
            TocEntryCollection siteMap, mergedToc;
            EntityReference er;
            bool hasSelectedItem = false;

            if(tableOfContents != null)
                return tableOfContents;

            tableOfContents = new List<EntityReference>();

            // Get content from open file editors
            var args = new FileContentNeededEventArgs(FileContentNeededEvent, this);
            base.RaiseEvent(args);

            try
            {
                // Get the content layout and site map files
                currentProject.EnsureProjectIsCurrent(false);

                contentLayoutFiles = new FileItemCollection(currentProject, BuildAction.ContentLayout);
                siteMapFiles = new FileItemCollection(currentProject, BuildAction.SiteMap);
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

                    tocFiles.Add(contentLayout);
                }

                // Load all site maps and add them to the list
                foreach(FileItem fileItem in siteMapFiles)
                {
                    // If open in an editor, use the edited values
                    if(!args.SiteMapFiles.TryGetValue(fileItem.FullPath, out siteMap))
                    {
                        siteMap = new TocEntryCollection(fileItem);
                        siteMap.Load();
                    }

                    tocFiles.Add(siteMap);
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

                // Create the merged TOC.  For the purpose of adding links, we'll include everything
                // even topics marked as invisible.
                mergedToc = new TocEntryCollection();

                foreach(ITableOfContents file in tocFiles)
                    file.GenerateTableOfContents(mergedToc, currentProject, true);

                // Convert the TOC info to entity references
                foreach(var t in mergedToc)
                {
                    er = new EntityReference
                    {
                        EntityType = EntityType.TocEntry,
                        Id = t.Id,
                        Label = (t.Title ?? t.Id ?? "(No title)"),
                        ToolTip = String.Format(CultureInfo.CurrentCulture, "ID: {0}\nFile: {1}",
                            (t.Id ?? t.Title ?? "(No ID)"), t.SourceFile),
                        Tag = t,
                        IsExpanded = t.IsExpanded,
                        IsSelected = (t.IsSelected && !hasSelectedItem)
                    };

                    // Only use the first selected item
                    if(er.IsSelected)
                        hasSelectedItem = true;

                    tableOfContents.Add(er);

                    if(t.Children.Count != 0)
                        hasSelectedItem = this.AddChildTocEntries(t, er, hasSelectedItem);
                }
            }
            catch(Exception ex)
            {
                tableOfContents.Add(new EntityReference
                {
                    EntityType = EntityType.File,
                    Label = "Unable to load TOC info: " + ex.Message,
                    ToolTip = "Error"
                });
            }

            if(!hasSelectedItem && tableOfContents.Count != 0)
                tableOfContents[0].IsSelected = true;

            return tableOfContents;
        }

        /// <summary>
        /// This is used to recursively add child TOC entries to the entity reference collection
        /// </summary>
        /// <param name="t">The TOC entry</param>
        /// <param name="er">The parent entity reference</param>
        /// <param name="hasSelectedItem">The selected item state.  Only first selected item found is
        /// marked as the selected item.</param>
        private bool AddChildTocEntries(TocEntry t, EntityReference er, bool hasSelectedItem)
        {
            EntityReference subEnt;

            foreach(var child in t.Children)
            {
                subEnt = new EntityReference
                {
                    EntityType = EntityType.TocEntry,
                    Id = child.Id,
                    Label = (child.Title ?? child.Id ?? "(No title)"),
                    ToolTip = String.Format(CultureInfo.CurrentCulture, "ID: {0}\nFile: {1}",
                        (child.Id ?? child.Title ?? "(No ID)"), child.SourceFile),
                    Tag = child,
                    IsExpanded = child.IsExpanded,
                    IsSelected = (child.IsSelected && !hasSelectedItem)
                };

                if(subEnt.IsSelected)
                    hasSelectedItem = true;

                er.SubEntities.Add(subEnt);

                if(child.Children.Count != 0)
                    hasSelectedItem = this.AddChildTocEntries(child, subEnt, hasSelectedItem);
            }

            return hasSelectedItem;
        }
        #endregion

        #region Load code snippet info
        //=====================================================================

        /// <summary>
        /// This loads the tree view with code snippet file entries from the project
        /// </summary>
        private List<EntityReference> LoadCodeSnippetInfo()
        {
            FileItemCollection codeSnippetFiles;
            EntityReference snippetFileEntity = null;
            XPathDocument snippets;
            XPathNavigator navSnippets;
            CodeReference cr;

            if(codeSnippets != null)
                return codeSnippets;

            codeSnippets = new List<EntityReference>();

            currentProject.EnsureProjectIsCurrent(false);
            codeSnippetFiles = new FileItemCollection(currentProject, BuildAction.CodeSnippets);

            foreach(FileItem snippetFile in codeSnippetFiles)
                try
                {
                    if(File.Exists(snippetFile.FullPath))
                    {
                        snippetFileEntity = new EntityReference
                        {
                            EntityType = EntityType.File,
                            Id = snippetFile.FullPath,
                            Label = Path.GetFileName(snippetFile.FullPath),
                            ToolTip = snippetFile.FullPath
                        };

                        codeSnippets.Add(snippetFileEntity);

                        snippets = new XPathDocument(snippetFile.FullPath);
                        navSnippets = snippets.CreateNavigator();

                        foreach(XPathNavigator nav in navSnippets.Select("examples/item/@id"))
                        {
                            cr = new CodeReference(nav.Value);

                            snippetFileEntity.SubEntities.Add(new EntityReference
                            {
                                EntityType = EntityType.CodeSnippet,
                                Id = cr.Id,
                                Label = cr.Id,
                                ToolTip = cr.Id,
                                Tag = cr
                            });
                        }
                    }

                    snippetFileEntity = null;
                }
                catch(Exception ex)
                {
                    if(snippetFileEntity == null)
                        codeSnippets.Add(new EntityReference
                        {
                            EntityType = EntityType.File,
                            Label = "Unable to load file '" + snippetFile.FullPath +
                                "'.  Reason: " + ex.Message,
                            ToolTip = "Error"
                        });
                    else
                        codeSnippets.Add(new EntityReference
                        {
                            EntityType = EntityType.File,
                            Label = "Unable to load file: " + ex.Message,
                            ToolTip = "Error"
                        });
                }

            if(codeSnippets.Count != 0)
            {
                codeSnippets[0].IsSelected = true;

                if(codeSnippets[0].SubEntities.Count != 0)
                    codeSnippets[0].IsExpanded = true;
            }

            return codeSnippets;
        }
        #endregion

        #region Load code entity info
        //=====================================================================

        /// <summary>
        /// This loads the code entities from the project and base framework
        /// </summary>
        private void LoadCodeEntities()
        {
            if(codeEntities == null)
            {
                spIndexingPanel.Visibility = Visibility.Visible;
                txtFindName.Text = null;

                // Ignore the request if the task is already running
                if(indexTokenSource == null)
                {
                    var ui = TaskScheduler.FromCurrentSynchronizationContext();

                    indexTokenSource = new CancellationTokenSource();
                    indexTask = Task.Factory.StartNew<IndexedCommentsCache>(this.IndexComments,
                        indexTokenSource.Token);

                    indexTask.ContinueWith(t => this.IndexCompleted(t.Result),
                        CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, ui);

                    indexTask.ContinueWith(t => this.IndexFailed(t.Exception),
                        CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, ui);
                }
            }
            else
            {
                txtFindName.IsReadOnly = false;
                tvEntities.IsEnabled = btnGo.IsEnabled = true;
            }
        }

        /// <summary>
        /// This is used to stop the task if it is still running
        /// </summary>
        private void DisposeOfTask()
        {
            if(indexTokenSource != null)
            {
                try
                {
                    // Cancel the task if it is running and wait for it to see the request
                    if(!indexTask.IsCompleted)
                    {
                        indexTokenSource.Cancel();
                        indexTask.Wait();
                    }
                }
                catch(AggregateException )
                {
                    // Ignore the "Task cancelled" exception.  If we don't catch it, the
                    // application never shuts down.
                }
                finally
                {
                    indexTokenSource.Dispose();
                    indexTask.Dispose();

                    indexTokenSource = null;
                    indexTask = null;
                }
            }
        }

        /// <summary>
        /// This is the method that indexes the comments files
        /// </summary>
        /// <remarks>Rather than a partial build, we'll just index the comments files.</remarks>
        private IndexedCommentsCache IndexComments()
        {
            HashSet<string> projectDictionary = new HashSet<string>();
            IndexedCommentsCache cache = new IndexedCommentsCache(100);
            MSBuildProject projRef;
            string lastSolution = null;

            currentProject.EnsureProjectIsCurrent(false);

            // Index the framework comments based on the framework version in the project
            FrameworkSettings frameworkSettings = FrameworkVersionTypeConverter.AllFrameworks.GetFrameworkWithRedirect(
                currentProject.FrameworkVersion);

            if(frameworkSettings == null)
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    "Unable to locate information for a framework version or its redirect: {0}",
                    currentProject.FrameworkVersion));

            foreach(var location in frameworkSettings.CommentsFileLocations(currentProject.Language))
            {
                indexTokenSource.Token.ThrowIfCancellationRequested();

                cache.IndexCommentsFiles(Path.GetDirectoryName(location), Path.GetFileName(location),
                    false, null);
            }

            // Index the comments file documentation sources
            foreach(string file in currentProject.DocumentationSources.CommentsFiles)
            {
                indexTokenSource.Token.ThrowIfCancellationRequested();

                cache.IndexCommentsFiles(Path.GetDirectoryName(file), Path.GetFileName(file), false, null);
            }

            // Also, index the comments files in project documentation sources
            foreach(DocumentationSource ds in currentProject.DocumentationSources)
                foreach(var sourceProject in DocumentationSource.Projects(ds.SourceFile, ds.IncludeSubFolders,
                    !String.IsNullOrEmpty(ds.Configuration) ? ds.Configuration : currentProject.Configuration,
                    !String.IsNullOrEmpty(ds.Platform) ? ds.Platform : currentProject.Platform))
                {
                    indexTokenSource.Token.ThrowIfCancellationRequested();

                    // NOTE: This code should be similar to the code in BuildProcess.ValidateDocumentationSources!

                    // Solutions are followed by the projects that they contain
                    if(sourceProject.ProjectFileName.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
                    {
                        lastSolution = sourceProject.ProjectFileName;
                        continue;
                    }

                    // Ignore projects that we've already seen
                    if(projectDictionary.Add(sourceProject.ProjectFileName))
                    {
                        projRef = new MSBuildProject(sourceProject.ProjectFileName);

                        // Use the project file configuration and platform properties if they are set.  If not,
                        // use the documentation source values.  If they are not set, use the SHFB project settings.
                        projRef.SetConfiguration(
                            !String.IsNullOrEmpty(sourceProject.Configuration) ? sourceProject.Configuration :
                                !String.IsNullOrEmpty(ds.Configuration) ? ds.Configuration : currentProject.Configuration,
                            !String.IsNullOrEmpty(sourceProject.Platform) ? sourceProject.Platform :
                                !String.IsNullOrEmpty(ds.Platform) ? ds.Platform : currentProject.Platform,
                            currentProject.MSBuildOutDir);

                        // Add Visual Studio solution macros if necessary
                        if(lastSolution != null)
                            projRef.SetSolutionMacros(lastSolution);

                        if(!String.IsNullOrEmpty(projRef.XmlCommentsFile))
                            cache.IndexCommentsFiles(Path.GetDirectoryName(projRef.XmlCommentsFile),
                                Path.GetFileName(projRef.XmlCommentsFile), false, null);
                    }
                }

            return cache;
        }

        /// <summary>
        /// This is called if indexing completes successfully
        /// </summary>
        /// <param name="cache">The index cache</param>
        private void IndexCompleted(IndexedCommentsCache cache)
        {
            this.DisposeOfTask();

            spIndexingPanel.Visibility = Visibility.Collapsed;

            codeEntities = cache.GetKeys();

            if(cboEntityType.SelectedIndex == (int)EntityType.CodeEntity)
            {
                txtFindName.IsReadOnly = false;
                tvEntities.IsEnabled = btnGo.IsEnabled = true;
            }
        }

        /// <summary>
        /// This is called if indexing fails
        /// </summary>
        /// <param name="ex">The exception that caused the failure</param>
        private void IndexFailed(AggregateException ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            this.DisposeOfTask();

            spIndexingPanel.Visibility = Visibility.Collapsed;

            if(cboEntityType.SelectedIndex == (int)EntityType.CodeEntity)
                txtFindName.Text = "Indexing failed.  Reason: " + ex.InnerException.Message;
        }
        #endregion

        #region Other helper methods
        //=====================================================================

        /// <summary>
        /// Search the entities to find those with the specified text in their ID or label
        /// </summary>
        /// <param name="matchText">The text to find</param>
        /// <returns>An enumerable list of all matching entities</returns>
        private IEnumerable<EntityReference> Find(string matchText)
        {
            var entities = (List<EntityReference>)tvEntities.ItemsSource;

            if(entities != null)
                foreach(var e in entities)
                    foreach(var match in e.Find(matchText))
                        yield return match;
        }

        /// <summary>
        /// Get the text to copy or use for drag and drop operations
        /// </summary>
        /// <returns>The text to use or null if there is nothing to copy</returns>
        private string GetTextToCopy()
        {
            EntityReference r = tvEntities.SelectedItem as EntityReference;
            string textToCopy = null;

            if(r != null)
                switch(r.EntityType)
                {
                    case EntityType.File:
                        // Not useable
                        break;

                    case EntityType.Token:
                        Token t = (Token)r.Tag;
                        textToCopy = t.ToToken();
                        break;

                    case EntityType.Image:
                        ImageReference ir = (ImageReference)r.Tag;

                        if(rbMediaLink.IsChecked.Value)
                            textToCopy = ir.ToMediaLink();
                        else
                            if(rbMediaLinkInline.IsChecked.Value)
                                textToCopy = ir.ToMediaLinkInline();
                            else
                                if(rbExternalLink.IsChecked.Value)
                                    textToCopy = ir.ToExternalLink();
                                else
                                    textToCopy = ir.ToImageLink();
                        break;

                    case EntityType.CodeSnippet:
                        CodeReference cr = (CodeReference)r.Tag;
                        textToCopy = cr.ToCodeReference();
                        break;

                    case EntityType.TocEntry:
                        TocEntry toc = (TocEntry)r.Tag;

                        // MAML topic?
                        if(!String.IsNullOrEmpty(toc.Id))
                        {
                            if(rbMamlLink.IsChecked.Value)
                                textToCopy = String.Format(CultureInfo.InvariantCulture,
                                    "<link xlink:href=\"{0}\" />", (toc.Id ?? "[Unknown ID]"));
                            else
                                textToCopy = String.Format(CultureInfo.InvariantCulture,
                                    "<a href=\"html/{0}.htm\">{1}</a>", toc.Id, toc.Title);
                        }
                        else
                            textToCopy = toc.ToAnchor(toc.Title);
                        break;

                    default:    // Code entity reference
                        CodeEntityReference ce = (CodeEntityReference)r.Tag;

                        if(rbCodeEntityRef.IsChecked.Value)
                            textToCopy = ce.ToCodeEntityReference();
                        else
                            textToCopy = ce.ToSee();
                        break;
                }

            return textToCopy;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// This loads the content when the control is first made visible
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ucEntityReferences_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool loadInfo = false;

            if(currentProject != null)
                if(this.IsVisible)
                {
                    switch((EntityType)cboEntityType.SelectedIndex)
                    {
                        case EntityType.Token:
                            loadInfo = (tokens == null);
                            break;

                        case EntityType.Image:
                            loadInfo = (images == null);
                            break;

                        case EntityType.TocEntry:
                            loadInfo = (tableOfContents == null);
                            break;

                        case EntityType.CodeSnippet:
                            loadInfo = (codeSnippets == null);
                            break;

                        default:
                            loadInfo = (codeEntities == null);
                            break;
                    }

                    if(loadInfo)
                        this.cboEntityType_SelectionChanged(sender, null);
                }
                else
                    this.DisposeOfTask();
        }

        /// <summary>
        /// View help for this tool window
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Utility.ShowHelpTopic("e49eea91-a9ef-4aa5-ad8f-16ebd61b798a");
        }

        /// <summary>
        /// Refresh the currently displayed entity information
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdRefresh_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Don't refresh while the index task is running as it may cause an exception
            // when it goes to ensure the current project is current.
            if(indexTask == null)
            {
                switch((EntityType)cboEntityType.SelectedIndex)
                {
                    case EntityType.Token:
                        tokens = null;
                        break;

                    case EntityType.Image:
                        images = null;
                        break;

                    case EntityType.TocEntry:
                        tableOfContents = null;
                        break;

                    case EntityType.CodeSnippet:
                        codeSnippets = null;
                        break;

                    default:
                        codeEntities = null;
                        break;
                }

                this.cboEntityType_SelectionChanged(sender, null);
            }
        }

        /// <summary>
        /// Determine if the copy command can be executed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdCopy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (tvEntities != null && tvEntities.SelectedItem != null);
        }

        /// <summary>
        /// Copy the selected item to the clipboard as a link
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdCopy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string textToCopy;

            if(tvEntities.SelectedItem != null)
            {
                textToCopy = this.GetTextToCopy();

                if(textToCopy != null)
                    Clipboard.SetText(textToCopy);
            }
        }

        /// <summary>
        /// Change the type of entities listed in the window
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cboEntityType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<EntityReference> entities = null;

            if(this.IsLoaded)
            {
                spImageFormats.Visibility = spTocFormats.Visibility = spCodeEntityRefFormats.Visibility =
                    spIndexingPanel.Visibility = Visibility.Collapsed;

                txtFindName.IsReadOnly = true;
                tvEntities.IsEnabled = btnGo.IsEnabled = false;
                tvEntities.ItemsSource = null;
            }

            if(currentProject != null)
            {
                switch((EntityType)cboEntityType.SelectedIndex)
                {
                    case EntityType.Token:
                        entities = this.LoadTokenInfo();
                        break;

                    case EntityType.Image:
                        spImageFormats.Visibility = Visibility.Visible;
                        entities = this.LoadImageInfo();
                        break;

                    case EntityType.TocEntry:
                        spTocFormats.Visibility = Visibility.Visible;
                        entities = this.LoadTableOfContentsInfo();
                        break;

                    case EntityType.CodeSnippet:
                        entities = this.LoadCodeSnippetInfo();
                        break;

                    default:    // Code entities
                        spCodeEntityRefFormats.Visibility = Visibility.Visible;
                        this.LoadCodeEntities();
                        break;
                }

                if(entities != null && entities.Count != 0)
                {
                    tvEntities.ItemsSource = entities;
                    txtFindName.IsReadOnly = false;
                    tvEntities.IsEnabled = btnGo.IsEnabled = true;
                }
            }

            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Find entities matching the entered text
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            List<EntityReference> entities;

            if(txtFindName.Text.Trim().Length == 0)
            {
                if(matchEnumerator != null)
                {
                    matchEnumerator.Dispose();
                    matchEnumerator = null;
                }

                return;
            }

            txtFindName.Text = txtFindName.Text.Trim();

            if((EntityType)cboEntityType.SelectedIndex != EntityType.CodeEntity)
            {
                // If this is the first time, get all matches
                if(matchEnumerator == null)
                    matchEnumerator = this.Find(txtFindName.Text).GetEnumerator();

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

                    MessageBox.Show("No more matches found", "Entity References", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                return;
            }

            // Search for code entity references
            List<string> matches = new List<string>();
            entities = new List<EntityReference>();

            tvEntities.ItemsSource = null;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                Regex reSearch = new Regex(txtFindName.Text, RegexOptions.IgnoreCase);

                foreach(string key in codeEntities)
                    if(reSearch.IsMatch(key))
                    {
                        matches.Add(key);

                        // Limit it to 1000 matches
                        if(matches.Count == 1000)
                            break;
                    }

                if(matches.Count != 0)
                {
                    matches.Sort((x, y) =>
                    {
                        return String.Compare(x, y, StringComparison.CurrentCulture);
                    });

                    foreach(string member in matches)
                        entities.Add(new EntityReference
                        {
                            Id = member,
                            EntityType = EntityType.CodeEntity,
                            Label = member,
                            ToolTip = member,
                            Tag = new CodeEntityReference(member)
                        });

                    tvEntities.IsEnabled = true;
                }
                else
                {
                    entities.Add(new EntityReference
                    {
                        EntityType = EntityType.CodeEntity,
                        Label = "No members found",
                        ToolTip = "Error"
                    });

                    tvEntities.IsEnabled = false;
                }

                tvEntities.ItemsSource = entities;
            }
            catch(ArgumentException ex)
            {
                MessageBox.Show("The search regular expression is not valid: " + ex.Message,
                    "Entity References", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            finally
            {
                if(entities.Count != 0)
                    entities[0].IsSelected = true;

                Mouse.OverrideCursor = null;

                if(entities.Count == 1000)
                    MessageBox.Show("Too many matches found.  Only the first 1000 are shown.",
                        "Entity References", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Clear the match enumerator when the text changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void txtFindName_TextChanged(object sender, TextChangedEventArgs e)
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
        private void txtFindName_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter && btnGo.IsEnabled)
            {
                e.Handled = true;
                btnGo_Click(sender, null);
            }
        }

        /// <summary>
        /// Allow drag and drop of the items.  The items are converted to their text form based on the
        /// "Insert As" option if applicable.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvEntities_MouseMove(object sender, MouseEventArgs e)
        {
            string textToCopy;

            // Make sure we are actually within an item when the left mouse button is pressed
            if(e.LeftButton == MouseButtonState.Pressed &&
              (e.OriginalSource as FrameworkElement).ParentElementOfType<TreeViewItem>() != null)
            {
                textToCopy = this.GetTextToCopy();

                if(textToCopy != null)
                    DragDrop.DoDragDrop(tvEntities, textToCopy, DragDropEffects.Copy);
            }
        }
        #endregion
    }
}
