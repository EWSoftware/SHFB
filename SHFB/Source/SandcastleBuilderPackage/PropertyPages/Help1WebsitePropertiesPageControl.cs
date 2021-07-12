//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : Help1WebsitePropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 04/20/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This user control is used to edit the Help 1 and Website category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
//===============================================================================================================
// 03/27/2011  EFW  Created the code
// 03/08/2014  EFW  Updated for use with the Open XML file format
// 10/07/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.Runtime.InteropServices;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the Help 1, Website, and Open XML category project properties
    /// </summary>
    /// <remarks>The only Open XML property is for SDK links so we re-use the Website link type</remarks>
    [Guid("690D191C-5614-4A82-92A4-96EB519828CA")]
    public partial class Help1WebsitePropertiesPageControl : BasePropertyPage
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public Help1WebsitePropertiesPageControl()
        {
            InitializeComponent();

            this.Title = "Help 1/Website";
            this.HelpKeyword = "7d28bf8f-923f-44c1-83e1-337a416947a1";
            this.MinimumSize = DetermineMinimumSize(ucHelp1WebsitePropertiesContent);
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override bool IsEscapedProperty(string propertyName)
        {
            return propertyName == "WebsiteAdContent";
        }
        #endregion
    }
}
