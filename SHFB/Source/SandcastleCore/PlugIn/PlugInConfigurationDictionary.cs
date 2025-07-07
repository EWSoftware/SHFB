//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : PlugInConfigurationDictionary.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2007-2025, Eric Woodruff, All rights reserved
//
// This file contains a dictionary class used to hold the configurations for third party build process plug-ins
// such as the AjaxDoc plug-in.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/10/2007  EFW  Created the code
// 07/01/2008  EFW  Reworked to support MSBuild project format
// 04/07/2011  EFW  Made the from/to XML members public so that it can be used from the VSPackage
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Sandcastle.Core.PlugIn
{
    /// <summary>
    /// This dictionary class is used to hold the configurations for third party build process plug-ins such as
    /// the Additional Content Only plug-in.
    /// </summary>
    public class PlugInConfigurationDictionary : Dictionary<string, PlugInConfiguration>
    {
        #region Read/write plug-in configuration items from/to XML
        //=====================================================================

        /// <summary>
        /// This is used to load existing plug-in configuration items from the project file
        /// </summary>
        /// <param name="plugIns">The plug-in items</param>
        /// <remarks>The information is stored as an XML fragment</remarks>
        public void FromXml(string plugIns)
        {
            string id, config;
            bool enabled;

            using var xr = new XmlTextReader(plugIns, XmlNodeType.Element,
              new XmlParserContext(null, null, null, XmlSpace.Default));
            
            xr.Namespaces = false;
            xr.MoveToContent();

            while(!xr.EOF)
            {
                if(xr.NodeType == XmlNodeType.Element && xr.Name == "PlugInConfig")
                {
                    id = xr.GetAttribute("id");
                    enabled = Convert.ToBoolean(xr.GetAttribute("enabled"), CultureInfo.InvariantCulture);

                    xr.ReadToDescendant("configuration");
                    config = xr.ReadOuterXml();

                    this.Add(id, new PlugInConfiguration(enabled, config));
                }

                xr.Read();
            }
        }

        /// <summary>
        /// This is used to write the plug-in configuration info to an XML fragment ready for storing in the
        /// project file.
        /// </summary>
        /// <returns>The XML fragment containing the plug-in configuration info.</returns>
        public string ToXml()
        {
            PlugInConfiguration config;

            using var ms = new MemoryStream(10240);
            using var xw = new XmlTextWriter(ms, new UTF8Encoding(false));
            
            xw.Formatting = Formatting.Indented;

            foreach(string key in this.Keys)
            {
                config = this[key];

                xw.WriteStartElement("PlugInConfig");
                xw.WriteAttributeString("id", key);
                xw.WriteAttributeString("enabled", config.Enabled.ToString());
                xw.WriteRaw(config.Configuration);
                xw.WriteEndElement();
            }

            xw.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }
        #endregion

        #region Add a new plug-in configuration item
        //=====================================================================

        /// <summary>
        /// Add a new item to the dictionary
        /// </summary>
        /// <param name="id">The plug-in ID</param>
        /// <param name="enabled">True for enabled, false for disabled</param>
        /// <param name="config">The plug-in configuration</param>
        /// <returns>The <see cref="PlugInConfiguration" /> added to the project.  If the ID already exists in
        /// the collection, the existing item is returned.</returns>
        public PlugInConfiguration Add(string id, bool enabled, string config)
        {
            if(!this.TryGetValue(id, out PlugInConfiguration item))
            {
                item = new PlugInConfiguration(enabled, config);
                this.Add(id, item);
            }

            return item;
        }
        #endregion
    }
}
