//===============================================================================================================
// System  : Sandcastle Guided Installation
// File    : VisualStudioInstance.cs
// Author  : Eric Woodruff
// Updated : 07/06/2025
// Note    : Copyright 2019-2025, Eric Woodruff, All rights reserved
//
// This file contains the class used to get a description and installation path for all installed version of
// Visual Studio.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/15/2019  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: Xml Env

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Setup.Configuration;

namespace Sandcastle.Installer
{
    /// <summary>
    /// This is used to get a description and installation path for all installed version of Visual Studio
    /// </summary>
    public class VisualStudioInstance
    {
        #region Private data members
        //=====================================================================

        private const int REGDB_E_CLASSNOTREG = unchecked((int)0x80040154);

        private static List<VisualStudioInstance> installedInstances;

        private static readonly object syncRoot = new();

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This returns an enumerable list of all installed Visual Studio instances
        /// </summary>
        /// <remarks>The instances are ordered by version, earliest to latest</remarks>
        public static IEnumerable<VisualStudioInstance> AllInstances
        {
            get
            {
                if(installedInstances == null)
                {
                    lock(syncRoot)
                    {
                        installedInstances ??= DetermineInstalledInstances();
                    }
                }

                return installedInstances;
            }
        }

        /// <summary>
        /// This read-only property is used to get the version number of the Visual Studio instance
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// This read-only property is used to get the display name of the Visual Studio instance
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// This read-only property is used to get the installation path of the Visual Studio instance
        /// </summary>
        public string InstallationPath { get; private set; }

        /// <summary>
        /// This read-only property is used to get the path to DevEnv.exe for the Visual Studio instance
        /// </summary>
        public string DevEnvPath { get; private set; }

        /// <summary>
        /// This read-only property is used to get the path to VSIXInstaller.exe for the Visual Studio instance
        /// </summary>
        public string VSIXInstallerPath { get; private set; }

        /// <summary>
        /// This read-only property is used to get the path to the extensions installed for all users for the
        /// Visual Studio instance.
        /// </summary>
        public string AllUsersExtensionsPath { get; private set; }

        /// <summary>
        /// This read-only property is used to get the path to the XML schema cache for the Visual Studio instance
        /// </summary>
        public string XmlSchemaCachePath { get; private set; }

        /// <summary>
        /// This read-only property is used to get the path to the user templates base folder for the Visual
        /// Studio instance (relative to the My Documents folder).
        /// </summary>
        public string UserTemplatesBaseFolder { get; private set; }

        /// <summary>
        /// This read-only property is used to determine whether or not the Sandcastle Help File Builder package
        /// is installed.
        /// </summary>
        /// <remarks>This only checks for the latest releases which install in the All Users extensions folder</remarks>
        public bool HelpFileBuilderIsInstalled => Directory.Exists(AllUsersExtensionsPath) &&
            Directory.EnumerateFiles(AllUsersExtensionsPath, "SandcastleBuilder.Package.dll",
                SearchOption.AllDirectories).Any();

        #endregion

        #region Private constructor
        //=====================================================================

        /// <summary>
        /// Private constructor
        /// </summary>
        private VisualStudioInstance()
        {
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to determine all installed Visual Studio instances
        /// </summary>
        /// <returns>A list of installed Visual Studio instances</returns>
        private static List<VisualStudioInstance> DetermineInstalledInstances()
        {
            var instances = new List<VisualStudioInstance>();

            try
            {
                // VS 2015 and earlier install to a single location pointed to by an environment variable
                string vsPath = Environment.ExpandEnvironmentVariables(@"%VS140COMNTOOLS%\..\IDE");

                if(!String.IsNullOrWhiteSpace(vsPath) && vsPath.IndexOf('%') == -1 &&
                  File.Exists(Path.Combine(vsPath, "devenv.exe")))
                {
                    vsPath = vsPath.Replace(@"\\..", @"\..");

                    instances.Add(new VisualStudioInstance
                    {
                        Version = "14.0",
                        DisplayName = "Visual Studio 2015",
                        InstallationPath = vsPath,
                        DevEnvPath = Path.Combine(vsPath, "devenv.exe"),
                        VSIXInstallerPath = Path.Combine(vsPath, "VSIXInstaller.exe"),
                        AllUsersExtensionsPath = Path.Combine(vsPath, "Extensions"),
                        XmlSchemaCachePath = Path.Combine(vsPath, @"..\..\Xml\Schemas"),
                        UserTemplatesBaseFolder = "Visual Studio 2015"
                    });
                }

                // VS 2017 and later install side-by-side and no longer user environment variables to point to
                // their location.  Instead, we query the list of installed versions and editions.
                var query2 = (ISetupConfiguration2)new SetupConfiguration();
                var e = query2.EnumAllInstances();
                var setupInstances = new ISetupInstance[1];

                int fetched;

                do
                {
                    e.Next(1, setupInstances, out fetched);

                    if(fetched > 0)
                    {
                        var instance2 = (ISetupInstance2)setupInstances[0];
                        var state = instance2.GetState();

                        if(state == InstanceState.Complete && (state & InstanceState.Local) == InstanceState.Local)
                        {
                            vsPath = instance2.GetInstallationPath();

                            string userTemplatesFolder = null;

                            if(instance2 is ISetupInstanceCatalog instanceCatalog)
                            {
                                var catalogProps = instanceCatalog.GetCatalogInfo();

                                if(catalogProps != null)
                                {
                                    var propNames = catalogProps.GetNames();

                                    if(propNames.Contains("productName") && propNames.Contains("productLineVersion"))
                                    {
                                        string productName = (string)catalogProps.GetValue("productName"),
                                            productLineVersion = (string)catalogProps.GetValue("productLineVersion");

                                        userTemplatesFolder = $"{productName} {productLineVersion}";
                                    }
                                }
                            }

                            instances.Add(new VisualStudioInstance
                            {
                                Version = instance2.GetInstallationVersion(),
                                DisplayName = instance2.GetDisplayName(),
                                InstallationPath = vsPath,
                                DevEnvPath = Path.Combine(vsPath, @"Common7\IDE\devenv.exe"),
                                VSIXInstallerPath = Path.Combine(vsPath, @"Common7\IDE\VSIXInstaller.exe"),
                                AllUsersExtensionsPath = Path.Combine(vsPath, @"Common7\IDE\Extensions"),
                                XmlSchemaCachePath = Path.Combine(vsPath, @"Xml\Schemas"),
                                UserTemplatesBaseFolder = userTemplatesFolder
                            });
                        }
                    }

                } while(fetched > 0);
            }
            catch(COMException ex) when(ex.HResult == REGDB_E_CLASSNOTREG)
            {
                // Ignore exceptions.  We'll just assume there are no higher versions installed.
                System.Diagnostics.Debug.WriteLine("The query API is not registered. Assuming no instances are installed.");
            }
            catch(Exception ex)
            {
                // Ignore exceptions.  We just won't list them.
                System.Diagnostics.Debug.WriteLine($"Error 0x{ex.HResult:x8}: {ex.Message}");
            }

            return [.. instances.OrderBy(i => i.Version).ThenBy(i => i.DisplayName)];
        }
        #endregion
    }
}
