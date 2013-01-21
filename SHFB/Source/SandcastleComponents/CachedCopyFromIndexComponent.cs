//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : CachedCopyFromIndexComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/11/2012
// Compiler: Microsoft Visual C#
//
// This component is now obsolete and will be removed in a future release
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.6.0.3  11/11/2007  EFW  Created the code
// 1.8.0.3  07/04/2009  EFW  Add parameter to Dispose() to match base class
// 1.9.3.3  11/19/2011  EFW  Opened cache files for shared read to allow for concurrent builds
// 1.9.7.0  12/22/2012  EFW  Moved index caching support into the base Sandcastle component
//===============================================================================================================

using System;
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
    /// This build component is obsolete and will be removed in a future release.
    /// </summary>
    public class CachedCopyFromIndexComponent : CopyFromIndexComponent
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembler">A reference to the build assembler.</param>
        /// <param name="configuration">The configuration information</param>
        /// <exception cref="ConfigurationErrorsException">This is thrown if
        /// an error is detected in the configuration.</exception>
        public CachedCopyFromIndexComponent(BuildAssembler assembler, XPathNavigator configuration) :
          base(assembler, configuration)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            base.WriteMessage(MessageLevel.Info, String.Format(CultureInfo.InvariantCulture,
                "\r\n    [{0}, version {1}]\r\n    Cached Copy From Index Component.  {2}\r\n" +
                "    http://SHFB.CodePlex.com", fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright));

            base.WriteMessage(MessageLevel.Error, "\r\nThe CachedCopyFromIndexComponent is obsolete and must " +
                "be removed from your project's Component Configurations.\r\nIndex caching support is built " +
                "into base Sandcastle component now.\r\nSee the Sandcastle Help File Builder v1.9.7.0 release " +
                "notes for more information.");
        }
        #endregion

/* TODO: Move cache usage reporting to the base component's dispose method
        /// <summary>
        /// This is used in the debug build to get an idea of how many files
        /// were kept loaded in the cache.
        /// </summary>
        /// <param name="disposing">Pass true to dispose of the managed and unmanaged resources or false to just
        /// dispose of the unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                Object cacheData, cache;
                Type type;
                FieldInfo field;

                cacheData = BuildComponent.Data[cacheId];
                type = cacheData.GetType();
                field = type.GetField("cache", BindingFlags.NonPublic | BindingFlags.Instance);
                cache = field.GetValue(cacheData);

                type = cache.GetType();
                field = type.GetField("count", BindingFlags.NonPublic | BindingFlags.Instance);

                this.WriteMessage(MessageLevel.Info, String.Format(CultureInfo.InvariantCulture,
                    "Used \"{0}\" cache entries: {1}", cacheId, field.GetValue(cache).ToString()));
            }

            base.Dispose(disposing);
        }*/
        #region Apply the component
        //=====================================================================

        /// <summary>
        /// This component is obsolete and will do nothing.
        /// </summary>
        /// <param name="document">The XML document with which to work.</param>
        /// <param name="key">The key (member name) of the item being documented.</param>
        public override void Apply(XmlDocument document, string key)
        {
        }
        #endregion
    }
}
