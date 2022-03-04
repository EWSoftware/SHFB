//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : LogoPlacement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/30/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains an enumerated type used to define logo alignment values
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/30/2022  EFW  Created the code
//===============================================================================================================

using System;

namespace Sandcastle.Core.PresentationStyle.Transformation
{
    /// <summary>
    /// This enumerated type defines logo alignment values used when the logo is placed above the title
    /// </summary>
    [Serializable]
    public enum LogoAlignment
    {
        /// <summary>
        /// Left align the logo
        /// </summary>
        Left,
        /// <summary>
        /// Right align the logo
        /// </summary>
        Right,
        /// <summary>
        /// Center the logo
        /// </summary>
        Center
    }
}
