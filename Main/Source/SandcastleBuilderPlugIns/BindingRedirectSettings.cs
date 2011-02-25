//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BindingRedirectSettings.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/19/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing binding redirection settings for the
// Assembly Binding Redirection Resolver plug-in.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.1  11/14/2008  EFW  Created the code
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
    /// This represents binding redirection settings for the
    /// <see cref="BindingRedirectResolverPlugIn"/>.
    /// </summary>
    [DefaultProperty("AssemblyName")]
    public class BindingRedirectSettings
    {
        #region Private data members
        //=====================================================================
        // Private data members

        private string assemblyName, publicKeyToken, culture;
        private Version oldVersionFrom, oldVersionTo, newVersion;
        private FilePath configFile;
        #endregion

        #region Properties
        //=====================================================================
        // Properties

        /// <summary>
        /// This is used to get or set the assembly name (no extension)
        /// </summary>
        [Category("Binding Redirect"), Description("The assembly name " +
            "(no path or extension)")]
        public string AssemblyName
        {
            get { return assemblyName; }
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
        [Category("Binding Redirect"), Description("The public key token " +
            "of the assembly.  If omitted, \"null\" is assumed.")]
        public string PublicKeyToken
        {
            get { return publicKeyToken; }
            set { publicKeyToken = value; }
        }

        /// <summary>
        /// This is used to get or set the culture for the assembly
        /// </summary>
        /// <value>If omitted, "neutral" is assumed</value>
        [Category("Binding Redirect"), Description("The culture of the " +
            "assembly.  If omitted, \"neutral\" is assumed.")]
        public string Culture
        {
            get { return culture; }
            set { culture = value; }
        }

        /// <summary>
        /// This is used to get or set the old version number to redirect to
        /// the new version number.
        /// </summary>
        [Category("Binding Redirect"), Description("The old version number " +
          "to redirect to the new version number."),
          TypeConverter(typeof(VersionTypeConverter))]
        public Version OldVersion
        {
            get { return oldVersionFrom; }
            set
            {
                if(value == null)
                    value = new Version(1, 0, 0, 0);

                oldVersionFrom = value;
            }
        }

        /// <summary>
        /// This is used to get or set the ending old version number range to
        /// redirect to the new version number.
        /// </summary>
        /// <value>If not set, only <see cref="OldVersion" /> will be used to
        /// redirect a single version.</value>
        [Category("Binding Redirect"), Description("The ending old version " +
          "number range to redirect to the new version number.  If not set, " +
          "only OldVersion will be used to redirect a single version."),
          TypeConverter(typeof(VersionTypeConverter))]
        public Version OldVersionTo
        {
            get { return oldVersionTo; }
            set { oldVersionTo = value; }
        }

        /// <summary>
        /// This is used to get or set the new version number to which the old
        /// versions are redirected.
        /// </summary>
        [Category("Binding Redirect"), Description("The new version number " +
            "to which the old versions are redirected."),
          TypeConverter(typeof(VersionTypeConverter))]
        public Version NewVersion
        {
            get { return newVersion; }
            set
            {
                if(value == null)
                    value = new Version(1, 0, 0, 1);

                newVersion = value;
            }
        }

        /// <summary>
        /// This is used to get or set the path to the configuration file from
        /// which the settings should be imported.
        /// </summary>
        /// <value>If specified, the properties in the Binding Redirect
        /// category are ignored.</value>
        [Category("Import"), Description("The path to configuration file from " +
          "which to import settings.  If specified, all properties in the " +
          "Binding Redirect category are ignored"),
          Editor(typeof(FilePathObjectEditor), typeof(UITypeEditor)),
          RefreshProperties(RefreshProperties.All),
          FileDialog("Select the configuration file to use",
            "Configuration Files (*.config)|*.config|All Files (*.*)|*.*",
            FileDialogType.FileOpen)]
        public FilePath ConfigurationFile
        {
            get { return configFile; }
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
        // Methods, etc.

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="provider">The base path provider</param>
        public BindingRedirectSettings(IBasePathProvider provider)
        {
            configFile = new FilePath(provider);
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

            if(configFile.Path.Length != 0)
                return configFile.PersistablePath;

            if(oldVersionTo == null)
                range = oldVersionFrom.ToString();
            else
                range = String.Format(CultureInfo.InvariantCulture, "{0}-{1}",
                    oldVersionFrom, oldVersionTo);

            return String.Format(CultureInfo.InvariantCulture, "{0}, Culture=" +
                "{1}, PublicKeyToken={2}, Version(s) {3} redirect to Version {4}",
                assemblyName, String.IsNullOrEmpty(culture) ? "neutral" : culture,
                String.IsNullOrEmpty(publicKeyToken) ? "null" : publicKeyToken,
                range, newVersion);
        }
        #endregion

        #region Convert from/to XML
        //=====================================================================

        /// <summary>
        /// Create a binding redirect settings instance from an XPath navigator
        /// containing the settings.
        /// </summary>
        /// <param name="pathProvider">The base path provider object</param>
        /// <param name="navigator">The XPath navigator from which to
        /// obtain the settings.</param>
        /// <returns>A <see cref="BindingRedirectSettings"/> object containing
        /// the settings from the XPath navigator.</returns>
        /// <remarks>It should contain an element called <c>dependentAssembly</c>
        /// with a configFile attribute or a nested <c>assemblyIdentity</c> and
        /// <c>bindingRedirect</c> element that define the settings.</remarks>
        public static BindingRedirectSettings FromXPathNavigator(
          IBasePathProvider pathProvider, XPathNavigator navigator)
        {
            BindingRedirectSettings brs = new BindingRedirectSettings(pathProvider);
            XPathNavigator nav;
            string value;
            string[] versions;
            Version tempVersion;

            if(navigator != null)
            {
                value = navigator.GetAttribute("importFrom", String.Empty).Trim();

                if(value.Length != 0)
                    brs.ConfigurationFile = new FilePath(value, pathProvider);
                else
                {
                    nav = navigator.SelectSingleNode("assemblyIdentity");

                    if(nav != null)
                    {
                        brs.AssemblyName = nav.GetAttribute("name",
                            String.Empty).Trim();
                        brs.PublicKeyToken = nav.GetAttribute("publicKeyToken",
                            String.Empty).Trim();
                        brs.Culture = nav.GetAttribute("culture",
                            String.Empty).Trim();
                    }

                    nav = navigator.SelectSingleNode("bindingRedirect");

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
        /// Store the binding redirect settings as a node in the given XML
        /// document.
        /// </summary>
        /// <param name="config">The XML document</param>
        /// <param name="root">The node in which to store the element</param>
        /// <returns>Returns the node that was added.</returns>
        /// <remarks>The settings are stored in an element called
        /// <c>dependentAssembly</c>.</remarks>
        public XmlNode ToXml(XmlDocument config, XmlNode root)
        {
            XmlNode node, child;
            XmlAttribute attr;
            Version tempVersion;

            if(config == null)
                throw new ArgumentNullException("config");

            if(root == null)
                throw new ArgumentNullException("root");

            node = config.CreateNode(XmlNodeType.Element, "dependentAssembly", null);
            root.AppendChild(node);

            if(configFile.Path.Length != 0)
            {
                attr = config.CreateAttribute("importFrom");
                attr.Value = configFile.PersistablePath;
                node.Attributes.Append(attr);
                return node;
            }

            child = config.CreateNode(XmlNodeType.Element, "assemblyIdentity", null);
            node.AppendChild(child);

            attr = config.CreateAttribute("name");
            attr.Value = assemblyName;
            child.Attributes.Append(attr);

            if(!String.IsNullOrEmpty(publicKeyToken))
            {
                attr = config.CreateAttribute("publicKeyToken");
                attr.Value = publicKeyToken;
                child.Attributes.Append(attr);
            }

            if(!String.IsNullOrEmpty(culture))
            {
                attr = config.CreateAttribute("culture");
                attr.Value = culture;
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
                if(oldVersionFrom > oldVersionTo)
                {
                    tempVersion = oldVersionFrom;
                    oldVersionFrom = oldVersionTo;
                    oldVersionTo = tempVersion;
                }

                attr.Value = String.Format(CultureInfo.InvariantCulture,
                    "{0}-{1}", oldVersionFrom, oldVersionTo);
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
