//=============================================================================
// System  : Sandcastle Help File Builder Components
// File    : CachedResolveReferenceLinksComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/11/2012
// Compiler: Microsoft Visual C#
//
// This file contains a build component that is derived from
// ResolveReferenceLinksComponent2.  The main difference is that this one loads
// cached MSDN URLs from a serialized binary file rather than letting the
// base component invoke the web service to look them up.  This can
// significantly decrease the amount of time needed to perform a build.
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
// 1.9.3.3  11/19/2011  EFW  Opened cache files for shared read to allow for
//                           concurrent builds.
//=============================================================================

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.XPath;

using Microsoft.Ddue.Tools;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This is a derived <b>ResolveReferenceLinksComponent2</b> class that
    /// loads cached MSDN URLs from a serialized binary file rather than
    /// letting the base component invoke the web service to look them up.
    /// This can significantly decrease the amount of time needed to perform
    /// a build.
    /// </summary>
    /// <remarks>The cache is built cumulatively over time rather than having
    /// all 170,000+ resolved entries loaded, most of which would never be
    /// used.  If new URLs are added to the cache during a build, the cache is
    /// saved during disposal so that the new entries are used on subsequent
    /// builds.</remarks>
    /// <example>
    /// <code lang="xml" title="Example configuration">
    /// &lt;!-- Cached MSDN URL references component.  This should replace
    ///      the standard ResolveReferenceLinksComponent2 build component. --&gt;
    /// &lt;component type="SandcastleBuilder.Components.CachedResolveReferenceLinksComponent"
    ///   assembly="C:\SandcastleBuilder\SandcastleBuilder.Components.dll"
    ///   locale="en-us" linkTarget="_blank"&gt;
    ///     &lt;cache filename="C:\SandcastleBuilder\Cache\MsdnUrl.cache" /&gt;
    ///     &lt;targets base="C:\Program Files\Sandcastle\Data\Reflection"
    ///         recurse="true" files="*.xml" type="MSDN" /&gt;
    ///     &lt;targets files="reflection.xml" type="Local" /&gt;
    /// &lt;/component&gt;
    /// </code>
    /// </example>
    public class CachedResolveReferenceLinksComponent :
      ResolveReferenceLinksComponent2
    {
        #region Private data members
        //=====================================================================

        private int originalCacheCount;
        private string cacheFile;
        private Dictionary<string, string> msdnCache;
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
        public CachedResolveReferenceLinksComponent(BuildAssembler assembler,
          XPathNavigator configuration) : base(assembler, configuration)
        {
            BinaryFormatter bf = new BinaryFormatter();
            Dictionary<string, string> cachedUrls;
            TargetCollection targets;
            Target target;
            MsdnResolver resolver;
            Type type;
            FieldInfo field;
            XPathNavigator node;
            FileStream fs = null;
            string path, url;

            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            base.WriteMessage(MessageLevel.Info, String.Format(
                CultureInfo.InvariantCulture,
                "\r\n    [{0}, version {1}]\r\n    Cached Resolve Reference " +
                " Links 2 Component.  {2}\r\n    http://SHFB.CodePlex.com",
                fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright));

            // If a <cache> element is not specified, this will behave just
            // like the base class.
            node = configuration.SelectSingleNode("cache");

            if(node == null)
            {
                base.WriteMessage(MessageLevel.Warn, "No <cache> element was " +
                    "specified.  No MSDN URL caching will occur in this build.");
                return;
            }

            cacheFile = node.GetAttribute("filename", String.Empty);

            if(String.IsNullOrEmpty(cacheFile))
                throw new ConfigurationErrorsException("You must specify " +
                    "a filename value on the cache element.");

            // Create the folder if it doesn't exist
            cacheFile = Path.GetFullPath(Environment.ExpandEnvironmentVariables(
                cacheFile));
            path = Path.GetDirectoryName(cacheFile);

            if(!Directory.Exists(path))
                Directory.CreateDirectory(path);

            type = this.GetType().BaseType;
            field = type.GetField("msdn", BindingFlags.NonPublic |
                BindingFlags.Instance);
            resolver = (MsdnResolver)field.GetValue(this);

            // No need to load the cache if MSDN links are not used
            if(resolver != null)
            {
                type = resolver.GetType();
                field = type.GetField("cachedMsdnUrls",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                msdnCache = (Dictionary<string, string>)field.GetValue(resolver);

                // Load the cache if it exists
                if(!File.Exists(cacheFile))
                    base.WriteMessage(MessageLevel.Info, "The MSDN URL cache '" +
                        cacheFile + "' does not exist yet.  All IDs will be " +
                        "looked up in this build.");
                else
                    try
                    {
                        fs = new FileStream(cacheFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                        cachedUrls = (Dictionary<string, string>)bf.Deserialize(fs);

                        // Get the target collection for marking unknown URLs
                        // as "None" links.
                        type = this.GetType().BaseType;
                        field = type.GetField("targets",
                            BindingFlags.NonPublic | BindingFlags.Instance);
                        targets = (TargetCollection)field.GetValue(this);

                        type = typeof(Target);
                        field = type.GetField("type", BindingFlags.NonPublic |
                            BindingFlags.Instance);

                        foreach(string key in cachedUrls.Keys)
                        {
                            url = cachedUrls[key];
                            msdnCache.Add(key, url);

                            // For IDs with a null URL, mark the target as a
                            // "None" link so that it doesn't waste a lookup.
                            if(url == null)
                            {
                                target = targets[key];
                                if(target != null)
                                    field.SetValue(target, LinkType2.None);
                            }
                        }

                        base.WriteMessage(MessageLevel.Info, String.Format(
                            CultureInfo.InvariantCulture, "Loaded {0} cached " +
                            "MSDN URL entries", msdnCache.Count));
                    }
                    finally
                    {
                        if(fs != null)
                            fs.Close();
                    }

                originalCacheCount = msdnCache.Count;
            }
        }
        #endregion

        #region Dispose of the component
        //=====================================================================

        /// <summary>
        /// This is overriden to save the updated cache when disposed
        /// </summary>
        /// <param name="disposing">Pass true to dispose of the managed and unmanaged resources or false to just
        /// dispose of the unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if(disposing && msdnCache != null && msdnCache.Count != originalCacheCount)
            {
                base.WriteMessage(MessageLevel.Info, "MSDN URL cache updated.  Saving new information to " +
                    cacheFile);

                try
                {
                    using(FileStream fs = new FileStream(cacheFile, FileMode.Create))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(fs, msdnCache);

                        base.WriteMessage(MessageLevel.Info, String.Format(CultureInfo.InvariantCulture,
                            "New cache size: {0} entries", msdnCache.Count));
                    }
                }
                catch(IOException ex)
                {
                    // Most likely it couldn't access the file.  We'll issue a warning but will continue with
                    // the build.
                    base.WriteMessage(MessageLevel.Warn, "Unable to create cache file.  It will be created " +
                        "or updated on a subsequent build: " + ex.Message);
                }
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
