//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : BuildEventPropertiesPageContent.xaml.cs
// Author  : Eric Woodruff
// Updated : 11/14/2017
// Note    : Copyright 2017, Eric Woodruff, All rights reserved
//
// This user control is used to edit the Build Events category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/04/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System.Windows;
using System.Windows.Controls;

using Microsoft.Build.Evaluation;

using Sandcastle.Platform.Windows;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This user control is used to edit the Build Events category properties
    /// </summary>
    public partial class BuildEventPropertiesPageContent : UserControl
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set a reference to the current project
        /// </summary>
        public Project Project { get; set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public BuildEventPropertiesPageContent()
        {
            InitializeComponent();

#if !STANDALONEGUI
            // The standalone GUI does not execute pre-build and post-build events.  Visual Studio does.
            bdStandaloneGUI.Visibility = Visibility.Collapsed;
#endif
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Edit the pre-build/post-build event in the extended editor form
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnEditBuildEvent_Click(object sender, RoutedEventArgs e)
        {
            TextBox tb;
            string title;

            if(e.OriginalSource == btnEditPreBuildEvent)
            {
                tb = txtPreBuildEvent;
                title = "Edit Pre-Build Event Command Line";
            }
            else
            {
                tb = txtPostBuildEvent;
                title = "Edit Post-Build Event Command Line";
            }

            var dlg = new BuildEventEditorDlg() { Title = title, BuildEventText = tb.Text };

            dlg.DetermineMacroValues(this.Project);

            if(dlg.ShowModalDialog() ?? false)
            {
                tb.Text = dlg.BuildEventText;
                tb.Select(0, 0);
                tb.ScrollToHome();
            }
        }
        #endregion
    }
}
