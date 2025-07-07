//===============================================================================================================
// System  : Sandcastle Guided Installation
// File    : WhatsNewPage.cs
// Author  : Eric Woodruff
// Updated : 07/06/2025
//
// This file contains a What's New page for the installer.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/13/2012  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: url

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml.Linq;

namespace Sandcastle.Installer.InstallerPages
{
    /// <summary>
    /// This is the What's New page used by the installer
    /// </summary>
    public partial class WhatsNewPage : BasePage
    {
        #region Properties
        //=====================================================================

        /// <inheritdoc />
        public override string PageTitle => "What's New";

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public WhatsNewPage()
        {
            InitializeComponent();
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XElement configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            // Load the What's New link information
            foreach(var wn in configuration.Elements("whatsNew"))
            {
                Hyperlink hyperLink = new(new Run(wn.Attribute("description").Value))
                {
                    Tag = wn.Attribute("url").Value
                };

                hyperLink.Click += LaunchLink;

                pnlLinks.Children.Add(new Label
                {
                    Margin = new Thickness(20, 0, 0, 0),
                    Content = hyperLink
                });
            }

            if(pnlLinks.Children.Count == 0)
                pnlLinks.Children.Add(new Label
                {
                    Margin = new Thickness(20, 0, 0, 0),
                    Content = "(No new information to report)"
                });

            base.Initialize(configuration);
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Launch a What's New link
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void LaunchLink(object sender, EventArgs e)
        {
            try
            {
                // We could use the ExitWindows API, but this is quicker and should work fine
                System.Diagnostics.Process.Start((string)((Hyperlink)sender).Tag);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to launch What's New link: " + ex.Message, this.PageTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
