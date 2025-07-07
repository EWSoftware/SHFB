//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ComponentConfigurationDictionary.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2006-2025, Eric Woodruff, All rights reserved
//
// This file contains a dictionary class used to hold the configurations for third party build components such
// as the Code Block Component.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/24/2006  EFW  Created the code
// 11/01/2007  EFW  Reworked to support better handling of components
// 07/01/2008  EFW  Reworked to support MSBuild project format
// 04/07/2011  EFW  Made the from/to XML members public so that it can be used from the VSPackage
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Sandcastle.Core.BuildAssembler.BuildComponent
{
    /// <summary>
    /// This dictionary class is used to hold the third party build component configuration properties for a
    /// project such as the Code Block Component.
    /// </summary>
    public class ComponentConfigurationDictionary : Dictionary<string, BuildComponentConfiguration>
    {
        #region Read/write component configuration items from/to XML
        //=====================================================================

        /// <summary>
        /// This is used to load existing component configuration items from the project file
        /// </summary>
        /// <param name="components">The component items</param>
        /// <remarks>The information is stored as an XML fragment</remarks>
        public void FromXml(string components)
        {
            string id, config;
            bool enabled;

            using var xr = new XmlTextReader(components, XmlNodeType.Element,
              new XmlParserContext(null, null, null, XmlSpace.Default));
            
            xr.Namespaces = false;
            xr.MoveToContent();

            while(!xr.EOF)
            {
                if(xr.NodeType == XmlNodeType.Element && xr.Name == "ComponentConfig")
                {
                    id = xr.GetAttribute("id");
                    enabled = Convert.ToBoolean(xr.GetAttribute("enabled"), CultureInfo.InvariantCulture);

                    xr.ReadToDescendant("component");
                    config = xr.ReadOuterXml();

                    this.Add(id, new BuildComponentConfiguration(enabled, config));
                }

                xr.Read();
            }
        }

        /// <summary>
        /// This is used to write the component configuration info to an XML fragment ready for storing in the
        /// project file.
        /// </summary>
        /// <returns>The XML fragment containing the component configuration info.</returns>
        public string ToXml()
        {
            BuildComponentConfiguration config;

            using var ms = new MemoryStream(10240);
            using var xw = new XmlTextWriter(ms, new UTF8Encoding(false));
            
            xw.Formatting = Formatting.Indented;

            foreach(string key in this.Keys)
            {
                config = this[key];

                xw.WriteStartElement("ComponentConfig");
                xw.WriteAttributeString("id", key);
                xw.WriteAttributeString("enabled", config.Enabled.ToString());
                xw.WriteRaw(config.Configuration);
                xw.WriteEndElement();
            }

            xw.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }
        #endregion

        #region Add a new component configuration item
        //=====================================================================

        /// <summary>
        /// Add a new item to the dictionary
        /// </summary>
        /// <param name="id">The component ID</param>
        /// <param name="enabled">True for enabled, false for disabled</param>
        /// <param name="config">The component configuration</param>
        /// <returns>The <see cref="BuildComponentConfiguration" /> added to the project.  If the ID already
        /// exists in the collection, the existing item is returned.</returns>
        public BuildComponentConfiguration Add(string id, bool enabled, string config)
        {
            if(!this.TryGetValue(id, out BuildComponentConfiguration item))
            {
                if(String.IsNullOrWhiteSpace(config))
                    config = String.Format(CultureInfo.InvariantCulture, "<component id=\"{0}\" />", id);

                item = new BuildComponentConfiguration(enabled, config);
                this.Add(id, item);
            }

            return item;
        }
        #endregion
    }
}
