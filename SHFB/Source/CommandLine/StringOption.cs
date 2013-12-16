// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 03/10/2012 - Updated code to fix FxCop warnings

using System;
using System.IO;

namespace Microsoft.Ddue.Tools.CommandLine
{
    /// <summary>
    /// This defines a string option
    /// </summary>
    /// <remarks>A string option is one that has a name/value pair separated by a colon</remarks>
    public class StringOption : BaseOption
    {
        #region Private data members
        //=====================================================================

        private string template;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to specify the template used when showing the command line syntax
        /// </summary>
        public string Template
        {
            get { return template; }
            set
            {
                if(String.IsNullOrEmpty(value))
                    template = "xxxx";
                else
                    template = value;
            }
        }
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The string option name</param>
        /// <param name="description">The string option description</param>
        public StringOption(string name, string description) : this(name, description, "xxxx")
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The string option name</param>
        /// <param name="description">The string option description</param>
        /// <param name="template">A template to use when showing the command line syntax</param>
        public StringOption(string name, string description, string template) : base(name, description)
        {
            this.Template = template;
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        internal override ParseResult ParseArgument(string argument)
        {
            if(argument.Length == 0)
                return ParseResult.MalformedArgument;

            if(argument[0] != ':')
                return ParseResult.MalformedArgument;

            if(base.IsPresent)
                return ParseResult.MultipleOccurence;

            base.Value = argument.Substring(1);

            return ParseResult.Success;
        }

        /// <inheritdoc />
        internal override void WriteTemplate(TextWriter writer)
        {
            writer.WriteLine("/{0}:{1}", base.Name, this.Template);
        }
        #endregion
    }
}
