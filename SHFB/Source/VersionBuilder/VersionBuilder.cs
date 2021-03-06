﻿//===============================================================================================================
// System  : Sandcastle Version Builder Tool
// File    : VersionBuilder.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/13/2021
//
// This file contains the class used to make VersionBuilder callable from MSBuild projects.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Date        Who  Comments
// ==============================================================================================================
// 12/28/2013  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Sandcastle.Core;

namespace Microsoft.Ddue.Tools.MSBuild
{
    public class VersionBuilder : Task, ICancelableTask
    {
        #region Task properties
        //=====================================================================

        /// <summary>
        /// This is used to pass in the working folder where the files are located
        /// </summary>
        /// <value>If not set, no working folder will be set for the build</value>
        public string WorkingFolder { get; set; }

        /// <summary>
        /// This is used to pass in the configuration filename
        /// </summary>
        [Required]
        public string ConfigurationFile { get; set; }

        /// <summary>
        /// This is used to pass in the output filename
        /// </summary>
        [Required]
        public string OutputFile { get; set; }

        /// <summary>
        /// This is used to get or set whether or not to rip old APIs not supported by the latest version
        /// </summary>
        public bool RipOldApis { get; set; }

        #endregion

        #region ICancelableTask Members
        //=====================================================================

        /// <summary>
        /// Cancel the build
        /// </summary>
        /// <remarks>The build will terminate as soon as possible</remarks>
        public void Cancel()
        {
            VersionBuilderCore.Cancel = true;
        }
        #endregion

        #region Task execution
        //=====================================================================

        /// <summary>
        /// This executes the task
        /// </summary>
        /// <returns>True on success, false on failure</returns>
        public override bool Execute()
        {
            List<string> args = new List<string>();
            string currentDirectory = null;
            bool success = false;

            // Log messages via MSBuild
            ConsoleApplication.Log = this.Log;
            ConsoleApplication.ToolName = "VersionBuilder";

            try
            {
                // Switch to the working folder for the build so that relative paths are resolved properly
                if(!String.IsNullOrWhiteSpace(this.WorkingFolder))
                {
                    currentDirectory = Directory.GetCurrentDirectory();
                    Directory.SetCurrentDirectory(Path.GetFullPath(this.WorkingFolder));
                }

                args.Add("/config:" + this.ConfigurationFile);
                args.Add("/out:" + this.OutputFile);
                args.Add("/rip" + (this.RipOldApis ? "+" : "-"));

                success = (VersionBuilderCore.MainEntryPoint(args.ToArray()) == 0);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                ConsoleApplication.WriteMessage(LogLevel.Error, "An unexpected error occurred trying to " +
                    "execute the VersionBuilder MSBuild task: {0}", ex);
            }
            finally
            {
                if(currentDirectory != null)
                    Directory.SetCurrentDirectory(currentDirectory);
            }

            return success;
        }
        #endregion
    }
}
