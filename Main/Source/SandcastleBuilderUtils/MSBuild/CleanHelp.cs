//=============================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : CleanHelp.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/09/2011
// Note    : Copyright 2008-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the MSBuild task used to clean (remove) help file output
// from the last build.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  06/27/2008  EFW  Created the code
// ============================================================================

using System;
using System.IO;
using System.Linq;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.Utils.MSBuild
{
    /// <summary>
    /// This task is used to clean (remove) help file output from the last
    /// build.
    /// </summary>
    public class CleanHelp : Task
    {
        #region Task input properties
        //=====================================================================

        /// <summary>
        /// This is used to pass in the project filename
        /// </summary>
        [Required]
        public string ProjectFile { get; set; }

        /// <summary>
        /// This is used to pass in the output path that needs to be cleaned
        /// </summary>
        [Required]
        public string OutputPath { get; set; }

        /// <summary>
        /// This is used to pass in the optional working path that needs to be
        /// cleaned.
        /// </summary>
        public string WorkingPath { get; set; }

        /// <summary>
        /// This is used to pass in the optional log filename that needs
        /// to be cleaned.
        /// </summary>
        public string LogFileLocation { get; set; }
        #endregion

        #region Execute method
        //=====================================================================

        /// <summary>
        /// This is used to execute the task and clean the output folder
        /// </summary>
        /// <returns>True on success or false on failure.</returns>
        public override bool Execute()
        {
            string projectPath;

            try
            {
                projectPath = Path.GetDirectoryName(Path.GetFullPath(this.ProjectFile));

                // Make sure we start out in the project's output folder
                // in case the output folder is relative to it.
                Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(projectPath)));

                // Clean the working folder
                if(!String.IsNullOrEmpty(this.WorkingPath))
                {
                    if(!Path.IsPathRooted(this.WorkingPath))
                        this.WorkingPath = Path.GetFullPath(Path.Combine(projectPath, this.WorkingPath));

                    if(Directory.Exists(this.WorkingPath))
                    {
                        BuildProcess.VerifySafePath("WorkingPath", this.WorkingPath, projectPath);
                        Log.LogMessage("Removing working folder...");
                        Directory.Delete(this.WorkingPath, true);
                    }
                }

                if(!Path.IsPathRooted(this.OutputPath))
                    this.OutputPath = Path.GetFullPath(Path.Combine(projectPath, this.OutputPath));

                if(Directory.Exists(this.OutputPath))
                {
                    Log.LogMessage("Removing build files...");
                    BuildProcess.VerifySafePath("OutputPath", this.OutputPath, projectPath);

                    // Read-only and/or hidden files and folders are ignored as they are assumed to be
                    // under source control.
                    foreach(string file in Directory.EnumerateFiles(this.OutputPath))
                        if((File.GetAttributes(file) & (FileAttributes.ReadOnly | FileAttributes.Hidden)) == 0)
                            File.Delete(file);
                        else
                            Log.LogMessage("Skipping read-only or hidden file '{0}'", file);

                    Log.LogMessage("Removing build folders...");

                    foreach(string folder in Directory.EnumerateDirectories(this.OutputPath))
                        try
                        {
                            // Some source control providers have a mix of read-only/hidden files within a folder
                            // that isn't read-only/hidden (i.e. Subversion).  In such cases, leave the folder alone.
                            if(Directory.EnumerateFileSystemEntries(folder, "*", SearchOption.AllDirectories).Any(f =>
                              (File.GetAttributes(f) & (FileAttributes.ReadOnly | FileAttributes.Hidden)) != 0))
                                Log.LogMessage("Skipping folder '{0}' as it contains read-only or hidden folders/files", folder);
                            else
                                if((File.GetAttributes(folder) & (FileAttributes.ReadOnly | FileAttributes.Hidden)) == 0)
                                    Directory.Delete(folder, true);
                                else
                                    Log.LogMessage("Skipping folder '{0}' as it is read-only or hidden", folder);
                        }
                        catch(IOException ioEx)
                        {
                            Log.LogMessage("Did not delete folder '{0}': {1}", folder, ioEx.Message);
                        }
                        catch(UnauthorizedAccessException uaEx)
                        {
                            Log.LogMessage("Did not delete folder '{0}': {1}", folder, uaEx.Message);
                        }

                    // Delete the log file too if it exists
                    if(!String.IsNullOrEmpty(this.LogFileLocation) && File.Exists(this.LogFileLocation))
                    {
                        Log.LogMessage("Removing build log...");
                        File.Delete(this.LogFileLocation);
                    }
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                Log.LogError(null, "CT0001", "CT0001", "SHFB", 0, 0, 0, 0,
                    "Unable to clean output folder.  Reason: {0}", ex);
                return false;
            }

            return true;
        }
        #endregion
    }
}
