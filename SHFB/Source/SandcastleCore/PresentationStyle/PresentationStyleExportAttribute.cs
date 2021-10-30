//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : PresentationStyleExportAttribute.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/06/2021
// Note    : Copyright 2014-2021, Eric Woodruff, All rights reserved
//
// This file contains a presentation style export attribute used to mark classes as presentation style plug-ins
// and define their metadata.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/04/2014  EFW  Created the code
//===============================================================================================================

using System;
using System.ComponentModel.Composition;

namespace Sandcastle.Core.PresentationStyle
{
    /// <summary>
    /// This is a custom version of the <see cref="ExportAttribute"/> that contains metadata for presentation
    /// style plug-ins.
    /// </summary>
    [MetadataAttribute, AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PresentationStyleExportAttribute : ExportAttribute
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get the presentation style ID
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// This read-only property is used to get the presentation style title
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// This is used to get or set a brief description of the presentation style
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// This is used to get or set the presentation style version number
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// This is used to get or set copyright information for the presentation style
        /// </summary>
        public string Copyright { get; set; }

        /// <summary>
        /// This read-only property is used to indicate that the presentation style has been deprecated
        /// </summary>
        public bool IsDeprecated { get; set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The required presentation style ID</param>
        /// <param name="title">The required presentation style title</param>
        public PresentationStyleExportAttribute(string id, string title) : base(typeof(PresentationStyleSettings))
        {
            if(String.IsNullOrWhiteSpace(id))
                throw new ArgumentException("An ID value is required", nameof(id));

            if(String.IsNullOrWhiteSpace(title))
                throw new ArgumentException("A title value is required", nameof(title));

            this.Id = id;
            this.Title = title;
        }
        #endregion
    }
}
