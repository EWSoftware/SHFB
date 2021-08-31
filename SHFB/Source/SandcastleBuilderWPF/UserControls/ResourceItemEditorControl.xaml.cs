//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : ResourceItemEditorControl.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/20/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This file contains the WPF user control used to edit resource item files
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/21/2011  EFW  Created the code
// 10/27/2012  EFW  Updated to use the presentation style configuration file
// 01/07/2014  EFW  Updated to use MEF to load presentation style information
// 08/06/2014  EFW  Added support for syntax generator resource item files
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

using Sandcastle.Core;
using Sandcastle.Core.PresentationStyle;

using Sandcastle.Platform.Windows;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.ConceptualContent;

namespace SandcastleBuilder.WPF.UserControls
{
    /// <summary>
    /// This user control is used to edit resource item files
    /// </summary>
    public partial class ResourceItemEditorControl : UserControl
    {
        #region Private data members
        //=====================================================================

        private BindingList<ResourceItem> resourceItems;
        private readonly SortedDictionary<string, ResourceItem> allItems, sandcastleItems;
        private IEnumerator<ResourceItem> matchEnumerator;
        private string resourceItemFilename;

        #endregion

        #region Routed events
        //=====================================================================

        /// <summary>
        /// This registers the <see cref="ContentModified"/> event
        /// </summary>
        public static readonly RoutedEvent ContentModifiedEvent = EventManager.RegisterRoutedEvent(
            "ContentModifiedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler),
            typeof(ResourceItemEditorControl));

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
        public ResourceItemEditorControl()
        {
            InitializeComponent();

            allItems = new SortedDictionary<string, ResourceItem>();
            sandcastleItems = new SortedDictionary<string, ResourceItem>();
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Load a resource items file for editing
        /// </summary>
        /// <param name="resourceItemsFile">The resource items file to load</param>
        /// <param name="project">The current Sandcastle Builder project</param>
        public void LoadResourceItemsFile(string resourceItemsFile, SandcastleProject project)
        {
            PresentationStyleSettings pss = null;
            string presentationStylePath, shfbStyleContent;
            List<string> syntaxGeneratorFiles = new List<string>();

            if(resourceItemsFile == null)
                throw new ArgumentNullException(nameof(resourceItemsFile));

            if(project == null)
                throw new ArgumentNullException(nameof(project));

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                resourceItemFilename = resourceItemsFile;

                using(var container = ComponentUtilities.CreateComponentContainer(project.ComponentSearchPaths,
                  CancellationToken.None))
                {
                    var presentationStyles = container.GetExports<PresentationStyleSettings, IPresentationStyleMetadata>();
                    var style = presentationStyles.FirstOrDefault(s => s.Metadata.Id.Equals(
                        project.PresentationStyle, StringComparison.OrdinalIgnoreCase));

                    if(style == null)
                        style = presentationStyles.FirstOrDefault(s => s.Metadata.Id.Equals(
                            Constants.DefaultPresentationStyle, StringComparison.OrdinalIgnoreCase));

                    if(style != null)
                        pss = style.Value;
                    else
                    {
                        MessageBox.Show("Unable to locate the presentation style ID " + project.PresentationStyle,
                            Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    syntaxGeneratorFiles.AddRange(ComponentUtilities.SyntaxGeneratorResourceItemFiles(container, null));
                }

                // Get the presentation style folders
                presentationStylePath = pss.ResolvePath(pss.ResourceItemsPath);
                shfbStyleContent = pss.ResolvePath(pss.ToolResourceItemsPath);

                // Use the language-specific files if they are present
                if(Directory.Exists(Path.Combine(presentationStylePath, project.Language.Name)))
                    presentationStylePath = Path.Combine(presentationStylePath, project.Language.Name);

                if(File.Exists(Path.Combine(shfbStyleContent, project.Language.Name + ".xml")))
                    shfbStyleContent = Path.Combine(shfbStyleContent, project.Language.Name + ".xml");
                else
                    shfbStyleContent = Path.Combine(shfbStyleContent, "en-US.xml");

                // Load the presentation style content files first followed by the syntax generator files, the
                // help file builder content items, and then the user's resource item file.
                foreach(string file in Directory.EnumerateFiles(presentationStylePath, "*.xml"))
                    this.LoadItemFile(file, false);

                foreach(string file in syntaxGeneratorFiles)
                    this.LoadItemFile(file, false);

                if(File.Exists(shfbStyleContent))
                    this.LoadItemFile(shfbStyleContent, false);

                this.LoadItemFile(resourceItemFilename, true);

                // Load everything into the list box
                resourceItems = new BindingList<ResourceItem>(allItems.Values.ToArray());
                resourceItems.ListChanged += resourceItems_ListChanged;

                if(resourceItems.Count != 0)
                    resourceItems[0].IsSelected = true;

                lbResourceItems.ItemsSource = resourceItems;

                this.resourceItems_ListChanged(this, new ListChangedEventArgs(ListChangedType.Reset, -1));
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                MessageBox.Show("Unable to load resource item files: " + ex.Message, Constants.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// This is used to load a resource item file's content into the dictionaries used by the editor
        /// </summary>
        /// <param name="filename">The file to load</param>
        /// <param name="containsOverrides">True if this file contains overrides for the Sandcastle items</param>
        private void LoadItemFile(string filename, bool containsOverrides)
        {
            ResourceItem r;
            XmlReaderSettings settings = new XmlReaderSettings { CloseInput = true };

            try
            {
                using(var xr = XmlReader.Create(filename, settings))
                {
                    xr.MoveToContent();

                    while(!xr.EOF)
                    {
                        if(xr.NodeType == XmlNodeType.Element && xr.Name == "item")
                        {
                            r = new ResourceItem(filename, xr.GetAttribute("id"), xr.ReadInnerXml(), containsOverrides);

                            allItems[r.Id] = r;

                            // Create a clone of the original for Sandcastle items
                            if(!containsOverrides)
                            {
                                r = new ResourceItem(filename, r.Id, r.Value, false);
                                sandcastleItems[r.Id] = r;
                            }
                        }

                        xr.Read();
                    }
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                MessageBox.Show("Unable to save resource item file: " + ex.Message, Constants.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// This is used to find all resource items that match the specified predicate
        /// </summary>
        /// <param name="match">The match predicate</param>
        /// <returns>An enumerable list of all matches</returns>
        private IEnumerable<ResourceItem> Find(Predicate<ResourceItem> match)
        {
            foreach(var r in resourceItems)
                if(match(r))
                    yield return r;
        }

        /// <summary>
        /// Save the modified resource items to the project's resource item file
        /// </summary>
        /// <param name="resourceItemsFile">The resource item filename</param>
        public void Save(string resourceItemsFile)
        {
            XmlWriterSettings settings = new XmlWriterSettings { Indent = true, CloseOutput = true };

            this.CommitChanges();

            using(var writer = XmlWriter.Create(resourceItemsFile, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("content");
                writer.WriteAttributeString("xml", "space", null, "preserve");

                foreach(ResourceItem r in allItems.Values)
                    if(r.IsOverridden)
                    {
                        writer.WriteStartElement("item");
                        writer.WriteAttributeString("id", r.Id);

                        // The value is written as raw text to preserve any XML within it.  The item value is
                        // also trimmed to remove unnecessary whitespace that might affect the layout.
                        writer.WriteRaw(r.Value.Trim());
                        writer.WriteEndElement();
                    }

                writer.WriteEndElement();   // </content>
                writer.WriteEndDocument();
            }
        }
        #endregion

        #region General event handlers
        //=====================================================================

        /// <summary>
        /// This is used to mark the file as dirty when the collection changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void resourceItems_ListChanged(object sender, ListChangedEventArgs e)
        {
            if(e.PropertyDescriptor != null)
                switch(e.PropertyDescriptor.Name)
                {
                    case "IsOverridden":
                    case "IsSelected":
                    case "SourceFile":
                        // We don't care about changes to these properties as they are for the editor and don't
                        // affect the state of the resource item collection.
                        return;

                    case "Value":
                        // Mark the item as overridden if the value changes
                        if(lbResourceItems.SelectedItem is ResourceItem r)
                        {
                            r.SourceFile = resourceItemFilename;
                            r.IsOverridden = true;
                        }
                        break;

                    default:
                        break;
                }

            if(sender != this)
                this.RaiseEvent(new RoutedEventArgs(ContentModifiedEvent, this));

            // Update control state based on the collection content
            lbResourceItems.IsEnabled = txtValue.IsEnabled = (resourceItems != null && resourceItems.Count != 0);

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
            if(resourceItems == null || resourceItems.Count == 0)
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
                matchEnumerator = this.Find(r =>
                  (!String.IsNullOrEmpty(r.Id) && r.Id.IndexOf(txtFindID.Text,
                    StringComparison.CurrentCultureIgnoreCase) != -1) ||
                  (!String.IsNullOrEmpty(r.Value) && r.Value.IndexOf(txtFindID.Text,
                    StringComparison.CurrentCultureIgnoreCase) != -1)).GetEnumerator();

            // Move to the next match
            if(matchEnumerator.MoveNext())
            {
                matchEnumerator.Current.IsSelected = true;
                lbResourceItems.ScrollIntoView(matchEnumerator.Current);
            }
            else
            {
                if(matchEnumerator != null)
                {
                    matchEnumerator.Dispose();
                    matchEnumerator = null;
                }

                MessageBox.Show("No more matches found", Constants.AppName, MessageBoxButton.OK,
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
        /// Show all items or limit the list to overridden items
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkLimitToOverridden_Click(object sender, RoutedEventArgs e)
        {
            if(lbResourceItems.SelectedItem is ResourceItem currentItem)
                currentItem.IsSelected = false;

            resourceItems.ListChanged -= resourceItems_ListChanged;
            lbResourceItems.ItemsSource = null;

            if(chkLimitToOverridden.IsChecked.Value)
                resourceItems = new BindingList<ResourceItem>(allItems.Values.Where(r => r.IsOverridden).ToArray());
            else
                resourceItems = new BindingList<ResourceItem>(allItems.Values.ToArray());

            if(resourceItems.Count != 0)
                resourceItems[0].IsSelected = true;

            resourceItems.ListChanged += resourceItems_ListChanged;
            lbResourceItems.ItemsSource = resourceItems;

            this.resourceItems_ListChanged(this, new ListChangedEventArgs(ListChangedType.Reset, -1));
        }
        #endregion

        #region Command event handlers
        //=====================================================================

        /// <summary>
        /// Determine if the Undo commands can be executed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdUndo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (lbResourceItems.SelectedItem is ResourceItem r && r.IsOverridden);
        }

        /// <summary>
        /// Copy the selected item to the clipboard as a link
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdUndo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ResourceItem r = lbResourceItems.SelectedItem as ResourceItem;

            if(MessageBox.Show("Do you want to revert the resource item '" + r.Id + "' to its default value?",
              Constants.AppName, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) ==
              MessageBoxResult.Yes)
            {
                if(sandcastleItems.TryGetValue(r.Id, out ResourceItem defaultItem))
                {
                    r.SourceFile = defaultItem.SourceFile;
                    r.Value = defaultItem.Value;
                    r.IsOverridden = false;
                }

                lbResourceItems.Focus();
            }
        }

        /// <summary>
        /// View help for this editor
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            UiUtility.ShowHelpTopic("fcf8e4ac-5b32-4d5f-9bce-2e85c3468fdc");
        }
        #endregion
    }
}
