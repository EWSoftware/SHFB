//===============================================================================================================
// System  : Sandcastle Guided Installation
// File    : HelpFileFormatsPage.cs
// Author  : Eric Woodruff
// Updated : 04/21/2021
//
// This file contains a help file format information page for the installer.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/06/2011  EFW  Created the code
// 03/05/2012  EFW  Converted to use WPF
//===============================================================================================================

namespace Sandcastle.Installer.InstallerPages
{
    /// <summary>
    /// This is the help file format information page used by the installer
    /// </summary>
    public partial class HelpFileFormatsPage : BasePage
    {
        #region Properties
        //=====================================================================

        /// <inheritdoc />
        public override string PageTitle => "Help File Formats";

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public HelpFileFormatsPage()
        {
            InitializeComponent();
        }
        #endregion
    }
}
