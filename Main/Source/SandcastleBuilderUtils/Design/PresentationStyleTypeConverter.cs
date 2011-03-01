//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : PresenationStyleTypeConverter.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/16/2011
// Note    : Copyright 2007-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a type converter that allows you to select a presentation
// style folder from those currently installed in the .\Presentation folder
// found in the main installation folder of Sandcastle.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  08/08/2006  EFW  Created the code
// 1.5.0.0  06/19/2007  EFW  Updated for use with the June CTP
//=============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This type converter allows you to select a presentation style folder
    /// from those currently installed in the <b>.\Presentation</b> folder
    /// found in the main installation folder of Sandcastle.
    /// </summary>
    internal sealed class PresentationStyleTypeConverter : StringConverter
    {
        #region Private data members
        //=====================================================================

        private static List<string> styles = new List<string>();
        private static StandardValuesCollection standardValues =
            PresentationStyleTypeConverter.InitializeStandardValues();
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This returns the default style
        /// </summary>
        /// <value>Returns <b>vs2005</b> if present.  If not, it returns the
        /// first best match or, failing that, the first style in the list.</value>
        public static string DefaultStyle
        {
            get
            {
                string defaultStyle = "vs2005";

                if(!IsPresent(defaultStyle))
                    defaultStyle = FirstMatching(defaultStyle);

                return defaultStyle;
            }
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// This is used to get the standard values by searching for the
        /// .NET Framework versions installed on the current system.
        /// </summary>
        private static StandardValuesCollection InitializeStandardValues()
        {
            string folder;

            try
            {
                // Try the DXROOT environment variable first
                folder = Environment.GetEnvironmentVariable("DXROOT");

                if(String.IsNullOrEmpty(folder) || !folder.Contains(@"\Sandcastle"))
                    folder = String.Empty;

                // Try to find Sandcastle based on the path if not there
                if(folder.Length == 0)
                {
                    Match m = Regex.Match(Environment.GetEnvironmentVariable("PATH"),
                        @"[A-Z]:\\.[^;]+\\Sandcastle(?=\\Prod)", RegexOptions.IgnoreCase);

                    // If not found in the path, search all fixed drives
                    if(m.Success)
                        folder = m.Value;
                    else
                    {
                        folder = BuildProcess.FindOnFixedDrives(@"\Sandcastle");

                        // If not found there, try the VS 2005 SDK folders
                        if(folder.Length == 0)
                        {
                            folder = BuildProcess.FindSdkExecutable("MRefBuilder.exe");

                            if(folder.Length != 0)
                                folder = folder.Substring(0, folder.LastIndexOf('\\'));
                        }
                    }
                }

                if(folder.Length != 0)
                {
                    folder += @"\Presentation";

                    // The Shared folder is omitted as it contains files
                    // common to all presentation styles.
                    foreach(string s in Directory.EnumerateDirectories(folder))
                        if(!s.EndsWith("Shared", StringComparison.Ordinal))
                            styles.Add(s.Substring(s.LastIndexOf('\\') + 1));
                }
            }
            catch(Exception)
            {
                // Eat the exception.  If we can't find Sandcastle here, the
                // build will fail too so the user will be notified then.
            }

            // Add the three basic styles as placeholders if nothing was found.
            // The build will fail but the project will still load.
            if(styles.Count == 0)
            {
                styles.Add("hana");
                styles.Add("Prototype");
                styles.Add("vs2005");
            }

            styles.Sort();

            return new StandardValuesCollection(styles);
        }

        /// <summary>
        /// This is overridden to return the values for the type converter's
        /// dropdown list.
        /// </summary>
        /// <param name="context">The format context object</param>
        /// <returns>Returns the standard values for the type</returns>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return standardValues;
        }

        /// <summary>
        /// This is overridden to indicate that the values are exclusive
        /// and values outside the list cannot be entered.
        /// </summary>
        /// <param name="context">The format context object</param>
        /// <returns>Always returns true</returns>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// This is overridden to indicate that standard values are supported
        /// and can be chosen from a list.
        /// </summary>
        /// <param name="context">The format context object</param>
        /// <returns>Always returns true</returns>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// This is used to find out if the specified style is present on the
        /// system.
        /// </summary>
        /// <param name="style">The style for which to look</param>
        /// <returns>True if present, false if not found</returns>
        public static bool IsPresent(string style)
        {
            return styles.Contains(style);
        }

        /// <summary>
        /// This is used to get the first style that matches case-insensitively
        /// or, failing that, starts with or contains the given value
        /// case-insensitively.
        /// </summary>
        /// <param name="style">The style for which to look</param>
        /// <returns>The best match or the first style if not found.</returns>
        public static string FirstMatching(string style)
        {
            string compareStyle;

            if(!String.IsNullOrEmpty(style))
            {
                // Try for a case-insensitive match first
                foreach(string s in styles)
                    if(String.Compare(s, style, StringComparison.OrdinalIgnoreCase) == 0)
                        return s;

                // Try for the closest match
                style = style.ToLower(CultureInfo.InvariantCulture);

                foreach(string s in styles)
                {
                    compareStyle = s.ToLower(CultureInfo.InvariantCulture);

                    if(compareStyle.StartsWith(style, StringComparison.Ordinal) || compareStyle.Contains(style))
                        return s;
                }
            }

            // Not found, return the first style
            return styles[0];
        }
        #endregion
    }
}
