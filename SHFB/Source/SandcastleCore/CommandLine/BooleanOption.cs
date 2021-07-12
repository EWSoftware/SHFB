// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 03/10/2012 - EFW - Updated code to fix FxCop warnings
// 12/21/2013 - EFW - Moved class to Sandcastle.Core assembly

using System.IO;

namespace Sandcastle.Core.CommandLine
{
    /// <summary>
    /// This defines a Boolean option
    /// </summary>
    /// <remarks>A Boolean option is one that has a name followed by a '+' for true or a '-' for false</remarks>
    public sealed class BooleanOption : BaseOption
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The Boolean option name</param>
        /// <param name="description">The Boolean option description</param>
        public BooleanOption(string name, string description) : base(name, description)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        internal override ParseResult ParseArgument(string argument)
        {
            if(argument != "+" && argument != "-")
                return ParseResult.MalformedArgument;

            if(this.IsPresent)
                return ParseResult.MultipleOccurence;

            this.Value = (argument == "+");

            return ParseResult.Success;
        }

        /// <inheritdoc />
        internal override void WriteTemplate(TextWriter writer)
        {
            writer.WriteLine("/{0}+|-", this.Name);
        }
        #endregion
    }
}
