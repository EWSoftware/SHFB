//=============================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : Build1xHelpFile.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/05/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the MSBuild task used to run HHC.EXE which is used to
// compile a Help 1 (CHM) help file.
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
using System.Globalization;
using System.IO;
using System.Text;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.Utils.MSBuild
{
    /// <summary>
    /// This task is used to run HHC.EXE which is used to compile a Help 1
    /// (CHM) help file.
    /// </summary>
    /// <remarks>Support is provided for wrapping the tool in a call to an
    /// application such as SBAppLocale.exe to workaround encoding issues with
    /// the Help 1 compiler.</remarks>
    public class Build1xHelpFile : ToolTask
    {
        #region Private data members
        //=====================================================================

        private const string HelpCompilerName = "HHC.EXE";

        private string workingFolder, helpCompilerFolder, helpProjectName,
            localizeApp;
        private int langId;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the tool name (HHC.EXE or the
        /// value of <see cref="LocalizeApp" /> if specified).
        /// </summary>
        protected override string ToolName
        {
            get
            {
                if(!String.IsNullOrEmpty(localizeApp))
                    return localizeApp;

                return HelpCompilerName;
            }
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
        /// This is used to pass in the working folder where the files are
        /// located.
        /// </summary>
        [Required]
        public string WorkingFolder
        {
            get { return workingFolder; }
            set { workingFolder = value; }
        }

        /// <summary>
        /// This is used to pass in the path to the help compiler
        /// </summary>
        [Required]
        public string HelpCompilerFolder
        {
            get { return helpCompilerFolder; }
            set { helpCompilerFolder = value; }
        }

        /// <summary>
        /// This is used to pass in the help project filename
        /// </summary>
        [Required]
        public string HelpProjectName
        {
            get { return helpProjectName; }
            set { helpProjectName = value; }
        }

        /// <summary>
        /// This is used to pass in the name of the application to use as the
        /// localization wrapper.
        /// </summary>
        /// <remarks>This is optional.  If specified, it will be used to run
        /// the help compiler to work around encoding issues.</remarks>
        public string LocalizeApp
        {
            get { return localizeApp; }
            set { localizeApp = value; }
        }

        /// <summary>
        /// This is used to get or set the language ID for the localization
        /// tool (see cref="LocalizeApp" />).
        /// </summary>
        /// <remarks>This is optional.  If not specified, it defaults to 1033.
        /// It is ignored if <see cref="LocalizeApp" /> is not set.</remarks>
        public int LanguageId
        {
            get { return langId; }
            set { langId = value; }
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
            if(String.IsNullOrEmpty(helpCompilerFolder))
            {
                Log.LogError(null, "B1X0001", "B1X0001", "SHFB", 0, 0, 0, 0,
                  "A help compiler path must be specified");
                return false;
            }

            if(String.IsNullOrEmpty(helpProjectName) ||
              !File.Exists(helpProjectName))
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
            if(!String.IsNullOrEmpty(localizeApp))
                return localizeApp;

            return Path.Combine(helpCompilerFolder, HelpCompilerName);
        }

        /// <summary>
        /// Generate the command line parameters
        /// </summary>
        /// <returns>The command line parameters</returns>
        protected override string GenerateCommandLineCommands()
        {
            if(!String.IsNullOrEmpty(localizeApp))
            {
                if(langId == 0)
                    langId = 1033;

                return String.Format(CultureInfo.InvariantCulture,
                    "{0} \"{1}\" \"{2}\"", langId,
                    Path.Combine(helpCompilerFolder, HelpCompilerName),
                    helpProjectName);
            }

            return "\"" + helpProjectName + "\"";
        }

        /// <summary>
        /// This is overridden to set the working folder before executing
        /// the task and to invert the result returned from the help compiler.
        /// </summary>
        /// <returns>True if successful or false on failure</returns>
        public override bool Execute()
        {
            Directory.SetCurrentDirectory(workingFolder);

            // HHC is backwards and returns zero on failure and non-zero
            // on success.
            if(base.Execute() && base.ExitCode == 0 &&
              String.IsNullOrEmpty(localizeApp))
                return base.HandleTaskExecutionErrors();

            return true;
        }

        /// <summary>
        /// This is overridden to invert the result of the HHC exit code
        /// </summary>
        /// <returns>True on success, false on failure.  HXCOMP is backwards
        /// and returns 0 on failures and 1 on success.  We invert the result
        /// to be consistent with other tasks.</returns>
        protected override bool HandleTaskExecutionErrors()
        {
            if(String.IsNullOrEmpty(localizeApp) && base.ExitCode == 1)
                return true;

            return base.HandleTaskExecutionErrors();
        }
        #endregion
    }
}
