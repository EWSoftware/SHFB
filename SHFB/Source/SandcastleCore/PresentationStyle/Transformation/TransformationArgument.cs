//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : TranformationArgument.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/31/2022
// Note    : Copyright 2012-2022, Eric Woodruff, All rights reserved
//
// This file contains a class that is used to contain transformation argument values
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.   This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/24/2012  EFW  Created the code
// 01/04/2014  EFW  Moved the code into Sandcastle.Core
// 01/30/2022  EFW  Renamed from TransformationComponentArgument and modified for use with the new code-based
//                  presentation style transformation.
//===============================================================================================================

using System;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation
{
    /// <summary>
    /// This class is used to contain transformation argument values
    /// </summary>
    /// <remarks>These are modified using any values stored in the help file builder project at build time</remarks>
    public class TransformationArgument
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the argument key
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// This read-only property returns whether or not the argument applies to conceptual builds
        /// </summary>
        public bool IsForConceptualBuild { get; }

        /// <summary>
        /// This read-only property returns whether or not the argument applies to reference builds
        /// </summary>
        public bool IsForReferenceBuild { get; }

        /// <summary>
        /// This read-only property returns a description of the argument
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// This is used to get or set the argument's value for simple arguments
        /// </summary>
        /// <remarks>If set to a non-null value, the value of the <see cref="Content"/> property is ignored.
        /// This property will take precedence if both are set.</remarks>
        public string Value { get; set; }

        /// <summary>
        /// This is used to get or set the argument's content for complex arguments
        /// </summary>
        /// <remarks>If set to a non-null value, the value of the <see cref="Value"/> property is ignored.  The
        /// <c>Value</c> property takes precedence if both are set.  The root element name is not
        /// relevant.  Only the content is significant and will be used.</remarks>
        public XElement Content { get; set; }

        /// <summary>
        /// The default value if one was specified
        /// </summary>
        public string DefaultValue { get; }

        /// <summary>
        /// The default content if any was specified
        /// </summary>
        public XElement DefaultContent { get; }

        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Private copy constructor
        /// </summary>
        /// <param name="clone">The transformation argument to clone</param>
        private TransformationArgument(TransformationArgument clone)
        {
            this.Key = clone.Key;
            this.IsForConceptualBuild = clone.IsForConceptualBuild;
            this.IsForReferenceBuild = clone.IsForReferenceBuild;
            this.Description = clone.Description;
            this.Value = this.DefaultValue = clone.Value;

            if(this.Value == null && this.Content != null)
                this.Content = this.DefaultContent = new XElement(clone.Content);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">The transformation argument key name</param>
        /// <param name="isForConceptualBuild">True if used in conceptual builds, false if not</param>
        /// <param name="isForReferenceBuild">True if used in reference builds, false if not</param>
        /// <param name="value">The default value of the argument</param>
        /// <param name="description">A description of the transformation argument</param>
        /// <overloads>There are two overloads for the constructor</overloads>
        public TransformationArgument(string key, bool isForConceptualBuild, bool isForReferenceBuild,
          string value, string description)
        {
            this.Key = key;
            this.IsForConceptualBuild = isForConceptualBuild;
            this.IsForReferenceBuild = isForReferenceBuild;
            this.Description = description;
            this.Value = this.DefaultValue = value;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">The transformation argument key name</param>
        /// <param name="isForConceptualBuild">True if used in conceptual builds, false if not</param>
        /// <param name="isForReferenceBuild">True if used in reference builds, false if not</param>
        /// <param name="description">A description of the transformation argument</param>
        /// <param name="content">The default content of the argument</param>
        public TransformationArgument(string key, bool isForConceptualBuild, bool isForReferenceBuild,
          string description, XElement content)
        {
            this.Key = key;
            this.IsForConceptualBuild = isForConceptualBuild;
            this.IsForReferenceBuild = isForReferenceBuild;
            this.Description = description;
            this.Content = this.DefaultContent = new XElement(content);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="argument">The XML element containing the transform component argument settings</param>
        /// <remarks>This is used by designers to create arguments from serialized project settings</remarks>
        public TransformationArgument(XElement argument)
        {
            if(argument == null)
                throw new ArgumentNullException(nameof(argument));

            this.Key = argument.Attribute("Key").Value;
            this.IsForConceptualBuild = (bool?)argument.Attribute("ForConceptualBuild") ?? false;
            this.IsForReferenceBuild = (bool?)argument.Attribute("ForReferenceBuild") ?? false;
            this.Description = (string)argument.Attribute("Description");
            this.Value = (string)argument.Attribute("Value");

            if(this.Value == null && (argument.HasElements || !argument.IsEmpty))
            {
                var reader = argument.CreateReader();
                reader.MoveToContent();

                this.Content = XElement.Parse("<Content>" + reader.ReadInnerXml() + "</Content>");
            }
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// This is used to clone a transformation argument
        /// </summary>
        /// <returns>A clone of the current transformation argument</returns>
        public TransformationArgument Clone()
        {
            return new TransformationArgument(this);
        }

        /// <summary>
        /// This is used to convert the transformation argument to an XML element for storing in a help file
        /// builder project.
        /// </summary>
        /// <returns>The transformation argument key and value as an XML element</returns>
        public XElement ToXml()
        {
            var e = new XElement("Argument", new XAttribute("Key", this.Key));

            if(this.Value != null || this.Content == null)
                e.Add(new XAttribute("Value", this.Value ?? String.Empty));
            else
                e.Add(this.Content.Elements());

            return e;
        }
        #endregion
    }
}
