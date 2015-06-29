//===============================================================================================================
// System  : Sandcastle Reflection Data Manager
// File    : AboutDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/27/2015
// Note    : Copyright 2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the window class that shows description and version information for the application
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/21/2015  EFW  Created the code
//===============================================================================================================

using System;
using System.Reflection;
using System.Windows;

namespace ReflectionDataManager
{
    /// <summary>
    /// This window shows description and version information for the application
    /// </summary>
    public partial class AboutDlg : Window
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public AboutDlg()
        {
            InitializeComponent();

            Assembly asm = Assembly.GetExecutingAssembly();

            var titleAttr = (AssemblyTitleAttribute)AssemblyTitleAttribute.GetCustomAttribute(asm,
                typeof(AssemblyTitleAttribute));
            var versionAttr = (AssemblyInformationalVersionAttribute)AssemblyInformationalVersionAttribute.GetCustomAttribute(
                asm, typeof(AssemblyInformationalVersionAttribute));
            var descAttr = (AssemblyDescriptionAttribute)AssemblyDescriptionAttribute.GetCustomAttribute(asm,
                typeof(AssemblyDescriptionAttribute));

            // Set the labels
            this.Title = "About " + titleAttr.Title;
            tbApplicationName.Text = titleAttr.Title;
            tbVersion.Text = "Version: " + versionAttr.InformationalVersion;
            tbDescription.Text = descAttr.Description;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Go to the project website
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lnkSHFB_RequestNavigate(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(lnkSHFB.NavigateUri.AbsoluteUri);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                MessageBox.Show("Unable to launch link target.  Reason: " + ex.Message, "About",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        #endregion
    }
}
