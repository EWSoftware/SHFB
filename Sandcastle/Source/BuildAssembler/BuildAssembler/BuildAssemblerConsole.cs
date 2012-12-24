// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 02/16/2012 - EFW - Added support for setting a verbosity level.  Messages with a log level below
// the current verbosity level are ignored.

using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools.CommandLine;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This controls the BuildAssembler process
    /// </summary>
    class BuildAssemblerConsole
    {
        /// <summary>
        /// Main program entry point
        /// </summary>
        /// <param name="args">The command line arguments</param>
        /// <returns>Zero on success or one on failure</returns>
        public static int Main(string[] args)
        {
            ConsoleApplication.WriteBanner();

            #region Read command line arguments, and setup config

            // Specify options
            OptionCollection options = new OptionCollection();
            options.Add(new SwitchOption("?", "Show this help page."));
            options.Add(new StringOption("config", "Specify a configuration file.", "configFilePath"));

            // Process options
            ParseArgumentsResult results = options.ParseArguments(args);

            // Process help option
            if(results.Options["?"].IsPresent)
            {
                Console.WriteLine("BuildAssembler [options] manifestFilename");
                options.WriteOptionSummary(Console.Out);
                return 1;
            }

            // check for invalid options
            if(!results.Success)
            {
                results.WriteParseErrors(Console.Out);
                return 1;
            }

            // Check for manifest
            if(results.UnusedArguments.Count != 1)
            {
                Console.WriteLine("You must supply exactly one manifest file.");
                return 1;
            }

            string manifest = results.UnusedArguments[0];

            // Load the configuration file
            XPathDocument configuration;

            try
            {
                if(results.Options["config"].IsPresent)
                    configuration = ConsoleApplication.GetConfigurationFile((string)results.Options["config"].Value);
                else
                    configuration = ConsoleApplication.GetConfigurationFile();
            }
            catch(IOException e)
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, "The specified configuration file could not " +
                    "be loaded. The error message is: {0}", e.Message);
                return 1;
            }
            catch(XmlException e)
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, "The specified configuration file is not " +
                    "well-formed. The error message is: {0}", e.Message);
                return 1;
            }
            #endregion

            // Create a BuildAssembler instance to do the work
            BuildAssembler buildAssembler = new BuildAssembler();

            try
            {
                XPathNavigator configNav = configuration.CreateNavigator();

                // See if a verbosity level has been specified.  If so, set it.
                var verbosity = configNav.SelectSingleNode("/configuration/@verbosity");
                MessageLevel level;

                if(verbosity == null || !Enum.TryParse<MessageLevel>(verbosity.Value, out level))
                    level = MessageLevel.Info;

                BuildAssembler.VerbosityLevel = level;

                if(level > MessageLevel.Info)
                    ConsoleApplication.WriteMessage(LogLevel.Info, "Loading configuration...");

                // Load the context
                XPathNavigator contextNode = configNav.SelectSingleNode("/configuration/dduetools/builder/context");

                if(contextNode != null)
                    buildAssembler.Context.Load(contextNode);

                // Load the build components
                XPathNavigator componentsNode = configNav.SelectSingleNode("/configuration/dduetools/builder/components");

                if(componentsNode != null)
                    buildAssembler.AddComponents(componentsNode);

                // Proceed through the build manifest, processing all topics named there
                if(level > MessageLevel.Info)
                    ConsoleApplication.WriteMessage(LogLevel.Info, "Processing topics...");

                int count = buildAssembler.Apply(manifest);

                ConsoleApplication.WriteMessage(LogLevel.Info, "Processed {0} topics", count);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                ConsoleApplication.WriteMessage(LogLevel.Error, ex.ToString());
            }
            finally
            {
                buildAssembler.Dispose();
            }

            return 0;
        }
    }
}
