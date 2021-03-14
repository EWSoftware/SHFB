//===============================================================================================================
// System  : Sandcastle Reflection Data Manager
// File    : App.xaml.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/13/2021
// Note    : Copyright 2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the startup code for the Reflection Data Manager tool
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/21/2015  EFW  Created the code
//===============================================================================================================

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

using Microsoft.Build.Locator;

using Sandcastle.Core;
using Sandcastle.Core.CommandLine;
using Sandcastle.Core.Reflection;

namespace ReflectionDataManager
{
    /// <summary>
    /// This contains the startup code for the reflection data manager tool
    /// </summary>
    public partial class App : Application, IProgress<string>
    {
        #region Method imports
        //=====================================================================

        /// <summary>
        /// This is used to hide the console window on startup when running interactively
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nCmdShow"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, Int32 nCmdShow);

        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// This is overridden to handle command line builds
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            int exitCode = 0;

            base.OnStartup(e);

            try
            {
                MSBuildLocator.RegisterDefaults();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Unable to register MSBuild defaults: " + ex.Message + "\r\n\r\n" +
                    "You probably need to install the Microsoft Build Tools for Visual Studio 2017 or later.");
                return;
            }

            // If command line options are present, perform a build
            if(e.Args.Length != 0)
            {
                ConsoleApplication.WriteBanner();

                // Specify options
                OptionCollection options = new OptionCollection
                {
                    new SwitchOption("?", "Show this help page."),
                    new StringOption("platform", "Specify the platform to use for the build", "platformName")
                    { RequiredMessage = "A platform parameter value is required" },
                    new StringOption("version", "Specify the version to use for the build.  If not " +
                        "specified, the most recent version for the specified platform is used.", "version"),
                    new ListOption("path", "Specify additional paths to search for reflection data set " +
                        "files if necessary.", "dataSetPath")
                };

                // Process options
                ParseArgumentsResult parsedArguments = options.ParseArguments(e.Args);

                if(parsedArguments.Options["?"].IsPresent)
                {
                    Console.WriteLine("ReflectionDataManager [options]");
                    options.WriteOptionSummary(Console.Out);
                    exitCode = 1;
                }
                else
                    if(!parsedArguments.Success)
                    {
                        parsedArguments.WriteParseErrors(Console.Out);
                        exitCode = 1;
                    }

                if(exitCode == 0)
                    exitCode = PerformBuild(parsedArguments);
                
                this.Shutdown(exitCode);
                return;
            }

            // Run interactively
            IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;

            // If we own the console, hide it
            if(hWnd != IntPtr.Zero)
                ShowWindow(hWnd, 0);

            new MainWindow().ShowDialog();
            this.Shutdown();
        }
        #endregion

        #region Build method
        //=====================================================================

        /// <summary>
        /// Build the reflection data based on the command line arguments
        /// </summary>
        /// <param name="arguments">The command line arguments</param>
        /// <returns>Zero if successful, or a non-zero value on failure</returns>
        private int PerformBuild(ParseArgumentsResult arguments)
        {
            ReflectionDataSetDictionary rdsd;
            ReflectionDataSet dataSet;
            Version version = new Version();
            string platform;
            int exitCode = 0;

            try
            {
                platform = (string)arguments.Options["platform"].Value;

                if(arguments.Options["version"].IsPresent && !Version.TryParse(
                  (string)arguments.Options["version"].Value, out version))
                {
                    Console.WriteLine("Invalid version value");
                    return 1;
                }

                if(arguments.Options["path"].IsPresent)
                    rdsd = new ReflectionDataSetDictionary((string[])arguments.Options["path"].Value);
                else
                    rdsd = new ReflectionDataSetDictionary(null);

                if(version.Major != 0)
                    dataSet = rdsd.CoreFrameworkMatching(platform, version, true);
                else
                    dataSet = rdsd.CoreFrameworkMostRecent(platform);

                if(dataSet == null)
                {
                    Console.WriteLine("A suitable framework could not be found for the given parameters");
                    return 1;
                }

                Console.WriteLine("Building reflection data for {0} found in {1}", dataSet.Title,
                    dataSet.Filename);

                using(var bp = new BuildProcess(dataSet) { ProgressProvider = this })
                {
                    bp.Build();
                }
            }
            catch(Exception ex)
            {
                exitCode = 1;
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                Console.WriteLine("\r\n\r\nUnable to generate reflection data files: " + ex.Message + "\r\n");
            }

            return exitCode;
        }
        #endregion

        #region IProgress<string> Members
        //=====================================================================

        /// <inheritdoc />
        public void Report(string value)
        {
            Console.WriteLine(value);
        }

        #endregion
    }
}
