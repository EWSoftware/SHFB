//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : PlugInConfigurationDictionary.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/10/2011
// Note    : Copyright 2007-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a dictionary class used to hold the configurations for
// third party build process plug-ins such as the AjaxDoc plug-in.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.2.0  09/10/2007  EFW  Created the code
// 1.8.0.0  07/01/2008  EFW  Reworked to support MSBuild project format
// 1.9.3.0  04/07/2011  EFW  Made the from/to XML members public so that it can
//                           be used from the VSPackage.
//=============================================================================

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Utils.PlugIn
{
    /// <summary>
    /// This dictionary class is used to hold the configurations for third
    /// party build process plug-ins such as the AjaxDoc plug-in.
    /// </summary>
    [TypeConverter(typeof(PlugInConfigurationDictionaryTypeConverter)),
      Editor(typeof(PlugInConfigurationEditor), typeof(UITypeEditor)),
      Serializable]
    public class PlugInConfigurationDictionary : Dictionary<string, PlugInConfiguration>
    {
        #region Private data members
        //=====================================================================

        private SandcastleProject projectFile;
        private bool isDirty;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This returns a reference to the owning project file
        /// </summary>
        public SandcastleProject ProjectFile
        {
            get { return projectFile; }
        }

        /// <summary>
        /// This is used to get or set the dirty state of the collection
        /// </summary>
        public bool IsDirty
        {
            get
            {
                foreach(PlugInConfiguration pc in this.Values)
                    if(pc.IsDirty)
                        return true;

                return isDirty;
            }
            set
            {
                foreach(PlugInConfiguration pc in this.Values)
                    pc.IsDirty = value;

                isDirty = value;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// This event is raised when the dictionary is modified
        /// </summary>
        public event ListChangedEventHandler DictionaryChanged;

        /// <summary>
        /// This raises the <see cref="DictionaryChanged"/> event.
        /// </summary>
        /// <param name="e">The event arguments</param>
        /// <remarks>The dictionary doesn't raise events automatically so
        /// this is raised manually as needed.</remarks>
        internal void OnDictionaryChanged(ListChangedEventArgs e)
        {
            var handler = DictionaryChanged;

            if(handler != null)
                handler(this, e);

            isDirty = true;
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="project">The project that owns the collection</param>
        public PlugInConfigurationDictionary(SandcastleProject project)
        {
            projectFile = project;
        }
        #endregion

        #region Read/write plug-in configuration items from/to XML
        //=====================================================================

        /// <summary>
        /// This is used to load existing plug-in configuration items from
        /// the project file.
        /// </summary>
        /// <param name="plugIns">The plug-in items</param>
        /// <remarks>The information is stored as an XML fragment</remarks>
        public void FromXml(string plugIns)
        {
            XmlTextReader xr = null;
            string id, config;
            bool enabled;

            try
            {
                xr = new XmlTextReader(plugIns, XmlNodeType.Element,
                    new XmlParserContext(null, null, null, XmlSpace.Default));
                xr.Namespaces = false;
                xr.MoveToContent();

                while(!xr.EOF)
                {
                    if(xr.NodeType == XmlNodeType.Element &&
                      xr.Name == "PlugInConfig")
                    {
                        id = xr.GetAttribute("id");
                        enabled = Convert.ToBoolean(xr.GetAttribute("enabled"), CultureInfo.InvariantCulture);

                        xr.ReadToDescendant("configuration");
                        config = xr.ReadOuterXml();

                        this.Add(id, new PlugInConfiguration(enabled, config, projectFile));
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
        /// This is used to write the plug-in configuration info to an XML
        /// fragment ready for storing in the project file.
        /// </summary>
        /// <returns>The XML fragment containing the plug-in configuration
        /// info.</returns>
        public string ToXml()
        {
            PlugInConfiguration config;
            MemoryStream ms = new MemoryStream(10240);
            XmlTextWriter xw = null;

            try
            {
                xw = new XmlTextWriter(ms, new UTF8Encoding(false));
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
            finally
            {
                if(xw != null)
                    xw.Close();

                ms.Dispose();
            }
        }
        #endregion

        #region Add a new plug-in configuration item
        //=====================================================================

        /// <summary>
        /// Add a new item to the dictionary
        /// </summary>
        /// <param name="id">The plug-in ID</param>
        /// <param name="enabled">True for enabled, false disabled</param>
        /// <param name="config">The plug-in configuration</param>
        /// <returns>The <see cref="PlugInConfiguration" /> added to the
        /// project.  If the ID already exists in the collection, the existing
        /// item is returned.</returns>
        /// <remarks>The <see cref="PlugInConfiguration" /> constructor is
        /// internal so that we control creation of the items and can
        /// associate them with the project.</remarks>
        public PlugInConfiguration Add(string id, bool enabled, string config)
        {
            PlugInConfiguration item;

            if(!this.TryGetValue(id, out item))
            {
                item = new PlugInConfiguration(enabled, config, projectFile);
                base.Add(id, item);
            }

            return item;
        }
        #endregion
    }
}
