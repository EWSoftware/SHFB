//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : SpellCheckerConfiguration.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/13/2014
// Note    : Copyright 2013-2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to contain the spell checker's configuration settings
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/11/2013  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: lt arn az Cyrl Latn

using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

using Sandcastle.Core;

namespace SandcastleBuilder.Gui.Spelling
{
    /// <summary>
    /// This class is used to contain the spell checker's configuration
    /// </summary>
    internal static class SpellCheckerConfiguration
    {
        #region Private data members
        //=====================================================================

        private static HashSet<string> ignoredXmlElements, spellCheckedXmlAttributes;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the configuration file path
        /// </summary>
        /// <value>This location is also where custom dictionary files are located</value>
        public static string ConfigurationFilePath
        {
            get
            {
                string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    Constants.ProgramDataFolder);

                if(!Directory.Exists(configPath))
                    Directory.CreateDirectory(configPath);

                return configPath;
            }
        }

        /// <summary>
        /// This is used to get or set the default language for the spell checker
        /// </summary>
        /// <remarks>The default is to use the English US dictionary (en-US)</remarks>
        public static CultureInfo DefaultLanguage { get; set; }

        /// <summary>
        /// This is used to get or set whether or not to ignore words containing digits
        /// </summary>
        /// <value>This is true by default</value>
        public static bool IgnoreWordsWithDigits { get; set; }

        /// <summary>
        /// This is used to get or set whether or not to ignore words in all uppercase
        /// </summary>
        /// <value>This is true by default</value>
        public static bool IgnoreWordsInAllUppercase { get; set; }

        /// <summary>
        /// This is used to get or set whether or not to ignore words that look like filenames or e-mail
        /// addresses.
        /// </summary>
        /// <value>This is true by default</value>
        public static bool IgnoreFilenamesAndEMailAddresses { get; set; }

        /// <summary>
        /// This is used to get or set whether or not to ignore XML elements in the text being spell checked
        /// (text within '&amp;lt;' and '&amp;gt;').
        /// </summary>
        /// <value>This is true by default</value>
        public static bool IgnoreXmlElementsInText { get; set; }


        /// <summary>
        /// This is used to get or set whether or not underscores are treated as a word separator
        /// </summary>
        /// <value>This is false by default</value>
        public static bool TreatUnderscoreAsSeparator { get; set; }

        /// <summary>
        /// This read-only property returns an enumerable list of ignored XML element names that will not have
        /// their content spell checked.
        /// </summary>
        public static IEnumerable<string> IgnoredXmlElements
        {
            get { return ignoredXmlElements; }
        }

        /// <summary>
        /// This read-only property returns an enumerable list of XML attribute names that will not have their
        /// values spell checked.
        /// </summary>
        public static IEnumerable<string> SpellCheckedXmlAttributes
        {
            get { return spellCheckedXmlAttributes; }
        }

        /// <summary>
        /// This read-only property returns a list of available dictionary languages
        /// </summary>
        /// <remarks>The returned enumerable list contains the default English (en-US) dictionary along with
        /// any custom dictionaries found in the <see cref="ConfigurationFilePath"/> folder.</remarks>
        public static IEnumerable<CultureInfo> AvailableDictionaryLanguages
        {
            get
            {
                CultureInfo info;

                // This is supplied with the application and is always available
                yield return new CultureInfo("en-US");

                // Culture names can vary in format (en-US, arn, az-Cyrl, az-Cyrl-AZ, az-Latn, az-Latn-AZ, etc.)
                // so look for any affix files with a related dictionary file and see if they are valid cultures.
                // If so, we'll take them.
                foreach(string dictionary in Directory.EnumerateFiles(ConfigurationFilePath, "*.aff"))
                    if(File.Exists(Path.ChangeExtension(dictionary, ".dic")))
                    {
                        try
                        {
                            info = new CultureInfo(Path.GetFileNameWithoutExtension(dictionary).Replace("_", "-"));
                        }
                        catch(CultureNotFoundException )
                        {
                            // Ignore filenames that are not cultures
                            info = null;
                        }

                        if(info != null)
                            yield return info;
                    }
            }
        }

        /// <summary>
        /// This read-only property returns the default list of ignored XML elements
        /// </summary>
        public static IEnumerable<string> DefaultIgnoredXmlElements
        {
            get
            {
                return new string[] { "c", "code", "codeEntityReference", "codeReference", "codeInline",
                    "command", "environmentVariable", "fictitiousUri", "foreignPhrase", "link", "linkTarget",
                    "linkUri", "localUri", "replaceable", "see", "seeAlso", "unmanagedCodeEntityReference",
                    "token" };
            }
        }

