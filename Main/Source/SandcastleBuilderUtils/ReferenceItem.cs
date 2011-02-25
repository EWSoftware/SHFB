//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ReferenceItem.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/20/2008
// Note    : Copyright 2006-2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing a reference item that can be used
// by MRefBuilder to locate assembly dependencies for the assemblies being
// documented.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.1.0.0  08/23/2006  EFW  Created the code
// 1.8.0.0  06/30/2008  EFW  Rewrote to support the MSBuild project format
//=============================================================================

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Xml;

using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This represents a reference item that can be used by <b>MRefBuilder</b>
    /// to locate assembly dependencies for the assemblies being documented.
    /// </summary>
    public class ReferenceItem : BaseBuildItem, ICustomTypeDescriptor
    {
        #region Private data members
        //=====================================================================
        // Private data members

        private FilePath hintPath;
        #endregion

        #region Properties
        //=====================================================================
        // Properties

        /// <summary>
        /// This is used to set or path to the dependency
        /// </summary>
        /// <value>For GAC dependencies, this should be null.</value>
        [Category("Metadata"), Description("The path to the referenced assembly."),
          Editor(typeof(FilePathObjectEditor), typeof(UITypeEditor)),
          RefreshProperties(RefreshProperties.All), MergableProperty(false),
          FileDialog("Select the reference item",
            "Library and Executable Files (*.dll, *.exe)|*.dll;*.exe|" +
            "Library Files (*.dll)|*.dll|Executable Files (*.exe)|*.exe|" +
            "All Files (*.*)|*.*", FileDialogType.FileOpen)]
        public virtual FilePath HintPath
        {
            get { return hintPath; }
            set
            {
                if(value == null || value.Path.Length == 0 ||
                  value.Path.IndexOfAny(new char[] { '*', '?' }) != -1)
                    throw new ArgumentException("A hint path must be " +
                        "specified and cannot contain wildcards (* or ?)",
                        "value");

                base.ProjectElement.SetMetadata(ProjectElement.HintPath,
                    value.PersistablePath);
                hintPath = value;
                hintPath.PersistablePathChanging += new EventHandler(
                    hintPath_PersistablePathChanging);
            }
        }

        /// <summary>
        /// This is used to get the reference description
        /// </summary>
        [Category("Reference"), Description("The reference name")]
        public virtual string Reference
        {
            get
            {
                // This will be the filename for file references, a GAC name
                // for GAC references, or a COM object name for COM references.
                return base.ProjectElement.Include;
            }
        }
        #endregion

        #region Designer methods
        //=====================================================================
        // Designer methods

        /// <summary>
        /// This is used to see if the <see cref="HintPath"/> property should
        /// be serialized.
        /// </summary>
        /// <returns>True to serialize it, false if it matches the default
        /// and should not be serialized.  This property cannot be reset
        /// as it should always have a value.</returns>
        private bool ShouldSerializeHintPath()
        {
            return (this.HintPath.Path.Length != 0);
        }
        #endregion

        #region Private helper methods
        //=====================================================================

        /// <summary>
        /// This is used to handle changes in the <see cref="HintPath" />
        /// properties such that the hint path gets stored in the project file.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void hintPath_PersistablePathChanging(object sender, EventArgs e)
        {
            base.ProjectElement.SetMetadata(ProjectElement.HintPath,
                hintPath.PersistablePath);
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Internal Constructor
        /// </summary>
        /// <param name="element">The project element</param>
        internal ReferenceItem(ProjectElement element) : base(element)
        {
            if(base.ProjectElement.HasMetadata(ProjectElement.HintPath))
            {
                hintPath = new FilePath(base.ProjectElement.GetMetadata(
                    ProjectElement.HintPath), base.ProjectElement.Project);
                hintPath.PersistablePathChanging += new EventHandler(
                    hintPath_PersistablePathChanging);
            }
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
            PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(
                this, attributes, true);

            return this.FilterProperties(pdc);
        }

        /// <inheritdoc />
        public PropertyDescriptorCollection GetProperties()
        {
            PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(
                this, true);

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
        /// This is used to filter out the <see cref="HintPath"/> property if
        /// not used.
        /// </summary>
        /// <param name="pdc">The property descriptor collection to filter</param>
        /// <returns>The filtered property descriptor collection</returns>
        private PropertyDescriptorCollection FilterProperties(
          PropertyDescriptorCollection pdc)
        {
            PropertyDescriptorCollection adjustedProps = new
                PropertyDescriptorCollection(new PropertyDescriptor[] { });

            foreach(PropertyDescriptor pd in pdc)
                if(pd.Name != "HintPath")
                    adjustedProps.Add(pd);
                else
                    if(pd.GetValue(this) != null)
                        adjustedProps.Add(pd);

            return adjustedProps;
        }
        #endregion
    }
}
