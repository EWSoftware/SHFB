//===============================================================================================================
// System  : Help Library Manager Launcher
// File    : CommandLineArgument.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2025
// Note    : Copyright 2010-2025, Eric Woodruff, All rights reserved
//
// This file contains a class representing a command line argument
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/03/2010  EFW  Created the code
//===============================================================================================================

using System;

namespace Sandcastle.Platform.Windows.MicrosoftHelpViewer
{
    /// <summary>
    /// This class represents a command line argument
    /// </summary>
    public class CommandLineArgument
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the argument value
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// This read-only property returns whether or not the value is a
        /// command line switch.
        /// </summary>
        /// <value>This returns true if the value is prefixed with a slash
        /// or a dash.</value>
        public bool IsSwitch { get; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="argument">The command line argument</param>
        public CommandLineArgument(string argument)
        {
            this.Value = argument;

            if(this.Value.Length > 1 && (this.Value[0] == '-' || this.Value[0] == '/'))
                this.IsSwitch = true;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to see if the argument matches the specified
        /// switch argument name.
        /// </summary>
        /// <param name="argumentName">The switch argument name to match</param>
        /// <returns>True if this option matches the given name, False if not.</returns>
        public bool MatchesSwitch(string argumentName)
        {
            return this.IsSwitch && this.Value.Substring(1).Equals(argumentName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns the value as a command line option
        /// </summary>
        /// <returns>The value ready for use as a command line option.  If it contains spaces, it will be
        /// enclosed in quotes.</returns>
        public string ToCommandLineOption()
        {
            if(Value.IndexOf(' ') != -1)
                return "\"" + this.Value + "\"";

            return this.Value;
        }
        #endregion
    }
}
