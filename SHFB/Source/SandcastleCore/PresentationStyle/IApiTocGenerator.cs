//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : IApiTocGenerator.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/05/2021
// Note    : Copyright 2021, Eric Woodruff, All rights reserved
//
// This file contains an interface used to implement a table of contents generator for API content
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/05/2021  EFW  Created the code
//===============================================================================================================

using System.Collections.Generic;

using Sandcastle.Core.Reflection;

namespace Sandcastle.Core.PresentationStyle
{
    /// <summary>
    /// This interface is used to implement a table of contents generator for API content
    /// </summary>
    public interface IApiTocGenerator
    {
        /// <summary>
        /// This is used to get or set the list topic order in the table of contents
        /// </summary>
        IEnumerable<ApiMemberGroup> ListTopicOrder { get; set; }

        /// <summary>
        /// This is used to generate a table of contents file for API content
        /// </summary>
        /// <param name="reflectionDataFile">The source reflection data file</param>
        /// <param name="tocFile">The table of contents file to generate</param>
        void GenerateApiTocFile(string reflectionDataFile, string tocFile);
    }
}
