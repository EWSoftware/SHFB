//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : SqlConnectionDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/24/2021
// Note    : Copyright 2013-2021, Eric Woodruff, All rights reserved
//
// This file contains a form that is used to specify the SQL Server connection string and set up the database
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/14/2013  EFW  Created the code
// 04/24/2021  EFW  Converted the form to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

using Sandcastle.Core;
using Sandcastle.Platform.Windows;

namespace Sandcastle.Tools.BuildComponents.UI
{
    /// <summary>
    /// This form is used to define the SQL database connection string and create the database itself
    /// </summary>
    public partial class SqlConnectionDlg : Window
    {
        #region Private data members
        //=====================================================================

        private readonly Regex reCatalog = new Regex("(Database|Initial Catalog)\\s*=\\s*Sandcastle(;)?",
            RegexOptions.IgnoreCase);
        private readonly Regex reBatch = new Regex("\\s+GO\\s*", RegexOptions.IgnoreCase);

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the connection string
        /// </summary>
        public string ConnectionString
        {
            get => txtConnectionString.Text;
            set => txtConnectionString.Text = value;
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public SqlConnectionDlg()
        {
            InitializeComponent();

            txtDBScript.Text = Properties.Resources.CreateSandcastleDB;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Validate the connection string
        /// </summary>
        /// <param name="connectionString">The connection string to test</param>
        /// <returns>True if valid, false if not</returns>
        private bool ValidateConnectionString(string connectionString)
        {
            connectionString = connectionString.Trim();

            if(connectionString.Length == 0)
            {
                txtConnectionString.SetValidationState(false, "A valid connection string is required");
                return false;
            }

            txtConnectionString.SetValidationState(true, null);

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                using(SqlConnection cn = new SqlConnection(connectionString))
                {
                    cn.Open();
                }
            }
            catch(SqlException ex)
            {
                MessageBox.Show("Unable to connect to SQL Server.  Reason:\r\n\r\n" + ex.Message,
                    Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

            return true;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Save the changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if(this.ValidateConnectionString(txtConnectionString.Text))
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        /// <summary>
        /// Create the database and its tables
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            // Since the database doesn't exist, remove the initial catalog entry
            string connectionString = reCatalog.Replace(txtConnectionString.Text.Trim(), String.Empty);

            if(this.ValidateConnectionString(connectionString))
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;

                    using(SqlConnection cn = new SqlConnection(connectionString))
                    {
                        cn.Open();

                        using(SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = cn;

                            foreach(string command in reBatch.Split(txtDBScript.Text))
                            {
                                cmd.CommandText = command.Trim();

                                if(cmd.CommandText.Length != 0)
                                    cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    MessageBox.Show("The database and tables have been created successfully", Constants.AppName,
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch(SqlException ex)
                {
                    MessageBox.Show("Unable to execute script.  Reason:\r\n\r\n" + ex.Message,
                        Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }
        #endregion
    }
}
