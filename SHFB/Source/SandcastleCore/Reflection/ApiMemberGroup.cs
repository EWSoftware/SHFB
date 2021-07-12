//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : ApiMemberGroup.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/12/2021
// Note    : Copyright 2021, Eric Woodruff, All rights reserved
//
// This file contains the enumerated type that defines the groups and subgroups for an API member
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/03/2021  EFW  Created the code
//===============================================================================================================

using System;

namespace Sandcastle.Core.Reflection
{
    /// <summary>
    /// This enumerated type represents the groups and subgroups to which an API member can belong
    /// </summary>
    [Serializable]
    public enum ApiMemberGroup
    {
        /// <summary>
        /// Not specified
        /// </summary>
        None,

        /// <summary>
        /// An unknown value was encountered
        /// </summary>
        Unknown,

        // These are the groups

        /// <summary>
        /// Root namespace container (R:)
        /// </summary>
        /// <remarks>There will only be one of these if present</remarks>
        Root,
        /// <summary>
        /// Root group namespace container (G:)
        /// </summary>
        /// <remarks>There will only be one of these if present</remarks>
        RootGroup,
        /// <summary>
        /// A namespace (N:NamespaceName)
        /// </summary>
        Namespace,
        /// <summary>
        /// A namespace group (G:NamespaceGroupName)
        /// </summary>
        NamespaceGroup,
        /// <summary>
        /// A type
        /// </summary>
        Type,
        /// <summary>
        /// A type member
        /// </summary>
        Member,

        // These are the subgroups

        /// <summary>
        /// A class
        /// </summary>
        Class,
        /// <summary>
        /// A structure
        /// </summary>
        Structure,
        /// <summary>
        /// An interface
        /// </summary>
        Interface,
        /// <summary>
        /// An enumeration
        /// </summary>
        Enumeration,
        /// <summary>
        /// Constructor
        /// </summary>
        Constructor,
        /// <summary>
        /// A property
        /// </summary>
        Property,
        /// <summary>
        /// A method
        /// </summary>
        Method,
        /// <summary>
        /// An event
        /// </summary>
        Event,
        /// <summary>
        /// A delegate
        /// </summary>
        Delegate,
        /// <summary>
        /// A field
        /// </summary>
        Field,

        // These are sub-subgroups

        /// <summary>
        /// An operator
        /// </summary>
        Operator,
        /// <summary>
        /// An extension method
        /// </summary>
        Extension,
        /// <summary>
        /// An attached property
        /// </summary>
        AttachedProperty,
        /// <summary>
        /// An attached event
        /// </summary>
        AttachedEvent,

        // These are topic data groups

        /// <summary>
        /// A member list topic
        /// </summary>
        List,
        /// <summary>
        /// An API member
        /// </summary>
        Api,
        /// <summary>
        /// A properties list topic
        /// </summary>
        Properties,
        /// <summary>
        /// A methods list topic
        /// </summary>
        Methods,
        /// <summary>
        /// An overloads list topic
        /// </summary>
        Overload,
        /// <summary>
        /// An operators list topic
        /// </summary>
        Operators,
        /// <summary>
        /// An events list topic
        /// </summary>
        Events,
        /// <summary>
        /// A fields list topic
        /// </summary>
        Fields,
        /// <summary>
        /// An attached properties list topic
        /// </summary>
        AttachedProperties,
        /// <summary>
        /// An attached events list topic
        /// </summary>
        AttachedEvents
    }
}
