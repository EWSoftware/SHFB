//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : NamingMethod.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/01/2021
// Note    : Copyright 2006-2021, Eric Woodruff, All rights reserved
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
        /// <summary>Use the member name without parameters as the filename.  The characters ":", ".", "#", and
        /// "`" in the name are replaced with an underscore (i.e. T:CustomType becomes T_CustomType,
        /// M:CustomType.#ctor becomes M_CustomType__ctor, P:CustomType.Property becomes P:CustomType_Property,
        /// etc).  Duplicate names will have an incrementing value appended to the end of the name (i.e.
        /// M_CustomType_Method, M_CustomType.Method_1, M_CustomType_Method_2, etc).</summary>
        MemberName,
        /// <summary>Use the hashed member name without parameters as the filename.  No character replacements
        /// are made for this option and the <b>GetHashCode</b> method is used to generate the hash value and it
        /// is formatted as a hex value.  This is useful for extremely long type names that cause the filename
        /// to exceed the maximum length when the full path is included.  Duplicate names will have an
        /// incrementing value appended to the name prior to creating the hash value as needed.</summary>
        HashedMemberName /*,
        TODO: Add back if implemented
        /// <summary>This is the same as the GUID format but has the first two characters added to the front as a
        /// folder to reduce the overall number of topics in a single folder.  This is useful for projects with a
        /// large number of topics to keep the number of files per folder more manageable.
        /// </summary>
        GuidFolder*/
    }
}
