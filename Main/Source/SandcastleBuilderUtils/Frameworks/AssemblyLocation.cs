//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : AssemblyLocation.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/10/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is used to contain information about a location and the assemblies for a .NET
// Framework version.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.   This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.5.0  09/09/2012  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace SandcastleBuilder.Utils.Frameworks
{
    /// <summary>
    /// This class defines the settings for a .NET Framework assembly location
    /// </summary>
    public sealed class AssemblyLocation
    {
        #region Private data members
        //=====================================================================

        private List<AssemblyDetails> assemblyDetails;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the path to the assembly location
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// This read-only property determine if this is the core assembly location
        /// </summary>
        /// <value>True if it is the core location, false if not</value>
        public bool IsCoreLocation { get; private set; }

        /// <summary>
        /// This read-only property returns an enumerable list of assembly details for this location
        /// </summary>
        public IEnumerable<AssemblyDetails> Assemblies
        {
            get { return assemblyDetails; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Private constructor
        /// </summary>
        private AssemblyLocation()
        {
            assemblyDetails = new List<AssemblyDetails>();
        }
        #endregion

        #region Methods used to convert to/from XML
        //=====================================================================

        /// <summary>
        /// This is used to load the settings for an assembly location from an XML element
        /// </summary>
        /// <param name="location">The XML element containing the settings</param>
        /// <returns>The new assembly location item</returns>
        /// <remarks>If the location element is empty, the assembly details will be created by scanning the
        /// location for assemblies.</remarks>
        internal static AssemblyLocation FromXml(XElement location)
        {
            string path = Environment.ExpandEnvironmentVariables(location.Attribute("Path").Value);

            // If x86 but it didn't exist, assume it's a 32-bit system and change the name
            if(path.IndexOf("%ProgramFiles(x86)%", StringComparison.Ordinal) != -1)
                path = Environment.ExpandEnvironmentVariables(path.Replace("(x86)", String.Empty));

            AssemblyLocation al = new AssemblyLocation
            {
                Path = path,
                IsCoreLocation = ((bool?)location.Attribute("IsCore") ?? false)
            };

            foreach(var a in location.Descendants("AssemblyDetails"))
                al.assemblyDetails.Add(AssemblyDetails.FromXml(path, a));

            return al;
        }

        /// <summary>
        /// This is used to convert the assembly location to an XML element
        /// </summary>
        /// <returns>The assembly location as an XML element</returns>
        internal XElement ToXml()
        {
            XElement e = new XElement("Location", new[] {
                this.IsCoreLocation ? new XAttribute("IsCore", this.IsCoreLocation) : null,
                new XAttribute("Path", this.Path)
            });

            e.Add(this.Assemblies.Select(a => a.ToXml()));

            return e;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This can be used to load an empty location with information about the assemblies it contains
        /// </summary>
        /// <remarks>If the location already has assembly details, it will do nothing.</remarks>
        public void DetermineAssemblyDetails()
        {
            if(assemblyDetails.Count == 0)
                foreach(string assembly in Directory.EnumerateFiles(this.Path, "*.dll").Concat(
                  Directory.EnumerateFiles(this.Path, "*.winmd")))
                {
                    try
                    {
                        assemblyDetails.Add(AssemblyDetails.FromAssemblyName(
                            AssemblyName.GetAssemblyName(assembly)));
                    }
                    catch(BadImageFormatException)
                    {
                        // Ignore, not a .NET assembly
                    }
                }
        }
        #endregion
    }
}
