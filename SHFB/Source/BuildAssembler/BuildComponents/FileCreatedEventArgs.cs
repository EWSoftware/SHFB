//===============================================================================================================
// System  : Sandcastle Build Components
// File    : FileCreatedEventArgs.cs
//
// This file contains an event arguments class used by build components to indicate that it has saved a file of
// some sort.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Change History
// 12/24/2012 - EFW - Moved the class into its own file and changed it to only contain the target filename and
// a flag indicating whether it is a whole document or just a fragment.  The event handler is responsible for
// figuring out what to do with the event (i.e. HxFGeneratorComponent should determine where to put its files).
//===============================================================================================================

using System;
using System.IO;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This event arguments class is used by build components to indicate that they have saved a file of some
    /// sort (help content or fragment).
    /// </summary>
    public class FileCreatedEventArgs : EventArgs
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the path to the saved file
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// This read-only property indicates whether or not the file is a help content file
        /// </summary>
        public bool IsContentFile { get; private set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filePath">The path to the saved file</param>
        /// <param name="isContentFile">True if the saved file is a help content file, false if not</param>
        public FileCreatedEventArgs(string filePath, bool isContentFile)
        {
            this.FilePath = Path.GetFullPath(filePath);
            this.IsContentFile = isContentFile;
        }
        #endregion
    }
}
