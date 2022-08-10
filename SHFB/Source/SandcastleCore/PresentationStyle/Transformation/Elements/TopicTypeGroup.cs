//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : TopicTypeGroup.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/28/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains the enumerated type used to define topic type groups
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/28/2022  EFW  Created the code
//===============================================================================================================

using System;

namespace Sandcastle.Core.PresentationStyle.Transformation.Elements
{
    /// <summary>
    /// This enumerated type defines topic type groups
    /// </summary>
    [Serializable]
    public enum TopicTypeGroup
    {
        /// <summary>
        /// Concepts
        /// </summary>
        Concepts,
        /// <summary>
        /// How To
        /// </summary>
        HowTo,
        /// <summary>
        /// Tasks
        /// </summary>
        Tasks,
        /// <summary>
        /// Reference
        /// </summary>
        Reference,
        /// <summary>
        /// Samples
        /// </summary>
        Samples,
        /// <summary>
        /// Other Resources
        /// </summary>
        OtherResources
    }
}
