//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : FileSpellChecker.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/28/2013
// Note    : Copyright 2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to spell check files in the SHFB project
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.8.0  05/11/2013  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;

namespace SandcastleBuilder.Gui.Spelling
{
    /// <summary>
    /// This is used to spell check files
    /// </summary>
    internal class FileSpellChecker
    {
        #region Private data members
        //=====================================================================

        // Word break characters.  Specifically excludes: _ . ' @
        private const string wordBreakChars = ",/<>?;:\"[]\\{}|-=+~!#$%^&*() \t\r\n";

        // Regular expressions used to find thinks that look like XML elements
        private static Regex reXml = new Regex(@"<[A-Za-z/]+?.*?>");

        private GlobalDictionary dictionary;
        #endregion

        #region Events
        //=====================================================================

        /// <summary>
        /// This event is raised when spell checking is about to start on a
        /// file.
        /// </summary>
        /// <remarks>If <c>Cancel</c> is set to true in the event arguments,
        /// the spell checking operation will be cancelled.</remarks>
        public event CancelEventHandler SpellCheckFileStarting;

        /// <summary>
        /// This raises the <see cref="SpellCheckFileStarting" /> event
        /// </summary>
        protected virtual void OnSpellCheckFileStarting(CancelEventArgs e)
        {
            CancelEventHandler handler = SpellCheckFileStarting;

            if(handler != null)
                handler(this, e);
        }

        /// <summary>
        /// This event is raised when spell checking is about to start on a
        /// block of text.
        /// </summary>
        /// <remarks>If <c>Cancel</c> is set to true in the event arguments,
        /// the spell checking operation will be cancelled.</remarks>
        public event CancelEventHandler SpellCheckTextStarting;

        /// <summary>
        /// This raises the <see cref="SpellCheckTextStarting" /> event
        /// </summary>
        protected virtual void OnSpellCheckTextStarting(CancelEventArgs e)
        {
            CancelEventHandler handler = SpellCheckTextStarting;

            if(handler != null)
                handler(this, e);
        }

        /// <summary>
        /// This event is raised when a misspelled word is found
        /// </summary>
        public event EventHandler<SpellingEventArgs> MisspelledWord;

        /// <summary>
        /// This raises the <see cref="MisspelledWord" /> event
        /// </summary>
        protected virtual void OnMisspelledWord(SpellingEventArgs e)
        {
            EventHandler<SpellingEventArgs> handler = MisspelledWord;

            if(handler != null)
                handler(this, e);
        }

        /// <summary>
        /// This event is raised when a doubled word is found
        /// </summary>
        public event EventHandler<SpellingEventArgs> DoubledWord;

        /// <summary>
        /// This raises the <see cref="DoubledWord" /> event
        /// </summary>
        protected virtual void OnDoubledWord(SpellingEventArgs e)
        {
            EventHandler<SpellingEventArgs> handler = DoubledWord;

            if(handler != null)
                handler(this, e);
        }

        /// <summary>
        /// This event is raised when the spell checking has been cancelled
        /// for a block of text.
        /// </summary>
        public event EventHandler SpellCheckTextCancelled;

        /// <summary>
        /// This raises the <see cref="SpellCheckTextCancelled" /> event
        /// </summary>
        protected virtual void OnSpellCheckTextCancelled(EventArgs e)
        {
            EventHandler handler = SpellCheckTextCancelled;

            if(handler != null)
                handler(this, e);
        }

        /// <summary>
        /// This event is raised when the spell checking has been completed
        /// for a block of text.
        /// </summary>
        public event EventHandler SpellCheckTextCompleted;

        /// <summary>
        /// This raises the <see cref="SpellCheckTextCompleted" /> event
        /// </summary>
        protected virtual void OnSpellCheckTextCompleted(EventArgs e)
        {
            EventHandler handler = SpellCheckTextCompleted;

            if(handler != null)
                handler(this, e);
        }

        /// <summary>
        /// This event is raised when the spell checking has been cancelled
        /// for a file.
        /// </summary>
        public event EventHandler SpellCheckFileCancelled;

        /// <summary>
        /// This raises the <see cref="SpellCheckFileCancelled" /> event
        /// </summary>
        protected virtual void OnSpellCheckFileCancelled(EventArgs e)
        {
            EventHandler handler = SpellCheckFileCancelled;

            if(handler != null)
                handler(this, e);
        }

        /// <summary>
        /// This event is raised when the spell checking has been completed
        /// for a file.
        /// </summary>
        public event EventHandler SpellCheckFileCompleted;

