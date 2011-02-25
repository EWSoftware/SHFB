//=============================================================================
// System  : Sandcastle Help File Builder Components
// File    : TargetDirectoryCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/08/2008
// Note    : This is a slightly modified version of the Microsoft
//           TargetDirectoryCollection class (Copyright 2007-2008 Microsoft
//           Corporation). My changes are indicated by my initials "EFW" in a
//           comment on the changes.
// Compiler: Microsoft Visual C#
//
// This file contains a reimplementation of the TargetDirectoryCollection class
// used by the ResolveConceptualLinksComponent.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.7  05/07/2008  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This is a reimplementation of the <c>TargetDirectoryCollection</c>
    /// class used by <see cref="ResolveConceptualLinksComponent" />.
    /// </summary>
    internal sealed class TargetDirectoryCollection
    {
        #region Private data members
        //=====================================================================

        private List<TargetDirectory> targetDirectories =
            new List<TargetDirectory>();
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

        #region Methods, etc.
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
