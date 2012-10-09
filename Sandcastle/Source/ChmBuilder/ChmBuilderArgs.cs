// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 03/11/2012 - Updated code to fix FxCop warnings

using System;
using System.IO;
using System.Reflection;

namespace Microsoft.Ddue.Tools
{
    public class ChmBuilderArgs
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the configuration file name
        /// </summary>
        /// <value>The default is <b>ChmBuilder.config</b> in the same folder as the executable</value>
        public string ConfigurationFile { get; set; }

        /// <summary>
        /// This is used to get or set the HTML directory name
        /// </summary>
        public string HtmlDirectory { get; set; }

        /// <summary>
        /// This is used to get or set the language ID (LCID)
        /// </summary>
        /// <value>The default is 1033 (en-US)</value>
        public int LanguageId { get; set; }

        /// <summary>
        /// This is used to indicate whether or not to output Help 2 metadata
        /// </summary>
        /// <value>The default is false to omit metadata</value>
        public bool OutputMetadata { get; set; }

        /// <summary>
        /// This is used to get or set the output directory
        /// </summary>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// This is used to get or set the project name
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// This is used to get or set the table of contents filename
        /// </summary>
        /// <value>The default is an empty string and no TOC file will be created</value>
        public string TocFile { get; set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public ChmBuilderArgs()
        {
            this.LanguageId = 1033;
            this.TocFile = String.Empty;
            this.ConfigurationFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "ChmBuilder.config");
        }
        #endregion
    }
}
