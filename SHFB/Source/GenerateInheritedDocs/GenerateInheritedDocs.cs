//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : GenerateInheritedDocs.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/13/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains the MSBuild task implementation for generating inherited documentation
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/13/2021  EFW  Split task out into a separate class to allow for dynamic loading of MSBuild assemblies
//===============================================================================================================

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SandcastleBuilder.InheritedDocumentation
{
    /// <summary>
    /// This class represents the tool that scans XML comments files for <b>&lt;inheritdoc /&gt;</b> tags and
    /// produces a new XML comments file containing the inherited documentation for use by Sandcastle.
    /// </summary>
    public class GenerateInheritedDocs : Task
    {
        #region MSBuild task interface
        //=====================================================================

        /// <summary>
        /// This is used to set the configuration file to use from the MSBuild project file
        /// </summary>
        [Required]
        public string ConfigurationFile { get; set; }

        /// <summary>
        /// This is used to execute the task and generate the inherited documentation
        /// </summary>
        /// <returns>True on success or false on failure.</returns>
        public override bool Execute()
        {
            return (GenerateInheritedDocsCore.MainEntryPoint(new string[] { this.ConfigurationFile }) == 0);
        }
        #endregion
    }
}
