//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : CodeBlockConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/21/2012
// Note    : Copyright 2006-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a form that is used to configure the settings for the Code Block Component.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.3.3.0  11/24/2006  EFW  Created the code
// 1.4.0.0  02/12/2007  EFW  Added code block language filter option, default title option, and "Copy" image URL
// 1.6.0.5  03/05/2008  EFW  Added support for the keepSeeTags attribute
// 1.6.0.7  04/05/2008  EFW  JavaScript and XAMl are now treated as separate languages for proper language filter
//                           support.
// 1.8.0.0  08/15/2008  EFW  Added option to allow missing source/regions
// 1.8.0.1  01/23/2009  EFW  Added removeRegionMarkers option
// 1.9.6.0  10/21/2012  EFW  Removed obsolete options and moved options over from the Post-Transform Component
//                           configuration dialog box.
//===============================================================================================================

using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This form is used to configure the settings for the <see cref="CodeBlockComponent"/>
    /// </summary>
    internal partial class CodeBlockConfigDlg : Form
    {
        #region Private data members
        //=====================================================================

        private static string[] languages = { "none", "cs", "vbnet", "cpp", "c", "javascript", "jscriptnet",
            "jsharp", "vbscript", "xml", "xaml", "python", "sql", "pshell" };

        private XElement config;     // The configuration
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to return the configuration information
        /// </summary>
        public string Configuration
        {
            get { return config.ToString(); }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="currentConfig">The current XML configuration XML fragment</param>
        public CodeBlockConfigDlg(string currentConfig)
        {
            XElement node;
            XAttribute attr;

            InitializeComponent();

            cboLanguage.SelectedIndex = 1;
            lnkCodePlexSHFB.Links[0].LinkData = "http://SHFB.CodePlex.com";

            // Load the current settings
            config = XElement.Parse(currentConfig);
            node = config.Element("basePath");

            if(node != null)
                txtBasePath.Text = node.Attribute("value").Value;

            node = config.Element("allowMissingSource");

            if(node != null)
                chkAllowMissingSource.Checked = (bool)node.Attribute("value");

            node = config.Element("removeRegionMarkers");

            if(node != null)
                chkRemoveRegionMarkers.Checked = (bool)node.Attribute("value");

            node = config.Element("colorizer");

            txtSyntaxFile.Text = node.Attribute("syntaxFile").Value;
            txtXsltStylesheetFile.Text = node.Attribute("styleFile").Value;

            // These two may not be there for older configurations
            if(node.Attribute("stylesheet") != null)
                txtCssStylesheet.Text = node.Attribute("stylesheet").Value;
            else
                txtCssStylesheet.Text = @"{@SHFBFolder}Colorizer\highlight.css";

            if(node.Attribute("scriptFile") != null)
                txtScriptFile.Text = node.Attribute("scriptFile").Value;
            else
                txtScriptFile.Text = @"{@SHFBFolder}Colorizer\highlight.js";

            attr = node.Attribute("language");

            if(attr != null)
                for(int i = 0; i < languages.Length; i++)
                    if(attr.Value == languages[i])
                    {
                        cboLanguage.SelectedIndex = i;
                        break;
                    }

            attr = node.Attribute("tabSize");

            if(attr != null)
                udcTabSize.Value = (int)attr;

            attr = node.Attribute("numberLines");

            if(attr != null)
                chkNumberLines.Checked = (bool)attr;

            attr = node.Attribute("outlining");

            if(attr != null)
                chkOutlining.Checked = (bool)attr;

            attr = node.Attribute("keepSeeTags");

            if(attr != null)
                chkKeepSeeTags.Checked = (bool)attr;

            attr = node.Attribute("defaultTitle");

            if(attr != null)
                chkDefaultTitle.Checked = (bool)attr;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Close without saving
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Go to the CodePlex home page of the Sandcastle Help File Builder
        /// project.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lnkCodePlexSHFB_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start((string)e.Link.LinkData);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                MessageBox.Show("Unable to launch link target.  Reason: " + ex.Message,
                    "Sandcastle Help File Builder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            bool isValid = true;

            txtBasePath.Text = txtBasePath.Text.Trim();
            txtSyntaxFile.Text = txtSyntaxFile.Text.Trim();
            txtXsltStylesheetFile.Text = txtXsltStylesheetFile.Text.Trim();
            txtCssStylesheet.Text = txtCssStylesheet.Text.Trim();
            txtScriptFile.Text = txtScriptFile.Text.Trim();

            epErrors.Clear();

            if(txtSyntaxFile.Text.Length == 0)
            {
                epErrors.SetError(txtSyntaxFile, "The syntax filename is required");
                isValid = false;
            }

            if(txtXsltStylesheetFile.Text.Length == 0)
            {
                epErrors.SetError(txtXsltStylesheetFile, "The XSLT stylesheet filename is required");
                isValid = false;
            }

            if(txtCssStylesheet.Text.Length == 0)
            {
                epErrors.SetError(txtCssStylesheet, "The CSS stylesheet filename is required");
                isValid = false;
            }

            if(txtScriptFile.Text.Length == 0)
            {
                epErrors.SetError(txtScriptFile, "The script filename is required");
                isValid = false;
            }


            if(!isValid)
                return;

            // Store the changes
            config.RemoveNodes();

            config.Add(
                new XElement("basePath",
                    new XAttribute("value", txtBasePath.Text)),
                new XElement("outputPaths", "{@HelpFormatOutputPaths}"),
                new XElement("allowMissingSource",
                    new XAttribute("value", chkAllowMissingSource.Checked)),
                new XElement("removeRegionMarkers",
                    new XAttribute("value", chkRemoveRegionMarkers.Checked)),
                new XElement("colorizer",
                    new XAttribute("syntaxFile", txtSyntaxFile.Text),
                    new XAttribute("styleFile", txtXsltStylesheetFile.Text),
                    new XAttribute("stylesheet", txtCssStylesheet.Text),
                    new XAttribute("scriptFile", txtScriptFile.Text),
                    new XAttribute("language", languages[cboLanguage.SelectedIndex]),
                    new XAttribute("tabSize", (int)udcTabSize.Value),
                    new XAttribute("numberLines", chkNumberLines.Checked),
                    new XAttribute("outlining", chkOutlining.Checked),
                    new XAttribute("keepSeeTags", chkKeepSeeTags.Checked),
                    new XAttribute("defaultTitle", chkDefaultTitle.Checked)));

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Select the base source folder
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using(FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select the base source folder";
                dlg.SelectedPath = Directory.GetCurrentDirectory();

                // If selected, set the new folder
                if(dlg.ShowDialog() == DialogResult.OK)
                    txtBasePath.Text = dlg.SelectedPath + @"\";
            }
        }

        /// <summary>
        /// Select one of the file types
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void SelectFile_Click(object sender, EventArgs e)
        {
            Button b = sender as Button;
            TextBox t;

            using(OpenFileDialog dlg = new OpenFileDialog())
            {
                if(b == btnSelectSyntax)
                {
                    t = txtSyntaxFile;
                    dlg.Title = "Select the language syntax file";
                    dlg.Filter = "XML files (*.xml)|*.xml|All Files (*.*)|*.*";
                    dlg.DefaultExt = "xml";
                }
                else
                    if(b == btnSelectXsltStylesheet)
                    {
                        t = txtXsltStylesheetFile;
                        dlg.Title = "Select the XSL transformation file";
                        dlg.Filter = "XSL files (*.xsl, *.xslt)|*.xsl;*.xslt|All Files (*.*)|*.*";
                        dlg.DefaultExt = "xsl";
                    }
                    else
                        if(b == btnSelectCssStylesheet)
                        {
                            t = txtCssStylesheet;
                            dlg.Title = "Select the colorized code stylesheet file";
                            dlg.Filter = "Stylesheet files (*.css)|*.css|All Files (*.*)|*.*";
                            dlg.DefaultExt = "css";
                        }
                        else
                        {
                            t = txtScriptFile;
                            dlg.Title = "Select the colorized code JavaScript file";
                            dlg.Filter = "JavaScript files (*.js)|*.js|All Files (*.*)|*.*";
                            dlg.DefaultExt = "js";
                        }

                dlg.InitialDirectory = Directory.GetCurrentDirectory();

                // If selected, set the filename
                if(dlg.ShowDialog() == DialogResult.OK)
                    t.Text = dlg.FileName;
            }
        }
        #endregion
    }
}
