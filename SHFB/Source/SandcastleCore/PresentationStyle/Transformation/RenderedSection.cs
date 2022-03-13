//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : RenderedSectionEventArgs.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/11/2022
// Note    : Copyright 2022, Eric Woodruff, All rights reserved
//
// This file contains an enumerated type that defines the sections that are rendered in an API topic
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/11/2022  EFW  Created the code
//===============================================================================================================

using System;

namespace Sandcastle.Core.PresentationStyle.Transformation
{
    /// <summary>
    /// This enumerated type defines the sections that are rendered in an API topic
    /// </summary>
    /// <remarks>Section order may vary from one presentation style to another</remarks>
    [Serializable]
    public enum RenderedSection
    {
        /// <summary>
        /// The "preliminary", "obsolete", and other such messages that are displayed at the top of the topic
        /// </summary>
        HeaderNotes,
        /// <summary>
        /// The summary section
        /// </summary>
        Summary,
        /// <summary>
        /// The abbreviated inheritance hierarchy
        /// </summary>
        InheritanceHierarchyAbbreviated,
        /// <summary>
        /// Namespace and assembly information
        /// </summary>
        NamespaceAndAssemblyInfo,
        /// <summary>
        /// The syntax section
        /// </summary>
        SyntaxSection,
        /// <summary>
        /// A member list based on the topic type (root, root group, namespace group, namespace, enumeration,
        /// type, or members).
        /// </summary>
        MemberList,
        /// <summary>
        /// The events section
        /// </summary>
        Events,
        /// <summary>
        /// The exceptions section
        /// </summary>
        Exceptions,
        /// <summary>
        /// The remarks section
        /// </summary>
        Remarks,
        /// <summary>
        /// The examples section
        /// </summary>
        Examples,
        /// <summary>
        /// The platforms section (reserved for future use)
        /// </summary>
        Platforms,  // TODO: Remove reserved note when implemented
        /// <summary>
        /// The versions section
        /// </summary>
        Versions,
        /// <summary>
        /// The permissions section
        /// </summary>
        Permissions,
        /// <summary>
        /// The thread safety section
        /// </summary>
        ThreadSafety,
        /// <summary>
        /// The revision history section
        /// </summary>
        RevisionHistory,
        /// <summary>
        /// The bibliography section
        /// </summary>
        Bibliography,
        /// <summary>
        /// The See Also section
        /// </summary>
        SeeAlso,
        /// <summary>
        /// The full inheritance hierarchy
        /// </summary>
        InheritanceHierarchyFull,
        /// <summary>
        /// A custom section
        /// </summary>
        CustomSection
    }
}
