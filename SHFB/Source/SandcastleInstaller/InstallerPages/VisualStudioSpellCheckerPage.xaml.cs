//===============================================================================================================
// System  : Sandcastle Guided Installation
// File    : VisualStudioSpellCheckerPage.cs
// Author  : Eric Woodruff
// Updated : 04/21/2021
//
// This file contains a page containing information about the Visual Studio Spell Checker
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
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
    /// This page contains information about the Visual Studio Spell Checker
    /// </summary>
    public partial class VisualStudioSpellCheckerPage : BasePage
    {
        #region Properties
        //=====================================================================

        /// <inheritdoc />
        public override string PageTitle => "Visual Studio Spell Checker";

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public VisualStudioSpellCheckerPage()
        {
            InitializeComponent();

            // Handle hyperlink clicks using the default handler
            fdDocument.AddHandler(Hyperlink.ClickEvent, new RoutedEventHandler(Utility.HyperlinkClick));
        }
        #endregion
    }
}