        /// <summary>
        /// This raises the <see cref="SpellCheckFileCompleted" /> event
        /// </summary>
        protected virtual void OnSpellCheckFileCompleted(EventArgs e)
        {
            EventHandler handler = SpellCheckFileCompleted;

            if(handler != null)
                handler(this, e);
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public FileSpellChecker(GlobalDictionary dictionary)
        {
            this.dictionary = dictionary;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Check to see if the characters in the given text between the two given index points are all
        /// whitespace characters.
        /// </summary>
        /// <param name="text">The text</param>
        /// <param name="start">The starting index</param>
        /// <param name="end">The ending index</param>
        /// <returns>True if all characters are whitespace, False if not</returns>
        private static bool IsAllWhitespace(string text, int start, int end)
        {
            while(start <= end && Char.IsWhiteSpace(text[start]))
                start++;

            return (start > end);
        }

        /// <summary>
        /// Spell check the given text
        /// </summary>
        /// <param name="text">The text to spell check</param>
        /// <param name="location">The location of the text being spell checked</param>
        /// <returns>False if it was cancelled or True if it completed.</returns>
        private bool SpellCheckInternal(string text, TextLocation location)
        {
            SpellingEventArgs se = null;
            List<Match> xmlTags = null;
            TextLocation priorWord = new TextLocation();
            string currentWord;
            int textIdx;

            // Signal the start and allow it to be cancelled
            CancelEventArgs ce = new CancelEventArgs();
            this.OnSpellCheckTextStarting(ce);

            if(ce.Cancel)
            {
                this.OnSpellCheckTextCancelled(EventArgs.Empty);
                return false;
            }

            // Note the location of all XML elements if needed
            if(SpellCheckerConfiguration.IgnoreXmlElementsInText)
                xmlTags = reXml.Matches(text).OfType<Match>().ToList();

            // Spell check each word in the given text
            foreach(var word in GetWordsInText(text))
            {
                currentWord = text.Substring(word.Start, word.Length);
                textIdx = word.Start;

                if(!IsProbablyARealWord(currentWord) || (xmlTags != null && xmlTags.Count != 0 &&
                  xmlTags.Any(match => textIdx >= match.Index && textIdx <= match.Index + match.Length - 1)))
                    continue;

                if(!dictionary.ShouldIgnoreWord(currentWord) && !dictionary.IsSpelledCorrectly(currentWord))
                {
                    // Sometimes it flags a word as misspelled if it ends with "'s".  Try checking the word
                    // without the "'s".  If ignored or correct without it, don't flag it.  This appears to be
                    // caused by the definitions in the dictionary rather than Hunspell.
                    if(currentWord.EndsWith("'s", StringComparison.OrdinalIgnoreCase))
                    {
                        currentWord = currentWord.Substring(0, currentWord.Length - 2);

                        if(dictionary.ShouldIgnoreWord(currentWord) || dictionary.IsSpelledCorrectly(currentWord))
                            continue;

                        currentWord += "'s";
                    }

                    se = new SpellingEventArgs(currentWord, location.ToPoint(text, textIdx));
                    this.OnMisspelledWord(se);

                    if(se.Cancel)
                        break;
                }
                else
                    if(priorWord.Length != 0 && String.Compare(text.Substring(priorWord.Start, priorWord.Length),
                      currentWord, StringComparison.OrdinalIgnoreCase) == 0 &&
                      IsAllWhitespace(text, priorWord.Start + priorWord.Length, word.Start - 1))
                    {
                        se = new SpellingEventArgs(currentWord, location.ToPoint(text, textIdx));
                        this.OnDoubledWord(se);

                        if(se.Cancel)
                            break;
                    }

                priorWord = word;
            }

            if(se != null && se.Cancel)
            {
                this.OnSpellCheckTextCancelled(EventArgs.Empty);
                return false;
            }

            this.OnSpellCheckTextCompleted(EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Get all words in the specified text string
        /// </summary>
        /// <param name="text">The text to break into words</param>
        /// <returns>An enumerable list of word spans</returns>
        private static IEnumerable<TextLocation> GetWordsInText(string text)
        {
            if(String.IsNullOrWhiteSpace(text))
                yield break;

            for(int i = 0; i < text.Length; i++)
            {
                if(IsWordBreakCharacter(text[i]))
                {
                    // Skip escape sequences.  If not, they can end up as part of the word or cause words to be
                    // missed.  For example, "This\r\nis\ta\ttest \x22missing\x22" would incorrectly yield "r",
                    // "nis", "ta", and "ttest" and incorrectly exclude "missing").  This can cause the
                    // occasional false positive in file paths (i.e. \Folder\transform\File.txt flags "ransform"
                    // as a misspelled word because of the lowercase "t" following the backslash) but I can live
                    // with that.
                    if(text[i] == '\\' && i + 1 < text.Length)
                        switch(text[i + 1])
                        {
                            case '\'':
                            case '\"':
                            case '\\':
                            case '0':
                            case 'a':
                            case 'b':
                            case 'f':
                            case 'n':
                            case 'r':
                            case 't':
                            case 'v':
                                i++;
                                break;

                            case 'u':
                            case 'U':
                            case 'x':
                                i++;

                                // Special handling for \x, \u, and \U.  Skip the hex digits too.
                                if(i + 1 < text.Length)
                                {
                                    do
                                    {
                                        i++;

                                    } while(i < text.Length && (Char.IsDigit(text[i]) ||
                                      (Char.ToLower(text[i]) >= 'a' && Char.ToLower(text[i]) <= 'f')));

                                    i--;
                                }
                                break;

                            default:
                                break;
                        }

                    continue;
                }

                int end = i;

                for(; end < text.Length; end++)
                    if(IsWordBreakCharacter(text[end]))
                        break;

                // If it looks like an XML entity, ignore it
                if(i == 0 || end >= text.Length || text[i - 1] != '&' || text[end] != ';')
                {
                    // Ignore leading apostrophes
                    while(i < end && text[i] == '\'')
                        i++;

                    // Ignore trailing apostrophes, periods, and at-signs
                    while(end > i && (text[end - 1] == '\'' || text[end - 1] == '.' || text[end - 1] == '@'))
                        end--;

                    // Ignore anything less than two characters
                    if(end <= i)
                        end++;
                    else
                        if(end - i > 1)
                            yield return new TextLocation { Start = i, Length = end - i };
                }

                i = end - 1;
            }
        }

        /// <summary>
        /// See if the specified character is a word break character
        /// </summary>
        /// <param name="c">The character to check</param>
        /// <returns>True if the character is a word break, false if not</returns>
        private static bool IsWordBreakCharacter(char c)
        {
            return wordBreakChars.Contains(c) || Char.IsWhiteSpace(c) ||
                (c == '_' && SpellCheckerConfiguration.TreatUnderscoreAsSeparator) ||
                ((c == '.' || c == '@') && !SpellCheckerConfiguration.IgnoreFilenamesAndEMailAddresses);
        }

        /// <summary>
        /// Determine if a word is probably a real word
        /// </summary>
        /// <param name="word">The word to check</param>
        /// <returns>True if it appears to be a real word or false if any of the following conditions are met:
        ///
        /// <list type="bullet">
        ///     <description>The word contains a period or an at-sign (it looks like a filename or an e-mail
        /// address) and those words are being ignored.  We may miss a few real misspellings in this case due
        /// to a missed space after a period, but that's acceptable.</description>
        ///     <description>The word contains an underscore and underscores are not being treated as
        /// separators.</description>
        ///     <description>The word contains a digit and words with digits are being ignored.</description>
        ///     <description>The word is composed entirely of digits when words with digits are not being
        /// ignored.</description>
        ///     <description>The word is in all uppercase and words in all uppercase are being ignored.</description>
        ///     <description>The word is camel cased.</description>
        /// </list>
        /// </returns>
        private static bool IsProbablyARealWord(string word)
        {
            if(String.IsNullOrWhiteSpace(word))
                return false;

            word = word.Trim();

            // Check for a period or an at-sign in the word (things that look like filenames and e-mail addresses)
            if(word.IndexOfAny(new[] { '.', '@' }) >= 0)
                return false;

            // Check for underscores and digits
            if(word.Any(c => c == '_' || (Char.IsDigit(c) && SpellCheckerConfiguration.IgnoreWordsWithDigits)))
                return false;

            // Ignore if all digits (this only happens if the Ignore Words With Digits option is false)
            if(!word.Any(c => Char.IsLetter(c)))
                return false;

            // Ignore if all uppercase, accounting for apostrophes and digits
            if(word.All(c => Char.IsUpper(c) || !Char.IsLetter(c)))
                return !SpellCheckerConfiguration.IgnoreWordsInAllUppercase;

            // Ignore if camel cased
            if(Char.IsLetter(word[0]) && word.Skip(1).Any(c => Char.IsUpper(c)))
                return false;

            return true;
        }
        #endregion

        #region Spell checking methods
        //=====================================================================

        /// <summary>
        /// Spell check a block of text
        /// </summary>
        /// <param name="text">The text to spell check</param>
        /// <returns>False if it was cancelled or True if it completed.</returns>
        public bool SpellCheckText(string text)
        {
            return this.SpellCheckInternal(text, new TextLocation { Line = 1 });
        }

        /// <summary>
        /// Spell check a plain text file
        /// </summary>
        /// <param name="filename">The name of the file to check</param>
        /// <returns>False if it was cancelled or True if it completed.</returns>
        public bool SpellCheckPlainTextFile(string filename)
        {
            // Signal the start and allow it to be cancelled
            CancelEventArgs ce = new CancelEventArgs();
            this.OnSpellCheckFileStarting(ce);

            if(ce.Cancel)
            {
                this.OnSpellCheckFileCancelled(EventArgs.Empty);
                return false;
            }

            if(!this.SpellCheckInternal(File.ReadAllText(filename), new TextLocation { Line = 1 }))
                return false;

            this.OnSpellCheckFileCompleted(EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Spell check an XML file
        /// </summary>
        /// <param name="filename">The name of the file to check</param>
        /// <returns>False if it was cancelled or True if it completed.</returns>
        /// <remarks>The inner text and the content of selected attributes will be spell checked</remarks>
        public bool SpellCheckXmlFile(string filename)
        {
            // Signal the start and allow it to be cancelled
            CancelEventArgs ce = new CancelEventArgs();
            this.OnSpellCheckFileStarting(ce);

            if(ce.Cancel)
            {
                this.OnSpellCheckFileCancelled(EventArgs.Empty);
                return false;
            }

            XmlReaderSettings rs = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore };

            if(!this.SpellCheckXmlReader(XmlReader.Create(filename, rs)))
            {
                this.OnSpellCheckFileCancelled(EventArgs.Empty);
                return false;
            }

            this.OnSpellCheckFileCompleted(EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Spell check the content of an XML reader
        /// </summary>
        /// <param name="reader">The reader to spell check</param>
        /// <returns>False if it was cancelled or True if it completed.</returns>
        public bool SpellCheckXmlReader(XmlReader reader)
        {
            IXmlLineInfo lineInfo = (IXmlLineInfo)reader;
            TextLocation location = new TextLocation();
            bool wasCancelled = false;
            int idx;

            reader.MoveToContent();

            while(!reader.EOF && !wasCancelled)
            {
                switch(reader.NodeType)
                {
                    case XmlNodeType.Element:
                        location.Line = lineInfo.LineNumber;
                        location.Column = lineInfo.LinePosition;

                        if(reader.HasAttributes)
                        {
                            for(idx = 0; idx < reader.AttributeCount; idx++)
                            {
                                reader.MoveToAttribute(idx);

                                if(SpellCheckerConfiguration.SpellCheckedXmlAttributes.Contains(reader.LocalName))
                                {
                                    // Set the approximate position of the value assuming the format:
                                    // attrName="value"
                                    location.Line = lineInfo.LineNumber;
                                    location.Column = lineInfo.LinePosition + reader.Settings.LinePositionOffset +
                                        reader.Name.Length + 2;

                                    // The value must be encoded to get an accurate position (quotes excluded)
                                    wasCancelled = !this.SpellCheckInternal(WebUtility.HtmlEncode(
                                        reader.Value).Replace("&quot;", "\"").Replace("&#39;", "'"), location);
                                }
                            }
 
                            reader.MoveToElement();
                        }

                        // Is it an element in which to skip the content?
                        if(SpellCheckerConfiguration.IgnoredXmlElements.Contains(reader.LocalName))
                        {
                            reader.Skip();
                            continue;
                        }
                        break;

                    case XmlNodeType.Comment:
                    case XmlNodeType.CDATA:
                        location.Line = lineInfo.LineNumber;
                        location.Column = lineInfo.LinePosition;

                        wasCancelled = !this.SpellCheckInternal(reader.Value, location);
                        break;

                    case XmlNodeType.Text:
                        location.Line = lineInfo.LineNumber;
                        location.Column = lineInfo.LinePosition;

                        // The value must be encoded to get an accurate position (quotes excluded)
                        wasCancelled = !this.SpellCheckInternal(WebUtility.HtmlEncode(reader.Value).Replace(
                            "&quot;", "\"").Replace("&#39;", "'"), location);
                        break;

                    default:
                        break;
                }

                reader.Read();
            }

            return wasCancelled;
        }
        #endregion
    }
}
