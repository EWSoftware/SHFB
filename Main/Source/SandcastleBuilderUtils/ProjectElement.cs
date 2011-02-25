//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ProjectElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/09/2010
// Note    : Copyright 2008-2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a wrapper class for build items in the project.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  06/23/2008  EFW  Created the code
//=============================================================================

using System;
using System.ComponentModel;

using Microsoft.Build.BuildEngine;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This class is a wrapper for build items in the project.
    /// </summary>
    public sealed class ProjectElement
    {
        #region Constants
        //=====================================================================

        /// <summary>Build action</summary>
        public const string BuildAction = "BuildAction";
        /// <summary>Include item</summary>
        public const string IncludePath = "Include";

        /// <summary>File reference hint path</summary>
        public const string HintPath = "HintPath";
        /// <summary>Linked item path</summary>
        public const string LinkPath = "Link";

        /// <summary>Project GUID item</summary>
        public const string ProjectGuid = "Project";
        /// <summary>Project name item</summary>
        public const string Name = "Name";

        /// <summary>Project COM object's GUID</summary>
        public const string Guid = "Guid";
        /// <summary>Project COM object's major version</summary>
        public const string VersionMajor = "VersionMajor";
        /// <summary>Project COM object's minor version</summary>
        public const string VersionMinor = "VersionMinor";
        /// <summary>Project COM object's wrapper tool</summary>
        public const string WrapperTool = "WrapperTool";

        /// <summary>Configuration setting</summary>
        public const string Configuration = "Configuration";
        /// <summary>Platform setting</summary>
        public const string Platform = "Platform";
        /// <summary>Output directory setting</summary>
        public const string OutDir = "OutDir";

        /// <summary>Image ID</summary>
        public const string ImageId = "ImageId";
        /// <summary>Alternate text</summary>
        public const string AlternateText = "AlternateText";
        /// <summary>Copy to media folder</summary>
        public const string CopyToMedia = "CopyToMedia";
        /// <summary>Exclude from table of contents</summary>
        public const string ExcludeFromToc = "ExcludeFromToc";
        /// <summary>Sort order</summary>
        public const string SortOrder = "SortOrder";

        // Visual Studio solution macros
        /// <summary>Solution path (directory and filename)</summary>
        public const string SolutionPath = "SolutionPath";
        /// <summary>Solution directory</summary>
        public const string SolutionDir = "SolutionDir";
        /// <summary>Solution filename (no path)</summary>
        public const string SolutionFileName = "SolutionFileName";
        /// <summary>Solution name (no path or extension)</summary>
        public const string SolutionName = "SolutionName";
        /// <summary>Solution extension</summary>
        public const string SolutionExt = "SolutionExt";
        #endregion

        #region Private data members
        //=====================================================================

        private SandcastleProject projectFile;
        private BuildItem item;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to set or get the item name (a.k.a BuildAction)
        /// </summary>
        public string ItemName
        {
            get { return item.Name; }
            set
            {
                this.CheckProjectIsEditable();
                item.Name = value;
                projectFile.MarkAsDirty();
            }
        }

        /// <summary>
        /// This is used to set or get the filename (Include attribute)
        /// </summary>
        public string Include
        {
            get { return item.Include; }
            set
            {
                if(item.Include != value)
                {
                    if(String.IsNullOrEmpty(value) || value.IndexOfAny(new char[] { '*', '?' }) != -1)
                        throw new ArgumentException("The filename cannot be " +
                            "blank and cannot contain wildcards (* or ?)");

                    // Folder items must end in a backslash
                    if(item.Name == Utils.BuildAction.Folder.ToString() &&
                      value[value.Length - 1] != '\\')
                        value += @"\";

                    this.CheckProjectIsEditable();
                    item.Include = value;
                    projectFile.MarkAsDirty();
                }
            }
        }

        /// <summary>
        /// This read-only property is used to get the containing project
        /// </summary>
        public SandcastleProject Project
        {
            get { return projectFile; }
        }
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// This constructor is used to wrap an existing project item.
        /// </summary>
        /// <param name="project">The project that owns the item</param>
        /// <param name="existingItem">The existing item</param>
        /// <overloads>There are two overloads for the constructor</overloads>
        internal ProjectElement(SandcastleProject project, BuildItem existingItem)
        {
            if(project == null)
                throw new ArgumentNullException("project");

            if(existingItem == null)
                throw new ArgumentNullException("existingItem");

            projectFile = project;
            item = existingItem;
        }

        /// <summary>
        /// This constructor is used to create a new build item and add it to
        /// the project.
        /// </summary>
        /// <param name="project">The project that will own the item</param>
        /// <param name="itemType">The type of build item to create</param>
        /// <param name="itemPath">The path to the item.  This can be relative
        /// or absolute and may contain variable references.</param>
        internal ProjectElement(SandcastleProject project, string itemType, string itemPath)
        {
            if(project == null)
                throw new ArgumentNullException("project");

            if(String.IsNullOrEmpty(itemPath))
                throw new ArgumentException("Cannot be null or empty",
                    "itemPath");

            if(String.IsNullOrEmpty(itemType))
                throw new ArgumentException("Cannot be null or empty",
                    "itemType");

            projectFile = project;
            this.CheckProjectIsEditable();

            if(itemType == Utils.BuildAction.Folder.ToString() && itemPath[itemPath.Length - 1] != '\\')
                itemPath += @"\";

            item = project.MSBuildProject.AddNewItem(itemType, itemPath);
            projectFile.MarkAsDirty();
        }
        #endregion

        #region Project item methods
        //=====================================================================

        /// <summary>
        /// This is used to see if the project can be edited.  If not, abort
        /// the change by throwing an exception.
        /// </summary>
        private void CheckProjectIsEditable()
        {
            CancelEventArgs ce = new CancelEventArgs();
            projectFile.OnQueryEditProjectFile(ce);

            if(ce.Cancel)
                throw new OperationCanceledException("Project cannot be edited");
        }

        /// <summary>
        /// See if the named metadata item exists
        /// </summary>
        /// <param name="name">The metadata name for which to check</param>
        /// <returns>True if present, false if not</returns>
        public bool HasMetadata(string name)
        {
            // Build Action is the name, not metadata.
            // Include is an attribute, not metadata.
            if(String.Compare(name, ProjectElement.BuildAction, StringComparison.OrdinalIgnoreCase) == 0 ||
              String.Compare(name, ProjectElement.IncludePath, StringComparison.OrdinalIgnoreCase) == 0)
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
            if(String.Compare(name, ProjectElement.BuildAction, StringComparison.OrdinalIgnoreCase) == 0)
                return item.Name;

            // Include is an attribute, not metadata
            if(String.Compare(name, ProjectElement.IncludePath, StringComparison.OrdinalIgnoreCase) == 0)
                return item.Include;

            return item.GetMetadata(name);
        }

        /// <summary>
        /// Set a metadata value in the project item
        /// </summary>
        /// <param name="name">The name of the metadata element</param>
        /// <param name="value">The value to store in the element</param>
        public void SetMetadata(string name, string value)
        {
            this.CheckProjectIsEditable();

            // Build Action is the name, not metadata
            if(String.Compare(name, ProjectElement.BuildAction, StringComparison.OrdinalIgnoreCase) == 0)
            {
                item.Name = value;
                return;
            }

            // Include is an attribute, not metadata
            if(String.Compare(name, ProjectElement.IncludePath, StringComparison.OrdinalIgnoreCase) == 0)
            {
                item.Include = value;
                return;
            }

            if(String.IsNullOrEmpty(value))
                item.RemoveMetadata(name);
            else
                item.SetMetadata(name, value);

            projectFile.MarkAsDirty();
        }

        /// <summary>
        /// Remove the item from the project
        /// </summary>
        public void RemoveFromProjectFile()
        {
            this.CheckProjectIsEditable();
            projectFile.MSBuildProject.RemoveItem(item);
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
        /// <returns>True if equal, false if not.</returns>
        public static bool operator == (ProjectElement element1,
          ProjectElement element2)
        {
            // Do they reference the same element?
            if(Object.ReferenceEquals(element1, element2))
                return true;

            // Check null reference first (cast to object first to avoid
            // stack overflow).
            if(element1 as object == null || element2 as object == null)
                return false;

            // Do they reference the same project?
            if(!element1.projectFile.Equals(element2.projectFile))
                return false;

            return String.Equals(element1.GetMetadata(ProjectElement.IncludePath),
                element2.GetMetadata(ProjectElement.IncludePath),
                StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Overload for not equal operator
        /// </summary>
        /// <param name="element1">The first element object</param>
        /// <param name="element2">The second element object</param>
        /// <returns>True if not equal, false if they are equal.</returns>
        public static bool operator !=(ProjectElement element1,
          ProjectElement element2)
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
            return item.Include.GetHashCode();
        }
        #endregion
    }
}
