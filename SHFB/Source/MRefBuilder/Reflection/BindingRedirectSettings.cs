﻿//===============================================================================================================
// System  : Sandcastle MRefBuilder Tool
// File    : BindingRedirectSettings.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/26/2021
//
// This file contains a class representing binding redirection settings for the MRefBuilder assembly resolver
// class.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Change History
// 03/02/2012 - EFW - Added my code to the MRefBuilder project
// 12/17/2013 - EFW - Updated regex to ignore anything after the public key token such as ", Retargetable=Yes".
//===============================================================================================================

// Ignore Spelling: microsoft

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace Sandcastle.Tools.Reflection
{
    /// <summary>
    /// This represents binding redirection settings for the <see cref="AssemblyResolver"/>.
    /// </summary>
    public class BindingRedirectSettings
    {
        #region Private data members
        //=====================================================================

        private static readonly Regex reStrongName = new Regex(@"(?<Name>.*?),\s*" +
            @"Version=(?<Version>.*?),\s*Culture=(?<Culture>.*?),\s*" +
            "PublicKeyToken=(?<PublicKeyToken>[^,]*)", RegexOptions.IgnoreCase);

        private string configFile, assemblyName, publicKeyToken, culture;
        private Version oldVersionFrom, oldVersionTo, newVersion;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the assembly name (no extension)
        /// </summary>
        public string AssemblyName
        {
            get => assemblyName;
            set
            {
                if(String.IsNullOrEmpty(value))
                    value = "assemblyName";

                assemblyName = value;
            }
        }

        /// <summary>
        /// This is used to get or set the public key token for the assembly
        /// </summary>
        /// <value>If omitted, "null" is assumed</value>
        public string PublicKeyToken
        {
            get => publicKeyToken;
            set => publicKeyToken = value;
        }

        /// <summary>
        /// This is used to get or set the culture for the assembly
        /// </summary>
        /// <value>If omitted, "neutral" is assumed</value>
        public string Culture
        {
            get => culture;
            set => culture = value;
        }

        /// <summary>
        /// This is used to get or set the old version number to redirect to the new version number
        /// </summary>
        public Version OldVersion
        {
            get => oldVersionFrom;
            set
            {
                if(value == null)
                    value = new Version(1, 0, 0, 0);

                oldVersionFrom = value;
            }
        }

        /// <summary>
        /// This is used to get or set the ending old version number range to redirect to the new version
        /// number.
        /// </summary>
        /// <value>If not set, only <see cref="OldVersion" /> will be used to redirect a single
        /// version.</value>
        public Version OldVersionTo
        {
            get => oldVersionTo;
            set => oldVersionTo = value;
        }

        /// <summary>
        /// This is used to get or set the new version number to which the old versions are redirected
        /// </summary>
        public Version NewVersion
        {
            get => newVersion;
            set
            {
                if(value == null)
                    value = new Version(1, 0, 0, 1);

                newVersion = value;
            }
        }

        /// <summary>
        /// This is used to get or set the path to the configuration file from which the settings should be
        /// imported.
        /// </summary>
        /// <value>If specified, the properties in the Binding Redirect category are ignored</value>
        public string ConfigurationFile
        {
            get => configFile;
            set => configFile = value;
        }

        /// <summary>
        /// This returns the strong name for the redirect assembly
        /// </summary>
        public string StrongName => String.Format(CultureInfo.InvariantCulture,
            "{0}, Version={1}, Culture={2}, PublicKeyToken={3}", assemblyName, newVersion,
            String.IsNullOrEmpty(culture) ? "neutral" : culture,
            String.IsNullOrEmpty(publicKeyToken) ? "null" : publicKeyToken);

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public BindingRedirectSettings()
        {
            assemblyName = "assemblyName";
            oldVersionFrom = new Version(1, 0, 0, 0);
            newVersion = new Version(1, 0, 0, 1);
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// This returns a string representation of the item
        /// </summary>
        /// <returns>The item value as a string</returns>
        public override string ToString()
        {
            string range;

            // Shouldn't happen, but just in case...
            if(!String.IsNullOrWhiteSpace(configFile))
                return "!!! ERROR: Configuration file entry not imported !!!";

            if(oldVersionTo == null)
                range = oldVersionFrom.ToString();
            else
                range = String.Format(CultureInfo.InvariantCulture, "{0}-{1}", oldVersionFrom, oldVersionTo);

            return String.Format(CultureInfo.InvariantCulture, "{0}, Culture=" +
                "{1}, PublicKeyToken={2}, Version(s) {3} redirect to Version {4}",
                assemblyName, String.IsNullOrEmpty(culture) ? "neutral" : culture,
                String.IsNullOrEmpty(publicKeyToken) ? "null" : publicKeyToken,
                range, newVersion);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to determine whether or not this entry is a redirect for the given strong name
        /// </summary>
        /// <param name="strongName">The strong name to compare</param>
        /// <returns>True if it is a redirect for the strong name, false if not</returns>
        public bool IsRedirectFor(string strongName)
        {
            Match m = reStrongName.Match(strongName);
            string compareCulture = culture, compareKey = publicKeyToken;
            Version compareVersion;

            if(m.Success)
            {
                if(String.IsNullOrEmpty(compareCulture))
                    compareCulture = "neutral";

                if(String.IsNullOrEmpty(compareKey))
                    compareKey = "null";

                if(String.Compare(m.Groups["Name"].Value, assemblyName,
                  StringComparison.OrdinalIgnoreCase) == 0 &&
                  String.Compare(m.Groups["Culture"].Value, compareCulture,
                  StringComparison.OrdinalIgnoreCase) == 0 &&
                  String.Compare(m.Groups["PublicKeyToken"].Value, compareKey,
                  StringComparison.OrdinalIgnoreCase) == 0)
                {
                    compareVersion = new Version(m.Groups["Version"].Value);

                    if(oldVersionTo == null)
                        return (oldVersionFrom == compareVersion);

                    return (compareVersion >= oldVersionFrom &&
                        compareVersion <= oldVersionTo);
                }
            }

            return false;
        }
        #endregion

        #region Load from XML
        //=====================================================================

        /// <summary>
        /// Create a binding redirect settings instance from an XPath navigator containing the settings
        /// </summary>
        /// <param name="navigator">The XPath navigator from which to obtain the settings.</param>
        /// <param name="resolver">An optional namespace resolver.  Pass null if one is not needed.</param>
        /// <param name="namespacePrefix">The namespace to prefix the elements with if needed.  This is
        /// ignored if <c>resolver</c> is null.</param>
        /// <returns>A <see cref="BindingRedirectSettings"/> object containing the settings from the XPath
        /// navigator.</returns>
        /// <remarks>It should contain an element called <c>dependentAssembly</c>  with a <c>configFile</c>
        /// attribute or a nested <c>assemblyIdentity</c> and <c>bindingRedirect</c> element that define
        /// the settings.</remarks>
        public static BindingRedirectSettings FromXPathNavigator(XPathNavigator navigator,
          IXmlNamespaceResolver resolver, string namespacePrefix)
        {
            BindingRedirectSettings brs = new BindingRedirectSettings();
            XPathNavigator nav;
            string value;
            string[] versions;
            Version tempVersion;

            if(navigator != null)
            {
                value = navigator.GetAttribute("importFrom", String.Empty).Trim();

                if(value.Length != 0)
                    brs.ConfigurationFile = value;
                else
                {
                    if(resolver != null)
                    {
                        if(!String.IsNullOrEmpty(namespacePrefix))
                            namespacePrefix += ":";
                        else
                            namespacePrefix = String.Empty;
                    }
                    else
                        namespacePrefix = String.Empty;

                    nav = navigator.SelectSingleNode(namespacePrefix + "assemblyIdentity", resolver);

                    if(nav != null)
                    {
                        brs.AssemblyName = nav.GetAttribute("name", String.Empty).Trim();
                        brs.PublicKeyToken = nav.GetAttribute("publicKeyToken", String.Empty).Trim();
                        brs.Culture = nav.GetAttribute("culture", String.Empty).Trim();
                    }

                    nav = navigator.SelectSingleNode(namespacePrefix + "bindingRedirect", resolver);

                    if(nav != null)
                    {
                        value = nav.GetAttribute("newVersion", String.Empty).Trim();

                        if(value.Length != 0)
                            brs.NewVersion = new Version(value);

                        value = nav.GetAttribute("oldVersion", String.Empty).Trim();
                        versions = value.Split('-');

                        if(versions.Length == 2)
                        {
                            if(versions[0].Trim().Length != 0)
                                brs.OldVersion = new Version(versions[0]);

                            if(versions[1].Trim().Length != 0)
                                brs.OldVersionTo = new Version(versions[1]);

                            if(brs.OldVersion > brs.oldVersionTo)
                            {
                                tempVersion = brs.OldVersion;
                                brs.OldVersion = brs.oldVersionTo;
                                brs.oldVersionTo = tempVersion;
                            }
                        }
                        else
                            brs.OldVersion = new Version(versions[0]);
                    }
                }
            }

            return brs;
        }

        /// <summary>
        /// Load assembly binding redirects from a configuration file.
        /// </summary>
        /// <param name="configFile">The configuration filename</param>
        /// <returns>A collection containing the assembly binding redirects</returns>
        public static Collection<BindingRedirectSettings> FromConfigFile(string configFile)
        {
            XmlNamespaceManager nsm;
            XmlDocument config;
            XPathNavigator navConfig;

            Collection<BindingRedirectSettings> redirects =
                new Collection<BindingRedirectSettings>();

            config = new XmlDocument();
            config.Load(configFile);
            nsm = new XmlNamespaceManager(config.NameTable);
            nsm.AddNamespace("binding", "urn:schemas-microsoft-com:asm.v1");

            navConfig = config.CreateNavigator();

            foreach(XPathNavigator nav in navConfig.Select(
                "configuration/runtime/binding:assemblyBinding/binding:dependentAssembly", nsm))
                redirects.Add(BindingRedirectSettings.FromXPathNavigator(nav, nsm, "binding"));

            return redirects;
        }
        #endregion
    }
}
