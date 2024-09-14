//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : NamingMethod.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/14/2024
// Note    : Copyright 2006-2024, Eric Woodruff, All rights reserved
//
// This file contains the enumerated type that defines the naming method to use for the help topic filenames
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/03/2006  EFW  Created the code
//===============================================================================================================

using System;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This public enumerated type defines the naming method to use for the help topic filenames
    /// </summary>
    [Serializable]
    public enum NamingMethod
    {
        /// <summary>Use the default GUID file naming method (actually an MD5 hash of the member ID in GUID
        /// form).</summary>
        Guid,
        /// <summary>Use the member name without parameters as the filename.  Characters in the set
        /// " :.`@#{}&lt;&gt;*?|" are replaced with an underscore (i.e. T:CustomType becomes T_CustomType,
        /// M:CustomType.#ctor becomes M_CustomType__ctor, P:CustomType.Property becomes P:CustomType_Property,
        /// etc).  Duplicate names will have an incrementing value appended to the end of the name (i.e.
        /// M_CustomType_Method, M_CustomType.Method_1, M_CustomType_Method_2, etc).</summary>
        MemberName,
        /// <summary>Use the hashed member name without parameters as the filename.  No character replacements
        /// are made for this option and the <b>GetHashCode</b> method is used to generate the hash value and it
        /// is formatted as a hex value.  This is useful for extremely long type names that cause the filename
        /// to exceed the maximum length when the full path is included.  Duplicate names will have an
        /// incrementing value appended to the name prior to creating the hash value as needed.</summary>
        HashedMemberName
    }
}
