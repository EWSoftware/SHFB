//===============================================================================================================
// System  : Sandcastle Guided Installation
// File    : MamlIntelliSensePage.cs
// Author  : Eric Woodruff
// Updated : 04/07/2017
// Compiler: Microsoft Visual C#
//
// This file contains a page used to help the user install the Sandcastle MAML schema files for use with Visual
// Studio IntelliSense.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/05/2011  EFW  Created the code
// 04/14/2012  EFW  Converted to use WPF
//===============================================================================================================

using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml.Linq;

namespace Sandcastle.Installer.InstallerPages
{
    /// <summary>
    /// This page is used to help the user install the Sandcastle MAML schema files for use with Visual
    /// Studio IntelliSense.
    /// </summary>
    public partial class MamlIntelliSensePage : BasePage
    {
        #region Private data members
        //=====================================================================

        private string sandcastleSchemaFolder;

        #endregion

        #region Properties
        //=====================================================================

        /// <inheritdoc />
        public override string PageTitle
        {
            get { return "MAML Schema IntelliSense"; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public MamlIntelliSensePage()
        {
            InitializeComponent();
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to see if it is safe to install the MAML schemas in the given version of Visual
        /// Studio.
        /// </summary>
        /// <param name="vsVersionName">The Visual Studio version name</param>
        /// <param name="vsPath">The path to the XML schema cache for the version of Visual Studio</param>
        /// <returns>True if it is safe, false if it is not safe to install the schemas in the given
        /// version of Visual Studio.</returns>
        private bool CheckForSafeInstallation(string vsVersionName, ref string vsPath)
        {
            Paragraph para;
            string checkPath;

            para = new Paragraph();
            secResults.Blocks.Add(para);

            para.Inlines.AddRange(new Inline[] { new Bold(new Run(vsVersionName)), new Run(" - ") });

            if(vsPath != null)
                vsPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(vsPath));

            try
            {
                // Check for the folder.  Also check for devenv.exe as some tools install under the Visual Studio
                // folder and create the environment variable even though the IDE is not there.
                if(String.IsNullOrEmpty(vsPath) || !Directory.Exists(vsPath) || !File.Exists(Path.Combine(
                  vsPath, @"..\..\Common7\IDE\devenv.exe")))
                {
                    para.Inlines.Add(new Run("Unable to locate this version of Visual Studio."));
                    return false;
                }

                // Search for one of the schema files.  If not found, assume it's safe to install them unless
                // the folder doesn't exist.  It has been known to get by the check above for some reason.
                checkPath = Directory.GetFiles(vsPath, "developerStructure.xsd",
                    SearchOption.AllDirectories).FirstOrDefault();
            }
            catch(DirectoryNotFoundException )
            {
                para.Inlines.Add(new Run("Unable to locate this version of Visual Studio."));
                return false;
            }

            if(checkPath != null)
            {
                checkPath = Path.GetDirectoryName(checkPath);

                // If found but in an unexpected location, prevent installation
                if(!checkPath.EndsWith("MAML", StringComparison.OrdinalIgnoreCase))
                {
                    para.Inlines.Add(new Run("The MAML schemas appear to be present but in a different " +
                        "location in the global schema cache.  To avoid problems, installation of the " +
                        "schemas from this package will not be allowed for this version of Visual Studio."));
                    return false;
                }

                // If in the expected location but our catalog file wasn't found, prevent installation
                if(!File.Exists(Path.Combine(checkPath, "catalog.xml")))
                {
                    para.Inlines.Add(new Run("The MAML schemas appear to be present in the expected " +
                        "location but the catalog file used by this package could not be found.  To " +
                        "avoid problems, installation of the schemas from this package will not be " +
                        "allowed for this version of Visual Studio."));
                    return false;
                }
            }

            para.Inlines.Add(new Run("The schemas can be installed for this version of Visual Studio."));
            return true;
        }

        /// <summary>
        /// This is used to install the MAML schemas in the specified Visual Studio global schema cache
        /// </summary>
        /// <param name="vsPath">The path to which the schemas are installed</param>
        /// <returns>True if successful, false if not</returns>
        private bool InstallMamlSchemas(string vsPath)
        {
            XNamespace cns = "http://schemas.microsoft.com/xsd/catalog";
            XDocument schemaDoc;
            string filePath;
            bool changed = false;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                // If this folder exists, remove it.  We no longer install the actual schema files in the Visual
                // Studio cache.  We just update the catalog to reference them in the SHFBROOT location.  That
                // way, the most current versions will always be used.
                filePath = Path.Combine(vsPath, "MAML");

                if(Directory.Exists(filePath))
                    Directory.Delete(filePath, true);

                // Add the reference to our catalog in the main Visual Studio schema catalog if it is not
                // already there.
                filePath = Path.Combine(vsPath, "catalog.xml");
                schemaDoc = XDocument.Load(filePath);

                // Check for the old catalog entry using both path separators as people may have added it
                // manually and used a backslash instead of a forward slash.
                var oldCatalog = schemaDoc.Descendants(cns + "Catalog").Attributes("href").FirstOrDefault(
                    h => h.Value.EndsWith("MAML/catalog.xml", StringComparison.OrdinalIgnoreCase) ||
                    h.Value.EndsWith(@"MAML\catalog.xml", StringComparison.OrdinalIgnoreCase));

                if(oldCatalog != null)
                {
                    oldCatalog.Parent.Remove();
                    changed = true;
                }

                if(!schemaDoc.Descendants(cns + "Catalog").Attributes("href").Any(
                  h => h.Value.IndexOf("%SHFBROOT%Schemas/Authoring", StringComparison.OrdinalIgnoreCase) != -1))
                {
                    schemaDoc.Root.Add(new XElement(cns + "Catalog",
                        new XAttribute("href", "%SHFBROOT%Schemas/Authoring/catalog.xml")));
                    changed = true;
                }

                if(changed)
                    schemaDoc.Save(filePath);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to install the Sandcastle MAML schemas:\r\n\r\n" + ex.Message,
                    this.PageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

            return true;
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XElement configuration)
        {
            CheckBox cb;
            string versionName, location;

            // Load the possible versions, figure out if the schemas are already installed, and if we can
            // safely overwrite them.
            foreach(var vs in configuration.Elements("visualStudio"))
            {
                versionName = vs.Attribute("version").Value;
                location = vs.Attribute("location").Value;

                if(!Environment.Is64BitProcess && location.IndexOf("(x86)%", StringComparison.Ordinal) != -1)
                    location = location.Replace("(x86)%", "%");

                cb = new CheckBox { Margin = new Thickness(20, 5, 0, 0) };
                cb.Content = versionName;
                cb.IsEnabled = this.CheckForSafeInstallation(versionName, ref location);
                cb.IsChecked = cb.IsEnabled;
                cb.Tag = location;

                pnlVersions.Children.Add(cb);
            }

            if(pnlVersions.Children.Count == 0)
                throw new InvalidOperationException("At least one visualStudio element must be defined");

            base.Initialize(configuration);
        }

        /// <inheritdoc />
        public override void ShowPage()
        {
            // SHFBROOT will exist as a system environment variable if it is installed correctly
            sandcastleSchemaFolder = Environment.GetEnvironmentVariable("SHFBROOT", EnvironmentVariableTarget.Machine);

            // It may not be there if we just installed it so look for the folder manually
            if(String.IsNullOrEmpty(sandcastleSchemaFolder))
            {
                sandcastleSchemaFolder = Path.Combine(Environment.GetFolderPath(Environment.Is64BitProcess ?
                    Environment.SpecialFolder.ProgramFilesX86 : Environment.SpecialFolder.ProgramFiles),
                    @"EWSoftware\Sandcastle Help File Builder");

                if(!Directory.Exists(sandcastleSchemaFolder))
                    sandcastleSchemaFolder = null;
            }

            if(!String.IsNullOrEmpty(sandcastleSchemaFolder))
                sandcastleSchemaFolder = Path.Combine(sandcastleSchemaFolder, @"Schemas\Authoring");

            if(String.IsNullOrEmpty(sandcastleSchemaFolder) || !Directory.Exists(sandcastleSchemaFolder))
            {
                secResults.Blocks.Clear();
                secResults.Blocks.Add(new Paragraph(
                    new Bold(new Run("Unable to locate the Sandcastle installation folder"))));
                pnlVersions.IsEnabled = btnInstallSchemas.IsEnabled = false;
            }

            base.ShowPage();
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Install the Sandcastle MAML schema files
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnInstallSchemas_Click(object sender, EventArgs e)
        {
            var folders = pnlVersions.Children.OfType<CheckBox>().Where(cb => cb.IsChecked.Value).Select(
                cb => (string)cb.Tag).ToList();

            if(folders.Count == 0)
            {
                MessageBox.Show("Select at least one version of Visual Studio into which the MAML schemas " +
                    "will be installed", this.PageTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if(MessageBox.Show("This will install the Sandcastle MAML schemas into the global schema cache " +
              "for the selected versions of Visual Studio.  Click OK to continue or CANCEL to stop.",
              this.PageTitle, MessageBoxButton.OKCancel, MessageBoxImage.Information,
              MessageBoxResult.Cancel) == MessageBoxResult.Cancel)
                return;

            foreach(string f in folders)
                if(!this.InstallMamlSchemas(f))
                    return;

            MessageBox.Show("The Sandcastle MAML schemas have been installed successfully.", this.PageTitle,
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion
    }
}
