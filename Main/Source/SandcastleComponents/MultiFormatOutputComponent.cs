//=============================================================================
// System  : Sandcastle Help File Builder Components
// File    : MultiFormatOutputComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/05/2010
// Note    : Copyright 2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a build component that is used to execute one or more
// sets of build components each based on a specific help file output format.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.0.0  06/06/2010  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This build component is used to execute one or more sets of build
    /// components each based on a specific help file output format.
    /// </summary>
    /// <remarks>One or more components can be executed based on a specified
    /// list of one or more help file output formats.  Only the components
    /// related to the requested set of format types will be executed.</remarks>
    /// <example>
    /// <code lang="xml" title="Example Configuration"
    ///     source="..\SandcastleBuilderGUI\Templates\VS2005.config"
    ///     region="Multi-format output component" />
    /// </example>
    public class MultiFormatOutputComponent : BuildComponent
    {
        #region Private data members
        //=====================================================================

        private Dictionary<string, IEnumerable<BuildComponent>> formatComponents;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the current help format being generated
        /// </summary>
        public static string CurrentFormat { get; private set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembler">A reference to the build assembler.</param>
        /// <param name="configuration">The configuration information</param>
        public MultiFormatOutputComponent(BuildAssembler assembler, XPathNavigator configuration) :
          base(assembler, configuration)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            List<string> buildFormats = new List<string>();
            XPathNavigator nav;
            XPathNodeIterator outputSets;
            string format;

            base.WriteMessage(MessageLevel.Info, String.Format(CultureInfo.InvariantCulture,
                "\r\n    [{0}, version {1}]\r\n    Multi-Format Output Component. {2}\r\n    http://SHFB.CodePlex.com",
                fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright));

            formatComponents = new Dictionary<string, IEnumerable<BuildComponent>>();

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

                if(String.IsNullOrEmpty(format))
                    throw new ConfigurationErrorsException("You must specify a string value for the <output> " +
                        "'format' attribute.");

                // Only include formats that were requested
                if(buildFormats.Contains(format))
                {
                    base.WriteMessage(MessageLevel.Info, "Loading components for " + format + " format");
                    formatComponents.Add(format, base.BuildAssembler.LoadComponents(set));
                }
            }
        }
        #endregion

        #region Apply the component
        //=====================================================================

        /// <summary>
        /// This is implemented to execute each set of components for the
        /// requested output formats.
        /// </summary>
        /// <param name="document">The XML document with which to work.</param>
        /// <param name="key">The key (member name) of the item being documented.</param>
        public override void Apply(XmlDocument document, string key)
        {
            XmlDocument clone;

            foreach(string format in formatComponents.Keys)
            {
                CurrentFormat = format;
                clone = (XmlDocument)document.Clone();

                foreach(BuildComponent component in formatComponents[format])
                    component.Apply(clone, key);
            }
        }
        #endregion

        #region Dispose of the component
        //=====================================================================

        /// <summary>
        /// Dispose of the nested components
        /// </summary>
        public override void Dispose()
        {
            foreach(IEnumerable<BuildComponent> list in formatComponents.Values)
                foreach(BuildComponent component in list)
                    component.Dispose();

            base.Dispose();
        }
        #endregion
    }
}
