//=============================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : Build2xHelpFile.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/06/2009
// Note    : Copyright 2008-2009, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the MSBuild task used to run HXCOMP.EXE which is used to
// compile a Help 2 (HxS) help file.
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

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SandcastleBuilder.Utils.MSBuild
{
    /// <summary>
    /// This task is used to run HXCOMP.EXE which is used to compile a Help 2
    /// (HxS) help file.
    /// </summary>
    public class Build2xHelpFile : ToolTask
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the tool name (HXCOMP.EXE)
        /// </summary>
        protected override string ToolName
        {
            get { return "HXCOMP.EXE"; }
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
        public string WorkingFolder { get; set; }

        /// <summary>
        /// This is used to pass in the path to the help compiler
        /// </summary>
        [Required]
        public string HelpCompilerFolder { get; set; }

        /// <summary>
        /// This is used to pass in the name of the help file (no path or
        /// extension).
        /// </summary>
        [Required]
        public string HtmlHelpName { get; set; }
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
                Log.LogError(null, "B2X0001", "B2X0001", "SHFB", 0, 0, 0, 0,
                  "A help compiler path must be specified");
                return false;
            }

            if(String.IsNullOrEmpty(this.HtmlHelpName))
            {
                Log.LogError(null, "B2X0002", "B2X0002", "SHFB", 0, 0, 0, 0,
                  "An HTML help base name must be specified");
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
            return Path.Combine(this.HelpCompilerFolder, "HXCOMP.EXE");
        }

        /// <summary>
        /// Generate the command line parameters
        /// </summary>
        /// <returns>The command line parameters</returns>
        protected override string GenerateCommandLineCommands()
        {
            return String.Format(CultureInfo.InvariantCulture,
                "-p \"{0}.HxC\" -l \"{0}.log\"", this.HtmlHelpName);
        }

        /// <summary>
        /// This is overridden to set the working folder before executing
        /// the task and to dump the compiler log file after the build.
        /// </summary>
        /// <returns>True if successful or false on failure</returns>
        public override bool Execute()
        {
            string line;
            bool result;

            Directory.SetCurrentDirectory(this.WorkingFolder);

            result = base.Execute();

            if(File.Exists(this.HtmlHelpName + ".log"))
                using(StreamReader sr = new StreamReader(this.HtmlHelpName + ".log"))
                {
                    while(!sr.EndOfStream)
                    {
                        line = sr.ReadLine();

                        if(line != null)
                            base.Log.LogMessage(MessageImportance.High, line);
                    }
                }

            return result;
        }

        /// <summary>
        /// This is overridden to invert the result of the HXCOMP exit code
        /// </summary>
        /// <returns>True on success, false on failure.  HXCOMP returns 0
        /// on success, 1 if warnings were issued (which is okay), and another
        /// non-zero value for failures.  We ignore the warning return code and
        /// treat it as successful.</returns>
        protected override bool HandleTaskExecutionErrors()
        {
            if(base.ExitCode < 2)
                return true;

            return base.HandleTaskExecutionErrors();
        }
        #endregion
    }
}
