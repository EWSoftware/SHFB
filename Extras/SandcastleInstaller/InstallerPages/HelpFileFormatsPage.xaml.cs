//=============================================================================
// System  : Sandcastle Guided Installation
// File    : HelpFileFormatsPage.cs
// Author  : Eric Woodruff
// Updated : 03/06/2012
// Compiler: Microsoft Visual C#
//
// This file contains a help file format information page for the installer.
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
// 1.1.0.0  03/05/2012  EFW  Converted to use WPF
//=============================================================================

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
        public override string PageTitle
        {
            get { return "Help File Formats"; }
        }
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
