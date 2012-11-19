//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : PresentationStyleDictionary.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/18/2012
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using SandcastleBuilder.Utils.BuildComponent;

namespace SandcastleBuilder.Utils.PresentationStyle
{
    /// <summary>
    /// This dictionary contains the settings for all known Sandcastle presentation styles
    /// </summary>
    public sealed class PresentationStyleDictionary : Dictionary<string, PresentationStyleSettings>
    {
        #region Private data members
        //=====================================================================

        private static PresentationStyleDictionary styles;
        #endregion

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

        /// <summary>
        /// This read-only property returns an enumerable list of third-party presentation style definition files
        /// </summary>
        /// <remarks>Third party presentation styles should be located in an
        /// "EWSoftware\Sandcastle Help File Builder\Presentation Styles" folder in the common application data
        /// folder.  It will also return other presentation style definition files from the SHFB folder.</remarks>
        public static IEnumerable<string> ThirdPartyPresentationStyleDefinitions
        {
            get
            {
                string folder = FolderPath.TerminatePath(Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.CommonApplicationData), Constants.PresentationStylesFolder));

                if(Directory.Exists(folder))
                    foreach(string file in Directory.EnumerateFiles(folder, "*.presentation", SearchOption.AllDirectories))
                        yield return file;

                foreach(string file in Directory.EnumerateFiles(BuildComponentManager.HelpFileBuilderFolder,
                  "*.presentation", SearchOption.AllDirectories))
                    if(!Path.GetFileNameWithoutExtension(file).Equals("StandardStyles", StringComparison.OrdinalIgnoreCase))
                        yield return file;
            }
        }

        /// <summary>
        /// This read-only property returns the values in the collection
        /// </summary>
        public static PresentationStyleDictionary AllStyles
        {
            get
            {
                if(styles == null)
                    styles = LoadStandardPresentationStyleDictionary();

                return styles;
            }
        }

        /// <summary>
        /// This is used to get the default presentation style to use
        /// </summary>
        /// <remarks>The default is the VS2005 style</remarks>
        public static string DefaultStyle
        {
            get
            {
                PresentationStyleSettings pss = null;

                if(!AllStyles.TryGetValue("VS2005", out pss))
                    pss = AllStyles.Values.First();

                return pss.Id;
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
            PresentationStyleDictionary psd, thirdPartyPSD;

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

            try
            {
                foreach(string file in ThirdPartyPresentationStyleDefinitions)
                {
                    thirdPartyPSD = FromXml(file);

                    // Add the definitions to the main dictionary.  We do not allow overriding existing keys
                    // to avoid any issues with a third-party style overriding a default style without warning.
                    foreach(var settings in thirdPartyPSD)
                        psd.Add(settings.Key, settings.Value);
                }
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
