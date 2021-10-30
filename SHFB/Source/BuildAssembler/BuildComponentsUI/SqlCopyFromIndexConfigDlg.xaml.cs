//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : SqlCopyFromIndexConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/31/2021
// Note    : Copyright 2013-2021, Eric Woodruff, All rights reserved
//
// This file contains a form that is used to configure the settings for the SQL Copy From Index component.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/15/2013  EFW  Created the code
// 04/24/2021  EFW  Added MEF configuration editor export and converted the form to WPF for better high DPI
//                  scaling support on 4K displays.
//===============================================================================================================

using System;
using System.ComponentModel.Composition.Hosting;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler;

using Sandcastle.Platform.Windows;

namespace Sandcastle.Tools.BuildComponents.UI
{
    /// <summary>
    /// Interaction logic for SqlCopyFromIndexConfigDlg.xaml
    /// </summary>
    public partial class SqlCopyFromIndexConfigDlg : Window
    {
        #region Build component configuration editor factories for MEF
        //=====================================================================

        /// <summary>
        /// This allows editing of the component configuration
        /// </summary>
        [ConfigurationEditorExport("Reflection Index Data (SQL Cache)")]
        public sealed class ReflectionIndexDataFactory : IConfigurationEditor
        {
            /// <inheritdoc />
            public bool EditConfiguration(XElement configuration, CompositionContainer container)
            {
                var dlg = new SqlCopyFromIndexConfigDlg(configuration);

                return dlg.ShowModalDialog() ?? false;
            }
        }

        /// <summary>
        /// This allows editing of the component configuration
        /// </summary>
        [ConfigurationEditorExport("Comments Index Data (SQL Cache)")]
        public sealed class CommentsIndexDataFactory : IConfigurationEditor
        {
            /// <inheritdoc />
            public bool EditConfiguration(XElement configuration, CompositionContainer container)
            {
                var dlg = new SqlCopyFromIndexConfigDlg(configuration);

                return dlg.ShowModalDialog() ?? false;
            }
        }
        #endregion

        #region private data members
        //=====================================================================

        private readonly XElement configuration;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">The current configuration element</param>
        public SqlCopyFromIndexConfigDlg(XElement configuration)
        {
            InitializeComponent();

            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            var node = configuration.Element("index");

            txtConnectionString.Text = node.Attribute("connectionString").Value;
            udcInMemoryCacheSize.Value = (int)node.Attribute("cache");
            udcLocalCacheSize.Value = (int)node.Attribute("localCacheSize");
            chkEnableLocalCache.IsChecked = (bool)node.Attribute("cacheProject");
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// View help for this component
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            UiUtility.ShowHelpTopic("3a1c4bf1-8ecf-4ab3-8010-277bed8d3819");
        }

        /// <summary>
        /// Set the connection string
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnSetup_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SqlConnectionDlg();

            if(txtConnectionString.Text.Trim().Length != 0)
                dlg.ConnectionString = txtConnectionString.Text;

            if(dlg.ShowModalDialog() ?? false)
                txtConnectionString.Text = dlg.ConnectionString;
        }

        /// <summary>
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            txtConnectionString.Text = txtConnectionString.Text.Trim();

            if(txtConnectionString.Text.Length == 0)
            {
                txtConnectionString.SetValidationState(false, "A connection string is required");
                return;
            }

            txtConnectionString.SetValidationState(true, null);

            var node = configuration.Element("index");

            node.Attribute("connectionString").Value = txtConnectionString.Text;
            node.SetAttributeValue("cache", (int)udcInMemoryCacheSize.Value);
            node.SetAttributeValue("localCacheSize", (int)udcLocalCacheSize.Value);
            node.SetAttributeValue("cacheProject", chkEnableLocalCache.IsChecked);

            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Purge the content ID and target cache tables
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnPurge_Click(object sender, RoutedEventArgs e)
        {
            txtConnectionString.Text = txtConnectionString.Text.Trim();

            if(txtConnectionString.Text.Length == 0)
            {
                txtConnectionString.SetValidationState(false, "A connection string is required");
                return;
            }

            txtConnectionString.SetValidationState(true, null);

            if(MessageBox.Show("WARNING: This will delete all of the current SQL index cache data.  The " +
              "information will need to be created the next time this project is built.  Are you sure you " +
              "want to delete it?", Constants.AppName, MessageBoxButton.YesNo, MessageBoxImage.Question,
              MessageBoxResult.No) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                using(SqlConnection cn = new SqlConnection(txtConnectionString.Text))
                {
                    cn.Open();

                    using(SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = cn;
                        cmd.CommandText = "TRUNCATE TABLE IndexData";
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("The cache data has been deleted", Constants.AppName, MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch(SqlException ex)
            {
                MessageBox.Show("Unable to purge cache tables.  Reason:\r\n\r\n" + ex.Message, Constants.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Go to the Sandcastle Help File Builder project site
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
        #endregion
    }
}
