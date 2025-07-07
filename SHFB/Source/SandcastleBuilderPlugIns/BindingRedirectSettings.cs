//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BindingRedirectSettings.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2008-2025, Eric Woodruff, All rights reserved
//
// This file contains a class representing binding redirection settings for the Assembly Binding Redirection
// Resolver plug-in.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/14/2008  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

using Sandcastle.Core;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This represents binding redirection settings for the <see cref="BindingRedirectResolverPlugIn"/>.
    /// </summary>
    public class BindingRedirectSettings : INotifyPropertyChanged
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the assembly name (no extension)
        /// </summary>
        public string AssemblyName
        {
            get => field;
            set
            {
                if(field != value)
                {
                    field = value?.Trim();

                    this.Validate();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set the public key token for the assembly
        /// </summary>
        /// <value>If omitted, "null" is assumed</value>
        public string PublicKeyToken
        {
            get => field;
            set
            {
                if(field != value)
                {
                    field = value?.Trim();

                    this.Validate();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set the culture for the assembly
        /// </summary>
        /// <value>If omitted, "neutral" is assumed</value>
        public string Culture
        {
            get => field;
            set
            {
                if(field != value)
                {
                    field = value?.Trim();

                    this.Validate();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set the old version number to redirect to the new version number
        /// </summary>
        public string OldVersion
        {
            get => field;
            set
            {
                if(field != value)
                {
                    field = value?.Trim();

                    this.Validate();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set the ending old version number range to redirect to the new version
        /// number.
        /// </summary>
        /// <value>If not set, only <see cref="OldVersion" /> will be used to redirect a single
        /// version.</value>
        public string OldVersionTo
        {
            get => field;
            set
            {
                if(field != value)
                {
                    field = value?.Trim();

                    this.Validate();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set the new version number to which the old versions are redirected
        /// </summary>
        public string NewVersion
        {
            get => field;
            set
            {
                if(field != value)
                {
                    field = value?.Trim();

                    this.Validate();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set the path to the configuration file from which the settings should be
        /// imported.
        /// </summary>
        /// <value>If specified, the properties in the Binding Redirect category are ignored.</value>
        public FilePath ConfigurationFile
        {
            get => field;
            set
            {
                if(field != value)
                {
                    if(field != null)
                        field.PersistablePathChanged -= this.configFile_PersistablePathChanged;

                    if(value == null)
                        value = new FilePath(field.BasePathProvider);

                    field = value;
                    field.PersistablePathChanged += this.configFile_PersistablePathChanged;

                    this.Validate();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This read-only property returns an error message describing any issues with the settings
        /// </summary>
        public string ErrorMessage
        {
            get => field;
            private set
            {
                field = value;

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// This read-only property returns a description of the settings
        /// </summary>
        public string BindingRedirectDescription
        {
            get => field;
            private set
            {
                field = value;

                this.OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="provider">The base path provider</param>
        public BindingRedirectSettings(IBasePathProvider provider)
        {
            this.ConfigurationFile = new FilePath(provider);
        }
        #endregion

        #region INotifyPropertyChanged implementation
        //=====================================================================

        /// <summary>
        /// The property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This raises the <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="propertyName">The property name that changed</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Update the display description when the configuration file changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void configFile_PersistablePathChanged(object sender, EventArgs e)
        {
            this.Validate();
        }

        /// <summary>
        /// This is used to validate the settings
        /// </summary>
        private void Validate()
        {
            List<string> problems = [];

            if(String.IsNullOrWhiteSpace(this.AssemblyName) && String.IsNullOrWhiteSpace(this.ConfigurationFile))
                problems.Add("An assembly name or configuration file is required");
            else
            {
                if(!String.IsNullOrWhiteSpace(this.AssemblyName) && !String.IsNullOrWhiteSpace(this.ConfigurationFile))
                    problems.Add("Specify an assembly name or configuration file but not both");
            }

            if(!String.IsNullOrWhiteSpace(this.AssemblyName))
            {
                if(String.IsNullOrWhiteSpace(this.OldVersion))
                    problems.Add("An old version number is required");

                if(String.IsNullOrWhiteSpace(this.NewVersion))
                    problems.Add("An new version number is required");
            }

            if(problems.Count != 0)
                this.ErrorMessage = String.Join(" / ", problems);
            else
                this.ErrorMessage = null;

            if(this.ConfigurationFile.Path.Length != 0)
                this.BindingRedirectDescription = this.ConfigurationFile.PersistablePath;
            else
            {
                string range;

                if(this.OldVersionTo == null)
                    range = this.OldVersion;
                else
                    range = String.Format(CultureInfo.InvariantCulture, "{0}-{1}", this.OldVersion, this.OldVersionTo);

                this.BindingRedirectDescription = String.Format(CultureInfo.InvariantCulture,
                    "{0}, Culture={1}, PublicKeyToken={2}, Version(s) {3} redirect to Version {4}",
                    this.AssemblyName ?? "(Undefined)", String.IsNullOrWhiteSpace(this.Culture) ? "neutral" : this.Culture,
                    String.IsNullOrWhiteSpace(this.PublicKeyToken) ? "null" : this.PublicKeyToken, range, this.NewVersion);
            }
        }
        #endregion

        #region Convert from/to XML
        //=====================================================================

        /// <summary>
        /// Create a binding redirect settings instance from an XML element containing the settings
        /// </summary>
        /// <param name="pathProvider">The base path provider object</param>
        /// <param name="configuration">The XML element from which to obtain the settings</param>
        /// <returns>A <see cref="BindingRedirectSettings"/> object containing the settings from the XPath
        /// navigator.</returns>
        /// <remarks>It should contain an element called <c>dependentAssembly</c> with a <c>configFile</c>
        /// attribute or a nested <c>assemblyIdentity</c> and <c>bindingRedirect</c> element that define
        /// the settings.</remarks>
        public static BindingRedirectSettings FromXml(IBasePathProvider pathProvider, XElement configuration)
        {
            BindingRedirectSettings brs = new(pathProvider);

            if(configuration != null)
            {
                string value = configuration.Attribute("importFrom")?.Value;

                if(!String.IsNullOrWhiteSpace(value))
                    brs.ConfigurationFile = new FilePath(value, Path.IsPathRooted(value), pathProvider);
                else
                {
                    var settings = configuration.Element("assemblyIdentity");

                    if(settings != null)
                    {
                        brs.AssemblyName = settings.Attribute("name").Value;
                        brs.PublicKeyToken = settings.Attribute("publicKeyToken")?.Value;
                        brs.Culture = settings.Attribute("culture")?.Value;
                    }

                    settings = configuration.Element("bindingRedirect");

                    if(settings != null)
                    {
                        value = settings.Attribute("newVersion").Value;

                        if(!String.IsNullOrWhiteSpace(value))
                            brs.NewVersion = value;

                        value = settings.Attribute("oldVersion").Value;

                        string[] versions = value.Split('-');

                        if(versions.Length == 2)
                        {
                            if(versions[0].Trim().Length != 0)
                                brs.OldVersion = versions[0];

                            if(versions[1].Trim().Length != 0)
                                brs.OldVersionTo = versions[1];

                            if(Version.TryParse(brs.OldVersion, out Version oldVersion) &&
                              Version.TryParse(brs.OldVersionTo, out Version oldVersionTo) &&
                              oldVersion > oldVersionTo)
                            {
                                Version tempVersion = oldVersion;
                                brs.OldVersion = oldVersionTo.ToString();
                                brs.OldVersionTo = tempVersion.ToString();
                            }
                        }
                        else
                            brs.OldVersion = versions[0];
                    }
                }
            }

            return brs;
        }

        /// <summary>
        /// Store the binding redirect settings in an XML element
        /// </summary>
        /// <param name="relativePath">True to allow a relative path on <c>importFrom</c> attributes, false to
        /// fully qualify the path.</param>
        /// <returns>Returns the XML element</returns>
        /// <remarks>The settings are stored in an element called <c>dependentAssembly</c>.</remarks>
        public XElement ToXml(bool relativePath)
        {
            if(this.ConfigurationFile.Path.Length != 0)
            {
                return new XElement("dependentAssembly",
                    new XAttribute("importFrom", relativePath ? this.ConfigurationFile.PersistablePath :
                        this.ConfigurationFile.ToString()));
            }

            var el = new XElement("dependentAssembly",
                new XElement("assemblyIdentity",
                    new XAttribute("name", this.AssemblyName),
                    !String.IsNullOrWhiteSpace(this.PublicKeyToken) ? new XAttribute("publicKeyToken", this.PublicKeyToken) : null,
                    !String.IsNullOrWhiteSpace(this.Culture) ? new XAttribute("culture", this.Culture) : null));

            var br = new XElement("bindingRedirect");

            el.Add(br);

            var attr = new XAttribute("oldVersion", this.OldVersion);
            br.Add(attr);

            if(this.OldVersionTo != null)
            {
                if(Version.TryParse(this.OldVersion, out Version oldFrom) &&
                  Version.TryParse(this.OldVersionTo, out Version oldTo) && oldFrom > oldTo)
                {
                    Version tempVersion = oldFrom;
                    this.OldVersion = oldTo.ToString();
                    this.OldVersionTo = tempVersion.ToString();
                }

                attr.Value = String.Format(CultureInfo.InvariantCulture, "{0}-{1}", this.OldVersion,
                    this.OldVersionTo);
            }

            br.Add(new XAttribute("newVersion", this.NewVersion.ToString()));

            return el;
        }
        #endregion
    }
}
