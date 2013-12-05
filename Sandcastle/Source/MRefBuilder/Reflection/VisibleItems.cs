//===============================================================================================================
// System  : Sandcastle Tools - MRefBuilder
// File    : VisibleItems.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/27/2013
// Note    : Copyright 2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the enumerated type that defines the optional visible items to include in the output
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.   This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 2.7.5.0  11/27/2013  EFW  Merged visibility settings from SHFB into MRefBuilder
//===============================================================================================================

using System;

namespace Microsoft.Ddue.Tools.Reflection
{
    /// <summary>
    /// This public enumerated type defines the optional visible items to include in the output
    /// </summary>
    [Flags, Serializable]
    public enum VisibleItems
    {
        /// <summary>None of the optional items are included.  Only public types and members will be included.</summary>
        None                              = 0x0000,
        /// <summary>Include attributes.</summary>
        Attributes                        = 0x0001,
        /// <summary>Include explicit interface implementations.</summary>
        ExplicitInterfaceImplementations  = 0x0002,
        /// <summary>Include inherited members.</summary>
        InheritedMembers                  = 0x0004,
        /// <summary>Include inherited framework members.  For this to work, <c>InheritedMembers</c> must also
        /// be enabled.</summary>
        InheritedFrameworkMembers         = 0x0008,
        /// <summary>Include inherited internal framework members.  For this to work <c>InheritedFrameworkMembers</c>
        /// must also be enabled.</summary>
        InheritedFrameworkInternalMembers = 0x0010,
        /// <summary>Include inherited private framework members.  For this to work <c>InheritedFrameworkMembers</c>
        /// must also be enabled.</summary>
        InheritedFrameworkPrivateMembers  = 0x0020,
        /// <summary>Include internal members.</summary>
        Internals                         = 0x0040,
        /// <summary>Include private members.</summary>
        Privates                          = 0x0080,
        /// <summary>Include private fields.  For this to work, <c>Privates</c> must also be enabled.</summary>
        PrivateFields                     = 0x0100,
        /// <summary>Include protected members.</summary>
        Protected                         = 0x0200,
        /// <summary>Include protected members of sealed classes.  For this to work, <c>Protected</c> must also
        /// be enabled.</summary>
        SealedProtected                   = 0x0400,
        /// <summary>Include "protected internal" members as "protected" only.</summary>
        ProtectedInternalAsProtected      = 0x0800,
        /// <summary>Include no-PIA (Primary Interop Assembly) embedded COM types.</summary>
        NoPIATypes                        = 0x1000
    }
}
