//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : HelpFileProperitesPageContent.xaml.cs
// Author  : Eric Woodruff
// Updated : 09/14/2024
// Note    : Copyright 2017-2024, Eric Woodruff, All rights reserved
//
// This user control is used to edit the Help File category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
//===============================================================================================================
// 11/10/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

using Sandcastle.Core;
using SandcastleBuilder.Utils;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This user control is used to edit the Help File category properties
    /// </summary>
    public partial class HelpFilePropertiesPageContent : UserControl
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public HelpFilePropertiesPageContent()
        {
            InitializeComponent();

            cboContentPlacement.ItemsSource = (new Dictionary<string, string> {
                { ContentPlacement.AboveNamespaces.ToString(), "Above namespaces" },
                { ContentPlacement.BelowNamespaces.ToString(), "Below namespaces" } }).ToList();

            cboNamingMethod.ItemsSource = (new Dictionary<string, string> {
                { NamingMethod.Guid.ToString(), "GUID" },
                { NamingMethod.MemberName.ToString(), "Member name" },
                { NamingMethod.HashedMemberName.ToString(), "Hashed member name" }
            }).ToList();

            cboSdkLinkTarget.ItemsSource = Enum.GetNames(typeof(SdkLinkTarget));
            cboSdkLinkTarget.SelectedValue = SdkLinkTarget.Blank.ToString();

            var supportedLanguages = ComponentUtilities.SupportedLanguages.ToList();

            cboLanguage.ItemsSource = supportedLanguages;
            cboLanguage.SelectedValue = supportedLanguages.First(c => c.Name.Equals("en-US",
                StringComparison.OrdinalIgnoreCase));

            cboContentPlacement.SelectedIndex = cboNamingMethod.SelectedIndex = 0;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to fix up property values before storing them in the project
        /// </summary>
        public void FixUpPropertyValues()
        {
            txtCopyrightHref.Text = txtCopyrightHref.Text.Trim();
            txtCopyrightText.Text = txtCopyrightText.Text.Trim();
            txtFeedbackEMailAddress.Text = txtFeedbackEMailAddress.Text.Trim();
            txtFeedbackEMailLinkText.Text = txtFeedbackEMailLinkText.Text.Trim();
            txtFooterText.Text = txtFooterText.Text.Trim();
            txtHeaderText.Text = txtHeaderText.Text.Trim();
            txtHelpTitle.Text = txtHelpTitle.Text.Trim();
            txtHtmlHelpName.Text = txtHtmlHelpName.Text.Trim();
            txtRootNamespaceTitle.Text = txtRootNamespaceTitle.Text.Trim();
            txtHelpFileVersion.Text = txtHelpFileVersion.Text.Trim();

            if(txtHelpTitle.Text.Length == 0)
                txtHelpTitle.Text = "A Sandcastle Documented Class Library";

            if(txtHtmlHelpName.Text.Length == 0)
                txtHtmlHelpName.Text = "Documentation";

            if(txtHelpFileVersion.Text.Length == 0)
                txtHelpFileVersion.Text = "1.0.0.0";

            if(udcMaximumGroupParts.Text.Trim().Length == 0)
                udcMaximumGroupParts.Value = 2;
        }

        /// <summary>
        /// This is used to set the selected language
        /// </summary>
        /// <param name="language">The language to use as the current selection</param>
        public void SetLanguage(string language)
        {
            cboLanguage.SelectedValue = this.AddLanguage(language ?? "en-US").Name;
        }

        /// <summary>
        /// This adds a language to the language combo box if possible
        /// </summary>
        /// <param name="language">The language name or English name to add</param>
        /// <returns>The <see cref="CultureInfo"/> instance added or the default English-US culture if the
        /// specified language could not be found.</returns>
        private CultureInfo AddLanguage(string language)
        {
            CultureInfo ci = null;

            language = (language ?? String.Empty).Trim();

            if(language.Length != 0)
            {
                // If it's already there, ignore the request
                ci = cboLanguage.Items.OfType<CultureInfo>().FirstOrDefault(
                    c => c.Name.Equals(language, StringComparison.OrdinalIgnoreCase) ||
                         c.EnglishName.Equals(language, StringComparison.OrdinalIgnoreCase));

                if(ci != null)
                    return ci;

                ci = CultureInfo.GetCultures(CultureTypes.AllCultures).FirstOrDefault(
                    c => c.Name.Equals(language, StringComparison.OrdinalIgnoreCase) ||
                         c.EnglishName.Equals(language, StringComparison.OrdinalIgnoreCase));
            }

            if(ci != null)
            {
                var languages = (List<CultureInfo>)cboLanguage.ItemsSource;
                languages.Add(ci);
            }
            else
                ci = ComponentUtilities.SupportedLanguages.First(c => c.Name.Equals("en-US",
                    StringComparison.OrdinalIgnoreCase));

            return ci;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// When losing the keyboard focus, if the entered language is not recognized, add it to the list
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cboLanguage_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if(cboLanguage.SelectedIndex == -1)
                cboLanguage.SelectedValue = this.AddLanguage(cboLanguage.Text).Name;
        }
        #endregion
    }
}
