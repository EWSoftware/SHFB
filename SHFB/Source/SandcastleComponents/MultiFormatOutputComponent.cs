//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : MultiFormatOutputComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/03/2016
// Note    : Copyright 2010-2016, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a build component that is used to execute one or more sets of build components each based
// on a specific help file output format.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/06/2010  EFW  Created the code
// 12/26/2013  EFW  Updated the build component to be discoverable via MEF
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This build component is used to execute one or more sets of build components each based on a specific
    /// help file output format.
    /// </summary>
    /// <remarks>One or more components can be executed based on a specified list of one or more help file output
    /// formats.  Only the components related to the requested set of format types will be executed.</remarks>
    /// <example>
    /// <code lang="xml" title="Example Configuration"
    ///     source="..\..\SHFB\Source\PresentationStyles\VS2013\Configuration\BuildAssembler.config"
    ///     region="Multi-format output component" />
    /// </example>
    public class MultiFormatOutputComponent : BuildComponentCore
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("Multi-format Output Component")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new MultiFormatOutputComponent(this.BuildAssembler);
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private Dictionary<string, IEnumerable<BuildComponentCore>> formatComponents;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected MultiFormatOutputComponent(BuildAssemblerCore buildAssembler) : base(buildAssembler)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>This sets a unique group ID for each format</remarks>
        public override string GroupId
        {
            get { return base.GroupId; }
            set
            {
                base.GroupId = value;

                foreach(var keyValue in formatComponents)
                {
                    string formatGroupId = value + "/" + keyValue.Key;

                    foreach(var component in keyValue.Value)
                        component.GroupId = formatGroupId;
                }
            }
        }

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            List<string> buildFormats = new List<string>();
            XPathNavigator nav;
            XPathNodeIterator outputSets;
            string format;

            this.WriteMessage(MessageLevel.Info, String.Format(CultureInfo.InvariantCulture,
                "[{0}, version {1}]\r\n    Multi-Format Output Component. {2}" +
                "\r\n    https://GitHub.com/EWSoftware/SHFB", fvi.ProductName, fvi.ProductVersion,
                fvi.LegalCopyright));

            formatComponents = new Dictionary<string, IEnumerable<BuildComponentCore>>();

            // Get the requested formats
            nav = configuration.SelectSingleNode("build");
            format = nav.GetAttribute("formats", String.Empty);

            if(String.IsNullOrEmpty(format))
                throw new ConfigurationErrorsException("You must specify a string value for the <build> " +
                    "'formats' attribute.");

            foreach(string f in format.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                buildFormats.Add(f.Trim());

            // Get the component configurations for each of the requested formats
            outputSets = configuration.Select("helpOutput");

            foreach(XPathNavigator set in outputSets)
            {
                format = set.GetAttribute("format", String.Empty);

                if(String.IsNullOrWhiteSpace(format))
                    throw new ConfigurationErrorsException("You must specify a string value for the " +
                        "<helpOutput> 'format' attribute.");

                // Only include formats that were requested
                if(buildFormats.Contains(format))
                {
                    this.WriteMessage(MessageLevel.Info, "Loading components for " + format + " format");
                    formatComponents.Add(format, this.BuildAssembler.LoadComponents(set));
                }
            }

            // Set a default group ID
            this.GroupId = null;
        }

        /// <summary>
        /// This is implemented to execute each set of components for the requested output formats.
        /// </summary>
        /// <param name="document">The XML document with which to work.</param>
        /// <param name="key">The key (member name) of the item being documented.</param>
        public override void Apply(XmlDocument document, string key)
        {
            XmlDocument clone;

            // No need to clone the document if there's only one format
            if(formatComponents.Count == 1)
            {
                foreach(var component in formatComponents.Values.First())
                    component.Apply(document, key);
            }
            else
                foreach(var componentSet in formatComponents.Values)
                {
                    // Do not use XmlNode.Clone() here!  For some reason, it can cause sporadic failures in the
                    // Shared Content component due to supposed invalid/missing parent nodes on the replaced
                    // items.  Creating a copy of the document from the XML works around this issue.
                    clone = new XmlDocument();
                    clone.PreserveWhitespace = document.PreserveWhitespace;
                    clone.LoadXml(document.OuterXml);

                    foreach(var component in componentSet)
                        component.Apply(clone, key);
                }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if(disposing)
                foreach(var list in formatComponents.Values)
                    foreach(var component in list)
                        component.Dispose();

            base.Dispose(disposing);
        }
        #endregion
    }
}
