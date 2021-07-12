//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : HelpFileBuilderPlugInExportAttribute.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/23/2021
// Note    : Copyright 2013-2021, Eric Woodruff, All rights reserved
//
// This file contains a plug-in export attribute used to mark classes as Sandcastle Help File Builder build
// process plug-ins and define their metadata.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/17/2013  EFW  Created the code
//===============================================================================================================

using System;
using System.ComponentModel.Composition;

namespace SandcastleBuilder.Utils.BuildComponent
{
    /// <summary>
    /// This is a custom version of the <see cref="ExportAttribute"/> that contains metadata for the help file
    /// builder plug-ins.
    /// </summary>
    [MetadataAttribute, AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class HelpFileBuilderPlugInExportAttribute : ExportAttribute
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get the plug-in ID
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// This is used to indicate whether or not the plug-in is hidden in the project plug-in property page
        /// </summary>
        /// <value>If set to true, the plug-in is hidden in the project plug-in property page and cannot
        /// be added to the project.  If false, the default, it can be added to projects.  This is useful for
        /// presentation style dependency plug-ins that have no configurable elements and thus do not need to be
        /// manually added to the project to override settings.</value>
        public bool IsHidden { get; set; }

        /// <summary>
        /// This is used to get or set whether or not the plug-in runs in partial builds
        /// </summary>
        /// <value>If not specified, the default is false</value>
        public bool RunsInPartialBuild { get; set; }

        /// <summary>
        /// This is used to get or set a brief description of the plug-in
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// This is used to get or set the plug-in version number
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// This is used to get or set copyright information for the plug-in
        /// </summary>
        public string Copyright { get; set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The required plug-in ID</param>
        public HelpFileBuilderPlugInExportAttribute(string id) : base(typeof(IPlugIn))
        {
            if(String.IsNullOrWhiteSpace(id))
                throw new ArgumentException("An ID value is required", nameof(id));

            this.Id = id;
        }
        #endregion
    }
}
