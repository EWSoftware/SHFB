// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 03/10/2012 - EFW - Updated code to fix FxCop warnings
// 12/21/2013 - EFW - Moved class to Sandcastle.Core assembly

using System;
using System.IO;
using System.Collections.Generic;

namespace Sandcastle.Core.CommandLine
{
    /// <summary>
    /// This defines a list option
    /// </summary>
    /// <remarks>A a list option is like a <see cref="StringOption"/> but the value is a comma-separated
    /// list of one or more values.</remarks>
    public sealed class ListOption : StringOption
    {
        #region Private data members
        //=====================================================================

        private List<string> values;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is overridden to return the value as a string array
        /// </summary>
        public override object Value
        {
            get
            {
                if(values == null)
                    throw new InvalidOperationException("Option value has not been set");

                return values.ToArray();
            }
            protected set
            {
                throw new NotImplementedException();
            }
        }
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The list option name</param>
        /// <param name="description">The list option description</param>
        public ListOption(string name, string description) : base(name, description)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The list option name</param>
        /// <param name="description">The list option description</param>
        /// <param name="template">A template to use when showing the command line syntax</param>
        public ListOption(string name, string description, string template) : base(name, description, template)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>If the option appears multiple times, the values are combined into a single list</remarks>
        internal override ParseResult ParseArgument(string argument)
        {
            if(argument.Length == 0)
                return ParseResult.MalformedArgument;

            if(argument[0] != ':')
                return ParseResult.MalformedArgument;

            if(values == null)
            {
                values = new List<string>();
                base.Value = values;
            }

            // Empty values are retained in case they are wanted
            values.AddRange(argument.Substring(1).Split(','));

            return ParseResult.Success;
        }

        /// <inheritdoc />
        internal override void WriteTemplate(TextWriter writer)
        {
            writer.WriteLine("/{0}:{1}[,{1},{1},...]", base.Name, base.Template);
        }
        #endregion
    }
}
