//===============================================================================================================
// System  : Sandcastle Guided Installation
// File    : MainForm.cs
// Author  : Eric Woodruff
// Updated : 12/28/2013
// Compiler: Microsoft Visual C#
//
// This is the main form for the Sandcastle Guided Installer.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.0.0.0  02/05/2011  EFW  Created the code
// 1.1.0.0  03/05/2012  EFW  Converted to use WPF
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

using IOPath = System.IO.Path;

using Sandcastle.Installer.InstallerPages;

namespace Sandcastle.Installer
{
    /// <summary>
    /// This is the main form for the application
    /// </summary>
    public partial class MainWindow : Window, IInstaller
    {
        #region Constants and private data members
        //=====================================================================

        private const string ApplicationTitle = "Sandcastle Guided Installation";

        private Dictionary<string, Assembly> assemblies;
        private List<IInstallerPage> allPages;
        private int lastPage, currentPage;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            assemblies = new Dictionary<string, Assembly>();
            allPages = new List<IInstallerPage>();
            lastPage = -1;
            currentPage = 0;

            InitializeComponent();

            this.LoadConfiguration();
            this.SetCurrentState();
        }
        #endregion

        #region IInstaller Members
        //=====================================================================

        /// <inheritdoc />
        public IEnumerable<IInstallerPage> AllPages
        {
            get { return allPages; }
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to load the configuration settings
        /// </summary>
        private void LoadConfiguration()
        {
            string version = null, configFile = IOPath.Combine(Utility.BasePath, "InstallerConfiguration.xml");

            try
            {
                XDocument config = XDocument.Load(configFile);

                var offset = config.Root.Element("pathOffset");

                if(offset != null && offset.Attribute("location") != null)
                    Utility.PathOffset = offset.Attribute("location").Value;

                var sandcastle = config.Root.Element("sandcastle");

                if(sandcastle != null && sandcastle.Attribute("version") != null)
                    version = sandcastle.Attribute("version").Value.Trim();

                if(sandcastle == null || String.IsNullOrEmpty(version))
                {
                    lblTitle.Content = String.Format(CultureInfo.CurrentCulture, lblTitle.Content.ToString(),
                        "No configuration", String.Empty);
                    throw new InvalidOperationException("Unable to find a valid Sandcastle version element in " +
                        "configuration file");
                }

                lblTitle.Content = String.Format(CultureInfo.CurrentCulture, lblTitle.Content.ToString(), version);

                // Load the root pages and all child pages
                foreach(var page in config.Root.Descendants("pages").Elements("page"))
                    this.AddHelpPage(page, null, version);
            }
            catch(Exception ex)
            {
                // The 0x80131515 error usually means the ZIP file that contained us was blocked after download.
                // As such all the content is considered untrusted and we can't do certain things like load
                // other assemblies.  Unblocking the ZIP file before extraction fixes that issue.
                if(ex.Message.IndexOf("80131515", StringComparison.Ordinal) != -1)
                    MessageBox.Show("Unable to load configuration page assembly.  The most likely cause is " +
                        "that the ZIP file containing these files was blocked.  Use the file properties on " +
                        "the ZIP file to unblock it, re-extract the content, and run this installer again.",
                        ApplicationTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show("Unable to load configuration settings:\r\n\r\n" + ex.Message, ApplicationTitle,
                        MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// This is used to recursively load help page definitions
        /// </summary>
        /// <param name="page">The help page to load</param>
        /// <param name="root">The root tree node to which the page is added.  If null, it is added as a root
        /// page.</param>
        /// <param name="sandcastleDate">The Sandcastle version</param>
        private void AddHelpPage(XElement page, TreeViewItem root, string sandcastleVersion)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            IInstallerPage pageInstance;
            TreeViewItem node;
            Type pageType;
            string value;

            // Load the assembly?
            if(page.Attribute("assembly") != null)
            {
                value = page.Attribute("assembly").Value.Trim();

                if(!String.IsNullOrEmpty(value) && !IOPath.IsPathRooted(value))
                    value = IOPath.Combine(Utility.BasePath, value);

                if(!String.IsNullOrEmpty(value) && !assemblies.ContainsKey(value))
                {
                    if(!File.Exists(value))
                    {
                        MessageBox.Show("Unable to load installer page assembly: " + value, ApplicationTitle,
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    assemblies.Add(value, Assembly.LoadFrom(value));
                }

                if(!String.IsNullOrEmpty(value))
                    asm = assemblies[value];
            }

            if(page.Attribute("type") == null)
                throw new InvalidOperationException("Invalid installer page definition.  Missing type name.");

            value = page.Attribute("type").Value.Trim();

            pageType = asm.GetType(value);

            if(pageType == null)
            {
                MessageBox.Show("Unable to load installer page type: " + value, ApplicationTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            pageInstance = (IInstallerPage)Activator.CreateInstance(pageType);
            pageInstance.Installer = this;
            pageInstance.SandcastleVersion = sandcastleVersion;

            pageInstance.Initialize(page);
            pageInstance.Control.Visibility = Visibility.Collapsed;

            node = new TreeViewItem();
            node.Header = pageInstance.PageTitle;
            node.IsExpanded = true;
            node.Name = String.Format(CultureInfo.InvariantCulture, "P_{0}", allPages.Count);

            if(root == null)
                tvPages.Items.Add(node);
            else
                root.Items.Add(node);

            allPages.Add(pageInstance);
            pnlPages.Children.Add(pageInstance.Control);

            foreach(var childPage in page.Elements("page"))
                this.AddHelpPage(childPage, node, sandcastleVersion);
        }

        /// <summary>
        /// Set the form state based on the current page
        /// </summary>
        private void SetCurrentState()
        {
            string id;

            btnPrevious.IsEnabled = (currentPage > 0);
            btnNext.IsEnabled = (currentPage < allPages.Count - 1);

            if(lastPage != -1 && lastPage < allPages.Count)
            {
                pnlPages.Children[lastPage].Visibility = Visibility.Collapsed;
                allPages[lastPage].HidePage();
            }

            if(currentPage < allPages.Count)
            {
                pnlPages.Children[currentPage].Visibility = Visibility.Visible;
                allPages[currentPage].ShowPage();

                id = String.Format(CultureInfo.InvariantCulture, "P_{0}", currentPage);

                foreach(var ti in tvPages.Items.OfType<TreeViewItem>().SelectMany(t => Flatten(t)))
                    if(ti.Name == id)
                    {
                        ti.IsSelected = true;
                        break;
                    }
            }

            lastPage = currentPage;

            if(currentPage == allPages.Count - 1)
            {
                btnClose.ToolTip = "Close the guided installation";
                btnClose.Content = "Close";
            }
            else
            {
                btnClose.ToolTip = "Cancel the guided installation";
                btnClose.Content = "Cancel";
            }
        }

        /// <summary>
        /// Flatten the tree view hierarchy to make it searchable
        /// </summary>
        /// <param name="item">The initial tree view item</param>
        /// <returns>An enumerable collection containing the initial item and all of its children recursively</returns>
        private static IEnumerable<TreeViewItem> Flatten(TreeViewItem item)
        {
            return (new[] { item }).Concat(item.Items.OfType<TreeViewItem>().SelectMany(ti => Flatten(ti)));
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Set the focus to the Next or Close button when activated
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void Window_Activated(object sender, EventArgs e)
        {
            if(btnNext.IsEnabled)
                btnNext.Focus();
            else
                btnClose.Focus();
        }

        /// <summary>
        /// Prompt to confirm closing if not finished.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if(currentPage < allPages.Count - 1 && MessageBox.Show("You have not finished the guided " +
              "installation yet.  You can run it again later to finish the remaining steps.  Do you want to " +
              "exit now?", ApplicationTitle, MessageBoxButton.YesNo, MessageBoxImage.Question,
              MessageBoxResult.No) == MessageBoxResult.No)
                e.Cancel = true;
        }

        /// <summary>
        /// Prevent the user from changing the selected tree view item using the mouse
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvPages_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Close the installer
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Move to the next page if allowed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if(currentPage >= 0 && !allPages[currentPage].CanContinue)
                return;

            if(currentPage < allPages.Count - 1)
                currentPage++;

            this.SetCurrentState();
        }

        /// <summary>
        /// Move to the previous page
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if(currentPage > 0)
                currentPage--;

            this.SetCurrentState();
        }
        #endregion
    }
}
