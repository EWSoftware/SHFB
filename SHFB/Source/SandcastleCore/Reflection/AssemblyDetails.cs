//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : AssemblyDetails.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/02/2025
// Note    : Copyright 2012-2025, Eric Woodruff, All rights reserved
//
// This file contains a class that is used to contain settings for an assembly.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/09/2012  EFW  Created the code
// 01/02/2014  EFW  Moved the frameworks code to Sandcastle.Core
// 06/21/2015  EFW  Moved to the Reflection namespace and reworked for use with the Reflection Data Manager
// 09/20/2015  EFW  Added support for relative paths on the saved assembly filenames
//===============================================================================================================

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Sandcastle.Core.Reflection
{
    /// <summary>
    /// This class is used to hold the details for an assembly
    /// </summary>
    public sealed class AssemblyDetails : INotifyPropertyChanged
    {
        #region Private data members
        //=====================================================================

        private bool isIncluded;

        #endregion

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

        /// <summary>
        /// This is used to get or set whether or not to include the assembly
        /// </summary>
        /// <remarks>Certain assemblies cannot be parsed or do not contain any useful information.  This can
        /// be set to false to exclude them from being processed.</remarks>
        public bool IsIncluded
        {
            get => isIncluded;
            set
            {
                if(isIncluded != value)
                {
                    isIncluded = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This read-only property returns the assembly description which is a combination of the name,
        /// version, culture, and public key token.
        /// </summary>
        public string Description { get; private set; }

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

        #region INotifyPropertyChanged Members
        //=====================================================================

        /// <summary>
        /// The property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This raises the <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="propertyName">The property name that changed</param>
        private void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Methods used to convert to/from XML and assembly name instances
        //=====================================================================

        /// <summary>
        /// This is used to create an assembly details instance from an assembly name
        /// </summary>
        /// <param name="name">The assembly name information</param>
        /// <returns>The new assembly details item</returns>
        internal static AssemblyDetails FromAssemblyName(AssemblyName name)
        {
            Uri codeBase = new(name.CodeBase);
            string location;

            if(codeBase.IsFile)
                location = codeBase.LocalPath;
            else
                location = name.Name;

            var ad = new AssemblyDetails
            {
                Filename = location,
                Name = name.Name,
                Version = name.Version,
                Culture = name.CultureInfo.ToString().ToLowerInvariant(),
                PublicKeyToken = String.Concat(name.GetPublicKeyToken().Select(c => c.ToString("x2",
                    CultureInfo.InvariantCulture))),
                IsIncluded = true
            };

            ad.Description = String.Format(CultureInfo.InvariantCulture,
                "{0}, Version={1}, Culture={2}, PublicKeyToken={3}", ad.Name, ad.Version,
                String.IsNullOrEmpty(ad.Culture) ? "neutral" : ad.Culture,
                String.IsNullOrEmpty(ad.PublicKeyToken) ? "null" : ad.PublicKeyToken);

            return ad;
        }

        /// <summary>
        /// This is used to create an assembly details instance from an XML element
        /// </summary>
        /// <param name="path">The path to the assembly</param>
        /// <param name="details">The XML element containing the details</param>
        /// <returns>The new assembly details item</returns>
        internal static AssemblyDetails FromXml(string path, XElement details)
        {
            string filename = details.Attribute("Filename").Value;

            if(!Path.IsPathRooted(filename))
                filename = Path.Combine(path, filename);

            var ad = new AssemblyDetails
            {
                Filename = filename,
                Name = details.Attribute("Name").Value,
                Version = new Version(details.Attribute("Version").Value),
                Culture = (string)details.Attribute("Culture"),
                PublicKeyToken = (string)details.Attribute("PublicKeyToken"),
                IsIncluded = !details.Attributes("IsExcluded").Any()
            };

            ad.Description = String.Format(CultureInfo.InvariantCulture,
                "{0}, Version={1}, Culture={2}, PublicKeyToken={3}", ad.Name, ad.Version,
                String.IsNullOrEmpty(ad.Culture) ? "neutral" : ad.Culture,
                String.IsNullOrEmpty(ad.PublicKeyToken) ? "null" : ad.PublicKeyToken);

            return ad;
        }

        /// <summary>
        /// This is used to convert the assembly detail to an XML element
        /// </summary>
        /// <param name="basePath">The base path for the assembly</param>
        /// <returns>The assembly details as an XML element</returns>
        internal XElement ToXml(string basePath)
        {
            string filename = this.Filename;

            if(filename.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                filename = filename.Substring(basePath.Length + 1);

            return new XElement("AssemblyDetails",
                new XAttribute("Filename", filename),
                new XAttribute("Name", this.Name),
                new XAttribute("Version", this.Version.ToString()),
                String.IsNullOrEmpty(this.Culture) ? null : new XAttribute("Culture", this.Culture),
                String.IsNullOrEmpty(this.PublicKeyToken) ? null : new XAttribute("PublicKeyToken", this.PublicKeyToken),
                isIncluded ? null : new XAttribute("IsExcluded", "true")
            );
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
            return this.Description;
        }

        /// <summary>
        /// This is used to return a hash code for the assembly details in string format
        /// </summary>
        /// <returns>The hash code for the assembly details</returns>
        public override int GetHashCode()
        {
            return this.Description.GetHashCode();
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

            return this.Description.Equals(otherDetails.Description, StringComparison.Ordinal);
        }
        #endregion
    }
}
