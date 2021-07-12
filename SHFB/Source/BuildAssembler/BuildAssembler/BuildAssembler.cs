//===============================================================================================================
// System  : Sandcastle BuildAssembler Tool
// File    : BuildAssembler.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/31/2021
//
// This file contains the class used to make BuildAssembler callable from MSBuild projects.
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
using System.ComponentModel.Composition;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler;

namespace Sandcastle.Tools.MSBuild
{
#pragma warning disable CA1724
    /// <summary>
    /// This task is used to run the BuildAssembler tool from MSBuild
    /// </summary>
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

        #region BuildAssembler export
        //=====================================================================

        /// <summary>
        /// The build assembler instance
        /// </summary>
        [Export(typeof(BuildAssemblerCore))]
        public BuildAssemblerCore BuildAssemblerInstance { get; private set; }

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
            if(BuildAssemblerInstance != null)
                BuildAssemblerInstance.Cancel();
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
            string currentDirectory = null;
            bool success = false;

#if NETCOREAPP3_1_OR_GREATER
            // TODO: This can go away once we get rid of the XSL transformations in the presentation styles
            // Allow loading of external URIs in XSL transformations
            AppContext.SetSwitch("Switch.System.Xml.AllowDefaultResolver", true);
#endif
            this.WriteBanner();

            try
            {
                // Switch to the working folder for the build so that relative paths are resolved properly
                if(!String.IsNullOrWhiteSpace(this.WorkingFolder))
                {
                    currentDirectory = Directory.GetCurrentDirectory();
                    Directory.SetCurrentDirectory(Path.GetFullPath(this.WorkingFolder));
                }

                success = BuildTopics();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                this.WriteMessage(LogLevel.Error, "An unexpected error occurred trying to " +
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

#region Build topics method
        //=====================================================================


        /// <summary>
        /// This builds the topics based on the configuration and manifest
        /// </summary>
        /// <returns>True on success, false on failure</returns>
        public bool BuildTopics()
        {
            // Create a build assembler instance to do the work.  Messages are logged to the task log.
            this.BuildAssemblerInstance = new BuildAssemblerCore((lvl, msg) => this.WriteMessage(lvl, msg));

            try
            {
                // Execute it using the given configuration and manifest
                this.BuildAssemblerInstance.Execute(this.ConfigurationFile, this.ManifestFile);
            }
            catch(Exception ex)
            {
                // Ignore aggregate exceptions where the inner exception is OperationCanceledException.
                // These are the result of logging an error message.
                if(!(ex is AggregateException) || !(ex.InnerException is OperationCanceledException))
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    this.WriteMessage(LogLevel.Error, ex.GetExceptionMessage());
                }

                return false;
            }
            finally
            {
                this.BuildAssemblerInstance.Dispose();
            }

            return true;
        }
#endregion
    }
}
