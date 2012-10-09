//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : WildcardReferenceSettings.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/17/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing wildcard reference settings for the
// Wildcard Reference plug-in.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.2.0  01/17/2010  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Xml.Linq;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This represents wildcard reference settings for the
    /// <see cref="WildcardReferencesPlugIn"/>.
    /// </summary>
    [DefaultProperty("ReferencePath")]
    public class WildcardReferenceSettings
    {
        #region Private data members
        //=====================================================================

        private FolderPath referencePath;
        private string wildcard;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the path to scan for references.
        /// </summary>
        [Category("Reference"), Description("The path to scan for reference assemblies"),
          Editor(typeof(FolderPathObjectEditor), typeof(UITypeEditor)),
          RefreshProperties(RefreshProperties.All),
          FolderDialog("Select the reference file location to scan")]
        public FolderPath ReferencePath
        {
            get { return referencePath; }
            set
            {
                if(value == null || value.Path.Length == 0)
                    throw new BuilderException("WR0001", "The reference path cannot be blank");

                referencePath = value;
            }
        }

        /// <summary>
        /// This is used to get or set the wildcard to use with the folder
        /// </summary>
        [Category("Reference"), Description("The wildcard to use with the folder"),
          DefaultValue("*.dll")]
        public string Wildcard
        {
            get { return wildcard; }
            set
            {
                if(String.IsNullOrEmpty(value))
                    value = "*.dll";

                wildcard = value;
            }
        }

        /// <summary>
        /// This is used to get or set whether or not to scan sub-folders recursively
        /// </summary>
        [Category("Reference"), Description("True to scan sub-folders, false to only scan the given folder"),
          DefaultValue(true)]
        public bool Recursive { get; set; }

        /// <summary>
        /// This returns a description of the entry suitable for display in a
        /// bound list control.
        /// </summary>
        [Category("Reference Info"), Description("List description")]
        public string ListDescription
        {
            get
            {
                return String.Format(CultureInfo.CurrentCulture, "{0}{1}  ({2})", referencePath.PersistablePath,
                    this.Wildcard, (this.Recursive) ? "Recursive" : "This folder only");
            }
        }
        #endregion

        #region Designer methods
        //=====================================================================

        /// <summary>
        /// This is used to see if the <see cref="ReferencePath"/> property
        /// should be serialized.
        /// </summary>
        /// <returns>True to serialize it, false if it matches the default
        /// and should not be serialized.  This property cannot be reset
        /// as it should always have a value.</returns>
        private bool ShouldSerializeReferencePath()
        {
            return (this.ReferencePath.Path.Length != 0);
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public WildcardReferenceSettings()
        {
            this.Wildcard = "*.dll";
            this.Recursive = true;
        }
        #endregion

        #region Convert from/to XML
        //=====================================================================

        /// <summary>
        /// Create a wildcard reference settings instance from an XElement
        /// containing the settings.
        /// </summary>
        /// <param name="pathProvider">The base path provider object</param>
        /// <param name="element">The XElement from which to obtain the settings.</param>
        /// <returns>A <see cref="WildcardReferenceSettings"/> object containing the
        /// settings from the XElement.</returns>
        /// <remarks>It should contain an element called <c>reference</c>
        /// with three attributes (<c>path</c>, <c>wildcard</c>, and <c>recurse</c>).
        /// </remarks>
        public static WildcardReferenceSettings FromXPathNavigator(IBasePathProvider pathProvider,
          XElement element)
        {
            WildcardReferenceSettings wr = new WildcardReferenceSettings();

            if(element != null)
            {
                wr.ReferencePath = new FolderPath(element.Attribute("path").Value.Trim(), pathProvider);
                wr.Wildcard = element.Attribute("wildcard").Value;
                wr.Recursive = (bool)element.Attribute("recurse");
            }

            return wr;
        }

        /// <summary>
        /// Store the wildcard reference settings as a node in the given XML
        /// element.
        /// </summary>
        /// <returns>Returns the node to add.</returns>
        /// <remarks>The reference link settings are stored in an element
        /// called <c>reference</c> with three attributes (<c>path</c>,
        /// <c>wildcard</c>, and <c>recurse</c>).</remarks>
        public XElement ToXml()
        {
            return new XElement("reference",
                new XAttribute("path", referencePath.PersistablePath),
                new XAttribute("wildcard", wildcard),
                new XAttribute("recurse", this.Recursive));
        }
        #endregion
    }
}
