//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : SandcastleHtmlExtract.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/13/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains the MSBuild task implementation for extracting HTML info from help topics
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

using System;
using System.Collections.Generic;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SandcastleBuilder.HtmlExtract
{
    /// <summary>
    /// This is the MSBuild task used to extract title and keyword information from HTML files for use in
    /// creating the CHM table of contents and keyword index files.
    /// </summary>
    public class SandcastleHtmlExtract : Task
    {
        #region MSBuild task interface
        //=====================================================================

        /// <summary>
        /// This is used to set the project name
        /// </summary>
        [Required]
        public string ProjectName { get; set; }

        /// <summary>
        /// This is used to set the language ID (LCID)
        /// </summary>
        /// <value>This is optional.  If not set, it defaults to 1033.</value>
        public int LanguageId { get; set; }

        /// <summary>
        /// This is used to set the HTML Help 1 file folder name containing the Help 1 files to be processed.
        /// </summary>
        /// <value>This is optional.  If not set, no HTML help 1 files will be processed.</value>
        public string Help1Folder { get; set; }

        /// <summary>
        /// This is used to set the website file folder name containing the website files to be processed.
        /// </summary>
        /// <value>This is optional.  If not set, no HTML help 1 files will be processed.</value>
        public string WebsiteFolder { get; set; }

        /// <summary>
        /// This is used to set the localized output folder name
        /// </summary>
        /// <value>This is optional.  If not set, the HTML files will not be localized.</value>
        public string LocalizedFolder { get; set; }

        /// <summary>
        /// This is used to set the general output folder name
        /// </summary>
        /// <value>This is optional.  If not set, it defaults to the current working folder.</value>
        public string OutputFolder { get; set; }

        /// <summary>
        /// This is used to set the table of contents XML filename
        /// </summary>
        /// <value>This is optional.  If not set, it defaults to <b>toc.xml</b>.</value>
        public string TocFile { get; set; }

        /// <summary>
        /// This is used to get or set the maximum degree of parallelism used to process the HTML files
        /// </summary>
        /// <value>If not set, it defaults to a maximum of 20 threads per processor.  Increase or decrease this
        /// value as needed based on your system.  Setting it to a value less than 1 will allow for an unlimited
        /// number of threads.  However, this is a largely IO-bound process so allowing an excessive number of
        /// threads may slow overall system performance on very large help files.</value>
        public int MaxDegreeOfParallelism { get; set; }

        /// <summary>
        /// This is used to execute the task and process the HTML files
        /// </summary>
        /// <returns>True on success or false on failure.</returns>
        public override bool Execute()
        {
            List<string> parameters = new List<string>();

            if(!String.IsNullOrWhiteSpace(this.Help1Folder))
                parameters.Add("-help1Folder=" + this.Help1Folder);

            if(!String.IsNullOrWhiteSpace(this.WebsiteFolder))
                parameters.Add("-websiteFolder=" + this.WebsiteFolder);

            if(!String.IsNullOrWhiteSpace(this.OutputFolder))
                parameters.Add("-outputFolder=" + this.OutputFolder);

            if(!String.IsNullOrWhiteSpace(this.LocalizedFolder))
                parameters.Add("-localizedFolder=" + this.LocalizedFolder);

            if(this.LanguageId > 0)
                parameters.Add($"-lcid={this.LanguageId}");

            if(!String.IsNullOrWhiteSpace(this.ProjectName))
                parameters.Add("-projectName=" + this.ProjectName);

            if(!String.IsNullOrWhiteSpace(this.TocFile))
                parameters.Add("-tocFile=" + this.TocFile);

            if(this.MaxDegreeOfParallelism != 0)
                parameters.Add($"-maxDegreeOfParallelism={this.MaxDegreeOfParallelism}");

            SandcastleHtmlExtractCore.IsMSBuildTask = true;

            return SandcastleHtmlExtractCore.MainEntryPoint(parameters.ToArray()) == 0;
        }
        #endregion
    }
}

