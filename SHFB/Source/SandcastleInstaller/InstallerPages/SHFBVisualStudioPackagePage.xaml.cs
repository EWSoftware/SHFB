//===============================================================================================================
// System  : Sandcastle Guided Installation - Sandcastle Help File Builder
// File    : SHFBVisualStudioPackagePage.cs
// Author  : Eric Woodruff
// Updated : 04/06/2017
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
using System.Threading.Tasks;
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
        #region Visual Studio Installer package options
        //=====================================================================

        /// <summary>
        /// This is used to define VSIX package options
        /// </summary>
        private class VisualStudioInstallerPackage
        {
            private HashSet<string> installedLocations = new HashSet<string>();

            /// <summary>
            /// The VSIX package name
            /// </summary>
            public string PackageName { get; set; }

            /// <summary>
            /// The package description
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// This is used to define the path to the VSIX installer executable
            /// </summary>
            /// <remarks>If null, an installer for this version of the package could not be found</remarks>
            public string VsixInstallerPath { get; set; }

            /// <summary>
            /// This returns a unique set of locations in which this package is installed
            /// </summary>
            public ICollection<string> InstalledLocations
            {
                get { return installedLocations; }
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private XElement pageConfiguration;
        private Task initializationTask;
        private List<VisualStudioInstallerPackage> vsixPackages;

        private bool searchPerformed, installerExecuted;
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
                if(!vsixPackages.Any(p => p.InstalledLocations.Count != 0) &&
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

            vsixPackages = new List<VisualStudioInstallerPackage>();
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XElement configuration)
        {
            pageConfiguration = configuration;
            initializationTask = Task.Run(() => this.InitializeInternal(pageConfiguration));
        }

        public void InitializeInternal(XElement configuration)
        {
            if(configuration.Attribute("frameworkVersion") == null)
                throw new InvalidOperationException("A frameworkVersion attribute value is required");

            if(configuration.Attribute("packageGuid") == null)
                throw new InvalidOperationException("A packageGuid attribute value is required");

            frameworkVersion = new Version(configuration.Attribute("frameworkVersion").Value);
            packageGuid = new Guid(configuration.Attribute("packageGuid").Value);

            // Load the Visual Studio package versions
            foreach(var package in configuration.Elements("package"))
            {
                var vsix = new VisualStudioInstallerPackage
                {
                    PackageName = package.Attribute("name").Value,
                    Description = package.Attribute("description").Value
                };

                vsixPackages.Add(vsix);

                foreach(var vs in package.Elements("visualStudio"))
                {
                    string versionNumber = vs.Attribute("version").Value, basePath = vs.Attribute("basePath").Value,
                        editionPaths = (string)vs.Attribute("editionPaths");

                    if(!Environment.Is64BitProcess && basePath.IndexOf("(x86)%", StringComparison.Ordinal) != -1)
                        basePath = basePath.Replace("(x86)%", "%");

                    // VS2015 and earlier don't install side-by-side
                    if(editionPaths == null)
                    {
                        string vsixPath = Path.Combine(Environment.ExpandEnvironmentVariables(basePath),
                            "VSIXInstaller.exe");

                        // Use the latest one found
                        if(File.Exists(vsixPath))
                            vsix.VsixInstallerPath = vsixPath;

                        // Version 2015.7.25.0 and earlier were installed for the current user only
                        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            packagePath = Path.Combine(appDataPath, @"Microsoft\VisualStudio\" + versionNumber + @"\Extensions");

                        if(Directory.Exists(packagePath))
                        {
                            // This is used to suppress prompting if the package appears to be installed and the
                            // user clicks Next to continue.  VS2012 and later VSIX installers puts the package in
                            // a randomly named folder so we'll have to search for it.
                            packagePath = Directory.EnumerateFiles(packagePath, "SandcastleBuilder.Utils.dll",
                                SearchOption.AllDirectories).FirstOrDefault();

                            if(packagePath != null)
                                vsix.InstalledLocations.Add(Path.GetDirectoryName(packagePath));
                        }

                        // Versions after 2015.7.25.0 are installed for all users
                        appDataPath = Environment.GetFolderPath(Environment.Is64BitProcess ?
                            Environment.SpecialFolder.ProgramFilesX86 : Environment.SpecialFolder.ProgramFiles);
                        packagePath = Path.Combine(appDataPath, "Microsoft Visual Studio " + versionNumber,
                            @"Common7\IDE\Extensions");

                        if(Directory.Exists(packagePath))
                        {
                            packagePath = Directory.EnumerateFiles(packagePath, "SandcastleBuilder.Utils.dll",
                                SearchOption.AllDirectories).FirstOrDefault();

                            if(packagePath != null)
                                vsix.InstalledLocations.Add(Path.GetDirectoryName(packagePath));
                        }
                    }
                    else
                        foreach(string edition in editionPaths.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            // VS2017 and later can be installed side-by-side so find the first one available
                            string vsixPath = String.Format(CultureInfo.InvariantCulture,
                                Path.Combine(Environment.ExpandEnvironmentVariables(basePath),
                                "VSIXInstaller.exe"), edition);

                            // Use the latest one found.  Within an edition it doesn't matter but we still need
                            // to know about each installed edition.
                            if(File.Exists(vsixPath))
                                vsix.VsixInstallerPath = vsixPath;

                            // Version 2015.7.25.0 and earlier were installed for the current user only
                            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                packagePath = Path.Combine(appDataPath, @"Microsoft\VisualStudio\" + versionNumber + @"\Extensions");

                            if(Directory.Exists(packagePath))
                            {
                                // This is used to suppress prompting if the package appears to be installed and the
                                // user clicks Next to continue.  VS2012 and later VSIX installers puts the package in
                                // a randomly named folder so we'll have to search for it.
                                packagePath = Directory.EnumerateFiles(packagePath, "SandcastleBuilder.Utils.dll",
                                    SearchOption.AllDirectories).FirstOrDefault();

                                if(packagePath != null)
                                    vsix.InstalledLocations.Add(Path.GetDirectoryName(packagePath));
                            }

                            // Versions after 2015.7.25.0 are installed for all users
                            packagePath = Path.Combine(Path.GetDirectoryName(vsixPath), "Extensions");

                            if(Directory.Exists(packagePath))
                            {
                                packagePath = Directory.EnumerateFiles(packagePath, "SandcastleBuilder.Utils.dll",
                                    SearchOption.AllDirectories).FirstOrDefault();

                                if(packagePath != null)
                                    vsix.InstalledLocations.Add(Path.GetDirectoryName(packagePath));
                            }
                        }
                }
            }

            if(vsixPackages.Count == 0)
                throw new InvalidOperationException("At least one package element must be defined");

            base.Initialize(configuration);
        }

        /// <summary>
        /// This is overridden to figure out if the Sandcastle Help File Builder is installed
        /// </summary>
        public override void ShowPage()
        {
            Paragraph para;

            if(searchPerformed)
                return;

            btnInstallPackage.Visibility = Visibility.Collapsed;
            secResults.Blocks.Clear();
            lstVersions.ListItems.Clear();

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                initializationTask.Wait();

                foreach(var vsix in vsixPackages)
                    foreach(string v in vsix.Description.Split(';'))
                        lstVersions.ListItems.Add(new ListItem(new Paragraph(new Run(v))));

                if(!vsixPackages.Any(p => !String.IsNullOrWhiteSpace(p.VsixInstallerPath)))
                {
                    pnlControls.Visibility = Visibility.Collapsed;

                    secResults.Blocks.Add(new Paragraph(new Bold(new Run(
                        "Unable to locate the VSIX package installer.")) { FontSize = 13 }));

                    para = new Paragraph();
                    para.Inlines.Add(new Run("This could be because a supported version Visual Studio is not " +
                        "installed on this PC.  The extension package cannot be installed.  If you will not be " +
                        "using Visual Studio to create help projects, you can safely skip this step."));
                    secResults.Blocks.Add(para);

                    return;
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine(initializationTask.Exception);

                imgSpinner.Visibility = lblPleaseWait.Visibility = Visibility.Collapsed;

                para = new Paragraph(new Bold(new Run("An error occurred while searching for Visual Studio:")))
                {
                    Foreground = new SolidColorBrush(Colors.Red)
                };

                para.Inlines.AddRange(new Inline[] { new LineBreak(), new LineBreak(), new Run(
                    initializationTask.Exception.InnerException.Message) });

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
            if(vsixPackages.Any(p => p.InstalledLocations.Count != 0) && installerExecuted)
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
        private async void btnInstallPackage_Click(object sender, EventArgs e)
        {
            bool success = false;
            int exitCode;

            searchPerformed = installerExecuted = false;
            btnInstallPackage.IsEnabled = false;
            imgSpinner.Visibility = lblPleaseWait.Visibility = Visibility.Visible;

            try
            {
                await Task.Run(() =>
                {
                    // Do the uninstall first.  Packages for later versions will still see the package in
                    // older versions of Visual Studio and uninstall it if not.
                    foreach(var package in vsixPackages.Where(p => !String.IsNullOrEmpty(p.VsixInstallerPath)))
                        if(package.InstalledLocations.Count != 0)
                        {
                            // Try uninstalling any prior version before installing the latest release.
                            exitCode = Utility.RunInstaller(package.VsixInstallerPath, "/u:" + packageGuid);

                            // The VSIX installer that comes with VS2017 and later requires that all instances
                            // of Visual Studio and its subtasks be shut down before removal or installation.
                            if(exitCode == 2004)
                                throw new InvalidOperationException("Visual Studio or one of its subtasks is " +
                                    "still running.  Please shut down all instances of Visual Studio and, if " +
                                    "necessary, kill off any subtasks and try again.  You can run the VSIX " +
                                    "installer manually if necessary to find out which tasks are still running.");

                            // An exit code of 0 (success), 1002 (not installed), or 2003 (not found) is okay
                            if(exitCode != 0 && exitCode != 1002 && exitCode != 2003)
                                throw new InvalidOperationException("Unexpected exit code returned from VSIX " +
                                    "installer trying to uninstall any prior version of the package: " +
                                    exitCode.ToString(CultureInfo.InvariantCulture));

                            // If it was uninstalled and it was the current version, purge the folder.  The files
                            // are not physically removed until Visual Studio runs again.  Normally, that isn't a
                            // problem but when reinstalling the same version, the VSIX installer tends to do odd
                            // things like marking just one of the dependency assemblies for removal.  As such,
                            // we play it safe and remove the files so that the VSIX installer doesn't get
                            // confused.
                            foreach(string packagePath in package.InstalledLocations)
                                if(Directory.Exists(packagePath))
                                    Directory.Delete(packagePath, true);
                        }

                    // Now install the latest release
                    foreach(var package in vsixPackages.Where(p => !String.IsNullOrEmpty(p.VsixInstallerPath)))
                    {
                        exitCode = Utility.RunInstaller(package.VsixInstallerPath, "\"" + Path.Combine(
                            Utility.InstallResourcesPath, package.PackageName) + "\"");

                        if(exitCode == 2004)
                            throw new InvalidOperationException("Visual Studio or one of its subtasks is " +
                                "still running.  Please shut down all instances of Visual Studio and, if " +
                                "necessary, kill off any subtasks and try again.  You can run the VSIX " +
                                "installer manually if necessary to find out which tasks are still running.");

                        // If we get a zero exit code, assume it was successfully installed
                        if(exitCode == 0)
                            installerExecuted = true;
                        else
                            throw new InvalidOperationException("Unexpected exit code returned from VSIX " +
                                "installer trying to install the package: " +
                                exitCode.ToString(CultureInfo.InvariantCulture));
                    }
                });

                success = true;
            }
            catch(UnauthorizedAccessException)
            {
                MessageBox.Show("Unable to remove prior version.  Is Visual Studio running?",
                    this.PageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch(InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, this.PageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to execute installer: " + ex.Message, this.PageTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                imgSpinner.Visibility = lblPleaseWait.Visibility = Visibility.Collapsed;

                if(success)
                {
                    vsixPackages.Clear();
                    initializationTask = Task.Run(() => this.InitializeInternal(pageConfiguration));
                    this.ShowPage();
                }
                else
                    btnInstallPackage.IsEnabled = true;
            }
        }
        #endregion
    }
}
