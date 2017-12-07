//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : Help1WebsitePropertiesPageContent.xaml.cs
// Author  : Eric Woodruff
// Updated : 10/07/2017
// Note    : Copyright 2017, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to edit the Help 1, Website, Open XML, and Markdown category properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
//===============================================================================================================
// 10/07/2017  EFW  Converted the control to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This user control is used to edit the Help 1, Website, Open XML, and Markdown category properties
    /// </summary>
    public partial class Help1WebsitePropertiesPageContent : UserControl
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public Help1WebsitePropertiesPageContent()
        {
            InitializeComponent();

            cboHtmlSdkLinkType.ItemsSource = cboWebsiteSdkLinkType.ItemsSource = (new Dictionary<string, string> {
                { HtmlSdkLinkType.Msdn.ToString(), "Online links to MSDN help topics" },
                { HtmlSdkLinkType.None.ToString(), "No SDK links" } }).ToList();

            cboHtmlSdkLinkType.SelectedIndex = cboWebsiteSdkLinkType.SelectedIndex = 0;
        }
        #endregion
    }
}
