// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 03/10/2012 - EFW - Updated code to fix FxCop warnings.  Reworked to use Collection<T> as the base class to
// simplify the code.  Added code to property support required options.  Added support or '-' option prefix.
// 12/21/2013 - EFW - Moved class to Sandcastle.Core assembly

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Sandcastle.Core.CommandLine
{
    /// <summary>
    /// This collection is used to hold a set of command line option definitions
    /// </summary>
    public sealed class OptionCollection : Collection<BaseOption>
    {
        #region Private data members
        //=====================================================================

        private readonly Dictionary<string, BaseOption> map = new Dictionary<string, BaseOption>();

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property can be used to retrieve an option by name
        /// </summary>
        /// <param name="name">The name of the option to retrieve</param>
        /// <returns></returns>
        public BaseOption this[string name]
        {
            get
            {
                if(map.TryGetValue(name, out BaseOption option))
                    return option;

                return null;
            }
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override void InsertItem(int index, BaseOption item)
        {
            if(item == null)
                throw new ArgumentNullException(nameof(item));

            base.InsertItem(index, item);
            map[item.Name] = item;
        }

        /// <inheritdoc />
        protected override void ClearItems()
        {
            base.ClearItems();
            map.Clear();
        }

        /// <inheritdoc />
        protected override void RemoveItem(int index)
        {
            BaseOption o = base[index];

            base.RemoveItem(index);
            map.Remove(o.Name);
        }

        /// <inheritdoc />
        protected override void SetItem(int index, BaseOption item)
        {
            if(item == null)
                throw new ArgumentNullException(nameof(item));

            BaseOption o = base[index];

            base.SetItem(index, item);

            map.Remove(o.Name);
            map[item.Name] = item;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Parse an array of command line option strings into command line option instances
        /// </summary>
        /// <param name="args">The array of options to parse</param>
        /// <returns>The results of parsing the command line option strings</returns>
        public ParseArgumentsResult ParseArguments(string[] args)
        {
            if(args == null)
                throw new ArgumentNullException(nameof(args));

            ParseArgumentsResult results = new ParseArgumentsResult(this);

            this.ParseArguments(args, results);

            // Make sure the required options were present
            foreach(BaseOption option in this.Where(o => !o.IsPresent && !String.IsNullOrEmpty(o.RequiredMessage)))
                results.AddError(option.Name, ParseResult.MissingOption);

            return results;
        }

        /// <summary>
        /// This is used to write out a summary of the options
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the summary is written</param>
        /// <exception cref="ArgumentNullException">This is thrown if the <paramref name="writer"/> parameter
        /// is null.</exception>
        public void WriteOptionSummary(TextWriter writer)
        {
            if(writer == null)
                throw new ArgumentNullException(nameof(writer));

            foreach(BaseOption option in this)
            {
                writer.WriteLine();
                option.WriteTemplate(writer);
                writer.WriteLine(option.Description);
            }
        }

        /// <summary>
        /// This is used to parse the command line options and return the results
        /// </summary>
        /// <param name="args">The array of option strings to parse</param>
        /// <param name="results">The results of the parsing operation</param>
        private void ParseArguments(string[] args, ParseArgumentsResult results)
        {
            foreach(string arg in args)
            {
                if(arg.Length == 0)
                    continue;

                // Is it an option?
                if(arg[0] == '/' || arg[0] == '-')
                {
                    // Find the named option
                    int index = 1;

                    while(index < arg.Length && (Char.IsLetter(arg, index) || arg[index] == '?'))
                        index++;

                    string key = arg.Substring(1, index - 1);
                    string value = arg.Substring(index);

                    // Invoke the appropriate logic
                    if(map.ContainsKey(key))
                    {
                        BaseOption option = map[key];
                        ParseResult result = option.ParseArgument(value);

                        if(result != ParseResult.Success)
                            results.AddError(arg, result);
                    }
                    else
                        results.AddError(arg, ParseResult.UnrecognizedOption);
                }
                else
                    if(arg[0] == '@')   // Is it a response file?
                        this.ParseArguments(File.ReadAllLines(arg.Substring(1)), results);
                    else
                        results.AddNonOption(arg);      // Non-option
            }
        }
        #endregion
    }
}
