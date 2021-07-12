//===============================================================================================================
// System  : Sandcastle Reflection Data Manager
// File    : BuildReflectionDataDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/10/2021
// Note    : Copyright 2015-2021, Eric Woodruff, All rights reserved
//
// This file contains the window class that is used to manage the build settings and build the reflection data
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/27/2015  EFW  Created the code
//===============================================================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Sandcastle.Core.Reflection;

namespace ReflectionDataManager
{
    /// <summary>
    /// This window is used to manage the build settings and build the reflection data for a given reflection
    /// data set.
    /// </summary>
    public partial class BuildReflectionDataDlg : Window
    {
        #region Private data members
        //=====================================================================

        private readonly ReflectionDataSet dataSet;
        private CancellationTokenSource cancellationTokenSource;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataSet">The data set for which to build reflection data</param>
        public BuildReflectionDataDlg(ReflectionDataSet dataSet)
        {
            InitializeComponent();

            this.DataContext = this.dataSet = dataSet;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to report build progress
        /// </summary>
        /// <param name="message">The progress message</param>
        private void ReportProgress(string message)
        {
            txtBuildOutput.AppendText(message + "\r\n");
            txtBuildOutput.CaretIndex = txtBuildOutput.Text.Length;
            txtBuildOutput.ScrollToEnd();
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Confirm closing if a build is in progress
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(cancellationTokenSource != null)
            {
                if(cancellationTokenSource.IsCancellationRequested)
                    return;

                if(MessageBox.Show("A build is currently taking place.  Do you want to abort it and exit?",
                  "Reflection Data Manager", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if(cancellationTokenSource != null)
                        cancellationTokenSource.Cancel();
                }

                e.Cancel = true;
            }
        }

        /// <summary>
        /// Build the reflection data
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private async void btnBuild_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                cancellationTokenSource = new CancellationTokenSource();

                btnBuild.IsEnabled = false;
                pbProgress.Visibility = Visibility.Visible;

                using(var bp = new BuildProcess(dataSet)
                    {
                        CancellationToken = cancellationTokenSource.Token,
                        ProgressProvider = new Progress<string>(this.ReportProgress)
                    })
                {
                    await Task.Run(() => bp.Build(), cancellationTokenSource.Token).ConfigureAwait(true);
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                txtBuildOutput.AppendText("\r\n\r\nUnable to generate reflection data files: " + ex.Message + "\r\n");
                txtBuildOutput.CaretIndex = txtBuildOutput.Text.Length;
                txtBuildOutput.ScrollToEnd();
            }
            finally
            {
                btnBuild.IsEnabled = true;
                pbProgress.Visibility = Visibility.Hidden;

                if(cancellationTokenSource.IsCancellationRequested)
                    this.Close();

                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }
        #endregion
    }
}
