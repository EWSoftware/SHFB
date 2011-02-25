//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : ConfigurationEditorDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/07/2007
// Note    : Copyright 2007, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a form used to edit a build component configuration as
// XML text.  This is used for components that have no built-in configuration
// method.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.0.2  07/02/2007  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This form is used to edit a build component configuration as XML text.
    /// This is used for components that have to built-in configuration method.
    /// </summary>
    internal partial class ConfigurationEditorDlg : Form
    {
        /// <summary>
        /// This is used to set or get the configuration text
        /// </summary>
        public string Configuration
        {
            get { return txtConfiguration.Text; }
            set { txtConfiguration.Text = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ConfigurationEditorDlg()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Close the form without saving
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Save the changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                // Make an attempt to see if it's valid
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(txtConfiguration.Text);

                // If we get here, it's probably ok
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch(XmlException ex)
            {
                MessageBox.Show("The XML configuration is not valid.  Reason:" +
                    ex.Message, Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Clear the text selection on entry to prevent accidental
        /// deletion of the text.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void txtConfiguration_Enter(object sender, EventArgs e)
        {
            txtConfiguration.Select(0, 0);
            txtConfiguration.ScrollToCaret();
        }
    }
}
