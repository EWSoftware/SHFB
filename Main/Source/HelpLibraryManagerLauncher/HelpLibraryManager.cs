//=============================================================================
// System  : Help Library Manager Launcher
// File    : HelpLibraryManager.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/24/2012
// Note    : Copyright 2010-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used to interact with the Help Library Manager.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  07/03/2010  EFW  Created the code
// 1.0.0.1  03/24/2012  EFW  Merged changes submitted by Don Fehr
//=============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace SandcastleBuilder.MicrosoftHelpViewer
{
    /// <summary>
    /// This class is used to interact with the Help Library Manager to install
    /// and uninstall Microsoft Help Viewer files.
    /// </summary>
    public class HelpLibraryManager
    {
        #region Private catalog information class
        //=====================================================================

        /// <summary>
        /// This is used internally to store information about the installed catalogs
        /// </summary>
        private class Catalog
        {
            /// <summary>Get the locale</summary>
            public string Locale { get; private set; }

            /// <summary>Get the product</summary>
            public string Product { get; private set; }

            /// <summary>Get the version</summary>
            public string Version { get; private set; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="product">The product</param>
            /// <param name="version">The version</param>
            /// <param name="locale">The locale</param>
            public Catalog(string product, string version, string locale)
            {
                this.Product = product;
                this.Version = version;
                this.Locale = locale;
            }
        }
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the path to the local store folder.
        /// </summary>
        public string LocalStorePath { get; private set; }

        /// <summary>
        /// This read-only property returns the path to the MS Help Viewer installation folder
        /// </summary>
        public string HelpViewerInstallPath { get; private set; }

        /// <summary>
        /// This read-only property returns the path to the Help Library Manager executable
        /// </summary>
        public string HelpLibraryManagerPath { get; private set; }

        /// <summary>
        /// This read-only property returns the path to the MS Help Viewer application
        /// </summary>
        public string HelpViewerPath { get; private set; }

        /// <summary>
        /// This read-only property is used to see if the local store has been initialized
        /// </summary>
        public bool LocalStoreInitialized
        {
            get { return (!String.IsNullOrEmpty(this.LocalStorePath) && Directory.Exists(this.LocalStorePath)); }
        }

        // TODO: Longer term, these may return different values from MS Help Viewer 2.0 (VS 2011)

        /// <summary>
        /// This read-only property returns The default MS Help Viewer product version
        /// </summary>
        public string DefaultCatalogProductVersion
        {
            get { return "100"; }
        }

        /// <summary>
        /// This read-only property returns the name of the default branding package used by MS Help Viewer
        /// </summary>
        public string DefaultBrandingPackage
        {
            get { return "Dev10"; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        // TODO: To support Help Viewer 2.0, this will most likely require a parameter that specifies which
        // version of the help viewer to use (1.0 or 2.0).  This will determine the registry key to use as well
        // as the property values returned above.

        /// <summary>
        /// Constructor
        /// </summary>
        public HelpLibraryManager()
        {
            string appRoot, appName;

            this.LocalStorePath = UnsafeNativeMethods.GetRegistryValue(@"SOFTWARE\Microsoft\Help\v1.0", "LocalStore");

            appRoot = UnsafeNativeMethods.GetRegistryValue(@"SOFTWARE\Microsoft\Help\v1.0", "AppRoot");

            if(appRoot != null)
            {
                if(Directory.Exists(appRoot))
                    this.HelpViewerInstallPath = appRoot;

                appName = Path.Combine(appRoot, "HelpLibManager.exe");

                if(File.Exists(appName))
                    this.HelpLibraryManagerPath = appName;

                appName = Path.Combine(appRoot, "HlpViewer.exe");

                if(File.Exists(appName))
                    this.HelpViewerPath = appName;
            }
        }
        #endregion

        #region Public helper methods
        //=====================================================================

        /// <summary>
        /// This is used to find the installed locale of the specified product
        /// version.
        /// </summary>
        /// <param name="product">The product for which to get the locale.</param>
        /// <param name="version">The version of the product for which to get the
        /// locale.</param>
        /// <returns>The locale found for the specified product.  If not found, it returns null.</returns>
        public string FindLocaleFor(string product, string version)
        {
            XmlDocument manifest;
            List<Catalog> allInstalledCatalogs = new List<Catalog>();
            string locale = null;

            if(String.IsNullOrEmpty(this.LocalStorePath))
                return null;

            // I suppose it's possible there may be more than one so we'll look at all of them
            foreach(string file in Directory.EnumerateFiles(Path.Combine(this.LocalStorePath, "manifest"),
              "queryManifest.*.xml"))
            {
                manifest = new XmlDocument();
                manifest.Load(file);

                foreach(XmlNode node in manifest.SelectNodes("/queryManifest/catalogs/catalog"))
                    allInstalledCatalogs.Add(new Catalog(node.Attributes["productId"].Value,
                        node.Attributes["productVersion"].Value, node.Attributes["productLocale"].Value));
            }

            // Look for the product and version to find the locale.  If, by chance, we
            // find more than one we'll use the last one found or "en-US" if present.
            foreach(Catalog catalog in allInstalledCatalogs)
                if(catalog.Product.Equals(product, StringComparison.OrdinalIgnoreCase) &&
                  catalog.Version.Equals(version, StringComparison.OrdinalIgnoreCase) &&
                  (locale == null || !locale.Equals("en-us", StringComparison.OrdinalIgnoreCase)))
                    locale = catalog.Locale.ToLowerInvariant();

            return locale;
        }

        /// <summary>
        /// This is used to see if the specified content file is installed
        /// </summary>
        /// <param name="contentFilename">The filename for which to check</param>
        /// <returns>True if the file is installed, false if not</returns>
        public bool HelpContentFileInstalled(string contentFilename)
        {
            XmlDocument manifest;
            string filename = Path.GetFileNameWithoutExtension(contentFilename);

            if(String.IsNullOrEmpty(this.LocalStorePath))
                return false;

            // Periods in the filename aren't allowed.  SHFB replaces them with an
            // underscore for the name in the setup file.
            if(filename.IndexOf('.') != -1)
                filename = filename.Replace(".", "_");

            filename += Path.GetExtension(contentFilename);

            // I suppose it's possible there may be more than one so we'll look at all of them
            foreach(string file in Directory.EnumerateFiles(Path.Combine(this.LocalStorePath, "manifest"),
              "queryManifest.*.xml"))
            {
                manifest = new XmlDocument();
                manifest.Load(file);

                if(manifest.SelectNodes("/queryManifest/catalogs/catalog/catalogSources//catalogSource/sourceFiles/" +
                  "sourceFile/contentFiles/contentFile[@fileName='" + filename + "']").Count != 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Run the Help Library Manager as a normal user
        /// </summary>
        /// <param name="arguments">The command line arguments to pass to it</param>
        /// <param name="windowStyle">The window style to use</param>
        /// <returns>The Help Library Manager exit code</returns>
        public int RunAsNormalUser(string arguments, ProcessWindowStyle windowStyle)
        {
            using(Process process = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    FileName = this.HelpLibraryManagerPath,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    WindowStyle = windowStyle
                }
            })
            {
                process.Start();
                process.WaitForExit();

                return process.ExitCode;
            }
        }

        /// <summary>
        /// Run the Help Library Manager as an administrator
        /// </summary>
        /// <param name="arguments">The command line arguments to pass to it</param>
        /// <param name="windowStyle">The window style to use</param>
        /// <returns>The Help Library Manager exit code</returns>
        public int RunAsAdministrator(string arguments, ProcessWindowStyle windowStyle)
        {
            using(Process process = new Process
            {
                StartInfo =
                {
                    UseShellExecute = true,
                    FileName = this.HelpLibraryManagerPath,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    WindowStyle = windowStyle
                }
            })
            {
                // If on Vista or above, force it to run as an administrator
                if(Environment.OSVersion.Version.Major >= 6)
                    process.StartInfo.Verb = "runas";

                process.Start();
                process.WaitForExit();

                return process.ExitCode;
            }
        }

        /// <summary>
        /// Launch the help file viewer for interactive use
        /// </summary>
        /// <param name="arguments">The command line arguments to pass to it</param>
        /// <remarks>This always runs it as an administrator on Vista and above.</remarks>
        public void LaunchInteractive(string arguments)
        {
            using(Process process = new Process
            {
                StartInfo =
                {
                    UseShellExecute = true,
                    FileName = this.HelpLibraryManagerPath,
                    Arguments = arguments,
                    WindowStyle = ProcessWindowStyle.Normal
                }
            })
            {
                // If on Vista or above, force it to run as an administrator
                if(Environment.OSVersion.Version.Major >= 6)
                    process.StartInfo.Verb = "runas";

                process.Start();
            }
        }
        #endregion
    }
}
