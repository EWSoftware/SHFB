//=============================================================================
// System  : Sandcastle Guided Installation
// File    : SandcastlePage.cs
// Author  : Eric Woodruff
// Updated : 04/15/2012
// Compiler: Microsoft Visual C#
//
// This file contains a page used to help the user install Sandcastle
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice and
// all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  02/06/2011  EFW  Created the code
// 1.1.0.0  03/23/2012  EFW  Converted to use WPF
//=============================================================================

using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;

namespace Sandcastle.Installer.InstallerPages
{
    /// <summary>
    /// This page is used to help the user install Sandcastle
    /// </summary>
    public partial class SandcastlePage : BasePage
    {
        #region Private data members
        //=====================================================================

        private string sandcastleFolder, installerName;
        private bool searchPerformed, suggestReboot, mustUninstall;
        private Version frameworkVersion, sandcastleVersion;

        private static Regex rePath = new Regex(@"[A-Z]:\\.[^;]+\\Sandcastle\\ProductionTools\\",
            RegexOptions.IgnoreCase);
        #endregion

        #region Properties
        //=====================================================================

        /// <inheritdoc />
        public override string PageTitle
        {
            get { return "Microsoft Sandcastle Tools"; }
        }

        /// <inheritdoc />
        /// <remarks>This returns the .NET Framework version required by the Sandcastle tools installed
        /// by this release of the package.</remarks>
        public override Version RequiredFrameworkVersion
        {
            get { return frameworkVersion; }
        }

