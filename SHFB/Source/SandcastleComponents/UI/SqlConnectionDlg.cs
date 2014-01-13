//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : SqlConnectionDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/02/2014
// Note    : Copyright 2013-2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a form that is used to specify the SQL Server connection string and set up the database
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.7.0  03/14/2013  EFW  Created the code
//===============================================================================================================

using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using Sandcastle.Core;
using SandcastleBuilder.Components.Properties;

namespace SandcastleBuilder.Components.UI
{
    /// <summary>
    /// This form is used to enter the SQL Server connection string and set up the database
    /// </summary>
    internal partial class SqlConnectionDlg : Form
    {
        #region Private data members
        //=====================================================================

        private Regex reCatalog = new Regex("(Database|Initial Catalog)\\s*=\\s*Sandcastle(;)?",
            RegexOptions.IgnoreCase);
        private Regex reBatch = new Regex("\\s+GO\\s*", RegexOptions.IgnoreCase);
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the connection string
        /// </summary>
        public string ConnectionString
        {
            get { return txtConnectionString.Text; }
            set { txtConnectionString.Text = value; }
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

            txtDBScript.Text = Resources.CreateSandcastleDB;
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

            epErrors.Clear();

            if(connectionString.Length == 0)
            {
                epErrors.SetError(txtConnectionString, "A valid connection string is required");
                return false;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                using(SqlConnection cn = new SqlConnection(connectionString))
                {
                    cn.Open();
                }
            }
            catch(SqlException ex)
            {
                MessageBox.Show("Unable to connect to SQL Server.  Reason:\r\n\r\n" + ex.Message,
                    Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            return true;
        }
        #endregion
        
        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Clear the text selection when a text box gets the focus
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void TextBox_Enter(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;

            tb.Select(0, 0);
            tb.ScrollToCaret();
        }

        /// <summary>
        /// Save the changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            if(this.ValidateConnectionString(txtConnectionString.Text))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        /// <summary>
        /// Create the database and its tables
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnCreate_Click(object sender, EventArgs e)
        {
            // Since the database doesn't exist, remove the initial catalog entry
            string connectionString = reCatalog.Replace(txtConnectionString.Text.Trim(), String.Empty);

            if(this.ValidateConnectionString(connectionString))
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;

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
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch(SqlException ex)
                {
                    MessageBox.Show("Unable to execute script.  Reason:\r\n\r\n" + ex.Message,
                        Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }
        #endregion
    }
}
