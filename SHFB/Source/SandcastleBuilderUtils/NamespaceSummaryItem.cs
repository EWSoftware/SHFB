//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : NamespaceSummaryItem.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/14/2021
// Note    : Copyright 2006-2021, Eric Woodruff, All rights reserved
//
// This file contains a class representing a namespace summary item that can be used to add comments to a
// namespace in the help file or exclude it completely from the help file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/02/2006  EFW  Created the code
// 06/30/2008  EFW  Rewrote to support the MSBuild project format
// 11/30/2013  EFW  Merged changes from Stazzz to support namespace grouping
//===============================================================================================================

// Ignore Spelling: Stazzz

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This represents a a namespace summary item that can be used to add comments to a namespace in the help
    /// file or exclude it completely from the help file.
    /// </summary>
    public class NamespaceSummaryItem : INotifyPropertyChanged
    {
        #region Private data members
        //=====================================================================

        private readonly string name;
        private string summary;
        private bool isDocumented;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get the namespace name
        /// </summary>
        public string Name => (name.Length == 0) ? "(global)" : name;

        /// <summary>
        /// This read-only property is used to check whether or not this is a namespace group
        /// </summary>
        public Boolean IsGroup { get; }

        /// <summary>
        /// This is used to get or set whether or not the namespace is included in the help file
        /// </summary>
        public bool IsDocumented
        {
            get => isDocumented;
            set
            {
                isDocumented = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// This is used to get or set the namespace summary comments
        /// </summary>
        public string Summary
        {
            get => summary;
            set
            {
                summary = (value ?? String.Empty).Trim();
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// This read-only property can be used to determine if the namespace summary item was changed
        /// </summary>
        public bool IsDirty { get; private set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemName">The namespace's name</param>
        /// <param name="isGroup">This indicates whether or not the namespace is a group namespace</param>
        /// <param name="isDocumented">This indicates whether or not the namespace is to be documented</param>
        /// <param name="summaryText">The summary text</param>
        public NamespaceSummaryItem(string itemName, bool isGroup, bool isDocumented, string summaryText)
        {
            this.name = (itemName ?? String.Empty);
            this.IsGroup = isGroup;
            this.IsDocumented = isDocumented;
            this.Summary = summaryText;
            this.IsDirty = false;
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// Return the namespace name as the string representation of the object
        /// </summary>
        /// <returns>The namespace name</returns>
        public override string ToString()
        {
            return this.Name;
        }
        #endregion

        #region INotifyPropertyChanged implementation
        //=====================================================================

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This is used to raise the <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="caller">The name of the caller used as the property name</param>
        private void OnPropertyChanged([CallerMemberName]string caller = null)
        {
            this.IsDirty = true;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }
        #endregion
    }
}
