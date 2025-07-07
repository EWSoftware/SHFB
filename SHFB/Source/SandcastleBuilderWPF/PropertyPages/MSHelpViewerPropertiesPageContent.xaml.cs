//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : MSHelpViewerPropertiesPageContent.xaml.cs
// Author  : Eric Woodruff
// Updated : 06/21/2025
// Note    : Copyright 2017-2025, Eric Woodruff, All rights reserved
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
// 10/17/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;

using Sandcastle.Core.Project;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This user control is used to edit the MS Help Viewer category properties
    /// </summary>
    public partial class MSHelpViewerPropertiesPageContent : UserControl
    {
        #region Private data members
        //=====================================================================

        // Bad characters for the vendor name property
        private static readonly Regex reBadVendorNameChars = new(@"[:\\/\.,#&]");

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public MSHelpViewerPropertiesPageContent()
        {
            InitializeComponent();

            cboMSHelpViewerSdkLinkType.ItemsSource = (new Dictionary<string, string> {
                { MSHelpViewerSdkLinkType.Msdn.ToString(), "Links to online help topics" },
                { MSHelpViewerSdkLinkType.Id.ToString(), "ID links within the collection" },
                { MSHelpViewerSdkLinkType.None.ToString(), "No SDK links" } }).ToList();

            cboMSHelpViewerSdkLinkType.SelectedIndex = 0;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to fix up invalid values before storing them in the project properties
        /// </summary>
        public void FixUpInvalidValues()
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
        }
        #endregion
    }
}
