//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : NamedSectionElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/26/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle named topic section elements
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/21/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Linq;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements
{
    /// <summary>
    /// This is used to handle named section elements in a topic
    /// </summary>
    /// <remarks>The title is assumed to be a localized include item named after the element with a "title_"
    /// prefix.</remarks>
    public class NamedSectionElement : Element
    {
        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public NamedSectionElement(string name) : base(name)
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

            if(element.Elements().Any() || element.Value.NormalizeWhiteSpace().Length != 0)
            {
                var (title, content) = transformation.CreateSection(element.GenerateUniqueId(), true,
                    "title_" + this.Name, null);

                if(title != null)
                    transformation.CurrentElement.Add(title);

                if(content != null)
                    transformation.CurrentElement.Add(content);

                transformation.RenderChildElements(content ?? transformation.CurrentElement, element.Nodes());
            }
        }
        #endregion
    }
}
