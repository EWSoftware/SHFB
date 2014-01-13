//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : IPresentationStyleMetadata.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/04/2014
// Note    : Copyright 2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a presentation style metadata interface definition used to implement a presentation style
// plug-in.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/04/2014  EFW  Created the code
//===============================================================================================================

namespace Sandcastle.Core.PresentationStyle
{
    /// <summary>
    /// This class defines the metadata for a presentation style plug-in
    /// </summary>
    public interface IPresentationStyleMetadata
    {
        /// <summary>
        /// This read-only property returns the ID for the presentation style
        /// </summary>
        string Id { get; }

        /// <summary>
        /// This read-only property returns the title for the presentation style
        /// </summary>
        string Title { get; }

        /// <summary>
        /// This read-only property returns a brief description of the presentation style
        /// </summary>
        string Description { get; }

        /// <summary>
        /// This read-only property returns the version of the presentation style
        /// </summary>
        string Version { get; }

        /// <summary>
        /// This read-only property returns the copyright information for the presentation style
        /// </summary>
        string Copyright { get; }
    }
}
