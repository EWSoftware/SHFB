//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ISyntaxGeneratorFactory.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/27/2013
// Note    : Copyright 2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that defines the metadata for a BuildAssembler syntax generator component
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

namespace Sandcastle.Core.BuildAssembler.SyntaxGenerator
{
    /// <summary>
    /// This class defines the metadata for a BuildAssembler syntax generator component
    /// </summary>
    public interface ISyntaxGeneratorMetadata
    {
        /// <summary>
        /// This read-only property returns the ID for the syntax generator (typically the language name)
        /// </summary>
        string Id { get; }

        /// <summary>
        /// This read-only property is used to get the value used as the XML element name and in resource item
        /// IDs used during XSL transformation such as for label text.
        /// </summary>
        string LanguageElementName { get; }

        /// <summary>
        /// This read-only property is used to get the keyword style parameter value used by the client side
        /// script in the topics for language-specific keyword text.
        /// </summary>
        string KeywordStyleParameter { get; }

        /// <summary>
        /// This read-only property returns true if the syntax generator is configurable or false if it is not
        /// </summary>
        /// <value>Configuration is handled by the <c>SyntaxComponent</c> build component.</value>
        bool IsConfigurable { get; }

        /// <summary>
        /// This read-only property returns a brief description of the syntax generator
        /// </summary>
        string Description { get; }

        /// <summary>
        /// This read-only property returns the version of the syntax generator
        /// </summary>
        string Version { get; }

        /// <summary>
        /// This read-only property returns the copyright information for the syntax generator
        /// </summary>
        string Copyright { get; }

        /// <summary>
        /// This read-only property returns the value that defines the order in which the syntax generators are
        /// added the to the configuration file.
        /// </summary>
        /// <value>The sort order determines the order of the syntax sections in each topic</value>
        int SortOrder { get; }

        /// <summary>
        /// This read-only property returns a comma-separated list of alternate language IDs that can be used
        /// by designers to translate an alternate ID to the primary <see cref="Id"/>
        /// </summary>
        string AlternateIds { get; }

        /// <summary>
        /// This read-only property returns a string containing an XML fragment that defines the default
        /// syntax generator configuration if supported.
        /// </summary>
        /// <value>Designers can use this value as a default if they provide configuration editing support</value>
        string DefaultConfiguration { get; }
    }
}
