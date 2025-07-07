//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : FileCreatedEventArgs.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/05/2025
// Note    : Copyright 2012-2025, Eric Woodruff, All rights reserved
//
// This file contains an event arguments class used by build components to indicate that it has saved a file of
// some sort.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/18/2022  EFW  Moved to Sandcastle.Core and added component group, component ID, and topic key properties
//===============================================================================================================

using System;
using System.IO;

namespace Sandcastle.Core.BuildAssembler.BuildComponent
{
    /// <summary>
    /// This event arguments class is used by build components to indicate that they have saved a file of some
    /// sort (help content or fragment).  The event handler is responsible for figuring out what to do with the
    /// event.
    /// </summary>
    public class FileCreatedEventArgs : EventArgs
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the group ID of the component that saved the file
        /// </summary>
        public string GroupId { get; }

        /// <summary>
        /// This read-only property returns the ID of the component that saved the file
        /// </summary>
        public string ComponentId { get; }

        /// <summary>
        /// This read-only property returns the topic key or null if not saved while generating a specific topic
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// This read-only property returns the path to the saved file
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// This read-only property indicates whether or not the file is a help content file
        /// </summary>
        public bool IsContentFile { get; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="groupId">The group ID of the component</param>
        /// <param name="componentId">The component ID</param>
        /// <param name="key">The topic key</param>
        /// <param name="filePath">The path to the saved file</param>
        /// <param name="isContentFile">True if the saved file is a help content file, false if not</param>
        public FileCreatedEventArgs(string groupId, string componentId, string key, string filePath,
          bool isContentFile)
        {
            this.GroupId = groupId;
            this.ComponentId = componentId;
            this.Key = key;
            this.FilePath = Path.GetFullPath(filePath);
            this.IsContentFile = isContentFile;
        }
        #endregion
    }
}
