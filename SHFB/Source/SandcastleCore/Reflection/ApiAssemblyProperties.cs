//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ApiAssemblyProperties.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/30/2021
// Note    : Copyright 2021, Eric Woodruff, All rights reserved
//
// This file contains the class used to contain API assembly properties
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/06/2021  EFW  Created the code
//===============================================================================================================

using System;
using System.Linq;
using System.Xml.Linq;

namespace Sandcastle.Core.Reflection
{
    /// <summary>
    /// This is used to contain information about an API assembly entry in a reflection information file
    /// </summary>
    public class ApiAssemblyProperties
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the assembly name
        /// </summary>
        public string AssemblyName { get; }

        /// <summary>
        /// This read-only property returns the assembly version
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// This read-only property returns whether or not the assembly allows partially trusted callers
        /// </summary>
        public bool AllowsPartiallyTrustedCallers { get; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assemblyInfo">The XML element containing the assembly properties</param>
        public ApiAssemblyProperties(XElement assemblyInfo)
        {
            if(assemblyInfo == null)
                throw new ArgumentNullException(nameof(assemblyInfo));

            this.AssemblyName = assemblyInfo.Attribute("name").Value;

            var attributes = assemblyInfo.Element("attributes");

            if(attributes != null)
            {
                // If an informational version is available, use it alone
                var versionAttr = attributes.Descendants("type").FirstOrDefault(
                    t => t.Attribute("api").Value.Equals("T:System.Reflection.AssemblyInformationalVersionAttribute",
                        StringComparison.Ordinal));

                if(versionAttr != null)
                    this.Version = versionAttr.Parent.Element("argument").Element("value").Value;
                else
                {
                    this.Version = assemblyInfo.Element("assemblydata").Attribute("version").Value;

                    // If a file version is available use it with the assembly version.  Otherwise, just use
                    // the assembly version.
                    versionAttr = attributes.Descendants("type").FirstOrDefault(
                        t => t.Attribute("api").Value.Equals("T:System.Reflection.AssemblyFileVersionAttribute",
                             StringComparison.Ordinal));

                    if(versionAttr != null)
                        this.Version += " (" + versionAttr.Parent.Element("argument").Element("value").Value + ")";
                }

                var aptca = attributes.Descendants("type").FirstOrDefault(
                        t => t.Attribute("api").Value.Equals("T:System.Security.AllowPartiallyTrustedCallersAttribute",
                             StringComparison.Ordinal));

                this.AllowsPartiallyTrustedCallers = (aptca != null);
            }
            else
                this.Version = assemblyInfo.Element("assemblydata")?.Attribute("version")?.Value ?? String.Empty;
        }
        #endregion
    }
}
