//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : HelpFileBuilderPlugInExportAttribute.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/17/2013
// Note    : Copyright 2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a plug-in export attribute used to mark classes as Sandcastle Help File Builder build
// process plug-ins and define their metadata.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/17/2013  EFW  Created the code
//===============================================================================================================

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;

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
        public string Id { get; private set; }

        /// <summary>
        /// This is used to get or set whether or not the plug-in is configurable
        /// </summary>
        /// <value>If not specified, the default is false</value>
        public bool IsConfigurable { get; set; }

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
        /// This is used to get or set an additional user-defined copyright information separate from the
        /// assembly copyright information.
        /// </summary>
        public string AdditionalCopyrightInfo { get; set; }

        /// <summary>
        /// This read-only property returns the version of the plug-in
        /// </summary>
        /// <value>This returns the product version value from the assembly attributes or "0.0.0.0" if one does
        /// not exist.</value>
        public string Version
        {
            get
            {
                // Use the assembly version
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(base.ContractType.Assembly.Location);

                return fvi.ProductVersion;
            }
        }

        /// <summary>
        /// This read-only property returns the copyright information for the plug-in
        /// </summary>
        /// <value>This returns the legal copyright value from the assembly attributes or null if one does not
        /// exist.</value>
        public string Copyright
        {
            get
            {
                // Use the assembly version
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(base.ContractType.Assembly.Location);

                return fvi.LegalCopyright;
            }
        }
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
                throw new ArgumentException("An ID value is required", "id");

            this.Id = id;
        }
        #endregion
    }
}