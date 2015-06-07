//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : MSHelpViewerPropertiesPageControl.cs
// Author  : Eric Woodruff
// Updated : 06/05/2015
// Note    : Copyright 2011-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the MS Help Viewer category properties.
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
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

#if !STANDALONEGUI
using SandcastleBuilder.Package.Nodes;
#endif
using SandcastleBuilder.Utils;

using Microsoft.Build.Evaluation;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used to edit the MS Help Viewer category project properties
    /// </summary>
    [Guid("DCD56A8C-8A49-4A9C-804F-0BE55420D77F")]
    public partial class MSHelpViewerPropertiesPageControl : BasePropertyPage
    {
        #region Private data members
        //=====================================================================

        // Bad characters for the vendor name property
        private static Regex reBadVendorNameChars = new Regex(@"[:\\/\.,#&]");

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public MSHelpViewerPropertiesPageControl()
        {
            InitializeComponent();

            // Set the maximum size to prevent an unnecessary vertical scrollbar
            this.MaximumSize = new System.Drawing.Size(2048, this.Height);

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
                txtCatalogName.Text = txtCatalogName.Text.Trim();
                txtProductTitle.Text = txtProductTitle.Text.Trim();
                txtVendorName.Text = txtVendorName.Text.Trim();
                txtTocParentId.Text = txtTocParentId.Text.Trim();
                txtTocParentVersion.Text = txtTocParentVersion.Text.Trim();
                txtTopicVersion.Text = txtTopicVersion.Text.Trim();

                if(txtCatalogProductId.Text.Length == 0)
                    txtCatalogProductId.Text = "VS";

                if(txtCatalogVersion.Text.Length == 0)
                    txtCatalogVersion.Text = "100";

                // The vendor name has some restrictions with regard to certain characters in their normal
                // and encoded forms.
                txtVendorName.Text = reBadVendorNameChars.Replace(Uri.UnescapeDataString(txtVendorName.Text),
                    String.Empty);
                
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
