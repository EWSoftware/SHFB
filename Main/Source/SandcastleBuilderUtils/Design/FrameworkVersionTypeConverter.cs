//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : FrameworkVersionTypeConverter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/28/2011
// Note    : Copyright 2006-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a type converter that allows you to select a .NET
// Framework or Silverlight Framework version from those currently installed
// on the system.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  08/08/2006  EFW  Created the code
// 1.9.2.0  01/16/2011  EFW  Updated to support selection of Silverlight
//                           Framework versions.
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This type converter allows you to select a .NET Framework or Silverlight
    /// Framework version from those currently installed on the system.
    /// </summary>
    public class FrameworkVersionTypeConverter : StringConverter
    {
        #region Private data members
        //=====================================================================

        private static List<string> versions = new List<string>();
        private static StandardValuesCollection standardValues = InitializeStandardValues();
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the values in the collection
        /// </summary>
        public static IEnumerable<string> StandardValues
        {
            get { return versions; }
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// This is used to get the standard values by searching for the
        /// .NET Framework and Silverlight Framework versions installed on the
        /// current system.
        /// </summary>
        private static StandardValuesCollection InitializeStandardValues()
        {
            string programFilesFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            IEnumerable<string> silverlightVersions;

            if(String.IsNullOrEmpty(programFilesFolder))
                programFilesFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            string silverlightFolder = programFilesFolder + @"\Reference Assemblies\Microsoft\Framework\Silverlight";

            // Add Silverlight Framework versions if present
            if(!Directory.Exists(silverlightFolder))
                silverlightVersions = Enumerable.Empty<string>();
            else
                silverlightVersions = Directory.EnumerateDirectories(silverlightFolder).Where(d =>
                    {
                        string dir = d.Substring(d.LastIndexOf('\\') + 1);

                        return dir.Length > 2 && (dir[0] == 'v' || dir[0] == 'V') && Char.IsDigit(dir[1]);
                    }).Select(d => "Silverlight " + d.Substring(d.LastIndexOf('\\') + 2));

            versions.AddRange(
                // .NET Framework versions
                Directory.EnumerateDirectories(Environment.GetFolderPath(Environment.SpecialFolder.System) +
                    @"\..\Microsoft.NET\Framework").Where(d =>
                    {
                        string dir = d.Substring(d.LastIndexOf('\\') + 1);

                        return dir.Length > 2 && (dir[0] == 'v' || dir[0] == 'V') && Char.IsDigit(dir[1]);
                    }).Select(d => ".NET " + d.Substring(d.LastIndexOf('\\') + 2)).Concat(
                // Plus Silverlight versions if present
                silverlightVersions).OrderBy(d => d));

            return new StandardValuesCollection(versions);
        }

        /// <summary>
        /// This is overridden to return the values for the type converter's
        /// dropdown list.
        /// </summary>
        /// <param name="context">The format context object</param>
        /// <returns>Returns the standard values for the type</returns>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return standardValues;
        }

        /// <summary>
        /// This is overridden to indicate that the values are exclusive
        /// and values outside the list cannot be entered.
        /// </summary>
        /// <param name="context">The format context object</param>
        /// <returns>Always returns true</returns>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// This is overridden to indicate that standard values are supported
        /// and can be chosen from a list.
        /// </summary>
        /// <param name="context">The format context object</param>
        /// <returns>Always returns true</returns>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// This is used to find out if the specified version of the .NET
        /// Framework is present on the system.
        /// </summary>
        /// <param name="version">The version for which to look</param>
        /// <returns>True if present, false if not found</returns>
        public static bool IsPresent(string version)
        {
            return versions.Contains(version);
        }

        /// <summary>
        /// This is used to get the latest framework version that starts with
        /// the given value.
        /// </summary>
        /// <param name="version">The version for which to look.  The value
        /// can be prefixed with ".NET" or "Silverlight".  If not specified,
        /// ".NET" is assumed.</param>
        /// <returns>The latest framework version starting with the specified
        /// value or the most recent version if not found.</returns>
        public static string LatestFrameworkMatching(string version)
        {
            string latestVersion;

            // If a framework type is missing, assume .NET
            if(Char.IsNumber(version[0]))
                version = ".NET " + version;

            latestVersion = versions.LastOrDefault(v => v.StartsWith(version, StringComparison.OrdinalIgnoreCase));

            if(latestVersion == null)
            {
                if(version.StartsWith("Silverlight", StringComparison.OrdinalIgnoreCase))
                    latestVersion = versions.LastOrDefault(v => v.StartsWith("Silverlight",
                        StringComparison.OrdinalIgnoreCase));

                // If Silverlight isn't there, fall back to .NET.  The build will likely fail but
                // at least the GUI won't crash due to a bad value.
                if(latestVersion == null)
                    latestVersion = versions.LastOrDefault(v => v.StartsWith(".NET",
                        StringComparison.OrdinalIgnoreCase));
            }

            return latestVersion;
        }

        /// <summary>
        /// This is used to get the latest framework version number that starts
        /// with the given value.
        /// </summary>
        /// <param name="version">The version for which to look.  The value
        /// can be prefixed with ".NET" or "Silverlight".  If not specified,
        /// ".NET" is assumed.</param>
        /// <returns>The latest framework version number starting with the
        /// specified value or the most recent version if not found.</returns>
        public static string LatestFrameworkNumberMatching(string version)
        {
            string latestVersion = LatestFrameworkMatching(version);

            if(latestVersion.StartsWith(".NET ", StringComparison.OrdinalIgnoreCase))
                return latestVersion.Substring(5);

            return latestVersion.Substring(12);
        }
        #endregion
    }
}
