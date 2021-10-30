//===============================================================================================================
// System  : Sandcastle Help File Builder MSBuild Tasks
// File    : BuildHelpViewerFile.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/15/2021
// Note    : Copyright 2009-2021, Eric Woodruff, All rights reserved
//
// This file contains the MSBuild task used to compress the help content into a Microsoft Help Container (a ZIP
// file with a .mshc extension).
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/06/2009  EFW  Created the code
// 11/28/2012  EFW  Reduced the number of informational messages generated
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SandcastleBuilder.MSBuild
{
    /// <summary>
    /// This task is used to compress the help content into a Microsoft Help Container (a ZIP file with a .mshc
    /// extension).
    /// </summary>
    public class BuildHelpViewerFile : Task
    {
        #region Private data members
        //=====================================================================

        private IProgress<int> progressProvider;
        private HashSet<string> folders;

        private long fileCount, uncompressedSize;
        private string archiveName;

        #endregion

        #region Task input properties
        //=====================================================================

        /// <summary>
        /// This is used to pass in the working folder where the files to compress are located
        /// </summary>
        [Required]
        public string WorkingFolder { get; set; }

        /// <summary>
        /// This is used to pass in the output folder where the compressed output file is stored
        /// </summary>
        [Required]
        public string OutputFolder { get; set; }

        /// <summary>
        /// This is used to pass in the name of the help file (no path or extension)
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
            progressProvider = new Progress<int>(ReportProgress);
            folders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            archiveName = Path.Combine(this.OutputFolder, this.HtmlHelpName) + ".mshc";

            Log.LogMessage(MessageImportance.High, "Compressing help content files into {0}", archiveName);

            var t = System.Threading.Tasks.Task.Run(() => this.CompressFiles());

            t.Wait();

            var archiveInfo = new FileInfo(archiveName);

            Log.LogMessage(MessageImportance.High, "Finished saving {0}\r\n" +
                "Compressed {1} folders, {2} files.  Reduced size by {3:N0} bytes ({4:N0}%).",
                archiveName, folders.Count, fileCount, uncompressedSize - archiveInfo.Length,
                (uncompressedSize != 0) ? 100.0 - (100.0 * archiveInfo.Length / uncompressedSize) : 100.0);

            return true;
        }
        #endregion

        #region Compression task helper methods
        //=====================================================================

        /// <summary>
        /// This is used to handle file compression as a background task
        /// </summary>
        private void CompressFiles()
        {
            int addCount = 0, baseFolderLength = this.WorkingFolder.Length;

            fileCount = Directory.EnumerateFiles(this.WorkingFolder, "*", SearchOption.AllDirectories).Count();
            
            using(var archive = ZipFile.Open(archiveName, ZipArchiveMode.Create))
            {
                // Compress the entire working folder.  Files are stored relative to the root.  We'll handle
                // enumerating the files so that we can report progress.
                foreach(var file in Directory.EnumerateFiles(this.WorkingFolder, "*", SearchOption.AllDirectories))
                {
                    archive.CreateEntryFromFile(file, file.Substring(baseFolderLength), CompressionLevel.Optimal);

                    var fi = new FileInfo(file);

                    folders.Add(fi.DirectoryName);
                    addCount++;
                    uncompressedSize += fi.Length;

                    if((addCount % 500) == 0)
                        progressProvider.Report(addCount);
                }
            }
        }

        /// <summary>
        /// Report progress as the content is compressed
        /// </summary>
        /// <param name="value">The number of files added to the archive so far</param>
        private void ReportProgress(int value)
        {
            Log.LogMessage(MessageImportance.High, "Saved {0} of {1} items", value, fileCount);
        }
        #endregion
    }
}
