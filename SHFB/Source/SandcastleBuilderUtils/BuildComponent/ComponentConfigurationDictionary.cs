//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ComponentConfigurationDictionary.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/24/2014
// Note    : Copyright 2006-2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a dictionary class used to hold the configurations for third party build components such
// as the Code Block Component.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.3.3.0  11/24/2006  EFW  Created the code
// 1.6.0.2  11/01/2007  EFW  Reworked to support better handling of components
// 1.8.0.0  07/01/2008  EFW  Reworked to support MSBuild project format
// 1.9.3.0  04/07/2011  EFW  Made the from/to XML members public so that it can be used from the VSPackage
//===============================================================================================================

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace SandcastleBuilder.Utils.BuildComponent
{
    /// <summary>
    /// This dictionary class is used to hold the third party build component configuration properties for a
    /// project such as the Code Block Component.
    /// </summary>
    public class ComponentConfigurationDictionary : Dictionary<string, BuildComponentConfiguration>
    {
        #region Private data members
        //=====================================================================

        private SandcastleProject projectFile;
        private bool isDirty;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the dirty state of the collection
        /// </summary>
        public bool IsDirty
        {
            get
            {
                return isDirty || this.Values.Any(c => c.IsDirty);
            }
            set
            {
                foreach(var bc in this.Values)
                    bc.IsDirty = value;

                isDirty = value;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="project">The project that owns the collection</param>
        public ComponentConfigurationDictionary(SandcastleProject project)
        {
            projectFile = project;
        }
        #endregion

        #region Read/write component configuration items from/to XML
        //=====================================================================

        /// <summary>
        /// This is used to load existing component configuration items from the project file
        /// </summary>
        /// <param name="components">The component items</param>
        /// <remarks>The information is stored as an XML fragment</remarks>
        public void FromXml(string components)
        {
            XmlTextReader xr = null;
            string id, config;
            bool enabled;

            try
            {
                xr = new XmlTextReader(components, XmlNodeType.Element,
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

                        this.Add(id, new BuildComponentConfiguration(enabled, config, projectFile));
                    }

                    xr.Read();
                }
            }
            finally
            {
                if(xr != null)
                    xr.Close();

                isDirty = false;
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

            using(var ms = new MemoryStream(10240))
            {
                using(var xw = new XmlTextWriter(ms, new UTF8Encoding(false)))
                {
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
            }
        }
        #endregion

        #region Add a new component configuration item
        //=====================================================================

        /// <summary>
        /// Add a new item to the dictionary
        /// </summary>
        /// <param name="id">The component ID</param>
        /// <param name="enabled">True for enabled, false disabled</param>
        /// <param name="config">The component configuration</param>
        /// <returns>The <see cref="BuildComponentConfiguration" /> added to the project.  If the ID already
        /// exists in the collection, the existing item is returned.</returns>
        /// <remarks>The <see cref="BuildComponentConfiguration" /> constructor is internal so that we control
        /// creation of the items and can associate them with the project.</remarks>
        public BuildComponentConfiguration Add(string id, bool enabled, string config)
        {
            BuildComponentConfiguration item;

            if(!this.TryGetValue(id, out item))
            {
                if(String.IsNullOrWhiteSpace(config))
                    config = String.Format(CultureInfo.InvariantCulture, "<component id=\"{0}\" />", id);

                item = new BuildComponentConfiguration(enabled, config, projectFile);
                base.Add(id, item);
            }

            return item;
        }
        #endregion
    }
}
