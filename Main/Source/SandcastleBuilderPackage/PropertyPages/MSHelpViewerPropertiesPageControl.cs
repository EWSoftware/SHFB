//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : MSHelpViewerPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 04/11/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the MS Help Viewer category properties.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.0  03/27/2011  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the MS Help 2 category project properties
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

            cboMSHelpViewerSdkLinkType.DisplayMember = "Value";
            cboMSHelpViewerSdkLinkType.ValueMember = "Key";

            cboMSHelpViewerSdkLinkType.DataSource = (new Dictionary<string, string> {
                { MSHelpViewerSdkLinkType.Msdn.ToString(), "Online links to MSDN help topics" },
                { MSHelpViewerSdkLinkType.Id.ToString(), "ID links within the collection" },
                { MSHelpViewerSdkLinkType.None.ToString(), "No SDK links" } }).ToList();
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override bool IsValid
        {
            get
            {
                txtCatalogProductId.Text = txtCatalogProductId.Text.Trim();
                txtCatalogVersion.Text = txtCatalogVersion.Text.Trim();
                txtProductTitle.Text = txtProductTitle.Text.Trim();
                txtVendorName.Text = txtVendorName.Text.Trim();
                txtTocParentId.Text = txtTocParentId.Text.Trim();
                txtTocParentVersion.Text = txtTocParentVersion.Text.Trim();
                txtTopicVersion.Text = txtTopicVersion.Text.Trim();

                if(txtCatalogProductId.Text.Length == 0)
                    txtCatalogProductId.Text = "VS";

                if(txtCatalogVersion.Text.Length == 0)
                    txtCatalogVersion.Text = "100";

                if(txtTocParentId.Text.Length == 0)
                    txtTocParentId.Text = "-1";

                if(txtTocParentVersion.Text.Length == 0)
                    txtTocParentVersion.Text = "100";

                if(txtTopicVersion.Text.Length == 0)
                    txtTopicVersion.Text = "100";

                if(udcTocOrder.Text.Trim().Length == 0)
                    udcTocOrder.Value = -1;

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
