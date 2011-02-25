//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ResourceItem.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/06/2009
// Note    : Copyright 2009, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing a Sandcastle transformation resource
// item that can be used to insert a common item, value, or construct into
// generated topics.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.3  12/04/2009  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Xml;

namespace SandcastleBuilder.Utils.ConceptualContent
{
    /// <summary>
    /// This represents a Sandcastle transformation resource item that can be
    /// used to insert a common item, value, or construct into generated topics.
    /// </summary>
    [DefaultProperty("Id")]
    public class ResourceItem
    {
        #region Private data members
        //=====================================================================

        private string itemId;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the name of the file containing the
        /// resource item.
        /// </summary>
        public string SourceFile { get; set; }

        /// <summary>
        /// This read-only property is used to get the item ID
        /// </summary>
        [Category("ResourceItem"), Description("The resource item ID"),
          DefaultValue(null)]
        public string Id
        {
            get { return itemId; }
        }

        /// <summary>
        /// This is used to get or set the item value
        /// </summary>
        /// <value>The value can contain help file builder replacement tags.
        /// These will be replaced at build time with the appropriate project
        /// value.</value>
        [Category("ResourceItem"), Description("The value of the item"),
          DefaultValue(null),
          Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string Value { get; set; }

        /// <summary>
        /// This is used to get or set whether or not the item has been edited
        /// and thus overrides a default item with the same ID.
        /// </summary>
        [Browsable(false)]
        public bool IsOverridden { get; set; }
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="file">The file containing the resource item</param>
        /// <param name="id">The item ID</param>
        /// <param name="value">The item value</param>
        /// <param name="isOverride">True if this is an override, false if not</param>
        public ResourceItem(string file, string id, string value, bool isOverride)
        {
            itemId = id;
            this.SourceFile = file;
            this.Value = value;
            this.IsOverridden = isOverride;
        }
        #endregion
    }
}
