//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ResourceItem.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/16/2015
// Note    : Copyright 2009-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing a Sandcastle transformation resource item that can be used to insert
// a common item, value, or construct into generated topics.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/04/2009  EFW  Created the code
// 12/22/2011  EFW  Updated for use with the new resource item file editor
//===============================================================================================================

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SandcastleBuilder.Utils.ConceptualContent
{
    /// <summary>
    /// This represents a Sandcastle transformation resource item that can be used to insert a common item,
    /// value, or construct into generated topics.
    /// </summary>
    public class ResourceItem : INotifyPropertyChanged
    {
        #region Private data members
        //=====================================================================

        private string sourceFile, itemId, itemValue;
        private bool isSelected, isOverridden;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the name of the file containing the resource item
        /// </summary>
        public string SourceFile
        {
            get { return sourceFile; }
            set
            {
                if(value != sourceFile)
                {
                    sourceFile = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This read-only property is used to get the item ID
        /// </summary>
        public string Id
        {
            get { return itemId; }
        }

        /// <summary>
        /// This is used to get or set the item value
        /// </summary>
        /// <value>The value can contain help file builder replacement tags.  These will be replaced at build
        /// time with the appropriate project value.</value>
        public string Value
        {
            get { return itemValue; }
            set
            {
                if(value != itemValue)
                {
                    itemValue = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set whether or not the entity is selected
        /// </summary>
        /// <remarks>Used by the editor for binding in the list box</remarks>
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if(value != isSelected)
                {
                    isSelected = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set whether or not the item has been edited and thus overrides a default item
        /// with the same ID.
        /// </summary>
        public bool IsOverridden
        {
            get { return isOverridden; }
            set
            {
                if(value != isOverridden)
                {
                    isOverridden = value;
                    this.OnPropertyChanged();
                }
            }
        }
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

        #region INotifyPropertyChanged Members
        //=====================================================================

        /// <summary>
        /// The property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This raises the <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="propertyName">The property name that changed</param>
        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            var handler = PropertyChanged;

            if(handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
