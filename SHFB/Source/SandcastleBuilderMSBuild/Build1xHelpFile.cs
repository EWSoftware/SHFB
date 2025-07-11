//===============================================================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : Build1xHelpFile.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/09/2025
// Note    : Copyright 2008-2025, Eric Woodruff, All rights reserved
//
// This file contains the MSBuild task used to run HHC.EXE which is used to compile a Help 1 (CHM) help file
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/11/2008  EFW  Created the code
//===============================================================================================================

using System;
using System.Globalization;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Sandcastle.Core;

namespace SandcastleBuilder.MSBuild
{
    /// <summary>
    /// This task is used to run HHC.EXE which is used to compile a Help 1 (CHM) help file
    /// </summary>
    /// <remarks>Support is provided for wrapping the tool in a call to an application such as SBAppLocale.exe to
    /// workaround encoding issues with the Help 1 compiler.</remarks>
    public class Build1xHelpFile : ToolTask
    {
        #region Private data members
        //=====================================================================

        private const string HelpCompilerName = "HHC.EXE";

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the tool name (HHC.EXE or the value of <see cref="LocalizeApp" /> if
        /// specified).
        /// </summary>
        protected override string ToolName
        {
            get
            {
                if(!String.IsNullOrEmpty(this.LocalizeApp))
                    return this.LocalizeApp;

                return HelpCompilerName;
            }
        }

        /// <summary>
        /// This is overridden to force all standard error info to be logged
        /// </summary>
        protected override MessageImportance StandardErrorLoggingImportance => MessageImportance.High;

        /// <summary>
        /// This is overridden to force all standard output info to be logged
        /// </summary>
        protected override MessageImportance StandardOutputLoggingImportance => MessageImportance.High;

        /// <summary>
        /// This is used to pass in the working folder where the files are located
        /// </summary>
        [Required]
        public string WorkingFolder { get; set; }

        /// <summary>
        /// This is used to pass in the path to the help compiler
        /// </summary>
        [Required]
        public string HelpCompilerFolder { get; set; }

        /// <summary>
        /// This is used to pass in the help project filename
        /// </summary>
        [Required]
        public string HelpProjectName { get; set; }

        /// <summary>
        /// This is used to pass in the name of the application to use as the localization wrapper
        /// </summary>
        /// <remarks>This is optional.  If specified, it will be used to run the help compiler to work around
        /// encoding issues.</remarks>
        public string LocalizeApp { get; set; }

        /// <summary>
        /// This is used to get or set the language ID for the localization tool (<see cref="LocalizeApp" />)
        /// </summary>
        /// <remarks>This is optional.  If not specified, it defaults to 1033.  It is ignored if
        /// <see cref="LocalizeApp" /> is not set.</remarks>
        public int LanguageId { get; set; }

        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// Validate the parameters
        /// </summary>
        /// <returns>True if the parameters are valid, false if not</returns>
        protected override bool ValidateParameters()
        {
            if(String.IsNullOrEmpty(this.HelpCompilerFolder))
            {
                Log.LogError(null, "B1X0001", "B1X0001", "SHFB", 0, 0, 0, 0,
                    "A help compiler path must be specified");
                return false;
            }

            if(String.IsNullOrEmpty(this.HelpProjectName) || !File.Exists(this.HelpProjectName))
            {
                Log.LogError(null, "B1X0002", "B1X0002", "SHFB", 0, 0, 0, 0,
                  "A help project file must be specified and it must exist");
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
            if(!String.IsNullOrEmpty(this.LocalizeApp))
                return this.LocalizeApp;

            return Path.Combine(this.HelpCompilerFolder, HelpCompilerName);
        }

        /// <summary>
        /// Generate the command line parameters
        /// </summary>
        /// <returns>The command line parameters</returns>
        protected override string GenerateCommandLineCommands()
        {
            if(!String.IsNullOrEmpty(this.LocalizeApp))
            {
                if(this.LanguageId == 0)
                    this.LanguageId = 1033;

                return String.Format(CultureInfo.InvariantCulture, "{0} \"{1}\" \"{2}\"", this.LanguageId,
                    Path.Combine(this.HelpCompilerFolder, HelpCompilerName), this.HelpProjectName);
            }

            return "\"" + this.HelpProjectName + "\"";
        }

        /// <summary>
        /// This is overridden to set the working folder before executing the task and to invert the result
        /// returned from the help compiler.
        /// </summary>
        /// <returns>True if successful or false on failure</returns>
        public override bool Execute()
        {
            Directory.SetCurrentDirectory(this.WorkingFolder.CorrectFilePathSeparators());

            // HHC is backwards and returns zero on failure and non-zero on success
            if(base.Execute() && this.ExitCode == 0 && String.IsNullOrEmpty(this.LocalizeApp))
                return base.HandleTaskExecutionErrors();

            return true;
        }

        /// <summary>
        /// This is overridden to invert the result of the HHC exit code
        /// </summary>
        /// <returns>True on success, false on failure.  HHC is backwards and returns 0 on failures and 1 on
        /// success.  We invert the result to be consistent with other tasks.</returns>
        protected override bool HandleTaskExecutionErrors()
        {
            if(String.IsNullOrEmpty(this.LocalizeApp) && this.ExitCode == 1)
                return true;

            return base.HandleTaskExecutionErrors();
        }
        #endregion
    }
}
