//===============================================================================================================
// System  : Sandcastle Guided Installation - Sandcastle Help File Builder
// File    : SandcastleHelpFileBuilderPage.cs
// Author  : Eric Woodruff
// Updated : 12/26/2016
// Compiler: Microsoft Visual C#
//
// This file contains a page used to help the user install the Sandcastle Help File Builder
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/12/2011  EFW  Created the code
// 04/14/2012  EFW  Converted to use WPF
// 10/06/2012  EFW  Merged SHFB installer pages into the main project
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;

namespace Sandcastle.Installer.InstallerPages
{
    /// <summary>
    /// This page is used to help the user install the Sandcastle Help File Builder
    /// </summary>
    public partial class SandcastleHelpFileBuilderPage : BasePage
    {
        #region Private data members
        //=====================================================================

        private string shfbFolder, installerName;
        private bool searchPerformed, suggestReboot;
        private Version frameworkVersion, shfbVersion;
        #endregion

        #region Properties
        //=====================================================================

        /// <inheritdoc />
        public override string PageTitle
        {
            get { return "Sandcastle Help File Builder and Tools"; }
        }

        /// <inheritdoc />
        /// <remarks>This returns the .NET Framework version required by the Sandcastle Help File Builder
        /// installed by this release of the package.</remarks>
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
                if(String.IsNullOrEmpty(shfbFolder))
                {
                    MessageBox.Show("The Sandcastle Help File Builder and Tools must be installed in order to " +
                        "install the remainder of the tools in this package.  Follow the instructions on this " +
                        "page to install them.", this.PageTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return false;
                }

                return base.CanContinue;
            }
        }

        /// <summary>
        /// This is overridden to return true if the installer was executed.
        /// </summary>
        public override bool SuggestReboot
        {
            get { return suggestReboot; }
        }

        /// <summary>
        /// This is overridden to return completion actions that offers to open the Sandcastle and SHFB help
        /// files.
        /// </summary>
        public override IEnumerable<CompletionAction> CompletionActions
        {
            get
            {
                if(!String.IsNullOrEmpty(shfbFolder))
                {
                    yield return new CompletionAction
                    {
                        Description = "Open the Sandcastle Help File Builder help file",
                        Action = new Action(() =>
                        {
                            Utility.Open(Path.Combine(shfbFolder, @"Help\SandcastleBuilder.chm"));
                        })
                    };
                }
            }
        }

