//===============================================================================================================
// System  : Sandcastle Guided Installation
// File    : RequiredFrameworkPage.cs
// Author  : Eric Woodruff
// Updated : 05/09/2015
// Compiler: Microsoft Visual C#
//
// This file contains a page used to help the user download and install the minimum .NET Framework version
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/05/2011  EFW  Created the code
// 03/06/2012  EFW  Converted to use WPF
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;

namespace Sandcastle.Installer.InstallerPages
{
    /// <summary>
    /// This page is used to help the user download and install the minimum .NET Framework version.
    /// </summary>
    public partial class RequiredFrameworkPage : BasePage
    {
        #region Private data members
        //=====================================================================

        private Version minVersion, installerVersion;
        private List<string> allVersions;
        #endregion

        #region Properties
        //=====================================================================

        /// <inheritdoc />
        public override string PageTitle
        {
            get { return ".NET Framework"; }
        }

        /// <summary>
        /// This is overridden to prevent continuing until the required .NET Framework version is installed
        /// </summary>
        public override bool CanContinue
        {
            get
            {
                if(!this.IsRequiredVersionPresent)
                {
                    MessageBox.Show("The required .NET Framework version must be present in order to " +
                        "install all of the required tools.  Follow the instructions on this page to " +
                        "download and install it.", this.PageTitle, MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                    return false;
                }

                return base.CanContinue;
            }
        }

        /// <summary>
        /// This is used to see if the required framework version is present
        /// </summary>
        private bool IsRequiredVersionPresent
        {
            get
            {
                // We could look at the registry but we'll take the simple approach as most of the time,
                // nothing will require a later version than what we are running under.
                allVersions.Clear();
                allVersions.AddRange(
                    Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.System) +
                        @"\..\Microsoft.NET\Framework").Where(d =>
                        {
                            string dir = d.Substring(d.LastIndexOf('\\') + 1);

                            return dir.Length > 2 && (dir[0] == 'v' || dir[0] == 'V') &&
                                Char.IsDigit(dir[1]);

                        }).Select(d => d.Substring(d.LastIndexOf('\\') + 2)));

                minVersion = this.Installer.AllPages.Max(p => p.RequiredFrameworkVersion) ?? minVersion;

                if(minVersion <= installerVersion)
                    return true;

                string required = minVersion.ToString();

                return allVersions.Any(v => v == required || v.StartsWith(required, StringComparison.Ordinal));
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public RequiredFrameworkPage()
        {
            minVersion = installerVersion = new Version(4, 5);
            allVersions = new List<string>();

            InitializeComponent();

            // Handle hyperlink clicks using the default handler
            fdDocument.AddHandler(Hyperlink.ClickEvent, new RoutedEventHandler(Utility.HyperlinkClick));
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// This is overridden to figure out the minimum .NET Framework version required by all pages in
        /// the installer.
        /// </summary>
        public override void ShowPage()
        {
            Paragraph para;

            // If already checked, just return
            if(allVersions.Count != 0)
                return;

            if(this.IsRequiredVersionPresent)
            {
                fdDocument.Blocks.Add(new Paragraph(
                    new Bold(new Run("Minimum Required Version Found"))) { FontSize = 13 });

                fdDocument.Blocks.Add(new Paragraph(
                    new Run(String.Format(CultureInfo.CurrentCulture, "It has been determined that the " +
                        "minimum .NET Framework version (v{0}) required by the tools installed by this " +
                        "package is present.  No further action is required in this step.", minVersion))));
                        
                para = new Paragraph();

                para.Inlines.AddRange(new Inline[] {
                    new Run("Click the "), new Bold(new Run("Next")), new Run(" button to continue.") });

                fdDocument.Blocks.Add(para);
            }
            else
            {
                fdDocument.Blocks.Add(new Paragraph(new Run(String.Format(CultureInfo.CurrentCulture,
                    "The .NET Framework {0} is required by the following tools installed by this package:",
                    minVersion))));

                var list = new List();

                list.ListItems.AddRange(this.Installer.AllPages.Where(
                    p => p.RequiredFrameworkVersion >= minVersion).Select(
                    p => new ListItem(new Paragraph(new Run(p.PageTitle)))));

                fdDocument.Blocks.Add(list);
                fdDocument.Blocks.Add(new Paragraph(new Run("To install it, do the following:")));

                list = new List();
                    
                para = new Paragraph(new Run("Click this link to go to the download page: "));
                para.Inlines.Add(new Hyperlink(new Run(".NET Framework and .NET SDKs")) {
                    NavigateUri = new Uri("https://dotnet.microsoft.com/")
                });

                list.ListItems.Add(new ListItem(para));

                para = new Paragraph();
                para.Inlines.AddRange(new Inline[] {
                    new Run("Once the page opens, download and install the required version.") });
                list.ListItems.Add(new ListItem(para));

                para = new Paragraph();
                para.Inlines.AddRange(new Inline[] {
                    new Run("When it has finished, come back to this application and click the "),
                    new Bold(new Run("Next")),
                    new Run(" button to continue installing the rest of the tools.") });
                list.ListItems.Add(new ListItem(para));

                fdDocument.Blocks.Add(list);
            }

            base.ShowPage();
        }
        #endregion
    }
}
