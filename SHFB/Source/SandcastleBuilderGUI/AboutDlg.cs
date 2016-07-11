//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : AboutDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/09/2016
// Note    : Copyright 2006-2016, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This form is used to display application version information.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/02/2006  EFW  Created the code
//===============================================================================================================

using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

using Sandcastle.Core;

using SandcastleBuilder.Gui.Properties;

namespace SandcastleBuilder.Gui
{
    /// <summary>
    /// This form is used to display application version information.
    /// </summary>
    public partial class AboutDlg : System.Windows.Forms.Form
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public AboutDlg()
        {
            InitializeComponent();
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Load the controls on the forms with data
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void AboutDlg_Load(object sender, EventArgs e)
        {
            // Get assembly information not available from the application object
            Assembly asm = Assembly.GetExecutingAssembly();
            AssemblyDescriptionAttribute aDescr = (AssemblyDescriptionAttribute)
                Attribute.GetCustomAttribute(asm, typeof(AssemblyDescriptionAttribute));
            AssemblyCopyrightAttribute aCopyright = (AssemblyCopyrightAttribute)
                Attribute.GetCustomAttribute(asm, typeof(AssemblyCopyrightAttribute));

            // Set the labels
            this.Text = "About " + Application.ProductName;
            lblName.Text = Application.ProductName;
            lblVersion.Text = "Version: " + Application.ProductVersion;
            lblDescription.Text = aDescr.Description;
            lblCopyright.Text = aCopyright.Copyright;
            lnkHelp.Text = Settings.Default.AuthorEMailAddress;
            lnkEWoodruffUrl.Text = Settings.Default.EWoodruffURL;
            lnkProjectUrl.Text = Settings.Default.ProjectURL;

            // Display components used by this assembly sorted by name
            AssemblyName[] anComponents = asm.GetReferencedAssemblies();

            foreach(AssemblyName an in anComponents)
            {
                ListViewItem lvi = lvComponents.Items.Add(an.Name);
                lvi.SubItems.Add(an.Version.ToString());
            }

            lvComponents.Sorting = SortOrder.Ascending;
            lvComponents.Sort();

            // Set the e-mail and URL links
            lnkHelp.Links[0].LinkData = "mailto:" + lnkHelp.Text + "?Subject=" + Application.ProductName;
            lnkEWoodruffUrl.Links[0].LinkData = lnkEWoodruffUrl.Text;
            lnkProjectUrl.Links[0].LinkData = lnkProjectUrl.Text;
        }

        /// <summary>
        /// View system information using <b>MSInfo32.exe</b>
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnSysInfo_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("MSInfo32.exe");
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());

                MessageBox.Show("Unable to launch system information viewer.  Reason: " + ex.Message,
                    Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Open the target of the clicked link
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void Link_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                // Launch the e-mail URL, this will fail if user does not have an association for e-mail URLs
                Process.Start((string)e.Link.LinkData);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());

                MessageBox.Show("Unable to launch link target.  Reason: " + ex.Message, Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        #endregion
    }
}