        /// <summary>
        /// This is overridden to prevent continuing until the Sandcastle tools are installed
        /// </summary>
        public override bool CanContinue
        {
            get
            {
                if(String.IsNullOrEmpty(sandcastleFolder))
                {
                    MessageBox.Show("The Sandcastle tools must be installed in order to install the " +
                        "remainder of the tools in this package.  Follow the instructions on this " +
                        "page to install them.", this.PageTitle, MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                    return false;
                }

                return base.CanContinue;
            }
        }

        /// <summary>
        /// This is overridden to return true if environment variables were fixed or the installer was
        /// executed.
        /// </summary>
        public override bool SuggestReboot
        {
            get { return suggestReboot; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public SandcastlePage()
        {
            InitializeComponent();

            imgSpinner.Visibility = lblPleaseWait.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Check to see if the PATH and DXROOT environment variables are valid
        /// </summary>
        /// <returns>True if they are, false if they are not</returns>
        private static bool CheckForValidEnvironmentVariables()
        {
            string variable;

            // See if a user version of the DXROOT environment variable exists
            variable = Environment.GetEnvironmentVariable("DXROOT", EnvironmentVariableTarget.User);

            if(!String.IsNullOrEmpty(variable))
                return false;

            // See if there is a Sandcastle reference in a user version of the PATH environment variable
            variable = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);

            if(!String.IsNullOrEmpty(variable) && rePath.IsMatch(variable))
                return false;

            return true;
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XElement configuration)
        {
            if(configuration.Attribute("frameworkVersion") == null)
                throw new InvalidOperationException("A frameworkVersion attribute value is required");

            if(configuration.Attribute("sandcastleVersion") == null)
                throw new InvalidOperationException("A sandcastleVersion attribute value is required");

            if(configuration.Attribute("installerName") == null)
                throw new InvalidOperationException("An installer attribute value is required");

            frameworkVersion = new Version(configuration.Attribute("frameworkVersion").Value);
            sandcastleVersion = new Version(configuration.Attribute("sandcastleVersion").Value);
            installerName = configuration.Attribute("installerName").Value;

            base.Initialize(configuration);
        }

        /// <summary>
        /// This is overridden to figure out if Sandcastle is installed and if there are issues with an old
        /// SDK version of it.
        /// </summary>
        public override void ShowPage()
        {
            Paragraph para;
            FileVersionInfo fvi;
            Version installedVersion;
            bool noDxRoot = false;

            if(searchPerformed)
                return;

            btnInstallSandcastle.Visibility = btnFixEnvironment.Visibility = Visibility.Collapsed;
            secResults.Blocks.Clear();

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                searchPerformed = false;

                if(!CheckForValidEnvironmentVariables())
                {
                    btnFixEnvironment.Visibility = Visibility.Visible;

                    // Append the fix instructions to the flow document
                    secResults.Blocks.AppendFrom("Sandcastle.Installer.Resources.FixSandcastle.xaml");
                    return;
                }

                // DXROOT will exist as a system environment variable if it is installed correctly
                sandcastleFolder = Environment.GetEnvironmentVariable("DXROOT", EnvironmentVariableTarget.Machine);

                // If we fixed the environment variables, it may not be there but the old version may still exist
                // so look for the expected folder.
                if(String.IsNullOrEmpty(sandcastleFolder))
                {
                    noDxRoot = true;
                    sandcastleFolder = Environment.GetEnvironmentVariable("ProgramFiles(x86)");

                    if(String.IsNullOrEmpty(sandcastleFolder))
                        sandcastleFolder = Environment.GetEnvironmentVariable("ProgramFiles");

                    if(!String.IsNullOrEmpty(sandcastleFolder))
                    {
                        sandcastleFolder = Path.Combine(sandcastleFolder, "Sandcastle");

                        if(!Directory.Exists(sandcastleFolder))
                            sandcastleFolder = null;
                    }
                }

                if(!String.IsNullOrEmpty(sandcastleFolder))
                {
                    // If there is an installed version, make sure it is older than what we expect
                    if(!Directory.Exists(sandcastleFolder) || !File.Exists(Path.Combine(sandcastleFolder,
                      @"ProductionTools\MRefBuilder.exe")))
                        installedVersion = new Version(0, 0, 0, 0);
                    else
                    {
                        fvi = FileVersionInfo.GetVersionInfo(Path.Combine(sandcastleFolder,
                            @"ProductionTools\MRefBuilder.exe"));

                        installedVersion = new Version(fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart,
                            fvi.FilePrivatePart);
                    }

                    if(installedVersion < sandcastleVersion)
                    {
                        sandcastleFolder = null;

                        // Versions prior to 2.7.0.0 must be uninstalled manually due to the use of
                        // a new MSI installer.
                        if(installedVersion.Major != 0 && (installedVersion.Major < 2 || (
                          installedVersion.Major == 2 && installedVersion.Minor < 7)))
                            mustUninstall = true;
                    }
                    else
                    {
                        // If the version is greater, we can't go on as this package is out of date
                        if(installedVersion > sandcastleVersion)
                        {
                            sandcastleFolder = null;
                            pnlControls.Visibility = Visibility.Collapsed;

                            secResults.Blocks.Add(new Paragraph(new Bold(
                                new Run("Newer Version Detected")) { FontSize = 13 }));

                            para = new Paragraph();
                            para.Inlines.Add(new Run("It has been determined that a newer version of the " +
                                "Sandcastle tools are installed on this system.  You will need to " +
                                "download a newer version of this package that is compatible with this " +
                                "more recent release of Sandcastle."));
                            secResults.Blocks.Add(para);

                            return;
                        }
                    }

                    if(noDxRoot)
                        sandcastleFolder = null;
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                imgSpinner.Visibility = lblPleaseWait.Visibility = Visibility.Collapsed;

                para = new Paragraph(new Bold(new Run(
                    "An error occurred while searching for the Sandcastle tools:")));

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

            if(!String.IsNullOrEmpty(sandcastleFolder))
            {
                pnlControls.Visibility = Visibility.Collapsed;

                secResults.Blocks.Add(new Paragraph(new Bold(
                    new Run("Sandcastle Tools Found")) { FontSize = 13 }));

                para = new Paragraph();
                para.Inlines.AddRange(new Inline[] {
                    new Run("It has been determined that the Sandcastle tools are installed on this " +
                        "system (Location: "),
                    new Italic(new Run(sandcastleFolder)),
                    new Run(").  No further action is required in this step.") });
                secResults.Blocks.Add(para);

                para = new Paragraph();
                para.Inlines.AddRange(new Inline[] {
                    new Run("Click the "), new Bold(new Run("Next")), new Run(" button to continue.") });
                secResults.Blocks.Add(para);

                return;
            }

            btnInstallSandcastle.Visibility = Visibility.Visible;
            btnInstallSandcastle.IsEnabled = true;

            if(mustUninstall)
            {
                para = new Paragraph
                {
                    Margin = new Thickness(20),
                    Padding = new Thickness(10),
                    Background = new SolidColorBrush(Color.FromRgb(255, 255, 204)),
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Colors.Black)
                };

                para.Inlines.AddRange(new Inline[] {
                    new Run("An older version of Sandcastle was found.  The updated installer will not " +
                        "detect and remove versions prior to version 2.7.0.0.  As such, the old version " +
                        "must be removed manually using the Add/Remove Programs or Programs and Features " +
                        "option in the Control Panel."),
                    new LineBreak(),
                    new LineBreak(),
                    new Bold(new Run("NOTE: ")),
                    new Run("You should also check for the old Sandcastle folder after uninstalling and " +
                        "remove it if found to clear out any old patch files that may interfer with the " +
                        "new release.") });
                secResults.Blocks.Add(para);

                para = new Paragraph();
                para.Inlines.Add(
                    new Run("Once you have removed the prior version, return here and click the "));
            }
            else
            {
                para = new Paragraph();
                para.Inlines.Add(
                    new Run("The Sandcastle tools could not be found on this system.  Please click the "));
            }

            para.Inlines.AddRange(new Inline[] {
                    new Bold(new Run("Install Sandcastle")),
                    new Run(" button below.  A separate installer will be launched to perform the " +
                        "installation.  Once it has completed, return to this application to continue " +
                        "installing the remainder of the tools.  You may need to reboot when done so " +
                        "that changes to the system environment variables take effect.") });
            secResults.Blocks.Add(para);
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Fix up issues with the environment variables used by Sandcastle
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnFixEnvironment_Click(object sender, RoutedEventArgs e)
        {
            string variable;

            try
            {
                searchPerformed = false;
                suggestReboot = true;

                // See if a user version of the DXROOT environment variable exists
                variable = Environment.GetEnvironmentVariable("DXROOT", EnvironmentVariableTarget.User);

                // If there is, delete it
                if(!String.IsNullOrEmpty(variable))
                    Environment.SetEnvironmentVariable("DXROOT", null, EnvironmentVariableTarget.User);

                // See if there is a Sandcastle reference in a user version of the PATH environment variable
                variable = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);

                if(!String.IsNullOrEmpty(variable))
                {
                    Match m = rePath.Match(variable);

                    // If there is, remove it
                    if(m.Success)
                    {
                        variable = rePath.Replace(variable, String.Empty).Trim();

                        if(variable.Length == 0)
                            variable = null;

                        Environment.SetEnvironmentVariable("PATH", variable, EnvironmentVariableTarget.User);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to automatically fix environment variables:\r\n\r\n" + ex.Message,
                    this.PageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            this.ShowPage();
        }

        /// <summary>
        /// Install Sandcastle and show the results
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnInstallSandcastle_Click(object sender, RoutedEventArgs e)
        {
            // If the old version needed uninstalling manually, check again to be sure it's gone.
            if(mustUninstall)
            {
                searchPerformed = mustUninstall = false;
                this.ShowPage();

                if(mustUninstall)
                {
                    MessageBox.Show("The prior version of Sandcastle must be manually uninstalled first.  " +
                        "Read this page for details.", this.PageTitle, MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                    return;
                }
            }

            searchPerformed = suggestReboot = false;
            btnInstallSandcastle.IsEnabled = false;

            bool launched = Utility.RunInstaller(Path.Combine(Utility.InstallResourcesPath, installerName), null,
                (exitCode) =>
                {
                    suggestReboot = true;
                    imgSpinner.Visibility = lblPleaseWait.Visibility = Visibility.Collapsed;
                    this.ShowPage();
                },
                (ex) =>
                {
                    imgSpinner.Visibility = lblPleaseWait.Visibility = Visibility.Collapsed;
                    MessageBox.Show("Unable to execute installer: " + ex.ExceptionMessage(), this.PageTitle,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });

            if(launched)
                imgSpinner.Visibility = lblPleaseWait.Visibility = Visibility.Visible;
        }
        #endregion
    }
}
