//=============================================================================
// System  : Sandcastle Help File Builder
// File    : UserPreferencesDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/05/2010
// Note    : Copyright 2007-2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This form is used to allow the user to modify help file builder preferences
// unrelated to projects.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.0.0  06/23/2007  EFW  Created the code
// 1.5.0.2  07/02/2007  EFW  Added content file editor collection option
//=============================================================================

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using SandcastleBuilder.Gui.Properties;
using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Gui
{
    /// <summary>
    /// This form is used to allow the user to modify help file builder
    /// preferences unrelated to projects.
    /// </summary>
    public partial class UserPreferencesDlg : Form
    {
        #region Private data members
        //=====================================================================

        private bool wasModified;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public UserPreferencesDlg()
        {
            InitializeComponent();

            txtHTMLHelp2ViewerPath.Text = Settings.Default.HTMLHelp2ViewerPath;
            txtMSHelpViewerPath.Text = Settings.Default.MSHelpViewerPath;
            udcASPNetDevServerPort.Value = Settings.Default.ASPNETDevServerPort;
            cboBeforeBuildAction.SelectedIndex = (int)Settings.Default.BeforeBuild;
            chkVerboseLogging.Checked = Settings.Default.VerboseLogging;
            chkOpenHelp.Checked = Settings.Default.OpenHelpAfterBuild;
            chkShowLineNumbers.Checked = Settings.Default.ShowLineNumbers;
            chkEnterMatching.Checked = Settings.Default.EnterMatching;
            lblBuildExample.BackColor = Settings.Default.BuildOutputBackground;
            lblBuildExample.ForeColor = Settings.Default.BuildOutputForeground;
            lblBuildExample.Font = Settings.Default.BuildOutputFont;
            lblEditorExample.Font = Settings.Default.TextEditorFont;

            lbContentEditors.DisplayMember = lbContentEditors.ValueMember = "EditorDescription";

            if(Settings.Default.ContentFileEditors.Count == 0)
                pgProps.Enabled = btnDelete.Enabled = false;
            else
            {
                // Binding the collection to the list box caused some
                // odd problems with the property grid so we'll add clones
                // of the items to the list box directly.
                foreach(ContentFileEditor e in Settings.Default.ContentFileEditors)
                    lbContentEditors.Items.Add(e.Clone());

                lbContentEditors.SelectedIndex = 0;
            }
        }
        #endregion

        #region General event handlers
        //=====================================================================

        /// <summary>
        /// Validate the information and save the results if necessary when
        /// closing.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void UserPreferencesDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            ContentFileEditorCollection currentEditors;

            if(this.DialogResult == DialogResult.Cancel)
                return;

            string filePath = txtHTMLHelp2ViewerPath.Text.Trim();

            if(filePath.Length != 0)
            {
                txtHTMLHelp2ViewerPath.Text = filePath = Path.GetFullPath(filePath);

                if(!File.Exists(filePath))
                {
                    epErrors.SetError(btnSelectHxSViewer, "The viewer application does not exist");
                    e.Cancel = true;
                }
            }

            filePath = txtMSHelpViewerPath.Text.Trim();

            if(filePath.Length != 0)
            {
                txtMSHelpViewerPath.Text = filePath = Path.GetFullPath(filePath);

                if(!File.Exists(filePath))
                {
                    epErrors.SetError(btnSelectMSHCViewer, "The viewer application does not exist");
                    e.Cancel = true;
                }
            }

            if(!e.Cancel)
            {
                if(lblBuildExample.BackColor == lblBuildExample.ForeColor)
                {
                    lblBuildExample.BackColor = SystemColors.Window;
                    lblBuildExample.ForeColor = SystemColors.WindowText;
                }

                Settings.Default.HTMLHelp2ViewerPath = txtHTMLHelp2ViewerPath.Text;
                Settings.Default.MSHelpViewerPath = txtMSHelpViewerPath.Text;
                Settings.Default.ASPNETDevServerPort = (int)udcASPNetDevServerPort.Value;
                Settings.Default.BeforeBuild = (BeforeBuildAction)cboBeforeBuildAction.SelectedIndex;
                Settings.Default.VerboseLogging = chkVerboseLogging.Checked;
                Settings.Default.OpenHelpAfterBuild = chkOpenHelp.Checked;
                Settings.Default.ShowLineNumbers = chkShowLineNumbers.Checked;
                Settings.Default.EnterMatching = chkEnterMatching.Checked;
                Settings.Default.BuildOutputBackground = lblBuildExample.BackColor;
                Settings.Default.BuildOutputForeground = lblBuildExample.ForeColor;
                Settings.Default.BuildOutputFont = lblBuildExample.Font;
                Settings.Default.TextEditorFont = lblEditorExample.Font;

                if(wasModified)
                {
                    currentEditors = Settings.Default.ContentFileEditors;
                    currentEditors.Clear();

                    for(int idx = 0; idx < lbContentEditors.Items.Count; idx++)
                        currentEditors.Add((ContentFileEditor)lbContentEditors.Items[idx]);

                    currentEditors.Sort();

                    ContentFileEditorCollection.GlobalEditors.Clear();
                    ContentFileEditorCollection.GlobalEditors.AddRange(
                        currentEditors);
                }

                Settings.Default.Save();
            }
        }
        #endregion

        #region General preferences tab event handler
        //=====================================================================

        /// <summary>
        /// Select a help viewer application
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnSelectViewer_Click(object sender, EventArgs e)
        {
            TextBox tb = (sender == btnSelectHxSViewer) ? txtHTMLHelp2ViewerPath : txtMSHelpViewerPath;

            using(OpenFileDialog dlg = new OpenFileDialog())
            {
                if(tb == txtHTMLHelp2ViewerPath)
                    dlg.Title = "Select the MS Help 2 (.HxS) viewer application";
                else
                    dlg.Title = "Select the MS Help Viewer (.mshc) viewer application";

                dlg.Filter = "Executable files (*.exe)|*.exe|All Files (*.*)|*.*";
                dlg.InitialDirectory = Directory.GetCurrentDirectory();
                dlg.DefaultExt = "exe";

                // If one is selected, use that file
                if(dlg.ShowDialog() == DialogResult.OK)
                    tb.Text = dlg.FileName;
            }
        }

        /// <summary>
        /// Select a color
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnColor_Click(object sender, EventArgs e)
        {
            string name = ((Control)sender).Name;

            using(ColorDialog dlg = new ColorDialog())
            {
                dlg.AnyColor = dlg.FullOpen = true;

                if(name == "btnBuildBackground")
                    dlg.Color = lblBuildExample.BackColor;
                else
                    dlg.Color = lblBuildExample.ForeColor;

                if(dlg.ShowDialog() == DialogResult.OK)
                    if(name == "btnBuildBackground")
                        lblBuildExample.BackColor = dlg.Color;
                    else
                        lblBuildExample.ForeColor = dlg.Color;
            }
        }

        /// <summary>
        /// Select a font
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnFont_Click(object sender, EventArgs e)
        {
            string name = ((Control)sender).Name;

            using(FontDialog dlg = new FontDialog())
            {
                dlg.ShowEffects = false;

                if(name == "btnBuildFont")
                    dlg.Font = lblBuildExample.Font;
                else
                    dlg.Font = lblEditorExample.Font;

                if(dlg.ShowDialog() == DialogResult.OK)
                    if(name == "btnBuildFont")
                        lblBuildExample.Font = dlg.Font;
                    else
                        lblEditorExample.Font = dlg.Font;
            }
        }
        #endregion

        #region Content editor tab event handlers
        //=====================================================================

        /// <summary>
        /// Add a new content editor definition
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAddFile_Click(object sender, EventArgs e)
        {
            ContentFileEditor newItem = new ContentFileEditor();

            lbContentEditors.Items.Add(newItem);
            pgProps.Enabled = btnDelete.Enabled = true;
            lbContentEditors.SelectedIndex = lbContentEditors.Items.Count - 1;
            wasModified = true;
        }

        /// <summary>
        /// Delete a content editor item
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            int idx = lbContentEditors.SelectedIndex;

            if(idx == -1)
                lbContentEditors.SelectedIndex = 0;
            else
            {
                lbContentEditors.Items.RemoveAt(idx);

                if(lbContentEditors.Items.Count == 0)
                    pgProps.Enabled = btnDelete.Enabled = false;
                else
                    if(idx < lbContentEditors.Items.Count)
                        lbContentEditors.SelectedIndex = idx;
                    else
                        lbContentEditors.SelectedIndex =
                            lbContentEditors.Items.Count - 1;

                wasModified = true;
            }
        }

        /// <summary>
        /// Update the property grid with the selected item
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lbContentEditors_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(lbContentEditors.SelectedItem != null)
            {
                ContentFileEditor editor = (ContentFileEditor)lbContentEditors.SelectedItem;
                pgProps.SelectedObject = editor;
                pgProps.Enabled = true;
            }
            else
            {
                pgProps.SelectedObject = null;
                pgProps.Enabled = false;
            }

            pgProps.Refresh();
        }

        /// <summary>
        /// Refresh the list box item when a property changes
        /// </summary>
        /// <param name="s">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void pgProps_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            lbContentEditors.Refresh(lbContentEditors.SelectedIndex);
            wasModified = true;
        }
        #endregion
    }
}
