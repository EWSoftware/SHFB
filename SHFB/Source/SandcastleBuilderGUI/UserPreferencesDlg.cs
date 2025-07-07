//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : UserPreferencesDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/05/2025
// Note    : Copyright 2007-2025, Eric Woodruff, All rights reserved
//
// This form is used to allow the user to modify help file builder preferences unrelated to projects
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/23/2007  EFW  Created the code
// 07/02/2007  EFW  Added content file editor collection option
// 05/14/2013  EFW  Added spell checking options
// 05/03/2015  EFW  Removed support for the MS Help 2 file format
//===============================================================================================================

// Ignore Spelling: exe

using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Sandcastle.Core;

using SandcastleBuilder.Gui.ContentEditors;
using SandcastleBuilder.Gui.Properties;
using SandcastleBuilder.Gui.Spelling;

namespace SandcastleBuilder.Gui
{
    /// <summary>
    /// This form is used to allow the user to modify help file builder preferences unrelated to projects
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

            cboDefaultLanguage.Items.AddRange(
                [.. SpellCheckerConfiguration.AvailableDictionaryLanguages.OrderBy(c => c.Name)]);

            txtMSHelpViewerPath.Text = Settings.Default.MSHelpViewerPath;
            udcASPNetDevServerPort.Value = Settings.Default.ASPNETDevServerPort;
            chkPerUserProjectState.Checked = Settings.Default.PerUserProjectState;
            cboBeforeBuildAction.SelectedIndex = (int)Settings.Default.BeforeBuild;
            chkVerboseLogging.Checked = Settings.Default.VerboseLogging;
            chkOpenHelp.Checked = Settings.Default.OpenHelpAfterBuild;
            chkShowLineNumbers.Checked = Settings.Default.ShowLineNumbers;
            chkEnterMatching.Checked = Settings.Default.EnterMatching;
            lblBuildExample.BackColor = Settings.Default.BuildOutputBackground;
            lblBuildExample.ForeColor = Settings.Default.BuildOutputForeground;
            lblBuildExample.Font = Settings.Default.BuildOutputFont;
            lblEditorExample.Font = Settings.Default.TextEditorFont;

            if(cboDefaultLanguage.Items.Contains(SpellCheckerConfiguration.DefaultLanguage))
                cboDefaultLanguage.SelectedItem = SpellCheckerConfiguration.DefaultLanguage;

            chkIgnoreWordsWithDigits.Checked = SpellCheckerConfiguration.IgnoreWordsWithDigits;
            chkIgnoreAllUppercase.Checked = SpellCheckerConfiguration.IgnoreWordsInAllUppercase;
            chkIgnoreFilenamesAndEMail.Checked = SpellCheckerConfiguration.IgnoreFilenamesAndEMailAddresses;
            chkIgnoreXmlInText.Checked = SpellCheckerConfiguration.IgnoreXmlElementsInText;
            chkTreatUnderscoresAsSeparators.Checked = SpellCheckerConfiguration.TreatUnderscoreAsSeparator;

            lbIgnoredXmlElements.Items.AddRange([.. SpellCheckerConfiguration.IgnoredXmlElements]);
            lbSpellCheckedAttributes.Items.AddRange([.. SpellCheckerConfiguration.SpellCheckedXmlAttributes]);

            lbContentEditors.DisplayMember = lbContentEditors.ValueMember = "EditorDescription";

