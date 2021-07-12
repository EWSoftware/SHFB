//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : FileItem.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/14/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains a class representing a file that is part of the project (MAML/additional content, site
// map, style sheet, etc.).
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/24/2008  EFW  Created the code
// 12/04/2009  EFW  Added support for resource item files
// 07/09/2010  EFW  Updated for use with .NET 4.0 and MSBuild 4.0.
// 04/08/2012  EFW  Added support for XAML configuration files
// 05/08/2015  EFW  Removed support for ExcludeFromToc metadata item
//===============================================================================================================

// Ignore Spelling: Za

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

using Microsoft.Build.Evaluation;

using SandcastleBuilder.Utils.ConceptualContent;
using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This class represents a file that is part of the project (MAML/additional content, site map, style
    /// sheet, etc.).
    /// </summary>
    public class FileItem : ProjectElement, ICustomTypeDescriptor
    {
        #region Private data members
        //=====================================================================

        private BuildAction buildAction;
        private FilePath includePath, linkPath;
        private string imageId, altText;
        private bool copyToMedia;
        private int sortOrder;

        private static readonly Regex reInsertSpaces = new Regex(@"((?<=[a-z0-9])[A-Z](?=[a-z0-9]))|((?<=[A-Za-z])\d+)");

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to set or get the build action of the item
        /// </summary>
        /// <value>If set to <c>Image</c>, <see cref="ImageId"/> and <see cref="AlternateText" /> will be set to
        /// the filename if not set already.</value>
        [Category("Build Action"), Description("The build action for this item"),
          RefreshProperties(RefreshProperties.All), TypeConverter(typeof(BuildActionEnumConverter))]
        public BuildAction BuildAction
        {
            get => buildAction;
            set
            {
                string baseName;

                this.ItemType = value.ToString();
                buildAction = value;

                // Set default ID and description if set to Image
                if(buildAction == BuildAction.Image)
                {
                    baseName = Path.GetFileNameWithoutExtension(includePath);

                    if(String.IsNullOrEmpty(this.ImageId))
                        this.ImageId = baseName;

                    if(String.IsNullOrEmpty(this.AlternateText))
                    {
                        baseName = baseName.Replace("_", " ");
                        this.AlternateText = reInsertSpaces.Replace(baseName, " $&").Trim();
                    }
                }
            }
        }

        /// <summary>
        /// This is used to set or get the filename (include path)
        /// </summary>
        [Browsable(false)]
        public FilePath IncludePath
        {
            get => includePath;
            set
            {
                if(value == null || value.Path.Length == 0 || value.Path.IndexOfAny(new char[] { '*', '?' }) != -1)
                    throw new ArgumentException("A file path must be specified and cannot contain wildcards " +
                        "(* or ?)", nameof(value));

                this.Include = value.PersistablePath;

                includePath = value;
                includePath.PersistablePathChanging += includePath_PersistablePathChanging;
            }
        }

        /// <summary>
        /// This is used to set or get the link path
        /// </summary>
        /// <value>If the item has no link path, this returns the <see cref="IncludePath" /> path</value>
        [Browsable(false)]
        public FilePath LinkPath
        {
            get => linkPath ?? includePath;
            set
            {
                if(value != null && value.Path.Length != 0 && value.Path.IndexOfAny(new char[] { '*', '?' }) != -1)
                    throw new ArgumentException("A file path must be specified and cannot contain wildcards " +
                        "(* or ?)", nameof(value));

                if(value != null && value.Path.Length != 0)
                {
                    // Do this first in case the project isn't editable
                    this.SetMetadata(BuildItemMetadata.LinkPath, value.PersistablePath);
                    linkPath = value;
                    linkPath.PersistablePathChanging += linkPath_PersistablePathChanging;
                }
                else
                {
                    this.SetMetadata(BuildItemMetadata.LinkPath, null);
                    linkPath = null;
                }
            }
        }

        /// <summary>
        /// This is used to get the full path to the item
        /// </summary>
        [Category("File"), Description("The full path to the file")]
        public string FullPath => includePath;

        /// <summary>
        /// This is used to set or get the name of the item
        /// </summary>
        [Category("File"), Description("The name of the item"), RefreshProperties(RefreshProperties.All)]
        public string Name
        {
            get
            {
                string path = includePath;

                if(buildAction == BuildAction.Folder && path.EndsWith(@"\", StringComparison.Ordinal))
                    return Path.GetFileName(path.Substring(0, path.Length - 1));

                return Path.GetFileName(path);
            }
            set
            {
                string tempPath, newPath, path = includePath;

                if(String.IsNullOrEmpty(value) || value.IndexOfAny(new char[] { ':', '\\', '*', '?' }) != -1)
                    throw new ArgumentException("New name cannot be blank and cannot contain ':', '\\', '*', or '?'");

                if(buildAction != BuildAction.Folder)
                {
                    // If it's a link, copy the file to the project folder and remove the link metadata
                    if(this.HasMetadata(BuildItemMetadata.LinkPath))
                    {
                        newPath = linkPath;
                        File.Copy(path, newPath, true);
                        File.SetAttributes(newPath, FileAttributes.Normal);
                        path = newPath;
                        this.SetMetadata(BuildItemMetadata.LinkPath, null);
                    }

                    newPath = Path.Combine(Path.GetDirectoryName(path), value);

                    if(path != newPath)
                    {
                        // If the file exists and it isn't just a case change, disallow it
                        if(File.Exists(newPath) && String.Compare(path, newPath,
                          StringComparison.OrdinalIgnoreCase) != 0)
                            throw new ArgumentException("A file with that name already exists in the project folder");

                        File.Move(path, newPath);
                        this.IncludePath = new FilePath(newPath, this.Project);
                    }

                    return;
                }

                // Rename the folder and all items starting with the folder name
                if(path.EndsWith(@"\", StringComparison.Ordinal))
                    path = path.Substring(0, path.Length - 1);

                newPath = Path.Combine(Path.GetDirectoryName(path), value);

                if(Directory.Exists(newPath) && String.Compare(path, newPath,
                  StringComparison.OrdinalIgnoreCase) != 0)
                    throw new ArgumentException("A folder with that name already exists in the project folder");

                // To allow renaming a folder by changing its case, move it to a temporary name first and then
                // the new name.
                if(String.Compare(path, newPath, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    tempPath = Guid.NewGuid().ToString();
                    Directory.Move(path, tempPath);
                    path = tempPath;
                }

                Directory.Move(path, newPath);
                path = this.Include;
                newPath = Path.Combine(Path.GetDirectoryName(path.Substring(0, path.Length - 1)), value) + "\\";
                this.IncludePath = new FilePath(newPath, this.Project);

                foreach(ProjectItem item in this.Project.MSBuildProject.AllEvaluatedItems)
                    if(item.EvaluatedInclude.StartsWith(path, StringComparison.OrdinalIgnoreCase))
                        item.UnevaluatedInclude = newPath + item.UnevaluatedInclude.Substring(path.Length);
            }
        }

        /// <summary>
        /// This is used to get or set an ID for a conceptual content image
        /// </summary>
        /// <remarks>This is used to indicate that an image file is part of the conceptual content.  Image items
        /// without an ID are not valid and will be ignored.</remarks>
        [Category("Metadata"), Description("The ID for a conceptual content image"), DefaultValue(null)]
        public string ImageId
        {
            get => imageId;
            set
            {
                if(value != null)
                    value = value.Trim();

                this.SetMetadata(BuildItemMetadata.ImageId, value);
                imageId = value;
            }
        }

        /// <summary>
        /// This is used to get or set alternate text for an image
        /// </summary>
        [Category("Metadata"), Description("Image alternate text"), DefaultValue(null)]
        public string AlternateText
        {
            get => altText;
            set
            {
                if(value != null)
                    value = value.Trim();

                this.SetMetadata(BuildItemMetadata.AlternateText, value);
                altText = value;
            }
        }

        /// <summary>
        /// This is used to get or set whether an item is copied to the output folder during a build
        /// </summary>
        /// <remarks>If this is set to true, the image will always be copied to the build's media folder.  If
        /// false, it is only copied if referenced in a topic.</remarks>
        [Category("Metadata"), Description("If set to true, the image will always be copied to the build's " +
          "media folder.  If false, it is only copied if referenced in a topic."), DefaultValue(false)]
        public bool CopyToMedia
        {
            get => copyToMedia;
            set
            {
                this.SetMetadata(BuildItemMetadata.CopyToMedia, value.ToString(CultureInfo.InvariantCulture));
                copyToMedia = value;
            }
        }

        /// <summary>
        /// This is used to get or set the sort order for content layout and site map files
        /// </summary>
        [Category("Metadata"), Description("For content layout and site map files, this defines the sort " +
          "order for merging them into the table of contents."), DefaultValue(0)]
        public int SortOrder
        {
            get => sortOrder;
            set
            {
                this.SetMetadata(BuildItemMetadata.SortOrder, value.ToString(CultureInfo.InvariantCulture));
                sortOrder = value;
            }
        }
        #endregion

        #region Private helper methods
        //=====================================================================

        /// <summary>
        /// This is used to handle changes in the <see cref="IncludePath" /> properties such that the path gets
        /// stored in the project file.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void includePath_PersistablePathChanging(object sender, EventArgs e)
        {
            this.Include = includePath.PersistablePath;
        }

        /// <summary>
        /// This is used to handle changes in the <see cref="LinkPath" /> properties such that the path gets stored
        /// in the project file.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void linkPath_PersistablePathChanging(object sender, EventArgs e)
        {
            this.SetMetadata(BuildItemMetadata.LinkPath, includePath.PersistablePath);
        }
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// This constructor is used to wrap an existing project item
        /// </summary>
        /// <param name="project">The project that owns the item</param>
        /// <param name="existingItem">The existing project item</param>
        /// <overloads>There are two overloads for the constructor</overloads>
        internal FileItem(SandcastleProject project, ProjectItem existingItem) : base(project, existingItem)
        {
            buildAction = (BuildAction)Enum.Parse(typeof(BuildAction), this.ItemType, true);
            includePath = new FilePath(this.Include, this.Project);
            includePath.PersistablePathChanging += includePath_PersistablePathChanging;

            this.Include = includePath.PersistablePath;

            if(this.HasMetadata(BuildItemMetadata.LinkPath))
            {
                linkPath = new FilePath(this.GetMetadata(BuildItemMetadata.LinkPath), this.Project);
                linkPath.PersistablePathChanging += linkPath_PersistablePathChanging;
            }

            if(this.HasMetadata(BuildItemMetadata.ImageId))
                imageId = this.GetMetadata(BuildItemMetadata.ImageId);

            if(this.HasMetadata(BuildItemMetadata.AlternateText))
                altText = this.GetMetadata(BuildItemMetadata.AlternateText);

            if(this.HasMetadata(BuildItemMetadata.CopyToMedia))
                if(!Boolean.TryParse(this.GetMetadata(BuildItemMetadata.CopyToMedia), out copyToMedia))
                    copyToMedia = false;

            if(this.HasMetadata(BuildItemMetadata.SortOrder))
                if(!Int32.TryParse(this.GetMetadata(BuildItemMetadata.SortOrder), out sortOrder))
                    sortOrder = 0;
        }

        /// <summary>
        /// This constructor is used to create a new item and add it to the project
        /// </summary>
        /// <param name="project">The project that will own the item</param>
        /// <param name="itemType">The type of item to create</param>
        /// <param name="itemPath">The path to the item</param>
        internal FileItem(SandcastleProject project, string itemType, string itemPath) :
          base(project, itemType, itemPath)
        {
            buildAction = (BuildAction)Enum.Parse(typeof(BuildAction), this.ItemType, true);
            includePath = new FilePath(this.Include, this.Project);
            includePath.PersistablePathChanging += includePath_PersistablePathChanging;

            this.Include = includePath.PersistablePath;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Refresh the paths due to a parent path being renamed
        /// </summary>
        public void RefreshPaths()
        {
            includePath = new FilePath(this.Include, this.Project);

            if(this.HasMetadata(BuildItemMetadata.LinkPath))
                this.LinkPath = new FilePath(this.GetMetadata(BuildItemMetadata.LinkPath), this.Project);
        }

        /// <summary>
        /// This is used to convert the file item to a <see cref="ContentFile"/> instance
        /// </summary>
        /// <returns>The file item as a <see cref="ContentFile"/></returns>
        public ContentFile ToContentFile()
        {
            return new ContentFile(includePath)
            {
                LinkPath = linkPath,
                SortOrder = sortOrder,
                ContentFileProvider = this.Project
            };
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
        /// This is used to filter the properties based on the
        /// <see cref="BuildAction" />.
        /// </summary>
        /// <param name="pdc">The property descriptor collection to filter</param>
        /// <returns>The filtered property descriptor collection</returns>
        private PropertyDescriptorCollection FilterProperties(PropertyDescriptorCollection pdc)
        {
            List<string> removeProps = new List<string>();

            switch(buildAction)
            {
                case BuildAction.None:
                case BuildAction.CodeSnippets:
                case BuildAction.ResourceItems:
                case BuildAction.Tokens:
                case BuildAction.XamlConfiguration:
                    removeProps.AddRange(new string[] { "ImageId", "AlternateText", "CopyToMedia", "SortOrder" });
                    break;

                case BuildAction.Content:
                    removeProps.AddRange(new string[] { "ImageId", "AlternateText", "CopyToMedia", "SortOrder" });
                    break;

                case BuildAction.Folder:
                    removeProps.AddRange(new string[] { "BuildAction", "ImageId", "AlternateText", "CopyToMedia",
                        "SortOrder" });
                    break;

                case BuildAction.Image:
                    removeProps.AddRange(new string[] { "SortOrder" });
                    break;

                case BuildAction.ContentLayout:
                case BuildAction.SiteMap:
                    removeProps.AddRange(new string[] { "ImageId", "AlternateText", "CopyToMedia" });
                    break;

                default:    // Leave them all in
                    break;
            }

            PropertyDescriptorCollection adjustedProps = new PropertyDescriptorCollection(Array.Empty<PropertyDescriptor>());

            foreach(PropertyDescriptor pd in pdc)
                if(removeProps.IndexOf(pd.Name) == -1)
                    adjustedProps.Add(pd);

            return adjustedProps;
        }
        #endregion
    }
}
