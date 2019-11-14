//===============================================================================================================
// System  : Sandcastle Guided Installation
// File    : ExtendedDocCommentsProviderPage.cs
// Author  : Eric Woodruff
// Updated : 11/07/2019
//
// This file contains a page containing information about the Extended Documentation Comments Provider
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/02/2013  EFW  Created the code
//===============================================================================================================

using System.Windows;
using System.Windows.Documents;

namespace Sandcastle.Installer.InstallerPages
{
    /// <summary>
    /// This page contains information about the Extended Documentation Comments Provider
    /// </summary>
    public partial class ExtendedDocCommentsProviderPage : BasePage
    {
        #region Properties
        //=====================================================================

        /// <inheritdoc />
        public override string PageTitle => "Extended XML Doc Comments Provider";

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public ExtendedDocCommentsProviderPage()
        {
            InitializeComponent();

            // Handle hyperlink clicks using the default handler
            fdDocument.AddHandler(Hyperlink.ClickEvent, new RoutedEventHandler(Utility.HyperlinkClick));
        }
        #endregion
    }
}
