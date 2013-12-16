// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using Microsoft.Ddue.Tools.CommandLine;

namespace Microsoft.Ddue.Tools
{
    class MSHCPackager
    {
        public static int Main(string[] args)
        {
            BaseOption v_operation = null;
            BaseOption v_extractOperation;
            BaseOption v_saveOperation;
            BaseOption v_copyOperation;
            String v_mshcFile = null;
            String v_sourceDirectory = null;
            String v_targetDirectory = null;
            String v_manifestFile = null;

            // specify options
            OptionCollection v_options = new OptionCollection();
            v_options.Add(new SwitchOption("?", "Show this help."));
            v_options.Add(v_extractOperation = new SwitchOption("extract", "Extract files from a help package."));
            v_options.Add(v_saveOperation = new SwitchOption("save", "Create a help package from a set of files."));
            v_options.Add(v_copyOperation = new SwitchOption("copy", "Copy selected package files."));
            v_options.Add(new StringOption("manifest", "Specify manifest file name", "manifest-file"));
            v_options.Add(new ListOption("arg", "Specify manifest arguments. An argument with no value is set to 'true'.", "name[=value]"));
            v_options.Add(new SwitchOption("r", "Replace existing targets."));

            ConsoleApplication.WriteBanner();
            // process options
            ParseArgumentsResult v_parseResults = v_options.ParseArguments(args);
            if(v_parseResults.Options["?"].IsPresent)
            {
                Console.WriteLine("MSHCPackager /extract mshc_file target_directory [/manifest:manifest_file [/arg:...]]");
                Console.WriteLine("MSHCPackager /save source_directory mshc_file");
                Console.WriteLine("MSHCPackager /copy target_directory /manifest:manifest_file [/arg:...]");
                v_options.WriteOptionSummary(Console.Out);
                return (0);
            }

            // check for invalid options
            if(!v_parseResults.Success)
            {
                v_parseResults.WriteParseErrors(Console.Out);
                return (1);
            }

            if(v_parseResults.Options[v_extractOperation.Name].IsPresent)
            {
                v_operation = v_extractOperation;
            }
            if(v_parseResults.Options[v_saveOperation.Name].IsPresent)
            {
                if(v_operation == null)
                {
                    v_operation = v_saveOperation;
                }
                else
                {
                    Console.WriteLine("Specify either extract, save, or copy.");
                }
            }
            if(v_parseResults.Options[v_copyOperation.Name].IsPresent)
            {
                if(v_operation == null)
                {
                    v_operation = v_copyOperation;
                }
                else
                {
                    Console.WriteLine("Specify either extract, save, or copy.");
                }
            }
            if(v_operation == null)
            {
                Console.WriteLine("Specify extract, save, or copy.");
                return (1);
            }

            if(v_parseResults.Options["manifest"].IsPresent)
            {
                v_manifestFile = v_parseResults.Options["manifest"].Value as String;
            }

            foreach(String v_argument in v_parseResults.UnusedArguments)
            {
                if(v_operation == v_extractOperation)
                {
                    if(string.IsNullOrEmpty(v_mshcFile))
                    {
                        v_mshcFile = v_argument;
                    }
                    else if(string.IsNullOrEmpty(v_targetDirectory))
                    {
                        v_targetDirectory = v_argument;
                    }
                    else
                    {
                        Console.WriteLine("Too many arguments '{0}'.", v_argument);
                        return (1);
                    }
                }
                else if(v_operation == v_saveOperation)
                {
                    if(string.IsNullOrEmpty(v_sourceDirectory))
                    {
                        v_sourceDirectory = v_argument;
                    }
                    else if(string.IsNullOrEmpty(v_mshcFile))
                    {
                        v_mshcFile = v_argument;
                    }
                    else
                    {
                        Console.WriteLine("Too many arguments '{0}'.", v_argument);
                        return (1);
                    }
                }
                else if(v_operation == v_copyOperation)
                {
                    if(string.IsNullOrEmpty(v_targetDirectory))
                    {
                        v_targetDirectory = v_argument;
                    }
                    else
                    {
                        Console.WriteLine("Too many arguments '{0}'.", v_argument);
                        return (1);
                    }
                }
            }

            if(v_operation == v_extractOperation)
            {
                if(string.IsNullOrEmpty(v_mshcFile))
                {
                    Console.WriteLine("No help package specified.");
                    return (1);
                }
                else if(string.IsNullOrEmpty(v_targetDirectory))
                {
                    Console.WriteLine("No target directory specified.");
                    return (1);
                }
            }
            else if(v_operation == v_saveOperation)
            {
                if(string.IsNullOrEmpty(v_mshcFile))
                {
                    Console.WriteLine("No help package specified.");
                    return (1);
                }
                else if(string.IsNullOrEmpty(v_sourceDirectory))
                {
                    Console.WriteLine("No source directory specified.");
                    return (1);
                }
                else if(!string.IsNullOrEmpty(v_manifestFile))
                {
                    Console.WriteLine("Save does not use a manifest file.");
                    return (1);
                }
                else if(v_parseResults.Options["arg"].IsPresent)
                {
                    Console.WriteLine("Save does not use manifest file arguments.");
                    return (1);
                }
            }
            else if(v_operation == v_copyOperation)
            {
                if(string.IsNullOrEmpty(v_targetDirectory))
                {
                    Console.WriteLine("No target directory specified.");
                    return (1);
                }
                else if(string.IsNullOrEmpty(v_manifestFile))
                {
                    Console.WriteLine("No manifest specified.");
                    return (1);
                }
            }

            Boolean v_replaceTargets = v_parseResults.Options["r"].IsPresent;

            if(v_operation == v_extractOperation)
            {
                MSHCPackage v_sourcePackage = new MSHCPackage(v_mshcFile, FileMode.Open);

                if(v_sourcePackage.IsOpen)
                {
                    Console.WriteLine("{0} -> {1}", v_sourcePackage.PackagePath, v_targetDirectory);
                    v_sourcePackage.LoggingTarget = Console.Out;
                    v_sourcePackage.LoggingPrefix = "  ";

                    if(String.IsNullOrEmpty(v_manifestFile))
                    {
                        v_sourcePackage.GetAllParts(v_targetDirectory, v_replaceTargets);
                    }
                    else
                    {
                        PutManifestArguments(v_parseResults, v_sourcePackage);
                        v_sourcePackage.GetTheseParts(v_targetDirectory, v_manifestFile, v_replaceTargets);
                    }
                }
                else
                {
                    Console.WriteLine("Unable to open {0}", v_mshcFile);
                    return (1);
                }
            }
            else if(v_operation == v_saveOperation)
            {
                MSHCPackage v_sourcePackage = new MSHCPackage(v_mshcFile, FileMode.Create);

                if(v_sourcePackage.IsOpen)
                {
                    v_sourcePackage.LoggingTarget = Console.Out;
                    v_sourcePackage.PutAllParts(v_sourceDirectory, v_replaceTargets);
                }
                else
                {
                    Console.WriteLine("Unable to create {0}", v_mshcFile);
                    return (1);
                }
            }
            else if(v_operation == v_copyOperation)
            {
                MSHCPackage v_dummyPackage = new MSHCPackage(v_targetDirectory);

                Console.WriteLine("{0} -> {1}", Path.GetDirectoryName(v_manifestFile), v_targetDirectory);
                v_dummyPackage.LoggingTarget = Console.Out;
                v_dummyPackage.LoggingPrefix = "  ";

                PutManifestArguments(v_parseResults, v_dummyPackage);
                v_dummyPackage.CopyTheseParts(v_manifestFile, v_replaceTargets);
            }

            return (0);
        }

        private static void PutManifestArguments(ParseArgumentsResult parseResults, MSHCPackage package)
        {
            if(parseResults.Options["arg"].IsPresent)
            {
                String[] v_nameValueStrings = (String[])parseResults.Options["arg"].Value;
                foreach(String v_nameValueString in v_nameValueStrings)
                {
                    String[] v_nameValuePair = v_nameValueString.Split('=');
                    if(v_nameValuePair.Length == 2)
                    {
                        package.ManifestProperties.Add(v_nameValuePair[0], v_nameValuePair[1]);
                    }
                    else if(v_nameValuePair.Length == 1)
                    {
                        package.ManifestProperties.Add(v_nameValuePair[0], Boolean.TrueString);
                    }
                }
            }
        }
    }
}
