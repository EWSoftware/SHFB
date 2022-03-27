//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ApiTopicSectionType.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/26/2022
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
    /// <remarks>Section order may vary from one presentation style to another and may be customized by plug-ins</remarks>
    [Serializable]
    public enum ApiTopicSectionType
    {
        /// <summary>
        /// The notices such as the preliminary API and obsolete warnings
        /// </summary>
        /// <remarks>This is typically rendered at the very top of the topic.</remarks>
        Notices,
        /// <summary>
        /// The summary section.
        /// </summary>
        /// <remarks>For overload topics, the summary will come from the first <c>overloads</c> element.</remarks>
        Summary,
        /// <summary>
        /// The abbreviated inheritance hierarchy.
        /// </summary>
        /// <remarks>  This is typically used in conjunction with <see cref="InheritanceHierarchyFull" /> with
        /// the abbreviated hierarchy appearing at the top of the topic and the full hierarchy appearing at the
        /// end of the topic if needed if the descendant count exceeds the abbreviated threshold (four
        /// descendants).  In such cases, the abbreviated hierarchy will have a "More..." link to it.</remarks>
        InheritanceHierarchyAbbreviated,
        /// <summary>
        /// Namespace and assembly information.
        /// </summary>
        /// <remarks>Only API member pages get namespace and assembly info.</remarks>
        NamespaceAndAssemblyInfo,
        /// <summary>
        /// The syntax section.
        /// </summary>
        /// <remarks>Only API member pages get a syntax section.  This includes the declaration syntax,
        /// parameters, templates, return value, and implemented member information.</remarks>
        SyntaxSection,
        /// <summary>
        /// A member list based on the topic type (root, root group, namespace group, namespace, enumeration,
        /// type, or type members).
        /// </summary>
        MemberList,
        /// <summary>
        /// The events section.
        /// </summary>
        Events,
        /// <summary>
        /// The exceptions section.
        /// </summary>
        Exceptions,
        /// <summary>
        /// The remarks section.
        /// </summary>
        /// <remarks>For overload topics, the remarks will come from the first <c>overloads</c> element.</remarks>
        Remarks,
        /// <summary>
        /// The examples section.
        /// </summary>
        /// <remarks>For overload topics, the examples will come from the first <c>overloads</c> element.</remarks>
        Examples,
        /// <summary>
        /// The platforms section (reserved for future use).
        /// </summary>
        Platforms,  // TODO: Remove reserved note when implemented
        /// <summary>
        /// The versions section.
        /// </summary>
        /// <remarks>Only API member pages get version information.</remarks>
        Versions,
        /// <summary>
        /// The permissions section.
        /// </summary>
        Permissions,
        /// <summary>
        /// The thread safety section.
        /// </summary>
        ThreadSafety,
        /// <summary>
        /// The revision history section.
        /// </summary>
        RevisionHistory,
        /// <summary>
        /// The bibliography section.
        /// </summary>
        Bibliography,
        /// <summary>
        /// The See Also section.
        /// </summary>
        SeeAlso,
        /// <summary>
        /// The full inheritance hierarchy.
        /// </summary>
        InheritanceHierarchyFull,
        /// <summary>
        /// A custom section.
        /// </summary>
        /// <remarks>This can be used by plug-ins to add new sections to an API topic.</remarks>
        CustomSection
    }
}
