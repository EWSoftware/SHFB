//===============================================================================================================
// System  : Sandcastle Guided Installation - Sandcastle Help File Builder
// File    : SHFBVisualStudioPackagePage.cs
// Author  : Eric Woodruff
// Updated : 04/11/2016
// Compiler: Microsoft Visual C#
//
// This file contains a page used to help the user install the Sandcastle Help File Builder Visual Studio package
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/23/2011  EFW  Created the code
// 04/14/2012  EFW  Converted to use WPF
// 09/22/2012  EFW  Updated for use with the VS 2012 VSIX installer
// 10/06/2012  EFW  Merged SHFB installer pages into the main project
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;

namespace Sandcastle.Installer.InstallerPages
{
    /// <summary>
    /// This page is used to help the user install the Sandcastle Help File Builder Visual Studio package.
    /// </summary>
    public partial class SHFBVisualStudioPackagePage : BasePage
    {
        #region Private data members
        //=====================================================================

        private SortedDictionary<string, string> vsixInstallerLocations;
        private List<string> packagePaths;

        private string installerName, vsixInstallerPath;
        private bool searchPerformed, packageInstalled, installerExecuted;
        private Version frameworkVersion;
        private Guid packageGuid;
        #endregion

        #region Properties
        //=====================================================================

        /// <inheritdoc />
        public override string PageTitle
        {
            get { return "SHFB Visual Studio Package"; }
        }

        /// <inheritdoc />
        /// <remarks>This returns the .NET Framework version required by the Sandcastle Help File Builder
        /// installed by this release of the package.</remarks>
        public override Version RequiredFrameworkVersion
        {
            get { return frameworkVersion; }
        }

