//===============================================================================================================
// System  : Sandcastle Guided Installation
// File    : BasePage.cs
// Author  : Eric Woodruff
// Updated : 12/28/2013
// Compiler: Microsoft Visual C#
//
// This file contains a simple base page for displaying HTML content with a panel for additional controls
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.0.0.0  02/05/2011  EFW  Created the code
// 1.1.0.0  03/05/2012  EFW  Converted to use WPF
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Sandcastle.Installer.InstallerPages
{
    /// <summary>
    /// This is a simple base installer page that displays HTML content with a panel for additional controls
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
        public virtual string PageTitle
        {
            get { return "Installer Page"; }
        }

        /// <inheritdoc />
        [Browsable(false)]
        public virtual string SandcastleVersion { get; set; }

        /// <inheritdoc />
        /// <remarks>The default implementation always returns true</remarks>
        [Browsable(false)]
        public virtual bool CanContinue
        {
            get { return true; }
        }

        /// <inheritdoc />
        [Browsable(false)]
        public virtual Control Control
        {
            get { return this; }
        }

        /// <inheritdoc />
        /// <remarks>The default implementation always returns null</remarks>
        [Browsable(false)]
        public virtual Version RequiredFrameworkVersion
        {
            get { return null; }
        }

        /// <inheritdoc />
        /// <remarks>The default implementation does nothing</remarks>
        public virtual IEnumerable<CompletionAction> CompletionActions
        {
            get { return Enumerable.Empty<CompletionAction>(); }
        }

        /// <inheritdoc />
        /// <remarks>The default implementation always returns false</remarks>
        public virtual bool SuggestReboot
        {
            get { return false; }
        }

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
