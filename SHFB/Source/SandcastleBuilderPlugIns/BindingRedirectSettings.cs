//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BindingRedirectSettings.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/30/2019
// Note    : Copyright 2008-2019, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
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
    /// This represents binding redirection settings for the <see cref="BindingRedirectResolverPlugIn"/>.
    /// </summary>
    [DefaultProperty("AssemblyName")]
    public class BindingRedirectSettings
    {
        #region Private data members
        //=====================================================================

        private string assemblyName, oldVersionFrom, oldVersionTo, newVersion;
        private FilePath configFile;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the assembly name (no extension)
        /// </summary>
        [Category("Binding Redirect"), Description("The assembly name (no path or extension)")]
        public string AssemblyName
        {
            get => assemblyName;
            set
            {
                if(String.IsNullOrWhiteSpace(value))
                    value = "assemblyName";

                assemblyName = value;
            }
        }

        /// <summary>
        /// This is used to get or set the public key token for the assembly
        /// </summary>
        /// <value>If omitted, "null" is assumed</value>
        [Category("Binding Redirect"), Description("The public key token of the assembly.  If omitted, " +
          "\"null\" is assumed.")]
        public string PublicKeyToken { get; set; }

        /// <summary>
        /// This is used to get or set the culture for the assembly
        /// </summary>
        /// <value>If omitted, "neutral" is assumed</value>
        [Category("Binding Redirect"), Description("The culture of the assembly.  If omitted, " +
          "\"neutral\" is assumed.")]
        public string Culture { get; set; }

        /// <summary>
        /// This is used to get or set the old version number to redirect to the new version number
        /// </summary>
        [Category("Binding Redirect"), Description("The old version number to redirect to the new " +
          "version number."), DefaultValue("1.0.0.0")]
        public string OldVersion
        {
            get => oldVersionFrom;
            set
            {
                if(value == null)
                    value = "1.0.0.0";

                oldVersionFrom = value;
            }
        }

        /// <summary>
        /// This is used to get or set the ending old version number range to redirect to the new version
        /// number.
        /// </summary>
        /// <value>If not set, only <see cref="OldVersion" /> will be used to redirect a single
        /// version.</value>
        [Category("Binding Redirect"), Description("The ending old version number range to redirect to " +
          "the new version number.  If not set, only OldVersion will be used to redirect a single version."),
          DefaultValue(null)]
        public string OldVersionTo { get; set; }

        /// <summary>
        /// This is used to get or set the new version number to which the old versions are redirected
        /// </summary>
        [Category("Binding Redirect"), Description("The new version number to which the old versions " +
          "are redirected."), DefaultValue("1.0.0.1")]
        public string NewVersion
        {
            get => newVersion;
            set
            {
                if(value == null)
                    value = "1.0.0.1";

                newVersion = value;
            }
        }

        /// <summary>
        /// This is used to get or set the path to the configuration file from which the settings should be
        /// imported.
        /// </summary>
        /// <value>If specified, the properties in the Binding Redirect category are ignored.</value>
        [Category("Import"), Description("The path to configuration file from which to import settings.  " +
          "If specified, all properties in the Binding Redirect category are ignored"),
          Editor(typeof(FilePathObjectEditor), typeof(UITypeEditor)),
          RefreshProperties(RefreshProperties.All),
          FileDialog("Select the configuration file to use",
            "Configuration Files (*.config)|*.config|All Files (*.*)|*.*", FileDialogType.FileOpen)]
        public FilePath ConfigurationFile
        {
            get => configFile;
            set
            {
                if(value == null)
                    value = new FilePath(configFile.BasePathProvider);

                configFile = value;
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
            configFile = new FilePath(provider);
            assemblyName = "assemblyName";
            oldVersionFrom = "1.0.0.0";
            newVersion = "1.0.0.1";
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

            if(configFile.Path.Length != 0)
                return configFile.PersistablePath;

            if(oldVersionTo == null)
                range = oldVersionFrom.ToString();
            else
                range = String.Format(CultureInfo.InvariantCulture, "{0}-{1}", oldVersionFrom, oldVersionTo);

            return String.Format(CultureInfo.InvariantCulture, "{0}, Culture={1}, PublicKeyToken={2}, " +
                "Version(s) {3} redirect to Version {4}", assemblyName,
                String.IsNullOrWhiteSpace(this.Culture) ? "neutral" : this.Culture,
                String.IsNullOrWhiteSpace(this.PublicKeyToken) ? "null" : this.PublicKeyToken, range, newVersion);
        }
        #endregion

        #region Convert from/to XML
        //=====================================================================

        /// <summary>
        /// Create a binding redirect settings instance from an XPath navigator containing the settings
        /// </summary>
        /// <param name="pathProvider">The base path provider object</param>
        /// <param name="navigator">The XPath navigator from which to obtain the settings</param>
        /// <returns>A <see cref="BindingRedirectSettings"/> object containing the settings from the XPath
        /// navigator.</returns>
        /// <remarks>It should contain an element called <c>dependentAssembly</c> with a <c>configFile</c>
        /// attribute or a nested <c>assemblyIdentity</c> and <c>bindingRedirect</c> element that define
        /// the settings.</remarks>
        public static BindingRedirectSettings FromXPathNavigator(IBasePathProvider pathProvider,
          XPathNavigator navigator)
        {
            BindingRedirectSettings brs = new BindingRedirectSettings(pathProvider);

            if(navigator != null)
            {
                string value = navigator.GetAttribute("importFrom", String.Empty).Trim();

                if(value.Length != 0)
                    brs.ConfigurationFile = new FilePath(value, pathProvider);
                else
                {
                    XPathNavigator nav = navigator.SelectSingleNode("assemblyIdentity");

                    if(nav != null)
                    {
                        brs.AssemblyName = nav.GetAttribute("name", String.Empty).Trim();
                        brs.PublicKeyToken = nav.GetAttribute("publicKeyToken", String.Empty).Trim();
                        brs.Culture = nav.GetAttribute("culture", String.Empty).Trim();
                    }

                    nav = navigator.SelectSingleNode("bindingRedirect");

                    if(nav != null)
                    {
                        value = nav.GetAttribute("newVersion", String.Empty).Trim();

                        if(value.Length != 0)
                            brs.NewVersion = value;

                        value = nav.GetAttribute("oldVersion", String.Empty).Trim();

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
                                brs.oldVersionTo = tempVersion.ToString();
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
        /// Store the binding redirect settings as a node in the given XML document
        /// </summary>
        /// <param name="config">The XML document</param>
        /// <param name="root">The node in which to store the element</param>
        /// <param name="relativePath">True to allow a relative path on <c>importFrom</c> attributes, false to
        /// fully qualify the path.</param>
        /// <returns>Returns the node that was added.</returns>
        /// <remarks>The settings are stored in an element called <c>dependentAssembly</c>.</remarks>
        public XmlNode ToXml(XmlDocument config, XmlNode root, bool relativePath)
        {
            XmlAttribute attr;

            if(config == null)
                throw new ArgumentNullException(nameof(config));

            if(root == null)
                throw new ArgumentNullException(nameof(root));

            XmlNode node = config.CreateNode(XmlNodeType.Element, "dependentAssembly", null);
            root.AppendChild(node);

            if(configFile.Path.Length != 0)
            {
                attr = config.CreateAttribute("importFrom");
                attr.Value = relativePath ? configFile.PersistablePath : configFile.ToString();
                node.Attributes.Append(attr);
                return node;
            }

            XmlNode child = config.CreateNode(XmlNodeType.Element, "assemblyIdentity", null);
            node.AppendChild(child);

            attr = config.CreateAttribute("name");
            attr.Value = assemblyName;
            child.Attributes.Append(attr);

            if(!String.IsNullOrWhiteSpace(this.PublicKeyToken))
            {
                attr = config.CreateAttribute("publicKeyToken");
                attr.Value = this.PublicKeyToken;
                child.Attributes.Append(attr);
            }

            if(!String.IsNullOrWhiteSpace(this.Culture))
            {
                attr = config.CreateAttribute("culture");
                attr.Value = this.Culture;
                child.Attributes.Append(attr);
            }

            child = config.CreateNode(XmlNodeType.Element, "bindingRedirect", null);
            node.AppendChild(child);

            attr = config.CreateAttribute("oldVersion");
            attr.Value = assemblyName;

            if(oldVersionTo == null)
                attr.Value = oldVersionFrom.ToString();
            else
            {
                if(Version.TryParse(oldVersionFrom, out Version oldFrom) &&
                  Version.TryParse(oldVersionTo, out Version oldTo) && oldFrom > oldTo)
                {
                    Version tempVersion = oldFrom;
                    oldVersionFrom = oldTo.ToString();
                    oldVersionTo = tempVersion.ToString();
                }

                attr.Value = String.Format(CultureInfo.InvariantCulture, "{0}-{1}", oldVersionFrom,
                    oldVersionTo);
                child.Attributes.Append(attr);
            }

            child.Attributes.Append(attr);

            attr = config.CreateAttribute("newVersion");
            attr.Value = newVersion.ToString();
            child.Attributes.Append(attr);

            return node;
        }
        #endregion
    }
}
