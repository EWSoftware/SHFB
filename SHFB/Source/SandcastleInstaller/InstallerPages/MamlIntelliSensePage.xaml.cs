//===============================================================================================================
// System  : Sandcastle Guided Installation
// File    : MamlIntelliSensePage.cs
// Author  : Eric Woodruff
// Updated : 07/06/2025
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

// Ignore Spelling: href

using System;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using System.Xml.Linq;

namespace Sandcastle.Installer.InstallerPages
{
    /// <summary>
    /// This page is used to help the user install the Sandcastle MAML schema files for use with Visual
    /// Studio IntelliSense.
    /// </summary>
    public partial class MamlIntelliSensePage : BasePage
    {
        #region Properties
        //=====================================================================

        /// <inheritdoc />
        public override string PageTitle => "MAML Schema IntelliSense";

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

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XElement configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if(configuration.Attribute("supportedVersions") == null)
                throw new InvalidOperationException("A supportedVersions attribute value is required");

            var supportedVersions = configuration.Attribute("supportedVersions").Value.Split([',', ' '],
                StringSplitOptions.RemoveEmptyEntries).ToList();

            // Load the possible versions, figure out if the schemas are already installed, and if we can
            // safely overwrite them.
            foreach(var vs in VisualStudioInstance.AllInstances)
            {
                if(supportedVersions.Any(v => vs.Version.StartsWith(v, StringComparison.Ordinal)))
                {
                    Paragraph para = new();
                    secResults.Blocks.Add(para);

                    para.Inlines.AddRange(new Inline[] { new Bold(new Run(vs.DisplayName)), new Run(" - ") });

                    try
                    {
                        XNamespace cns = "http://schemas.microsoft.com/xsd/catalog";
                        bool changed = false;
                        string catalogPath;

                        // Search for one of the schema files.  If found, look for the old catalog file.  If found,
                        // remove the folder as it is an old, manually installed copy of the schemas and we don't want
                        // it to interfere with the ones installed by the SHFB package.
                        foreach(string checkPath in Directory.GetFiles(vs.XmlSchemaCachePath, "developerStructure.xsd",
                          SearchOption.AllDirectories))
                        {
                            catalogPath = Path.Combine(Path.GetDirectoryName(checkPath), "catalog.xml");

                            if(File.Exists(catalogPath))
                            {
                                Directory.Delete(Path.GetDirectoryName(catalogPath), true);

                                para.Inlines.Add(new Run("Found and removed an old copy of the MAML schemas.  "));
                            }
                        }

                        catalogPath = Path.Combine(vs.XmlSchemaCachePath, "catalog.xml");

                        var rootCatalog = XDocument.Load(catalogPath);

                        // Check for old catalog entries using both path separators as people may have added it
                        // manually and used a backslash instead of a forward slash.  The VSIX package installs a
                        // catalog file in the root schemas folder which Visual Studio finds automatically so we
                        // don't need to modify the root catalog file anymore.
                        foreach(var oldCatalog in rootCatalog.Descendants(cns + "Catalog").Attributes("href").Where(
                          h => h.Value.EndsWith("MAML/catalog.xml", StringComparison.OrdinalIgnoreCase) ||
                          h.Value.EndsWith(@"MAML\catalog.xml", StringComparison.OrdinalIgnoreCase) ||
                          h.Value.IndexOf("%SHFBROOT%", StringComparison.OrdinalIgnoreCase) != -1).ToArray())
                        {
                            oldCatalog.Parent.Remove();
                            changed = true;
                        }

                        if(changed)
                        {
                            rootCatalog.Save(catalogPath);

                            para.Inlines.Add(new Run("Found and removed an old catalog entry for a prior " +
                                "version of the MAML schemas.  "));
                        }

                        para.Inlines.Add(new Run("The latest schemas have been installed for this version of Visual Studio."));
                    }
                    catch(DirectoryNotFoundException)
                    {
                        para.Inlines.Add(new Run("Unable to locate this version of Visual Studio."));
                    }
                    catch(Exception ex)
                    {
                        para.Inlines.Add(new Run("An unexpected error occurred while trying to search for and " +
                            "remove an old copy of the MAML schemas.  Error: " + ex.Message));
                    }
                }
            }

            base.Initialize(configuration);
        }
        #endregion
    }
}
