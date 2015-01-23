//===============================================================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : NumberingStyle.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/16/2014
// Note    : Copyright 2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to track custom numbering style details
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/16/2014  EFW  Created the code
//===============================================================================================================

namespace SandcastleBuilder.Utils.MSBuild
{
    /// <summary>
    /// This is used to track the custom numbering styles found during the step that converts the list elements
    /// to Open XML lists.
    /// </summary>
    public class NumberingStyle
    {
        /// <summary>
        /// The ID for the numbering style
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The numbering style
        /// </summary>
        public string Style { get; set; }

        /// <summary>
        /// The numbering level to override
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// The starting number for ordered lists
        /// </summary>
        public int Start { get; set; }
    }
}
