//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : SchemaHierarchyElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/23/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle schemaHierarchy elements
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/23/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements
{
    /// <summary>
    /// This is used to handle general <c>schemaHierarchy</c> elements in a topic
    /// </summary>
    public class SchemaHierarchyElement : Element
    {
        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public SchemaHierarchyElement() : base("schemaHierarchy")
        {
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <inheritdoc />
        public override void Render(TopicTransformationCore transformation, XElement element)
        {
            if(transformation == null)
                throw new ArgumentNullException(nameof(transformation));

            if(element == null)
                throw new ArgumentNullException(nameof(element));

            int indent = 0;
            var lineBreak = new XElement(Ddue + "lineBreak");

            foreach(var link in element.Elements(Ddue + "link"))
            {
                if(indent > 0)
                    transformation.CurrentElement.Add(indent.ToIndent());

                transformation.RenderNode(link);
                transformation.RenderNode(lineBreak);
                indent++;
            }
        }
        #endregion
    }
}
