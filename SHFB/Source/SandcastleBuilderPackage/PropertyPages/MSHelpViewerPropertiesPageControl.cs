//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : MSHelpViewerPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 10/19/2017
// Note    : Copyright 2011-2017, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the MS Help Viewer category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/27/2011  EFW  Created the code
// 10/05/2012  EFW  Added Catalog Name property
// 10/17/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.Runtime.InteropServices;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the MS Help Viewer category project properties
    /// </summary>
    [Guid("DCD56A8C-8A49-4A9C-804F-0BE55420D77F")]
    public partial class MSHelpViewerPropertiesPageControl : BasePropertyPage
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public MSHelpViewerPropertiesPageControl()
        {
            InitializeComponent();

            this.Title = "MS Help Viewer";
            this.HelpKeyword = "5f743a6e-3239-409a-a8c1-0bff4b5375f4";
            this.MinimumSize = DetermineMinimumSize(ucMSHelpViewerPropertiesContent);
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override bool IsValid
        {
            get
            {
                ucMSHelpViewerPropertiesContent.FixUpInvalidValues();
                return true;
            }
        }

        /// <inheritdoc />
        protected override bool IsEscapedProperty(string propertyName)
        {
            switch(propertyName)
            {
                case "CatalogProductId":
                case "CatalogVersion":
                case "CatalogName":
                case "ProductTitle":
                case "TocParentId":
                case "TocParentVersion":
                case "TopicVersion":
                case "VendorName":
                    return true;

                default:
                    return false;
            }
        }
        #endregion
    }
}
