//===============================================================================================================
// System  : Sandcastle BuildAssembler Tool
// File    : BuildAssembler.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/28/2013
// Compiler: Microsoft Visual C#
//
// This file contains the class used to make BuildAssembler callable from MSBuild projects.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
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
    public class BuildAssembler : Task, ICancelableTask
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
        /// This is used to pass in the manifest filename
        /// </summary>
        [Required]
        public string ManifestFile { get; set; }

        #endregion

        #region ICancelableTask Members
        //=====================================================================

        /// <summary>
        /// Cancel the build
        /// </summary>
        /// <remarks>The build will terminate as soon as possible after initializing a component or after a
        /// topic finishes being generated.</remarks>
        public void Cancel()
        {
            if(BuildAssemblerConsole.BuildAssembler != null)
                BuildAssemblerConsole.BuildAssembler.Cancel();
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
            ConsoleApplication.ToolName = "BuildAssembler";

            try
            {
                // Switch to the working folder for the build so that relative paths are resolved properly
                if(!String.IsNullOrWhiteSpace(this.WorkingFolder))
                {
                    currentDirectory = Directory.GetCurrentDirectory();
                    Directory.SetCurrentDirectory(Path.GetFullPath(this.WorkingFolder));
                }

                args.Add("/config:" + this.ConfigurationFile);
                args.Add(this.ManifestFile);

                success = (BuildAssemblerConsole.Main(args.ToArray()) == 0);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                ConsoleApplication.WriteMessage(LogLevel.Error, "An unexpected error occurred trying to " +
                    "execute the BuildAssembler MSBuild task: {0}", ex);
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
