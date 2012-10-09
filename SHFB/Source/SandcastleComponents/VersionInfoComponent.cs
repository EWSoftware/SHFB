//=============================================================================
// System  : Sandcastle Help File Builder Components
// File    : VersionInfoComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/06/2009
// Note    : Copyright 2007-2009, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a build component that is used to obtain version
// information for each topic so that it can be placed in the footer by the
// PostTransformComponent.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.4.0.0  02/17/2007  EFW  Created the code
// 1.5.0.0  06/19/2007  EFW  Updated for use with the June CTP
// 1.6.0.1  10/28/2007  EFW  Merged changes from Martin Darilek to show the
//                           file version attribute value if present.
// 1.6.0.7  03/24/2008  EFW  Updated it to handle multiple assemblies
//=============================================================================

using System;
using System.Configuration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This build component is used to obtain version information for each
    /// topic so that it can be placed in the footer by the
    /// <see cref="PostTransformComponent"/>.
    /// </summary>
    /// <remarks>The <see cref="PostTransformComponent"/> adds the version
    /// information to the topic after it has been transformed into HTML.
    /// We need to get the version information here though as the reference
    /// information is lost once it has been transformed.</remarks>
    /// <example>
    /// <code lang="xml" title="Example configuration">
    /// &lt;!-- Version information component configuration.  This must
    ///      appear before the TransformComponent.  See also:
    ///      PostTransformComponent --&gt;
    /// &lt;component type="SandcastleBuilder.Components.VersionInfoComponent"
    ///   assembly="C:\SandcastleBuilder\SandcastleBuilder.Components.dll"&gt;
    ///     &lt;!-- Reflection information file for version info (required) --&gt;
    ///     &lt;reflectionFile filename="reflection.xml" /&gt;
    /// &lt;/component&gt;
    /// </code>
    /// </example>
    public class VersionInfoComponent : BuildComponent
    {
        #region Private data members
        //=====================================================================
        // Private data members

        private static Collection<string> itemVersions = new Collection<string>();
        private Dictionary<string, string> assemblyVersions;

        #endregion

        #region Properties

        /// <summary>
        /// This is used by the <see cref="PostTransformComponent"/> to get the
        /// assembly versions for the current topic.
        /// </summary>
        public static Collection<string> ItemVersions
        {
            get { return itemVersions; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembler">A reference to the build assembler.</param>
        /// <param name="configuration">The configuration information</param>
        /// <remarks>See the <see cref="VersionInfoComponent"/> class topic
        /// for an example of the configuration</remarks>
        /// <exception cref="ConfigurationErrorsException">This is thrown if
        /// an error is detected in the configuration.</exception>
        public VersionInfoComponent(BuildAssembler assembler,
          XPathNavigator configuration) : base(assembler, configuration)
        {
            XPathDocument assemblies;
            XPathNavigator nav, fileVersion;

            string reflectionFilename, line, asmVersion;
            StringBuilder sb = new StringBuilder(1024);

            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            base.WriteMessage(MessageLevel.Info, String.Format(
                CultureInfo.InvariantCulture,
                "\r\n    [{0}, version {1}]\r\n    Version Information " +
                "Component. {2}\r\n    http://SHFB.CodePlex.com",
                fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright));

            // The reflectionFile element is required.  The file must exist if
            // specified.  This is a hack to get version information into
            // each topic.  Note that it requires a modification to the
            // reference_content.xml file for each style.
            nav = configuration.SelectSingleNode("reflectionFile");
            if(nav == null)
                throw new ConfigurationErrorsException(
                    "The <reflectionFile> element is required for the " +
                    "VersionInfoComponent");

            reflectionFilename = nav.GetAttribute("filename", String.Empty);

            if(String.IsNullOrEmpty(reflectionFilename))
                throw new ConfigurationErrorsException("A filename is " +
                    "required in the <reflectionFile> element");

            if(!File.Exists(reflectionFilename))
                throw new ConfigurationErrorsException(
                    "The reflection file '" + reflectionFilename +
                    "' must exist");

            assemblyVersions = new Dictionary<string, string>();

            // Load the part of the reflection info file that contains the
            // assembly definitions.  The file itself can be huge so we
            // will use a stream reader to extract the assembly info and
            // load that into the XPath document.
            using(StreamReader sr = new StreamReader(reflectionFilename))
            {
                do
                {
                    line = sr.ReadLine();
                    sb.Append(line);

                } while(!line.Contains("/assemblies") &&
                  !line.Contains("<apis>"));
            }

            // Some projects like those for AjaxDoc don't have assemblies
            if(line.Contains("<apis>"))
                sb.Append("</apis>");

            sb.Append("</reflection>");

            using(StringReader sr = new StringReader(sb.ToString()))
            {
                assemblies = new XPathDocument(sr);
                nav = assemblies.CreateNavigator();

                foreach(XPathNavigator asmNode in nav.Select(
                  "reflection/assemblies/assembly"))
                {
                    asmVersion = asmNode.SelectSingleNode(
                        "assemblydata/@version").Value;

                    // If the file version attribute is present, show it too
                    fileVersion = asmNode.SelectSingleNode(
                        "attributes/attribute[type/@api='T:System.Reflection." +
                        "AssemblyFileVersionAttribute']/argument/value");

                    if(fileVersion != null)
                        asmVersion = String.Format(CultureInfo.InvariantCulture,
                            "{0} ({1})", asmVersion, fileVersion.InnerXml);

                    assemblyVersions.Add(asmNode.GetAttribute("name",
                        String.Empty), asmVersion);
                }
            }
        }
        #endregion

        #region Apply the component
        /// <summary>
        /// This is implemented to set the version information ready for use
        /// by the <see cref="PostTransformComponent"/>.
        /// </summary>
        /// <param name="document">The XML document with which to work.</param>
        /// <param name="key">The key (member name) of the item being
        /// documented.</param>
        public override void Apply(XmlDocument document, string key)
        {
            string version;

            itemVersions.Clear();

            // Project and namespace items don't have version info.  Some
            // projects like those for AjaxDoc don't either.
            if(key[0] != 'R' && key[0] != 'N' && assemblyVersions.Count != 0)
                foreach(XmlNode assembly in document.SelectNodes(
                  "document/reference/containers/library/@assembly"))
                {
                    if(!assemblyVersions.TryGetValue(assembly.Value,
                      out version))
                    {
                        base.WriteMessage(MessageLevel.Warn, "Unable to " +
                            "find version information for " + key);
                        version = "?.?.?.?";
                    }

                    itemVersions.Add(version);
                }
        }
        #endregion
    }
}
