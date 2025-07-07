//===============================================================================================================
// System  : Sandcastle Guided Installation
// File    : BasePage.cs
// Author  : Eric Woodruff
// Updated : 07/06/2025
//
// This file contains a simple base page for displaying HTML content with a panel for additional controls
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/05/2011  EFW  Created the code
// 03/05/2012  EFW  Converted to use WPF
//===============================================================================================================

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Sandcastle.Installer.InstallerPages
{
    /// <summary>
    /// This is a simple base installer page that displays WPF content with a panel for additional controls
    /// </summary>
    public partial class BasePage : UserControl, IInstallerPage
    {
        #region IInstallerPage Members
        //=====================================================================

        /// <inheritdoc />
        [Browsable(false)]
        public IInstaller Installer { get; set; }

        /// <inheritdoc />
        [Browsable(false)]
        public virtual string PageTitle => "Installer Page";

        /// <inheritdoc />
        /// <remarks>The default implementation always returns true</remarks>
        [Browsable(false)]
        public virtual bool CanContinue => true;

        /// <inheritdoc />
        [Browsable(false)]
        public virtual Control Control => this;

        /// <inheritdoc />
        /// <remarks>The default implementation does nothing</remarks>
        public virtual IEnumerable<CompletionAction> CompletionActions => [];

        /// <inheritdoc />
        /// <remarks>The default implementation always returns false</remarks>
        public virtual bool SuggestReboot => false;

        /// <inheritdoc />
        /// <remarks>The default implementation does nothing</remarks>
        public virtual void Initialize(XElement configuration)
        {
        }

        /// <inheritdoc />
        /// <remarks>The default implementation does nothing</remarks>
        public virtual void ShowPage()
        {
        }

        /// <inheritdoc />
        /// <remarks>The default implementation does nothing</remarks>
        public virtual void HidePage()
        {
        }
        #endregion
    }
}
