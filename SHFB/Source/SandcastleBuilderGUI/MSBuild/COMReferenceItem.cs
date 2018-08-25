//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : COMReferenceItem.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/13/2015
// Note    : Copyright 2008-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing a COM reference item that can be used by MRefBuilder to locate
// assembly dependencies for the assemblies being documented.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 08/23/2006  EFW  Created the code
// 06/30/2008  EFW  Rewrote to support the MSBuild project format
// 05/13/2015  EFW  Moved the file to the GUI project as it is only used there
//===============================================================================================================

// Ignore Spelling: Guid

using System.ComponentModel;

using Microsoft.Build.Evaluation;

using SandcastleBuilder.Utils;

namespace SandcastleBuilder.Gui.MSBuild
{
    /// <summary>
    /// This represents a COM reference item that can be used by <strong>MRefBuilder</strong> to locate assembly
    /// dependencies for the assemblies being documented.
    /// </summary>
    public class COMReferenceItem : ReferenceItem
    {
        #region Metadata name constants
        //=====================================================================

        /// <summary>COM object GUID</summary>
        public const string GuidMetadata = "Guid";
        /// <summary>COM object major version</summary>
        public const string VersionMajorMetadata = "VersionMajor";
        /// <summary>COM object minor version</summary>
        public const string VersionMinorMetadata = "VersionMinor";
        /// <summary>COM object wrapper tool</summary>
        public const string WrapperToolMetadata = "WrapperTool";

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// Hint path isn't applicable to COM references
        /// </summary>
        [Browsable(false)]
        public override FilePath HintPath
        {
            get { return base.HintPath; }
            set { }
        }

        /// <summary>
        /// This is used to get the COM reference's GUID
        /// </summary>
        [Category("Metadata"), Description("The COM object's GUID")]
        public string Guid
        {
            get
            {
                return this.GetMetadata(GuidMetadata);
            }
        }

        /// <summary>
        /// This is used to get the major version number
        /// </summary>
        [Category("Metadata"), Description("The major version number")]
        public string VersionMajor
        {
            get
            {
                return this.GetMetadata(VersionMajorMetadata);
            }
        }

        /// <summary>
        /// This is used to get the minor version number
        /// </summary>
        [Category("Metadata"), Description("The minor version number")]
        public string VersionMinor
        {
            get
            {
                return this.GetMetadata(VersionMinorMetadata);
            }
        }

        /// <summary>
        /// This is used to get the wrapper tool
        /// </summary>
        [Category("Metadata"), Description("The wrapper tool")]
        public string WrapperTool
        {
            get
            {
                return this.GetMetadata(WrapperToolMetadata);
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// This constructor is used to wrap an existing reference
        /// </summary>
        /// <param name="project">The project that owns the reference</param>
        /// <param name="existingItem">The existing reference</param>
        internal COMReferenceItem(SandcastleProject project, ProjectItem existingItem) : base(project, existingItem)
        {
        }
        #endregion
    }
}
