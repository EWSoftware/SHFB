//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : VisibleItems.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/15/2025
// Note    : Copyright 2006-2025, Eric Woodruff, All rights reserved
//
// This file contains the enumerated type that defines the optional visible items in the help file
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/03/2006  EFW  Created the code
// 08/24/2007  EFW  Added support for the inherited private/internal framework member flags
// 12/03/2013  EFW  Added support for no-PIA embedded interop types
// 06/19/2015  EFW  Added support for public compiler generated types/members
// 09/22/2017  EFW  Added support for filtering by EditorBrowsableAttribute and BrowsableAttribute state
// 09/27/2017  EFW  Moved to Sandcastle.Core to share between projects
//===============================================================================================================

using System;

namespace Sandcastle.Core.Project
{
    /// <summary>
    /// This public enumerated type defines the optional visible items to include in the help file
    /// </summary>
    [Flags, Serializable]
    public enum VisibleItems
    {
        /// <summary>None of the optional items are documented.  Only public types and members will be documented.</summary>
        None                              = 0x0000,
        /// <summary>Document attributes.</summary>
        Attributes                        = 0x0001,
        /// <summary>Document explicit interface implementations.</summary>
        ExplicitInterfaceImplementations  = 0x0002,
        /// <summary>Document inherited members.</summary>
        InheritedMembers                  = 0x0004,
        /// <summary>Document inherited framework members.  For this to work, <c>InheritedMembers</c> must also
        /// be enabled.</summary>
        InheritedFrameworkMembers         = 0x0008,
        /// <summary>Document inherited internal framework members.  For this to work <c>InheritedFrameworkMembers</c>
        /// must also be enabled.</summary>
        InheritedFrameworkInternalMembers = 0x0010,
        /// <summary>Document inherited private framework members.  For this to work <c>InheritedFrameworkMembers</c>
        /// must also be enabled.</summary>
        InheritedFrameworkPrivateMembers  = 0x0020,
        /// <summary>Document internal members.</summary>
        Internals                         = 0x0040,
        /// <summary>Document private members.</summary>
        Privates                          = 0x0080,
        /// <summary>Document private fields.  For this to work, <c>Privates</c> must also be enabled.</summary>
        PrivateFields                     = 0x0100,
        /// <summary>Document protected members.</summary>
        Protected                         = 0x0200,
        /// <summary>Document protected members of sealed classes.  For this to work, <c>Protected</c> must also
        /// be enabled.</summary>
        SealedProtected                   = 0x0400,
        /// <summary>Document "protected internal" members as "protected" only.</summary>
        ProtectedInternalAsProtected      = 0x0800,
        /// <summary>Document no-PIA (Primary Interop Assembly) embedded COM types.</summary>
        NoPIATypes                        = 0x1000,
        /// <summary>Include public compiler generated types/members.</summary>
        PublicCompilerGenerated           = 0x2000,
        /// <summary>Document members marked with an <see cref="System.ComponentModel.EditorBrowsableAttribute"/>
        /// set to <c>Never</c>.</summary>
        EditorBrowsableNever              = 0x4000,
        /// <summary>Document members marked with a <see cref="System.ComponentModel.BrowsableAttribute"/>
        /// set to <c>False</c>.</summary>
        NonBrowsable                      = 0x8000,
        /// <summary>Include internal members from other assemblies and private members from base types.  For
        /// this to work, the internal and/or private member options must also be enabled.  Inherited internal
        /// and private framework members are not affected by this as they are controlled by the other related
        /// options.</summary>
        InternalAndPrivateIfExternal      = 0x00010000
    }
}
