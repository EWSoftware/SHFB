//=============================================================================
// System  : Sandcastle Guided Installation
// File    : Help2CompilerPage.cs
// Author  : Eric Woodruff
// Updated : 03/06/2012
// Compiler: Microsoft Visual C#
//
// This file contains a page used to help the user download and install the
// Microsoft Help 2 compiler via the Visual Studio SDK.
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
// 1.1.0.0  03/06/2012  EFW  Converted to use WPF
//=============================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Sandcastle.Installer.InstallerPages
{
    /// <summary>
    /// This page is used to help the user download and install the Microsoft Help 2 compiler via the
    /// Visual Studio SDK.
    /// </summary>
    public partial class Help2CompilerPage : BasePage
    {
        #region Private data members
        //=====================================================================

        private string hxcompFolder;
        private bool searchPerformed;

        private Task<String> searchTask;
        #endregion

        #region Properties
        //=====================================================================

        /// <inheritdoc />
        public override string PageTitle
        {
            get { return "Microsoft Help 2 Compiler"; }
        }

        /// <summary>
        /// This is overridden to confirm that the user wants to continue without installing the Help 2
        /// compiler.
        /// </summary>
        public override bool CanContinue
        {
            get
            {
                if(searchTask != null)
                {
                    MessageBox.Show("Please wait for the search to complete before continuing",
                        this.PageTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }

                if(String.IsNullOrEmpty(hxcompFolder) &&
                  MessageBox.Show("The Microsoft Help 2 compiler does not appear to be installed.  " +
                    "Microsoft Help 2 files cannot be produced without it.  If you will not be creating " +
                    "them, you can safely skip this step.  If you will, you should download and install " +
                    "it before proceeding.\r\n\r\nDo you want to proceed without it?", this.PageTitle,
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) ==
                    MessageBoxResult.No)
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
        public Help2CompilerPage()
        {
            InitializeComponent();

            imgSpinner.Visibility = lblPleaseWait.Visibility = Visibility.Collapsed;

            // Handle hyperlink clicks using the default handler
            fdDocument.AddHandler(Hyperlink.ClickEvent, new RoutedEventHandler(Utility.HyperlinkClick));
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is called if the search for the Help 2 compiler completes successfully
        /// </summary>
        /// <param name="cache">The index cache</param>
        private void SearchCompleted(string folder)
        {
            searchTask.Dispose();
            searchTask = null;
            searchPerformed = true;

            imgSpinner.Visibility = lblPleaseWait.Visibility = Visibility.Collapsed;
            btnSearchAgain.IsEnabled = true;

            hxcompFolder = folder;

            if(!String.IsNullOrEmpty(hxcompFolder))
            {
                pnlControls.Visibility = Visibility.Collapsed;

                secResults.Blocks.Add(new Paragraph(new Bold(
                    new Run("Help 2 Compiler Found")) { FontSize = 13 }));

                var para = new Paragraph();
                para.Inlines.AddRange(new Inline[] {
                    new Run("It has been determined that the Microsoft Help 2 compiler is installed on " +
                        "this system (Location: "),
                    new Italic(new Run(hxcompFolder)),
                    new Run(").  No further action is required in this step.") });
                secResults.Blocks.Add(para);

                para = new Paragraph();
                para.Inlines.AddRange(new Inline[] {
                    new Run("Click the "), new Bold(new Run("Next")), new Run(" button to continue.") });
                secResults.Blocks.Add(para);
            }
            else
            {
                // Append the Help 2 compiler installation instructions to the flow document
                secResults.Blocks.AppendFrom("Sandcastle.Installer.Resources.InstallHelp2Compiler.xaml");
            }
        }

        /// <summary>
        /// This is called if the search for the Help 2 compiler fails
        /// </summary>
        /// <param name="ex">The exception that caused the failure</param>
        private void SearchFailed(AggregateException ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            searchTask.Dispose();
            searchTask = null;

            imgSpinner.Visibility = lblPleaseWait.Visibility = Visibility.Collapsed;
            btnSearchAgain.IsEnabled = true;

            var para = new Paragraph(new Bold(new Run(
                "An error occurred while searching for the Help 2 compiler:")));

            para.Foreground = new SolidColorBrush(Colors.Red);
            para.Inlines.Add(new LineBreak());

            foreach(var innerEx in ex.InnerExceptions)
            {
                para.Inlines.Add(new LineBreak());
                para.Inlines.Add(new Run(innerEx.Message));
            }

            secResults.Blocks.Add(para);
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// This is overridden to figure out if the Help 1 compiler is installed
        /// </summary>
        public override void ShowPage()
        {
            if(!searchPerformed && searchTask == null)
                btnSearchAgain_Click(this, new RoutedEventArgs());
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Search for the Microsoft Help 2 compiler and show the results
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnSearchAgain_Click(object sender, RoutedEventArgs e)
        {
            secResults.Blocks.Clear();

            btnSearchAgain.IsEnabled = false;
            imgSpinner.Visibility = lblPleaseWait.Visibility = Visibility.Visible;
            searchPerformed = false;

            // Ignore the request if the task is already running
            if(searchTask == null)
            {
                var ui = TaskScheduler.FromCurrentSynchronizationContext();

                searchTask = Task.Factory.StartNew<String>(() =>
                {
                    // Search the Visual Studio SDK folders first as it usually has a more recent version
                    string folder = Utility.FindSdkExecutable("hxcomp.exe");

                    // If not found there, try the default installation folders
                    if(folder.Length == 0)
                    {
                        folder= Utility.FindOnFixedDrives(@"\Common Files\Microsoft Shared\Help 2.0 Compiler");

                        if(folder.Length == 0)
                            folder = Utility.FindOnFixedDrives(@"\Microsoft Help 2.0 SDK");
                    }

                    return folder;
                });

                searchTask.ContinueWith(t => this.SearchCompleted(t.Result),
                    CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, ui);

                searchTask.ContinueWith(t => this.SearchFailed(t.Exception),
                    CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, ui);
            }
        }
        #endregion
    }
}
