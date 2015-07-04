//===============================================================================================================
// System  : Sandcastle Segregate By Namespace Tool
// File    : SegregateByNamespace.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/03/2015
// Compiler: Microsoft Visual C#
//
// This file contains the class used to make SegregateByNamespace callable from MSBuild projects.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Date        Who  Comments
// ==============================================================================================================
// 07/03/2015  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Sandcastle.Core;

namespace Microsoft.Ddue.Tools.MSBuild
{
    public class SegregateByNamespace : Task, ICancelableTask
    {
        #region Task properties
        //=====================================================================

        /// <summary>
        /// This is used to pass in the working folder where the files are located
        /// </summary>
        /// <value>If not set, no working folder will be set for the build</value>
        public string WorkingFolder { get; set; }

        /// <summary>
        /// This is used to pass in the output folder where the files are written
        /// </summary>
        /// <value>If not set, the files are written to the working folder</value>
        public string OutputFolder { get; set; }

        /// <summary>
        /// This is used to pass in the reflection data filename to split
        /// </summary>
        [Required]
        public string ReflectionFilename { get; set; }

        #endregion

        #region ICancelableTask Members
        //=====================================================================

        /// <summary>
        /// Cancel the build
        /// </summary>
        /// <remarks>The build will be canceled as soon as the current type has finished being visited</remarks>
        public void Cancel()
        {
            SegregateByNamespaceCore.Canceled = true;
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
            ConsoleApplication.ToolName = "SegregateByNamespace";

            try
            {
                // Switch to the working folder for the build so that relative paths are resolved properly
                if(!String.IsNullOrWhiteSpace(this.WorkingFolder))
                {
                    currentDirectory = Directory.GetCurrentDirectory();
                    Directory.SetCurrentDirectory(Path.GetFullPath(this.WorkingFolder));
                }

                if(!String.IsNullOrWhiteSpace(this.OutputFolder))
                    args.Add("/out:" + this.OutputFolder);

                args.Add(this.ReflectionFilename);

                success = (SegregateByNamespaceCore.Main(args.ToArray()) == 0);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                ConsoleApplication.WriteMessage(LogLevel.Error, "An unexpected error occurred trying to " +
                    "execute the SegregateByNamespace MSBuild task: {0}", ex);
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
