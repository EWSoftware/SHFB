//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BuildItemMetadata.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/14/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains a class that holds a set of constants that define build item metadata names
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/13/2013  EFW  Moved the metadata names from ProjectElement into their own class
//===============================================================================================================

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This class holds a set of constants that define build item metadata names
    /// </summary>
    public static class BuildItemMetadata
    {
        /// <summary>Build action</summary>
        public const string BuildAction = "BuildAction";
        /// <summary>Include item</summary>
        public const string IncludePath = "Include";

        /// <summary>File reference hint path</summary>
        public const string HintPath = "HintPath";
        /// <summary>Linked item path</summary>
        public const string LinkPath = "Link";

        /// <summary>Project GUID item</summary>
        public const string ProjectGuid = "Project";
        /// <summary>Project name item</summary>
        public const string Name = "Name";
        /// <summary>Reference output assembly item</summary>
        public const string ReferenceOutputAssembly = "ReferenceOutputAssembly";

        /// <summary>Configuration setting</summary>
        public const string Configuration = "Configuration";
        /// <summary>Platform setting</summary>
        public const string Platform = "Platform";
        /// <summary>Output directory setting</summary>
        public const string OutDir = "OutDir";
        /// <summary>Project name setting</summary>
        public const string ProjectName = "ProjectName";

        /// <summary>Image ID</summary>
        public const string ImageId = "ImageId";
        /// <summary>Alternate text</summary>
        public const string AlternateText = "AlternateText";
        /// <summary>Copy to media folder</summary>
        public const string CopyToMedia = "CopyToMedia";
        /// <summary>Sort order</summary>
        public const string SortOrder = "SortOrder";
    }
}
