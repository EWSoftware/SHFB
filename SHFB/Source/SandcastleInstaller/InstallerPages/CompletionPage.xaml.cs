//=============================================================================
// System  : Sandcastle Guided Installation
// File    : CompletionPage.cs
// Author  : Eric Woodruff
// Updated : 04/14/2012
// Compiler: Microsoft Visual C#
//
// This file contains a completion page for the installer.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: https://GitHub.com/EWSoftware/SHFB.   This notice and
// all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  02/05/2011  EFW  Created the code
// 1.1.0.0  04/14/2012  EFW  Converted to use WPF
//=============================================================================

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Sandcastle.Installer.InstallerPages
{
    /// <summary>
    /// This is the completion page used by the installer
    /// </summary>
    public partial class CompletionPage : BasePage
    {
        #region Properties
        //=====================================================================

        /// <inheritdoc />
        public override string PageTitle
        {
            get { return "Completion"; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public CompletionPage()
        {
            InitializeComponent();
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// Add a list of links that allow further action before exiting
        /// </summary>
        public override void ShowPage()
        {
            Hyperlink hyperLink;

            pnlActions.Children.Clear();

            foreach(var page in this.Installer.AllPages)
                foreach(var action in page.CompletionActions)
                {
                    hyperLink = new Hyperlink(new Run(action.Description));
                    hyperLink.Click += (s, e) => { action.Action(); };

                    pnlActions.Children.Add(new Label
                    {
                        Margin = new Thickness(20, 0, 0, 0),
                        Content = hyperLink
                    });
                }

            // Add an option to reboot if needed
            if(this.Installer.AllPages.Any(p => p.SuggestReboot))
            {
                hyperLink = new Hyperlink(new Run("Reboot so that changes to the system can take effect"));
                hyperLink.Click += Reboot;

                pnlActions.Children.Add(new Label
                {
                    Margin = new Thickness(20, 0, 0, 0),
                    Content = hyperLink
                });
            }

            if(pnlActions.Children.Count == 0)
                pnlActions.Children.Add(new Label
                {
                    Margin = new Thickness(20, 0, 0, 0),
                    Content = "(No further actions specified)"
                });

            base.ShowPage();
        }

        /// <summary>
        /// Restart the system
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void Reboot(object sender, EventArgs e)
        {
            if(MessageBox.Show("This will reboot the system so that changes to the environment variables " +
              "can take effect.  Before doing so, you should save any work in other open applications.  " +
              "Click OK to reboot or CANCEL to stop.", this.PageTitle, MessageBoxButton.OKCancel,
              MessageBoxImage.Information, MessageBoxResult.Cancel) == MessageBoxResult.Cancel)
                return;

            try
            {
                // We could use the ExitWindows API, but this is quicker and should work fine
                System.Diagnostics.Process.Start("shutdown.exe", "/r /t 0 /d p:0:0");
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to restart system: " + ex.Message, this.PageTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
