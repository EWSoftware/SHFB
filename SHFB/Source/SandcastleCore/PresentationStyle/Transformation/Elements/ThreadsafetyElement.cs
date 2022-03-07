//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ThreadsafetyElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/12/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle threadsafety elements
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/12/2022  EFW  Created the code
//===============================================================================================================

using System;
using System.Linq;
using System.Xml.Linq;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements
{
    /// <summary>
    /// This is used to handle <c>threadsafety</c> elements
    /// </summary>
    public class ThreadsafetyElement : Element
    {
        #region Constructor
        //=====================================================================

        /// <inheritdoc />
        public ThreadsafetyElement() : base("threadsafety")
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

            var (title, content) = transformation.CreateSection(element.GenerateUniqueId(), true,
                "title_threadSafety", null);

            transformation.CurrentElement.Add(title);
            transformation.CurrentElement.Add(content);

            if(element.Value.NormalizeWhiteSpace().Length != 0)
                transformation.RenderChildElements(content, element.Nodes());
            else
            {
                if(!Boolean.TryParse(element.Attribute("static")?.Value, out bool staticThreadSafe))
                    staticThreadSafe = true;

                if(!Boolean.TryParse(element.Attribute("instance")?.Value, out bool instanceThreadSafe))
                    instanceThreadSafe = false;

                if(staticThreadSafe && !instanceThreadSafe)
                    content.Add(new XElement("include", new XAttribute("item", "boilerplate_threadSafety")));
                else
                {
                    if(staticThreadSafe)
                        content.Add(new XElement("include", new XAttribute("item", "text_staticThreadSafe")));
                    else
                        content.Add(new XElement("include", new XAttribute("item", "text_staticNotThreadSafe")));

                    if(instanceThreadSafe)
                        content.Add(new XElement("include", new XAttribute("item", "text_instanceThreadSafe")));
                    else
                        content.Add(new XElement("include", new XAttribute("item", "text_instanceNotThreadSafe")));
                }
            }
        }
        #endregion
    }
}
