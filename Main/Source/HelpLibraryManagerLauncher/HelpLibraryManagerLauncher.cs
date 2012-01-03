//=============================================================================
// System  : Help Library Manager Launcher
// File    : StartUp.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/03/2012
// Note    : Copyright 2010-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the main program entry point.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://www.CodePlex.com/SandcastleStyles.   This
// notice, the author's name, and all copyright notices must remain intact in
// all applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  07/03/2010  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace SandcastleBuilder.MicrosoftHelpViewer
{
    /// <summary>
    /// This class contains the main program entry point.  The application is
    /// used to perform the necessary housekeeping tasks prior to launching the
    /// Help Library Manager application to install or remove a Microsoft Help
    /// Viewer content file.
    /// </summary>
    /// <remarks>The tasks performed are based on the information found at
    /// http://mshcmigrate.helpmvp.com/faq and http://mshcmigrate.helpmvp.com/faq/install.
    /// </remarks>
    public static class HelpLibraryManagerLauncher
    {
        #region Main program entry point
        //=====================================================================

        /// <summary>
        /// This is the main program entry point
        /// </summary>
        /// <param name="args">The command line arguments</param>
        /// <returns>An exit code that indicates the success or failure of the process</returns>
        public static int Main(string[] args)
        {
            List<CommandLineArgument> allArgs = new List<CommandLineArgument>();
            List<string> execArgs = new List<string>();
            string product = null, version = null, locale = null, commandLine;
            int result = HelpLibraryManagerException.Success;
            bool isInstall = false, isSilent = false, showHelp = false;

            Assembly asm = Assembly.GetExecutingAssembly();

            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            Console.WriteLine("{0}, version {1}\r\n{2}\r\nE-Mail: Eric@EWoodruff.us\r\n",
                fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright);

            try
            {
                HelpLibraryManager hlm = new HelpLibraryManager();

                // Can't do anything if the Help Library Manager is not installed
                if(hlm.HelpLibraryManagerPath == null)
                    throw new HelpLibraryManagerException(HelpLibraryManagerException.HelpLibraryManagerNotFound);

                // Can't do anything if the Help Library Manager is already running
                if(Process.GetProcessesByName(Path.GetFileNameWithoutExtension(hlm.HelpLibraryManagerPath)).Length > 0)
                    throw new HelpLibraryManagerException(HelpLibraryManagerException.HelpLibraryManagerAlreadyRunning);

                // Can't do anything if the local store is not initialized
                if(!hlm.LocalStoreInitialized)
                    throw new HelpLibraryManagerException(HelpLibraryManagerException.LocalStoreNotInitialized);

                // Parse the command line arguments
                foreach(string arg in args)
                    allArgs.Add(new CommandLineArgument(arg));

                for(int idx = 0; idx < allArgs.Count; idx++)
                {
                    if(allArgs[idx].IsSwitch)
                    {
                        if(allArgs[idx].MatchesSwitch("product") && idx < allArgs.Count - 1)
                            product = allArgs[idx + 1].Value;

                        if(allArgs[idx].MatchesSwitch("version") && idx < allArgs.Count - 1)
                            version = allArgs[idx + 1].Value;

                        if(allArgs[idx].MatchesSwitch("locale") && idx < allArgs.Count - 1)
                            locale = allArgs[idx + 1].Value;

                        if(allArgs[idx].MatchesSwitch("install") || allArgs[idx].MatchesSwitch("sourceMedia"))
                            isInstall = true;

                        if(allArgs[idx].MatchesSwitch("silent"))
                            isSilent = true;

                        if(allArgs[idx].MatchesSwitch("help") || allArgs[idx].Value[1] == '?')
                            showHelp = true;
                    }

                    execArgs.Add(allArgs[idx].ToCommandLineOption());
                }

                if(allArgs.Count == 0 || showHelp)
                {
                    ShowHelp();
                    return HelpLibraryManagerException.MissingCommandLineArgument;
                }

                // These two are required
                if(String.IsNullOrEmpty(product))
                    throw new HelpLibraryManagerException(HelpLibraryManagerException.MissingCommandLineArgument,
                        "/product");

                if(String.IsNullOrEmpty(version))
                    throw new HelpLibraryManagerException(HelpLibraryManagerException.MissingCommandLineArgument,
                        "/version");

                // If not specified, try to figure out the default locale
                if(String.IsNullOrEmpty(locale))
                {
                    locale = hlm.FindLocaleFor(product, version);

                    if(locale == null)
                        throw new HelpLibraryManagerException(HelpLibraryManagerException.CatalogNotInstalled,
                            String.Format(CultureInfo.InvariantCulture, "Product: {0}  Version: {1}", product, version));

                    Console.WriteLine("No locale specified, the default locale '{0}' will be used", locale);

                    execArgs.Add("/locale");
                    execArgs.Add(locale);
                }

                // Execute the request
                Console.WriteLine("Running Help Library Manager to perform the requested action.  Please wait...");

                commandLine = String.Join(" ", execArgs.ToArray());

                try
                {
                    // If installing, we must always run as administrator.  Everything else can
                    // run as a normal user.
                    if(isInstall)
                    {
                        if(isSilent)
                            result = hlm.RunAsAdministrator(commandLine, ProcessWindowStyle.Minimized);
                        else
                            result = hlm.RunAsAdministrator(commandLine, ProcessWindowStyle.Normal);
                    }
                    else
                        result = hlm.RunAsNormalUser(commandLine, ProcessWindowStyle.Minimized);
                }
                catch(Exception ex)
                {
                    throw new HelpLibraryManagerException(HelpLibraryManagerException.UnknownError, String.Format(
                        CultureInfo.InvariantCulture, "Failed to execute \"{0}\" {1}.\r\nError: {2}",
                        hlm.HelpLibraryManagerPath, commandLine, ex.Message));
                }

                if(result != HelpLibraryManagerException.Success)
                    throw new HelpLibraryManagerException(result);

                Console.WriteLine("The operation completed successfully");
            }
            catch(HelpLibraryManagerException hlmEx)
            {
                result = hlmEx.ErrorCode;
                Console.WriteLine("\r\nERROR: The requested operation could not be performed.\r\nDetails: {0}",
                    hlmEx.Message);
            }
            catch(Exception ex)
            {
                result = HelpLibraryManagerException.UnknownError;
                Console.WriteLine("\r\nERROR: The requested operation could not be performed.\r\nDetails: {0}",
                    ex.ToString());
            }
#if DEBUG
            if(System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Hit ENTER to exit...");
                Console.ReadLine();
            }
#endif
            return result;
        }
        #endregion

        #region Show command line help
        //=====================================================================

        /// <summary>
        /// Show command line help
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine(@"Syntax:
HelpLibraryManagerLauncher /product id /version ver /locale loc [...]

Prefix options with '-' or '/'.  Names and values are case-insensitive.  Values
should be enclosed in double quotes if they contain spaces, commas, or other
special characters.

/help or /?         Show this help.

/product id         Specify the product ID.  This option is required.

/version ver        Specify the product version.  This option is required.

/locale loc         Specify the locale.  Optional.  If not specified an attempt
                    is made to determine the default locale based on the
                    product and version values.

[...]               Any other options to pass to the Help Library Manager.

Examples:

Install a help file:

HelpLibraryManagerLauncher.exe /product VS /version 100 /brandingPackage
    Dev10.mshc /sourceMedia HelpContentSetup.msha");

            Console.WriteLine("Remove a help file:\r\n" +
"HelpLibraryManagerLauncher.exe /product VS /version 100 /uninstall /silent\r\n" +
"    /vendor \"EWSoftware\" /mediaBookList \"Standalone Build Components\"\r\n" +
"    /productName \"Sandcastle Help File Builder\"");
        }
        #endregion
    }
}
