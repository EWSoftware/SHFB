//===============================================================================================================
// System  : Sandcastle Tools - Add Namespace Groups Utility
// File    : AddNamespaceGroups.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/12/2013
// Compiler: Microsoft Visual C#
//
// This file contains the class used to make AddNamespaceGroups callable from MSBuild projects.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Date        Who  Comments
// ==============================================================================================================
// 12/12/2013  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Sandcastle.Core;

namespace Microsoft.Ddue.Tools.MSBuild
{
    public class AddNamespaceGroups : Task, ICancelableTask
    {
        #region Task properties
        //=====================================================================

        /// <summary>
        /// This is used to pass in the working folder where the files are located
        /// </summary>
        /// <value>If not set, no working folder will be set for the build</value>
        public string WorkingFolder { get; set; }

        /// <summary>
        /// This is used to pass in the input filename
        /// </summary>
        [Required]
        public string InputFile { get; set; }

        /// <summary>
        /// This is used to pass in the output filename
        /// </summary>
        [Required]
        public string OutputFile { get; set; }

        /// <summary>
        /// This is used to pass in the maximum number of namespace parts to consider when creating groups
        /// </summary>
        /// <value>If not specified, the default is 2 parts</value>
        public int MaximumParts { get; set; }

        #endregion

        #region ICancelableTask Members
        //=====================================================================

        /// <summary>
        /// Cancel the process
        /// </summary>
        /// <remarks>The process is stopped as soon as possible</remarks>
        public void Cancel()
        {
            AddNamespaceGroupsCore.Canceled = true;
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
            ConsoleApplication.ToolName = "AddNamespaceGroups";

            if(!File.Exists(this.InputFile))
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, "An input file must be specified and it must exist");
                return false;
            }

            try
            {
                // Switch to the working folder for the process so that relative paths are resolved properly
                if(!String.IsNullOrWhiteSpace(this.WorkingFolder))
                {
                    currentDirectory = Directory.GetCurrentDirectory();
                    Directory.SetCurrentDirectory(Path.GetFullPath(this.WorkingFolder));
                }

                args.Add(this.InputFile);
                args.Add("/out:" + this.OutputFile);

                if(this.MaximumParts > 1)
                    args.Add("/maxParts:" + this.MaximumParts.ToString(CultureInfo.InvariantCulture));

                success = (AddNamespaceGroupsCore.Main(args.ToArray()) == 0);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                ConsoleApplication.WriteMessage(LogLevel.Error, "An unexpected error occurred trying to " +
                    "execute the AddNamespaceGroups MSBuild task: {0}", ex);
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

