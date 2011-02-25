//=============================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : XslTransformTask.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/15/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the MSBuild task used to run XslTransform.exe which is
// used to run various XSL transformations on the Sandcastle data files.
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
    /// This task is used to run XslTransform.exe which is used to run various
    /// XSL transformations on the Sandcastle data files.
    /// </summary>
    public class XslTransform : ToolTask
    {
        #region Private data members
        //=====================================================================

        private string sandcastlePath, inputFile, outputFile, workingFolder;
        private string[] transformations, arguments;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the tool name (XslTransform.exe)
        /// </summary>
        protected override string ToolName
        {
            get { return "XslTransform.exe"; }
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
        /// This is used to pass in the input filename
        /// </summary>
        [Required]
        public string InputFile
        {
            get { return inputFile; }
            set { inputFile = value; }
        }

        /// <summary>
        /// This is used to pass in the output filename
        /// </summary>
        [Required]
        public string OutputFile
        {
            get { return outputFile; }
            set { outputFile = value; }
        }

        /// <summary>
        /// This is used to pass in the list of transformations to run
        /// </summary>
        /// <remarks>Separate multiple transforms with a semi-colon.  Relative
        /// paths are assumed to refer to Sandcastle transformations and will
        /// be fully qualified with the <see cref="SandcastlePath" /> value.
        /// Absolute paths are assumed to be custom transforms and the path is
        /// not modified.</remarks>
        [Required]
        public string[] Transformations
        {
            get { return transformations; }
            set { transformations = value; }
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
        /// This is used to pass in any optional XSL transform arguments
        /// </summary>
        /// <value>The optional XSL transform arguments in the form
        /// "argName=argValue".  Separate multiple arguments with a
        /// semi-colon.</value>
        public string[] Arguments
        {
            get { return arguments; }
            set { arguments = value; }
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
            if(transformations == null || transformations.Length == 0)
            {
                Log.LogError(null, "XTT0001", "XTT0001", "SHFB", 0, 0, 0, 0,
                  "At least one XSL transformation is required");
                return false;
            }

            if(String.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
            {
                Log.LogError(null, "XTT0002", "XTT0002", "SHFB", 0, 0, 0, 0,
                  "An input file must be specified and it must exist");
                return false;
            }

            if(String.IsNullOrEmpty(outputFile))
            {
                Log.LogError(null, "XTT0003", "XTT0003", "SHFB", 0, 0, 0, 0,
                  "An output filename must be specified");
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
            bool isFirst = true;

            StringBuilder sb = new StringBuilder(1024);

            sb.Append("/xsl:");

            foreach(string transform in transformations)
            {
                if(!isFirst)
                    sb.Append(',');

                // If a transform path isn't rooted, assume it is in the
                // Sandcastle tools folder.
                if(!Path.IsPathRooted(transform))
                    sb.AppendFormat("\"{0}\"", Path.Combine(sandcastlePath,
                        transform));
                else
                    sb.AppendFormat("\"{0}\"", transform);

                isFirst = false;
            }

            if(arguments != null)
                foreach(string arg in arguments)
                    sb.AppendFormat(" /arg:{0}", arg);

            sb.AppendFormat(" \"{0}\" /out:\"{1}\"", inputFile, outputFile);

            return sb.ToString();
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
