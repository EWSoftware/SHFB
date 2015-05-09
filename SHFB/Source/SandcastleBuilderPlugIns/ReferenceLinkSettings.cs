//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ReferenceLinkSettings.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/03/2015
// Note    : Copyright 2008-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
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
using System.Drawing.Design;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This represents reference link settings for the
    /// <see cref="AdditionalReferenceLinksPlugIn"/>.
    /// </summary>
    [DefaultProperty("HelpFileProject")]
    public class ReferenceLinkSettings
    {
        #region Private data members
        //=====================================================================

        private FilePath helpFileProject;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the location of the reflection file
        /// at build time.
        /// </summary>
        internal string ReflectionFilename { get; set; }

        /// <summary>
        /// This is used to get or set the HTML Help 1 SDK link type for the target
        /// </summary>
        [Category("Link"), Description("The reference link type to use for HTML Help 1 files"),
          DefaultValue(HtmlSdkLinkType.None)]
        public HtmlSdkLinkType HtmlSdkLinkType { get; set; }

        /// <summary>
        /// This is used to get or set the MS Help Viewer SDK link type for the target
        /// </summary>
        [Category("Link"), Description("The reference link type to use for MS Help Viewer files"),
          DefaultValue(MSHelpViewerSdkLinkType.Id)]
        public MSHelpViewerSdkLinkType MSHelpViewerSdkLinkType { get; set; }

        /// <summary>
        /// This is used to get or set the website SDK link type for the target
        /// </summary>
        [Category("Link"), Description("The reference link type to use for website files"),
          DefaultValue(HtmlSdkLinkType.None)]
        public HtmlSdkLinkType WebsiteSdkLinkType { get; set; }

        /// <summary>
        /// This is used to get or set the path to the help file builder
        /// project used to generate reference link information.
        /// </summary>
        /// <value>The help file builder project makes it simple to manage
        /// settings for the other target's assemblies such as references,
        /// API filter settings, etc.</value>
        [Category("Link"), Description("The path to the help file builder project for the other reference links."),
          Editor(typeof(FilePathObjectEditor), typeof(UITypeEditor)),
          RefreshProperties(RefreshProperties.All),
          FileDialog("Select the help file builder project",
            "Sandcastle Help File Builder Project Files " +
            "(*.shfbproj)|*.shfbproj|All Files (*.*)|*.*", FileDialogType.FileOpen)]
        public FilePath HelpFileProject
        {
            get { return helpFileProject; }
            set
            {
                if(value == null || value.Path.Length == 0)
                    throw new BuilderException("ARL0007", "The help file project cannot be blank");

                helpFileProject = value;
            }
        }

        /// <summary>
        /// This returns a description of the entry suitable for display in a
        /// bound list control.
        /// </summary>
        [Category("Link Info"), Description("List description")]
        public string ListDescription
        {
            get
            {   
                return String.Format(CultureInfo.CurrentCulture, "{0} (HTML: {1}, MSHC: {2}, " +
                    "Website/Markdown/Open XML: {3})", helpFileProject.PersistablePath, this.HtmlSdkLinkType,
                    this.MSHelpViewerSdkLinkType, this.WebsiteSdkLinkType);
            }
        }
        #endregion

        #region Designer methods
        //=====================================================================

        /// <summary>
        /// This is used to see if the <see cref="HelpFileProject"/> property
        /// should be serialized.
        /// </summary>
        /// <returns>True to serialize it, false if it matches the default
        /// and should not be serialized.  This property cannot be reset
        /// as it should always have a value.</returns>
        private bool ShouldSerializeHelpFileProject()
        {
            return (this.HelpFileProject.Path.Length != 0);
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public ReferenceLinkSettings()
        {
            this.MSHelpViewerSdkLinkType = MSHelpViewerSdkLinkType.Id;
        }
        #endregion

        #region Convert from/to XML
        //=====================================================================

        /// <summary>
        /// Create a reference link settings instance from an XPath navigator
        /// containing the settings.
        /// </summary>
        /// <param name="pathProvider">The base path provider object</param>
        /// <param name="navigator">The XPath navigator from which to
        /// obtain the settings.</param>
        /// <returns>A <see cref="ReferenceLinkSettings"/> object containing the
        /// settings from the XPath navigator.</returns>
        /// <remarks>It should contain an element called <c>target</c>
        /// with two attributes (<c>linkType</c> and <c>helpFileProject</c>).
        /// </remarks>
        public static ReferenceLinkSettings FromXPathNavigator(
          IBasePathProvider pathProvider, XPathNavigator navigator)
        {
            ReferenceLinkSettings rl = new ReferenceLinkSettings();

            if(navigator != null)
            {
                // Ignore if it's the older style
                if(navigator.GetAttribute("linkType", String.Empty).Length == 0)
                {
                    rl.HtmlSdkLinkType = (HtmlSdkLinkType)Enum.Parse(typeof(HtmlSdkLinkType), navigator.GetAttribute(
                        "htmlSdkLinkType", String.Empty).Trim(), true);
                    rl.MSHelpViewerSdkLinkType = (MSHelpViewerSdkLinkType)Enum.Parse(typeof(MSHelpViewerSdkLinkType),
                        navigator.GetAttribute("helpViewerSdkLinkType", String.Empty).Trim(), true);
                    rl.WebsiteSdkLinkType = (HtmlSdkLinkType)Enum.Parse(typeof(HtmlSdkLinkType), navigator.GetAttribute(
                        "websiteSdkLinkType", String.Empty).Trim(), true);
                }

                rl.HelpFileProject = new FilePath(navigator.GetAttribute("helpFileProject", String.Empty).Trim(),
                    pathProvider);
            }

            return rl;
        }

        /// <summary>
        /// Store the reference link settings as a node in the given XML
        /// document.
        /// </summary>
        /// <param name="config">The XML document</param>
        /// <param name="root">The node in which to store the element</param>
        /// <returns>Returns the node that was added.</returns>
        /// <remarks>The reference link settings are stored in an element
        /// called <c>target</c> with two attributes (<c>linkType</c> and
        /// <c>helpFileProject</c>).</remarks>
        public XmlNode ToXml(XmlDocument config, XmlNode root)
        {
            XmlNode node;
            XmlAttribute attr;

            if(config == null)
                throw new ArgumentNullException("config");

            if(root == null)
                throw new ArgumentNullException("root");

            node = config.CreateNode(XmlNodeType.Element, "target", null);
            root.AppendChild(node);

            attr = config.CreateAttribute("htmlSdkLinkType");
            attr.Value = this.HtmlSdkLinkType.ToString();
            node.Attributes.Append(attr);

            attr = config.CreateAttribute("helpViewerSdkLinkType");
            attr.Value = this.MSHelpViewerSdkLinkType.ToString();
            node.Attributes.Append(attr);

            attr = config.CreateAttribute("websiteSdkLinkType");
            attr.Value = this.WebsiteSdkLinkType.ToString();
            node.Attributes.Append(attr);

            attr = config.CreateAttribute("helpFileProject");
            attr.Value = helpFileProject.PersistablePath;
            node.Attributes.Append(attr);

            return node;
        }
        #endregion
    }
}
