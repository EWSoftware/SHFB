//===============================================================================================================
// System  : Help Library Manager Launcher
// File    : HelpLibraryManager.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/29/2016
// Note    : Copyright 2010-2016, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used to interact with the Help Library Manager.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/03/2010  EFW  Created the code
// 03/24/2012  EFW  Merged changes submitted by Don Fehr
// 10/05/2012  EFW  Added support for Help Viewer 2.0
// 03/03/2014  EFW  Fixed FindLocaleFor() so that it works properly when multiple languages are present
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace SandcastleBuilder.MicrosoftHelpViewer
{
    /// <summary>
    /// This class is used to interact with the Help Library Manager to install and uninstall Microsoft Help
    /// Viewer files.
    /// </summary>
    public class HelpLibraryManager
    {
        #region Private data members
        //=====================================================================

        private Version viewerVersion;
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
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <remarks>Help Viewer 1.0 will be used</remarks>
        public HelpLibraryManager() : this(new Version(1, 0))
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="version">The help viewer version to use for all operations</param>
        public HelpLibraryManager(Version version)
        {
            string appRoot, appName, registryPath = @"SOFTWARE\Microsoft\Help\v" + version.ToString();

            viewerVersion = version;

            if(viewerVersion.Major == 1)
                this.LocalStorePath = UnsafeNativeMethods.GetRegistryValue(registryPath, "LocalStore");
            else
                this.LocalStorePath = UnsafeNativeMethods.GetRegistryValue(registryPath + @"\Catalogs",
                    "ContentStore");

            appRoot = UnsafeNativeMethods.GetRegistryValue(registryPath, "AppRoot");

            if(appRoot != null)
            {
                if(Directory.Exists(appRoot))
                    this.HelpViewerInstallPath = appRoot;

                if(version.Major == 1)
                    appName = Path.Combine(appRoot, "HelpLibManager.exe");
                else
                    appName = Path.Combine(appRoot, "HlpCtntMgr.exe");

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
        /// This is used to get the default MS Help Viewer 2.x catalog name based on the help viewer version
        /// </summary>
        /// <param name="viewerVersion">The help viewer version</param>
        /// <returns>The default catalog name for the related help viewer version or null if it could not be
        /// determined.</returns>
        public static string DefaultCatalogName(Version viewerVersion)
        {
            switch(viewerVersion.Minor)
            {
                case 0:     // Visual Studio 2012
                    return "VisualStudio11";

                case 1:     // Visual Studio 2013
                    return "VisualStudio12";

                case 2:     // Visual Studio 2015
                    return "VisualStudio14";

                case 3:     // Visual Studio 2016
                    return "VisualStudio15";

                default:
                    return null;
            }
        }

        /// <summary>
        /// This is used to find the installed locale of the specified product version for Help Viewer 1.0
        /// </summary>
        /// <param name="product">The product for which to get the locale.</param>
        /// <param name="version">The version of the product for which to get the locale.</param>
        /// <returns>The locale found for the specified product.  If not found, it returns null.</returns>
        public string FindLocaleFor(string product, string version)
        {
            XmlDocument manifest;
            Dictionary<string, int> allCatalogLocales = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            string path, productId, productVersion, productLocale;
            int sourceFileCount, currentCount;

            if(String.IsNullOrEmpty(this.LocalStorePath))
                return null;

            path = Path.Combine(this.LocalStorePath, "manifest");

            if(!Directory.Exists(path))
                return null;

            // I suppose it's possible there may be more than one so we'll look at all of them
            foreach(string file in Directory.EnumerateFiles(path, "queryManifest.*.xml"))
            {
                manifest = new XmlDocument();
                manifest.Load(file);

                foreach(XmlNode node in manifest.SelectNodes("/queryManifest/catalogs/catalog"))
                {
                    productId = node.Attributes["productId"].Value;
                    productVersion = node.Attributes["productVersion"].Value;

                    if(product.Equals(productId, StringComparison.OrdinalIgnoreCase) &&
                        version.Equals(productVersion, StringComparison.OrdinalIgnoreCase))
                    {
                        productLocale = node.Attributes["productLocale"].Value;
                        sourceFileCount = node.SelectNodes("catalogSources/catalogSource/sourceFiles/sourceFile").Count;

                        if(!allCatalogLocales.TryGetValue(productLocale, out currentCount))
                            currentCount = 0;

                        allCatalogLocales[productLocale] = currentCount + sourceFileCount;
                    }
                }
            }

            if(allCatalogLocales.Count == 0)
                return null;

            // On systems with a different language installed, we can still get en-US as well.  If we've got more
            // than one language, take the one with the most source files as it's most likely the language of
            // choice.
            int maxSourceFileCount = allCatalogLocales.Max(c => c.Value);

            // It's a long shot, but if we end up with two or more with the same count, pick the first one that
            // isn't en-US.  Failing that, just return the first one.
            var matches = allCatalogLocales.Where(c => c.Value == maxSourceFileCount).ToList();

            if(matches.Count > 1 && matches.Any(c => !c.Key.Equals("en-US")))
                return matches.First(c => !c.Key.Equals("en-US")).Key;

            return matches.First().Key;
        }

        /// <summary>
        /// This is used to find the installed locale of the specified product version for Help Viewer 2.x
        /// </summary>
        /// <param name="catalogName">The catalogName for which to get the locale</param>
        /// <returns>The locale found for the specified catalog.  If not found, it returns null.</returns>
        public string FindLocaleFor(string catalogName)
        {
            XmlDocument manifest;
            Dictionary<string, int> allCatalogLocales = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            string path, bookLocale;
            int bookCount, currentCount;

            if(String.IsNullOrEmpty(this.LocalStorePath))
                return null;

            path = Path.Combine(this.LocalStorePath, catalogName + @"\ContentStore");

            if(!Directory.Exists(path))
                return null;

            // I suppose it's possible there may be more than one so we'll look at all of them
            foreach(string file in Directory.EnumerateFiles(path, "installedBooks.*.xml"))
            {
                manifest = new XmlDocument();
                manifest.Load(file);

                foreach(XmlNode node in manifest.SelectNodes("/installed-books/locale-membership/locale"))
                {
                    bookLocale = node.Attributes["name"].Value;
                    bookCount = node.SelectNodes("book-list/book").Count;

                    if(!allCatalogLocales.TryGetValue(bookLocale, out currentCount))
                        currentCount = 0;

                    allCatalogLocales[bookLocale] = currentCount + bookCount;
                }
            }

            if(allCatalogLocales.Count == 0)
                return null;

            // On systems with a different language installed, we can still get en-US as well.  If we've got more
            // than one language, take the one with the most source files as it's most likely the language of
            // choice.
            int maxSourceFileCount = allCatalogLocales.Max(c => c.Value);

            // It's a long shot, but if we end up with two or more with the same count, pick the first one that
            // isn't en-US.  Failing that, just return the first one.
            var matches = allCatalogLocales.Where(c => c.Value == maxSourceFileCount).ToList();

            if(matches.Count > 1 && matches.Any(c => !c.Key.Equals("en-US")))
                return matches.First(c => !c.Key.Equals("en-US")).Key;

            return matches.First().Key;
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

            // Periods in the filename aren't allowed.  SHFB replaces them with an underscore for the name in
            // the setup file.
            if(filename.IndexOf('.') != -1)
                filename = filename.Replace(".", "_");

            if(viewerVersion.Major == 1)
            {
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
            }
            else
            {
                // I suppose it's possible there may be more than one so we'll look at all of them
                foreach(string file in Directory.EnumerateFiles(this.LocalStorePath, "installedBooks.*.xml",
                  SearchOption.AllDirectories))
                {
                    manifest = new XmlDocument();
                    manifest.Load(file);

                    foreach(XmlNode r in manifest.SelectNodes("/installed-books/locale-membership/locale" +
                      "/book-membership/book/package/@ref"))
                        if(r.Value.Equals(filename, StringComparison.OrdinalIgnoreCase))
                            return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Run the Help Library Content Manager as a normal user
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
        /// Run the Help Library Content Manager as an administrator
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
        /// Launch the help content manager for interactive use
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
                    FileName = (viewerVersion.Major == 1) ? this.HelpLibraryManagerPath : this.HelpViewerPath,
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
