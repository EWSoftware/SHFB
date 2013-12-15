//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : PartialBuildType.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/13/2013
// Note    : Copyright 2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the enumerated type that defines the partial build types
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.9.0  12/13/2013  EFW  Created the code
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
        /// <summary>The build will stop after applying the document model XSL transformation</summary>
        TransformReflectionInfo,
    }
}
