//===============================================================================================================
// System  : Sandcastle Guided Installation
// File    : WelcomePage.cs
// Author  : Eric Woodruff
// Updated : 04/21/2021
//
// This file contains a welcome page for the installer.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/05/2011  EFW  Created the code
// 03/05/2012  EFW  Converted to use WPF
//===============================================================================================================

using System.Windows;
using System.Windows.Documents;

namespace Sandcastle.Installer.InstallerPages
{
    /// <summary>
    /// This is the welcome page used by the installer
    /// </summary>
    public partial class WelcomePage : BasePage
    {
        #region Properties
        //=====================================================================

        /// <inheritdoc />
        public override string PageTitle => "Welcome";

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public WelcomePage()
        {
            InitializeComponent();

            // Handle hyperlink clicks using the default handler
            fdDocument.AddHandler(Hyperlink.ClickEvent, new RoutedEventHandler(Utility.HyperlinkClick));
        }
        #endregion
    }
}
