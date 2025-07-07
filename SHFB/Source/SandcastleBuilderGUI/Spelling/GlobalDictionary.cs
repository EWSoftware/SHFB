//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : GlobalDictionary.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/05/2025
// Note    : Copyright 2013-2025, Eric Woodruff, All rights reserved
//
// This file contains the class that implements the global dictionary
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/11/2013  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

using NHunspell;

namespace SandcastleBuilder.Gui.Spelling
{
    /// <summary>
    /// This class implements the global dictionary
    /// </summary>
    internal sealed class GlobalDictionary
    {
        #region Private data members
        //=====================================================================

        private static Dictionary<string, GlobalDictionary> globalDictionaries;
        private static SpellEngine spellEngine;

        private readonly HashSet<string> ignoredWords;
        private readonly SpellFactory spellFactory;
        private readonly string ignoredWordsFile;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the dictionary language
        /// </summary>
        public CultureInfo Language { get; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Private constructor
        /// </summary>
        /// <param name="culture">The language to use for the dictionary</param>
        /// <param name="spellFactory">The spell factory to use when checking words</param>
        private GlobalDictionary(CultureInfo culture, SpellFactory spellFactory)
        {
            this.Language = culture;
            this.spellFactory = spellFactory;

            ignoredWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            ignoredWordsFile = Path.Combine(SpellCheckerConfiguration.ConfigurationFilePath,
                culture.Name + "_Ignored.dic");

            this.LoadIgnoredWordsFile();
        }
        #endregion

        #region Spelling dictionary members
        //=====================================================================

        /// <inheritdoc />
        public bool IsSpelledCorrectly(string word)
        {
            try
            {
                if(spellFactory != null && !String.IsNullOrWhiteSpace(word))
                    return spellFactory.Spell(word);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                // Eat exceptions, there's not much we can do
            }

            return true;
        }

        /// <inheritdoc />
        public IEnumerable<string> SuggestCorrections(string word)
        {
            List<string> suggestions = null;

            try
            {
                if(spellFactory != null && !String.IsNullOrWhiteSpace(word))
                    suggestions = spellFactory.Suggest(word);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                // Eat exceptions, there's not much we can do
            }

            return suggestions ?? [];
        }

        /// <inheritdoc />
        public bool AddWordToDictionary(string word)
        {
            if(String.IsNullOrWhiteSpace(word))
                return false;

            using(StreamWriter writer = new(ignoredWordsFile, true))
            {
                writer.WriteLine(word);
            }

            return this.IgnoreWord(word);
        }

        /// <inheritdoc />
        public bool IgnoreWord(string word)
        {
            if(String.IsNullOrWhiteSpace(word) || this.ShouldIgnoreWord(word))
                return true;

            lock(ignoredWords)
            {
                ignoredWords.Add(word);
            }

            return true;
        }

        /// <inheritdoc />
        public bool ShouldIgnoreWord(string word)
        {
            lock(ignoredWords)
            {
                return ignoredWords.Contains(word);
            }
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// Create a global dictionary for the specified culture
        /// </summary>
        /// <param name="culture">The language to use for the dictionary</param>
        /// <returns>The spell factory to use or null if one could not be created</returns>
        public static GlobalDictionary CreateGlobalDictionary(CultureInfo culture)
        {
            GlobalDictionary globalDictionary = null;

            try
            {
                globalDictionaries ??= [];

                // If no culture is specified, use the default culture
                culture ??= SpellCheckerConfiguration.DefaultLanguage;

                // If not already loaded, create the dictionary and the thread-safe spell factory instance for
                // the given culture.
                if(!globalDictionaries.TryGetValue(culture.Name, out GlobalDictionary gd))
                {
                    string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                    if(spellEngine == null)
                    {
                        Hunspell.NativeDllPath = dllPath;
                        spellEngine = new SpellEngine();
                    }

                    // Look in the configuration folder first for user-supplied dictionaries
                    string dictionaryFile = Path.Combine(SpellCheckerConfiguration.ConfigurationFilePath,
                        culture.Name.Replace("-", "_") + ".aff");

                    // If not found, default to the English dictionary supplied with the package.  This can at
                    // least clue us in that it didn't find the language-specific dictionary when the suggestions
                    // are in English.
                    if(!File.Exists(dictionaryFile))
                        dictionaryFile = Path.Combine(dllPath, "en_US.aff");

                    LanguageConfig lc = new()
                    {
                        LanguageCode = culture.Name,
                        HunspellAffFile = dictionaryFile,
                        HunspellDictFile = Path.ChangeExtension(dictionaryFile, ".dic")
                    };

                    spellEngine.AddLanguage(lc);

                    gd = new GlobalDictionary(culture, spellEngine[culture.Name]);
                    
                    globalDictionaries.Add(culture.Name, gd);
                }

                globalDictionary = gd;
            }
            catch(Exception ex)
            {
                // Ignore exceptions.  Not much we can do, we'll just not spell check anything.
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return globalDictionary;
        }

        /// <summary>
        /// This is used to load the ignored words file for a specific language if it exists
        /// </summary>
        public static void LoadIgnoredWordsFile(CultureInfo language)
        {
            if(globalDictionaries != null && globalDictionaries.TryGetValue(language.Name, out GlobalDictionary gd))
                gd.LoadIgnoredWordsFile();
        }

        /// <summary>
        /// This is used to load the ignored words file
        /// </summary>
        private void LoadIgnoredWordsFile()
        {
            ignoredWords.Clear();

            if(File.Exists(ignoredWordsFile))
            {
                try
                {
                    foreach(string word in File.ReadLines(ignoredWordsFile))
                    {
                        if(!String.IsNullOrWhiteSpace(word))
                            ignoredWords.Add(word.Trim());
                    }
                }
                catch(Exception ex)
                {
                    // Ignore exceptions.  Not much we can do, we'll just not ignore anything by default.
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
        }
        #endregion
    }
}
