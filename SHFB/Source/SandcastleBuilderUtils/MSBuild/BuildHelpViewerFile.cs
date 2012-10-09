//=============================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : BuildHelpViewerFile.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/19/2010
// Note    : Copyright 2009-2010, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the MSBuild task used to compress the help content into
// a Microsoft Help Container (a ZIP file with a .mshc extension).
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.3  07/06/2009  EFW  Created the code
// ============================================================================

using System;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Ionic.Zip;

namespace SandcastleBuilder.Utils.MSBuild
{
    /// <summary>
    /// This task is used to compress the help content into a Microsoft Help
    /// Container (a ZIP file with a .mshc extension).
    /// </summary>
    public class BuildHelpViewerFile : Task
    {
        #region Private data members
        //=====================================================================

        private long fileCount, folderCount, compressedSize, uncompressedSize;
        #endregion

        #region Task input properties
        //=====================================================================

        /// <summary>
        /// This is used to pass in the working folder where the files to
        /// compress are located.
        /// </summary>
        [Required]
        public string WorkingFolder { get; set; }

        /// <summary>
        /// This is used to pass in the output folder where the compressed
        /// output file is stored.
        /// </summary>
        [Required]
        public string OutputFolder { get; set; }

        /// <summary>
        /// This is used to pass in the name of the help file (no path or
        /// extension).
        /// </summary>
        [Required]
        public string HtmlHelpName { get; set; }

        #endregion

        #region Execute method
        //=====================================================================

        /// <summary>
        /// This is used to execute the task and perform the build
        /// </summary>
        /// <returns>True on success or false on failure.</returns>
        public override bool Execute()
        {
            using(ZipFile zip = new ZipFile())
            {
                fileCount = folderCount = compressedSize = uncompressedSize = 0;

                // There may be a large number of files so enable ZIP64
                // if needed.
                zip.UseZip64WhenSaving = Zip64Option.AsNecessary;

                // Go for best compression.  We can reduce this or expose it
                // as a property later if it causes problems.
                zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;

                // Track progress when adding and saving
                zip.AddProgress += new EventHandler<AddProgressEventArgs>(zip_AddProgress);
                zip.SaveProgress += new EventHandler<SaveProgressEventArgs>(zip_SaveProgress);

                // Compress the entire working folder.  Files are stored
                // relative to the root.
                zip.AddDirectory(this.WorkingFolder, null);

                // Save it to the output folder with a .mshc extension
                zip.Save(Path.Combine(this.OutputFolder, this.HtmlHelpName) + ".mshc");
            }

            return true;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// This is used to report progress as files are added to the archive
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void zip_AddProgress(object sender, AddProgressEventArgs e)
        {
            switch(e.EventType)
            {
                case ZipProgressEventType.Adding_Started:
                    Log.LogMessage(MessageImportance.High,
                        "Compressing help content files...");
                    break;

                case ZipProgressEventType.Adding_AfterAddEntry:
                    Log.LogMessage(MessageImportance.High, "Added {0}",
                        e.CurrentEntry.FileName);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// This is used to report progress as the archive is saved
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        void zip_SaveProgress(object sender, SaveProgressEventArgs e)
        {
            switch(e.EventType)
            {
                case ZipProgressEventType.Saving_Started:
                    Log.LogMessage(MessageImportance.High, "Saving {0}...",
                        e.ArchiveName);
                    break;

                case ZipProgressEventType.Saving_AfterWriteEntry:
                    if(!e.CurrentEntry.FileName.EndsWith("/", StringComparison.Ordinal))
                    {
                        compressedSize += e.CurrentEntry.CompressedSize;
                        uncompressedSize += e.CurrentEntry.UncompressedSize;
                        fileCount++;
                    }
                    else
                        folderCount++;

                    Log.LogMessage(MessageImportance.High, "Saved {0} of {1}: {2}",
                        e.EntriesSaved, e.EntriesTotal, e.CurrentEntry.FileName);
                    break;

                case ZipProgressEventType.Saving_Completed:
                    Log.LogMessage(MessageImportance.High, "Finished saving {0}\r\n" +
                        "Compressed {1} folders, {2} files.  Reduced size by " +
                        "{3:N0} bytes ({4:N0}%).", e.ArchiveName, folderCount,
                        fileCount, uncompressedSize - compressedSize,
                        (uncompressedSize != 0) ? 100.0 - (
                        100.0 * compressedSize / uncompressedSize) : 100.0);
                    break;

                default:
                    break;
            }
        }
        #endregion
    }
}
