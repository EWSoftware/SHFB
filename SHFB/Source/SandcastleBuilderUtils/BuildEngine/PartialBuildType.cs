//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : PartialBuildType.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/07/2021
// Note    : Copyright 2013-2021, Eric Woodruff, All rights reserved
//
// This file contains the enumerated type that defines the partial build types
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/13/2013  EFW  Created the code
//===============================================================================================================

using System;

namespace SandcastleBuilder.Utils.BuildEngine
{
    /// <summary>
    /// This public enumerated type defines the partial build types
    /// </summary>
    [Serializable]
    public enum PartialBuildType
    {
        /// <summary>The build will run to completion</summary>
        None,
        /// <summary>The build will stop after generating reflection information</summary>
        GenerateReflectionInfo,
        /// <summary>The build will stop after applying the document model, namespace grouping option if
        /// applicable, and adding filenames to each topic.</summary>
        TransformReflectionInfo,
    }
}
