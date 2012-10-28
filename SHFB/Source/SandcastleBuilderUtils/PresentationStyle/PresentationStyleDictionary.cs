//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : PresentationStyleDictionary.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/24/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class representing a dictionary of Sandcastle presentation style settings
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.   This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.6.0  10/24/2012  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

using SandcastleBuilder.Utils.BuildComponent;

// TODO: This needs to load 3rd party style definitions from the common app data folder too like build components and plug-ins.

namespace SandcastleBuilder.Utils.PresentationStyle
{
    /// <summary>
    /// This dictionary contains the settings for all known Sandcastle presentation styles
    /// </summary>
    public sealed class PresentationStyleDictionary : Dictionary<string, PresentationStyleSettings>
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the path to the standard presentation style definition file
        /// </summary>
        public static string PresentationStyleFilePath
        {
            get
            {
                return Path.Combine(BuildComponentManager.HelpFileBuilderFolder, "StandardStyles.presentation");
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Private constructor
        /// </summary>
        /// <remarks>Keys are case-insensitive</remarks>
        private PresentationStyleDictionary() : base(StringComparer.OrdinalIgnoreCase)
        {
        }
        #endregion

        #region Methods use to convert from XML
        //=====================================================================

        /// <summary>
        /// This is used to load the presentation style dictionary used by the Sandcastle tools
        /// </summary>
        /// <returns>The standard presentation style dictionary is loaded from the <b>StandardStyles.presentation</b>
        /// file located in the Sandcastle Help File Builder folder.  If the file cannot be found, an empty
        /// dictionary is returned.</returns>
        public static PresentationStyleDictionary LoadStandardPresentationStyleDictionary()
        {
            PresentationStyleDictionary psd;

            if(!File.Exists(PresentationStyleFilePath))
                throw new FileNotFoundException("Unable to locate standard presentation style definition file");

            try
            {
                psd = FromXml(PresentationStyleFilePath);
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException("Unable to parse standard presentation style definition file", ex);
            }

            return psd;
        }

        /// <summary>
        /// This is used to load a presentation style dictionary XML file
        /// </summary>
        /// <param name="filename">The filename to load</param>
        /// <returns>The new presentation style dictionary containing the file's content</returns>
        public static PresentationStyleDictionary FromXml(string filename)
        {
            PresentationStyleDictionary psd = new PresentationStyleDictionary();
            PresentationStyleSettings pss;
            XDocument presentationStyles = XDocument.Load(filename);
            string parentFilePath = Path.GetDirectoryName(Path.GetFullPath(filename));

            foreach(var style in presentationStyles.Descendants("PresentationStyle"))
            {
                pss = PresentationStyleSettings.FromXml(parentFilePath, style);
                psd.Add(pss.Id, pss);
            }

            return psd;
        }
        #endregion
    }
}
