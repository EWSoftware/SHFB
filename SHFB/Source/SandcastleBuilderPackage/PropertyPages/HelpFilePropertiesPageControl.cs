//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : HelpFilePropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 05/19/2016
// Note    : Copyright 2011-2016, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Help File category properties.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/27/2011  EFW  Created the code
// 10/27/2012  EFW  Added support for the new presentation style definition file
// 12/13/2013  EFW  Added support for namespace grouping
// 12/21/2013  EFW  Updated to use MEF for loading the syntax generators
// 01/05/2014  EFW  Updated to use MEF for loading the presentation styles
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
        private ComponentCache componentCache;
        private string messageBoxTitle;

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
                txtHelpFileVersion.Text = txtHelpFileVersion.Text.Trim();

                if(txtHelpTitle.Text.Length == 0)
                    txtHelpTitle.Text = "A Sandcastle Documented Class Library";

                if(txtHtmlHelpName.Text.Length == 0)
                    txtHtmlHelpName.Text = "Documentation";

                if(txtHelpFileVersion.Text.Length == 0)
                    txtHelpFileVersion.Text = "1.0.0.0";

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
                case "HelpFileVersion":
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
            SandcastleProject currentProject = null;
            ProjectProperty projProp = null;
            string[] searchFolders;

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

            // Load the presentation styles and syntax filters
            if(control.Name == "cblSyntaxFilters")
            {
#if !STANDALONEGUI
                currentProject = ((SandcastleBuilderProjectNode)this.ProjectMgr).SandcastleProject;
#else
                currentProject = this.CurrentProject;
#endif
                searchFolders = new[] { currentProject.ComponentPath, Path.GetDirectoryName(currentProject.Filename) };

                if(componentCache == null)
                {
                    componentCache = ComponentCache.CreateComponentCache(currentProject.Filename);

                    componentCache.ComponentContainerLoaded += componentCache_ComponentContainerLoaded;
                    componentCache.ComponentContainerLoadFailed += componentCache_ComponentContainerLoadFailed;
                    componentCache.ComponentContainerReset += componentCache_ComponentContainerReset;
                }

                if(componentCache.LoadComponentContainer(searchFolders))
                    this.componentCache_ComponentContainerLoaded(this, EventArgs.Empty);
                else
                    this.componentCache_ComponentContainerReset(this, EventArgs.Empty);

                return true;
            }

            if(control.Name == "cboPresentationStyle")
                return true;

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
                this.ProjectMgr.SetProjectProperty("SyntaxFilters",
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

            if(presentationStyles == null)
                return;

            // If the presentation style wasn't recognized, it may not have matched by case
            if(cboPresentationStyle.SelectedIndex == -1)
            {
#if !STANDALONEGUI
                string prop = this.ProjectMgr.GetProjectProperty("PresentationStyle");
#else
                string prop = this.CurrentProject.MSBuildProject.GetPropertyValue("PresentationStyle");
#endif
                // Try to get it based on the current setting.  If still not found, use the default.
                pss = presentationStyles.FirstOrDefault(s => s.Id.Equals(prop, StringComparison.OrdinalIgnoreCase));

                if(pss == null)
                {
                    pss = presentationStyles.FirstOrDefault(s => s.Id.Equals(Constants.DefaultPresentationStyle,
                        StringComparison.OrdinalIgnoreCase));

                    if(pss == null)
                        cboPresentationStyle.SelectedIndex = 0;
                    else
                        cboPresentationStyle.SelectedValue = pss.Id;
                }
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
            if(syntaxGenerators != null && cblSyntaxFilters.SelectedIndex != -1)
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

        /// <summary>
        /// This is called when the component cache is reset prior to loading it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void componentCache_ComponentContainerReset(object sender, EventArgs e)
        {
            if(this.IsDisposed)
                return;

            // May already be binding so preserve the original state
            bool isBinding = this.IsBinding;

            try
            {
                this.IsBinding = true;

                syntaxGenerators = null;
                presentationStyles = null;

                cboPresentationStyle.Enabled = cblSyntaxFilters.Enabled = false;

                cblSyntaxFilters.Items.Clear();
                cblSyntaxFilters.Items.Add("Loading...");

                cboPresentationStyle.DataSource = null;
                cboPresentationStyle.Items.Clear();
                cboPresentationStyle.Items.Add("Loading...");
                cboPresentationStyle.SelectedIndex = 0;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            finally
            {
                this.IsBinding = isBinding;
            }
        }

        /// <summary>
        /// This is called when the component cache load operation fails
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void componentCache_ComponentContainerLoadFailed(object sender, EventArgs e)
        {
            if(this.IsDisposed)
                return;

            // May already be binding so preserve the original state
            bool isBinding = this.IsBinding;

            try
            {
                this.IsBinding = true;

                cblSyntaxFilters.Enabled = false;

                cblSyntaxFilters.Items.Clear();
                cblSyntaxFilters.Items.Add("Unable to load syntax filters");
                epNotes.SetError(cblSyntaxFilters, componentCache.LastError.ToString());

                cboPresentationStyle.Items.Clear();
                cboPresentationStyle.Items.Add("Unable to load presentation styles");
                cboPresentationStyle.SelectedIndex = 0;
                epNotes.SetError(cboPresentationStyle, componentCache.LastError.ToString());
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            finally
            {
                this.IsBinding = isBinding;
            }
        }

        /// <summary>
        /// This is called when the component cache has finished being loaded and is available for use
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void componentCache_ComponentContainerLoaded(object sender, EventArgs e)
        {
            ProjectProperty syntaxFilterProp = null, presentationStyleProp = null;
            List<string> allFilters;
            int idx;

            if(this.IsDisposed)
                return;

            // May already be binding so preserve the original state
            bool isBinding = this.IsBinding;

            HashSet<string> generatorIds = new HashSet<string>(), presentationStyleIds = new HashSet<string>();

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                this.IsBinding = true;

                cboPresentationStyle.Enabled = cblSyntaxFilters.Enabled = true;
                cboPresentationStyle.DataSource = null;

                cblSyntaxFilters.Items.Clear();
                cboPresentationStyle.Items.Clear();

                syntaxGenerators = new List<ISyntaxGeneratorMetadata>();
                presentationStyles = new List<IPresentationStyleMetadata>();

                var generators = componentCache.ComponentContainer.GetExports<ISyntaxGeneratorFactory,
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

                var styles = componentCache.ComponentContainer.GetExports<PresentationStyleSettings,
                    IPresentationStyleMetadata>().Select(g => g.Metadata).ToList();

                // As above for duplicates
                foreach(var style in styles)
                    if(!presentationStyleIds.Contains(style.Id))
                    {
                        presentationStyles.Add(style);
                        presentationStyleIds.Add(style.Id);
                    }

                cblSyntaxFilters.Items.AddRange(syntaxGenerators.Select(f => f.Id).OrderBy(f => f).ToArray());

                cboPresentationStyle.DisplayMember = "Title";
                cboPresentationStyle.ValueMember = "Id";
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

                if(cblSyntaxFilters.Items.Count != 0)
                    cblSyntaxFilters.SelectedIndex = 0;
                else
                    MessageBox.Show("No valid syntax generators found", messageBoxTitle, MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                if(cboPresentationStyle.Items.Count == 0)
                    MessageBox.Show("No valid presentation styles found", messageBoxTitle, MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

#if !STANDALONEGUI
                if(this.ProjectMgr != null)
                {
                    presentationStyleProp = this.ProjectMgr.BuildProject.GetProperty("PresentationStyle");
                    syntaxFilterProp = this.ProjectMgr.BuildProject.GetProperty("SyntaxFilters");
                }
#else
                if(this.CurrentProject != null)
                {
                    presentationStyleProp = this.CurrentProject.MSBuildProject.GetProperty("PresentationStyle");
                    syntaxFilterProp = this.CurrentProject.MSBuildProject.GetProperty("SyntaxFilters");
                }
#endif
                if(presentationStyleProp != null)
                {
                    var match = cboPresentationStyle.Items.Cast<IPresentationStyleMetadata>().FirstOrDefault(p =>
                        p.Id == presentationStyleProp.UnevaluatedValue);

                    if(match != null)
                        cboPresentationStyle.SelectedValue = presentationStyleProp.UnevaluatedValue;
                }

                if(syntaxFilterProp != null)
                {
                    allFilters = ComponentUtilities.SyntaxFiltersFrom(syntaxGenerators,
                        syntaxFilterProp.UnevaluatedValue).Select(f => f.Id).ToList();
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
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                MessageBox.Show("Unexpected error loading syntax generators and presentation styles: " +
                    ex.Message, messageBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.IsBinding = isBinding;
                Cursor.Current = Cursors.Default;
            }
        }
        #endregion
    }
}
