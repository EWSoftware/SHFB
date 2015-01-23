//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : TranformComponentArgument.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/07/2014
// Note    : Copyright 2012-2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is used to contain transform component argument values
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.   This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.6.0  10/24/2012  EFW  Created the code
// -------  01/04/2014  EFW  Moved the code into Sandcastle.Core
//===============================================================================================================

using System;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle
{
    /// <summary>
    /// This class is used to contain transform component argument values
    /// </summary>
    /// <remarks>These are inserted into the BuildAssembler configuration file for the <c>TransformComponent</c>
    /// to use at build time.</remarks>
    public class TransformComponentArgument
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the argument key
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// This read-only property returns whether or not the argument applies to conceptual builds
        /// </summary>
        public bool IsForConceptualBuild { get; private set; }

        /// <summary>
        /// This read-only property returns whether or not the argument applies to reference builds
        /// </summary>
        public bool IsForReferenceBuild { get; private set; }

        /// <summary>
        /// This read-only property returns a description of the argument
        /// </summary>
        public string Description { get; private set; }

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
        /// <value>Value</value> property takes precedence if both are set.  The root element name is not
        /// relevant.  Only the content is significant and will be used.</remarks>
        public XElement Content { get; set; }
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Private copy constructor
        /// </summary>
        /// <param name="clone">The transform component argument to clone</param>
        private TransformComponentArgument(TransformComponentArgument clone)
        {
            this.Key = clone.Key;
            this.IsForConceptualBuild = clone.IsForConceptualBuild;
            this.IsForReferenceBuild = clone.IsForReferenceBuild;
            this.Description = clone.Description;
            this.Value = clone.Value;

            if(this.Value == null && this.Content != null)
                this.Content = new XElement(clone.Content);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">The transform argument key name</param>
        /// <param name="isForConceptualBuild">True if used in conceptual builds, false if not</param>
        /// <param name="isForReferenceBuild">True if used in reference builds, false if not</param>
        /// <param name="value">The default value of the argument</param>
        /// <param name="description">A description of the transform argument</param>
        /// <overloads>There are three overloads for the constructor</overloads>
        public TransformComponentArgument(string key, bool isForConceptualBuild, bool isForReferenceBuild,
          string value, string description)
        {
            this.Key = key;
            this.IsForConceptualBuild = isForConceptualBuild;
            this.IsForReferenceBuild = isForReferenceBuild;
            this.Description = description;
            this.Value = value;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">The transform argument key name</param>
        /// <param name="isForConceptualBuild">True if used in conceptual builds, false if not</param>
        /// <param name="isForReferenceBuild">True if used in reference builds, false if not</param>
        /// <param name="description">A description of the transform argument</param>
        /// <param name="content">The default content of the argument</param>
        public TransformComponentArgument(string key, bool isForConceptualBuild, bool isForReferenceBuild,
          string description, XElement content)
        {
            this.Key = key;
            this.IsForConceptualBuild = isForConceptualBuild;
            this.IsForReferenceBuild = isForReferenceBuild;
            this.Description = description;
            this.Content = new XElement(content);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="argument">The XML element containing the transform component argument settings</param>
        /// <remarks>This is used by designers to create arguments from serialized project settings</remarks>
        public TransformComponentArgument(XElement argument)
        {
            this.Key = argument.Attribute("Key").Value;
            this.IsForConceptualBuild = ((bool?)argument.Attribute("ForConceptualBuild") ?? false);
            this.IsForReferenceBuild = ((bool?)argument.Attribute("ForReferenceBuild") ?? false);
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
        /// This is used to clone a transform component argument
        /// </summary>
        /// <returns>A clone of the current transform component argument</returns>
        public TransformComponentArgument Clone()
        {
            return new TransformComponentArgument(this);
        }

        /// <summary>
        /// This is used to convert the transform component argument to an XML element for storing in a project
        /// </summary>
        /// <returns>The transform component argument as an XML element</returns>
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
