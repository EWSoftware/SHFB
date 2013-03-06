//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : PostTransformConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/27/2013
// Note    : Copyright 2006-2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// The post transform component is obsolete and will be removed in a future release.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.3.3.0  11/24/2006  EFW  Created the code
// 1.4.0.0  01/31/2007  EFW  Added support for logo placement options and the colorizer's "Copy" image filename.
// 1.9.0.0  06/06/2010  EFW  Removed outputPath as a configurable option as it is now represented by multiple
//                           paths.
// 1.9.6.0  10/21/2012  EFW  Disabled editing as the component is now obsolete.
//===============================================================================================================

using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace SandcastleBuilder.Components.UI
{
    /// <summary>
    /// This form is used to configure the settings for the <see cref="PostTransformComponent"/>
    /// </summary>
    internal partial class PostTransformConfigDlg : Form
    {
        #region Private data members
        //=====================================================================

        private XmlDocument config;     // The configuration

        // Current image information
        private string currentLogoFile;
        private Bitmap bmImage;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to return the configuration information
        /// </summary>
        public string Configuration
        {
            get { return config.OuterXml; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="currentConfig">The current XML configuration
        /// XML fragment</param>
        public PostTransformConfigDlg(string currentConfig)
        {
            XmlAttribute attr;
            XmlNode component, node;
            int width = 0, height = 0;

            InitializeComponent();
            cboPlacement.SelectedIndex = cboAlignment.SelectedIndex = 0;

            lnkCodePlexSHFB.Links[0].LinkData = "http://SHFB.CodePlex.com";

            // Load the current settings
            config = new XmlDocument();
            config.LoadXml(currentConfig);

            component = config.SelectSingleNode("component");
            node = component.SelectSingleNode("logoFile");

            if(node != null)
            {
                txtLogoFile.Text = node.Attributes["filename"].Value;

                attr = node.Attributes["altText"];

                if(attr != null)
                    txtAltText.Text = attr.Value;

                attr = node.Attributes["width"];

                if(attr != null)
                    Int32.TryParse(attr.Value, out width);

                attr = node.Attributes["height"];

                if(attr != null)
                    Int32.TryParse(attr.Value, out height);

                attr = node.Attributes["placement"];

                if(attr != null)
                    cboPlacement.SelectedIndex = cboPlacement.FindString(attr.Value);

                attr = node.Attributes["alignment"];

                if(attr != null)
                    cboAlignment.SelectedIndex = cboAlignment.FindString(attr.Value);

                udcWidth.Value = width;
                udcHeight.Value = height;
                txtLogoFile_Leave(this, EventArgs.Empty);
            }
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
        /// Go to the CodePlex home page of the Sandcastle Help File Builder
        /// project.
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
                    "Sandcastle Help File Builder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Update the image information if it changed
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void txtLogoFile_Leave(object sender, EventArgs e)
        {
            string logoFile = txtLogoFile.Text.Trim();

            if(txtLogoFile.Text == currentLogoFile)
                return;

            currentLogoFile = txtLogoFile.Text;
            lblActualSize.Text = "--";

            if(bmImage != null)
            {
                bmImage.Dispose();
                bmImage = null;
            }

            try
            {
                if(logoFile.Length != 0 && File.Exists(logoFile))
                {
                    bmImage = new Bitmap(logoFile);
                    lblActualSize.Text = String.Format(CultureInfo.InvariantCulture, "{0} x {1}",
                        bmImage.Width, bmImage.Height);
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show("Unable to load image file.  Error: " + ex.Message,
                    "Sandcastle Help File Builder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            pnlImage.Invalidate();
            pnlImage.Update();
        }

        /// <summary>
        /// Draw the image on the panel using the width and height settings to give an idea of how big it
        /// will be.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void pnlImage_Paint(object sender, PaintEventArgs e)
        {
            if(bmImage != null)
                if(udcWidth.Value == 0 && udcHeight.Value == 0)
                    e.Graphics.DrawImage(bmImage, 0, 0);
                else
                {
                    Rectangle r = new Rectangle(0, 0, (int)udcWidth.Value, (int)udcHeight.Value);
                    e.Graphics.DrawImage(bmImage, r);
                }
        }
        #endregion
    }
}
