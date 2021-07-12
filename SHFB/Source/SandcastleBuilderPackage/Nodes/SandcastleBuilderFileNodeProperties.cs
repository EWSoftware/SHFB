//===============================================================================================================
// System  : Sandcastle Help File Builder Package
// File    : SandcastleBuilderFileNodeProperties.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/26/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This file contains the class that exposes the properties for the SandcastleBuilderFileNode object
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
//===============================================================================================================
// 03/27/2011  EFW  Created the code
// 04/08/2012  EFW  Added support for XAML configuration files
// 05/03/2015  EFW  Removed support for topic transformation files and ExcludeFromToc metadata
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Project;

using SandcastleBuilder.Utils.Design;
using SandcastleBuildAction = SandcastleBuilder.Utils.BuildAction;
using SandcastleBuildItemMetadata = SandcastleBuilder.Utils.BuildItemMetadata;

namespace SandcastleBuilder.Package.Nodes
{
    /// <summary>
    /// This is used to expose the properties for <see cref="SandcastleBuilderFileNode" /> objects.
    /// </summary>
    [CLSCompliant(false), ComVisible(true), Guid("6F22569B-BAF8-415a-9E9A-C84D808BD9AC")]
    public sealed class SandcastleBuilderFileNodeProperties : FileNodeProperties
    {
        #region Hidden properties
        //=====================================================================

        /// <summary>
        /// This is overridden to hide the default build action property since the help file builder supports
        /// several other build actions.
        /// </summary>
        /// <remarks>Use <see cref="SandcastleBuildAction"/> instead.</remarks>
        [Browsable(false), AutomationBrowsable(false)]
        public override BuildAction BuildAction
        {
            get => base.BuildAction;
            set => base.BuildAction = value;
        }

        /// <summary>
        /// The URL of the item
        /// </summary>
        /// <remarks>The examples don't show this property but it does get used.  If not present, an exception is
        /// thrown.</remarks>
        [Browsable(false)]
        public string URL => "file:///" + this.Node.Url;

        #endregion

        #region File node properties
        //=====================================================================

        // NOTE: If you apply a type converter attribute to any property, it must be of the type
        //       PropertyPageTypeConverterAttribute or it will be ignored.

        /// <summary>
        /// This is used to get or set the Sandcastle Help File Builder build action
        /// </summary>
        [Category("Advanced"), DisplayName("Build Action"), Description("The build action for this item"),
          RefreshProperties(RefreshProperties.All), PropertyPageTypeConverter(typeof(BuildActionEnumConverter))]
        public SandcastleBuildAction SandcastleBuildAction
        {
            get
            {
                string value = this.Node.ItemNode.ItemName;

                if(value == null || value.Length == 0)
                    return SandcastleBuildAction.None;

                return (SandcastleBuildAction)Enum.Parse(typeof(SandcastleBuildAction), value);
            }
            set
            {
                this.Node.ItemNode.ItemName = value.ToString();

                // Set default ID and alternate text if set to Image
                if(value == SandcastleBuildAction.Image)
                    this.Node.ItemNode.SetImageMetadata();
            }
        }

        /// <summary>
        /// This is used to get or set an ID for a conceptual content image
        /// </summary>
        /// <remarks>This is used to indicate that an image file is part of the conceptual content.  Image items
        /// without an ID are not valid and will be ignored.</remarks>
        [Category("Metadata"), Description("The ID for a conceptual content image"), DefaultValue(null),
          DisplayName("Image ID")]
        public string ImageId
        {
            get => this.Node.ItemNode.GetMetadata(SandcastleBuildItemMetadata.ImageId);
            set
            {
                if(value != null)
                    value = value.Trim();

                this.Node.ItemNode.SetMetadata(SandcastleBuildItemMetadata.ImageId, value);
            }
        }

        /// <summary>
        /// This is used to get or set alternate text for an image
        /// </summary>
        [Category("Metadata"), Description("Image alternate text"), DefaultValue(null),
          DisplayName("Alternate Text")]
        public string AlternateText
        {
            get => this.Node.ItemNode.GetMetadata(SandcastleBuildItemMetadata.AlternateText);
            set
            {
                if(value != null)
                    value = value.Trim();

                this.Node.ItemNode.SetMetadata(SandcastleBuildItemMetadata.AlternateText, value);
            }
        }

