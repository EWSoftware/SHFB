//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : COMReferenceItem.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/17/2007
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing a COM reference item that can be
// used by MRefBuilder to locate assembly dependencies for the assemblies being
// documented.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.1.0.0  08/23/2006  EFW  Created the code
// 1.8.0.0  06/30/2008  EFW  Rewrote to support the MSBuild project format
//=============================================================================

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Xml;

using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This represents a COM reference item that can be used by
    /// <b>MRefBuilder</b> to locate assembly dependencies for the assemblies
    /// being documented.
    /// </summary>
    public class COMReferenceItem : ReferenceItem
    {
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
        /// This is used to get the project reference's GUID
        /// </summary>
        [Category("Metadata"), Description("The COM object's GUID")]
        public string Guid
        {
            get
            {
                return base.ProjectElement.GetMetadata(ProjectElement.Guid);
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
                return base.ProjectElement.GetMetadata(ProjectElement.VersionMajor);
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
                return base.ProjectElement.GetMetadata(ProjectElement.VersionMinor);
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
                return base.ProjectElement.GetMetadata(ProjectElement.WrapperTool);
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Internal Constructor
        /// </summary>
        /// <param name="element">The project element</param>
        internal COMReferenceItem(ProjectElement element) : base(element)
        {
        }
        #endregion
    }
}
