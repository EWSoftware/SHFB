//===============================================================================================================
// System  : Sandcastle Guided Installation
// File    : IInstaller.cs
// Author  : Eric Woodruff
// Updated : 04/21/2021
//
// This file contains an interface definition used to implement an installer
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/05/2011  EFW  Created the code
//===============================================================================================================

using System.Collections.Generic;

namespace Sandcastle.Installer.InstallerPages
{
    /// <summary>
    /// This interface defines the methods required to implement an installer
    /// </summary>
    public interface IInstaller
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This returns an enumerable list of all of the current installer pages
        /// </summary>
        IEnumerable<IInstallerPage> AllPages { get; }

        #endregion
    }
}
