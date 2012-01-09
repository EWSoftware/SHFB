//=============================================================================
// System  : Sandcastle Help File Builder
// File    : NewFromOtherFormatDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/08/2012
// Note    : Copyright 2008-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to convert a project in a different format
// to the new MSBuild format used by SHFB 1.8.0.0 and later.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  08/04/2008  EFW  Created the code
// 1.9.1.0  01/09/2011  EFW  Updated for use with .NET 4.0 and MSBuild 4.0
// 1.9.3.4  01/08/2012  EFW  Moved the form into the SandcastleBuilder.Utils
//                           library so that it can be used from the VSPackage.
//=============================================================================

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.Utils.Conversion
{
    /// <summary>
    /// This form is used to convert a project in a different format to the new
    /// MSBuild format used by SHFB 1.8.0.0 and later.
    /// </summary>
    public partial class NewFromOtherFormatDlg : Form
    {
        #region Private data members
        //=====================================================================

        private SandcastleProject currentProject;
        private string newProjectFilename;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// Upon successful conversion, this will return the name of the new project file
        /// </summary>
        public string NewProjectFilename
        {
            get { return newProjectFilename; }
        }
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <overloads>There are two overloads for the constructor</overloads>
        public NewFromOtherFormatDlg()
        {
            InitializeComponent();
            cboProjectFormat.SelectedIndex = 0;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">The project into which the converted elements will be inserted</param>
        /// <remarks>This constructor is used by the VSPackage to convert an old project after it creates
        /// a new project instance.</remarks>
        public NewFromOtherFormatDlg(SandcastleProject project) : this()
        {
            currentProject = project;
            txtNewProjectFolder.Text = project.Filename;
            txtNewProjectFolder.Enabled = btnSelectNewFolder.Enabled = false;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Close the dialog without converting a project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Select the project to convert
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnSelectProject_Click(object sender, EventArgs e)
        {
            using(OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                switch(cboProjectFormat.SelectedIndex)
                {
                    case 1:
                        dlg.Title = "Select the NDoc project to convert";
                        dlg.Filter = "NDoc Project files (*.ndoc)|*.ndoc|All files (*.*)|*.*";
                        dlg.DefaultExt = "ndoc";
                        break;

                    case 2:
                        dlg.Title = "Select the DocProject project to convert";
                        dlg.Filter = "DocProject files (*.csproj, *.vbproj)|*.csproj;*.vbproj|All files (*.*)|*.*";
                        dlg.DefaultExt = "csproj";
                        break;

                    case 3:
                        dlg.Title = "Select the SandcastleGUI project to convert";
                        dlg.Filter = "SandcastleGUI Project files (*.SandcastleGUI)|*.SandcastleGUI|All files (*.*)|*.*";
                        dlg.DefaultExt = "SandcastleGUI";
                        break;

                    case 4:
                        dlg.Title = "Select the Microsoft Example GUI project to convert";
                        dlg.Filter = "Example GUI Project files (*.scproj)|*.scproj|All files (*.*)|*.*";
                        dlg.DefaultExt = "scproj";
                        break;

                    default:
                        dlg.Title = "Select the old SHFB project to convert";
                        dlg.Filter = "Old SHFB files (*.shfb)|*.shfb|All Files (*.*)|*.*";
                        dlg.DefaultExt = "shfb";
                        break;
                }

                // If selected, set the filename
                if(dlg.ShowDialog() == DialogResult.OK)
                    txtProjectFile.Text = dlg.FileName;
            }
        }

        /// <summary>
        /// Select the folder for the new project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnSelectNewFolder_Click(object sender, EventArgs e)
        {
            using(FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select the folder for the new project";
                dlg.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                // If selected, set the new folder
                if(dlg.ShowDialog() == DialogResult.OK)
                    txtNewProjectFolder.Text = dlg.SelectedPath + @"\";
            }
        }

        /// <summary>
        /// Convert the project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnConvert_Click(object sender, EventArgs e)
        {
            ConvertToMSBuildFormat converter = null;
            string project, folder;
            bool isValid = true;

            epErrors.Clear();
            epErrors.SetIconPadding(txtProjectFile, btnSelectProject.Width + 5);
            epErrors.SetIconPadding(txtNewProjectFolder, btnSelectNewFolder.Width + 5);

            project = txtProjectFile.Text.Trim();

            if(txtNewProjectFolder.Enabled)
                folder = txtNewProjectFolder.Text.Trim();
            else
                folder = Path.GetDirectoryName(txtNewProjectFolder.Text);

            if(project.Length == 0)
            {
                epErrors.SetError(txtProjectFile, "A project file must be specified");
                isValid = false;
            }
            else
                if(!File.Exists(project))
                {
                    epErrors.SetError(txtProjectFile, "The specified project file does not exist");
                    isValid = false;
                }

            if(folder.Length == 0)
            {
                epErrors.SetError(txtNewProjectFolder, "An output folder for the converted project must " +
                    "be specified");
                isValid = false;
            }

            if(isValid)
            {
                project = Path.GetFullPath(project);
                folder = Path.GetFullPath(folder);

                if(FolderPath.TerminatePath(Path.GetDirectoryName(project)) == FolderPath.TerminatePath(folder))
                {
                    epErrors.SetError(txtNewProjectFolder, "The output folder cannot match the folder of the " +
                        "original project");
                    isValid = false;
                }
            }

            if(!isValid)
                return;

            try
            {
                this.Cursor = Cursors.WaitCursor;

                switch(cboProjectFormat.SelectedIndex)
                {
                    case 1:
                        if(currentProject == null)
                            converter = new ConvertFromNDoc(txtProjectFile.Text, txtNewProjectFolder.Text);
                        else
                            converter = new ConvertFromNDoc(txtProjectFile.Text, currentProject);
                        break;

                    case 2:
                        if(currentProject == null)
                            converter = new ConvertFromDocProject(txtProjectFile.Text, txtNewProjectFolder.Text);
                        else
                            converter = new ConvertFromDocProject(txtProjectFile.Text, currentProject);
                        break;

                    case 3:
                        if(currentProject == null)
                            converter = new ConvertFromSandcastleGui(txtProjectFile.Text, txtNewProjectFolder.Text);
                        else
                            converter = new ConvertFromSandcastleGui(txtProjectFile.Text, currentProject);
                        break;

                    case 4:
                        if(currentProject == null)
                            converter = new ConvertFromMSExampleGui(txtProjectFile.Text, txtNewProjectFolder.Text);
                        else
                            converter = new ConvertFromMSExampleGui(txtProjectFile.Text, currentProject);
                        break;

                    default:
                        if(currentProject == null)
                            converter = new ConvertFromShfbFile(txtProjectFile.Text, txtNewProjectFolder.Text);
                        else
                            converter = new ConvertFromShfbFile(txtProjectFile.Text, currentProject);
                        break;
                }

                newProjectFilename = converter.ConvertProject();

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                BuilderException bex = ex as BuilderException;

                if(bex != null)
                    MessageBox.Show("Unable to convert project.  Reason: Error " + bex.ErrorCode +
                        ":" + bex.Message, Constants.AppName, MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                else
                    MessageBox.Show("Unable to convert project.  Reason: " + ex.Message, Constants.AppName,
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                this.Cursor = Cursors.Default;

                if(converter != null)
                    converter.Dispose();
            }
        }

        /// <summary>
        /// View help for this form
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnHelp_Click(object sender, EventArgs e)
        {
            Utility.ShowHelpTopic("f68822d2-97ba-48da-a98b-46747983b4a0");
        }
        #endregion
    }
}
