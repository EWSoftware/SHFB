//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : EntityReference.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/02/2025
// Note    : Copyright 2011-2025, Eric Woodruff, All rights reserved
//
// This file contains the entity reference class used by the Entity References control
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/04/2011  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SandcastleBuilder.WPF
{
    /// <summary>
    /// This defines an entity that is bound and displayed in the tree view
    /// </summary>
    internal class EntityReference : INotifyPropertyChanged
    {
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
            get => field;
            set
            {
                if(field != value)
                {
                    field = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to get or set whether or not the entity is expanded
        /// </summary>
        public bool IsExpanded
        {
            get => field && this.SubEntities.Count != 0;
            set
            {
                if(field != value)
                {
                    field = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// A list of sub-entities for this entity (only used for file entities)
        /// </summary>
        public IList<EntityReference> SubEntities { get; } = [];

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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to find the specified text within the reference's label text or ID value or any of its
        /// sub-entities.
        /// </summary>
        /// <param name="findText">The text to find</param>
        /// <returns>An enumerable list of all matches</returns>
        public IEnumerable<EntityReference> Find(string findText)
        {
            bool found = !String.IsNullOrWhiteSpace(this.Label) && this.Label.IndexOf(findText,
              StringComparison.CurrentCultureIgnoreCase) != -1;

            if(!found)
            {
                found = !String.IsNullOrWhiteSpace(this.Id) && this.Id.IndexOf(findText,
                    StringComparison.CurrentCultureIgnoreCase) != -1;
            }

            if(found)
                yield return this;

            foreach(var sub in this.SubEntities)
            {
                var subMatches = sub.Find(findText);

                if(subMatches.Any())
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
