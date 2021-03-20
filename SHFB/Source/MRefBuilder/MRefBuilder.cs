//===============================================================================================================
// System  : Sandcastle MRefBuilder Tool
// File    : MRefBuilder.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/18/2021
//
// This file contains the class used to make MRefBuilder callable from MSBuild projects.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Date        Who  Comments
// ==============================================================================================================
// 12/10/2013  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: dep

using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Sandcastle.Core;

namespace Microsoft.Ddue.Tools.MSBuild
{
    public class MRefBuilder : Task, ICancelableTask
    {
        #region Task properties
        //=====================================================================

        /// <summary>
        /// This is used to pass in the working folder where the files are located
        /// </summary>
        /// <value>If not set, no working folder will be set for the build</value>
        public string WorkingFolder { get; set; }

        /// <summary>
        /// This is used to pass in the MRefBuilder configuration file name
        /// </summary>
        /// <value>If not set, the default configuration file is used</value>
        public string ConfigurationFile { get; set; }

        /// <summary>
        /// This is used to pass in the filename to which the reflection data is written
        /// </summary>
        [Required]
        public string ReflectionFilename { get; set; }

        /// <summary>
        /// This is used to pass in the assemblies to reflect over
        /// </summary>
        [Required]
        public ITaskItem[] Assemblies { get; set; }

        /// <summary>
        /// This is used to pass in any resolved references
        /// </summary>
        /// <value>References are optional</value>
        public ITaskItem[] References { get; set; }

        #endregion

        #region ICancelableTask Members
        //=====================================================================

        /// <summary>
        /// Cancel the build
        /// </summary>
        /// <remarks>The build will be canceled as soon as the current type has finished being visited</remarks>
        public void Cancel()
        {
            if(MRefBuilderCore.ApiVisitor != null)
                MRefBuilderCore.ApiVisitor.Canceled = true;
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
            ConsoleApplication.ToolName = "MRefBuilder";

            if(this.Assemblies == null || this.Assemblies.Length == 0)
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, "At least one assembly (.dll or .exe) is " +
                    "required for MRefBuilder to parse");
                return false;
            }

            try
            {
                // Switch to the working folder for the build so that relative paths are resolved properly
                if(!String.IsNullOrWhiteSpace(this.WorkingFolder))
                {
                    currentDirectory = Directory.GetCurrentDirectory();
                    Directory.SetCurrentDirectory(Path.GetFullPath(this.WorkingFolder));
                }

                if(!String.IsNullOrWhiteSpace(this.ConfigurationFile))
                    args.Add("/config:" + this.ConfigurationFile);

                args.Add("/out:" + this.ReflectionFilename);

                if(this.References != null)
                {
                    foreach(ITaskItem item in this.References)
                        args.Add("/dep:" + item.ItemSpec);
                }

                foreach(ITaskItem item in this.Assemblies)
                    args.Add(item.ItemSpec);

                success = (MRefBuilderCore.MainEntryPoint(args.ToArray()) == 0);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                ConsoleApplication.WriteMessage(LogLevel.Error, "An unexpected error occurred trying to " +
                    "execute the MRefBuilder MSBuild task: {0}", ex);
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
