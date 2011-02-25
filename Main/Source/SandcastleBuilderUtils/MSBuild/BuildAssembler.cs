//=============================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : BuildAssemblerTask.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/05/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the MSBuild task used to run BuildAssembler.exe which is
// used to generate help topics.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.0  07/11/2008  EFW  Created the code
// ============================================================================

using System;
using System.IO;
using System.Text;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.Utils.MSBuild
{
    /// <summary>
    /// This task is used to run BuildAssembler.exe which is used to generate
    /// help topics.
    /// </summary>
    public class BuildAssembler : ToolTask
    {
        #region Private data members
        //=====================================================================

        private string sandcastlePath, configFile, manifestFile, workingFolder;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the tool name (BuildAssembler.exe)
        /// </summary>
        protected override string ToolName
        {
            get { return "BuildAssembler.exe"; }
        }

        /// <summary>
        /// This is overridden to force all standard error info to be logged
        /// </summary>
        protected override MessageImportance StandardErrorLoggingImportance
        {
            get { return MessageImportance.High; }
        }

        /// <summary>
        /// This is overridden to force all standard output info to be logged
        /// </summary>
        protected override MessageImportance StandardOutputLoggingImportance
        {
            get { return MessageImportance.High; }
        }

        /// <summary>
        /// This is used to pass in the path to the Sandcastle tools
        /// </summary>
        [Required]
        public string SandcastlePath
        {
            get { return sandcastlePath; }
            set { sandcastlePath = value; }
        }

        /// <summary>
        /// This is used to pass in the configuration filename
        /// </summary>
        [Required]
        public string ConfigurationFile
        {
            get { return configFile; }
            set { configFile = value; }
        }

        /// <summary>
        /// This is used to pass in the manifest filename
        /// </summary>
        [Required]
        public string ManifestFile
        {
            get { return manifestFile; }
            set { manifestFile = value; }
        }

        /// <summary>
        /// This is used to pass in the working folder where the files are
        /// located.
        /// </summary>
        [Required]
        public string WorkingFolder
        {
            get { return workingFolder; }
            set { workingFolder = value; }
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// Validate the parameters
        /// </summary>
        /// <returns>True if the parameters are valid, false if not</returns>
        protected override bool ValidateParameters()
        {
            if(String.IsNullOrEmpty(configFile) || !File.Exists(configFile))
            {
                Log.LogError(null, "BAT0001", "BAT0001", "SHFB", 0, 0, 0, 0,
                  "A configuration file must be specified and it must exist");
                return false;
            }

            if(String.IsNullOrEmpty(manifestFile) || !File.Exists(manifestFile))
            {
                Log.LogError(null, "BAT0002", "BAT0002", "SHFB", 0, 0, 0, 0,
                  "A manifest file must be specified and it must exist");
                return false;
            }

            return true;
        }

        /// <summary>
        /// This returns the full path to the tool
        /// </summary>
        /// <returns>The full path to the tool</returns>
        protected override string GenerateFullPathToTool()
        {
            return Path.Combine(sandcastlePath, "ProductionTools\\" +
                this.ToolName);
        }

        /// <summary>
        /// Generate the command line parameters
        /// </summary>
        /// <returns>The command line parameters</returns>
        protected override string GenerateCommandLineCommands()
        {
            return String.Concat("/config:", configFile, " ", manifestFile);
        }

        /// <summary>
        /// This is overridden to return the working folder for the build
        /// </summary>
        /// <returns>The working folder for the build</returns>
        protected override string GetWorkingDirectory()
        {
            return Path.GetFullPath(workingFolder);
        }
        #endregion
    }
}
