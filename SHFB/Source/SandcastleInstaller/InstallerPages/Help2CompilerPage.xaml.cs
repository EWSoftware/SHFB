//===============================================================================================================
// System  : Sandcastle Guided Installation
// File    : Help2CompilerPage.cs
// Author  : Eric Woodruff
// Updated : 11/21/2014
// Compiler: Microsoft Visual C#
//
// This file contains a page used to help the user download and install the Microsoft Help 2 compiler via the
// Visual Studio SDK.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/06/2011  EFW  Created the code
// 03/06/2012  EFW  Converted to use WPF
// 11/21/2014  EFW  Made the page informational.  Help 2 support has been deprecated.
//===============================================================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Sandcastle.Installer.InstallerPages
{
    /// <summary>
    /// This page is used to help the user download and install the Microsoft Help 2 compiler via the Visual
    /// Studio SDK.
    /// </summary>
    public partial class Help2CompilerPage : BasePage
    {
        #region Properties
        //=====================================================================

        /// <inheritdoc />
        public override string PageTitle
        {
            get { return "Microsoft Help 2 Compiler"; }
        }

        /// <inheritdoc />
        public override bool CanContinue
        {
            get { return true; }
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

            // Handle hyperlink clicks using the default handler
            fdDocument.AddHandler(Hyperlink.ClickEvent, new RoutedEventHandler(Utility.HyperlinkClick));
        }
        #endregion
    }
}
