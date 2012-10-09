//=============================================================================
// System  : Sandcastle Help File Builder
// File    : AboutDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/30/2006
// Note    : Copyright 2006, Eric Woodruff, All rights reserved
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
// 1.0.0.0  08/02/2006  EFW  Created the code
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

using SandcastleBuilder.Gui.Properties;
using SandcastleBuilder.Utils;

namespace SandcastleBuilder.Gui
{
    /// <summary>
    /// This form is used to display application version information.
    /// </summary>
	public class AboutDlg : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ColumnHeader ColumnHeader1;
		private System.Windows.Forms.ColumnHeader ColumnHeader2;
		private System.Windows.Forms.Label Label1;
		private System.Windows.Forms.Label lblName;
		private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Button btnSysInfo;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ListView lvComponents;
        private System.Windows.Forms.LinkLabel lnkHelp;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ToolTip toolTip1;
        private SandcastleBuilder.Utils.Controls.StatusBarTextProvider sbMessage;
        private LinkLabel lnkEWoodruffUrl;
        private LinkLabel lnkProjectUrl;
        private Label lblCopyright;
		private System.Windows.Forms.Label lblVersion;

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.lnkHelp = new System.Windows.Forms.LinkLabel();
            this.ColumnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.btnSysInfo = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.lvComponents = new System.Windows.Forms.ListView();
            this.ColumnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.Label1 = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.lnkEWoodruffUrl = new System.Windows.Forms.LinkLabel();
            this.lnkProjectUrl = new System.Windows.Forms.LinkLabel();
            this.sbMessage = new SandcastleBuilder.Utils.Controls.StatusBarTextProvider(this.components);
            this.lblCopyright = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lnkHelp
            // 
            this.lnkHelp.Location = new System.Drawing.Point(493, 321);
            this.lnkHelp.Name = "lnkHelp";
            this.lnkHelp.Size = new System.Drawing.Size(281, 18);
            this.sbMessage.SetStatusBarText(this.lnkHelp, "Send e-mail to the help desk");
            this.lnkHelp.TabIndex = 7;
            this.lnkHelp.TabStop = true;
            this.lnkHelp.Text = "Eric@EWoodruff.us";
            this.toolTip1.SetToolTip(this.lnkHelp, "Send e-mail requesting help");
            this.lnkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.Link_LinkClicked);
            // 
            // ColumnHeader1
            // 
            this.ColumnHeader1.Text = "Name";
            this.ColumnHeader1.Width = 250;
            // 
            // btnSysInfo
            // 
            this.btnSysInfo.Location = new System.Drawing.Point(544, 385);
            this.btnSysInfo.Name = "btnSysInfo";
            this.btnSysInfo.Size = new System.Drawing.Size(112, 32);
            this.sbMessage.SetStatusBarText(this.btnSysInfo, "System Info: View system information for the PC on which the application is runni" +
                    "ng");
            this.btnSysInfo.TabIndex = 10;
            this.btnSysInfo.Text = "System Info...";
            this.toolTip1.SetToolTip(this.btnSysInfo, "Display system information");
            this.btnSysInfo.Click += new System.EventHandler(this.btnSysInfo_Click);
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(664, 385);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(112, 32);
            this.sbMessage.SetStatusBarText(this.btnOK, "OK: Close this dialog box");
            this.btnOK.TabIndex = 11;
            this.btnOK.Text = "OK";
            this.toolTip1.SetToolTip(this.btnOK, "Close this dialog box");
            // 
            // lvComponents
            // 
            this.lvComponents.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnHeader1,
            this.ColumnHeader2});
            this.lvComponents.FullRowSelect = true;
            this.lvComponents.GridLines = true;
            this.lvComponents.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvComponents.Location = new System.Drawing.Point(336, 128);
            this.lvComponents.MultiSelect = false;
            this.lvComponents.Name = "lvComponents";
            this.lvComponents.Size = new System.Drawing.Size(438, 160);
            this.sbMessage.SetStatusBarText(this.lvComponents, "Component name and version information");
            this.lvComponents.TabIndex = 4;
            this.lvComponents.UseCompatibleStateImageBehavior = false;
            this.lvComponents.View = System.Windows.Forms.View.Details;
            // 
            // ColumnHeader2
            // 
            this.ColumnHeader2.Text = "Version";
            this.ColumnHeader2.Width = 150;
            // 
            // Label1
            // 
            this.Label1.Location = new System.Drawing.Point(336, 104);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(160, 19);
            this.Label1.TabIndex = 3;
            this.Label1.Text = "Product Components";
            // 
            // lblName
            // 
            this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.Location = new System.Drawing.Point(336, 8);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(440, 19);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "<Application Name>";
            // 
            // lblDescription
            // 
            this.lblDescription.Location = new System.Drawing.Point(336, 56);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(440, 46);
            this.lblDescription.TabIndex = 2;
            this.lblDescription.Text = "<Description>";
            // 
            // lblVersion
            // 
            this.lblVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVersion.Location = new System.Drawing.Point(336, 32);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(440, 19);
            this.lblVersion.TabIndex = 1;
            this.lblVersion.Text = "<Version>";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(336, 321);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(160, 19);
            this.label2.TabIndex = 6;
            this.label2.Text = "For help send e-mail to";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Tan;
            this.pictureBox1.Image = global::SandcastleBuilder.Gui.Properties.Resources.Sandcastle;
            this.pictureBox1.Location = new System.Drawing.Point(8, 8);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(312, 409);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 12;
            this.pictureBox1.TabStop = false;
            // 
            // lnkEWoodruffUrl
            // 
            this.lnkEWoodruffUrl.Location = new System.Drawing.Point(336, 339);
            this.lnkEWoodruffUrl.Name = "lnkEWoodruffUrl";
            this.lnkEWoodruffUrl.Size = new System.Drawing.Size(438, 18);
            this.sbMessage.SetStatusBarText(this.lnkEWoodruffUrl, "Open a browser to view www.EWoodruff.us");
            this.lnkEWoodruffUrl.TabIndex = 8;
            this.lnkEWoodruffUrl.TabStop = true;
            this.lnkEWoodruffUrl.Text = "http://www.EWoodruff.us";
            this.toolTip1.SetToolTip(this.lnkEWoodruffUrl, "View www.EWoodruff.us");
            this.lnkEWoodruffUrl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.Link_LinkClicked);
            // 
            // lnkProjectUrl
            // 
            this.lnkProjectUrl.Location = new System.Drawing.Point(336, 357);
            this.lnkProjectUrl.Name = "lnkProjectUrl";
            this.lnkProjectUrl.Size = new System.Drawing.Size(438, 18);
            this.sbMessage.SetStatusBarText(this.lnkProjectUrl, "View the project website on CodePlex");
            this.lnkProjectUrl.TabIndex = 9;
            this.lnkProjectUrl.TabStop = true;
            this.lnkProjectUrl.Text = "http://SHFB.CodePlex.com";
            this.toolTip1.SetToolTip(this.lnkProjectUrl, "View project website");
            this.lnkProjectUrl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.Link_LinkClicked);
            // 
            // lblCopyright
            // 
            this.lblCopyright.Location = new System.Drawing.Point(336, 291);
            this.lblCopyright.Name = "lblCopyright";
            this.lblCopyright.Size = new System.Drawing.Size(440, 19);
            this.lblCopyright.TabIndex = 5;
            this.lblCopyright.Text = "<Copyright>";
            // 
            // AboutDlg
            // 
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnOK;
            this.ClientSize = new System.Drawing.Size(786, 429);
            this.Controls.Add(this.lblCopyright);
            this.Controls.Add(this.lnkProjectUrl);
            this.Controls.Add(this.lnkEWoodruffUrl);
            this.Controls.Add(this.lnkHelp);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnSysInfo);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lvComponents);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.label2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About";
            this.Load += new System.EventHandler(this.AboutDlg_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }
		#endregion

        private System.ComponentModel.IContainer components;

        /// <summary>
        /// Constructor
        /// </summary>
		public AboutDlg()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

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
			lnkHelp.Links[0].LinkData = "mailto:" + lnkHelp.Text +
                "?Subject=" + Application.ProductName;
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
                MessageBox.Show("Unable to launch system information " +
                    "viewer.  Reason: " + ex.Message, Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
                MessageBox.Show("Unable to launch link target.  " +
                    "Reason: " + ex.Message, Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
	}
}
