//=============================================================================
// System  : Sandcastle Help File Builder Components
// File    : PostTransformConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/06/2010
// Note    : Copyright 2006-2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a form that is used to configure the settings for the
// Post Transform Component.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.3.3.0  11/24/2006  EFW  Created the code
// 1.4.0.0  01/31/2007  EFW  Added support for logo placement options and the
//                           colorizer's "Copy" image filename.
// 1.9.0.0  06/06/2010  EFW  Removed outputPath as a configurable option as it
//                           is now represented by multiple paths.
//=============================================================================

using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This form is used to configure the settings for the
    /// <see cref="PostTransformComponent"/>.
    /// </summary>
    internal partial class PostTransformConfigDlg : Form
    {
        #region Private data members
        //=====================================================================

        private XmlDocument config;     // The configuration

        // Current image information
        private string currentLogoFile;
        private Bitmap bmImage;
        private bool changingValue;

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
            node = component.SelectSingleNode("colorizer");

            txtStylesheet.Text = node.Attributes["stylesheet"].Value;
            txtScriptFile.Text = node.Attributes["scriptFile"].Value;
            txtCopyImage.Text = node.Attributes["copyImage"].Value;

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
                    cboPlacement.SelectedIndex = (int)Enum.Parse(
                        typeof(PostTransformComponent.LogoPlacement), attr.Value, true);

                attr = node.Attributes["alignment"];
                if(attr != null)
                    cboAlignment.SelectedIndex = (int)Enum.Parse(
                        typeof(PostTransformComponent.LogoAlignment), attr.Value, true);

                try
                {
                    changingValue = true;
                    udcWidth.Value = width;
                    udcHeight.Value = height;
                    txtLogoFile_Leave(this, EventArgs.Empty);
                }
                finally
                {
                    changingValue = false;
                }
            }
            else
                txtLogoFile_Leave(this, EventArgs.Empty);
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
                MessageBox.Show("Unable to launch link target.  " +
                    "Reason: " + ex.Message, "Sandcastle Help File Builder",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            XmlAttribute attr;
            XmlNode component, node;
            bool isValid = true;

            txtStylesheet.Text = txtStylesheet.Text.Trim();
            txtScriptFile.Text = txtScriptFile.Text.Trim();
            txtCopyImage.Text = txtCopyImage.Text.Trim();
            epErrors.Clear();

            if(txtStylesheet.Text.Length == 0)
            {
                epErrors.SetError(txtStylesheet, "The stylesheet filename is required");
                isValid = false;
            }

            if(txtScriptFile.Text.Length == 0)
            {
                epErrors.SetError(txtScriptFile, "The script filename is required");
                isValid = false;
            }

            if(txtCopyImage.Text.Length == 0)
            {
                epErrors.SetError(txtCopyImage, "The \"Copy\" image filename is required");
                isValid = false;
            }

            if(!isValid)
                return;

            // Store the changes
            component = config.SelectSingleNode("component");
            node = component.SelectSingleNode("colorizer");
            node.Attributes["stylesheet"].Value = txtStylesheet.Text;
            node.Attributes["scriptFile"].Value = txtScriptFile.Text;
            node.Attributes["copyImage"].Value = txtCopyImage.Text;

            // Auto-correct the configuration if it is still using the old outputPath element
            node = component.SelectSingleNode("outputPath");

            if(node != null)
            {
                XmlNode outputPaths = config.CreateNode(XmlNodeType.Element, "outputPaths", null);
                outputPaths.InnerText = "{@HelpFormatOutputPaths}";
                component.ReplaceChild(outputPaths, node);
            }

            node = component.SelectSingleNode("logoFile");
            if(node == null)
            {
                node = config.CreateNode(XmlNodeType.Element, "logoFile", null);
                component.AppendChild(node);

                attr = config.CreateAttribute("filename");
                node.Attributes.Append(attr);
            }
            else
                attr = node.Attributes["filename"];

            attr.Value = txtLogoFile.Text;

            attr = node.Attributes["altText"];
            if(attr == null)
            {
                attr = config.CreateAttribute("altText");
                node.Attributes.Append(attr);
            }

            attr.Value = txtAltText.Text;

            attr = node.Attributes["width"];
            if(attr == null)
            {
                attr = config.CreateAttribute("width");
                node.Attributes.Append(attr);
            }

            attr.Value = ((int)udcWidth.Value).ToString(
                CultureInfo.InvariantCulture);

            attr = node.Attributes["height"];
            if(attr == null)
            {
                attr = config.CreateAttribute("height");
                node.Attributes.Append(attr);
            }

            attr.Value = ((int)udcHeight.Value).ToString(
                CultureInfo.InvariantCulture);

            attr = node.Attributes["placement"];
            if(attr == null)
            {
                attr = config.CreateAttribute("placement");
                node.Attributes.Append(attr);
            }

            attr.Value = ((PostTransformComponent.LogoPlacement)
                cboPlacement.SelectedIndex).ToString().ToLower(
                    CultureInfo.InvariantCulture);

            attr = node.Attributes["alignment"];
            if(attr == null)
            {
                attr = config.CreateAttribute("alignment");
                node.Attributes.Append(attr);
            }

            attr.Value = ((PostTransformComponent.LogoAlignment)
                cboAlignment.SelectedIndex).ToString().ToLower(
                    CultureInfo.InvariantCulture);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Select the syntax or style file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void SelectFile_Click(object sender, EventArgs e)
        {
            Button b = sender as Button;
            TextBox t;

            using(OpenFileDialog dlg = new OpenFileDialog())
            {
                if(b == btnSelectStylesheet)
                {
                    t = txtStylesheet;
                    dlg.Title = "Select the colorizer stylesheet file";
                    dlg.Filter = "Stylesheet files (*.css)|*.css|" +
                        "All Files (*.*)|*.*";
                    dlg.DefaultExt = "css";
                }
                else
                {
                    t = txtScriptFile;
                    dlg.Title = "Select the colorizer JavaScript file";
                    dlg.Filter = "JavaScript files (*.js)|*.js|" +
                        "All Files (*.*)|*.*";
                    dlg.DefaultExt = "js";
                }

                dlg.InitialDirectory = Directory.GetCurrentDirectory();

                // If selected, add the new file(s)
                if(dlg.ShowDialog() == DialogResult.OK)
                    t.Text = dlg.FileName;
            }
        }

        /// <summary>
        /// Select the logo or "Copy" image file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void SelectImage_Click(object sender, EventArgs e)
        {
            Button b = sender as Button;
            TextBox t;

            using(OpenFileDialog dlg = new OpenFileDialog())
            {
                if(b == btnSelectLogo)
                {
                    t = txtLogoFile;
                    dlg.Title = "Select the logo image file";
                }
                else
                {
                    t = txtCopyImage;
                    dlg.Title = "Select \"Copy\" image file";
                }

                dlg.Filter = "Image files (*.gif, *.jpg, *.jpe*, *.png, " +
                    "*.bmp)|*.gif;*.jpg;*.jpe*;*.png;*.bmp|" +
                    "All Files (*.*)|*.*";
                dlg.InitialDirectory = Directory.GetCurrentDirectory();
                dlg.DefaultExt = "jpg";

                // If selected, add the new file(s)
                if(dlg.ShowDialog() == DialogResult.OK)
                {
                    t.Text = dlg.FileName;

                    if(t == txtLogoFile)
                        txtLogoFile_Leave(sender, e);
                }
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

            chkProportional.Enabled = false;

            if(bmImage != null)
            {
                bmImage.Dispose();
                bmImage = null;
            }

            try
            {
                if(logoFile.Length != 0 && File.Exists(logoFile))
                {
                    if(!changingValue)
                        udcHeight.Value = udcWidth.Value = 0;

                    bmImage = new Bitmap(logoFile);
                    lblActualSize.Text = String.Format(
                        CultureInfo.InvariantCulture, "{0} x {1}",
                        bmImage.Width, bmImage.Height);
                    chkProportional.Enabled = true;
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show("Unable to load image file.  Error: " +
                    ex.Message, "Sandcastle Help File Builder",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            pnlImage.Invalidate();
            pnlImage.Update();
        }

        /// <summary>
        /// Draw the image on the panel using the width and height settings
        /// to give an idea of how big it will be.
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
                    Rectangle r = new Rectangle(0, 0, (int)udcWidth.Value,
                        (int)udcHeight.Value);
                    e.Graphics.DrawImage(bmImage, r);
                }
        }

        /// <summary>
        /// Redraw the image when the width or height changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void WidthHeight_ValueChanged(object sender, EventArgs e)
        {
            if(changingValue)
                return;

            if(chkProportional.Enabled && chkProportional.Checked)
            {
                try
                {
                    changingValue = true;

                    if(sender == udcHeight || sender == chkProportional)
                        udcWidth.Value = udcHeight.Value *
                            ((decimal)bmImage.Width / (decimal)bmImage.Height);
                    else
                        udcHeight.Value = udcWidth.Value *
                            ((decimal)bmImage.Height / (decimal)bmImage.Width);
                }
                finally
                {
                    changingValue = false;
                }
            }

            pnlImage.Invalidate();
            pnlImage.Update();
        }

        /// <summary>
        /// Disable alignment unless "Above" is selected for placement
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cboPlacement_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboAlignment.Enabled = (cboPlacement.SelectedIndex ==
                (int)PostTransformComponent.LogoPlacement.Above);
        }
        #endregion
    }
}
