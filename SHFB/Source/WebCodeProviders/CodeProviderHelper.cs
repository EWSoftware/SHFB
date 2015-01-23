//=============================================================================
// System  : EWSoftware Custom Code Providers
// File    : CodeProviderHelper.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/03/2009
// Note    : Copyright 2008-2009, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains some common helper methods used by the code providers.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: https://GitHub.com/EWSoftware/SHFB.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  04/16/2008  EFW  Created the code
//=============================================================================

using System;
using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace EWSoftware.CodeDom
{
    /// <summary>
    /// This is a helper class that contains items common to all of the code
    /// providers.
    /// </summary>
    internal static class CodeProviderHelper
    {
        #region Private data members
        //=====================================================================

        // The purged flag
        private static bool docFilesPurged;

        // Regex used to find the /doc option
        private static Regex reDocPathOpt = new Regex(
            "[/-]docpath:(\"([^\"])+\"|([^\\s\"])+)", RegexOptions.IgnoreCase);
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to extract the "/docpath" parameter from the
        /// compiler options and set the XML comments compiler option.
        /// </summary>
        /// <param name="options">The compiler parameters</param>
        /// <param name="additionalOptions">Other command line options to add
        /// or null if there are none.</param>
        /// <remarks>If a "/docpath" option is not found, no comments will be
        /// produced for the compiled items.</remarks>
        internal static void ReplaceDocPathOption(CompilerParameters options,
          string[] additionalOptions)
        {
            Match m;
            string docPath, docFile;

            // Replace /docpath with /doc
            if(!String.IsNullOrEmpty(options.CompilerOptions))
            {
                m = reDocPathOpt.Match(options.CompilerOptions);

                if(m.Success)
                {
                    docPath = m.Groups[1].Value.Replace("\"", String.Empty);

                    if(!Directory.Exists(docPath))
                    {
                        Directory.CreateDirectory(docPath);
                        docFilesPurged = true;
                    }
                    else
                        if(!docFilesPurged)
                        {
                            // Purge the comments files from the folder on
                            // the first call.
                            foreach(string file in Directory.GetFiles(
                              docPath, "App_*.xml"))
                                File.Delete(file);

                            docFilesPurged = true;
                        }

                    docFile = Path.Combine(docPath, Path.GetFileName(
                        Path.ChangeExtension(options.OutputAssembly, ".xml")));

                    options.CompilerOptions = String.Format(
                        CultureInfo.InvariantCulture, "{0} /doc:\"{1}\"",
                        reDocPathOpt.Replace(options.CompilerOptions,
                        String.Empty), docFile);
                }
            }

            // Append the other options
            if(additionalOptions != null && additionalOptions.Length != 0)
                options.CompilerOptions = String.Concat(options.CompilerOptions,
                    " ", String.Join(" ", additionalOptions));
        }
        #endregion
    }
}
