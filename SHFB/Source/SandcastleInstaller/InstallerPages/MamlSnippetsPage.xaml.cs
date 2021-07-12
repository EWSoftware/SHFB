﻿//===============================================================================================================
// System  : Sandcastle Guided Installation
// File    : MamlSnippetsPage.cs
// Author  : Eric Woodruff
// Updated : 04/21/2021
//
// This file contains a page used to help the user install the Sandcastle MAML snippet files for use with Visual
// Studio.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/25/2012  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: Xml

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml.Linq;

namespace Sandcastle.Installer.InstallerPages
{
    /// <summary>
    /// This page is used to help the user install the Sandcastle MAML snippet files for use with Visual Studio
    /// </summary>
    public partial class MamlSnippetsPage : BasePage
    {
        #region Private data members
        //=====================================================================

        private string sandcastleSnippetsFolder, baseSnippetsFolder;

        private List<string> supportedVersions;

        #endregion

        #region Properties
        //=====================================================================

        /// <inheritdoc />
        public override string PageTitle => "MAML Snippet Files";

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public MamlSnippetsPage()
        {
            InitializeComponent();

            baseSnippetsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to see if it is safe to install the MAML snippets in the given version of Visual
        /// Studio.
        /// </summary>
        /// <param name="vsVersionName">The Visual Studio version name</param>
        /// <param name="vsPath">The path to the snippet cache for the version of Visual Studio</param>
        /// <returns>True if it is safe, false if it is not safe to install the snippets in the given
        /// version of Visual Studio.</returns>
        private bool CheckForSafeInstallation(string vsVersionName, string vsPath)
        {
            Paragraph para = new Paragraph();
            secResults.Blocks.Add(para);

            para.Inlines.AddRange(new Inline[] { new Bold(new Run(vsVersionName)), new Run(" - ") });

            // Check for the folder
            if(String.IsNullOrWhiteSpace(vsPath) || !Directory.Exists(vsPath))
            {
                para.Inlines.Add(new Run("Unable to locate snippet cache for this version of Visual Studio."));
                return false;
            }

            para.Inlines.Add(new Run("The snippets can be installed for this version of Visual Studio."));
            return true;
        }

        /// <summary>
        /// This is used to install the MAML snippets in the specified Visual Studio local snippets cache
        /// </summary>
        /// <param name="vsPath">The path to which the snippets are installed</param>
        /// <returns>True if successful, false if not</returns>
        private bool InstallMamlSnippets(string vsPath)
        {
            string destination;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                // Copy the files from the Sandcastle folder to the local snippets cache
                foreach(string source in Directory.EnumerateFiles(sandcastleSnippetsFolder, "*.*",
                  SearchOption.AllDirectories))
                {
                    destination = Path.Combine(vsPath, @"Code Snippets\XML\My Xml Snippets",
                        source.Substring(sandcastleSnippetsFolder.Length));

                    if(!Directory.Exists(Path.GetDirectoryName(destination)))
                        Directory.CreateDirectory(Path.GetDirectoryName(destination));

                    File.Copy(source, destination, true);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to install the Sandcastle MAML snippets:\r\n\r\n" + ex.Message,
                    this.PageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

            return true;
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XElement configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if(configuration.Attribute("supportedVersions") == null)
                throw new InvalidOperationException("A supportedVersions attribute value is required");

            supportedVersions = configuration.Attribute("supportedVersions").Value.Split(new[] { ',', ' ' },
                StringSplitOptions.RemoveEmptyEntries).ToList();

            base.Initialize(configuration);
        }

        /// <inheritdoc />
        public override void ShowPage()
        {
            pnlVersions.Children.Clear();
            secResults.Blocks.Clear();

            // Load the possible versions and see if we can safely install them
            foreach(var vs in VisualStudioInstance.AllInstances)
            {
                if(supportedVersions.Any(v => vs.Version.StartsWith(v, StringComparison.Ordinal)))
                {
                    string location = Path.Combine(baseSnippetsFolder, vs.UserTemplatesBaseFolder);

                    CheckBox cb = new CheckBox
                    {
                        Margin = new Thickness(20, 5, 0, 0),
                        Content = vs.DisplayName,
                        IsEnabled = this.CheckForSafeInstallation(vs.DisplayName, location)
                    };

                    cb.IsChecked = cb.IsEnabled;
                    cb.Tag = location;

                    pnlVersions.Children.Add(cb);
                }
            }

            if(pnlVersions.Children.Count == 0)
            {
                btnInstallSnippets.IsEnabled = btnChangeFolder.IsEnabled = false;
                pnlVersions.Children.Add(new Label { Content = "No usable versions of Visual Studio were found" });
            }

            // SHFBROOT will exist as a system environment variable if it is installed correctly
            sandcastleSnippetsFolder = Environment.GetEnvironmentVariable("SHFBROOT", EnvironmentVariableTarget.Machine);

            // It may not be there if we just installed it so look for the folder manually
            if(String.IsNullOrEmpty(sandcastleSnippetsFolder))
            {
                sandcastleSnippetsFolder = Path.Combine(Environment.GetFolderPath(Environment.Is64BitProcess ?
                    Environment.SpecialFolder.ProgramFilesX86 : Environment.SpecialFolder.ProgramFiles),
                    @"EWSoftware\Sandcastle Help File Builder");

                if(!Directory.Exists(sandcastleSnippetsFolder))
                    sandcastleSnippetsFolder = null;
            }

            if(!String.IsNullOrEmpty(sandcastleSnippetsFolder))
                sandcastleSnippetsFolder = Path.Combine(sandcastleSnippetsFolder, @"Snippets\");

            if(String.IsNullOrEmpty(sandcastleSnippetsFolder) || !Directory.Exists(sandcastleSnippetsFolder))
            {
                secResults.Blocks.Clear();
                secResults.Blocks.Add(new Paragraph(
                    new Bold(new Run("Unable to locate the Sandcastle installation folder"))));
                pnlVersions.IsEnabled = btnInstallSnippets.IsEnabled = false;
            }

            base.ShowPage();
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Install the Sandcastle MAML snippet files
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnInstallSnippets_Click(object sender, EventArgs e)
        {
            var folders = pnlVersions.Children.OfType<CheckBox>().Where(cb => cb.IsChecked.Value).Select(
                cb => (string)cb.Tag).ToList();

            if(folders.Count == 0)
            {
                MessageBox.Show("Select at least one version of Visual Studio into which the MAML snippets " +
                    "will be installed", this.PageTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if(MessageBox.Show("This will install the Sandcastle MAML snippets into the local snippets cache " +
              "for the selected versions of Visual Studio.  Click OK to continue or CANCEL to stop.",
              this.PageTitle, MessageBoxButton.OKCancel, MessageBoxImage.Information,
              MessageBoxResult.Cancel) == MessageBoxResult.Cancel)
                return;

            foreach(string f in folders)
                if(!this.InstallMamlSnippets(f))
                    return;

            MessageBox.Show("The Sandcastle MAML snippets have been installed successfully.", this.PageTitle,
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Change the location of the base folder for the snippet caches
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnChangeFolder_Click(object sender, RoutedEventArgs e)
        {
            using(System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                dlg.Description = "Select your My Documents folder";
                dlg.SelectedPath = baseSnippetsFolder;

                // If selected, set the new folder
                if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    baseSnippetsFolder = dlg.SelectedPath;
                    this.ShowPage();
                }
            }
        }
        #endregion
    }
}
