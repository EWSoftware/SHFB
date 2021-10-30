//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : NuGetPackageManager.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/10/2021
// Note    : Copyright 2021, Eric Woodruff, All rights reserved
//
// This file contains a class that is used to implement a simple NuGet package manager
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/06/2021  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: http

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace SandcastleBuilder.WPF.NuGet
{
    /// <summary>
    /// This is a simple NuGet package manager
    /// </summary>
    public static class NuGetPackageManager
    {
        #region Private data members and constants
        //=====================================================================

        private const string NuGetServiceIndexUrl = "https://api.nuget.org/v3/index.json";

        #endregion

        /// <summary>
        /// This read-only property returns an enumerable list of the NuGet package sources available on the
        /// current system.
        /// </summary>
        public static IEnumerable<INuGetPackageSource> PackageSources
        {
            get
            {
                int packageSourceCount = 0;

                string nuGetConfig = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "NuGet", "NuGet.config");

                if(File.Exists(nuGetConfig))
                {
                    var config = XDocument.Load(nuGetConfig);

                    foreach(var ps in config.Root.Element("packageSources").Descendants("add"))
                    {
                        string name = ps.Attribute("key").Value, location = ps.Attribute("value").Value;

                        if(location.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                            yield return new NuGetOrgPackageSource(name, location);
                        else
                            yield return new LocalPackageSource(name, location);

                        packageSourceCount++;
                    }
                }

                if(packageSourceCount == 0)
                    yield return new NuGetOrgPackageSource("nuget.org", NuGetServiceIndexUrl);
            }
        }
    }
}
