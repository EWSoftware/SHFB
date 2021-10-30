// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace
// 12/30/2012 - EFW - Cleaned up the code and marked the class as serializable

using System;
using System.Collections.Generic;

namespace Sandcastle.Tools.BuildComponents.Targets
{
    /// <summary>
    /// This is the base class for all other target types
    /// </summary>
    [Serializable]
    public class Target
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the target's member ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// This is used to get or set the target's container
        /// </summary>
        public string Container { get; set; }

        /// <summary>
        /// This is used to get or set the target's reference topic filename
        /// </summary>
        public string File { get; set; }

        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// Add the target to the given collection
        /// </summary>
        /// <param name="targets">The targets dictionary to which this target is added</param>
        /// <remarks>This can be overridden to add dependent targets to the dictionary as well</remarks>
        public virtual void Add(IDictionary<string, Target> targets)
        {
            if(targets == null)
                throw new ArgumentNullException(nameof(targets));

            targets[this.Id] = this;
        }
        #endregion
    }
}
