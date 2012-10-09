//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : ApiEntryType.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/22/2007
// Note    : Copyright 2007, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains an enumerated type that defines the API entry types.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.0.2  07/16/2007  EFW  Created the code
//=============================================================================

using System;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This public enumerated type defines the API entry types that can be
    /// removed from the reflection information file.
    /// </summary>
    [Serializable]
    public enum ApiEntryType
    {
        /// <summary>An unknown entry</summary>
        None,
        /// <summary>A namespace</summary>
        Namespace,
        /// <summary>A class</summary>
        Class,
        /// <summary>An interface</summary>
        Structure,
        /// <summary>An interface</summary>
        Interface,
        /// <summary>An enumeration</summary>
        Enumeration,
        /// <summary>A delegate</summary>
        Delegate,
        /// <summary>A constructor</summary>
        Constructor,
        /// <summary>A method</summary>
        Method,
        /// <summary>An operator</summary>
        Operator,
        /// <summary>A property</summary>
        Property,
        /// <summary>An event</summary>
        Event,
        /// <summary>A field</summary>
        Field
    }
}
