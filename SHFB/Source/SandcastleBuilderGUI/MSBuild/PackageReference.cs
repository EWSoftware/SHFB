//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : PackageReferenceItem.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/22/2025
// Note    : Copyright 2021-2025, Eric Woodruff, All rights reserved
//
// This file contains a class representing a package reference item used by the build engine to location
// additional components for the build.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/20/2021  EFW  Created the code
//===============================================================================================================

using System.ComponentModel;

using Microsoft.Build.Evaluation;

using Sandcastle.Core;

using SandcastleBuilder.MSBuild.HelpProject;

namespace SandcastleBuilder.Gui.MSBuild
{
    /// <summary>
    /// This represents a package reference item used by the build engine to location additional components for
    /// the build.
    /// </summary>
    public class PackageReferenceItem : ReferenceItem
    {
        #region Metadata name constants
        //=====================================================================

        /// <summary>Item name</summary>
        public const string PackageReferenceItemType = "PackageReference";
        /// <summary>Version</summary>
        public const string VersionMetadata = "Version";

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// Hint path isn't applicable to COM references
        /// </summary>
        [Browsable(false)]
        public override FilePath HintPath
        {
            get => base.HintPath;
            set { }
        }

        /// <summary>
        /// This is used to get the version number
        /// </summary>
        [Category("Metadata"), Description("The version number")]
        public string Version => this.GetMetadata(VersionMetadata);

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// This constructor is used to wrap an existing reference
        /// </summary>
        /// <param name="project">The project that owns the reference</param>
        /// <param name="existingItem">The existing reference</param>
        internal PackageReferenceItem(SandcastleProject project, ProjectItem existingItem) : base(project, existingItem)
        {
        }
        #endregion
    }
}
