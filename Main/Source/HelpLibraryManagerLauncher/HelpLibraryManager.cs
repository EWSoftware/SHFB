//=============================================================================
// System  : Help Library Manager Launcher
// File    : HelpLibraryManager.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/06/2010
// Note    : Copyright 2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used to interact with the Help Library Manager.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://www.CodePlex.com/SandcastleStyles.   This
// notice, the author's name, and all copyright notices must remain intact in
// all applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  07/03/2010  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
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
            /// <param name="product"></param>
            /// <param name="version"></param>
            /// <param name="locale"></param>
            public Catalog(string product, string version, string locale)
            {
                this.Product = product;
                this.Version = version;
                this.Locale = locale;
            }
        }
        #endregion

        #region Registry access definitions and external methods
        //=====================================================================

        private static readonly UIntPtr HKEY_LOCAL_MACHINE = (UIntPtr)0x80000002; // Local Machine key
        private const int KEY_WOW64_64KEY = 0x100;  // Access the 64-bit registry
        private const int KEY_WOW64_32KEY = 0x200;  // Access the 32-bit registry
        private const int KEY_READ = 0x20019;       // Read only access

        // Win32 API registry access methods
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegOpenKeyExW", SetLastError = true)]
        private static extern int RegOpenKeyEx(UIntPtr hKey, string lpSubKey, uint ulOptions, int samDesired, out UIntPtr phkResult);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW", SetLastError = true)]
        private static extern int RegQueryValueEx(UIntPtr hKey, string lpValueName, IntPtr lpReserved, out uint lpType,
            System.Text.StringBuilder lpData, ref uint lpcbData);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegCloseKey(UIntPtr hKey);
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the path to the local store folder.
        /// </summary>
        public string LocalStorePath { get; private set; }

        /// <summary>
        /// This read-only property returns the path to the Help Library Manager
        /// executable.
        /// </summary>
        public string HelpLibraryManagerPath { get; private set; }

        /// <summary>
        /// This read-only property is used to see if the local store has been
        /// initialized.
        /// </summary>
        public bool LocalStoreInitialized
        {
            get { return (!String.IsNullOrEmpty(this.LocalStorePath) && Directory.Exists(this.LocalStorePath)); }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public HelpLibraryManager()
        {
            string appRoot;

            this.LocalStorePath = GetRegistryValue(@"SOFTWARE\Microsoft\Help\v1.0", "LocalStore");

            appRoot = GetRegistryValue(@"SOFTWARE\Microsoft\Help\v1.0", "AppRoot");

            if(appRoot != null)
            {
                appRoot = Path.Combine(appRoot, "HelpLibManager.exe");

                if(File.Exists(appRoot))
                    this.HelpLibraryManagerPath = appRoot;
            }
        }
        #endregion

        #region Private helper methods
        //=====================================================================

        /// <summary>
        /// This is used to try and find the specified regsitry key value in the 64 bit part
        /// of the registry. If not found, an attempt is made to try and find it in the 32 bit
        /// part of the registry.
        /// </summary>
        /// <param name="registryKeyPath">The registry key path to find</param>
        /// <param name="valueName">The value name to find</param>
        /// <returns>The value in the named registry key value</returns>
        /// <remarks>This method uses an API call that is unsupported on Windows 2000.</remarks>
        private static string GetRegistryValue(string registryKeyPath, string valueName)
        {
            UIntPtr regKeyHandle = UIntPtr.Zero;
            string value = null;
            uint size = 4096;
            uint type;
            StringBuilder keyBuffer = new StringBuilder((int)size);

            if(String.IsNullOrEmpty(registryKeyPath))
                throw new ArgumentNullException("registryKeyPath", "registryKeyPath cannot be null or empty");

            if(String.IsNullOrEmpty(valueName))
                throw new ArgumentNullException("valueName", "valueName cannot be null or empty");

            try
            {
                // See if the registry key can be found in the 64 bit registry
                if(RegOpenKeyEx(HKEY_LOCAL_MACHINE, registryKeyPath, 0, KEY_READ | KEY_WOW64_64KEY,
                  out regKeyHandle) == 0)
                {
                    // See if the value exists
                    if(RegQueryValueEx(regKeyHandle, valueName, IntPtr.Zero, out type, keyBuffer, ref size) == 0)
                        value = keyBuffer.ToString();
                }
                else
                {
                    // See if the registry key can be found in the 32 bit registry
                    if(RegOpenKeyEx(HKEY_LOCAL_MACHINE, registryKeyPath, 0, KEY_READ | KEY_WOW64_32KEY,
                      out regKeyHandle) == 0)
                    {
                        // See if the value exists
                        if(RegQueryValueEx(regKeyHandle, valueName, IntPtr.Zero, out type, keyBuffer, ref size) == 0)
                            value = keyBuffer.ToString();
                    }
                }
            }
            finally
            {
                if(regKeyHandle != UIntPtr.Zero)
                    RegCloseKey(regKeyHandle);
            }

            return value;
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
        /// <returns></returns>
        public string FindLocaleFor(string product, string version)
        {
            XmlDocument manifest;
            List<Catalog> allInstalledCatalogs = new List<Catalog>();
            string locale = null;

            if(String.IsNullOrEmpty(this.LocalStorePath))
                return null;

            // I suppose it's possible there may be more than one so we'll look at all of them
            foreach(string file in Directory.GetFiles(Path.Combine(this.LocalStorePath, "manifest"),
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
            foreach(string file in Directory.GetFiles(Path.Combine(this.LocalStorePath, "manifest"),
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
