//===============================================================================================================
// System  : Help Library Manager Launcher
// File    : HelpLibraryManagerLauncher.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/24/2015
// Note    : Copyright 2010-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the main program entry point.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/03/2010  EFW  Created the code
// 10/05/2012  EFW  Added support for Help Viewer 2.0
// 03/03/2014  EFW  Fixed FindLocaleFor() so that it works properly when multiple languages are present
// 03/24/2015  EFW  Made catalogName optional and set a default based on the viewer version
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace SandcastleBuilder.MicrosoftHelpViewer
{
    /// <summary>
    /// This class contains the main program entry point.  The application is used to perform the necessary
    /// housekeeping tasks prior to launching the Help Library Manager application to install or remove a
    /// Microsoft Help Viewer content file.
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
            string product = null, version = null, locale = null, catalogName = null, commandLine;
            int result = HelpLibraryManagerException.Success;
            bool isInstall = false, isSilent = false, showHelp = false;
            Version viewerVersion = new Version(1, 0);

            Assembly asm = Assembly.GetExecutingAssembly();

            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            Console.WriteLine("{0}, version {1}\r\n{2}\r\nE-Mail: Eric@EWoodruff.us\r\n",
                fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright);

            try
            {
                // Parse the command line arguments
                foreach(string arg in args)
                    allArgs.Add(new CommandLineArgument(arg));

                for(int idx = 0; idx < allArgs.Count; idx++)
                {
                    if(allArgs[idx].IsSwitch)
                    {
                        // This is used internally and isn't passed on
                        if(allArgs[idx].MatchesSwitch("viewerVersion") && idx < allArgs.Count - 1)
                        {
                            idx++;
                            viewerVersion = new Version(allArgs[idx].Value);
                            continue;
                        }

                        if(allArgs[idx].MatchesSwitch("catalogName") && idx < allArgs.Count - 1)
                            catalogName = allArgs[idx + 1].Value;

                        if(allArgs[idx].MatchesSwitch("product") && idx < allArgs.Count - 1)
                            product = allArgs[idx + 1].Value;

                        if(allArgs[idx].MatchesSwitch("version") && idx < allArgs.Count - 1)
                            version = allArgs[idx + 1].Value;

                        if(allArgs[idx].MatchesSwitch("locale") && idx < allArgs.Count - 1)
                            locale = allArgs[idx + 1].Value;

                        if(allArgs[idx].MatchesSwitch("install") || allArgs[idx].MatchesSwitch("sourceMedia") ||
                          allArgs[idx].MatchesSwitch("sourceUri"))
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

                if(viewerVersion.Major == 1)
                {
                    // These two are required for Help Viewer 1.0
                    if(String.IsNullOrEmpty(product))
                        throw new HelpLibraryManagerException(viewerVersion,
                            HelpLibraryManagerException.MissingCommandLineArgument, "/product");

                    if(String.IsNullOrEmpty(version))
                        throw new HelpLibraryManagerException(viewerVersion,
                            HelpLibraryManagerException.MissingCommandLineArgument, "/version");

                    // This is only used by Help Viewer 2.0
                    if(!String.IsNullOrEmpty(catalogName))
                        throw new HelpLibraryManagerException(viewerVersion,
                            HelpLibraryManagerException.InvalidCmdArgs,
                            "/catalogName is only valid for Help Viewer 2.0");
                }
                else
                {
                    if(!String.IsNullOrEmpty(product))
                        throw new HelpLibraryManagerException(viewerVersion,
                            HelpLibraryManagerException.InvalidCmdArgs,
                            "/product is only valid for Help Viewer 1.0");

                    if(!String.IsNullOrEmpty(version))
                        throw new HelpLibraryManagerException(viewerVersion,
                            HelpLibraryManagerException.InvalidCmdArgs,
                            "/version is only valid for Help Viewer 1.0");

                    // If not specified, default the catalog name based on the viewer version
                    if(String.IsNullOrEmpty(catalogName))
                    {
                        catalogName = HelpLibraryManager.DefaultCatalogName(viewerVersion);

                        if(catalogName == null)
                            throw new HelpLibraryManagerException(viewerVersion,
                                HelpLibraryManagerException.MissingCommandLineArgument, "/catalogName");

                        Console.WriteLine("Catalog name not specified, the default catalog name '{0}' will " +
                            "be used.", catalogName);
                    }
                }

                HelpLibraryManager hlm = new HelpLibraryManager(viewerVersion);

                // Can't do anything if the Help Library Manager is not installed
                if(hlm.HelpLibraryManagerPath == null)
                    throw new HelpLibraryManagerException(viewerVersion,
                        HelpLibraryManagerException.HelpLibraryManagerNotFound);

                // Can't do anything if the Help Library Manager is already running
                if(Process.GetProcessesByName(Path.GetFileNameWithoutExtension(hlm.HelpLibraryManagerPath)).Length > 0)
                    throw new HelpLibraryManagerException(viewerVersion,
                        HelpLibraryManagerException.HelpLibraryManagerAlreadyRunning);

                // Can't do anything if the local store is not initialized
                if(!hlm.LocalStoreInitialized)
                    throw new HelpLibraryManagerException(viewerVersion,
                        HelpLibraryManagerException.LocalStoreNotInitialized);

                // If not specified, try to figure out the default locale
                if(String.IsNullOrEmpty(locale))
                {
                    if(viewerVersion.Major == 1)
                    {
                        locale = hlm.FindLocaleFor(product, version);

                        if(locale == null)
                            throw new HelpLibraryManagerException(viewerVersion,
                                HelpLibraryManagerException.CatalogNotInstalled, String.Format(
                                CultureInfo.InvariantCulture, "Product: {0}  Version: {1}", product, version));
                    }
                    else
                    {
                        locale = hlm.FindLocaleFor(catalogName);

                        if(locale == null)
                            throw new HelpLibraryManagerException(viewerVersion,
                                HelpLibraryManagerException.CatalogNotInstalled, String.Format(
                                CultureInfo.InvariantCulture, "Catalog Name: {0}", catalogName));
                    }

                    Console.WriteLine("No locale specified, the default locale '{0}' will be used", locale);

                    execArgs.Add("/locale");
                    execArgs.Add(locale);
                }

                // Execute the request
                Console.WriteLine("Running Help Library Manager to perform the requested action.  Please wait...");

                commandLine = String.Join(" ", execArgs.ToArray());

                try
                {
                    // If installing, we must always run as administrator.  Everything else can run as a normal
                    // user.
                    if(isInstall)
                    {
                        if(isSilent)
                            result = hlm.RunAsAdministrator(commandLine, ProcessWindowStyle.Minimized);
                        else
                            result = hlm.RunAsAdministrator(commandLine, ProcessWindowStyle.Normal);
                    }
                    else
                    {
                        result = hlm.RunAsNormalUser(commandLine, ProcessWindowStyle.Minimized);

                        // For content manager for Help Viewer 2.0 returns almost immediately.  If there is not
                        // content to uninstall, a subsequent install attempt can fail because the other instance
                        // hasn't quite shutdown yet.  This works around the issue by pausing for a couple of
                        // seconds.
                        if(viewerVersion.Major == 2)
                            System.Threading.Thread.Sleep(2000);
                    }
                }
                catch(Exception ex)
                {
                    throw new HelpLibraryManagerException(viewerVersion, HelpLibraryManagerException.UnknownError,
                        String.Format(CultureInfo.InvariantCulture, "Failed to execute \"{0}\" {1}.\r\nError: {2}",
                        hlm.HelpLibraryManagerPath, commandLine, ex.Message));
                }

                if(result != HelpLibraryManagerException.Success)
                    throw new HelpLibraryManagerException(viewerVersion, result);

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
HelpLibraryManagerLauncher /requiredOption value ... [...]

Prefix options with '-' or '/'.  Names and values are case-insensitive.  Values
should be enclosed in double quotes if they contain spaces, commas, or other
special characters.

/help or /?         Show this help.

/viewerVersion ver  Specify 1.0 (the default) to use Help Viewer 1.0 or 2.x to
                    use Help Viewer 2.x where 'x' is the minor version.

/product id         Specify the product ID.  This option is required for Help
                    Viewer 1.0.  Omit for Help Viewer 2.0.

/version ver        Specify the product version.  This option is required for
                    Help Viewer 1.0.  Omit for Help Viewer 2.0.

/catalogName name   Specify the catalog name.  Omit for Help Viewer 1.0.
                    Optional for Help Viewer 2.x.  If not specified, it
                    defaults to the related Visual Studio version catalog.

/locale loc         Specify the locale.  Optional.  If not specified an attempt
                    is made to determine the default locale based on the
                    product and version or catalog name values.

[...]               Any other options to pass to the Help Library Manager.

Examples:

Install a help file in Help Viewer 1.0:

HelpLibraryManagerLauncher.exe /product VS /version 100
    /sourceMedia HelpContentSetup.msha

Install a help file in Help Viewer 2.0:

HelpLibraryManagerLauncher.exe /viewerVersion 2.0 /operation install
    /sourceUri HelpContentSetup.msha
");

            Console.WriteLine("\r\nRemove a help file from Help Viewer 1.0:\r\n\r\n" +
"HelpLibraryManagerLauncher.exe /product VS /version 100 /uninstall /silent\r\n" +
"    /vendor \"EWSoftware\" /productName \"Sandcastle Help File Builder\"\r\n" +
"    /mediaBookList \"Standalone Build Components\"");

            Console.WriteLine("\r\nRemove a help file from Help Viewer 2.0:\r\n\r\n" +
"HelpLibraryManagerLauncher.exe /viewerVersion 2.0 /operation uninstall " +
"    /vendor \"EWSoftware\" /productName \"Sandcastle Help File Builder\"\r\n" +
"    /bookList \"Standalone Build Components\"");
        }
        #endregion
    }
}
