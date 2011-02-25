//=============================================================================
// System  : Sandcastle Help File Builder Components
// File    : CachedCopyFromIndexComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/22/2010
// Compiler: Microsoft Visual C#
//
// This file contains a build component that is derived from
// CopyFromIndexComponent.  The main difference is that this one loads the
// index document cache from a serialized binary file rather than loading it
// from the XML reflection data files.  This can significantly decrease the
// amount of time needed to instantiate the component.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.3  11/11/2007  EFW  Created the code
// 1.8.0.3  07/04/2009  EFW  Add parameter to Dispose() to match base class
//=============================================================================

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This is a derived <b>CopyFromIndexComponent</b> class that loads the
    /// index document cache from a serialized binary file rather than loading
    /// it from the XML reflection data files.  This can significantly decrease
    /// the amount of time needed to instantiate the component.
    /// </summary>
    /// <example>
    /// <code lang="xml" title="Cached Reflection Index Data Example">
    /// &lt;!-- Cached Reflection Index Data component.  This should replace
    ///      the first instance of the CopyFromIndexComponent. --&gt;
    /// &lt;component id="Cached Reflection Index Data"
    ///   type="SandcastleBuilder.Components.CachedCopyFromIndexComponent"
    ///   assembly="C:\SandcastleBuilder\SandcastleBuilder.Components.dll"&gt;
    ///     &lt;index name="reflection" value="/reflection/apis/api"
    ///        key="@id" cache="10"&gt;
    ///         &lt;cache base="C:\Program Files\Sandcastle\Data\Reflection"
    ///             recurse="true" files="*.xml"
    ///             cacheFile="C:\SandcastleBuilder\Cache\Reflection.cache"/&gt;
    ///         &lt;data files="reflection.xml" /&gt;
    ///     &lt;/index&gt;
    ///     &lt;copy name="reflection" source="*" target="/document/reference" /&gt;
    /// &lt;/component&gt;
    /// </code>
    ///
    /// <code lang="xml" title="Cached Framework Comments Index Data Example">
    /// &lt;!-- Cached Framework Comments Index Data component.  This should
    ///      replace the third instance of the CopyFromIndexComponent. --&gt;
    /// &lt;component id="Cached Framework Comments Index Data"
    ///   type="SandcastleBuilder.Components.CachedCopyFromIndexComponent"
    ///   assembly="C:\SandcastleBuilder\SandcastleBuilder.Components.dll"&gt;
    ///     &lt;index name="comments" value="/doc/members/member"
    ///       key="@name" cache="100"&gt;
    ///       &lt;cache files="%SystemRoot%\Microsoft.NET\Framework\v2.0.50727\en\*.xml"
    ///         cacheFile="C:\SandcastleBuilder\Cache\en_2.0.50727.cache" /&gt;
    ///       &lt;data files="ExtraComments.xml" /&gt;
    ///       &lt;data files="TestDoc.xml" /&gt;
    ///       &lt;data files="_InheritedDocs_.xml" /&gt;
    ///     &lt;/index&gt;
    ///     &lt;copy name="comments" source="*" target="/document/comments" /&gt;
    /// &lt;/component&gt;
    /// </code>
    /// </example>
    public class CachedCopyFromIndexComponent : CopyFromIndexComponent
    {
        #region Private data members
        //=====================================================================

        private string cacheId;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembler">A reference to the build assembler.</param>
        /// <param name="configuration">The configuration information</param>
        /// <exception cref="ConfigurationErrorsException">This is thrown if
        /// an error is detected in the configuration.</exception>
        public CachedCopyFromIndexComponent(BuildAssembler assembler,
          XPathNavigator configuration) : base(assembler, configuration)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            base.WriteMessage(MessageLevel.Info, String.Format(
                CultureInfo.InvariantCulture,
                "\r\n    [{0}, version {1}]\r\n    Cached Copy From Index " +
                " Component.  {2}\r\n    http://SHFB.CodePlex.com",
                fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright));

            // For each index, search for cached entries to load
            foreach(XPathNavigator index in configuration.Select("index"))
            {
                cacheId = index.GetAttribute("name", String.Empty);

                foreach(XPathNavigator cache in index.Select("cache"))
                    this.LoadCache(index, cacheId, cache);
            }
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Load a cached index
        /// </summary>
        /// <param name="index">The parent index node</param>
        /// <param name="name">The name of the index</param>
        /// <param name="cache">The cache settings</param>
        private void LoadCache(XPathNavigator index, string name,
          XPathNavigator cache)
        {
            XPathDocument xdoc;
            XPathNavigator nav;
            CopyFromIndexComponent copyComp = null;
            BinaryFormatter bf = new BinaryFormatter();
            Dictionary<string, string> cachedIndex, actualIndex;
            FieldInfo field;
            Object cacheData;
            Type type;
            FileStream fs = null;
            string parent, config, path, cacheFile, tempName;

            cacheFile = cache.GetAttribute("cacheFile", String.Empty);

            if(String.IsNullOrEmpty(cacheFile))
                throw new ConfigurationErrorsException("You must specify " +
                    "a cacheFile value on the cache element.");

            // Create the folder if it doesn't exist
            cacheFile = Path.GetFullPath(Environment.ExpandEnvironmentVariables(
                cacheFile));
            path = Path.GetDirectoryName(cacheFile);

            if(!Directory.Exists(path))
                Directory.CreateDirectory(path);

            try
            {
                // If it doesn't exist, create it on first use
                if(!File.Exists(cacheFile))
                {
                    base.WriteMessage(MessageLevel.Warn, cacheFile +
                        " does not exist and is being created");

                    parent = index.OuterXml;
                    config = cache.OuterXml.Replace("<cache ", "<data ");
                    tempName = "@" + name;

                    parent = parent.Substring(0, parent.IndexOf('>') + 1).Replace(
                        "\"" + name + "\"", "\"" + tempName + "\"");
                    config = String.Format(CultureInfo.InvariantCulture,
                        "<component>\r\n{0}\r\n{1}\r\n</index>\r\n</component>",
                        parent, config);

                    // Create a second CopyFromIndex component and pass it
                    // just enough information to create the index.
                    xdoc = new XPathDocument(new StringReader(config));
                    nav = xdoc.CreateNavigator().SelectSingleNode("component");
                    copyComp = new CopyFromIndexComponent(this.BuildAssembler,
                        nav);

                    // Get the data from the temporary index
                    cacheData = BuildComponent.Data[tempName];
                    type = cacheData.GetType();
                    field = type.GetField("index", BindingFlags.NonPublic |
                        BindingFlags.Instance);
                    cachedIndex = (Dictionary<string, string>)field.GetValue(
                        cacheData);

                    // Save the cached data for use in subsequent builds
                    fs = new FileStream(cacheFile, FileMode.Create);
                    bf.Serialize(fs, cachedIndex);

                    BuildComponent.Data.Remove(tempName);
                }
                else
                {
                    // Load the existing cached index
                    fs = new FileStream(cacheFile, FileMode.Open);
                    cachedIndex = (Dictionary<string, string>)bf.Deserialize(fs);
                }
            }
            finally
            {
                if(fs != null)
                    fs.Close();

                if(copyComp != null)
                    copyComp.Dispose();
            }

            // Get the dictionary containing the user's index info
            cacheData = BuildComponent.Data[name];
            type = cacheData.GetType();
            field = type.GetField("index", BindingFlags.NonPublic |
                BindingFlags.Instance);
            actualIndex = (Dictionary<string, string>)field.GetValue(cacheData);

            // Add the cached info from the framework files.  If there's a
            // duplicate, the later copy will take precedence.
            foreach(string key in cachedIndex.Keys)
                if(!actualIndex.ContainsKey(key))
                    actualIndex.Add(key, cachedIndex[key]);
                else
                {
                    base.WriteMessage(MessageLevel.Warn, String.Format(
                        CultureInfo.InvariantCulture, "Key '{0}' in " +
                        "this framework cache matches an item from a prior " +
                        "cache with the same key.  The item from this cache " +
                        "will take precedence.", key));
                    actualIndex[key] = cachedIndex[key];
                }

            base.WriteMessage(MessageLevel.Info, String.Format(
                CultureInfo.InvariantCulture, "Loaded {0} items from the " +
                "cache file '{1}'", cachedIndex.Count, cacheFile));
        }
        #endregion

        #region Dispose method
        //=====================================================================

        /// <summary>
        /// This is used in the debug build to get an idea of how many files
        /// were kept loaded in the cache.
        /// </summary>
        public override void Dispose()
        {
            Object cacheData, cache;
            Type type;
            FieldInfo field;

            cacheData = BuildComponent.Data[cacheId];
            type = cacheData.GetType();
            field = type.GetField("cache", BindingFlags.NonPublic |
                BindingFlags.Instance);
            cache = field.GetValue(cacheData);

            type = cache.GetType();
            field = type.GetField("count", BindingFlags.NonPublic |
                BindingFlags.Instance);

            this.WriteMessage(MessageLevel.Info, String.Format(
                CultureInfo.InvariantCulture, "Used \"{0}\" cache entries: {1}",
                cacheId, field.GetValue(cache).ToString()));

            base.Dispose();
        }
        #endregion
    }
}
