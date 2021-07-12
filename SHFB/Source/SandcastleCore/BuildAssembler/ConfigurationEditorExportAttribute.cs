//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ConfigurationEditorExportAttribute.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/31/2021
// Note    : Copyright 2021, Eric Woodruff, All rights reserved
//
// This file contains an export attribute used to mark classes as build component configuration editors
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 04/23/2021  EFW  Created the code
//===============================================================================================================

using System;
using System.ComponentModel.Composition;

namespace Sandcastle.Core.BuildAssembler
{
    /// <summary>
    /// This is a custom version of the <see cref="ExportAttribute"/> that contains metadata for the
    /// BuilderAssembler component configuration editors.
    /// </summary>
    [MetadataAttribute, AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ConfigurationEditorExportAttribute : ExportAttribute
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get the component ID
        /// </summary>
        /// <value>This must match the ID of the build component for which this will provide a configuration
        /// editor.</value>
        public string Id { get; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The required component ID</param>
        public ConfigurationEditorExportAttribute(string id) : base(typeof(IConfigurationEditor))
        {
            if(String.IsNullOrWhiteSpace(id))
                throw new ArgumentException("An ID value is required", nameof(id));

            this.Id = id;
        }
        #endregion
    }
}
