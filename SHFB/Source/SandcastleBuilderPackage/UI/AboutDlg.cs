//=============================================================================
// System  : Sandcastle Help File Builder
// File    : AboutDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/04/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This form is used to display application version information.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.0  04/04/2011  EFW  Created the code
//=============================================================================

using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using SandcastleBuilder.Package.Properties;

namespace SandcastleBuilder.Package.UI
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
		private void AboutDlg_Load(object sender, System.EventArgs e)
		{
			// Get assembly information not available from the application object
            Assembly asm = Assembly.GetExecutingAssembly();
			AssemblyDescriptionAttribute aDescr = (AssemblyDescriptionAttribute)
				AssemblyDescriptionAttribute.GetCustomAttribute(asm,
				typeof(AssemblyDescriptionAttribute));
            AssemblyCopyrightAttribute aCopyright = (AssemblyCopyrightAttribute)
                AssemblyCopyrightAttribute.GetCustomAttribute(asm,
                typeof(AssemblyCopyrightAttribute));
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

			// Set the labels
            this.Text = "About " + Resources.PackageTitle;
            lblName.Text = Resources.PackageTitle;
			lblVersion.Text = "Version: " + fvi.FileVersion;
			lblDescription.Text = aDescr.Description;
            lblCopyright.Text = aCopyright.Copyright;
            lnkHelp.Text = Resources.AuthorEMailAddress;
            lnkEWoodruffUrl.Text = Resources.EWoodruffURL;
            lnkProjectUrl.Text = Resources.ProjectURL;

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
			lnkHelp.Links[0].LinkData = "mailto:" + lnkHelp.Text + "?Subject=" + Resources.PackageTitle;
            lnkEWoodruffUrl.Links[0].LinkData = lnkEWoodruffUrl.Text;
            lnkProjectUrl.Links[0].LinkData = lnkProjectUrl.Text;
        }

        /// <summary>
        /// View system information using <b>MSInfo32.exe</b>
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
		private void btnSysInfo_Click(object sender, System.EventArgs e)
		{
			try
			{
				System.Diagnostics.Process.Start("MSInfo32.exe");
			}
			catch(Exception ex)
			{
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show("Unable to launch system information viewer.  Reason: " + ex.Message,
                    Resources.PackageTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
		}

        /// <summary>
        /// Open the target of the clicked link
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void Link_LinkClicked(object sender,
          System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
			try
			{
				// Launch the e-mail URL, this will fail if user does not
                // have an association for e-mail URLs.
				System.Diagnostics.Process.Start((string)e.Link.LinkData);
			}
			catch(Exception ex)
			{
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show("Unable to launch link target.  Reason: " + ex.Message,
                    Resources.PackageTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        #endregion
    }
}
