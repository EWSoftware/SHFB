//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : ReferenceItem.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2006-2025, Eric Woodruff, All rights reserved
//
// This file contains a class representing a reference item that can be used by MRefBuilder to locate assembly
// dependencies for the assemblies being documented.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/23/2006  EFW  Created the code
// 06/30/2008  EFW  Rewrote to support the MSBuild project format
// 05/13/2015  EFW  Moved the file to the GUI project as it is only used there
// 05/13/2015  EFW  Moved the file to the GUI project as it is only used there
//===============================================================================================================

using System;
using System.ComponentModel;
using System.Drawing.Design;

using Microsoft.Build.Evaluation;

using Sandcastle.Core;
using Sandcastle.Platform.Windows.Design;

using SandcastleBuilder.MSBuild;
using SandcastleBuilder.MSBuild.HelpProject;

namespace SandcastleBuilder.Gui.MSBuild
{
    /// <summary>
    /// This represents a reference item that can be used by <strong>MRefBuilder</strong> to locate assembly
    /// dependencies for the assemblies being documented.
    /// </summary>
    public class ReferenceItem : ProjectElement, ICustomTypeDescriptor
    {
        #region Private data members
        //=====================================================================

        private FilePath hintPath;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to set or path to the dependency
        /// </summary>
        /// <value>For GAC dependencies, this should be null.</value>
        [Category("Metadata"), Description("The path to the referenced assembly."),
          Editor(typeof(FilePathObjectEditor), typeof(UITypeEditor)), RefreshProperties(RefreshProperties.All),
          MergableProperty(false), FileDialog("Select the reference item", "Library and Executable Files " +
            "(*.dll, *.exe)|*.dll;*.exe|Library Files (*.dll)|*.dll|Executable Files (*.exe)|*.exe|" +
            "All Files (*.*)|*.*", FileDialogType.FileOpen)]
        public virtual FilePath HintPath
        {
            get => hintPath;
            set
            {
                if(value == null || value.Path.Length == 0 || value.Path.IndexOfAny(FilePath.Wildcards) != -1)
                    throw new ArgumentException("A hint path must be specified and cannot contain wildcards (* or ?)",
                        nameof(value));

                this.SetMetadata(BuildItemMetadata.HintPath, value.PersistablePath);
                hintPath = value;
                hintPath.PersistablePathChanging += hintPath_PersistablePathChanging;
            }
        }

        /// <summary>
        /// This is used to get the reference description
        /// </summary>
        /// <remarks>This will be the filename for file references, a GAC name for GAC references, or a COM
        /// object name for COM references.</remarks>
        [Category("Reference"), Description("The reference name")]
        public virtual string Reference => this.Include;

        #endregion

        #region Designer methods
        //=====================================================================

        /// <summary>
        /// This is used to see if the <see cref="HintPath"/> property should be serialized
        /// </summary>
        /// <returns>True to serialize it, false if it matches the default and should not be serialized.  This
        /// property cannot be reset as it should always have a value.</returns>
        private bool ShouldSerializeHintPath()
        {
            return (this.HintPath.Path.Length != 0);
        }
        #endregion

        #region Private helper methods
        //=====================================================================

        /// <summary>
        /// This is used to handle changes in the <see cref="HintPath" /> properties such that the hint path gets
        /// stored in the project file.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void hintPath_PersistablePathChanging(object sender, EventArgs e)
        {
            this.SetMetadata(BuildItemMetadata.HintPath, hintPath.PersistablePath);
        }
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// This constructor is used to wrap an existing reference
        /// </summary>
        /// <param name="project">The project that owns the reference</param>
        /// <param name="existingItem">The existing reference</param>
        /// <overloads>There are two overloads for the constructor</overloads>
        internal ReferenceItem(SandcastleProject project, ProjectItem existingItem) : base(project, existingItem)
        {
            if(this.HasMetadata(BuildItemMetadata.HintPath))
            {
                hintPath = new FilePath(this.GetMetadata(BuildItemMetadata.HintPath), this.Project);
                hintPath.PersistablePathChanging += hintPath_PersistablePathChanging;
            }
        }

        /// <summary>
        /// This constructor is used to create a new reference and add it to the project
        /// </summary>
        /// <param name="project">The project that will own the reference</param>
        /// <param name="itemType">The type of reference to create</param>
        /// <param name="itemPath">The path to the reference</param>
        internal ReferenceItem(SandcastleProject project, string itemType, string itemPath) :
          base(project, itemType, itemPath)
        {
        }
        #endregion

        #region Equality and ToString methods
        //=====================================================================

        /// <summary>
        /// See if specified item equals this one by name alone
        /// </summary>
        /// <param name="obj">The object to compare to this one</param>
        /// <returns>True if equal, false if not</returns>
        public override bool Equals(object obj)
        {
            ReferenceItem refItem = obj as ReferenceItem;

            if(refItem == null)
                return false;

            return (this == refItem || (this.Reference == refItem.Reference));
        }

        /// <summary>
        /// Get a hash code for this item
        /// </summary>
        /// <returns>Returns the hash code for the reference description</returns>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        /// <summary>
        /// Return a string representation of the item
        /// </summary>
        /// <returns>Returns the reference description</returns>
        public override string ToString()
        {
            return this.Reference;
        }
        #endregion

        #region ICustomTypeDescriptor Members
        //=====================================================================

        /// <inheritdoc />
        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        /// <inheritdoc />
        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        /// <inheritdoc />
        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        /// <inheritdoc />
        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        /// <inheritdoc />
        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        /// <inheritdoc />
        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        /// <inheritdoc />
        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        /// <inheritdoc />
        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        /// <inheritdoc />
        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        /// <inheritdoc />
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(this, attributes, true);

            return this.FilterProperties(pdc);
        }

        /// <inheritdoc />
        public PropertyDescriptorCollection GetProperties()
        {
            PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(this, true);

            return this.FilterProperties(pdc);
        }

        /// <inheritdoc />
        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }
        #endregion

        #region Property filter method
        //=====================================================================

        /// <summary>
        /// This is used to filter out the <see cref="HintPath"/> property if not used
        /// </summary>
        /// <param name="pdc">The property descriptor collection to filter</param>
        /// <returns>The filtered property descriptor collection</returns>
        private PropertyDescriptorCollection FilterProperties(PropertyDescriptorCollection pdc)
        {
            PropertyDescriptorCollection adjustedProps = new([]);

            foreach(PropertyDescriptor pd in pdc)
            {
                if(pd.Name != "HintPath")
                    adjustedProps.Add(pd);
                else
                {
                    if(pd.GetValue(this) != null)
                        adjustedProps.Add(pd);
                }
            }

            return adjustedProps;
        }
        #endregion
    }
}