        /// <summary>
        /// This is overridden to confirm that the user wants to continue without installing the VS package.
        /// </summary>
        public override bool CanContinue
        {
            get
            {
                // Note that the package may have been detected if Visual Studio was not executed after manually
                // removing the package to let it clean up and remove the files.  In that case, the user will
                // not get prompted here.
                if(!String.IsNullOrEmpty(vsixInstallerPath) && !packageInstalled &&
                  MessageBox.Show("The SHFB Visual Studio package does not appear to be installed.  Projects " +
                    "cannot be loaded in Visual Studio without it.  If you will not be using Visual Studio " +
                    "with the help file builder, you can safely skip this step.  If you will, you should " +
                    "install it before proceeding.\r\n\r\nDo you want to proceed without it?", this.PageTitle,
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
                {
                    return false;
                }

                return base.CanContinue;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public SHFBVisualStudioPackagePage()
        {
            InitializeComponent();

            imgSpinner.Visibility = lblPleaseWait.Visibility = Visibility.Collapsed;

            vsixInstallerLocations = new SortedDictionary<string, string>();
            packagePaths = new List<string>();
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XElement configuration)
        {
            if(configuration.Attribute("frameworkVersion") == null)
                throw new InvalidOperationException("A frameworkVersion attribute value is required");

            if(configuration.Attribute("packageGuid") == null)
                throw new InvalidOperationException("A packageGuid attribute value is required");

            if(configuration.Attribute("installerName") == null)
                throw new InvalidOperationException("An installer attribute value is required");

            frameworkVersion = new Version(configuration.Attribute("frameworkVersion").Value);
            packageGuid = new Guid(configuration.Attribute("packageGuid").Value);
            installerName = configuration.Attribute("installerName").Value;

            // Load the supported versions
            foreach(var vs in configuration.Elements("visualStudio"))
            {
                lstVersions.ListItems.Add(new ListItem(new Paragraph(new Run(vs.Attribute("name").Value))));
                vsixInstallerLocations.Add(vs.Attribute("version").Value, vs.Attribute("location").Value);
            }

            if(lstVersions.ListItems.Count == 0)
                throw new InvalidOperationException("At least on visualStudio element must be defined");

            base.Initialize(configuration);
        }

        /// <summary>
        /// This is overridden to figure out if the Sandcastle Help File Builder is installed
        /// </summary>
        public override void ShowPage()
        {
            Paragraph para;
            string basePath, packagePath, installerPath;

            if(searchPerformed)
                return;

            btnInstallPackage.Visibility = Visibility.Collapsed;
            packageInstalled = false;
            secResults.Blocks.Clear();
            vsixInstallerPath = null;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                // Get the path to the VSIX installer.  It will be in one of the Visual Studio folders and we
                // need to find latest one as it will install the package in the latest version as well as any
                // earlier versions (true so far through VS2015 anyway).  The configuration elements should be
                // in version order from earliest to most recent.
                foreach(var kvp in vsixInstallerLocations)
                {
                    installerPath = Path.GetFullPath(Path.Combine(
                        Environment.ExpandEnvironmentVariables(kvp.Value), "VSIXInstaller.exe"));

                    if(File.Exists(installerPath))
                        vsixInstallerPath = installerPath;
                }

                if(vsixInstallerPath == null)
                {
                    pnlControls.Visibility = Visibility.Collapsed;

                    secResults.Blocks.Add(new Paragraph(new Bold(
                        new Run("Unable to locate the VSIX package installer.")) { FontSize = 13 }));

                    para = new Paragraph();
                    para.Inlines.Add(new Run("This could be because a supported version Visual Studio is not " +
                        "installed on this PC.  The extension package cannot be installed.  If you will not be " +
                        "using Visual Studio to create help projects, you can safely skip this step."));
                    secResults.Blocks.Add(para);

                    return;
                }

                // Version 2015.7.25.0 and earlier were installed for the current user only
                basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                foreach(var kvp in vsixInstallerLocations)
                {
                    packagePath = Path.Combine(basePath, @"Microsoft\VisualStudio\" + kvp.Key + @"\Extensions");

                    if(Directory.Exists(packagePath))
                    {
                        // This is used to suppress prompting if the package appears to be installed and the
                        // user clicks Next to continue.  VS2012 and later VSIX installers puts the package in
                        // a randomly named folder so we'll have to search for it.
                        packagePath = Directory.EnumerateFiles(packagePath, "SandcastleBuilder.Utils.dll",
                            SearchOption.AllDirectories).FirstOrDefault();

                        if(packagePath != null)
                        {
                            packagePaths.Add(Path.GetDirectoryName(packagePath));
                            packageInstalled = true;
                        }
                    }
                }

                // Versions after 2015.7.25.0 are installed for all users
                basePath = Environment.GetFolderPath(Environment.Is64BitProcess ?
                    Environment.SpecialFolder.ProgramFilesX86 : Environment.SpecialFolder.ProgramFiles);

                foreach(var kvp in vsixInstallerLocations)
                {
                    packagePath = Path.Combine(basePath, "Microsoft Visual Studio " + kvp.Key,
                        @"Common7\IDE\Extensions");

                    if(Directory.Exists(packagePath))
                    {
                        packagePath = Directory.EnumerateFiles(packagePath, "SandcastleBuilder.Utils.dll",
                            SearchOption.AllDirectories).FirstOrDefault();

                        if(packagePath != null)
                        {
                            packagePaths.Add(Path.GetDirectoryName(packagePath));
                            packageInstalled = true;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                imgSpinner.Visibility = lblPleaseWait.Visibility = Visibility.Collapsed;

                para = new Paragraph(new Bold(new Run("An error occurred while searching for Visual Studio:")));

                para.Foreground = new SolidColorBrush(Colors.Red);
                para.Inlines.AddRange(new Inline[] { new LineBreak(), new LineBreak(), new Run(ex.Message) });

                secResults.Blocks.Add(para);
                return;
            }
            finally
            {
                Mouse.OverrideCursor = null;
                searchPerformed = true;
            }

            // If the package looks like it is installed and we ran the installer, signal success.  If not,
            // let the installer run again.  After removal, the actual package files will not go away until
            // Visual Studio runs again and cleans out the removed packages.
            if(packageInstalled && installerExecuted)
            {
                pnlControls.Visibility = Visibility.Collapsed;

                secResults.Blocks.Add(new Paragraph(new Bold(
                    new Run("The package was installed successfully")) { FontSize = 13 }));
                para = new Paragraph();
                para.Inlines.AddRange(new Inline[] {
                    new Run("Please restart any open instances of Visual Studio so that they detect the new " +
                        "extension package.  Click the "),
                    new Bold(new Run("Next")), new Run(" button below to continue.") });
                secResults.Blocks.Add(para);

                return;
            }

            btnInstallPackage.Visibility = Visibility.Visible;
            btnInstallPackage.IsEnabled = true;

            para = new Paragraph();
            para.Inlines.AddRange(new Inline[] {
                    new Run("Please click the "),
                    new Bold(new Run("Install Package")),
                    new Run(" button below to install the latest release of the extension package.  If a " +
                        "prior version is installed, it will be removed before installing the latest release.  " +
                        "Once it has completed, you will need to restart any open instances of Visual Studio " +
                        "so that they can detect and register the new extension package.") });
            secResults.Blocks.Add(para);
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Install the Sandcastle Help File Builder package and show the results
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnInstallPackage_Click(object sender, EventArgs e)
        {
            searchPerformed = installerExecuted = false;
            btnInstallPackage.IsEnabled = false;

            // Try uninstalling any prior version before installing the latest release.
            bool launched = Utility.RunInstaller(vsixInstallerPath, "/q /u:" + packageGuid,
                (uninstallExitCode) =>
                {
                    // An exit code of 0 (success), 1002 (not installed), or 2003 (not found) is okay
                    if(uninstallExitCode != 0 && uninstallExitCode != 1002 && uninstallExitCode != 2003)
                    {
                        imgSpinner.Visibility = lblPleaseWait.Visibility = Visibility.Collapsed;
                        MessageBox.Show("Unexpected exit code returned from VSIX installer trying to " +
                            "uninstall any prior version of the package: " +
                            uninstallExitCode.ToString(CultureInfo.InvariantCulture), this.PageTitle,
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // If it was uninstalled and it was the current version, purge the folder.  The files are not
                    // physically removed until Visual Studio runs again.  Normally, that isn't a problem but
                    // when reinstalling the same version, the VSIX installer tends to do odd things like marking
                    // just one of the dependency assemblies for removal.  As such, we play it safe and remove
                    // the files so that the VSIX installer doesn't get confused.
                    if(uninstallExitCode == 0 && packagePaths.Count != 0)
                        foreach(string packagePath in packagePaths)
                            foreach(string f in Directory.GetFiles(packagePath, "*.*", SearchOption.AllDirectories))
                            {
                                try
                                {
                                    File.Delete(f);
                                }
                                catch(UnauthorizedAccessException)
                                {
                                    MessageBox.Show("Unable to remove prior version.  Is Visual Studio running?",
                                        this.PageTitle, MessageBoxButton.OK, MessageBoxImage.Error);

                                    imgSpinner.Visibility = lblPleaseWait.Visibility = Visibility.Collapsed;
                                    return;
                                }
                            }

                    // Now install the latest release
                    launched = Utility.RunInstaller(vsixInstallerPath, "/q \"" + Path.Combine(
                      Utility.InstallResourcesPath, installerName) + "\"",
                        (installExitCode) =>
                        {
                            // If we get a zero exit code, it was successfully installed
                            if(installExitCode == 0)
                                installerExecuted = true;
                            else
                                MessageBox.Show("Unexpected exit code returned from VSIX installer trying to " +
                                    "install the package: " + installExitCode.ToString(CultureInfo.InvariantCulture),
                                    this.PageTitle, MessageBoxButton.OK, MessageBoxImage.Error);

                            imgSpinner.Visibility = lblPleaseWait.Visibility = Visibility.Collapsed;
                            this.ShowPage();
                        },
                        (ex) =>
                        {
                            imgSpinner.Visibility = lblPleaseWait.Visibility = Visibility.Collapsed;
                            MessageBox.Show("Unable to execute installer: " + ex.Message, this.PageTitle,
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                },
                (ex) =>
                {
                    imgSpinner.Visibility = lblPleaseWait.Visibility = Visibility.Collapsed;
                    MessageBox.Show("Unable to execute installer: " + ex.Message, this.PageTitle,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });

            if(launched)
                imgSpinner.Visibility = lblPleaseWait.Visibility = Visibility.Visible;
        }
        #endregion
    }
}
