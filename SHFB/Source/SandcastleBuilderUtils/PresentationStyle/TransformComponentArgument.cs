//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : TranformComponentArgument.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/23/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is used to contain transform component argument values
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.   This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.6.0  10/24/2012  EFW  Created the code
//===============================================================================================================

using System.Xml.Linq;

namespace SandcastleBuilder.Utils.PresentationStyle
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
        /// <remarks>If set to a non-null value, the value of the <see cref="Content"/> property is ignored</remarks>
        public string Value { get; set; }

        /// <summary>
        /// This is used to get or set the argument's content for complex arguments
        /// </summary>
        /// <remarks>If set to a non-null value, the value of the <see cref="Value"/> property is ignored</remarks>
        public string Content { get; set; }
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

            if(this.Value == null)
                this.Content = clone.Content;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="argument">The XML element containing the transform component argument settings</param>
        public TransformComponentArgument(XElement argument)
        {
            this.Key = argument.Attribute("Key").Value;
            this.IsForConceptualBuild = ((bool?)argument.Attribute("ForConceptualBuild") ?? false);
            this.IsForReferenceBuild = ((bool?)argument.Attribute("ForReferenceBuild") ?? false);
            this.Description = (string)argument.Attribute("Description");
            this.Value = (string)argument.Attribute("Value");

            if(this.Value == null)
            {
                var reader = argument.CreateReader();
                reader.MoveToContent();

                this.Content = reader.ReadInnerXml();
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

            if(this.Value != null)
                e.Add(new XAttribute("Value", this.Value));
            else
                e.Add(XElement.Parse("<Content>" + this.Content + "</Content>").Elements());

            return e;
        }
        #endregion
    }
}
