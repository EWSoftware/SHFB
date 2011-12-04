//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : GeneralOptionsControl.cs
// Author  : Eric Woodruff
// Updated : 11/19/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used to modify the general help file builder package
// preferences that are unrelated to individual projects.
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
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This user control is used to modify the general help file builder
    /// package preferences that are unrelated to individual projects.
    /// </summary>
    /// <remarks><b>NOTE:</b> There appears to be a bug in Visual Studio that
    /// cases the tab order property on the controls to be ignored.  Currently,
    /// the tab order is determined by the control creation order.  You need to
    /// manually edit the .Designer.cs file to order the controls.
    /// 
    /// <p/>Also, the control uses the Segoe UI font whether you want it or not
    /// so it's best to set it in the designer so that the control size and
    /// positioning is not thrown off at runtime.</remarks>
    [ToolboxItem(false)]
    public partial class GeneralOptionsControl : UserControl
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to see if the values are valid
        /// </summary>
        public bool IsValid
        {
            get
            {
                bool isValid = true;

                epErrors.Clear();

                string filePath = txtHxSViewerPath.Text.Trim();

                if(filePath.Length != 0)
                {
                    txtHxSViewerPath.Text = filePath = Path.GetFullPath(filePath);

                    if(!File.Exists(filePath))
                    {
                        epErrors.SetError(btnSelectHxSViewer, "The viewer application does not exist");
                        isValid = false;
                    }
                }

                filePath = txtMSHelpViewerPath.Text.Trim();

                if(filePath.Length != 0)
                {
                    txtMSHelpViewerPath.Text = filePath = Path.GetFullPath(filePath);

                    if(!File.Exists(filePath))
                    {
                        epErrors.SetError(btnSelectMSHCViewer, "The viewer application does not exist");
                        isValid = false;
                    }
                }

                return isValid;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public GeneralOptionsControl()
        {
            InitializeComponent();
            this.Font = Utility.GetDialogFont();
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to update the controls with the current property values
        /// </summary>
        /// <param name="optionsPage">The options page</param>
        public void SetValues(SandcastleBuilderOptionsPage optionsPage)
        {
            txtHxSViewerPath.Text = optionsPage.HxsViewerPath;
            txtMSHelpViewerPath.Text = optionsPage.MSHelpViewerPath;
            udcASPNetDevServerPort.Value = optionsPage.AspNetDevelopmentServerPort;
            chkVerboseLogging.Checked = optionsPage.VerboseLogging;
            chkOpenLogViewerOnFailure.Checked = optionsPage.OpenLogViewerOnFailedBuild;
            chkOpenHelpAfterBuild.Checked = optionsPage.OpenHelpAfterBuild;
            chkUseExternalBrowser.Checked = optionsPage.UseExternalWebBrowser;
        }

        /// <summary>
        /// This is called by the owning dialog page to validate the values
        /// and apply changes to the properties.
        /// </summary>
        /// <param name="optionsPage">The options page</param>
        public void ApplyChanges(SandcastleBuilderOptionsPage optionsPage)
        {
            if(this.IsValid)
            {
                optionsPage.HxsViewerPath = txtHxSViewerPath.Text;
                optionsPage.MSHelpViewerPath = txtMSHelpViewerPath.Text;
                optionsPage.AspNetDevelopmentServerPort = (int)udcASPNetDevServerPort.Value;
                optionsPage.VerboseLogging = chkVerboseLogging.Checked;
                optionsPage.OpenLogViewerOnFailedBuild = chkOpenLogViewerOnFailure.Checked;
                optionsPage.OpenHelpAfterBuild = chkOpenHelpAfterBuild.Checked;
                optionsPage.UseExternalWebBrowser = chkUseExternalBrowser.Checked;
            }
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Select a help viewer application
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnSelectViewer_Click(object sender, EventArgs e)
        {
            TextBox tb = (sender == btnSelectHxSViewer) ? txtHxSViewerPath : txtMSHelpViewerPath;

            using(OpenFileDialog dlg = new OpenFileDialog())
            {
                if(tb == txtHxSViewerPath)
                    dlg.Title = "Select the MS Help 2 (.HxS) viewer application";
                else
                    dlg.Title = "Select the MS Help Viewer (.mshc) viewer application";

                dlg.Filter = "Executable files (*.exe)|*.exe|All Files (*.*)|*.*";
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                dlg.DefaultExt = "exe";

                // If one is selected, use that file
                if(dlg.ShowDialog() == DialogResult.OK)
                    tb.Text = dlg.FileName;
            }
        }
        #endregion
    }
}
