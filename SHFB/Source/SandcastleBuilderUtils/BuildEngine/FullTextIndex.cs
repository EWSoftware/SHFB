//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : FullTextIndex.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/15/2021
// Note    : Copyright 2007-2021, Eric Woodruff, All rights reserved
//
// This file contains a class used to create a full-text index used to search for topics in the ASP.NET web
// pages.  It's a really basic implementation but should get the job done.
//
// Design Decision:
//    In order to keep the serialized index files free from dependencies on user-defined data types, the index
//    is created using only built-in data types (string and long).  It could have used classes to contain the
//    index data which would be more "object oriented" but then it would require deploying an assembly
//    containing those types with the search pages.  Doing it this way keeps deployment as simple as possible.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 06/24/2007  EFW  Created the code
// 02/17/2012  EFW  Switched to JSON serialization to support websites that use something other than
//                  ASP.NET such as PHP.
//===============================================================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace SandcastleBuilder.Utils.BuildEngine
{
    /// <summary>
    /// This is a really basic implementation of an algorithm used to create a full-text index of the website
    /// pages so that they can be searched using the ASP.NET web pages.
    /// </summary>
    /// <remarks>So that an assembly does not have to be deployed to deserialize the index information, the
    /// index information is represented using built-in data types (string and long).
    /// </remarks>
    public class FullTextIndex
    {
        #region Private class members
        //=====================================================================

        // Parsing regular expressions
        private static readonly Regex rePageTitle = new Regex(@"<title>(?<Title>.*)</title>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static readonly Regex reStripScriptStyleHead = new Regex(
            @"<script[^>]*(?<!/)>.*?</script\s*>|<style[^>]*(?<!/)>.*?</style\s*>|<head[^>]*(?<!/)>.*?</head\s*>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static readonly Regex reStripTags = new Regex("<[^>]+>");

        private static readonly Regex reStripApos = new Regex(@"\w'\w{1,2}");

        private static readonly Regex reCondenseWS = new Regex(@"\s+");

        private static readonly Regex reSplitWords = new Regex(@"\W");

        // Exclusion word list
        private readonly HashSet<string> exclusionWords;
        private readonly CultureInfo lang;

        // Index information
        private readonly ConcurrentQueue<string> fileList;
        private readonly ConcurrentDictionary<string, ConcurrentQueue<long>> wordDictionary;
        private int fileListCount;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exclusions">The file containing common word exclusions.  The file should contain one
        /// work per line in lowercase.  These words will not appear in the index.</param>
        /// <param name="language">The culture information</param>
        public FullTextIndex(string exclusions, CultureInfo language)
        {
            Encoding enc = Encoding.Default;
            string content;
            string[] words;

            if(String.IsNullOrEmpty(exclusions) || !File.Exists(exclusions))
                throw new ArgumentException("Exclusion file cannot be null or an empty string and must exist");

            content = Utility.ReadWithEncoding(exclusions, ref enc);
            content = reCondenseWS.Replace(content, " ");
            lang = language;

            exclusionWords = new HashSet<string>();
            words = reSplitWords.Split(content);

            foreach(string word in words)
                if(word.Length > 2)
                    exclusionWords.Add(word);

            fileList = new ConcurrentQueue<string>();
            wordDictionary = new ConcurrentDictionary<string, ConcurrentQueue<long>>();
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// Create a full-text index from web pages found in the specified file path
        /// </summary>
        /// <param name="filePath">The path containing the files to index</param>
        /// <remarks>Words in the exclusion list, those that are less than three characters long, and anything
        /// starting with a digit will not appear in the index.</remarks>
        public void CreateFullTextIndex(string filePath)
        {
            int rootPathLength;

            if(filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            if(filePath[filePath.Length - 1] == '\\')
                rootPathLength = filePath.Length;
            else
                rootPathLength = filePath.Length + 1;

            Parallel.ForEach(Directory.EnumerateFiles(filePath, "*.htm?", SearchOption.AllDirectories),
              new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 20 }, name =>
            {
                string fileInfo, title;
                string[] words;

                Encoding enc = Encoding.Default;
                string content = Utility.ReadWithEncoding(name, ref enc);

                // Extract the page title
                Match m = rePageTitle.Match(content);

                if(!m.Success)
                    title = Path.GetFileNameWithoutExtension(name);
                else
                    title = m.Groups["Title"].Value.Trim();

                // Put some space between tags
                content = content.Replace("><", "> <");

                // Remove script, style sheet, and head blocks as they won't contain any usable keywords.  Pre
                // tags contain code which may or may not be useful but we'll leave them alone for now.
                content = reStripScriptStyleHead.Replace(content, " ");

                // Remove all HTML tags
                content = reStripTags.Replace(content, " ");

                // Decode the text
                content = WebUtility.HtmlDecode(content);

                // Strip apostrophe suffixes
                content = reStripApos.Replace(content, String.Empty);

                // Condense all runs of whitespace to a single space
                content = reCondenseWS.Replace(content, " ");

                // Convert to lowercase and split text on non-word boundaries
                words = reSplitWords.Split(content.ToLower(lang));

                // We're going to use simple types for the index structure so that we don't have to deploy an
                // assembly to deserialize it.  As such, concatenate the title, filename, and its word count
                // into a string separated by nulls.  Note that file paths are assumed to be relative to the
                // root folder.
                fileInfo = String.Join("\x0", new string[] { title,
                    name.Substring(rootPathLength).Replace('\\', '/'),
                    words.Length.ToString(CultureInfo.InvariantCulture) });

                var wordCounts = new Dictionary<string, int>();

                // Get a list of all unique words and the number of time that they appear in this file.
                // Exclude words that are less than three characters in length, start with a digit, or
                // are in the common words exclusion list.
                foreach(string word in words)
                {
                    if(word.Length < 3 || Char.IsDigit(word[0]) || exclusionWords.Contains(word))
                        continue;

                    // The number of times it occurs helps determine the ranking of the search results
                    if(wordCounts.ContainsKey(word))
                        wordCounts[word] += 1;
                    else
                        wordCounts.Add(word, 1);
                }

                // Shouldn't happen but just in case, ignore files with no usable words
                if(wordCounts.Keys.Count != 0)
                {
                    fileList.Enqueue(fileInfo);
                    int count = Interlocked.Increment(ref fileListCount);

                    // Add the index information to the word dictionary
                    foreach(string word in wordCounts.Keys)
                    {
                        // For each unique word, we'll track the files in which it occurs and the number
                        // of times it occurs in each file.
                        if(!wordDictionary.ContainsKey(word))
                            wordDictionary.TryAdd(word, new ConcurrentQueue<long>());

                        // Store the file index in the upper part of a 64-bit integer and the word count
                        // in the lower 16-bits.  More room is given to the file count as some builds
                        // contain a large number of topics.
                        wordDictionary[word].Enqueue(((long)(count - 1) << 16) + (wordCounts[word] & 0xFFFF));
                    }
                }
            });
        }

        /// <summary>
        /// Save the index information to the specified location.
        /// </summary>
        /// <param name="indexPath">The path to which the index files are
        /// saved.</param>
        /// <remarks>JSON serialization is used to save the index data.</remarks>
        public void SaveIndex(string indexPath)
        {
            Dictionary<char, Dictionary<string, List<long>>> letters =
                new Dictionary<char, Dictionary<string, List<long>>>();
            char firstLetter;

            if(!Directory.Exists(indexPath))
                Directory.CreateDirectory(indexPath);

            // First, the easy part.  Save the filename index
            using(StreamWriter sw = new StreamWriter(indexPath + "FTI_Files.json"))
            {
                sw.Write(JsonSerializer.Serialize(new List<string>(fileList)));
            }

            // Now split the word dictionary up into pieces by first letter.  This will help the search
            // as it only has to load data related to words in the search and reduces what it has to look
            // at as well.
            foreach(string word in wordDictionary.Keys)
            {
                firstLetter = word[0];

                if(!letters.ContainsKey(firstLetter))
                    letters.Add(firstLetter, new Dictionary<string, List<long>>());

                letters[firstLetter].Add(word, new List<long>(wordDictionary[word]));
            }

            // Save each part.  The letter is specified as an integer to allow for Unicode characters
            foreach(char letter in letters.Keys)
                using(StreamWriter sw = new StreamWriter(String.Format(CultureInfo.InvariantCulture,
                  "{0}\\FTI_{1}.json", indexPath, (int)letter)))
                {
                    sw.Write(JsonSerializer.Serialize(letters[letter]));
                }
        }
        #endregion
    }
}
