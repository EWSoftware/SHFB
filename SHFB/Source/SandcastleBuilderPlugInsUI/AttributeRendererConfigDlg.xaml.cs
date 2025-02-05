using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Linq;
using System.Xml.XPath;
using Sandcastle.Core;
using Sandcastle.Platform.Windows;
using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;

namespace SandcastleBuilder.PlugIns.UI
{
    public partial class AttributeRendererConfigDlg : Window
    {
        public AttributeRendererConfigDlg()
        {
            InitializeComponent();
        }
        #region Plug-in configuration editor factory for MEF
        //=====================================================================

        /// <summary>
        /// This allows editing of the plug-in configuration
        /// </summary>
        [PlugInConfigurationEditorExport("Attribute Renderer Plugin")]
        public sealed class Factory : IPlugInConfigurationEditor
        {
            /// <inheritdoc />
            public bool EditConfiguration(SandcastleProject project, XElement configuration)
            {
                var dlg = new AttributeRendererConfigDlg(configuration);

                return dlg.ShowModalDialog() ?? false;
            }
        }
        #endregion

        #region Expression item for the list box
        //=====================================================================

        /// <summary>
        /// This is used to edit the expression
        /// </summary>
        // private class ExpressionItem : INotifyPropertyChanged
        // {
        //     private AttributeRepresentationEntry attributeRepresentation, errorMessage;
        //
        //     /// <summary>
        //     /// The XPath query expression
        //     /// </summary>
        //     public AttributeRepresentationEntry AttributeRepresentation
        //     {
        //         get => attributeRepresentation;
        //         set
        //         {
        //             if(attributeRepresentation != value)
        //             {
        //                 attributeRepresentation = value;
        //
        //                 // TODO Do I need this? 
        //                 // try
        //                 // {
        //                 //     // Make an attempt at validating the expression.  Just its syntax, not necessarily that
        //                 //     // it will work in the reflection file.
        //                 //     XDocument doc = new XDocument();
        //                 //     doc.XPathSelectElements(expression);
        //                 //
        //                 //     this.ErrorMessage = null;
        //                 // }
        //                 // catch(Exception ex)
        //                 // {
        //                 //     this.ErrorMessage = "Invalid expression: " + ex.Message;
        //                 // }
        //
        //                 this.OnPropertyChanged();
        //             }
        //         }
        //     }
        //
        //     /// <summary>
        //     /// The error if the expression is not valid
        //     /// </summary>
        //     public string ErrorMessage
        //     {
        //         get => errorMessage;
        //         set
        //         {
        //             errorMessage = value;
        //             this.OnPropertyChanged();
        //         }
        //     }
        //
        //     /// <summary>
        //     /// The property changed event
        //     /// </summary>
        //     public event PropertyChangedEventHandler PropertyChanged;
        //
        //     /// <summary>
        //     /// This raises the <see cref="PropertyChanged"/> event
        //     /// </summary>
        //     /// <param name="propertyName">The property name that changed</param>
        //     protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //     {
        //         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //     }
        // }
        #endregion

        #region Private data members
        //=====================================================================

        private readonly XElement configuration;
        private readonly AttributeRendererPlugin plugin = new AttributeRendererPlugin();
        
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">The configuration to edit</param>
        public AttributeRendererConfigDlg(XElement configuration)
        {
            InitializeComponent();

            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            plugin.ReadConfiguration(configuration);
            
            this.DataContext = plugin;
            
            // Load the current settings
            // foreach(var n in configuration.Descendants("expression"))
                // lbAttributeRenderEntries.Items.Add(new XPathReflectionFileFilterConfigDlg.ExpressionItem { Expression = n.Value });

            btnDelete.IsEnabled = GridItemContent.IsEnabled = lbAttributeRenderEntries.Items.Count != 0;

            if(lbAttributeRenderEntries.Items.Count != 0)
                lbAttributeRenderEntries.SelectedIndex = 0;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// View help for this plug-in
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            UiUtility.ShowHelpTopic("cd68ef02-3493-4af6-96de-1957b0aaf858");
        }

        /// <summary>
        /// Go to the project site
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lnkProjectSite_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(e.Uri.ToString());
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show("Unable to launch link target.  Reason: " + ex.Message, Constants.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// Add a new XPath expression
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var entry = new AttributeRepresentationEntry(plugin) {
                                        AttributeClassName = "T:System.ObsoleteAttribute" ,
                                        ShortRepresentation = "Obsolete.",
                                        LongRepresentation = "This API is obsolete."};
            plugin.AttributeRepresentationEntries.Add(entry);
            lbAttributeRenderEntries.SelectedItem = entry;

            btnDelete.IsEnabled = GridItemContent.IsEnabled = true;
            txtAttributeName.Focus();
        }

        /// <summary>
        /// Delete the selected XPath expression
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            int idx = lbAttributeRenderEntries.SelectedIndex;

            if(idx != -1 && MessageBox.Show("Do you want to delete the expression '" +
              ((AttributeRepresentationEntry)lbAttributeRenderEntries.SelectedItem).AttributeClassName + "'?", Constants.AppName,
              MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                lbAttributeRenderEntries.Items.RemoveAt(idx);

                if(lbAttributeRenderEntries.Items.Count == 0)
                {
                    txtAttributeName.Text = null;
                    txtShortRepresentation.Text = null;
                    txtLongRepresentation.Text = null;
                    btnDelete.IsEnabled = GridItemContent.IsEnabled = false;
                }
                else
                {
                    if(idx < lbAttributeRenderEntries.Items.Count)
                        lbAttributeRenderEntries.SelectedIndex = idx;
                    else
                        lbAttributeRenderEntries.SelectedIndex = lbAttributeRenderEntries.Items.Count - 1;
                }
            }
        }

        /// <summary>
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if(plugin.AttributeRepresentationEntries.Any(entry => entry.ErrorMessage != null))
            {
                MessageBox.Show("One or more XPath query expressions are invalid.  Please fix the expressions " +
                    "before saving them.", Constants.AppName, MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }

            // Store the changes
            configuration.RemoveNodes();

            configuration.Add(new XElement("attributeRepresentations",
                plugin.AttributeRepresentationEntries.Select(entry => entry.ToXml())));

            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Enable or disable the controls based on the selection
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbXPathQueries_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            btnDelete.IsEnabled = GridItemContent.IsEnabled = lbAttributeRenderEntries.SelectedIndex != -1;
        }
        #endregion
    }
}