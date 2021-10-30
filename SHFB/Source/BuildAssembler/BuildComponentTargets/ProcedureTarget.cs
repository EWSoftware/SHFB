// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace
// 12/30/2012 - EFW - Cleaned up the code and marked the class as serializable

using System;

namespace Sandcastle.Tools.BuildComponents.Targets
{
    /// <summary>
    /// This represents a procedure target
    /// </summary>
    [Serializable]
    public class ProcedureTarget : MemberTarget
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set whether or not the target is a conversion operator
        /// </summary>
        public bool IsConversionOperator { get; set; }

        /// <summary>
        /// This is used to specify the member explicitly implemented if applicable
        /// </summary>
        public MemberReference ExplicitlyImplements { get; set; }

        #endregion
    }
}
