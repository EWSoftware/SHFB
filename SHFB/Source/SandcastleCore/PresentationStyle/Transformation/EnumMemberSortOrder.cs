//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : EnumMemberSortOrder.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/16/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains an enumerated type used to define how enumeration members are sorted
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/16/2022  EFW  Created the code
//===============================================================================================================

using System;

namespace Sandcastle.Core.PresentationStyle.Transformation
{
    /// <summary>
    /// This enumerated type defines how enumeration members are sorted
    /// </summary>
    [Serializable]
    public enum EnumMemberSortOrder
    {
        /// <summary>
        /// Sort by name
        /// </summary>
        Name,
        /// <summary>
        /// Sort by value
        /// </summary>
        Value
    }
}
