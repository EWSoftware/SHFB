//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : MefProviderOptions.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/08/2014
// Note    : Copyright 2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to contain the MEF provider configuration settings
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
//===============================================================================================================
// 12/08/2014  EFW  Created the code
//===============================================================================================================

using System;
using System.IO;
using System.Xml.Linq;

namespace SandcastleBuilder.Package
{
    /// <summary>
    /// This class is used to contain the MEF provider configuration options
    /// </summary>
    /// <remarks>Settings are stored in an XML file in the user's local application data folder and will be used
    /// by all versions of Visual Studio in which the package is installed.  These are separate from the main
    /// package options but are editable using the package options page.  Since these are not directly related to
    /// the package, we don't want to force it to load just to access these few settings.</remarks>
    internal static class MefProviderOptions
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the configuration file path
        /// </summary>
        public static string ConfigurationFilePath
        {
            get
            {
                string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"EWSoftware\Sandcastle Help File Builder");

                if(!Directory.Exists(configPath))
                    Directory.CreateDirectory(configPath);

                return configPath;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not the extended XML comments completion source options are
        /// enabled.
        /// </summary>
        /// <value>This is true by default</value>
        public static bool EnableExtendedXmlCommentsCompletion{ get; set; }

        /// <summary>
        /// This is used to get or set whether or not the MAML and XML comments element Go To Definition and tool
        /// tip option is enabled.
        /// </summary>
        /// <value>This is true by default</value>
        public static bool EnableGoToDefinition { get; set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Static constructor
        /// </summary>
        static MefProviderOptions()
        {
            if(!LoadConfiguration())
                ResetConfiguration(false);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to load the MEF provider configuration settings
        /// </summary>
        /// <returns>True if loaded successfully or false if the file does not exist or could not be loaded</returns>
        /// <remarks>The settings are loaded from <strong>MefProviderOptions.config</strong> in the
        /// <see cref="ConfigurationFilePath"/> folder.</remarks>
        private static bool LoadConfiguration()
        {
            string filename = Path.Combine(ConfigurationFilePath, "MefProviderOptions.config");
            bool success = true;

            try
            {
                if(!File.Exists(filename))
                    return false;

                var root = XDocument.Load(filename).Root;

                EnableExtendedXmlCommentsCompletion = (root.Element("EnableExtendedXmlCommentsCompletion") != null);
                EnableGoToDefinition = (root.Element("EnableGoToDefinition") != null);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// This is used to save the MEF provider configuration settings
        /// </summary>
        /// <remarks>The settings are saved to <strong>MefProviderOptions.config</strong> in the
        /// <see cref="ConfigurationFilePath"/> folder.</remarks>
        public static bool SaveConfiguration()
        {
            string filename = Path.Combine(ConfigurationFilePath, "MefProviderOptions.config");
            bool success = true;

            try
            {
                XElement root = new XElement("MefProviderOptions",
                    EnableExtendedXmlCommentsCompletion ? new XElement("EnableExtendedXmlCommentsCompletion") : null,
                    EnableGoToDefinition ? new XElement("EnableGoToDefinition") : null);

                root.Save(filename);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// This is used to reset the configuration to its default state
        /// </summary>
        /// <param name="deleteConfigurationFile">True to delete the configuration file if it exists, false to
        /// just set the default values</param>
        public static void ResetConfiguration(bool deleteConfigurationFile)
        {
            EnableExtendedXmlCommentsCompletion = EnableGoToDefinition = true;

            if(deleteConfigurationFile)
            {
                string filename = Path.Combine(ConfigurationFilePath, "MefProviderOptions.config");

                try
                {
                    if(File.Exists(filename))
                        File.Delete(filename);
                }
                catch(Exception ex)
                {
                    // Ignore exception encountered while trying to delete the configuration file
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
        }
        #endregion
    }
}
