//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : AssemblyDetails.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/10/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is used to contain settings for a .NET Framework assembly.
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace SandcastleBuilder.Utils.Frameworks
{
    /// <summary>
    /// This class is used to hold the details for a .NET Framework assembly
    /// </summary>
    public sealed class AssemblyDetails
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the assembly filename
        /// </summary>
        public string Filename { get; private set; }

        /// <summary>
        /// This read-only property returns the assembly name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// This read-only property is used to get the version for the assembly
        /// </summary>
        public Version Version { get; private set; }

        /// <summary>
        /// This read-only property is used to get the culture for the assembly
        /// </summary>
        public string Culture { get; private set; }

        /// <summary>
        /// This read-only property returns the public key token for the assembly
        /// </summary>
        public string PublicKeyToken { get; private set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Private constructor
        /// </summary>
        private AssemblyDetails()
        {
        }
        #endregion

        #region Methods used to convert to/from XML and assembly name instances
        //=====================================================================

        /// <summary>
        /// This is used to create an assembly details instance from an assembly name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>The new assembly details item</returns>
        internal static AssemblyDetails FromAssemblyName(AssemblyName name)
        {
            return new AssemblyDetails
            {
                Filename = name.CodeBase,
                Name = name.Name,
                Version = name.Version,
                Culture = name.CultureInfo.ToString().ToLowerInvariant(),
                PublicKeyToken = String.Concat(name.GetPublicKeyToken().Select(c => c.ToString("x2")))
            };
        }

        /// <summary>
        /// This is used to create an assembly details instance from an XML element
        /// </summary>
        /// <param name="path">The path to the assembly</param>
        /// <param name="details">The XML element containing the details</param>
        /// <returns>The new assembly details item</returns>
        internal static AssemblyDetails FromXml(string path, XElement details)
        {
            return new AssemblyDetails
            {
                Filename = Path.Combine(path, details.Attribute("Filename").Value),
                Name = details.Attribute("Name").Value,
                Version = new Version(details.Attribute("Version").Value),
                Culture = (string)details.Attribute("Culture"),
                PublicKeyToken = (string)details.Attribute("PublicKeyToken")
            };
        }

        /// <summary>
        /// This is used to convert the assembly detail to an XML element
        /// </summary>
        /// <returns>The assembly details as an XML element</returns>
        internal XElement ToXml()
        {
            return new XElement("AssemblyDetails", new[] {
                new XAttribute("Filename", Path.GetFileName(this.Filename)),
                new XAttribute("Name", this.Name),
                new XAttribute("Version", this.Version.ToString()),
                String.IsNullOrEmpty(this.Culture) ? null : new XAttribute("Culture", this.Culture),
                String.IsNullOrEmpty(this.PublicKeyToken) ? null : new XAttribute("PublicKeyToken", this.PublicKeyToken)
            });
        }
        #endregion

        #region String conversion and equality overrides
        //=====================================================================

        /// <summary>
        /// This is overridden to convert the assembly details to a string in the strong name format
        /// </summary>
        /// <returns>The assembly details as a strong name string</returns>
        public override string ToString()
        {
            return String.Format("{0}, Version={1}, Culture={2}, PublicKeyToken={3}", this.Name, this.Version,
                String.IsNullOrEmpty(this.Culture) ? "neutral" : this.Culture,
                String.IsNullOrEmpty(this.PublicKeyToken) ? "null" : this.PublicKeyToken);
        }

        /// <summary>
        /// This is used to return a hash code for the assembly details in string format
        /// </summary>
        /// <returns>The hash code for the assembly details</returns>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        /// <summary>
        /// This is overridden to allow proper comparison of assembly detail objects
        /// </summary>
        /// <param name="obj">The object to which this instance is compared</param>
        /// <returns>Returns true if the object equals this instance, false if it does not</returns>
        public override bool Equals(object obj)
        {
            if(obj == null || obj.GetType() != this.GetType())
                return false;

            AssemblyDetails otherDetails = obj as AssemblyDetails;

            return this.ToString().Equals(otherDetails.ToString(), StringComparison.Ordinal);
        }
        #endregion
    }
}
