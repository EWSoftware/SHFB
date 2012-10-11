//=============================================================================
// System  : Sandcastle Build Components
// File    : TargetDirectoryCollection.cs
// Note    : Copyright 2010-2012 Microsoft Corporation
//
// This file contains the TargetDirectoryCollection class used by the
// ResolveConceptualLinksComponent.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice and
// all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Change History
// 02/16/2012 - EFW - Merged my changes into the code
//=============================================================================

using System.Collections.Generic;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This class contains a set of <see cref="TargetDirectory"/> items used
    /// by <see cref="ResolveConceptualLinksComponent" />.
    /// </summary>
    public sealed class TargetDirectoryCollection
    {
        #region Private data members
        //=====================================================================

        private List<TargetDirectory> targetDirectories = new List<TargetDirectory>();
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the number of items in the
        /// collection.
        /// </summary>
        public int Count
        {
            get { return targetDirectories.Count; }
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// Add a new target directory to the collection
        /// </summary>
        /// <param name="targetDirectory">The target directory to add</param>
        public void Add(TargetDirectory targetDirectory)
        {
            targetDirectories.Add(targetDirectory);
        }

        /// <summary>
        /// Find the target info for the specified file
        /// </summary>
        /// <param name="file">The file for which to find target info</param>
        /// <returns>A <see cref="TargetInfo" /> object if found or null if
        /// not found.</returns>
        public TargetInfo GetTargetInfo(string file)
        {
            TargetInfo targetInfo;

            foreach(TargetDirectory directory in targetDirectories)
            {
                targetInfo = directory.GetTargetInfo(file);

                if(targetInfo != null)
                    return targetInfo;
            }

            return null;
        }
        #endregion
    }
}