        /// <summary>
        /// This read-only property returns the default list of spell checked XML attributes
        /// </summary>
        public static IEnumerable<string> DefaultSpellCheckedAttributes
        {
            get
            {
                return new[] { "altText", "Caption", "Content", "Header", "lead", "title", "term", "Text",
                    "ToolTip" };
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Static constructor
        /// </summary>
        static SpellCheckerConfiguration()
        {
            if(!LoadConfiguration())
            {
                IgnoreWordsWithDigits = IgnoreWordsInAllUppercase = IgnoreFilenamesAndEMailAddresses =
                    IgnoreXmlElementsInText = true;

                ignoredXmlElements = new HashSet<string>(DefaultIgnoredXmlElements);

                spellCheckedXmlAttributes = new HashSet<string>(DefaultSpellCheckedAttributes);

                DefaultLanguage = new CultureInfo("en-US");
            }
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Set the list of ignored XML elements
        /// </summary>
        /// <param name="ignoredElements">The list of XML elements to ignore</param>
        public static void SetIgnoredXmlElements(IEnumerable<string> ignoredElements)
        {
            ignoredXmlElements = new HashSet<string>(ignoredElements);
        }

        /// <summary>
        /// Set the list of spell checked XML attributes
        /// </summary>
        /// <param name="spellCheckedAttributes">The list of spell checked XML attributes</param>
        public static void SetSpellCheckedXmlAttributes(IEnumerable<string> spellCheckedAttributes)
        {
            spellCheckedXmlAttributes = new HashSet<string>(spellCheckedAttributes);
        }

        /// <summary>
        /// This is used to load the spell checker configuration settings
        /// </summary>
        /// <returns>True if loaded successfully or false if the file does not exist or could not be loaded</returns>
        private static bool LoadConfiguration()
        {
            string filename = Path.Combine(ConfigurationFilePath, "SpellChecker.config");
            bool success = true;

            try
            {
                if(!File.Exists(filename))
                    return false;

                var root = XDocument.Load(filename).Root;

                var node = root.Element("DefaultLanguage");

                if(node != null)
                    DefaultLanguage = new CultureInfo(node.Value);
                else
                    DefaultLanguage = new CultureInfo("en-US");

                IgnoreWordsWithDigits = (root.Element("IgnoreWordsWithDigits") != null);
                IgnoreWordsInAllUppercase = (root.Element("IgnoreWordsInAllUppercase") != null);
                IgnoreFilenamesAndEMailAddresses = (root.Element("IgnoreFilenamesAndEMailAddresses") != null);
                IgnoreXmlElementsInText = (root.Element("IgnoreXmlElementsInText") != null);
                TreatUnderscoreAsSeparator = (root.Element("TreatUnderscoreAsSeparator") != null);

                node = root.Element("IgnoredXmlElements");

                if(node != null)
                    ignoredXmlElements = new HashSet<string>(node.Descendants().Select(n => n.Value));
                else
                    ignoredXmlElements = new HashSet<string>(DefaultIgnoredXmlElements);

                node = root.Element("SpellCheckedXmlAttributes");

                if(node != null)
                    spellCheckedXmlAttributes = new HashSet<string>(node.Descendants().Select(n => n.Value));
                else
                    spellCheckedXmlAttributes = new HashSet<string>(DefaultSpellCheckedAttributes);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// This is used to save the spell checker configuration settings
        /// </summary>
        /// <remarks>The settings are saved to <b>SpellChecker.config</b> in the <see cref="ConfigurationFilePath"/>
        /// folder.</remarks>
        public static bool SaveConfiguration()
        {
            string filename = Path.Combine(ConfigurationFilePath, "SpellChecker.config");
            bool success = true;

            try
            {
                XElement root = new XElement("SpellCheckerConfiguration",
                    new XElement("DefaultLanguage") { Value = DefaultLanguage.Name },
                    IgnoreWordsWithDigits ? new XElement("IgnoreWordsWithDigits") : null,
                    IgnoreWordsInAllUppercase ? new XElement("IgnoreWordsInAllUppercase") : null,
                    IgnoreFilenamesAndEMailAddresses ? new XElement("IgnoreFilenamesAndEMailAddresses") : null,
                    IgnoreXmlElementsInText ? new XElement("IgnoreXmlElementsInText") : null,
                    TreatUnderscoreAsSeparator ? new XElement("TreatUnderscoreAsSeparator") : null);

                if(ignoredXmlElements.Count != DefaultIgnoredXmlElements.Count() ||
                  DefaultIgnoredXmlElements.Except(ignoredXmlElements).Count() != 0)
                    root.Add(new XElement("IgnoredXmlElements",
                        ignoredXmlElements.Select(i => new XElement("Ignore") { Value = i })));

                if(spellCheckedXmlAttributes.Count != DefaultSpellCheckedAttributes.Count() ||
                  DefaultSpellCheckedAttributes.Except(spellCheckedXmlAttributes).Count() != 0)
                    root.Add(new XElement("SpellCheckedXmlAttributes",
                        spellCheckedXmlAttributes.Select(i => new XElement("SpellCheck") { Value = i })));

                root.Save(filename);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                success = false;
            }

            return success;
        }
        #endregion
    }
}
