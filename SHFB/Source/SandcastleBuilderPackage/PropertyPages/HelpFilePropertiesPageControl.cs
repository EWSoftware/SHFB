﻿//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : HelpFilePropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 05/01/2014
// Note    : Copyright 2011-2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Help File category properties.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.3.0  03/27/2011  EFW  Created the code
// 1.9.6.0  10/27/2012  EFW  Added support for the new presentation style definition file
// -------  12/13/2013  EFW  Added support for namespace grouping
//          12/21/2013  EFW  Updated to use MEF for loading the syntax generators
//          01/05/2014  EFW  Updated to use MEF for loading the presentation styles
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Microsoft.Build.Evaluation;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler.SyntaxGenerator;
using Sandcastle.Core.PresentationStyle;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.Design;

#if !STANDALONEGUI
using SandcastleBuilder.Package.Nodes;
using SandcastleBuilder.Package.Properties;
using _PersistStorageType = Microsoft.VisualStudio.Shell.Interop._PersistStorageType;
#endif

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the Help Format category project properties
    /// </summary>
    [Guid("714E7D74-A81C-403B-91E9-D052F0438FC6")]
    public partial class HelpFilePropertiesPageControl : BasePropertyPage
    {
        #region Private data members
        //=====================================================================

        private List<ISyntaxGeneratorMetadata> syntaxGenerators;
        private List<IPresentationStyleMetadata> presentationStyles;
        private string messageBoxTitle, lastProjectName;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public HelpFilePropertiesPageControl()
        {
            InitializeComponent();

#if !STANDALONEGUI
            messageBoxTitle = Resources.PackageTitle;
#else
            messageBoxTitle = Constants.AppName;
#endif
            // Set the maximum size to prevent an unnecessary vertical scrollbar
            this.MaximumSize = new System.Drawing.Size(2048, this.Height);

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

            // The presentation style data source is loaded when needed
            cboPresentationStyle.DisplayMember = "Title";
            cboPresentationStyle.ValueMember = "Id";

            cboSdkLinkTarget.Items.AddRange(Enum.GetNames(typeof(SdkLinkTarget)).OfType<Object>().ToArray());
            cboSdkLinkTarget.SelectedItem = SdkLinkTarget.Blank.ToString();

            cboLanguage.DisplayMember = "EnglishName";
            cboLanguage.Items.AddRange(LanguageResourceConverter.StandardValues.ToArray());
            cboLanguage.SelectedItem = LanguageResourceConverter.StandardValues.First(
                c => c.Name.Equals("en-US", StringComparison.OrdinalIgnoreCase));
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Try to load information about all available syntax generators so that they can be selected for use
        /// in the project.
        /// </summary>
        /// <returns>True on success, false on failure or if no project is loaded</returns>
        private void LoadAvailableSyntaxGeneratorsAndPresentationStyles()
        {
            SandcastleProject currentProject = null;
            HashSet<string> generatorIds = new HashSet<string>(), presentationStyleIds = new HashSet<string>();
            string[] searchFolders;

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                syntaxGenerators = new List<ISyntaxGeneratorMetadata>();
                presentationStyles = new List<IPresentationStyleMetadata>();

#if !STANDALONEGUI
                if(base.ProjectMgr != null)
                    currentProject = ((SandcastleBuilderProjectNode)base.ProjectMgr).SandcastleProject;
#else
                currentProject = base.CurrentProject;
#endif
                lastProjectName = currentProject == null ? null : currentProject.Filename;

                if(currentProject != null)
                    searchFolders = new[] { currentProject.ComponentPath, Path.GetDirectoryName(currentProject.Filename) };
                else
                    searchFolders = new string[] { };

                using(var componentContainer = ComponentUtilities.CreateComponentContainer(searchFolders))
                {
                    cblSyntaxFilters.Items.Clear();

                    var generators = componentContainer.GetExports<ISyntaxGeneratorFactory,
                        ISyntaxGeneratorMetadata>().Select(g => g.Metadata).ToList();

                    // There may be duplicate generator IDs across the assemblies found.  See
                    // BuildComponentManger.GetComponentContainer() for the folder search precedence.  Only the
                    // first component for a unique ID will be used.
                    foreach(var generator in generators)
                        if(!generatorIds.Contains(generator.Id))
                        {
                            syntaxGenerators.Add(generator);
                            generatorIds.Add(generator.Id);
                        }

                    cboPresentationStyle.DataSource = null;

                    var styles = componentContainer.GetExports<PresentationStyleSettings,
                        IPresentationStyleMetadata>().Select(g => g.Metadata).ToList();

                    // As above for duplicates
                    foreach(var style in styles)
                        if(!presentationStyleIds.Contains(style.Id))
                        {
                            presentationStyles.Add(style);
                            presentationStyleIds.Add(style.Id);
                        }
                }

                cblSyntaxFilters.Items.AddRange(syntaxGenerators.Select(f => f.Id).OrderBy(f => f).ToArray());
                cboPresentationStyle.DataSource = presentationStyles.OrderBy(s => s.IsDeprecated ? 1 : 0).ThenBy(
                    s => s.Id).ToList();
                cboPresentationStyle.SelectedValue = Constants.DefaultPresentationStyle;

                // Resize the syntax filter columns to the widest entry
                int width, maxWidth = 0;

                foreach(string s in cblSyntaxFilters.Items)
                {
                    width = TextRenderer.MeasureText(s, this.Font).Width;

                    if(width > maxWidth)
                        maxWidth = width;
                }

                cblSyntaxFilters.ColumnWidth = maxWidth + 20;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                MessageBox.Show("Unexpected error loading syntax generators and presentation styles: " +
                    ex.Message, messageBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            if(cblSyntaxFilters.Items.Count != 0)
                cblSyntaxFilters.SelectedIndex = 0;
            else
                MessageBox.Show("No valid syntax generators found", messageBoxTitle, MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

            if(cboPresentationStyle.Items.Count == 0)
                MessageBox.Show("No valid presentation styles found", messageBoxTitle, MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
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

                if(udcMaximumGroupParts.Text.Trim().Length == 0)
                    udcMaximumGroupParts.Value = 2;

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
            ProjectProperty projProp = null;
            List<string> allFilters;
            int idx;

#if !STANDALONEGUI
            if(this.ProjectMgr == null)
                return false;
#else
            if(this.CurrentProject == null)
                return false;
#endif
            // Add the project's selected language to the list if it is not there
            if(control.Name == "cboLanguage")
            {
#if !STANDALONEGUI
                projProp = this.ProjectMgr.BuildProject.GetProperty("Language");
#else
                projProp = this.CurrentProject.MSBuildProject.GetProperty("Language");
#endif
                if(projProp != null)
                    cboLanguage.SelectedItem = this.AddLanguage(projProp.UnevaluatedValue ?? "en-US");
                else
                    cboLanguage.SelectedItem = LanguageResourceConverter.StandardValues.First(
                        c => c.Name.Equals("en-US", StringComparison.OrdinalIgnoreCase));

                return true;
            }

            // Set the selected syntax filters
            if(control.Name == "cblSyntaxFilters")
            {
                for(idx = 0; idx < cblSyntaxFilters.Items.Count; idx++)
                    cblSyntaxFilters.SetItemChecked(idx, false);

#if !STANDALONEGUI
                SandcastleProject currentProject = null;

                if(base.ProjectMgr != null)
                    currentProject = ((SandcastleBuilderProjectNode)base.ProjectMgr).SandcastleProject;

                if(syntaxGenerators == null || presentationStyles == null || currentProject == null ||
                  currentProject.Filename != lastProjectName)
                    this.LoadAvailableSyntaxGeneratorsAndPresentationStyles();

                if(base.ProjectMgr != null)
                    projProp = base.ProjectMgr.BuildProject.GetProperty("SyntaxFilters");
#else
                if(syntaxGenerators == null || presentationStyles == null || base.CurrentProject == null ||
                  base.CurrentProject.Filename != lastProjectName)
                    this.LoadAvailableSyntaxGeneratorsAndPresentationStyles();

                if(base.CurrentProject != null)
                    projProp = base.CurrentProject.MSBuildProject.GetProperty("SyntaxFilters");
#endif

                if(projProp != null)
                {
                    allFilters = ComponentUtilities.SyntaxFiltersFrom(syntaxGenerators,
                        projProp.UnevaluatedValue).Select(f => f.Id).ToList();
                }
                else
                    allFilters = ComponentUtilities.SyntaxFiltersFrom(syntaxGenerators, "Standard").Select(
                        f => f.Id).ToList();

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
#if !STANDALONEGUI
            if(this.ProjectMgr == null)
                return false;
#else
            if(this.CurrentProject == null)
                return false;
#endif
            // Set the selected syntax filters value
            if(control.Name == "cblSyntaxFilters")
            {
#if !STANDALONEGUI
                this.ProjectMgr.SetProjectProperty("SyntaxFilters", _PersistStorageType.PST_PROJECT_FILE,
                    ComponentUtilities.ToRecognizedSyntaxFilterIds(syntaxGenerators, String.Join(", ",
                        cblSyntaxFilters.CheckedItems.Cast<string>().ToArray())));
#else
                this.CurrentProject.MSBuildProject.SetProperty("SyntaxFilters",
                    ComponentUtilities.ToRecognizedSyntaxFilterIds(syntaxGenerators, String.Join(", ",
                        cblSyntaxFilters.CheckedItems.Cast<string>().ToArray())));
#endif
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

        /// <summary>
        /// Update the info provider text when the presentation style changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cboPresentationStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            IPresentationStyleMetadata pss;

            // If the presentation style wasn't recognized, it may not have matched by case
            if(cboPresentationStyle.SelectedIndex == -1)
            {
#if !STANDALONEGUI
                string prop = base.ProjectMgr.GetProjectProperty("PresentationStyle", _PersistStorageType.PST_PROJECT_FILE);
#else
                string prop = base.CurrentProject.MSBuildProject.GetPropertyValue("PresentationStyle");
#endif
                // Try to get it based on the current setting.  If still not found, use the first one.
                pss = presentationStyles.FirstOrDefault(s => s.Id.Equals(prop, StringComparison.OrdinalIgnoreCase));

                if(pss == null)
                    cboPresentationStyle.SelectedIndex = 0;
                else
                    cboPresentationStyle.SelectedValue = pss.Id;

                return;
            }

            pss = (IPresentationStyleMetadata)cboPresentationStyle.Items[cboPresentationStyle.SelectedIndex];

            epNotes.SetError(cboPresentationStyle, String.Format(CultureInfo.InvariantCulture,
                "{0}\r\n\r\nVersion {1}\r\n{2}", pss.Description, pss.Version, pss.Copyright));
        }

        /// <summary>
        /// Update the info provider text when the syntax filter selection changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void cblSyntaxFilters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cblSyntaxFilters.SelectedIndex != -1)
            {
                var generator = syntaxGenerators.FirstOrDefault(sf => sf.Id.Equals(cblSyntaxFilters.SelectedItem));

                if(generator == null)
                    epNotes.SetError(cblSyntaxFilters, String.Empty);
                else
                    epNotes.SetError(cblSyntaxFilters, String.Format(CultureInfo.InvariantCulture,
                        "{0}\r\n\r\nVersion {1}\r\n{2}", generator.Description, generator.Version,
                        generator.Copyright));
            }
        }
        #endregion
    }
}
