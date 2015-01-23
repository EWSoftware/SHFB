//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : CopyComponentExportAttribute.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/28/2013
// Note    : Copyright 2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a custom version of the ExportAttribute that contains metadata for the BuildAssembler
// copy components.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/27/2013  EFW  Created the code
//===============================================================================================================

using System;
using System.ComponentModel.Composition;

namespace Sandcastle.Core.BuildAssembler.BuildComponent
{
    /// <summary>
    /// This is a custom version of the <see cref="ExportAttribute"/> that contains metadata for the
    /// BuildAssembler copy components.
    /// </summary>
    [MetadataAttribute, AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CopyComponentExportAttribute : ExportAttribute
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get the copy component ID
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// This is used to get or set a brief description of the copy component
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// This is used to get or set the copy component version number
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// This is used to get or set copyright information for the copy component
        /// </summary>
        public string Copyright { get; set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The required copy component ID</param>
        public CopyComponentExportAttribute(string id) : base(typeof(ICopyComponentFactory))
        {
            if(String.IsNullOrWhiteSpace(id))
                throw new ArgumentException("An ID value is required", "id");

            this.Id = id;
        }
        #endregion
    }
}
