//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : BuildComponentExportAttribute.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/23/2014
// Note    : Copyright 2013-2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a custom version of the ExportAttribute that contains metadata for the BuildAssembler
// build components.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/23/2013  EFW  Created the code
//===============================================================================================================

using System;
using System.ComponentModel.Composition;

namespace Sandcastle.Core.BuildAssembler.BuildComponent
{
    /// <summary>
    /// This is a custom version of the <see cref="ExportAttribute"/> that contains metadata for the
    /// BuildAssembler build components.
    /// </summary>
    [MetadataAttribute, AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class BuildComponentExportAttribute : ExportAttribute
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get the build component ID
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// This is used to get or set whether or not the component is visible to configuration tools
        /// </summary>
        /// <remarks>Configuration tools can use this to exclude components that should not appear for the user
        /// to select.  It is false by default.</remarks>
        public bool IsVisible { get; set; }

        /// <summary>
        /// This is used to get or set whether or not the build component is configurable
        /// </summary>
        /// <value>If this returns true, the <see cref="BuildComponentFactory.ConfigureComponent"/> method can be
        /// called to allow the user to configure the build component's settings when requested.  The default is
        /// false.</value>
        public bool IsConfigurable { get; set; }

        /// <summary>
        /// This is used to get or set a brief description of the build component
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// This is used to get or set the build component version number
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// This is used to get or set copyright information for the build component
        /// </summary>
        public string Copyright { get; set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The required build component ID</param>
        public BuildComponentExportAttribute(string id) : base(typeof(BuildComponentFactory))
        {
            if(String.IsNullOrWhiteSpace(id))
                throw new ArgumentException("An ID value is required", "id");

            this.Id = id;
        }
        #endregion
    }
}
