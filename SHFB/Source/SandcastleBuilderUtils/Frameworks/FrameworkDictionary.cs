//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : FrameworkDictionary.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/17/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing a dictionary of .NET Framework version settings
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.   This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.5.0  09/09/2012  EFW  Created the code
// 1.9.6.0  11/17/2012  EFW  Moved the code from the framework type converter into this class
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using SandcastleBuilder.Utils.BuildComponent;

namespace SandcastleBuilder.Utils.Frameworks
{
    /// <summary>
    /// This dictionary contains the settings for all known .NET Framework versions
    /// </summary>
    public sealed class FrameworkDictionary : Dictionary<string, FrameworkSettings>
    {
        #region Private data members
        //=====================================================================

        private static FrameworkDictionary frameworks;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the values in the collection
        /// </summary>
        public static FrameworkDictionary AllFrameworks
        {
            get
            {
                if(frameworks == null)
                    frameworks = LoadSandcastleFrameworkDictionary();

                return frameworks; 
            }
        }

        /// <summary>
        /// This is used to get the default framework version to use
        /// </summary>
        /// <remarks>The default is the .NET Framework 4.0</remarks>
        public static string DefaultFramework
        {
            get
            {
                FrameworkSettings fs = null;

                // If the framework file didn't exist or wasn't valid, the collection will be null here
                if(frameworks == null)
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                        "The framework definitions were not loaded.  Does a valid definition file exist ({0})?",
                        FrameworkDictionary.FrameworkFilePath));

                if(!frameworks.TryGetValue(DefaultFrameworkInternal, out fs))
                    fs = frameworks.Values.First();

                return fs.Title;
            }
        }

        /// <summary>
        /// This returns the hard-coded default framework value used internally
        /// </summary>
        internal static string DefaultFrameworkInternal
        {
            get { return ".NET Framework 4.0"; }
        }

        /// <summary>
        /// This read-only property returns the path to the framework definition file
        /// </summary>
        public static string FrameworkFilePath
        {
            get
            {
                return Path.Combine(BuildComponentManager.SandcastlePath, @"ProductionTools\Frameworks.xml");
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Private constructor
        /// </summary>
        /// <remarks>Keys are case-insensitive</remarks>
        private FrameworkDictionary() : base(StringComparer.OrdinalIgnoreCase)
        {
        }
        #endregion

        #region Methods use to convert to/from XML
        //=====================================================================

        /// <summary>
        /// This is used to load the framework dictionary used by the Sandcastle tools
        /// </summary>
        /// <returns>The framework dictionary loaded from the <b>Frameworks.xml</b> file located in the
        /// Sandcastle production tools folder.  If the file cannot be found, an empty dictionary is
        /// returned.</returns>
        public static FrameworkDictionary LoadSandcastleFrameworkDictionary()
        {
            FrameworkDictionary fd;

            if(!File.Exists(FrameworkFilePath))
                throw new FileNotFoundException("Unable to locate Sandcastle framework definition file: " +
                    FrameworkFilePath);

            try
            {
                fd = FromXml(FrameworkFilePath);
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException("Unable to parse Sandcastle framework definition file: " +
                    FrameworkFilePath, ex);
            }

            return fd;
        }

        /// <summary>
        /// This is used to load a framework dictionary XML file
        /// </summary>
        /// <param name="filename">The filename to load</param>
        /// <returns>The new framework dictionary containing the file's content</returns>
        public static FrameworkDictionary FromXml(string filename)
        {
            FrameworkDictionary fd = new FrameworkDictionary();
            FrameworkSettings fs;
            XDocument frameworks = XDocument.Load(filename);

            foreach(var framework in frameworks.Descendants("Framework"))
            {
                fs = FrameworkSettings.FromXml(framework);
                fd.Add(fs.Title, fs);
            }

            return fd;
        }

        /// <summary>
        /// This is used to convert the framework dictionary to an XML document
        /// </summary>
        /// <returns>The framework dictionary as an XML document</returns>
        public XDocument ToXml()
        {
            XDocument doc = new XDocument();

            doc.Add(new XElement("Frameworks", this.Values.Select(f => f.ToXml())));

            return doc;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to find the first framework matching the specified platform and version
        /// </summary>
        /// <param name="platform">The platform to find</param>
        /// <param name="version">The version to find.  This can be a full or partial version number</param>
        /// <param name="withRedirect">True to redirect to a version that is present or false to return the
        /// exact match even if not present.</param>
        /// <returns>The framework settings if found or null if not found.</returns>
        public FrameworkSettings FrameworkMatching(string platform, Version version, bool withRedirect)
        {
            var fs = this.Values.FirstOrDefault(f => f.Platform == platform && (f.Version == version ||
                f.Version.ToString().StartsWith(version.ToString(), StringComparison.Ordinal)));

            if(fs != null && !fs.IsFrameworkPresent && withRedirect)
                fs = this.GetFrameworkWithRedirect(fs.Title);

            return fs;
        }

        /// <summary>
        /// This is used to get the specified framework settings or a suitable redirected version if not
        /// present on the current system.
        /// </summary>
        /// <param name="key">The title of the framework to find</param>
        /// <returns>The specified framework settings.  If the specified framework does not appear to be
        /// present and redirect information is available, the settings for the redirected version are
        /// returned.  If no suitable redirect can be found, it returns null.</returns>
        public FrameworkSettings GetFrameworkWithRedirect(string key)
        {
            FrameworkSettings frameworkSettings;

            do
            {
                if(String.IsNullOrEmpty(key) || !this.TryGetValue(key, out frameworkSettings))
                    return null;

                if(frameworkSettings.IsFrameworkPresent)
                    return frameworkSettings;

                // Try to redirect to a version that does exist
                key = frameworkSettings.Redirect;

            } while(key != null);

            return null;
        }

        /// <summary>
        /// This is used to convert old SHFB project framework version values to the new framework version values
        /// </summary>
        /// <param name="oldValue">The old value to convert</param>
        /// <returns>The equivalent new value</returns>
        internal static string ConvertFromOldValue(string oldValue)
        {
            FrameworkSettings fs = null;

            if(String.IsNullOrWhiteSpace(oldValue))
                return DefaultFramework;

            oldValue = oldValue.Trim();

            if(oldValue.IndexOf(".NET ", StringComparison.OrdinalIgnoreCase) != -1 || Char.IsDigit(oldValue[0]))
            {
                oldValue = oldValue.ToUpperInvariant().Replace(".NET ", String.Empty).Trim();

                if(oldValue.Length == 0)
                    oldValue = "4.0";
                else
                    if(oldValue.Length > 3)
                        oldValue = oldValue.Substring(0, 3);

                oldValue = ".NET Framework " + oldValue;
            }
            else
                if(oldValue.IndexOf("Silverlight ", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    oldValue = oldValue.ToUpperInvariant().Trim();

                    if(oldValue.EndsWith(".0", StringComparison.Ordinal))
                        oldValue = oldValue.Substring(0, oldValue.Length - 2);
                }
                else
                    if(oldValue.IndexOf("Portable ", StringComparison.OrdinalIgnoreCase) != -1)
                        oldValue = ".NET Portable Library 4.0 (Legacy)";

            // If not found, default to .NET 4.0
            if(!frameworks.TryGetValue(oldValue, out fs))
                return DefaultFramework;

            return fs.Title;
        }
        #endregion
    }
}
