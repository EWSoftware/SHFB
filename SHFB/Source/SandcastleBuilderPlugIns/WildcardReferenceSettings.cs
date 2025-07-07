//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : WildcardReferenceSettings.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2011-2025, Eric Woodruff, All rights reserved
//
// This file contains a class representing wildcard reference settings for the Wildcard Reference plug-in
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/17/2010  EFW  Created the code
//===============================================================================================================

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

using Sandcastle.Core;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This represents wildcard reference settings for the
    /// <see cref="WildcardReferencesPlugIn"/>.
    /// </summary>
    public class WildcardReferenceSettings : INotifyPropertyChanged
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the path to scan for references.
        /// </summary>
        public FolderPath ReferencePath
        {
            get => field;
            set
            {
                if(field != value)
                {
                    if(field != null)
                        field.PersistablePathChanged -= this.referencePath_PersistablePathChanged;

                    if(value == null)
                        value = new FolderPath(field.BasePathProvider);

                    field = value;
                    field.PersistablePathChanged += this.referencePath_PersistablePathChanged;

                    this.Validate();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set the wildcard to use with the folder
        /// </summary>
        public string Wildcard
        {
            get => field;
            set
            {
                if(field != value)
                {
                    if(String.IsNullOrEmpty(value))
                        value = "*.dll";

                    field = value;

                    this.Validate();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set whether or not to scan sub-folders recursively
        /// </summary>
        public bool Recursive
        {
            get => field;
            set
            {
                if(field != value)
                {
                    field = value;

                    this.Validate();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This read-only property returns an error message describing any issues with the settings
        /// </summary>
        public string ErrorMessage
        {
            get => field;
            private set
            {
                field = value;

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// This returns a description of the entry suitable for display in a bound list control
        /// </summary>
        public string ReferenceDescription
        {
            get => field;
            set
            {
                field = value;

                this.OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public WildcardReferenceSettings()
        {
            this.ReferencePath = new FolderPath(null);
            this.Wildcard = "*.dll";
            this.Recursive = true;
        }
        #endregion

        #region INotifyPropertyChanged implementation
        //=====================================================================

        /// <summary>
        /// The property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This raises the <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="propertyName">The property name that changed</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Update the display description when the reference path changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void referencePath_PersistablePathChanged(object sender, EventArgs e)
        {
            this.Validate();
        }

        /// <summary>
        /// This is used to validate the settings
        /// </summary>
        private void Validate()
        {
            if(this.ReferencePath.Path.Length == 0)
                this.ErrorMessage = "A reference path is required";
            else
                this.ErrorMessage = null;

            this.ReferenceDescription = String.Format(CultureInfo.CurrentCulture, "{0}{1}  ({2})",
                this.ReferencePath.PersistablePath, this.Wildcard, (this.Recursive) ? "Recursive" : "This folder only");
        }
        #endregion

        #region Convert from/to XML
        //=====================================================================

        /// <summary>
        /// Create a wildcard reference settings instance from an XElement containing the settings
        /// </summary>
        /// <param name="pathProvider">The base path provider object</param>
        /// <param name="element">The XElement from which to obtain the settings</param>
        /// <returns>A <see cref="WildcardReferenceSettings"/> object containing the settings from the XElement</returns>
        /// <remarks>It should contain an element called <c>reference</c> with three attributes (<c>path</c>,
        /// <c>wildcard</c>, and <c>recurse</c>).
        /// </remarks>
        public static WildcardReferenceSettings FromXml(IBasePathProvider pathProvider, XElement element)
        {
            WildcardReferenceSettings wr = new();

            if(element != null)
            {
                string path = element.Attribute("path").Value.Trim();

                wr.ReferencePath = new FolderPath(path, Path.IsPathRooted(path), pathProvider);
                wr.Wildcard = element.Attribute("wildcard").Value;
                wr.Recursive = (bool)element.Attribute("recurse");
            }

            return wr;
        }

        /// <summary>
        /// Store the wildcard reference settings as a node in the given XML element
        /// </summary>
        /// <returns>Returns the node to add</returns>
        /// <remarks>The reference link settings are stored in an element called <c>reference</c> with three
        /// attributes (<c>path</c>, <c>wildcard</c>, and <c>recurse</c>).</remarks>
        public XElement ToXml()
        {
            return new XElement("reference",
                new XAttribute("path", this.ReferencePath.PersistablePath),
                new XAttribute("wildcard", this.Wildcard),
                new XAttribute("recurse", this.Recursive));
        }
        #endregion
    }
}
