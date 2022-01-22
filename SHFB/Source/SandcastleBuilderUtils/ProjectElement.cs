//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ProjectElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/14/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains a wrapper class for build items in the project
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/23/2008  EFW  Created the code
// 07/09/2010  EFW  Updated for use with .NET 4.0 and MSBuild 4.0.
//===============================================================================================================

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Microsoft.Build.Evaluation;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This class is a wrapper for build items in the project.
    /// </summary>
    public class ProjectElement : INotifyPropertyChanged
    {
        #region Private data members
        //=====================================================================

        private SandcastleProject projectFile;
        private ProjectItem item;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to set or get the item type (a.k.a BuildAction)
        /// </summary>
        [Browsable(false)]
        protected internal string ItemType
        {
            get => item.ItemType;
            set
            {
                if(item.ItemType != value)
                {
                    item.ItemType = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This is used to set or get the filename (Include attribute)
        /// </summary>
        [Browsable(false)]
        public string Include
        {
            get => item.UnevaluatedInclude;
            set
            {
                if(item.UnevaluatedInclude != value)
                {
                    if(String.IsNullOrEmpty(value) || value.IndexOfAny(new char[] { '*', '?' }) != -1)
                        throw new ArgumentException("The filename cannot be blank and cannot contain wildcards (* or ?)");

                    // Folder items must end in a backslash
                    if(item.ItemType == BuildAction.Folder.ToString() && value[value.Length - 1] != '\\')
                        value += @"\";

                    item.UnevaluatedInclude = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// This read-only property is used to get the containing project
        /// </summary>
        [Browsable(false)]
        public SandcastleProject Project => projectFile;

        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// This constructor is used to wrap an existing project item
        /// </summary>
        /// <param name="project">The project that owns the item</param>
        /// <param name="existingItem">The existing item</param>
        /// <overloads>There are two overloads for the constructor</overloads>
        protected ProjectElement(SandcastleProject project, ProjectItem existingItem)
        {
            projectFile = project ?? throw new ArgumentNullException(nameof(project));
            item = existingItem ?? throw new ArgumentNullException(nameof(existingItem));
        }

        /// <summary>
        /// This constructor is used to create a new build item and add it to the project
        /// </summary>
        /// <param name="project">The project that will own the item</param>
        /// <param name="itemType">The type of build item to create</param>
        /// <param name="itemPath">The path to the item.  This can be relative or absolute and may contain
        /// variable references.</param>
        protected ProjectElement(SandcastleProject project, string itemType, string itemPath)
        {
            if(String.IsNullOrEmpty(itemPath))
                throw new ArgumentException("Cannot be null or empty", nameof(itemPath));

            if(String.IsNullOrEmpty(itemType))
                throw new ArgumentException("Cannot be null or empty", nameof(itemType));

            projectFile = project ?? throw new ArgumentNullException(nameof(project));

            if(itemType == BuildAction.Folder.ToString() && itemPath[itemPath.Length - 1] != '\\')
                itemPath += @"\";

            item = project.MSBuildProject.AddItem(itemType, itemPath)[0];
            projectFile.MSBuildProject.ReevaluateIfNecessary();
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Project item methods
        //=====================================================================

        /// <summary>
        /// See if the named metadata item exists
        /// </summary>
        /// <param name="name">The metadata name for which to check</param>
        /// <returns>True if present, false if not</returns>
        public bool HasMetadata(string name)
        {
            // Build Action is the name, not metadata.  Include is an attribute, not metadata.
            if(String.Equals(name, BuildItemMetadata.BuildAction, StringComparison.OrdinalIgnoreCase) ||
              String.Equals(name, BuildItemMetadata.IncludePath, StringComparison.OrdinalIgnoreCase))
                return true;

            return item.HasMetadata(name);
        }

        /// <summary>
        /// Get a metadata value from a project element
        /// </summary>
        /// <param name="name">The name of the metadata element to get</param>
        /// <returns>The value of the metadata element</returns>
        public string GetMetadata(string name)
        {
            // Build Action is the name, not metadata
            if(String.Equals(name, BuildItemMetadata.BuildAction, StringComparison.OrdinalIgnoreCase))
                return item.ItemType;

            // Include is an attribute, not metadata
            if(String.Equals(name, BuildItemMetadata.IncludePath, StringComparison.OrdinalIgnoreCase))
                return item.UnevaluatedInclude;

            return item.GetMetadataValue(name);
        }

        /// <summary>
        /// Set a metadata value in the project item
        /// </summary>
        /// <param name="name">The name of the metadata element</param>
        /// <param name="value">The value to store in the element</param>
        public void SetMetadata(string name, string value)
        {
            // Build Action is the name, not metadata
            if(String.Equals(name, BuildItemMetadata.BuildAction, StringComparison.OrdinalIgnoreCase))
            {
                item.ItemType = value;
                this.OnPropertyChanged(name);
                return;
            }

            // Include is an attribute, not metadata
            if(String.Equals(name, BuildItemMetadata.IncludePath, StringComparison.OrdinalIgnoreCase))
            {
                item.UnevaluatedInclude = value;
                this.OnPropertyChanged(name);
                return;
            }

            if(String.IsNullOrEmpty(value))
                item.RemoveMetadata(name);
            else
                item.SetMetadataValue(name, value);

            this.OnPropertyChanged(name);
        }

        /// <summary>
        /// Remove the item from the project
        /// </summary>
        public void RemoveFromProjectFile()
        {
            projectFile.MSBuildProject.RemoveItem(item);
            projectFile.MSBuildProject.ReevaluateIfNecessary();

            projectFile = null;
            item = null;
        }
        #endregion

        #region Equality, ToString, etc.
        //=====================================================================

        /// <summary>
        /// Overload for equal operator
        /// </summary>
        /// <param name="element1">The first element object</param>
        /// <param name="element2">The second element object</param>
        /// <returns>True if equal, false if not</returns>
        public static bool operator == (ProjectElement element1, ProjectElement element2)
        {
            // Do they reference the same element?
            if(ReferenceEquals(element1, element2))
                return true;

            // Check null reference first
            if(element1 is null || element2 is null)
                return false;

            // Do they reference the same project?
            if(!element1.projectFile.Equals(element2.projectFile))
                return false;

            return String.Equals(element1.GetMetadata(BuildItemMetadata.IncludePath),
                element2.GetMetadata(BuildItemMetadata.IncludePath), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Overload for not equal operator
        /// </summary>
        /// <param name="element1">The first element object</param>
        /// <param name="element2">The second element object</param>
        /// <returns>True if not equal, false if they are equal</returns>
        public static bool operator !=(ProjectElement element1, ProjectElement element2)
        {
            return !(element1 == element2);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            ProjectElement element2 = obj as ProjectElement;

            if(element2 == null)
                return false;

            return this == element2;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return item.UnevaluatedInclude.GetHashCode();
        }
        #endregion
    }
}
