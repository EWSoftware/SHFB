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
    /// This defines a switch option
    /// </summary>
    /// <remarks>A switch option is one that is only represented by its name</remarks>
    public sealed class SwitchOption : BaseOption
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The switch option name</param>
        /// <param name="description">The switch option description</param>
        public SwitchOption(string name, string description) : base(name, description)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        internal override ParseResult ParseArgument(string argument)
        {
            if(argument.Length > 0)
                return ParseResult.MalformedArgument;

            if(this.IsPresent)
                return ParseResult.MultipleOccurrence;

            this.Value = this.Name;

            return ParseResult.Success;
        }

        /// <inheritdoc />
        internal override void WriteTemplate(TextWriter writer)
        {
            writer.WriteLine("/{0}", this.Name);
        }
        #endregion
    }
}
