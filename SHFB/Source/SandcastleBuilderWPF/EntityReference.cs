//=============================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : EntityReference.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/29/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the entity reference class used by the Entity References
// control.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.3  12/04/2011  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SandcastleBuilder.WPF
{
    /// <summary>
    /// This defines an entity that is bound and displayed in the tree view
    /// </summary>
    internal class EntityReference : INotifyPropertyChanged
    {
        #region Private data members
        //=====================================================================

        private bool isExpanded, isSelected;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// The entity type
        /// </summary>
        public EntityType EntityType { get; set; }

        /// <summary>
        /// The ID value of the entity
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The label for the entity
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The tool tip for the entity
        /// </summary>
        public string ToolTip { get; set; }

        /// <summary>
        /// The entity object
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// This is used to get or set whether or not the entity is selected
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if(value != isSelected)
                {
                    isSelected = value;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        /// <summary>
        /// This is used to get or set whether or not the entity is expanded
        /// </summary>
        public bool IsExpanded
        {
            get { return isExpanded && this.SubEntities.Count != 0; }
            set
            {
                if(value != isExpanded)
                {
                    isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }
            }
        }

        /// <summary>
        /// A list of sub-entities for this entity (only used for file entities)
        /// </summary>
        public List<EntityReference> SubEntities { get; private set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public EntityReference()
        {
            this.SubEntities = new List<EntityReference>();
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
        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;

            if(handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to find the specified text within the reference's label text or ID value
        /// or any of its sub-entities.
        /// </summary>
        /// <param name="findText">The text to find</param>
        /// <returns>An enumerable list of all matches</returns>
        public IEnumerable<EntityReference> Find(string findText)
        {
            bool found = (!String.IsNullOrEmpty(this.Label) && this.Label.IndexOf(findText,
              StringComparison.CurrentCultureIgnoreCase) != -1);

            if(!found)
                found = (!String.IsNullOrEmpty(this.Id) && this.Id.IndexOf(findText,
                    StringComparison.CurrentCultureIgnoreCase) != -1);

            if(found)
                yield return this;

            foreach(var sub in this.SubEntities)
            {
                var subMatches = sub.Find(findText);

                if(subMatches.Count() != 0)
                {
                    // Make sure this node is expanded so that we can move to it in the tree view
                    this.IsExpanded = true;

                    foreach(var m in subMatches)
                        yield return m;
                }
            }
        }
        #endregion
    }
}
