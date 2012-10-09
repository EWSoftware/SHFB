//=============================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : ResourceItemEditorControl.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/03/2012
// Note    : Copyright 2011-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the WPF user control used to edit resource item files
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
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.ConceptualContent;
using SandcastleBuilder.WPF;

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
        private SortedDictionary<string, ResourceItem> allItems, sandcastleItems;
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
            string shfbStyleContent, shfbSharedContent, sharedFolder, presentationFolder;

            if(resourceItemsFile == null)
                throw new ArgumentNullException("resourceItemsFile",
                    "A resource items file name must be specified");

            resourceItemFilename = resourceItemsFile;

            // Get the presentation style folders
            shfbStyleContent = BuildComponentManager.HelpFileBuilderFolder;
            shfbStyleContent = shfbSharedContent = Path.Combine(shfbStyleContent.Substring(0,
                shfbStyleContent.LastIndexOf('\\')), "SharedContent");

            shfbStyleContent = Path.Combine(shfbStyleContent, project.PresentationStyle + "BuilderContent_");
            shfbSharedContent = Path.Combine(shfbSharedContent, "SharedBuilderContent_");

            if(!String.IsNullOrEmpty(project.SandcastlePath))
                presentationFolder = Path.Combine(project.SandcastlePath, "Presentation");
            else
                presentationFolder = Path.Combine(BuildComponentManager.SandcastlePath, "Presentation");

            sharedFolder = Path.Combine(presentationFolder, @"Shared\Content");
            presentationFolder = Path.Combine(presentationFolder, project.PresentationStyle + @"\Content");

            // Use the language-specific files if they are present
            if(Directory.Exists(Path.Combine(sharedFolder, project.Language.Name)))
                sharedFolder = Path.Combine(sharedFolder, project.Language.Name);

            if(Directory.Exists(Path.Combine(presentationFolder, project.Language.Name)))
                presentationFolder = Path.Combine(presentationFolder, project.Language.Name);

            if(File.Exists(Path.Combine(shfbStyleContent, project.Language.Name + ".xml")))
                shfbStyleContent = shfbStyleContent + project.Language.Name + ".xml";
            else
                shfbStyleContent = shfbStyleContent + "en-US.xml";

            if(File.Exists(Path.Combine(shfbSharedContent, project.Language.Name + ".xml")))
                shfbSharedContent = shfbSharedContent + project.Language.Name + ".xml";
            else
                shfbSharedContent = shfbSharedContent + "en-US.xml";

            // Load the sandcastle and SHFB content files in the order in the
            // configuration files:
            //      shared_content.xml
            //      reference_content.xml
            //      syntax_content.xml
            //      feedback_content.xml
            //      conceptual_content.xml
            //      SharedBuilderContent.xml
            //      PresentationStyleBuilderContent.xml
            foreach(string file in new string[] { "shared_content.xml", "reference_content.xml",
              "syntax_content.xml", "feedback_content.xml", "conceptual_content.xml" })
            {
                if(File.Exists(Path.Combine(presentationFolder, file)))
                    this.LoadItemFile(Path.Combine(presentationFolder, file), false);
                else
                    if(File.Exists(Path.Combine(sharedFolder, file)))
                        this.LoadItemFile(Path.Combine(sharedFolder, file), false);
            }

            if(File.Exists(shfbSharedContent))
                this.LoadItemFile(shfbSharedContent, false);

            if(File.Exists(shfbStyleContent))
                this.LoadItemFile(shfbStyleContent, false);

            // Load the user's file with their overrides
            this.LoadItemFile(resourceItemFilename, true);

            // Load everything into the list box
            resourceItems = new BindingList<ResourceItem>(allItems.Values.ToArray());
            resourceItems.ListChanged += resourceItems_ListChanged;

            if(resourceItems.Count != 0)
                resourceItems[0].IsSelected = true;

            lbResourceItems.ItemsSource = resourceItems;

            this.resourceItems_ListChanged(this, new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        /// <summary>
        /// This is used to load a resource item file's content into the
        /// dictionaries used by the editor.
        /// </summary>
        /// <param name="filename">The file to load</param>
        /// <param name="containsOverrides">True if this file contains overrides
        /// for the Sandcastle items</param>
        private void LoadItemFile(string filename, bool containsOverrides)
        {
            ResourceItem r;
            XmlReaderSettings settings = new XmlReaderSettings();
            XmlReader xr = null;

            try
            {
                settings.CloseInput = true;

                xr = XmlReader.Create(filename, settings);
                xr.MoveToContent();

                while(!xr.EOF)
                {
                    if(xr.NodeType == XmlNodeType.Element && xr.Name == "item")
                    {
                        r = new ResourceItem(filename, xr.GetAttribute("id"),
                            xr.ReadInnerXml(), containsOverrides);

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
            finally
            {
                if(xr != null)
                    xr.Close();
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
            XmlWriterSettings settings = new XmlWriterSettings();
            XmlWriter writer = null;

            this.CommitChanges();

            try
            {
                settings.Indent = true;
                settings.CloseOutput = true;
                writer = XmlWriter.Create(resourceItemsFile, settings);

                writer.WriteStartDocument();
                writer.WriteStartElement("content");
                writer.WriteAttributeString("xml", "space", null, "preserve");

                foreach(ResourceItem r in allItems.Values)
                    if(r.IsOverridden)
                    {
                        writer.WriteStartElement("item");
                        writer.WriteAttributeString("id", r.Id);

                        // The value is written as raw text to preserve any XML
                        // within it.  The item value is also trimmed to remove
                        // unnecessary whitespace that might affect the layout.
                        writer.WriteRaw(r.Value.Trim());
                        writer.WriteEndElement();
                    }

                writer.WriteEndElement();   // </content>
                writer.WriteEndDocument();
            }
            finally
            {
                if(writer != null)
                    writer.Close();
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
        void resourceItems_ListChanged(object sender, ListChangedEventArgs e)
        {
            if(e.PropertyDescriptor != null)
                switch(e.PropertyDescriptor.Name)
                {
                    case "IsOverridden":
                    case "IsSelected":
                    case "SourceFile":
                        // We don't care about changes to these properties as they are for the
                        // editor and don't affect the state of the resource item collection.
                        return;

                    case "Value":
                        // Mark the item as overridden if the value changes
                        var r = lbResourceItems.SelectedItem as ResourceItem;

                        if(r != null)
                        {
                            r.SourceFile = resourceItemFilename;
                            r.IsOverridden = true;
                        }
                        break;

                    default:
                        break;
                }

            if(sender != this)
                base.RaiseEvent(new RoutedEventArgs(ContentModifiedEvent, this));

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

                MessageBox.Show("No more matches found", "Resource Item Editor", MessageBoxButton.OK,
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
            ResourceItem currentItem = lbResourceItems.SelectedItem as ResourceItem;

            if(currentItem != null)
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
            ResourceItem r = lbResourceItems.SelectedItem as ResourceItem;

            e.CanExecute = (r != null && r.IsOverridden);
        }

        /// <summary>
        /// Copy the selected item to the clipboard as a link
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cmdUndo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ResourceItem defaultItem, r = lbResourceItems.SelectedItem as ResourceItem;

            if(MessageBox.Show("Do you want to revert the resource item '" +
              r.Id + "' to its default value?", Constants.AppName, MessageBoxButton.YesNo,
              MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                if(sandcastleItems.TryGetValue(r.Id, out defaultItem))
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
            Utility.ShowHelpTopic("fcf8e4ac-5b32-4d5f-9bce-2e85c3468fdc");
        }
        #endregion
    }
}
