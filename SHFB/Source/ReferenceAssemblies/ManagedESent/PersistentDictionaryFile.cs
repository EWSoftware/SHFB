// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersistentDictionaryFile.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//   Methods that deal with PersistentDictionary database files.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Collections.Generic
{
    using System;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// Methods that deal with <see cref="PersistentDictionary{TKey,TValue}"/>
    /// database files.
    /// </summary>
    public static class PersistentDictionaryFile
    {
        // !EFW - Added to allow reference type serialization regardless of any potential issues
        /// <summary>
        /// This is used to get or set whether or not to allow reference type serialiation
        /// </summary>
        /// <value>False by default to disallow reference type serialization to avoid issues with persisted
        /// copies not containing changes made to the original object.  Set to true to allow serialization in
        /// cases where you don't care or if the persisted data is used in a read-only fashion when read back in
        /// and will not change and is thus not affected by the issue.</value>
        public static bool AllowReferenceTypeSerialization { get; set; }

        /// <summary>
        /// Determine if a dictionary database file exists in the specified directory.
        /// </summary>
        /// <param name="directory">The directory to look in.</param>
        /// <returns>True if the database file exists, false otherwise.</returns>
        public static bool Exists(string directory)
        {
            if (null == directory)
            {
                throw new ArgumentNullException("directory");    
            }

            if (Directory.Exists(directory))
            {
                var config = new PersistentDictionaryConfig();
                var databasePath = Path.Combine(directory, config.Database);
                return File.Exists(databasePath);
            }

            return false;
        }

        /// <summary>
        /// Delete all files associated with a PersistedDictionary database from
        /// the specified directory.
        /// </summary>
        /// <param name="directory">The directory to delete the files from.</param>
        public static void DeleteFiles(string directory)
        {
            if (null == directory)
            {
                throw new ArgumentNullException("directory");
            }

            if (Directory.Exists(directory))
            {
                var config = new PersistentDictionaryConfig();
                var databasePath = Path.Combine(directory, config.Database);
                File.Delete(databasePath);
                File.Delete(Path.Combine(directory, string.Format(CultureInfo.InvariantCulture, "{0}.chk", config.BaseName)));
                foreach (string file in Directory.GetFiles(directory, string.Format(CultureInfo.InvariantCulture, "{0}*.log", config.BaseName)))
                {
                    File.Delete(file);
                }

                foreach (string file in Directory.GetFiles(directory, string.Format(CultureInfo.InvariantCulture, "res*.log", config.BaseName)))
                {
                    File.Delete(file);
                }

                foreach (string file in Directory.GetFiles(directory, string.Format(CultureInfo.InvariantCulture, "{0}*.jrs", config.BaseName)))
                {
                    File.Delete(file);
                }
            }
        }
    }
}
