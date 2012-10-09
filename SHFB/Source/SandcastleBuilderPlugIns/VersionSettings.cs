//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : VersionSettings.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/13/2008
// Note    : Copyright 2007-2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing version settings for the Version
// Builder plug-in.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.3  12/01/2007  EFW  Created the code
// 1.8.0.0  08/13/2008  EFW  Updated to support the new project format
//=============================================================================

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This represents version settings for the <see cref="VersionBuilderPlugIn"/>.
    /// </summary>
    [DefaultProperty("Version")]
    public class VersionSettings
    {
        #region Private data members
        //=====================================================================
        // Private data members

        private string frameworkLabel, version, reflectionFilename;
        private FilePath helpFileProject;
        #endregion

        #region Properties
        //=====================================================================
        // Properties

        /// <summary>
        /// This is used to get or set the location of the reflection file
        /// at build time.
        /// </summary>
        internal string ReflectionFilename
        {
            get { return reflectionFilename; }
            set { reflectionFilename = value; }
        }

        /// <summary>
        /// This is used to get or set the framework label for the related
        /// project.
        /// </summary>
        /// <remarks>This is used to group like versions of projects.</remarks>
        [Category("Framework"), Description("The framework label used to " +
          "group like version of the projects.")]
        public string FrameworkLabel
        {
            get { return frameworkLabel; }
            set
            {
                if(!String.IsNullOrEmpty(value))
                    frameworkLabel = value.Trim();
                else
                    frameworkLabel = value;
            }
        }

        /// <summary>
        /// This is used to get or set the version number description for the
        /// related project.
        /// </summary>
        [Category("Framework"), Description("The version represented by " +
          "the project.")]
        public string Version
        {
            get { return version; }
            set
            {
                version = value;

                if(String.IsNullOrEmpty(version))
                    version = "1.0";
            }
        }

        /// <summary>
        /// This is used to get or set the path to the help file builder
        /// project used to generate version information.
        /// </summary>
        /// <value>The help file builder project makes it simple to manage
        /// settings for the prior version's assemblies such as references,
        /// API filter settings, etc.</value>
        [Category("Framework"), Description("The path to the help file " +
          "builder project for the prior version."),
          Editor(typeof(FilePathObjectEditor), typeof(UITypeEditor)),
          RefreshProperties(RefreshProperties.All),
          FileDialog("Select the help file builder project",
            "Sandcastle Help File Builder Project Files " +
            "(*.shfbproj)|*.shfbproj|All Files (*.*)|*.*",
            FileDialogType.FileOpen)]
        public FilePath HelpFileProject
        {
            get { return helpFileProject; }
            set
            {
                if(value == null || value.Path.Length == 0)
                    throw new BuilderException("VBP0001",
                        "The help file project cannot be blank");

                helpFileProject = value;
            }
        }

        /// <summary>
        /// This returns a description of the entry suitable for display in a
        /// bound list control.
        /// </summary>
        [Category("Info"), Description("Version description")]
        public string ListDescription
        {
            get
            {
                return String.Format(CultureInfo.CurrentCulture,
                    "{0} {1} - {2}", frameworkLabel, version,
                    helpFileProject.PersistablePath);
            }
        }
        #endregion

        #region Designer methods
        //=====================================================================
        // Designer methods

        /// <summary>
        /// This is used to see if the <see cref="Version"/> property
        /// should be serialized.
        /// </summary>
        /// <returns>True to serialize it, false if it matches the default
        /// and should not be serialized.  This property cannot be reset
        /// as it should always have a value.</returns>
        private bool ShouldSerializeVersion()
        {
            return !String.IsNullOrEmpty(version);
        }

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
        // Methods, etc.

        /// <summary>
        /// Constructor
        /// </summary>
        public VersionSettings()
        {
            frameworkLabel = " ";
            version = "1.0";
        }
        #endregion

        #region Convert from/to XML
        //=====================================================================

        /// <summary>
        /// Create a version settings instance from an XPath navigator
        /// containing the settings.
        /// </summary>
        /// <param name="pathProvider">The base path provider object</param>
        /// <param name="navigator">The XPath navigator from which to
        /// obtain the settings.</param>
        /// <returns>A <see cref="VersionSettings"/> object containing the
        /// settings from the XPath navigator.</returns>
        /// <remarks>It should contain an element called <c>version</c>
        /// with three attributes (<c>label</c>, <c>version</c> and
        /// <c>helpFileProject</c>).</remarks>
        public static VersionSettings FromXPathNavigator(
          IBasePathProvider pathProvider, XPathNavigator navigator)
        {
            VersionSettings vs = new VersionSettings();

            if(navigator != null)
            {
                vs.FrameworkLabel = navigator.GetAttribute("label",
                    String.Empty).Trim();
                vs.Version = navigator.GetAttribute("version",
                    String.Empty).Trim();
                vs.HelpFileProject = new FilePath(navigator.GetAttribute(
                    "helpFileProject", String.Empty).Trim(), pathProvider);
            }

            return vs;
        }

        /// <summary>
        /// Store the version settings as a node in the given XML document
        /// </summary>
        /// <param name="config">The XML document</param>
        /// <param name="root">The node in which to store the element</param>
        /// <returns>Returns the node that was added.</returns>
        /// <remarks>The version settings are stored in an element called
        /// <c>version</c> with three attributes (<c>label</c>, <c>version</c>
        /// and <c>helpFileProject</c>).</remarks>
        public XmlNode ToXml(XmlDocument config, XmlNode root)
        {
            XmlNode node;
            XmlAttribute attr;

            if(config == null)
                throw new ArgumentNullException("config");

            if(root == null)
                throw new ArgumentNullException("root");

            node = config.CreateNode(XmlNodeType.Element, "version", null);
            root.AppendChild(node);

            attr = config.CreateAttribute("label");
            attr.Value = frameworkLabel;
            node.Attributes.Append(attr);

            attr = config.CreateAttribute("version");
            attr.Value = version;
            node.Attributes.Append(attr);

            attr = config.CreateAttribute("helpFileProject");
            attr.Value = helpFileProject.PersistablePath;
            node.Attributes.Append(attr);

            return node;
        }

        /// <summary>
        /// This is overridden to return the hash code of the combined
        /// framework and version number.
        /// </summary>
        /// <returns>The hash code for the version settings.</returns>
        public override int GetHashCode()
        {
            string hash = frameworkLabel + version;
            return hash.GetHashCode();
        }
        #endregion
    }
}