            if(Settings.Default.ContentFileEditors.Count == 0)
                pgProps.Enabled = btnDelete.Enabled = false;
            else
            {
                // Binding the collection to the list box caused some odd problems with the property grid so
                // we'll add clones of the items to the list box directly.
                foreach(ContentFileEditor e in Settings.Default.ContentFileEditors)
                    lbContentEditors.Items.Add(e.Clone());

                lbContentEditors.SelectedIndex = 0;
            }
        }
        #endregion

        #region General event handlers
        //=====================================================================

        /// <summary>
        /// Validate the information and save the results if necessary when closing
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void UserPreferencesDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            ContentFileEditorCollection currentEditors;

            if(this.DialogResult == DialogResult.Cancel)
                return;

            string filePath = txtMSHelpViewerPath.Text.Trim();

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

                Settings.Default.MSHelpViewerPath = txtMSHelpViewerPath.Text;
                Settings.Default.ASPNETDevServerPort = (int)udcASPNetDevServerPort.Value;
                Settings.Default.PerUserProjectState = chkPerUserProjectState.Checked;
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
                    ContentFileEditorCollection.GlobalEditors.AddRange(currentEditors);
                }

                Settings.Default.Save();

                SpellCheckerConfiguration.DefaultLanguage = (CultureInfo)cboDefaultLanguage.SelectedItem;

                SpellCheckerConfiguration.IgnoreWordsWithDigits = chkIgnoreWordsWithDigits.Checked;
                SpellCheckerConfiguration.IgnoreWordsInAllUppercase = chkIgnoreAllUppercase.Checked;
                SpellCheckerConfiguration.IgnoreFilenamesAndEMailAddresses = chkIgnoreFilenamesAndEMail.Checked;
                SpellCheckerConfiguration.IgnoreXmlElementsInText = chkIgnoreXmlInText.Checked;
                SpellCheckerConfiguration.TreatUnderscoreAsSeparator = chkTreatUnderscoresAsSeparators.Checked;

                SpellCheckerConfiguration.SetIgnoredXmlElements(lbIgnoredXmlElements.Items.OfType<string>());
                SpellCheckerConfiguration.SetSpellCheckedXmlAttributes(lbSpellCheckedAttributes.Items.OfType<string>());

                if(!SpellCheckerConfiguration.SaveConfiguration())
                    MessageBox.Show("Unable to save spell checking configuration", Constants.AppName,
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
            using OpenFileDialog dlg = new();
            
            dlg.Title = "Select the MS Help Viewer (.mshc) viewer application";
            dlg.Filter = "Executable files (*.exe)|*.exe|All Files (*.*)|*.*";
            dlg.InitialDirectory = Directory.GetCurrentDirectory();
            dlg.DefaultExt = "exe";

            // If one is selected, use that file
            if(dlg.ShowDialog() == DialogResult.OK)
                txtMSHelpViewerPath.Text = dlg.FileName;
        }

        /// <summary>
        /// Select a color
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnColor_Click(object sender, EventArgs e)
        {
            string name = ((Control)sender).Name;
            using ColorDialog dlg = new();
            
            dlg.AnyColor = dlg.FullOpen = true;

            if(name == "btnBuildBackground")
                dlg.Color = lblBuildExample.BackColor;
            else
                dlg.Color = lblBuildExample.ForeColor;

            if(dlg.ShowDialog() == DialogResult.OK)
            {
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

            using FontDialog dlg = new();
            
            dlg.ShowEffects = false;

            if(name == "btnBuildFont")
                dlg.Font = lblBuildExample.Font;
            else
                dlg.Font = lblEditorExample.Font;

            if(dlg.ShowDialog() == DialogResult.OK)
            {
                if(name == "btnBuildFont")
                    lblBuildExample.Font = dlg.Font;
                else
                    lblEditorExample.Font = dlg.Font;
            }
        }
        #endregion

        #region Spell checker tab event handlers
        //=====================================================================

        /// <summary>
        /// Load the user dictionary file when the selected language changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cboDefaultLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filename = Path.Combine(SpellCheckerConfiguration.ConfigurationFilePath,
                ((CultureInfo)cboDefaultLanguage.SelectedItem).Name + "_Ignored.dic");

            lbUserDictionary.Items.Clear();

            if(File.Exists(filename))
            {
                try
                {
                    lbUserDictionary.Items.AddRange(File.ReadAllLines(filename));
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Unable to load user dictionary.  Reason: " + ex.Message, Constants.AppName,
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        /// <summary>
        /// Remove the selected word from the user dictionary
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnRemoveWord_Click(object sender, EventArgs e)
        {
            int idx = lbUserDictionary.SelectedIndex;

            if(idx != -1)
                lbUserDictionary.Items.RemoveAt(idx);

            if(lbUserDictionary.Items.Count != 0)
            {
                if(idx < 0)
                    idx = 0;
                else
                    if(idx >= lbUserDictionary.Items.Count)
                        idx = lbUserDictionary.Items.Count - 1;

                lbUserDictionary.SelectedIndex = idx;
            }

            CultureInfo culture = (CultureInfo)cboDefaultLanguage.SelectedItem;
            string filename = Path.Combine(SpellCheckerConfiguration.ConfigurationFilePath,
                culture.Name + "_Ignored.dic");

            try
            {
                File.WriteAllLines(filename, lbUserDictionary.Items.OfType<string>());
                GlobalDictionary.LoadIgnoredWordsFile(culture);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to save user dictionary.  Reason: " + ex.Message, Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Add a new ignored XML element name to the list
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAddIgnored_Click(object sender, EventArgs e)
        {
            int idx;

            txtIgnoredElement.Text = txtIgnoredElement.Text.Trim();

            if(txtIgnoredElement.Text.Length != 0)
            {
                idx = lbIgnoredXmlElements.Items.IndexOf(txtIgnoredElement.Text);

                if(idx == -1)
                    idx = lbIgnoredXmlElements.Items.Add(txtIgnoredElement.Text);

                if(idx != -1)
                    lbIgnoredXmlElements.SelectedIndex = idx;

                txtIgnoredElement.Text = null;
            }
        }

        /// <summary>
        /// Remove the selected element from the list of ignored elements
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnRemoveIgnored_Click(object sender, EventArgs e)
        {
            int idx = lbIgnoredXmlElements.SelectedIndex;

            if(idx != -1)
                lbIgnoredXmlElements.Items.RemoveAt(idx);

            if(lbIgnoredXmlElements.Items.Count != 0)
            {
                if(idx < 0)
                    idx = 0;
                else
                    if(idx >= lbIgnoredXmlElements.Items.Count)
                        idx = lbIgnoredXmlElements.Items.Count - 1;

                lbIgnoredXmlElements.SelectedIndex = idx;
            }
        }

        /// <summary>
        /// Reset the ignored XML elements to the default list
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDefaultIgnored_Click(object sender, EventArgs e)
        {
            lbIgnoredXmlElements.Items.Clear();
            lbIgnoredXmlElements.Items.AddRange([.. SpellCheckerConfiguration.DefaultIgnoredXmlElements]);
        }

        /// <summary>
        /// Add a new spell checked attribute name to the list
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnAddAttribute_Click(object sender, EventArgs e)
        {
            int idx;

            txtAttributeName.Text = txtAttributeName.Text.Trim();

            if(txtAttributeName.Text.Length != 0)
            {
                idx = lbSpellCheckedAttributes.Items.IndexOf(txtAttributeName.Text);

                if(idx == -1)
                    idx = lbSpellCheckedAttributes.Items.Add(txtAttributeName.Text);

                if(idx != -1)
                    lbSpellCheckedAttributes.SelectedIndex = idx;

                txtAttributeName.Text = null;
            }
        }

        /// <summary>
        /// Remove the selected attribute from the list of spell checked attributes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnRemoveAttribute_Click(object sender, EventArgs e)
        {
            int idx = lbSpellCheckedAttributes.SelectedIndex;

            if(idx != -1)
                lbSpellCheckedAttributes.Items.RemoveAt(idx);

            if(lbSpellCheckedAttributes.Items.Count != 0)
            {
                if(idx < 0)
                    idx = 0;
                else
                    if(idx >= lbSpellCheckedAttributes.Items.Count)
                        idx = lbSpellCheckedAttributes.Items.Count - 1;

                lbSpellCheckedAttributes.SelectedIndex = idx;
            }
        }

        /// <summary>
        /// Reset the spell checked attributes to the default list
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnDefaultAttributes_Click(object sender, EventArgs e)
        {
            lbSpellCheckedAttributes.Items.Clear();
            lbSpellCheckedAttributes.Items.AddRange([.. SpellCheckerConfiguration.DefaultSpellCheckedAttributes]);
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
            ContentFileEditor newItem = new();

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
                {
                    if(idx < lbContentEditors.Items.Count)
                        lbContentEditors.SelectedIndex = idx;
                    else
                        lbContentEditors.SelectedIndex = lbContentEditors.Items.Count - 1;
                }

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