        /// <summary>
        /// This is used to retrieve the help file builder version
        /// </summary>
        public Version HelpFileBuilderVersion
        {
            get { return shfbVersion; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public SandcastleHelpFileBuilderPage()
        {
            InitializeComponent();

            imgSpinner.Visibility = lblPleaseWait.Visibility = Visibility.Collapsed;

            // Handle hyperlink clicks using the default handler
            fdDocument.AddHandler(Hyperlink.ClickEvent, new RoutedEventHandler(Utility.HyperlinkClick));
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XElement configuration)
        {
            if(configuration.Attribute("frameworkVersion") == null)
                throw new InvalidOperationException("A frameworkVersion attribute value is required");

            if(configuration.Attribute("shfbVersion") == null)
                throw new InvalidOperationException("A shfbVersion attribute value is required");

            if(configuration.Attribute("installerName") == null)
                throw new InvalidOperationException("An installer attribute value is required");

            frameworkVersion = new Version(configuration.Attribute("frameworkVersion").Value);
            shfbVersion = new Version(configuration.Attribute("shfbVersion").Value);
            installerName = configuration.Attribute("installerName").Value;

            base.Initialize(configuration);
        }

        /// <summary>
        /// This is overridden to figure out if the Sandcastle Help File Builder is installed
        /// </summary>
        public override void ShowPage()
        {
            Paragraph para;
            FileVersionInfo fvi;
            Version installedVersion;

            if(searchPerformed)
                return;

            btnInstallSHFB.Visibility = Visibility.Collapsed;
            secResults.Blocks.Clear();

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                // SHFBROOT will exist as a system environment variable if it is installed correctly
                shfbFolder = Environment.GetEnvironmentVariable("SHFBROOT", EnvironmentVariableTarget.Machine);

                if(!String.IsNullOrEmpty(shfbFolder))
                {
                    // If there is an installed version, make sure it is older than what we expect
                    if(!Directory.Exists(shfbFolder) || !File.Exists(Path.Combine(shfbFolder, "SHFBProjectLauncher.exe")))
                        installedVersion = new Version(0, 0, 0, 0);
                    else
                    {
                        fvi = FileVersionInfo.GetVersionInfo(Path.Combine(shfbFolder, "SHFBProjectLauncher.exe"));

                        // The file version is missing the century to satisfy the MSI rule for the major version
                        // value so we'll add the century back from the SHFB version to get a match.
                        installedVersion = new Version(fvi.FileMajorPart + (shfbVersion.Major / 100 * 100),
                            fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart);
                    }

                    if(installedVersion < shfbVersion)
                        shfbFolder = null;
                    else
                    {
                        // If the version is greater, we can't go on as this package is out of date
                        if(installedVersion > shfbVersion)
                        {
                            shfbFolder = null;
                            pnlControls.Visibility = Visibility.Collapsed;

                            secResults.Blocks.Add(new Paragraph(new Bold(
                                new Run("Newer Version Detected")) { FontSize = 13 }));

                            para = new Paragraph();
                            para.Inlines.Add(new Run("It has been determined that a newer version of the " +
                                "Sandcastle Help File Builder is installed on this system.  You will need to " +
                                "download a newer version of this package that is compatible with this " +
                                "more recent release of the Sandcastle Help File Builder."));
                            secResults.Blocks.Add(para);

                            return;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                imgSpinner.Visibility = lblPleaseWait.Visibility = Visibility.Collapsed;

                para = new Paragraph(new Bold(new Run(
                    "An error occurred while searching for the Sandcastle Help File Builder:")));

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

            if(!String.IsNullOrEmpty(shfbFolder))
            {
                pnlControls.Visibility = Visibility.Collapsed;

                secResults.Blocks.Add(new Paragraph(new Bold(
                    new Run("Sandcastle Help File Builder Found")) { FontSize = 13 }));

                para = new Paragraph();
                para.Inlines.AddRange(new Inline[] {
                    new Run("It has been determined that the Sandcastle Help File Builder is installed on " +
                        "this system (Location: "),
                    new Italic(new Run(shfbFolder)),
                    new Run(").  No further action is required in this step.") });
                secResults.Blocks.Add(para);

                para = new Paragraph();
                para.Inlines.AddRange(new Inline[] {
                    new Run("Click the "), new Bold(new Run("Next")), new Run(" button below to continue.") });
                secResults.Blocks.Add(para);

                return;
            }

            btnInstallSHFB.Visibility = Visibility.Visible;
            btnInstallSHFB.IsEnabled = true;

            para = new Paragraph();
            para.Inlines.AddRange(new Inline[] {
                    new Run("The Sandcastle Help File Builder could not be found on this system.  Please " +
                        " click the "),
                    new Bold(new Run("Install SHFB")),
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
        /// Install the Sandcastle Help File Builder and show the results
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private async void btnInstallSHFB_Click(object sender, EventArgs e)
        {
            bool success = false;

            searchPerformed = suggestReboot = false;
            btnInstallSHFB.IsEnabled = false;
            imgSpinner.Visibility = lblPleaseWait.Visibility = Visibility.Visible;

            try
            {
                await Task.Run(() =>
                {
                    Utility.RunInstaller(Path.Combine(Utility.InstallResourcesPath, installerName), null);
                });

                success = true;
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
                    suggestReboot = true;
                    this.ShowPage();
                }
                else
                    btnInstallSHFB.IsEnabled = true;
            }
        }
        #endregion
    }
}
