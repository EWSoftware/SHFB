//===============================================================================================================
// System  : HTML to MAML Converter
// File    : ConvertHtmlToMaml.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/07/2021
// Note    : Copyright 2008-2021, Eric Woodruff, All rights reserved
//
// This file contains a utility used to convert HTML files to MAML topics and create some supporting files.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 09/12/2008  EFW  Created the code
// 08/07/2012  EFW  Incorporated various changes from Dany R
//===============================================================================================================

// Ignore Spelling: Dany

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace HtmlToMamlConversion
{
    /// <summary>
    /// This utility is used to convert HTML files to MAML topics and create some supporting files.
    /// </summary>
    public static class ConvertHtmlToMaml
    {
        private static StreamWriter swLog;

        /// <summary>
        /// Main program entry point
        /// </summary>
        /// <param name="args">The command line arguments (the source folder, the destination folder, and the
        /// optional "/companion" option to create companion files for each topic file).</param>
        /// <returns>Zero on success, non-zero on failure</returns>
        public static int Main(string[] args)
        {
            string sourceFolder = null, destFolder = null;
            bool success = false, createCompanionFile = false, moveIntro = false, badArgs = false;

            Assembly asm = Assembly.GetExecutingAssembly();

            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            Console.WriteLine("{0}, version {1}\r\n{2}\r\nE-Mail: Eric@EWoodruff.us\r\n", fvi.ProductName,
                fvi.ProductVersion, fvi.LegalCopyright);

            try
            {
                if(args.Length < 2 || args.Length > 4)
                    badArgs = true;
                else
                {
                    sourceFolder = Path.GetFullPath(args[0]);
                    destFolder = Path.GetFullPath(args[1]);

                    for(int i = 2; i < args.Length; i++)
                        if(args[i].Length > 1 && args[i].Substring(1).Equals("companion", StringComparison.OrdinalIgnoreCase))
                            createCompanionFile = true;
                        else
                            if(args[i].Length > 1 && args[i].Substring(1).Equals("moveIntro", StringComparison.OrdinalIgnoreCase))
                                moveIntro = true;
                            else
                                badArgs = true;
                }

                if(badArgs)
                {
                    Console.WriteLine("Syntax: ConvertHtmlToMaml sourceFolder destFolder [/companion] [/moveIntro]");
                    return 1;
                }

                if(!Directory.Exists(sourceFolder))
                {
                    Console.WriteLine("Source folder does not exist");
                    return 2;
                }

                if(String.Compare(sourceFolder, destFolder, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Console.WriteLine("Destination folder cannot match source folder");
                    return 3;
                }

                if(!Directory.Exists(destFolder))
                    Directory.CreateDirectory(destFolder);

                swLog = new StreamWriter(Path.Combine(destFolder, "_ConversionLog.log"));

                HtmlToMaml htmlToMaml = new HtmlToMaml(sourceFolder, destFolder, createCompanionFile, moveIntro);

                htmlToMaml.ConversionProgress += htmlToMaml_ConversionProgress;

                htmlToMaml.ConvertTopics();
                success = true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if(swLog != null)
                    swLog.Close();

#if DEBUG
                if(Debugger.IsAttached)
                {
                    Console.WriteLine("Hit ENTER...");
                    Console.ReadLine();
                }
#endif
            }

            return (success) ? 0 : 4;
        }

        /// <summary>
        /// Report progress from the conversion process
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private static void htmlToMaml_ConversionProgress(object sender, ConversionProgressEventArgs e)
        {
            swLog.WriteLine(e.Message);
            Console.WriteLine(e.Message);
        }
    }
}
