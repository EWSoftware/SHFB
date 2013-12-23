//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ISyntaxGeneratorFactory.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/17/2013
// Note    : Copyright 2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a custom version of the ExportAttribute that contains metadata for the BuildAssembler
// syntax generators.
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

namespace Sandcastle.Core.BuildAssembler.SyntaxGenerator
{
    /// <summary>
    /// This is a custom version of the <see cref="ExportAttribute"/> that contains metadata for the
    /// BuildAssembler syntax generators.
    /// </summary>
    [MetadataAttribute, AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class SyntaxGeneratorExportAttribute : ExportAttribute
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property is used to get the syntax generator ID
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// This read-only property is used to get the prefix for resource items used during XSL transformation
        /// such as for label text.
        /// </summary>
        public string ResourceItemPrefix { get; private set; }

        /// <summary>
        /// This read-only property is used to get the style name parameter used by the client side script in the
        /// topics.
        /// </summary>
        public string StyleNameParameter { get; private set; }

        /// <summary>
        /// This is used to get or set whether or not the syntax generator is configurable
        /// </summary>
        /// <value>TODO: Configuration implementation to be determined.</value>
        public bool IsConfigurable { get; set; }

        /// <summary>
        /// This is used to get or set a brief description of the syntax generator
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// This is used to get or set an additional user-defined copyright information separate from the
        /// assembly copyright information.
        /// </summary>
        public string AdditionalCopyrightInfo { get; set; }

        /// <summary>
        /// This read-only property returns the version of the syntax generator
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
        /// This read-only property returns the copyright information for the syntax generator
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

        /// <summary>
        /// This is used to get or set the the value that defines the order in which the syntax generators are
        /// added the to the configuration file.
        /// </summary>
        /// <value>The sort order determines the order of the syntax sections in each topic</value>
        public int SortOrder { get; set; }

        /// <summary>
        /// This is used to get or set a comma-separated list of alternate language IDs that can be used by
        /// designers to translate an alternate ID to the primary <see cref="Id"/>
        /// </summary>
        public string AlternateIds { get; set; }

        /// <summary>
        /// This is used to get or set a string containing an XML fragment that defines the default syntax
        /// generator configuration if supported.
        /// </summary>
        /// <value>Designers can use this value as a default if they provide configuration editing support</value>
        public string DefaultConfiguration { get; set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The required plug-in ID</param>
        /// <param name="resourceItemPrefix">The resource item prefix</param>
        /// <param name="styleNameParameter">The style parameter name for client side script</param>
        public SyntaxGeneratorExportAttribute(string id, string resourceItemPrefix, string styleNameParameter) :
            base(typeof(ISyntaxGeneratorFactory))
        {
            if(String.IsNullOrWhiteSpace(id))
                throw new ArgumentException("An ID value is required", "id");

            if(String.IsNullOrWhiteSpace(resourceItemPrefix))
                throw new ArgumentException("A resource item prefix value is required", "resourceItemPrefix");

            if(String.IsNullOrWhiteSpace(styleNameParameter))
                throw new ArgumentException("A style parameter name value is required", "styleNameParameter");

            this.Id = id;
            this.ResourceItemPrefix = resourceItemPrefix;
            this.StyleNameParameter = styleNameParameter;
        }
        #endregion
    }
}
