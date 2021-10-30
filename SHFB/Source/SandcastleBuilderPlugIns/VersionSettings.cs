//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : VersionSettings.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/16/2021
// Note    : Copyright 2007-2021, Eric Woodruff, All rights reserved
//
// This file contains a class representing version settings for the Version Builder plug-in
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/01/2007  EFW  Created the code
// 08/13/2008  EFW  Updated to support the new project format
//===============================================================================================================

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This represents version settings for the <see cref="VersionBuilderPlugIn"/>.
    /// </summary>
    public class VersionSettings : INotifyPropertyChanged
    {
        #region Private data members
        //=====================================================================

        private FilePath helpFileProject;
        private string frameworkLabel, version, errorMessage, versionInfoDescription;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the location of the reflection file at build time
        /// </summary>
        internal string ReflectionFilename { get; set; }

        /// <summary>
        /// This is used to get or set the framework label for the related
        /// project.
        /// </summary>
        /// <remarks>This is used to group like versions of projects.</remarks>
        public string FrameworkLabel
        {
            get => frameworkLabel;
            set
            {
                if(frameworkLabel != value)
                {
                    if(!String.IsNullOrEmpty(value))
                        frameworkLabel = value.Trim();
                    else
                        frameworkLabel = value;

                    this.Validate();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set the version number description for the related project
        /// </summary>
        public string Version
        {
            get => version;
            set
            {
                if(version != value)
                {
                    version = value;

                    if(String.IsNullOrWhiteSpace(version))
                        version = "1.0";

                    this.Validate();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set the path to the help file builder project used to generate version
        /// information.
        /// </summary>
        /// <value>The help file builder project makes it simple to manage settings for the prior version's
        /// assemblies such as references, API filter settings, etc.</value>
        public FilePath HelpFileProject
        {
            get => helpFileProject;
            set
            {
                if(helpFileProject != value)
                {
                    if(helpFileProject != null)
                        helpFileProject.PersistablePathChanged -= this.helpFileProject_PersistablePathChanged;

                    if(value == null)
                        value = new FilePath(helpFileProject.BasePathProvider);

                    helpFileProject = value;
                    helpFileProject.PersistablePathChanged += this.helpFileProject_PersistablePathChanged;

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
            get => errorMessage;
            private set
            {
                errorMessage = value;

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// This read-only property returns a description of the settings
        /// </summary>
        public string VersionInfoDescription
        {
            get => versionInfoDescription;
            private set
            {
                versionInfoDescription = value;

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// This read-only property returns a unique ID for the item
        /// </summary>
        /// <value>Returns a hash code of the framework label and version.  This is used rather than overriding
        /// <see cref="Object.GetHashCode"/> as the WPF list box relies on an immutable hash value to track
        /// elements.  Editing the framework and version values in an existing instance caused it to throw
        /// duplicate key errors.</value>
        public int UniqueId => (frameworkLabel + version).GetHashCode();

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="provider">The base path provider for the help file project property</param>
        public VersionSettings(IBasePathProvider provider)
        {
            this.HelpFileProject = new FilePath(provider);
            this.FrameworkLabel = "ProjectLabel";
            this.Version = "1.0";
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
        /// Update the display description when the help file project changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void helpFileProject_PersistablePathChanged(object sender, EventArgs e)
        {
            this.Validate();
        }

        /// <summary>
        /// This is used to validate the settings
        /// </summary>
        private void Validate()
        {
            if(helpFileProject.Path.Length == 0)
                this.ErrorMessage = "A help file project is required";
            else
                this.ErrorMessage = null;

            this.VersionInfoDescription = String.Format(CultureInfo.CurrentCulture, "{0} {1} - {2}",
                frameworkLabel, version, helpFileProject.PersistablePath);
        }
        #endregion

        #region Convert from/to XML
        //=====================================================================

        /// <summary>
        /// Create a version settings instance from an XML element containing the settings
        /// </summary>
        /// <param name="pathProvider">The base path provider object</param>
        /// <param name="configuration">The XML element from which to obtain the settings</param>
        /// <returns>A <see cref="VersionSettings"/> object containing the settings from the XML element</returns>
        /// <remarks>It should contain an element called <c>version</c> with three attributes (<c>label</c>,
        /// <c>version</c> and <c>helpFileProject</c>).</remarks>
        public static VersionSettings FromXml(IBasePathProvider pathProvider, XElement configuration)
        {
            VersionSettings vs = new VersionSettings(pathProvider);

            if(configuration != null)
            {
                vs.FrameworkLabel = configuration.Attribute("label").Value;
                vs.Version = configuration.Attribute("version").Value;

                string path = configuration.Attribute("helpFileProject").Value;

                vs.HelpFileProject = new FilePath(path, Path.IsPathRooted(path), pathProvider);
            }

            return vs;
        }

        /// <summary>
        /// Store the version settings in an XML element
        /// </summary>
        /// <returns>Returns the settings in an XML element</returns>
        /// <remarks>The version settings are stored in an element called <c>version</c> with three attributes
        /// (<c>label</c>, <c>version</c> and <c>helpFileProject</c>).</remarks>
        public XElement ToXml()
        {
            return new XElement("version",
                new XAttribute("label", this.FrameworkLabel),
                new XAttribute("version", this.Version),
                new XAttribute("helpFileProject", this.HelpFileProject.PersistablePath));
        }
        #endregion
    }
}
