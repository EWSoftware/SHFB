//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : HelpFilePropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 12/31/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Help File category properties.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.0  03/27/2011  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Microsoft.Build.Evaluation;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the Help Format category project properties
    /// </summary>
    [Guid("714E7D74-A81C-403B-91E9-D052F0438FC6")]
    public partial class HelpFilePropertiesPageControl : BasePropertyPage
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public HelpFilePropertiesPageControl()
        {
            InitializeComponent();

            this.Title = "Help File";
            this.HelpKeyword = "1b2dff59-92cc-4578-b261-f3849f30c26c";

            cboContentPlacement.DisplayMember = cboNamingMethod.DisplayMember = "Value";
            cboContentPlacement.ValueMember = cboNamingMethod.ValueMember = "Key";

            cboContentPlacement.DataSource = (new Dictionary<string, string> {
                { ContentPlacement.AboveNamespaces.ToString(), "Above namespaces" },
                { ContentPlacement.BelowNamespaces.ToString(), "Below namespaces" } }).ToList();

            cboNamingMethod.DataSource = (new Dictionary<string, string> {
                { NamingMethod.Guid.ToString(), "GUID" },
                { NamingMethod.MemberName.ToString(), "Member name" },
                { NamingMethod.HashedMemberName.ToString(), "Hashed member name" } }).ToList();

            cboPresentationStyle.Items.AddRange(PresentationStyleTypeConverter.StandardValues.ToArray());
            cboPresentationStyle.SelectedItem = PresentationStyleTypeConverter.DefaultStyle;

            cboSdkLinkTarget.Items.AddRange(Enum.GetNames(typeof(SdkLinkTarget)).OfType<Object>().ToArray());
            cboSdkLinkTarget.SelectedItem = SdkLinkTarget.Blank.ToString();

            cboLanguage.DisplayMember = "EnglishName";
            cboLanguage.Items.AddRange(LanguageResourceConverter.StandardValues.ToArray());
            cboLanguage.SelectedItem = LanguageResourceConverter.StandardValues.First(
                c => c.Name.Equals("en-US", StringComparison.OrdinalIgnoreCase));

            cblSyntaxFilters.Items.AddRange(BuildComponentManager.SyntaxFilters.Select(
                f => f.Value.Id).OrderBy(f => f).ToArray());
        }
        #endregion

        #region Helper methods
        //=====================================================================

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
                cboLanguage.Items.Add(ci);
            else
                ci = LanguageResourceConverter.StandardValues.First(
                    c => c.Name.Equals("en-US", StringComparison.OrdinalIgnoreCase));

            return ci;
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override bool IsValid
        {
            get
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

                if(txtHelpTitle.Text.Length == 0)
                    txtHelpTitle.Text = "A Sandcastle Documented Class Library";

                if(txtHtmlHelpName.Text.Length == 0)
                    txtHtmlHelpName.Text = "Documentation";

                return true;
            }
        }

        /// <inheritdoc />
        protected override bool IsEscapedProperty(string propertyName)
        {
            switch(propertyName)
            {
                case "HelpTitle":
                case "HtmlHelpName":
                case "CopyrightHref":
                case "CopyrightText":
                case "FeedbackEMailAddress":
                case "FeedbackEMailLinkText":
                case "HeaderText":
                case "FooterText":
                case "PresentationStyle":
                case "RootNamespaceTitle":
                    return true;

                default:
                    return false;
            }
        }

        /// <inheritdoc />
        protected override bool BindControlValue(Control control)
        {
            ProjectProperty projProp;
            List<string> allFilters;
            int idx;

            if(this.ProjectMgr == null)
                return false;

            // Add the project's selected language to the list if it is not there
            if(control.Name == "cboLanguage")
            {
                projProp = this.ProjectMgr.BuildProject.GetProperty("Language");

                if(projProp != null)
                    cboLanguage.SelectedItem = this.AddLanguage(projProp.UnevaluatedValue ?? "en-US");

                return true;
            }

            // Set the selected syntax filters
            if(control.Name == "cblSyntaxFilters")
            {
                for(idx = 0; idx < cblSyntaxFilters.Items.Count; idx++)
                    cblSyntaxFilters.SetItemChecked(idx, false);

                projProp = this.ProjectMgr.BuildProject.GetProperty("SyntaxFilters");

                if(projProp != null)
                    allFilters = BuildComponentManager.SyntaxFiltersFrom(projProp.UnevaluatedValue).Select(f => f.Id).ToList();
                else
                    allFilters = BuildComponentManager.SyntaxFiltersFrom("Standard").Select(f => f.Id).ToList();

                foreach(string s in allFilters)
                {
                    idx = cblSyntaxFilters.FindStringExact(s);

                    if(idx != -1)
                        cblSyntaxFilters.SetItemChecked(idx, true);
                }

                return true;
            }

            return false;
        }

        /// <inheritdoc />
        protected override bool StoreControlValue(Control control)
        {
            if(this.ProjectMgr == null)
                return false;

            // Set the selected syntax filters value
            if(control.Name == "cblSyntaxFilters")
            {
                this.ProjectMgr.SetProjectProperty("SyntaxFilters",
                    SyntaxFilterTypeConverter.ToRecognizedFilterIds(String.Join(", ",
                        cblSyntaxFilters.CheckedItems.Cast<string>().ToArray())));

                return true;
            }

            return false;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Validate the entered language and add it to the list if necessary
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cboLanguage_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(cboLanguage.SelectedIndex == -1)
            {
                cboLanguage.SelectedItem = this.AddLanguage(cboLanguage.Text);
                this.IsDirty = true;
            }
        }
        #endregion
    }
}