        /// <summary>
        /// This is used to get or set whether an item is copied to the output folder during a build
        /// </summary>
        /// <remarks>If this is set to true, the image will always be copied to the build's media folder.  If
        /// false, it is only copied if referenced in a topic.</remarks>
        [Category("Metadata"), Description("If set to true, the image will always be copied to the " +
          "build's media folder.  If false, it is only copied if referenced in a topic."),
          DefaultValue(false), DisplayName("Copy To Media")]
        public bool CopyToMedia
        {
            get
            {
                if(!Boolean.TryParse(this.Node.ItemNode.GetMetadata(SandcastleBuildItemMetadata.CopyToMedia), out bool value))
                    return false;

                return value;
            }
            set => this.Node.ItemNode.SetMetadata(SandcastleBuildItemMetadata.CopyToMedia, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// This is used to get or set the sort order for content layout and site map files
        /// </summary>
        [Category("Metadata"), Description("For content layout and site map files, this defines the " +
          "sort order for merging them into the table of contents."), DefaultValue(0), DisplayName("Sort Order")]
        public int SortOrder
        {
            get
            {
                if(!Int32.TryParse(this.Node.ItemNode.GetMetadata(SandcastleBuildItemMetadata.SortOrder), out int value))
                    return 0;

                return value;
            }
            set => this.Node.ItemNode.SetMetadata(SandcastleBuildItemMetadata.SortOrder, value.ToString(CultureInfo.InvariantCulture));
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">The node that contains the properties to expose via the Property Browser.</param>
        public SandcastleBuilderFileNodeProperties(HierarchyNode node) : base(node)
        {
        }
        #endregion

        #region ICustomTypeDescriptor Members
        //=====================================================================

        /// <inheritdoc />
        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return this.FilterProperties(TypeDescriptor.GetProperties(this, attributes, true));
        }

        /// <inheritdoc />
        public override PropertyDescriptorCollection GetProperties()
        {
            return this.FilterProperties(TypeDescriptor.GetProperties(this, true));
        }
        #endregion

        #region Property filter method
        //=====================================================================

        /// <summary>
        /// This is used to filter the properties based on the <see cref="BuildAction" />
        /// </summary>
        /// <param name="pdc">The property descriptor collection to filter</param>
        /// <returns>The filtered property descriptor collection</returns>
        private PropertyDescriptorCollection FilterProperties(PropertyDescriptorCollection pdc)
        {
            List<string> removeProps = new List<string>();

            switch(this.SandcastleBuildAction)
            {
                case SandcastleBuildAction.None:
                case SandcastleBuildAction.CodeSnippets:
                case SandcastleBuildAction.ResourceItems:
                case SandcastleBuildAction.Tokens:
                case SandcastleBuildAction.XamlConfiguration:
                    removeProps.AddRange(new string[] { "ImageId", "AlternateText", "CopyToMedia", "SortOrder" });
                    break;

                case SandcastleBuildAction.Content:
                    removeProps.AddRange(new string[] { "ImageId", "AlternateText", "CopyToMedia",
                        "SortOrder" });
                    break;

                case SandcastleBuildAction.Image:
                    removeProps.AddRange(new string[] { "SortOrder" });
                    break;

                case SandcastleBuildAction.ContentLayout:
                case SandcastleBuildAction.SiteMap:
                    removeProps.AddRange(new string[] { "ImageId", "AlternateText", "CopyToMedia" });
                    break;

                default:    // Leave them all in
                    break;
            }

            var adjustedProps = new PropertyDescriptorCollection(Array.Empty<PropertyDescriptor>());

            // NOTE: Visual Studio requires that the property descriptors be wrapped in a
            // DesignPropertyDescriptor.  In addition, any TypeConverterAttribute must be
            // of the type PropertyPageTypeConverterAttribute.  If either condition is not
            // met they will be ignored.
            foreach(PropertyDescriptor pd in pdc)
                if(removeProps.IndexOf(pd.Name) == -1)
                    adjustedProps.Add(base.CreateDesignPropertyDescriptor(pd));

            return adjustedProps;
        }
        #endregion
    }
}
