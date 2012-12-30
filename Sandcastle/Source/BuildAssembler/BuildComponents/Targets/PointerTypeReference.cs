// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 12/26/2012 - EFW - Moved the classes into the Targets namespace

using System;

namespace Microsoft.Ddue.Tools.Targets
{
    public class PointerTypeReference : TypeReference
    {
        private TypeReference pointedToType;

        public TypeReference PointedToType
        {
            get
            {
                return (pointedToType);
            }
        }

        internal PointerTypeReference(TypeReference pointedToType)
        {
            if(pointedToType == null)
                throw new ArgumentNullException("pointedToType");
            this.pointedToType = pointedToType;
        }
    }
}
