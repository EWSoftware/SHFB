//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ISyntaxGeneratorFactory.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/27/2013
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
        /// This read-only property is used to get the syntax generator ID (typically the language name)
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// This read-only property is used to get the value used as the XML element name and in resource item
        /// IDs used during XSL transformation such as for label text.
        /// </summary>
        public string LanguageElementName { get; private set; }

        /// <summary>
        /// This read-only property is used to get the keyword style parameter value used by the client side
        /// script in the topics for language specific keyword/separator text.
        /// </summary>
        /// <value>This will typically be one of the following: <c>cs</c> (C# or equivalent), <c>vb</c> (VB.NET
        /// or equivalent), <c>cpp</c> (C++ or equivalent), <c>fs</c> (F# or equivalent).</value>
        public string KeywordStyleParameter { get; private set; }

        /// <summary>
        /// This is used to get or set whether or not the syntax generator is configurable
        /// </summary>
        /// <value>Configuration is handled by the <c>SyntaxComponent</c> build component.</value>
        public bool IsConfigurable { get; set; }

        /// <summary>
        /// This is used to get or set a brief description of the syntax generator
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// This is used to get or set the syntax generator version number
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// This is used to get or set copyright information for the syntax generator
        /// </summary>
        public string Copyright { get; set; }

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
        /// <param name="languageElementName">The language element name and resource item ID</param>
        /// <param name="keywordStyleParameter">The keyword style parameter value for client side script</param>
        public SyntaxGeneratorExportAttribute(string id, string languageElementName, string keywordStyleParameter) :
          base(typeof(ISyntaxGeneratorFactory))
        {
            if(String.IsNullOrWhiteSpace(id))
                throw new ArgumentException("An ID value is required", "id");

            if(String.IsNullOrWhiteSpace(languageElementName))
                throw new ArgumentException("A language element name value is required", "languageElementName");

            if(String.IsNullOrWhiteSpace(keywordStyleParameter))
                throw new ArgumentException("A keyword style parameter value is required", "keywordStyleParameter");

            this.Id = id;
            this.LanguageElementName = languageElementName;
            this.KeywordStyleParameter = keywordStyleParameter;
        }
        #endregion
    }
}
