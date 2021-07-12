//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ReferenceLinkSettings.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/23/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains a class representing reference link settings for the Additional Reference Links plug-in
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/25/2008  EFW  Created the code
// 08/13/2008  EFW  Updated to support the new project format
// 06/20/2010  EFW  Updated to support multi-format build output
// 05/03/2015  EFW  Removed support for the MS Help 2 file format
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
    /// This represents reference link settings for the <see cref="AdditionalReferenceLinksPlugIn"/>
    /// </summary>
    public class ReferenceLinkSettings : INotifyPropertyChanged
    {
        #region Private data members
        //=====================================================================

        private FilePath helpFileProject;
        private HtmlSdkLinkType htmlSdkLinkType, websiteSdkLinkType;
        private MSHelpViewerSdkLinkType msHelpViewerSdkLinkType;
        private string referenceDescription, errorMessage;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the location of the reflection file at build time
        /// </summary>
        internal string ReflectionFilename { get; set; }

        /// <summary>
        /// This is used to get or set the HTML Help 1 SDK link type for the target
        /// </summary>
        public HtmlSdkLinkType HtmlSdkLinkType
        {
            get => htmlSdkLinkType;
            set
            {
                if(htmlSdkLinkType != value)
                {
                    htmlSdkLinkType = value;

                    this.Validate();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set the MS Help Viewer SDK link type for the target
        /// </summary>
        public MSHelpViewerSdkLinkType MSHelpViewerSdkLinkType
        {
            get => msHelpViewerSdkLinkType;
            set
            {
                if(msHelpViewerSdkLinkType != value)
                {
                    msHelpViewerSdkLinkType = value;

                    this.Validate();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set the website SDK link type for the target
        /// </summary>
        public HtmlSdkLinkType WebsiteSdkLinkType
        {
            get => websiteSdkLinkType;
            set
            {
                if(websiteSdkLinkType != value)
                {
                    websiteSdkLinkType = value;

                    this.Validate();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set the path to the help file builder project used to generate reference link
        /// information.
        /// </summary>
        /// <value>The help file builder project makes it simple to manage settings for the other target's
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
        public string ReferenceLinkDescription
        {
            get => referenceDescription;
            private set
            {
                referenceDescription = value;

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
        public ReferenceLinkSettings(IBasePathProvider provider)
        {
            this.HelpFileProject = new FilePath(provider);
            this.MSHelpViewerSdkLinkType = MSHelpViewerSdkLinkType.Id;
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

            this.ReferenceLinkDescription = String.Format(CultureInfo.CurrentCulture, "{0} (HTML: {1}, MSHC: {2}, " +
                "Website/Markdown/Open XML: {3})", helpFileProject.PersistablePath, this.HtmlSdkLinkType,
                this.MSHelpViewerSdkLinkType, this.WebsiteSdkLinkType);
        }
        #endregion

        #region Convert from/to XML
        //=====================================================================

        /// <summary>
        /// Create a reference link settings instance from an XML element containing the settings
        /// </summary>
        /// <param name="pathProvider">The base path provider object</param>
        /// <param name="configuration">The XML element from which to obtain the settings.</param>
        /// <returns>A <see cref="ReferenceLinkSettings"/> object containing the settings from the XML element</returns>
        /// <remarks>It should contain an element called <c>target</c> with two attributes (<c>linkType</c> and
        /// <c>helpFileProject</c>).</remarks>
        public static ReferenceLinkSettings FromXml(IBasePathProvider pathProvider, XElement configuration)
        {
            ReferenceLinkSettings rl = new ReferenceLinkSettings(pathProvider);

            if(configuration != null)
            {
                // Ignore if it's the older style
                if(configuration.Attribute("linkType") == null)
                {
                    rl.HtmlSdkLinkType = (HtmlSdkLinkType)Enum.Parse(typeof(HtmlSdkLinkType),
                        configuration.Attribute("htmlSdkLinkType").Value, true);
                    rl.MSHelpViewerSdkLinkType = (MSHelpViewerSdkLinkType)Enum.Parse(typeof(MSHelpViewerSdkLinkType),
                        configuration.Attribute("helpViewerSdkLinkType").Value, true);
                    rl.WebsiteSdkLinkType = (HtmlSdkLinkType)Enum.Parse(typeof(HtmlSdkLinkType),
                        configuration.Attribute("websiteSdkLinkType").Value, true);
                }

                string path = configuration.Attribute("helpFileProject").Value;

                rl.HelpFileProject = new FilePath(path, Path.IsPathRooted(path), pathProvider);
            }

            return rl;
        }

        /// <summary>
        /// Store the reference link settings as an XML element
        /// </summary>
        /// <returns>Returns the XML element containing the settings</returns>
        /// <remarks>The reference link settings are stored in an element called <c>target</c> with attributes
        /// for each of the link types and the project name (<c>htmlSdkLinkType</c>, <c>helpViewerSdkLinkType</c>,
        /// <c>websiteSdkLinkType</c>, and <c>helpFileProject</c>).</remarks>
        public XElement ToXml()
        {
            return new XElement("target",
                new XAttribute("htmlSdkLinkType", this.HtmlSdkLinkType.ToString()),
                new XAttribute("helpViewerSdkLinkType", this.MSHelpViewerSdkLinkType.ToString()),
                new XAttribute("websiteSdkLinkType", this.WebsiteSdkLinkType.ToString()),
                new XAttribute("helpFileProject", helpFileProject.PersistablePath));
        }
        #endregion
    }
}
