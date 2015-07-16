//===============================================================================================================
// System  : Sandcastle Build Components
// File    : TargetDirectoryCollection.cs
// Note    : Copyright 2010-2012 Microsoft Corporation
//
// This file contains the TargetDirectoryCollection class used by the ResolveConceptualLinksComponent.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Change History
// 02/16/2012 - EFW - Merged my changes into the code
// 12/26/2012 - EFW - Moved the class into the Targets namespace and made it a proper collection
//===============================================================================================================

using System.Collections.ObjectModel;

using Microsoft.Ddue.Tools.BuildComponent;

namespace Microsoft.Ddue.Tools.Targets
{
    /// <summary>
    /// This class contains a set of <see cref="TargetDirectory"/> items used by
    /// <see cref="ResolveConceptualLinksComponent" />.
    /// </summary>
    public sealed class TargetDirectoryCollection : Collection<TargetDirectory>
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only indexer can be used to retrieve the target info for the specified target ID
        /// </summary>
        /// <param name="targetId">The target ID of the file to locate.  This is expected to be in the form of
        /// a GUID.</param>
        /// <returns>A <see cref="TargetInfo" /> object if found or null if not found.</returns>
        public TargetInfo this[string targetId]
        {
            get
            {
                TargetInfo targetInfo;

                foreach(var directory in this)
                {
                    targetInfo = directory.GetTargetInfo(targetId);

                    if(targetInfo != null)
                        return targetInfo;
                }

                return null;
            }
        }
        #endregion
    }
}
