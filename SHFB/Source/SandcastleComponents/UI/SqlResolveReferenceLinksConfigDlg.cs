//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : SqlResolveReferenceLinksConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/15/2013
// Note    : Copyright 2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a form that is used to configure the settings for the SQL Resolve Reference Links
// component.
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
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.Components.UI
{
    /// <summary>
    /// This form is used to configure the SQL Resolve Reference Links component
    /// </summary>
    public partial class SqlResolveReferenceLinksConfigDlg : Form
    {
        #region Private data members
        //=====================================================================

        private XElement config;     // The configuration
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to return the configuration information
        /// </summary>
        public string Configuration
        {
            get { return config.ToString(); }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="currentConfig">The current XML configuration XML fragment</param>
        public SqlResolveReferenceLinksConfigDlg(string currentConfig)
        {
            XElement node;

            InitializeComponent();

            lnkCodePlexSHFB.Links[0].LinkData = "http://SHFB.CodePlex.com";

            // Load the current settings.  Note that there are multiple configurations (one for each help file
            // format).  However, the settings will be the same across all of them.
            config = XElement.Parse(currentConfig);

            node = config.Descendants("sqlCache").First();

            txtConnectionString.Text = node.Attribute("connectionString").Value;
            udcContentIdLocalCacheSize.Value = (int)node.Attribute("msdnLocalCacheSize");
            udcFrameworkTargetsLocalCacheSize.Value = (int)node.Attribute("frameworkLocalCacheSize");
            udcProjectTargetsLocalCacheSize.Value = (int)node.Attribute("projectLocalCacheSize");
            chkEnableLocalCache.Checked = (bool)node.Attribute("cacheProject");
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Close without saving
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Go to the CodePlex home page of the Sandcastle Help File Builder project.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lnkCodePlexSHFB_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start((string)e.Link.LinkData);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                MessageBox.Show("Unable to launch link target.  Reason: " + ex.Message,
                    Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Select a cache folder
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnSetConnectionString_Click(object sender, EventArgs e)
        {
            using(SqlConnectionDlg dlg = new SqlConnectionDlg())
            {
                if(txtConnectionString.Text.Trim().Length != 0)
                    dlg.ConnectionString = txtConnectionString.Text;

                if(dlg.ShowDialog() == DialogResult.OK)
                    txtConnectionString.Text = dlg.ConnectionString;
            }
        }

        /// <summary>
        /// Enable or disable current project target caching
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkEnableLocalCache_CheckedChanged(object sender, EventArgs e)
        {
            udcProjectTargetsLocalCacheSize.Enabled = chkEnableLocalCache.Checked;
        }

        /// <summary>
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            txtConnectionString.Text = txtConnectionString.Text.Trim();

            epErrors.Clear();

            if(txtConnectionString.Text.Length == 0)
            {
                epErrors.SetError(txtConnectionString, "A connection string is required");
                return;
            }

            // Update each help format configuration with the same values
            foreach(var n in config.Descendants("sqlCache"))
            {
                n.Attribute("connectionString").Value = txtConnectionString.Text;
                n.SetAttributeValue("msdnLocalCacheSize", (int)udcContentIdLocalCacheSize.Value);
                n.SetAttributeValue("frameworkLocalCacheSize", (int)udcFrameworkTargetsLocalCacheSize.Value);
                n.SetAttributeValue("projectLocalCacheSize", (int)udcProjectTargetsLocalCacheSize.Value);
                n.SetAttributeValue("cacheProject", chkEnableLocalCache.Checked);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Purge the content ID and target cache tables
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnPurge_Click(object sender, EventArgs e)
        {
            txtConnectionString.Text = txtConnectionString.Text.Trim();

            epErrors.Clear();

            if(txtConnectionString.Text.Length == 0)
            {
                epErrors.SetError(txtConnectionString, "A connection string is required");
                return;
            }

            if(MessageBox.Show("WARNING: This will delete all of the current SQL reference link target " +
              "cache data.  The information will need to be created the next time this project is built.  " +
              "Are you sure you want to delete it?", Constants.AppName, MessageBoxButtons.YesNo,
              MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                return;

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                using(SqlConnection cn = new SqlConnection(txtConnectionString.Text))
                {
                    cn.Open();

                    using(SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = cn;
                        cmd.CommandText = "TRUNCATE TABLE ContentIds";
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "TRUNCATE TABLE Targets";
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("The cache data has been deleted", Constants.AppName, MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch(SqlException ex)
            {
                MessageBox.Show("Unable to purge cache tables.  Reason:\r\n\r\n" + ex.Message, Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// Clear the selection when the connection string text box gets the focus
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void txtConnectionString_Enter(object sender, EventArgs e)
        {
            txtConnectionString.Select(0, 0);
            txtConnectionString.ScrollToCaret();
        }
        #endregion
    }
}
